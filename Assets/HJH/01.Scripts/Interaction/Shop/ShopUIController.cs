using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    [Header("Content Root")]
    [SerializeField] private GameObject _contentRoot;

    [Header("Overlay Root")]
    [SerializeField] private GameObject _overlayRoot;
    [SerializeField] private ShopDetailUI _shopDetail;
    [SerializeField] private Button _overlayBg;

    [Header("Category")]
    [SerializeField] private Button _skillCategoryBtn;
    [SerializeField] private Button _consumableCategoryBtn;
    [SerializeField] private Button _rescueCategoryBtn;
    [SerializeField] private Button _recipeCategoryBtn;
    [SerializeField] private Button _skinCategoryBtn;

    [Header("Gold")]
    [SerializeField] private TMP_Text _goldText;

    [Header("Slot")]
    [SerializeField] private ShopSlotUI _slotPrefab;
    [SerializeField] private Transform _slotContent;

    private List<ShopItemEntry> _shopItems = new();
    private ShopItemCategory _currentCategory;
    public bool IsDetailOpen => _overlayRoot.activeSelf;

    private void Awake()
    {
        _skillCategoryBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); SelectCategory(ShopItemCategory.Skill); });
        _consumableCategoryBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); SelectCategory(ShopItemCategory.ConsumableItem); });
        _rescueCategoryBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); SelectCategory(ShopItemCategory.RescueItem); });
        _recipeCategoryBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); SelectCategory(ShopItemCategory.Recipe); });
        _skinCategoryBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); SelectCategory(ShopItemCategory.Skin); });
        _overlayBg.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); CloseDetail(); });
    }

    public void OpenPanel(List<ShopItemEntry> shopItems)
    {
        if (shopItems != null)
        {
            _shopItems = shopItems;
        }
        else
        {
            _shopItems = new List<ShopItemEntry>();
        }

        _contentRoot.SetActive(true);
        _overlayRoot.SetActive(false);
        _shopDetail.ResetDetail();

        RefreshCategoryButtons();
        RefreshGoldText();

        ShopItemCategory firstAvailable = GetFirstAvailableCategory();
        SelectCategory(firstAvailable);
    }


    public void CloseDetail()
    {
        _shopDetail.ResetDetail();
        _overlayRoot.SetActive(false);
    }

    private ShopItemCategory GetFirstAvailableCategory()
    {
        ShopItemCategory[] priority = new ShopItemCategory[]
        {
            ShopItemCategory.ConsumableItem,
            ShopItemCategory.RescueItem,
            ShopItemCategory.Skill,
            ShopItemCategory.Recipe,
            ShopItemCategory.Skin
        };

        foreach (ShopItemCategory category in priority)
        {
            if (_shopItems.Any(item => item.category == category))
            {
                return category;
            }
        }

        return ShopItemCategory.ConsumableItem;
    }

    private void RefreshCategoryButtons()
    {
        _skillCategoryBtn.gameObject.SetActive(_shopItems.Any(i => i.category == ShopItemCategory.Skill));
        _consumableCategoryBtn.gameObject.SetActive(_shopItems.Any(i => i.category == ShopItemCategory.ConsumableItem));
        _rescueCategoryBtn.gameObject.SetActive(_shopItems.Any(i => i.category == ShopItemCategory.RescueItem));
        _recipeCategoryBtn.gameObject.SetActive(_shopItems.Any(i => i.category == ShopItemCategory.Recipe));
        _skinCategoryBtn.gameObject.SetActive(_shopItems.Any(i => i.category == ShopItemCategory.Skin));
    }

    private void SelectCategory(ShopItemCategory category)
    {
        _currentCategory = category;
        RefreshSlotList();
    }

    private void RefreshSlotList()
    {
        for (int i = _slotContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_slotContent.GetChild(i).gameObject);
        }

        List<ShopItemEntry> filtered = _shopItems
            .Where(item => item.category == _currentCategory)
            .ToList();

        if (_currentCategory == ShopItemCategory.Skill)
        {
            filtered = filtered
                .OrderBy(item => item.conditionWeapon != null ? item.conditionWeapon.name : string.Empty)
                .ThenBy(item => item.conditionValue)
                .ToList();
        }

        foreach (ShopItemEntry entry in filtered)
        {
            ShopSlotUI slot = Instantiate(_slotPrefab, _slotContent);
            slot.Setup(entry, OnSlotClicked);
        }
    }

    private void OnSlotClicked(ShopItemEntry entry)
    {
        // _contentRoot.SetActive(false);
        _overlayRoot.SetActive(true);

        _shopDetail.Setup(entry, OnPurchase);
    }

    private void OnPurchase(ShopItemEntry entry, int quantity)
    {
        ShopPurchaseResult result = GameCore.ShopManager.TryPurchase(entry, quantity);

        Debug.Log($"[Shop] {entry.GetDisplayName()} x{quantity} - 결과: {result}");

        if (result == ShopPurchaseResult.Success)
        {
            CloseDetail();
            RefreshSlotList();
            RefreshGoldText();

            // 여기에 AlertUI 같은거 추가해야할듯..
        }

        // 실패 시에도 이유 추가..
    }

    private void RefreshGoldText()
    {
        if (_goldText == null)
        {
            return;
        }

        _goldText.text = GameCore.PlayerInventory.Gold.ToString("N0");
    }
}