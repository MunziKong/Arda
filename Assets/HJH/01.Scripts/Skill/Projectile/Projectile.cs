using System.Collections.Generic;
using UnityEngine;

public enum ProjectileMoveType
{
    Forward,
    TargetPoint,
    Homing,
    TargetCenter,
    Orbit,
    Parabolic
}

public class Projectile : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private ProjectileMoveType _moveType = ProjectileMoveType.Forward;

    [Header("Hit")]
    [SerializeField] private bool _destroyOnHit = true;
    [SerializeField] private bool _canDamage = true;

    private float _currentOrbitAngle;

    private ProjectileContext _context;

    private Vector3 _targetPoint;
    private GameObject _target;

    // 포물선
    private Vector3 _arcStart;
    private Vector3 _arcEnd;
    private float _arcT;

    private readonly HashSet<IDamageable> _hitTargets = new();
    [Header("Rotation Offset")]
    [SerializeField] private Vector3 _rotationOffsetEuler = Vector3.zero;

    private GameObject _originPrefab;
    private Vector3 _moveDirection = Vector3.forward;

    public void SetOriginPrefab(GameObject prefab)
    {
        _originPrefab = prefab;
    }

    public void SetMoveDirection(Vector3 dir)
    {
        _moveDirection = dir.normalized;
    }

    public Quaternion RotationOffset => Quaternion.Euler(_rotationOffsetEuler);

    public void Initialize(ProjectileContext context)
    {
        if (context == null || context.Owner == null)
        {
            Debug.LogWarning("ProjectileContext 또는 Owner가 없습니다.");
            return;
        }

        _context = context;

        _hitTargets.Clear();

        _target = context.Target;
        _targetPoint = context.TargetPoint;

        if (_moveType == ProjectileMoveType.TargetCenter && context.Target != null)
        {
            var col = context.Target.GetComponentInChildren<Collider>();
            _targetPoint = col != null ? col.bounds.center : context.Target.transform.position + Vector3.up;
            _target = null;
        }

        if (_moveType == ProjectileMoveType.Orbit)
        {
            _currentOrbitAngle = context.OrbitAngleOffset;
            UpdateOrbitPosition();
        }

        if (_moveType == ProjectileMoveType.Parabolic)
        {
            _arcStart = transform.position;
            _arcEnd = context.TargetPoint;
            _arcT = 0f;
            return; // 포물선은 타이머 대신 착지로 Despawn
        }

        CancelInvoke();
        Invoke(nameof(Despawn), context.LifeTime);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (_context == null)
        {
            return;
        }

        switch (_moveType)
        {
            case ProjectileMoveType.Forward:
                MoveForward();
                break;

            case ProjectileMoveType.TargetPoint:
                MoveToTargetPoint();
                break;

            case ProjectileMoveType.Homing:
                MoveHoming();
                break;

            case ProjectileMoveType.TargetCenter:
                MoveToTargetCenter();
                break;
            case ProjectileMoveType.Orbit:
                MoveOrbit();
                break;

            case ProjectileMoveType.Parabolic:
                MoveParabolic();
                break;
        }
    }

    private void MoveForward()
    {
        transform.position += _moveDirection * (_context.MoveSpeed * Time.deltaTime);
    }

    private void MoveToTargetPoint()
    {
        Vector3 direction = _targetPoint - transform.position;

        if (direction.sqrMagnitude <= 0.01f)
        {
            Despawn();
            return;
        }

        Vector3 dir = direction.normalized;
        transform.rotation = Quaternion.LookRotation(dir) * RotationOffset;
        transform.position += dir * (_context.MoveSpeed * Time.deltaTime);
    }

    private void MoveHoming()
    {
        if (_target == null)
        {
            MoveForward();
            return;
        }

        Vector3 direction = _target.transform.position - transform.position;

        if (direction.sqrMagnitude <= 0.01f)
        {
            Despawn();
            return;
        }

        Vector3 dir = direction.normalized;
        transform.rotation = Quaternion.LookRotation(dir) * RotationOffset;
        transform.position += dir * (_context.MoveSpeed * Time.deltaTime);
    }

    private void MoveToTargetCenter()
    {
        MoveToTargetPoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canDamage)
        {
            return;
        }

        if (_context == null || _context.Owner == null)
        {
            return;
        }

        if (!IsValidTarget(other))
        {
            return;
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        if (_hitTargets.Contains(damageable))
        {
            return;
        }

        _hitTargets.Add(damageable);

        int damage = Mathf.RoundToInt(_context.AttackPower * _context.DamageMultiplier);
        damageable.TakeDamage(damage);

        if (_destroyOnHit)
        {
            Despawn();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (_moveType != ProjectileMoveType.Orbit)
        {
            return;
        }

        if (_context == null)
        {
            return;
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        _hitTargets.Remove(damageable);
    }

    private bool IsValidTarget(Collider other)
    {
        int ownerLayer = _context.Owner.layer;

        if (ownerLayer == LayerMask.NameToLayer("Player"))
        {
            return other.gameObject.layer == LayerMask.NameToLayer("Enemy");
        }

        if (ownerLayer == LayerMask.NameToLayer("Enemy"))
        {
            return other.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        return false;
    }

    public void Despawn()
    {
        CancelInvoke();

        _context = null;
        _target = null;
        _targetPoint = Vector3.zero;
        _hitTargets.Clear();

        if (ProjectilePool.Instance == null || _originPrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        ProjectilePool.Instance.Release(_originPrefab, this);
    }

    private void MoveOrbit()
    {
        if (_context.Owner == null)
        {
            Despawn();
            return;
        }

        _currentOrbitAngle += _context.MoveSpeed * Time.deltaTime;
        UpdateOrbitPosition();
    }

    private void UpdateOrbitPosition()
    {
        float rad = _currentOrbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * _context.OrbitRadius;
        Vector3 center = _context.Owner.transform.position + Vector3.up * _context.OrbitHeight;

        transform.position = center + offset;
        transform.rotation = Quaternion.LookRotation(offset.normalized) * RotationOffset;
    }

    private void MoveParabolic()
    {
        _arcT += Time.deltaTime;
        float t = Mathf.Clamp01(_arcT / _context.LifeTime);

        float height = _context.ArcHeight * 4f * t * (1f - t);
        Vector3 flatPos = Vector3.Lerp(_arcStart, _arcEnd, t);
        transform.position = flatPos + Vector3.up * height;

        // 진행 방향으로 회전
        float tNext = Mathf.Clamp01((_arcT + 0.05f) / _context.LifeTime);
        float hNext = _context.ArcHeight * 4f * tNext * (1f - tNext);
        Vector3 nextPos = Vector3.Lerp(_arcStart, _arcEnd, tNext) + Vector3.up * hNext;
        Vector3 dir = nextPos - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir.normalized) * RotationOffset;
        }

        if (t >= 1f)
        {
            OnLand();
        }
    }

    private void OnLand()
    {
        if (_context.OnLandVfxPrefab != null)
        {
            int mask = ~LayerMask.GetMask("Player", "Enemy", "Monster");
            Vector3 landPos = transform.position;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 3f, mask))
            {
                landPos = hit.point;
            }

            GameObject vfx = Instantiate(_context.OnLandVfxPrefab, landPos, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * _context.OnLandVfxScale;
            Destroy(vfx, _context.OnLandVfxLifeTime);
        }

        Despawn();
    }
}