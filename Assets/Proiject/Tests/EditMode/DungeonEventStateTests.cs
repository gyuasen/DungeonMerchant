using NUnit.Framework;

public sealed class DungeonEventStateTests
{
    [Test]
    public void Empty_HasNoEventAndEmptyPresentation()
    {
        DungeonEventState state = DungeonEventState.Empty;

        Assert.That(state.Type, Is.EqualTo(DungeonEventType.None));
        Assert.That(state.Presentation.Title, Is.Empty);
        Assert.That(state.Presentation.Description, Is.Empty);
        Assert.That(state.Presentation.FirstOptionLabel, Is.Empty);
        Assert.That(state.Presentation.SecondOptionLabel, Is.Empty);
        Assert.That(state.Presentation.ThirdOptionLabel, Is.Empty);
    }

    [Test]
    public void State_ContainsMatchingTypeAndPresentationAsOneValue()
    {
        DungeonEventPresentation presentation =
            new DungeonEventPresentation("Title", "Description", "First", "Second", "Third");
        DungeonEventState state =
            new DungeonEventState(DungeonEventType.TreasureCache, presentation);

        Assert.That(state.Type, Is.EqualTo(DungeonEventType.TreasureCache));
        Assert.That(state.Presentation.Title, Is.EqualTo("Title"));
        Assert.That(state.Presentation.Description, Is.EqualTo("Description"));
        Assert.That(state.Presentation.FirstOptionLabel, Is.EqualTo("First"));
        Assert.That(state.Presentation.SecondOptionLabel, Is.EqualTo("Second"));
        Assert.That(state.Presentation.ThirdOptionLabel, Is.EqualTo("Third"));
    }
}
