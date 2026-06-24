using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Arda/Item/ConsumableItem")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable")]
    [SerializeField] private ConsumableType _consumableType;
    [SerializeField] private int _value;

    public ConsumableType ConsumableType => _consumableType;
    public int Value => _value;
}