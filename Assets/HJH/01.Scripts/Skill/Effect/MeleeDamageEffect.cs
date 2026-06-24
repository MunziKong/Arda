using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeDamageEffect", menuName = "Arda/Effect/MeleeDamageEffect")]
public class MeleeDamageEffect : SkillEffect
{
    // multipliers[0] : 데미지 배율
    // multipliers[1] : Duration 배율 
    public override void Execute(SkillContext context, float[] multipliers)
    {
        Debug.Log("MeleeEffect 실행됨.");
        if (context.Caster == null)
        {
            return;
        }

        ICoroutineRunner runner = context.Caster.GetComponent<ICoroutineRunner>();

        if (runner == null)
        {
            return;
        }

        runner.RunCoroutine(
            MeleeDamageRoutine(context, multipliers)
        );
    }

    private IEnumerator MeleeDamageRoutine(SkillContext context, float[] multipliers)
    {
        WeaponHitArea[] hitAreas = context.Caster.GetComponentsInChildren<WeaponHitArea>();
        int slotIndex = context.Skill != null ? context.Skill.weaponSlotIndex : 0;
        WeaponHitArea found = System.Array.Find(hitAreas, h => h.slotIndex == slotIndex);
        WeaponHitArea hitArea = found != null ? found : (hitAreas.Length > 0 ? hitAreas[0] : null);

        if (hitArea == null)
        {
            yield break;
        }

        GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);

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

        if (targetLayer == 0)
        {
            hitArea.DebugHitArea = false;
            yield break;
        }

        hitArea.DebugHitArea = true;
        int damage = Mathf.RoundToInt(context.AttackPower * multipliers[0]);

        HashSet<IDamageable> targets = new();

        float timer = 0f;

        while (timer < multipliers[1])
        {
            Collider[] hits = Physics.OverlapCapsule(hitArea.HitStart.position, hitArea.HitEnd.position, hitArea.Radius, targetLayer);

            foreach (Collider hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();

                if (damageable == null)
                {
                    continue;
                }

                if (targets.Contains(damageable))
                {
                    continue;
                }

                targets.Add(damageable);

                damageable.TakeDamage(damage);
            }

            timer += Time.deltaTime;

            yield return null;
        }

        hitArea.DebugHitArea = false;
    }
}
