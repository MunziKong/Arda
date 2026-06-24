using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestDefinition ActiveQuest { get; private set; }
    public QuestDefinition PendingQuest { get; private set; }
    public bool IsQuestCompleted { get; private set; }

    public event Action<QuestDefinition> OnQuestStarted;
    public event Action<QuestDefinition> OnQuestCompleted;
    public event Action<QuestDefinition> OnQuestUpdated;

    private QuestRuntimeData _runtimeData;

    public QuestRuntimeData RuntimeData => _runtimeData;

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
    }

    public void StartQuest(QuestDefinition quest)
    {
        if (quest == null)
        {
            return;
        }

        ActiveQuest = quest;
        _runtimeData = new QuestRuntimeData { Quest = quest };
        IsQuestCompleted = false;

        if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
        {
            SaveManager.Instance.Data.lastStartedQuestId = quest.questId;
        }

        OnQuestStarted?.Invoke(quest);
        SaveManager.Instance?.Save();
    }

    public void ReportInteract(string objectId)
    {
        if (ActiveQuest == null || IsQuestCompleted)
        {
            return;
        }

        _runtimeData.interactedObjectIds.Add(objectId);
        CheckCompletion();
    }

    public void ReportKill(EnemyData enemyData)
    {
        if (ActiveQuest == null || IsQuestCompleted)
        {
            return;
        }

        _runtimeData.AddKill(enemyData);
        OnQuestUpdated?.Invoke(ActiveQuest);
        CheckCompletion();
        SaveManager.Instance?.Save();
    }

    public void ReportEnhance()
    {
        if (ActiveQuest == null || IsQuestCompleted)
        {
            return;
        }

        OnQuestUpdated?.Invoke(ActiveQuest);
        CheckCompletion();
    }

    public void ReportRescue(MonsterData monsterData)
    {
        if (ActiveQuest == null || IsQuestCompleted)
        {
            return;
        }

        _runtimeData.AddRescue(monsterData);
        OnQuestUpdated?.Invoke(ActiveQuest);
        CheckCompletion();
        SaveManager.Instance?.Save();
    }

    public void CheckCompletion()
    {
        if (ActiveQuest == null || IsQuestCompleted)
        {
            return;
        }

        foreach (var condition in ActiveQuest.conditions)
        {
            if (!condition.IsCompleted(_runtimeData))
            {
                return;
            }
        }

        IsQuestCompleted = true;
        OnQuestUpdated?.Invoke(ActiveQuest);
    }

    public void CompleteQuest()
    {
        if (ActiveQuest == null)
        {
            return;
        }

        GiveRewards(ActiveQuest);
        GameCore.AlertManager?.Enqueue(AlertType.QuestComplete, ActiveQuest.questName);
        OnQuestCompleted?.Invoke(ActiveQuest);

        QuestDefinition next = ActiveQuest.nextQuest;
        ActiveQuest = null;
        IsQuestCompleted = false;

        if (next != null)
        {
            PendingQuest = next;
        }

        SaveManager.Instance?.Save();
    }

    private void GiveRewards(QuestDefinition quest)
    {
        if (quest.rewards == null || quest.rewards.Length == 0)
        {
            return;
        }

        PlayerInventory inventory = GameCore.PlayerInventory;

        if (inventory == null)
        {
            return;
        }

        foreach (QuestRewardEntry entry in quest.rewards)
        {
            if (entry.item == null || entry.quantity <= 0)
            {
                continue;
            }

            inventory.AddItem(entry.item, entry.quantity);

            GameCore.MessageUIController?.Enqueue(new MessageData
            {
                Type = PopupMessageType.Item,
                Icon = entry.item.Icon,
                Name = entry.item.ItemName,
                Quantity = entry.quantity
            });
        }
    }

    public void RestoreQuest(QuestDefinition quest)
    {
        if (quest == null)
        {
            return;
        }

        ActiveQuest = quest;
        _runtimeData = new QuestRuntimeData { Quest = quest };
        IsQuestCompleted = false;
    }

    public void SetPendingQuest(QuestDefinition quest)
    {
        PendingQuest = quest;
    }

    public void ResetState()
    {
        ActiveQuest = null;
        PendingQuest = null;
        IsQuestCompleted = false;
        _runtimeData = null;
    }

    public void DebugForceComplete()
    {
        if (ActiveQuest == null || IsQuestCompleted)
        {
            return;
        }

        IsQuestCompleted = true;
        OnQuestUpdated?.Invoke(ActiveQuest);
    }

    public void AcceptPendingQuest()
    {
        if (PendingQuest == null)
        {
            return;
        }

        QuestDefinition quest = PendingQuest;
        PendingQuest = null;
        StartQuest(quest);
    }
}
