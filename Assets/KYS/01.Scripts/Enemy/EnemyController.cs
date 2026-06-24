using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IDamageable, IHealable, ICoroutineRunner
{
    public EnemyData data;
    public bool IsAttacking { get; set; }

    private float currentHp;
    private NavMeshAgent agent;
    private Animator _animator;

    private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    private float _alertedUntil;
    public bool IsAlerted => Time.time < _alertedUntil;

    [Tooltip("공격 받았을 때 강제 전투 상태 유지 시간(초)")]
    public float alertDuration = 5f;

    [Tooltip("사망 애니메이션 후 풀 반납까지 대기 시간(초)")]
    public float deathDestroyDelay = 3f;

    public bool IsDead => _isDead;
    private bool _isDead;

    public bool IsReturningHome { get; set; }
    public Vector3 HomePosition { get; set; }
    public float PatrolRadius { get; set; }
    private Collider _collider;
    private Unity.Behavior.BehaviorGraphAgent _btAgent;

    [SerializeField] private EnemyHealthBar _healthBar;
    [SerializeField] private EnemyHitFlash _hitFlash;

    // 풀 반납용
    private EntityPool _pool;
    private string _poolKey;

    // SpawnPoint가 구독 - 사망 시 호출됨
    public Action OnDied;

    public float HpRatio => data != null ? currentHp / data.maxHp : 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<Collider>();
        _btAgent = GetComponent<Unity.Behavior.BehaviorGraphAgent>();
    }

    // 풀에서 꺼낼 때 SpawnPoint가 호출
    public void Initialize(float hpRatio, string poolKey, EntityPool pool)
    {
        OnDied = null;
        _isDead = false;
        IsAttacking = false;
        _alertedUntil = 0f;
        _poolKey = poolKey;
        _pool = pool;

        currentHp = data.maxHp * hpRatio;
        _healthBar?.Hide();

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = data.moveSpeed;
            // isStopped / ResetPath 는 SetActive(true) 후 NavMesh에 올라간 뒤 호출해야 함
        }
        if (_collider != null)
        {
            _collider.enabled = true;
        }
        if (_btAgent != null)
        {
            _btAgent.enabled = false;
        }
        if (_animator != null)
        {
            _animator.Rebind();
        }
    }

    private void Update()
    {
        if (_animator == null || agent == null || _isDead)
        {
            return;
        }
        float speed = agent.velocity.magnitude / Mathf.Max(data.moveSpeed, 0.01f);
        _animator.SetFloat("SpeedMagnitude", speed);
    }

    public void Heal(int amount)
    {
        if (_isDead)
        {
            return;
        }
        currentHp = Mathf.Min(currentHp + amount, data.maxHp);
        _healthBar?.Show(HpRatio);
    }

    public void TakeDamage(int amount)
    {
        if (_isDead)
        {
            return;
        }

        currentHp -= amount;
        Debug.Log($"[Enemy] {gameObject.name} 공격 받음: -{amount} | 현재 HP: {currentHp} / {data.maxHp}");

        _healthBar?.Show(HpRatio);
        _hitFlash?.Flash();
        GameCore.SoundManager?.PlayEnemyHit();
        AlertByAttack();

        if (!IsAttacking && _animator != null)
        {
            _animator.SetTrigger(HitTriggerHash);
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void AlertByAttack()
    {
        _alertedUntil = Time.time + alertDuration;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    private void LateUpdate()
    {
        if (!_isDead || _animator == null)
        {
            return;
        }
        // BT가 Update에서 애니메이터를 덮어쓰므로 LateUpdate에서 Die 트리거 재설정
        // Die 상태 shortNameHash == DieTriggerHash 이면 이미 전환됐으므로 스킵
        if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != DieTriggerHash)
        {
            _animator.SetTrigger(DieTriggerHash);
        }
    }

    private void Die()
    {
        _isDead = true;
        IsAttacking = true;
        _healthBar?.Hide();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        if (_animator != null)
        {
            _animator.SetTrigger(DieTriggerHash);
        }

        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        yield return new WaitForSeconds(deathDestroyDelay);

        GiveDrop();
        QuestManager.Instance?.ReportKill(data);
        OnDied?.Invoke();
        _pool?.Return(_poolKey, gameObject);
    }

    private void GiveDrop()
    {
        if (data == null || data.dropTable == null || data.dropTable.Count == 0)
        {
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }

        foreach (var entry in data.dropTable)
        {
            Debug.Log("entry : " + entry.item.ItemName);
            if (entry.item == null)
            {
                continue;
            }

            float randomValue = UnityEngine.Random.value;
            if (randomValue > entry.dropChance)
            {
                Debug.Log($"randomValue : {randomValue} / entry.dropChance : {entry.dropChance} ");
                continue;
            }

            int amount = UnityEngine.Random.Range(entry.minQuantity, entry.maxQuantity + 1);
            inventory.AddItem(entry.item, amount);

            if (entry.item is CurrencyItemData)
            {
                GameCore.MessageUIController?.Enqueue(new MessageData
                {
                    Type = PopupMessageType.Gold,
                    Quantity = amount
                });
            }
            else
            {
                GameCore.MessageUIController?.Enqueue(new MessageData
                {
                    Type = PopupMessageType.Item,
                    Icon = entry.item.Icon,
                    Name = entry.item.ItemName,
                    Quantity = amount
                });
            }
        }
    }
}
