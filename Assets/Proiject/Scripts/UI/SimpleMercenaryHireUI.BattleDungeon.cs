using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildBattlePage()
    {
        battleView.pageTitleText = CreateText(
            battlePage, "ダンジョン戦闘", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f),
            new Color(0.98f, 0.91f, 0.72f));
        Outline titleOutline =
            battleView.pageTitleText.gameObject.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        titleOutline.effectDistance = new Vector2(1f, -1f);

        battleView.encounterText = CreateText(
            battlePage, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f),
            new Color(1f, 0.94f, 0.76f));
        Outline encounterOutline =
            battleView.encounterText.gameObject.AddComponent<Outline>();
        encounterOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        encounterOutline.effectDistance = new Vector2(1f, -1f);

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

        battleView.speedButton =
            CreateActionButton(
                battlePage,
                "速度 x1",
                () => dungeonBattleController.CycleBattleSpeed());
        RectTransform battleSpeedRect =
            battleView.speedButton.GetComponent<RectTransform>();
        battleSpeedRect.anchorMin = battleSpeedRect.anchorMax =
            new Vector2(1f, 1f);
        battleSpeedRect.pivot = new Vector2(1f, 1f);
        battleSpeedRect.sizeDelta = new Vector2(100f, 38f);
        battleSpeedRect.anchoredPosition = new Vector2(-250f, -36f);

        battleView.pauseButton = CreateActionButton(
            battlePage,
            "一時停止",
            () => dungeonBattleController.ToggleBattlePause());
        RectTransform battlePauseRect =
            battleView.pauseButton.GetComponent<RectTransform>();
        battlePauseRect.anchorMin = battlePauseRect.anchorMax =
            new Vector2(1f, 1f);
        battlePauseRect.pivot = new Vector2(1f, 1f);
        battlePauseRect.sizeDelta = new Vector2(100f, 38f);
        battlePauseRect.anchoredPosition = new Vector2(-140f, -36f);

        battleView.skipButton = CreateActionButton(
            battlePage,
            "結果まで",
            () => dungeonBattleController.SkipBattleToEnd());
        RectTransform battleSkipRect =
            battleView.skipButton.GetComponent<RectTransform>();
        battleSkipRect.anchorMin = battleSkipRect.anchorMax =
            new Vector2(1f, 1f);
        battleSkipRect.pivot = new Vector2(1f, 1f);
        battleSkipRect.sizeDelta = new Vector2(110f, 38f);
        battleSkipRect.anchoredPosition = new Vector2(-20f, -36f);

        RectTransform battleVisualRoot =
            CreateUIObject("Battle Visuals", battlePage);
        battleVisualRoot.anchorMin = Vector2.zero;
        battleVisualRoot.anchorMax = Vector2.one;
        battleVisualRoot.offsetMin = Vector2.zero;
        battleVisualRoot.offsetMax = Vector2.zero;
        battleVisualController =
            battleVisualRoot.gameObject.AddComponent<BattleVisualController>();
        battleVisualController.Configure(
            battleManager,
            uiBodyFont != null ? uiBodyFont : uiFont);
        battleVisualController.PresentationLog += HandlePresentationLog;
        battleVisualController.PresentationSound += HandlePresentationSound;
        battleVisualController.PresentationCompleted +=
            HandleBattleVisualPresentationCompleted;

        battleView.logPanel = CreateUIObject("Battle Log", battlePage);
        battleView.logPanel.anchorMin = new Vector2(0f, 0f);
        battleView.logPanel.anchorMax = new Vector2(1f, 0.24f);
        battleView.logPanel.offsetMin = Vector2.zero;
        battleView.logPanel.offsetMax = Vector2.zero;

        Image logBackground = battleView.logPanel.gameObject.AddComponent<Image>();
        logBackground.color =
            new Color(RowColor.r, RowColor.g, RowColor.b, 0.78f);

        battleView.logViewport =
            CreateUIObject("Battle Log Viewport", battleView.logPanel);
        battleView.logViewport.anchorMin = Vector2.zero;
        battleView.logViewport.anchorMax = Vector2.one;
        battleView.logViewport.offsetMin = new Vector2(16f, 16f);
        battleView.logViewport.offsetMax = new Vector2(-16f, -16f);

        Image viewportImage = battleView.logViewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = battleView.logViewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        battleView.logContent = CreateUIObject("Battle Log Content", battleView.logViewport);
        battleView.logContent.anchorMin = new Vector2(0f, 1f);
        battleView.logContent.anchorMax = new Vector2(1f, 1f);
        battleView.logContent.pivot = new Vector2(0.5f, 1f);
        battleView.logContent.anchoredPosition = Vector2.zero;
        battleView.logContent.sizeDelta = new Vector2(0f, 430f);

        battleView.logScrollRect = battleView.logViewport.gameObject.AddComponent<ScrollRect>();
        battleView.logScrollRect.content = battleView.logContent;
        battleView.logScrollRect.viewport = battleView.logViewport;
        battleView.logScrollRect.horizontal = false;
        battleView.logScrollRect.vertical = true;
        battleView.logScrollRect.movementType = ScrollRect.MovementType.Clamped;
        battleView.logScrollRect.scrollSensitivity = 28f;

        battleView.logText = CreateText(battleView.logContent, "戦闘準備完了。", 14, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(16f, 16f), new Vector2(-16f, -16f),
            MutedTextColor);
        battleView.logText.supportRichText = true;
        battleView.logText.rectTransform.anchorMin = Vector2.zero;
        battleView.logText.rectTransform.anchorMax = Vector2.one;
        battleView.logText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        battleView.logText.rectTransform.offsetMin = new Vector2(0f, 8f);
        battleView.logText.rectTransform.offsetMax = new Vector2(0f, -8f);

        BuildDungeonEventOverlay();

        BattlePageUI pageUI =
            battlePage.GetComponent<BattlePageUI>() ??
            battlePage.gameObject.AddComponent<BattlePageUI>();
        pageUI.Configure(RefreshBattlePage);
        pageRouter.Register(battlePage);
    }

    private void BuildRoadBattlePage()
    {
        Text roadBattleTitle = CreateText(
            roadBattlePage,
            "街道戦闘",
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -38f),
            new Vector2(0f, 0f),
            new Color(1f, 0.94f, 0.76f));
        Outline roadTitleOutline =
            roadBattleTitle.gameObject.AddComponent<Outline>();
        roadTitleOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        roadTitleOutline.effectDistance = new Vector2(1f, -1f);

        roadBattle.routeText = CreateText(
            roadBattlePage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -82f),
            new Vector2(0f, -42f),
            new Color(1f, 0.94f, 0.76f));
        Outline roadRouteOutline =
            roadBattle.routeText.gameObject.AddComponent<Outline>();
        roadRouteOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        roadRouteOutline.effectDistance = new Vector2(1f, -1f);

        roadBattle.speedButton =
            CreateActionButton(
                roadBattlePage,
                "速度 x1",
                () => dungeonBattleController.CycleBattleSpeed());
        RectTransform roadSpeedRect =
            roadBattle.speedButton.GetComponent<RectTransform>();
        roadSpeedRect.anchorMin = roadSpeedRect.anchorMax =
            new Vector2(1f, 1f);
        roadSpeedRect.pivot = new Vector2(1f, 1f);
        roadSpeedRect.sizeDelta = new Vector2(100f, 38f);
        roadSpeedRect.anchoredPosition = new Vector2(-270f, -4f);

        roadBattle.pauseButton = CreateActionButton(
            roadBattlePage,
            "一時停止",
            () => dungeonBattleController.ToggleBattlePause());
        RectTransform roadPauseRect =
            roadBattle.pauseButton.GetComponent<RectTransform>();
        roadPauseRect.anchorMin = roadPauseRect.anchorMax =
            new Vector2(1f, 1f);
        roadPauseRect.pivot = new Vector2(1f, 1f);
        roadPauseRect.sizeDelta = new Vector2(100f, 38f);
        roadPauseRect.anchoredPosition = new Vector2(-380f, -4f);

        roadBattle.skipButton = CreateActionButton(
            roadBattlePage,
            "結果まで",
            () => dungeonBattleController.SkipBattleToEnd());
        RectTransform roadSkipRect =
            roadBattle.skipButton.GetComponent<RectTransform>();
        roadSkipRect.anchorMin = roadSkipRect.anchorMax =
            new Vector2(1f, 1f);
        roadSkipRect.pivot = new Vector2(1f, 1f);
        roadSkipRect.sizeDelta = new Vector2(100f, 38f);
        roadSkipRect.anchoredPosition = new Vector2(-490f, -4f);

        roadBattle.continueButton =
            CreateActionButton(
                roadBattlePage,
                "次へ進む",
                () => townTravelController.ContinueTownTravel());
        RectTransform continueRect =
            roadBattle.continueButton.GetComponent<RectTransform>();
        continueRect.anchorMin = continueRect.anchorMax =
            new Vector2(1f, 1f);
        continueRect.pivot = new Vector2(1f, 1f);
        continueRect.sizeDelta = new Vector2(120f, 40f);
        continueRect.anchoredPosition = new Vector2(-130f, -4f);

        roadBattle.retreatButton =
            CreateActionButton(
                roadBattlePage,
                "撤退する",
                () => townTravelController.RetreatFromTownTravel());
        RectTransform retreatRect =
            roadBattle.retreatButton.GetComponent<RectTransform>();
        retreatRect.anchorMin = retreatRect.anchorMax =
            new Vector2(1f, 1f);
        retreatRect.pivot = new Vector2(1f, 1f);
        retreatRect.sizeDelta = new Vector2(120f, 40f);
        retreatRect.anchoredPosition = new Vector2(0f, -4f);
        roadBattle.retreatButton.targetGraphic.color = ImportantButtonColor;

        roadBattle.continueButton.gameObject.SetActive(false);
        roadBattle.retreatButton.gameObject.SetActive(false);

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

        dungeonView.statusText = CreateText(
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

        // 上端アンカー型で作る。DungeonPageUI.RefreshSelection が
        // sizeDelta.y を「リストの高さ」として設定するため、ストレッチ型
        // (anchorMin.y=0) にすると矩形が上方向へ拡張されてヘッダーに
        // 重なる（旧UI崩れの原因）。他のスクロールリストと同じ規約に合わせる。
        dungeonView.selectionList = CreateUIObject("Dungeon Selection List", dungeonPage);
        dungeonView.selectionList.anchorMin = new Vector2(0f, 1f);
        dungeonView.selectionList.anchorMax = new Vector2(1f, 1f);
        dungeonView.selectionList.pivot = new Vector2(0.5f, 1f);
        dungeonView.selectionList.anchoredPosition = new Vector2(0f, -174f);
        dungeonView.selectionList.sizeDelta = new Vector2(0f, 150f);

        dungeonView.resultPanel =
            CreateUIObject("Dungeon Floor Result", dungeonPage);
        dungeonView.resultPanel.anchorMin = Vector2.zero;
        dungeonView.resultPanel.anchorMax = Vector2.one;
        dungeonView.resultPanel.offsetMin = new Vector2(40f, 42f);
        dungeonView.resultPanel.offsetMax = new Vector2(-40f, -42f);
        Image resultBackground =
            dungeonView.resultPanel.gameObject.AddComponent<Image>();
        resultBackground.color = RowColor;
        AddFantasyFrame(resultBackground, 2f);

        dungeonView.resultText = CreateText(
            dungeonView.resultPanel,
            string.Empty,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(36f, 100f),
            new Vector2(-36f, -70f),
            ButtonTextColor);
        dungeonView.resultText.rectTransform.anchorMin = Vector2.zero;
        dungeonView.resultText.rectTransform.anchorMax = Vector2.one;

        dungeonView.nextFloorButton = CreateActionButton(
            dungeonView.resultPanel,
            "次のフロアへ進む",
            ContinueToNextDungeonFloor);
        RectTransform nextFloorRect =
            dungeonView.nextFloorButton.GetComponent<RectTransform>();
        nextFloorRect.anchorMin = nextFloorRect.anchorMax =
            new Vector2(0.5f, 0f);
        nextFloorRect.pivot = new Vector2(0.5f, 0f);
        nextFloorRect.sizeDelta = new Vector2(220f, 50f);
        nextFloorRect.anchoredPosition = new Vector2(-125f, 28f);

        Button returnTownButton = CreateActionButton(
            dungeonView.resultPanel,
            "町へ戻る",
            ReturnToTownAfterDungeon);
        RectTransform returnTownRect =
            returnTownButton.GetComponent<RectTransform>();
        returnTownRect.anchorMin = returnTownRect.anchorMax =
            new Vector2(0.5f, 0f);
        returnTownRect.pivot = new Vector2(0.5f, 0f);
        returnTownRect.sizeDelta = new Vector2(220f, 50f);
        returnTownRect.anchoredPosition = new Vector2(125f, 28f);

        dungeonView.resultPanel.gameObject.SetActive(false);

        UpdateDungeonEventUI();

        DungeonPageUI pageUI =
            dungeonPage.GetComponent<DungeonPageUI>() ??
            dungeonPage.gameObject.AddComponent<DungeonPageUI>();
        pageUI.Configure(RefreshDungeonPage);
        pageUI.ConfigureSelectionList(
            dungeonView.selectionList,
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
            dungeonBattleController.SelectDungeon,
            CanShowExpeditionAction,
            HasExpedition,
            ShowExpeditionForDungeon);
        pageRouter.Register(dungeonPage);
        RefreshPage(dungeonPage);
    }

    private void BuildDungeonEventOverlay()
    {
        dungeonView.eventPanel = CreateUIObject("Dungeon Event Overlay", battlePage);
        dungeonView.eventPanel.anchorMin = new Vector2(0f, 0.28f);
        dungeonView.eventPanel.anchorMax = new Vector2(1f, 0.79f);
        dungeonView.eventPanel.offsetMin = Vector2.zero;
        dungeonView.eventPanel.offsetMax = Vector2.zero;

        Image eventBackground = dungeonView.eventPanel.gameObject.AddComponent<Image>();
        eventBackground.color = new Color(0.055f, 0.035f, 0.02f, 0.94f);
        AddFantasyFrame(eventBackground, 3f);

        Text eventHeader = CreateText(
            dungeonView.eventPanel,
            "探索イベント",
            15,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            new Color(0.98f, 0.84f, 0.5f));
        eventHeader.rectTransform.anchorMin = new Vector2(0.02f, 0.91f);
        eventHeader.rectTransform.anchorMax = new Vector2(0.22f, 0.99f);
        eventHeader.rectTransform.offsetMin = Vector2.zero;
        eventHeader.rectTransform.offsetMax = Vector2.zero;
        Outline headerOutline = eventHeader.gameObject.AddComponent<Outline>();
        headerOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        headerOutline.effectDistance = new Vector2(1f, -1f);

        dungeonView.eventTitleText = CreateText(
            dungeonView.eventPanel,
            string.Empty,
            25,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            new Color(1f, 0.94f, 0.76f));
        dungeonView.eventTitleText.rectTransform.anchorMin =
            new Vector2(0.22f, 0.84f);
        dungeonView.eventTitleText.rectTransform.anchorMax =
            new Vector2(0.98f, 0.98f);
        dungeonView.eventTitleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        dungeonView.eventTitleText.rectTransform.offsetMin = Vector2.zero;
        dungeonView.eventTitleText.rectTransform.offsetMax = Vector2.zero;
        dungeonView.eventTitleText.alignment = TextAnchor.MiddleCenter;
        dungeonView.eventTitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dungeonView.eventTitleText.verticalOverflow = VerticalWrapMode.Overflow;
        dungeonView.eventTitleText.resizeTextForBestFit = true;
        dungeonView.eventTitleText.resizeTextMinSize = 16;
        dungeonView.eventTitleText.resizeTextMaxSize = 25;

        dungeonView.eventDescriptionText = CreateText(
            dungeonView.eventPanel,
            string.Empty,
            17,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            Color.white);
        dungeonView.eventDescriptionText.rectTransform.anchorMin =
            new Vector2(0.05f, 0.68f);
        dungeonView.eventDescriptionText.rectTransform.anchorMax =
            new Vector2(0.95f, 0.82f);
        dungeonView.eventDescriptionText.rectTransform.pivot =
            new Vector2(0.5f, 0.5f);
        dungeonView.eventDescriptionText.rectTransform.offsetMin = Vector2.zero;
        dungeonView.eventDescriptionText.rectTransform.offsetMax = Vector2.zero;
        dungeonView.eventDescriptionText.alignment = TextAnchor.MiddleCenter;
        dungeonView.eventDescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dungeonView.eventDescriptionText.verticalOverflow = VerticalWrapMode.Overflow;
        dungeonView.eventDescriptionText.resizeTextForBestFit = true;
        dungeonView.eventDescriptionText.resizeTextMinSize = 12;
        dungeonView.eventDescriptionText.resizeTextMaxSize = 17;

        dungeonView.eventPreviewText = CreateText(
            dungeonView.eventPanel,
            string.Empty,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            new Color(1f, 0.86f, 0.42f));
        dungeonView.eventPreviewText.rectTransform.anchorMin =
            new Vector2(0.04f, 0.01f);
        dungeonView.eventPreviewText.rectTransform.anchorMax =
            new Vector2(0.96f, 0.23f);
        dungeonView.eventPreviewText.rectTransform.offsetMin = Vector2.zero;
        dungeonView.eventPreviewText.rectTransform.offsetMax = Vector2.zero;

        firstDungeonEventButton = CreateActionButton(
            dungeonView.eventPanel,
            "選択肢1",
            () => dungeonBattleController.ChooseDungeonEventOption(0));
        PositionDungeonEventButton(firstDungeonEventButton, 0);
        ConfigureDungeonEventHover(firstDungeonEventButton, 0);

        secondDungeonEventButton = CreateActionButton(
            dungeonView.eventPanel,
            "選択肢2",
            () => dungeonBattleController.ChooseDungeonEventOption(1));
        PositionDungeonEventButton(secondDungeonEventButton, 1);
        ConfigureDungeonEventHover(secondDungeonEventButton, 1);

        thirdDungeonEventButton = CreateActionButton(
            dungeonView.eventPanel,
            "撤退",
            () => dungeonBattleController.ChooseDungeonEventOption(2));
        PositionDungeonEventButton(thirdDungeonEventButton, 2);
        ConfigureDungeonEventHover(thirdDungeonEventButton, 2);

        dungeonView.eventPanel.gameObject.SetActive(false);
    }

    private static void PositionDungeonEventButton(Button button, int index)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        float columnWidth = 1f / 3f;
        rect.anchorMin = new Vector2(index * columnWidth + 0.025f, 0.24f);
        rect.anchorMax = new Vector2((index + 1) * columnWidth - 0.025f, 0.66f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.fontSize = 14;
            label.alignment = TextAnchor.MiddleCenter;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.offsetMin = new Vector2(12f, 14f);
            label.rectTransform.offsetMax = new Vector2(-12f, -14f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = 14;
            Outline outline = label.GetComponent<Outline>() ??
                              label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(1f, -1f);
        }
    }

    private void ConfigureDungeonEventHover(Button button, int optionIndex)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>() ??
                               button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers = new List<EventTrigger.Entry>();

        EventTrigger.Entry enter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        enter.callback.AddListener(_ => ShowDungeonEventPreview(optionIndex));
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exit.callback.AddListener(_ => HideDungeonEventPreview());
        trigger.triggers.Add(exit);
    }

    private void ShowDungeonEventPreview(int optionIndex)
    {
        if (dungeonView.eventPreviewText != null)
        {
            dungeonView.eventPreviewText.text =
                dungeonRunManager.GetEventOptionPreview(optionIndex);
        }
    }

    private void HideDungeonEventPreview()
    {
        if (dungeonView.eventPreviewText != null)
        {
            dungeonView.eventPreviewText.text = string.Empty;
        }
    }

    private void ApplyDungeonEventChoiceImage(Button button, int optionIndex)
    {
        Image image = button != null ? button.targetGraphic as Image : null;
        if (image == null)
        {
            return;
        }

        string imageKey = dungeonRunManager.GetEventOptionImageKey(optionIndex);
        Sprite eventSprite = Resources.Load<Sprite>($"Battle/Events/{imageKey}");
        bool hasEventSprite = eventSprite != null;
        image.sprite = hasEventSprite
            ? eventSprite
            : Resources.Load<Sprite>("UI/ParchmentPanel");
        image.type = hasEventSprite ? Image.Type.Simple : Image.Type.Sliced;
        image.preserveAspect = hasEventSprite;
        image.color = hasEventSprite
            ? Color.white
            : optionIndex == 2
                ? new Color(0.55f, 0.22f, 0.18f, 1f)
                : optionIndex == 0
                    ? new Color(0.34f, 0.48f, 0.28f, 1f)
                    : new Color(0.46f, 0.36f, 0.20f, 1f);
    }

    private void ResetBattleLog()
    {
        dungeonBattleController.ClearBattleLog();
        battleView.logText.text = string.Empty;

        if (battleLogScrollCoroutine != null)
        {
            StopCoroutine(battleLogScrollCoroutine);
            battleLogScrollCoroutine = null;
        }

        Canvas.ForceUpdateCanvases();
        float viewportHeight = battleView.logViewport != null
            ? battleView.logViewport.rect.height
            : 0f;
        battleView.logContent.sizeDelta = new Vector2(0f, Mathf.Max(1f, viewportHeight));
        battleView.logContent.anchoredPosition = Vector2.zero;

        if (battleView.logScrollRect != null)
        {
            battleView.logScrollRect.StopMovement();
            battleView.logScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void HandleBattleMessage(string message, BattleLogType logType)
    {
        AppendBattleMessage(message, logType);
    }

    private void HandlePresentationLog(string message, BattleLogType logType)
    {
        AppendBattleMessage(message, logType);
    }

    private void AppendBattleMessage(string message, BattleLogType logType)
    {
        battleView.logText.text =
            dungeonBattleController.AppendBattleMessage(message, logType);

        if (battleView.logContent != null)
        {
            UpdateBattleLogContentHeight();
        }

        ScrollBattleLogToLatest();
    }

    private void HandlePresentationSound(BattleSoundCue soundCue)
    {
        UISoundCue uiSoundCue;
        switch (soundCue)
        {
            case BattleSoundCue.Attack:
            case BattleSoundCue.Impact:
            case BattleSoundCue.Evade:
            case BattleSoundCue.Defeat:
                uiSoundCue = UISoundCue.BattleAttack;
                break;
            case BattleSoundCue.Heal:
            case BattleSoundCue.Skill:
            case BattleSoundCue.Victory:
                uiSoundCue = UISoundCue.Confirm;
                break;
            case BattleSoundCue.Loss:
                uiSoundCue = UISoundCue.Warning;
                break;
            case BattleSoundCue.Reward:
                uiSoundCue = UISoundCue.Reward;
                break;
            default:
                return;
        }
        audioFeedbackService?.Play(uiSoundCue);
    }

    private void UpdateBattleLogContentHeight()
    {
        if (battleView.logContent == null || battleView.logText == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        float viewportHeight = battleView.logViewport != null
            ? battleView.logViewport.rect.height
            : 0f;
        float height = Mathf.Max(viewportHeight, battleView.logText.preferredHeight + 32f);
        battleView.logContent.sizeDelta = new Vector2(0f, height);
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
        battleView.logScrollRect.verticalNormalizedPosition = 0f;
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

    private void UpdateDungeonEventUI()
    {
        if (dungeonView.eventPanel == null ||
            dungeonView.eventTitleText == null ||
            dungeonView.eventDescriptionText == null ||
            firstDungeonEventButton == null ||
            secondDungeonEventButton == null ||
            thirdDungeonEventButton == null)
        {
            return;
        }

        bool showEvent =
            dungeonRunManager.IsAwaitingEventChoice &&
            (battleVisualController == null ||
             !battleVisualController.IsPresentationBusy);
        if (dungeonView.selectionList != null)
        {
            dungeonView.selectionList.gameObject.SetActive(!dungeonRunManager.IsRunning);
        }

        dungeonView.eventPanel.gameObject.SetActive(showEvent);

        if (!showEvent)
        {
            return;
        }

        dungeonView.eventPanel.SetAsLastSibling();
        dungeonView.eventTitleText.text = dungeonRunManager.EventTitle;
        dungeonView.eventDescriptionText.text = dungeonRunManager.EventDescription;
        HideDungeonEventPreview();
        SetButtonLabel(firstDungeonEventButton, dungeonRunManager.FirstOptionLabel);
        SetButtonLabel(secondDungeonEventButton, dungeonRunManager.SecondOptionLabel);
        SetButtonLabel(thirdDungeonEventButton, dungeonRunManager.ThirdOptionLabel);
        ApplyDungeonEventChoiceImage(firstDungeonEventButton, 0);
        ApplyDungeonEventChoiceImage(secondDungeonEventButton, 1);
        ApplyDungeonEventChoiceImage(thirdDungeonEventButton, 2);
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
