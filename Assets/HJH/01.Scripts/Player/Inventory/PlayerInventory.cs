using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly List<InventoryItem> _items = new();
    [SerializeField] private int _gold = 0;

    public List<InventoryItem> Items => _items;
    public int Gold => _gold;
    public event Action OnInventoryChanged;

    private PlayerInputs _inputs;

    [Header("Test Items")]
    [SerializeField] private ItemData _healthPotion;
    [SerializeField] private ItemData _materialItem;
    [SerializeField] private ItemData _rescueItem;
    private List<ItemData> _testData = new();

    void Awake()
    {
        _inputs = GetComponent<PlayerInputs>();
        _testData.Add(_healthPotion);
        _testData.Add(_materialItem);
        _testData.Add(_rescueItem);
    }

    void OnEnable()
    {
        _inputs.TestEvent += TestItem;
        _inputs.Test1Event += Test1Item;
        _inputs.Test2Event += Test2Item;
    }

    void OnDisable()
    {
        _inputs.TestEvent -= TestItem;
        _inputs.Test1Event -= Test1Item;
        _inputs.Test2Event -= Test2Item;
    }


    void TestItem()
    {
        AddItem(_healthPotion, 1);
    }

    void Test1Item()
    {
        RemoveItem(_healthPotion, 1);
    }

    void Test2Item()
    {
        AddItem(_rescueItem, 1);
    }

    public void AddItem(ItemData itemData, int amount)
    {
        Debug.Log($"AddItem : {itemData.ItemName} * {amount}");
        if (itemData == null || amount <= 0)
        {
            Debug.Log("AddItem  itemData == null || amount <= 0");
            return;
        }

        if (itemData is CurrencyItemData)
        {
            Debug.Log("AddItem itemData is CurrencyItemData");
            AddGold(amount);
            return;
        }

        InventoryItem existingItem = FindItem(itemData);

        if (existingItem != null)
        {
            if (existingItem.Quantity >= itemData.MaxStack)
            {
                Debug.Log($"<color=red>[Inventory]</color> {itemData.ItemName} 최대 수량 초과");
                return;
            }

            existingItem.AddQuantity(amount);

            if (existingItem.Quantity > itemData.MaxStack)
            {
                existingItem.SetQuantity(itemData.MaxStack);
            }

            Debug.Log($"<color=blue>[Inventory]</color> Item : {itemData.ItemName} (+{amount}) / Current : {existingItem.Quantity}");
        }
        else
        {
            _items.Add(new InventoryItem(itemData, amount));

            Debug.Log($"<color=blue>[Inventory]</color> New Item : {itemData.ItemName} x{amount}");
        }

        // PrintInventory();

        OnInventoryChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _gold += amount;

        Debug.Log($"<color=yellow>[Inventory]</color> Gold +{amount} / Current : {_gold}");

        OnInventoryChanged?.Invoke();
    }

    public bool RemoveGold(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (_gold < amount)
        {
            return false;
        }

        _gold -= amount;

        Debug.Log($"<color=yellow>[Inventory]</color> Gold -{amount} / Current : {_gold}");

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        if (itemData is CurrencyItemData)
        {
            return RemoveGold(amount);
        }

        InventoryItem existingItem = FindItem(itemData);

        if (existingItem == null)
        {
            return false;
        }

        if (existingItem.Quantity < amount)
        {
            return false;
        }

        existingItem.RemoveQuantity(amount);


        if (existingItem.IsEmpty)
        {
            _items.Remove(existingItem);
        }

        // PrintInventory();

        OnInventoryChanged?.Invoke();
        return true;
    }

    private void PrintInventory()
    {
        Debug.Log("===== Inventory =====");

        if (_items.Count == 0)
        {
            Debug.Log("Empty");
        }

        foreach (InventoryItem item in _items)
        {
            Debug.Log($"{item.ItemData.ItemName} x{item.Quantity}");
        }

        Debug.Log("=====================");
    }

    public bool HasItem(ItemData itemData, int amount = 1)
    {
        if (itemData is CurrencyItemData)
        {
            return _gold >= amount;
        }

        return GetQuantity(itemData) >= amount;
    }

    public int GetQuantity(ItemData itemData)
    {
        if (itemData is CurrencyItemData)
        {
            return _gold;
        }

        InventoryItem existingItem = FindItem(itemData);

        if (existingItem == null)
        {
            return 0;
        }

        return existingItem.Quantity;
    }

    public void ResetState()
    {
        _items.Clear();
        _gold = 0;
        OnInventoryChanged?.Invoke();
    }

    private InventoryItem FindItem(ItemData itemData)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].ItemData == itemData)
            {
                return _items[i];
            }

            if (!string.IsNullOrEmpty(itemData.ItemId) &&
                _items[i].ItemData.ItemId == itemData.ItemId)
            {
                return _items[i];
            }
        }

        return null;
    }
}