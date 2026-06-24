using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Combat Chase",
    story: "[Self] chases [Target] speed [ChaseSpeed] anim [ChaseAnim] sight [CanSeeTarget] home [HomePosition] radius [PatrolRadius]",
    category: "Action/Combat",
    id: "00998877665544332211ffeeddccbbaa")]
public partial class CombatChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;
    [SerializeReference] public BlackboardVariable<string> ChaseAnim;
    [SerializeReference] public BlackboardVariable<bool> CanSeeTarget;
    [SerializeReference] public BlackboardVariable<Vector3> HomePosition;
    [SerializeReference] public BlackboardVariable<float> PatrolRadius;

    private static readonly int SpeedMagnitudeHash = Animator.StringToHash("SpeedMagnitude");

    private NavMeshAgent _agent;
    private Animator _animator;
    private bool _hasSpeedMagnitude;

    protected override Status OnStart()
    {
        if (Self?.Value == null)
        {
            return Status.Failure;
        }

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            return Status.Failure;
        }

        var ec = Self.Value.GetComponent<EnemyController>();
        if (ec != null && ec.IsDead)
        {
            return Status.Failure;
        }

        if (Target?.Value == null)
        {
            return Status.Failure;
        }

        _animator = Self.Value.GetComponentInChildren<Animator>();
        if (_animator != null)
        {
            string anim = string.IsNullOrEmpty(ChaseAnim?.Value) ? "Move" : ChaseAnim.Value;
            _animator.CrossFade(anim, 0.15f, 0);

            _hasSpeedMagnitude = false;
            foreach (var p in _animator.parameters)
            {
                if (p.nameHash == SpeedMagnitudeHash)
                {
                    _hasSpeedMagnitude = true;
                    break;
                }
            }
        }

        _agent.speed = ChaseSpeed.Value;

        var ecHome = Self.Value.GetComponent<EnemyController>();
        if (ecHome != null && HomePosition != null && PatrolRadius != null)
        {
            ecHome.HomePosition = HomePosition.Value;
            ecHome.PatrolRadius = PatrolRadius.Value;
        }

        var debugVis = Self.Value.GetComponent<CombatAIDebugVisualizer>();
        if (debugVis != null)
        {
            debugVis.ReportChase();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target?.Value == null)
        {
            return Status.Failure;
        }

        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return Status.Failure;
        }

        if (CanSeeTarget != null && !CanSeeTarget.Value)
        {
            _agent.ResetPath();
            return Status.Failure;
        }

        if (HomePosition != null && PatrolRadius != null && PatrolRadius.Value > 0f)
        {
            float distFromHome = Vector3.Distance(Self.Value.transform.position, HomePosition.Value);
            if (distFromHome > PatrolRadius.Value)
            {
                _agent.ResetPath();
                CanSeeTarget.Value = false;
                var ec = Self.Value.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.IsReturningHome = true;
                }
                return Status.Failure;
            }
        }

        float dist = Vector3.Distance(Self.Value.transform.position, Target.Value.position);

        if (dist > _agent.stoppingDistance)
        {
            _agent.isStopped = false;
            _agent.SetDestination(Target.Value.position);
        }
        else
        {
            _agent.isStopped = true;
        }

        if (_animator != null && _hasSpeedMagnitude)
        {
            _animator.SetFloat(SpeedMagnitudeHash, _agent.velocity.magnitude);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.ResetPath();
        }
    }
}
