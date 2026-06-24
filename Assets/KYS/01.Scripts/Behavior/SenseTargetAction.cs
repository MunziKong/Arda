using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Sense Target",
    story: "[Self] senses [Target] range [DetectionRange] angle [ViewAngle] result [CanSeeTarget]",
    category: "Action/Monster",
    id: "9f8e7d6c5b4a3f2e1d0c9b8a7f6e5d4c")]
public partial class SenseTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> DetectionRange;
    [SerializeReference] public BlackboardVariable<float> ViewAngle;
    [SerializeReference] public BlackboardVariable<bool> CanSeeTarget;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || Target?.Value == null)
        {
            CanSeeTarget.Value = false;
            return Status.Success;
        }

        Transform self = Self.Value.transform;

        var controller = Self.Value.GetComponent<EnemyController>();
        if (controller != null)
        {
            if (controller.IsReturningHome)
            {
                if (controller.PatrolRadius > 0f &&
                    Vector3.Distance(self.position, controller.HomePosition) <= controller.PatrolRadius)
                {
                    controller.IsReturningHome = false;
                }
                else
                {
                    CanSeeTarget.Value = false;
                    return Status.Success;
                }
            }
            if (controller.IsAlerted)
            {
                CanSeeTarget.Value = true;
                return Status.Success;
            }
        }

        var monsterController = Self.Value.GetComponentInChildren<MonsterController>();
        if (monsterController != null && monsterController.IsReturningHome)
        {
            if (monsterController.PatrolRadius > 0f &&
                Vector3.Distance(self.position, monsterController.HomePosition) <= monsterController.PatrolRadius)
            {
                monsterController.IsReturningHome = false;
            }
            else
            {
                CanSeeTarget.Value = false;
                return Status.Success;
            }
        }

        Transform target = Target.Value;

        Vector3 toTarget = target.position - self.position;
        float distance = toTarget.magnitude;

        if (distance > DetectionRange.Value)
        {
            CanSeeTarget.Value = false;
            return Status.Success;
        }

        // 이미 전투 중이면 거리 안에만 있으면 유지
        if (CanSeeTarget.Value)
        {
            return Status.Success;
        }

        // 최초 감지는 앵글 + 레이캐스트 체크
        bool visible = false;

        Vector3 flatDir = toTarget;
        flatDir.y = 0f;
        float angle = Vector3.Angle(self.forward, flatDir.normalized);

        if (angle <= ViewAngle.Value * 0.5f)
        {
            Vector3 origin = self.position + Vector3.up * 1.5f;
            Vector3 dir = (target.position + Vector3.up * 1.0f - origin).normalized;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, distance))
            {
                visible = hit.transform == target || hit.transform.IsChildOf(target);
            }
            else
            {
                visible = true;
            }
        }

        CanSeeTarget.Value = visible;

        return Status.Success;
    }
}
