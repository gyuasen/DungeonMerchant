using System;
using UnityEngine;

public static class MercenaryContractRules
{
    public static int GetRequiredMerchantLevel(MercenaryContractType contractType)
    {
        switch (contractType)
        {
            case MercenaryContractType.Exclusive: return 5;
            case MercenaryContractType.Temporary: return 2;
            default: return 1;
        }
    }

    public static int GetInitialCostMultiplier(MercenaryContractType contractType)
    {
        switch (contractType)
        {
            case MercenaryContractType.Temporary: return 10;
            case MercenaryContractType.Exclusive: return 20;
            default: return 1;
        }
    }

    public static int CalculateInitialCost(
        int baseHireCost,
        MercenaryContractType contractType)
    {
        long cost = (long)Math.Max(0, baseHireCost) *
            GetInitialCostMultiplier(contractType);
        return cost > int.MaxValue ? int.MaxValue : (int)cost;
    }

    public static int CalculateRenewalCost(
        int baseHireCost,
        MercenaryContractType contractType,
        float renewalCostMultiplier = 1f)
    {
        if (contractType == MercenaryContractType.Exclusive)
        {
            return 0;
        }

        int divisor = contractType == MercenaryContractType.Temporary ? 2 : 3;
        int baseRenewalCost = Math.Max(1, Math.Max(0, baseHireCost) / divisor);
        return Math.Max(1, Mathf.RoundToInt(baseRenewalCost * renewalCostMultiplier));
    }

    public static bool IsUpgrade(
        MercenaryContractType currentType,
        MercenaryContractType newType)
    {
        return GetContractRank(newType) > GetContractRank(currentType);
    }

    private static int GetContractRank(MercenaryContractType contractType)
    {
        switch (contractType)
        {
            case MercenaryContractType.Exclusive: return 2;
            case MercenaryContractType.Temporary: return 1;
            default: return 0;
        }
    }
}
