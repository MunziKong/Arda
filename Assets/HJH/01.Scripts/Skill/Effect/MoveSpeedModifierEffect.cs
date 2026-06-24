using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveSpeedModifierEffect", menuName = "Arda/Effect/MoveSpeedModifierEffect")]
public class SpeedModifierEffectcs : SkillEffect
{
    private const int SPEED_MULTIPLIER_IDX = 0;
    private const int DURATION_MULTIPLIER_IDX = 1;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null)
        {
            return;
        }

        float speedMultiplier = GetMultiplier(multipliers, SPEED_MULTIPLIER_IDX, 1f);
        float durationMultiplier = GetMultiplier(multipliers, DURATION_MULTIPLIER_IDX, 1f);
        ICoroutineRunner runner = context.Caster.GetComponent<ICoroutineRunner>();

        runner.RunCoroutine(ApplySpeedBuff(context, speedMultiplier, durationMultiplier));
        GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);
    }

    private float GetMultiplier(float[] multipliers, int index, float defaultValue)
    {
        if (multipliers == null || multipliers.Length <= index)
        {
            return defaultValue;
        }

        return multipliers[index];
    }

    private IEnumerator ApplySpeedBuff(SkillContext context, float speedMultiplier, float durationMultiplier)
    {
        PlayerStats stats = context.Caster.GetComponent<PlayerStats>();
        PlayerMove move = context.Caster.GetComponent<PlayerMove>();

        stats.SetSpeedValue(speedMultiplier);
        move.SetFootstepIntervalMultiplier(speedMultiplier);

        float timer = 0f;
        while (timer < durationMultiplier)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        stats.SetSpeedValue(1f);
        move.ResetFootstepInterval();
    }
}