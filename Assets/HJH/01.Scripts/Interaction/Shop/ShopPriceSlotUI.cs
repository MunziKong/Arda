using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPriceSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _priceText;

    public void Setup(ItemRequirement requirement, int quantity = 1)
    {
        if (requirement == null || requirement.item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        _icon.sprite = requirement.item.Icon;
        _priceText.text = (requirement.amount * quantity).ToString();
    }
}