using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Characterization tests for MerchantInventory (Action 2.4 of the god-object
// refactor plan). The primary goal here is to pin down the exact current
// equipment sell-price formula in GetSellPrice(EquipmentInstance) -- the
// per-EquipmentQuality multiplier table and the per-enhancement-level +0.12f
// scaling -- so that Action 4.3 (moving this formula onto EquipmentInstance
// itself) has a hard regression guard. Basic item-stacking and
// equipment-instance-selling behavior is also covered.
//
// No MarketPriceManager or ProgressionManager is added to the test object
// graph on purpose: GetSellPrice(ItemDataSO) falls back to item.basePrice
// whenever marketPriceManager is null (keeping equipment sell-price math
// fully deterministic and controlled by the test), and the
// progressionManager-gated storage-capacity checks in AddItem short-circuit
// to "always allowed" whenever progressionManager is null, while equipment
// instances are always permitted regardless of storage capacity.
public sealed class MerchantInventoryTests
{
    private GameObject root;
    private MerchantInventory inventory;
    private MerchantData merchantData;
    private readonly List<UnityEngine.Object> createdObjects = new List<UnityEngine.Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Merchant Inventory Test");
        inventory = root.AddComponent<MerchantInventory>();
        merchantData = root.AddComponent<MerchantData>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);

        foreach (UnityEngine.Object created in createdObjects)
        {
            if (created != null)
            {
                UnityEngine.Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    // --- GetSellPrice(EquipmentInstance): quality multiplier table ---
    // Formula: Mathf.Max(1, Mathf.RoundToInt(basePrice * qualityMultiplier * (1f + enhancementLevel * 0.12f)))
    // At enhancementLevel 0 the enhancement term is 1.0f, so expected == basePrice * qualityMultiplier.
    //   Poor:      100 * 0.65 * 1.0 = 65.0  -> 65
    //   Normal:    100 * 1.00 * 1.0 = 100.0 -> 100
    //   Fine:      100 * 1.20 * 1.0 = 120.0 -> 120
    //   Rare:      100 * 1.55 * 1.0 = 155.0 -> 155
    //   Legendary: 100 * 2.20 * 1.0 = 220.0 -> 220
    [TestCase(EquipmentQuality.Poor, 65)]
    [TestCase(EquipmentQuality.Normal, 100)]
    [TestCase(EquipmentQuality.Fine, 120)]
    [TestCase(EquipmentQuality.Rare, 155)]
    [TestCase(EquipmentQuality.Legendary, 220)]
    public void GetSellPrice_EquipmentInstance_ForEachQuality_ReturnsExactHandComputedPrice(
        EquipmentQuality quality, int expectedSellPrice)
    {
        ItemDataSO baseItem = CreateItem("Quality Test Sword", basePrice: 100);
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null, baseItem, quality, Array.Empty<EquipmentModifier>(), enhancementLevel: 0);

        int sellPrice = inventory.GetSellPrice(equipment);

        Assert.That(sellPrice, Is.EqualTo(expectedSellPrice));
    }

    // --- GetSellPrice(EquipmentInstance): enhancement-level scaling ---
    // Fixed quality Normal (multiplier 1.0f), basePrice 100.
    // Formula per level: 100 * 1.0 * (1 + level * 0.12)
    //   level 0:  100 * (1 + 0.00) = 100.0 -> 100
    //   level 5:  100 * (1 + 0.60) = 160.0 -> 160
    //   level 10: 100 * (1 + 1.20) = 220.0 -> 220
    [TestCase(0, 100)]
    [TestCase(5, 160)]
    [TestCase(10, 220)]
    public void GetSellPrice_EquipmentInstance_AtVariousEnhancementLevels_ReturnsExactHandComputedPrice(
        int enhancementLevel, int expectedSellPrice)
    {
        ItemDataSO baseItem = CreateItem("Enhancement Test Sword", basePrice: 100);
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null, baseItem, EquipmentQuality.Normal, Array.Empty<EquipmentModifier>(),
            enhancementLevel: enhancementLevel);

        int sellPrice = inventory.GetSellPrice(equipment);

        Assert.That(sellPrice, Is.EqualTo(expectedSellPrice));
    }

    [Test]
    public void GetSellPrice_EquipmentInstance_NullEquipment_ReturnsZero()
    {
        Assert.That(inventory.GetSellPrice((EquipmentInstance)null), Is.EqualTo(0));
    }

    [Test]
    public void GetSellPrice_EquipmentInstance_NullBaseItem_ReturnsZero()
    {
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null, null, EquipmentQuality.Rare, Array.Empty<EquipmentModifier>());

        Assert.That(inventory.GetSellPrice(equipment), Is.EqualTo(0));
    }

    // --- SellEquipmentInstance(EquipmentInstance) ---

    [Test]
    public void SellEquipmentInstance_ValidUnlockedInstance_ReturnsTrue_RemovesIt_AndCreditsExactGold()
    {
        // basePrice 100, Fine (1.2x), enhancement 0 -> 100 * 1.2 = 120.0 -> 120
        ItemDataSO baseItem = CreateItem("Sellable Sword", basePrice: 100);
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null, baseItem, EquipmentQuality.Fine, Array.Empty<EquipmentModifier>(), enhancementLevel: 0);
        inventory.AddEquipmentInstance(equipment);

        int expectedSellPrice = inventory.GetSellPrice(equipment);
        MerchantInventorySale sale = default;
        int saleCount = 0;
        inventory.ItemSold += value =>
        {
            sale = value;
            saleCount++;
        };
        Assert.That(expectedSellPrice, Is.EqualTo(120));
        int goldBefore = merchantData.Gold;

        bool result = inventory.SellEquipmentInstance(equipment);

        Assert.That(result, Is.True);
        CollectionAssert.DoesNotContain(inventory.EquipmentInstances, equipment);
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore + expectedSellPrice));
        Assert.That(saleCount, Is.EqualTo(1));
        Assert.That(sale.Equipment, Is.EqualTo(equipment));
        Assert.That(sale.Item, Is.EqualTo(baseItem));
        Assert.That(sale.Amount, Is.EqualTo(1));
        Assert.That(sale.TotalPrice, Is.EqualTo(expectedSellPrice));
    }

    [Test]
    public void SellEquipmentInstance_LockedInstance_ReturnsFalse_DoesNotRemoveOrChangeGold()
    {
        ItemDataSO baseItem = CreateItem("Locked Sword", basePrice: 100);
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null, baseItem, EquipmentQuality.Fine, Array.Empty<EquipmentModifier>(), enhancementLevel: 0);
        inventory.AddEquipmentInstance(equipment);
        equipment.ToggleLock();
        Assert.That(equipment.IsLocked, Is.True);
        int goldBefore = merchantData.Gold;

        bool result = inventory.SellEquipmentInstance(equipment);

        Assert.That(result, Is.False);
        CollectionAssert.Contains(inventory.EquipmentInstances, equipment);
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
    }

    [Test]
    public void SellEquipmentInstance_NullEquipment_ReturnsFalse()
    {
        int goldBefore = merchantData.Gold;

        bool result = inventory.SellEquipmentInstance(null);

        Assert.That(result, Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
    }

    [Test]
    public void SellEquipmentInstance_InstanceNeverAddedToInventory_ReturnsFalse()
    {
        ItemDataSO baseItem = CreateItem("Unowned Sword", basePrice: 100);
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null, baseItem, EquipmentQuality.Fine, Array.Empty<EquipmentModifier>(), enhancementLevel: 0);
        int goldBefore = merchantData.Gold;

        bool result = inventory.SellEquipmentInstance(equipment);

        Assert.That(result, Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
    }

    // --- Basic item stacking: AddItem / TryRemoveItem / GetItemAmount / HasItem ---

    [Test]
    public void AddItem_CalledTwiceForSameItem_StacksIntoSingleEntry()
    {
        ItemDataSO item = CreateItem("Stackable Ore", basePrice: 10);

        inventory.AddItem(item, 3);
        inventory.AddItem(item, 2);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(5));
        Assert.That(inventory.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public void TryRemoveItem_PartialAmount_LeavesRemainderAndKeepsStack()
    {
        ItemDataSO item = CreateItem("Stackable Ore", basePrice: 10);
        inventory.AddItem(item, 3);
        inventory.AddItem(item, 2); // amount == 5

        bool result = inventory.TryRemoveItem(item, 4);

        Assert.That(result, Is.True);
        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(1));
        Assert.That(inventory.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public void TryRemoveItem_DrainsStackToZero_RemovesEntryFromItems()
    {
        ItemDataSO item = CreateItem("Stackable Ore", basePrice: 10);
        inventory.AddItem(item, 3);
        inventory.AddItem(item, 2); // amount == 5
        inventory.TryRemoveItem(item, 4); // amount == 1

        bool result = inventory.TryRemoveItem(item, 1); // amount == 0

        Assert.That(result, Is.True);
        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(0));
        Assert.That(inventory.Items.Count, Is.EqualTo(0));
        Assert.That(inventory.HasItem(item), Is.False);
    }

    [Test]
    public void TryRemoveItem_MoreThanAvailable_ReturnsFalse_AndDoesNotChangeStoredAmount()
    {
        ItemDataSO item = CreateItem("Stackable Ore", basePrice: 10);
        inventory.AddItem(item, 3);

        bool result = inventory.TryRemoveItem(item, 5);

        Assert.That(result, Is.False);
        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(3));
    }

    [Test]
    public void ItemSold_FiresOnlyAfterASuccessfulItemSale()
    {
        ItemDataSO item = CreateItem("Sale Event Ore", basePrice: 10);
        int saleCount = 0;
        MerchantInventorySale sale = default;
        inventory.ItemSold += value =>
        {
            sale = value;
            saleCount++;
        };

        inventory.AddItem(item, 2);
        inventory.TryRemoveItem(item, 1);
        bool sold = inventory.SellItem(item, 1);

        Assert.That(sold, Is.True);
        Assert.That(saleCount, Is.EqualTo(1));
        Assert.That(sale.Item, Is.EqualTo(item));
        Assert.That(sale.Equipment, Is.Null);
        Assert.That(sale.Amount, Is.EqualTo(1));
        Assert.That(sale.TotalPrice, Is.EqualTo(10));
    }

    [Test]
    public void TryAddItem_WhenStorageHasRoom_AddsEntireAmountAndReturnsTrue()
    {
        root.AddComponent<ProgressionManager>();
        ItemDataSO item = CreateItem("Stored Ore", basePrice: 10);

        bool result = inventory.TryAddItem(item, 4);

        Assert.That(result, Is.True);
        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(4));
        Assert.That(inventory.GetUsedStorageSlots(), Is.EqualTo(4));
    }

    [Test]
    public void TryAddItem_WhenAmountWouldExceedCapacity_ReturnsFalseAndAddsNothing()
    {
        root.AddComponent<ProgressionManager>();
        ItemDataSO existingItem = CreateItem("Existing Ore", basePrice: 10);
        ItemDataSO rejectedItem = CreateItem("Rejected Ore", basePrice: 10);
        Assert.That(inventory.TryAddItem(existingItem, 30), Is.True);

        bool result = inventory.TryAddItem(rejectedItem, 1);

        Assert.That(result, Is.False);
        Assert.That(inventory.GetItemAmount(rejectedItem), Is.Zero);
        Assert.That(inventory.GetUsedStorageSlots(), Is.EqualTo(30));
    }

    [Test]
    public void Equipment_DoesNotConsumeStorageSlots_AndCanBeStoredWhenFull()
    {
        root.AddComponent<ProgressionManager>();
        ItemDataSO material = CreateItem("Full Storage Ore", basePrice: 10);
        ItemDataSO equipmentItem = CreateEquipment("Stored Sword");
        Assert.That(inventory.TryAddItem(material, 30), Is.True);

        for (int index = 0; index < 40; index++)
        {
            inventory.AddEquipmentInstance(
                EquipmentInstance.CreateFixed(equipmentItem));
        }

        Assert.That(inventory.EquipmentInstances.Count, Is.EqualTo(40));
        Assert.That(inventory.GetUsedStorageSlots(), Is.EqualTo(30));
        Assert.That(inventory.TryAddItem(material, 1), Is.False);
    }

    private ItemDataSO CreateItem(string itemName, int basePrice)
    {
        ItemDataSO item = Track(ScriptableObject.CreateInstance<ItemDataSO>());
        item.itemName = itemName;
        item.basePrice = basePrice;
        return item;
    }

    private ItemDataSO CreateEquipment(string itemName)
    {
        ItemDataSO item = CreateItem(itemName, basePrice: 10);
        item.itemType = ItemType.Equipment;
        return item;
    }

    private T Track<T>(T created) where T : UnityEngine.Object
    {
        createdObjects.Add(created);
        return created;
    }
}
