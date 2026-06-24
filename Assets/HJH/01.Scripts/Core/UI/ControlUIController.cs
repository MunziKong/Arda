using UnityEngine;
using UnityEngine.UI;

public class ControlUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _controlWrap;

    [Header("Panels")]
    [SerializeField] private GameObject _basePanel;
    [SerializeField] private GameObject _resolutionPanel;
    [SerializeField] private GameObject _soundPanel;

    [Header("Base Buttons")]
    [SerializeField] private Button _resumeBtn;
    [SerializeField] private Button _resolutionBtn;
    [SerializeField] private Button _soundBtn;
    [SerializeField] private Button _saveBtn;
    [SerializeField] private Button _exitBtn;

    [Header("Input")]
    [SerializeField] private PlayerInputs _playerInputs;

    private void Awake()
    {
        _resumeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); ClosePanel(); });
        _resolutionBtn.onClick.AddListener(OpenResolutionPanel);
        _soundBtn.onClick.AddListener(OpenSoundPanel);
        _exitBtn.onClick.AddListener(OnExitClicked);

        _controlWrap.SetActive(false);
    }

    private void OnEnable()
    {
        _playerInputs.PauseEvent += HandlePause;
        _playerInputs.CloseEvent += HandleClose;
    }

    private void OnDisable()
    {
        _playerInputs.PauseEvent -= HandlePause;
        _playerInputs.CloseEvent -= HandleClose;
    }

    private void HandlePause()
    {
        if (!_controlWrap.activeSelf)
        {
            OpenPanel();
        }
    }

    private void HandleClose()
    {
        if (!_controlWrap.activeSelf)
        {
            return;
        }

        if (_resolutionPanel.activeSelf || _soundPanel.activeSelf)
        {
            OpenBasePanel();
            return;
        }

        ClosePanel();
    }

    public void OpenPanel()
    {
        _controlWrap.SetActive(true);
        OpenBasePanel();

        Time.timeScale = 0f;
        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();
    }

    public void ClosePanel()
    {
        if (!_controlWrap.activeSelf)
        {
            return;
        }

        _controlWrap.SetActive(false);

        Time.timeScale = 1f;
        GameCore.PlayerInputs.SetCursorLock(true);
        GameCore.PlayerInputs.SetPlayerInput();
    }

    private void OpenBasePanel()
    {
        _basePanel.SetActive(true);
        _resolutionPanel.SetActive(false);
        _soundPanel.SetActive(false);
    }

    private void OpenResolutionPanel()
    {
        SoundManager.Instance?.PlayButtonClick();
        _basePanel.SetActive(false);
        _resolutionPanel.SetActive(true);
        _soundPanel.SetActive(false);
    }

    private void OpenSoundPanel()
    {
        SoundManager.Instance?.PlayButtonClick();
        _basePanel.SetActive(false);
        _resolutionPanel.SetActive(false);
        _soundPanel.SetActive(true);
    }

    private void OnExitClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        GameCore.GameManager.ExitGame();
    }
}