using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "WarningCircleEffect", menuName = "Arda/Effect/WarningCircleEffect")]
public class WarningCircleEffect : SkillEffect
{
    private const int RADIUS_IDX = 0;
    private const int LIFE_TIME_IDX = 1;
    private const int Y_OFFSET_IDX = 2;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Skill == null)
        {
            return;
        }

        float radius = GetMultiplier(multipliers, RADIUS_IDX, 1f);
        float lifeTime = GetMultiplier(multipliers, LIFE_TIME_IDX, 1f);
        float yOffset = GetMultiplier(multipliers, Y_OFFSET_IDX, 0.02f);

        GameObject warningPrefab = context.Skill.GetPrefab(SkillPrefabType.CastVFX);

        if (warningPrefab == null)
        {
            Debug.LogWarning($"{context.Skill.skillId}에 WarningVFX Prefab이 없습니다.");
            return;
        }

        float size = radius * 2f;
        GameObject warningObject;

        if (warningPrefab.GetComponentInChildren<CastIndicator>() != null)
        {
            Vector3 decalPosition = context.Point + Vector3.up * 3f;
            warningObject = Instantiate(warningPrefab, decalPosition, Quaternion.Euler(90f, 0f, 0f));
            warningObject.GetComponentInChildren<CastIndicator>().Initialize(size, lifeTime);
        }
        else if (warningPrefab.GetComponentInChildren<DecalProjector>() != null)
        {
            Vector3 decalPosition = context.Point + Vector3.up * 3f;
            warningObject = Instantiate(warningPrefab, decalPosition, Quaternion.Euler(90f, 0f, 0f));
            warningObject.GetComponentInChildren<DecalProjector>().size = new Vector3(size, size, 6f);
        }
        else
        {
            Vector3 spawnPosition = context.Point + Vector3.up * yOffset;
            warningObject = Instantiate(warningPrefab, spawnPosition, Quaternion.identity);
            warningObject.transform.localScale = new Vector3(size, warningObject.transform.localScale.y, size);
        }

        Destroy(warningObject, lifeTime);
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