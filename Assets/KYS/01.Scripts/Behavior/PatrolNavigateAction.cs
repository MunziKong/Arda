using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol Navigate",
    story: "[Self] navigates to [PatrolPoint] speed [MoveSpeed]",
    category: "Action/Monster",
    id: "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d")]
public partial class PatrolNavigateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> PatrolPoint;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;

    private NavMeshAgent _agent;

    protected override Status OnStart()
    {
        if (Self?.Value == null)
        {
            return Status.Failure;
        }

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return Status.Failure;
        }

        if (MoveSpeed != null && MoveSpeed.Value > 0f)
        {
            _agent.speed = MoveSpeed.Value;
        }

        _agent.isStopped = false;
        _agent.SetDestination(PatrolPoint.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return Status.Failure;
        }

        if (_agent.pathPending)
        {
            return Status.Running;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            return Status.Success;
        }

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
