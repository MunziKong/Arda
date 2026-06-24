using UnityEngine;

[CreateAssetMenu(fileName = "TargetDamageEffect", menuName = "Arda/Effect/TargetDamageEffect")]
public class TargetDamageEffect : SkillEffect
{
    private const int DAMAGE_MULTIPLIER_IDX = 0;
    private const int LIFE_TIME_IDX = 1;
    private const int VFX_Y_OFFSET_IDX = 2;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        Debug.Log($"TargetDamageEffect 실행 / Target: {context?.Target}");
        if (context == null || context.Caster == null || context.Target == null)
        {
            return;
        }

        IDamageable damageable = context.Target.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        float damageMultiplier = GetMultiplier(multipliers, DAMAGE_MULTIPLIER_IDX, 1f);
        float lifeTime = GetMultiplier(multipliers, LIFE_TIME_IDX, 1f);
        float yOffset = GetMultiplier(multipliers, VFX_Y_OFFSET_IDX, 0f);

        int damage = Mathf.RoundToInt(context.AttackPower * damageMultiplier);

        SpawnVfx(
            context,
            context.Target.transform.position + Vector3.up * yOffset,
            lifeTime
        );

        GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);

        damageable.TakeDamage(damage);

    }

    private void SpawnVfx(SkillContext context, Vector3 position, float lifeTime)
    {
        GameObject vfxPrefab = context.Skill.GetPrefab(SkillPrefabType.HitVFX);

        if (vfxPrefab == null)
        {
            return;
        }

        GameObject vfxObject = Instantiate(vfxPrefab, position, Quaternion.identity);

        Destroy(vfxObject, lifeTime);
    }

    private float GetMultiplier(float[] multipliers, int index, float defaultValue)
    {
        if (multipliers == null || multipliers.Length <= index)
        {
            return defaultValue;
        }

        return multipliers[index];
    }
}