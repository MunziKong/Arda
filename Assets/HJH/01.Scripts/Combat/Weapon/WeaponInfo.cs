using UnityEngine;

public class WeaponInfo : MonoBehaviour
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private Sprite _skinIcon;
    [SerializeField] private string _skinName;

    public WeaponType WeaponType => _weaponType;
    public Sprite SkinIcon => _skinIcon;
    public string SkinName => _skinName;
}