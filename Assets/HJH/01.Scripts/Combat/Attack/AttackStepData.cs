using UnityEngine;
using System;

public enum AttackType
{
    Melee,
    Projectile
}

[Serializable]
public class AttackStepData
{
    [Header("Attack")]
    [SerializeField] private AttackType _attackType = AttackType.Melee;
    [SerializeField] private float _damageMultiplier = 1f;

    [Header("Move")]
    [SerializeField] private bool _useMove = false;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _moveDuration = 0.2f;
    [SerializeField] private ActionDirectionMode _directionMode = ActionDirectionMode.Dynamic;

    [Header("Melee")]
    [SerializeField] private float _hitDuration = 0.1f;

    [Header("Projectile")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private int _projectileCount = 1;
    [SerializeField] private float _projectileInterval = 0.1f;
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _projectileLifeTime = 3f;
    [SerializeField] private float _spreadAngle = 0f;

    [Header("SFX")]
    [SerializeField] private AudioClip _sfxClip;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

    public AudioClip SfxClip => _sfxClip;
    public float SfxVolume => _sfxVolume;

    public AttackType AttackType => _attackType;
    public float DamageMultiplier => _damageMultiplier;

    public bool UseMove => _useMove;
    public float MoveSpeed => _moveSpeed;
    public float MoveDuration => _moveDuration;
    public ActionDirectionMode ActionDirectionMode => _directionMode;

    public GameObject ProjectilePrefab => _projectilePrefab;
    public int ProjectileCount => _projectileCount;
    public float ProjectileInterval => _projectileInterval;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifeTime => _projectileLifeTime;
    public float SpreadAngle => _spreadAngle;
    public float HitDuration => _hitDuration;
}