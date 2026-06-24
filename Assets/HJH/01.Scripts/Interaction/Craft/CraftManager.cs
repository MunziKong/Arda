using System.Collections.Generic;
using UnityEngine;

public class CraftSummary
{
    public int TotalSuccess;
    public int TotalCritical;
    public int TotalFail;
    public int TotalAcquired;
}

public class CraftManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private CraftMonsterGradeConfig _gradeConfig;

    [Header("Ref")]
    [SerializeField] private PlayerOwnedData _playerOwnedData;
    [SerializeField] private PlayerInventory _playerInventory;

    public IReadOnlyList<GradeBonus> GradeBonuses => _gradeConfig.gradeBonuses;

    public bool HasEnoughMaterials(RecipeData recipe, int quantity = 1)
    {
        if (recipe == null)
        {
            return false;
        }

        foreach (ItemRequirement req in recipe.materials)
        {
            if (!_playerInventory.HasItem(req.item, req.amount * quantity))
            {
                return false;
            }
        }

        return true;
    }

    public List<CraftResult> TryCraft(RecipeData recipe, int quantity, MonsterGrade? selectedGrade = null)
    {
        List<CraftResult> results = new();

        if (recipe == null)
        {
            return results;
        }

        if (!_playerOwnedData.IsRecipeUnlocked(recipe))
        {
            results.Add(new CraftResult { ResultType = CraftResultType.RecipeNotUnlocked, ResultQuantity = 0 });
            return results;
        }

        if (!HasEnoughMaterials(recipe, quantity))
        {
            results.Add(new CraftResult { ResultType = CraftResultType.NotEnoughMaterials, ResultQuantity = 0 });
            return results;
        }

        // 재료 소모 (수량 × 재료)
        foreach (ItemRequirement req in recipe.materials)
        {
            _playerInventory.RemoveItem(req.item, req.amount * quantity);
        }

        // 사용 가능한 몬스터 수량 계산
        int availableMonsterCount = 0;

        if (selectedGrade.HasValue)
        {
            availableMonsterCount = GetMonsterCountByGrade(selectedGrade.Value);
        }

        // 회차별 제작 판정
        for (int i = 0; i < quantity; i++)
        {
            bool useMonster = selectedGrade.HasValue && availableMonsterCount > 0;

            float successRate = recipe.baseSuccessRate;
            float criticalRate = 0f;

            if (useMonster)
            {
                successRate += _gradeConfig.GetSuccessBonus(selectedGrade.Value);
                criticalRate = _gradeConfig.GetCriticalRate(selectedGrade.Value);

                ConsumeMonster(selectedGrade.Value);
                availableMonsterCount--;
            }

            successRate = Mathf.Clamp01(successRate);

            CraftResult result = Evaluate(successRate, criticalRate);
            results.Add(result);

            if (result.ResultQuantity > 0 && recipe.resultItem != null)
            {
                _playerInventory.AddItem(recipe.resultItem, result.ResultQuantity);
            }
        }

        return results;
    }

    private CraftResult Evaluate(float successRate, float criticalRate)
    {
        float roll = Random.value;

        if (roll >= successRate)
        {
            return new CraftResult { ResultType = CraftResultType.Fail, ResultQuantity = 0 };
        }

        float criticalRoll = Random.value;

        if (criticalRate > 0f && criticalRoll < criticalRate)
        {
            return new CraftResult { ResultType = CraftResultType.CriticalSuccess, ResultQuantity = 2 };
        }

        return new CraftResult { ResultType = CraftResultType.Success, ResultQuantity = 1 };
    }

    private int GetMonsterCountByGrade(MonsterGrade grade)
    {
        int count = 0;

        foreach (MonsterRescueRecord record in _playerOwnedData.MonsterRescueRecords)
        {
            if (record.MonsterData != null && record.MonsterData.grade == grade)
            {
                count += record.Count;
            }
        }

        return count;
    }

    private void ConsumeMonster(MonsterGrade grade)
    {
        foreach (MonsterRescueRecord record in _playerOwnedData.MonsterRescueRecords)
        {
            if (record.MonsterData != null && record.MonsterData.grade == grade && record.Count > 0)
            {
                record.Count--;
                return;
            }
        }
    }

    public Dictionary<MonsterGrade, int> GetAvailableMonstersByGrade()
    {
        Dictionary<MonsterGrade, int> result = new();

        foreach (MonsterRescueRecord record in _playerOwnedData.MonsterRescueRecords)
        {
            if (record.MonsterData == null || record.Count <= 0)
            {
                continue;
            }

            MonsterGrade grade = record.MonsterData.grade;

            if (result.ContainsKey(grade))
            {
                result[grade] += record.Count;
            }
            else
            {
                result[grade] = record.Count;
            }
        }

        return result;
    }

    public CraftSummary Summarize(List<CraftResult> results)
    {
        CraftSummary summary = new CraftSummary();

        foreach (CraftResult result in results)
        {
            switch (result.ResultType)
            {
                case CraftResultType.Success:
                    summary.TotalSuccess++;
                    summary.TotalAcquired += 1;
                    break;

                case CraftResultType.CriticalSuccess:
                    summary.TotalCritical++;
                    summary.TotalAcquired += 2;
                    break;

                case CraftResultType.Fail:
                    summary.TotalFail++;
                    break;
            }
        }

        return summary;
    }

    public float GetSuccessBonus(MonsterGrade grade)
    {
        return _gradeConfig.GetSuccessBonus(grade);
    }

    public float GetCriticalRate(MonsterGrade grade)
    {
        return _gradeConfig.GetCriticalRate(grade);
    }
}