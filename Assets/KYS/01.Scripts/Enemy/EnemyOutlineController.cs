using EPOOutline;
using UnityEngine;

public class EnemyOutlineController : MonoBehaviour
{
    [SerializeField] private float _detectionRange = 20f;
    [SerializeField] private Outlinable _outlinable;

    private Transform _player;
    private float _nextCheckTime;
    private const float CheckInterval = 0.1f;

    private void Awake()
    {
        if (_outlinable == null)
            _outlinable = GetComponentInChildren<Outlinable>();

        if (_outlinable != null)
            _outlinable.enabled = false;
    }

    private void OnEnable()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + CheckInterval;

        if (_player == null || _outlinable == null) return;

        float distSq = (transform.position - _player.position).sqrMagnitude;
        bool inRange = distSq <= _detectionRange * _detectionRange;

        if (_outlinable.enabled != inRange)
            _outlinable.enabled = inRange;
    }

    private void OnDisable()
    {
        if (_outlinable != null)
            _outlinable.enabled = false;
    }
}
