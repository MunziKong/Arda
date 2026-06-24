using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Target",
    story: "[Self] chases [Target] while [CanSeeTarget] speed [MoveSpeed] home [HomePosition] radius [PatrolRadius]",
    category: "Action/Monster",
    id: "c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8")]
public partial class ChaseTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<bool> CanSeeTarget;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<Vector3> HomePosition;
    [SerializeReference] public BlackboardVariable<float> PatrolRadius;

    private NavMeshAgent _agent;

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

        _agent.speed = MoveSpeed.Value;

        var mc = Self.Value.GetComponentInChildren<MonsterController>();
        if (mc != null && HomePosition != null && PatrolRadius != null)
        {
            mc.HomePosition = HomePosition.Value;
            mc.PatrolRadius = PatrolRadius.Value;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!CanSeeTarget.Value)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.ResetPath();
            }
            return Status.Failure;
        }

        if (Target?.Value == null)
        {
            return Status.Failure;
        }

        float distFromHome = Vector3.Distance(Self.Value.transform.position, HomePosition.Value);
        if (distFromHome > PatrolRadius.Value)
        {
            _agent.ResetPath();
            CanSeeTarget.Value = false;
            var mc = Self.Value.GetComponentInChildren<MonsterController>();
            if (mc != null)
            {
                mc.IsReturningHome = true;
            }
            return Status.Failure;
        }

        if (!_agent.isOnNavMesh)
        {
            return Status.Failure;
        }

        _agent.SetDestination(Target.Value.position);
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.ResetPath();
        }
            
    }
}
