using UnityEngine;

[CreateAssetMenu(fileName = "NewRescueItem", menuName = "Arda/Item/RescueItem")]
public class RescueItemData : ItemData
{
    [Header("Rescue")]
    [Range(0f, 1f)]
    [SerializeField] private float _successBonusRate;
    [SerializeField] private bool _isDefaultKit;

    public float SuccessBonusRate => _successBonusRate;
    public bool IsDefaultKit => _isDefaultKit;

}