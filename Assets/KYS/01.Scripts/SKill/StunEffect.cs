using UnityEngine;

// multipliers[0] = 스턴 시간 배율 (기본 1초 × 배율)
[CreateAssetMenu(fileName = "StunEffect", menuName = "Arda/Effect/StunEffect")]
public class StunEffect : SkillEffect
{
    public const int DURATION_MULTIPLIER = 0;

    private const float BaseDuration = 1f;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context.Target == null)
        {
            return;
        }

        // WeaponHitArea가 있으면 실제 캡슐 충돌 체크 — 범위 밖이면 스턴 안 걸림
        if (context.Caster != null)
        {
            WeaponHitArea hitArea = context.Caster.GetComponentInChildren<WeaponHitArea>();
            if (hitArea != null)
            {
                int casterLayer = context.Caster.layer;
                LayerMask targetLayer = 0;

                if (casterLayer == LayerMask.NameToLayer("Player"))
                {
                    targetLayer = LayerMask.GetMask("Enemy");
                }
                else if (casterLayer == LayerMask.NameToLayer("Enemy"))
                {
                    targetLayer = LayerMask.GetMask("Player");
                }

                if (targetLayer != 0)
                {
                    Collider[] hits = Physics.OverlapCapsule(
                        hitArea.HitStart.position,
                        hitArea.HitEnd.position,
                        hitArea.Radius,
                        targetLayer
                    );

                    bool targetInRange = false;
                    foreach (Collider hit in hits)
                    {
                        if (hit.GetComponentInParent<IStunnable>() != null)
                        {
                            targetInRange = true;
                            break;
                        }
                    }

                    if (!targetInRange)
                    {
                        return;
                    }
                }
            }
        }

        var stunnable = context.Target.GetComponent<IStunnable>();
        if (stunnable == null)
        {
            return;
        }

        float mult = multipliers != null && multipliers.Length > DURATION_MULTIPLIER
            ? multipliers[DURATION_MULTIPLIER]
            : 1f;

        float duration = BaseDuration * mult;
        Debug.Log($"스턴 걸리면 스턴 {duration}초!");
        stunnable.Stun(duration);
    }
}
