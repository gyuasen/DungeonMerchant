using System;
using UnityEngine;

public class MerchantData : MonoBehaviour
{
    public const int MaxMerchantLevel = 100;
    public const int MaxSkillRank = 50;

    [Header("Merchant Status")]
    [SerializeField, Min(0)] private int gold = 500;
    [SerializeField, Min(1)] private int merchantLevel = 1;
    [SerializeField, Min(0)] private int merchantExperience;
    [SerializeField, Min(0)] private int lifetimeGoldEarned;
    [SerializeField, Min(0)] private int merchantSkillPoints = 2;
    [SerializeField, Range(0, MaxSkillRank)] private int negotiation;
    [SerializeField, Range(0, MaxSkillRank)] private int leadership;
    [SerializeField, Range(0, MaxSkillRank)] private int appraisal;
    [SerializeField, Range(0, MaxSkillRank)] private int logistics;

    public int Gold => gold;
    public int MerchantLevel => merchantLevel;
    public int MerchantExperience => merchantExperience;
    public int ExperienceToNextLevel => GetGoldRequiredForLevel(merchantLevel);
    public int LifetimeGoldEarned => lifetimeGoldEarned;
    public int MerchantSkillPoints => merchantSkillPoints;
    public int Negotiation => negotiation;
    public int Leadership => leadership;
    public int Appraisal => appraisal;
    public int Logistics => logistics;

    public event Action<int> GoldChanged;
    public event Action ProgressionChanged;

    private void OnValidate()
    {
        gold = Mathf.Max(0, gold);
    }

    public bool CanPay(int amount)
    {
        return amount >= 0 && gold >= amount;
    }

    public bool TryPayGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Invalid payment amount.");
            return false;
        }

        if (!CanPay(amount))
        {
            Debug.Log("Not enough gold.");
            return false;
        }

        gold -= amount;
        GoldChanged?.Invoke(gold);
        Debug.Log($"Paid {amount} G. Current gold: {gold} G");
        return true;
    }

    public void PayGold(int amount)
    {
        TryPayGold(amount);
    }

    public void AddGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Invalid gold reward amount.");
            return;
        }

        gold += amount;
        lifetimeGoldEarned += amount;
        RecalculateLevelFromEarnings();
        GoldChanged?.Invoke(gold);
        Debug.Log($"Gained {amount} G. Current gold: {gold} G");
    }

    public void SetGold(int value)
    {
        gold = Mathf.Max(0, value);
        GoldChanged?.Invoke(gold);
    }

    public void AddExperience(int amount)
    {
        // Merchant growth is based only on earned gold.
    }

    public void RestoreProgression(
        int level,
        int experience,
        int savedLifetimeGoldEarned = -1)
    {
        lifetimeGoldEarned = savedLifetimeGoldEarned >= 0
            ? savedLifetimeGoldEarned
            : EstimateLifetimeEarnings(level, experience);
        RecalculateLevelFromEarnings(false);
        ProgressionChanged?.Invoke();
    }

    public void RestoreSkills(
        int skillPoints,
        int negotiationRank,
        int leadershipRank,
        int appraisalRank,
        int logisticsRank)
    {
        merchantSkillPoints = Mathf.Max(0, skillPoints);
        negotiation = Mathf.Clamp(negotiationRank, 0, MaxSkillRank);
        leadership = Mathf.Clamp(leadershipRank, 0, MaxSkillRank);
        appraisal = Mathf.Clamp(appraisalRank, 0, MaxSkillRank);
        logistics = Mathf.Clamp(logisticsRank, 0, MaxSkillRank);
        ProgressionChanged?.Invoke();
    }

    public int GetSkillRank(MerchantSkillType skill)
    {
        switch (skill)
        {
            case MerchantSkillType.Negotiation: return negotiation;
            case MerchantSkillType.Leadership: return leadership;
            case MerchantSkillType.Appraisal: return appraisal;
            case MerchantSkillType.Logistics: return logistics;
            default: return 0;
        }
    }

    public bool TryIncreaseSkill(MerchantSkillType skill)
    {
        if (merchantSkillPoints <= 0 ||
            GetSkillRank(skill) >= MaxSkillRank)
        {
            return false;
        }

        switch (skill)
        {
            case MerchantSkillType.Negotiation: negotiation++; break;
            case MerchantSkillType.Leadership: leadership++; break;
            case MerchantSkillType.Appraisal: appraisal++; break;
            case MerchantSkillType.Logistics: logistics++; break;
            default: return false;
        }

        merchantSkillPoints--;
        ProgressionChanged?.Invoke();
        return true;
    }

    public bool IsContractUnlocked(MercenaryContractType type)
    {
        switch (type)
        {
            case MercenaryContractType.Exclusive: return merchantLevel >= 5;
            case MercenaryContractType.Temporary: return merchantLevel >= 2;
            default: return true;
        }
    }

    public float GetHireSuccessRate()
    {
        float leadershipBonus = leadership * 0.015f;
        if (leadership >= 3)
        {
            leadershipBonus += 0.05f;
        }
        return Mathf.Clamp01(
            0.65f + (merchantLevel - 1) * 0.025f + leadershipBonus);
    }

    public float GetMarketBuyMultiplier()
    {
        float multiplier = 1f - negotiation * 0.02f;
        if (negotiation >= 3)
        {
            multiplier -= 0.05f;
        }
        return Mathf.Max(0.7f, multiplier);
    }

    public float GetMarketSellMultiplier()
    {
        float multiplier = 1f + negotiation * 0.015f;
        if (negotiation >= 7)
        {
            multiplier += 0.1f;
        }
        return multiplier;
    }

    public float GetRenewalCostMultiplier()
    {
        float multiplier = 1f - leadership * 0.02f;
        if (leadership >= 7)
        {
            multiplier -= 0.15f;
        }
        return Mathf.Max(0.6f, multiplier);
    }

    public float GetQuestGoldMultiplier()
    {
        float multiplier = 1f + appraisal * 0.03f;
        if (appraisal >= 3)
        {
            multiplier += 0.1f;
        }
        return multiplier;
    }

    public float GetQuestExperienceMultiplier()
    {
        float multiplier = 1f + appraisal * 0.02f;
        if (appraisal >= 7)
        {
            multiplier += 0.15f;
        }
        return multiplier;
    }

    public int GetStorageCapacityBonus()
    {
        return logistics * 3 + (logistics >= 3 ? 10 : 0);
    }

    public float GetExplorationExpenseMultiplier()
    {
        float multiplier = 1f - logistics * 0.025f;
        if (logistics >= 7)
        {
            multiplier -= 0.15f;
        }
        return Mathf.Max(0.55f, multiplier);
    }

    public string GetUnlockedMerchantSkills()
    {
        System.Collections.Generic.List<string> skills =
            new System.Collections.Generic.List<string>();
        if (negotiation >= 3) skills.Add("値切り術");
        if (negotiation >= 7) skills.Add("商談の達人");
        if (leadership >= 3) skills.Add("人を見る目");
        if (leadership >= 7) skills.Add("契約管理");
        if (appraisal >= 3) skills.Add("目利き");
        if (appraisal >= 7) skills.Add("慧眼");
        if (logistics >= 3) skills.Add("荷役整理");
        if (logistics >= 7) skills.Add("遠征計画");
        return skills.Count > 0 ? string.Join("、", skills) : "なし";
    }

    private static int GetGoldRequiredForLevel(int level)
    {
        if (level >= MaxMerchantLevel)
        {
            return 0;
        }

        return 500 + level * level * 100;
    }

    private static int EstimateLifetimeEarnings(int level, int experience)
    {
        long total = Mathf.Max(0, experience);
        int targetLevel = Mathf.Clamp(level, 1, MaxMerchantLevel);
        for (int currentLevel = 1; currentLevel < targetLevel; currentLevel++)
        {
            total += GetGoldRequiredForLevel(currentLevel);
        }
        return (int)Math.Min(int.MaxValue, total);
    }

    public static int EstimateLifetimeEarningsForMigration(
        int level,
        int experience)
    {
        return EstimateLifetimeEarnings(level, experience);
    }

    private void RecalculateLevelFromEarnings(bool grantSkillPoints = true)
    {
        int previousLevel = merchantLevel;
        int calculatedLevel = 1;
        long remaining = Mathf.Max(0, lifetimeGoldEarned);

        while (calculatedLevel < MaxMerchantLevel)
        {
            int required = GetGoldRequiredForLevel(calculatedLevel);
            if (remaining < required)
            {
                break;
            }
            remaining -= required;
            calculatedLevel++;
        }

        merchantLevel = calculatedLevel;
        merchantExperience = merchantLevel >= MaxMerchantLevel
            ? 0
            : (int)Math.Min(int.MaxValue, remaining);

        if (grantSkillPoints && merchantLevel > previousLevel)
        {
            merchantSkillPoints += merchantLevel - previousLevel;
        }
        ProgressionChanged?.Invoke();
    }
}

public enum MerchantSkillType
{
    Negotiation,
    Leadership,
    Appraisal,
    Logistics
}
