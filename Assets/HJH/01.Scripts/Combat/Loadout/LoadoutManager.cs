using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private PlayerOwnedData _playerOwnedData;

    [Header("Default Loadout")]
    [SerializeField] private WeaponData _defaultWeapon;
    [SerializeField] private AttackData _defaultAttack;
    [SerializeField] private List<SkillDefinition> _defaultSkills = new();


    private LoadoutData _currentLoadout = new();

    public LoadoutData CurrentLoadout => _currentLoadout;

    // 테스트용
    private void Start()
    {
        if (!HasValidLoadout())
        {
            WeaponEnhanceState defaultState = _playerOwnedData.GetWeaponEnhanceState(_defaultWeapon);
            WeaponData weapon = defaultState != null ? defaultState.WeaponData : _defaultWeapon;

            SetLoadout(weapon, _defaultAttack, _defaultSkills, weapon != null ? weapon.WeaponPrefab : null);
        }
    }

    public List<WeaponData> GetSelectableWeapons()
    {
        return _playerOwnedData.OwnedWeapons
            .Select(s => s.WeaponData)
            .ToList();
    }

    public WeaponEnhanceState GetWeaponEnhanceState(WeaponData weapon)
    {
        return _playerOwnedData.GetWeaponEnhanceState(weapon);
    }

    public List<AttackData> GetSelectableAttacks(WeaponData selectedWeapon)
    {
        if (selectedWeapon == null)
        {
            return new List<AttackData>();
        }

        return _playerOwnedData.OwnedAttacks
            .Where(attack => attack.WeaponType == selectedWeapon.WeaponType)
            .ToList();
    }

    public List<SkillDefinition> GetSelectableSkills(WeaponData selectedWeapon)
    {
        if (selectedWeapon == null)
        {
            return new List<SkillDefinition>();
        }

        return _playerOwnedData.OwnedSkills
            .Where(skill => skill.weaponType == selectedWeapon.WeaponType)
            .ToList();
    }

    public List<GameObject> GetSelectableSkins(WeaponData selectedWeapon)
    {
        if (selectedWeapon == null)
        {
            return new List<GameObject>();
        }

        return _playerOwnedData.OwnedSkins
            .Where(skin =>
            {
                WeaponInfo info = skin.GetComponent<WeaponInfo>();
                return info != null && info.WeaponType == selectedWeapon.WeaponType;
            })
            .ToList();
    }

    public void SetLoadout(WeaponData weapon, AttackData basicAttack, List<SkillDefinition> skills, GameObject skin = null)
    {
        if (weapon == null)
        {
            Debug.Log("무기 null");
            return;
        }

        if (basicAttack == null)
        {
            Debug.Log("기본공격 null");
            return;
        }

        if (skills == null)
        {
            skills = new List<SkillDefinition>();
        }

        if (skills.Count > 3)
        {
            Debug.Log("스킬 최대 갯수 초과");
            return;
        }

        _currentLoadout = new LoadoutData
        {
            Weapon = weapon,
            Skin = skin,
            BasicAttack = basicAttack,
            Skills = new List<SkillDefinition>(skills)
        };

        GameCore.PlayerEquipment.EquipWeapon(_currentLoadout.Weapon, _currentLoadout.Skin);
        GameCore.PlayerAttack.SetAttackData(_currentLoadout.BasicAttack);
        GameCore.PlayerSkillController.SetSkills(_currentLoadout.Skills);
        GameCore.SkillSetUIController.SetSkills(_currentLoadout.Skills);

        Debug.Log($"프리셋 : {weapon.WeaponName} / {basicAttack.name} / 스킬 개수: {skills.Count} / 스킨: {(skin != null ? skin.name : "기본")}");

        SaveManager.Instance?.Save();
    }

    public bool HasValidLoadout()
    {
        return _currentLoadout != null && _currentLoadout.IsValid();
    }

    public void ResetToDefault()
    {
        SetLoadout(_defaultWeapon, _defaultAttack, _defaultSkills, null);
    }

    public void ClearLoadout()
    {
        _currentLoadout = new LoadoutData();
        GameCore.PlayerEquipment?.UnequipWeapon();
    }
}