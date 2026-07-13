using System.Collections.Generic;
using NUnit.Framework;

public sealed class GameAssetRepositoryTests
{
    [Test]
    public void DungeonPersistentIds_ArePresentAndUnique()
    {
        IReadOnlyList<DungeonDataSO> dungeons =
            GameAssetRepository.LoadAll<DungeonDataSO>();
        HashSet<string> ids = new HashSet<string>();

        Assert.That(dungeons.Count, Is.GreaterThan(0));
        foreach (DungeonDataSO dungeon in dungeons)
        {
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.PersistentId, Is.Not.Null.And.Not.Empty);
            Assert.That(
                ids.Add(dungeon.PersistentId),
                Is.True,
                $"Duplicate dungeon PersistentId: {dungeon.PersistentId}");
            Assert.That(
                GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                    dungeon.PersistentId,
                    dungeon.name),
                Is.SameAs(dungeon));
        }
    }

    [Test]
    public void HighestDungeonForEachTown_IsUniqueAndUsesExpectedEquipmentRank()
    {
        IReadOnlyList<DungeonDataSO> dungeons =
            GameAssetRepository.LoadAll<DungeonDataSO>();

        for (int townIndex = 0;
             townIndex < WorldMapService.TownCount;
             townIndex++)
        {
            DungeonDataSO highest = null;
            int highestCount = 0;
            foreach (DungeonDataSO dungeon in dungeons)
            {
                if (dungeon == null || dungeon.nearbyTownIndex != townIndex)
                {
                    continue;
                }

                if (highest == null || dungeon.grade > highest.grade)
                {
                    highest = dungeon;
                    highestCount = 1;
                }
                else if (dungeon.grade == highest.grade)
                {
                    highestCount++;
                }
            }

            Assert.That(
                highest,
                Is.Not.Null,
                $"Town {townIndex} has no dungeon.");
            Assert.That(
                highestCount,
                Is.EqualTo(1),
                $"Town {townIndex} has multiple highest-grade dungeons.");
            Assert.That(
                highest.limitedEquipmentDrops,
                Is.Not.Null.And.Not.Empty,
                $"{highest.name} has no unique equipment.");

            int expectedRank =
                WorldMapService.GetDungeonEquipmentRank(townIndex);
            foreach (ItemDataSO item in highest.limitedEquipmentDrops)
            {
                Assert.That(item, Is.Not.Null, $"{highest.name} has a missing drop.");
                Assert.That(item.IsEquipment, Is.True, item.name);
                Assert.That(
                    item.equipmentRank,
                    Is.EqualTo(expectedRank),
                    $"{highest.name}: {item.name}");
            }
        }
    }
}
