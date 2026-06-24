using UnityEngine;

[CreateAssetMenu(fileName = "InteractObjectCondition", menuName = "Arda/Quest/Condition/InteractObject")]
public class InteractObjectCondition : QuestCondition
{
    [Tooltip("상호작용해야 하는 오브젝트 ID")]
    public string targetObjectId;

    public override bool IsCompleted(QuestRuntimeData data)
    {
        return data.interactedObjectIds.Contains(targetObjectId);
    }
}
