using System;

public readonly struct HealingCostBreakdown
{
    public HealingCostBreakdown(
        int maxHP,
        int missingHP,
        int lightInjuryHP,
        int moderateInjuryHP,
        int severeInjuryHP,
        int lightInjuryCost,
        int moderateInjuryCost,
        int severeInjuryCost,
        int revivalCost)
    {
        MaxHP = maxHP;
        MissingHP = missingHP;
        LightInjuryHP = lightInjuryHP;
        ModerateInjuryHP = moderateInjuryHP;
        SevereInjuryHP = severeInjuryHP;
        LightInjuryCost = lightInjuryCost;
        ModerateInjuryCost = moderateInjuryCost;
        SevereInjuryCost = severeInjuryCost;
        RevivalCost = revivalCost;
    }

    public int MaxHP { get; }
    public int MissingHP { get; }
    public int LightInjuryHP { get; }
    public int ModerateInjuryHP { get; }
    public int SevereInjuryHP { get; }
    public int LightInjuryCost { get; }
    public int ModerateInjuryCost { get; }
    public int SevereInjuryCost { get; }
    public int RevivalCost { get; }
    public int TotalCost =>
        LightInjuryCost + ModerateInjuryCost + SevereInjuryCost + RevivalCost;
}

public static class HealingCostService
{
    public const int LightInjuryRate = 1;
    public const int ModerateInjuryRate = 2;
    public const int SevereInjuryRate = 3;
    public const int RevivalCost = 50;
    public const int LightInjuryPercent = 25;
    public const int ModerateInjuryPercent = 50;

    public static HealingCostBreakdown CalculateFullHealCost(
        int maxHP,
        int currentHP,
        bool isIncapacitated)
    {
        int safeMaxHP = Math.Max(0, maxHP);
        int safeCurrentHP = Math.Max(0, Math.Min(currentHP, safeMaxHP));
        int missingHP = safeMaxHP - safeCurrentHP;

        if (missingHP == 0)
        {
            return new HealingCostBreakdown(
                safeMaxHP, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        int lightBoundary = PercentCeiling(
            safeMaxHP, LightInjuryPercent);
        int moderateBoundary = PercentCeiling(
            safeMaxHP, ModerateInjuryPercent);

        int lightHP = Math.Min(missingHP, lightBoundary);
        int moderateHP = Math.Min(
            Math.Max(0, missingHP - lightHP),
            Math.Max(0, moderateBoundary - lightBoundary));
        int severeHP = Math.Max(0, missingHP - lightHP - moderateHP);
        int revivalCost = isIncapacitated ? RevivalCost : 0;

        return new HealingCostBreakdown(
            safeMaxHP,
            missingHP,
            lightHP,
            moderateHP,
            severeHP,
            lightHP * LightInjuryRate,
            moderateHP * ModerateInjuryRate,
            severeHP * SevereInjuryRate,
            revivalCost);
    }

    private static int PercentCeiling(int value, int percent)
    {
        return (int)(((long)value * percent + 99L) / 100L);
    }
}
