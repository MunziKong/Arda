using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Flee Point",
    story: "[Self] flees from [PlayerTransform] within [HomePosition] [PatrolRadius] sets [PatrolPoint]",
    category: "Action/Monster",
    id: "f1e2d3c4b5a6f7e8d9c0b1a2f3e4d5c6")]
public partial class SetFleePointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;
    [SerializeReference] public BlackboardVariable<Vector3> HomePosition;
    [SerializeReference] public BlackboardVariable<float> PatrolRadius;
    [SerializeReference] public BlackboardVariable<Vector3> PatrolPoint;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<float> FleeSpeed;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || PlayerTransform?.Value == null)
        {
            return Status.Failure;
        }

        Vector3 fleeDir = (Self.Value.transform.position - PlayerTransform.Value.position).normalized;
        Vector3 candidate = HomePosition.Value + fleeDir * PatrolRadius.Value;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, PatrolRadius.Value, NavMesh.AllAreas))
        {
            PatrolPoint.Value = hit.position;
        }
        else
        {
            PatrolPoint.Value = HomePosition.Value;
        }

        if (FleeSpeed != null && MoveSpeed != null)
        {
            FleeSpeed.Value = MoveSpeed.Value * 2f;
        }

        return Status.Success;
    }
}
