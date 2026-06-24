using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponEnhanceTable", menuName = "Arda/GameConfig/WeaponEnhanceTable")]
public class WeaponEnhanceTable : ScriptableObject
{
    public List<EnhanceStepData> steps = new();

    public EnhanceStepData GetStep(int level)
    {
        foreach (EnhanceStepData step in steps)
        {
            if (step.level == level)
            {
                return step;
            }
        }

        return null;
    }

    public int MaxLevel
    {
        get
        {
            int max = 0;

            foreach (EnhanceStepData step in steps)
            {
                if (step.level > max)
                {
                    max = step.level;
                }
            }

            return max;
        }
    }
}