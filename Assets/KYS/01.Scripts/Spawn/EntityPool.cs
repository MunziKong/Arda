using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EntityPool : MonoBehaviour
{
    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _prefabCache = new();

    // 비활성 상태로 반환 — SpawnPoint에서 위치 설정 후 직접 SetActive(true)
    public System.Collections.IEnumerator GetAsync(string key, System.Action<GameObject> onComplete)
    {
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
        }

        if (_pools[key].Count > 0)
        {
            var pooled = _pools[key].Dequeue();
            pooled.transform.SetParent(null);
            // SetActive는 SpawnPoint가 위치 설정 후 호출
            onComplete(pooled);
            yield break;
        }

        if (!_prefabCache.ContainsKey(key))
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[EntityPool] Addressable 로드 실패: {key}");
                onComplete(null);
                yield break;
            }
            _prefabCache[key] = handle.Result;
        }

        var instance = Instantiate(_prefabCache[key], transform);
        instance.SetActive(false);
        onComplete(instance);
    }

    public void Return(string key, GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
        }
        _pools[key].Enqueue(go);
    }
}
