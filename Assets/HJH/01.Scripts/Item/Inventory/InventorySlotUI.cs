using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private Button _button;

    private InventoryItem _currentItem;

    public event Action<InventoryItem> OnClicked;

    private void Awake()
    {
        _button.onClick.AddListener(HandleClick);
    }

    public void Bind(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.ItemData == null)
        {
            Clear();
            return;
        }

        _currentItem = inventoryItem;

        ItemData itemData = inventoryItem.ItemData;

        _icon.gameObject.SetActive(true);
        _icon.sprite = itemData.Icon;

        _nameText.text = itemData.ItemName;
        _quantityText.text = inventoryItem.Quantity.ToString();

        _button.interactable = true;
    }

    public void Clear()
    {
        _currentItem = null;

        _icon.sprite = null;
        _icon.gameObject.SetActive(false);

        _nameText.text = string.Empty;
        _quantityText.text = string.Empty;

        _button.interactable = false;
    }

    private void HandleClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_currentItem == null)
        {
            return;
        }

        OnClicked?.Invoke(_currentItem);
    }
}