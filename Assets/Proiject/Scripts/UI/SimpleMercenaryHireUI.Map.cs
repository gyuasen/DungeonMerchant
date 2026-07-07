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

    private static void StyleUnavailableWorldMapButton(Button button)
    {
        ConfigureWorldRegionHover(
            button,
            new Color(0.12f, 0.14f, 0.17f, 0.42f));
        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.color = new Color(0.72f, 0.75f, 0.78f, 1f);
        }
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
            CreateActionButton(window, "移動する", ConfirmTownTravel);
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

        viewedWorldMapIndex = CurrentWorldMapIndex;
        SetVisibleRegionMap(viewedWorldMapIndex);
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
                TownNames[townIndex],
                townPositions[localIndex],
                () => TravelToTown(townIndex)));
            CreateMapButton(
                regionPage,
                GetNearbyDungeonMapLabel(townIndex),
                dungeonPositions[localIndex],
                new Vector2(112f, 42f),
                () => TravelToDungeon(townIndex));
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
            $"{WorldRegionNames[worldMapIndex]}  |  " +
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

        hireFacilityButton = CreateMapButton(
            townMapPage, "酒場\n雇用", new Vector2(-255f, 105f),
            new Vector2(110f, 54f), ShowHirePage);
        CreateMapButton(
            townMapPage, "商会本部", new Vector2(0f, 135f),
            new Vector2(110f, 48f), ShowCompanyPage);
        CreateMapButton(
            townMapPage, "市場", new Vector2(175f, 105f),
            new Vector2(100f, 48f), ShowMarketPage);
        CreateMapButton(
            townMapPage, "鍛冶屋", new Vector2(290f, 75f),
            new Vector2(100f, 48f), ShowBlacksmithPage);
        CreateMapButton(
            townMapPage, "倉庫", new Vector2(-260f, -45f),
            new Vector2(100f, 48f), ShowInventoryPage);
        CreateMapButton(
            townMapPage, "編成所", new Vector2(-105f, -20f),
            new Vector2(100f, 48f), ShowPartyPage);
        CreateMapButton(
            townMapPage, "治療院", new Vector2(235f, -42f),
            new Vector2(100f, 48f), ShowHealPage);
        CreateMapButton(
            townMapPage, "近隣ダンジョン", new Vector2(0f, -172f),
            new Vector2(150f, 52f), OpenNearbyDungeon);
        jobFacilityButton = CreateMapButton(
            townMapPage, "転職神殿", new Vector2(105f, -105f),
            new Vector2(110f, 48f), ShowJobChangePage);
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
        statusText.text =
            $"現在地: {TownNames[currentTownIndex]}  |  大陸を選択";
    }

    private void ShowUnavailableWorldMap(string worldName)
    {
        statusText.text =
            $"{worldName}は今後のアップデートで解放されます。";
    }

    private void ShowWorldMap()
    {
        ShowWorldMap(CurrentWorldMapIndex);
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

    private bool CanEnterWorldRegion(int worldMapIndex)
    {
        return WorldMapService.CanEnterWorldRegion(
            worldMapIndex,
            currentTownIndex,
            unlockedTownIndices,
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

    private void ShowTownMap()
    {
        SwitchToMapPage(townMapPage, false);
    }

    private void RefreshTownMapPage()
    {
        statusText.text =
            $"{TownNames[currentTownIndex]}  |  利用する施設を選択";
        if (jobFacilityButton != null)
        {
            jobFacilityButton.gameObject.SetActive(
                currentTownIndex == 0 || currentTownIndex >= 3);
        }
        if (hireFacilityButton != null)
        {
            hireFacilityButton.gameObject.SetActive(
                TownServicePolicy.IsHiringAvailable(currentTownIndex));
        }
    }

    private void TravelToTown(int townIndex)
    {
        townIndex = Mathf.Clamp(townIndex, 0, TownNames.Length - 1);

        if (townIndex == currentTownIndex)
        {
            ShowTownMap();
            return;
        }

        RequestTownTravel(townIndex, false);
    }

    private void RequestTownTravel(
        int townIndex,
        bool openDungeonAfterTravel)
    {
        if (!AreTownsAdjacent(townIndex, currentTownIndex))
        {
            int nextTownIndex =
                GetNextTownToward(currentTownIndex, townIndex);
            statusText.text =
                $"{TownNames[townIndex]}へ直接は移動できません。" +
                (nextTownIndex >= 0
                    ? $"先に{TownNames[nextTownIndex]}を経由してください。"
                    : string.Empty);
            return;
        }

        int destinationWorld = GetWorldMapIndexForTown(townIndex);
        if (!CanEnterWorldRegion(destinationWorld))
        {
            statusText.text =
                $"{WorldRegionNames[destinationWorld]}の解放条件を満たしていません。";
            return;
        }

        bool isUnlocked = unlockedTownIndices.Contains(townIndex);
        if (!isUnlocked)
        {
            int nextTownIndex = GetNextUnlockableTownIndex();
            if (townIndex != nextTownIndex)
            {
                statusText.text = nextTownIndex >= 0
                    ? $"先に{TownNames[nextTownIndex]}への移動クエストを攻略してください。"
                    : "これ以上解放できる町はありません。";
                return;
            }
        }

        if (partyManager.Members.Count == 0)
        {
            statusText.text =
                "町の移動には街道戦闘が発生するため、傭兵の編成が必要です。";
            return;
        }

        confirmationTravelTownIndex = townIndex;
        confirmationOpenDungeonAfterTravel = openDungeonAfterTravel;
        string unlockNotice = isUnlocked
            ? string.Empty
            : "\n勝利すると新しい町が解放されます。";
        travelConfirmationText.text =
            $"{TownNames[currentTownIndex]} から\n" +
            $"{TownNames[townIndex]} へ移動します。\n\n" +
            $"・両地域の通常モンスターと3～5回接敵します\n" +
            $"・勝利すると1日経過します" +
            unlockNotice;
        travelConfirmationOverlay.SetAsLastSibling();
        travelConfirmationOverlay.gameObject.SetActive(true);
    }

    private void ConfirmTownTravel()
    {
        int destinationTownIndex = confirmationTravelTownIndex;
        bool openDungeonAfterTravel = confirmationOpenDungeonAfterTravel;
        HideTravelConfirmation();

        if (destinationTownIndex < 0)
        {
            return;
        }

        StartTownTravelBattle(
            destinationTownIndex,
            openDungeonAfterTravel);
    }

    private void HideTravelConfirmation()
    {
        travelConfirmationOverlay?.gameObject.SetActive(false);
        confirmationTravelTownIndex = -1;
        confirmationOpenDungeonAfterTravel = false;
    }

    private void StartTownTravelBattle(
        int destinationTownIndex,
        bool openDungeonAfterTravel)
    {
        if (battleManager.IsBattling)
        {
            statusText.text = "戦闘中は町を移動できません。";
            return;
        }

        if (partyManager.Members.Count == 0)
        {
            statusText.text =
                $"{TownNames[destinationTownIndex]}への移動クエストには傭兵の編成が必要です。";
            return;
        }

        if (!AreTownsAdjacent(destinationTownIndex, currentTownIndex))
        {
            statusText.text = "街道で結ばれていない町へは移動できません。";
            return;
        }

        ResetBattleLog();
        pendingTravelTownIndex = destinationTownIndex;
        pendingTravelWasUnlock =
            !unlockedTownIndices.Contains(destinationTownIndex);
        pendingOpenDungeonAfterTravel = openDungeonAfterTravel;
        pendingTravelEncounterCount = UnityEngine.Random.Range(3, 6);
        pendingTravelEncounterIndex = 1;
        isAwaitingRoadTravelChoice = false;
        List<EnemyDataSO> enemies =
            roadEncounterService.CreateEncounter(
                currentTownIndex,
                destinationTownIndex,
                out pendingRoadRareEncounter);

        if (enemies == null ||
            enemies.Count == 0 ||
            !battleManager.StartBattle(partyManager.Members, enemies))
        {
            pendingTravelTownIndex = -1;
            pendingTravelWasUnlock = false;
            pendingOpenDungeonAfterTravel = false;
            pendingTravelEncounterCount = 0;
            pendingTravelEncounterIndex = 0;
            isAwaitingRoadTravelChoice = false;
            statusText.text = "街道戦闘を開始できませんでした。";
            return;
        }

        ShowRoadBattlePage(currentTownIndex, destinationTownIndex);
        statusText.text =
            $"町の移動: 街道戦闘 {pendingTravelEncounterIndex}/" +
            $"{pendingTravelEncounterCount}";
    }

    private IEnumerator ContinueTownTravelBattleRoutine()
    {
        yield return null;

        int destinationTownIndex = pendingTravelTownIndex;
        if (destinationTownIndex < 0)
        {
            yield break;
        }

        List<EnemyDataSO> enemies =
            roadEncounterService.CreateEncounter(
                currentTownIndex,
                destinationTownIndex,
                out pendingRoadRareEncounter);
        if (enemies == null ||
            enemies.Count == 0 ||
            !battleManager.StartBattle(partyManager.Members, enemies))
        {
            pendingTravelTownIndex = -1;
            pendingTravelWasUnlock = false;
            pendingOpenDungeonAfterTravel = false;
            pendingTravelEncounterCount = 0;
            pendingTravelEncounterIndex = 0;
            isAwaitingRoadTravelChoice = false;
            ShowWorldMap();
            statusText.text = "次の街道戦闘を開始できませんでした。";
            yield break;
        }

        ShowRoadBattlePage(currentTownIndex, destinationTownIndex);
        statusText.text =
            $"町の移動: 街道戦闘 {pendingTravelEncounterIndex}/" +
            $"{pendingTravelEncounterCount}";
    }

    private void ContinueTownTravel()
    {
        if (!isAwaitingRoadTravelChoice ||
            pendingTravelTownIndex < 0 ||
            battleManager.IsBattling)
        {
            return;
        }

        isAwaitingRoadTravelChoice = false;
        roadContinueButton.gameObject.SetActive(false);
        roadRetreatButton.gameObject.SetActive(false);
        pendingTravelEncounterIndex++;
        statusText.text =
            $"街道戦闘 {pendingTravelEncounterIndex}/" +
            $"{pendingTravelEncounterCount} の敵が接近しています。";
        StartCoroutine(ContinueTownTravelBattleRoutine());
    }

    private void RetreatFromTownTravel()
    {
        if (!isAwaitingRoadTravelChoice || battleManager.IsBattling)
        {
            return;
        }

        pendingTravelTownIndex = -1;
        pendingTravelWasUnlock = false;
        pendingOpenDungeonAfterTravel = false;
        pendingTravelEncounterCount = 0;
        pendingTravelEncounterIndex = 0;
        isAwaitingRoadTravelChoice = false;
        pendingRoadRareEncounter = false;
        roadContinueButton.gameObject.SetActive(false);
        roadRetreatButton.gameObject.SetActive(false);
        ShowTownMap();
        statusText.text =
            "街道から撤退し、出発した町へ戻りました。";
    }

    private void OpenNearbyDungeon()
    {
        DungeonDataSO preferred =
            dungeonRunManager.GetDungeonNearTown(currentTownIndex);
        if (preferred == null)
        {
            statusText.text =
                $"{TownNames[currentTownIndex]}近隣に探索可能なダンジョンはありません。";
        }
        else if (!dungeonRunManager.TrySelectDungeon(preferred))
        {
            statusText.text =
                $"{TownNames[currentTownIndex]}近隣のダンジョンは未開放です。";
        }
        ShowDungeonPage();
    }

    private void TravelToDungeon(int townIndex)
    {
        townIndex = Mathf.Clamp(townIndex, 0, TownNames.Length - 1);

        if (townIndex == currentTownIndex)
        {
            OpenNearbyDungeon();
            return;
        }

        RequestTownTravel(townIndex, true);
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
        for (int i = 0; i < townMapButtons.Count && i < TownNames.Length; i++)
        {
            Button button = townMapButtons[i];
            if (button == null)
            {
                continue;
            }

            bool unlocked = unlockedTownIndices.Contains(i);
            bool reachable =
                i == currentTownIndex ||
                AreTownsAdjacent(i, currentTownIndex);
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                string state = i == currentTownIndex
                    ? "\n【現在地】"
                    : !reachable
                        ? "\n【要経由】"
                        : unlocked
                        ? string.Empty
                        : "\n【未解放】";
                label.text = TownNames[i] + state;
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

    private int GetNextUnlockableTownIndex()
    {
        return WorldMapService.GetNextUnlockableTownIndex(
            unlockedTownIndices);
    }

    private void ApplyTownServiceSettings(
        bool regenerateCandidates,
        bool regenerateMarket)
    {
        if (mercenaryGenerator != null)
        {
            mercenaryGenerator.SetTownIndex(currentTownIndex, false);
            if (!TownServicePolicy.IsHiringAvailable(currentTownIndex))
            {
                mercenaryGenerator.ClearCandidates();
            }
            else if (regenerateCandidates)
            {
                mercenaryGenerator.GenerateCandidates();
            }
        }
        marketStockManager?.SetTownIndex(
            currentTownIndex, regenerateMarket);
        blacksmithManager?.SetTownIndex(currentTownIndex);
    }

    private void SyncDungeonUnlocks()
    {
        if (dungeonRunManager == null)
        {
            dungeonRunManager =
                GetComponent<DungeonRunManager>() ??
                FindObjectOfType<DungeonRunManager>();
        }

        dungeonRunManager?.SetUnlockedTownIndices(GetUnlockedTownIndices());
    }

}
