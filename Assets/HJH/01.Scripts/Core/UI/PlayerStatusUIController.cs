using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUIController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private PlayerEquipment _equipment;
    [SerializeField] private PlayerOwnedData _playerOwnedData;

    [Header("HP")]
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private Slider _hpSlider;

    [Header("Attack")]
    [SerializeField] private TMP_Text _attackText;

    [Header("Weapon")]
    [SerializeField] private TMP_Text _weaponNameText;
    [SerializeField] private TMP_Text _weaponLevelText;

    private void OnEnable()
    {
        _stats.ChangeHpEvent += OnHpChanged;
    }

    private void OnDisable()
    {
        _stats.ChangeHpEvent -= OnHpChanged;
    }

    private void Start()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        RefreshHP();
        RefreshAttack();
        RefreshWeapon();
    }

    private void OnHpChanged(int current, int max)
    {
        RefreshHP();
    }

    private void RefreshHP()
    {
        int current = _stats.CurrentHp;
        int max = _stats.MaxHp;

        _hpText.text = $"{current} / {max}";
        _hpSlider.value = max > 0 ? (float)current / max : 0f;
    }

    private void RefreshAttack()
    {
        int attack = Mathf.Max(_stats.MeleePower, _stats.MagicPower);
        _attackText.text = attack.ToString();
    }

    private void RefreshWeapon()
    {
        WeaponData weapon = _equipment.CurrentWeapon;

        if (weapon == null)
        {
            _weaponNameText.text = string.Empty;
            _weaponLevelText.text = string.Empty;
            return;
        }

        _weaponNameText.text = weapon.WeaponName;

        WeaponEnhanceState state = _playerOwnedData.GetWeaponEnhanceState(weapon);
        int level = state != null ? state.CurrentLevel : 0;

        _weaponLevelText.text = $"+{level}";
    }
}