using UnityEngine;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public bool IsAlreadyOwned(ShopItemEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        if (entry.category == ShopItemCategory.Skill && entry.skillItem != null)
        {
            return GameCore.PlayerOwnedData.OwnedSkills.Contains(entry.skillItem);
        }

        if (entry.category == ShopItemCategory.Recipe && entry.recipeItem != null)
        {
            return GameCore.PlayerOwnedData.IsRecipeUnlocked(entry.recipeItem);
        }

        if (entry.category == ShopItemCategory.Skin && entry.skinItem != null)
        {
            return GameCore.PlayerOwnedData.OwnedSkins.Contains(entry.skinItem);
        }

        return false;
    }

    public bool CanPurchase(ShopItemEntry entry, int quantity = 1)
    {
        if (entry == null)
        {
            return false;
        }

        if (IsAlreadyOwned(entry))
        {
            return false;
        }

        if (!IsConditionMet(entry))
        {
            return false;
        }

        if (!HasEnoughResources(entry, quantity))
        {
            return false;
        }

        return true;
    }

    public ShopPurchaseResult TryPurchase(ShopItemEntry entry, int quantity = 1)
    {
        if (entry == null)
        {
            return ShopPurchaseResult.Failed;
        }

        if (IsAlreadyOwned(entry))
        {
            return ShopPurchaseResult.Failed;
        }

        if (!IsConditionMet(entry))
        {
            GameCore.AlertManager.Enqueue(AlertType.PurchaseFailByCondition);
            return ShopPurchaseResult.ConditionNotMet;
        }

        if (!HasEnoughResources(entry, quantity))
        {
            GameCore.AlertManager.Enqueue(AlertType.PurchaseFailByResource);
            return ShopPurchaseResult.NotEnoughResources;
        }

        foreach (ItemRequirement req in entry.price)
        {
            GameCore.PlayerInventory.RemoveItem(req.item, req.amount * quantity);
        }

        GrantToPlayer(entry, quantity);

        GameCore.AlertManager.Enqueue(AlertType.PurchaseSuccess, entry.GetCategoryName(), entry.GetDisplayName());

        if (entry.category == ShopItemCategory.Skin ||
        entry.category == ShopItemCategory.Skill ||
        entry.category == ShopItemCategory.Recipe)
        {
            SaveManager.Instance?.Save();
        }

        return ShopPurchaseResult.Success;
    }

    public bool IsConditionMet(ShopItemEntry entry)
    {
        switch (entry.conditionType)
        {
            case ShopConditionType.None:
                return true;

            case ShopConditionType.WeaponEnhanceLevel:
                if (entry.conditionWeapon == null)
                {
                    return false;
                }

                WeaponEnhanceState state = GameCore.PlayerOwnedData.GetWeaponEnhanceState(entry.conditionWeapon);
                return state != null && state.CurrentLevel >= entry.conditionValue;

            case ShopConditionType.RescueCount:
                return GameCore.PlayerOwnedData.TotalRescueCount >= entry.conditionValue;

            default:
                return false;
        }
    }

    private bool HasEnoughResources(ShopItemEntry entry, int quantity = 1)
    {
        foreach (ItemRequirement req in entry.price)
        {
            if (!GameCore.PlayerInventory.HasItem(req.item, req.amount * quantity))
            {
                return false;
            }
        }

        return true;
    }

    private void GrantToPlayer(ShopItemEntry entry, int quantity = 1)
    {
        switch (entry.category)
        {
            case ShopItemCategory.Skill:
                if (entry.skillItem != null)
                {
                    GameCore.PlayerOwnedData.AddOwnedSkill(entry.skillItem);
                }
                break;

            case ShopItemCategory.ConsumableItem:
                if (entry.consumableItem != null)
                {
                    GameCore.PlayerInventory.AddItem(entry.consumableItem, quantity);
                }
                break;

            case ShopItemCategory.RescueItem:
                if (entry.rescueItem != null)
                {
                    GameCore.PlayerInventory.AddItem(entry.rescueItem, quantity);
                }
                break;

            case ShopItemCategory.Recipe:
                if (entry.recipeItem != null)
                {
                    GameCore.PlayerOwnedData.UnlockRecipe(entry.recipeItem);
                }
                break;

            case ShopItemCategory.Skin:
                if (entry.skinItem != null)
                {
                    GameCore.PlayerOwnedData.AddOwnedSkin(entry.skinItem);
                }
                break;
        }
    }
}