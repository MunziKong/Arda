using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Pool Setting")]
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxSize = 50;

    [Header("Hierarchy")]
    [SerializeField] private Transform _projectileRoot;

    private readonly Dictionary<GameObject, ObjectPool<Projectile>> _pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private ObjectPool<Projectile> GetOrCreatePool(GameObject prefab)
    {
        if (_pools.TryGetValue(prefab, out ObjectPool<Projectile> pool))
        {
            return pool;
        }

        ObjectPool<Projectile> newPool = new ObjectPool<Projectile>(
            createFunc: () => CreateProjectile(prefab),
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyProjectile,
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );

        _pools.Add(prefab, newPool);

        return newPool;
    }

    private Projectile CreateProjectile(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, _projectileRoot);

        Projectile projectile = obj.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError($"{prefab.name} 프리팹에 Projectile 컴포넌트가 없습니다.");
            Destroy(obj);
            return null;
        }

        projectile.SetOriginPrefab(prefab);
        obj.SetActive(false);

        return projectile;
    }

    public Projectile Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        ObjectPool<Projectile> pool = GetOrCreatePool(prefab);
        Projectile projectile = pool.Get();

        projectile.transform.SetPositionAndRotation(position, rotation * projectile.RotationOffset);
        projectile.SetMoveDirection(rotation * Vector3.forward);
        projectile.SetOriginPrefab(prefab);

        return projectile;
    }

    public void Release(GameObject prefab, Projectile projectile)
    {
        if (prefab == null || projectile == null)
        {
            return;
        }

        if (!_pools.TryGetValue(prefab, out ObjectPool<Projectile> pool))
        {
            Destroy(projectile.gameObject);
            return;
        }

        pool.Release(projectile);
    }



    private void OnGet(Projectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.gameObject.SetActive(true);
    }

    private void OnRelease(Projectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        Destroy(projectile.gameObject);
    }
}