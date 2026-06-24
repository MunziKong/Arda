using UnityEngine;

[CreateAssetMenu(fileName = "RescueMonsterCondition", menuName = "Arda/Quest/Condition/RescueMonster")]
public class RescueMonsterCondition : QuestCondition
{
    [Tooltip("구조해야 하는 몬스터")]
    public MonsterData targetMonster;

    [Tooltip("필요한 구조 마릿수")]
    public int requiredCount = 1;

    public override bool IsCompleted(QuestRuntimeData data)
    {
        if (targetMonster == null)
        {
            return false;
        }

        return data.GetRescueCount(targetMonster) >= requiredCount;
    }

    public override string GetDisplayText(QuestRuntimeData data)
    {
        int current = data != null ? data.GetRescueCount(targetMonster) : 0;
        int display = Mathf.Min(current, requiredCount);
        return $"{description} {display}/{requiredCount}";
    }
}
