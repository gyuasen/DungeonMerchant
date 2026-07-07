using System.Collections.Generic;
using NUnit.Framework;

public sealed class TownMapServiceTests
{
    [Test]
    public void GetNextUnlockableTownIndex_FollowsProgressionOrder()
    {
        HashSet<int> unlocked = new HashSet<int> { 2 };

        Assert.That(
            TownMapService.GetNextUnlockableTownIndex(unlocked),
            Is.EqualTo(1));

        unlocked.Add(1);
        unlocked.Add(0);

        Assert.That(
            TownMapService.GetNextUnlockableTownIndex(unlocked),
            Is.EqualTo(3));
    }

    [Test]
    public void AreTownsAdjacent_UsesProgressionRoute()
    {
        Assert.That(TownMapService.AreTownsAdjacent(2, 1), Is.True);
        Assert.That(TownMapService.AreTownsAdjacent(1, 0), Is.True);
        Assert.That(TownMapService.AreTownsAdjacent(0, 3), Is.True);
        Assert.That(TownMapService.AreTownsAdjacent(2, 0), Is.False);
    }

    [Test]
    public void CanEnterWorldRegion_AllowsCurrentOrUnlockedRegion()
    {
        HashSet<int> unlocked = new HashSet<int> { 2, 3 };

        Assert.That(
            TownMapService.CanEnterWorldRegion(0, 2, unlocked, null),
            Is.True);
        Assert.That(
            TownMapService.CanEnterWorldRegion(1, 2, unlocked, null),
            Is.True);
        Assert.That(
            TownMapService.CanEnterWorldRegion(2, 2, unlocked, null),
            Is.False);
    }

    [Test]
    public void ValidateTravelRequest_RejectsNonAdjacentDestination()
    {
        HashSet<int> unlocked = new HashSet<int> { 2 };

        TownMapService.TravelValidationResult result =
            TownMapService.ValidateTravelRequest(2, 0, unlocked, true, null);

        Assert.That(result.CanTravel, Is.False);
        Assert.That(result.FailureMessage, Does.Contain("直接は移動できません"));
    }

    [Test]
    public void ValidateTravelRequest_RejectsEmptyParty()
    {
        HashSet<int> unlocked = new HashSet<int> { 2 };

        TownMapService.TravelValidationResult result =
            TownMapService.ValidateTravelRequest(2, 1, unlocked, false, null);

        Assert.That(result.CanTravel, Is.False);
        Assert.That(result.IsUnlockTravel, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("傭兵の編成が必要"));
    }

    [Test]
    public void ValidateTravelRequest_AllowsNextUnlockTravel()
    {
        HashSet<int> unlocked = new HashSet<int> { 2 };

        TownMapService.TravelValidationResult result =
            TownMapService.ValidateTravelRequest(2, 1, unlocked, true, null);

        Assert.That(result.CanTravel, Is.True);
        Assert.That(result.IsUnlockTravel, Is.True);
        Assert.That(result.FailureMessage, Is.Empty);
    }
}
