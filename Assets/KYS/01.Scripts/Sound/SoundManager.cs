using System;
using System.Collections;
using UnityEngine;

public enum BgmType
{
    Intro,
    Main,
    Forest
}

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("AudioSource")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _skillSource;

    [Header("BGM 클립")]
    [SerializeField] private AudioClip _introBgmClip;
    [SerializeField] private AudioClip _mainBgmClip;
    [SerializeField] private AudioClip _forestBgmClip;

    [Header("UI 클립")]
    [SerializeField] private AudioClip _buttonClickClip;

    [Header("플레이어 Move 클립")]
    [SerializeField] private AudioClip _footstepClip;
    [SerializeField] private AudioClip _jumplandClip;
    [SerializeField] private AudioClip _rollClip;

    [Header("플레이어 Combat 클립")]
    [SerializeField] private AudioClip _playerHitClip;

    [Header("Enemy 클립")]
    [SerializeField] private AudioClip _enemyHitClip1;
    [SerializeField] private AudioClip _enemyHitClip2;

    [Header("Alert")]
    [SerializeField] private AudioClip _alertClip;

    [Header("Rescue")]
    [SerializeField] private AudioClip _rescueSuccessClip;
    [SerializeField] private AudioClip _rescueFailedClip;

    public float MasterVolume { get; private set; } = 1f;
    public float BgmVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    public bool IsMasterMuted { get; private set; }
    public bool IsBgmMuted { get; private set; }
    public bool IsSfxMuted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
    }

    // ── BGM ──────────────────────────────────────────────
    public void PlayBgm(BgmType type, bool loop = true)
    {
        AudioClip target = type switch
        {
            BgmType.Intro => _introBgmClip,
            BgmType.Main => _mainBgmClip,
            BgmType.Forest => _forestBgmClip,
            _ => null
        };

        if (target == null || _bgmSource == null) return;

        _bgmSource.clip = target;
        _bgmSource.loop = loop;
        _bgmSource.volume = IsBgmMuted ? 0f : BgmVolume;
        _bgmSource.Play();
    }


    public void StopBgm()
    {
        if (_bgmSource == null) return;
        _bgmSource.Stop();
    }

    public void FadeOutBgm(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeOutBgmRoutine(duration, onComplete));
    }

    private IEnumerator FadeOutBgmRoutine(float duration, Action onComplete)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = 0f;
        _bgmSource.Stop();
        onComplete?.Invoke();
    }

    // ── UI / 버튼 ─────────────────────────────────────────
    public void PlayButtonClick()
    {
        PlaySfx(_buttonClickClip);
    }

    // ── 플레이어 Move ────────────────────────────────────────────
    public void PlayFootstep()
    {
        PlaySfx(_footstepClip);
    }

    public void PlayJumpland()
    {
        PlaySfx(_jumplandClip);
    }

    public void PlayRoll()
    {
        PlaySfx(_rollClip);
    }

    // ── 플레이어 Move ────────────────────────────────────────────
    public void PlayPlayerHit()
    {
        PlaySfx(_playerHitClip);
    }

    // ── Enemy ────────────────────────────────────────────
    public void PlayEnemyHit()
    {
        AudioClip clip = UnityEngine.Random.value < 0.5f ? _enemyHitClip1 : _enemyHitClip2;
        PlaySfx(clip);
    }

    // ── Alert ────────────────────────────────────────────
    public void PlayAlert()
    {
        PlaySfx(_alertClip, 0.5f);
    }

    // ── Rescue ────────────────────────────────────────────
    public void PlayRescue(bool isSuccess)
    {
        if (isSuccess)
        {
            PlaySfx(_rescueSuccessClip, 0.5f);
        }
        else
        {
            PlaySfx(_rescueFailedClip, 0.5f);
        }
    }

    // ── 일반 SFX (클립 직접 전달) ─────────────────────────
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        float appliedVolume = IsSfxMuted ? 0f : SfxVolume * volume;
        _sfxSource.PlayOneShot(clip, Mathf.Clamp01(appliedVolume));
    }

    public void PlaySkill(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || _skillSource == null) return;

        _skillSource.pitch = pitch;
        float appliedVolume = IsSfxMuted ? 0f : SfxVolume * volume;
        _skillSource.PlayOneShot(clip, Mathf.Clamp01(appliedVolume));
    }

    // ─────────────────────────────────────────────────────
    private void EnsureAudioSources()
    {
        if (_bgmSource == null)
            _bgmSource = gameObject.AddComponent<AudioSource>();

        if (_sfxSource == null)
            _sfxSource = gameObject.AddComponent<AudioSource>();

        if (_skillSource == null)
            _skillSource = gameObject.AddComponent<AudioSource>();

        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;

        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;

        _skillSource.playOnAwake = false;
        _skillSource.loop = false;
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = IsMasterMuted ? 0f : MasterVolume;
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        if (_bgmSource != null)
            _bgmSource.volume = IsBgmMuted ? 0f : BgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        float appliedVolume = IsSfxMuted ? 0f : SfxVolume;
        if (_sfxSource != null) _sfxSource.volume = appliedVolume;
        if (_skillSource != null) _skillSource.volume = appliedVolume;
    }

    public void ToggleMasterMute()
    {
        IsMasterMuted = !IsMasterMuted;
        AudioListener.volume = IsMasterMuted ? 0f : MasterVolume;
    }

    public void ToggleBgmMute()
    {
        IsBgmMuted = !IsBgmMuted;
        if (_bgmSource != null)
            _bgmSource.volume = IsBgmMuted ? 0f : BgmVolume;
    }

    public void ToggleSfxMute()
    {
        IsSfxMuted = !IsSfxMuted;
        float appliedVolume = IsSfxMuted ? 0f : SfxVolume;
        if (_sfxSource != null) _sfxSource.volume = appliedVolume;
        if (_skillSource != null) _skillSource.volume = appliedVolume;
    }

    public void SetMasterMute(bool mute)
    {
        IsMasterMuted = mute;
        AudioListener.volume = IsMasterMuted ? 0f : MasterVolume;
    }

    public void SetBgmMute(bool mute)
    {
        IsBgmMuted = mute;
        if (_bgmSource != null)
            _bgmSource.volume = IsBgmMuted ? 0f : BgmVolume;
    }

    public void SetSfxMute(bool mute)
    {
        IsSfxMuted = mute;
        float appliedVolume = IsSfxMuted ? 0f : SfxVolume;
        if (_sfxSource != null) _sfxSource.volume = appliedVolume;
        if (_skillSource != null) _skillSource.volume = appliedVolume;
    }
}
