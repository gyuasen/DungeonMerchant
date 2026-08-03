using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class BattleDungeonPresenter
{
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color ButtonTextColor = UITheme.ButtonTextColor;
    private static readonly Color RowColor = UITheme.RowColor;
    private static readonly Color WoodButtonColor = UITheme.WoodButtonColor;
    private static readonly Color FrameColor = UITheme.FrameColor;
    private static readonly Color ImportantButtonColor = UITheme.ImportantButtonColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly UIPageRouter pageRouter;
    private readonly RectTransform battlePage;
    private readonly RectTransform roadBattlePage;
    private readonly RectTransform dungeonPage;
    private readonly Font uiFont;
    private readonly Font uiBodyFont;
    private readonly DungeonRunManager dungeonRunManager;
    private readonly BattleManager battleManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly TownProgressState townProgressState;
    private readonly DungeonBattleController dungeonBattleController;
    private readonly TownTravelController townTravelController;
    private readonly SimpleMercenaryHireUIView.BattleViewReferences battleView;
    private readonly SimpleMercenaryHireUIView.RoadBattleReferences roadBattle;
    private readonly SimpleMercenaryHireUIView.DungeonViewReferences dungeonView;
    private readonly Func<Text> statusTextProvider;
    private readonly Func<Button> startBattleButtonProvider;
    private readonly Func<Button> startDungeonButtonProvider;
    private readonly Func<Button> firstDungeonEventButtonProvider;
    private readonly Func<Button> secondDungeonEventButtonProvider;
    private readonly Func<Button> thirdDungeonEventButtonProvider;
    private readonly Func<BattleVisualController> battleVisualControllerProvider;
    private readonly Func<Button> battleTabButtonProvider;
    private readonly Func<Button> dungeonTabButtonProvider;
    private readonly Action<Button> setStartBattleButton;
    private readonly Action<Button> setStartDungeonButton;
    private readonly Action<Button> setFirstDungeonEventButton;
    private readonly Action<Button> setSecondDungeonEventButton;
    private readonly Action<Button> setThirdDungeonEventButton;
    private readonly Action<BattleVisualController> bindBattleVisualController;
    private readonly UnityAction refreshBattlePage;
    private readonly UnityAction refreshRoadBattlePage;
    private readonly UnityAction refreshDungeonPage;
    private readonly UnityAction continueToNextDungeonFloor;
    private readonly UnityAction returnToTownAfterDungeon;
    private readonly Func<DungeonDataSO, bool> canShowExpeditionAction;
    private readonly Func<DungeonDataSO, bool> hasExpedition;
    private readonly Action<DungeonDataSO> showExpeditionForDungeon;
    private readonly Action<RectTransform> refreshPage;
    private readonly Func<AudioFeedbackService> audioFeedbackServiceProvider;
    private readonly Action<RectTransform, Button> switchToPage;
    private readonly Action showTownMap;
    private readonly Action refreshUI;
    private readonly Func<bool> isProgressionLocked;
    private readonly Func<bool> hasPendingRoadBattleOutcome;
    private readonly Func<Button> mapButtonProvider;
    private readonly Func<Button> townMapButtonProvider;
    private int displayedRoadOriginTownIndex = -1;
    private int displayedRoadDestinationTownIndex = -1;

    public BattleDungeonPresenter(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView activeView,
        UIPageRouter pageRouter,
        RectTransform battlePage,
        RectTransform roadBattlePage,
        RectTransform dungeonPage,
        Font uiFont,
        Font uiBodyFont,
        BattleManager battleManager,
        DungeonRunManager dungeonRunManager,
        MercenaryPartyManager partyManager,
        TownProgressState townProgressState,
        ProgressionManager progressionManager,
        DungeonBattleController dungeonBattleController,
        TownTravelController townTravelController,
        SimpleMercenaryHireUIView.BattleViewReferences battleView,
        SimpleMercenaryHireUIView.RoadBattleReferences roadBattle,
        SimpleMercenaryHireUIView.DungeonViewReferences dungeonView,
        Func<Text> statusTextProvider,
        Func<Button> startBattleButtonProvider,
        Func<Button> startDungeonButtonProvider,
        Func<Button> firstDungeonEventButtonProvider,
        Func<Button> secondDungeonEventButtonProvider,
        Func<Button> thirdDungeonEventButtonProvider,
        Func<BattleVisualController> battleVisualControllerProvider,
        Func<Button> battleTabButtonProvider,
        Func<Button> dungeonTabButtonProvider,
        Action<Button> setStartBattleButton,
        Action<Button> setStartDungeonButton,
        Action<Button> setFirstDungeonEventButton,
        Action<Button> setSecondDungeonEventButton,
        Action<Button> setThirdDungeonEventButton,
        Action<BattleVisualController> bindBattleVisualController,
        UnityAction refreshBattlePage,
        UnityAction refreshRoadBattlePage,
        UnityAction refreshDungeonPage,
        UnityAction continueToNextDungeonFloor,
        UnityAction returnToTownAfterDungeon,
        Func<DungeonDataSO, bool> canShowExpeditionAction,
        Func<DungeonDataSO, bool> hasExpedition,
        Action<DungeonDataSO> showExpeditionForDungeon,
        Action<RectTransform> refreshPage,
        Func<AudioFeedbackService> audioFeedbackServiceProvider,
        Action<RectTransform, Button> switchToPage,
        Action showTownMap,
        Action refreshUI,
        Func<bool> isProgressionLocked,
        Func<bool> hasPendingRoadBattleOutcome,
        Func<Button> mapButtonProvider,
        Func<Button> townMapButtonProvider)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        if (activeView == null) throw new ArgumentNullException(nameof(activeView));
        if (pageRouter == null) throw new ArgumentNullException(nameof(pageRouter));
        if (battlePage == null) throw new ArgumentNullException(nameof(battlePage));
        if (roadBattlePage == null) throw new ArgumentNullException(nameof(roadBattlePage));
        if (dungeonPage == null) throw new ArgumentNullException(nameof(dungeonPage));
        if (uiFont == null) throw new ArgumentNullException(nameof(uiFont));
        if (uiBodyFont == null) throw new ArgumentNullException(nameof(uiBodyFont));
        if (battleManager == null) throw new ArgumentNullException(nameof(battleManager));
        if (dungeonRunManager == null) throw new ArgumentNullException(nameof(dungeonRunManager));
        if (partyManager == null) throw new ArgumentNullException(nameof(partyManager));
        if (townProgressState == null) throw new ArgumentNullException(nameof(townProgressState));
        if (progressionManager == null) throw new ArgumentNullException(nameof(progressionManager));
        if (dungeonBattleController == null) throw new ArgumentNullException(nameof(dungeonBattleController));
        if (townTravelController == null) throw new ArgumentNullException(nameof(townTravelController));
        this.battleView = battleView ?? throw new ArgumentNullException(nameof(battleView));
        this.roadBattle = roadBattle ?? throw new ArgumentNullException(nameof(roadBattle));
        this.dungeonView = dungeonView ?? throw new ArgumentNullException(nameof(dungeonView));
        if (statusTextProvider == null) throw new ArgumentNullException(nameof(statusTextProvider));
        if (startBattleButtonProvider == null) throw new ArgumentNullException(nameof(startBattleButtonProvider));
        if (startDungeonButtonProvider == null) throw new ArgumentNullException(nameof(startDungeonButtonProvider));
        if (firstDungeonEventButtonProvider == null) throw new ArgumentNullException(nameof(firstDungeonEventButtonProvider));
        if (secondDungeonEventButtonProvider == null) throw new ArgumentNullException(nameof(secondDungeonEventButtonProvider));
        if (thirdDungeonEventButtonProvider == null) throw new ArgumentNullException(nameof(thirdDungeonEventButtonProvider));
        if (battleVisualControllerProvider == null) throw new ArgumentNullException(nameof(battleVisualControllerProvider));
        if (battleTabButtonProvider == null) throw new ArgumentNullException(nameof(battleTabButtonProvider));
        if (dungeonTabButtonProvider == null) throw new ArgumentNullException(nameof(dungeonTabButtonProvider));
        this.pageRouter = pageRouter;
        this.battlePage = battlePage;
        this.roadBattlePage = roadBattlePage;
        this.dungeonPage = dungeonPage;
        this.uiFont = uiFont;
        this.uiBodyFont = uiBodyFont;
        this.dungeonRunManager = dungeonRunManager;
        this.battleManager = battleManager;
        this.partyManager = partyManager;
        this.townProgressState = townProgressState;
        this.dungeonBattleController = dungeonBattleController;
        this.townTravelController = townTravelController;
        this.statusTextProvider = statusTextProvider;
        this.startBattleButtonProvider = startBattleButtonProvider;
        this.startDungeonButtonProvider = startDungeonButtonProvider;
        this.firstDungeonEventButtonProvider = firstDungeonEventButtonProvider;
        this.secondDungeonEventButtonProvider = secondDungeonEventButtonProvider;
        this.thirdDungeonEventButtonProvider = thirdDungeonEventButtonProvider;
        this.battleVisualControllerProvider = battleVisualControllerProvider;
        this.battleTabButtonProvider = battleTabButtonProvider;
        this.dungeonTabButtonProvider = dungeonTabButtonProvider;
        this.setStartBattleButton = setStartBattleButton ?? throw new ArgumentNullException(nameof(setStartBattleButton));
        this.setStartDungeonButton = setStartDungeonButton ?? throw new ArgumentNullException(nameof(setStartDungeonButton));
        this.setFirstDungeonEventButton = setFirstDungeonEventButton ?? throw new ArgumentNullException(nameof(setFirstDungeonEventButton));
        this.setSecondDungeonEventButton = setSecondDungeonEventButton ?? throw new ArgumentNullException(nameof(setSecondDungeonEventButton));
        this.setThirdDungeonEventButton = setThirdDungeonEventButton ?? throw new ArgumentNullException(nameof(setThirdDungeonEventButton));
        this.bindBattleVisualController = bindBattleVisualController ?? throw new ArgumentNullException(nameof(bindBattleVisualController));
        this.refreshBattlePage = refreshBattlePage ?? throw new ArgumentNullException(nameof(refreshBattlePage));
        this.refreshRoadBattlePage = refreshRoadBattlePage ?? throw new ArgumentNullException(nameof(refreshRoadBattlePage));
        this.refreshDungeonPage = refreshDungeonPage ?? throw new ArgumentNullException(nameof(refreshDungeonPage));
        this.continueToNextDungeonFloor = continueToNextDungeonFloor ?? throw new ArgumentNullException(nameof(continueToNextDungeonFloor));
        this.returnToTownAfterDungeon = returnToTownAfterDungeon ?? throw new ArgumentNullException(nameof(returnToTownAfterDungeon));
        this.canShowExpeditionAction = canShowExpeditionAction ?? throw new ArgumentNullException(nameof(canShowExpeditionAction));
        this.hasExpedition = hasExpedition ?? throw new ArgumentNullException(nameof(hasExpedition));
        this.showExpeditionForDungeon = showExpeditionForDungeon ?? throw new ArgumentNullException(nameof(showExpeditionForDungeon));
        this.refreshPage = refreshPage ?? throw new ArgumentNullException(nameof(refreshPage));
        if (audioFeedbackServiceProvider == null) throw new ArgumentNullException(nameof(audioFeedbackServiceProvider));
        this.audioFeedbackServiceProvider = audioFeedbackServiceProvider;
        this.switchToPage = switchToPage ?? throw new ArgumentNullException(nameof(switchToPage));
        this.showTownMap = showTownMap ?? throw new ArgumentNullException(nameof(showTownMap));
        this.refreshUI = refreshUI ?? throw new ArgumentNullException(nameof(refreshUI));
        this.isProgressionLocked = isProgressionLocked ?? throw new ArgumentNullException(nameof(isProgressionLocked));
        this.hasPendingRoadBattleOutcome = hasPendingRoadBattleOutcome ?? throw new ArgumentNullException(nameof(hasPendingRoadBattleOutcome));
        this.mapButtonProvider = mapButtonProvider ?? throw new ArgumentNullException(nameof(mapButtonProvider));
        this.townMapButtonProvider = townMapButtonProvider ?? throw new ArgumentNullException(nameof(townMapButtonProvider));
    }

    public void BuildBattlePage()
    {
        battleView.pageTitleText = CreateText(battlePage, "ダンジョン戦闘", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), Vector2.zero, new Color(0.98f, 0.91f, 0.72f));
        AddOutline(battleView.pageTitleText, new Color(0f, 0f, 0f, 0.85f));
        battleView.encounterText = CreateText(battlePage, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, -78f), new Vector2(-160f, -42f), new Color(1f, 0.94f, 0.76f));
        AddOutline(battleView.encounterText, new Color(0f, 0f, 0f, 0.9f));
        Button startBattleButton = CreateActionButton(battlePage, "開始", () => dungeonBattleController.StartPartyBattle());
        setStartBattleButton(startBattleButton);
        RectTransform startRect = startBattleButton.GetComponent<RectTransform>(); startRect.anchorMin = startRect.anchorMax = new Vector2(1f, 1f); startRect.pivot = new Vector2(1f, 1f); startRect.anchoredPosition = new Vector2(0f, -36f); startBattleButton.gameObject.SetActive(false);
        battleView.speedButton = CreateActionButton(battlePage, "速度 x1", () => dungeonBattleController.CycleBattleSpeed());
        SetTopRight(battleView.speedButton, new Vector2(100f, 38f), new Vector2(-250f, -36f));
        battleView.pauseButton = CreateActionButton(battlePage, "一時停止", () => dungeonBattleController.ToggleBattlePause());
        SetTopRight(battleView.pauseButton, new Vector2(100f, 38f), new Vector2(-140f, -36f));
        battleView.skipButton = CreateActionButton(battlePage, "結果まで", () => dungeonBattleController.SkipBattleToEnd());
        SetTopRight(battleView.skipButton, new Vector2(110f, 38f), new Vector2(-20f, -36f));
        RectTransform battleVisualRoot = CreateUIObject("Battle Visuals", battlePage); battleVisualRoot.anchorMin = Vector2.zero; battleVisualRoot.anchorMax = Vector2.one; battleVisualRoot.offsetMin = Vector2.zero; battleVisualRoot.offsetMax = Vector2.zero;
        bindBattleVisualController(battleVisualRoot.gameObject.AddComponent<BattleVisualController>());
        battleView.logPanel = CreateUIObject("Battle Log", battlePage); battleView.logPanel.anchorMin = Vector2.zero; battleView.logPanel.anchorMax = new Vector2(1f, 0.24f); battleView.logPanel.offsetMin = Vector2.zero; battleView.logPanel.offsetMax = Vector2.zero;
        Image logBackground = battleView.logPanel.gameObject.AddComponent<Image>(); logBackground.color = new Color(RowColor.r, RowColor.g, RowColor.b, 0.78f);
        battleView.logViewport = CreateUIObject("Battle Log Viewport", battleView.logPanel); battleView.logViewport.anchorMin = Vector2.zero; battleView.logViewport.anchorMax = Vector2.one; battleView.logViewport.offsetMin = new Vector2(16f, 16f); battleView.logViewport.offsetMax = new Vector2(-16f, -16f);
        Image viewportImage = battleView.logViewport.gameObject.AddComponent<Image>(); viewportImage.color = new Color(0f, 0f, 0f, 0.01f); Mask mask = battleView.logViewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false;
        battleView.logContent = CreateUIObject("Battle Log Content", battleView.logViewport); battleView.logContent.anchorMin = new Vector2(0f, 1f); battleView.logContent.anchorMax = new Vector2(1f, 1f); battleView.logContent.pivot = new Vector2(0.5f, 1f); battleView.logContent.anchoredPosition = Vector2.zero; battleView.logContent.sizeDelta = new Vector2(0f, 430f);
        battleView.logScrollRect = battleView.logViewport.gameObject.AddComponent<ScrollRect>(); battleView.logScrollRect.content = battleView.logContent; battleView.logScrollRect.viewport = battleView.logViewport; battleView.logScrollRect.horizontal = false; battleView.logScrollRect.vertical = true; battleView.logScrollRect.movementType = ScrollRect.MovementType.Clamped; battleView.logScrollRect.scrollSensitivity = 28f;
        battleView.logText = CreateText(battleView.logContent, "戦闘準備完了。", 14, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(16f, 16f), new Vector2(-16f, -16f), MutedTextColor); battleView.logText.supportRichText = true; battleView.logText.rectTransform.anchorMin = Vector2.zero; battleView.logText.rectTransform.anchorMax = Vector2.one; battleView.logText.rectTransform.pivot = new Vector2(0.5f, 0.5f); battleView.logText.rectTransform.offsetMin = new Vector2(0f, 8f); battleView.logText.rectTransform.offsetMax = new Vector2(0f, -8f);
        BuildDungeonEventOverlay();
        BattlePageUI pageUI = battlePage.GetComponent<BattlePageUI>() ?? battlePage.gameObject.AddComponent<BattlePageUI>(); pageUI.Configure(refreshBattlePage); pageRouter.Register(battlePage);
    }

    public void BuildRoadBattlePage()
    {
        Text roadBattleTitle = CreateText(roadBattlePage, "街道戦闘", 24, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, -38f), Vector2.zero, new Color(1f, 0.94f, 0.76f)); AddOutline(roadBattleTitle, new Color(0f, 0f, 0f, 0.9f));
        roadBattle.routeText = CreateText(roadBattlePage, string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -82f), new Vector2(0f, -42f), new Color(1f, 0.94f, 0.76f)); AddOutline(roadBattle.routeText, new Color(0f, 0f, 0f, 0.9f));
        roadBattle.speedButton = CreateActionButton(roadBattlePage, "速度 x1", () => dungeonBattleController.CycleBattleSpeed()); SetTopRight(roadBattle.speedButton, new Vector2(100f, 38f), new Vector2(-270f, -4f));
        roadBattle.pauseButton = CreateActionButton(roadBattlePage, "一時停止", () => dungeonBattleController.ToggleBattlePause()); SetTopRight(roadBattle.pauseButton, new Vector2(100f, 38f), new Vector2(-380f, -4f));
        roadBattle.skipButton = CreateActionButton(roadBattlePage, "結果まで", () => dungeonBattleController.SkipBattleToEnd()); SetTopRight(roadBattle.skipButton, new Vector2(100f, 38f), new Vector2(-490f, -4f));
        roadBattle.continueButton = CreateActionButton(roadBattlePage, "次へ進む", () => townTravelController.ContinueTownTravel()); SetTopRight(roadBattle.continueButton, new Vector2(120f, 40f), new Vector2(-130f, -4f));
        roadBattle.retreatButton = CreateActionButton(roadBattlePage, "撤退する", () => townTravelController.RetreatFromTownTravel()); SetTopRight(roadBattle.retreatButton, new Vector2(120f, 40f), new Vector2(0f, -4f)); roadBattle.retreatButton.targetGraphic.color = ImportantButtonColor;
        roadBattle.continueButton.gameObject.SetActive(false); roadBattle.retreatButton.gameObject.SetActive(false);
        RoadBattlePageUI pageUI = roadBattlePage.GetComponent<RoadBattlePageUI>() ?? roadBattlePage.gameObject.AddComponent<RoadBattlePageUI>(); pageUI.Configure(refreshRoadBattlePage); pageRouter.Register(roadBattlePage);
    }

    public void BuildDungeonPage()
    {
        CreateText(dungeonPage, "ダンジョン探索", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), Vector2.zero, ParchmentMutedColor);
        dungeonView.statusText = CreateText(dungeonPage, "パーティーを編成してダンジョンへ向かいましょう。", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, -154f), new Vector2(-170f, -42f), ParchmentTextColor);
        Button startDungeonButton = CreateActionButton(dungeonPage, "探索開始", () => dungeonBattleController.StartDungeonRun()); setStartDungeonButton(startDungeonButton);
        RectTransform startRect = startDungeonButton.GetComponent<RectTransform>(); startRect.anchorMin = startRect.anchorMax = new Vector2(1f, 1f); startRect.pivot = new Vector2(1f, 1f); startRect.anchoredPosition = new Vector2(0f, -36f);
        dungeonView.selectionList = CreateUIObject("Dungeon Selection List", dungeonPage); dungeonView.selectionList.anchorMin = new Vector2(0f, 1f); dungeonView.selectionList.anchorMax = new Vector2(1f, 1f); dungeonView.selectionList.pivot = new Vector2(0.5f, 1f); dungeonView.selectionList.anchoredPosition = new Vector2(0f, -174f); dungeonView.selectionList.sizeDelta = new Vector2(0f, 150f);
        dungeonView.resultPanel = CreateUIObject("Dungeon Floor Result", dungeonPage); dungeonView.resultPanel.anchorMin = Vector2.zero; dungeonView.resultPanel.anchorMax = Vector2.one; dungeonView.resultPanel.offsetMin = new Vector2(40f, 42f); dungeonView.resultPanel.offsetMax = new Vector2(-40f, -42f);
        Image resultBackground = dungeonView.resultPanel.gameObject.AddComponent<Image>(); resultBackground.color = RowColor; AddFantasyFrame(resultBackground, 2f);
        dungeonView.resultText = CreateText(dungeonView.resultPanel, string.Empty, 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(36f, 100f), new Vector2(-36f, -70f), ButtonTextColor); dungeonView.resultText.rectTransform.anchorMin = Vector2.zero; dungeonView.resultText.rectTransform.anchorMax = Vector2.one;
        dungeonView.nextFloorButton = CreateActionButton(dungeonView.resultPanel, "次のフロアへ進む", continueToNextDungeonFloor); SetBottomCenter(dungeonView.nextFloorButton, new Vector2(-125f, 28f));
        Button returnTownButton = CreateActionButton(dungeonView.resultPanel, "町へ戻る", returnToTownAfterDungeon); SetBottomCenter(returnTownButton, new Vector2(125f, 28f));
        dungeonView.resultPanel.gameObject.SetActive(false); UpdateDungeonEventUI();
        DungeonPageUI pageUI = dungeonPage.GetComponent<DungeonPageUI>() ?? dungeonPage.gameObject.AddComponent<DungeonPageUI>(); pageUI.Configure(refreshDungeonPage);
        pageUI.ConfigureSelectionList(dungeonView.selectionList, uiFont, Color.white, ParchmentTextColor, RowColor, WoodButtonColor, FrameColor, ButtonTextColor, () => dungeonRunManager.AvailableDungeons, () => townProgressState.CurrentTownIndex, WorldMapService.GetTownName, dungeonRunManager.GetClearedFloors, dungeonRunManager.IsDungeonUnlocked, () => dungeonRunManager.SelectedDungeon, dungeonBattleController.SelectDungeon, canShowExpeditionAction, hasExpedition, showExpeditionForDungeon);
        pageRouter.Register(dungeonPage); refreshPage(dungeonPage);
    }

    public void BuildDungeonEventOverlay()
    {
        dungeonView.eventPanel = CreateUIObject("Dungeon Event Overlay", battlePage);
        dungeonView.eventPanel.anchorMin = new Vector2(0f, 0.28f);
        dungeonView.eventPanel.anchorMax = new Vector2(1f, 0.79f);
        dungeonView.eventPanel.offsetMin = Vector2.zero;
        dungeonView.eventPanel.offsetMax = Vector2.zero;

        Image eventBackground = dungeonView.eventPanel.gameObject.AddComponent<Image>();
        eventBackground.color = new Color(0.055f, 0.035f, 0.02f, 0.94f);
        AddFantasyFrame(eventBackground, 3f);

        Text eventHeader = CreateText(dungeonView.eventPanel, "探索イベント", 15, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Color(0.98f, 0.84f, 0.5f));
        eventHeader.rectTransform.anchorMin = new Vector2(0.02f, 0.91f); eventHeader.rectTransform.anchorMax = new Vector2(0.22f, 0.99f); eventHeader.rectTransform.offsetMin = Vector2.zero; eventHeader.rectTransform.offsetMax = Vector2.zero;
        Outline headerOutline = eventHeader.gameObject.AddComponent<Outline>(); headerOutline.effectColor = new Color(0f, 0f, 0f, 0.85f); headerOutline.effectDistance = new Vector2(1f, -1f);

        dungeonView.eventTitleText = CreateText(dungeonView.eventPanel, string.Empty, 25, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Color(1f, 0.94f, 0.76f));
        dungeonView.eventTitleText.rectTransform.anchorMin = new Vector2(0.22f, 0.84f); dungeonView.eventTitleText.rectTransform.anchorMax = new Vector2(0.98f, 0.98f); dungeonView.eventTitleText.rectTransform.pivot = new Vector2(0.5f, 0.5f); dungeonView.eventTitleText.rectTransform.offsetMin = Vector2.zero; dungeonView.eventTitleText.rectTransform.offsetMax = Vector2.zero; dungeonView.eventTitleText.alignment = TextAnchor.MiddleCenter; dungeonView.eventTitleText.horizontalOverflow = HorizontalWrapMode.Wrap; dungeonView.eventTitleText.verticalOverflow = VerticalWrapMode.Overflow; dungeonView.eventTitleText.resizeTextForBestFit = true; dungeonView.eventTitleText.resizeTextMinSize = 16; dungeonView.eventTitleText.resizeTextMaxSize = 25;

        dungeonView.eventDescriptionText = CreateText(dungeonView.eventPanel, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Color.white);
        dungeonView.eventDescriptionText.rectTransform.anchorMin = new Vector2(0.05f, 0.68f); dungeonView.eventDescriptionText.rectTransform.anchorMax = new Vector2(0.95f, 0.82f); dungeonView.eventDescriptionText.rectTransform.pivot = new Vector2(0.5f, 0.5f); dungeonView.eventDescriptionText.rectTransform.offsetMin = Vector2.zero; dungeonView.eventDescriptionText.rectTransform.offsetMax = Vector2.zero; dungeonView.eventDescriptionText.alignment = TextAnchor.MiddleCenter; dungeonView.eventDescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap; dungeonView.eventDescriptionText.verticalOverflow = VerticalWrapMode.Overflow; dungeonView.eventDescriptionText.resizeTextForBestFit = true; dungeonView.eventDescriptionText.resizeTextMinSize = 12; dungeonView.eventDescriptionText.resizeTextMaxSize = 17;

        dungeonView.eventPreviewText = CreateText(dungeonView.eventPanel, string.Empty, 16, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Color(1f, 0.86f, 0.42f));
        dungeonView.eventPreviewText.rectTransform.anchorMin = new Vector2(0.04f, 0.01f); dungeonView.eventPreviewText.rectTransform.anchorMax = new Vector2(0.96f, 0.23f); dungeonView.eventPreviewText.rectTransform.offsetMin = Vector2.zero; dungeonView.eventPreviewText.rectTransform.offsetMax = Vector2.zero;

        Button firstButton = CreateActionButton(dungeonView.eventPanel, "選択肢1", () => dungeonBattleController.ChooseDungeonEventOption(0));
        setFirstDungeonEventButton(firstButton); PositionDungeonEventButton(firstButton, 0); ConfigureDungeonEventHover(firstButton, 0);
        Button secondButton = CreateActionButton(dungeonView.eventPanel, "選択肢2", () => dungeonBattleController.ChooseDungeonEventOption(1));
        setSecondDungeonEventButton(secondButton); PositionDungeonEventButton(secondButton, 1); ConfigureDungeonEventHover(secondButton, 1);
        Button thirdButton = CreateActionButton(dungeonView.eventPanel, "撤退", () => dungeonBattleController.ChooseDungeonEventOption(2));
        setThirdDungeonEventButton(thirdButton); PositionDungeonEventButton(thirdButton, 2); ConfigureDungeonEventHover(thirdButton, 2);
        dungeonView.eventPanel.gameObject.SetActive(false);
    }

    private static void PositionDungeonEventButton(Button button, int index)
    {
        RectTransform rect = button.GetComponent<RectTransform>(); float columnWidth = 1f / 3f;
        rect.anchorMin = new Vector2(index * columnWidth + 0.025f, 0.24f); rect.anchorMax = new Vector2((index + 1) * columnWidth - 0.025f, 0.66f); rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = Vector2.zero; rect.anchoredPosition = Vector2.zero;
        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.fontSize = 14; label.alignment = TextAnchor.MiddleCenter; label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one; label.rectTransform.pivot = new Vector2(0.5f, 0.5f); label.rectTransform.offsetMin = new Vector2(12f, 14f); label.rectTransform.offsetMax = new Vector2(-12f, -14f); label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; label.resizeTextForBestFit = true; label.resizeTextMinSize = 10; label.resizeTextMaxSize = 14;
            Outline outline = label.GetComponent<Outline>() ?? label.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0f, 0f, 0f, 0.95f); outline.effectDistance = new Vector2(1f, -1f);
        }
    }

    private void ConfigureDungeonEventHover(Button button, int optionIndex)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>(); trigger.triggers = new List<EventTrigger.Entry>();
        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter }; enter.callback.AddListener(_ => ShowDungeonEventPreview(optionIndex)); trigger.triggers.Add(enter);
        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit }; exit.callback.AddListener(_ => HideDungeonEventPreview()); trigger.triggers.Add(exit);
    }

    private void ShowDungeonEventPreview(int optionIndex) { if (dungeonView.eventPreviewText != null) dungeonView.eventPreviewText.text = dungeonRunManager.GetEventOptionPreview(optionIndex); }
    private void HideDungeonEventPreview() { if (dungeonView.eventPreviewText != null) dungeonView.eventPreviewText.text = string.Empty; }

    private void ApplyDungeonEventChoiceImage(Button button, int optionIndex)
    {
        Image image = button != null ? button.targetGraphic as Image : null;
        if (image == null) return;
        string imageKey = dungeonRunManager.GetEventOptionImageKey(optionIndex); Sprite eventSprite = Resources.Load<Sprite>($"Battle/Events/{imageKey}"); bool hasEventSprite = eventSprite != null;
        image.sprite = hasEventSprite ? eventSprite : Resources.Load<Sprite>("UI/ParchmentPanel"); image.type = hasEventSprite ? Image.Type.Simple : Image.Type.Sliced; image.preserveAspect = hasEventSprite;
        image.color = hasEventSprite ? Color.white : optionIndex == 2 ? new Color(0.55f, 0.22f, 0.18f, 1f) : optionIndex == 0 ? new Color(0.34f, 0.48f, 0.28f, 1f) : new Color(0.46f, 0.36f, 0.20f, 1f);
    }

    public void UpdateDungeonEventUI()
    {
        Button firstButton = firstDungeonEventButtonProvider(); Button secondButton = secondDungeonEventButtonProvider(); Button thirdButton = thirdDungeonEventButtonProvider();
        if (dungeonView.eventPanel == null || dungeonView.eventTitleText == null || dungeonView.eventDescriptionText == null || firstButton == null || secondButton == null || thirdButton == null) return;
        BattleVisualController battleVisualController = battleVisualControllerProvider();
        bool showEvent = dungeonRunManager.IsAwaitingEventChoice &&
                         (battleVisualController == null ||
                          !battleVisualController.IsPresentationBusy);
        if (dungeonView.selectionList != null) dungeonView.selectionList.gameObject.SetActive(!dungeonRunManager.IsRunning);
        dungeonView.eventPanel.gameObject.SetActive(showEvent);
        if (!showEvent) return;
        dungeonView.eventPanel.SetAsLastSibling(); dungeonView.eventTitleText.text = dungeonRunManager.EventTitle; dungeonView.eventDescriptionText.text = dungeonRunManager.EventDescription; HideDungeonEventPreview();
        SetButtonLabel(firstButton, dungeonRunManager.FirstOptionLabel); SetButtonLabel(secondButton, dungeonRunManager.SecondOptionLabel); SetButtonLabel(thirdButton, dungeonRunManager.ThirdOptionLabel);
        ApplyDungeonEventChoiceImage(firstButton, 0); ApplyDungeonEventChoiceImage(secondButton, 1); ApplyDungeonEventChoiceImage(thirdButton, 2);
    }

    public void ResetBattleLogView()
    {
        if (battleView.logText == null || battleView.logContent == null) return;
        battleView.logText.text = string.Empty; Canvas.ForceUpdateCanvases(); float viewportHeight = battleView.logViewport != null ? battleView.logViewport.rect.height : 0f; battleView.logContent.sizeDelta = new Vector2(0f, Mathf.Max(1f, viewportHeight)); battleView.logContent.anchoredPosition = Vector2.zero;
        if (battleView.logScrollRect != null) { battleView.logScrollRect.StopMovement(); battleView.logScrollRect.verticalNormalizedPosition = 1f; }
    }

    public void SetBattleLogText(string text) { if (battleView.logText != null) battleView.logText.text = text; }
    public void UpdateBattleLogContentHeight()
    {
        if (battleView.logContent == null || battleView.logText == null) return;
        Canvas.ForceUpdateCanvases(); float viewportHeight = battleView.logViewport != null ? battleView.logViewport.rect.height : 0f; battleView.logContent.sizeDelta = new Vector2(0f, Mathf.Max(viewportHeight, battleView.logText.preferredHeight + 32f));
    }
    public void ScrollBattleLogToLatestView() { if (battleView.logScrollRect != null) battleView.logScrollRect.verticalNormalizedPosition = 0f; }
    public void HandlePresentationSound(BattleSoundCue soundCue)
    {
        UISoundCue uiSoundCue;
        switch (soundCue) { case BattleSoundCue.Attack: case BattleSoundCue.Impact: case BattleSoundCue.Evade: case BattleSoundCue.Defeat: uiSoundCue = UISoundCue.BattleAttack; break; case BattleSoundCue.Heal: case BattleSoundCue.Skill: case BattleSoundCue.Victory: uiSoundCue = UISoundCue.Confirm; break; case BattleSoundCue.Loss: uiSoundCue = UISoundCue.Warning; break; case BattleSoundCue.Reward: uiSoundCue = UISoundCue.Reward; break; default: return; }
        audioFeedbackServiceProvider()?.Play(uiSoundCue);
    }

    public void ShowDungeonCompletionResult(
        bool cleared,
        bool hiddenIslandUnlocked,
        string explorationResult,
        bool fullyCleared)
    {
        Text statusText = statusTextProvider();
        statusText.text = cleared
            ? fullyCleared
                ? "ダンジョンを完全攻略しました。"
                : $"フロアを攻略しました。次回は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "ダンジョン探索を終了しました。";
        if (!string.IsNullOrEmpty(explorationResult))
        {
            statusText.text += $" {explorationResult}";
        }
        if (hiddenIslandUnlocked)
        {
            statusText.text =
                "全条件を達成しました。全体マップ中央に新たな島が出現しました。";
        }

        ShowDungeonPage();
        dungeonView.resultText.text = cleared
            ? fullyCleared
                ? $"{dungeonRunManager.DungeonName}\n完全攻略！\n\n" +
                  "すべてのフロアを攻略しました。"
                : "フロア攻略完了\n\n" +
                  $"次は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "探索終了\n\n町へ戻って態勢を整えましょう。";
        dungeonView.nextFloorButton.gameObject.SetActive(cleared && !fullyCleared);
        dungeonView.resultPanel.SetAsLastSibling();
        dungeonView.resultPanel.gameObject.SetActive(true);
        UpdateDungeonEventUI();
        dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
        refreshUI();
    }

    public void ShowBattlePage()
    {
        MoveBattleLogTo(battlePage);
        switchToPage(battlePage, battleTabButtonProvider());
    }

    public void RefreshBattlePage()
    {
        UpdateDungeonEventUI();
        Button startBattleButton = startBattleButtonProvider();
        startBattleButton.interactable = partyManager.Members.Count > 0 && !isProgressionLocked();
        startBattleButton.gameObject.SetActive(false);
        battleView.skipButton.interactable = battleManager.IsBattling && !battleManager.IsSkippingToBattleEnd;
        battleView.pauseButton.interactable = battleManager.IsBattling && !battleManager.IsSkippingToBattleEnd;
        SetButtonLabel(battleView.pauseButton, battleManager.IsPaused ? "再開" : "一時停止");
        statusTextProvider().text = $"戦闘参加: 傭兵{partyManager.Members.Count}人";
    }

    public void ShowRoadBattlePage(int originTownIndex, int destinationTownIndex)
    {
        if (townTravelController == null || !townTravelController.RoadTravelState.IsActive ||
            string.IsNullOrEmpty(WorldMapService.GetTownName(originTownIndex)) ||
            string.IsNullOrEmpty(WorldMapService.GetTownName(destinationTownIndex)))
        {
            return;
        }

        displayedRoadOriginTownIndex = originTownIndex;
        displayedRoadDestinationTownIndex = destinationTownIndex;
        MoveBattleLogTo(roadBattlePage);
        switchToPage(roadBattlePage, null);
    }

    public void RefreshRoadBattlePage()
    {
        RoadTravelState roadTravelState = townTravelController.RoadTravelState;
        bool isActive = roadTravelState != null && roadTravelState.IsActive &&
                        !string.IsNullOrEmpty(WorldMapService.GetTownName(roadTravelState.DestinationTownIndex)) &&
                        !string.IsNullOrEmpty(WorldMapService.GetTownName(displayedRoadOriginTownIndex)) &&
                        !string.IsNullOrEmpty(WorldMapService.GetTownName(displayedRoadDestinationTownIndex));
        mapButtonProvider()?.gameObject.SetActive(false);
        townMapButtonProvider()?.gameObject.SetActive(false);
        roadBattle.continueButton.gameObject.SetActive(isActive && roadTravelState.IsAwaitingChoice && !hasPendingRoadBattleOutcome());
        roadBattle.retreatButton.gameObject.SetActive(isActive && roadTravelState.IsAwaitingChoice && !hasPendingRoadBattleOutcome());
        roadBattle.skipButton.interactable = battleManager.IsBattling && !battleManager.IsSkippingToBattleEnd;
        roadBattle.pauseButton.interactable = battleManager.IsBattling && !battleManager.IsSkippingToBattleEnd;
        SetButtonLabel(roadBattle.pauseButton, battleManager.IsPaused ? "再開" : "一時停止");
        string originTownName = WorldMapService.GetTownName(displayedRoadOriginTownIndex);
        string destinationTownName = WorldMapService.GetTownName(displayedRoadDestinationTownIndex);
        if (!isActive || string.IsNullOrEmpty(originTownName) || string.IsNullOrEmpty(destinationTownName))
        {
            roadBattle.routeText.text = "街道移動は終了しました。";
            return;
        }

        roadBattle.routeText.text =
            $"{originTownName} → {destinationTownName}\n" +
            $"接敵 {roadTravelState.EncounterIndex}/{roadTravelState.EncounterCount}  |  " +
            (roadTravelState.ContainsRareEncounter ? "幻獣の気配を確認！" : "両地域の通常モンスターが街道を塞いでいます。");
    }

    public void MoveBattleLogTo(RectTransform destinationPage)
    {
        if (battleView.logPanel == null || destinationPage == null) return;
        battleView.logPanel.SetParent(destinationPage, false);
        battleView.logPanel.anchorMin = Vector2.zero;
        battleView.logPanel.anchorMax = new Vector2(1f, 0.24f);
        battleView.logPanel.offsetMin = Vector2.zero;
        battleView.logPanel.offsetMax = Vector2.zero;
        battleVisualControllerProvider()?.MoveTo(destinationPage);
    }

    public void ShowDungeonPage() => switchToPage(dungeonPage, dungeonTabButtonProvider());

    public void RefreshDungeonPage()
    {
        dungeonView.resultPanel?.gameObject.SetActive(false);
        dungeonBattleController.EnsureNearbyDungeonSelected();
        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            dungeonView.statusText.text = $"遭遇 {dungeonRunManager.CurrentEncounter}/{dungeonRunManager.EncounterCount} を突破。次の行動を選んでください。";
        }
        else
        {
            dungeonView.statusText.text = dungeonRunManager.IsRunning
                ? $"第{dungeonRunManager.CurrentFloor}/{dungeonRunManager.TotalFloors}フロア探索中: {dungeonRunManager.CurrentEncounter}/{dungeonRunManager.EncounterCount}"
                : $"{dungeonRunManager.DungeonName}  |  第{dungeonRunManager.CurrentFloor}/{dungeonRunManager.TotalFloors}フロア  |  遭遇{dungeonRunManager.EncounterCount}回\nフロア報酬 {Mathf.Max(0, dungeonRunManager.SelectedDungeon != null ? dungeonRunManager.SelectedDungeon.floorClearGoldReward : 0)} G  |  完全攻略報酬 {dungeonRunManager.ClearGoldReward} G\n" + DungeonBattleController.BuildDungeonRewardPreview(dungeonRunManager.SelectedDungeon);
        }
        UpdateDungeonEventUI();
        dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
        statusTextProvider().text = $"探索パーティー: 傭兵{partyManager.Members.Count}人";
        refreshUI();
    }

    public void ContinueToNextDungeonFloor()
    {
        dungeonView.resultPanel?.gameObject.SetActive(false);
        dungeonBattleController.StartDungeonRun();
    }

    public void ReturnToTownAfterDungeon()
    {
        dungeonView.resultPanel?.gameObject.SetActive(false);
        showTownMap();
        statusTextProvider().text = $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}へ戻りました。";
    }
    public static void SetButtonLabel(Button button, string label) { Text buttonText = button.GetComponentInChildren<Text>(); if (buttonText != null) buttonText.text = label; }

    private Text CreateText(RectTransform parent, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax, Color color) => factory.CreateText(parent, content, fontSize, fontStyle, alignment, offsetMin, offsetMax, color);
    private Button CreateActionButton(RectTransform parent, string label, UnityAction action) => factory.CreateActionButton(parent, label, action);
    private static RectTransform CreateUIObject(string objectName, Transform parent) => SimpleMercenaryHireUIFactory.CreateUIObject(objectName, parent);
    private static void AddFantasyFrame(Image image, float thickness) => SimpleMercenaryHireUIFactory.AddFantasyFrame(image, thickness);
    private static void AddOutline(Text text, Color color) { Outline outline = text.gameObject.AddComponent<Outline>(); outline.effectColor = color; outline.effectDistance = new Vector2(1f, -1f); }
    private static void SetTopRight(Button button, Vector2 size, Vector2 position) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f); rect.sizeDelta = size; rect.anchoredPosition = position; }
    private static void SetBottomCenter(Button button, Vector2 position) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f); rect.pivot = new Vector2(0.5f, 0f); rect.sizeDelta = new Vector2(220f, 50f); rect.anchoredPosition = position; }
}
