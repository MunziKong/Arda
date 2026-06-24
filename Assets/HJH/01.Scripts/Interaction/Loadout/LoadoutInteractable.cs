using UnityEngine;

public class LoadoutInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction UI")]
    [SerializeField] private Transform _popupPoint;
    [SerializeField] private Sprite _interactionIcon;

    [Header("Quest")]
    [SerializeField] private string _objectId;
    [SerializeField] private QuestDefinition _targetQuest;
    [SerializeField] private QuestDefinition _unlockFromQuest;
    [SerializeField] private DialoguePanel _completionDialoguePanel;

    [Header("Loadout UI")]
    [SerializeField] private LoadoutUIController _loadoutUI;

    [Header("Outline")]
    [SerializeField] private EPOOutline.Outlinable _outlinable;


    public Transform PopupPoint => _popupPoint;
    public Sprite InteractionIcon => _interactionIcon;

    public bool IsEnabled => IsUnlocked();
    public bool RequireHold => false;
    public float HoldTime => 0f;

    private bool IsUnlocked()
    {
        if (_unlockFromQuest == null)
        {
            return true;
        }

        QuestManager qm = QuestManager.Instance;
        if (qm == null)
        {
            return true;
        }

        QuestDefinition active = qm.ActiveQuest;

        QuestDefinition check = _unlockFromQuest;
        while (check != null)
        {
            if (check == active)
            {
                return true;
            }

            check = check.nextQuest;
        }

        if (active == null && SaveManager.Instance != null && SaveManager.Instance.Data != null)
        {
            string lastId = SaveManager.Instance.Data.lastStartedQuestId;
            if (!string.IsNullOrEmpty(lastId))
            {
                QuestDefinition last = GameCore.GameDatabase != null ? GameCore.GameDatabase.GetQuest(lastId) : null;
                check = _unlockFromQuest;
                while (check != null)
                {
                    if (check == last)
                    {
                        return true;
                    }

                    check = check.nextQuest;
                }
            }
        }

        return false;
    }

    public void Interact()
    {
        if (!string.IsNullOrEmpty(_objectId) && QuestManager.Instance != null
            && (_targetQuest == null || QuestManager.Instance.ActiveQuest == _targetQuest))
        {
            QuestManager.Instance.ReportInteract(_objectId);

            if (QuestManager.Instance.IsQuestCompleted && QuestManager.Instance.ActiveQuest != null)
            {
                string[] completeDialogues = QuestManager.Instance.ActiveQuest.completeDialogues;

                if (_completionDialoguePanel != null && completeDialogues != null && completeDialogues.Length > 0)
                {
                    GameCore.PlayerInputs.SetUIInput();
                    GameCore.PlayerInputs.SetCursorLock(false);
                    GameCore.GameController.InactiveCoreUI();

                    _completionDialoguePanel.Show(completeDialogues, () =>
                    {
                        GameCore.GameController.ActiveCoreUI();
                        QuestManager.Instance.CompleteQuest();
                        GameCore.ActivateSkillSetPanel();
                        _loadoutUI.OpenPanel();
                    });
                    return;
                }

                QuestManager.Instance.CompleteQuest();
                GameCore.ActivateSkillSetPanel();
                return;
            }
        }

        _loadoutUI.OpenPanel();
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