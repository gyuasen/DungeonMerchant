using UnityEngine;

public sealed class RoadTravelState
{
    public int DestinationTownIndex { get; private set; } = -1;
    public bool WasUnlock { get; private set; }
    public bool OpenDungeonAfterTravel { get; private set; }
    public int EncounterCount { get; private set; }
    public int EncounterIndex { get; private set; }
    public bool ContainsRareEncounter { get; private set; }
    public bool IsAwaitingChoice { get; private set; }
    public bool IsActive => DestinationTownIndex >= 0;

    public void Begin(
        int destinationTownIndex,
        bool wasUnlock,
        bool openDungeonAfterTravel,
        int encounterCount)
    {
        DestinationTownIndex = destinationTownIndex;
        WasUnlock = wasUnlock;
        OpenDungeonAfterTravel = openDungeonAfterTravel;
        EncounterCount = Mathf.Max(1, encounterCount);
        EncounterIndex = 1;
        ContainsRareEncounter = false;
        IsAwaitingChoice = false;
    }

    public void SetRareEncounter(bool containsRareEncounter)
    {
        ContainsRareEncounter = containsRareEncounter;
    }

    public bool ShouldAskToContinueAfterVictory()
    {
        return IsActive && EncounterIndex < EncounterCount;
    }

    public void AwaitChoice()
    {
        if (IsActive)
        {
            IsAwaitingChoice = true;
        }
    }

    public bool ContinueToNextEncounter()
    {
        if (!IsActive || !IsAwaitingChoice)
        {
            return false;
        }

        IsAwaitingChoice = false;
        EncounterIndex++;
        ContainsRareEncounter = false;
        return EncounterIndex <= EncounterCount;
    }

    public void Clear()
    {
        DestinationTownIndex = -1;
        WasUnlock = false;
        OpenDungeonAfterTravel = false;
        EncounterCount = 0;
        EncounterIndex = 0;
        ContainsRareEncounter = false;
        IsAwaitingChoice = false;
    }
}
