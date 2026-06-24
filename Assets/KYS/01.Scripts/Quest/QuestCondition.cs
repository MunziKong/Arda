using UnityEngine;

public abstract class QuestCondition : ScriptableObject
{
    [TextArea]
    public string description;

    public abstract bool IsCompleted(QuestRuntimeData data);

    public virtual string GetDisplayText(QuestRuntimeData data)
    {
        return description;
    }
}
