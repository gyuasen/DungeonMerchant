using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class CharacterEquipmentControllerTests
{
    private GameObject root;
    private MerchantData merchantData;
    private MerchantInventory inventory;
    private MercenaryHireManager hireManager;
    private BattleManager battleManager;
    private EconomyController economyController;
    private FakeEquipmentDetailView detailView;
    private CharacterEquipmentController controller;
    private readonly List<UnityEngine.Object> createdObjects =
        new List<UnityEngine.Object>();
    private string status;
    private int refreshCompanyCount;
    private int refreshPartyCount;
    private int refreshInventoryCount;
    private int refreshUICount;
    private int saveEquipmentCount;
    private int saveGameCount;
    private MercenaryInstance shownMercenary;

    [SetUp]
    public void SetUp()
    {
        status = null;
        refreshCompanyCount = 0;
        refreshPartyCount = 0;
        refreshInventoryCount = 0;
        refreshUICount = 0;
        saveEquipmentCount = 0;
        saveGameCount = 0;
        shownMercenary = null;

        root = new GameObject("Character Equipment Controller Test");
        merchantData = root.AddComponent<MerchantData>();
        inventory = root.AddComponent<MerchantInventory>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        battleManager = root.AddComponent<BattleManager>();
        economyController = new EconomyController(
            inventory,
            null,
            null,
            message => status = message,
            () => { },
            () => { },
            () => { },
            () => { },
            _ => { },
            _ => { });
        detailView = new FakeEquipmentDetailView();
        controller = new CharacterEquipmentController(
            merchantData,
            inventory,
            hireManager,
            battleManager,
            economyController,
            detailView,
            message => status = message,
            (title, body) => { },
            mercenary => shownMercenary = mercenary,
            () => refreshCompanyCount++,
            () => refreshPartyCount++,
            () => refreshInventoryCount++,
            () => refreshUICount++,
            () => saveEquipmentCount++,
            () => saveGameCount++);
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
    public void EquipSelectedEquipment_WithInstance_MovesItOutOfStorageAndOntoTheMercenary()
    {
        MercenaryInstance mercenary = SelectMercenary();
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);

        controller.EquipSelectedEquipment(sword);

        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Weapon),
            Is.SameAs(sword));
        Assert.That(
            inventory.EquipmentInstances,
            Has.No.Member(sword),
            "Equipped gear must leave the storage list.");
        Assert.That(refreshCompanyCount, Is.EqualTo(1));
        Assert.That(refreshPartyCount, Is.EqualTo(1));
        Assert.That(saveEquipmentCount, Is.EqualTo(1));
        Assert.That(shownMercenary, Is.SameAs(mercenary));
    }

    [Test]
    public void EquipSelectedEquipment_WithInstance_ReturnsThePreviousInstanceToStorage()
    {
        MercenaryInstance mercenary = SelectMercenary();
        EquipmentInstance oldSword = AddEquipmentToInventory(
            "Old Sword",
            EquipmentSlot.Weapon);
        EquipmentInstance newSword = AddEquipmentToInventory(
            "New Sword",
            EquipmentSlot.Weapon);
        controller.EquipSelectedEquipment(oldSword);

        controller.EquipSelectedEquipment(newSword);

        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Weapon),
            Is.SameAs(newSword));
        Assert.That(
            inventory.EquipmentInstances,
            Does.Contain(oldSword),
            "Swapping gear must hand the replaced piece back to storage.");
        Assert.That(inventory.EquipmentInstances, Has.No.Member(newSword));
    }

    [Test]
    public void EquipSelectedEquipment_WithWrongClassRequirement_LeavesStorageAndSlotUntouched()
    {
        SelectMercenary();
        ItemDataSO baseItem = CreateEquipment("Staff", EquipmentSlot.Weapon);
        // The selected mercenary is a Warrior, so a Mage-only weapon must be
        // rejected by MercenaryInstance.EquipEquipment.
        baseItem.allClassesCanEquip = false;
        baseItem.requiredClass = MercenaryClass.Mage;
        EquipmentInstance staff = EquipmentInstance.CreateFixed(baseItem);
        inventory.AddEquipmentInstance(staff);

        controller.EquipSelectedEquipment(staff);

        Assert.That(
            controller.SelectedDetailMercenary.GetEquippedInstance(
                EquipmentSlot.Weapon),
            Is.Null);
        Assert.That(
            inventory.EquipmentInstances,
            Does.Contain(staff),
            "A rejected equip must not consume the item from storage.");
        Assert.That(saveEquipmentCount, Is.Zero);
    }

    [Test]
    public void EquipSelectedEquipment_WithoutSelectedMercenary_IsNoOp()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);

        controller.EquipSelectedEquipment(sword);

        Assert.That(inventory.EquipmentInstances, Does.Contain(sword));
        Assert.That(status, Is.Null);
        Assert.That(saveEquipmentCount, Is.Zero);
    }

    [Test]
    public void EquipSelectedEquipment_WithPlainItem_RequiresItToBeInStorage()
    {
        SelectMercenary();
        ItemDataSO sword = CreateEquipment("Sword", EquipmentSlot.Weapon);

        controller.EquipSelectedEquipment(sword);

        Assert.That(
            controller.SelectedDetailMercenary.GetEquippedItem(
                EquipmentSlot.Weapon),
            Is.Null,
            "Equipment the merchant does not own can never be equipped.");
        Assert.That(saveEquipmentCount, Is.Zero);
    }

    [Test]
    public void EquipSelectedEquipment_UsesTheSlotDeclaredByTheBaseItem()
    {
        MercenaryInstance mercenary = SelectMercenary();
        EquipmentInstance armor = AddEquipmentToInventory(
            "Plate",
            EquipmentSlot.Armor);
        EquipmentInstance accessory = AddEquipmentToInventory(
            "Ring",
            EquipmentSlot.Accessory);

        controller.EquipSelectedEquipment(armor);
        controller.EquipSelectedEquipment(accessory);

        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Armor),
            Is.SameAs(armor));
        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Accessory),
            Is.SameAs(accessory));
        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Weapon),
            Is.Null,
            "Filling the armor and accessory slots must not touch the weapon.");
    }

    [Test]
    public void UnequipSelectedEquipment_ReturnsTheInstanceToStorageAndClearsTheSlot()
    {
        MercenaryInstance mercenary = SelectMercenary();
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        controller.EquipSelectedEquipment(sword);

        controller.UnequipSelectedEquipment(EquipmentSlot.Weapon);

        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Weapon),
            Is.Null);
        Assert.That(inventory.EquipmentInstances, Does.Contain(sword));
        Assert.That(status, Does.Contain("解除"));
        Assert.That(saveEquipmentCount, Is.EqualTo(2));
    }

    [Test]
    public void UnequipSelectedEquipment_WithEmptySlot_IsNoOp()
    {
        SelectMercenary();

        controller.UnequipSelectedEquipment(EquipmentSlot.Weapon);

        Assert.That(status, Is.Null);
        Assert.That(saveEquipmentCount, Is.Zero);
        Assert.That(refreshCompanyCount, Is.Zero);
    }

    [Test]
    public void LoadConsumable_MovesOnePotionFromStorageIntoTheSlot()
    {
        MercenaryInstance mercenary = SelectMercenary();
        ItemDataSO potion = CreateConsumable("Potion");
        inventory.AddItem(potion, 2);

        controller.LoadConsumable(0, potion);

        Assert.That(mercenary.ConsumableSlots[0].Item, Is.SameAs(potion));
        Assert.That(mercenary.ConsumableSlots[0].Count, Is.EqualTo(1));
        Assert.That(
            inventory.GetItemAmount(potion),
            Is.EqualTo(1),
            "Loading must consume exactly one unit from storage.");
        Assert.That(saveEquipmentCount, Is.EqualTo(1));
    }

    [Test]
    public void LoadConsumable_WithNonConsumableItem_IsNoOp()
    {
        MercenaryInstance mercenary = SelectMercenary();
        ItemDataSO sword = CreateEquipment("Sword", EquipmentSlot.Weapon);
        inventory.AddItem(sword);

        controller.LoadConsumable(0, sword);

        Assert.That(mercenary.ConsumableSlots[0].Item, Is.Null);
        Assert.That(inventory.GetItemAmount(sword), Is.EqualTo(1));
        Assert.That(saveEquipmentCount, Is.Zero);
    }

    [Test]
    public void UnloadConsumable_ReturnsTheWholeStackToStorageAndClearsTheSlot()
    {
        MercenaryInstance mercenary = SelectMercenary();
        ItemDataSO potion = CreateConsumable("Potion");
        inventory.AddItem(potion, 3);
        controller.LoadConsumable(0, potion);
        controller.LoadConsumable(0, potion);

        controller.UnloadConsumable(0);

        Assert.That(mercenary.ConsumableSlots[0].Item, Is.Null);
        Assert.That(mercenary.ConsumableSlots[0].Count, Is.Zero);
        Assert.That(
            inventory.GetItemAmount(potion),
            Is.EqualTo(3),
            "Every loaded unit must come back to storage.");
        Assert.That(status, Does.Contain("x2"));
    }

    [Test]
    public void UnloadConsumable_WithEmptySlot_IsNoOp()
    {
        SelectMercenary();

        controller.UnloadConsumable(0);

        Assert.That(status, Is.Null);
        Assert.That(refreshInventoryCount, Is.Zero);
        Assert.That(saveEquipmentCount, Is.Zero);
    }

    [Test]
    public void ToggleSelectedEquipmentLock_FlipsTheLockAndReportsBothDirections()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        controller.SelectedEquipmentDetail = sword;

        controller.ToggleSelectedEquipmentLock();
        Assert.That(sword.IsLocked, Is.True);
        Assert.That(status, Does.Contain("ロックしました"));

        controller.ToggleSelectedEquipmentLock();
        Assert.That(sword.IsLocked, Is.False);
        Assert.That(status, Does.Contain("ロックを解除"));
    }

    [Test]
    public void SellSelectedEquipment_RemovesItFromStorageAndCreditsTheQuotedPrice()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        controller.SelectedEquipmentDetail = sword;
        // Derive the expectation from the live pricing rules rather than a
        // literal, so town-demand multipliers cannot make this test brittle.
        int expectedPrice = inventory.GetSellPrice(sword);
        int goldBefore = merchantData.Gold;

        controller.SellSelectedEquipment();

        Assert.That(inventory.EquipmentInstances, Has.No.Member(sword));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore + expectedPrice));
        Assert.That(detailView.HideOverlayCount, Is.EqualTo(1));
    }

    [Test]
    public void SellSelectedEquipment_WhenLocked_KeepsTheEquipmentAndPaysNothing()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        sword.ToggleLock();
        controller.SelectedEquipmentDetail = sword;
        int goldBefore = merchantData.Gold;

        controller.SellSelectedEquipment();

        Assert.That(
            inventory.EquipmentInstances,
            Does.Contain(sword),
            "Locked equipment must survive a sell attempt.");
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Does.Contain("売却できませんでした"));
    }

    [Test]
    public void SellSelectedEquipment_ForEquipmentNotInStorage_IsNoOp()
    {
        MercenaryInstance mercenary = SelectMercenary();
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        controller.EquipSelectedEquipment(sword);
        controller.SelectedEquipmentDetail = sword;
        int goldBefore = merchantData.Gold;

        controller.SellSelectedEquipment();

        Assert.That(
            mercenary.GetEquippedInstance(EquipmentSlot.Weapon),
            Is.SameAs(sword),
            "Worn equipment is not on the shelf and must not be sellable.");
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(detailView.HideOverlayCount, Is.Zero);
    }

    [Test]
    public void EnhanceSelectedEquipment_WithoutEnhancementMaterials_ReportsShortage()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        controller.SelectedEquipmentDetail = sword;
        int goldBefore = merchantData.Gold;

        controller.EnhanceSelectedEquipment();

        Assert.That(status, Does.Contain("強化鉱石が不足"));
        Assert.That(
            sword.EnhancementLevel,
            Is.Zero,
            "A failed material check must not raise the enhancement level.");
        Assert.That(
            merchantData.Gold,
            Is.EqualTo(goldBefore),
            "Gold is only spent after the material check passes.");
        Assert.That(refreshInventoryCount, Is.EqualTo(1));
        Assert.That(refreshUICount, Is.EqualTo(1));
        Assert.That(saveEquipmentCount, Is.EqualTo(1));
    }

    [Test]
    public void EnhanceSelectedEquipment_WithoutSelection_IsNoOp()
    {
        controller.EnhanceSelectedEquipment();

        Assert.That(status, Is.Null);
        Assert.That(refreshInventoryCount, Is.Zero);
        Assert.That(saveEquipmentCount, Is.Zero);
    }

    [Test]
    public void EnhancementCost_GrowsWithTheEnhancementLevel()
    {
        ItemDataSO baseItem = CreateEquipment("Sword", EquipmentSlot.Weapon);
        EquipmentInstance plain = EquipmentInstance.CreateFixed(baseItem);
        EquipmentInstance enhanced = EquipmentInstance.CreateRestored(
            null,
            baseItem,
            EquipmentQuality.Normal,
            new List<EquipmentModifier>(),
            5);

        Assert.That(
            enhanced.GetEnhancementCost(),
            Is.GreaterThan(plain.GetEnhancementCost()),
            "Each enhancement step must cost more than the previous one.");
        Assert.That(
            enhanced.GetEnhancementMaterialAmount(),
            Is.GreaterThan(plain.GetEnhancementMaterialAmount()));
    }

    [Test]
    public void ShowEquipmentDetails_WithoutOverlay_DoesNotSelectOrDraw()
    {
        detailView.HasOverlay = false;
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);

        controller.ShowEquipmentDetails(sword);

        Assert.That(controller.SelectedEquipmentDetail, Is.Null);
        Assert.That(detailView.ShowOverlayCount, Is.Zero);
    }

    [Test]
    public void ShowEquipmentDetails_ForStoredEquipment_EnablesSellingAtTheQuotedPrice()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        int expectedPrice = inventory.GetSellPrice(sword);

        controller.ShowEquipmentDetails(sword);

        Assert.That(controller.SelectedEquipmentDetail, Is.SameAs(sword));
        Assert.That(detailView.SellInteractable, Is.True);
        Assert.That(detailView.SellLabel, Is.EqualTo($"売却 {expectedPrice}G"));
        Assert.That(detailView.LockLabel, Is.EqualTo("ロック"));
        Assert.That(detailView.ShowOverlayCount, Is.EqualTo(1));
    }

    [Test]
    public void ShowEquipmentDetails_ForLockedEquipment_DisablesSellingAndOffersUnlock()
    {
        EquipmentInstance sword = AddEquipmentToInventory(
            "Sword",
            EquipmentSlot.Weapon);
        sword.ToggleLock();

        controller.ShowEquipmentDetails(sword);

        Assert.That(detailView.SellInteractable, Is.False);
        Assert.That(detailView.LockLabel, Is.EqualTo("ロック解除"));
    }

    [Test]
    public void ShowEquipmentDetails_AtMaximumEnhancement_DisablesTheEnhanceButton()
    {
        ItemDataSO baseItem = CreateEquipment("Sword", EquipmentSlot.Weapon);
        EquipmentInstance maxed = EquipmentInstance.CreateRestored(
            null,
            baseItem,
            EquipmentQuality.Normal,
            new List<EquipmentModifier>(),
            10);
        inventory.AddEquipmentInstance(maxed);

        controller.ShowEquipmentDetails(maxed);

        Assert.That(detailView.EnhanceInteractable, Is.False);
        Assert.That(detailView.EnhanceLabel, Is.EqualTo("強化完了"));
    }

    [Test]
    public void UseConsumable_WithNonConsumableItem_RefusesAndKeepsTheItem()
    {
        ItemDataSO sword = CreateEquipment("Sword", EquipmentSlot.Weapon);
        inventory.AddItem(sword);

        controller.UseConsumable(sword);

        Assert.That(status, Does.Contain("使用できません"));
        Assert.That(
            inventory.GetItemAmount(sword),
            Is.EqualTo(1),
            "Equipment must never be swallowed by the consumable path.");
        Assert.That(saveGameCount, Is.Zero);
    }

    [Test]
    public void UseConsumable_WithoutCureEffect_ReportsNoUsableEffect()
    {
        ItemDataSO snack = CreateConsumable("Snack");
        snack.consumableEffect = ConsumableEffectType.None;
        inventory.AddItem(snack);

        controller.UseConsumable(snack);

        Assert.That(status, Does.Contain("使用効果がありません"));
        Assert.That(
            inventory.GetItemAmount(snack),
            Is.EqualTo(1),
            "An item with no cure effect must never be consumed.");
    }

    [Test]
    public void UseConsumable_WithNoAfflictedMercenary_KeepsTheItem()
    {
        ItemDataSO antidote = CreateConsumable("Antidote");
        antidote.consumableEffect = ConsumableEffectType.CurePoison;
        inventory.AddItem(antidote);

        controller.UseConsumable(antidote);

        Assert.That(status, Does.Contain("治療対象"));
        Assert.That(inventory.GetItemAmount(antidote), Is.EqualTo(1));
        Assert.That(saveGameCount, Is.Zero);
    }

    [Test]
    public void GetEquipmentDisplayName_AppendsTheEnhancementSuffixOnlyWhenEnhanced()
    {
        ItemDataSO baseItem = CreateEquipment("Sword", EquipmentSlot.Weapon);
        EquipmentInstance plain = EquipmentInstance.CreateFixed(baseItem);
        EquipmentInstance enhanced = EquipmentInstance.CreateRestored(
            null,
            baseItem,
            EquipmentQuality.Normal,
            new List<EquipmentModifier>(),
            3);
        string displayName = JapaneseDisplayText.GetItemName(baseItem);

        Assert.That(
            CharacterEquipmentController.GetEquipmentDisplayName(plain),
            Is.EqualTo(displayName));
        Assert.That(
            CharacterEquipmentController.GetEquipmentDisplayName(enhanced),
            Is.EqualTo($"{displayName} +3"));
        Assert.That(
            CharacterEquipmentController.GetEquipmentDisplayName(null),
            Is.EqualTo("不明な装備"));
    }

    [Test]
    public void IsSetTierActive_RequiresAtLeastTheTierPieceCount()
    {
        EquipmentSetTier tier = new EquipmentSetTier(2, 10, 2, 2, 0.01f);

        Assert.That(CharacterEquipmentController.IsSetTierActive(tier, 1), Is.False);
        Assert.That(CharacterEquipmentController.IsSetTierActive(tier, 2), Is.True);
        Assert.That(CharacterEquipmentController.IsSetTierActive(tier, 3), Is.True);
    }

    private MercenaryInstance SelectMercenary()
    {
        MercenaryDataSO data =
            ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryName = "装備太郎";
        data.mercenaryClass = MercenaryClass.Warrior;
        data.maxHP = 100;
        data.attack = 10;
        data.defense = 3;
        data.attackSpeed = 1f;
        createdObjects.Add(data);

        MercenaryInstance mercenary = new MercenaryInstance(data);
        controller.SelectedDetailMercenary = mercenary;
        return mercenary;
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

    private ItemDataSO CreateEquipment(string itemName, EquipmentSlot slot)
    {
        ItemDataSO item = CreateItem(itemName, 100);
        item.itemType = ItemType.Equipment;
        item.equipmentSlot = slot;
        item.allClassesCanEquip = true;
        return item;
    }

    private ItemDataSO CreateConsumable(string itemName)
    {
        ItemDataSO item = CreateItem(itemName, 30);
        item.itemType = ItemType.Consumable;
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

    /// <summary>
    /// Minimal stand-in for the equipment-detail overlay. It records the last
    /// values the controller pushed so the tests can assert on them without
    /// building any real UI widgets.
    /// </summary>
    private sealed class FakeEquipmentDetailView : IEquipmentDetailView
    {
        public bool HasOverlay { get; set; } = true;
        public string Title { get; private set; }
        public Color TitleColor { get; private set; }
        public string DetailText { get; private set; }
        public bool EnhanceInteractable { get; private set; }
        public string EnhanceLabel { get; private set; }
        public bool SellInteractable { get; private set; }
        public string SellLabel { get; private set; }
        public string LockLabel { get; private set; }
        public int ShowOverlayCount { get; private set; }
        public int HideOverlayCount { get; private set; }

        public void SetTitle(string title, Color color)
        {
            Title = title;
            TitleColor = color;
        }

        public void SetDetailText(string text)
        {
            DetailText = text;
        }

        public void SetEnhanceButton(bool interactable, string label)
        {
            EnhanceInteractable = interactable;
            EnhanceLabel = label;
        }

        public void SetSellButton(bool interactable, string label)
        {
            SellInteractable = interactable;
            SellLabel = label;
        }

        public void SetLockButtonLabel(string label)
        {
            LockLabel = label;
        }

        public void ShowOverlay()
        {
            ShowOverlayCount++;
        }

        public void HideOverlay()
        {
            HideOverlayCount++;
        }
    }
}
