using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void HandleBattleMessage(string message, BattleLogType logType)
    {
        AppendBattleMessage(message, logType);
    }

    private void HandlePresentationLog(string message, BattleLogType logType)
    {
        AppendBattleMessage(message, logType);
    }

    private void HandlePresentationSound(BattleSoundCue soundCue)
    {
        battleDungeonPresenter.HandlePresentationSound(soundCue);
    }

    private void ScrollBattleLogToLatest()
    {
        if (battleView.logScrollRect == null)
        {
            return;
        }

        if (battleLogScrollCoroutine != null)
        {
            StopCoroutine(battleLogScrollCoroutine);
        }

        battleLogScrollCoroutine = StartCoroutine(ScrollBattleLogToLatestRoutine());
    }

    private IEnumerator ScrollBattleLogToLatestRoutine()
    {
        yield return null;
        UpdateBattleLogContentHeight();
        Canvas.ForceUpdateCanvases();
        battleDungeonPresenter.ScrollBattleLogToLatestView();
        battleLogScrollCoroutine = null;
    }

    private void HandleBattleCompleted(bool victory)
    {
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !IsProgressionLocked;
        RefreshPageOrMarkDirty(companyPage);
        RefreshPageOrMarkDirty(partyPage);
        RefreshPageOrMarkDirty(healPage);
        RefreshUI();

        if (townTravelController.RoadTravelState.IsActive &&
            battleVisualController != null &&
            battleVisualController.isActiveAndEnabled &&
            battleVisualController.IsPresentationBusy)
        {
            hasPendingRoadBattleOutcome = true;
            pendingRoadBattleVictory = victory;
            roadBattle.continueButton?.gameObject.SetActive(false);
            roadBattle.retreatButton?.gameObject.SetActive(false);
            RefreshUI();
            if (pendingRoadBattleOutcomeCoroutine == null)
            {
                pendingRoadBattleOutcomeCoroutine = StartCoroutine(
                    WaitForRoadBattlePresentationCompletion());
            }
            return;
        }

        if (townTravelController.HandleRoadBattleOutcome(victory))
        {
            return;
        }

        statusText.text = victory ? "戦闘に勝利しました。" : "戦闘に敗北しました。";
    }

    private void HandleDungeonMessage(string message)
    {
        statusText.text = message;
        if (dungeonView.statusText != null)
        {
            dungeonView.statusText.text = message;
        }
    }

    private void HandleDungeonStateChanged()
    {
        UpdateDungeonEventUI();
        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            ShowBattlePage();
            if (battleVisualController != null &&
                battleVisualController.IsPresentationBusy &&
                dungeonEventPresentationCoroutine == null)
            {
                dungeonEventPresentationCoroutine = StartCoroutine(
                    WaitForDungeonEventPresentationCompletion());
            }
        }
        else if (!dungeonRunManager.IsRunning)
        {
            RefreshPage(dungeonPage);
        }

        RefreshUI();
    }

    private IEnumerator WaitForDungeonEventPresentationCompletion()
    {
        const float timeoutSeconds = 8f;
        float elapsed = 0f;
        while (dungeonRunManager.IsAwaitingEventChoice &&
               battleVisualController != null &&
               battleVisualController.IsPresentationBusy &&
               elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (dungeonRunManager.IsAwaitingEventChoice &&
            battleVisualController != null &&
            battleVisualController.IsPresentationBusy)
        {
            Debug.LogWarning(
                "Battle presentation did not complete before a dungeon event. " +
                "Finishing it immediately so exploration can continue.",
                this);
            battleVisualController.FinishPresentationImmediately();
        }

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            UpdateDungeonEventUI();
            ShowBattlePage();
        }

        dungeonEventPresentationCoroutine = null;
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        if (battleVisualController != null &&
            battleVisualController.IsPresentationBusy)
        {
            hasPendingDungeonCompletion = true;
            pendingDungeonCompletionCleared = cleared;
            dungeonView.eventPanel?.gameObject.SetActive(false);
            ShowBattlePage();
            if (pendingDungeonCompletionCoroutine == null)
            {
                pendingDungeonCompletionCoroutine = StartCoroutine(
                    WaitForDungeonPresentationCompletion());
            }
            return;
        }

        ShowDungeonCompletionResult(cleared);
    }

    private void HandleBattleVisualPresentationCompleted()
    {
        if (hasPendingRoadBattleOutcome)
        {
            CompletePendingRoadBattleOutcome();
        }

        if (hasPendingDungeonCompletion)
        {
            CompletePendingDungeonResult();
        }

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            UpdateDungeonEventUI();
            ShowBattlePage();
        }

        ShowPendingDailyResultIfReady();
    }

    private IEnumerator WaitForRoadBattlePresentationCompletion()
    {
        const float stalledPresentationTimeoutSeconds = 30f;
        float stalledElapsed = 0f;
        int lastProgressVersion = battleVisualController != null
            ? battleVisualController.PresentationProgressVersion
            : 0;
        while (hasPendingRoadBattleOutcome &&
               battleVisualController != null &&
               battleVisualController.IsPresentationBusy)
        {
            if (battleVisualController.PresentationProgressVersion !=
                lastProgressVersion)
            {
                lastProgressVersion =
                    battleVisualController.PresentationProgressVersion;
                stalledElapsed = 0f;
            }
            else if (battleManager == null || !battleManager.IsPaused)
            {
                stalledElapsed += Time.unscaledDeltaTime;
            }

            if (stalledElapsed >= stalledPresentationTimeoutSeconds)
            {
                break;
            }

            yield return null;
        }

        if (hasPendingRoadBattleOutcome)
        {
            try
            {
                if (battleVisualController != null &&
                    battleVisualController.IsPresentationBusy)
                {
                    Debug.LogWarning(
                        "Road battle presentation stalled. Completing it so travel can continue.",
                        this);
                    battleVisualController.FinishPresentationImmediately();
                }
            }
            finally
            {
                CompletePendingRoadBattleOutcome();
            }
        }

        pendingRoadBattleOutcomeCoroutine = null;
    }

    private void CompletePendingRoadBattleOutcome()
    {
        if (!hasPendingRoadBattleOutcome)
        {
            return;
        }

        bool victory = pendingRoadBattleVictory;
        hasPendingRoadBattleOutcome = false;
        pendingRoadBattleVictory = false;
        townTravelController.HandleRoadBattleOutcome(victory);
        ShowPendingDailyResultIfReady();
    }

    private IEnumerator WaitForDungeonPresentationCompletion()
    {
        const float stalledPresentationTimeoutSeconds = 30f;
        float stalledElapsed = 0f;
        int lastProgressVersion = battleVisualController != null
            ? battleVisualController.PresentationProgressVersion
            : 0;
        while (hasPendingDungeonCompletion &&
               battleVisualController != null &&
               battleVisualController.IsPresentationBusy)
        {
            if (battleVisualController.PresentationProgressVersion !=
                lastProgressVersion)
            {
                lastProgressVersion =
                    battleVisualController.PresentationProgressVersion;
                stalledElapsed = 0f;
            }
            else if (battleManager == null || !battleManager.IsPaused)
            {
                stalledElapsed += Time.unscaledDeltaTime;
            }

            if (stalledElapsed >= stalledPresentationTimeoutSeconds)
            {
                break;
            }

            yield return null;
        }

        if (hasPendingDungeonCompletion &&
            battleVisualController != null &&
            battleVisualController.IsPresentationBusy)
        {
            Debug.LogWarning(
                "Battle presentation did not complete. " +
                "Finishing it immediately so dungeon progression can continue.",
                this);
            battleVisualController.FinishPresentationImmediately();
        }

        if (hasPendingDungeonCompletion)
        {
            CompletePendingDungeonResult();
        }
        ShowPendingDailyResultIfReady();
        pendingDungeonCompletionCoroutine = null;
    }

    private void CompletePendingDungeonResult()
    {
        bool cleared = pendingDungeonCompletionCleared;
        hasPendingDungeonCompletion = false;
        ShowDungeonCompletionResult(cleared);
    }

    private void ShowDungeonCompletionResult(bool cleared)
    {
        bool hiddenIslandUnlocked = TryUnlockHiddenIsland();
        string result = progressionManager != null
            ? progressionManager.LastExplorationResult
            : string.Empty;
        statusText.text = cleared
            ? dungeonRunManager.IsSelectedDungeonFullyCleared
                ? "ダンジョンを完全攻略しました。"
                : $"フロアを攻略しました。次回は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "ダンジョン探索を終了しました。";
        if (!string.IsNullOrEmpty(result))
        {
            statusText.text += $" {result}";
        }
        if (hiddenIslandUnlocked)
        {
            statusText.text =
                "全条件を達成しました。全体マップ中央に新たな島が出現しました。";
        }
        ShowDungeonPage();
        bool fullyCleared =
            dungeonRunManager.IsSelectedDungeonFullyCleared;
        dungeonView.resultText.text = cleared
            ? fullyCleared
                ? $"{dungeonRunManager.DungeonName}\n完全攻略！\n\n" +
                  "すべてのフロアを攻略しました。"
                : $"フロア攻略完了\n\n" +
                  $"次は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "探索終了\n\n町へ戻って態勢を整えましょう。";
        dungeonView.nextFloorButton.gameObject.SetActive(
            cleared && !fullyCleared);
        dungeonView.resultPanel.SetAsLastSibling();
        dungeonView.resultPanel.gameObject.SetActive(true);
        UpdateDungeonEventUI();
        dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
        RefreshUI();
    }

    private void ShowBattlePage()
    {
        MoveBattleLogTo(battlePage);
        SwitchToPage(battlePage, battleTabButton);
    }

    private void RefreshBattlePage()
    {
        UpdateDungeonEventUI();
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !IsProgressionLocked;
        startBattleButton.gameObject.SetActive(false);
        battleView.skipButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        battleView.pauseButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        SetButtonLabel(
            battleView.pauseButton,
            battleManager.IsPaused ? "再開" : "一時停止");
        statusText.text = $"戦闘参加: 傭兵{partyManager.Members.Count}人";
    }

    private void ShowRoadBattlePage(
        int originTownIndex,
        int destinationTownIndex)
    {
        if (townTravelController == null ||
            !townTravelController.RoadTravelState.IsActive ||
            string.IsNullOrEmpty(WorldMapService.GetTownName(originTownIndex)) ||
            string.IsNullOrEmpty(
                WorldMapService.GetTownName(destinationTownIndex)))
        {
            return;
        }

        displayedRoadOriginTownIndex = originTownIndex;
        displayedRoadDestinationTownIndex = destinationTownIndex;
        MoveBattleLogTo(roadBattlePage);
        SwitchToPage(roadBattlePage);
    }

    private void RefreshRoadBattlePage()
    {
        RoadTravelState roadTravelState = townTravelController.RoadTravelState;
        bool isActive = roadTravelState != null &&
                        roadTravelState.IsActive &&
                        !string.IsNullOrEmpty(WorldMapService.GetTownName(
                            roadTravelState.DestinationTownIndex)) &&
                        !string.IsNullOrEmpty(WorldMapService.GetTownName(
                            displayedRoadOriginTownIndex)) &&
                        !string.IsNullOrEmpty(WorldMapService.GetTownName(
                            displayedRoadDestinationTownIndex));
        mapButton?.gameObject.SetActive(false);
        townMapButton?.gameObject.SetActive(false);
        roadBattle.continueButton.gameObject.SetActive(
            isActive &&
            roadTravelState.IsAwaitingChoice &&
            !hasPendingRoadBattleOutcome);
        roadBattle.retreatButton.gameObject.SetActive(
            isActive &&
            roadTravelState.IsAwaitingChoice &&
            !hasPendingRoadBattleOutcome);
        roadBattle.skipButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        roadBattle.pauseButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        SetButtonLabel(
            roadBattle.pauseButton,
            battleManager.IsPaused ? "再開" : "一時停止");
        string originTownName = WorldMapService.GetTownName(
            displayedRoadOriginTownIndex);
        string destinationTownName = WorldMapService.GetTownName(
            displayedRoadDestinationTownIndex);
        if (!isActive ||
            string.IsNullOrEmpty(originTownName) ||
            string.IsNullOrEmpty(destinationTownName))
        {
            roadBattle.routeText.text = "街道移動は終了しました。";
            return;
        }

        roadBattle.routeText.text =
            $"{originTownName} → {destinationTownName}\n" +
            $"接敵 {roadTravelState.EncounterIndex}/" +
            $"{roadTravelState.EncounterCount}  |  " +
            (roadTravelState.ContainsRareEncounter
                ? "幻獣の気配を確認！"
                : "両地域の通常モンスターが街道を塞いでいます。");
    }

    private void MoveBattleLogTo(RectTransform destinationPage)
    {
        if (battleView.logPanel == null || destinationPage == null)
        {
            return;
        }

        battleView.logPanel.SetParent(destinationPage, false);
        battleView.logPanel.anchorMin = Vector2.zero;
        battleView.logPanel.anchorMax = new Vector2(1f, 0.24f);
        battleView.logPanel.offsetMin = Vector2.zero;
        battleView.logPanel.offsetMax = Vector2.zero;
        battleVisualController?.MoveTo(destinationPage);
    }

    private void ShowDungeonPage()
    {
        SwitchToPage(dungeonPage, dungeonTabButton);
    }

    private void RefreshDungeonPage()
    {
        dungeonView.resultPanel?.gameObject.SetActive(false);
        dungeonBattleController.EnsureNearbyDungeonSelected();

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            dungeonView.statusText.text =
                $"遭遇 {dungeonRunManager.CurrentEncounter}/" +
                $"{dungeonRunManager.EncounterCount} を突破。次の行動を選んでください。";
        }
        else
        {
            dungeonView.statusText.text = dungeonRunManager.IsRunning
                ? $"第{dungeonRunManager.CurrentFloor}/" +
                  $"{dungeonRunManager.TotalFloors}フロア探索中: " +
                  $"{dungeonRunManager.CurrentEncounter}/" +
                  $"{dungeonRunManager.EncounterCount}"
                : $"{dungeonRunManager.DungeonName}  |  " +
                  $"第{dungeonRunManager.CurrentFloor}/" +
                  $"{dungeonRunManager.TotalFloors}フロア  |  " +
                  $"遭遇{dungeonRunManager.EncounterCount}回\n" +
                  $"フロア報酬 " +
                  $"{Mathf.Max(0, dungeonRunManager.SelectedDungeon != null ? dungeonRunManager.SelectedDungeon.floorClearGoldReward : 0)} G  |  " +
                  $"完全攻略報酬 {dungeonRunManager.ClearGoldReward} G\n" +
                  DungeonBattleController.BuildDungeonRewardPreview(
                      dungeonRunManager.SelectedDungeon);
        }

        UpdateDungeonEventUI();
        dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
        statusText.text = $"探索パーティー: 傭兵{partyManager.Members.Count}人";
        RefreshUI();
    }

    private void ContinueToNextDungeonFloor()
    {
        dungeonView.resultPanel?.gameObject.SetActive(false);
        dungeonBattleController.StartDungeonRun();
    }

    private void ReturnToTownAfterDungeon()
    {
        dungeonView.resultPanel?.gameObject.SetActive(false);
        ShowTownMap();
        statusText.text = $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}へ戻りました。";
    }

}
