using System;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public class QuestHint
{
    [Tooltip("이 퀘스트 ID가 완료된 후 힌트를 표시합니다")]
    public string afterQuestId;
    [TextArea(1, 3)]
    public string hintMessage;
}

public class MainQuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _questNameText;
    [SerializeField] private TMP_Text _conditionText;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("퀘스트 없을 때 힌트")]
    [SerializeField] private string _hintTitle = "다음 목표";
    [SerializeField] private QuestHint[] _questHints;

    private string _lastCompletedQuestId;
    private const string PrefsKey = "MainQuestUI_LastCompletedQuestId";
    private bool _initialized;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        if (!_initialized) return;

        if (QuestManager.Instance != null && QuestManager.Instance.ActiveQuest != null)
            Show(QuestManager.Instance.ActiveQuest);
        else
            ShowHintOrHide();
    }

    private void Start()
    {
        _initialized = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;
        }

        if (QuestManager.Instance != null && QuestManager.Instance.ActiveQuest != null)
        {
            Show(QuestManager.Instance.ActiveQuest);
        }
        else
        {
            _lastCompletedQuestId = PlayerPrefs.GetString(PrefsKey, string.Empty);

            // 저장 데이터 기반 폴백: activeQuestId가 없으면 lastStartedQuestId가 완료된 퀘스트
            if (string.IsNullOrEmpty(_lastCompletedQuestId))
            {
                if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                var data = SaveManager.Instance.Data;
                if (string.IsNullOrEmpty(data.activeQuestId) && !string.IsNullOrEmpty(data.lastStartedQuestId))
                    _lastCompletedQuestId = data.lastStartedQuestId;
            }
            }

            ShowHintOrHide();
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
        }
    }

    private void OnQuestStarted(QuestDefinition quest) => Show(quest);
    private void OnQuestUpdated(QuestDefinition quest) => Show(quest);

    private void OnQuestCompleted(QuestDefinition quest)
    {
        _lastCompletedQuestId = quest.questId;
        PlayerPrefs.SetString(PrefsKey, _lastCompletedQuestId);
        PlayerPrefs.Save();
        ShowHintOrHide();
    }

    public void ForceHide() => Hide();

    private void ShowHintOrHide()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.endingCompleted)
        {
            Hide();
            return;
        }

        if (!string.IsNullOrEmpty(_lastCompletedQuestId) && _questHints != null)
        {
            foreach (QuestHint hint in _questHints)
            {
                if (hint.afterQuestId == _lastCompletedQuestId && !string.IsNullOrEmpty(hint.hintMessage))
                {
                    _questNameText.text = _hintTitle;
                    _conditionText.text = hint.hintMessage;
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.blocksRaycasts = true;
                    return;
                }
            }
        }

        Hide();
    }

    private void Show(QuestDefinition quest)
    {
        _questNameText.text = quest.questName;

        var sb = new StringBuilder();
        var runtimeData = QuestManager.Instance.RuntimeData;

        foreach (QuestCondition condition in quest.conditions)
        {
            if (!string.IsNullOrEmpty(condition.description))
            {
                bool done = runtimeData != null && condition.IsCompleted(runtimeData);
                string text = condition.GetDisplayText(runtimeData);
                sb.AppendLine(done
                    ? $"<color=#808080><s>{text}</s></color>"
                    : text);
            }
        }

        _conditionText.text = sb.ToString().TrimEnd();
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    private void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }
}
