using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnProjectileEffect", menuName = "Arda/Effect/SpawnProjectileEffect")]
public class SpawnProjectileEffect : SkillEffect
{
    private const int DAMAGE_MULTIPLIER_IDX = 0;
    private const int SPEED_MULTIPLIER_IDX = 1;
    private const int LIFE_TIME_MULTIPLIER_IDX = 2;
    private const int SPAWN_DELAY_IDX = 3;

    public override void Execute(SkillContext context, float[] multipliers)
    {
        if (context == null || context.Caster == null) return;
        if (context.Skill == null) return;
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

        WeaponHitArea hitArea = context.Caster.GetComponentInChildren<WeaponHitArea>();
        if (hitArea == null || hitArea.MuzzlePoint == null) return;

        ProjectileContext projectileContext = CreateProjectileContext(context, multipliers);

        Vector3 muzzlePosition = hitArea.MuzzlePoint.position;
        SpawnMuzzleVFX(context, muzzlePosition, hitArea.MuzzlePoint.rotation);

        float spawnDelay = GetMultiplier(multipliers, SPAWN_DELAY_IDX, 0f);

        if (spawnDelay <= 0f)
        {
            Fire(context, projectilePrefab, hitArea, projectileContext);
        }
        else
        {
            ProjectilePool.Instance.StartCoroutine(
                DelayedFire(context, projectilePrefab, hitArea, projectileContext, spawnDelay)
            );
        }
    }

    private IEnumerator DelayedFire(SkillContext context, GameObject projectilePrefab, WeaponHitArea hitArea, ProjectileContext projectileContext, float delay)
    {
        yield return new WaitForSeconds(delay);
        Fire(context, projectilePrefab, hitArea, projectileContext);
    }

    private void Fire(SkillContext context, GameObject projectilePrefab, WeaponHitArea hitArea, ProjectileContext projectileContext)
    {
        Vector3 spawnPosition = hitArea.MuzzlePoint.position;
        Vector3 direction = GetProjectileDirection(context, spawnPosition);

        Projectile projectile = ProjectilePool.Instance.Get(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        if (projectile == null) return;

        projectile.Initialize(projectileContext);
        GameCore.SoundManager?.PlaySkill(context.Sfx.clip, context.Sfx.volume, context.Sfx.pitch);
    }

    private ProjectileContext CreateProjectileContext(SkillContext context, float[] multipliers)
    {
        return new ProjectileContext
        {
            Owner = context.Caster,
            Target = context.Target,
            TargetPoint = context.Point,
            AttackPower = Mathf.RoundToInt(context.AttackPower),
            DamageMultiplier = GetMultiplier(multipliers, DAMAGE_MULTIPLIER_IDX, 1f),
            MoveSpeed = GetMultiplier(multipliers, SPEED_MULTIPLIER_IDX, 5f),
            LifeTime = GetMultiplier(multipliers, LIFE_TIME_MULTIPLIER_IDX, 3f)
        };
    }

    private Vector3 GetProjectileDirection(SkillContext context, Vector3 spawnPosition)
    {
        if (context.Skill.aimMode == AimMode.Crosshair)
        {
            Vector3 directionToAimPoint = context.Point - spawnPosition;

            if (directionToAimPoint.sqrMagnitude > 0.0001f)
            {
                return directionToAimPoint.normalized;
            }
        }

        return context.Caster.transform.forward;
    }

    private void SpawnMuzzleVFX(SkillContext context, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = context.Skill.GetPrefab(SkillPrefabType.MuzzleVFX);
        if (prefab == null) return;

        Quaternion finalRotation = rotation * prefab.transform.rotation;
        GameObject vfx = Object.Instantiate(prefab, position, finalRotation);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2f;
        Object.Destroy(vfx, lifetime);
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