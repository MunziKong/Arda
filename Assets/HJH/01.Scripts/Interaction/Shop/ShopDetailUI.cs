using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopDetailUI : MonoBehaviour
{
    [Header("Data Section")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _typeText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Transform _priceContent;
    [SerializeField] private ShopPriceSlotUI _pricePrefab;

    [Header("Purchase Section")]
    [SerializeField] private Button _minusBtn;
    [SerializeField] private Button _plusBtn;
    [SerializeField] private Button _maxBtn;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private Button _purchaseBtn;

    private ShopItemEntry _currentEntry;
    private int _currentQuantity;
    private int _maxQuantity;
    private Action<ShopItemEntry, int> _onPurchase;

    private void Awake()
    {
        _minusBtn.onClick.AddListener(OnMinusClicked);
        _plusBtn.onClick.AddListener(OnPlusClicked);
        _maxBtn.onClick.AddListener(OnMaxClicked);
        _purchaseBtn.onClick.AddListener(OnPurchaseClicked);
    }

    public void Setup(ShopItemEntry entry, Action<ShopItemEntry, int> onPurchase)
    {
        _currentEntry = entry;
        _onPurchase = onPurchase;
        _currentQuantity = 1;

        _maxQuantity = GetMaxQuantity(entry);

        RefreshInfo();
        RefreshQuantity();
        RefreshPurchaseButton();
    }

    public void ResetDetail()
    {
        _currentEntry = null;
        _currentQuantity = 1;

        for (int i = _priceContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_priceContent.GetChild(i).gameObject);
        }
    }

    private int GetMaxQuantity(ShopItemEntry entry)
    {
        if (IsFixedQuantity(entry))
        {
            return 1;
        }

        ItemData item = entry.category == ShopItemCategory.ConsumableItem
            ? entry.consumableItem
            : entry.rescueItem;

        return item != null ? item.MaxStack : 1;
    }

    private bool IsFixedQuantity(ShopItemEntry entry)
    {
        return entry != null &&
               (entry.category == ShopItemCategory.Skill || entry.category == ShopItemCategory.Skin);
    }

    private void RefreshInfo()
    {
        if (_currentEntry == null)
        {
            return;
        }

        _icon.sprite = _currentEntry.GetIcon();
        _nameText.text = _currentEntry.GetDisplayName();
        _descriptionText.text = _currentEntry.GetDescription();

        _typeText.text = _currentEntry.GetCategoryName();

        RefreshPriceSlots();
    }

    private void RefreshPriceSlots()
    {
        for (int i = _priceContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_priceContent.GetChild(i).gameObject);
        }

        if (_currentEntry == null)
        {
            return;
        }

        foreach (ItemRequirement req in _currentEntry.price)
        {
            ShopPriceSlotUI priceSlot = Instantiate(_pricePrefab, _priceContent);
            priceSlot.Setup(req, _currentQuantity);
        }
    }

    private void RefreshQuantity()
    {
        _quantityText.text = _currentQuantity.ToString();

        bool isFixedQuantity = IsFixedQuantity(_currentEntry);

        _minusBtn.interactable = !isFixedQuantity && _currentQuantity > 1;
        _plusBtn.interactable = !isFixedQuantity && _currentQuantity < _maxQuantity;
        _maxBtn.interactable = !isFixedQuantity && _currentQuantity < _maxQuantity;
    }

    private void RefreshPurchaseButton()
    {
        if (_currentEntry == null)
        {
            _purchaseBtn.interactable = false;
            return;
        }

        bool canPress = !GameCore.ShopManager.IsAlreadyOwned(_currentEntry) && GameCore.ShopManager.IsConditionMet(_currentEntry);

        _purchaseBtn.interactable = canPress;
    }

    private void OnMinusClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_currentQuantity <= 1)
        {
            return;
        }

        _currentQuantity--;

        RefreshPriceSlots();
        RefreshQuantity();
        RefreshPurchaseButton();
    }

    private void OnPlusClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_currentQuantity >= _maxQuantity)
        {
            return;
        }

        _currentQuantity++;

        RefreshPriceSlots();
        RefreshQuantity();
        RefreshPurchaseButton();
    }

    private void OnMaxClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (IsFixedQuantity(_currentEntry))
        {
            return;
        }

        _currentQuantity = GetMaxAffordableQuantity(_currentEntry);

        RefreshPriceSlots();
        RefreshQuantity();
        RefreshPurchaseButton();
    }

    private void OnPurchaseClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_currentEntry == null)
        {
            return;
        }

        _onPurchase?.Invoke(_currentEntry, _currentQuantity);
    }

    private int GetMaxAffordableQuantity(ShopItemEntry entry)
    {
        if (entry == null || entry.price == null || entry.price.Count == 0)
            return _maxQuantity;

        int affordable = _maxQuantity;

        foreach (ItemRequirement req in entry.price)
        {
            if (req.item == null || req.amount <= 0) continue;

            int canBuy;

            if (req.item is CurrencyItemData)
            {
                canBuy = GameCore.PlayerInventory.Gold / req.amount;
            }
            else
            {
                int owned = GameCore.PlayerInventory.GetQuantity(req.item);
                canBuy = owned / req.amount;
            }

            affordable = Mathf.Min(affordable, canBuy);
        }

        return Mathf.Max(1, affordable);
    }
}