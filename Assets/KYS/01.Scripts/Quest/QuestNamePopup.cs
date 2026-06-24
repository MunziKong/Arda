using System.Collections;
using TMPro;
using UnityEngine;

public class QuestNamePopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup _panel;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private float _fadeDuration = 0.4f;
    [SerializeField] private float _holdDuration = 1.5f;

    private void Start()
    {
        _panel.alpha = 0f;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
        }
    }

    private void OnQuestStarted(QuestDefinition quest)
    {
        if (quest.suppressNamePopup) return;

        StopAllCoroutines();
        StartCoroutine(ShowRoutine(quest.questName));
    }

    private IEnumerator ShowRoutine(string questName)
    {
        _nameText.text = questName;
        _panel.alpha = 0f;

        yield return StartCoroutine(Fade(0f, 1f));
        yield return new WaitForSeconds(_holdDuration);
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        _panel.alpha = from;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _panel.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / _fadeDuration));
            yield return null;
        }

        _panel.alpha = to;
    }
}
