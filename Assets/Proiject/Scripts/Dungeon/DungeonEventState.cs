public readonly struct DungeonEventState
{
    public static readonly DungeonEventState Empty =
        new DungeonEventState(DungeonEventType.None, DungeonEventPresentation.Empty);

    public readonly DungeonEventType Type;
    public readonly DungeonEventPresentation Presentation;

    public DungeonEventState(
        DungeonEventType type,
        DungeonEventPresentation presentation)
    {
        Type = type;
        Presentation = presentation;
    }
}
