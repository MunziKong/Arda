using UnityEngine;

[CreateAssetMenu(fileName = "AllowMoveEffect", menuName = "Arda/Effect/AllowMoveEffect")]
public class AllowMoveEffect : SkillEffect
{
    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context.Caster == null)
        {
            return;
        }

        PlayerMove move = context.Caster.GetComponent<PlayerMove>();

        if (move == null)
        {
            return;
        }

        move.SetCanMoveWhileSkill(true);
    }
}
