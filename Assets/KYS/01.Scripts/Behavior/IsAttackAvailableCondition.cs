using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[Condition(name: "Is Attack Available",
    story: "[Self] can attack basic [BasicAttack] [BasicAttackRange] skill1 [Skill1] [Skill1Range] skill2 [Skill2] [Skill2Range]",
    category: "Condition/Combat",
    id: "bb11cc22dd33ee44ff55aa66bb77cc88")]
public partial class IsAttackAvailableCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<SkillDefinition> BasicAttack;
    [SerializeReference] public BlackboardVariable<float> BasicAttackRange;

    [SerializeReference] public BlackboardVariable<SkillDefinition> Skill1;
    [SerializeReference] public BlackboardVariable<float> Skill1Range;

    [SerializeReference] public BlackboardVariable<SkillDefinition> Skill2;
    [SerializeReference] public BlackboardVariable<float> Skill2Range;

    public override bool IsTrue()
    {
        if (Self?.Value == null)
        {
            return false;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return false;
        }

        var tracker = Self.Value.GetComponent<SkillCooldownTracker>();
        float dist = Vector3.Distance(Self.Value.transform.position, player.transform.position);

        if (BasicAttack?.Value != null && dist <= BasicAttackRange.Value && (tracker == null || tracker.IsReady(BasicAttack.Value)))
        {
            return true;
        }
        if (Skill1?.Value != null && dist <= Skill1Range.Value && (tracker == null || tracker.IsReady(Skill1.Value)))
        {
            return true;
        }
        if (Skill2?.Value != null && dist <= Skill2Range.Value && (tracker == null || tracker.IsReady(Skill2.Value)))
        {
            return true;
        }

        return false;
    }
}
