using NUnit.Framework;

public sealed class DungeonEventServiceTests
{
    [Test]
    public void AbandonedCamp_TradesFullRecoveryForOneDay()
    {
        DungeonEventChoiceResult careful = DungeonEventService.ResolveChoice(
            DungeonEventType.AbandonedCamp, 0, 40, 100, 15);
        DungeonEventChoiceResult quick = DungeonEventService.ResolveChoice(
            DungeonEventType.AbandonedCamp, 1, 40, 100, 15);

        Assert.That(careful.HealAmount, Is.EqualTo(40));
        Assert.That(careful.AddExplorationDelay, Is.True);
        Assert.That(quick.HealAmount, Is.EqualTo(20));
        Assert.That(quick.AddExplorationDelay, Is.False);
    }

    [Test]
    public void TreasureCache_TradesTimeForSafetyAndFullReward()
    {
        DungeonEventChoiceResult careful = DungeonEventService.ResolveChoice(
            DungeonEventType.TreasureCache, 0, 40, 100, 15);
        DungeonEventChoiceResult forced = DungeonEventService.ResolveChoice(
            DungeonEventType.TreasureCache, 1, 40, 100, 15);

        Assert.That(careful.GoldAmount, Is.EqualTo(100));
        Assert.That(careful.DamageAmount, Is.Zero);
        Assert.That(careful.AddExplorationDelay, Is.True);
        Assert.That(forced.GoldAmount, Is.EqualTo(50));
        Assert.That(forced.DamageAmount, Is.EqualTo(15));
        Assert.That(forced.AddExplorationDelay, Is.False);
    }

    [Test]
    public void CollapsedPassage_TradesOneDayForAvoidingDamage()
    {
        DungeonEventChoiceResult detour = DungeonEventService.ResolveChoice(
            DungeonEventType.CollapsedPassage, 0, 40, 100, 15);
        DungeonEventChoiceResult forced = DungeonEventService.ResolveChoice(
            DungeonEventType.CollapsedPassage, 1, 40, 100, 15);

        Assert.That(detour.DamageAmount, Is.Zero);
        Assert.That(detour.AddExplorationDelay, Is.True);
        Assert.That(forced.DamageAmount, Is.EqualTo(15));
        Assert.That(forced.AddExplorationDelay, Is.False);
    }
}
