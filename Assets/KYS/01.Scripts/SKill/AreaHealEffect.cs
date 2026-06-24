using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// multipliers[0] : 틱당 힐량
// multipliers[1] : 범위 반경
// multipliers[2] : 틱 간격 (초)
// multipliers[3] : VFX 스케일 배율 (기본 1.0, 작게하려면 0.5 등)
[CreateAssetMenu(fileName = "AreaHealEffect", menuName = "Arda/Effect/AreaHealEffect")]
public class AreaHealEffect : SkillEffect
{
    private const int HEAL_AMOUNT_IDX = 0;
    private const int RADIUS_IDX = 1;
    private const int TICK_INTERVAL_IDX = 2;
    private const int VFX_SCALE_IDX = 3;
    private const int TICK_COUNT = 3;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null)
        {
            return;
        }

        ICoroutineRunner runner = context.Caster.GetComponent<ICoroutineRunner>();
        if (runner == null)
        {
            return;
        }

        runner.RunCoroutine(HealRoutine(context, multipliers));
    }

    private IEnumerator HealRoutine(SkillContext context, float[] multipliers)
    {
        if (context.Caster == null)
        {
            yield break;
        }

        int healAmount = Mathf.RoundToInt(GetMultiplier(multipliers, HEAL_AMOUNT_IDX, 20f));
        float radius = GetMultiplier(multipliers, RADIUS_IDX, 5f);
        float interval = GetMultiplier(multipliers, TICK_INTERVAL_IDX, 0.5f);

        float vfxScale = GetMultiplier(multipliers, VFX_SCALE_IDX, 1f);
        GameObject vfxObject = SpawnVfx(context, radius, vfxScale);

        LayerMask enemyLayer = LayerMask.GetMask("Enemy");

        for (int tick = 0; tick < TICK_COUNT; tick++)
        {
            if (context.Caster == null)
            {
                break;
            }

            Vector3 center = context.Caster.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, radius, enemyLayer);

            HashSet<IHealable> healed = new();
            foreach (Collider hit in hits)
            {
                IHealable healable = hit.GetComponentInParent<IHealable>();
                if (healable == null || healed.Contains(healable))
                {
                    continue;
                }

                healed.Add(healable);
                healable.Heal(healAmount);
            }

            yield return new WaitForSeconds(interval);
        }

        if (vfxObject != null)
        {
            Object.Destroy(vfxObject);
        }
    }

    private GameObject SpawnVfx(SkillContext context, float radius, float vfxScale)
    {
        GameObject vfxPrefab = context.Skill?.GetPrefab(SkillPrefabType.CastVFX);
        if (vfxPrefab == null)
        {
            return null;
        }

        Vector3 position = context.Caster.transform.position + Vector3.up * 0.02f;
        GameObject vfxObject = Object.Instantiate(vfxPrefab, position, Quaternion.identity);
        float size = radius * 2f * vfxScale;
        vfxObject.transform.localScale = new Vector3(size, vfxObject.transform.localScale.y, size);
        return vfxObject;
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
