using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundUIController : MonoBehaviour
{
    [Header("Integration (전체)")]
    [SerializeField] private Button _integrationMuteBtn;
    [SerializeField] private Image _integrationMuteBtnImg;
    [SerializeField] private Slider _integrationSlider;
    [SerializeField] private TMP_Text _integrationValueTxt;

    [Header("BGM")]
    [SerializeField] private Button _bgmMuteBtn;
    [SerializeField] private Image _bgmMuteBtnImg;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private TMP_Text _bgmValueTxt;

    [Header("SFX")]
    [SerializeField] private Button _sfxMuteBtn;
    [SerializeField] private Image _sfxMuteBtnImg;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _sfxValueTxt;

    [Header("Mute Sprites")]
    [SerializeField] private Sprite _muteSelectedSprite;
    [SerializeField] private Sprite _muteNormalSprite;

    private void Awake()
    {
        _integrationMuteBtn.onClick.AddListener(OnIntegrationMuteClicked);
        _bgmMuteBtn.onClick.AddListener(OnBgmMuteClicked);
        _sfxMuteBtn.onClick.AddListener(OnSfxMuteClicked);

        _integrationSlider.onValueChanged.AddListener(OnIntegrationSliderChanged);
        _bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    private void OnEnable()
    {
        RefreshState();
    }

    private void RefreshState()
    {
        if (SoundManager.Instance == null) return;

        _integrationSlider.SetValueWithoutNotify(SoundManager.Instance.MasterVolume);
        _bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BgmVolume);
        _sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SfxVolume);

        RefreshValueText(_integrationValueTxt, SoundManager.Instance.MasterVolume);
        RefreshValueText(_bgmValueTxt, SoundManager.Instance.BgmVolume);
        RefreshValueText(_sfxValueTxt, SoundManager.Instance.SfxVolume);

        RefreshMuteSprite(_integrationMuteBtnImg, SoundManager.Instance.IsMasterMuted);
        RefreshMuteSprite(_bgmMuteBtnImg, SoundManager.Instance.IsBgmMuted);
        RefreshMuteSprite(_sfxMuteBtnImg, SoundManager.Instance.IsSfxMuted);
    }

    private void OnBgmSliderChanged(float value)
    {
        SoundManager.Instance?.SetBgmVolume(value);
        RefreshValueText(_bgmValueTxt, value);
        SaveManager.Instance?.Save();
    }

    private void OnSfxSliderChanged(float value)
    {
        SoundManager.Instance?.SetSfxVolume(value);
        RefreshValueText(_sfxValueTxt, value);
        SaveManager.Instance?.Save();
    }

    private void OnIntegrationSliderChanged(float value)
    {
        SoundManager.Instance?.SetMasterVolume(value);
        RefreshValueText(_integrationValueTxt, value);
        SaveManager.Instance?.Save();
    }

    private void OnBgmMuteClicked()
    {
        SoundManager.Instance?.ToggleBgmMute();
        RefreshMuteSprite(_bgmMuteBtnImg, SoundManager.Instance.IsBgmMuted);
        SaveManager.Instance?.Save();
    }

    private void OnSfxMuteClicked()
    {
        SoundManager.Instance?.ToggleSfxMute();
        RefreshMuteSprite(_sfxMuteBtnImg, SoundManager.Instance.IsSfxMuted);
        SaveManager.Instance?.Save();
    }

    private void OnIntegrationMuteClicked()
    {
        SoundManager.Instance?.ToggleMasterMute();
        RefreshMuteSprite(_integrationMuteBtnImg, SoundManager.Instance.IsMasterMuted);
        SaveManager.Instance?.Save();
    }

    private void RefreshValueText(TMP_Text text, float value)
    {
        if (text == null) return;
        text.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    private void RefreshMuteSprite(Image image, bool isMuted)
    {
        if (image == null) return;
        image.sprite = isMuted ? _muteSelectedSprite : _muteNormalSprite;
    }
}