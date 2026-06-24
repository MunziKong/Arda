using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string _itemId;
    [SerializeField] private string _itemName;
    [TextArea]
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;

    [Header("Stack")]
    [SerializeField] private int _maxStack = 99;

    public string ItemId => _itemId;
    public string ItemName => _itemName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int MaxStack => _maxStack;
}