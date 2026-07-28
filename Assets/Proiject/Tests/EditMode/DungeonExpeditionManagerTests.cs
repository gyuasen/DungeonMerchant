using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class DungeonExpeditionManagerTests
{
    private GameObject root;
    private DayManager dayManager;
    private MerchantData merchantData;
    private MerchantInventory inventory;
    private MercenaryHireManager hireManager;
    private DungeonRunManager dungeonRunManager;
    private DungeonExpeditionManager expeditionManager;
    private readonly List<Object> assets = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Dungeon Expedition Test");
        dayManager = root.AddComponent<DayManager>();
        merchantData = root.AddComponent<MerchantData>();
        inventory = root.AddComponent<MerchantInventory>();
        root.AddComponent<TownProgressState>().Initialize(2, new[] { 2 });
        hireManager = root.AddComponent<MercenaryHireManager>();
        root.AddComponent<HealingManager>();
        root.AddComponent<MercenaryPartyManager>();
        root.AddComponent<TrainingGroundManager>();
        root.AddComponent<RoadCargoSession>();
        dungeonRunManager = root.AddComponent<DungeonRunManager>();
        expeditionManager = root.AddComponent<DungeonExpeditionManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        foreach (Object asset in assets)
        {
            Object.DestroyImmediate(asset);
        }
    }

    [Test]
    public void Formation_RequiresClearedVisibleDungeonAndLocalMember()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance local = Hire("local", 2);
        MercenaryInstance remote = Hire("remote", 1);
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { remote }), Is.EqualTo(ExpeditionFormationResult.InvalidMembers));
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { local }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { local }), Is.EqualTo(ExpeditionFormationResult.DungeonAlreadyAssigned));
        DungeonDataSO hidden = CreateDungeon(2, true);
        Assert.That(expeditionManager.TryFormExpedition(hidden, new[] { local }), Is.EqualTo(ExpeditionFormationResult.HiddenDungeon));
    }

    [Test]
    public void Formation_RejectsMoreThanThreeAndDuplicates()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance one = Hire("one", 2);
        MercenaryInstance two = Hire("two", 2);
        MercenaryInstance three = Hire("three", 2);
        MercenaryInstance four = Hire("four", 2);
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { one, two, three, four }), Is.EqualTo(ExpeditionFormationResult.InvalidMembers));
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { one, one }), Is.EqualTo(ExpeditionFormationResult.InvalidMembers));
    }

    [Test]
    public void DayChanged_StrongPartyDepositsRewardsAndSettlesHealing()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        ItemDataSO material = ScriptableObject.CreateInstance<ItemDataSO>();
        material.itemType = ItemType.Material;
        assets.Add(material);
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.goldReward = 10;
        enemy.itemDrops = new[] { new ItemDropEntry { item = material, amount = 1, dropChance = 1f } };
        dungeon.normalEnemies = new[] { enemy };
        assets.Add(enemy);
        MercenaryInstance strong = Hire("strong", 2, 100, 100, 100);
        List<GoldTransaction> recordedTransactions = new List<GoldTransaction>();
        merchantData.GoldTransactionRecorded += transaction => recordedTransactions.Add(transaction);
        DailyResultController dailyResults = new DailyResultController(
            merchantData,
            hireManager,
            root.GetComponent<MercenaryPartyManager>(),
            inventory,
            null,
            CharacterEquipmentController.GetEquipmentDisplayName,
            expeditionManager);
        dailyResults.CaptureDailySnapshot(dayManager.CurrentDay);
        expeditionManager.SetRandomProvider(() => 0f);
        int goldBefore = merchantData.Gold;
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { strong }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();
        Assert.That(merchantData.Gold, Is.GreaterThan(goldBefore));
        Assert.That(inventory.GetItemAmountIn(2, material), Is.GreaterThan(0));
        GoldTransaction rewardTransaction = recordedTransactions.Find(transaction => transaction.Reason == GoldTransactionReason.ExpeditionReward);
        GoldTransaction healingTransaction = recordedTransactions.Find(transaction => transaction.Reason == GoldTransactionReason.ExpeditionHealing);
        Assert.That(rewardTransaction, Is.Not.Null);
        Assert.That(healingTransaction, Is.Not.Null);
        Assert.That(healingTransaction.SignedAmount, Is.LessThan(0));
        Assert.That(rewardTransaction.AccountingDay, Is.EqualTo(dayManager.CurrentDay - 1));
        Assert.That(healingTransaction.AccountingDay, Is.EqualTo(dayManager.CurrentDay - 1));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore + rewardTransaction.SignedAmount + healingTransaction.SignedAmount));
        Assert.That(strong.CurrentHP, Is.EqualTo(strong.MaxHP));
        string dailyText = dailyResults.BuildDailyResultText(dayManager.CurrentDay);
        Assert.That(dailyText, Does.Contain("別動隊の成果"));
        Assert.That(dailyText, Does.Contain("素材"));
        Assert.That(dailyText, Does.Contain("別動隊報酬"));
        expeditionManager.RecallExpedition(expeditionManager.ActiveExpeditions[0]);
        strong.SetCurrentHP(1);
        strong.SetCurrentTownIndex(2);
        dungeon.grade = DungeonGrade.Highest;
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { strong }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();
        Assert.That(strong.CurrentHP, Is.EqualTo(strong.MaxHP));
    }

    [Test]
    public void DayChanged_SuccessWithoutEnoughGold_LeavesExpeditionMemberInjured()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.goldReward = 1;
        enemy.attack = 100;
        dungeon.normalEnemies = new[] { enemy };
        assets.Add(enemy);
        MercenaryInstance member = Hire("unpaid", 2, 100, 100, 100);
        merchantData.SetGold(0);
        List<GoldTransaction> recordedTransactions = new List<GoldTransaction>();
        merchantData.GoldTransactionRecorded += transaction => recordedTransactions.Add(transaction);
        expeditionManager.SetRandomProvider(() => 0f);

        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(member.CurrentHP, Is.LessThan(member.MaxHP));
        Assert.That(merchantData.Gold, Is.GreaterThanOrEqualTo(0));
        Assert.That(recordedTransactions.Exists(transaction => transaction.Reason == GoldTransactionReason.ExpeditionHealing), Is.False);
    }

    [Test]
    public void DayChanged_FailedExpedition_AlsoPaysHealingAtAccountingDay()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        dungeon.grade = DungeonGrade.Highest;
        MercenaryInstance member = Hire("failed", 2, 100, 0, 0);
        GoldTransaction healingTransaction = null;
        merchantData.GoldTransactionRecorded += transaction =>
        {
            if (transaction.Reason == GoldTransactionReason.ExpeditionHealing)
            {
                healingTransaction = transaction;
            }
        };

        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(member.CurrentHP, Is.EqualTo(member.MaxHP));
        Assert.That(healingTransaction, Is.Not.Null);
        Assert.That(healingTransaction.SignedAmount, Is.LessThan(0));
        Assert.That(healingTransaction.AccountingDay, Is.EqualTo(dayManager.CurrentDay - 1));
        Assert.That(healingTransaction.Detail, Does.Contain("別動隊の治療費"));
    }

    [Test]
    public void DutyService_ReportsPartyAndExpedition()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance partyMember = Hire("party", 2);
        MercenaryPartyManager partyManager = root.GetComponent<MercenaryPartyManager>();
        Assert.That(partyManager.TryAdd(partyMember), Is.True);
        Assert.That(MercenaryDutyService.GetDuty(partyMember.InstanceId), Is.EqualTo(MercenaryDuty.Party));
        partyManager.Remove(partyMember);
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { partyMember }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        Assert.That(MercenaryDutyService.GetDuty(partyMember.InstanceId), Is.EqualTo(MercenaryDuty.Expedition));
        Assert.That(MercenaryDutyService.GetDuty("none"), Is.EqualTo(MercenaryDuty.None));
    }

    [Test]
    public void SaveData_RestoresOnlyValidFullyClearedExpedition()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance member = Hire("saved", 2);
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        List<SavedDungeonExpedition> saved = expeditionManager.CreateSaveData();
        expeditionManager.RecallExpedition(expeditionManager.ActiveExpeditions[0]);
        expeditionManager.Restore(saved, new Dictionary<string, MercenaryInstance> { { member.InstanceId, member } });
        Assert.That(expeditionManager.ActiveExpeditions.Count, Is.EqualTo(1));
        expeditionManager.Restore(new List<SavedDungeonExpedition>
        {
            new SavedDungeonExpedition
            {
                dungeonPersistentId = "invalid",
                dungeonAssetName = "invalid",
                memberInstanceIds = new List<string> { "missing" }
            }
        }, new Dictionary<string, MercenaryInstance>());
        Assert.That(expeditionManager.ActiveExpeditions.Count, Is.Zero);
    }

    [Test]
    public void LootPolicy_SellAll_SellsDropsWithoutDepositingThem()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        ItemDataSO material = ConfigureSingleMaterialDrop(dungeon, 20);
        MercenaryInstance member = Hire("sell-all", 2);
        List<GoldTransaction> sales = RecordItemSales();
        expeditionManager.SetRandomProvider(() => 0.5f);

        Assert.That(expeditionManager.TryFormExpedition(
            dungeon, new[] { member }, ExpeditionLootPolicy.SellAll),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(inventory.GetItemAmountIn(2, material), Is.Zero);
        Assert.That(sales.Count, Is.EqualTo(4));
        Assert.That(sales.TrueForAll(sale => sale.SignedAmount == inventory.GetSellPrice(material)), Is.True);
        Assert.That(sales.TrueForAll(sale => sale.AccountingDay == dayManager.CurrentDay - 1), Is.True);
    }

    [Test]
    public void LootPolicy_SellNonEquipment_SellsMaterialsAndStoresLimitedEquipment()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        ItemDataSO material = ConfigureSingleMaterialDrop(dungeon, 20);
        ItemDataSO equipment = ConfigureLimitedEquipment(dungeon, 40);
        MercenaryInstance member = Hire("sell-non-equipment", 2);
        List<GoldTransaction> sales = RecordItemSales();
        expeditionManager.SetRandomProvider(() => 0.5f);

        Assert.That(expeditionManager.TryFormExpedition(
            dungeon, new[] { member }, ExpeditionLootPolicy.SellNonEquipment),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(inventory.GetItemAmountIn(2, material), Is.Zero);
        Assert.That(inventory.GetEquipmentInstancesIn(2), Has.Count.EqualTo(1));
        Assert.That(inventory.GetEquipmentInstancesIn(2)[0].BaseItem, Is.EqualTo(equipment));
        Assert.That(sales, Has.Count.EqualTo(4));
    }

    [Test]
    public void LootPolicy_Store_WhenFull_SellsItemsAndLimitedEquipment()
    {
        root.AddComponent<ProgressionManager>();
        DungeonDataSO dungeon = CreateClearedDungeon();
        ItemDataSO material = ConfigureSingleMaterialDrop(dungeon, 20);
        ItemDataSO equipment = ConfigureLimitedEquipment(dungeon, 40);
        Assert.That(inventory.DepositItemTo(2, material, 30), Is.True);
        MercenaryInstance member = Hire("store-full", 2);
        List<GoldTransaction> sales = RecordItemSales();
        expeditionManager.SetRandomProvider(() => 0.5f);

        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { member }),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(inventory.GetItemAmountIn(2, material), Is.EqualTo(30));
        Assert.That(inventory.GetEquipmentInstancesIn(2), Is.Empty);
        Assert.That(sales, Has.Count.EqualTo(5));
        Assert.That(sales.Exists(sale => sale.SignedAmount == inventory.GetSellPrice(equipment)), Is.True);
    }

    [Test]
    public void LootPolicy_SellNonEquipment_WhenFull_SellsLimitedEquipment()
    {
        root.AddComponent<ProgressionManager>();
        DungeonDataSO dungeon = CreateClearedDungeon();
        ItemDataSO material = ConfigureSingleMaterialDrop(dungeon, 20);
        ItemDataSO equipment = ConfigureLimitedEquipment(dungeon, 40);
        Assert.That(inventory.DepositItemTo(2, material, 30), Is.True);
        MercenaryInstance member = Hire("sell-non-equipment-full", 2);
        List<GoldTransaction> sales = RecordItemSales();
        expeditionManager.SetRandomProvider(() => 0.5f);

        Assert.That(expeditionManager.TryFormExpedition(
            dungeon, new[] { member }, ExpeditionLootPolicy.SellNonEquipment),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(inventory.GetEquipmentInstancesIn(2), Is.Empty);
        Assert.That(sales.Exists(sale => sale.SignedAmount == inventory.GetSellPrice(equipment)), Is.True);
    }

    [Test]
    public void LootPolicy_Store_WhenSpaceAvailable_DepositsWithoutSelling()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        ItemDataSO material = ConfigureSingleMaterialDrop(dungeon, 20);
        MercenaryInstance member = Hire("store", 2);
        List<GoldTransaction> sales = RecordItemSales();
        expeditionManager.SetRandomProvider(() => 0.5f);

        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { member }),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(inventory.GetItemAmountIn(2, material), Is.EqualTo(4));
        Assert.That(sales, Is.Empty);
    }

    [Test]
    public void LootPolicy_SaveRoundTrip_PreservesPolicyAndDefaultsMissingValueToStore()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance member = Hire("policy-save", 2);
        Assert.That(expeditionManager.TryFormExpedition(
            dungeon, new[] { member }, ExpeditionLootPolicy.SellAll),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));

        List<SavedDungeonExpedition> saved = expeditionManager.CreateSaveData();
        Assert.That(saved[0].lootPolicy, Is.EqualTo((int)ExpeditionLootPolicy.SellAll));
        expeditionManager.RecallExpedition(expeditionManager.ActiveExpeditions[0]);
        expeditionManager.Restore(saved,
            new Dictionary<string, MercenaryInstance> { { member.InstanceId, member } });
        Assert.That(expeditionManager.ActiveExpeditions[0].lootPolicy,
            Is.EqualTo(ExpeditionLootPolicy.SellAll));

        expeditionManager.RecallExpedition(expeditionManager.ActiveExpeditions[0]);
        saved[0].lootPolicy = 0;
        expeditionManager.Restore(saved,
            new Dictionary<string, MercenaryInstance> { { member.InstanceId, member } });
        Assert.That(expeditionManager.ActiveExpeditions[0].lootPolicy,
            Is.EqualTo(ExpeditionLootPolicy.Store));
    }

    [Test]
    public void ExpeditionDuty_BlocksOtherManagerEntryPointsUntilRecall()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance member = Hire("occupied", 2);
        member.SetCurrentHP(50);
        HealingManager healingManager = root.GetComponent<HealingManager>();
        MercenaryPartyManager partyManager = root.GetComponent<MercenaryPartyManager>();
        RoadCargoSession roadCargoSession = root.GetComponent<RoadCargoSession>();
        Assert.That(expeditionManager.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        Assert.That(partyManager.TryAdd(member), Is.False);
        Assert.That(healingManager.CanHeal(member), Is.False);
        Assert.That(healingManager.TryHealFull(member), Is.False);
        Assert.That(roadCargoSession.TryBegin(2, 1, null, new[] { member.InstanceId }), Is.EqualTo(RoadCargoDepartureResult.InvalidCargo));
        Assert.That(hireManager.TryReleaseMercenary(member), Is.False);
        Assert.That(hireManager.CanChangeContract(member, MercenaryContractType.Temporary), Is.False);
        expeditionManager.RecallExpedition(expeditionManager.ActiveExpeditions[0]);
        Assert.That(partyManager.TryAdd(member), Is.True);
    }

    [Test]
    public void DailyResults_SeparatesExpeditionEventsByAccountingDay()
    {
        DungeonDataSO firstDungeon = CreateDungeon(2, false);
        firstDungeon.dungeonName = "First Day Dungeon";
        DungeonDataSO secondDungeon = CreateDungeon(2, false);
        secondDungeon.dungeonName = "Second Day Dungeon";
        DailyResultController dailyResults = new DailyResultController(
            merchantData,
            hireManager,
            root.GetComponent<MercenaryPartyManager>(),
            inventory,
            null,
            CharacterEquipmentController.GetEquipmentDisplayName);
        dailyResults.CaptureDailySnapshot(1);
        dailyResults.RecordExpeditionEvent(
            new ExpeditionEvent(
                ExpeditionEventType.Succeeded,
                new DungeonExpedition { dungeon = firstDungeon },
                0,
                new ItemDataSO[0]),
            1);
        string firstDay = dailyResults.BuildDailyResultText(2);
        dailyResults.CaptureDailySnapshot(2);
        dailyResults.RecordExpeditionEvent(
            new ExpeditionEvent(
                ExpeditionEventType.Failed,
                new DungeonExpedition { dungeon = secondDungeon },
                0,
                new ItemDataSO[0]),
            2);
        string secondDay = dailyResults.BuildDailyResultText(3);
        Assert.That(firstDay, Does.Contain("First Day Dungeon"));
        Assert.That(secondDay, Does.Contain("Second Day Dungeon"));
        Assert.That(secondDay, Does.Not.Contain("First Day Dungeon"));
    }

    [Test]
    public void AdvanceDays_FinalizesEachDayAfterDayChanged()
    {
        List<string> events = new List<string>();
        dayManager.DayChanged += day => events.Add("changed:" + day);
        dayManager.DayChangeFinalized += day => events.Add("finalized:" + day);

        dayManager.AdvanceDays(2);

        Assert.That(events, Is.EqualTo(new[]
        {
            "changed:2",
            "finalized:2",
            "changed:3",
            "finalized:3"
        }));
    }

    private DungeonDataSO CreateClearedDungeon()
    {
        DungeonDataSO dungeon = CreateDungeon(2, false);
        typeof(DungeonRunManager).GetField("availableDungeons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(dungeonRunManager, new List<DungeonDataSO> { dungeon });
        dungeonRunManager.RestoreProgress(DungeonGrade.Low, dungeon.name, dungeon.PersistentId, new[] { new SavedDungeonFloorProgress { dungeonPersistentId = dungeon.PersistentId, dungeonAssetName = dungeon.name, clearedFloors = dungeon.totalFloors } });
        return dungeon;
    }

    private DungeonDataSO CreateDungeon(int townIndex, bool hidden)
    {
        DungeonDataSO dungeon = ScriptableObject.CreateInstance<DungeonDataSO>();
        dungeon.name = "ExpeditionDungeon" + assets.Count;
        dungeon.grade = DungeonGrade.Low;
        dungeon.totalFloors = 1;
        dungeon.nearbyTownIndex = hidden ? WorldMapService.HiddenIslandTownIndex : townIndex;
        assets.Add(dungeon);
        return dungeon;
    }

    private MercenaryInstance Hire(string id, int townIndex, int hp = 100, int attack = 100, int defense = 100)
    {
        MercenaryInstance mercenary = MercenaryInstance.CreateRestored(id, null, null, id, MercenaryClass.Warrior, MercenaryContractType.Local, 1, 0, hp, hp, attack, defense, 0, 1f, 0);
        mercenary.SetCurrentTownIndex(townIndex);
        List<MercenaryInstance> hired = new List<MercenaryInstance>(hireManager.HiredMercenaries)
        {
            mercenary
        };
        hireManager.RestoreHiredMercenaries(hired);
        return mercenary;
    }

    private ItemDataSO ConfigureSingleMaterialDrop(DungeonDataSO dungeon, int basePrice)
    {
        dungeon.firstEncounterEnemyCount = 1;
        dungeon.maxEnemyCountPerEncounter = 1;
        dungeon.enemyCountIncreasePerEncounter = 0;
        ItemDataSO material = ScriptableObject.CreateInstance<ItemDataSO>();
        material.name = "Expedition Material " + assets.Count;
        material.itemType = ItemType.Material;
        material.basePrice = basePrice;
        assets.Add(material);
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.goldReward = 0;
        enemy.itemDrops = new[]
        {
            new ItemDropEntry { item = material, amount = 1, dropChance = 1f }
        };
        dungeon.normalEnemies = new[] { enemy };
        assets.Add(enemy);
        return material;
    }

    private ItemDataSO ConfigureLimitedEquipment(DungeonDataSO dungeon, int basePrice)
    {
        ItemDataSO equipment = ScriptableObject.CreateInstance<ItemDataSO>();
        equipment.name = "Expedition Limited Equipment " + assets.Count;
        equipment.itemType = ItemType.Equipment;
        equipment.basePrice = basePrice;
        assets.Add(equipment);
        dungeon.bossLimitedDropChance = 1f;
        dungeon.limitedEquipmentDrops = new[] { equipment };
        return equipment;
    }

    private List<GoldTransaction> RecordItemSales()
    {
        List<GoldTransaction> sales = new List<GoldTransaction>();
        merchantData.GoldTransactionRecorded += transaction =>
        {
            if (transaction.Reason == GoldTransactionReason.ItemSale)
            {
                sales.Add(transaction);
            }
        };
        return sales;
    }
}
