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
