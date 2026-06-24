using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableQuickSlotUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("Input")]
    [SerializeField] private PlayerInputs _playerInputs;

    [Header("Player")]
    [SerializeField] private PlayerStats _playerStats;

    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private Image _cooldownFillImage;
    [SerializeField] private TMP_Text _cooldownText;

    [Header("Settings")]
    [SerializeField] private float _cooldownDuration = 3f;

    private ConsumableItemData _currentItemData;
    public ConsumableItemData CurrentItemData => _currentItemData;

    private float _cooldownRemaining;
    private bool _isOnCooldown => _cooldownRemaining > 0f;

    private void Awake()
    {
        Clear();
        SetCooldown(0f, 0f);
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
        {
            _cooldownRemaining -= Time.deltaTime;
            if (_cooldownRemaining < 0f) _cooldownRemaining = 0f;
            SetCooldown(_cooldownRemaining, _cooldownDuration);
        }
    }

    private void OnEnable()
    {
        _playerInventory.OnInventoryChanged += Refresh;
        _playerInputs.UseEvent += UseCurrentItem;

        if (_currentItemData == null)
            TryRestoreFromSave();

        Refresh();
    }

    private void TryRestoreFromSave()
    {
        if (SaveManager.Instance?.Data == null) return;
        string savedId = SaveManager.Instance.Data.consumableQuickSlotItemId;
        if (string.IsNullOrEmpty(savedId)) return;

        foreach (InventoryItem item in _playerInventory.Items)
        {
            if (item.ItemData is ConsumableItemData consumable && consumable.ItemId == savedId && item.Quantity > 0)
            {
                _currentItemData = consumable;
                return;
            }
        }
    }

    private void OnDisable()
    {
        _playerInventory.OnInventoryChanged -= Refresh;
        _playerInputs.UseEvent -= UseCurrentItem;
    }

    public void RegisterItem(ConsumableItemData itemData)
    {
        if (itemData == null)
        {
            Clear();
            return;
        }

        _currentItemData = itemData;
        Refresh();

        Debug.Log($"[ConsumableQuickSlot] 등록된 아이템 {itemData.ItemName}");
        SaveManager.Instance?.Save();
    }

    private void UseCurrentItem()
    {
        if (_currentItemData == null)
        {
            Debug.Log("[ConsumableQuickSlot] 아이템 없음");
            return;
        }

        if (_isOnCooldown)
        {
            Debug.Log("[ConsumableQuickSlot] 쿨타임 중");
            return;
        }

        if (!_playerInventory.HasItem(_currentItemData, 1))
        {
            Debug.Log("[ConsumableQuickSlot] 아이템 없음");
            Clear();
            return;
        }

        bool used = ApplyConsumableEffect(_currentItemData);

        if (!used)
        {
            Debug.Log("[ConsumableQuickSlot] 아이템 사용 불가");
            return;
        }

        bool removed = _playerInventory.RemoveItem(_currentItemData, 1);

        if (!removed)
        {
            Debug.Log("[ConsumableQuickSlot] 아이템 제거 에러");
            return;
        }

        _cooldownRemaining = _cooldownDuration;
        SetCooldown(_cooldownRemaining, _cooldownDuration);

        Refresh();
    }

    private bool ApplyConsumableEffect(ConsumableItemData itemData)
    {
        switch (itemData.ConsumableType)
        {
            case ConsumableType.Heal:
                if (_playerStats.CurrentHp >= _playerStats.MaxHp)
                {
                    return false;
                }

                _playerStats.Heal(itemData.Value);
                return true;
        }

        return false;
    }

    private void SetCooldown(float remaining, float duration)
    {
        if (_cooldownFillImage == null) return;

        if (remaining <= 0f)
        {
            _cooldownFillImage.fillAmount = 0f;
            _cooldownFillImage.enabled = false;

            if (_cooldownText != null)
            {
                _cooldownText.text = string.Empty;
                _cooldownText.gameObject.SetActive(false);
            }

            return;
        }

        float normalizedValue = duration > 0f ? remaining / duration : 0f;

        _cooldownFillImage.enabled = true;
        _cooldownFillImage.fillAmount = normalizedValue;

        if (_cooldownText != null)
        {
            _cooldownText.gameObject.SetActive(true);
            _cooldownText.text = Mathf.CeilToInt(remaining).ToString();
        }
    }

    private void Refresh()
    {
        if (_currentItemData == null)
        {
            Clear();
            return;
        }

        int quantity = _playerInventory.GetQuantity(_currentItemData);

        if (quantity <= 0)
        {
            Clear();
            return;
        }

        _icon.gameObject.SetActive(true);
        _icon.sprite = _currentItemData.Icon;
        _quantityText.text = quantity.ToString();
    }

    private void Clear()
    {
        _currentItemData = null;

        _icon.sprite = null;
        _icon.gameObject.SetActive(false);
        _quantityText.text = string.Empty;
    }
}