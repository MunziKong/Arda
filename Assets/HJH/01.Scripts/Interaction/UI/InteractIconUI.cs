using UnityEngine;
using UnityEngine.UI;

public class InteractIconUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;

    private IInteractable _target;

    public IInteractable Target => _target;

    public void SetTarget(IInteractable target)
    {
        _target = target;

        if (_iconImage != null)
        {
            _iconImage.sprite = target.InteractionIcon;
            _iconImage.enabled = target.InteractionIcon != null;
        }

        UpdatePosition();
    }

    public void Clear()
    {
        _target = null;

        if (_iconImage != null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
        }

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_target == null || _target.PopupPoint == null || Camera.main == null)
        {
            return;
        }

        Vector3 screenPos =
            Camera.main.WorldToScreenPoint(_target.PopupPoint.position);

        if (screenPos.z < 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.position = screenPos;
    }
}