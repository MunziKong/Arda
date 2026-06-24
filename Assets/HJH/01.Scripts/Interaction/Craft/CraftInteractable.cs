using UnityEngine;

public class CraftInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction UI")]
    [SerializeField] private Transform _popupPoint;
    [SerializeField] private Sprite _interactionIcon;

    [Header("Controller")]
    [SerializeField] private CraftUIController _craftUI;

    [Header("Quest")]
    [SerializeField] private QuestDefinition _unlockFromQuest;

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
        _craftUI.OpenPanel();
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