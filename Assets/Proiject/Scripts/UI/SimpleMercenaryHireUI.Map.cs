using System.Collections;
using UnityEngine;

public partial class SimpleMercenaryHireUI
{
    private void BuildGlobalMapPage() => mapPresenter.BuildGlobalMapPage();
    private void BuildWorldMapPage() => mapPresenter.BuildWorldMapPage();
    private void BuildTownMapPage() => mapPresenter.BuildTownMapPage();
    private void ShowGlobalMap() => mapPresenter.ShowGlobalMap();
    private void ShowWorldMap() => mapPresenter.ShowWorldMap();
    private void ShowWorldMap(int worldMapIndex) => mapPresenter.ShowWorldMap(worldMapIndex);
    private void ShowTownMap() => mapPresenter.ShowTownMap();
    private void RefreshGlobalMapPage() => mapPresenter.RefreshGlobalMapPage();
    private void RefreshWorldMapPage() => mapPresenter.RefreshWorldMapPage();
    private void RefreshTownMapPage() => mapPresenter.RefreshTownMapPage();
    private void SetMapHeaderButtons(bool showTownMapButton) => mapPresenter.SetMapHeaderButtons(showTownMapButton);
    private void RefreshTownMapButtons() => mapPresenter.RefreshTownMapButtons();

    private IEnumerator ContinueTownTravelBattleRoutine()
    {
        yield return null;

        townTravelController.StartNextTravelEncounter();
    }

    private void SyncDungeonUnlocks()
    {
        if (dungeonRunManager == null)
        {
            dungeonRunManager =
                GetComponent<DungeonRunManager>() ??
                FindObjectOfType<DungeonRunManager>();
        }

        dungeonRunManager?.SetUnlockedTownIndices(townProgressState.GetUnlockedTownIndices());
    }

    private bool TryUnlockHiddenIsland()
    {
        bool unlocked = HiddenIslandUnlockService.TryUnlock(
            townProgressState,
            dungeonRunManager,
            merchantInventory,
            hireManager != null ? hireManager.HiredMercenaries : null);
        if (unlocked)
        {
            SyncDungeonUnlocks();
            saveManager?.SaveGame();
        }
        return unlocked;
    }
}
