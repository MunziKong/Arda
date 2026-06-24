using System;
using System.Collections.Generic;
using UnityEngine;

public class RescueManager : MonoBehaviour
{
    [SerializeField] private GradeRescueConfig _gradeRescueConfig;
    [SerializeField] private RescueQuickSlotUI _rescue;
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private GameObject _rescueBoard;

    private List<RescuedMonsterEntry> _rescuedMonsters = new();

    public List<RescuedMonsterEntry> RescuedMonsters => _rescuedMonsters;

    public event Action OnRescuedMonstersChanged;

    public float GetRescueRatio(MonsterData monsterData)
    {
        float bonusRatio = _rescue.CurrentItem.SuccessBonusRate;
        float gradeRatio = _gradeRescueConfig.GetProbability(monsterData.grade);

        return bonusRatio + gradeRatio >= 1.0f ? 1.0f : bonusRatio + gradeRatio;
    }

    public void ClearList()
    {
        _rescuedMonsters.Clear();
        OnRescuedMonstersChanged?.Invoke();
        _rescueBoard.SetActive(false);
    }

    public void OpenBoard()
    {
        _rescueBoard.SetActive(true);
    }

    public void AddItemByDroptable(MonsterData monsterData)
    {
        if (monsterData == null)
        {
            return;
        }

        foreach (DropEntry dropEntry in monsterData.dropTable)
        {
            if (dropEntry.item == null)
            {
                continue;
            }

            float randomValue = UnityEngine.Random.value;

            if (randomValue > dropEntry.dropChance)
            {
                continue;
            }

            int quantity = UnityEngine.Random.Range(dropEntry.minQuantity, dropEntry.maxQuantity + 1);

            _inventory.AddItem(dropEntry.item, quantity);

            EnqueueItemMessage(dropEntry.item, quantity);

            Debug.Log($"[RescueManager] {dropEntry.item.ItemName} x{quantity} 획득 ");
        }
    }
    private void EnqueueItemMessage(ItemData item, int quantity)
    {
        if (item is CurrencyItemData)
        {
            GameCore.MessageUIController.Enqueue(new MessageData
            {
                Type = PopupMessageType.Gold,
                Quantity = quantity
            });
        }
        else
        {
            GameCore.MessageUIController.Enqueue(new MessageData
            {
                Type = PopupMessageType.Item,
                Icon = item.Icon,
                Name = item.ItemName,
                Quantity = quantity
            });
        }

    }

    public bool UseCurrentRescueItem()
    {
        Debug.Log($"[RescueManager] 추가 성공 확률 : {_rescue.CurrentItem.SuccessBonusRate}");
        return _rescue.UseCurrentRescueItem();
    }

    public void AddRescuedMonster(MonsterData monsterData)
    {
        if (monsterData == null)
        {
            return;
        }

        RescuedMonsterEntry existingEntry = FindEntry(monsterData);

        if (existingEntry != null)
        {
            existingEntry.AddAmount(1);
        }
        else
        {
            _rescuedMonsters.Add(new RescuedMonsterEntry(monsterData, 1));
        }

        GameCore.PlayerOwnedData.IncrementRescueCount(monsterData);

        GameCore.MessageUIController.Enqueue(new MessageData
        {
            Type = PopupMessageType.SuccessRescue,
            Icon = monsterData.icon,
            Name = monsterData.monsterName,
            Quantity = 1
        });



        AddItemByDroptable(monsterData);

        Debug.Log($"[RescueManager] 구조 몬스터 추가 : {monsterData.name}");

        QuestManager.Instance?.ReportRescue(monsterData);

        OnRescuedMonstersChanged?.Invoke();
    }

    public void FailRescuedMonster(MonsterData monsterData)
    {
        if (monsterData == null)
        {
            return;
        }

        GameCore.MessageUIController.Enqueue(new MessageData
        {
            Type = PopupMessageType.FailRescue,
            Icon = monsterData.icon,
            Name = monsterData.monsterName,
            Quantity = 1
        });

        Debug.Log($"[RescueManager] 구조 실패 : {monsterData.name}");
    }

    private RescuedMonsterEntry FindEntry(MonsterData monsterData)
    {
        for (int i = 0; i < _rescuedMonsters.Count; i++)
        {
            if (_rescuedMonsters[i].MonsterData == monsterData)
            {
                return _rescuedMonsters[i];
            }
        }

        return null;
    }
}