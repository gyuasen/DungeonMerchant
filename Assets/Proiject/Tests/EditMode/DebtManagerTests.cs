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
    public void FirstMonth_WithInsufficientGold_ForcesGoldNegative()
    {
        // 所持金5000でも月次1万Gを強制徴収し、所持金は-5000へ沈む。
        // 残債は1万G減り、延滞金は繰り越さない。
        dayManager.SetCurrentDay(31);

        Assert.That(merchant.Gold, Is.EqualTo(-5000));
        Assert.That(merchant.HasNegativeGold, Is.True);
        Assert.That(debtManager.PaymentArrears, Is.Zero);
        Assert.That(
            debtManager.RemainingDebt,
            Is.EqualTo(DebtManager.InitialDebt - DebtManager.MonthlyMinimumPayment));
        Assert.That(
            debtManager.NextMinimumPayment,
            Is.EqualTo(DebtManager.MonthlyMinimumPayment));
    }

    [Test]
    public void NegativeGold_ContinuesDeductingNextMonth()
    {
        // マイナス中も月次徴収は続き、所持金はさらに沈み、残債は着実に減る。
        dayManager.SetCurrentDay(31);
        Assert.That(merchant.Gold, Is.EqualTo(-5000));

        dayManager.SetCurrentDay(61);

        Assert.That(merchant.Gold, Is.EqualTo(-15000));
        Assert.That(
            debtManager.RemainingDebt,
            Is.EqualTo(DebtManager.InitialDebt - 2 * DebtManager.MonthlyMinimumPayment));
        Assert.That(debtManager.PaymentArrears, Is.Zero);
    }

    [Test]
    public void ManualRepayment_IsLimitedByOwnedGold()
    {
        int paid = debtManager.Repay(10000);

        Assert.That(paid, Is.EqualTo(5000));
        Assert.That(merchant.Gold, Is.Zero);
    }

    [Test]
    public void ManualRepayment_IsLimitedByRemainingDebt()
    {
        // 残債より所持金が多くても、返済額は残債でクランプされ完済で止まる。
        debtManager.Restore(3000, 0, 0);
        merchant.SetGold(1000000);

        int paid = debtManager.Repay(1000000);

        Assert.That(paid, Is.EqualTo(3000));
        Assert.That(debtManager.RemainingDebt, Is.Zero);
        Assert.That(debtManager.IsDebtCleared, Is.True);
        Assert.That(merchant.Gold, Is.EqualTo(1000000 - 3000));
    }
}
