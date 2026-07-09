using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Characterization tests for MarketStockManager (Action 2.3 of the
// god-object refactor plan). GetBuyMultiplier(ItemDataSO, int) is the only
// fully synchronous, side-effect-free public surface worth pinning down here:
// it is a pure function of CurrentDay, the private currentTownIndex field,
// the salt argument, and the injected ItemDataSO's itemType/rarity/itemName,
// combined via a private unchecked-int hash (GetStableHash) and lerped
// between minimumBuyMultiplier/maximumBuyMultiplier. These tests do not
// assert anything about Stock's contents: OnEnable() (fired the instant
// AddComponent<MarketStockManager>() runs) calls PopulatePurchasableItemsIfNeeded()
// and GenerateDailyStock(), which depend on this project's real
// GameAssetRepository/Resources assets (or a runtime fallback item) and are
// out of scope for a GetBuyMultiplier-focused test.
public sealed class MarketStockManagerTests
{
    private GameObject root;
    private MarketStockManager marketStockManager;
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Market Stock Manager Test");
        marketStockManager = root.AddComponent<MarketStockManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);

        foreach (Object created in createdObjects)
        {
            if (created != null)
            {
                Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    // --- GetBuyMultiplier(ItemDataSO, int) ---

    [Test]
    public void GetBuyMultiplier_CalledTwiceWithSameInputs_ReturnsIdenticalValue()
    {
        // Pure integer-hash-derived function: no accumulated floating point
        // drift between calls, so exact equality is safe here.
        ItemDataSO item = CreateItem("Determinism Sword", ItemType.Equipment, ItemRarity.Rare);

        float first = marketStockManager.GetBuyMultiplier(item, salt: 5);
        float second = marketStockManager.GetBuyMultiplier(item, salt: 5);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void GetBuyMultiplier_NullItem_ReturnsOne()
    {
        Assert.That(marketStockManager.GetBuyMultiplier(null, salt: 3), Is.EqualTo(1f));
    }

    [Test]
    public void GetBuyMultiplier_DifferentSalts_ProduceNotAllIdenticalValues()
    {
        // Demonstrates the salt parameter is actually mixed into the hash --
        // not a proof that every salt yields a distinct value, just that
        // varying it changes the result for at least one pair in a small
        // sample.
        ItemDataSO item = CreateItem("Salted Bow", ItemType.Equipment, ItemRarity.Uncommon);
        HashSet<float> results = new HashSet<float>();

        for (int salt = 0; salt < 6; salt++)
        {
            results.Add(marketStockManager.GetBuyMultiplier(item, salt));
        }

        Assert.That(results.Count, Is.GreaterThan(1));
    }

    [TestCase("Iron Dagger", ItemType.Equipment, ItemRarity.Common)]
    [TestCase("Iron Dagger", ItemType.Equipment, ItemRarity.Epic)]
    [TestCase("Phoenix Feather", ItemType.Relic, ItemRarity.Rare)]
    [TestCase("Healing Draught", ItemType.Consumable, ItemRarity.Uncommon)]
    [TestCase("Raw Ore", ItemType.Material, ItemRarity.Common)]
    [TestCase("", ItemType.Equipment, ItemRarity.Common)]
    public void GetBuyMultiplier_ForVariousItems_StaysWithinConfiguredRange(
        string itemName, ItemType itemType, ItemRarity rarity)
    {
        ItemDataSO item = CreateItem(itemName, itemType, rarity);
        if (string.IsNullOrEmpty(itemName))
        {
            // itemName left blank on purpose to exercise the
            // "fall back to item.name" branch of GetStableHash's key.
            item.name = "FallbackAssetName";
        }

        float minimumBuyMultiplier = GetPrivateFloat(marketStockManager, "minimumBuyMultiplier");
        float maximumBuyMultiplier = GetPrivateFloat(marketStockManager, "maximumBuyMultiplier");

        for (int salt = 0; salt < 4; salt++)
        {
            float multiplier = marketStockManager.GetBuyMultiplier(item, salt);

            Assert.That(multiplier, Is.GreaterThanOrEqualTo(minimumBuyMultiplier));
            Assert.That(multiplier, Is.LessThanOrEqualTo(maximumBuyMultiplier));
        }
    }

    [Test]
    public void MinimumAndMaximumBuyMultiplier_DefaultTo_0_65And1_15()
    {
        // Pins down the defaults this suite's range assertions rely on, so a
        // future change to these [SerializeField] defaults is caught here
        // explicitly rather than only surfacing as a range-test failure.
        float minimumBuyMultiplier = GetPrivateFloat(marketStockManager, "minimumBuyMultiplier");
        float maximumBuyMultiplier = GetPrivateFloat(marketStockManager, "maximumBuyMultiplier");

        Assert.That(minimumBuyMultiplier, Is.EqualTo(0.65f));
        Assert.That(maximumBuyMultiplier, Is.EqualTo(1.15f));
    }

    private ItemDataSO CreateItem(string itemName, ItemType itemType, ItemRarity rarity)
    {
        ItemDataSO item = Track(ScriptableObject.CreateInstance<ItemDataSO>());
        item.itemName = itemName;
        item.itemType = itemType;
        item.rarity = rarity;
        return item;
    }

    private static float GetPrivateFloat(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        return (float)field.GetValue(target);
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
