using NUnit.Framework;
using UnityEngine;

public sealed class DebtManagerTests
{
    private GameObject root;
    private MerchantData merchant;
    private DayManager dayManager;
    private DebtManager debtManager;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Debt Test");
        merchant = root.AddComponent<MerchantData>();
        dayManager = root.AddComponent<DayManager>();
        debtManager = root.AddComponent<DebtManager>();
        debtManager.Initialize(merchant, dayManager);
        merchant.SetGold(5000);
        debtManager.Restore(DebtManager.InitialDebt, 0, 0);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    [Test]
    public void FirstMonth_WithInsufficientGold_CarriesArrears()
    {
        dayManager.SetCurrentDay(31);

        Assert.That(merchant.Gold, Is.Zero);
        Assert.That(debtManager.PaymentArrears, Is.EqualTo(5000));
        Assert.That(
            debtManager.RemainingDebt,
            Is.EqualTo(DebtManager.InitialDebt - 5000));
        Assert.That(debtManager.NextMinimumPayment, Is.EqualTo(15000));
    }

    [Test]
    public void ManualRepayment_IsLimitedByOwnedGold()
    {
        int paid = debtManager.Repay(10000);

        Assert.That(paid, Is.EqualTo(5000));
        Assert.That(merchant.Gold, Is.Zero);
    }
}
