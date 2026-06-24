using UnityEngine;

public class EnhanceManager : MonoBehaviour
{
    [Header("Table")]
    [SerializeField] private WeaponEnhanceTable _enhanceTable;

    [Header("Ref")]
    [SerializeField] private PlayerOwnedData _playerOwnedData;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private PlayerEquipment _playerEquipment;

    public EnhanceStepData GetNextStep(WeaponData weapon)
    {
        WeaponEnhanceState state = _playerOwnedData.GetWeaponEnhanceState(weapon);

        if (state == null)
        {
            return null;
        }

        return _enhanceTable.GetStep(state.CurrentLevel + 1);
    }

    public float GetCurrentSuccessRate(WeaponData weapon)
    {
        WeaponEnhanceState state = _playerOwnedData.GetWeaponEnhanceState(weapon);
        EnhanceStepData step = GetNextStep(weapon);

        if (state == null || step == null)
        {
            return 0f;
        }

        if (step.pityMaxStack > 0 && state.PityStack >= step.pityMaxStack)
        {
            return 1f;
        }

        return step.successRate;
    }

    public bool HasEnoughResources(WeaponData weapon)
    {
        EnhanceStepData step = GetNextStep(weapon);

        if (step == null)
        {
            return false;
        }

        foreach (ItemRequirement req in step.requiredItems)
        {
            if (!_playerInventory.HasItem(req.item, req.amount))
            {
                return false;
            }
        }

        return true;
    }

    public EnhanceResult TryEnhance(WeaponData weapon)
    {
        WeaponEnhanceState state = _playerOwnedData.GetWeaponEnhanceState(weapon);

        if (state == null)
        {
            return new EnhanceResult { ResultType = EnhanceResultType.NotEnoughResources };
        }

        EnhanceStepData step = _enhanceTable.GetStep(state.CurrentLevel + 1);

        if (step == null)
        {
            return new EnhanceResult { ResultType = EnhanceResultType.MaxLevelReached, NewLevel = state.CurrentLevel };
        }

        foreach (ItemRequirement req in step.requiredItems)
        {
            if (!_playerInventory.HasItem(req.item, req.amount))
            {
                return new EnhanceResult { ResultType = EnhanceResultType.NotEnoughResources, NewLevel = state.CurrentLevel };
            }
        }

        foreach (ItemRequirement req in step.requiredItems)
        {
            _playerInventory.RemoveItem(req.item, req.amount);
        }

        bool isPityGuaranteed = step.pityMaxStack > 0 && state.PityStack >= step.pityMaxStack;
        float successRate = isPityGuaranteed ? 1f : step.successRate;

        bool success = Random.value < successRate;

        if (success)
        {
            state.CurrentLevel++;
            state.TotalBonusDamage += step.bonusDamage;
            state.PityStack = 0;

            _playerEquipment.RefreshEquippedWeaponPower();
            QuestManager.Instance?.ReportEnhance();
            SaveManager.Instance?.Save();

            return new EnhanceResult
            {
                ResultType = EnhanceResultType.Success,
                NewLevel = state.CurrentLevel,
                PityGuaranteed = isPityGuaranteed
            };
        }
        else
        {
            state.PityStack++;
            SaveManager.Instance?.Save();

            return new EnhanceResult
            {
                ResultType = EnhanceResultType.Fail,
                NewLevel = state.CurrentLevel,
                PityGuaranteed = isPityGuaranteed
            };
        }
    }
}