using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject _coreUI;
    [SerializeField] private PlayerCameraController _playerCameraController;
    [SerializeField] private PlayerInput _input;
    [SerializeField] private PlayerInputs _playerInputs;
    [SerializeField] private PlayerInteraction _playerInteraction;
    [SerializeField] private CinemachineCamera _titleCam;
    [SerializeField] private CinemachineCamera _gameplayCam;

    [Header("Interaction Camera")]
    [SerializeField] private CinemachineCamera _npcCam;
    [SerializeField] private int _npcCamActivePriority = 40;
    [SerializeField] private int _npcCamInactivePriority = 0;
    [SerializeField] private GameObject _playerVisual;

    [Header("Interaction Camera Move")]
    [SerializeField] private float _npcCamContentOffsetX = 1f;
    [SerializeField] private float _npcCamMoveDuration = 0.3f;

    [Header("Camera Blend")]
    [SerializeField] private float _defaultBlendTime = 3f;
    [SerializeField] private float _focusBlendTime = 1f;
    [SerializeField] private float _releaseBlendTime = 1f;

    [Header("Cinemachine Brain")]
    [SerializeField] private CinemachineBrain _cinemachineBrain;

    [Header("Test")]
    [SerializeField] private bool _activePlayerCam = false;

    private bool _lastActivePlayerCam;
    private Vector3 _npcCamBasePosition;
    private Coroutine _npcCamMoveRoutine;

    private void Awake()
    {
        _input.enabled = false;
        _lastActivePlayerCam = _activePlayerCam;
        SetBlendTime(_defaultBlendTime);

        if (_npcCam != null)
        {
            _npcCam.Priority = _npcCamInactivePriority;
        }
    }

    private void Update()
    {
        if (_lastActivePlayerCam == _activePlayerCam)
        {
            return;
        }

        _lastActivePlayerCam = _activePlayerCam;

        if (_activePlayerCam)
        {
            EnableGameplayCameraForTest();
        }
        else
        {
            PrepareTitleMode();
        }
    }

    private void EnableGameplayCameraForTest()
    {
        StartGameplayCamera();
        EnableGameplay();
    }

    public void PrepareTitleMode()
    {
        _coreUI.SetActive(false);

        _titleCam.gameObject.SetActive(true);
        _gameplayCam.gameObject.SetActive(false);

        _titleCam.Priority = 30;
        _gameplayCam.Priority = 0;

        _playerInputs.SetCursorLock(false);
        _playerCameraController.SetCameraControl(false);
        _input.enabled = false;
        _playerInteraction.enabled = false;
    }

    public void StartGameplayCamera()
    {
        _gameplayCam.gameObject.SetActive(true);

        _titleCam.Priority = 0;
        _gameplayCam.Priority = 30;
    }

    public void EnableGameplay()
    {
        _coreUI.SetActive(true);

        _playerInputs.SetCursorLock(true);
        _playerCameraController.SetCameraControl(true);
        _input.enabled = true;
        _playerInteraction.enabled = true;
        GameCore.RescueManager.ClearList();
    }

    public void SkipTitleMode()
    {
        _coreUI.SetActive(true);

        _titleCam.gameObject.SetActive(false);
        _gameplayCam.gameObject.SetActive(true);

        _titleCam.Priority = 0;
        _gameplayCam.Priority = 30;

        _playerInputs.SetCursorLock(true);
        _playerCameraController.SetCameraControl(true);
        _input.enabled = true;
        _playerInteraction.enabled = true;
    }

    public void InactiveCoreUI()
    {
        _coreUI.SetActive(false);
    }

    public void ActiveCoreUI()
    {
        _coreUI.SetActive(true);
    }

    public void FocusNPC(Transform viewPoint, Action onBlendComplete = null)
    {
        if (_npcCam == null || viewPoint == null)
        {
            onBlendComplete?.Invoke();
            return;
        }
        SetBlendTime(_focusBlendTime);

        _npcCam.transform.SetPositionAndRotation(viewPoint.position, viewPoint.rotation);
        _npcCamBasePosition = viewPoint.position;
        _npcCam.Priority = _npcCamActivePriority;

        StartCoroutine(WaitForBlendComplete(onBlendComplete));
    }

    private IEnumerator WaitForBlendComplete(Action onComplete)
    {
        yield return null;
        _playerVisual.SetActive(false);
        GameCore.GameController.InactiveCoreUI();
        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();

        while (_cinemachineBrain != null && _cinemachineBrain.IsBlending)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void ReleaseFocus()
    {
        if (_npcCam == null)
        {
            return;
        }

        if (_npcCamMoveRoutine != null)
        {
            StopCoroutine(_npcCamMoveRoutine);
            _npcCamMoveRoutine = null;
        }

        SetBlendTime(_releaseBlendTime);

        _npcCam.Priority = _npcCamInactivePriority;
        _playerVisual.SetActive(true);
    }

    public void MoveNPCCamForContent()
    {
        Vector3 targetPosition = _npcCamBasePosition + _npcCam.transform.right * _npcCamContentOffsetX;
        StartNPCCamMove(targetPosition);
    }

    public void ReturnNPCCamToBase()
    {
        StartNPCCamMove(_npcCamBasePosition);
    }

    private void StartNPCCamMove(Vector3 targetPosition)
    {
        if (_npcCamMoveRoutine != null)
        {
            StopCoroutine(_npcCamMoveRoutine);
        }

        _npcCamMoveRoutine = StartCoroutine(MoveNPCCamRoutine(targetPosition));
    }

    private IEnumerator MoveNPCCamRoutine(Vector3 targetPosition)
    {
        Vector3 startPosition = _npcCam.transform.position;
        float elapsed = 0f;

        while (elapsed < _npcCamMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float timer = Mathf.Clamp01(elapsed / _npcCamMoveDuration);

            _npcCam.transform.position = Vector3.Lerp(startPosition, targetPosition, timer);

            yield return null;
        }

        _npcCam.transform.position = targetPosition;
        _npcCamMoveRoutine = null;
    }

    public void SetInstantBlend()
    {
        SetBlendTime(0f);
    }

    public void ResetBlend()
    {
        SetBlendTime(_defaultBlendTime);
    }

    public void DisablePlayerInteraction()
    {
        if (_playerInteraction != null)
            _playerInteraction.enabled = false;
    }

    public void DisableAllOutlines()
    {
        foreach (EPOOutline.Outlinable outlinable in FindObjectsByType<EPOOutline.Outlinable>(FindObjectsSortMode.None))
            outlinable.enabled = false;
    }

    private void SetBlendTime(float time)
    {
        if (_cinemachineBrain == null)
        {
            return;
        }

        CinemachineBlendDefinition blend = _cinemachineBrain.DefaultBlend;
        blend.Time = time;
        _cinemachineBrain.DefaultBlend = blend;
    }
}