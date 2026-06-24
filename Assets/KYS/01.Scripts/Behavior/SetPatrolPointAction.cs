using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Patrol Point",
    story: "Set [PatrolPoint] near [HomePosition] within [PatrolRadius]",
    category: "Action/Monster",
    id: "b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7")]
public partial class SetPatrolPointAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> HomePosition;
    [SerializeReference] public BlackboardVariable<float> PatrolRadius;
    [SerializeReference] public BlackboardVariable<Vector3> PatrolPoint;

    protected override Status OnUpdate()
    {
        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * PatrolRadius.Value;
        randomOffset.y = 0f;
        Vector3 candidate = HomePosition.Value + randomOffset;

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, PatrolRadius.Value, NavMesh.AllAreas))
        {
            PatrolPoint.Value = hit.position;
        }
        else
        {
            PatrolPoint.Value = HomePosition.Value;
        }

        return Status.Success;
    }
}
