using UnityEngine;

[CreateAssetMenu(fileName = "GradeRescueConfig", menuName = "Arda/GameConfig/GradeRescueConfig")]
public class GradeRescueConfig : ScriptableObject
{
    // 등급별 구조 기본 확률 설정 (0 = 0%, 1 = 100%)
    [Header("Rescue Probability by Grade")]
    [Range(0f, 1f)]
    public float common = 0.8f;

    [Range(0f, 1f)]
    public float uncommon = 0.6f;

    [Range(0f, 1f)]
    public float rare = 0.4f;

    [Range(0f, 1f)]
    public float epic = 0.2f;

    [Range(0f, 1f)]
    public float legendary = 0.05f;

    // 등급을 넣으면 해당 확률 반환 (Unique는 확률 아닌 아이템 조건 — MonsterData.requiredItemIds 참조)
    public float GetProbability(MonsterGrade grade)
    {
        return grade switch
        {
            MonsterGrade.Common => common,
            MonsterGrade.Uncommon => uncommon,
            MonsterGrade.Rare => rare,
            MonsterGrade.Epic => epic,
            MonsterGrade.Legendary => legendary,
            _ => 0f
        };
    }
}
