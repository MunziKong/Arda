using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponData _startWeapon;
    [SerializeField] private Transform _weaponSocket;

    [Header("Ref")]
    [SerializeField] private PlayerOwnedData _playerOwnedData;

    [Header("Tooltips UI")]
    [SerializeField] private GameObject _tooltipsRmb;

    private WeaponData _currentWeapon;
    private GameObject _currentWeaponObject;
    private PlayerStats _stats;
    private GameObject _currentSkinPrefab;
    public WeaponData CurrentWeapon => _currentWeapon;
    public GameObject CurrentWeaponObject => _currentWeaponObject;

    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        if (_startWeapon != null)
        {
            EquipWeapon(_startWeapon);
        }
    }

    public void EquipWeapon(WeaponData weaponData, GameObject skinPrefab = null)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("장착할 WeaponData가 없습니다.");
            return;
        }

        UnequipWeapon();
        _currentWeapon = weaponData;
        _currentSkinPrefab = skinPrefab;
        UpdatePowerByEquip(_currentWeapon);

        GameObject prefabToSpawn = skinPrefab != null ? skinPrefab : _currentWeapon.WeaponPrefab;

        if (prefabToSpawn != null && _weaponSocket != null)
        {
            _currentWeaponObject = Instantiate(prefabToSpawn, _weaponSocket);

            _currentWeaponObject.transform.localPosition = Vector3.zero;
            _currentWeaponObject.transform.localRotation = Quaternion.identity;
        }

        if (_currentWeapon.WeaponType == WeaponType.Staff)
        {
            _tooltipsRmb.SetActive(true);
        }
        else
        {
            _tooltipsRmb.SetActive(false);
        }
    }

    private void UpdatePowerByEquip(WeaponData weaponData)
    {
        int totalDamage = weaponData.BaseDamage + GetEnhanceBonusDamage(weaponData);

        switch (weaponData.WeaponType)
        {
            case WeaponType.Sword:
                _stats.SetWeaponMeleePower(totalDamage);
                break;
            case WeaponType.Staff:
                _stats.SetWeaponMagicPower(totalDamage);
                break;
        }
    }

    private int GetEnhanceBonusDamage(WeaponData weaponData)
    {
        if (_playerOwnedData == null)
        {
            return 0;
        }

        WeaponEnhanceState state = _playerOwnedData.GetWeaponEnhanceState(weaponData);

        return state != null ? state.TotalBonusDamage : 0;
    }

    public void RefreshEquippedWeaponPower()
    {
        if (_currentWeapon == null)
        {
            return;
        }

        UpdatePowerByEquip(_currentWeapon);
    }

    public void UnequipWeapon()
    {
        if (_currentWeaponObject != null)
        {
            Destroy(_currentWeaponObject);
            _currentWeaponObject = null;
        }

        if (_stats != null)
        {
            _stats.SetWeaponMeleePower(0);
            _stats.SetWeaponMagicPower(0);
        }

        _currentWeapon = null;
    }
}