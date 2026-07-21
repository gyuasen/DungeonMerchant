using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class EquipmentAvailabilityTests
{
    private static readonly string[] NormalRank08Ids =
    {
        "item.normal.rank08",
        "item.normal.rank08.accessory",
        "item.normal.rank08.weapon"
    };

    private static readonly string[] NormalRank09Ids =
    {
        "item.normal.rank09",
        "item.normal.rank09.armor",
        "item.normal.rank09.weapon"
    };

    [Test]
    public void EveryMarketEquipment_ReachesAStandardTownMarket()
    {
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item == null ||
                !item.IsEquipment ||
                item.acquisitionType != ItemAcquisitionType.Market)
            {
                continue;
            }

            bool canReachMarket = false;
            for (int townIndex = 0;
                 townIndex < WorldMapService.HiddenIslandTownIndex;
                 townIndex++)
            {
                if (WorldMapService.IsMarketEquipmentAllowedInTown(
                    townIndex,
                    item.requiredClass,
                    item.equipmentRank,
                    item.equipmentSlot))
                {
                    canReachMarket = true;
                    break;
                }
            }

            Assert.That(canReachMarket, Is.True,
                $"Market equipment has no reachable market: {item.name}");
        }
    }

    [Test]
    public void Rank08Equipment_HasOneRecipePerItemAtAbyssBlacksmith()
    {
        EquipmentRecipeSO[] recipes =
            Resources.LoadAll<EquipmentRecipeSO>("GameData/Recipes");
        HashSet<string> expectedIds =
            new HashSet<string>(NormalRank08Ids);
        HashSet<string> actualIds = new HashSet<string>();

        Assert.That(recipes.Length, Is.EqualTo(NormalRank08Ids.Length));
        foreach (EquipmentRecipeSO recipe in recipes)
        {
            Assert.That(recipe, Is.Not.Null);
            Assert.That(recipe.resultItem, Is.Not.Null, recipe.name);
            Assert.That(recipe.resultItem.equipmentRank, Is.EqualTo(8));
            Assert.That(recipe.resultItem.acquisitionType,
                Is.EqualTo(ItemAcquisitionType.Blacksmith));
            Assert.That(WorldMapService.IsBlacksmithEquipmentAllowedInTown(
                6,
                recipe.resultItem.requiredClass,
                recipe.resultItem.equipmentRank,
                recipe.resultItem.equipmentSlot), Is.True, recipe.name);
            int expectedGoldCost = recipe.resultItem.equipmentSlot == EquipmentSlot.Weapon
                ? 550
                : recipe.resultItem.equipmentSlot == EquipmentSlot.Armor
                    ? 500
                    : 600;
            Assert.That(recipe.goldCost, Is.EqualTo(expectedGoldCost), recipe.name);
            Assert.That(recipe.materials, Is.Not.Null.And.Not.Empty,
                recipe.name);
            foreach (CraftingMaterialRequirement material in recipe.materials)
            {
                Assert.That(material, Is.Not.Null, recipe.name);
                Assert.That(material.item, Is.Not.Null, recipe.name);
                Assert.That(material.item.itemType, Is.EqualTo(ItemType.Material),
                    recipe.name);
                Assert.That(material.amount, Is.GreaterThan(0), recipe.name);
            }

            Assert.That(expectedIds.Contains(recipe.resultItem.PersistentId),
                Is.True, recipe.name);
            Assert.That(actualIds.Add(recipe.resultItem.PersistentId),
                Is.True, $"Duplicate Rank 8 recipe: {recipe.name}");
        }

        Assert.That(actualIds, Is.EquivalentTo(expectedIds));
    }

    [Test]
    public void HighestAbyss_UsesOnlyItsDedicatedThreePieceSet()
    {
        DungeonDataSO abyss =
            Resources.Load<DungeonDataSO>("Dungeons/FinalBlackSoilAbyss");
        Assert.That(abyss, Is.Not.Null);
        Assert.That(abyss.nearbyTownIndex, Is.EqualTo(6));
        Assert.That(abyss.limitedEquipmentDrops, Is.Not.Null);

        HashSet<string> drops = new HashSet<string>();
        HashSet<EquipmentSlot> slots = new HashSet<EquipmentSlot>();
        foreach (ItemDataSO item in abyss.limitedEquipmentDrops)
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(drops.Add(item.PersistentId), Is.True,
                $"Duplicate Highest Abyss drop: {item.name}");
            Assert.That(item.equipmentRank, Is.EqualTo(9), item.name);
            Assert.That(item.equipmentSet,
                Is.EqualTo(EquipmentSetId.AbyssThrone), item.name);
            Assert.That(slots.Add(item.equipmentSlot), Is.True, item.name);
        }

        Assert.That(abyss.limitedEquipmentDrops.Length, Is.EqualTo(3));
        Assert.That(slots, Is.EquivalentTo(new[]
        {
            EquipmentSlot.Weapon,
            EquipmentSlot.Armor,
            EquipmentSlot.Accessory
        }));
        Assert.That(drops, Is.EquivalentTo(new[]
        {
            "item.dungeon.abyss_fang",
            "item.dungeon.abyss_mantle",
            "item.dungeon.abyss_seal"
        }));
    }
}
