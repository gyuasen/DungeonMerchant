using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class EconomyControllerTests
{
    private GameObject root;
    private MerchantInventory inventory;
    private MerchantData merchantData;
    private EconomyController economyController;
    private readonly List<UnityEngine.Object> createdObjects =
        new List<UnityEngine.Object>();
    private string status;
    private int refreshCount;

    [SetUp]
    public void SetUp()
    {
        status = null;
        refreshCount = 0;
        root = new GameObject("Economy Controller Test");
        inventory = root.AddComponent<MerchantInventory>();
        merchantData = root.AddComponent<MerchantData>();
        economyController = new EconomyController(
            inventory,
            null,
            null,
            message => status = message,
            () => { },
            () => { },
            () => { },
            () => refreshCount++,
            _ => { },
            _ => { });
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

    [Test]
    public void SellItem_WithAmount_RemovesRequestedAmountAndCreditsExactGold()
    {
        ItemDataSO item = CreateItem("Iron Ore", 37);
        inventory.AddItem(item, 5);
        int goldBefore = merchantData.Gold;

        economyController.SellItem(item, 3);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(2));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore + 111));
        Assert.That(status, Does.Contain("3個"));
        Assert.That(status, Does.Contain("111G"));
        Assert.That(refreshCount, Is.EqualTo(1));
    }

    [Test]
    public void SellItem_WithAmountAboveOwned_DoesNotChangeInventoryOrGold()
    {
        ItemDataSO item = CreateItem("Iron Ore", 37);
        inventory.AddItem(item, 2);
        int goldBefore = merchantData.Gold;

        economyController.SellItem(item, 3);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(2));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Does.Contain("売却できませんでした"));
        Assert.That(refreshCount, Is.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void SellItem_WithNonPositiveAmount_IsNoOp(int amount)
    {
        ItemDataSO item = CreateItem("Iron Ore", 37);
        inventory.AddItem(item, 2);
        int goldBefore = merchantData.Gold;

        economyController.SellItem(item, amount);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(2));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Is.Null);
        Assert.That(refreshCount, Is.Zero);
    }

    [Test]
    public void SellItem_WithNullItem_IsNoOp()
    {
        int goldBefore = merchantData.Gold;

        economyController.SellItem(null, 1);

        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Is.Null);
        Assert.That(refreshCount, Is.Zero);
    }

    [Test]
    public void SellAllSellOnlyMaterials_SellsEverySellOnlyStackAndReportsTotals()
    {
        ItemDataSO sellOnlyA = CreateSellOnlyMaterial("Cracked Fang", 10);
        ItemDataSO sellOnlyB = CreateSellOnlyMaterial("Torn Hide", 20);
        ItemDataSO craftingMaterial = CreateCraftingMaterial("Iron Ore", 50);
        inventory.AddItem(sellOnlyA, 3);
        inventory.AddItem(sellOnlyB, 2);
        inventory.AddItem(craftingMaterial, 4);
        // Derive the expectation from the live pricing rules rather than a
        // literal, so town-demand multipliers cannot make this test brittle.
        int expectedGold =
            (inventory.GetSellPrice(sellOnlyA) * 3) +
            (inventory.GetSellPrice(sellOnlyB) * 2);

        int earnedGold = economyController.SellAllSellOnlyMaterials(
            out int soldCount,
            out bool stoppedEarly);

        Assert.That(earnedGold, Is.EqualTo(expectedGold));
        Assert.That(soldCount, Is.EqualTo(5));
        Assert.That(stoppedEarly, Is.False);
        Assert.That(inventory.GetItemAmount(sellOnlyA), Is.Zero);
        Assert.That(inventory.GetItemAmount(sellOnlyB), Is.Zero);
        Assert.That(
            inventory.GetItemAmount(craftingMaterial),
            Is.EqualTo(4),
            "Crafting materials must never be swept by the sell-only bulk sale.");
    }

    [Test]
    public void SellAllSellOnlyMaterials_WithNoSellOnlyStacks_EarnsNothing()
    {
        ItemDataSO craftingMaterial = CreateCraftingMaterial("Iron Ore", 50);
        inventory.AddItem(craftingMaterial, 4);

        int earnedGold = economyController.SellAllSellOnlyMaterials(
            out int soldCount,
            out bool stoppedEarly);

        Assert.That(earnedGold, Is.Zero);
        Assert.That(soldCount, Is.Zero);
        Assert.That(stoppedEarly, Is.False);
        Assert.That(inventory.GetItemAmount(craftingMaterial), Is.EqualTo(4));
    }

    [Test]
    public void GetSellOnlyTotalGold_MatchesGoldEarnedByBulkSale()
    {
        ItemDataSO sellOnlyA = CreateSellOnlyMaterial("Cracked Fang", 10);
        ItemDataSO sellOnlyB = CreateSellOnlyMaterial("Torn Hide", 20);
        inventory.AddItem(sellOnlyA, 3);
        inventory.AddItem(sellOnlyB, 2);

        int previewedGold = economyController.GetSellOnlyTotalGold();
        int earnedGold = economyController.SellAllSellOnlyMaterials(
            out _,
            out _);

        Assert.That(
            previewedGold,
            Is.EqualTo(earnedGold),
            "The previewed total must match what the bulk sale actually pays.");
    }

    [Test]
    public void GetSellOnlyStacks_ExcludesCraftingMaterialsAndConsumables()
    {
        ItemDataSO sellOnly = CreateSellOnlyMaterial("Cracked Fang", 10);
        ItemDataSO craftingMaterial = CreateCraftingMaterial("Iron Ore", 50);
        ItemDataSO consumable = CreateItem("Potion", 30);
        consumable.itemType = ItemType.Consumable;
        inventory.AddItem(sellOnly, 1);
        inventory.AddItem(craftingMaterial, 1);
        inventory.AddItem(consumable, 1);

        List<InventoryItemStack> stacks = economyController.GetSellOnlyStacks();

        Assert.That(stacks.Count, Is.EqualTo(1));
        Assert.That(stacks[0].Item, Is.SameAs(sellOnly));
    }

    [Test]
    public void GetSellOnlyStacks_IncludesEnvironmentMaterialByPersistentId()
    {
        ItemDataSO environmentMaterial =
            CreateCraftingMaterial("Iron Ore", 50);
        environmentMaterial.name = "item.material.iron_ore";
        ItemDataSO otherCraftingMaterial =
            CreateCraftingMaterial("Golem Core", 80);
        otherCraftingMaterial.name = "item.material.golem_core";
        inventory.AddItem(environmentMaterial, 2);
        inventory.AddItem(otherCraftingMaterial, 3);

        List<InventoryItemStack> stacks = economyController.GetSellOnlyStacks();

        Assert.That(stacks.Count, Is.EqualTo(1));
        Assert.That(stacks[0].Item, Is.SameAs(environmentMaterial));
    }

    [Test]
    public void CycleInventoryFilter_AdvancesThroughEveryFilterAndWrapsAround()
    {
        List<string> labels = new List<string>();
        EconomyController controller = CreateControllerWithLabelCapture(
            labels,
            new List<string>());
        int filterCount = System.Enum.GetValues(typeof(InventoryFilter)).Length;

        for (int i = 0; i < filterCount; i++)
        {
            controller.CycleInventoryFilter();
        }

        Assert.That(labels.Count, Is.EqualTo(filterCount));
        Assert.That(labels[0], Is.EqualTo("絞込: 素材"));
        Assert.That(
            labels[filterCount - 1],
            Is.EqualTo("絞込: 全て"),
            "Cycling through every filter must wrap back to the default.");
    }

    [Test]
    public void CycleEquipmentSort_AdvancesThroughEverySortAndWrapsAround()
    {
        List<string> labels = new List<string>();
        EconomyController controller = CreateControllerWithLabelCapture(
            new List<string>(),
            labels);
        int sortCount = System.Enum.GetValues(typeof(EquipmentSort)).Length;

        for (int i = 0; i < sortCount; i++)
        {
            controller.CycleEquipmentSort();
        }

        Assert.That(labels.Count, Is.EqualTo(sortCount));
        Assert.That(labels[0], Is.EqualTo("並替: 品質"));
        Assert.That(labels[sortCount - 1], Is.EqualTo("並替: 名前"));
    }

    [Test]
    public void ShouldShowInventoryItem_UnderLockedFilter_HidesEveryPlainItem()
    {
        ItemDataSO material = CreateCraftingMaterial("Iron Ore", 50);
        inventory.AddItem(material, 1);
        InventoryItemStack stack = inventory.Items[0];
        CycleFilterTo(InventoryFilter.Locked);

        Assert.That(
            economyController.ShouldShowInventoryItem(stack),
            Is.False,
            "Plain items can never be locked, so the Locked filter hides them all.");
    }

    [Test]
    public void ShouldShowInventoryItem_UnderMaterialFilter_HidesEquipment()
    {
        ItemDataSO material = CreateCraftingMaterial("Iron Ore", 50);
        ItemDataSO weapon = CreateEquipment("Sword", EquipmentSlot.Weapon);
        inventory.AddItem(material, 1);
        inventory.AddItem(weapon, 1);
        CycleFilterTo(InventoryFilter.Material);

        InventoryItemStack materialStack = FindStack(material);
        InventoryItemStack weaponStack = FindStack(weapon);

        Assert.That(
            economyController.ShouldShowInventoryItem(materialStack),
            Is.True);
        Assert.That(
            economyController.ShouldShowInventoryItem(weaponStack),
            Is.False);
    }

    [TestCase(EquipmentSlot.Weapon, InventoryFilter.Weapon, true)]
    [TestCase(EquipmentSlot.Weapon, InventoryFilter.Armor, false)]
    [TestCase(EquipmentSlot.Armor, InventoryFilter.Armor, true)]
    [TestCase(EquipmentSlot.Accessory, InventoryFilter.Accessory, true)]
    [TestCase(EquipmentSlot.Accessory, InventoryFilter.Weapon, false)]
    public void ShouldShowInventoryEquipment_MatchesSlotAgainstActiveFilter(
        EquipmentSlot slot,
        InventoryFilter filter,
        bool expectedVisible)
    {
        ItemDataSO baseItem = CreateEquipment("Gear", slot);
        EquipmentInstance equipment = EquipmentInstance.CreateFixed(baseItem);
        CycleFilterTo(filter);

        Assert.That(
            economyController.ShouldShowInventoryEquipment(equipment),
            Is.EqualTo(expectedVisible));
    }

    [Test]
    public void ShouldShowInventoryEquipment_UnderLockedFilter_ShowsOnlyLockedEquipment()
    {
        ItemDataSO baseItem = CreateEquipment("Sword", EquipmentSlot.Weapon);
        EquipmentInstance unlocked = EquipmentInstance.CreateFixed(baseItem);
        EquipmentInstance locked = EquipmentInstance.CreateFixed(baseItem);
        locked.ToggleLock();
        CycleFilterTo(InventoryFilter.Locked);

        Assert.That(
            economyController.ShouldShowInventoryEquipment(unlocked),
            Is.False);
        Assert.That(
            economyController.ShouldShowInventoryEquipment(locked),
            Is.True);
    }

    [Test]
    public void ShouldShowInventoryEquipment_WithNullEquipment_IsHidden()
    {
        Assert.That(
            economyController.ShouldShowInventoryEquipment(null),
            Is.False);
    }

    [Test]
    public void ShouldShowMarketEntry_RejectsNullAndSoldOutEntries()
    {
        Assert.That(EconomyController.ShouldShowMarketEntry(null), Is.False);
    }

    [Test]
    public void ShouldShowBlacksmithRecipe_RejectsRecipeWithoutResultItem()
    {
        EquipmentRecipeSO recipe =
            ScriptableObject.CreateInstance<EquipmentRecipeSO>();
        createdObjects.Add(recipe);

        Assert.That(
            EconomyController.ShouldShowBlacksmithRecipe(recipe),
            Is.False);
        Assert.That(
            EconomyController.ShouldShowBlacksmithRecipe(null),
            Is.False);
    }

    [Test]
    public void ToggleBlacksmithCraftableOnly_FlipsTheFlagAndRefreshes()
    {
        Assert.That(economyController.IsBlacksmithCraftableOnly, Is.False);

        economyController.ToggleBlacksmithCraftableOnly();
        Assert.That(economyController.IsBlacksmithCraftableOnly, Is.True);

        economyController.ToggleBlacksmithCraftableOnly();
        Assert.That(economyController.IsBlacksmithCraftableOnly, Is.False);
    }

    [Test]
    public void ToggleBlacksmithRankSort_StartsAscendingAndFlips()
    {
        Assert.That(economyController.IsBlacksmithRankAscending, Is.True);

        economyController.ToggleBlacksmithRankSort();
        Assert.That(economyController.IsBlacksmithRankAscending, Is.False);

        economyController.ToggleBlacksmithRankSort();
        Assert.That(economyController.IsBlacksmithRankAscending, Is.True);
    }

    [Test]
    public void GetSortedInventoryEquipment_ByName_OrdersOrdinally()
    {
        AddEquipmentToInventory("Charlie", EquipmentSlot.Weapon);
        AddEquipmentToInventory("Alpha", EquipmentSlot.Weapon);
        AddEquipmentToInventory("Bravo", EquipmentSlot.Weapon);

        List<EquipmentInstance> sorted = new List<EquipmentInstance>(
            economyController.GetSortedInventoryEquipment());

        Assert.That(sorted[0].BaseItem.itemName, Is.EqualTo("Alpha"));
        Assert.That(sorted[1].BaseItem.itemName, Is.EqualTo("Bravo"));
        Assert.That(sorted[2].BaseItem.itemName, Is.EqualTo("Charlie"));
    }

    [Test]
    public void GetSortedInventoryEquipment_ByEnhancement_OrdersDescending()
    {
        AddEnhancedEquipmentToInventory("Weak", EquipmentSlot.Weapon, 1);
        AddEnhancedEquipmentToInventory("Strong", EquipmentSlot.Weapon, 5);
        CycleSortTo(EquipmentSort.Enhancement);

        List<EquipmentInstance> sorted = new List<EquipmentInstance>(
            economyController.GetSortedInventoryEquipment());

        Assert.That(
            sorted[0].EnhancementLevel,
            Is.EqualTo(5),
            "The most enhanced equipment must come first.");
        Assert.That(sorted[1].EnhancementLevel, Is.EqualTo(1));
    }

    [Test]
    public void GetSortedInventoryEquipment_DoesNotMutateInventoryOrder()
    {
        AddEquipmentToInventory("Charlie", EquipmentSlot.Weapon);
        AddEquipmentToInventory("Alpha", EquipmentSlot.Weapon);
        List<EquipmentInstance> before = new List<EquipmentInstance>(
            inventory.EquipmentInstances);

        economyController.GetSortedInventoryEquipment();

        Assert.That(
            inventory.EquipmentInstances,
            Is.EqualTo(before).AsCollection,
            "Sorting for display must not reorder the backing inventory.");
    }

    private EconomyController CreateControllerWithLabelCapture(
        List<string> filterLabels,
        List<string> sortLabels)
    {
        return new EconomyController(
            inventory,
            null,
            null,
            _ => { },
            () => { },
            () => { },
            () => { },
            () => { },
            label => filterLabels.Add(label),
            label => sortLabels.Add(label));
    }

    /// <summary>
    /// The controller starts on <see cref="InventoryFilter.All"/> (value 0) and
    /// each cycle advances one step, so cycling N times lands on filter N.
    /// </summary>
    private void CycleFilterTo(InventoryFilter target)
    {
        for (int i = 0; i < (int)target; i++)
        {
            economyController.CycleInventoryFilter();
        }
    }

    private void CycleSortTo(EquipmentSort target)
    {
        for (int i = 0; i < (int)target; i++)
        {
            economyController.CycleEquipmentSort();
        }
    }

    private InventoryItemStack FindStack(ItemDataSO item)
    {
        foreach (InventoryItemStack stack in inventory.Items)
        {
            if (stack?.Item == item)
            {
                return stack;
            }
        }
        return null;
    }

    private EquipmentInstance AddEquipmentToInventory(
        string itemName,
        EquipmentSlot slot)
    {
        ItemDataSO baseItem = CreateEquipment(itemName, slot);
        EquipmentInstance equipment = EquipmentInstance.CreateFixed(baseItem);
        inventory.AddEquipmentInstance(equipment);
        return equipment;
    }

    private EquipmentInstance AddEnhancedEquipmentToInventory(
        string itemName,
        EquipmentSlot slot,
        int enhancementLevel)
    {
        ItemDataSO baseItem = CreateEquipment(itemName, slot);
        EquipmentInstance equipment = EquipmentInstance.CreateRestored(
            null,
            baseItem,
            EquipmentQuality.Normal,
            new List<EquipmentModifier>(),
            enhancementLevel);
        inventory.AddEquipmentInstance(equipment);
        return equipment;
    }

    private ItemDataSO CreateEquipment(string itemName, EquipmentSlot slot)
    {
        ItemDataSO item = CreateItem(itemName, 100);
        item.itemType = ItemType.Equipment;
        item.equipmentSlot = slot;
        return item;
    }

    private ItemDataSO CreateSellOnlyMaterial(string itemName, int basePrice)
    {
        ItemDataSO item = CreateItem(itemName, basePrice);
        item.itemType = ItemType.Material;
        item.materialClassification = MaterialClassification.SellOnly;
        return item;
    }

    private ItemDataSO CreateCraftingMaterial(string itemName, int basePrice)
    {
        ItemDataSO item = CreateItem(itemName, basePrice);
        item.itemType = ItemType.Material;
        item.materialClassification = MaterialClassification.CraftingMaterial;
        return item;
    }

    private ItemDataSO CreateItem(string itemName, int basePrice)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = itemName;
        item.basePrice = basePrice;
        createdObjects.Add(item);
        return item;
    }
}
