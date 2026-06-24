using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Use Random Skill",
    story: "[Self] uses random skill on [Target] basic [BasicAttack] [BasicAttackRange] skill1 [Skill1] [Skill1Range] skill2 [Skill2] [Skill2Range]",
    category: "Action/Combat",
    id: "c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2")]
public partial class UseRandomSkillAction : Action
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
    private EnemyAnimEventHandler _animEvents;
    private SkillCooldownTracker _tracker;
    private NavMeshAgent _agent;
    private Transform _target;
    private Animator _animator;
    private CombatAIDebugVisualizer _debugVis;
    private int _attackStateHash;
    private bool _damageDealt;
    private bool _endEventFired;
    private bool _animationStarted;
    private float _safetyTimeout;

    protected override Status OnStart()
    {
        if (Self?.Value == null || Target?.Value == null)
        {
            return Status.Failure;
        }

        _controller = Self.Value.GetComponent<EnemyController>();
        if (_controller == null)
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
        _endEventFired = false;
        _animationStarted = false;
        _safetyTimeout = Time.time + 5f;

        _tracker?.StartCooldown(_selected);
        _controller.IsAttacking = true;
        _agent?.ResetPath();

        Vector3 lookDir = _target.position - Self.Value.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Self.Value.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        _animator = Self.Value.GetComponentInChildren<Animator>();

        _animEvents = Self.Value.GetComponentInChildren<EnemyAnimEventHandler>();
        if (_animEvents != null)
        {
            _animEvents.OnHitStart  += HandleHitStart;
            _animEvents.OnAttackEnd += HandleAttackEnd;
        }

        if (_animator != null && !string.IsNullOrEmpty(_selected.skillId))
        {
            _attackStateHash = Animator.StringToHash(_selected.skillId);
            _animator.Play(_selected.skillId);
        }
        else
        {
            ApplyDamage();
            return Status.Success;
        }

        Debug.Log($"[UseRandomSkill] {Self.Value.name} → {_selected.skillId} 사용");
        _debugVis?.ReportAttack(_selected.skillId);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_endEventFired)
        {
            return Status.Success;
        }

        if (Time.time >= _safetyTimeout)
        {
            ApplyDamage();
            return Status.Success;
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

        // 공격 state에서 벗어남 → 완료
        if (info.shortNameHash != _attackStateHash)
        {
            ApplyDamage();
            return Status.Success;
        }

        if (info.normalizedTime >= 1f)
        {
            ApplyDamage();
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
            _animEvents.OnHitStart  -= HandleHitStart;
            _animEvents.OnAttackEnd -= HandleAttackEnd;
        }

        if (_controller != null && !_controller.IsDead && _animator != null && _animator.gameObject.activeInHierarchy)
        {
            _animator.CrossFade(MoveStateHash, 0.15f, 0);
        }

        if (_agent != null && _target != null && _agent.isOnNavMesh)
        {
            _agent.SetDestination(_target.position);
        }
    }

    private void HandleHitStart() => ApplyDamage();
    private void HandleAttackEnd() => _endEventFired = true;

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
    }
}
