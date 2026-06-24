using System;
using UnityEngine;

[Serializable]
public class SkillPrefabEntry
{
    public SkillPrefabType type;
    public GameObject prefab;
}

[Serializable]
public class SkillEffectEntry
{
    public SkillEffect effect;
    public float[] multipliers;
}