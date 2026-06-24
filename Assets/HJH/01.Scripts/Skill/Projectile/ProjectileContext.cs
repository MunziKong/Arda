using UnityEngine;

public class ProjectileContext
{
    public GameObject Owner;
    public GameObject Target;
    public Vector3 TargetPoint;

    public int AttackPower;
    public float DamageMultiplier = 1f;
    public float MoveSpeed = 1f;
    public float LifeTime = 1f;

    public float OrbitRadius = 1f;
    public float OrbitAngleOffset = 0f;
    public float OrbitHeight = 1f;

    // 포물선
    public float ArcHeight = 3f;

    // 착지 VFX
    public GameObject OnLandVfxPrefab;
    public float OnLandVfxLifeTime = 2f;
    public float OnLandVfxScale = 1f;
}