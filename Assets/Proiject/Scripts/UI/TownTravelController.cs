using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the town-travel flow: travel validation, the confirmation state,
/// the road-battle begin/continue/retreat sequence, the road-travel
/// completion sequence and the town/region unlock queries. Extracted from
/// SimpleMercenaryHireUI (step 3.10). Map page construction, routing,
/// coroutines and UI-only refreshes stay in SimpleMercenaryHireUI.Map.cs /
/// .BattleDungeon.cs; only the feature state and business actions live
/// here.
/// </summary>
public sealed class TownTravelController
{
    private readonly TownProgressState townProgressState;
    private readonly MercenaryPartyManager partyManager;
    private readonly BattleManager battleManager;
    private readonly RoadEncounterService roadEncounterService;
    private readonly DungeonRunManager dungeonRunManager;
    private readonly DayManager dayManager;
    private readonly MercenaryGenerator mercenaryGenerator;
    private readonly MarketStockManager marketStockManager;
    private readonly BlacksmithManager blacksmithManager;
    private readonly SaveManager saveManager;
    private readonly Action<string> setStatus;
    private readonly Action showTownMap;
    private readonly Action showWorldMap;
    private readonly Action<string> showTravelConfirmation;
    private readonly Action hideTravelConfirmation;
    private readonly Action resetBattleLog;
    private readonly Action<int, int> showRoadBattlePage;
    private readonly Action<bool> setRoadChoiceButtonsActive;
    private readonly Action<string> setRoadBattleRouteText;
    private readonly Action continueTravelBattle;
    private readonly Action openNearbyDungeon;
    private readonly Action syncDungeonUnlocks;
    private readonly Action refreshTownMapButtons;

    private readonly RoadTravelState roadTravelState = new RoadTravelState();
    private int confirmationTravelTownIndex = -1;
    private bool confirmationOpenDungeonAfterTravel;

    public TownTravelController(
        TownProgressState townProgressState,
        MercenaryPartyManager partyManager,
        BattleManager battleManager,
        RoadEncounterService roadEncounterService,
        DungeonRunManager dungeonRunManager,
        DayManager dayManager,
        MercenaryGenerator mercenaryGenerator,
        MarketStockManager marketStockManager,
        BlacksmithManager blacksmithManager,
        SaveManager saveManager,
        Action<string> setStatus,
        Action showTownMap,
        Action showWorldMap,
        Action<string> showTravelConfirmation,
        Action hideTravelConfirmation,
        Action resetBattleLog,
        Action<int, int> showRoadBattlePage,
        Action<bool> setRoadChoiceButtonsActive,
        Action<string> setRoadBattleRouteText,
        Action continueTravelBattle,
        Action openNearbyDungeon,
        Action syncDungeonUnlocks,
        Action refreshTownMapButtons)
    {
        this.townProgressState = townProgressState;
        this.partyManager = partyManager;
        this.battleManager = battleManager;
        this.roadEncounterService = roadEncounterService;
        this.dungeonRunManager = dungeonRunManager;
        this.dayManager = dayManager;
        this.mercenaryGenerator = mercenaryGenerator;
        this.marketStockManager = marketStockManager;
        this.blacksmithManager = blacksmithManager;
        this.saveManager = saveManager;
        this.setStatus = setStatus;
        this.showTownMap = showTownMap;
        this.showWorldMap = showWorldMap;
        this.showTravelConfirmation = showTravelConfirmation;
        this.hideTravelConfirmation = hideTravelConfirmation;
        this.resetBattleLog = resetBattleLog;
        this.showRoadBattlePage = showRoadBattlePage;
        this.setRoadChoiceButtonsActive = setRoadChoiceButtonsActive;
        this.setRoadBattleRouteText = setRoadBattleRouteText;
        this.continueTravelBattle = continueTravelBattle;
        this.openNearbyDungeon = openNearbyDungeon;
        this.syncDungeonUnlocks = syncDungeonUnlocks;
        this.refreshTownMapButtons = refreshTownMapButtons;
    }

    public RoadTravelState RoadTravelState => roadTravelState;

    public void TravelToTown(int townIndex)
    {
        townIndex = Mathf.Clamp(townIndex, 0, WorldMapService.TownNames.Length - 1);

        if (townIndex == townProgressState.CurrentTownIndex)
        {
            showTownMap();
            return;
        }

        RequestTownTravel(townIndex, false);
    }

    public void TravelToDungeon(int townIndex)
    {
        townIndex = Mathf.Clamp(townIndex, 0, WorldMapService.TownNames.Length - 1);

        if (townIndex == townProgressState.CurrentTownIndex)
        {
            openNearbyDungeon();
            return;
        }

        RequestTownTravel(townIndex, true);
    }

    private void RequestTownTravel(
        int townIndex,
        bool openDungeonAfterTravel)
    {
        WorldMapService.TravelValidationResult validation =
            WorldMapService.ValidateTravelRequest(
                townProgressState.CurrentTownIndex,
                townIndex,
                townProgressState.GetUnlockedTownIndices(),
                partyManager.Members.Count > 0,
                IsGateTownFullyCleared);
        if (!validation.CanTravel)
        {
            setStatus(validation.FailureMessage);
            return;
        }

        confirmationTravelTownIndex = townIndex;
        confirmationOpenDungeonAfterTravel = openDungeonAfterTravel;
        bool hiddenIslandRoute =
            townIndex == WorldMapService.HiddenIslandTownIndex ||
            townProgressState.CurrentTownIndex ==
                WorldMapService.HiddenIslandTownIndex;
        if (hiddenIslandRoute)
        {
            showTravelConfirmation(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]} から\n" +
                $"{WorldMapService.TownNames[townIndex]} へ移動します。\n\n" +
                "・発見済みの中央島航路を利用します\n" +
                "・街道戦闘と日数経過はありません");
            return;
        }

        string unlockNotice = validation.IsUnlockTravel
            ? "\n勝利すると新しい町が解放されます。"
            : string.Empty;
        showTravelConfirmation(
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]} から\n" +
            $"{WorldMapService.TownNames[townIndex]} へ移動します。\n\n" +
            $"・両地域の通常モンスターと3～5回接敵します\n" +
            $"・勝利すると1日経過します" +
            unlockNotice);
    }

    public void ConfirmTownTravel()
    {
        int destinationTownIndex = confirmationTravelTownIndex;
        bool openDungeonAfterTravel = confirmationOpenDungeonAfterTravel;
        hideTravelConfirmation();

        if (destinationTownIndex < 0)
        {
            return;
        }

        StartTownTravelBattle(
            destinationTownIndex,
            openDungeonAfterTravel);
    }

    public void ClearTravelConfirmation()
    {
        confirmationTravelTownIndex = -1;
        confirmationOpenDungeonAfterTravel = false;
    }

    private void StartTownTravelBattle(
        int destinationTownIndex,
        bool openDungeonAfterTravel)
    {
        if (destinationTownIndex == WorldMapService.HiddenIslandTownIndex ||
            townProgressState.CurrentTownIndex ==
                WorldMapService.HiddenIslandTownIndex)
        {
            CompleteHiddenIslandTravel(
                destinationTownIndex,
                openDungeonAfterTravel);
            return;
        }

        if (battleManager.IsBattling)
        {
            setStatus("戦闘中は町を移動できません。");
            return;
        }

        if (partyManager.Members.Count == 0)
        {
            setStatus(
                $"{WorldMapService.TownNames[destinationTownIndex]}への移動クエストには傭兵の編成が必要です。");
            return;
        }

        if (!WorldMapService.AreTownsAdjacent(destinationTownIndex, townProgressState.CurrentTownIndex))
        {
            setStatus("街道で結ばれていない町へは移動できません。");
            return;
        }

        resetBattleLog();
        roadTravelState.Begin(
            destinationTownIndex,
            !townProgressState.IsTownUnlocked(destinationTownIndex),
            openDungeonAfterTravel,
            UnityEngine.Random.Range(3, 6));
        List<EnemyDataSO> enemies =
            roadEncounterService.CreateEncounter(
                townProgressState.CurrentTownIndex,
                destinationTownIndex,
                out bool containsRareEncounter);
        roadTravelState.SetRareEncounter(containsRareEncounter);
        battleManager.SetNextBattleBackground(
            null,
            BuildRoadBattleBackgroundKey(
                townProgressState.CurrentTownIndex,
                destinationTownIndex));

        if (enemies == null ||
            enemies.Count == 0 ||
            !battleManager.StartBattle(partyManager.Members, enemies))
        {
            roadTravelState.Clear();
            setStatus("街道戦闘を開始できませんでした。");
            return;
        }

        showRoadBattlePage(townProgressState.CurrentTownIndex, destinationTownIndex);
        setStatus(
            $"町の移動: 街道戦闘 {roadTravelState.EncounterIndex}/" +
            $"{roadTravelState.EncounterCount}");
    }

    private void CompleteHiddenIslandTravel(
        int destinationTownIndex,
        bool openDungeonAfterTravel)
    {
        townProgressState.SetCurrentTown(destinationTownIndex);
        townProgressState.ViewedWorldMapIndex =
            townProgressState.CurrentWorldMapIndex;
        dungeonRunManager.SetCurrentWorldMapIndex(
            townProgressState.ViewedWorldMapIndex);
        ApplyTownServiceSettings(false, false);
        syncDungeonUnlocks();
        refreshTownMapButtons();

        if (openDungeonAfterTravel)
        {
            openNearbyDungeon();
        }
        else
        {
            showTownMap();
        }

        setStatus(
            $"中央島航路を利用し、{WorldMapService.TownNames[destinationTownIndex]}へ移動しました。");
        saveManager?.SaveGame();
    }

    /// <summary>
    /// Body of the former ContinueTownTravelBattleRoutine coroutine after
    /// its frame delay. The coroutine shell stays in
    /// SimpleMercenaryHireUI.Map.cs and calls this after one frame.
    /// </summary>
    public void StartNextTravelEncounter()
    {
        int destinationTownIndex = roadTravelState.DestinationTownIndex;
        if (destinationTownIndex < 0)
        {
            return;
        }

        List<EnemyDataSO> enemies =
            roadEncounterService.CreateEncounter(
                townProgressState.CurrentTownIndex,
                destinationTownIndex,
                out bool containsRareEncounter);
        roadTravelState.SetRareEncounter(containsRareEncounter);
        battleManager.SetNextBattleBackground(
            null,
            BuildRoadBattleBackgroundKey(
                townProgressState.CurrentTownIndex,
                destinationTownIndex));
        if (enemies == null ||
            enemies.Count == 0 ||
            !battleManager.StartBattle(partyManager.Members, enemies))
        {
            roadTravelState.Clear();
            showWorldMap();
            setStatus("次の街道戦闘を開始できませんでした。");
            return;
        }

        showRoadBattlePage(townProgressState.CurrentTownIndex, destinationTownIndex);
        setStatus(
            $"町の移動: 街道戦闘 {roadTravelState.EncounterIndex}/" +
            $"{roadTravelState.EncounterCount}");
    }

    public void ContinueTownTravel()
    {
        if (!roadTravelState.IsAwaitingChoice ||
            !roadTravelState.IsActive ||
            battleManager.IsBattling)
        {
            return;
        }

        if (!roadTravelState.ContinueToNextEncounter())
        {
            return;
        }

        setRoadChoiceButtonsActive(false);
        setStatus(
            $"街道戦闘 {roadTravelState.EncounterIndex}/" +
            $"{roadTravelState.EncounterCount} の敵が接近しています。");
        continueTravelBattle();
    }

    public void RetreatFromTownTravel()
    {
        if (!roadTravelState.IsAwaitingChoice || battleManager.IsBattling)
        {
            return;
        }

        roadTravelState.Clear();
        setRoadChoiceButtonsActive(false);
        showTownMap();
        setStatus(
            "街道から撤退し、出発した町へ戻りました。");
    }

    /// <summary>
    /// Road-travel branch of the former HandleBattleCompleted. Returns true
    /// when a road travel was active and the outcome has been handled;
    /// false when no road travel is in progress (normal battle completion).
    /// The statement order of the victory sequence (UnlockTown →
    /// SetCurrentTown → ViewedWorldMapIndex → SetCurrentWorldMapIndex →
    /// ApplyTownServiceSettings → AdvanceDay → SyncDungeonUnlocks →
    /// RefreshTownMapButtons) is load-bearing; do not reorder.
    /// </summary>
    public bool HandleRoadBattleOutcome(bool victory)
    {
        if (!roadTravelState.IsActive)
        {
            return false;
        }

        if (victory && roadTravelState.ShouldAskToContinueAfterVictory())
        {
            roadTravelState.AwaitChoice();
            setRoadChoiceButtonsActive(true);
            setRoadBattleRouteText(
                $"接敵 {roadTravelState.EncounterIndex}/" +
                $"{roadTravelState.EncounterCount} を突破しました。\n" +
                "次の区間へ進むか、出発した町へ撤退してください。");
            setStatus("街道戦闘を続行しますか？");
            return true;
        }

        RoadTravelCompletionResult travelResult =
            RoadTravelCompletionService.Complete(
                victory,
                townProgressState.CurrentTownIndex,
                roadTravelState);
        roadTravelState.Clear();

        if (!travelResult.IsValid)
        {
            showWorldMap();
            setStatus("街道戦闘の結果を処理できませんでした。");
            return true;
        }

        if (travelResult.Victory)
        {
            townProgressState.UnlockTown(travelResult.DestinationTownIndex);
            townProgressState.SetCurrentTown(travelResult.NewCurrentTownIndex);
            townProgressState.ViewedWorldMapIndex = travelResult.NewWorldMapIndex;
            dungeonRunManager.SetCurrentWorldMapIndex(
                townProgressState.ViewedWorldMapIndex);
            ApplyTownServiceSettings(false, false);
            if (travelResult.ShouldAdvanceDay)
            {
                dayManager.AdvanceDay();
            }
            syncDungeonUnlocks();
            refreshTownMapButtons();
            if (travelResult.OpenDungeonAfterTravel)
            {
                openNearbyDungeon();
            }
            else
            {
                showTownMap();
            }
            setStatus(travelResult.StatusMessage);
            if (travelResult.ShouldSave)
            {
                saveManager?.SaveGame();
            }
        }
        else
        {
            showWorldMap();
            setStatus(travelResult.StatusMessage);
        }
        return true;
    }

    public bool CanEnterWorldRegion(int worldMapIndex)
    {
        return WorldMapService.CanEnterWorldRegion(
            worldMapIndex,
            townProgressState.CurrentTownIndex,
            townProgressState.GetUnlockedTownIndices(),
            IsGateTownFullyCleared);
    }

    private bool IsGateTownFullyCleared(int gateTownIndex)
    {
        DungeonDataSO gateDungeon =
            dungeonRunManager.GetHighestGradeDungeonNearTown(gateTownIndex);
        return gateDungeon != null &&
               dungeonRunManager.GetClearedFloors(gateDungeon) >=
               Mathf.Max(1, gateDungeon.totalFloors);
    }

    private static string BuildRoadBattleBackgroundKey(
        int firstTownIndex,
        int secondTownIndex)
    {
        int lower = Mathf.Min(firstTownIndex, secondTownIndex);
        int upper = Mathf.Max(firstTownIndex, secondTownIndex);
        return $"Road_{lower}_{upper}";
    }

    public void ApplyTownServiceSettings(
        bool regenerateCandidates,
        bool regenerateMarket)
    {
        if (mercenaryGenerator != null)
        {
            mercenaryGenerator.SetTownIndex(townProgressState.CurrentTownIndex, false);
            if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
            {
                mercenaryGenerator.ClearCandidates();
            }
            else if (regenerateCandidates)
            {
                mercenaryGenerator.GenerateCandidates();
            }
        }
        marketStockManager?.SetTownIndex(
            townProgressState.CurrentTownIndex, regenerateMarket);
        blacksmithManager?.SetTownIndex(townProgressState.CurrentTownIndex);
    }
}
