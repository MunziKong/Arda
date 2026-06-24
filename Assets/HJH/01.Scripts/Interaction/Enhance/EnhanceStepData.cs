using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnhanceStepData
{
    [Header("Level")]
    public int level;
    [Header("Success Rate")]
    [Range(0f, 1f)]
    public float successRate;

    [Header("Reward")]
    public int bonusDamage;

    [Header("Cost")]
    public List<ItemRequirement> requiredItems = new();

    [Header("Pity")]
    public int pityMaxStack;
}