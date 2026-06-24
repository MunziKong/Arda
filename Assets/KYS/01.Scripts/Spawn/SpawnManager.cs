using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private EntityPool entityPool;
    [SerializeField] private float checkInterval = 0.5f;

    private SpawnPoint[] _spawnPoints;
    private Transform    _player;

    private void Start()
    {
        _player      = GameObject.FindGameObjectWithTag("Player")?.transform;
        _spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        if (_player == null)
        {
            Debug.LogWarning("[SpawnManager] Player 태그 오브젝트를 찾을 수 없습니다.");
        }

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnHourChanged.AddListener(OnHourChanged);
        }

        StartCoroutine(DistanceCheck());
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnHourChanged.RemoveListener(OnHourChanged);
        }
    }

    private void OnHourChanged(float hour)
    {
        if (_player == null)
        {
            return;
        }
        foreach (var sp in _spawnPoints)
        {
            if (sp == null)
            {
                continue;
            }
            float dist = Vector3.Distance(_player.position, sp.transform.position);
            sp.TrySpawnOnHour(dist, entityPool);
        }
    }

    private IEnumerator DistanceCheck()
    {
        var wait = new WaitForSeconds(checkInterval);
        while (true)
        {
            yield return wait;

            if (_player == null)
            {
                continue;
            }

            foreach (var sp in _spawnPoints)
            {
                if (sp == null)
                {
                    continue;
                }
                float dist = Vector3.Distance(_player.position, sp.transform.position);
                sp.Tick(dist, entityPool, checkInterval);
            }
        }
    }
}
