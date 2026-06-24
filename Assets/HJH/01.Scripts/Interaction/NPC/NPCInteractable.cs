using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction UI")]
    [SerializeField] private Transform _popupPoint;
    [SerializeField] private Sprite _interactionIcon;

    [Header("Camera")]
    [SerializeField] private Transform _cameraViewPoint;

    [Header("Controller")]
    [SerializeField] private NPCUIController _npcUI;

    [Header("Quest")]
    [SerializeField] private string _objectId;
    [SerializeField] private DialoguePanel _completionDialoguePanel;

    [Header("Contents")]
    [SerializeField] private List<NPCContentType> _availableContents = new();
    [SerializeField] private QuestDefinition _fullMenuUnlockQuest;

    [Header("엔딩")]
    [SerializeField] private QuestDefinition _lastQuestBeforeEnding;
    [SerializeField] private EndingPanel _endingPanel;

    [Header("Shop")]
    [SerializeField] private List<ShopItemEntry> _shopItems = new();

    [Header("Outline")]
    [SerializeField] private EPOOutline.Outlinable _outlinable;

    public List<ShopItemEntry> ShopItems => _shopItems;
    public Transform PopupPoint => _popupPoint;
    public Sprite InteractionIcon => _interactionIcon;

    public bool IsEnabled => true;
    public bool RequireHold => false;
    public float HoldTime => 0f;

    public void Interact()
    {
        SetOutline(false);
        QuestDefinition pending = QuestManager.Instance != null ? QuestManager.Instance.PendingQuest : null;
        if (pending != null && !pending.acceptAtPortalOnly)
        {
            GameCore.GameController.FocusNPC(_cameraViewPoint, () =>
            {
                string[] startDialogues = pending.startDialogues;
                if (_completionDialoguePanel != null && startDialogues != null && startDialogues.Length > 0)
                {
                    _completionDialoguePanel.Show(startDialogues, () =>
                    {
                        QuestManager.Instance.AcceptPendingQuest();
                        GameCore.ActivateQuickSlotPanel();
                        _npcUI.EndInteraction();
                    });
                }
                else
                {
                    QuestManager.Instance.AcceptPendingQuest();
                    GameCore.ActivateQuickSlotPanel();
                    _npcUI.EndInteraction();
                }
            });
            return;
        }

        if (!string.IsNullOrEmpty(_objectId) && QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportInteract(_objectId);

            if (QuestManager.Instance.IsQuestCompleted && QuestManager.Instance.ActiveQuest != null)
            {
                string[] completeDialogues = QuestManager.Instance.ActiveQuest.completeDialogues;

                if (_completionDialoguePanel != null && completeDialogues != null && completeDialogues.Length > 0)
                {
                    GameCore.GameController.FocusNPC(_cameraViewPoint, () =>
                    {
                        _completionDialoguePanel.Show(completeDialogues, () =>
                        {
                            _npcUI.EndInteraction();
                            QuestManager.Instance.CompleteQuest();
                        });
                    });
                    return;
                }

                QuestManager.Instance.CompleteQuest();
            }
        }

        if (_endingPanel != null && IsEndingConditionMet())
        {
            GameCore.GameController.FocusNPC(_cameraViewPoint, () =>
            {
                _endingPanel.StartEnding(() =>
                {
                    MarkEndingCompleted();
                    _npcUI.EndInteraction();
                });
            });
            return;
        }

        OpenNPCPanel();

    }

    private bool IsEndingConditionMet()
    {
        if (_lastQuestBeforeEnding == null) { Debug.Log("[Ending] _lastQuestBeforeEnding 미설정"); return false; }
        if (_endingPanel == null) { Debug.Log("[Ending] _endingPanel 미설정"); return false; }

        SaveData data = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
        if (data == null) { Debug.Log("[Ending] SaveData 없음"); return false; }
        if (data.endingCompleted) { Debug.Log("[Ending] 이미 완료됨"); return false; }

        QuestManager qm = QuestManager.Instance;
        Debug.Log($"[Ending] lastStartedQuestId='{data.lastStartedQuestId}' 필요='{_lastQuestBeforeEnding.questId}' ActiveQuest={qm?.ActiveQuest?.questId} PendingQuest={qm?.PendingQuest?.questId}");

        if (data.lastStartedQuestId != _lastQuestBeforeEnding.questId) return false;

        return qm != null && qm.ActiveQuest == null && qm.PendingQuest == null;
    }

    private void MarkEndingCompleted()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null) return;
        SaveManager.Instance.Data.endingCompleted = true;
        SaveManager.Instance.Save();
    }

    private void OpenNPCPanel()
    {
        GameCore.GameController.FocusNPC(_cameraViewPoint, () =>
        {
            _npcUI.OpenPanel(GetAvailableContents(), _shopItems);
        });
    }

    private List<NPCContentType> GetAvailableContents()
    {
        if (IsFullMenuUnlocked())
        {
            return _availableContents;
        }

        if (_availableContents.Contains(NPCContentType.Quest))
        {
            return new List<NPCContentType> { NPCContentType.Quest };
        }

        return _availableContents;
    }

    private bool IsFullMenuUnlocked()
    {
        if (_fullMenuUnlockQuest == null)
        {
            return true;
        }

        QuestManager qm = QuestManager.Instance;
        if (qm == null)
        {
            return true;
        }

        QuestDefinition active = qm.ActiveQuest;

        if (active == null)
        {
            return SaveManager.Instance != null
                && SaveManager.Instance.Data != null
                && SaveManager.Instance.Data.fullMenuUnlocked;
        }

        if (active == _fullMenuUnlockQuest)
        {
            MarkFullMenuUnlocked();
            return true;
        }

        QuestDefinition check = _fullMenuUnlockQuest.nextQuest;
        while (check != null)
        {
            if (check == active)
            {
                MarkFullMenuUnlocked();
                return true;
            }

            check = check.nextQuest;
        }

        return false;
    }

    private void MarkFullMenuUnlocked()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            return;
        }

        if (!SaveManager.Instance.Data.fullMenuUnlocked)
        {
            SaveManager.Instance.Data.fullMenuUnlocked = true;
            SaveManager.Instance.Save();
        }
    }

    public void SetOutline(bool enable)
    {
        if (_outlinable == null)
        {
            return;
        }

        _outlinable.enabled = enable;
    }

}