using UnityEngine;
using System;

[Serializable]
public class InventoryItem
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private int _quantity;

    public ItemData ItemData => _itemData;
    public int Quantity => _quantity;
    public bool IsEmpty => _quantity <= 0;

    public InventoryItem(ItemData itemData, int quantity)
    {
        _itemData = itemData;
        _quantity = quantity;
    }

    public void AddQuantity(int amount)
    {
        _quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        _quantity = Mathf.Max(0, _quantity - amount);
    }

    public void SetQuantity(int quantity)
    {
        _quantity = quantity;
    }
}