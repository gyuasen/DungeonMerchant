using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class RoadCargoSessionTests
{
    private GameObject root;
    private MerchantData merchantData;
    private MerchantInventory inventory;
    private RoadCargoSession cargoSession;
    private readonly List<Object> createdAssets = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Road Cargo Session Test");
        merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<MarketPriceManager>();
        root.AddComponent<TownProgressState>().Initialize(2, new[] { 0, 1, 2 });
        root.AddComponent<ProgressionManager>();
        inventory = root.AddComponent<MerchantInventory>();
        cargoSession = root.AddComponent<RoadCargoSession>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        foreach (Object asset in createdAssets)
        {
            Object.DestroyImmediate(asset);
        }
        createdAssets.Clear();
    }

    [Test]
    public void CalculateCapacity_UsesLogisticsAndCapsAtFifty()
    {
        Assert.That(RoadCargoSession.CalculateCapacity(merchantData), Is.EqualTo(20));
        merchantData.RestoreSkills(0, 0, 0, 0, 7);
        Assert.That(RoadCargoSession.CalculateCapacity(merchantData), Is.EqualTo(27));
        merchantData.RestoreSkills(0, 0, 0, 0, 50);
        Assert.That(RoadCargoSession.CalculateCapacity(merchantData), Is.EqualTo(50));
    }

    [Test]
    public void CompleteVictory_DepositsCargoAtDestination()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        inventory.DepositItemTo(2, item, 4);

        Assert.That(Begin(2, 0, item, 4), Is.EqualTo(RoadCargoDepartureResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(2, item), Is.Zero);
        Assert.That(cargoSession.CompleteVictory(), Is.EqualTo(RoadCargoResolutionResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(0, item), Is.EqualTo(4));
        Assert.That(cargoSession.IsActive, Is.False);
    }

    [Test]
    public void Retreat_ReturnsAllCargoToOrigin()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        inventory.DepositItemTo(2, item, 4);

        Assert.That(Begin(2, 0, item, 4), Is.EqualTo(RoadCargoDepartureResult.Succeeded));
        Assert.That(cargoSession.Retreat(), Is.EqualTo(RoadCargoResolutionResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(2, item), Is.EqualTo(4));
    }

    [Test]
    public void CompleteDefeat_LosesCeilingOfQuarterPerItemType()
    {
        ItemDataSO first = NormalItemAt(0);
        ItemDataSO second = NormalItemAt(1);
        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.Not.Null);
        inventory.DepositItemTo(2, first, 5);
        inventory.DepositItemTo(2, second, 1);

        List<RoadCargoEntry> cargo = new List<RoadCargoEntry>
        {
            new RoadCargoEntry(first, 5),
            new RoadCargoEntry(second, 1)
        };
        Assert.That(cargoSession.TryBegin(2, 0, cargo), Is.EqualTo(RoadCargoDepartureResult.Succeeded));
        Assert.That(cargoSession.CompleteDefeat(), Is.EqualTo(RoadCargoResolutionResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(2, first), Is.EqualTo(3));
        Assert.That(inventory.GetItemAmountIn(2, second), Is.Zero);
    }

    [Test]
    public void CompleteVictory_WhenDestinationIsFull_KeepsCargoSession()
    {
        ItemDataSO cargoItem = NormalItemAt(0);
        ItemDataSO storedItem = NormalItemAt(1);
        Assert.That(cargoItem, Is.Not.Null);
        Assert.That(storedItem, Is.Not.Null);
        inventory.DepositItemTo(2, cargoItem, 1);
        inventory.DepositItemTo(0, storedItem, 30);

        Assert.That(Begin(2, 0, cargoItem, 1), Is.EqualTo(RoadCargoDepartureResult.Succeeded));
        Assert.That(cargoSession.CompleteVictory(), Is.EqualTo(RoadCargoResolutionResult.StorageFull));
        Assert.That(cargoSession.IsActive, Is.True);
        Assert.That(cargoSession.UsedCapacity, Is.EqualTo(1));
        Assert.That(inventory.GetItemAmountIn(0, cargoItem), Is.Zero);
    }

    [Test]
    public void CreateSaveDataAndRestore_PreservesDepartedCargo()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        inventory.DepositItemTo(2, item, 2);
        Assert.That(Begin(2, 0, item, 2), Is.EqualTo(RoadCargoDepartureResult.Succeeded));

        SavedRoadCargoSession saved = cargoSession.CreateSaveData();
        cargoSession.Restore(saved);

        Assert.That(cargoSession.IsActive, Is.True);
        Assert.That(cargoSession.ActiveSession.cargo[0].amount, Is.EqualTo(2));
    }

    private RoadCargoDepartureResult Begin(
        int origin,
        int destination,
        ItemDataSO item,
        int amount)
    {
        return cargoSession.TryBegin(origin, destination,
            new List<RoadCargoEntry> { new RoadCargoEntry(item, amount) });
    }

    private ItemDataSO CreateItem(string itemName)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = itemName;
        item.itemName = itemName;
        item.itemType = ItemType.Material;
        createdAssets.Add(item);
        return item;
    }

    private static ItemDataSO NormalItemAt(int index)
    {
        int current = 0;
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item != null &&
                (item.itemType == ItemType.Material ||
                 item.itemType == ItemType.Consumable))
            {
                if (current == index)
                {
                    return item;
                }
                current++;
            }
        }
        return null;
    }
}
