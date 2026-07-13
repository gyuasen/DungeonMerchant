using NUnit.Framework;
using UnityEngine;

public sealed class TownAvailabilityParityTests
{
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
    public void Blacksmith_UsesOneRankAboveTheTownMarket(
        int townIndex, int expectedRank)
    {
        WorldMapService.EquipmentRankRange range =
            WorldMapService.GetBlacksmithEquipmentRankRange(townIndex);

        Assert.That(range.Minimum, Is.EqualTo(expectedRank));
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
            EquipmentSlot.Weapon), Is.False);
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
    public void ConfiguredDungeonLimitedDrops_MatchTheirTownRank()
    {
        DungeonDataSO[] dungeons =
            Resources.LoadAll<DungeonDataSO>("GameData/Dungeons");

        Assert.That(dungeons, Is.Not.Empty);
        foreach (DungeonDataSO dungeon in dungeons)
        {
            if (dungeon.limitedEquipmentDrops == null)
            {
                continue;
            }

            int expectedRank = WorldMapService.GetDungeonEquipmentRank(
                dungeon.nearbyTownIndex);
            foreach (ItemDataSO item in dungeon.limitedEquipmentDrops)
            {
                Assert.That(item, Is.Not.Null, dungeon.dungeonName);
                Assert.That(
                    item.equipmentRank,
                    Is.EqualTo(expectedRank),
                    $"{dungeon.dungeonName}: {item.itemName}");
                if (dungeon.nearbyTownIndex ==
                    WorldMapService.HiddenIslandTownIndex)
                {
                    Assert.That(item.equipmentRank, Is.EqualTo(10));
                }
                else
                {
                    Assert.That(item.equipmentRank, Is.LessThan(10));
                }
            }
        }
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
