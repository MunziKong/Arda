using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GradeBonus
{
    public MonsterGrade grade;
    [Range(0f, 1f)]
    public float successRateBonus;
    [Range(0f, 1f)]
    public float criticalRate;
}

[CreateAssetMenu(fileName = "CraftMonsterGradeConfig", menuName = "Arda/GameConfig/CraftMonsterGradeConfig")]
public class CraftMonsterGradeConfig : ScriptableObject
{

    public List<GradeBonus> gradeBonuses = new();

    public float GetSuccessBonus(MonsterGrade grade)
    {
        foreach (GradeBonus bonus in gradeBonuses)
        {
            if (bonus.grade == grade)
            {
                return bonus.successRateBonus;
            }
        }

        return 0f;
    }

    public float GetCriticalRate(MonsterGrade grade)
    {
        foreach (GradeBonus bonus in gradeBonuses)
        {
            if (bonus.grade == grade)
            {
                return bonus.criticalRate;
            }
        }

        return 0f;
    }
}