using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimController : MonoBehaviour
{
    private Animator _animator;

    [Header("State")]
    [SerializeField] private string _idleWalkStateName = "Idle Walk Blend";
    [SerializeField] private string _runStateName = "Run";

    [Header("Move Parameters")]
    [SerializeField] private string _moveXParamName = "MoveX";
    [SerializeField] private string _moveZParamName = "MoveZ";
    [SerializeField] private string _runningParamName = "IsRunning";

    [Header("Turn Parameters")]
    [SerializeField] private string _turnParamName = "Turn";

    [Header("Roll Parameters")]
    [SerializeField] private string _rollParamName = "Roll";
    [SerializeField] private string _rollXParamName = "RollX";
    [SerializeField] private string _rollZParamName = "RollZ";

    [Header("Jump Parameters")]
    [SerializeField] private string _landingParamName = "Landing";
    [SerializeField] private string _jumpParamName = "Jump";
    [SerializeField] private string _fallEnterParamName = "FallEnter";

    [Header("Attack Parameters")]
    [SerializeField] private string _attackParamName = "Attack";
    [SerializeField] private string _attackIndexParamName = "AttackIndex";
    [SerializeField] private string _attackStepIndexParamName = "AttackStepIndex";

    [Header("Skill Parameters")]
    [SerializeField] private string _skillParamName = "Skill";
    [SerializeField] private string _weaponTypeParamName = "WeaponType";
    [SerializeField] private string _skillIndexParamName = "SkillIndex";

    [Header("Hit Parameters")]
    [SerializeField] private string _hitParamName = "Hit";
    [SerializeField] private string _deathParamName = "Death";



    private int _moveXHash;
    private int _moveZHash;
    private int _runningHash;

    private int _rollHash;
    private int _rollXHash;
    private int _rollZHash;

    private int _landingHash;
    private int _jumpHash;
    private int _fallEnterHash;

    private int _attackHash;
    private int _attackIndexHash;
    private int _attackStepIndexHash;

    private int _skillHash;
    private int _weaponTypeHash;
    private int _skillIndexHash;

    private int _hitHash;

    private int _turnHash;
    private int _deathHash;

    private PlayerAttack _attack;
    private PlayerSkillController _skill;
    private PlayerStats _stats;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _moveXHash = Animator.StringToHash(_moveXParamName);
        _moveZHash = Animator.StringToHash(_moveZParamName);
        _runningHash = Animator.StringToHash(_runningParamName);

        _rollHash = Animator.StringToHash(_rollParamName);
        _rollXHash = Animator.StringToHash(_rollXParamName);
        _rollZHash = Animator.StringToHash(_rollZParamName);

        _landingHash = Animator.StringToHash(_landingParamName);
        _jumpHash = Animator.StringToHash(_jumpParamName);
        _fallEnterHash = Animator.StringToHash(_fallEnterParamName);

        _attackHash = Animator.StringToHash(_attackParamName);
        _attackIndexHash = Animator.StringToHash(_attackIndexParamName);
        _attackStepIndexHash = Animator.StringToHash(_attackStepIndexParamName);

        _skillHash = Animator.StringToHash(_skillParamName);
        _weaponTypeHash = Animator.StringToHash(_weaponTypeParamName);
        _skillIndexHash = Animator.StringToHash(_skillIndexParamName);

        _hitHash = Animator.StringToHash(_hitParamName);
        _turnHash = Animator.StringToHash(_turnParamName);
        _deathHash = Animator.StringToHash(_deathParamName);

        _attack = GetComponentInParent<PlayerAttack>();
        _skill = GetComponentInParent<PlayerSkillController>();
        _stats = GetComponentInParent<PlayerStats>();
    }

    private void OnEnable()
    {
        _stats.HitEvent += SetHitTrigger;
    }

    private void OnDisable()
    {
        _stats.HitEvent -= SetHitTrigger;
    }

    // Move 관련
    public void SetMoveDirection(float moveX, float moveZ)
    {
        _animator.SetFloat(_moveXHash, moveX, 0.1f, Time.deltaTime);
        _animator.SetFloat(_moveZHash, moveZ, 0.1f, Time.deltaTime);
    }

    public void SetRunning(bool isRunning)
    {
        _animator.SetBool(_runningHash, isRunning);

    }

    // Roll 관련
    public void SetRollTrigger()
    {
        _animator.SetTrigger(_rollHash);
    }

    public void SetRollDirection(float x, float z)
    {
        _animator.SetFloat(_rollXHash, x);
        _animator.SetFloat(_rollZHash, z);
    }

    // Jump 관련

    public void SetJump(bool value)
    {
        _animator.SetBool(_jumpHash, value);
    }

    public void SetFallEnter(bool value)
    {
        _animator.SetBool(_fallEnterHash, value);
    }

    public void SetLandingTrigger()
    {
        _animator.SetTrigger(_landingHash);
    }

    public bool IsInIdleOrRunState()
    {
        if (_animator.IsInTransition(0))
        {
            return false;
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName(_idleWalkStateName) || stateInfo.IsName(_runStateName);
    }

    // Attack
    public void SetAttackTrigger()
    {
        _animator.SetTrigger(_attackHash);
    }

    public void SetAttackStepIndex(int index)
    {
        _animator.SetInteger(_attackStepIndexHash, index);
    }

    public void SetAttackIndex(int index)
    {
        _animator.SetInteger(_attackIndexHash, index);
    }

    public void OnAttackStateEnter(int stepIndex)
    {
        _attack.OnAttackStateEnter(stepIndex);
    }

    public void OnAttackApply()
    {
        _attack.OnAttackApply();
    }

    public void OnOpenComboWindow()
    {
        _attack.OpenComboWindow();
    }

    public void OnCloseComboWindow()
    {
        _attack.CloseComboWindow();
    }

    public void OnAttackEnd()
    {
        _attack.OnAttackEnd();
    }

    public void OnAttackMoveStart(int stepIndex)
    {
        _attack.OnAttackMoveStart(stepIndex);
    }

    public void OnAttackMoveEnd()
    {
        _attack.OnAttackMoveEnd();
    }

    // Skill
    public void SetSkillTrigger()
    {
        _animator.SetTrigger(_skillHash);
    }

    public void SetWeaponType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                _animator.SetInteger(_weaponTypeHash, 0);
                break;
            case WeaponType.Staff:
                _animator.SetInteger(_weaponTypeHash, 1);
                break;
        }
    }

    public void SetSkillIndex(SkillDefinition skill)
    {
        _animator.SetInteger(_skillIndexHash, skill.animationIndex);
    }

    public void OnSkillStart()
    {
        _skill.OnSkillStart();
    }

    public void OnSkillApply()
    {
        _skill.OnSkillApply();
    }

    public void OnSkillEnd()
    {
        _skill.OnSkillEnd();
    }

    public void ResetSkillTrigger()
    {
        _animator.ResetTrigger(_skillHash);
    }

    // Hit
    public void SetHitTrigger()
    {
        if (!IsCurrentState(_idleWalkStateName))
        {
            return;
        }

        _animator.SetTrigger(_hitHash);
        GameCore.SoundManager?.PlayPlayerHit();
    }
    // Turn
    public void SetTurn(float value)
    {
        _animator.SetFloat(_turnHash, value);
    }

    public bool IsCurrentState(string stateName)
    {
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(stateName);
    }

    public void SetDeathTrigger()
    {
        _animator.SetTrigger(_deathHash);
    }

    public void ResetToIdle()
    {
        _animator.Play(_idleWalkStateName, 0, 0f);
    }
}