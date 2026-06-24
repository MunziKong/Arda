using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Arda/Effect/HealEffect")]
public class HealEffect : SkillEffect
{
    private const int HEAL_PERCENT_IDX = 0;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null)
        {
            return;
        }

        PlayerStats stats = context.Caster.GetComponent<PlayerStats>();

        if (stats == null)
        {
            return;
        }

        float healPercent = GetMultiplier(multipliers, HEAL_PERCENT_IDX, 0.3f);

        int healAmount = Mathf.RoundToInt(stats.MaxHp * healPercent);

        stats.Heal(healAmount);

        GameObject vfxPrefab = context.Skill.GetPrefab(SkillPrefabType.CastVFX);

        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, context.Caster.transform.position, Quaternion.identity);
            GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);
            Destroy(vfx, 1f);
        }

        Debug.Log($"<color=green>[+{healAmount} HP]</color>");
        Debug.Log($"Current HP : <color=yellow>{stats.CurrentHp}</color> / {stats.MaxHp}");
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