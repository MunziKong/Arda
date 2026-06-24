using UnityEngine;

public class CodexInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Transform _popupPoint;
    [SerializeField] private Sprite _interactionIcon;

    [Header("Outline")]
    [SerializeField] private EPOOutline.Outlinable _outlinable;

    public Transform PopupPoint => _popupPoint;
    public Sprite InteractionIcon => _interactionIcon;
    public bool IsEnabled => true;
    public bool RequireHold => false;
    public float HoldTime => 0f;

    private void Awake()
    {
        if (GameCore.PlayerOwnedData != null && GameCore.PlayerOwnedData.IsCodexUnlocked)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetOutline(bool enable)
    {
        if (_outlinable == null) return;
        _outlinable.enabled = enable;
    }

    public void Interact()
    {
        GameCore.PlayerOwnedData.UnlockCodex();
        GameCore.ActivateCodexIconPanel();
        GameCore.AlertManager.Enqueue(AlertType.EnableCodex);
        Destroy(gameObject);
    }
}