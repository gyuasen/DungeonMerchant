using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class TransportManagerTests
{
    private GameObject root;
    private DayManager dayManager;
    private MerchantData merchantData;
    private MerchantInventory inventory;
    private TownProgressState townProgress;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private TransportManager transportManager;
    private readonly List<UnityEngine.Object> createdAssets =
        new List<UnityEngine.Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Transport Manager Test");
        dayManager = root.AddComponent<DayManager>();
        merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<MarketPriceManager>();
        inventory = root.AddComponent<MerchantInventory>();
        townProgress = root.AddComponent<TownProgressState>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        partyManager = root.AddComponent<MercenaryPartyManager>();
        transportManager = root.AddComponent<TransportManager>();
        townProgress.Initialize(2, new[] { 2, 1, 0 });
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        foreach (UnityEngine.Object asset in createdAssets)
        {
            if (asset != null)
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }
        createdAssets.Clear();
    }

    [Test]
    public void TryDepartConvoy_InvalidDestinationsAndCargo_HaveNoSideEffects()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        inventory.AddItem(item, 2);
        AssertRejected(3, Cargo(item, 1), null,
            TransportDepartureResult.InvalidDestination);
        AssertRejected(2, Cargo(item, 1), null,
            TransportDepartureResult.InvalidDestination);
        AssertRejected(0, null, null, TransportDepartureResult.InvalidCargo);
        AssertRejected(0, new List<(ItemDataSO item, int amount)>(), null,
            TransportDepartureResult.InvalidCargo);
        AssertRejected(0, Cargo(item, 3), null,
            TransportDepartureResult.InsufficientCargo);
        AssertRejected(0, new List<(ItemDataSO item, int amount)>
        {
            (item, 2), (item, 1)
        }, null, TransportDepartureResult.InsufficientCargo);
    }

    [Test]
    public void TryDepartConvoy_InvalidEscortsAndGold_HaveNoSideEffects()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        inventory.AddItem(item, 10);
        List<MercenaryInstance> fourEscorts = new List<MercenaryInstance>();
        for (int i = 0; i < 4; i++)
        {
            MercenaryInstance mercenary = CreateMercenary("escort" + i);
            Hire(mercenary);
            fourEscorts.Add(mercenary);
        }
        AssertRejected(0, Cargo(item, 1), fourEscorts,
            TransportDepartureResult.InvalidEscort);

        Assert.That(partyManager.TryAdd(fourEscorts[0]), Is.True);
        AssertRejected(0, Cargo(item, 1), new[] { fourEscorts[0] },
            TransportDepartureResult.InvalidEscort);
        Assert.That(partyManager.Remove(fourEscorts[0]), Is.True);

        Assert.That(transportManager.TryDepartConvoy(0, Cargo(item, 1),
            new[] { fourEscorts[0] }), Is.EqualTo(TransportDepartureResult.Succeeded));
        AssertRejected(0, Cargo(item, 1), new[] { fourEscorts[0] },
            TransportDepartureResult.InvalidEscort);

        merchantData.SetGold(0);
        AssertRejected(0, Cargo(item, 1), null,
            TransportDepartureResult.InsufficientGold);
    }

    [Test]
    public void TryDepartConvoy_Success_RemovesCargoAndChargesExactCost()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        inventory.AddItem(item, 3);
        int beforeGold = merchantData.Gold;

        TransportDepartureResult result = transportManager.TryDepartConvoy(
            0,
            Cargo(item, 3),
            null);

        Assert.That(result, Is.EqualTo(TransportDepartureResult.Succeeded));
        Assert.That(inventory.GetItemAmount(item), Is.Zero);
        Assert.That(merchantData.Gold, Is.EqualTo(beforeGold - 300));
        Assert.That(transportManager.ActiveConvoys.Count, Is.EqualTo(1));
        Assert.That(transportManager.ActiveConvoys[0].totalSegments, Is.EqualTo(2));
        Assert.That(transportManager.ActiveConvoys[0].remainingDays, Is.EqualTo(2));
    }

    [Test]
    public void DayChanged_NoRaid_DecrementsOnly()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        inventory.AddItem(item, 1);
        transportManager.SetRandomProvider(() => .9f);
        transportManager.TryDepartConvoy(0, Cargo(item, 1), null);

        dayManager.AdvanceDay();

        Assert.That(transportManager.ActiveConvoys[0].remainingDays, Is.EqualTo(1));
        Assert.That(transportManager.ActiveConvoys[0].cargo[0].amount, Is.EqualTo(1));
    }

    [Test]
    public void DayChanged_RaidWithoutEscort_LosesHalfCargo()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        inventory.AddItem(item, 10);
        // 10 cargo units over 2 segments cost 1000G; the default 500G would
        // make TryDepartConvoy fail with InsufficientGold and no convoy.
        merchantData.SetGold(2000);
        TransportEvent occurred = null;
        transportManager.TransportEventOccurred += value => occurred = value;
        transportManager.SetRandomProvider(() => .1f);
        Assert.That(
            transportManager.TryDepartConvoy(0, Cargo(item, 10), null),
            Is.EqualTo(TransportDepartureResult.Succeeded));

        dayManager.AdvanceDay();

        Assert.That(occurred.Type, Is.EqualTo(TransportEventType.RaidLoss));
        Assert.That(occurred.LostCargo, Is.EqualTo(5));
        Assert.That(transportManager.ActiveConvoys[0].cargo[0].amount, Is.EqualTo(5));
    }

    [Test]
    public void DayChanged_WeakEscortRaid_DamagesHpButNeverBelowOne()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        MercenaryInstance escort = CreateMercenary("weak", 1, 0, 0);
        Hire(escort);
        inventory.AddItem(item, 10);
        // Same as the raid test above: cover the 1000G transport cost.
        merchantData.SetGold(2000);
        transportManager.SetRandomProvider(() => .1f);
        Assert.That(
            transportManager.TryDepartConvoy(0, Cargo(item, 10), new[] { escort }),
            Is.EqualTo(TransportDepartureResult.Succeeded));

        dayManager.AdvanceDay();

        Assert.That(escort.CurrentHP, Is.EqualTo(1));
        Assert.That(transportManager.ActiveConvoys[0].cargo[0].amount,
            Is.EqualTo(5));
    }

    [Test]
    public void DayChanged_StrongEscort_RepelsRaidAndGainsExperience()
    {
        ItemDataSO item = GameAssetRepository.LoadAll<ItemDataSO>()[0];
        MercenaryInstance escort = CreateMercenary("strong", 1000, 1000, 1000);
        Hire(escort);
        inventory.AddItem(item, 1);
        TransportEvent occurred = null;
        transportManager.TransportEventOccurred += value => occurred = value;
        transportManager.SetRandomProvider(() => .1f);
        int experience = escort.CurrentExperience;
        transportManager.TryDepartConvoy(0, Cargo(item, 1), new[] { escort });

        dayManager.AdvanceDay();

        Assert.That(occurred.Type, Is.EqualTo(TransportEventType.RaidRepelled));
        Assert.That(transportManager.ActiveConvoys[0].cargo[0].amount, Is.EqualTo(1));
        Assert.That(escort.CurrentExperience, Is.GreaterThan(experience));
    }

    [Test]
    public void DayChanged_Arrival_DepositsCargoAtDestinationAndReleasesEscort()
    {
        ItemDataSO item = CreateItem("Cargo", 100);
        MercenaryInstance escort = CreateMercenary("escort");
        Hire(escort);
        inventory.AddItem(item, 1);
        transportManager.SetRandomProvider(() => .9f);
        transportManager.TryDepartConvoy(1, Cargo(item, 1), new[] { escort });
        int goldBeforeArrival = merchantData.Gold;

        dayManager.AdvanceDay();

        Assert.That(merchantData.Gold, Is.EqualTo(goldBeforeArrival));
        Assert.That(inventory.GetItemAmountIn(1, item), Is.EqualTo(1));
        Assert.That(inventory.GetItemAmount(item), Is.Zero);
        Assert.That(transportManager.ActiveConvoys, Is.Empty);
        Assert.That(transportManager.IsMercenaryOnTransportDuty(escort.InstanceId), Is.False);
    }

    [Test]
    public void CreateSaveDataAndRestore_PreservesConvoyFields()
    {
        ItemDataSO item = GameAssetRepository.LoadAll<ItemDataSO>()[0];
        MercenaryInstance escort = CreateMercenary("escort");
        Hire(escort);
        inventory.AddItem(item, 1);
        transportManager.TryDepartConvoy(0, Cargo(item, 1), new[] { escort });
        List<SavedTransportConvoy> saved = transportManager.CreateSaveData();
        GameObject restoreRoot = new GameObject("Restored Transport Test");
        TransportManager restored = restoreRoot.AddComponent<TransportManager>();

        restored.Restore(saved, new Dictionary<string, MercenaryInstance>
        {
            { escort.InstanceId, escort }
        });

        TransportConvoy convoy = restored.ActiveConvoys[0];
        Assert.That(convoy.originTownIndex, Is.EqualTo(2));
        Assert.That(convoy.destinationTownIndex, Is.EqualTo(0));
        Assert.That(convoy.remainingDays, Is.EqualTo(2));
        Assert.That(convoy.cargo[0].amount, Is.EqualTo(1));
        Assert.That(convoy.escortInstanceIds, Is.EqualTo(new[] { escort.InstanceId }));
        UnityEngine.Object.DestroyImmediate(restoreRoot);
    }

    [Test]
    public void CalculateTransportCost_TwoSegmentsThreeCargoAtLogisticsZero_Is300()
    {
        Assert.That(transportManager.CalculateTransportCost(0, 3), Is.EqualTo(300));
    }

    private void AssertRejected(
        int destination,
        IReadOnlyList<(ItemDataSO item, int amount)> cargo,
        IReadOnlyList<MercenaryInstance> escorts,
        TransportDepartureResult expected)
    {
        int gold = merchantData.Gold;
        int convoyCount = transportManager.ActiveConvoys.Count;
        ItemDataSO item = cargo != null && cargo.Count > 0 ? cargo[0].item : null;
        int amount = item != null ? inventory.GetItemAmount(item) : 0;
        Assert.That(transportManager.TryDepartConvoy(destination, cargo, escorts),
            Is.EqualTo(expected));
        Assert.That(merchantData.Gold, Is.EqualTo(gold));
        Assert.That(transportManager.ActiveConvoys.Count, Is.EqualTo(convoyCount));
        if (item != null)
        {
            Assert.That(inventory.GetItemAmount(item), Is.EqualTo(amount));
        }
    }

    private ItemDataSO CreateItem(string itemName, int basePrice)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = itemName;
        item.itemName = itemName;
        item.basePrice = basePrice;
        createdAssets.Add(item);
        return item;
    }

    private static List<(ItemDataSO item, int amount)> Cargo(
        ItemDataSO item,
        int amount)
    {
        return new List<(ItemDataSO item, int amount)> { (item, amount) };
    }

    private static MercenaryInstance CreateMercenary(
        string instanceId,
        int maxHp = 100,
        int attack = 20,
        int defense = 10)
    {
        return MercenaryInstance.CreateRestored(
            instanceId,
            null,
            null,
            instanceId,
            MercenaryClass.Warrior,
            MercenaryContractType.Local,
            1,
            0,
            maxHp,
            maxHp,
            attack,
            defense,
            0,
            1f,
            0);
    }

    private void Hire(MercenaryInstance mercenary)
    {
        mercenary.SetCurrentTownIndex(townProgress.CurrentTownIndex);
        List<MercenaryInstance> hired = new List<MercenaryInstance>(
            hireManager.HiredMercenaries);
        hired.Add(mercenary);
        hireManager.RestoreHiredMercenaries(hired);
    }
}
