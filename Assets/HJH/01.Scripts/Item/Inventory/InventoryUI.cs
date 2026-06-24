using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("Slots")]
    [SerializeField] private Transform _slotRoot;

    [Header("Detail Panel")]
    [SerializeField] private GameObject _itemDetailPanel;
    [SerializeField] private Image _detailIcon;
    [SerializeField] private TMP_Text _detailQuantityText;
    [SerializeField] private TMP_Text _detailNameText;
    [SerializeField] private TMP_Text _detailDescriptionText;
    [SerializeField] private GameObject _equipButton;
    [SerializeField] private Button _dimmedBg;

    [Header("Gold")]
    [SerializeField] private TMP_Text _gold;

    [Header("Quick Slot")]
    [SerializeField] private ConsumableQuickSlotUI _consumableQuickSlotUI;

    private InventorySlotUI[] _slots;
    private InventoryItem _selectedItem;

    public bool IsDetailOpened => _itemDetailPanel.activeSelf;

    private void Awake()
    {
        _slots = _slotRoot.GetComponentsInChildren<InventorySlotUI>(true);

        Button _equip = _equipButton.GetComponent<Button>();
        _equip.onClick.AddListener(RegisterSelectedConsumableToQuickSlot);

        foreach (InventorySlotUI slot in _slots)
        {
            slot.OnClicked += OpenDetailPanel;
        }

        CloseDetailPanel();
        Refresh();
    }

    private void OnEnable()
    {
        _playerInventory.OnInventoryChanged += Refresh;
        _dimmedBg.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); CloseDetailPanel(); });
        Refresh();
    }

    private void OnDisable()
    {
        _playerInventory.OnInventoryChanged -= Refresh;
        _dimmedBg.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        if (_slots == null)
        {
            return;
        }

        foreach (InventorySlotUI slot in _slots)
        {
            slot.OnClicked -= OpenDetailPanel;
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < _playerInventory.Items.Count)
            {
                _slots[i].Bind(_playerInventory.Items[i]);
            }
            else
            {
                _slots[i].Clear();
            }
        }

        if (_gold != null)
        {
            _gold.text = _playerInventory.Gold.ToString("N0");
        }

        if (_selectedItem != null && _selectedItem.IsEmpty)
        {
            CloseDetailPanel();
        }
    }

    public void OpenDetailPanel(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.ItemData == null)
        {
            CloseDetailPanel();
            return;
        }

        _selectedItem = inventoryItem;

        ItemData itemData = inventoryItem.ItemData;

        _itemDetailPanel.SetActive(true);

        _detailIcon.gameObject.SetActive(true);
        _detailIcon.sprite = itemData.Icon;

        _detailQuantityText.text = "보유 수량 : " + inventoryItem.Quantity.ToString();
        _detailNameText.text = itemData.ItemName;
        _detailDescriptionText.text = itemData.Description;

        bool isConsumable = itemData is ConsumableItemData;
        _equipButton.SetActive(isConsumable);
    }

    public void CloseDetailPanel()
    {
        _selectedItem = null;

        _itemDetailPanel.SetActive(false);

        _detailIcon.sprite = null;
        _detailIcon.gameObject.SetActive(false);

        _detailQuantityText.text = string.Empty;
        _detailNameText.text = string.Empty;
        _detailDescriptionText.text = string.Empty;

        _equipButton.SetActive(false);
    }

    public void RegisterSelectedConsumableToQuickSlot()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_selectedItem == null || _selectedItem.ItemData == null)
        {
            return;
        }

        if (_selectedItem.ItemData is not ConsumableItemData consumableItemData)
        {
            return;
        }

        _consumableQuickSlotUI.RegisterItem(consumableItemData);
        CloseDetailPanel();
    }
}