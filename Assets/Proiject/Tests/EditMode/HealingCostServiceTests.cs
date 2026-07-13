using NUnit.Framework;

public sealed class HealingCostServiceTests
{
    [Test]
    public void CalculateFullHealCost_NoMissingHP_IsFree()
    {
        HealingCostBreakdown result =
            HealingCostService.CalculateFullHealCost(100, 100, false);

        Assert.That(result.MissingHP, Is.Zero);
        Assert.That(result.TotalCost, Is.Zero);
    }

    [TestCase(80, 20, 0, 0, 20)]
    [TestCase(60, 25, 15, 0, 55)]
    [TestCase(25, 25, 25, 25, 150)]
    public void CalculateFullHealCost_UsesProgressiveInjuryTiers(
        int currentHP,
        int expectedLightHP,
        int expectedModerateHP,
        int expectedSevereHP,
        int expectedTotal)
    {
        HealingCostBreakdown result =
            HealingCostService.CalculateFullHealCost(100, currentHP, false);

        Assert.That(result.LightInjuryHP, Is.EqualTo(expectedLightHP));
        Assert.That(result.ModerateInjuryHP, Is.EqualTo(expectedModerateHP));
        Assert.That(result.SevereInjuryHP, Is.EqualTo(expectedSevereHP));
        Assert.That(result.TotalCost, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void CalculateFullHealCost_Incapacitated_AddsFlatRevivalCost()
    {
        HealingCostBreakdown result =
            HealingCostService.CalculateFullHealCost(100, 0, true);

        Assert.That(result.LightInjuryCost, Is.EqualTo(25));
        Assert.That(result.ModerateInjuryCost, Is.EqualTo(50));
        Assert.That(result.SevereInjuryCost, Is.EqualTo(150));
        Assert.That(result.RevivalCost, Is.EqualTo(50));
        Assert.That(result.TotalCost, Is.EqualTo(275));
    }

    [Test]
    public void CalculateFullHealCost_OddMaxHP_AccountsForEveryMissingHP()
    {
        HealingCostBreakdown result =
            HealingCostService.CalculateFullHealCost(73, 0, true);

        Assert.That(
            result.LightInjuryHP + result.ModerateInjuryHP + result.SevereInjuryHP,
            Is.EqualTo(73));
        Assert.That(result.TotalCost, Is.EqualTo(213));
    }
}
