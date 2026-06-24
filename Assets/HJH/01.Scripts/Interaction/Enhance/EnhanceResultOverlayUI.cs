using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceResultOverlayUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private RectTransform _weaponIconRect;
    [SerializeField] private TMP_Text _resultText;

    [Header("Shake")]
    [SerializeField] private float _shakeDuration = 1f;
    [SerializeField] private float _shakeStrength = 15f;
    [SerializeField] private float _shakeSpeed = 20f;

    [Header("Result Display")]
    [SerializeField] private float _resultDisplayDuration = 1.5f;

    private Coroutine _routine;
    private Vector2 _iconOriginalPosition;

    private void Awake()
    {
        _iconOriginalPosition = _weaponIconRect.anchoredPosition;
    }

    public void Play(Sprite weaponIcon, bool isSuccess, int challengeLevel, Action onComplete)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _iconOriginalPosition = _weaponIconRect.anchoredPosition;

        _routine = StartCoroutine(PlayRoutine(weaponIcon, isSuccess, challengeLevel, onComplete));
    }

    public void Reset()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        _weaponIconRect.anchoredPosition = _iconOriginalPosition;
        _resultText.gameObject.SetActive(false);
    }

    private IEnumerator PlayRoutine(Sprite weaponIcon, bool isSuccess, int challengeLevel, Action onComplete)
    {
        _weaponIcon.sprite = weaponIcon;
        _weaponIconRect.anchoredPosition = _iconOriginalPosition;
        _resultText.gameObject.SetActive(false);

        float elapsed = 0f;

        while (elapsed < _shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float offsetX = Mathf.Sin(elapsed * _shakeSpeed) * _shakeStrength;
            _weaponIconRect.anchoredPosition = _iconOriginalPosition + new Vector2(offsetX, 0f);

            yield return null;
        }

        _weaponIconRect.anchoredPosition = _iconOriginalPosition;

        _resultText.gameObject.SetActive(true);
        _resultText.text = isSuccess
            ? $"+{challengeLevel} 강화 성공"
            : $"+{challengeLevel} 강화 실패";

        elapsed = 0f;

        while (elapsed < _resultDisplayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _routine = null;
        onComplete?.Invoke();
    }
}