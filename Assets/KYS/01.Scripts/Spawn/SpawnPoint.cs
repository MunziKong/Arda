using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnEntry
{
    public string addressableKey;
    [Range(0f, 100f)] public float weight = 1f;
}

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnType { Monster, Enemy }

    [SerializeField] private SpawnType spawnType;
    [SerializeField] private SpawnEntry[] spawnTable;
    [SerializeField] private float activeRange  = 40f;
    [SerializeField] private float respawnDelay = 60f;

#if UNITY_EDITOR
    [SerializeField] private bool showRangeGizmo = true;
#endif

    private enum State { Empty, Active, WaitingRespawn, WaitingTime, FadingOut }
    private State _state = State.Empty;

    private GameObject _ownedEntity;
    private string     _ownedKey;
    private float      _respawnTimer;
    private bool       _isLoading;
    private Coroutine  _afterNavMeshCoroutine;
    private Coroutine  _fadeInCoroutine;

    // 키별 MonsterData 캐시 — 첫 스폰 때 프리팹에서 읽어 저장
    private readonly Dictionary<string, MonsterData> _dataCache = new();

    public SpawnType Type => spawnType;

    public void Tick(float distToPlayer, EntityPool pool, float deltaTime)
    {
        switch (_state)
        {
            case State.Empty:
                if (!_isLoading && distToPlayer <= activeRange)
                {
                    StartCoroutine(SpawnAsync(pool));
                }
                break;

            case State.Active:
                if (distToPlayer > activeRange)
                {
                    ReturnToPool(pool);
                }
                else if (_ownedKey != null &&
                         _dataCache.TryGetValue(_ownedKey, out var activeData) &&
                         GameTimeManager.Instance != null &&
                         !GameTimeManager.Instance.IsSpawnTime(activeData))
                {
                    ReturnToPool(pool);
                }
                break;

            case State.WaitingRespawn:
                _respawnTimer -= deltaTime;
                if (!_isLoading && _respawnTimer <= 0f && distToPlayer <= activeRange)
                {
                    StartCoroutine(SpawnAsync(pool));
                }
                break;

            case State.WaitingTime:
                if (!_isLoading && distToPlayer <= activeRange && IsCurrentTimeValid())
                {
                    StartCoroutine(SpawnAsync(pool));
                }
                break;

            case State.FadingOut:
                // 페이드 아웃 코루틴 완료 대기
                break;
        }
    }

    private IEnumerator SpawnAsync(EntityPool pool)
    {
        _isLoading = true;
        Debug.Log($"[SpawnPoint] {name} SpawnAsync 시작, state={_state}");

        string key = _ownedKey ?? PickKey();
        if (key == null)
        {
            Debug.Log($"[SpawnPoint] {name} PickKey null → 시간 제한으로 스폰 불가, WaitingTime으로 전환");
            _isLoading = false;
            _state = State.WaitingTime;
            yield break;
        }

        Debug.Log($"[SpawnPoint] {name} 풀에서 '{key}' 요청 중...");
        GameObject entity = null;
        yield return pool.GetAsync(key, go => entity = go);

        if (entity == null)
        {
            Debug.LogWarning($"[SpawnPoint] {name} 풀/어드레서블에서 엔티티를 가져오지 못함 (key={key})");
            _isLoading = false;
            yield break;
        }
        Debug.Log($"[SpawnPoint] {name} 엔티티 획득 성공");

        // MonsterController에서 MonsterData 읽어 캐시 저장 + 시간 체크
        var monsterCtrl = entity.GetComponentInChildren<MonsterController>();
        if (monsterCtrl?.data != null)
        {
            _dataCache[key] = monsterCtrl.data;

            if (GameTimeManager.Instance != null &&
                !GameTimeManager.Instance.IsSpawnTime(monsterCtrl.data))
            {
                pool.Return(key, entity);
                _state     = State.WaitingTime;
                _isLoading = false;
                yield break;
            }
        }

        entity.transform.SetParent(null);

        // NavMesh 표면 위치로 보정해서 땅속 스폰 방지
        Vector3 spawnPos = transform.position;
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out var navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            spawnPos = navHit.position;
        }

        entity.transform.SetPositionAndRotation(spawnPos, transform.rotation);

        if (entity.TryGetComponent<EnemyController>(out var enemy))
        {
            enemy.Initialize(1f, key, pool);
            enemy.OnDied = () => OnEntityDied();
        }
        if (monsterCtrl != null)
        {
            monsterCtrl.Initialize(key, pool);
            monsterCtrl.OnRescued = () => OnEntityDied();
        }

        // BT를 먼저 끄고 SetActive → NavMesh 안착 후 BT 시작
        if (entity.TryGetComponent<Unity.Behavior.BehaviorGraphAgent>(out var bt))
        {
            bt.enabled = false;
        }

        // 페이드 인 준비 (SetActive 전에 알파=0 세팅)
        var fader = entity.GetComponent<EntityFadeController>();
        if (fader != null)
        {
            fader.PrepareForFadeIn();
        }

        entity.SetActive(true);
        StopPendingSpawnCoroutines();
        _afterNavMeshCoroutine = StartCoroutine(StartAfterNavMesh(entity));

        _ownedEntity = entity;
        _ownedKey    = key;
        _state       = State.Active;
        _isLoading   = false;
    }

    private IEnumerator StartAfterNavMesh(GameObject entity)
    {
        yield return null;
        if (entity == null || !entity.activeSelf)
        {
            yield break;
        }

        // 페이드 인 (NavMesh 안착과 병렬 진행)
        var fader = entity.GetComponent<EntityFadeController>();
        if (fader != null)
        {
            _fadeInCoroutine = StartCoroutine(fader.FadeIn());
        }

        if (entity.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var nav) && nav.enabled)
        {
            float timeout = 2f;
            while (!nav.isOnNavMesh && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            // 타임아웃 후에도 NavMesh 위에 없으면 Warp 시도
            if (!nav.isOnNavMesh &&
                UnityEngine.AI.NavMesh.SamplePosition(entity.transform.position, out var warpHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                nav.Warp(warpHit.position);
                yield return null; // Warp 반영 대기
            }

            if (nav.isActiveAndEnabled && nav.isOnNavMesh)
            {
                nav.isStopped = false;
            }
        }

        if (entity.TryGetComponent<Unity.Behavior.BehaviorGraphAgent>(out var bt))
        {
            bt.enabled = true;
            if (bt.GetVariable("HomePosition", out Unity.Behavior.BlackboardVariable<Vector3> homeVar))
            {
                homeVar.Value = transform.position;
            }
        }

        if (entity.TryGetComponent<PlayerBinder>(out var binder))
        {
            binder.Bind();
        }

        _afterNavMeshCoroutine = null;
    }

    private void StopPendingSpawnCoroutines()
    {
        if (_afterNavMeshCoroutine != null)
        {
            StopCoroutine(_afterNavMeshCoroutine);
            _afterNavMeshCoroutine = null;
        }
        if (_fadeInCoroutine != null)
        {
            StopCoroutine(_fadeInCoroutine);
            _fadeInCoroutine = null;
        }
    }

    private void ReturnToPool(EntityPool pool)
    {
        if (_ownedEntity == null)
        {
            _state = State.Empty;
            return;
        }

        StopPendingSpawnCoroutines();

        var fader = _ownedEntity.GetComponent<EntityFadeController>();
        if (fader != null)
        {
            var entity = _ownedEntity;
            var key    = _ownedKey;
            _ownedEntity = null;
            _state       = State.FadingOut;
            StartCoroutine(FadeOutAndReturn(fader, entity, key, pool));
        }
        else
        {
            pool.Return(_ownedKey, _ownedEntity);
            _ownedEntity = null;
            _state       = State.Empty;
        }
    }

    private IEnumerator FadeOutAndReturn(EntityFadeController fader, GameObject entity, string key, EntityPool pool)
    {
        yield return fader.FadeOut();

        // 페이드 중 죽음/구출로 이미 상태가 바뀐 경우 중복 반환 방지
        if (_state == State.FadingOut)
        {
            pool.Return(key, entity);
            _state = State.Empty;
        }
    }

    private void OnEntityDied()
    {
        StopPendingSpawnCoroutines();
        _ownedEntity  = null;
        _ownedKey     = null;
        _respawnTimer = respawnDelay;
        _state        = State.WaitingRespawn;
        Debug.Log($"[SpawnPoint] {name} OnEntityDied → WaitingRespawn, timer={respawnDelay}");
    }

    private bool IsCurrentTimeValid()
    {
        if (GameTimeManager.Instance == null)
        {
            return true;
        }
        foreach (var e in spawnTable)
        {
            if (!_dataCache.TryGetValue(e.addressableKey, out var data))
            {
                return true;
            }
            if (GameTimeManager.Instance.IsSpawnTime(data))
            {
                return true;
            }
        }
        return false;
    }

    public void TrySpawnOnHour(float distToPlayer, EntityPool pool)
    {
        if (_state == State.WaitingTime && !_isLoading && distToPlayer <= activeRange)
        {
            StartCoroutine(SpawnAsync(pool));
        }
    }

    private string PickKey()
    {
        if (spawnTable == null || spawnTable.Length == 0)
        {
            return null;
        }

        // 캐시에 있는 항목은 시간 미리 체크해서 제외
        float total = 0f;
        foreach (var e in spawnTable)
        {
            if (_dataCache.TryGetValue(e.addressableKey, out var data) &&
                GameTimeManager.Instance != null &&
                !GameTimeManager.Instance.IsSpawnTime(data))
            {
                continue;
            }

            total += e.weight;
        }

        if (total <= 0f)
        {
            return null;
        }

        float roll       = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var e in spawnTable)
        {
            if (_dataCache.TryGetValue(e.addressableKey, out var data) &&
                GameTimeManager.Instance != null &&
                !GameTimeManager.Instance.IsSpawnTime(data))
            {
                continue;
            }

            cumulative += e.weight;
            if (roll < cumulative)
            {
                return e.addressableKey;
            }
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (!showRangeGizmo)
        {
            return;
        }
#endif
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activeRange);

        // 실제 스폰 지점 표시
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out var hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // 초록 = NavMesh 위 보정된 스폰 위치
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.position, 0.2f);
            // 빨강 = SpawnPoint 원래 위치 (묻혀있으면 안 보임)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.15f);
        }
        else
        {
            // 마젠타 = NavMesh를 5m 내에서 못 찾음
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
    }
}
