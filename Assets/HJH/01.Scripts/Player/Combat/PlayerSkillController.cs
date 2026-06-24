using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour, ICoroutineRunner
{
    [Header("Skills")]
    [SerializeField] private List<SkillDefinition> _skills = new List<SkillDefinition>();

    [Header("Animation")]
    [SerializeField] private PlayerAnimController _anim;

    [Header("Settings")]
    [SerializeField] private float _skillInputBufferTime = 0.8f;
    [SerializeField] private float _aimSkillTimeout = 10f;

    [SerializeField] private float _baseAimToSkillAimDelay = 0.2f;


    private PlayerInputs _inputs;
    private PlayerStats _stats;
    private PlayerMove _move;
    private PlayerAttack _attack;
    private PlayerAimController _aimController;
    private PlayerCameraController _cameraController;

    private SkillDefinition _currentSkill;
    private SkillContext _currentContext;

    private SkillDefinition _pendingAimSkill;
    private Vector3 _confirmedAimPoint;
    private Coroutine _aimTimeoutRoutine;
    private GameObject _confirmedTarget;

    private Dictionary<SkillDefinition, float> _cooldownRemaining = new Dictionary<SkillDefinition, float>();

    private bool _isCasting;
    private bool _isSkillRecovering;
    private bool _isAimingSkill;
    private bool _consumeAttackInput;
    public bool ShouldBlockBaseAttack => _isAimingSkill || _consumeAttackInput;
    private bool _blockBaseAimInput;
    public bool ShouldBlockBaseAim => _isAimingSkill || _blockBaseAimInput;
    public bool IsAimingSkill => _isAimingSkill;

    public event Action<SkillDefinition, float, float> OnSkillCooldownUpdate;

    private void Awake()
    {
        _inputs = GetComponent<PlayerInputs>();
        _stats = GetComponent<PlayerStats>();
        _move = GetComponent<PlayerMove>();
        _attack = GetComponent<PlayerAttack>();
        _aimController = GetComponent<PlayerAimController>();
        _cameraController = GetComponent<PlayerCameraController>();
    }

    private void OnEnable()
    {
        _inputs.SkillEvent += TryUseSkill;
        _inputs.AttackEvent += ConfirmAimSkill;
        _inputs.AimEvent += StopAimSkill;
    }

    private void OnDisable()
    {
        _inputs.SkillEvent -= TryUseSkill;
        _inputs.AttackEvent -= ConfirmAimSkill;
        _inputs.AimEvent -= StopAimSkill;
    }

    // private void Start()
    // {
    //     PrintSkillSlot();
    // }

    private void PrintSkillSlot()
    {
        for (int i = 0; i < _skills.Count; i++)
        {
            Debug.Log($"{i} 슬롯 : {_skills[i].skillId}");
        }
    }

    public void SetSkills(List<SkillDefinition> skills)
    {
        _skills = new(skills);

        PrintSkillSlot();
    }

    private void TryUseSkill(int index)
    {
        if (_isCasting || _isSkillRecovering || _isAimingSkill)
        {
            Debug.Log("스킬 사용 중이라 입력 무시");
            return;
        }

        if (index < 0 || index >= _skills.Count)
        {
            return;
        }

        SkillDefinition skill = _skills[index];

        if (skill == null)
        {
            Debug.LogWarning($"Skill 슬롯 {index + 1}에 등록된 스킬이 없습니다.");
            return;
        }

        if (!IsCooldownReady(skill))
        {
            Debug.Log($"{skill.skillId} 쿨다운 : {Mathf.RoundToInt(GetRemainingCooldown(skill))}초 남음");
            return;
        }

        if (skill.aimMode != AimMode.None)
        {
            StartCoroutine(BeginAimSkillAfterBaseAimExit(skill));
            return;
        }

        UseSkill(skill);
    }

    private IEnumerator BeginAimSkillAfterBaseAimExit(SkillDefinition skill)
    {
        if (_attack != null && _attack.IsBaseAiming)
        {
            _attack.ExitBaseAimMode();

            yield return new WaitForSeconds(_baseAimToSkillAimDelay);
        }

        BeginAimSkill(skill);
    }

    private void BeginAimSkill(SkillDefinition skill)
    {
        _pendingAimSkill = skill;
        _isAimingSkill = true;

        _aimController.EnterAimMode(skill);

        if (_aimTimeoutRoutine != null)
        {
            StopCoroutine(_aimTimeoutRoutine);
        }

        _aimTimeoutRoutine = StartCoroutine(AimSkillTimeoutRoutine());
    }

    private void StopAimSkill()
    {
        if (!_isAimingSkill)
        {
            return;
        }

        _blockBaseAimInput = true;

        if (_aimTimeoutRoutine != null)
        {
            StopCoroutine(_aimTimeoutRoutine);
        }

        CancelAimSkill();

        StartCoroutine(ReleaseBaseAimBlockRoutine());
    }

    private IEnumerator ReleaseBaseAimBlockRoutine()
    {
        yield return null;
        _blockBaseAimInput = false;
    }

    private void ConfirmAimSkill()
    {
        if (!_isAimingSkill)
        {
            return;
        }

        if (_pendingAimSkill == null)
        {
            CancelAimSkill();
            return;
        }

        _consumeAttackInput = true;

        _confirmedAimPoint = _aimController.AimPoint;
        _confirmedTarget = _aimController.AimTarget;

        SkillDefinition skill = _pendingAimSkill;

        if (skill.targetType == SkillTargetType.Target && _confirmedTarget == null)
        {
            Debug.Log("타겟이 잡히지 않았습니다.");

            _consumeAttackInput = false;
            return;
        }

        EndAimSkillMode();

        UseSkill(skill);

        StartCoroutine(ReleaseAttackInputConsumeRoutine());
    }

    private IEnumerator ReleaseAttackInputConsumeRoutine()
    {
        yield return null;
        _consumeAttackInput = false;
    }

    private void CancelAimSkill()
    {
        EndAimSkillMode();

        _pendingAimSkill = null;
    }

    private void EndAimSkillMode()
    {
        _isAimingSkill = false;

        if (_aimTimeoutRoutine != null)
        {
            StopCoroutine(_aimTimeoutRoutine);
            _aimTimeoutRoutine = null;
        }

        _aimController.ExitAimMode();
    }

    private IEnumerator AimSkillTimeoutRoutine()
    {
        yield return new WaitForSeconds(_aimSkillTimeout);

        if (_isAimingSkill)
        {
            CancelAimSkill();
        }
    }

    private void UseSkill(SkillDefinition skill)
    {
        _isCasting = true;
        _currentSkill = skill;

        _move.StartSkillCasting();

        if (skill.actionDirectionMode == ActionDirectionMode.Fixed)
        {
            _cameraController.SetCameraControl(false);
        }

        Debug.Log($"스킬 사용 : {skill.skillId}");

        _anim.ResetSkillTrigger();
        _anim.SetSkillTrigger();
        _anim.SetWeaponType(skill.weaponType);
        _anim.SetSkillIndex(skill);
    }

    public void OnSkillStart()
    {
        if (_currentSkill == null)
        {
            return;
        }

        _currentContext = CreateSkillContext();
        _currentSkill.ExecuteStart(_currentContext);
    }

    public void OnSkillApply()
    {
        if (_currentSkill == null || _currentContext == null)
        {
            return;
        }

        _currentSkill.ExecuteApply(_currentContext);
    }

    public void OnSkillEnd()
    {
        if (_currentSkill != null)
        {
            StartCooldown(_currentSkill);
        }

        _isCasting = false;
        _currentSkill = null;
        _currentContext = null;

        _move.EndSkillCasting();
        _cameraController.SetCameraControl(true);

        _anim.ResetSkillTrigger();
        _confirmedTarget = null;

        StartCoroutine(SkillRecoverRoutine());
    }

    private IEnumerator SkillRecoverRoutine()
    {
        _isSkillRecovering = true;

        yield return new WaitForSeconds(_skillInputBufferTime);

        _isSkillRecovering = false;
    }

    private void StartCooldown(SkillDefinition skill)
    {
        if (skill == null || skill.cooldown <= 0f)
        {
            return;
        }

        StartCoroutine(SkillCooldownRoutine(skill, skill.cooldown));
    }

    private IEnumerator SkillCooldownRoutine(SkillDefinition skill, float duration)
    {
        _cooldownRemaining[skill] = duration;

        while (_cooldownRemaining[skill] > 0f)
        {
            OnSkillCooldownUpdate?.Invoke(skill, _cooldownRemaining[skill], duration);

            yield return null;

            _cooldownRemaining[skill] -= Time.deltaTime;
        }

        _cooldownRemaining[skill] = 0f;
        OnSkillCooldownUpdate?.Invoke(skill, 0f, duration);
    }

    private bool IsCooldownReady(SkillDefinition skill)
    {
        if (skill == null)
        {
            return true;
        }

        if (!_cooldownRemaining.TryGetValue(skill, out float remaining))
        {
            return true;
        }

        return remaining <= 0f;
    }

    private float GetRemainingCooldown(SkillDefinition skill)
    {
        if (skill == null)
        {
            return 0f;
        }

        _cooldownRemaining.TryGetValue(skill, out float remaining);
        return Mathf.Max(0f, remaining);
    }

    private SkillContext CreateSkillContext()
    {
        Vector3 point = transform.position;
        GameObject target = null;

        if (_currentSkill != null && _currentSkill.aimMode != AimMode.None)
        {
            point = _confirmedAimPoint;
        }

        if (_currentSkill != null && _currentSkill.aimMode == AimMode.Crosshair && _currentSkill.targetType == SkillTargetType.Target)
        {
            target = _confirmedTarget;
        }

        float applyPower = Mathf.Max(_stats.MeleePower, _stats.MagicPower);

        return new SkillContext(
            skill: _currentSkill,
            caster: gameObject,
            target: target,
            point: point,
            attackPower: applyPower
        );
    }

    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    public void ResetAllCooldowns()
    {
        StopAllCoroutines();

        foreach (SkillDefinition skill in _cooldownRemaining.Keys)
        {
            OnSkillCooldownUpdate?.Invoke(skill, 0f, 0f);
        }

        _cooldownRemaining.Clear();
        _isCasting = false;
        _isSkillRecovering = false;
        _isAimingSkill = false;
    }
}