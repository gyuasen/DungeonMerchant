using NUnit.Framework;
using UnityEngine;

public sealed class HealingManagerTests
{
    private GameObject root;
    private MerchantData merchantData;
    private HealingManager healingManager;
    private MercenaryDataSO mercenaryData;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Healing Manager Test Root");
        merchantData = root.AddComponent<MerchantData>();
        healingManager = root.AddComponent<HealingManager>();
        mercenaryData = ScriptableObject.CreateInstance<MercenaryDataSO>();
        mercenaryData.mercenaryName = "Healing Tester";
        mercenaryData.maxHP = 100;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(mercenaryData);
    }

    [Test]
    public void GetFullHealCostBreakdown_ExposesUiReadyComponents()
    {
        MercenaryInstance mercenary = CreateMercenary(25);

        HealingCostBreakdown result =
            healingManager.GetFullHealCostBreakdown(mercenary);

        Assert.That(result.MissingHP, Is.EqualTo(75));
        Assert.That(result.LightInjuryCost, Is.EqualTo(25));
        Assert.That(result.ModerateInjuryCost, Is.EqualTo(50));
        Assert.That(result.SevereInjuryCost, Is.EqualTo(75));
        Assert.That(result.RevivalCost, Is.Zero);
        Assert.That(healingManager.GetFullHealCost(mercenary), Is.EqualTo(150));
    }

    [Test]
    public void TryHealFull_WithInsufficientGold_DoesNotChargeOrHeal()
    {
        MercenaryInstance mercenary = CreateMercenary(0);
        merchantData.SetGold(274);

        bool healed = healingManager.TryHealFull(mercenary);

        Assert.That(healed, Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(274));
        Assert.That(mercenary.CurrentHP, Is.Zero);
    }

    [Test]
    public void TryHealFull_WhenAffordable_ChargesExactCostAndHeals()
    {
        MercenaryInstance mercenary = CreateMercenary(0);
        merchantData.SetGold(300);
        int changedCount = 0;
        healingManager.HealingChanged += () => changedCount++;

        bool healed = healingManager.TryHealFull(mercenary);

        Assert.That(healed, Is.True);
        Assert.That(merchantData.Gold, Is.EqualTo(25));
        Assert.That(mercenary.CurrentHP, Is.EqualTo(mercenary.MaxHP));
        Assert.That(changedCount, Is.EqualTo(1));
    }

    [Test]
    public void TryHealFull_WithoutInjury_IsFreeAndReturnsFalse()
    {
        MercenaryInstance mercenary = CreateMercenary(100);
        merchantData.SetGold(100);

        Assert.That(healingManager.GetFullHealCost(mercenary), Is.Zero);
        Assert.That(healingManager.TryHealFull(mercenary), Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(100));
    }

    private MercenaryInstance CreateMercenary(int currentHP)
    {
        MercenaryInstance mercenary = new MercenaryInstance(mercenaryData);
        mercenary.SetCurrentHP(currentHP);
        return mercenary;
    }
}
