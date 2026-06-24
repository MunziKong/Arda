using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RescueQuickSlotUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("Input")]
    [SerializeField] private PlayerInputs _playerInputs;

    [Header("Default RescueItemData")]
    [SerializeField] private RescueItemData _defaultRescueItem;

    [Header("Rescue UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _quantityText;

    private readonly List<RescueItemData> _availableItems = new();

    private int _currentIndex;

    public RescueItemData CurrentItem
    {
        get
        {
            if (_availableItems.Count == 0)
            {
                return null;
            }

            return _availableItems[_currentIndex];
        }
    }

    private void Awake()
    {
        RefreshAvailableItems();
        RefreshUI();
    }

    private void OnEnable()
    {
        _playerInventory.OnInventoryChanged += HandleInventoryChanged;
        _playerInputs.ChangeRescueItemEvent += NextItem;
    }

    private void OnDisable()
    {
        _playerInventory.OnInventoryChanged -= HandleInventoryChanged;
        _playerInputs.ChangeRescueItemEvent -= NextItem;
    }

    public void NextItem()
    {
        if (_availableItems.Count <= 1)
        {
            return;
        }

        _currentIndex++;

        if (_currentIndex >= _availableItems.Count)
        {
            _currentIndex = 0;
        }

        RefreshUI();
        SaveManager.Instance?.Save();

        // Debug.Log($"[RescueQuickSlot] Current Item: {CurrentItem.ItemName}");
    }

    public void RestoreItemById(string itemId)
    {
        RefreshAvailableItems();

        for (int i = 0; i < _availableItems.Count; i++)
        {
            if (_availableItems[i].ItemId == itemId)
            {
                _currentIndex = i;
                RefreshUI();
                return;
            }
        }

        _currentIndex = 0;
        RefreshUI();
    }

    private void HandleInventoryChanged()
    {
        RescueItemData previousItem = CurrentItem;

        Debug.Log("HandleInventoryChanged()");

        RefreshAvailableItems();

        if (previousItem != null)
        {
            int previousIndex = _availableItems.IndexOf(previousItem);

            if (previousIndex >= 0)
            {
                _currentIndex = previousIndex;
            }
            else
            {
                _currentIndex = 0;
            }
        }

        RefreshUI();
    }

    public void ForceRefresh()
    {
        RefreshAvailableItems();
        RefreshUI();
    }

    private void RefreshAvailableItems()
    {
        _availableItems.Clear();

        if (_defaultRescueItem != null)
            _availableItems.Add(_defaultRescueItem);

        var items = _playerInventory.Items.ToList();
        var rescueItems = new List<RescueItemData>();

        foreach (InventoryItem item in items)
        {
            if (item?.ItemData is RescueItemData rescueItem)
            {
                if (item.Quantity > 0 && !rescueItem.IsDefaultKit)
                    rescueItems.Add(rescueItem);
            }
        }

        rescueItems.Sort((a, b) => a.SuccessBonusRate.CompareTo(b.SuccessBonusRate));

        _availableItems.AddRange(rescueItems);

        if (_currentIndex >= _availableItems.Count)
            _currentIndex = 0;
    }

    private void RefreshUI()
    {
        RescueItemData item = CurrentItem;

        if (item == null)
        {
            Clear();
            return;
        }

        _icon.gameObject.SetActive(true);
        _icon.sprite = item.Icon;

        if (item.IsDefaultKit)
        {
            _quantityText.text = "∞";
        }
        else
        {
            int quantity = _playerInventory.GetQuantity(item);
            _quantityText.text = quantity.ToString();
        }
    }

    public bool UseCurrentRescueItem()
    {
        RescueItemData currentItem = CurrentItem;

        if (currentItem == null)
        {
            return false;
        }

        if (currentItem.IsDefaultKit)
        {
            return true;
        }
        bool removed = _playerInventory.RemoveItem(currentItem, 1);

        if (!removed)
        {
            RefreshAvailableItems();
            RefreshUI();
            return false;
        }

        Debug.Log($"<color=yellow>[RescueQuickSlot]</color> currentItem 사용 : {currentItem.ItemName}");
        RefreshAvailableItems();

        if (_currentIndex >= _availableItems.Count)
        {
            _currentIndex = 0;
        }

        RefreshUI();

        return true;
    }

    private void Clear()
    {
        _icon.sprite = null;
        _icon.gameObject.SetActive(false);
        _quantityText.text = string.Empty;
    }
}