using NUnit.Framework;

public sealed class RoadTravelCompletionServiceTests
{
    [Test]
    public void CompleteVictoryUnlocksDestinationAndRequestsSave()
    {
        RoadTravelState state = new RoadTravelState();
        state.Begin(0, true, false, 1);

        RoadTravelCompletionResult result =
            RoadTravelCompletionService.Complete(true, 2, state);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Victory, Is.True);
        Assert.That(result.DestinationTownIndex, Is.EqualTo(0));
        Assert.That(result.NewCurrentTownIndex, Is.EqualTo(0));
        Assert.That(
            result.NewWorldMapIndex,
            Is.EqualTo(WorldMapService.GetWorldMapIndexForTown(0)));
        Assert.That(result.ShouldAdvanceDay, Is.True);
        Assert.That(result.ShouldSave, Is.True);
        Assert.That(result.OpenDungeonAfterTravel, Is.False);
        Assert.That(result.StatusMessage, Does.Contain("解放"));
    }

    [Test]
    public void CompleteVictoryCanRequestOpeningDungeon()
    {
        RoadTravelState state = new RoadTravelState();
        state.Begin(3, false, true, 1);

        RoadTravelCompletionResult result =
            RoadTravelCompletionService.Complete(true, 0, state);

        Assert.That(result.OpenDungeonAfterTravel, Is.True);
        Assert.That(result.NewCurrentTownIndex, Is.EqualTo(3));
        Assert.That(result.ShouldAdvanceDay, Is.True);
        Assert.That(result.ShouldSave, Is.True);
    }

    [Test]
    public void CompleteDefeatKeepsCurrentTownAndDoesNotSave()
    {
        RoadTravelState state = new RoadTravelState();
        state.Begin(1, false, false, 1);

        RoadTravelCompletionResult result =
            RoadTravelCompletionService.Complete(false, 2, state);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Victory, Is.False);
        Assert.That(result.DestinationTownIndex, Is.EqualTo(1));
        Assert.That(result.NewCurrentTownIndex, Is.EqualTo(2));
        Assert.That(
            result.NewWorldMapIndex,
            Is.EqualTo(WorldMapService.GetWorldMapIndexForTown(2)));
        Assert.That(result.ShouldAdvanceDay, Is.False);
        Assert.That(result.ShouldSave, Is.False);
        Assert.That(result.OpenDungeonAfterTravel, Is.False);
        Assert.That(result.StatusMessage, Does.Contain("移動できませんでした"));
    }

    [Test]
    public void CompleteInactiveStateReturnsInvalidResult()
    {
        RoadTravelState state = new RoadTravelState();

        RoadTravelCompletionResult result =
            RoadTravelCompletionService.Complete(true, 2, state);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.NewCurrentTownIndex, Is.EqualTo(2));
        Assert.That(result.DestinationTownIndex, Is.EqualTo(2));
        Assert.That(result.ShouldAdvanceDay, Is.False);
        Assert.That(result.ShouldSave, Is.False);
        Assert.That(result.StatusMessage, Is.Empty);
    }
}
