using UnityEngine;

public class WeaponHitArea : MonoBehaviour
{

    [SerializeField] private Transform _hitStart;
    [SerializeField] private Transform _hitEnd;
    [SerializeField] private float _radius = 1f;

    [SerializeField] private Transform _muzzlePoint;

    [Header("Weapon Slot")]
    public int slotIndex = 0;


    public Transform HitStart => _hitStart;
    public Transform HitEnd => _hitEnd;
    public float Radius => _radius;
    public bool DebugHitArea;
    public Transform MuzzlePoint => _muzzlePoint;


    public float GetDistance()
    {
        if (_hitStart == null || _hitEnd == null)
        {
            return 0f;
        }

        return Vector3.Distance(_hitStart.position, _hitEnd.position);
    }

    private void OnDrawGizmos()
    {
        if (!DebugHitArea)
        {
            return;
        }

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(_hitStart.position, _radius);
        Gizmos.DrawWireSphere(_hitEnd.position, _radius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_hitStart.position, _hitEnd.position);
    }
}
