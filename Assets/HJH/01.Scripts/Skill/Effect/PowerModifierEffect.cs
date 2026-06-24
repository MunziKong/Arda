using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerModifierEffect", menuName = "Arda/Effect/PowerModifierEffect")]
public class PowerModifierEffect : SkillEffect
{
    private const int POWER_MULTIPLIER_IDX = 0;
    private const int DURATION_MULTIPLIER_IDX = 1;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null)
        {
            return;
        }

        float powerMultiplier = GetMultiplier(multipliers, POWER_MULTIPLIER_IDX, 1f);
        float durationMultiplier = GetMultiplier(multipliers, DURATION_MULTIPLIER_IDX, 1f);
        ICoroutineRunner runner = context.Caster.GetComponent<ICoroutineRunner>();

        runner.RunCoroutine(ApplyPowerBuff(context, powerMultiplier, durationMultiplier));

    }

    private float GetMultiplier(float[] multipliers, int index, float defaultValue)
    {
        if (multipliers == null || multipliers.Length <= index)
        {
            return defaultValue;
        }

        return multipliers[index];
    }

    private IEnumerator ApplyPowerBuff(SkillContext context, float damageMultiplier, float durationMultiplier)
    {
        PlayerStats stats = context.Caster.GetComponent<PlayerStats>();
        stats.SetPowerValue(damageMultiplier);

        float timer = 0f;
        while (timer < durationMultiplier)
        {
            // 버프 UI 업데이트가 필요하다면  이곳에.
            timer += Time.deltaTime;
            yield return null;
        }

        stats.SetPowerValue(1f);
    }


}