using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public readonly struct EquipmentSetTier
{
    public EquipmentSetTier(
        int requiredCount,
        int bonusMaxHP,
        int bonusAttack,
        int bonusDefense,
        float bonusAttackSpeed)
    {
        RequiredCount = requiredCount;
        BonusMaxHP = bonusMaxHP;
        BonusAttack = bonusAttack;
        BonusDefense = bonusDefense;
        BonusAttackSpeed = bonusAttackSpeed;
    }

    public int RequiredCount { get; }
    public int BonusMaxHP { get; }
    public int BonusAttack { get; }
    public int BonusDefense { get; }
    public float BonusAttackSpeed { get; }
}

public sealed class EquipmentSetDefinition
{
    public EquipmentSetDefinition(
        EquipmentSetId id,
        string displayName,
        int displayOrder,
        Color accentColor,
        EquipmentSetTier twoPiece,
        EquipmentSetTier threePiece)
    {
        Id = id;
        DisplayName = displayName;
        DisplayOrder = displayOrder;
        AccentColor = accentColor;
        TwoPiece = twoPiece;
        ThreePiece = threePiece;
    }

    public EquipmentSetId Id { get; }
    public string DisplayName { get; }
    public int DisplayOrder { get; }
    public Color AccentColor { get; }
    public EquipmentSetTier TwoPiece { get; }
    public EquipmentSetTier ThreePiece { get; }
}

public static class EquipmentSetCatalog
{
    private static readonly IReadOnlyList<EquipmentSetDefinition> definitions =
        new List<EquipmentSetDefinition>
        {
            Create(EquipmentSetId.AncientGuardian, "古代守護者", 10, new Color(.72f, .62f, .38f), 30, 0, 8, 0f, 0, 12, 0, .05f),
            Create(EquipmentSetId.Vanguard, "不屈の前衛", 20, new Color(.72f, .34f, .28f), 20, 0, 10, 0f, 0, 8, 0, 0f),
            Create(EquipmentSetId.Windstalker, "風狩り", 30, new Color(.32f, .72f, .54f), 0, 4, 0, .05f, 0, 7, 0, .04f),
            Create(EquipmentSetId.ArcaneSage, "秘術賢者", 40, new Color(.45f, .36f, .78f), 0, 8, 0, 0f, 0, 10, 0, .04f),
            Create(EquipmentSetId.OniHunter, "鬼狩り", 50, new Color(.78f, .31f, .24f), 10, 3, 0, 0f, 0, 5, 2, 0f),
            Create(EquipmentSetId.NornCanopy, "ノルン樹冠", 60, new Color(.24f, .58f, .36f), 15, 0, 0, 0f, 0, 2, 2, .01f),
            Create(EquipmentSetId.GlaadSkyFortress, "グラード天嶺", 70, new Color(.42f, .68f, .84f), 15, 0, 0, 0f, 0, 2, 2, .01f),
            Create(EquipmentSetId.VelmBlackIron, "ヴェルム黒鉄", 80, new Color(.36f, .38f, .44f), 20, 0, 0, 0f, 0, 3, 2, .015f),
            Create(EquipmentSetId.AbyssThrone, "アビス玉座", 90, new Color(.42f, .24f, .52f), 25, 0, 0, 0f, 0, 3, 3, .02f),
            Create(EquipmentSetId.AstralDepths, "星幽深層", 100, new Color(.28f, .42f, .78f), 45, 0, 0, .04f, 0, 10, 10, 0f),
            Create(EquipmentSetId.NornVerdantSettlement, "翠樹族の集落跡", 110, new Color(.28f, .62f, .38f), 15, 0, 0, 0f, 0, 2, 2, .01f),
            Create(EquipmentSetId.GlaadDragonScaleCanyon, "竜鱗峡谷", 120, new Color(.42f, .66f, .74f), 15, 0, 0, 0f, 0, 2, 2, .01f),
            Create(EquipmentSetId.VelmFurnaceDefenseZone, "熔炉防衛区", 130, new Color(.72f, .38f, .20f), 20, 0, 0, 0f, 0, 3, 2, .015f),
            Create(EquipmentSetId.AbyssGatewayThreshold, "奈落境門", 140, new Color(.36f, .18f, .46f), 25, 0, 0, 0f, 0, 3, 3, .02f),
            Create(EquipmentSetId.StartingCave, "はじまりの洞窟", 150, new Color(.48f, .48f, .36f), 5, 0, 0, 0f, 0, 1, 1, 0f),
            Create(EquipmentSetId.LeafForestTrail, "樹海の獣道", 160, new Color(.34f, .58f, .30f), 5, 0, 0, 0f, 0, 1, 1, 0f),
            Create(EquipmentSetId.EldUndergroundWaterway, "地下水路", 170, new Color(.28f, .50f, .66f), 5, 0, 0, 0f, 0, 1, 1, 0f),
            Create(EquipmentSetId.LowerMine, "封じられた廃坑", 180, new Color(.42f, .38f, .32f), 10, 0, 0, 0f, 0, 2, 1, 0f),
            Create(EquipmentSetId.EldOldQuarry, "旧採石場", 190, new Color(.50f, .44f, .34f), 10, 0, 0, 0f, 0, 2, 1, 0f),
            Create(EquipmentSetId.MiddleRuins, "霧の古代遺跡", 200, new Color(.46f, .56f, .60f), 10, 0, 0, 0f, 0, 2, 1, 0f)
        };

    public static IReadOnlyList<EquipmentSetDefinition> Definitions => definitions;

    public static bool TryGet(EquipmentSetId setId, out EquipmentSetDefinition definition)
    {
        foreach (EquipmentSetDefinition candidate in definitions)
        {
            if (candidate.Id == setId)
            {
                definition = candidate;
                return true;
            }
        }
        definition = null;
        return false;
    }

    public static EquipmentSetTier GetBonus(EquipmentSetId setId, int equippedCount)
    {
        if (!TryGet(setId, out EquipmentSetDefinition definition))
        {
            return default;
        }
        if (equippedCount >= definition.ThreePiece.RequiredCount)
        {
            return Add(definition.TwoPiece, definition.ThreePiece, equippedCount);
        }
        return equippedCount >= definition.TwoPiece.RequiredCount
            ? definition.TwoPiece
            : default;
    }

    public static string BuildDetailText(EquipmentSetId setId)
    {
        if (!TryGet(setId, out EquipmentSetDefinition definition))
        {
            return "セット効果: なし";
        }
        return $"セット: {definition.DisplayName}\n" +
               $"2部位: {BuildTierBonusText(definition.TwoPiece)}\n" +
               $"3部位: {BuildTierBonusText(definition.ThreePiece)}";
    }

    private static EquipmentSetDefinition Create(EquipmentSetId id, string displayName, int displayOrder, Color accentColor, int twoHp, int twoAttack, int twoDefense, float twoSpeed, int threeHp, int threeAttack, int threeDefense, float threeSpeed)
    {
        return new EquipmentSetDefinition(id, displayName, displayOrder, accentColor, new EquipmentSetTier(2, twoHp, twoAttack, twoDefense, twoSpeed), new EquipmentSetTier(3, threeHp, threeAttack, threeDefense, threeSpeed));
    }

    private static EquipmentSetTier Add(EquipmentSetTier first, EquipmentSetTier second, int requiredCount)
    {
        return new EquipmentSetTier(requiredCount, first.BonusMaxHP + second.BonusMaxHP, first.BonusAttack + second.BonusAttack, first.BonusDefense + second.BonusDefense, first.BonusAttackSpeed + second.BonusAttackSpeed);
    }

    public static string BuildTierBonusText(EquipmentSetTier tier)
    {
        List<string> bonuses = new List<string>();
        if (tier.BonusMaxHP != 0) bonuses.Add($"最大HP+{tier.BonusMaxHP}");
        if (tier.BonusAttack != 0) bonuses.Add($"攻撃+{tier.BonusAttack}");
        if (tier.BonusDefense != 0) bonuses.Add($"防御+{tier.BonusDefense}");
        if (Math.Abs(tier.BonusAttackSpeed) > .00001f) bonuses.Add($"攻撃速度+{tier.BonusAttackSpeed.ToString("0.###", CultureInfo.InvariantCulture)}");
        return bonuses.Count > 0 ? string.Join("、", bonuses) : "なし";
    }
}
