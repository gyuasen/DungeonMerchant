using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// MerchantData の金銭処理(支払い・入金・台帳記録)の契約を固定するテスト。
// 従来これらは他機能のテスト経由で間接的にしか検証されていなかった。
public sealed class MerchantDataAccountingTests
{
    private GameObject root;
    private MerchantData merchantData;
    private readonly List<GoldTransaction> recorded =
        new List<GoldTransaction>();
    private int lastGoldChanged;
    private int goldChangedCount;

    [SetUp]
    public void SetUp()
    {
        recorded.Clear();
        lastGoldChanged = -1;
        goldChangedCount = 0;
        root = new GameObject("MerchantData Accounting Test");
        merchantData = root.AddComponent<MerchantData>();
        merchantData.SetGold(1000);
        merchantData.GoldChanged += value =>
        {
            lastGoldChanged = value;
            goldChangedCount++;
        };
        merchantData.GoldTransactionRecorded += tx => recorded.Add(tx);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    [Test]
    public void TryPayGold_WithSufficientGold_DeductsAndRecordsNegativeAmount()
    {
        bool result = merchantData.TryPayGold(
            300, GoldTransactionReason.StorageUpgrade, "拡張");

        Assert.That(result, Is.True);
        Assert.That(merchantData.Gold, Is.EqualTo(700));
        Assert.That(lastGoldChanged, Is.EqualTo(700));
        Assert.That(recorded, Has.Count.EqualTo(1));
        Assert.That(recorded[0].SignedAmount, Is.EqualTo(-300));
        Assert.That(recorded[0].Reason,
            Is.EqualTo(GoldTransactionReason.StorageUpgrade));
    }

    [Test]
    public void TryPayGold_WithInsufficientGold_FailsWithoutChange()
    {
        bool result = merchantData.TryPayGold(
            2000, GoldTransactionReason.StorageUpgrade);

        Assert.That(result, Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(1000));
        Assert.That(goldChangedCount, Is.Zero);
        Assert.That(recorded, Is.Empty);
    }

    [Test]
    public void TryPayGold_WithNegativeAmount_FailsWithoutChange()
    {
        LogAssert.Expect(LogType.Error, "Invalid payment amount.");
        bool result = merchantData.TryPayGold(
            -100, GoldTransactionReason.Unclassified);

        Assert.That(result, Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(1000));
        Assert.That(recorded, Is.Empty);
    }

    [Test]
    public void TryPayGold_ExactBalance_Succeeds()
    {
        bool result = merchantData.TryPayGold(
            1000, GoldTransactionReason.Unclassified);

        Assert.That(result, Is.True);
        Assert.That(merchantData.Gold, Is.Zero);
    }

    [Test]
    public void AddGold_IncreasesGoldAndRecordsPositiveAmount()
    {
        merchantData.AddGold(250, GoldTransactionReason.ItemSale, "売却");

        Assert.That(merchantData.Gold, Is.EqualTo(1250));
        Assert.That(lastGoldChanged, Is.EqualTo(1250));
        Assert.That(recorded, Has.Count.EqualTo(1));
        Assert.That(recorded[0].SignedAmount, Is.EqualTo(250));
        Assert.That(recorded[0].Reason,
            Is.EqualTo(GoldTransactionReason.ItemSale));
    }

    [Test]
    public void AddGold_WithNegativeAmount_IsRejected()
    {
        LogAssert.Expect(LogType.Error, "Invalid gold reward amount.");
        merchantData.AddGold(-50, GoldTransactionReason.ItemSale);

        Assert.That(merchantData.Gold, Is.EqualTo(1000));
        Assert.That(recorded, Is.Empty);
    }

    [Test]
    public void GoldTransaction_UsesExplicitAccountingDay()
    {
        merchantData.AddGold(
            100, GoldTransactionReason.ExpeditionReward, "報酬", accountingDay: 7);

        Assert.That(recorded, Has.Count.EqualTo(1));
        Assert.That(recorded[0].AccountingDay, Is.EqualTo(7));
    }

    [Test]
    public void TryPayGold_ReturnsTransactionId()
    {
        bool result = merchantData.TryPayGold(
            100, GoldTransactionReason.Unclassified, "test",
            out string transactionId);

        Assert.That(result, Is.True);
        Assert.That(transactionId, Is.Not.Null.And.Not.Empty);
        Assert.That(recorded[0].TransactionId, Is.EqualTo(transactionId));
    }
}
