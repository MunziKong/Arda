using UnityEngine;

// multipliers[0] : 데미지 배율
// multipliers[1] : 체공 시간 (포물선 이동 완료까지 걸리는 초)
// multipliers[2] : 포물선 최고 높이
// multipliers[3] : 착지 VFX 지속 시간
// multipliers[4] : 착지 VFX 스케일
[CreateAssetMenu(fileName = "SpawnParabolicProjectileEffect", menuName = "Arda/Effect/SpawnParabolicProjectileEffect")]
public class SpawnParabolicProjectileEffect : SkillEffect
{
    private const int DAMAGE_IDX = 0;
    private const int LIFE_TIME_IDX = 1;
    private const int ARC_HEIGHT_IDX = 2;
    private const int VFX_LIFE_TIME_IDX = 3;
    private const int VFX_SCALE_IDX = 4;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null || context.Skill == null)
        {
            return;
        }

        if (ProjectilePool.Instance == null)
        {
            Debug.LogWarning("ProjectilePool이 씬에 없습니다.");
            return;
        }

        GameObject projectilePrefab = context.Skill.GetPrefab(SkillPrefabType.Projectile);
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{context.Skill.skillId}에 Projectile Prefab이 없음.");
            return;
        }

        WeaponHitArea[] hitAreas = context.Caster.GetComponentsInChildren<WeaponHitArea>();
        int slotIndex = context.Skill.weaponSlotIndex;
        WeaponHitArea hitArea = System.Array.Find(hitAreas, h => h.slotIndex == slotIndex);
        if (hitArea == null && hitAreas.Length > 0)
        {
            hitArea = hitAreas[0];
        }

        if (hitArea == null || hitArea.MuzzlePoint == null)
        {
            return;
        }

        Vector3 spawnPosition = hitArea.MuzzlePoint.position;

        Projectile projectile = ProjectilePool.Instance.Get(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (projectile == null)
        {
            return;
        }

        ProjectileContext projectileContext = new ProjectileContext
        {
            Owner = context.Caster,
            Target = context.Target,
            TargetPoint = context.Point,
            AttackPower = Mathf.RoundToInt(context.AttackPower),
            DamageMultiplier = GetMultiplier(multipliers, DAMAGE_IDX, 1f),
            LifeTime = GetMultiplier(multipliers, LIFE_TIME_IDX, 2f),
            ArcHeight = GetMultiplier(multipliers, ARC_HEIGHT_IDX, 3f),
            OnLandVfxPrefab = context.Skill.GetPrefab(SkillPrefabType.HitVFX),
            OnLandVfxLifeTime = GetMultiplier(multipliers, VFX_LIFE_TIME_IDX, 2f),
            OnLandVfxScale = GetMultiplier(multipliers, VFX_SCALE_IDX, 1f),
        };

        projectile.Initialize(projectileContext);
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
