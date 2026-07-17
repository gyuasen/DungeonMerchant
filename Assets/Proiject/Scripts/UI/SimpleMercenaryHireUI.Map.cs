using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildGlobalMapPage()
    {
        AddMapBackground(
            globalMapPage,
            "Maps/WorldMap",
            "Maps/ContinentMap");

        Button currentContinentButton = CreateWorldRegionButton(
            globalMapPage,
            "東方平原地域\n低級～中級",
            new Vector2(225f, -5f),
            new Vector2(390f, 390f),
            ShowWorldMap);
        ConfigureWorldRegionHover(
            currentContinentButton,
            new Color(0.08f, 0.7f, 0.35f, 0.28f));

        Button secondContinentButton = CreateWorldRegionButton(
            globalMapPage,
            "北西山岳森林地域\n上級",
            new Vector2(-225f, 120f),
            new Vector2(360f, 220f),
            () => ShowWorldMap(1));
        ConfigureWorldRegionHover(
            secondContinentButton,
            new Color(0.7f, 0.52f, 0.18f, 0.3f));

        Button thirdContinentButton = CreateWorldRegionButton(
            globalMapPage,
            "南西黒土地域\n最高級",
            new Vector2(-225f, -125f),
            new Vector2(360f, 245f),
            () => ShowWorldMap(2));
        ConfigureWorldRegionHover(
            thirdContinentButton,
            new Color(0.55f, 0.12f, 0.16f, 0.32f));

        hiddenIslandRegionButton = CreateWorldRegionButton(
            globalMapPage,
            "中央島アステラ\nRank 10",
            new Vector2(0f, -5f),
            new Vector2(155f, 135f),
            () => ShowWorldMap(WorldMapService.HiddenIslandWorldMapIndex));
        ConfigureWorldRegionHover(
            hiddenIslandRegionButton,
            new Color(0.38f, 0.72f, 0.95f, 0.38f));
        hiddenIslandRegionButton.gameObject.SetActive(false);

        CreateText(
            globalMapPage,
            "探索する大陸を選択してください。",
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(14f, -34f),
            new Vector2(-14f, -4f),
            Color.white);

        GlobalMapPageUI pageUI =
            globalMapPage.GetComponent<GlobalMapPageUI>() ??
            globalMapPage.gameObject.AddComponent<GlobalMapPageUI>();
        pageUI.Configure(RefreshGlobalMapPage);
        pageRouter.Register(globalMapPage);
    }

    private static void ConfigureWorldRegionHover(
        Button button,
        Color hoverColor)
    {
        if (button == null || button.targetGraphic == null)
        {
            return;
        }

        Color transparent = new Color(
            hoverColor.r,
            hoverColor.g,
            hoverColor.b,
            0f);
        button.targetGraphic.color = transparent;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = transparent;
        colors.highlightedColor = hoverColor;
        colors.selectedColor = hoverColor;
        colors.pressedColor = new Color(
            hoverColor.r,
            hoverColor.g,
            hoverColor.b,
            Mathf.Min(0.65f, hoverColor.a + 0.18f));
        colors.disabledColor = transparent;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
    }

    private Button CreateWorldRegionButton(
        RectTransform parent,
        string label,
        Vector2 position,
        Vector2 size,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateMapButton(
            parent,
            label,
            position,
            size,
            action);
        Text labelText = button.GetComponentInChildren<Text>();
        if (labelText != null)
        {
            labelText.fontSize = 20;
            labelText.alignment = TextAnchor.MiddleCenter;
            Outline textOutline = labelText.gameObject.AddComponent<Outline>();
            textOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            textOutline.effectDistance = new Vector2(2f, -2f);
        }

        return button;
    }

    private void BuildTravelConfirmationOverlay()
    {
        travelConfirmationOverlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.TravelConfirmation,
                "Travel Confirmation Overlay");
        travelConfirmationOverlay.gameObject.SetActive(false);
        travelConfirmationOverlay.anchorMin = Vector2.zero;
        travelConfirmationOverlay.anchorMax = Vector2.one;
        travelConfirmationOverlay.offsetMin = Vector2.zero;
        travelConfirmationOverlay.offsetMax = Vector2.zero;
        travelConfirmationOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.84f);

        RectTransform window =
            CreateUIObject("Travel Confirmation Window", travelConfirmationOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(560f, 300f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "町を移動しますか？",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(24f, -74f),
            new Vector2(-24f, -22f),
            ParchmentTextColor);

        travelConfirmationText = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -190f),
            new Vector2(-36f, -82f),
            ParchmentMutedColor);

        Button confirmButton =
            CreateActionButton(
                window,
                "移動する",
                () => townTravelController.ConfirmTownTravel());
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax =
            confirmRect.pivot = new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 28f);
        confirmButton.targetGraphic.color = AccentColor;

        Button cancelButton =
            CreateActionButton(window, "キャンセル", HideTravelConfirmation);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax =
            cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 28f);

        travelConfirmationOverlay.gameObject.SetActive(false);
    }

    private void BuildWorldMapPage()
    {
        CreateRegionMapPage(
            0,
            "Maps/EasternRegionMap",
            new[] { 0, 1, 2 },
            new[]
            {
                new Vector2(-330f, -15f),
                new Vector2(0f, 105f),
                new Vector2(300f, 35f)
            },
            new[]
            {
                new Vector2(-225f, -85f),
                new Vector2(115f, 100f),
                new Vector2(205f, -35f)
            });
        CreateRegionMapPage(
            1,
            "Maps/NorthwestRegionMap",
            new[] { 3, 4 },
            new[]
            {
                new Vector2(-230f, 95f),
                new Vector2(225f, -65f)
            },
            new[]
            {
                new Vector2(-100f, 20f),
                new Vector2(110f, -125f)
            });
        CreateRegionMapPage(
            2,
            "Maps/BlackSoilRegionMap",
            new[] { 5, 6 },
            new[]
            {
                new Vector2(-235f, 80f),
                new Vector2(215f, -95f)
            },
            new[]
            {
                new Vector2(-80f, 5f),
                new Vector2(95f, -145f)
            });
        CreateRegionMapPage(
            WorldMapService.HiddenIslandWorldMapIndex,
            "Maps/WorldMap",
            new[] { WorldMapService.HiddenIslandTownIndex },
            new[] { new Vector2(-90f, 35f) },
            new[] { new Vector2(145f, -45f) });

        townProgressState.ViewedWorldMapIndex = townProgressState.CurrentWorldMapIndex;
        SetVisibleRegionMap(townProgressState.ViewedWorldMapIndex);
        RefreshTownMapButtons();

        WorldMapPageUI pageUI =
            worldMapPage.GetComponent<WorldMapPageUI>() ??
            worldMapPage.gameObject.AddComponent<WorldMapPageUI>();
        pageUI.Configure(RefreshWorldMapPage);
        pageRouter.Register(worldMapPage);
    }

    private void CreateRegionMapPage(
        int worldMapIndex,
        string mapResourcePath,
        int[] townIndices,
        Vector2[] townPositions,
        Vector2[] dungeonPositions)
    {
        RectTransform regionPage =
            CreateUIObject($"Region Map {worldMapIndex}", worldMapPage);
        regionPage.anchorMin = Vector2.zero;
        regionPage.anchorMax = Vector2.one;
        regionPage.offsetMin = Vector2.zero;
        regionPage.offsetMax = Vector2.zero;
        regionMapPages.Add(regionPage);
        AddMapBackground(regionPage, mapResourcePath, "Maps/Map");

        for (int i = 1; i < townPositions.Length; i++)
        {
            CreateMapRoad(regionPage, townPositions[i - 1], townPositions[i]);
        }

        foreach (int townIndex in townIndices)
        {
            int localIndex = System.Array.IndexOf(townIndices, townIndex);
            townMapButtons.Add(CreateTownMapButton(
                regionPage,
                WorldMapService.TownNames[townIndex],
                townPositions[localIndex],
                () => townTravelController.TravelToTown(townIndex)));
            CreateMapButton(
                regionPage,
                GetNearbyDungeonMapLabel(townIndex),
                dungeonPositions[localIndex],
                new Vector2(112f, 42f),
                () => townTravelController.TravelToDungeon(townIndex));
        }

        Button globalMapButton = CreateMapButton(
            regionPage,
            "← 全体マップへ",
            new Vector2(-315f, -185f),
            new Vector2(150f, 46f),
            ShowGlobalMap);
        globalMapButton.targetGraphic.color =
            new Color(0.12f, 0.32f, 0.52f, 0.96f);

        CreateText(
            regionPage,
            $"{WorldMapService.WorldRegionNames[worldMapIndex]}  |  " +
            "未解放の町は街道戦闘に勝利すると解放されます。",
            14,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(14f, -34f),
            new Vector2(-14f, -4f),
            Color.white);
    }

    private static void CreateMapRoad(
        RectTransform parent,
        Vector2 start,
        Vector2 end)
    {
        Vector2 direction = end - start;
        RectTransform road = CreateUIObject("Town Road", parent);
        road.anchorMin = road.anchorMax = road.pivot =
            new Vector2(0.5f, 0.5f);
        road.sizeDelta = new Vector2(direction.magnitude, 9f);
        road.anchoredPosition = (start + end) * 0.5f;
        road.localRotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        Image roadImage = road.gameObject.AddComponent<Image>();
        roadImage.color = new Color(0.73f, 0.52f, 0.24f, 0.9f);
        roadImage.raycastTarget = false;

        Outline outline = road.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.18f, 0.09f, 0.03f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;
    }

    private string GetNearbyDungeonMapLabel(int townIndex)
    {
        DungeonDataSO dungeon = dungeonRunManager.GetDungeonNearTown(townIndex);
        return dungeon != null ? dungeon.dungeonName : "近隣ダンジョン";
    }

    private void BuildTownMapPage()
    {
        AddMapBackground(townMapPage, "Maps/TownMap");

        standardTownFacilityButtons.Clear();
        hireFacilityButton = CreateMapButton(
            townMapPage, "酒場\n雇用", new Vector2(-255f, 105f),
            new Vector2(110f, 54f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.TavernKey, ShowHirePage));
        standardTownFacilityButtons.Add(hireFacilityButton);
        standardTownFacilityButtons.Add(CreateMapButton(
            townMapPage, "商会組合", new Vector2(0f, 135f),
            new Vector2(110f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.GuildKey, ShowCompanyPage)));
        standardTownFacilityButtons.Add(CreateMapButton(
            townMapPage, "市場", new Vector2(175f, 105f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.MarketKey, ShowMarketPage)));
        CreateMapButton(
            townMapPage, "鍛冶屋", new Vector2(290f, 75f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.BlacksmithKey, ShowBlacksmithPage));
        standardTownFacilityButtons.Add(CreateMapButton(
            townMapPage, "倉庫", new Vector2(-260f, -45f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.WarehouseKey, ShowInventoryPage)));
        standardTownFacilityButtons.Add(CreateMapButton(
            townMapPage, "治療院", new Vector2(235f, -42f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.ClinicKey, ShowHealPage)));
        CreateMapButton(
            townMapPage, "近隣ダンジョン", new Vector2(0f, -172f),
            new Vector2(150f, 52f),
            () => dungeonBattleController.OpenNearbyDungeon());
        jobFacilityButton = CreateMapButton(
            townMapPage, "転職神殿", new Vector2(105f, -105f),
            new Vector2(110f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.TempleKey, ShowJobChangePage));
        standardTownFacilityButtons.Add(jobFacilityButton);
        Button continentButton = CreateMapButton(
            townMapPage, "← 地域マップへ", new Vector2(-300f, -172f),
            new Vector2(142f, 52f), ShowWorldMap);
        continentButton.targetGraphic.color =
            new Color(0.12f, 0.32f, 0.52f, 0.96f);
        ColorBlock continentColors = continentButton.colors;
        continentColors.normalColor = Color.white;
        continentColors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        continentColors.pressedColor = new Color(0.78f, 0.86f, 0.95f, 1f);
        continentButton.colors = continentColors;

        Outline continentOutline =
            continentButton.gameObject.AddComponent<Outline>();
        continentOutline.effectColor = new Color(0.35f, 0.72f, 1f, 0.9f);
        continentOutline.effectDistance = new Vector2(2f, -2f);

        TownMapPageUI pageUI =
            townMapPage.GetComponent<TownMapPageUI>() ??
            townMapPage.gameObject.AddComponent<TownMapPageUI>();
        pageUI.Configure(RefreshTownMapPage);
        pageRouter.Register(townMapPage);
    }

    private void AddMapBackground(
        RectTransform parent,
        string resourcePath,
        string fallbackResourcePath = null)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null && !string.IsNullOrEmpty(fallbackResourcePath))
        {
            texture = Resources.Load<Texture2D>(fallbackResourcePath);
        }
        RectTransform backgroundRect = CreateUIObject("Map Background", parent);
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        RawImage image = backgroundRect.gameObject.AddComponent<RawImage>();
        image.texture = texture;
        image.color = texture != null ? Color.white : RowColor;
        image.raycastTarget = false;
    }

    private Button CreateMapButton(
        RectTransform parent,
        string label,
        Vector2 position,
        Vector2 size,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateActionButton(parent, label, action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        button.targetGraphic.color = new Color(0.08f, 0.1f, 0.12f, 0.9f);
        return button;
    }

    private Button CreateTownMapButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateMapButton(
            parent,
            label,
            position,
            new Vector2(142f, 106f),
            action);
        button.targetGraphic.color = new Color(0.04f, 0.05f, 0.06f, 0.76f);

        Texture2D markerTexture = Resources.Load<Texture2D>("Maps/TownMarker");
        RectTransform markerRect =
            CreateUIObject("Town Marker Art", button.transform);
        markerRect.anchorMin = new Vector2(0.5f, 1f);
        markerRect.anchorMax = new Vector2(0.5f, 1f);
        markerRect.pivot = new Vector2(0.5f, 1f);
        markerRect.sizeDelta = new Vector2(78f, 70f);
        markerRect.anchoredPosition = new Vector2(0f, -3f);
        RawImage marker = markerRect.gameObject.AddComponent<RawImage>();
        marker.texture = markerTexture;
        marker.color = Color.white;
        marker.raycastTarget = false;
        markerRect.SetAsFirstSibling();

        Text labelText = button.GetComponentInChildren<Text>();
        if (labelText != null)
        {
            labelText.alignment = TextAnchor.LowerCenter;
            labelText.fontSize = 14;
            labelText.rectTransform.offsetMin = new Vector2(4f, 4f);
            labelText.rectTransform.offsetMax = new Vector2(-4f, -70f);
        }
        return button;
    }

    private void ShowGlobalMap()
    {
        SwitchToMapPage(globalMapPage, false);
    }

    private void RefreshGlobalMapPage()
    {
        bool newlyUnlocked = TryUnlockHiddenIsland();
        if (hiddenIslandRegionButton != null)
        {
            hiddenIslandRegionButton.gameObject.SetActive(
                townProgressState.IsTownUnlocked(
                    WorldMapService.HiddenIslandTownIndex));
        }
        statusText.text =
            newlyUnlocked
                ? "全条件を達成しました。中央島アステラへの航路が出現しました。"
                : $"現在地: {WorldMapService.TownNames[townProgressState.CurrentTownIndex]}  |  大陸を選択";
    }

    private void ShowWorldMap()
    {
        ShowWorldMap(townProgressState.CurrentWorldMapIndex);
    }

    private void SetVisibleRegionMap(int worldMapIndex)
    {
        for (int i = 0; i < regionMapPages.Count; i++)
        {
            if (regionMapPages[i] != null)
            {
                regionMapPages[i].gameObject.SetActive(i == worldMapIndex);
            }
        }
    }

    private void ShowTownMap()
    {
        SwitchToMapPage(townMapPage, false);
    }

    private void RefreshTownMapPage()
    {
        bool hiddenIsland = TownServicePolicy.IsHiddenIslandTown(
            townProgressState.CurrentTownIndex);
        statusText.text =
            hiddenIsland
                ? $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}  |  鍛冶屋と深層ダンジョンのみ利用可能"
                : $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}  |  利用する施設を選択";
        foreach (Button facilityButton in standardTownFacilityButtons)
        {
            if (facilityButton != null)
            {
                facilityButton.gameObject.SetActive(!hiddenIsland);
            }
        }

        if (hiddenIsland)
        {
            return;
        }

        if (jobFacilityButton != null)
        {
            jobFacilityButton.gameObject.SetActive(
                townProgressState.CurrentTownIndex == 0 ||
                townProgressState.CurrentTownIndex >= 3);
        }
        if (hireFacilityButton != null)
        {
            hireFacilityButton.gameObject.SetActive(
                TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex));
        }
    }

    private void HideTravelConfirmation()
    {
        travelConfirmationOverlay?.gameObject.SetActive(false);
        townTravelController.ClearTravelConfirmation();
    }

    private IEnumerator ContinueTownTravelBattleRoutine()
    {
        yield return null;

        townTravelController.StartNextTravelEncounter();
    }

    private void SetMapHeaderButtons(bool showTownMapButton)
    {
        if (mapButton != null)
        {
            mapButton.gameObject.SetActive(true);
        }

        if (townMapButton != null)
        {
            townMapButton.gameObject.SetActive(showTownMapButton);
        }
    }

    private void RefreshTownMapButtons()
    {
        for (int i = 0; i < townMapButtons.Count && i < WorldMapService.TownNames.Length; i++)
        {
            Button button = townMapButtons[i];
            if (button == null)
            {
                continue;
            }

            bool unlocked = townProgressState.IsTownUnlocked(i);
            if (i == WorldMapService.HiddenIslandTownIndex)
            {
                button.gameObject.SetActive(unlocked);
                if (!unlocked)
                {
                    continue;
                }
            }
            bool reachable =
                i == townProgressState.CurrentTownIndex ||
                WorldMapService.AreTownsAdjacent(i, townProgressState.CurrentTownIndex);
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                string state = i == townProgressState.CurrentTownIndex
                    ? "\n【現在地】"
                    : !reachable
                        ? "\n【要経由】"
                        : unlocked
                        ? string.Empty
                        : "\n【未解放】";
                label.text = WorldMapService.TownNames[i] + state;
                label.color = unlocked && reachable
                    ? Color.white
                    : new Color(0.38f, 0.4f, 0.42f, 1f);
            }

            button.interactable = reachable;
            button.targetGraphic.color = unlocked && reachable
                ? new Color(0.04f, 0.05f, 0.06f, 0.76f)
                : new Color(0.005f, 0.005f, 0.008f, 0.97f);

            RawImage[] markerImages = button.GetComponentsInChildren<RawImage>();
            foreach (RawImage markerImage in markerImages)
            {
                markerImage.color = unlocked && reachable
                    ? Color.white
                    : new Color(0.035f, 0.035f, 0.04f, 1f);
            }
        }
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
