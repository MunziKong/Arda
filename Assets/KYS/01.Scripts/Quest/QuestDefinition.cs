using UnityEngine;

[System.Serializable]
public class QuestRewardEntry
{
    public ItemData item;
    public int quantity = 1;
}

public enum QuestCompletionMode
{
    Immediate,   // 조건 달성 즉시 완료
    ReturnToNPC  // 창조신에게 돌아와야 완료 대사 출력
}

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "Arda/Quest/QuestDefinition")]
public class QuestDefinition : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;
    public string questName;

    [Header("수락 대사 (한 문장씩 순서대로)")]
    [TextArea(2, 4)]
    public string[] startDialogues;

    [Header("진행 중 창조신 상호작용 대사")]
    [TextArea(2, 4)]
    public string[] inProgressDialogues;

    [Header("완료 조건")]
    public QuestCondition[] conditions;

    [Header("완료 처리")]
    public QuestCompletionMode completionMode;

    [Header("완료 대사 (한 문장씩 순서대로)")]
    [TextArea(2, 4)]
    public string[] completeDialogues;

    [Header("수락 방식")]
    public bool acceptAtPortalOnly;
    public bool suppressNamePopup;

    [Header("퀘스트 보상 (클리어 시 인벤토리에 지급)")]
    public QuestRewardEntry[] rewards;

    [Header("다음 퀘스트 (없으면 비워두기)")]
    public QuestDefinition nextQuest;
}
