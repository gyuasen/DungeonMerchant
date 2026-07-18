using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class RemoteSaleManagerTests
{
    private GameObject root;
    private DayManager dayManager;
    private MerchantData merchantData;
    private MerchantInventory inventory;
    private TownProgressState towns;
    private RemoteSaleManager manager;
    private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Remote Sale Manager Test");
        dayManager = root.AddComponent<DayManager>();
        merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<MarketPriceManager>();
        inventory = root.AddComponent<MerchantInventory>();
        towns = root.AddComponent<TownProgressState>();
        towns.Initialize(2, new[] { 0, 1, 2 });
        manager = root.AddComponent<RemoteSaleManager>();
        manager.ResolveReferences();
        merchantData.SetGold(100);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        foreach (UnityEngine.Object asset in assets)
        {
            UnityEngine.Object.DestroyImmediate(asset);
        }
    }

    [Test]
    public void TryCreateItemOrder_RejectsCurrentTownInsufficientAndZeroWithoutSideEffects()
    {
        ItemDataSO item = CreateItem("Iron", 100);
        inventory.DepositItemTo(1, item, 2);
        Assert.That(manager.TryCreateItemOrder(2, item, 1), Is.EqualTo(RemoteSaleOrderResult.CurrentTown));
        Assert.That(manager.TryCreateItemOrder(1, item, 3), Is.EqualTo(RemoteSaleOrderResult.InsufficientInventory));
        Assert.That(manager.TryCreateItemOrder(1, item, 0), Is.EqualTo(RemoteSaleOrderResult.InvalidAmount));
        Assert.That(inventory.GetItemAmountIn(1, item), Is.EqualTo(2));
        Assert.That(manager.ActiveOrders, Is.Empty);
        Assert.That(merchantData.Gold, Is.EqualTo(100));
    }

    [Test]
    public void TryCreateItemOrder_RemovesReservedAmountFromTargetTown()
    {
        ItemDataSO item = CreateItem("Iron", 100);
        inventory.DepositItemTo(1, item, 5);
        Assert.That(manager.TryCreateItemOrder(1, item, 3), Is.EqualTo(RemoteSaleOrderResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(1, item), Is.EqualTo(2));
        Assert.That(manager.ActiveOrders[0].RemainingDays, Is.EqualTo(1));
    }

    [Test]
    public void CancelOrder_ReturnsReservedItemsToTargetTown()
    {
        ItemDataSO item = CreateItem("Iron", 100);
        inventory.DepositItemTo(1, item, 4);
        manager.TryCreateItemOrder(1, item, 4);
        Assert.That(manager.CancelOrder(manager.ActiveOrders[0]), Is.True);
        Assert.That(inventory.GetItemAmountIn(1, item), Is.EqualTo(4));
        Assert.That(manager.ActiveOrders, Is.Empty);
    }

    [Test]
    public void DayChanged_SettlesItemAtTargetDemandPrice()
    {
        ItemDataSO item = CreateItem("Leaf", 101);
        inventory.DepositItemTo(1, item, 2);
        manager.TryCreateItemOrder(1, item, 2);
        int expected = manager.GetSellPriceAt(1, item) * 2;
        dayManager.AdvanceDay();
        Assert.That(merchantData.Gold, Is.EqualTo(100 + expected));
        Assert.That(manager.ActiveOrders, Is.Empty);
    }

    [Test]
    public void EquipmentOrder_SettlesEquipmentInstance()
    {
        ItemDataSO item = CreateItem("Sword", 200);
        item.itemType = ItemType.Equipment;
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            "remote-sword", item, EquipmentQuality.Fine,
            new List<EquipmentModifier>(), 2);
        inventory.DepositEquipmentTo(1, equipment);
        Assert.That(manager.TryCreateEquipmentOrder(1, equipment), Is.EqualTo(RemoteSaleOrderResult.Succeeded));
        Assert.That(inventory.GetEquipmentInstancesIn(1), Is.Empty);
        int expected = manager.GetSellPriceAt(1, equipment);
        dayManager.AdvanceDay();
        Assert.That(merchantData.Gold, Is.EqualTo(100 + expected));
        Assert.That(manager.ActiveOrders, Is.Empty);
    }

    [Test]
    public void CreateSaveDataAndRestore_PreservesItemOrder()
    {
        ItemDataSO item = CreateItem("Saved Iron", 100);
        inventory.DepositItemTo(1, item, 2);
        manager.TryCreateItemOrder(1, item, 2);
        List<SavedRemoteSaleOrder> saved = manager.CreateSaveData();
        GameObject restoredRoot = new GameObject("Restored Remote Sale Test");
        restoredRoot.AddComponent<DayManager>();
        restoredRoot.AddComponent<MerchantData>();
        restoredRoot.AddComponent<MarketPriceManager>();
        restoredRoot.AddComponent<MerchantInventory>();
        TownProgressState restoredTowns = restoredRoot.AddComponent<TownProgressState>();
        restoredTowns.Initialize(2, new[] { 0, 1, 2 });
        RemoteSaleManager restored = restoredRoot.AddComponent<RemoteSaleManager>();
        restored.ResolveReferences();
        restored.Restore(saved);
        Assert.That(restored.ActiveOrders.Count, Is.EqualTo(1));
        Assert.That(restored.ActiveOrders[0].Item, Is.EqualTo(item));
        Assert.That(restored.ActiveOrders[0].Amount, Is.EqualTo(2));
        UnityEngine.Object.DestroyImmediate(restoredRoot);
    }

    [Test]
    public void ApplyDefaultGameSaveData_ClearsRemoteSaleOrders()
    {
        ItemDataSO item = CreateItem("Iron", 100);
        inventory.DepositItemTo(1, item, 1);
        manager.TryCreateItemOrder(1, item, 1);
        SaveManager saveManager = root.AddComponent<SaveManager>();
        MethodInfo resolve = typeof(SaveManager).GetMethod("ResolveReferences",
            BindingFlags.Instance | BindingFlags.NonPublic);
        resolve.Invoke(saveManager, null);
        MethodInfo apply = typeof(SaveManager).GetMethod("ApplySaveData",
            BindingFlags.Instance | BindingFlags.NonPublic);
        apply.Invoke(saveManager, new object[] { new GameSaveData() });
        Assert.That(manager.ActiveOrders, Is.Empty);
    }

    private ItemDataSO CreateItem(string itemName, int basePrice)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = itemName;
        item.itemName = itemName;
        item.basePrice = basePrice;
        assets.Add(item);
        return item;
    }
}
