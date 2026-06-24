using System.Collections;
using UnityEngine;

public enum ActionDirectionMode
{
    Fixed,
    Dynamic
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    private PlayerStats _stats;

    private float _walkSpeed;
    private float _runSpeed;

    [Header("Speed")]
    [SerializeField] private float _speedChangeRate = 10f;

    [Header("Roll")]
    [SerializeField] private float _rollSpeed = 8f;
    [SerializeField] private float _rollCooldown = 5f;
    [SerializeField] private float _rollDuration = 0.8f;
    [SerializeField] private float _rollMoveDuration = 0.4f;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -15f;
    [SerializeField] private float _groundedGravity = -2f;

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.2f;

    [Header("Grounded")]
    [SerializeField] private bool _grounded = true;
    [SerializeField] private float _groundedOffset = -0.14f;
    [SerializeField] private float _groundedRadius = 0.28f;
    [SerializeField] private LayerMask _groundLayers;

    [SerializeField] private float _terminalVelocity = 53f;

    [Header("Footstep")]
    [SerializeField] private float _footstepWalkInterval = 1.5f;
    [SerializeField] private float _footstepRunInterval = 1.0f;


    [Header("Jump Lock")]
    [SerializeField] private float _jumpLockAfterRoll = 0.4f;

    [Header("Anim")]
    [SerializeField] private PlayerAnimController _animController;

    [Header("UI")]
    [SerializeField] private RollUIController _rollUI;

    private bool _suppressLandSound;
    private bool _canJump = true;
    private bool _landingDetected;

    private bool _wasGrounded;
    private bool _isJumpStarted;

    private float _verticalVelocity;

    private CharacterController _controller;
    private PlayerInputs _input;
    private PlayerAttack _attack;

    private float _speed;

    private bool _isRollLocked;
    private bool _isRolling;
    private bool _isRollCooldown;
    private Vector3 _rollDirection;
    private float _rollMoveTimer;

    private float _footstepDistAccum;

    private bool _hasActionMoving;
    private Vector3 _actionMoveDirection;
    private float _actionMoveSpeed;
    private float _actionMoveTimer;

    private bool _isSkillCasting;
    private bool _canMoveWhileSkill;
    private ActionDirectionMode _actionDirectionMode;
    private float _lastYRotation;
    private float _footstepIntervalMultiplier = 1f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
        _attack = GetComponent<PlayerAttack>();
        _stats = GetComponent<PlayerStats>();

        _lastYRotation = transform.eulerAngles.y;
    }

    private void Start()
    {
        _walkSpeed = _stats.WalkSpeed;
        _runSpeed = _stats.RunSpeed;
    }

    private void OnEnable()
    {
        _input.RollEvent += Roll;
        _stats.SpeedChangeEvent += UpdateMoveSpeed;
    }

    private void OnDisable()
    {
        _input.RollEvent -= Roll;
        _stats.SpeedChangeEvent -= UpdateMoveSpeed;
    }

    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!_grounded && !_wasGrounded && hit.normal.y > 0.7f && _verticalVelocity < 0f)
        {
            _landingDetected = true;
        }
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - _groundedOffset,
            transform.position.z
        );

        _grounded = Physics.CheckSphere(
            spherePosition,
            _groundedRadius,
            _groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            HandleLanding();

            bool canDoJump = !_isRollLocked
                && _canJump
                && !_attack.IsAttacking
                && (_animController.IsInIdleOrRunState() || _animController.IsCurrentState("JumpLand"))
                && !_animController.IsCurrentState("JumpStart")
                && !_animController.IsCurrentState("InAir");

            if (_input.Jump)
            {
                if (canDoJump)
                {
                    ExecuteJump();
                }

                _input.Jump = false;
            }

            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedGravity;
            }
        }
        else
        {
            _input.Jump = false;

            if (_isJumpStarted && _verticalVelocity < 0f)
            {
                _isJumpStarted = false;
                _animController.SetJump(false);
            }
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        UpdateJumpAnimState();
    }

    private void ExecuteJump()
    {
        _verticalVelocity = Mathf.Sqrt(_jumpHeight * _groundedGravity * _gravity);

        _isJumpStarted = true;

        _animController.SetJump(true);
        _animController.SetFallEnter(false);
    }

    private void HandleLanding()
    {
        bool justLanded = _landingDetected || (!_wasGrounded && _grounded);

        if (justLanded)
        {
            if (_animController.IsCurrentState("JumpLand"))
            {
                _landingDetected = false;
                return;
            }

            _landingDetected = false;

            _isJumpStarted = false;
            _animController.SetJump(false);
            _animController.SetFallEnter(false);
            _animController.SetLandingTrigger();

            if (_suppressLandSound)
            {
                _suppressLandSound = false;
            }
            else
            {
                GameCore.SoundManager?.PlayJumpland();
            }
        }
    }

    public void SuppressNextLandSound()
    {
        _suppressLandSound = true;
    }

    private void UpdateJumpAnimState()
    {
        if (_grounded)
        {
            _animController.SetFallEnter(false);
        }
        else
        {
            bool isFalling = _verticalVelocity < 0f;
            bool fallEnter = isFalling && !_isJumpStarted;

            _animController.SetFallEnter(fallEnter);
        }

        _wasGrounded = _grounded;
    }

    private void Move()
    {
        Vector3 verticalMove = Vector3.up * (_verticalVelocity * Time.deltaTime);

        if (_isRollLocked)
        {
            HandleRollMove(verticalMove);
            return;
        }

        if (_hasActionMoving)
        {
            HandleActionMove(verticalMove);
            return;
        }

        if (_isSkillCasting && !_canMoveWhileSkill)
        {
            _controller.Move(verticalMove);
            _animController.SetMoveDirection(0f, 0f);
            _animController.SetRunning(false);
            return;
        }

        if (!_attack.IsAttacking)
        {
            HandleNormalMove(verticalMove);
        }
    }

    private void UpdateMoveSpeed()
    {
        _walkSpeed = _stats.WalkSpeed;
        _runSpeed = _stats.RunSpeed;
        _rollSpeed = _stats.RunSpeed;
    }

    private void HandleRollMove(Vector3 verticalMove)
    {
        Vector3 rollMove;

        if (_isRolling)
        {
            float timer = 1f - (_rollMoveTimer / _rollMoveDuration);
            float currentSpeed = Mathf.Lerp(_rollSpeed, 0f, timer);

            rollMove = _rollDirection * (currentSpeed * Time.deltaTime);

            _rollMoveTimer -= Time.deltaTime;
        }
        else
        {
            rollMove = Vector3.zero;
        }

        _controller.Move(rollMove + verticalMove);
        _animController.SetMoveDirection(0f, 0f);
        _animController.SetRunning(false);
    }

    private void HandleActionMove(Vector3 verticalMove)
    {
        Vector3 moveDirection = _actionDirectionMode == ActionDirectionMode.Dynamic
            ? transform.forward
            : _actionMoveDirection;

        Vector3 attackMove = moveDirection.normalized * (_actionMoveSpeed * Time.deltaTime);
        _controller.Move(attackMove + verticalMove);

        _actionMoveTimer -= Time.deltaTime;
        if (_actionMoveTimer <= 0f)
        {
            StopActionMove();
        }

        _animController.SetMoveDirection(0f, 0f);
        _animController.SetRunning(false);
    }

    private void HandleNormalMove(Vector3 verticalMove)
    {
        bool hasMoveInput = _input.Move != Vector2.zero;
        bool isForwardInput = _input.Move.y > 0f;
        bool isRunning = _input.Run && isForwardInput && !_canMoveWhileSkill;

        _animController.SetMoveDirection(_input.Move.x, _input.Move.y);
        _animController.SetRunning(isRunning);

        float targetSpeed = isRunning ? _runSpeed : _walkSpeed;

        if (_canMoveWhileSkill) targetSpeed = _walkSpeed;
        if (!hasMoveInput) targetSpeed = 0f;

        float currentHorizontalSpeed = new Vector3(
            _controller.velocity.x, 0f, _controller.velocity.z
        ).magnitude;

        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * _speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        Vector3 moveDirection = transform.forward * _input.Move.y + transform.right * _input.Move.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        _controller.Move(moveDirection * (_speed * Time.deltaTime) + verticalMove);

        if (hasMoveInput && _grounded)
        {
            float baseInterval = isRunning ? _footstepRunInterval : _footstepWalkInterval;
            float interval = baseInterval * _footstepIntervalMultiplier;

            _footstepDistAccum += _speed * Time.deltaTime;

            if (_footstepDistAccum >= interval && !_canMoveWhileSkill)
            {
                _footstepDistAccum = 0f;
                GameCore.SoundManager?.PlayFootstep();
            }
        }
        else
        {
            _footstepDistAccum = 0f;
        }
    }

    private void Roll()
    {
        if (_attack.IsAttacking) return;
        if (_isRollLocked || _isRollCooldown || !_grounded) return;

        SetRollDirection();

        _isRollLocked = true;
        _isRollCooldown = true;
        _isRolling = true;
        _rollMoveTimer = _rollMoveDuration;

        _animController.SetMoveDirection(0f, 0f);
        _animController.SetRunning(false);
        _animController.SetRollTrigger();
        GameCore.SoundManager?.PlayRoll();

        StartCoroutine(RollDurationRoutine());
        StartCoroutine(RollMoveDurationRoutine());
        StartCoroutine(RollCooldownRoutine());
    }

    private IEnumerator RollDurationRoutine()
    {
        yield return new WaitForSeconds(_rollDuration);

        _isRollLocked = false;

        StartCoroutine(JumpLockAfterRollRoutine());
    }

    private IEnumerator RollMoveDurationRoutine()
    {
        yield return new WaitForSeconds(_rollMoveDuration);

        _isRolling = false;
    }

    private void SetRollDirection()
    {
        Vector2 moveInput = _input.Move;

        if (moveInput == Vector2.zero) moveInput = Vector2.up;

        moveInput.Normalize();

        _rollDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        _animController.SetRollDirection(moveInput.x, moveInput.y);
    }

    private IEnumerator RollCooldownRoutine()
    {
        float elapsed = 0f;

        while (elapsed < _rollCooldown)
        {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, _rollCooldown - elapsed);

            _rollUI.UpdateRollUI(remaining, _rollCooldown);

            yield return null;
        }

        _rollUI.UpdateRollUI(0f, _rollCooldown);
        _isRollCooldown = false;
    }

    private IEnumerator JumpLockAfterRollRoutine()
    {
        _canJump = false;
        yield return new WaitForSeconds(_jumpLockAfterRoll);
        _canJump = true;
    }

    public void StartActionMove(ActionDirectionMode directionMode, Vector3 fixedDirection,
    float moveSpeed, float moveDuration, float jumpHeight = 0f, float jumpDelay = 0f)
    {
        _actionDirectionMode = directionMode;
        _actionMoveDirection = fixedDirection.normalized;
        _actionMoveSpeed = moveSpeed;
        _actionMoveTimer = moveDuration;
        _hasActionMoving = true;

        if (jumpHeight > 0f && _grounded)
        {
            if (jumpDelay > 0f)
            {
                StartCoroutine(DelayedJumpRoutine(jumpHeight, jumpDelay));
            }
            else
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * _groundedGravity * _gravity);
            }
        }
    }

    private IEnumerator DelayedJumpRoutine(float jumpHeight, float jumpDelay)
    {
        yield return new WaitForSeconds(jumpDelay);

        if (_grounded)
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * _groundedGravity * _gravity);
        }
    }

    public void StopActionMove()
    {
        _hasActionMoving = false;
        _actionMoveTimer = 0f;
    }

    public void StartSkillCasting()
    {
        _isSkillCasting = true;
        _canMoveWhileSkill = false;
    }

    public void EndSkillCasting()
    {
        _isSkillCasting = false;
        _canMoveWhileSkill = false;
    }

    public void SetCanMoveWhileSkill(bool value)
    {
        _canMoveWhileSkill = value;
    }

    public void UpdateTurnAnim()
    {
        bool hasMoveInput = _input.Move != Vector2.zero;

        bool isIdleState =
            _grounded
            && !hasMoveInput
            && !_isRollLocked
            && !_hasActionMoving
            && !_isSkillCasting
            && !_attack.IsAttacking;

        if (!isIdleState)
        {
            _animController.SetTurn(0f);
            _lastYRotation = transform.eulerAngles.y;
            return;
        }

        float currentY = transform.eulerAngles.y;
        float deltaAngle = Mathf.DeltaAngle(_lastYRotation, currentY);

        float turnSpeed = deltaAngle / Time.deltaTime;

        float normalizedTurn = Mathf.Clamp(turnSpeed / 300f, -1f, 1f);

        _animController.SetTurn(normalizedTurn);

        _lastYRotation = currentY;
    }

    public void SetFootstepIntervalMultiplier(float multiplier)
    {
        _footstepIntervalMultiplier = multiplier;
    }

    public void ResetFootstepInterval()
    {
        _footstepIntervalMultiplier = 1f;
    }

    public void ResetState()
    {
        StopAllCoroutines();

        _isJumpStarted = false;
        _canJump = true;
        _isRollLocked = false;
        _isRolling = false;
        _isRollCooldown = false;
        _isSkillCasting = false;
        _canMoveWhileSkill = false;
        _hasActionMoving = false;
        _actionMoveTimer = 0f;
        _verticalVelocity = 0f;
        _wasGrounded = false;
        _landingDetected = false;
        _suppressLandSound = false;

        _rollUI?.UpdateRollUI(0f, _rollCooldown);
    }
}