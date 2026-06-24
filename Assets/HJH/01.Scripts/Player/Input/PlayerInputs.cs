using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInputs : MonoBehaviour
{
    [Header("Move")]
    public Vector2 Move;

    [Header("Look")]
    public Vector2 Look;

    [Header("Run")]
    public bool Run;

    [Header("Jump")]
    public bool Jump;

    [Header("Cursor")]
    [SerializeField] private bool _cursorLocked = true;
    [SerializeField] private bool _cursorInputForLook = true;

    [Header("Scroll")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private float _scrollSpeed = 0.01f;


    public Action RollEvent;
    public Action AttackEvent;
    public Action<int> SkillEvent;
    public Action InteractPressedEvent;
    public Action InteractReleasedEvent;
    public Action CloseEvent;
    public Action AimEvent;
    public Action ToggleUIEvent;
    public Action TestEvent;
    public Action Test1Event;
    public Action Test2Event;
    public Action UseEvent;
    public Action ChangeRescueItemEvent;
    public Action<bool> OpenBoardEvent;
    public Action CodexEvent;
    public Action PauseEvent;
    private PlayerInput _input;

    public bool IsPlayerInputMode => _input.currentActionMap.name == "Player";
    void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    public void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (_cursorInputForLook)
        {
            Look = value.Get<Vector2>();
        }
    }

    public void OnRun(InputValue value)
    {
        Run = value.isPressed;
    }

    public void OnRoll()
    {
        RollEvent?.Invoke();
    }

    public void OnJump(InputValue value)
    {
        Jump = value.isPressed;
    }

    public void OnAttack()
    {
        AttackEvent?.Invoke();
    }

    public void OnSkill1()
    {
        // Debug.Log("OnSkill1");
        SkillEvent?.Invoke(0);
    }

    public void OnSkill2()
    {
        // Debug.Log("OnSkill2");
        SkillEvent?.Invoke(1);
    }

    public void OnSkill3()
    {
        // Debug.Log("OnSkill3");
        SkillEvent?.Invoke(2);
    }

    public void OnAim()
    {
        AimEvent?.Invoke();
    }

    public void OnTest()
    {
        GameCore.AlertManager.Enqueue(AlertType.QuestComplete, "마을 탐험");
        GameCore.AlertManager.Enqueue(AlertType.MapEnter, "초원 지대");
        TestEvent?.Invoke();
    }

    public void OnTest1()
    {
        Test1Event?.Invoke();
    }

    public void OnTest2()
    {
        Test2Event?.Invoke();
    }

    public void OnInteract(InputValue value)
    {
        bool isPressed = value.Get<float>() > 0f;

        if (isPressed)
        {
            InteractPressedEvent?.Invoke();
        }
        else
        {
            InteractReleasedEvent?.Invoke();
        }

        // Debug.Log($"Interact Pressed : {isPressed}");
    }

    public void OnToggleUI()
    {
        ToggleUIEvent?.Invoke();
    }

    public void OnClose()
    {
        CloseEvent?.Invoke();
    }

    public void OnUse()
    {
        UseEvent?.Invoke();
    }

    public void OnChangeRescueItem()
    {
        ChangeRescueItemEvent?.Invoke();
    }

    public void OnOpenBoard(InputValue value)
    {
        bool isPressed = value.Get<float>() > 0f;
        OpenBoardEvent?.Invoke(isPressed);
    }
    public void SetCursorLock(bool isLocked)
    {
        _cursorLocked = isLocked;

        Cursor.lockState =
            isLocked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !isLocked;
    }

    public void OnScroll(InputValue value)
    {
        Vector2 scroll = value.Get<Vector2>();

        if (Mathf.Abs(scroll.y) <= 0.01f)
        {
            return;
        }

        _scrollRect.verticalNormalizedPosition += scroll.y * _scrollSpeed;
        _scrollRect.verticalNormalizedPosition =
            Mathf.Clamp01(_scrollRect.verticalNormalizedPosition);
    }

    public void OnCodex(InputValue value)
    {
        if (value.isPressed)
        {
            CodexEvent?.Invoke();
        }
    }



    public void OnPause()
    {
        PauseEvent?.Invoke();
    }

    public void SetPlayerInput()
    {
        _input.SwitchCurrentActionMap("Player");
    }

    public void SetUIInput()
    {
        _input.SwitchCurrentActionMap("UI");
    }
}