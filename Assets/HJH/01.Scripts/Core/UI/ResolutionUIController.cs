using UnityEngine;
using UnityEngine.UI;

public class ResolutionUIController : MonoBehaviour
{
    [Header("Mode Buttons")]
    [SerializeField] private Button _windowedBtn;
    [SerializeField] private Button _fullScreenBtn;
    [SerializeField] private Image _windowedBtnImg;
    [SerializeField] private Image _fullScreenBtnImg;

    [Header("Resolution Buttons")]
    [SerializeField] private Button _option1Btn;
    [SerializeField] private Button _option2Btn;
    [SerializeField] private Button _option3Btn;
    [SerializeField] private Image _option1BtnImg;
    [SerializeField] private Image _option2BtnImg;
    [SerializeField] private Image _option3BtnImg;

    [Header("Radio Sprites")]
    [SerializeField] private Sprite _radioSelected;
    [SerializeField] private Sprite _radioNormal;

    private bool _isFullScreen = true;

    private void Awake()
    {
        _windowedBtn.onClick.AddListener(OnWindowedClicked);
        _fullScreenBtn.onClick.AddListener(OnFullScreenClicked);

        _option1Btn.onClick.AddListener(() => OnResolutionClicked(1920, 1080, 1));
        _option2Btn.onClick.AddListener(() => OnResolutionClicked(1600, 900, 2));
        _option3Btn.onClick.AddListener(() => OnResolutionClicked(1280, 720, 3));
    }

    private void OnEnable()
    {
        RefreshState();
    }

    private void RefreshState()
    {
        if (GameCore.GameManager == null) return;

        _isFullScreen = GameCore.GameManager.IsFullScreen;

        _fullScreenBtnImg.sprite = _isFullScreen ? _radioSelected : _radioNormal;
        _windowedBtnImg.sprite = _isFullScreen ? _radioNormal : _radioSelected;

        SetResolutionButtonsInteractable(!_isFullScreen);

        int width = GameCore.GameManager.ResolutionWidth;
        int height = GameCore.GameManager.ResolutionHeight;

        _option1BtnImg.sprite = (width == 1920 && height == 1080) ? _radioSelected : _radioNormal;
        _option2BtnImg.sprite = (width == 1600 && height == 900) ? _radioSelected : _radioNormal;
        _option3BtnImg.sprite = (width == 1280 && height == 720) ? _radioSelected : _radioNormal;
    }

    private void OnWindowedClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        _isFullScreen = false;

        _windowedBtnImg.sprite = _radioSelected;
        _fullScreenBtnImg.sprite = _radioNormal;

        SetResolutionButtonsInteractable(true);

        GameCore.GameManager.SetWindowMode(false);
        SaveManager.Instance?.Save();
    }

    private void OnFullScreenClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        _isFullScreen = true;

        _windowedBtnImg.sprite = _radioNormal;
        _fullScreenBtnImg.sprite = _radioSelected;

        SetResolutionButtonsInteractable(false);

        _option1BtnImg.sprite = _radioSelected;
        _option2BtnImg.sprite = _radioNormal;
        _option3BtnImg.sprite = _radioNormal;

        GameCore.GameManager.SetWindowMode(true);
        GameCore.GameManager.SetResolution(1920, 1080);
        SaveManager.Instance?.Save();
    }

    private void OnResolutionClicked(int width, int height, int optionIndex)
    {
        SoundManager.Instance?.PlayButtonClick();

        if (_isFullScreen) return;

        _option1BtnImg.sprite = optionIndex == 1 ? _radioSelected : _radioNormal;
        _option2BtnImg.sprite = optionIndex == 2 ? _radioSelected : _radioNormal;
        _option3BtnImg.sprite = optionIndex == 3 ? _radioSelected : _radioNormal;

        GameCore.GameManager.SetResolution(width, height);
        SaveManager.Instance?.Save();
    }

    private void SetResolutionButtonsInteractable(bool interactable)
    {
        _option1Btn.interactable = interactable;
        _option2Btn.interactable = interactable;
        _option3Btn.interactable = interactable;
    }
}