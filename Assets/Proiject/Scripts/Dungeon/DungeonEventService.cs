using UnityEngine;

public static class DungeonEventService
{
    public static DungeonEventType RollRandomEvent()
    {
        return (DungeonEventType)Random.Range(
            (int)DungeonEventType.AbandonedCamp,
            (int)DungeonEventType.QualityGrove + 1);
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
            case DungeonEventType.MineralVein:
                return new DungeonEventPresentation("鉱脈を見つけた", "採掘できる鉱脈が露出している。", "慎重に採掘する", "急いで採掘する", "撤退");
            case DungeonEventType.HerbGrove:
                return new DungeonEventPresentation("薬草の群生地を見つけた", "薬効のある草が群生している。", "丁寧に採取する", "急いで採取する", "撤退");
            case DungeonEventType.QualityGrove:
                return new DungeonEventPresentation("良質な木立を見つけた", "加工に適した木材が手に入りそうだ。", "慎重に伐採する", "急いで伐採する", "撤退");
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

    public static string CreateChoicePreview(
        DungeonEventType eventType,
        int optionIndex,
        int restHealAmount,
        int treasureGoldReward,
        int hazardDamage)
    {
        if (optionIndex == 2)
        {
            return "探索を終了し、安全に町へ戻ります。";
        }

        DungeonEventChoiceResult result = ResolveChoice(
            eventType,
            optionIndex,
            restHealAmount,
            treasureGoldReward,
            hazardDamage);
        string preview = string.Empty;
        if (result.HealAmount > 0)
        {
            preview = $"パーティー全員のHPを{result.HealAmount}回復します。";
        }
        if (result.GoldAmount > 0)
        {
            preview += $" {result.GoldAmount} Gを獲得します。";
        }
        if (result.DamageAmount > 0)
        {
            preview += $" パーティー全員が{result.DamageAmount}ダメージを受けます。";
        }
        if (!string.IsNullOrEmpty(result.LimitedDropSourceLabel))
        {
            preview += " 限定装備を発見できる可能性があります。";
        }
        if (result.AddExplorationDelay)
        {
            preview += " 探索日数が1日増加します。";
        }
        else
        {
            preview += " 探索日数は増加しません。";
        }
        return preview.Trim();
    }

    public static string GetChoiceImageKey(
        DungeonEventType eventType,
        int optionIndex)
    {
        if (optionIndex == 2)
        {
            return "Retreat";
        }

        switch (eventType)
        {
            case DungeonEventType.AbandonedCamp:
                return optionIndex == 0
                    ? "AbandonedCamp_Rest"
                    : "AbandonedCamp_QuickRest";
            case DungeonEventType.TreasureCache:
                return optionIndex == 0
                    ? "TreasureCache_Careful"
                    : "TreasureCache_Force";
            case DungeonEventType.CollapsedPassage:
                return optionIndex == 0
                    ? "CollapsedPassage_Detour"
                    : "CollapsedPassage_Force";
            case DungeonEventType.MineralVein:
                return optionIndex == 0 ? "MineralVein_Careful" : "MineralVein_Quick";
            case DungeonEventType.HerbGrove:
                return optionIndex == 0 ? "HerbGrove_Careful" : "HerbGrove_Quick";
            case DungeonEventType.QualityGrove:
                return optionIndex == 0 ? "QualityGrove_Careful" : "QualityGrove_Quick";
            default:
                return "Retreat";
        }
    }
}

public static class DungeonEnvironmentEventService
{
public static DungeonEventChoiceResult ResolveEnvironmentalChoice(
    DungeonEventType eventType,
    int optionIndex,
    DungeonGrade dungeonGrade)
{
    if (optionIndex == 2)
    {
        return DungeonEventChoiceResult.None;
    }

    bool highGrade = dungeonGrade >= DungeonGrade.Upper;
    string path = eventType == DungeonEventType.MineralVein
        ? highGrade ? "GameData/Items/SilverOre" : "GameData/Items/IronOre"
        : eventType == DungeonEventType.HerbGrove
            ? highGrade ? "GameData/Items/AntidoteHerb" : "GameData/Items/MedicinalHerb"
            : highGrade ? "GameData/Items/Spiritwood" : "GameData/Items/Hardwood";
    ItemDataSO item = Resources.Load<ItemDataSO>(path);
    int amount = (optionIndex == 0 ? 2 : 1) + (int)dungeonGrade;
    return DungeonEventChoiceResult.Material(item, amount, optionIndex == 0);
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
    public readonly ItemDataSO MaterialItem;
    public readonly int MaterialAmount;

    private DungeonEventChoiceResult(
        int healAmount,
        int damageAmount,
        int goldAmount,
        bool addExplorationDelay,
        string limitedDropSourceLabel,
        ItemDataSO materialItem = null,
        int materialAmount = 0)
    {
        HealAmount = healAmount;
        DamageAmount = damageAmount;
        GoldAmount = goldAmount;
        AddExplorationDelay = addExplorationDelay;
        LimitedDropSourceLabel = limitedDropSourceLabel;
        MaterialItem = materialItem;
        MaterialAmount = materialAmount;
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

    public static DungeonEventChoiceResult Material(
        ItemDataSO item,
        int amount,
        bool addExplorationDelay)
    {
        return new DungeonEventChoiceResult(
            0,
            0,
            0,
            addExplorationDelay,
            null,
            item,
            Mathf.Max(1, amount));
    }
}
