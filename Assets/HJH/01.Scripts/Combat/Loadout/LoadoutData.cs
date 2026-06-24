using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class LoadoutData
{
    public WeaponData Weapon;
    public GameObject Skin;
    public AttackData BasicAttack;
    public List<SkillDefinition> Skills = new();

    public bool IsValid()
    {
        return Weapon != null
            && BasicAttack != null
            && Skills != null
            && Skills.Count <= 3;
    }
}