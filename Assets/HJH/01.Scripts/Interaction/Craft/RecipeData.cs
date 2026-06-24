using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Arda/Craft/RecipeData")]
public class RecipeData : ScriptableObject
{
    [Header("Info")]
    public string recipeName;
    [TextArea]
    public string description;

    [Header("Result")]
    public ItemData resultItem;

    [Header("Materials")]
    public List<ItemRequirement> materials = new();

    [Header("Craft")]
    [Range(0f, 1f)]
    public float baseSuccessRate;
}