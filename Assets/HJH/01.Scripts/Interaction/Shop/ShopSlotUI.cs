using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Transform _priceContent;
    [SerializeField] private Button _button;

    [Header("Prefab")]
    [SerializeField] private ShopPriceSlotUI _pricePrefab;

    [Header("Condition Not Met")]
    [SerializeField] private GameObject _conditionNotMet;
    [SerializeField] private TMP_Text _tooltipsText;
    [SerializeField] private GameObject _conditionObject;
    [SerializeField] private TMP_Text _conditionText;

    private ShopItemEntry _entry;

    public ShopItemEntry Entry => _entry;

    public void Setup(ShopItemEntry entry, Action<ShopItemEntry> onClick)
    {
        _entry = entry;

        _icon.sprite = entry.GetIcon();
        _nameText.text = entry.GetDisplayName();

        RefreshPrice(1);
        RefreshCondition();

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); onClick?.Invoke(_entry); });
    }

    public void RefreshPrice(int quantity)
    {
        for (int i = _priceContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_priceContent.GetChild(i).gameObject);
        }

        if (_entry == null)
        {
            return;
        }

        foreach (ItemRequirement req in _entry.price)
        {
            ShopPriceSlotUI priceSlot = Instantiate(_pricePrefab, _priceContent);
            priceSlot.Setup(req, quantity);
        }
    }

    private void RefreshCondition()
    {
        if (_conditionNotMet == null || _entry == null)
        {
            return;
        }

        bool isAlreadyOwned = GameCore.ShopManager.IsAlreadyOwned(_entry);
        bool conditionMet = GameCore.ShopManager.IsConditionMet(_entry);

        bool shouldBlock = isAlreadyOwned || !conditionMet;

        _conditionNotMet.SetActive(shouldBlock);
        _button.interactable = !shouldBlock;

        if (!shouldBlock)
        {
            return;
        }

        if (isAlreadyOwned)
        {
            _tooltipsText.text = _entry.category switch
            {
                ShopItemCategory.Recipe => "이미 보유 중인 레시피",
                ShopItemCategory.Skin => "이미 보유 중인 스킨",
                _ => "이미 보유 중인 스킬"
            };

            _conditionObject.SetActive(false);
        }
        else
        {
            _tooltipsText.text = "아래 조건 달성 시 해금";
            _conditionObject.SetActive(true);
            _conditionText.text = GetConditionText(_entry);
        }
    }

    private string GetConditionText(ShopItemEntry entry)
    {
        switch (entry.conditionType)
        {
            case ShopConditionType.WeaponEnhanceLevel:
                string weaponName = entry.conditionWeapon != null ? entry.conditionWeapon.WeaponName : "무기";
                return $"+{entry.conditionValue} 강화 {weaponName} 보유";

            case ShopConditionType.RescueCount:
                return $"구조 횟수 {entry.conditionValue}회 이상";

            default:
                return string.Empty;
        }
    }
}