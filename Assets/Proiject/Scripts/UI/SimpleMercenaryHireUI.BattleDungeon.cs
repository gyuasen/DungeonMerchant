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

}
