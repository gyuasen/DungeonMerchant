using NUnit.Framework;
using UnityEngine;

public sealed class WorldMapServiceDemandTests
{
    [Test]
    public void GetTownDemandMultiplier_AllTownsAndItemTypes_StaysWithinBounds()
    {
        foreach (int townIndex in System.Linq.Enumerable.Range(0, WorldMapService.TownCount))
        {
            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
                item.itemType = itemType;
                float multiplier = WorldMapService.GetTownDemandMultiplier(townIndex, item);
                Assert.That(multiplier, Is.InRange(0.8f, 1.3f));
                Object.DestroyImmediate(item);
            }
        }
    }

    [Test]
    public void GetTownDemandMultiplier_SameInput_IsDeterministic()
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemType = ItemType.Material;
        item.itemName = "Bat Wing";

        float first = WorldMapService.GetTownDemandMultiplier(1, item);
        float second = WorldMapService.GetTownDemandMultiplier(1, item);

        Assert.That(second, Is.EqualTo(first));
        Object.DestroyImmediate(item);
    }

    [Test]
    public void GetTownDemandMultiplier_RepresentativeTownRules_ArePinned()
    {
        ItemDataSO leafMaterial = ScriptableObject.CreateInstance<ItemDataSO>();
        leafMaterial.itemType = ItemType.Material;
        leafMaterial.itemName = "Bat Wing";
        ItemDataSO consumable = ScriptableObject.CreateInstance<ItemDataSO>();
        consumable.itemType = ItemType.Consumable;

        Assert.That(WorldMapService.GetTownDemandMultiplier(1, leafMaterial), Is.GreaterThan(1f));
        Assert.That(WorldMapService.GetTownDemandMultiplier(2, consumable), Is.LessThan(1f));
        Assert.That(WorldMapService.GetTownDemandMultiplier(7, leafMaterial), Is.EqualTo(1f));
        Assert.That(WorldMapService.GetTownDemandMultiplier(7, consumable), Is.EqualTo(1f));

        Object.DestroyImmediate(leafMaterial);
        Object.DestroyImmediate(consumable);
    }

    [Test]
    public void GetTownDemandMultiplier_NullItem_ReturnsOne()
    {
        Assert.That(WorldMapService.GetTownDemandMultiplier(2, null), Is.EqualTo(1f));
    }
}
