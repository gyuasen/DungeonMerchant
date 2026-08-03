using System;
using UnityEngine;
using UnityEngine.Events;
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
    private readonly UnityAction buildDungeonEventOverlay;
    private readonly UnityAction refreshBattlePage;
    private readonly UnityAction refreshRoadBattlePage;
    private readonly UnityAction refreshDungeonPage;
    private readonly UnityAction updateDungeonEventUI;
    private readonly UnityAction continueToNextDungeonFloor;
    private readonly UnityAction returnToTownAfterDungeon;
    private readonly Func<DungeonDataSO, bool> canShowExpeditionAction;
    private readonly Func<DungeonDataSO, bool> hasExpedition;
    private readonly Action<DungeonDataSO> showExpeditionForDungeon;
    private readonly Action<RectTransform> refreshPage;

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
        UnityAction buildDungeonEventOverlay,
        UnityAction refreshBattlePage,
        UnityAction refreshRoadBattlePage,
        UnityAction refreshDungeonPage,
        UnityAction updateDungeonEventUI,
        UnityAction continueToNextDungeonFloor,
        UnityAction returnToTownAfterDungeon,
        Func<DungeonDataSO, bool> canShowExpeditionAction,
        Func<DungeonDataSO, bool> hasExpedition,
        Action<DungeonDataSO> showExpeditionForDungeon,
        Action<RectTransform> refreshPage)
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
        this.buildDungeonEventOverlay = buildDungeonEventOverlay ?? throw new ArgumentNullException(nameof(buildDungeonEventOverlay));
        this.refreshBattlePage = refreshBattlePage ?? throw new ArgumentNullException(nameof(refreshBattlePage));
        this.refreshRoadBattlePage = refreshRoadBattlePage ?? throw new ArgumentNullException(nameof(refreshRoadBattlePage));
        this.refreshDungeonPage = refreshDungeonPage ?? throw new ArgumentNullException(nameof(refreshDungeonPage));
        this.updateDungeonEventUI = updateDungeonEventUI ?? throw new ArgumentNullException(nameof(updateDungeonEventUI));
        this.continueToNextDungeonFloor = continueToNextDungeonFloor ?? throw new ArgumentNullException(nameof(continueToNextDungeonFloor));
        this.returnToTownAfterDungeon = returnToTownAfterDungeon ?? throw new ArgumentNullException(nameof(returnToTownAfterDungeon));
        this.canShowExpeditionAction = canShowExpeditionAction ?? throw new ArgumentNullException(nameof(canShowExpeditionAction));
        this.hasExpedition = hasExpedition ?? throw new ArgumentNullException(nameof(hasExpedition));
        this.showExpeditionForDungeon = showExpeditionForDungeon ?? throw new ArgumentNullException(nameof(showExpeditionForDungeon));
        this.refreshPage = refreshPage ?? throw new ArgumentNullException(nameof(refreshPage));
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
        buildDungeonEventOverlay();
        setFirstDungeonEventButton(firstDungeonEventButtonProvider());
        setSecondDungeonEventButton(secondDungeonEventButtonProvider());
        setThirdDungeonEventButton(thirdDungeonEventButtonProvider());
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
        dungeonView.resultPanel.gameObject.SetActive(false); updateDungeonEventUI();
        DungeonPageUI pageUI = dungeonPage.GetComponent<DungeonPageUI>() ?? dungeonPage.gameObject.AddComponent<DungeonPageUI>(); pageUI.Configure(refreshDungeonPage);
        pageUI.ConfigureSelectionList(dungeonView.selectionList, uiFont, Color.white, ParchmentTextColor, RowColor, WoodButtonColor, FrameColor, ButtonTextColor, () => dungeonRunManager.AvailableDungeons, () => townProgressState.CurrentTownIndex, WorldMapService.GetTownName, dungeonRunManager.GetClearedFloors, dungeonRunManager.IsDungeonUnlocked, () => dungeonRunManager.SelectedDungeon, dungeonBattleController.SelectDungeon, canShowExpeditionAction, hasExpedition, showExpeditionForDungeon);
        pageRouter.Register(dungeonPage); refreshPage(dungeonPage);
    }

    private Text CreateText(RectTransform parent, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax, Color color) => factory.CreateText(parent, content, fontSize, fontStyle, alignment, offsetMin, offsetMax, color);
    private Button CreateActionButton(RectTransform parent, string label, UnityAction action) => factory.CreateActionButton(parent, label, action);
    private static RectTransform CreateUIObject(string objectName, Transform parent) => SimpleMercenaryHireUIFactory.CreateUIObject(objectName, parent);
    private static void AddFantasyFrame(Image image, float thickness) => SimpleMercenaryHireUIFactory.AddFantasyFrame(image, thickness);
    private static void AddOutline(Text text, Color color) { Outline outline = text.gameObject.AddComponent<Outline>(); outline.effectColor = color; outline.effectDistance = new Vector2(1f, -1f); }
    private static void SetTopRight(Button button, Vector2 size, Vector2 position) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f); rect.sizeDelta = size; rect.anchoredPosition = position; }
    private static void SetBottomCenter(Button button, Vector2 position) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f); rect.pivot = new Vector2(0.5f, 0f); rect.sizeDelta = new Vector2(220f, 50f); rect.anchoredPosition = position; }
}
