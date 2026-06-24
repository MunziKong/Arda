using System.Collections;
using UnityEngine;

// multipliers[0] : VFX 지속 시간
// multipliers[1] : Y 오프셋 (지면 스냅 후 추가 오프셋)
// multipliers[2] : 스케일 (기본 1.0)
// multipliers[3] : 스폰 딜레이 (초, 생략하거나 0이면 즉시)
[CreateAssetMenu(fileName = "SpawnVfxAtPointEffect", menuName = "Arda/Effect/SpawnVfxAtPointEffect")]
public class SpawnVfxAtPointEffect : SkillEffect
{
    private const int LIFE_TIME_IDX = 0;
    private const int Y_OFFSET_IDX = 1;
    private const int SCALE_IDX = 2;
    private const int DELAY_IDX = 3;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Skill == null)
        {
            return;
        }

        GameObject vfxPrefab = context.Skill.GetPrefab(SkillPrefabType.HitVFX);
        if (vfxPrefab == null)
        {
            return;
        }

        float lifeTime = GetMultiplier(multipliers, LIFE_TIME_IDX, 2f);
        float yOffset = GetMultiplier(multipliers, Y_OFFSET_IDX, 0f);
        float scale = GetMultiplier(multipliers, SCALE_IDX, 1f);
        float delay = GetMultiplier(multipliers, DELAY_IDX, 0f);

        Vector3 spawnPos = GetGroundPosition(context.Point) + Vector3.up * yOffset;

        if (delay > 0f && context.Caster != null)
        {
            ICoroutineRunner runner = context.Caster.GetComponent<ICoroutineRunner>();
            if (runner != null)
            {
                runner.RunCoroutine(DelayedSpawn(vfxPrefab, spawnPos, scale, lifeTime, delay));
                return;
            }
        }

        SpawnVfx(vfxPrefab, spawnPos, scale, lifeTime);
    }

    private IEnumerator DelayedSpawn(GameObject prefab, Vector3 pos, float scale, float lifeTime, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnVfx(prefab, pos, scale, lifeTime);
    }

    private void SpawnVfx(GameObject prefab, Vector3 pos, float scale, float lifeTime)
    {
        GameObject vfxObject = Object.Instantiate(prefab, pos, Quaternion.identity, null);
        vfxObject.SetActive(false);

        float baseScale = vfxObject.transform.localScale.x;
        vfxObject.transform.localScale = Vector3.one * (baseScale * scale);

        foreach (var ps in vfxObject.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        vfxObject.SetActive(true);
        Object.Destroy(vfxObject, lifeTime);
    }

    private Vector3 GetGroundPosition(Vector3 origin)
    {
        int mask = ~LayerMask.GetMask("Player", "Enemy", "Monster");
        if (Physics.Raycast(origin + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, mask))
        {
            return hit.point;
        }
        return origin;
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
