using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Arda/Attack/AttackData")]
public class AttackData : ScriptableObject
{
    [Header("Base")]
    [SerializeField] private string _attackId;
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private string _attackName;
    [SerializeField] private Sprite _attackIcon;
    [SerializeField] private int _animationIndex;

    [Header("Attack Steps")]
    [SerializeField] private List<AttackStepData> _attackSteps = new();


    public string AttackId => _attackId;
    public WeaponType WeaponType => _weaponType;
    public string AttackName => _attackName;
    public Sprite AttackIcon => _attackIcon;
    public int AnimationIndex => _animationIndex;
    public List<AttackStepData> AttackSteps => _attackSteps;

    public AttackStepData GetStep(int index)
    {
        if (index < 0 || index >= _attackSteps.Count)
        {
            return null;
        }

        return _attackSteps[index];
    }
}