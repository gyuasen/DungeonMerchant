using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class TownAvailabilityParityTests
{
    [TestCase(0, true)]
    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(5, true)]
    [TestCase(6, true)]
    [TestCase(WorldMapService.HiddenIslandTownIndex, false)]
    public void TrainingGroundAvailability_ExcludesSailAndHiddenIsland(
        int townIndex,
        bool expected)
    {
        Assert.That(TownServicePolicy.IsTrainingGroundAvailable(townIndex),
            Is.EqualTo(expected));
    }

    [TestCase(2, 1)]
    [TestCase(1, 2)]
    [TestCase(0, 3)]
    [TestCase(3, 4)]
    [TestCase(4, 5)]
    [TestCase(5, 6)]
    [TestCase(6, 7)]
    public void MarketRankRange_FollowsTownProgression(
        int townIndex, int expectedRank)
    {
        WorldMapService.EquipmentRankRange range =
            WorldMapService.GetMarketEquipmentRankRange(townIndex);

        Assert.That(range.Minimum, Is.EqualTo(expectedRank));
        Assert.That(range.Maximum, Is.EqualTo(expectedRank));
        Assert.That(range.Contains(expectedRank - 1), Is.False);
        Assert.That(range.Contains(expectedRank + 1), Is.False);
    }

    [TestCase(2, 2)]
    [TestCase(1, 3)]
    [TestCase(0, 4)]
    [TestCase(3, 5)]
    [TestCase(4, 6)]
    [TestCase(5, 7)]
    [TestCase(6, 8)]
    public void Blacksmith_UnlocksEquipmentUpToOneRankAboveTheTownMarket(
        int townIndex, int expectedRank)
    {
        WorldMapService.EquipmentRankRange range =
            WorldMapService.GetBlacksmithEquipmentRankRange(townIndex);

        Assert.That(range.Minimum, Is.EqualTo(1));
        Assert.That(range.Maximum, Is.EqualTo(expectedRank));
        Assert.That(WorldMapService.IsBlacksmithEquipmentAllowedInTown(
            townIndex,
            MercenaryClass.Warrior,
            expectedRank,
            EquipmentSlot.Weapon), Is.True);
        Assert.That(WorldMapService.IsBlacksmithEquipmentAllowedInTown(
            townIndex,
            MercenaryClass.Warrior,
            expectedRank - 1,
            EquipmentSlot.Weapon), Is.True);
        Assert.That(WorldMapService.IsBlacksmithEquipmentAllowedInTown(
            townIndex,
            MercenaryClass.Warrior,
            expectedRank + 1,
            EquipmentSlot.Weapon), Is.False);
    }

    [Test]
    public void Blacksmith_NornKeepsRankFourRecipesAvailableAfterLeavingEld()
    {
        const int nornTownIndex = 3;
        const int eldBlacksmithRank = 4;

        Assert.That(WorldMapService.IsBlacksmithEquipmentAllowedInTown(
            nornTownIndex,
            MercenaryClass.Warrior,
            eldBlacksmithRank,
            EquipmentSlot.Weapon), Is.True);
    }

    [TestCase(2, 3)]
    [TestCase(1, 4)]
    [TestCase(0, 5)]
    [TestCase(3, 6)]
    [TestCase(4, 7)]
    [TestCase(5, 8)]
    [TestCase(6, 9)]
    public void DungeonEquipment_UsesTwoRanksAboveTheTownMarket(
        int townIndex, int expectedRank)
    {
        Assert.That(
            WorldMapService.GetDungeonEquipmentRank(townIndex),
            Is.EqualTo(expectedRank));
        Assert.That(
            WorldMapService.IsStandardDungeonEquipmentRank(
                townIndex,
                expectedRank),
            Is.True);
        Assert.That(
            WorldMapService.IsStandardDungeonEquipmentRank(townIndex, 10),
            Is.False);
    }

    [Test]
    public void ConfiguredDungeonLimitedDrops_AreWithinTheirTownAllowedRange()
    {
        DungeonDataSO[] dungeons = GameAssetRepository
            .LoadAll<DungeonDataSO>()
            .Where(dungeon => dungeon != null)
            .GroupBy(dungeon => dungeon.PersistentId)
            .Select(group => group.Single())
            .ToArray();

        Assert.That(dungeons, Has.Length.EqualTo(15));
        foreach (DungeonDataSO dungeon in dungeons)
        {
            if (dungeon.limitedEquipmentDrops == null)
            {
                continue;
            }

            foreach (ItemDataSO item in dungeon.limitedEquipmentDrops)
            {
                Assert.That(item, Is.Not.Null, dungeon.dungeonName);
                Assert.That(
                    WorldMapService.IsDungeonEquipmentRankAllowed(
                        dungeon.nearbyTownIndex,
                        item.equipmentRank),
                    Is.True,
                    $"{dungeon.dungeonName}: {item.itemName}");
            }
        }
    }

    [TestCase(0, 3, true)]
    [TestCase(0, 4, true)]
    [TestCase(0, 5, true)]
    [TestCase(0, 2, false)]
    [TestCase(0, 6, false)]
    [TestCase(6, 7, true)]
    [TestCase(6, 8, true)]
    [TestCase(6, 9, true)]
    [TestCase(6, 6, false)]
    [TestCase(6, 10, false)]
    [TestCase(WorldMapService.HiddenIslandTownIndex, 10, true)]
    [TestCase(WorldMapService.HiddenIslandTownIndex, 9, false)]
    public void DungeonEquipmentAllowedRange_UsesInclusiveTownBand(
        int townIndex,
        int equipmentRank,
        bool expected)
    {
        Assert.That(
            WorldMapService.IsDungeonEquipmentRankAllowed(
                townIndex,
                equipmentRank),
            Is.EqualTo(expected));
    }

    [TestCase("dungeon.EldUndergroundWaterway", 3)]
    [TestCase("dungeon.EldOldQuarry", 4)]
    [TestCase("dungeon.MiddleRuins", 5)]
    public void EldDungeonLimitedDrops_CanCreateEquipmentAtEveryAllowedRank(
        string dungeonPersistentId,
        int expectedRank)
    {
        DungeonDataSO dungeon =
            GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                dungeonPersistentId);

        EquipmentInstance equipment =
            DungeonRewardService.TryCreateLimitedEquipment(dungeon, () => 0f);

        Assert.That(equipment, Is.Not.Null, dungeonPersistentId);
        Assert.That(equipment.BaseItem.equipmentRank, Is.EqualTo(expectedRank));
    }

    [Test]
    public void HiddenIsland_ExclusivelyProvidesRankTen()
    {
        Assert.That(
            WorldMapService.GetBlacksmithEquipmentRankRange(
                WorldMapService.HiddenIslandTownIndex).Contains(10),
            Is.True);
        Assert.That(
            WorldMapService.GetDungeonEquipmentRank(
                WorldMapService.HiddenIslandTownIndex),
            Is.EqualTo(10));
        Assert.That(
            WorldMapService.IsStandardDungeonEquipmentRank(
                WorldMapService.HiddenIslandTownIndex,
                10),
            Is.False);
        Assert.That(
            WorldMapService.IsDungeonEquipmentRankAllowed(
                WorldMapService.HiddenIslandTownIndex,
                10),
            Is.True);
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public void RankPresentation_ProvidesColoredExplicitRank(int rank)
    {
        string text = EquipmentRankPresentation.GetRichText(rank);
        Assert.That(text, Does.Contain("Rank " + rank));
        Assert.That(text, Does.StartWith("<color=#"));
    }
}
