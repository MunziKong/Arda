using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Execute Random Attack",
    story: "[Self] attacks [Target] basic [BasicAttack] [BasicAttackRange] skill1 [Skill1] [Skill1Range] skill2 [Skill2] [Skill2Range]",
    category: "Action/Combat",
    id: "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6")]
public partial class ExecuteRandomAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    [SerializeReference] public BlackboardVariable<SkillDefinition> BasicAttack;
    [SerializeReference] public BlackboardVariable<float> BasicAttackRange;

    [SerializeReference] public BlackboardVariable<SkillDefinition> Skill1;
    [SerializeReference] public BlackboardVariable<float> Skill1Range;

    [SerializeReference] public BlackboardVariable<SkillDefinition> Skill2;
    [SerializeReference] public BlackboardVariable<float> Skill2Range;

    private static readonly int MoveStateHash = Animator.StringToHash("Move");

    private SkillDefinition _selected;
    private EnemyController _controller;
    private SkillCooldownTracker _tracker;
    private NavMeshAgent _agent;
    private Transform _target;
    private Animator _animator;
    private CombatAIDebugVisualizer _debugVis;
    private int _attackStateHash;
    private bool _damageDealt;
    private float _safetyTimeout;

    protected override Status OnStart()
    {
        if (Self?.Value == null || Target?.Value == null)
        {
            return Status.Failure;
        }

        _controller = Self.Value.GetComponent<EnemyController>();
        if (_controller == null || _controller.IsDead)
        {
            return Status.Failure;
        }

        _tracker = Self.Value.GetComponent<SkillCooldownTracker>();
        _debugVis = Self.Value.GetComponent<CombatAIDebugVisualizer>();
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        _target = Target.Value;

        float dist = Vector3.Distance(Self.Value.transform.position, _target.position);

        var pool = new List<SkillDefinition>();

        if (BasicAttack?.Value != null && dist <= BasicAttackRange.Value && (_tracker == null || _tracker.IsReady(BasicAttack.Value)))
        {
            pool.Add(BasicAttack.Value);
        }
        if (Skill1?.Value != null && dist <= Skill1Range.Value && (_tracker == null || _tracker.IsReady(Skill1.Value)))
        {
            pool.Add(Skill1.Value);
        }
        if (Skill2?.Value != null && dist <= Skill2Range.Value && (_tracker == null || _tracker.IsReady(Skill2.Value)))
        {
            pool.Add(Skill2.Value);
        }

        if (pool.Count == 0)
        {
            return Status.Failure;
        }

        _selected = pool[UnityEngine.Random.Range(0, pool.Count)];
        _damageDealt = false;
        _safetyTimeout = Time.time + 5f;

        if (_tracker != null)
        {
            _tracker.StartCooldown(_selected);
        }

        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.ResetPath();
        }

        Vector3 lookDir = _target.position - Self.Value.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Self.Value.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        string animName = _selected.skillId;

        _animator = Self.Value.GetComponentInChildren<Animator>();

        if (_animator != null && !string.IsNullOrEmpty(animName))
        {
            _attackStateHash = Animator.StringToHash(animName);
            _animator.Play(animName);
        }
        else
        {
            // 애니메이터 없으면 즉시 데미지
            ApplyDamage();
            return Status.Success;
        }

        Debug.Log($"[{Self.Value.name}] 공격 시작: {_selected.skillId}");
        if (_debugVis != null)
        {
            _debugVis.ReportAttack(_selected.skillId);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_animator == null || (_controller != null && _controller.IsDead))
        {
            return;
        }

        _animator.CrossFade(MoveStateHash, 0.15f, 0);

        // 공격 종료 즉시 이동 재개 — Chase 액션이 시작될 때까지 그 자리에 서있지 않도록
        if (_agent != null && _target != null && _agent.isOnNavMesh)
        {
            _agent.SetDestination(_target.position);
        }
    }

    protected override Status OnUpdate()
    {
        // 안전 타임아웃
        if (Time.time >= _safetyTimeout)
        {
            ApplyDamage();
            _animator.CrossFade(MoveStateHash, 0.15f, 0);
            return Status.Success;
        }

        // 전환 대기 또는 이전 상태에 머물러 있는 동안 대기
        if (_animator.IsInTransition(0))
        {
            return Status.Running;
        }

        var info = _animator.GetCurrentAnimatorStateInfo(0);

        // 아직 공격 애니메이션으로 전환 안 됐으면 대기
        if (info.shortNameHash != _attackStateHash)
        {
            return Status.Running;
        }

        // 공격 애니메이션이 끝날 때까지 대기
        if (info.normalizedTime < 1f)
        {
            return Status.Running;
        }

        ApplyDamage();
        _animator.CrossFade(MoveStateHash, 0.15f, 0);
        return Status.Success;
    }

    private void ApplyDamage()
    {
        if (_damageDealt)
        {
            return;
        }
        _damageDealt = true;

        if (_target == null || _controller == null)
        {
            return;
        }

        var context = new SkillContext(
            skill:       _selected,
            caster:      Self.Value,
            target:      _target.gameObject,
            point:       _target.position,
            attackPower: _controller.data.attackPower
        );
        _selected.ExecuteApply(context);
        Debug.Log($"[{Self.Value.name}] {_selected.skillId} 적중!");
    }
}
