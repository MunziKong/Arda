using UnityEngine;

// multipliers[0] = 데미지 배율
[CreateAssetMenu(fileName = "DamageEffect", menuName = "Arda/Effect/DamageEffect")]
public class DamageEffect : SkillEffect
{
    public const int DAMAGE = 0;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context.Target == null)
        {
            return;
        }

        var damageable = context.Target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            return;
        }

        float damageMultiplier = multipliers != null && multipliers.Length > DAMAGE ? multipliers[DAMAGE] : 1f;
        int damage = Mathf.RoundToInt(context.AttackPower * damageMultiplier);
        damageable.TakeDamage(damage);
    }
}
