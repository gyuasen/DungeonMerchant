public readonly struct RoadTravelCompletionResult
{
    public RoadTravelCompletionResult(
        bool isValid,
        bool victory,
        int destinationTownIndex,
        int newCurrentTownIndex,
        int newWorldMapIndex,
        bool openDungeonAfterTravel,
        bool shouldAdvanceDay,
        bool shouldSave,
        string statusMessage)
    {
        IsValid = isValid;
        Victory = victory;
        DestinationTownIndex = destinationTownIndex;
        NewCurrentTownIndex = newCurrentTownIndex;
        NewWorldMapIndex = newWorldMapIndex;
        OpenDungeonAfterTravel = openDungeonAfterTravel;
        ShouldAdvanceDay = shouldAdvanceDay;
        ShouldSave = shouldSave;
        StatusMessage = statusMessage;
    }

    public bool IsValid { get; }
    public bool Victory { get; }
    public int DestinationTownIndex { get; }
    public int NewCurrentTownIndex { get; }
    public int NewWorldMapIndex { get; }
    public bool OpenDungeonAfterTravel { get; }
    public bool ShouldAdvanceDay { get; }
    public bool ShouldSave { get; }
    public string StatusMessage { get; }
}

public static class RoadTravelCompletionService
{
    public static RoadTravelCompletionResult Complete(
        bool victory,
        int currentTownIndex,
        RoadTravelState travelState)
    {
        if (travelState == null || !travelState.IsActive)
        {
            return new RoadTravelCompletionResult(
                false,
                victory,
                currentTownIndex,
                currentTownIndex,
                WorldMapService.GetWorldMapIndexForTown(currentTownIndex),
                false,
                false,
                false,
                string.Empty);
        }

        int destinationTownIndex = travelState.DestinationTownIndex;
        string destinationTownName =
            WorldMapService.GetTownName(destinationTownIndex);

        if (victory)
        {
            string message = travelState.WasUnlock
                ? $"街道戦闘に勝利し、{destinationTownName}を解放しました。"
                : $"街道戦闘に勝利し、{destinationTownName}へ到着しました。";

            return new RoadTravelCompletionResult(
                true,
                true,
                destinationTownIndex,
                destinationTownIndex,
                WorldMapService.GetWorldMapIndexForTown(destinationTownIndex),
                travelState.OpenDungeonAfterTravel,
                true,
                true,
                message);
        }

        string defeatMessage = travelState.WasUnlock
            ? $"街道戦闘に敗北しました。{destinationTownName}は未解放のままです。"
            : $"街道戦闘に敗北したため、{destinationTownName}へ移動できませんでした。";

        return new RoadTravelCompletionResult(
            true,
            false,
            destinationTownIndex,
            currentTownIndex,
            WorldMapService.GetWorldMapIndexForTown(currentTownIndex),
            false,
            false,
            false,
            defeatMessage);
    }
}
