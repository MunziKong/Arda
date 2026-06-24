using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Return To Home",
    story: "[Self] returns to [HomePosition] speed [MoveSpeed] sight [CanSeeTarget]",
    category: "Action/Combat",
    id: "aabbccddeeff00112233445566778899")]
public partial class ReturnToHomeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> HomePosition;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<bool> CanSeeTarget;

    private const float ArrivalThreshold = 0.5f;

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

        if (CanSeeTarget != null)
        {
            CanSeeTarget.Value = false;
        }

        _agent.speed = MoveSpeed?.Value ?? _agent.speed;
        _agent.SetDestination(HomePosition.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent.pathPending)
        {
            return Status.Running;
        }

        if (_agent.remainingDistance <= ArrivalThreshold)
        {
            _agent.ResetPath();
            var mc = Self.Value.GetComponentInChildren<MonsterController>();
            if (mc != null)
            {
                mc.IsReturningHome = false;
            }
            var ec = Self.Value.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.IsReturningHome = false;
            }
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _agent?.ResetPath();
    }
}
