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
                    "静かな野営地を発見しました。十分に休めば探索日数が1日増えます。短い休息なら時間は増えません。",
                    $"本格休息 HP+{restHealAmount} / +1日",
                    $"短時間休息 HP+{halfHeal} / 日数増加なし",
                    "撤退");
            case DungeonEventType.TreasureCache:
                return new DungeonEventPresentation(
                    "隠された宝箱",
                    "施錠された宝箱です。慎重に解錠すれば安全ですが時間を使います。こじ開ければ早い代わりに罠を受けます。",
                    $"慎重に解錠 +{treasureGoldReward} G / +1日",
                    $"強引に開ける +{treasureGoldReward / 2} G / HP-{hazardDamage}",
                    "撤退");
            case DungeonEventType.CollapsedPassage:
                return new DungeonEventPresentation(
                    "崩れた通路",
                    "通路が崩落しています。安全な迂回路は時間を使い、強行突破は負傷する代わりに時間を節約できます。",
                    "安全に迂回 / +1日",
                    $"強行突破 HP-{hazardDamage} / 日数増加なし",
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
                    : DungeonEventChoiceResult.Heal(halfHeal, false);
            case DungeonEventType.TreasureCache:
                return optionIndex == 0
                    ? DungeonEventChoiceResult.Gold(
                        treasureGoldReward,
                        "宝箱限定ドロップ",
                        true)
                    : DungeonEventChoiceResult.Gold(
                        treasureGoldReward / 2,
                        "罠付き宝箱限定ドロップ",
                        false,
                        hazardDamage);
            case DungeonEventType.CollapsedPassage:
                return optionIndex == 0
                    ? DungeonEventChoiceResult.Delay()
                    : DungeonEventChoiceResult.Damage(hazardDamage, false);
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
        string limitedDropSourceLabel,
        bool addExplorationDelay = false,
        int damageAmount = 0)
    {
        return new DungeonEventChoiceResult(
            0,
            Mathf.Max(0, damageAmount),
            Mathf.Max(0, amount),
            addExplorationDelay,
            limitedDropSourceLabel);
    }

    public static DungeonEventChoiceResult Delay()
    {
        return new DungeonEventChoiceResult(0, 0, 0, true, null);
    }
}
