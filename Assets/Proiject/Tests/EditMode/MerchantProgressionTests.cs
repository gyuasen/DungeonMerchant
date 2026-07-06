using NUnit.Framework;
using UnityEngine;

public sealed class MerchantProgressionTests
{
    private GameObject root;
    private MerchantData merchant;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Merchant Progression Test");
        merchant = root.AddComponent<MerchantData>();
        merchant.SetGold(0);
        merchant.RestoreProgression(1, 0, 0);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    [Test]
    public void EarnedGold_RaisesLevelAndKeepsRemainder()
    {
        merchant.AddGold(750);

        Assert.That(merchant.MerchantLevel, Is.EqualTo(2));
        Assert.That(merchant.MerchantExperience, Is.EqualTo(150));
        Assert.That(merchant.LifetimeGoldEarned, Is.EqualTo(750));
        Assert.That(merchant.MerchantSkillPoints, Is.EqualTo(3));
    }

    [Test]
    public void SpendingGold_DoesNotIncreaseLifetimeEarnings()
    {
        merchant.AddGold(1000);
        int earnedBeforePayment = merchant.LifetimeGoldEarned;

        Assert.That(merchant.TryPayGold(400), Is.True);
        Assert.That(merchant.Gold, Is.EqualTo(600));
        Assert.That(
            merchant.LifetimeGoldEarned,
            Is.EqualTo(earnedBeforePayment));
    }
}
