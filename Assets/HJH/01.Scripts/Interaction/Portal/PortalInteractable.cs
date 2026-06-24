using UnityEngine;

public class PortalInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction UI")]
    [SerializeField] private Transform _popupPoint;
    [SerializeField] private Sprite _interactionIcon;

    [Header("Quest")]
    [SerializeField] private string _objectId;
    [SerializeField] private string _acceptQuestId;
    [SerializeField] private QuestDefinition _unlockFromQuest;
    [SerializeField] private DialoguePanel _dialoguePanel;

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

        // pending 상태도 허용 (Quest 2 완료 후 Quest 3 pending 시 포탈 열림)
        QuestDefinition pending = qm.PendingQuest;
        check = _unlockFromQuest;
        while (check != null)
        {
            if (check == pending)
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
        if (!string.IsNullOrEmpty(_objectId) && QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportInteract(_objectId);
        }

        QuestDefinition pending = QuestManager.Instance != null ? QuestManager.Instance.PendingQuest : null;
        if (pending != null && pending.questId == _acceptQuestId)
        {
            string[] startDialogues = pending.startDialogues;

            GameCore.PlayerInputs.SetUIInput();
            GameCore.PlayerInputs.SetCursorLock(false);
            GameCore.GameController.InactiveCoreUI();

            if (_dialoguePanel != null && startDialogues != null && startDialogues.Length > 0)
            {
                _dialoguePanel.Show(startDialogues, () =>
                {
                    GameCore.GameController.ActiveCoreUI();
                    QuestManager.Instance.AcceptPendingQuest();
                    GameCore.PortalUIController.OpenPortalUI();
                });
                return;
            }

            GameCore.GameController.ActiveCoreUI();
            QuestManager.Instance.AcceptPendingQuest();
            GameCore.PortalUIController.OpenPortalUI();
            return;
        }

        GameCore.PortalUIController.OpenPortalUI();
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