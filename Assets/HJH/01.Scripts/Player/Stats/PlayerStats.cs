using System.Collections;
using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable, IStunnable, IRootable
{
    [SerializeField] private CharacterStatsDefinition _playerStats;
    [SerializeField] private PlayerDeathHandler _deathHandler;

    [Header("VFX")]
    [SerializeField] private GameObject _stunVFX;

    [Header("확인용 - 임의 수정 x")]
    [SerializeField] private int _maxHp;
    [SerializeField] private int _currentHp;

    [SerializeField] private int _baseMeleePower;
    [SerializeField] private int _baseMagicPower;

    [SerializeField] private int _weaponMeleePower;
    [SerializeField] private int _weaponMagicPower;
    [SerializeField] private float _powerBuffValue;
    [SerializeField] private float _speedBuffValue;

    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;

    public int MaxHp => _maxHp;
    public int CurrentHp => _currentHp;

    // 실제 공격용 프로퍼티
    public int MeleePower => Mathf.RoundToInt((_baseMeleePower + _weaponMeleePower) * _powerBuffValue);
    public int MagicPower => Mathf.RoundToInt((_baseMagicPower + _weaponMagicPower) * _powerBuffValue);

    public float WalkSpeed => _walkSpeed * _speedBuffValue;
    public float RunSpeed => _runSpeed * _speedBuffValue;

    public Action HitEvent;
    public Action SpeedChangeEvent;
    public Action<int, int> ChangeHpEvent;


    private void Awake()
    {
        InitalizeStats();
    }



    private void InitalizeStats()
    {
        _maxHp = _playerStats.maxHp;
        _currentHp = _maxHp;

        _baseMeleePower = _playerStats.meleePower;
        _baseMagicPower = _playerStats.magicPower;

        _weaponMeleePower = 0;
        _weaponMagicPower = 0;

        _walkSpeed = _playerStats.walkSpeed;
        _runSpeed = _playerStats.runSpeed;
    }

    public void TakeDamage(int amount)
    {
        _currentHp -= amount;
        _currentHp = Mathf.Clamp(_currentHp, 0, _maxHp);

        Debug.Log($"<color=red>[-{amount} HP]</color> ");
        Debug.Log($"Current HP : <color=yellow>{_currentHp}</color> / {_maxHp}");

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            ChangeHpEvent?.Invoke(_currentHp, _maxHp);
            _deathHandler?.HandleDeath();
            return;
        }

        HitEvent?.Invoke();
        ChangeHpEvent?.Invoke(_currentHp, _maxHp);
    }

    public void Heal(int amount)
    {
        _currentHp += amount;
        _currentHp = Mathf.Clamp(_currentHp, 0, _maxHp);
        ChangeHpEvent?.Invoke(_currentHp, _maxHp);
    }


    public void Stun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        var move = GetComponent<PlayerMove>();
        var skill = GetComponent<PlayerSkillController>();

        if (move)
        {
            move.enabled = false;
        }
        if (skill)
        {
            skill.enabled = false;
        }

        _stunVFX?.SetActive(true);

        yield return new WaitForSeconds(duration);

        _stunVFX?.SetActive(false);

        if (move)
        {
            move.enabled = true;
        }
        if (skill)
        {
            skill.enabled = true;
        }
    }

    public void Root(float duration)
    {
        StartCoroutine(RootCoroutine(duration));
    }

    private IEnumerator RootCoroutine(float duration)
    {
        var move = GetComponent<PlayerMove>();

        if (move)
        {
            move.enabled = false;
        }

        yield return new WaitForSeconds(duration);

        if (move)
        {
            move.enabled = true;
        }
    }


    // Stats UI 구현 이후 무기 변경 시 Stats UI 변경 넣어야됨..
    public void RestoreHp(int hp)
    {
        _currentHp = Mathf.Clamp(hp, 1, _maxHp);
        ChangeHpEvent?.Invoke(_currentHp, _maxHp);
    }

    public void SetWeaponMeleePower(int damage)
    {
        _weaponMeleePower = damage;
    }

    public void SetWeaponMagicPower(int damage)
    {
        _weaponMagicPower = damage;
    }

    public void SetPowerValue(float value)
    {
        _powerBuffValue = value;
    }

    public void SetSpeedValue(float value)
    {
        _speedBuffValue = value;
        SpeedChangeEvent?.Invoke();
    }
}