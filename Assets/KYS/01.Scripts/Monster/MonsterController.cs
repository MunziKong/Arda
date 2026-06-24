using System;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    public MonsterData data;

    private EntityPool _pool;
    private string _poolKey;

    private NavMeshAgent _agent;
    private Unity.Behavior.BehaviorGraphAgent _btAgent;

    public Action OnRescued;
    public bool IsReturningHome { get; set; }
    public Vector3 HomePosition { get; set; }
    public float PatrolRadius { get; set; }

    private void Awake()
    {
        _agent = GetComponentInParent<NavMeshAgent>();
        _btAgent = GetComponentInParent<Unity.Behavior.BehaviorGraphAgent>();
    }

    // SpawnPoint가 풀에서 꺼낼 때 호출
    public void Initialize(string poolKey, EntityPool pool)
    {
        OnRescued = null;
        _poolKey  = poolKey;
        _pool     = pool;

        if (_agent != null)
        {
            _agent.enabled = true;
        }
    }

    // MonsterInteraction이 구조 성공 시 호출
    public void Rescue()
    {
        if (_agent != null)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
            }
            _agent.enabled = false;
        }

        OnRescued?.Invoke();
        _pool?.Return(_poolKey, transform.root.gameObject);
    }

}
