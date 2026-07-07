using NUnit.Framework;

public sealed class RoadTravelStateTests
{
    [Test]
    public void Begin_InitializesFirstEncounter()
    {
        RoadTravelState state = new RoadTravelState();

        state.Begin(4, true, true, 3);

        Assert.That(state.IsActive, Is.True);
        Assert.That(state.DestinationTownIndex, Is.EqualTo(4));
        Assert.That(state.WasUnlock, Is.True);
        Assert.That(state.OpenDungeonAfterTravel, Is.True);
        Assert.That(state.EncounterCount, Is.EqualTo(3));
        Assert.That(state.EncounterIndex, Is.EqualTo(1));
        Assert.That(state.IsAwaitingChoice, Is.False);
    }

    [Test]
    public void ContinueToNextEncounter_AdvancesOnlyWhileAwaitingChoice()
    {
        RoadTravelState state = new RoadTravelState();
        state.Begin(1, false, false, 3);

        Assert.That(state.ContinueToNextEncounter(), Is.False);

        state.AwaitChoice();

        Assert.That(state.ContinueToNextEncounter(), Is.True);
        Assert.That(state.EncounterIndex, Is.EqualTo(2));
        Assert.That(state.IsAwaitingChoice, Is.False);
    }

    [Test]
    public void Clear_ResetsState()
    {
        RoadTravelState state = new RoadTravelState();
        state.Begin(1, true, false, 2);
        state.SetRareEncounter(true);
        state.AwaitChoice();

        state.Clear();

        Assert.That(state.IsActive, Is.False);
        Assert.That(state.DestinationTownIndex, Is.EqualTo(-1));
        Assert.That(state.WasUnlock, Is.False);
        Assert.That(state.ContainsRareEncounter, Is.False);
        Assert.That(state.IsAwaitingChoice, Is.False);
    }
}
