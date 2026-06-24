using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Try Use Skill",
    story: "[Self] tries [Skill] on [Target] range [Range]",
    category: "Action/Combat",
    id: "a3f2e1d0c9b8a7f6e5d4c3b2a1f0e9d8")]
public partial class TryUseSkillAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<SkillDefinition> Skill;
    [SerializeReference] public BlackboardVariable<float> Range;

    private static readonly int MoveStateHash = Animator.StringToHash("Move");
    private static readonly Collider[] HitBuffer = new Collider[16];

    private EnemyController _controller;
    private EnemyAnimEventHandler _animEvents;
    private SkillCooldownTracker _tracker;
    private NavMeshAgent _agent;
    private Transform _target;
    private Animator _animator;
    private WeaponHitArea _hitArea;
    private int _attackStateHash;
    private bool _animationStarted;
    private bool _hitWindowActive;
    private bool _hitChecked;
    private readonly HashSet<IDamageable> _hitTargets = new();
    private int _targetLayerMask;
    private float _safetyTimeout;
    private bool _isMeleeSkill;
    private Vector3 _castTargetPoint;
    private bool _waitingForCastIndicator;

    protected override Status OnStart()
    {
        if (Self?.Value == null || Target?.Value == null || Skill?.Value == null)
        {
            return Status.Failure;
        }

        _controller = Self.Value.GetComponent<EnemyController>();
        if (_controller == null || _controller.IsDead)
        {
            return Status.Failure;
        }

        _target = Target.Value;
        float dist = Vector3.Distance(Self.Value.transform.position, _target.position);

        if (dist > Range.Value)
        {
            return Status.Failure;
        }

        _tracker = Self.Value.GetComponent<SkillCooldownTracker>();
        if (_tracker != null && !_tracker.IsReady(Skill.Value))
        {
            return Status.Failure;
        }

        _tracker?.StartCooldown(Skill.Value);
        _controller.IsAttacking = true;

        _agent = Self.Value.GetComponent<NavMeshAgent>();

        Vector3 lookDir = _target.position - Self.Value.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Self.Value.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        _animator = Self.Value.GetComponentInChildren<Animator>();

        var hitAreas = Self.Value.GetComponentsInChildren<WeaponHitArea>();
        int slotIndex = Skill.Value.weaponSlotIndex;
        var found = Array.Find(hitAreas, h => h.slotIndex == slotIndex);
        _hitArea = found != null ? found : (hitAreas.Length > 0 ? hitAreas[0] : null);
        _isMeleeSkill = _hitArea != null && _hitArea.HitStart != null;

        _targetLayerMask = LayerMask.GetMask("Player");
        _animationStarted = false;
        _hitWindowActive = false;
        _hitChecked = false;
        _waitingForCastIndicator = false;
        _hitTargets.Clear();
        _safetyTimeout = Time.time + 5f;

        _animEvents = Self.Value.GetComponentInChildren<EnemyAnimEventHandler>();
        if (_animEvents != null)
        {
            _animEvents.OnHitStart += HandleHitStart;
            _animEvents.OnHitEnd   += HandleHitEnd;
        }

        _castTargetPoint = _target.position;

        var startContext = new SkillContext(
            skill:       Skill.Value,
            caster:      Self.Value,
            target:      _target.gameObject,
            point:       _castTargetPoint,
            attackPower: _controller.data.attackPower
        );
        Skill.Value.ExecuteStart(startContext);

        if (_animator != null && !string.IsNullOrEmpty(Skill.Value.skillId))
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
            }
            _attackStateHash = Animator.StringToHash(Skill.Value.skillId);
            _animator.Play(Skill.Value.skillId);
        }
        else
        {
            DoHitCheck();
            return Status.Success;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time >= _safetyTimeout)
        {
            DoHitCheck();
            return Status.Success;
        }

        // 캐스트 인디케이터 완료 대기 중
        if (_waitingForCastIndicator)
        {
            if (CastIndicator.Current == null || CastIndicator.Current.IsDone)
            {
                _waitingForCastIndicator = false;
                if (!_hitChecked)
                {
                    _hitChecked = true;
                    FireProjectile();
                }
            }
            return Status.Running;
        }

        // 근접 스킬만 히트 윈도우 중 매 프레임 스캔
        if (_isMeleeSkill && _hitWindowActive)
        {
            PerformHitScan();
        }

        if (_animator.IsInTransition(0))
        {
            return Status.Running;
        }

        var info = _animator.GetCurrentAnimatorStateInfo(0);

        if (!_animationStarted)
        {
            if (info.shortNameHash == _attackStateHash)
            {
                _animationStarted = true;
            }
            return Status.Running;
        }

        // 애니메이션 이벤트 없을 때 중간 지점에서 한 번 체크 (fallback)
        if (!_hitChecked && !_hitWindowActive && info.shortNameHash == _attackStateHash && info.normalizedTime >= 0.5f)
        {
            DoHitCheck();
        }

        // 애니메이션 끝났으면 Success
        if (info.shortNameHash != _attackStateHash || info.normalizedTime >= 1f)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_controller != null)
        {
            _controller.IsAttacking = false;
        }

        if (_animEvents != null)
        {
            _animEvents.OnHitStart -= HandleHitStart;
            _animEvents.OnHitEnd   -= HandleHitEnd;
        }

        _hitWindowActive = false;
        _waitingForCastIndicator = false;

        if (_controller != null && !_controller.IsDead && _animator != null && _animator.gameObject.activeInHierarchy)
        {
            _animator.CrossFade(MoveStateHash, 0.15f, 0);
        }
    }

    private void HandleHitStart()
    {
        if (_isMeleeSkill)
        {
            _hitWindowActive = true;
        }
        else
        {
            // 캐스트 인디케이터가 진행 중이면 완료될 때까지 대기
            if (!_hitChecked)
            {
                if (CastIndicator.Current != null && !CastIndicator.Current.IsDone)
                {
                    _waitingForCastIndicator = true;
                }
                else
                {
                    _hitChecked = true;
                    FireProjectile();
                }
            }
        }
    }

    private void HandleHitEnd() => _hitWindowActive = false;

    // 근접 스킬 전용 — 히트 윈도우 중 매 프레임 호출
    private void PerformHitScan()
    {
        if (_hitArea == null || _controller == null || _target == null)
        {
            return;
        }

        int count = Physics.OverlapCapsuleNonAlloc(
            _hitArea.HitStart.position,
            _hitArea.HitEnd.position,
            _hitArea.Radius,
            HitBuffer,
            _targetLayerMask);

        for (int i = 0; i < count; i++)
        {
            var hit = HitBuffer[i];
            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || _hitTargets.Contains(damageable))
            {
                continue;
            }

            _hitTargets.Add(damageable);

            Debug.Log($"[TryUseSkill] {Skill.Value.skillId} 적중!");

            var context = new SkillContext(
                skill:       Skill.Value,
                caster:      Self.Value,
                target:      hit.gameObject,
                point:       hit.transform.position,
                attackPower: _controller.data.attackPower
            );
            Skill.Value.ExecuteApply(context);
        }
    }

    // 프로젝타일 스킬 전용 — 타겟 방향으로 발사
    private void FireProjectile()
    {
        if (_controller == null || _target == null)
        {
            return;
        }

        var context = new SkillContext(
            skill:       Skill.Value,
            caster:      Self.Value,
            target:      _target.gameObject,
            point:       _castTargetPoint,
            attackPower: _controller.data.attackPower
        );
        Skill.Value.ExecuteApply(context);
        Debug.Log($"[TryUseSkill] {Skill.Value.skillId} 발사!");
    }

    // 애니메이션 이벤트 없을 때 fallback — 스킬당 한 번만 실행
    private void DoHitCheck()
    {
        if (_hitChecked || _controller == null || _target == null)
        {
            return;
        }

        _hitChecked = true;

        if (_isMeleeSkill)
        {
            PerformHitScan();
        }
        else
        {
            FireProjectile();
        }
    }

}
