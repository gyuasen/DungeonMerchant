using System;
using System.Collections.Generic;
using UnityEngine;

public class TownProgressState : MonoBehaviour
{
    [SerializeField] private int currentTownIndex = 2;
    [SerializeField] private int viewedWorldMapIndex;
    [SerializeField] private MercenaryPartyManager partyManager;

    private readonly HashSet<int> unlockedTownIndices = new HashSet<int> { 2 };

    public int CurrentTownIndex => currentTownIndex;

    public int CurrentWorldMapIndex =>
        WorldMapService.GetWorldMapIndexForTown(currentTownIndex);

    public int ViewedWorldMapIndex
    {
        get => viewedWorldMapIndex;
        set => viewedWorldMapIndex = value;
    }

    public event Action TownProgressChanged;

    public List<int> GetUnlockedTownIndices()
    {
        List<int> result = new List<int>(unlockedTownIndices);
        result.Sort();
        return result;
    }

    public bool IsTownUnlocked(int townIndex)
    {
        return unlockedTownIndices.Contains(townIndex);
    }

    public void RestoreTownProgress(
        int townIndex,
        IReadOnlyList<int> savedUnlockedTownIndices)
    {
        currentTownIndex = Mathf.Clamp(
            townIndex, 0, WorldMapService.TownNames.Length - 1);
        unlockedTownIndices.Clear();
        foreach (int unlockedTownIndex in
                 WorldMapService.CreateRestoredUnlockedTownIndices(
                     currentTownIndex,
                     savedUnlockedTownIndices))
        {
            unlockedTownIndices.Add(unlockedTownIndex);
        }

        viewedWorldMapIndex = CurrentWorldMapIndex;
        TownProgressChanged?.Invoke();
    }

    public void UnlockTown(int townIndex)
    {
        if (!WorldMapService.IsValidTownIndex(townIndex))
        {
            return;
        }

        if (unlockedTownIndices.Add(townIndex))
        {
            TownProgressChanged?.Invoke();
        }
    }

    public void SetCurrentTown(int townIndex)
    {
        int clampedTownIndex = Mathf.Clamp(
            townIndex, 0, WorldMapService.TownNames.Length - 1);
        if (clampedTownIndex == currentTownIndex)
        {
            return;
        }

        currentTownIndex = clampedTownIndex;
        ResolveReferences();
        partyManager?.UpdateMemberLocations(currentTownIndex);
        TownProgressChanged?.Invoke();
    }

    public void Initialize(
        int startingTownIndex,
        IReadOnlyList<int> startingUnlocked)
    {
        currentTownIndex = Mathf.Clamp(
            startingTownIndex, 0, WorldMapService.TownNames.Length - 1);
        unlockedTownIndices.Clear();
        if (startingUnlocked != null)
        {
            foreach (int townIndex in startingUnlocked)
            {
                if (WorldMapService.IsValidTownIndex(townIndex))
                {
                    unlockedTownIndices.Add(townIndex);
                }
            }
        }

        unlockedTownIndices.Add(currentTownIndex);
        viewedWorldMapIndex = CurrentWorldMapIndex;
    }

    private void ResolveReferences()
    {
        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>() ??
                           FindObjectOfType<MercenaryPartyManager>();
        }
    }
}
