using System;
using UnityEngine;

[Serializable]
public class RescuedMonsterEntry
{
    [SerializeField] private MonsterData _monsterData;
    [SerializeField] private int _amount;

    public MonsterData MonsterData => _monsterData;
    public int Amount => _amount;

    public RescuedMonsterEntry(MonsterData monsterData, int amount)
    {
        _monsterData = monsterData;
        _amount = amount;
    }

    public void AddAmount(int amount)
    {
        _amount += amount;
    }
}