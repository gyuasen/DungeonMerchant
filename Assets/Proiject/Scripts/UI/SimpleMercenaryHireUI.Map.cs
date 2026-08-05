using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildTravelConfirmationOverlay()
    {
        travelConfirmation.overlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.TravelConfirmation,
                "Travel Confirmation Overlay");
        travelConfirmation.overlay.gameObject.SetActive(false);
        travelConfirmation.overlay.anchorMin = Vector2.zero;
        travelConfirmation.overlay.anchorMax = Vector2.one;
        travelConfirmation.overlay.offsetMin = Vector2.zero;
        travelConfirmation.overlay.offsetMax = Vector2.zero;
        travelConfirmation.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.84f);

        RectTransform window =
            CreateUIObject("Travel Confirmation Window", travelConfirmation.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(780f, 650f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "町を移動しますか？",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(24f, -58f),
            new Vector2(-24f, -16f),
            ParchmentTextColor);

        travelConfirmation.text = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -116f),
            new Vector2(-36f, -64f),
            ParchmentMutedColor);

        travelConfirmation.cargoSummaryText = CreateText(
            window,
            string.Empty,
            17,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(36f, -154f),
            new Vector2(-36f, -120f),
            ParchmentTextColor);
        travelConfirmation.cargoContent = CreateScrollableContent(
            window,
            "Travel Cargo Viewport",
            "Travel Cargo Content",
            new Vector2(36f, 96f),
            new Vector2(-36f, -170f));

        Button confirmButton =
            CreateActionButton(
                window,
                "移動する",
                ConfirmTownTravelWithCargo);
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

        travelConfirmation.overlay.gameObject.SetActive(false);
    }

    private void ConfirmTownTravelWithCargo()
    {
        List<RoadCargoEntry> cargo = new List<RoadCargoEntry>();
        foreach (KeyValuePair<ItemDataSO, int> entry in travelConfirmation.selectedCargo)
        {
            if (entry.Key != null && entry.Value > 0)
            {
                cargo.Add(new RoadCargoEntry(entry.Key, entry.Value));
            }
        }
        townTravelController.ConfirmTownTravel(
            cargo,
            new List<string>(travelConfirmation.selectedCompanions));
    }

    private void RefreshTravelCargoSelection()
    {
        if (travelConfirmation.cargoContent == null || townTravelController == null)
        {
            return;
        }

        for (int i = travelConfirmation.cargoContent.childCount - 1; i >= 0; i--)
        {
            Destroy(travelConfirmation.cargoContent.GetChild(i).gameObject);
        }

        int origin = townTravelController.ConfirmationOriginTownIndex;
        int destination = townTravelController.ConfirmationDestinationTownIndex;
        int used = 0;
        foreach (int amount in travelConfirmation.selectedCargo.Values)
        {
            used += amount;
        }
        int capacity = roadCargoSession != null ? roadCargoSession.Capacity : 0;
        travelConfirmation.cargoSummaryText.text =
            $"積載量 {used} / {capacity}　同行傭兵 {travelConfirmation.selectedCompanions.Count}人";
        float top = -10f;
        bool hasCargo = false;
        foreach (InventoryItemStack stack in merchantInventory.GetItemsIn(origin))
        {
            ItemDataSO item = stack?.Item;
            if (item == null ||
                (item.itemType != ItemType.Material &&
                 item.itemType != ItemType.Consumable))
            {
                continue;
            }

            hasCargo = true;
            travelConfirmation.selectedCargo.TryGetValue(item, out int selected);
            int currentPrice = GetTravelSellPrice(origin, item);
            int destinationPrice = GetTravelSellPrice(destination, item);
            int difference = destinationPrice - currentPrice;
            CreateTravelCargoRow(
                item,
                stack.Amount,
                selected,
                currentPrice,
                destinationPrice,
                difference,
                ref top);
        }

        if (!hasCargo)
        {
            CreateScrollLabel(travelConfirmation.cargoContent, "積める素材・消耗品はありません。", ref top);
        }
        top -= 12f;
        CreateScrollLabel(travelConfirmation.cargoContent, "同行する傭兵", ref top);
        bool hasCompanion = false;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (!CanTravelWithCompanion(mercenary, origin))
            {
                continue;
            }
            hasCompanion = true;
            CreateTravelCompanionRow(mercenary, ref top);
        }
        if (!hasCompanion)
        {
            CreateScrollLabel(
                travelConfirmation.cargoContent,
                "同行できる非編成傭兵はいません。",
                ref top);
        }
        travelConfirmation.cargoContent.sizeDelta = new Vector2(0f, Mathf.Max(260f, -top + 12f));
    }

    private bool CanTravelWithCompanion(MercenaryInstance mercenary, int origin)
    {
        return mercenary != null && mercenary.IsContractActive &&
               mercenary.CurrentTownIndex == origin &&
               !MercenaryDutyService.IsOnDuty(mercenary.InstanceId);
    }

    private void CreateTravelCompanionRow(
        MercenaryInstance mercenary,
        ref float top)
    {
        bool selected = travelConfirmation.selectedCompanions.Contains(mercenary.InstanceId);
        CreateScrollLabel(
            travelConfirmation.cargoContent,
            mercenary.MercenaryName + (mercenary.IsIncapacitated ? "（戦闘不能）" : ""),
            ref top);
        Button toggle = CreateActionButton(
            travelConfirmation.cargoContent,
            selected ? "解除" : "同行",
            () => ToggleTravelCompanion(mercenary.InstanceId));
        ConfigureTravelCargoStepButton(toggle, new Vector2(-18f, top + 18f));
        top -= 8f;
    }

    private void ToggleTravelCompanion(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return;
        }
        if (!travelConfirmation.selectedCompanions.Add(instanceId))
        {
            travelConfirmation.selectedCompanions.Remove(instanceId);
        }
        RefreshTravelCargoSelection();
    }

    private void CreateTravelCargoRow(
        ItemDataSO item,
        int owned,
        int selected,
        int currentPrice,
        int destinationPrice,
        int difference,
        ref float top)
    {
        string differenceText = difference >= 0 ? $"+{difference}" : difference.ToString();
        Text label = CreateText(
            travelConfirmation.cargoContent,
            $"{JapaneseDisplayText.GetItemName(item)} 所持 {owned} / 積載 {selected}\n" +
            $"売値 {currentPrice}G → {destinationPrice}G （差額 {differenceText}G）",
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(12f, top - 50f),
            new Vector2(-130f, top),
            ParchmentMutedColor);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;

        Button minus = CreateActionButton(
            travelConfirmation.cargoContent,
            "－",
            () => ChangeTravelCargo(item, -1));
        ConfigureTravelCargoStepButton(minus, new Vector2(-112f, top - 30f));
        Button plus = CreateActionButton(
            travelConfirmation.cargoContent,
            "＋",
            () => ChangeTravelCargo(item, 1));
        ConfigureTravelCargoStepButton(plus, new Vector2(-56f, top - 30f));
        top -= 58f;
    }

    private static void ConfigureTravelCargoStepButton(Button button, Vector2 position)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(48f, 32f);
        rect.anchoredPosition = position;
    }

    private void ChangeTravelCargo(ItemDataSO item, int delta)
    {
        if (item == null || delta == 0)
        {
            return;
        }

        travelConfirmation.selectedCargo.TryGetValue(item, out int selected);
        int owned = merchantInventory.GetItemAmountIn(
            townTravelController.ConfirmationOriginTownIndex,
            item);
        int used = 0;
        foreach (int amount in travelConfirmation.selectedCargo.Values)
        {
            used += amount;
        }
        int capacity = roadCargoSession != null ? roadCargoSession.Capacity : 0;
        if (delta > 0 && (selected >= owned || used >= capacity))
        {
            return;
        }

        selected = Mathf.Clamp(selected + delta, 0, owned);
        if (selected == 0)
        {
            travelConfirmation.selectedCargo.Remove(item);
        }
        else
        {
            travelConfirmation.selectedCargo[item] = selected;
        }
        RefreshTravelCargoSelection();
    }

    private int GetTravelSellPrice(int townIndex, ItemDataSO item)
    {
        int basePrice = marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item)
            : item.basePrice;
        return Mathf.Max(1, Mathf.RoundToInt(basePrice *
            WorldMapService.GetTownDemandMultiplier(townIndex, item)));
    }

    private void BuildTownMapPage()
    {
        townMapBackgroundImage = mapPresenter.AddMapBackground(townMapPage, "Maps/TownMap");

        standardTownFacilityButtons.Clear();
        hireFacilityButton = mapPresenter.CreateMapButton(
            townMapPage, "酒場\n雇用", new Vector2(-255f, 105f),
            new Vector2(110f, 54f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.TavernKey, ShowHirePage));
        standardTownFacilityButtons.Add(hireFacilityButton);
        standardTownFacilityButtons.Add(mapPresenter.CreateMapButton(
            townMapPage, "商会組合", new Vector2(0f, 135f),
            new Vector2(110f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.GuildKey, ShowCompanyPage)));
        standardTownFacilityButtons.Add(mapPresenter.CreateMapButton(
            townMapPage, "市場", new Vector2(175f, 105f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.MarketKey, ShowMarketPage)));
        mapPresenter.CreateMapButton(
            townMapPage, "鍛冶屋", new Vector2(290f, 75f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.BlacksmithKey, ShowBlacksmithPage));
        standardTownFacilityButtons.Add(mapPresenter.CreateMapButton(
            townMapPage, "倉庫", new Vector2(-260f, -45f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.WarehouseKey, ShowInventoryPage)));
        standardTownFacilityButtons.Add(mapPresenter.CreateMapButton(
            townMapPage, "治療院", new Vector2(235f, -42f),
            new Vector2(100f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.ClinicKey, ShowHealPage)));
        mapPresenter.CreateMapButton(
            townMapPage, "近隣ダンジョン", new Vector2(0f, -172f),
            new Vector2(150f, 52f),
            () => dungeonBattleController.OpenNearbyDungeon());
        jobFacilityButton = mapPresenter.CreateMapButton(
            townMapPage, "転職神殿", new Vector2(105f, -105f),
            new Vector2(110f, 48f),
            () => OpenFacilityWithGreeting(FacilityGreetingController.TempleKey, ShowJobChangePage));
        standardTownFacilityButtons.Add(jobFacilityButton);
        trainingGroundFacilityButton = mapPresenter.CreateMapButton(
            townMapPage, "修練場", new Vector2(-105f, -105f),
            new Vector2(110f, 48f),
            () => OpenFacilityWithGreeting(
                FacilityGreetingController.TrainingGroundKey,
                ShowTrainingGroundPage));
        standardTownFacilityButtons.Add(trainingGroundFacilityButton);
        roadBattle.cargoReceiveButton = mapPresenter.CreateMapButton(
            townMapPage,
            "街道荷物\n受取",
            new Vector2(285f, -172f),
            new Vector2(118f, 52f),
            () => townTravelController.TryReceivePendingRoadCargo());
        roadBattle.cargoReceiveButton.targetGraphic.color = AccentColor;
        roadBattle.cargoReceiveButton.gameObject.SetActive(false);
        Button continentButton = mapPresenter.CreateMapButton(
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

    // Swaps the town map background to the current town's dedicated image,
    // falling back to the shared TownMap when a town image is missing.
    private void UpdateTownMapBackground()
    {
        if (townMapBackgroundImage == null)
        {
            return;
        }

        string townImagePath = WorldMapService.GetTownMapImageResourcePath(
            townProgressState.CurrentTownIndex);
        Texture2D texture = string.IsNullOrEmpty(townImagePath)
            ? null
            : Resources.Load<Texture2D>(townImagePath);
        if (texture == null)
        {
            texture = Resources.Load<Texture2D>("Maps/TownMap");
        }
        townMapBackgroundImage.texture = texture;
        townMapBackgroundImage.color = texture != null ? Color.white : RowColor;
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
        UpdateTownMapBackground();
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
            if (roadBattle.cargoReceiveButton != null)
            {
                roadBattle.cargoReceiveButton.gameObject.SetActive(
                    townTravelController.CanReceivePendingRoadCargo());
            }
            return;
        }

        if (roadBattle.cargoReceiveButton != null)
        {
            roadBattle.cargoReceiveButton.gameObject.SetActive(
                townTravelController.CanReceivePendingRoadCargo());
        }

        if (jobFacilityButton != null)
        {
            jobFacilityButton.gameObject.SetActive(
                TownServicePolicy.IsJobChangeAvailable(
                    townProgressState.CurrentTownIndex));
        }
        if (hireFacilityButton != null)
        {
            hireFacilityButton.gameObject.SetActive(
                TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex));
        }
        if (trainingGroundFacilityButton != null)
        {
            trainingGroundFacilityButton.gameObject.SetActive(
                TownServicePolicy.IsTrainingGroundAvailable(
                    townProgressState.CurrentTownIndex));
        }
    }

    private void HideTravelConfirmation()
    {
        travelConfirmation.overlay?.gameObject.SetActive(false);
        travelConfirmation.selectedCargo.Clear();
        travelConfirmation.selectedCompanions.Clear();
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
