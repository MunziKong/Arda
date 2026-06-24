using UnityEngine;

[CreateAssetMenu(fileName = "SpawnOrbitProjectileEffect", menuName = "Arda/Effect/SpawnOrbitProjectileEffect")]
public class SpawnOrbitProjectileEffect : SkillEffect
{
    private const int DAMAGE_MULTIPLIER_IDX = 0;
    private const int RADIUS_IDX = 1;
    private const int DURATION_IDX = 2;
    private const int ORBIT_SPEED_IDX = 3;
    private const int PROJECTILE_COUNT_IDX = 4;
    private const int ORBIT_HEIGHT_IDX = 5;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null || context.Skill == null)
        {
            return;
        }

        if (ProjectilePool.Instance == null)
        {
            return;
        }

        GameObject projectilePrefab = context.Skill.GetPrefab(SkillPrefabType.Projectile);

        if (projectilePrefab == null)
        {
            return;
        }

        float damageMultiplier = GetMultiplier(multipliers, DAMAGE_MULTIPLIER_IDX, 1f);
        float radius = GetMultiplier(multipliers, RADIUS_IDX, 2f);
        float duration = GetMultiplier(multipliers, DURATION_IDX, 5f);
        float orbitSpeed = GetMultiplier(multipliers, ORBIT_SPEED_IDX, 90f);
        int count = Mathf.Max(1, Mathf.RoundToInt(GetMultiplier(multipliers, PROJECTILE_COUNT_IDX, 3f)));
        float orbitHeight = GetMultiplier(multipliers, ORBIT_HEIGHT_IDX, 1.2f);

        float angleStep = 360f / count;
        Vector3 center = context.Caster.transform.position + Vector3.up * orbitHeight;

        for (int i = 0; i < count; i++)
        {
            float angleOffset = angleStep * i;
            float rad = angleOffset * Mathf.Deg2Rad;

            Vector3 spawnPos = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

            Projectile projectile = ProjectilePool.Instance.Get(
                projectilePrefab,
                spawnPos,
                Quaternion.identity
            );

            if (projectile == null)
            {
                continue;
            }

            ProjectileContext projectileContext = new ProjectileContext
            {
                Owner = context.Caster,
                Target = null,
                TargetPoint = Vector3.zero,
                AttackPower = Mathf.RoundToInt(context.AttackPower),
                DamageMultiplier = damageMultiplier,
                MoveSpeed = orbitSpeed,
                LifeTime = duration,
                OrbitRadius = radius,
                OrbitAngleOffset = angleOffset,
                OrbitHeight = orbitHeight
            };

            projectile.Initialize(projectileContext);
            GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);
        }
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