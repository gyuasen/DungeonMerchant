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
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Contract Test Root");
        root.SetActive(false);
        merchantData = root.AddComponent<MerchantData>();
        dayManager = root.AddComponent<DayManager>();
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

    private MercenaryDataSO CreateMercenary(int hireCost)
    {
        MercenaryDataSO data = Track(
            ScriptableObject.CreateInstance<MercenaryDataSO>());
        data.mercenaryName = "Local Tester";
        data.mercenaryClass = MercenaryClass.Warrior;
        data.hireCost = hireCost;
        return data;
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
}
