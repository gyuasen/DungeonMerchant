using System.Collections.Generic;
using NUnit.Framework;

public sealed class MercenaryContractDetailModelTests
{
    private const int BaseHireCost = 90;

    [Test]
    public void BuildColumns_ReturnsThreeContractsInDisplayOrder()
    {
        IReadOnlyList<MercenaryContractDetailColumn> columns = BuildDefaultColumns(
            MercenaryContractType.Local,
            unlockedLevel: 5);

        Assert.That(columns.Count, Is.EqualTo(3));
        Assert.That(columns[0].ContractType, Is.EqualTo(MercenaryContractType.Local));
        Assert.That(columns[1].ContractType, Is.EqualTo(MercenaryContractType.Temporary));
        Assert.That(columns[2].ContractType, Is.EqualTo(MercenaryContractType.Exclusive));
    }

    [Test]
    public void BuildColumns_UsesSuppliedRealAmounts()
    {
        IReadOnlyList<MercenaryContractDetailColumn> columns = BuildDefaultColumns(
            MercenaryContractType.Local,
            unlockedLevel: 5);

        Assert.That(columns[0].InitialCost, Is.EqualTo(BaseHireCost));
        Assert.That(columns[1].InitialCost, Is.EqualTo(BaseHireCost * 10));
        Assert.That(columns[2].InitialCost, Is.EqualTo(BaseHireCost * 20));
    }

    [Test]
    public void RenewalCostText_MatchesSharedRuleAndExclusiveHasNoRenewal()
    {
        IReadOnlyList<MercenaryContractDetailColumn> columns = BuildDefaultColumns(
            MercenaryContractType.Local,
            unlockedLevel: 5);

        int expectedLocal = MercenaryContractRules.CalculateRenewalCost(
            BaseHireCost, MercenaryContractType.Local);
        int expectedTemporary = MercenaryContractRules.CalculateRenewalCost(
            BaseHireCost, MercenaryContractType.Temporary);

        Assert.That(columns[0].RenewalCost, Is.EqualTo(expectedLocal));
        Assert.That(columns[0].RenewalCostText, Is.EqualTo(expectedLocal + " G"));
        Assert.That(columns[1].RenewalCost, Is.EqualTo(expectedTemporary));
        Assert.That(columns[1].RenewalCostText, Is.EqualTo(expectedTemporary + " G"));
        Assert.That(columns[2].RenewalCost, Is.EqualTo(0));
        Assert.That(columns[2].RenewalCostText, Is.EqualTo("更新なし"));
    }

    [Test]
    public void TermText_DescribesEachContractDuration()
    {
        Assert.That(
            MercenaryContractDetailModel.GetTermText(MercenaryContractType.Local),
            Is.EqualTo("当日"));
        Assert.That(
            MercenaryContractDetailModel.GetTermText(MercenaryContractType.Temporary),
            Is.EqualTo("7日間"));
        Assert.That(
            MercenaryContractDetailModel.GetTermText(MercenaryContractType.Exclusive),
            Is.EqualTo("無期限"));
    }

    [Test]
    public void RenewalMethodText_DescribesEachContractProcess()
    {
        Assert.That(
            MercenaryContractDetailModel.GetRenewalMethodText(
                MercenaryContractType.Local),
            Is.EqualTo("毎日自動更新"));
        Assert.That(
            MercenaryContractDetailModel.GetRenewalMethodText(
                MercenaryContractType.Temporary),
            Is.EqualTo("7日ごとに自動更新"));
        Assert.That(
            MercenaryContractDetailModel.GetRenewalMethodText(
                MercenaryContractType.Exclusive),
            Is.EqualTo("更新不要"));
    }

    [Test]
    public void UnlockStatus_ReflectsMerchantLevelWithRequiredLevelShown()
    {
        IReadOnlyList<MercenaryContractDetailColumn> columns = BuildDefaultColumns(
            MercenaryContractType.Local,
            unlockedLevel: 1);

        Assert.That(columns[0].IsUnlocked, Is.True);
        Assert.That(columns[0].UnlockStatusText, Is.EqualTo("解放済み"));
        Assert.That(columns[1].IsUnlocked, Is.False);
        Assert.That(columns[1].RequiredMerchantLevel, Is.EqualTo(2));
        Assert.That(columns[1].UnlockStatusText, Does.Contain("商人Lv2で解放"));
        Assert.That(columns[2].IsUnlocked, Is.False);
        Assert.That(columns[2].RequiredMerchantLevel, Is.EqualTo(5));
        Assert.That(columns[2].UnlockStatusText, Does.Contain("商人Lv5で解放"));
    }

    [Test]
    public void SelectedColumn_IsFlaggedAndLabeledOnlyForSelectedContract()
    {
        IReadOnlyList<MercenaryContractDetailColumn> columns = BuildDefaultColumns(
            MercenaryContractType.Temporary,
            unlockedLevel: 5);

        Assert.That(columns[0].IsSelected, Is.False);
        Assert.That(columns[1].IsSelected, Is.True);
        Assert.That(columns[2].IsSelected, Is.False);

        Assert.That(
            MercenaryContractDetailModel.BuildColumnText(columns[1]),
            Does.StartWith("選択中"));
        Assert.That(
            MercenaryContractDetailModel.BuildColumnText(columns[0]),
            Does.Not.Contain("選択中"));
    }

    [Test]
    public void BuildColumnText_ContainsAllComparisonFields()
    {
        IReadOnlyList<MercenaryContractDetailColumn> columns = BuildDefaultColumns(
            MercenaryContractType.Local,
            unlockedLevel: 5);
        string text = MercenaryContractDetailModel.BuildColumnText(columns[1]);

        Assert.That(text, Does.Contain("臨時契約"));
        Assert.That(text, Does.Contain("契約金: " + (BaseHireCost * 10) + " G"));
        Assert.That(text, Does.Contain("期限: 7日間"));
        Assert.That(text, Does.Contain("更新方法:"));
        Assert.That(text, Does.Contain("7日ごとに自動更新"));
        Assert.That(text, Does.Contain("解放状況:"));
    }

    private static IReadOnlyList<MercenaryContractDetailColumn> BuildDefaultColumns(
        MercenaryContractType selected,
        int unlockedLevel)
    {
        return MercenaryContractDetailModel.BuildColumns(
            selected,
            contractType => unlockedLevel >=
                MercenaryContractRules.GetRequiredMerchantLevel(contractType),
            contractType => MercenaryContractRules.CalculateInitialCost(
                BaseHireCost, contractType),
            contractType => MercenaryContractRules.CalculateRenewalCost(
                BaseHireCost, contractType));
    }
}
