using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftMaterialSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _requiredText;

    [Header("Colors")]
    [SerializeField] private Color _enoughColor = Color.white;
    [SerializeField] private Color _notEnoughColor = Color.red;

    public void Setup(ItemRequirement requirement, int quantity = 1)
    {
        if (requirement == null || requirement.item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        _icon.sprite = requirement.item.Icon;
        _nameText.text = requirement.item.ItemName;

        int owned = GameCore.PlayerInventory.GetQuantity(requirement.item);
        int required = requirement.amount * quantity;
        bool isEnough = owned >= required;

        _requiredText.text = $"{owned} / {required}";
        _requiredText.color = isEnough ? _enoughColor : _notEnoughColor;
    }
}