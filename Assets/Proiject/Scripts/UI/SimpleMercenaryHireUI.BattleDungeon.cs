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
        battlePageTitleText = CreateText(
            battlePage, "ダンジョン戦闘", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f),
            new Color(0.98f, 0.91f, 0.72f));
        Outline titleOutline =
            battlePageTitleText.gameObject.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        titleOutline.effectDistance = new Vector2(1f, -1f);

        battleEncounterText = CreateText(
            battlePage, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f),
            new Color(1f, 0.94f, 0.76f));
        Outline encounterOutline =
            battleEncounterText.gameObject.AddComponent<Outline>();
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
        battleSpeedRect.anchoredPosition = new Vector2(-250f, -36f);

        battlePauseButton = CreateActionButton(
            battlePage,
            "一時停止",
            () => dungeonBattleController.ToggleBattlePause());
        RectTransform battlePauseRect =
            battlePauseButton.GetComponent<RectTransform>();
        battlePauseRect.anchorMin = battlePauseRect.anchorMax =
            new Vector2(1f, 1f);
        battlePauseRect.pivot = new Vector2(1f, 1f);
        battlePauseRect.sizeDelta = new Vector2(100f, 38f);
        battlePauseRect.anchoredPosition = new Vector2(-140f, -36f);

        battleSkipButton = CreateActionButton(
            battlePage,
            "結果まで",
            () => dungeonBattleController.SkipBattleToEnd());
        RectTransform battleSkipRect =
            battleSkipButton.GetComponent<RectTransform>();
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
        battleVisualController.PresentationCompleted +=
            HandleBattleVisualPresentationCompleted;

        battleLogPanel = CreateUIObject("Battle Log", battlePage);
        battleLogPanel.anchorMin = new Vector2(0f, 0f);
        battleLogPanel.anchorMax = new Vector2(1f, 0.24f);
        battleLogPanel.offsetMin = Vector2.zero;
        battleLogPanel.offsetMax = Vector2.zero;

        Image logBackground = battleLogPanel.gameObject.AddComponent<Image>();
        logBackground.color =
            new Color(RowColor.r, RowColor.g, RowColor.b, 0.78f);

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

        roadBattleRouteText = CreateText(
            roadBattlePage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -82f),
            new Vector2(0f, -42f),
            new Color(1f, 0.94f, 0.76f));
        Outline roadRouteOutline =
            roadBattleRouteText.gameObject.AddComponent<Outline>();
        roadRouteOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        roadRouteOutline.effectDistance = new Vector2(1f, -1f);

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

        roadPauseButton = CreateActionButton(
            roadBattlePage,
            "一時停止",
            () => dungeonBattleController.ToggleBattlePause());
        RectTransform roadPauseRect =
            roadPauseButton.GetComponent<RectTransform>();
        roadPauseRect.anchorMin = roadPauseRect.anchorMax =
            new Vector2(1f, 1f);
        roadPauseRect.pivot = new Vector2(1f, 1f);
        roadPauseRect.sizeDelta = new Vector2(100f, 38f);
        roadPauseRect.anchoredPosition = new Vector2(-380f, -4f);

        roadSkipButton = CreateActionButton(
            roadBattlePage,
            "結果まで",
            () => dungeonBattleController.SkipBattleToEnd());
        RectTransform roadSkipRect =
            roadSkipButton.GetComponent<RectTransform>();
        roadSkipRect.anchorMin = roadSkipRect.anchorMax =
            new Vector2(1f, 1f);
        roadSkipRect.pivot = new Vector2(1f, 1f);
        roadSkipRect.sizeDelta = new Vector2(100f, 38f);
        roadSkipRect.anchoredPosition = new Vector2(-490f, -4f);

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

        // 上端アンカー型で作る。DungeonPageUI.RefreshSelection が
        // sizeDelta.y を「リストの高さ」として設定するため、ストレッチ型
        // (anchorMin.y=0) にすると矩形が上方向へ拡張されてヘッダーに
        // 重なる（旧UI崩れの原因）。他のスクロールリストと同じ規約に合わせる。
        dungeonSelectionList = CreateUIObject("Dungeon Selection List", dungeonPage);
        dungeonSelectionList.anchorMin = new Vector2(0f, 1f);
        dungeonSelectionList.anchorMax = new Vector2(1f, 1f);
        dungeonSelectionList.pivot = new Vector2(0.5f, 1f);
        dungeonSelectionList.anchoredPosition = new Vector2(0f, -174f);
        dungeonSelectionList.sizeDelta = new Vector2(0f, 150f);

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

    private void BuildDungeonEventOverlay()
    {
        dungeonEventPanel = CreateUIObject("Dungeon Event Overlay", battlePage);
        dungeonEventPanel.anchorMin = new Vector2(0f, 0.28f);
        dungeonEventPanel.anchorMax = new Vector2(1f, 0.79f);
        dungeonEventPanel.offsetMin = Vector2.zero;
        dungeonEventPanel.offsetMax = Vector2.zero;

        Image eventBackground = dungeonEventPanel.gameObject.AddComponent<Image>();
        eventBackground.color = new Color(0.055f, 0.035f, 0.02f, 0.94f);
        AddFantasyFrame(eventBackground, 3f);

        Text eventHeader = CreateText(
            dungeonEventPanel,
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

        dungeonEventTitleText = CreateText(
            dungeonEventPanel,
            string.Empty,
            25,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            new Color(1f, 0.94f, 0.76f));
        dungeonEventTitleText.rectTransform.anchorMin =
            new Vector2(0.22f, 0.84f);
        dungeonEventTitleText.rectTransform.anchorMax =
            new Vector2(0.98f, 0.98f);
        dungeonEventTitleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        dungeonEventTitleText.rectTransform.offsetMin = Vector2.zero;
        dungeonEventTitleText.rectTransform.offsetMax = Vector2.zero;
        dungeonEventTitleText.alignment = TextAnchor.MiddleCenter;
        dungeonEventTitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dungeonEventTitleText.verticalOverflow = VerticalWrapMode.Overflow;
        dungeonEventTitleText.resizeTextForBestFit = true;
        dungeonEventTitleText.resizeTextMinSize = 16;
        dungeonEventTitleText.resizeTextMaxSize = 25;

        dungeonEventDescriptionText = CreateText(
            dungeonEventPanel,
            string.Empty,
            17,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            Color.white);
        dungeonEventDescriptionText.rectTransform.anchorMin =
            new Vector2(0.05f, 0.68f);
        dungeonEventDescriptionText.rectTransform.anchorMax =
            new Vector2(0.95f, 0.82f);
        dungeonEventDescriptionText.rectTransform.pivot =
            new Vector2(0.5f, 0.5f);
        dungeonEventDescriptionText.rectTransform.offsetMin = Vector2.zero;
        dungeonEventDescriptionText.rectTransform.offsetMax = Vector2.zero;
        dungeonEventDescriptionText.alignment = TextAnchor.MiddleCenter;
        dungeonEventDescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dungeonEventDescriptionText.verticalOverflow = VerticalWrapMode.Overflow;
        dungeonEventDescriptionText.resizeTextForBestFit = true;
        dungeonEventDescriptionText.resizeTextMinSize = 12;
        dungeonEventDescriptionText.resizeTextMaxSize = 17;

        dungeonEventPreviewText = CreateText(
            dungeonEventPanel,
            string.Empty,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            new Color(1f, 0.86f, 0.42f));
        dungeonEventPreviewText.rectTransform.anchorMin =
            new Vector2(0.04f, 0.01f);
        dungeonEventPreviewText.rectTransform.anchorMax =
            new Vector2(0.96f, 0.23f);
        dungeonEventPreviewText.rectTransform.offsetMin = Vector2.zero;
        dungeonEventPreviewText.rectTransform.offsetMax = Vector2.zero;

        firstDungeonEventButton = CreateActionButton(
            dungeonEventPanel,
            "選択肢1",
            () => dungeonBattleController.ChooseDungeonEventOption(0));
        PositionDungeonEventButton(firstDungeonEventButton, 0);
        ConfigureDungeonEventHover(firstDungeonEventButton, 0);

        secondDungeonEventButton = CreateActionButton(
            dungeonEventPanel,
            "選択肢2",
            () => dungeonBattleController.ChooseDungeonEventOption(1));
        PositionDungeonEventButton(secondDungeonEventButton, 1);
        ConfigureDungeonEventHover(secondDungeonEventButton, 1);

        thirdDungeonEventButton = CreateActionButton(
            dungeonEventPanel,
            "撤退",
            () => dungeonBattleController.ChooseDungeonEventOption(2));
        PositionDungeonEventButton(thirdDungeonEventButton, 2);
        ConfigureDungeonEventHover(thirdDungeonEventButton, 2);

        dungeonEventPanel.gameObject.SetActive(false);
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
        if (dungeonEventPreviewText != null)
        {
            dungeonEventPreviewText.text =
                dungeonRunManager.GetEventOptionPreview(optionIndex);
        }
    }

    private void HideDungeonEventPreview()
    {
        if (dungeonEventPreviewText != null)
        {
            dungeonEventPreviewText.text = string.Empty;
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
        if (logType == BattleLogType.Reward)
        {
            audioFeedbackService?.Play(UISoundCue.Reward);
        }
        else if ((logType == BattleLogType.Player ||
                  logType == BattleLogType.Enemy) &&
                 (battleManager == null ||
                  !battleManager.IsSkippingToBattleEnd))
        {
            audioFeedbackService?.Play(UISoundCue.BattleAttack);
        }

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
            dungeonEventPanel?.gameObject.SetActive(false);
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
        if (hasPendingDungeonCompletion)
        {
            CompletePendingDungeonResult();
            ShowPendingDailyResultIfReady();
            return;
        }

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            UpdateDungeonEventUI();
            ShowBattlePage();
        }

        ShowPendingDailyResultIfReady();
    }

    private IEnumerator WaitForDungeonPresentationCompletion()
    {
        const float timeoutSeconds = 8f;
        float elapsed = 0f;
        while (hasPendingDungeonCompletion &&
               battleVisualController != null &&
               battleVisualController.IsPresentationBusy &&
               elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
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
        if (dungeonEventPanel == null ||
            dungeonEventTitleText == null ||
            dungeonEventDescriptionText == null ||
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
        if (dungeonSelectionList != null)
        {
            dungeonSelectionList.gameObject.SetActive(!dungeonRunManager.IsRunning);
        }

        dungeonEventPanel.gameObject.SetActive(showEvent);

        if (!showEvent)
        {
            return;
        }

        dungeonEventPanel.SetAsLastSibling();
        dungeonEventTitleText.text = dungeonRunManager.EventTitle;
        dungeonEventDescriptionText.text = dungeonRunManager.EventDescription;
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
            partyManager.Members.Count > 0 && !battleManager.IsBattling;
        startBattleButton.gameObject.SetActive(false);
        battleSkipButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        battlePauseButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        SetButtonLabel(
            battlePauseButton,
            battleManager.IsPaused ? "再開" : "一時停止");
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
        roadSkipButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        roadPauseButton.interactable =
            battleManager.IsBattling &&
            !battleManager.IsSkippingToBattleEnd;
        SetButtonLabel(
            roadPauseButton,
            battleManager.IsPaused ? "再開" : "一時停止");
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
        battleLogPanel.anchorMax = new Vector2(1f, 0.24f);
        battleLogPanel.offsetMin = Vector2.zero;
        battleLogPanel.offsetMax = Vector2.zero;
        battleVisualController?.MoveTo(destinationPage);
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
