using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class DungeonEnemyVariantEquipmentDropTests
{
    private static readonly string[] AncientGuardianPaths =
    {
        "GameData/Items/AncientGuardianBlade",
        "GameData/Items/AncientGuardianArmor",
        "GameData/Items/AncientGuardianSeal"
    };

    private static readonly string[] OniHunterPaths =
    {
        "GameData/Items/OniHunterCleaver",
        "GameData/Items/OniHunterGarb",
        "GameData/Items/GoblinFangTalisman"
    };

    private readonly List<EnemyDataSO> createdEnemies = new List<EnemyDataSO>();

    [TearDown]
    public void TearDown()
    {
        foreach (EnemyDataSO enemy in createdEnemies)
        {
            if (enemy != null)
            {
                UnityEngine.Object.DestroyImmediate(enemy);
            }
        }
        createdEnemies.Clear();
    }

    [Test]
    public void MiddleIronGolem_NormalVariant_AddsOneAncientGuardianDropAtTenPercent()
    {
        EnemyDataSO variant = CreateVariant("Grade05IronGolem", DungeonGrade.Middle, false, 0f);

        ItemDropEntry drop = GetSetDrops(variant, AncientGuardianPaths).Single();
        Assert.That(drop.dropChance, Is.EqualTo(0.10f));
        Assert.That(drop.amount, Is.EqualTo(1));
    }

    [Test]
    public void MiddleRuinGuardian_BossVariant_AddsOneAncientGuardianDropAtTwentyPercent()
    {
        EnemyDataSO variant = CreateVariant("Boss04RuinGuardian", DungeonGrade.Middle, true, 0f);

        ItemDropEntry drop = GetSetDrops(variant, AncientGuardianPaths).Single();
        Assert.That(drop.dropChance, Is.EqualTo(0.20f));
        Assert.That(drop.amount, Is.EqualTo(1));
    }

    [Test]
    public void UpperOgreMage_AddsOneOniHunterDrop()
    {
        EnemyDataSO variant = CreateVariant("Grade05OgreMage", DungeonGrade.Upper, false, 0f);

        Assert.That(GetSetDrops(variant, OniHunterPaths).Count(), Is.EqualTo(1));
    }

    [Test]
    public void MiddleOgreMage_DoesNotAddOniHunterDrop()
    {
        EnemyDataSO variant = CreateVariant("Grade05OgreMage", DungeonGrade.Middle, false, 0f);

        Assert.That(GetSetDrops(variant, OniHunterPaths).Count(), Is.EqualTo(0));
    }

    [Test]
    public void UpperAncientGuardianTarget_DoesNotAddAncientGuardianDrop()
    {
        EnemyDataSO variant = CreateVariant("Grade05IronGolem", DungeonGrade.Upper, false, 0f);

        Assert.That(GetSetDrops(variant, AncientGuardianPaths).Count(), Is.EqualTo(0));
    }

    [Test]
    public void UnrelatedEnemy_DoesNotAddAnySpecialEquipment()
    {
        EnemyDataSO variant = CreateVariant("UnrelatedEnemy", DungeonGrade.Middle, false, 0f);

        Assert.That(GetSetDrops(variant, AncientGuardianPaths.Concat(OniHunterPaths)).Count(),
            Is.EqualTo(0));
    }

    [Test]
    public void Variant_AddsAtMostOneOfTheSixSpecialEquipmentItems()
    {
        EnemyDataSO variant = CreateVariant("Grade05IronGolem", DungeonGrade.Middle, false, .99f);

        Assert.That(GetSetDrops(variant, AncientGuardianPaths.Concat(OniHunterPaths)).Count(),
            Is.LessThanOrEqualTo(1));
    }

    [TestCase(0f, "AncientGuardianBlade")]
    [TestCase(.5f, "AncientGuardianArmor")]
    [TestCase(.99f, "AncientGuardianSeal")]
    public void AncientGuardianDrop_RandomBoundariesSelectEveryEquipmentPiece(
        float randomValue,
        string expectedAssetName)
    {
        EnemyDataSO variant = CreateVariant(
            "Grade05IronGolem", DungeonGrade.Middle, false, randomValue);

        Assert.That(GetSetDrops(variant, AncientGuardianPaths).Single().item.name,
            Is.EqualTo(expectedAssetName));
    }

    [Test]
    public void EquipmentDrop_AdditionKeepsMutantCoreDrop()
    {
        EnemyDataSO variant = CreateVariant("Grade05IronGolem", DungeonGrade.Middle, false, 0f);
        ItemDataSO mutantCore = Resources.Load<ItemDataSO>(
            DungeonEnemyVariantService.GetMutantCoreResourcePath(DungeonGrade.Middle));

        Assert.That(mutantCore, Is.Not.Null);
        Assert.That((variant.itemDrops ?? Array.Empty<ItemDropEntry>())
            .Any(drop => drop != null && drop.item == mutantCore), Is.True);
    }

    private EnemyDataSO CreateVariant(
        string sourceName,
        DungeonGrade dungeonGrade,
        bool isBossVariant,
        float randomValue)
    {
        EnemyDataSO source = ScriptableObject.CreateInstance<EnemyDataSO>();
        source.name = sourceName;
        createdEnemies.Add(source);

        EnemyDataSO variant = DungeonEnemyVariantService.CreateSpecialVariant(
            source,
            new[] { EnemySkillType.PowerStrike },
            dungeonGrade,
            isBossVariant,
            () => randomValue);
        createdEnemies.Add(variant);
        return variant;
    }

    private static IEnumerable<ItemDropEntry> GetSetDrops(
        EnemyDataSO variant,
        IEnumerable<string> equipmentPaths)
    {
        HashSet<ItemDataSO> equipment = new HashSet<ItemDataSO>(
            equipmentPaths.Select(Resources.Load<ItemDataSO>));
        Assert.That(equipment.Contains(null), Is.False);
        return (variant.itemDrops ?? Array.Empty<ItemDropEntry>())
            .Where(drop => drop != null && equipment.Contains(drop.item));
    }
}
