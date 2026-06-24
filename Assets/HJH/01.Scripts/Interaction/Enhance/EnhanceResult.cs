public enum EnhanceResultType
{
    Success,
    Fail,
    NotEnoughResources,
    MaxLevelReached
}

public class EnhanceResult
{
    public EnhanceResultType ResultType;
    public int NewLevel;
    public bool PityGuaranteed;
}