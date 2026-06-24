using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUIController : MonoBehaviour
{
    [Header("Content Root")]
    [SerializeField] private GameObject _contentRoot;

    [Header("Weapon List")]
    [SerializeField] private LoadoutSlotUI _slotPrefab;
    [SerializeField] private Transform _weaponContent;

    [Header("Info Panel")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _weaponNameText;
    [SerializeField] private TMP_Text _weaponLevelText;
    [SerializeField] private TMP_Text _totalDamageText;
    [SerializeField] private TMP_Text _pityText;
    [SerializeField] private TMP_Text _successRateText;
    [SerializeField] private TMP_Text _nextAtkText;

    [Header("Materials")]
    [SerializeField] private EnhanceMaterialSlotUI _materialSlotPrefab;
    [SerializeField] private Transform _materialContent;

    [Header("Gold")]
    [SerializeField] private TMP_Text _goldText;

    [Header("Enhance Button")]
    [SerializeField] private Button _enhanceButton;

    [Header("Result Overlay")]
    [SerializeField] private GameObject _overlayRoot;
    [SerializeField] private EnhanceResultOverlayUI _resultOverlay;

    private WeaponData _selectedWeapon;

    private void Awake()
    {
        _enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);
    }

    public void OpenPanel()
    {
        _contentRoot.SetActive(true);
        _overlayRoot.SetActive(false);
        _resultOverlay.Reset();

        _selectedWeapon = GameCore.PlayerEquipment.CurrentWeapon;

        RefreshWeaponList();
        RefreshInfoPanel();
        RefreshGoldText();
    }

    private void RefreshWeaponList()
    {
        ClearContent(_weaponContent);

        List<WeaponData> weapons = GameCore.LoadoutManager.GetSelectableWeapons();

        foreach (WeaponData weapon in weapons)
        {
            LoadoutSlotUI slot = Instantiate(_slotPrefab, _weaponContent);

            WeaponEnhanceState state = GameCore.LoadoutManager.GetWeaponEnhanceState(weapon);
            int level = state != null ? state.CurrentLevel : 0;

            slot.Init(weapon.WeaponIcon, weapon.WeaponName, () => SelectWeapon(weapon), level);
            slot.SetSelected(_selectedWeapon == weapon);
        }
    }

    private void SelectWeapon(WeaponData weapon)
    {
        _selectedWeapon = weapon;

        RefreshWeaponList();
        RefreshInfoPanel();
    }

    private void RefreshInfoPanel()
    {
        if (_selectedWeapon == null)
        {
            return;
        }

        WeaponEnhanceState state = GameCore.LoadoutManager.GetWeaponEnhanceState(_selectedWeapon);
        int currentLevel = state != null ? state.CurrentLevel : 0;
        int pityStack = state != null ? state.PityStack : 0;
        int totalBonusDamage = state != null ? state.TotalBonusDamage : 0;

        _weaponIcon.sprite = _selectedWeapon.WeaponIcon;
        _weaponNameText.text = _selectedWeapon.WeaponName;
        _totalDamageText.text = $"+{_selectedWeapon.BaseDamage + totalBonusDamage}";

        EnhanceStepData nextStep = GameCore.EnhanceManager.GetNextStep(_selectedWeapon);

        if (nextStep == null)
        {
            _weaponLevelText.text = "MAX";

            _pityText.text = string.Empty;
            _successRateText.text = string.Empty;
            _nextAtkText.text = string.Empty;

            ClearContent(_materialContent);

            _enhanceButton.interactable = false;
            return;
        }

        _weaponLevelText.text = $"+{currentLevel}";

        bool isPityGuaranteed = nextStep.pityMaxStack > 0 && pityStack >= nextStep.pityMaxStack;

        _pityText.text = nextStep.pityMaxStack > 0
            ? $"{pityStack} / {nextStep.pityMaxStack}"
            : string.Empty;

        _successRateText.text = isPityGuaranteed
            ? "100%"
            : $"{(nextStep.successRate * 100f).ToString("F1")}%";

        _nextAtkText.text = $"+{nextStep.bonusDamage}";

        RefreshMaterialList(nextStep);

        bool hasEnoughResources = GameCore.EnhanceManager.HasEnoughResources(_selectedWeapon);
        _enhanceButton.interactable = hasEnoughResources;
    }

    private void RefreshMaterialList(EnhanceStepData step)
    {
        ClearContent(_materialContent);

        foreach (ItemRequirement requirement in step.requiredItems)
        {
            EnhanceMaterialSlotUI slot = Instantiate(_materialSlotPrefab, _materialContent);
            slot.Setup(requirement);
        }
    }

    private void RefreshGoldText()
    {
        if (_goldText == null)
        {
            return;
        }

        // _goldText.text = $"현재 보유 골드 : {GameCore.PlayerInventory.Gold:N0}";
        _goldText.text = $"{GameCore.PlayerInventory.Gold:N0}";
    }

    private void OnEnhanceButtonClicked()
    {
        SoundManager.Instance?.PlayButtonClick();
        if (_selectedWeapon == null)
        {
            return;
        }

        WeaponEnhanceState stateBefore = GameCore.LoadoutManager.GetWeaponEnhanceState(_selectedWeapon);
        int currentLevel = stateBefore != null ? stateBefore.CurrentLevel : 0;
        int challengeLevel = currentLevel + 1;

        EnhanceResult result = GameCore.EnhanceManager.TryEnhance(_selectedWeapon);

        Debug.Log($"[Enhance] {_selectedWeapon.WeaponName} - 결과: {result.ResultType} / 레벨: {result.NewLevel} / 천장 확정: {result.PityGuaranteed}");

        bool isSuccess = result.ResultType == EnhanceResultType.Success;

        _contentRoot.SetActive(false);
        _overlayRoot.SetActive(true);

        _resultOverlay.Play(_selectedWeapon.WeaponIcon, isSuccess, challengeLevel, OnResultOverlayComplete);
    }

    private void OnResultOverlayComplete()
    {
        _overlayRoot.SetActive(false);
        _contentRoot.SetActive(true);

        RefreshWeaponList();
        RefreshInfoPanel();
        RefreshGoldText();
    }

    public bool IsResultOverlayOpen => _overlayRoot.activeSelf;

    public void CloseResultOverlay()
    {
        _resultOverlay.Reset();
        _overlayRoot.SetActive(false);
        _contentRoot.SetActive(true);

        RefreshWeaponList();
        RefreshInfoPanel();
        RefreshGoldText();
    }

    private void ClearContent(Transform content)
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}