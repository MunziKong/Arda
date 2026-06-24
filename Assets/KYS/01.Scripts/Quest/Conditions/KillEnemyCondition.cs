using UnityEngine;

[CreateAssetMenu(fileName = "KillEnemyCondition", menuName = "Arda/Quest/Condition/KillEnemy")]
public class KillEnemyCondition : QuestCondition
{
    [Tooltip("처치 대상 적")]
    public EnemyData targetEnemy;

    [Tooltip("필요 처치 수")]
    public int requiredCount = 1;

    public override bool IsCompleted(QuestRuntimeData data)
    {
        return data.GetKillCount(targetEnemy) >= requiredCount;
    }

    public override string GetDisplayText(QuestRuntimeData data)
    {
        int current = data != null ? Mathf.Min(data.GetKillCount(targetEnemy), requiredCount) : 0;
        return $"{description} {current}/{requiredCount}";
    }
}
