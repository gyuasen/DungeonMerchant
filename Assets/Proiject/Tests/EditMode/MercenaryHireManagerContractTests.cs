using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class MercenaryHireManagerContractTests
{
    private GameObject root;
    private MerchantData merchantData;
    private DayManager dayManager;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private MerchantInventory merchantInventory;
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Contract Test Root");
        root.SetActive(false);
        merchantData = root.AddComponent<MerchantData>();
        dayManager = root.AddComponent<DayManager>();
        merchantInventory = root.AddComponent<MerchantInventory>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        partyManager = root.AddComponent<MercenaryPartyManager>();
        root.SetActive(true);

        InvokeOnEnable(hireManager);
        InvokeOnEnable(partyManager);
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

    [Test]
    public void LocalContract_HasGuaranteedHireRate_AndAutoRenewsOnDayChange()
    {
        merchantData.SetGold(100);

        Assert.That(hireManager.GetSelectedContractSuccessRate(), Is.EqualTo(1f));
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(30), out var mercenary),
            Is.True);
        Assert.That(partyManager.TryAdd(mercenary), Is.True);

        dayManager.AdvanceDay();

        Assert.That(merchantData.Gold, Is.EqualTo(60));
        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
        Assert.That(mercenary.ContractEndDay, Is.EqualTo(dayManager.CurrentDay));
        Assert.That(hireManager.HiredMercenaries.Contains(mercenary), Is.True);
        Assert.That(partyManager.Members.Contains(mercenary), Is.True);
    }

    [Test]
    public void LocalContract_WithoutRenewalGold_WaitsButRemainsHired()
    {
        merchantData.SetGold(35);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(30), out var mercenary),
            Is.True);
        Assert.That(partyManager.TryAdd(mercenary), Is.True);

        dayManager.AdvanceDay();

        Assert.That(merchantData.Gold, Is.EqualTo(5));
        Assert.That(mercenary.ContractNeedsRenewal, Is.True);
        Assert.That(hireManager.HiredMercenaries.Contains(mercenary), Is.True);
        Assert.That(partyManager.Members.Contains(mercenary), Is.False);
    }

    [Test]
    public void TemporaryContract_DoesNotAutoRenewBeforeExpiry()
    {
        merchantData.SetGold(1000);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        mercenary.SetContract(MercenaryContractType.Temporary, dayManager.CurrentDay);
        int goldBeforeDayChanges = merchantData.Gold;

        for (int day = 0; day < 6; day++)
        {
            dayManager.AdvanceDay();
        }

        Assert.That(dayManager.CurrentDay, Is.EqualTo(7));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBeforeDayChanges));
        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
        Assert.That(mercenary.ContractEndDay, Is.EqualTo(7));
    }

    [Test]
    public void TemporaryContract_AutoRenewsAfterExpiryWithConfiguredRenewalCost()
    {
        merchantData.SetGold(1000);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        SetPrivateField(merchantData, "leadership", 4);
        mercenary.SetContract(MercenaryContractType.Temporary, dayManager.CurrentDay);
        int renewalCost = hireManager.GetRenewalCost(mercenary);
        int expectedRenewalCost = MercenaryContractRules.CalculateRenewalCost(
            mercenary.HireCost,
            MercenaryContractType.Temporary,
            merchantData.GetRenewalCostMultiplier());
        int goldBeforeDayChanges = merchantData.Gold;

        for (int day = 0; day < 7; day++)
        {
            dayManager.AdvanceDay();
        }

        Assert.That(dayManager.CurrentDay, Is.EqualTo(8));
        Assert.That(renewalCost, Is.EqualTo(expectedRenewalCost));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBeforeDayChanges - renewalCost));
        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
        Assert.That(mercenary.ContractEndDay, Is.EqualTo(dayManager.CurrentDay + 6));
    }

    [Test]
    public void TemporaryContract_WithoutRenewalGold_WaitsAndCanBeManuallyRenewed()
    {
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        mercenary.SetContract(MercenaryContractType.Temporary, dayManager.CurrentDay);
        Assert.That(partyManager.TryAdd(mercenary), Is.True);

        for (int day = 0; day < 7; day++)
        {
            dayManager.AdvanceDay();
        }

        int renewalCost = hireManager.GetRenewalCost(mercenary);
        Assert.That(mercenary.ContractNeedsRenewal, Is.True);
        Assert.That(hireManager.HiredMercenaries.Contains(mercenary), Is.True);
        Assert.That(partyManager.Members.Contains(mercenary), Is.False);
        merchantData.SetGold(renewalCost);

        Assert.That(hireManager.TryRenewContract(mercenary), Is.True);
        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
        Assert.That(mercenary.ContractEndDay, Is.EqualTo(dayManager.CurrentDay + 6));
    }

    [Test]
    public void TemporaryContract_WaitsForNextDayAutoRetryAfterGoldIsAdded()
    {
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        mercenary.SetContract(MercenaryContractType.Temporary, dayManager.CurrentDay);

        for (int day = 0; day < 7; day++)
        {
            dayManager.AdvanceDay();
        }

        int renewalCost = hireManager.GetRenewalCost(mercenary);
        Assert.That(mercenary.ContractNeedsRenewal, Is.True);
        merchantData.SetGold(renewalCost);

        dayManager.AdvanceDay();

        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
        Assert.That(mercenary.ContractEndDay, Is.EqualTo(dayManager.CurrentDay + 6));
        Assert.That(merchantData.Gold, Is.EqualTo(0));
    }

    [Test]
    public void ExclusiveContract_IsNotAutoRenewedOnDayChange()
    {
        merchantData.SetGold(1000);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        mercenary.SetContract(MercenaryContractType.Exclusive, dayManager.CurrentDay);
        Assert.That(partyManager.TryAdd(mercenary), Is.True);
        int goldBeforeDayChanges = merchantData.Gold;

        for (int day = 0; day < 8; day++)
        {
            dayManager.AdvanceDay();
        }

        Assert.That(merchantData.Gold, Is.EqualTo(goldBeforeDayChanges));
        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
        Assert.That(partyManager.Members.Contains(mercenary), Is.True);
    }

    [TestCase(MercenaryContractType.Local, 1)]
    [TestCase(MercenaryContractType.Temporary, 7)]
    public void AutoRenewal_RecordsClosingAccountingDayForRenewableContracts(
        MercenaryContractType contractType,
        int daysUntilRenewal)
    {
        merchantData.SetGold(1000);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        mercenary.SetContract(contractType, dayManager.CurrentDay);
        GoldTransaction renewalTransaction = null;
        merchantData.GoldTransactionRecorded += transaction =>
        {
            if (transaction.Reason == GoldTransactionReason.ContractRenewal)
            {
                renewalTransaction = transaction;
            }
        };

        for (int day = 0; day < daysUntilRenewal; day++)
        {
            dayManager.AdvanceDay();
        }

        Assert.That(renewalTransaction, Is.Not.Null);
        Assert.That(renewalTransaction.AccountingDay,
            Is.EqualTo(dayManager.CurrentDay - 1));
    }

    [Test]
    public void AutoRenewal_AndStorageMaintenance_AppearInClosingDailyResult()
    {
        ProgressionManager progressionManager = root.AddComponent<ProgressionManager>();
        SetPrivateField(progressionManager, "storageTier", 2);
        InvokeOnEnable(progressionManager);
        merchantData.SetGold(500);
        Assert.That(progressionManager.StorageMaintenanceCost, Is.EqualTo(100));
        DailyResultController controller = new DailyResultController(
            merchantData,
            hireManager,
            partyManager,
            merchantInventory,
            progressionManager,
            equipment => string.Empty);
        controller.CaptureDailySnapshot(dayManager.CurrentDay);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out _), Is.True);
        GoldTransaction storageMaintenance = null;
        GoldTransaction contractRenewal = null;
        merchantData.GoldTransactionRecorded += transaction =>
        {
            if (transaction.Reason == GoldTransactionReason.StorageMaintenance)
            {
                storageMaintenance = transaction;
            }
            if (transaction.Reason == GoldTransactionReason.ContractRenewal)
            {
                contractRenewal = transaction;
            }
        };

        dayManager.AdvanceDay();

        string result = controller.BuildDailyResultText(dayManager.CurrentDay);

        Assert.That(storageMaintenance, Is.Not.Null);
        Assert.That(storageMaintenance.SignedAmount, Is.EqualTo(-100));
        Assert.That(storageMaintenance.AccountingDay,
            Is.EqualTo(dayManager.CurrentDay - 1));
        Assert.That(contractRenewal, Is.Not.Null);
        Assert.That(contractRenewal.SignedAmount, Is.EqualTo(-33));
        Assert.That(contractRenewal.AccountingDay,
            Is.EqualTo(dayManager.CurrentDay - 1));
        Assert.That(merchantData.Gold, Is.EqualTo(267));
        Assert.That(result, Does.Contain("契約更新"));
        Assert.That(result, Does.Contain("倉庫維持費"));
        Assert.That(result, Does.Contain("差引  -233G"));
        Assert.That(result, Does.Not.Contain("その他/未分類"));
    }

    [Test]
    public void ManualRenewal_RecordsTheCurrentAccountingDay()
    {
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        mercenary.SetContract(MercenaryContractType.Local, dayManager.CurrentDay);
        dayManager.AdvanceDay();
        merchantData.SetGold(hireManager.GetRenewalCost(mercenary));
        GoldTransaction renewalTransaction = null;
        merchantData.GoldTransactionRecorded += transaction =>
        {
            if (transaction.Reason == GoldTransactionReason.ContractRenewal)
            {
                renewalTransaction = transaction;
            }
        };

        Assert.That(hireManager.TryRenewContract(mercenary), Is.True);

        Assert.That(renewalTransaction, Is.Not.Null);
        Assert.That(renewalTransaction.AccountingDay,
            Is.EqualTo(dayManager.CurrentDay));
    }

    [Test]
    public void TryReleaseMercenary_RemovesHiredPartyMember_AndRaisesEvent()
    {
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(30), out var mercenary),
            Is.True);
        Assert.That(partyManager.TryAdd(mercenary), Is.True);

        MercenaryInstance dismissed = null;
        hireManager.MercenaryDismissed += released => dismissed = released;

        Assert.That(hireManager.TryReleaseMercenary(mercenary), Is.True);
        Assert.That(dismissed, Is.SameAs(mercenary));
        Assert.That(hireManager.HiredMercenaries.Contains(mercenary), Is.False);
        Assert.That(partyManager.Members.Contains(mercenary), Is.False);
        Assert.That(hireManager.TryReleaseMercenary(mercenary), Is.False);
    }

    [Test]
    public void TryReleaseMercenary_ReturnsAllEquipmentToMercenaryTown()
    {
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(30), out var mercenary),
            Is.True);
        mercenary.SetCurrentTownIndex(1);
        EquipmentInstance weapon = EquipmentInstance.CreateFixed(
            CreateEquipment("Release Weapon", EquipmentSlot.Weapon));
        EquipmentInstance armor = EquipmentInstance.CreateFixed(
            CreateEquipment("Release Armor", EquipmentSlot.Armor));
        EquipmentInstance accessory = EquipmentInstance.CreateFixed(
            CreateEquipment("Release Accessory", EquipmentSlot.Accessory));
        Assert.That(mercenary.EquipEquipment(weapon), Is.True);
        Assert.That(mercenary.EquipEquipment(armor), Is.True);
        Assert.That(mercenary.EquipEquipment(accessory), Is.True);

        Assert.That(hireManager.TryReleaseMercenary(mercenary), Is.True);

        Assert.That(merchantInventory.GetEquipmentInstancesIn(1),
            Does.Contain(weapon));
        Assert.That(merchantInventory.GetEquipmentInstancesIn(1),
            Does.Contain(armor));
        Assert.That(merchantInventory.GetEquipmentInstancesIn(1),
            Does.Contain(accessory));
        Assert.That(merchantInventory.GetEquipmentInstancesIn(2),
            Is.Empty);
    }

    [Test]
    public void TryReleaseMercenary_ReturnsLegacyEquipmentWhenStorageIsFull()
    {
        root.AddComponent<ProgressionManager>();
        ItemDataSO material = Track(ScriptableObject.CreateInstance<ItemDataSO>());
        material.itemName = "Full Storage Material";
        Assert.That(merchantInventory.DepositItemTo(1, material, 30), Is.True);
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(30), out var mercenary),
            Is.True);
        mercenary.SetCurrentTownIndex(1);
        ItemDataSO legacyWeapon = CreateEquipment(
            "Legacy Release Weapon",
            EquipmentSlot.Weapon);
        Assert.That(mercenary.EquipEquipment(legacyWeapon), Is.True);

        Assert.That(hireManager.TryReleaseMercenary(mercenary), Is.True);

        Assert.That(merchantInventory.GetUsedStorageSlotsIn(1), Is.EqualTo(30));
        Assert.That(merchantInventory.GetEquipmentInstancesIn(1), Has.Count.EqualTo(1));
        Assert.That(merchantInventory.GetEquipmentInstancesIn(1)[0].BaseItem,
            Is.SameAs(legacyWeapon));
    }

    [Test]
    public void ExclusiveContract_FailedRoll_DoesNotChargeHireCost()
    {
        merchantData.SetGold(100);
        SetPrivateField(merchantData, "merchantLevel", 5);
        SetPrivateField(
            hireManager,
            "selectedContract",
            MercenaryContractType.Exclusive);
        float successRate = hireManager.GetSelectedContractSuccessRate();
        int failingSeed = FindSeedWithRollAbove(successRate);
        Random.InitState(failingSeed);

        bool hired = hireManager.TryHireMercenary(CreateMercenary(30), out _);

        Assert.That(hired, Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(100));
    }

    [Test]
    public void InitialContractCost_UsesConfiguredMultipliers()
    {
        MercenaryDataSO data = CreateMercenary(100);

        Assert.That(hireManager.GetInitialContractCost(
            data, MercenaryContractType.Local), Is.EqualTo(100));
        Assert.That(hireManager.GetInitialContractCost(
            data, MercenaryContractType.Temporary), Is.EqualTo(1000));
        Assert.That(hireManager.GetInitialContractCost(
            data, MercenaryContractType.Exclusive), Is.EqualTo(2000));
    }

    [Test]
    public void TryChangeContract_ChargesFullNewContractCostAndResetsTerm()
    {
        merchantData.SetGold(3000);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        SetPrivateField(merchantData, "merchantLevel", 2);

        Assert.That(hireManager.TryChangeContract(
            mercenary, MercenaryContractType.Temporary), Is.True);

        Assert.That(merchantData.Gold, Is.EqualTo(1900));
        Assert.That(mercenary.ContractType, Is.EqualTo(MercenaryContractType.Temporary));
        Assert.That(mercenary.ContractEndDay, Is.EqualTo(dayManager.CurrentDay + 6));
        Assert.That(mercenary.ContractNeedsRenewal, Is.False);
    }

    [Test]
    public void TryChangeContract_InsufficientGoldLeavesContractUnchanged()
    {
        merchantData.SetGold(150);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        SetPrivateField(merchantData, "merchantLevel", 2);

        Assert.That(hireManager.TryChangeContract(
            mercenary, MercenaryContractType.Temporary), Is.False);

        Assert.That(mercenary.ContractType, Is.EqualTo(MercenaryContractType.Local));
        Assert.That(merchantData.Gold, Is.EqualTo(50));
    }

    [Test]
    public void ExclusiveContract_SuccessfulHireChargesTwentyTimesBaseCost()
    {
        merchantData.SetGold(2000);
        SetPrivateField(merchantData, "merchantLevel", 5);
        SetPrivateField(
            hireManager,
            "selectedContract",
            MercenaryContractType.Exclusive);
        Random.InitState(FindSeedWithRollAtMost(
            hireManager.GetSelectedContractSuccessRate()));

        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);

        Assert.That(merchantData.Gold, Is.EqualTo(0));
        Assert.That(mercenary.ContractType, Is.EqualTo(MercenaryContractType.Exclusive));
    }

    [Test]
    public void CanAfford_UsesSelectedContractCostForFixedAndGeneratedCandidates()
    {
        MercenaryDataSO fixedCandidate = CreateMercenary(100);
        MercenaryInstance generatedCandidate = new MercenaryInstance(CreateMercenary(100));
        SetPrivateField(
            hireManager,
            "selectedContract",
            MercenaryContractType.Temporary);
        merchantData.SetGold(999);

        Assert.That(hireManager.CanAfford(fixedCandidate), Is.False);
        Assert.That(hireManager.CanAfford(generatedCandidate), Is.False);
        merchantData.SetGold(1000);
        Assert.That(hireManager.CanAfford(fixedCandidate), Is.True);
        Assert.That(hireManager.CanAfford(generatedCandidate), Is.True);
    }

    [Test]
    public void RenewalCost_SharedRuleMatchesInstanceAndManagerForAllContracts()
    {
        merchantData.SetGold(100);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(90), out var mercenary),
            Is.True);
        SetPrivateField(merchantData, "leadership", 4);
        float multiplier = merchantData.GetRenewalCostMultiplier();

        foreach (MercenaryContractType contractType in new[]
        {
            MercenaryContractType.Local,
            MercenaryContractType.Temporary,
            MercenaryContractType.Exclusive
        })
        {
            mercenary.SetContract(contractType, dayManager.CurrentDay);
            int expected = MercenaryContractRules.CalculateRenewalCost(
                mercenary.HireCost, contractType, multiplier);

            Assert.That(hireManager.GetRenewalCost(mercenary), Is.EqualTo(expected),
                contractType.ToString());
            Assert.That(hireManager.GetRenewalCost(mercenary, contractType),
                Is.EqualTo(expected), contractType.ToString());
        }

        mercenary.SetContract(MercenaryContractType.Local, dayManager.CurrentDay);
        Assert.That(mercenary.GetRenewalCost(), Is.EqualTo(
            MercenaryContractRules.CalculateRenewalCost(
                mercenary.HireCost, MercenaryContractType.Local)));
    }

    [Test]
    public void RenewalCost_ExclusiveHasNoRenewalAndLocalKeepsMinimumOne()
    {
        Assert.That(MercenaryContractRules.CalculateRenewalCost(
            1000, MercenaryContractType.Exclusive), Is.EqualTo(0));
        Assert.That(MercenaryContractRules.CalculateRenewalCost(
            1, MercenaryContractType.Local), Is.EqualTo(1));
        Assert.That(MercenaryContractRules.CalculateRenewalCost(
            1, MercenaryContractType.Temporary), Is.EqualTo(1));
    }

    [Test]
    public void InitialContractCost_ClampsToIntMaxValue()
    {
        Assert.That(MercenaryContractRules.CalculateInitialCost(
            int.MaxValue,
            MercenaryContractType.Exclusive), Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void ContractChangeValidation_MatchesExecutionForRejectedChanges()
    {
        merchantData.SetGold(5000);
        Assert.That(hireManager.TryHireMercenary(CreateMercenary(100), out var mercenary),
            Is.True);
        Assert.That(hireManager.GetContractChangeUnavailableReason(
            mercenary, MercenaryContractType.Temporary),
            Is.EqualTo(MercenaryHireManager.ContractChangeUnavailableReason.ContractLocked));
        SetPrivateField(merchantData, "merchantLevel", 2);

        Assert.That(hireManager.GetContractChangeUnavailableReason(
            mercenary, MercenaryContractType.Local),
            Is.EqualTo(MercenaryHireManager.ContractChangeUnavailableReason.SameOrLower));
        Assert.That(hireManager.CanChangeContract(
            mercenary, MercenaryContractType.Local), Is.False);
        Assert.That(hireManager.TryChangeContract(
            mercenary, MercenaryContractType.Local), Is.False);
        Assert.That(hireManager.TryChangeContract(
            mercenary, MercenaryContractType.Temporary), Is.True);
        Assert.That(hireManager.GetContractChangeUnavailableReason(
            mercenary, MercenaryContractType.Local),
            Is.EqualTo(MercenaryHireManager.ContractChangeUnavailableReason.SameOrLower));
    }

    private MercenaryDataSO CreateMercenary(int hireCost)
    {
        MercenaryDataSO data = Track(
            ScriptableObject.CreateInstance<MercenaryDataSO>());
        data.mercenaryName = "Local Tester";
        data.mercenaryClass = MercenaryClass.Warrior;
        data.hireCost = hireCost;
        return data;
    }

    private ItemDataSO CreateEquipment(string itemName, EquipmentSlot slot)
    {
        ItemDataSO item = Track(ScriptableObject.CreateInstance<ItemDataSO>());
        item.name = itemName;
        item.itemName = itemName;
        item.itemType = ItemType.Equipment;
        item.equipmentSlot = slot;
        item.allClassesCanEquip = true;
        return item;
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }

    private static void InvokeOnEnable(MonoBehaviour component)
    {
        component.GetType()
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(component, null);
    }

    private static int FindSeedWithRollAbove(float threshold)
    {
        for (int seed = 0; seed < 10000; seed++)
        {
            Random.InitState(seed);
            if (Random.value > threshold)
            {
                return seed;
            }
        }

        Assert.Fail($"Could not find a failed hire roll above {threshold}.");
        return 0;
    }

    private static int FindSeedWithRollAtMost(float threshold)
    {
        for (int seed = 0; seed < 10000; seed++)
        {
            Random.InitState(seed);
            if (Random.value <= threshold)
            {
                return seed;
            }
        }

        Assert.Fail($"Could not find a successful hire roll at or below {threshold}.");
        return 0;
    }

    private static void SetPrivateField(
        object target,
        string fieldName,
        object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }
}
