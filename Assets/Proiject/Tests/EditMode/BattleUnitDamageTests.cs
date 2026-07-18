using NUnit.Framework;

public sealed class BattleUnitDamageTests
{
    [TestCase(0, 100)]
    [TestCase(3, 97)]
    [TestCase(100, 50)]
    [TestCase(200, 33)]
    public void EstimateDamageTaken_UsesFixedKDefenseMitigation(int defense, int expectedDamage)
    {
        BattleUnit unit = CreateUnit(defense);
        Assert.That(unit.EstimateDamageTaken(100), Is.EqualTo(expectedDamage));
    }

    [Test]
    public void EstimateDamageTaken_WhenDefenseExceedsDamage_DoesNotClampToOne()
    {
        BattleUnit unit = CreateUnit(200);
        Assert.That(unit.EstimateDamageTaken(100), Is.EqualTo(33));
    }

    [Test]
    public void TakeDamage_UsesTheSameCalculationAsEstimateDamageTaken()
    {
        BattleUnit unit = CreateUnit(100);
        int expectedDamage = unit.EstimateDamageTaken(75);
        unit.TakeDamage(75);
        Assert.That(unit.CurrentHP, Is.EqualTo(200 - expectedDamage));
    }

    [Test]
    public void TakePureDamage_IgnoresDefense()
    {
        BattleUnit unit = CreateUnit(200);
        unit.TakePureDamage(30);
        Assert.That(unit.CurrentHP, Is.EqualTo(170));
    }

    private static BattleUnit CreateUnit(int defense)
    {
        return new BattleUnit("Target", 200, 200, 1, defense, 1f, false);
    }
}
