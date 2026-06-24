using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class QuestIntroUI : MonoBehaviour
{
    [Header("페이드 패널")]
    [SerializeField] private CanvasGroup _fadePanel;

    [Header("퀘스트 이름 패널")]
    [SerializeField] private CanvasGroup _questNamePanel;
    [SerializeField] private TMP_Text _questNameText;

    [Header("타이밍")]
    [SerializeField] private float _questNameFadeDuration = 0.4f;
    [SerializeField] private float _questNameHoldTime = 1.5f;
    [SerializeField] private float _fastBlinkDuration = 0.08f;
    [SerializeField] private float _slowBlinkDuration = 0.3f;

    public void PlayIntro(QuestDefinition quest, Action onComplete)
    {
        gameObject.SetActive(true);
        _questNameText.text = quest.questName;
        StartCoroutine(IntroRoutine(onComplete));
    }

    private IEnumerator IntroRoutine(Action onComplete)
    {
        _questNamePanel.alpha = 0f;
        _fadePanel.alpha = 1f;

        yield return StartCoroutine(Fade(_questNamePanel, 0f, 1f, _questNameFadeDuration));
        yield return new WaitForSeconds(_questNameHoldTime);
        yield return StartCoroutine(Fade(_questNamePanel, 1f, 0f, _questNameFadeDuration));

        // 느린 깜빡임 시작 시점에 컷씬 트리거, 깜빡임 완전히 끝난 후 QuestIntroUI 비활성화
        yield return StartCoroutine(GlitchReveal(onComplete));

        gameObject.SetActive(false);
    }

    // 빠르게 2번 → 느린 깜빡임 시작 시 onSlowBlinkStart 호출 → 느리게 2번 후 게임화면 유지
    private IEnumerator GlitchReveal(Action onSlowBlinkStart)
    {
        for (int i = 0; i < 2; i++)
        {
            _fadePanel.alpha = 0f;
            yield return new WaitForSeconds(_fastBlinkDuration);
            _fadePanel.alpha = 1f;
            yield return new WaitForSeconds(_fastBlinkDuration);
        }

        onSlowBlinkStart?.Invoke();

        _fadePanel.alpha = 0f;
        yield return new WaitForSeconds(_slowBlinkDuration);
        _fadePanel.alpha = 1f;
        yield return new WaitForSeconds(_slowBlinkDuration);
        _fadePanel.alpha = 0f;
        yield return new WaitForSeconds(_slowBlinkDuration);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }
}
