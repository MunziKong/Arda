using UnityEngine;

public interface IInteractable
{
    Transform PopupPoint { get; }
    // string InteractionText { get; }
    Sprite InteractionIcon { get; }

    bool IsEnabled { get; }
    bool RequireHold { get; }
    float HoldTime { get; }

    void Interact();
    void SetOutline(bool enable);
}