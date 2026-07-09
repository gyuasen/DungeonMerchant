using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildGlobalMenuOverlay()
    {
        globalMenuOverlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.GlobalMenu,
                "Global Menu Overlay");
        globalMenuOverlay.anchorMin = Vector2.zero;
        globalMenuOverlay.anchorMax = Vector2.one;
        globalMenuOverlay.offsetMin = Vector2.zero;
        globalMenuOverlay.offsetMax = Vector2.zero;
        globalMenuOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.78f);

        RectTransform window =
            CreateUIObject("Global Menu Window", globalMenuOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(570f, 430f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "メニュー",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(40f, -72f),
            new Vector2(-40f, -22f),
            ParchmentTextColor);

        CreateGlobalMenuButton(
            window, "傭兵一覧", new Vector2(-135f, 80f),
            () => OpenGlobalMenuDestination(ShowCompanyPage));
        CreateGlobalMenuButton(
            window, "パーティー編成", new Vector2(135f, 80f),
            () => OpenGlobalMenuDestination(ShowPartyPage));
        CreateGlobalMenuButton(
            window, "在庫確認", new Vector2(-135f, 15f),
            () => OpenGlobalMenuDestination(ShowInventoryPage));
        CreateGlobalMenuButton(
            window, "装備図鑑", new Vector2(135f, 15f),
            () => OpenGlobalMenuDestination(ShowEquipmentCollection));
        CreateGlobalMenuButton(
            window, "商人情報", new Vector2(-135f, -50f),
            () => OpenGlobalMenuDestination(ShowMerchantStatusOverlay));
        CreateGlobalMenuButton(
            window, "依頼確認", new Vector2(135f, -50f),
            () => OpenGlobalMenuDestination(ShowQuestOverlay));
        CreateGlobalMenuButton(
            window, "地域マップ", new Vector2(-135f, -115f),
            () => OpenGlobalMenuDestination(ShowWorldMap));
        CreateGlobalMenuButton(
            window, "閉じる", new Vector2(135f, -115f),
            HideGlobalMenu);

        globalMenuOverlay.gameObject.SetActive(false);
    }

    private void CreateGlobalMenuButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateActionButton(parent, label, action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot =
            new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220f, 50f);
        rect.anchoredPosition = position;
    }

    private void ShowGlobalMenu()
    {
        if (battleManager.IsBattling)
        {
            statusText.text = "戦闘中はメニューを開けません。";
            return;
        }

        globalMenuOverlay.SetAsLastSibling();
        globalMenuOverlay.gameObject.SetActive(true);
    }

    private void HideGlobalMenu()
    {
        globalMenuOverlay?.gameObject.SetActive(false);
    }

    private void OpenGlobalMenuDestination(
        UnityEngine.Events.UnityAction destination)
    {
        HideGlobalMenu();
        destination?.Invoke();
    }

    private static void ApplyParchmentPanel(Image target) =>
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(target);

    private RectTransform CreateRow(string rowName, RectTransform parent, float top) =>
        uiFactory.CreateRow(rowName, parent, top);

    private Button CreateNavigationButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action) =>
        uiFactory.CreateNavigationButton(parent, label, position, action);

    private Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action) =>
        uiFactory.CreateActionButton(parent, label, action);

    private static RectTransform CreateButtonFromPrefab(
        string resourcePath,
        string objectName,
        RectTransform parent) =>
        SimpleMercenaryHireUIFactory.CreateButtonFromPrefab(resourcePath, objectName, parent);

    private void SetOrCreateButtonLabel(
        RectTransform parent,
        string label,
        int fontSize) =>
        uiFactory.SetOrCreateButtonLabel(parent, label, fontSize);

    private static void AddFantasyFrame(Image image, float thickness) =>
        SimpleMercenaryHireUIFactory.AddFantasyFrame(image, thickness);

    private static void ApplyButtonTransitions(Button button) =>
        SimpleMercenaryHireUIFactory.ApplyButtonTransitions(button);

    private void CreateButtonLabel(RectTransform parent, string label, int fontSize) =>
        uiFactory.CreateButtonLabel(parent, label, fontSize);

    private void SwitchToPage(
        RectTransform targetPage,
        Button activeTab = null)
    {
        pageRouter.Show(targetPage);
        SetMapHeaderButtons(true);
        SetAllTabsInactive();
        SetTabActive(activeTab, true);
    }

    private void RefreshPage(RectTransform pageRoot)
    {
        pageRouter?.Refresh(pageRoot);
    }

    private void SwitchToMapPage(
        RectTransform targetPage,
        bool showTownMapButton)
    {
        pageRouter.Show(targetPage);
        SetAllTabsInactive();
        SetMapHeaderButtons(showTownMapButton);
    }

    private void SetAllTabsInactive()
    {
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
    }

    private void RefreshUI()
    {
        if (globalMenuButton != null)
        {
            globalMenuButton.interactable = !battleManager.IsBattling;
        }
        goldText.text =
            $"商人Lv{merchantData.MerchantLevel}  所持金 {merchantData.Gold} G";
        if (dayText != null)
        {
            dayText.text = debtManager != null
                ? $"{dayManager.CurrentDay}日目 / {debtManager.CurrentMonth}月目"
                : $"{dayManager.CurrentDay}日目";
        }

        RefreshTownMapButtons();

        for (int i = 0; i < hireButtons.Count; i++)
        {
            MercenaryDataSO candidate = displayedCandidates[i];
            hireButtons[i].interactable =
                !hiredCandidates.Contains(candidate) && hireManager.CanAfford(candidate);
        }

        for (int i = 0; i < generatedHireButtons.Count; i++)
        {
            generatedHireButtons[i].interactable =
                hireManager.CanAfford(displayedGeneratedCandidates[i]);
        }

        for (int i = 0; i < marketBuyButtons.Count; i++)
        {
            marketBuyButtons[i].interactable =
                marketStockManager.CanBuy(displayedMarketEntries[i]);
        }

        for (int i = 0; i < blacksmithCraftButtons.Count; i++)
        {
            blacksmithCraftButtons[i].interactable =
                blacksmithManager.CanCraft(displayedBlacksmithRecipes[i]);
        }

        if (marketInfoText != null)
        {
            marketInfoText.text =
                $"{marketPriceManager.GetMarketSummary()}  |  売却価格は日ごとに更新";
        }

        if (startDungeonButton != null)
        {
            DungeonDataSO selectedDungeon =
                dungeonRunManager.SelectedDungeon;
            startDungeonButton.gameObject.SetActive(!dungeonRunManager.IsRunning);
            startDungeonButton.interactable =
                partyManager.Members.Count > 0 &&
                !battleManager.IsBattling &&
                !dungeonRunManager.IsRunning &&
                selectedDungeon != null &&
                selectedDungeon.nearbyTownIndex == townProgressState.CurrentTownIndex;
        }
    }

    private static void SetTabActive(Button button, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        button.targetGraphic.color = isActive ? AccentColor : InactiveColor;
    }

    private static void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private Canvas CreateCanvas() => uiFactory.CreateCanvas();

    private SimpleMercenaryHireUIView CreateView()
    {
        SimpleMercenaryHireUIView prefab = uiViewPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<SimpleMercenaryHireUIView>(
                "UI/SimpleMercenaryHireUIView");
        }

        if (prefab != null)
        {
            SimpleMercenaryHireUIView view = Instantiate(prefab);
            Image panelImage = view.GuildPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                ApplyParchmentPanel(panelImage);
            }
            return view;
        }

        Canvas canvas = CreateCanvas();
        RectTransform panel = CreatePanel(canvas.transform);
        SimpleMercenaryHireUIView fallbackView =
            canvas.gameObject.AddComponent<SimpleMercenaryHireUIView>();
        fallbackView.Initialize(canvas, panel);
        Debug.LogWarning(
            "SimpleMercenaryHireUIView prefab was not found. " +
            "The runtime-generated fallback UI is being used.");
        return fallbackView;
    }

    private void BindPageLayout(
        SimpleMercenaryHireUIView view,
        RectTransform panel)
    {
        hirePage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Hire, "Hire Page");
        globalMapPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.GlobalMap, "Global Map Page");
        worldMapPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.WorldMap, "World Map Page");
        townMapPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.TownMap, "Town Map Page");
        companyPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Company, "Company Page");
        partyPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Party, "Party Page");
        healPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Heal, "Heal Page");
        battlePage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Battle, "Battle Page");
        roadBattlePage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.RoadBattle, "Road Battle Page");
        dungeonPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Dungeon, "Dungeon Page");
        marketPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Market, "Market Page");
        blacksmithPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Blacksmith, "Blacksmith Page");
        inventoryPage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.Inventory, "Inventory Page");
        jobChangePage = GetOrCreatePage(
            view, panel, SimpleMercenaryHirePageSlot.JobChange, "Job Change Page");

        if (view.HasHireCompanyLayout)
        {
            view.HireCompany.GetOrCreateHirePageUI();
            view.HireCompany.GetOrCreateCompanyPageUI();
        }
    }

    private RectTransform GetOrCreateOverlay(
        SimpleMercenaryHireOverlaySlot slot,
        string overlayName)
    {
        RectTransform prefabOverlay =
            activeView != null ? activeView.GetOverlay(slot) : null;
        return prefabOverlay != null
            ? prefabOverlay
            : CreateUIObject(overlayName, overlayRoot);
    }

    private static RectTransform GetOrCreatePage(
        SimpleMercenaryHireUIView view,
        RectTransform panel,
        SimpleMercenaryHirePageSlot slot,
        string pageName)
    {
        RectTransform prefabPage = view.GetPage(slot);
        return prefabPage != null
            ? prefabPage
            : CreatePage(pageName, panel);
    }

    private void BindChromeLayout(SimpleMercenaryHireUIView view)
    {
        SimpleMercenaryHireUIView.ChromeReferences chrome = view.Chrome;
        mapButton = chrome.mapButton;
        townMapButton = chrome.townMapButton;
        globalMenuButton = chrome.globalMenuButton;
        dayText = chrome.dayText;
        goldText = chrome.goldText;
        statusText = chrome.statusText;

        ConfigurePrefabText(
            chrome.titleText, uiFont, 28, FontStyle.Bold,
            TextAnchor.MiddleLeft, ParchmentTextColor);
        ConfigurePrefabText(
            dayText, uiFont, 18, FontStyle.Bold,
            TextAnchor.MiddleCenter, Color.white);
        ConfigurePrefabText(
            goldText, uiFont, 18, FontStyle.Bold,
            TextAnchor.MiddleCenter, AccentColor);
        ConfigurePrefabText(
            statusText, uiBodyFont, 15, FontStyle.Bold,
            TextAnchor.MiddleLeft, ParchmentMutedColor);

        ConfigurePrefabButton(mapButton, "全体マップ", ShowGlobalMap);
        ConfigurePrefabButton(townMapButton, "町マップ", ShowTownMap);
        ConfigurePrefabButton(globalMenuButton, "メニュー", ShowGlobalMenu);
        chrome.merchantStatusButton.onClick.RemoveAllListeners();
        chrome.merchantStatusButton.onClick.AddListener(
            ShowMerchantStatusOverlay);
        ApplyButtonTransitions(chrome.merchantStatusButton);
    }

    private void ConfigurePrefabButton(
        Button button,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        ApplyButtonTransitions(button);
        Text labelText = button.GetComponentInChildren<Text>();
        ConfigurePrefabText(
            labelText, uiFont, 17, FontStyle.Bold,
            TextAnchor.MiddleCenter, ButtonTextColor);
        labelText.text = label;
    }

    private static void ConfigurePrefabText(
        Text text,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color) =>
        SimpleMercenaryHireUIFactory.ConfigurePrefabText(
            text, font, fontSize, fontStyle, alignment, color);

    private RectTransform CreatePanel(Transform parent) => uiFactory.CreatePanel(parent);

    private static RectTransform CreatePage(string pageName, RectTransform parent) =>
        SimpleMercenaryHireUIFactory.CreatePage(pageName, parent);

    private Text CreateText(
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color) =>
        uiFactory.CreateText(
            parent, content, fontSize, fontStyle, alignment, offsetMin, offsetMax, color);

    private static RectTransform CreateUIObject(string objectName, Transform parent) =>
        SimpleMercenaryHireUIFactory.CreateUIObject(objectName, parent);

    private static void EnsureEventSystem() => SimpleMercenaryHireUIFactory.EnsureEventSystem();
}
