using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private AttackData _currentAttackData;

    [Header("Animation")]
    [SerializeField] private PlayerAnimController _anim;

    [SerializeField] private float _baseAimDistance = 20f;

    private bool _isBaseAiming;
    private const float HIT_CHECK_INTERVAL = 0.02f;
    private PlayerInputs _inputs;
    private PlayerEquipment _equipment;
    private PlayerMove _move;
    private PlayerCameraController _cameraController;
    private PlayerSkillController _skill;
    private PlayerStats _stats;
    private PlayerAimController _aimController;

    private int _currentStepIndex = 0;
    private bool _isAttacking = false;
    private bool _canQueueCombo = false;
    private bool _isComboQueued = false;
    private readonly HashSet<IDamageable> _hitTargets = new();
    public bool IsAttacking => _isAttacking;
    public bool IsBaseAiming => _isBaseAiming;
    public AttackData CurrentAttackData => _currentAttackData;

    private void Awake()
    {
        _inputs = GetComponent<PlayerInputs>();
        _equipment = GetComponent<PlayerEquipment>();
        _move = GetComponent<PlayerMove>();
        _cameraController = GetComponent<PlayerCameraController>();
        _skill = GetComponent<PlayerSkillController>();
        _stats = GetComponent<PlayerStats>();
        _aimController = GetComponent<PlayerAimController>();
    }

    private void OnEnable()
    {
        _inputs.AttackEvent += TryAttack;
        _inputs.AimEvent += ToggleBaseAimMode;
    }

    private void OnDisable()
    {
        _inputs.AttackEvent -= TryAttack;
        _inputs.AimEvent -= ToggleBaseAimMode;
    }

    public void SetAttackData(AttackData attackData)
    {
        _currentAttackData = attackData;
    }

    private void TryAttack()
    {
        if (_isAttacking)
        {
            if (_canQueueCombo)
            {
                QueueNextCombo();
            }

            return;
        }

        if (!CanAttack())
        {
            return;
        }

        StartAttack(0);
    }

    private bool CanAttack()
    {
        bool inIdleOrRun = _anim != null && _anim.IsInIdleOrRunState();
        bool inTurn = _anim != null && _anim.IsCurrentState("Turn");

        if (!inIdleOrRun && !inTurn)
        {
            return false;
        }

        if (_skill != null && _skill.ShouldBlockBaseAttack)
        {
            return false;
        }

        if (_currentAttackData == null)
        {
            Debug.LogWarning("현재 장착된 AttackData가 없습니다.");
            return false;
        }

        if (_equipment.CurrentWeapon == null)
        {
            Debug.LogWarning("현재 장착된 WeaponData가 없습니다.");
            return false;
        }

        return true;
    }

    private void StartAttack(int stepIndex)
    {
        AttackStepData stepData = GetStep(stepIndex);

        if (stepData == null)
        {
            EndAttack();
            return;
        }

        _currentStepIndex = stepIndex;
        _isAttacking = true;
        _canQueueCombo = false;
        _isComboQueued = false;

        _anim.SetAttackIndex(_currentAttackData.AnimationIndex);
        _anim.SetAttackStepIndex(stepIndex);
        _anim.SetAttackTrigger();
    }

    private void QueueNextCombo()
    {
        if (_isComboQueued)
        {
            return;
        }

        int nextStepIndex = _currentStepIndex + 1;
        AttackStepData nextStep = GetStep(nextStepIndex);

        if (nextStep == null)
        {
            return;
        }

        _isComboQueued = true;
        _canQueueCombo = false;

        _anim.SetAttackStepIndex(nextStepIndex);
    }

    public void OnAttackStateEnter(int stepIndex)
    {
        _currentStepIndex = stepIndex;
        _isAttacking = true;
        _canQueueCombo = false;
        _isComboQueued = false;
    }

    public void OpenComboWindow()
    {
        int nextStepIndex = _currentStepIndex + 1;

        if (GetStep(nextStepIndex) == null)
        {
            return;
        }

        _canQueueCombo = true;
    }

    public void CloseComboWindow()
    {
        _canQueueCombo = false;
    }

    public void OnAttackApply()
    {
        AttackStepData stepData = GetStep(_currentStepIndex);

        if (stepData == null)
        {
            return;
        }

        if (stepData.SfxClip != null)
        {
            GameCore.SoundManager?.PlaySfx(stepData.SfxClip, stepData.SfxVolume);
        }

        switch (stepData.AttackType)
        {
            case AttackType.Melee:
                ApplyMeleeAttack(stepData);
                break;

            case AttackType.Projectile:
                StartCoroutine(FireProjectiles(stepData));
                break;
        }
    }

    private void ApplyMeleeAttack(AttackStepData stepData)
    {
        StartCoroutine(ApplyMeleeAttackRoutine(stepData));
    }

    private IEnumerator ApplyMeleeAttackRoutine(AttackStepData stepData)
    {
        WeaponHitArea hitArea = GetComponentInChildren<WeaponHitArea>();

        if (hitArea == null ||
            hitArea.HitStart == null ||
            hitArea.HitEnd == null)
        {
            yield break;
        }

        _hitTargets.Clear();

        float timer = 0f;
        hitArea.DebugHitArea = true;
        while (timer < stepData.HitDuration)
        {
            Collider[] hits = Physics.OverlapCapsule(
                hitArea.HitStart.position,
                hitArea.HitEnd.position,
                hitArea.Radius
            );

            foreach (Collider hit in hits)
            {
                if (!IsValidTarget(hit))
                {
                    continue;
                }

                IDamageable damageable =
                    hit.GetComponentInParent<IDamageable>();

                if (damageable == null)
                {
                    continue;
                }

                if (_hitTargets.Contains(damageable))
                {
                    continue;
                }

                _hitTargets.Add(damageable);

                int damage = GetDamage(_currentStepIndex);
                damageable.TakeDamage(damage);
            }

            timer += HIT_CHECK_INTERVAL;
            yield return new WaitForSeconds(HIT_CHECK_INTERVAL);
        }
        hitArea.DebugHitArea = false;
    }

    private IEnumerator FireProjectiles(AttackStepData stepData)
    {
        if (ProjectilePool.Instance == null)
        {
            Debug.LogWarning("ProjectilePool이 씬에 없습니다.");
            yield break;
        }

        if (stepData.ProjectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab이 없습니다.");
            yield break;
        }

        WeaponHitArea hitArea = GetComponentInChildren<WeaponHitArea>();

        if (hitArea == null || hitArea.MuzzlePoint == null)
        {
            yield break;
        }

        int count = Mathf.Max(1, stepData.ProjectileCount);

        for (int i = 0; i < count; i++)
        {
            FireProjectile(stepData, hitArea.MuzzlePoint);

            if (stepData.ProjectileInterval > 0f)
            {
                yield return new WaitForSeconds(stepData.ProjectileInterval);
            }
        }
    }

    private void FireProjectile(AttackStepData stepData, Transform muzzlePoint)
    {
        Vector3 direction = GetProjectileDirection(stepData, muzzlePoint);

        Projectile projectile = ProjectilePool.Instance.Get(
            stepData.ProjectilePrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (projectile == null)
        {
            return;
        }

        ProjectileContext context = new ProjectileContext
        {
            Owner = gameObject,
            Target = null,
            TargetPoint = muzzlePoint.position + direction * 50f,
            AttackPower = Mathf.Max(_stats.MeleePower, _stats.MagicPower),
            DamageMultiplier = stepData.DamageMultiplier,
            MoveSpeed = stepData.ProjectileSpeed,
            LifeTime = stepData.ProjectileLifeTime
        };

        projectile.Initialize(context);
    }

    private Vector3 GetProjectileDirection(AttackStepData stepData, Transform muzzlePoint)
    {
        if (_isBaseAiming && _aimController != null)
        {
            Vector3 directionToAimPoint = _aimController.AimPoint - muzzlePoint.position;

            if (directionToAimPoint.sqrMagnitude > 0.0001f)
            {
                return directionToAimPoint.normalized;
            }
        }

        Vector3 direction = transform.forward;

        if (stepData.SpreadAngle == 0f)
        {
            return direction;
        }

        float halfSpread = stepData.SpreadAngle * 0.5f;
        float randomAngle = Random.Range(-halfSpread, halfSpread);

        return Quaternion.AngleAxis(randomAngle, Vector3.up) * direction;
    }

    public void OnAttackEnd()
    {
        if (_isComboQueued)
        {
            return;
        }

        EndAttack();
    }

    private void EndAttack()
    {
        _isAttacking = false;
        _canQueueCombo = false;
        _isComboQueued = false;
        _currentStepIndex = 0;

        _move.StopActionMove();
        _cameraController.SetCameraControl(true);
    }

    public void OnAttackMoveStart(int stepIndex)
    {
        AttackStepData stepData = GetStep(stepIndex);

        if (stepData == null || !stepData.UseMove)
        {
            return;
        }

        if (stepData.ActionDirectionMode == ActionDirectionMode.Fixed)
        {
            _cameraController.SetCameraControl(false);
        }

        _move.StartActionMove(
            stepData.ActionDirectionMode,
            transform.forward,
            stepData.MoveSpeed,
            stepData.MoveDuration
        );
    }

    public void OnAttackMoveEnd()
    {
        _move.StopActionMove();
        _cameraController.SetCameraControl(true);
    }

    public void EquipAttackData(AttackData attackData)
    {
        if (attackData == null)
        {
            Debug.LogWarning("장착할 AttackData가 없습니다.");
            return;
        }

        _currentAttackData = attackData;
    }

    public int GetDamage(int stepIndex)
    {
        AttackStepData stepData = GetStep(stepIndex);

        if (stepData == null)
        {
            return 0;
        }

        float damage = GetBaseAttackPower() * stepData.DamageMultiplier;
        return Mathf.RoundToInt(damage);
    }

    public int GetCurrentDamage()
    {
        return GetDamage(_currentStepIndex);
    }

    private float GetBaseAttackPower()
    {
        if (_stats == null)
        {
            return 0f;
        }

        return Mathf.Max(_stats.MeleePower, _stats.MagicPower);
    }

    private AttackStepData GetStep(int index)
    {
        if (_currentAttackData == null)
        {
            return null;
        }

        return _currentAttackData.GetStep(index);
    }

    private bool IsValidTarget(Collider other)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            return other.gameObject.layer == LayerMask.NameToLayer("Enemy");
        }

        if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            return other.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        return false;
    }

    private void ToggleBaseAimMode()
    {
        if (_skill != null && _skill.ShouldBlockBaseAim)
        {
            return;
        }

        if (!IsMagicWeapon())
        {
            return;
        }

        if (_isBaseAiming)
        {
            ExitBaseAimMode();
        }
        else
        {
            EnterBaseAimMode();
        }
    }

    private bool IsMagicWeapon()
    {
        if (_stats == null)
        {
            return false;
        }

        return _stats.MagicPower > _stats.MeleePower;
    }

    private void EnterBaseAimMode()
    {
        _isBaseAiming = true;

        if (_aimController != null)
        {
            _aimController.EnterBaseAimMode(_baseAimDistance);
        }
    }

    public void ExitBaseAimMode()
    {
        _isBaseAiming = false;

        if (_aimController != null)
        {
            _aimController.ExitAimMode();
        }
    }

}