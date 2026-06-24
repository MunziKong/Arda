using UnityEngine;
using UnityEngine.Pool;

public class InteractIconPool : MonoBehaviour
{
    [SerializeField] private InteractIconUI _iconPrefab;
    [SerializeField] private Transform _iconRoot;

    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxPoolSize = 50;

    private ObjectPool<InteractIconUI> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<InteractIconUI>(
            CreateIcon,
            OnGetIcon,
            OnReleaseIcon,
            OnDestroyIcon,
            true,
            _defaultCapacity,
            _maxPoolSize
        );
    }

    public InteractIconUI Get()
    {
        return _pool.Get();
    }

    public void Release(InteractIconUI icon)
    {
        if (icon == null)
        {
            return;
        }

        _pool.Release(icon);
    }

    private InteractIconUI CreateIcon()
    {
        InteractIconUI icon = Instantiate(_iconPrefab, _iconRoot);
        icon.gameObject.SetActive(false);
        return icon;
    }

    private void OnGetIcon(InteractIconUI icon)
    {
        icon.gameObject.SetActive(true);
    }

    private void OnReleaseIcon(InteractIconUI icon)
    {
        icon.Clear();
    }

    private void OnDestroyIcon(InteractIconUI icon)
    {
        Destroy(icon.gameObject);
    }
}