using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutUIController : MonoBehaviour
{

    [SerializeField] private GameObject _loadoutUIPanel;

    [Header("Loadout")]
    [SerializeField] private LoadoutSlotUI _slotPrefab;


    [Header("Contents")]
    [SerializeField] private Transform _weaponContent;
    [SerializeField] private Transform _attackContent;
    [SerializeField] private Transform _skillContent;
    [SerializeField] private Transform _skinContent;

    [Header("Summary")]
    [SerializeField] private LoadoutSummaryUI _summaryPrefab;
    [SerializeField] private Transform _summaryContent;

    [SerializeField] private Button _closeBtn;

    [Header("Scroll Rect")]
    [SerializeField] private ScrollRect _skinScrollRect;

    private WeaponData _selectedWeapon;
    private AttackData _selectedAttack;
    private readonly List<SkillDefinition> _selectedSkills = new();
    private GameObject _selectedSkin;

    private void OnEnable()
    {
        _closeBtn.onClick.AddListener(() => { SoundManager.Instance?.PlayButtonClick(); ClosePanel(); });
        GameCore.PlayerInputs.CloseEvent += ClosePanel;
    }

    private void OnDisable()
    {
        GameCore.PlayerInputs.CloseEvent -= ClosePanel;
        _closeBtn.onClick.RemoveAllListeners();
    }

    public void OpenPanel()
    {
        GetSelectionFromCurrentLoadout();

        RefreshWeaponList();
        RefreshAttackList();
        RefreshSkillList();
        RefreshSkinList();
        ResetScrollToTop(_skinScrollRect);
        RefreshSummary();

        GameCore.GameController.InactiveCoreUI();
        GameCore.PlayerInputs.SetCursorLock(false);
        GameCore.PlayerInputs.SetUIInput();
        Time.timeScale = 0f;
        _loadoutUIPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (!_loadoutUIPanel.activeSelf)
        {
            return;
        }

        if (!HasValidSelection())
        {
            Debug.Log("프리셋 조건이 충족되지 않았습니다.");
            return;
        }

        GameCore.PlayerInputs.SetPlayerInput();
        GameCore.GameController.ActiveCoreUI();
        GameCore.PlayerInputs.SetCursorLock(true);
        Time.timeScale = 1f;
        _loadoutUIPanel.SetActive(false);
        GameCore.AlertManager.Enqueue(AlertType.SaveLoadout);
    }

    private void RefreshWeaponList()
    {
        ClearContent(_weaponContent);
        ClearContent(_attackContent);
        ClearContent(_skillContent);
        ClearContent(_skinContent);

        List<WeaponData> weapons = GameCore.LoadoutManager.GetSelectableWeapons();

        foreach (WeaponData weapon in weapons)
        {
            LoadoutSlotUI slot = Instantiate(_slotPrefab, _weaponContent);

            WeaponEnhanceState enhanceState = GameCore.LoadoutManager.GetWeaponEnhanceState(weapon);
            int level = enhanceState != null ? enhanceState.CurrentLevel : 0;

            slot.Init(weapon.WeaponIcon, weapon.WeaponName, () => SelectWeapon(weapon), level);
            slot.SetSelected(_selectedWeapon == weapon);
        }
    }

    private void RefreshAttackList()
    {
        ClearContent(_attackContent);

        List<AttackData> attacks = GameCore.LoadoutManager.GetSelectableAttacks(_selectedWeapon);

        foreach (AttackData attack in attacks)
        {
            LoadoutSlotUI slot = Instantiate(_slotPrefab, _attackContent);

            slot.Init(attack.AttackIcon, attack.AttackName, () => SelectAttack(attack));
            slot.SetSelected(_selectedAttack == attack);
        }
    }

    private void RefreshSkillList()
    {
        ClearContent(_skillContent);

        List<SkillDefinition> skills = GameCore.LoadoutManager.GetSelectableSkills(_selectedWeapon);

        foreach (SkillDefinition skill in skills)
        {
            LoadoutSlotUI slot = Instantiate(_slotPrefab, _skillContent);

            slot.Init(skill.skillIcon, skill.skillName, () => SelectSkill(skill));
            slot.SetSelected(_selectedSkills.Contains(skill));
        }
    }

    private void RefreshSkinList()
    {
        ClearContent(_skinContent);

        List<GameObject> skins = GameCore.LoadoutManager.GetSelectableSkins(_selectedWeapon);

        foreach (GameObject skin in skins)
        {
            WeaponInfo info = skin.GetComponent<WeaponInfo>();

            if (info == null)
                continue;

            LoadoutSlotUI slot = Instantiate(_slotPrefab, _skinContent);

            slot.Init(info.SkinIcon, info.SkinName, () => SelectSkin(skin));
            slot.SetSelected(_selectedSkin == skin);
        }
    }

    private void SelectWeapon(WeaponData weapon)
    {
        _selectedWeapon = weapon;
        _selectedSkills.Clear();
        _selectedSkin = weapon != null ? weapon.WeaponPrefab : null;

        List<AttackData> attacks = GameCore.LoadoutManager.GetSelectableAttacks(weapon);
        _selectedAttack = attacks.Count > 0 ? attacks[0] : null;

        RefreshWeaponList();
        RefreshAttackList();
        RefreshSkillList();
        RefreshSkinList();
        ResetScrollToTop(_skinScrollRect);
        RefreshSummary();

        TryUpdateLoadout();
    }
    private void SelectAttack(AttackData attack)
    {
        _selectedAttack = attack;

        RefreshAttackList();
        RefreshSummary();

        TryUpdateLoadout();
    }

    private void SelectSkill(SkillDefinition skill)
    {
        if (_selectedSkills.Contains(skill))
        {
            _selectedSkills.Remove(skill);
        }
        else
        {
            if (_selectedSkills.Count >= 3)
            {
                Debug.Log("스킬은 최대 3개까지만 선택할 수 있습니다.");
                return;
            }

            _selectedSkills.Add(skill);
        }

        RefreshSkillList();
        RefreshSummary();

        TryUpdateLoadout();
    }

    private void SelectSkin(GameObject skin)
    {
        _selectedSkin = skin;

        RefreshSkinList();
        RefreshSummary();

        TryUpdateLoadout();
    }

    private void TryUpdateLoadout()
    {
        if (_selectedWeapon == null)
        {
            return;
        }

        if (_selectedAttack == null)
        {
            return;
        }

        GameCore.LoadoutManager.SetLoadout(_selectedWeapon, _selectedAttack, _selectedSkills, _selectedSkin);
    }

    private void ClearContent(Transform content)
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void RefreshSummary()
    {
        ClearContent(_summaryContent);

        if (_selectedWeapon != null)
        {
            LoadoutSummaryUI summary = Instantiate(_summaryPrefab, _summaryContent);
            summary.Init(
                _selectedWeapon.WeaponIcon,
                _selectedWeapon.WeaponName,
                "무기"
            );
        }

        if (_selectedSkin != null)
        {
            WeaponInfo info = _selectedSkin.GetComponent<WeaponInfo>();

            if (info != null)
            {
                LoadoutSummaryUI summary = Instantiate(_summaryPrefab, _summaryContent);
                summary.Init(
                    info.SkinIcon,
                    info.SkinName,
                    "스킨"
                );
            }
        }

        if (_selectedAttack != null)
        {
            LoadoutSummaryUI summary = Instantiate(_summaryPrefab, _summaryContent);
            summary.Init(
                _selectedAttack.AttackIcon,
                _selectedAttack.AttackName,
                "기본 공격"
            );
        }

        foreach (SkillDefinition skill in _selectedSkills)
        {
            LoadoutSummaryUI summary = Instantiate(_summaryPrefab, _summaryContent);
            summary.Init(
                skill.skillIcon,
                skill.skillName,
                "스킬"
            );
        }
    }

    private bool HasValidSelection()
    {
        return _selectedWeapon != null
            && _selectedAttack != null
            && _selectedSkills.Count <= 3;
    }

    private void GetSelectionFromCurrentLoadout()
    {
        _selectedWeapon = null;
        _selectedAttack = null;
        _selectedSkills.Clear();
        _selectedSkin = null;

        if (!GameCore.LoadoutManager.HasValidLoadout())
        {
            return;
        }

        LoadoutData currentLoadout = GameCore.LoadoutManager.CurrentLoadout;

        _selectedWeapon = currentLoadout.Weapon;
        _selectedAttack = currentLoadout.BasicAttack;
        _selectedSkin = currentLoadout.Skin;

        if (currentLoadout.Skills != null)
        {
            _selectedSkills.AddRange(currentLoadout.Skills);
        }
    }

    private void ResetScrollToTop(ScrollRect scrollRect)
    {
        if (scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        scrollRect.verticalNormalizedPosition = 1f;
    }
}