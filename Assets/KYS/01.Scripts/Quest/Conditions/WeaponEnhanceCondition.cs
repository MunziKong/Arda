using UnityEngine;

[CreateAssetMenu(fileName = "WeaponEnhanceCondition", menuName = "Arda/Quest/Condition/WeaponEnhance")]
public class WeaponEnhanceCondition : QuestCondition
{
    [Tooltip("필요한 강화 단계")]
    public int requiredLevel = 1;

    public override bool IsCompleted(QuestRuntimeData data)
    {
        foreach (WeaponEnhanceState state in GameCore.PlayerOwnedData.OwnedWeapons)
        {
            if (state.CurrentLevel >= requiredLevel)
            {
                return true;
            }
        }

        return false;
    }

    public override string GetDisplayText(QuestRuntimeData data)
    {
        int highest = 0;

        foreach (WeaponEnhanceState state in GameCore.PlayerOwnedData.OwnedWeapons)
        {
            if (state.CurrentLevel > highest)
            {
                highest = state.CurrentLevel;
            }
        }

        int display = Mathf.Min(highest, requiredLevel);
        return $"{description} {display}/{requiredLevel}";
    }
}
