public enum CraftResultType
{
    Success,
    CriticalSuccess,
    Fail,
    NotEnoughMaterials,
    RecipeNotUnlocked
}

public class CraftResult
{
    public CraftResultType ResultType;
    public int ResultQuantity;
}