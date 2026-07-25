using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class DailyResultControllerTests
{
    private GameObject root;
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        if (root != null)
        {
            Object.DestroyImmediate(root);
        }

        foreach (Object created in createdObjects)
        {
            if (created != null)
            {
                Object.DestroyImmediate(created);
            }
        }

        createdObjects.Clear();
    }

    [Test]
    public void BuildDailyResultText_IncludesStorageUsageCapacityChangeAndRemainingSpace()
    {
        root = new GameObject("Daily Result Storage Test");
        MerchantData merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<DayManager>();
        MercenaryHireManager hireManager =
            root.AddComponent<MercenaryHireManager>();
        MercenaryPartyManager partyManager =
            root.AddComponent<MercenaryPartyManager>();
        MerchantInventory inventory = root.AddComponent<MerchantInventory>();
        ProgressionManager progressionManager =
            root.AddComponent<ProgressionManager>();
        ItemDataSO item = Track(CreateItem("Daily Result Ore"));
        Assert.That(inventory.TryAddItem(item, 2), Is.True);

        DailyResultController controller = new DailyResultController(
            merchantData,
            hireManager,
            partyManager,
            inventory,
            progressionManager,
            equipment => equipment?.BaseItem?.itemName ?? string.Empty);
        controller.CaptureDailySnapshot(1);

        Assert.That(inventory.TryAddItem(item, 3), Is.True);
        progressionManager.Restore(new ProgressionSaveData
        {
            storageTier = 1,
            quests = new List<QuestRecord>()
        });

        string result = controller.BuildDailyResultText(2);

        Assert.That(result, Does.Contain("【倉庫】"));
        Assert.That(result, Does.Contain("使用量 2/30 → 5/60"));
        Assert.That(result, Does.Contain("空き容量 55"));
    }

    [Test]
    public void TrainingCompletion_IsShownOnceInDailyResult()
    {
        root = new GameObject("Daily Result Training Test");
        MerchantData merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<DayManager>();
        MercenaryHireManager hireManager = root.AddComponent<MercenaryHireManager>();
        MercenaryPartyManager partyManager = root.AddComponent<MercenaryPartyManager>();
        MercenaryDataSO data = Track(ScriptableObject.CreateInstance<MercenaryDataSO>());
        data.mercenaryName = "修練太郎";
        MercenaryInstance mercenary = new MercenaryInstance(data);
        hireManager.RestoreHiredMercenaries(new[] { mercenary });
        DailyResultController controller = new DailyResultController(
            merchantData,
            hireManager,
            partyManager,
            null,
            null,
            equipment => string.Empty);
        controller.CaptureDailySnapshot(1);
        TrainingReservation reservation = new TrainingReservation(
            mercenary.InstanceId, 2, 1, 2, 2, 100, mercenary);

        controller.RecordTrainingCompleted(reservation);
        controller.RecordTrainingCompleted(reservation);
        string result = controller.BuildDailyResultText(2);

        Assert.That(result.Split('\n').Length,
            Is.GreaterThan(0));
        Assert.That(result.IndexOf("修練太郎がLv2になった"),
            Is.GreaterThanOrEqualTo(0));
        Assert.That(result.IndexOf("修練太郎がLv2になった"),
            Is.EqualTo(result.LastIndexOf("修練太郎がLv2になった")));
    }

    [Test]
    public void BuildDailyResultText_AggregatesGoldTransactionsForPreviousDay()
    {
        root = new GameObject("Daily Result Gold Ledger Test");
        MerchantData merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<DayManager>();
        MercenaryHireManager hireManager = root.AddComponent<MercenaryHireManager>();
        MercenaryPartyManager partyManager = root.AddComponent<MercenaryPartyManager>();
        DailyResultController controller = new DailyResultController(
            merchantData, hireManager, partyManager, null, null, equipment => string.Empty);
        controller.CaptureDailySnapshot(1);
        merchantData.AddGold(1500, GoldTransactionReason.QuestReward);
        Assert.That(merchantData.TryPayGold(900, GoldTransactionReason.Healing), Is.True);

        string result = controller.BuildDailyResultText(2);

        Assert.That(result, Does.Contain("【本日の金銭収支】"));
        Assert.That(result, Does.Contain("依頼報酬"));
        Assert.That(result, Does.Contain("治療費"));
        Assert.That(result, Does.Contain("差引  +600G"));
    }

    [Test]
    public void BuildDailyResultText_AggregatesLedgerAcrossMultipleDays()
    {
        root = new GameObject("Daily Result Multi Day Ledger Test");
        MerchantData merchantData = root.AddComponent<MerchantData>();
        DayManager dayManager = root.AddComponent<DayManager>();
        MercenaryHireManager hireManager = root.AddComponent<MercenaryHireManager>();
        MercenaryPartyManager partyManager = root.AddComponent<MercenaryPartyManager>();
        DailyResultController controller = new DailyResultController(
            merchantData, hireManager, partyManager, null, null, equipment => string.Empty);
        controller.CaptureDailySnapshot(1);
        merchantData.AddGold(100, GoldTransactionReason.QuestReward);
        dayManager.SetCurrentDay(2);
        Assert.That(merchantData.TryPayGold(20, GoldTransactionReason.Healing), Is.True);

        string result = controller.BuildDailyResultText(3);

        Assert.That(result, Does.Contain("依頼報酬"));
        Assert.That(result, Does.Contain("治療費"));
        Assert.That(result, Does.Contain("差引  +80G"));
        Assert.That(result, Does.Not.Contain("その他/未分類"));
    }

    [Test]
    public void BuildDailyResultText_UsesExplicitAccountingDayForMaintenance()
    {
        root = new GameObject("Daily Result Maintenance Ledger Test");
        MerchantData merchantData = root.AddComponent<MerchantData>();
        DayManager dayManager = root.AddComponent<DayManager>();
        MercenaryHireManager hireManager = root.AddComponent<MercenaryHireManager>();
        MercenaryPartyManager partyManager = root.AddComponent<MercenaryPartyManager>();
        DailyResultController controller = new DailyResultController(
            merchantData, hireManager, partyManager, null, null, equipment => string.Empty);
        controller.CaptureDailySnapshot(1);
        dayManager.SetCurrentDay(2);
        Assert.That(merchantData.TryPayGold(
            10,
            GoldTransactionReason.StorageMaintenance,
            "倉庫維持費",
            1), Is.True);

        string closingResult = controller.BuildDailyResultText(2);
        controller.CaptureDailySnapshot(2);
        string nextResult = controller.BuildDailyResultText(3);

        Assert.That(closingResult, Does.Contain("倉庫維持費"));
        Assert.That(nextResult, Does.Not.Contain("倉庫維持費"));
    }

    private static ItemDataSO CreateItem(string itemName)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = itemName;
        item.itemName = itemName;
        item.itemType = ItemType.Material;
        return item;
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
