using System.Collections;
using UnityEngine;

// multipliers[0] : 데미지 배율
// multipliers[1] : 범위 반경
// multipliers[2] : VFX 지속 시간
// multipliers[3] : 데미지 딜레이 (초, 생략하거나 0이면 즉시)
// multipliers[4] : VFX 스킵 (0 = 스폰, 1 = 스킵)
[CreateAssetMenu(fileName = "AreaDamageEffect", menuName = "Arda/Effect/AreaDamageEffect")]
public class AreaDamageEffect : SkillEffect
{
    private const int DAMAGE_MULTIPLIER_IDX = 0;
    private const int RADIUS_IDX = 1;
    private const int LIFE_TIME_IDX = 2;
    private const int DELAY_IDX = 3;
    private const int SKIP_VFX_IDX = 4;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null)
        {
            return;
        }

        float damageMultiplier = GetMultiplier(multipliers, DAMAGE_MULTIPLIER_IDX, 1f);
        float radius = GetMultiplier(multipliers, RADIUS_IDX, 1f);
        float lifeTime = GetMultiplier(multipliers, LIFE_TIME_IDX, 1f);
        float delay = GetMultiplier(multipliers, DELAY_IDX, 0f);

        Vector3 center = context.Point;

        bool skipVfx = GetMultiplier(multipliers, SKIP_VFX_IDX, 0f) >= 1f;
        if (!skipVfx)
        {
            SpawnAreaVfx(context, center, radius, lifeTime);
        }

        GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);

        if (delay > 0f)
        {
            ICoroutineRunner runner = context.Caster.GetComponent<ICoroutineRunner>();
            if (runner != null)
            {
                runner.RunCoroutine(DelayedDamage(context, center, radius, damageMultiplier, delay));
                return;
            }
        }

        ApplyAreaDamage(context, center, radius, damageMultiplier);
    }

    private IEnumerator DelayedDamage(SkillContext context, Vector3 center, float radius, float damageMultiplier, float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyAreaDamage(context, center, radius, damageMultiplier);
    }

    private float GetMultiplier(float[] multipliers, int index, float defaultValue)
    {
        if (multipliers == null || multipliers.Length <= index)
        {
            return defaultValue;
        }

        return multipliers[index];
    }

    private void ApplyAreaDamage(SkillContext context, Vector3 center, float radius, float damageMultiplier)
    {
        LayerMask targetLayer = GetTargetLayer(context.Caster);

        if (targetLayer == 0)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer, QueryTriggerInteraction.Ignore);

        int damage = Mathf.RoundToInt(context.AttackPower * damageMultiplier);

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            damageable.TakeDamage(damage);
        }
    }

    private void SpawnAreaVfx(SkillContext context, Vector3 center, float radius, float lifeTime)
    {
        GameObject areaPrefab = context.Skill.GetPrefab(SkillPrefabType.HitVFX);

        if (areaPrefab == null)
        {
            return;
        }

        GameObject areaObject = Instantiate(areaPrefab, center, Quaternion.identity);

        areaObject.transform.localScale = new Vector3(radius * 2f, areaObject.transform.localScale.y, radius * 2f);
        Destroy(areaObject, lifeTime);
    }

    private LayerMask GetTargetLayer(GameObject caster)
    {
        int casterLayer = caster.layer;

        if (casterLayer == LayerMask.NameToLayer("Player"))
        {
            return LayerMask.GetMask("Enemy");
        }

        if (casterLayer == LayerMask.NameToLayer("Enemy"))
        {
            return LayerMask.GetMask("Player");
        }

        return 0;
    }



}