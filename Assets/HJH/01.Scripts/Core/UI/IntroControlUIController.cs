using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IntroControlUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _controlWrap;

    [Header("Panels")]
    [SerializeField] private GameObject _basePanel;
    [SerializeField] private GameObject _resolutionPanel;
    [SerializeField] private GameObject _soundPanel;

    [Header("Base Buttons")]
    [SerializeField] private Button _resolutionBtn;
    [SerializeField] private Button _soundBtn;
    [SerializeField] private Button _dimmedBgBtn;

    private void Awake()
    {
        _resolutionBtn.onClick.AddListener(OpenResolutionPanel);
        _soundBtn.onClick.AddListener(OpenSoundPanel);
        _dimmedBgBtn.onClick.AddListener(ClosePanel);

        _controlWrap.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleClose();
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
    }

    public void ClosePanel()
    {
        if (!_controlWrap.activeSelf)
        {
            return;
        }

        _controlWrap.SetActive(false);
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
}