using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceMaterialSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private Image _backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color _enoughColor = Color.green;
    [SerializeField] private Color _notEnoughColor = Color.red;

    public void Setup(ItemRequirement requirement)
    {
        if (requirement == null || requirement.item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        _icon.sprite = requirement.item.Icon;
        _name.text = requirement.item.ItemName;

        int owned = GameCore.PlayerInventory.GetQuantity(requirement.item);
        bool isEnough = owned >= requirement.amount;

        if (requirement.item is CurrencyItemData)
        {
            _quantityText.text = requirement.amount.ToString("N0");
        }
        else
        {
            _quantityText.text = $"{owned} / {requirement.amount}";
        }

        if (_backgroundImage != null)
        {
            _backgroundImage.color = isEnough ? _enoughColor : _notEnoughColor;
        }
    }
}