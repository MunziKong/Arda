using UnityEngine;

public enum SkillType { Melee, Magic }

public abstract class SkillEffect : ScriptableObject
{
    public abstract void Execute(SkillContext context, float[] multipliers);
}

public sealed class SkillContext
{
    public SkillDefinition Skill { get; }
    public GameObject Caster { get; }
    public GameObject Target { get; }
    public Vector3 Point { get; }
    public float AttackPower { get; }

    public Vector3 CastForward { get; }

    public SkillSfxEntry Sfx { get; set; }

    public SkillContext(
        SkillDefinition skill,
        GameObject caster,
        GameObject target,
        Vector3 point,
        float attackPower)
    {
        Skill = skill;
        Caster = caster;
        Target = target;
        Point = point;
        AttackPower = attackPower;

        CastForward = caster != null
            ? caster.transform.forward
            : Vector3.forward;
    }
}