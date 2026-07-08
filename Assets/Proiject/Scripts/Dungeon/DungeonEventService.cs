using UnityEngine;

public static class DungeonEventService
{
    public static DungeonEventType RollRandomEvent()
    {
        return (DungeonEventType)Random.Range(
            (int)DungeonEventType.AbandonedCamp,
            (int)DungeonEventType.CollapsedPassage + 1);
    }

    public static DungeonEventPresentation CreatePresentation(
        DungeonEventType eventType,
        int restHealAmount,
        int treasureGoldReward,
        int hazardDamage)
    {
        int halfHeal = Mathf.Max(1, restHealAmount / 2);
        switch (eventType)
        {
            case DungeonEventType.AbandonedCamp:
                return new DungeonEventPresentation(
                    "放棄された野営地",
                    "静かな野営地を発見しました。休息は時間を使いますが、今回のフロア探索は合計1日として処理されます。",
                    $"休息 HP+{restHealAmount}（時間消費）",
                    $"物資探索 +{treasureGoldReward / 2} G",
                    "撤退");
            case DungeonEventType.TreasureCache:
                return new DungeonEventPresentation(
                    "隠された宝箱",
                    "商人が隠したと思われる施錠された箱を発見しました。回収してもフロア探索日数は1日のままです。",
                    $"回収 +{treasureGoldReward} G",
                    $"休息 HP+{halfHeal}（短時間）",
                    "撤退");
            case DungeonEventType.CollapsedPassage:
                return new DungeonEventPresentation(
                    "崩れた通路",
                    "近道は崩れかけています。強行突破は時間を使いますが、今回のフロア探索は合計1日として処理されます。",
                    $"強行突破 HP-{hazardDamage}（時間消費）",
                    $"休息 HP+{halfHeal}（短時間）",
                    "撤退");
            default:
                return DungeonEventPresentation.Empty;
        }
    }

    public static DungeonEventChoiceResult ResolveChoice(
        DungeonEventType eventType,
        int optionIndex,
        int restHealAmount,
        int treasureGoldReward,
        int hazardDamage)
    {
        int halfHeal = Mathf.Max(1, restHealAmount / 2);
        switch (eventType)
        {
            case DungeonEventType.AbandonedCamp:
                return optionIndex == 0
                    ? DungeonEventChoiceResult.Heal(restHealAmount, true)
                    : DungeonEventChoiceResult.Gold(
                        treasureGoldReward / 2,
                        "探索イベント限定ドロップ");
            case DungeonEventType.TreasureCache:
                return optionIndex == 0
                    ? DungeonEventChoiceResult.Gold(
                        treasureGoldReward,
                        "宝箱限定ドロップ")
                    : DungeonEventChoiceResult.Heal(halfHeal, false);
            case DungeonEventType.CollapsedPassage:
                return optionIndex == 0
                    ? DungeonEventChoiceResult.Damage(hazardDamage, true)
                    : DungeonEventChoiceResult.Heal(halfHeal, false);
            default:
                return DungeonEventChoiceResult.None;
        }
    }
}

public readonly struct DungeonEventPresentation
{
    public static readonly DungeonEventPresentation Empty =
        new DungeonEventPresentation(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

    public readonly string Title;
    public readonly string Description;
    public readonly string FirstOptionLabel;
    public readonly string SecondOptionLabel;
    public readonly string ThirdOptionLabel;

    public DungeonEventPresentation(
        string title,
        string description,
        string firstOptionLabel,
        string secondOptionLabel,
        string thirdOptionLabel)
    {
        Title = title;
        Description = description;
        FirstOptionLabel = firstOptionLabel;
        SecondOptionLabel = secondOptionLabel;
        ThirdOptionLabel = thirdOptionLabel;
    }
}

public readonly struct DungeonEventChoiceResult
{
    public static readonly DungeonEventChoiceResult None =
        new DungeonEventChoiceResult(0, 0, 0, false, null);

    public readonly int HealAmount;
    public readonly int DamageAmount;
    public readonly int GoldAmount;
    public readonly bool AddExplorationDelay;
    public readonly string LimitedDropSourceLabel;

    private DungeonEventChoiceResult(
        int healAmount,
        int damageAmount,
        int goldAmount,
        bool addExplorationDelay,
        string limitedDropSourceLabel)
    {
        HealAmount = healAmount;
        DamageAmount = damageAmount;
        GoldAmount = goldAmount;
        AddExplorationDelay = addExplorationDelay;
        LimitedDropSourceLabel = limitedDropSourceLabel;
    }

    public static DungeonEventChoiceResult Heal(
        int amount,
        bool addExplorationDelay)
    {
        return new DungeonEventChoiceResult(
            Mathf.Max(0, amount),
            0,
            0,
            addExplorationDelay,
            null);
    }

    public static DungeonEventChoiceResult Damage(
        int amount,
        bool addExplorationDelay)
    {
        return new DungeonEventChoiceResult(
            0,
            Mathf.Max(0, amount),
            0,
            addExplorationDelay,
            null);
    }

    public static DungeonEventChoiceResult Gold(
        int amount,
        string limitedDropSourceLabel)
    {
        return new DungeonEventChoiceResult(
            0,
            0,
            Mathf.Max(0, amount),
            false,
            limitedDropSourceLabel);
    }
}
