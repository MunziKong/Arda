using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadePanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _penaltyText;

    [Header("Settings")]
    [SerializeField] private float _dialogueFadeDuration = 0.5f;
    [SerializeField] private float _dialogueDisplayDuration = 2f;

    private void Awake()
    {
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);

        _dialogueText.color = new Color(1f, 1f, 1f, 0f);
        _dialogueText.text = string.Empty;

        if (_penaltyText != null)
        {
            _penaltyText.color = new Color(1f, 0f, 0f, 0f);
            _penaltyText.text = string.Empty;
        }
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(0f, 1f, duration, onComplete));
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, 0f, duration, onComplete));
    }

    public void ShowDialogue(string text, string penaltyMessage, Action onComplete = null)
    {
        StartCoroutine(DialogueRoutine(text, penaltyMessage, onComplete));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
    {
        float elapsed = 0f;
        Color color = _fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, elapsed / duration);
            _fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        _fadeImage.color = color;
        onComplete?.Invoke();
    }

    private IEnumerator DialogueRoutine(string text, string penaltyMessage, Action onComplete)
    {
        _dialogueText.text = text;
        if (_penaltyText != null)
            _penaltyText.text = penaltyMessage;

        float elapsed = 0f;
        Color mainColor = _dialogueText.color;
        Color subColor = new Color(1f, 0f, 0f, 0f);

        while (elapsed < _dialogueFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / _dialogueFadeDuration);
            mainColor.a = alpha;
            subColor.a = alpha;
            _dialogueText.color = mainColor;
            if (_penaltyText != null) _penaltyText.color = subColor;
            yield return null;
        }

        mainColor.a = 1f;
        subColor.a = 1f;
        _dialogueText.color = mainColor;
        if (_penaltyText != null) _penaltyText.color = subColor;

        yield return new WaitForSecondsRealtime(_dialogueDisplayDuration);

        elapsed = 0f;
        while (elapsed < _dialogueFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / _dialogueFadeDuration);
            mainColor.a = alpha;
            subColor.a = alpha;
            _dialogueText.color = mainColor;
            if (_penaltyText != null) _penaltyText.color = subColor;
            yield return null;
        }

        mainColor.a = 0f;
        subColor.a = 0f;
        _dialogueText.color = mainColor;
        if (_penaltyText != null) _penaltyText.color = subColor;

        _dialogueText.text = string.Empty;
        if (_penaltyText != null) _penaltyText.text = string.Empty;

        onComplete?.Invoke();
    }

    public void SetBlack()
    {
        _fadeImage.color = new Color(0f, 0f, 0f, 1f);
    }

    public void SetClear()
    {
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }
}