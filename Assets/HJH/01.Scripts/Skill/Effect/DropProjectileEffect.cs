using UnityEngine;

[CreateAssetMenu(fileName = "DropProjectileEffect", menuName = "Arda/Effect/DropProjectileEffect")]
public class DropProjectileEffect : SkillEffect
{
    private const int HEIGHT_IDX = 0;
    private const int SPEED_IDX = 1;
    private const int LIFE_TIME_IDX = 2;

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
            Debug.LogWarning($"{context.Skill.skillId}에 Projectile Prefab이 없습니다.");
            return;
        }

        float height = GetMultiplier(multipliers, HEIGHT_IDX, 12f);
        float speed = GetMultiplier(multipliers, SPEED_IDX, 15f);
        float lifeTime = GetMultiplier(multipliers, LIFE_TIME_IDX, 3f);

        Vector3 targetPoint = context.Point;
        Vector3 spawnPosition = targetPoint + Vector3.up * height;

        Vector3 direction = targetPoint - spawnPosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector3.down;
        }

        Projectile projectile = ProjectilePool.Instance.Get(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction.normalized)
        );

        if (projectile == null)
        {
            return;
        }

        ProjectileContext projectileContext = new ProjectileContext
        {
            Owner = context.Caster,
            Target = null,
            TargetPoint = targetPoint,
            AttackPower = 0,
            DamageMultiplier = 0f,
            MoveSpeed = speed,
            LifeTime = lifeTime
        };

        projectile.Initialize(projectileContext);
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
}