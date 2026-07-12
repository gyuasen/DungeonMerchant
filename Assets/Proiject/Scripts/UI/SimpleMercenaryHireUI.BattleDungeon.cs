using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildBattlePage()
    {
        battlePageTitleText = CreateText(
            battlePage, "ダンジョン戦闘", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);

        battleEncounterText = CreateText(
            battlePage, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f),
            ParchmentTextColor);

        startBattleButton = CreateActionButton(
            battlePage,
            "開始",
            () => dungeonBattleController.StartPartyBattle());
        RectTransform startRect = startBattleButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);
        startBattleButton.gameObject.SetActive(false);

        battleSpeedButton =
            CreateActionButton(
                battlePage,
                "速度 x1",
                () => dungeonBattleController.CycleBattleSpeed());
        RectTransform battleSpeedRect =
            battleSpeedButton.GetComponent<RectTransform>();
        battleSpeedRect.anchorMin = battleSpeedRect.anchorMax =
            new Vector2(1f, 1f);
        battleSpeedRect.pivot = new Vector2(1f, 1f);
        battleSpeedRect.sizeDelta = new Vector2(100f, 38f);
        battleSpeedRect.anchoredPosition = new Vector2(-140f, -36f);

        battleLogPanel = CreateUIObject("Battle Log", battlePage);
        battleLogPanel.anchorMin = new Vector2(0f, 0f);
        battleLogPanel.anchorMax = new Vector2(1f, 1f);
        battleLogPanel.offsetMin = Vector2.zero;
        battleLogPanel.offsetMax = new Vector2(0f, -104f);

        Image logBackground = battleLogPanel.gameObject.AddComponent<Image>();
        logBackground.color = RowColor;

        battleLogViewport =
            CreateUIObject("Battle Log Viewport", battleLogPanel);
        battleLogViewport.anchorMin = Vector2.zero;
        battleLogViewport.anchorMax = Vector2.one;
        battleLogViewport.offsetMin = new Vector2(16f, 16f);
        battleLogViewport.offsetMax = new Vector2(-16f, -16f);

        Image viewportImage = battleLogViewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = battleLogViewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        battleLogContent = CreateUIObject("Battle Log Content", battleLogViewport);
        battleLogContent.anchorMin = new Vector2(0f, 1f);
        battleLogContent.anchorMax = new Vector2(1f, 1f);
        battleLogContent.pivot = new Vector2(0.5f, 1f);
        battleLogContent.anchoredPosition = Vector2.zero;
        battleLogContent.sizeDelta = new Vector2(0f, 430f);

        battleLogScrollRect = battleLogViewport.gameObject.AddComponent<ScrollRect>();
        battleLogScrollRect.content = battleLogContent;
        battleLogScrollRect.viewport = battleLogViewport;
        battleLogScrollRect.horizontal = false;
        battleLogScrollRect.vertical = true;
        battleLogScrollRect.movementType = ScrollRect.MovementType.Clamped;
        battleLogScrollRect.scrollSensitivity = 28f;

        battleLogText = CreateText(battleLogContent, "戦闘準備完了。", 14, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(16f, 16f), new Vector2(-16f, -16f),
            MutedTextColor);
        battleLogText.supportRichText = true;
        battleLogText.rectTransform.anchorMin = Vector2.zero;
        battleLogText.rectTransform.anchorMax = Vector2.one;
        battleLogText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        battleLogText.rectTransform.offsetMin = new Vector2(0f, 8f);
        battleLogText.rectTransform.offsetMax = new Vector2(0f, -8f);

        BattlePageUI pageUI =
            battlePage.GetComponent<BattlePageUI>() ??
            battlePage.gameObject.AddComponent<BattlePageUI>();
        pageUI.Configure(RefreshBattlePage);
        pageRouter.Register(battlePage);
    }

    private void BuildRoadBattlePage()
    {
        CreateText(
            roadBattlePage,
            "街道戦闘",
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -38f),
            new Vector2(0f, 0f),
            ParchmentTextColor);

        roadBattleRouteText = CreateText(
            roadBattlePage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -82f),
            new Vector2(0f, -42f),
            ParchmentTextColor);

        roadSpeedButton =
            CreateActionButton(
                roadBattlePage,
                "速度 x1",
                () => dungeonBattleController.CycleBattleSpeed());
        RectTransform roadSpeedRect =
            roadSpeedButton.GetComponent<RectTransform>();
        roadSpeedRect.anchorMin = roadSpeedRect.anchorMax =
            new Vector2(1f, 1f);
        roadSpeedRect.pivot = new Vector2(1f, 1f);
        roadSpeedRect.sizeDelta = new Vector2(100f, 38f);
        roadSpeedRect.anchoredPosition = new Vector2(-270f, -4f);

        roadContinueButton =
            CreateActionButton(
                roadBattlePage,
                "次へ進む",
                () => townTravelController.ContinueTownTravel());
        RectTransform continueRect =
            roadContinueButton.GetComponent<RectTransform>();
        continueRect.anchorMin = continueRect.anchorMax =
            new Vector2(1f, 1f);
        continueRect.pivot = new Vector2(1f, 1f);
        continueRect.sizeDelta = new Vector2(120f, 40f);
        continueRect.anchoredPosition = new Vector2(-130f, -4f);

        roadRetreatButton =
            CreateActionButton(
                roadBattlePage,
                "撤退する",
                () => townTravelController.RetreatFromTownTravel());
        RectTransform retreatRect =
            roadRetreatButton.GetComponent<RectTransform>();
        retreatRect.anchorMin = retreatRect.anchorMax =
            new Vector2(1f, 1f);
        retreatRect.pivot = new Vector2(1f, 1f);
        retreatRect.sizeDelta = new Vector2(120f, 40f);
        retreatRect.anchoredPosition = new Vector2(0f, -4f);
        roadRetreatButton.targetGraphic.color = ImportantButtonColor;

        roadContinueButton.gameObject.SetActive(false);
        roadRetreatButton.gameObject.SetActive(false);

        RoadBattlePageUI pageUI =
            roadBattlePage.GetComponent<RoadBattlePageUI>() ??
            roadBattlePage.gameObject.AddComponent<RoadBattlePageUI>();
        pageUI.Configure(RefreshRoadBattlePage);
        pageRouter.Register(roadBattlePage);
    }

    private void BuildDungeonPage()
    {
        CreateText(dungeonPage, "ダンジョン探索", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        dungeonStatusText = CreateText(
            dungeonPage,
            "パーティーを編成してダンジョンへ向かいましょう。",
            14,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -154f),
            new Vector2(-170f, -42f),
            ParchmentTextColor);

        startDungeonButton = CreateActionButton(
            dungeonPage,
            "探索開始",
            () => dungeonBattleController.StartDungeonRun());
        RectTransform startRect = startDungeonButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);

        dungeonSelectionList = CreateUIObject("Dungeon Selection List", dungeonPage);
        dungeonSelectionList.anchorMin = new Vector2(0f, 0f);
        dungeonSelectionList.anchorMax = new Vector2(1f, 1f);
        dungeonSelectionList.offsetMin = Vector2.zero;
        dungeonSelectionList.offsetMax = new Vector2(0f, -174f);

        dungeonEventTitleText = CreateText(
            dungeonPage,
            string.Empty,
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -230f),
            new Vector2(0f, -184f),
            ParchmentTextColor);

        dungeonEventDescriptionText = CreateText(
            dungeonPage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, -300f),
            new Vector2(0f, -238f),
            ParchmentTextColor);

        firstDungeonEventButton = CreateActionButton(
            dungeonPage,
            "選択肢1",
            () => dungeonBattleController.ChooseDungeonEventOption(0));
        PositionDungeonEventButton(firstDungeonEventButton, 0f);

        secondDungeonEventButton = CreateActionButton(
            dungeonPage,
            "選択肢2",
            () => dungeonBattleController.ChooseDungeonEventOption(1));
        PositionDungeonEventButton(secondDungeonEventButton, 248f);

        thirdDungeonEventButton = CreateActionButton(
            dungeonPage,
            "撤退",
            () => dungeonBattleController.ChooseDungeonEventOption(2));
        PositionDungeonEventButton(thirdDungeonEventButton, 496f);

        dungeonResultPanel =
            CreateUIObject("Dungeon Floor Result", dungeonPage);
        dungeonResultPanel.anchorMin = Vector2.zero;
        dungeonResultPanel.anchorMax = Vector2.one;
        dungeonResultPanel.offsetMin = new Vector2(40f, 42f);
        dungeonResultPanel.offsetMax = new Vector2(-40f, -42f);
        Image resultBackground =
            dungeonResultPanel.gameObject.AddComponent<Image>();
        resultBackground.color = RowColor;
        AddFantasyFrame(resultBackground, 2f);

        dungeonResultText = CreateText(
            dungeonResultPanel,
            string.Empty,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(36f, 100f),
            new Vector2(-36f, -70f),
            ButtonTextColor);
        dungeonResultText.rectTransform.anchorMin = Vector2.zero;
        dungeonResultText.rectTransform.anchorMax = Vector2.one;

        dungeonNextFloorButton = CreateActionButton(
            dungeonResultPanel,
            "次のフロアへ進む",
            ContinueToNextDungeonFloor);
        RectTransform nextFloorRect =
            dungeonNextFloorButton.GetComponent<RectTransform>();
        nextFloorRect.anchorMin = nextFloorRect.anchorMax =
            new Vector2(0.5f, 0f);
        nextFloorRect.pivot = new Vector2(0.5f, 0f);
        nextFloorRect.sizeDelta = new Vector2(220f, 50f);
        nextFloorRect.anchoredPosition = new Vector2(-125f, 28f);

        Button returnTownButton = CreateActionButton(
            dungeonResultPanel,
            "町へ戻る",
            ReturnToTownAfterDungeon);
        RectTransform returnTownRect =
            returnTownButton.GetComponent<RectTransform>();
        returnTownRect.anchorMin = returnTownRect.anchorMax =
            new Vector2(0.5f, 0f);
        returnTownRect.pivot = new Vector2(0.5f, 0f);
        returnTownRect.sizeDelta = new Vector2(220f, 50f);
        returnTownRect.anchoredPosition = new Vector2(125f, 28f);

        dungeonResultPanel.gameObject.SetActive(false);

        UpdateDungeonEventUI();

        DungeonPageUI pageUI =
            dungeonPage.GetComponent<DungeonPageUI>() ??
            dungeonPage.gameObject.AddComponent<DungeonPageUI>();
        pageUI.Configure(RefreshDungeonPage);
        pageUI.ConfigureSelectionList(
            dungeonSelectionList,
            uiFont,
            Color.white,
            ParchmentTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            ButtonTextColor,
            () => dungeonRunManager.AvailableDungeons,
            () => townProgressState.CurrentTownIndex,
            WorldMapService.GetTownName,
            dungeonRunManager.GetClearedFloors,
            dungeonRunManager.IsDungeonUnlocked,
            () => dungeonRunManager.SelectedDungeon,
            dungeonBattleController.SelectDungeon);
        pageRouter.Register(dungeonPage);
        RefreshPage(dungeonPage);
    }

    private static void PositionDungeonEventButton(Button button, float x)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(228f, 52f);
        rect.anchoredPosition = new Vector2(x, 18f);
    }

    private void ResetBattleLog()
    {
        dungeonBattleController.ClearBattleLog();
        battleLogText.text = string.Empty;

        if (battleLogScrollCoroutine != null)
        {
            StopCoroutine(battleLogScrollCoroutine);
            battleLogScrollCoroutine = null;
        }

        Canvas.ForceUpdateCanvases();
        float viewportHeight = battleLogViewport != null
            ? battleLogViewport.rect.height
            : 0f;
        battleLogContent.sizeDelta = new Vector2(0f, Mathf.Max(1f, viewportHeight));
        battleLogContent.anchoredPosition = Vector2.zero;

        if (battleLogScrollRect != null)
        {
            battleLogScrollRect.StopMovement();
            battleLogScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void HandleBattleMessage(string message, BattleLogType logType)
    {
        battleLogText.text =
            dungeonBattleController.AppendBattleMessage(message, logType);

        if (battleLogContent != null)
        {
            UpdateBattleLogContentHeight();
        }

        ScrollBattleLogToLatest();
    }

    private void UpdateBattleLogContentHeight()
    {
        if (battleLogContent == null || battleLogText == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        float viewportHeight = battleLogViewport != null
            ? battleLogViewport.rect.height
            : 0f;
        float height = Mathf.Max(viewportHeight, battleLogText.preferredHeight + 32f);
        battleLogContent.sizeDelta = new Vector2(0f, height);
    }

    private void ScrollBattleLogToLatest()
    {
        if (battleLogScrollRect == null)
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
        battleLogScrollRect.verticalNormalizedPosition = 0f;
        battleLogScrollCoroutine = null;
    }

    private void HandleBattleCompleted(bool victory)
    {
        startBattleButton.interactable = partyManager.Members.Count > 0;
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        RefreshPage(healPage);
        RefreshUI();

        if (townTravelController.HandleRoadBattleOutcome(victory))
        {
            return;
        }

        statusText.text = victory ? "戦闘に勝利しました。" : "戦闘に敗北しました。";
    }

    private void HandleDungeonMessage(string message)
    {
        statusText.text = message;
        if (dungeonStatusText != null)
        {
            dungeonStatusText.text = message;
        }
    }

    private void HandleDungeonStateChanged()
    {
        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            ShowDungeonPage();
        }
        else
        {
            RefreshPage(dungeonPage);
        }

        RefreshUI();
    }

    private void HandleDungeonCompleted(bool cleared)
    {
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
        ShowDungeonPage();
        bool fullyCleared =
            dungeonRunManager.IsSelectedDungeonFullyCleared;
        dungeonResultText.text = cleared
            ? fullyCleared
                ? $"{dungeonRunManager.DungeonName}\n完全攻略！\n\n" +
                  "すべてのフロアを攻略しました。"
                : $"フロア攻略完了\n\n" +
                  $"次は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "探索終了\n\n町へ戻って態勢を整えましょう。";
        dungeonNextFloorButton.gameObject.SetActive(
            cleared && !fullyCleared);
        dungeonResultPanel.SetAsLastSibling();
        dungeonResultPanel.gameObject.SetActive(true);
        UpdateDungeonEventUI();
        dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
        RefreshUI();
    }

    private void UpdateDungeonEventUI()
    {
        if (dungeonEventTitleText == null ||
            dungeonEventDescriptionText == null ||
            firstDungeonEventButton == null ||
            secondDungeonEventButton == null ||
            thirdDungeonEventButton == null)
        {
            return;
        }

        bool showEvent = dungeonRunManager.IsAwaitingEventChoice;
        if (dungeonSelectionList != null)
        {
            dungeonSelectionList.gameObject.SetActive(!dungeonRunManager.IsRunning);
        }

        dungeonEventTitleText.gameObject.SetActive(showEvent);
        dungeonEventDescriptionText.gameObject.SetActive(showEvent);
        firstDungeonEventButton.gameObject.SetActive(showEvent);
        secondDungeonEventButton.gameObject.SetActive(showEvent);
        thirdDungeonEventButton.gameObject.SetActive(showEvent);

        if (!showEvent)
        {
            return;
        }

        dungeonEventTitleText.text = dungeonRunManager.EventTitle;
        dungeonEventDescriptionText.text = dungeonRunManager.EventDescription;
        SetButtonLabel(firstDungeonEventButton, dungeonRunManager.FirstOptionLabel);
        SetButtonLabel(secondDungeonEventButton, dungeonRunManager.SecondOptionLabel);
        SetButtonLabel(thirdDungeonEventButton, dungeonRunManager.ThirdOptionLabel);
    }

    private static void SetButtonLabel(Button button, string label)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = label;
        }
    }

    private void ShowBattlePage()
    {
        MoveBattleLogTo(battlePage);
        SwitchToPage(battlePage, battleTabButton);
    }

    private void RefreshBattlePage()
    {
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !battleManager.IsBattling;
        startBattleButton.gameObject.SetActive(false);
        statusText.text = $"戦闘参加: 傭兵{partyManager.Members.Count}人";
    }

    private void ShowRoadBattlePage(
        int originTownIndex,
        int destinationTownIndex)
    {
        MoveBattleLogTo(roadBattlePage);
        SwitchToPage(roadBattlePage);
    }

    private void RefreshRoadBattlePage()
    {
        RoadTravelState roadTravelState = townTravelController.RoadTravelState;
        mapButton?.gameObject.SetActive(false);
        townMapButton?.gameObject.SetActive(false);
        roadContinueButton.gameObject.SetActive(
            roadTravelState.IsAwaitingChoice);
        roadRetreatButton.gameObject.SetActive(
            roadTravelState.IsAwaitingChoice);
        roadBattleRouteText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]} → " +
            $"{WorldMapService.TownNames[roadTravelState.DestinationTownIndex]}\n" +
            $"接敵 {roadTravelState.EncounterIndex}/" +
            $"{roadTravelState.EncounterCount}  |  " +
            (roadTravelState.ContainsRareEncounter
                ? "幻獣の気配を確認！"
                : "両地域の通常モンスターが街道を塞いでいます。");
    }

    private void MoveBattleLogTo(RectTransform destinationPage)
    {
        if (battleLogPanel == null || destinationPage == null)
        {
            return;
        }

        battleLogPanel.SetParent(destinationPage, false);
        battleLogPanel.anchorMin = Vector2.zero;
        battleLogPanel.anchorMax = Vector2.one;
        battleLogPanel.offsetMin = Vector2.zero;
        battleLogPanel.offsetMax = new Vector2(0f, -104f);
    }

    private void ShowDungeonPage()
    {
        SwitchToPage(dungeonPage, dungeonTabButton);
    }

    private void RefreshDungeonPage()
    {
        dungeonResultPanel?.gameObject.SetActive(false);
        dungeonBattleController.EnsureNearbyDungeonSelected();

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            dungeonStatusText.text =
                $"遭遇 {dungeonRunManager.CurrentEncounter}/" +
                $"{dungeonRunManager.EncounterCount} を突破。次の行動を選んでください。";
        }
        else
        {
            dungeonStatusText.text = dungeonRunManager.IsRunning
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
        dungeonResultPanel?.gameObject.SetActive(false);
        dungeonBattleController.StartDungeonRun();
    }

    private void ReturnToTownAfterDungeon()
    {
        dungeonResultPanel?.gameObject.SetActive(false);
        ShowTownMap();
        statusText.text = $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}へ戻りました。";
    }

}
