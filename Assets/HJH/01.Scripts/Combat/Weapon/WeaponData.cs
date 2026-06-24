using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Arda/Equipments/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string _weaponName;
    [SerializeField] private string _weaponDescription;
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private Sprite _weaponIcon;
    [SerializeField] private GameObject _weaponPrefab;

    [Header("Combat")]
    [SerializeField] private int _baseDamage = 10;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackCooldown = 0.5f;

    public string WeaponName => _weaponName;
    public WeaponType WeaponType => _weaponType;
    public Sprite WeaponIcon => _weaponIcon;
    public GameObject WeaponPrefab => _weaponPrefab;

    public int BaseDamage => _baseDamage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
}