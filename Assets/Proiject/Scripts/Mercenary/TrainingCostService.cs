public static class TrainingCostService
{
    public const int BaseCostMultiplier = 25;

    public static int GetCost(int targetLevel)
    {
        return BaseCostMultiplier * targetLevel * targetLevel;
    }
}
