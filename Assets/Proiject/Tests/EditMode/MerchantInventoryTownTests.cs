using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class MerchantInventoryTownTests
{
    private readonly List<Object> createdObjects = new List<Object>();
    private GameObject root;
    private MerchantInventory inventory;
    private TownProgressState towns;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Town Inventory Test");
        root.AddComponent<MerchantData>();
        inventory = root.AddComponent<MerchantInventory>();
        towns = root.AddComponent<TownProgressState>();
        towns.Initialize(2, new[] { 2, 1 });
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        foreach (Object created in createdObjects)
        {
            Object.DestroyImmediate(created);
        }
    }

    [Test]
    public void CurrentTownFacade_KeepsTownInventoriesSeparate()
    {
        ItemDataSO item = CreateItem("Town Ore");
        inventory.AddItem(item, 3);

        towns.RestoreTownProgress(1, new[] { 2, 1 });

        Assert.That(inventory.GetItemAmount(item), Is.Zero);
        Assert.That(inventory.GetItemAmountIn(2, item), Is.EqualTo(3));
    }

    [Test]
    public void CapacityAndDirectDeposit_AreIndependentPerTown()
    {
        root.AddComponent<ProgressionManager>();
        ItemDataSO item = CreateItem("Capacity Ore");
        Assert.That(inventory.TryAddItem(item, 30), Is.True);
        Assert.That(inventory.TryAddItem(item, 1), Is.False);

        towns.Initialize(1, new[] { 2, 1 });
        Assert.That(inventory.TryAddItem(item, 30), Is.True);
        Assert.That(inventory.DepositItemTo(1, item, 5), Is.True);
        Assert.That(inventory.GetUsedStorageSlotsIn(1), Is.EqualTo(35));
        Assert.That(inventory.GetUsedStorageSlotsIn(2), Is.EqualTo(30));
    }

    [Test]
    public void SaveRoundTrip_PreservesTownPlacement()
    {
        ItemDataSO item = CreateItem("Saved Ore");
        inventory.AddItem(item, 2);
        inventory.DepositItemTo(1, item, 4);
        SaveManager saveManager = root.AddComponent<SaveManager>();
        Invoke<object>(saveManager, "ResolveReferences");
        GameSaveData saved = Invoke<GameSaveData>(saveManager, "CreateSaveData");

        GameObject restoredRoot = new GameObject("Restored Town Inventory Test");
        restoredRoot.AddComponent<MerchantData>();
        MerchantInventory restoredInventory =
            restoredRoot.AddComponent<MerchantInventory>();
        restoredRoot.AddComponent<TownProgressState>();
        SaveManager restoredSaveManager = restoredRoot.AddComponent<SaveManager>();
        Invoke<object>(restoredSaveManager, "ResolveReferences");
        Invoke<object>(restoredSaveManager, "ApplySaveData", saved);

        Assert.That(restoredInventory.GetItemAmountIn(2, item), Is.EqualTo(2));
        Assert.That(restoredInventory.GetItemAmountIn(1, item), Is.EqualTo(4));
        Object.DestroyImmediate(restoredRoot);
    }

    private ItemDataSO CreateItem(string itemName)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = itemName;
        createdObjects.Add(item);
        return item;
    }

    private static T Invoke<T>(object target, string methodName, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        return (T)method.Invoke(target, args);
    }
}
