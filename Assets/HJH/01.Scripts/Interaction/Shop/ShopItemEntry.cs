using System.Collections.Generic;
using UnityEngine;

public enum ShopItemCategory
{
    Skill,
    ConsumableItem,
    RescueItem,
    Recipe,
    Skin
}

public enum ShopConditionType
{
    None,
    WeaponEnhanceLevel,
    RescueCount
}

[CreateAssetMenu(fileName = "ShopItemEntry", menuName = "Arda/Shop/ShopItemEntry")]
public class ShopItemEntry : ScriptableObject
{
    [Header("Category")]
    public ShopItemCategory category;

    [Header("Item (카테고리에 맞는 필드만 채우면 됨)")]
    public SkillDefinition skillItem;
    public ItemData consumableItem;
    public ItemData rescueItem;
    public RecipeData recipeItem;
    public GameObject skinItem;

    [Header("Price")]
    public List<ItemRequirement> price = new();

    [Header("Condition")]
    public ShopConditionType conditionType;
    public WeaponData conditionWeapon;
    public int conditionValue;

    public Sprite GetIcon()
    {
        return category switch
        {
            ShopItemCategory.Skill => skillItem?.skillIcon,
            ShopItemCategory.ConsumableItem => consumableItem?.Icon,
            ShopItemCategory.RescueItem => rescueItem?.Icon,
            ShopItemCategory.Recipe => recipeItem?.resultItem?.Icon,
            ShopItemCategory.Skin => skinItem != null ? skinItem.GetComponent<WeaponInfo>()?.SkinIcon : null,
            _ => null
        };
    }

    public string GetDisplayName()
    {
        return category switch
        {
            ShopItemCategory.Skill => skillItem?.skillName,
            ShopItemCategory.ConsumableItem => consumableItem?.ItemName,
            ShopItemCategory.RescueItem => rescueItem?.ItemName,
            ShopItemCategory.Recipe => recipeItem?.recipeName,
            ShopItemCategory.Skin => skinItem != null ? skinItem.GetComponent<WeaponInfo>()?.SkinName : string.Empty,
            _ => string.Empty
        };
    }

    public string GetDescription()
    {
        return category switch
        {
            ShopItemCategory.Skill => skillItem?.description,
            ShopItemCategory.ConsumableItem => consumableItem?.Description,
            ShopItemCategory.RescueItem => rescueItem?.Description,
            ShopItemCategory.Recipe => recipeItem?.description,
            ShopItemCategory.Skin => skinItem != null ? skinItem.GetComponent<WeaponInfo>()?.SkinName : string.Empty,
            _ => string.Empty
        };
    }

    public string GetCategoryName()
    {
        return category switch
        {
            ShopItemCategory.Skill => "스킬",
            ShopItemCategory.ConsumableItem => "소비",
            ShopItemCategory.RescueItem => "구조",
            ShopItemCategory.Recipe => "제작법",
            ShopItemCategory.Skin => "스킨",
            _ => string.Empty
        };
    }
}