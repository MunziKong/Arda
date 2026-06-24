using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// multipliers[0] : 틱당 데미지
// multipliers[1] : 범위 반경
// multipliers[2] : 틱 간격 (초)
// multipliers[3] : 틱 횟수
// multipliers[4] : 첫 틱까지 딜레이 (포물선 체공 시간과 맞춤)
[CreateAssetMenu(fileName = "AreaPoisonEffect", menuName = "Arda/Effect/AreaPoisonEffect")]
public class AreaPoisonEffect : SkillEffect
{
    private const int DAMAGE_IDX = 0;
    private const int RADIUS_IDX = 1;
    private const int TICK_INTERVAL_IDX = 2;
    private const int TICK_COUNT_IDX = 3;
    private const int DELAY_IDX = 4;

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

        runner.RunCoroutine(PoisonRoutine(context, multipliers));
    }

    private IEnumerator PoisonRoutine(SkillContext context, float[] multipliers)
    {
        int damage = Mathf.RoundToInt(GetMultiplier(multipliers, DAMAGE_IDX, 5f));
        float radius = GetMultiplier(multipliers, RADIUS_IDX, 3f);
        float interval = GetMultiplier(multipliers, TICK_INTERVAL_IDX, 1f);
        int tickCount = Mathf.RoundToInt(GetMultiplier(multipliers, TICK_COUNT_IDX, 3f));
        float delay = GetMultiplier(multipliers, DELAY_IDX, 0f);

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        int targetMask = GetTargetMask(context.Caster);
        Vector3 center = context.Point;

        HashSet<IDamageable> poisoned = new HashSet<IDamageable>();

        Collider[] cols = Physics.OverlapSphere(center, radius, targetMask);
        foreach (Collider col in cols)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                poisoned.Add(damageable);
            }
        }

        for (int tick = 0; tick < tickCount; tick++)
        {
            foreach (IDamageable target in poisoned)
            {
                target.TakeDamage(damage);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private int GetTargetMask(GameObject caster)
    {
        int ownerLayer = caster.layer;
        if (ownerLayer == LayerMask.NameToLayer("Player"))
        {
            return LayerMask.GetMask("Enemy");
        }
        if (ownerLayer == LayerMask.NameToLayer("Enemy"))
        {
            return LayerMask.GetMask("Player");
        }
        return 0;
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
