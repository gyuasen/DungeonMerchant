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

    private static void ApplyParchmentPanel(Image target)
    {
        if (parchmentPanelSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("UI/ParchmentPanel");
            if (texture != null)
            {
                texture.wrapMode = TextureWrapMode.Clamp;
                Rect paperRect = new Rect(
                    72f,
                    40f,
                    texture.width - 144f,
                    texture.height - 84f);
                parchmentPanelSprite = Sprite.Create(
                    texture,
                    paperRect,
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(54f, 54f, 54f, 54f));
            }
        }

        if (parchmentPanelSprite == null)
        {
            target.color = PanelColor;
            return;
        }

        target.sprite = parchmentPanelSprite;
        target.type = Image.Type.Sliced;
        target.color = Color.white;
    }

    private RectTransform CreateRow(string rowName, RectTransform parent, float top)
    {
        GameObject rowPrefab =
            Resources.Load<GameObject>("UI/Templates/ListRow");
        RectTransform row = rowPrefab != null
            ? Instantiate(rowPrefab, parent, false)
                .GetComponent<RectTransform>()
            : CreateUIObject(rowName, parent);
        row.name = rowName;
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 96f);
        row.offsetMax = new Vector2(0f, top);

        Image rowImage = row.GetComponent<Image>() ??
            row.gameObject.AddComponent<Image>();
        rowImage.color = RowColor;
        if (row.GetComponent<Outline>() == null)
        {
            AddFantasyFrame(rowImage, 1f);
        }
        return row;
    }

    private Button CreateNavigationButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateButtonFromPrefab(
            "UI/Templates/NavigationButton",
            $"{label} Tab",
            parent);
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.sizeDelta = new Vector2(84f, 38f);
        buttonRect.anchoredPosition = position;

        Image image = buttonRect.GetComponent<Image>() ??
            buttonRect.gameObject.AddComponent<Image>();
        image.color = InactiveColor;
        if (buttonRect.GetComponent<Outline>() == null)
        {
            AddFantasyFrame(image, 1.5f);
        }

        Button button = buttonRect.GetComponent<Button>() ??
            buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        ApplyButtonTransitions(button);
        SetOrCreateButtonLabel(buttonRect, label, 15);
        return button;
    }

    private Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateButtonFromPrefab(
            "UI/Templates/ActionButton",
            "Action Button",
            parent);
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.sizeDelta = new Vector2(130f, 52f);
        buttonRect.anchoredPosition = new Vector2(-18f, 0f);

        Image image = buttonRect.GetComponent<Image>() ??
            buttonRect.gameObject.AddComponent<Image>();
        image.color = WoodButtonColor;
        if (buttonRect.GetComponent<Outline>() == null)
        {
            AddFantasyFrame(image, 1.5f);
        }

        Button button = buttonRect.GetComponent<Button>() ??
            buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        ApplyButtonTransitions(button);
        SetOrCreateButtonLabel(buttonRect, label, 17);
        return button;
    }

    private static RectTransform CreateButtonFromPrefab(
        string resourcePath,
        string objectName,
        RectTransform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        RectTransform result = prefab != null
            ? Instantiate(prefab, parent, false)
                .GetComponent<RectTransform>()
            : CreateUIObject(objectName, parent);
        result.name = objectName;
        return result;
    }

    private void SetOrCreateButtonLabel(
        RectTransform parent,
        string label,
        int fontSize)
    {
        Text existing = parent.GetComponentInChildren<Text>();
        if (existing == null)
        {
            CreateButtonLabel(parent, label, fontSize);
            return;
        }

        existing.text = label;
        existing.font = uiFont;
        existing.fontSize = fontSize;
        existing.fontStyle = FontStyle.Bold;
        existing.alignment = TextAnchor.MiddleCenter;
        existing.color = ButtonTextColor;
    }

    private static void AddFantasyFrame(Image image, float thickness)
    {
        Outline outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = FrameColor;
        outline.effectDistance = new Vector2(thickness, -thickness);
        outline.useGraphicAlpha = true;
    }

    private static void ApplyButtonTransitions(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.18f, 1.12f, 0.96f, 1f);
        colors.pressedColor = new Color(0.76f, 0.68f, 0.56f, 1f);
        colors.selectedColor = new Color(1.08f, 1.02f, 0.88f, 1f);
        colors.disabledColor = new Color(0.42f, 0.38f, 0.32f, 0.72f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private void CreateButtonLabel(RectTransform parent, string label, int fontSize)
    {
        Text buttonText = CreateText(parent, label, fontSize, FontStyle.Bold,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero,
            ButtonTextColor);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;
    }

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
                selectedDungeon.nearbyTownIndex == currentTownIndex;
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

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Simple Hire UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = canvasObject.AddComponent<Image>();
        background.color = BackgroundColor;
        background.raycastTarget = false;
        return canvas;
    }

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
        Color color)
    {
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
    }

    private RectTransform CreatePanel(Transform parent)
    {
        RectTransform panel = CreateUIObject("Guild Panel", parent);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820f, 620f);
        panel.anchoredPosition = Vector2.zero;

        Image panelImage = panel.gameObject.AddComponent<Image>();
        ApplyParchmentPanel(panelImage);
        return panel;
    }

    private static RectTransform CreatePage(string pageName, RectTransform parent)
    {
        RectTransform page = CreateUIObject(pageName, parent);
        page.anchorMin = new Vector2(0f, 0f);
        page.anchorMax = new Vector2(1f, 1f);
        page.offsetMin = new Vector2(28f, 64f);
        page.offsetMax = new Vector2(-28f, -126f);
        return page;
    }

    private Text CreateText(
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        RectTransform rect = CreateUIObject("Text", parent);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = fontStyle == FontStyle.Normal ? uiBodyFont : uiFont;
        text.fontSize = fontSize;
        bool isDirectParchmentText =
            color == ParchmentTextColor || color == ParchmentMutedColor;
        text.fontStyle = isDirectParchmentText
            ? FontStyle.Bold
            : fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
