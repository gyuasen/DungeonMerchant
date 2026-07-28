using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SimpleMercenaryHireUIPrefabBuilder
{
    private const string ResourcesDirectory =
        "Assets/Proiject/Resources/UI";
    private const string PrefabPath =
        ResourcesDirectory + "/SimpleMercenaryHireUIView.prefab";
    private const string TemplatesDirectory =
        ResourcesDirectory + "/Templates";
    private const string ListRowPrefabPath =
        TemplatesDirectory + "/ListRow.prefab";
    private const string ActionButtonPrefabPath =
        TemplatesDirectory + "/ActionButton.prefab";
    private const string NavigationButtonPrefabPath =
        TemplatesDirectory + "/NavigationButton.prefab";

    [InitializeOnLoadMethod]
    private static void ScheduleInitialBuild()
    {
        if (NeedsRebuild())
        {
            EditorApplication.delayCall += BuildIfMissing;
        }
    }

    private static void BuildIfMissing()
    {
        if (!NeedsRebuild())
        {
            return;
        }

        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += BuildIfMissing;
            return;
        }

        Build();
    }

    private static bool NeedsRebuild()
    {
        SimpleMercenaryHireUIView existing =
            AssetDatabase.LoadAssetAtPath<SimpleMercenaryHireUIView>(
                PrefabPath);
        return existing == null ||
               existing.LayoutVersion <
               SimpleMercenaryHireUIView.CurrentLayoutVersion ||
               !File.Exists(ListRowPrefabPath) ||
               !File.Exists(ActionButtonPrefabPath) ||
               !File.Exists(NavigationButtonPrefabPath);
    }

    [MenuItem("Dungeon Merchant/UI/Rebuild Main UI Prefab")]
    public static void Build()
    {
        Directory.CreateDirectory(ResourcesDirectory);
        Directory.CreateDirectory(TemplatesDirectory);
        AssetDatabase.Refresh();
        BuildSharedTemplates();

        GameObject root = new GameObject(
            "Simple Hire UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(Image),
            typeof(UIPageRouter),
            typeof(SimpleMercenaryHireUIView));

        try
        {
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.07f, 0.08f, 0.1f, 1f);
            background.raycastTarget = false;

            GameObject panelObject = new GameObject(
                "Guild Panel",
                typeof(RectTransform),
                typeof(Image));
            RectTransform panel =
                panelObject.GetComponent<RectTransform>();
            panel.SetParent(root.transform, false);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(820f, 620f);
            panel.anchoredPosition = Vector2.zero;
            panelObject.GetComponent<Image>().color =
                new Color(0.13f, 0.15f, 0.18f, 1f);

            SimpleMercenaryHireUIView.ChromeReferences chrome =
                new SimpleMercenaryHireUIView.ChromeReferences();
            chrome.titleText = CreateText(
                "Guild Title",
                panel,
                "傭兵商会",
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(28f, -62f),
                new Vector2(-28f, -18f));
            chrome.mapButton = CreateButton(
                "Global Map Button", panel, "全体マップ",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(172f, -18f), new Vector2(120f, 40f));
            chrome.townMapButton = CreateButton(
                "Town Map Button", panel, "町マップ",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(296f, -18f), new Vector2(100f, 40f));

            RectTransform dayDisplay = CreateBox(
                "Day Display", panel,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(404f, -16f), new Vector2(78f, 44f));
            chrome.dayText = CreateStretchText(
                "Day Text", dayDisplay, string.Empty);

            RectTransform merchantStatus = CreateBox(
                "Merchant Status Button", panel,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -16f), new Vector2(310f, 44f));
            chrome.merchantStatusButton =
                merchantStatus.gameObject.AddComponent<Button>();
            chrome.merchantStatusButton.targetGraphic =
                merchantStatus.GetComponent<Image>();
            chrome.goldText = CreateStretchText(
                "Merchant Status Text", merchantStatus, string.Empty);
            chrome.goldText.rectTransform.offsetMin =
                new Vector2(12f, 0f);
            chrome.goldText.rectTransform.offsetMax =
                new Vector2(-12f, 0f);

            chrome.globalMenuButton = CreateButton(
                "Global Menu Button", panel, "メニュー",
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -68f), new Vector2(110f, 40f));

            int pageCount =
                System.Enum.GetValues(
                    typeof(SimpleMercenaryHirePageSlot)).Length;
            RectTransform[] pages = new RectTransform[pageCount];
            foreach (SimpleMercenaryHirePageSlot slot in
                     System.Enum.GetValues(
                         typeof(SimpleMercenaryHirePageSlot)))
            {
                pages[(int)slot] = CreatePage(
                    GetPageName(slot),
                    panel);
            }
            pages[(int)SimpleMercenaryHirePageSlot.JobChange]
                .gameObject.AddComponent<JobChangePageUI>();
            CreateTrainingGroundLayout(
                pages[(int)SimpleMercenaryHirePageSlot.TrainingGround]);

            SimpleMercenaryHireUIView.HireCompanyReferences hireCompany =
                CreateHireCompanyLayout(pages);

            chrome.statusText = CreateText(
                "Status Text",
                panel,
                "雇用する傭兵を選択してください。",
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(28f, 22f),
                new Vector2(-28f, 54f));

            chrome.onboardingBanner = CreateBox(
                "Onboarding Guide Banner",
                panel,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-20f, 58f), new Vector2(410f, 42f));
            chrome.onboardingObjectiveText = CreateStretchText(
                "Onboarding Objective Text",
                chrome.onboardingBanner,
                string.Empty);
            chrome.onboardingObjectiveText.rectTransform.offsetMin =
                new Vector2(12f, 0f);
            chrome.onboardingObjectiveText.rectTransform.offsetMax =
                new Vector2(-126f, 0f);
            chrome.onboardingSkipButton = CreateButton(
                "Onboarding Skip Button",
                chrome.onboardingBanner,
                "案内を終了",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-8f, 0f), new Vector2(108f, 30f));

            RectTransform overlayRoot = CreateOverlayRoot(panel);
            int overlayCount =
                System.Enum.GetValues(
                    typeof(SimpleMercenaryHireOverlaySlot)).Length;
            RectTransform[] overlays =
                new RectTransform[overlayCount];
            foreach (SimpleMercenaryHireOverlaySlot slot in
                     System.Enum.GetValues(
                         typeof(SimpleMercenaryHireOverlaySlot)))
            {
                overlays[(int)slot] = CreateOverlaySlot(
                    GetOverlayName(slot),
                    overlayRoot);
            }

            root.GetComponent<SimpleMercenaryHireUIView>()
                .Initialize(
                    canvas,
                    panel,
                    pages,
                    chrome,
                    overlayRoot,
                    overlays,
                    hireCompany);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Main UI prefab rebuilt: {PrefabPath}");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }

    private static void BuildSharedTemplates()
    {
        GameObject row = new GameObject(
            "List Row",
            typeof(RectTransform),
            typeof(Image),
            typeof(Outline));
        try
        {
            row.GetComponent<Image>().color =
                new Color(0.27f, 0.16f, 0.09f, 0.94f);
            Outline outline = row.GetComponent<Outline>();
            outline.effectColor =
                new Color(0.72f, 0.52f, 0.27f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);
            PrefabUtility.SaveAsPrefabAsset(row, ListRowPrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(row);
        }

        BuildButtonTemplate(
            "Action Button",
            ActionButtonPrefabPath,
            new Vector2(130f, 52f));
        BuildButtonTemplate(
            "Navigation Button",
            NavigationButtonPrefabPath,
            new Vector2(84f, 38f));
    }

    private static void BuildButtonTemplate(
        string objectName,
        string prefabPath,
        Vector2 size)
    {
        RectTransform buttonRect = CreateButton(
            objectName,
            null,
            string.Empty,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            size).GetComponent<RectTransform>();
        try
        {
            PrefabUtility.SaveAsPrefabAsset(
                buttonRect.gameObject,
                prefabPath);
        }
        finally
        {
            Object.DestroyImmediate(buttonRect.gameObject);
        }
    }

    private static RectTransform CreatePage(
        string pageName,
        RectTransform parent)
    {
        GameObject pageObject = new GameObject(
            pageName,
            typeof(RectTransform));
        RectTransform page =
            pageObject.GetComponent<RectTransform>();
        pageObject.AddComponent<SimpleUIPage>();
        page.SetParent(parent, false);
        page.anchorMin = Vector2.zero;
        page.anchorMax = Vector2.one;
        page.offsetMin = new Vector2(28f, 64f);
        page.offsetMax = new Vector2(-28f, -126f);
        pageObject.SetActive(false);
        return page;
    }

    private static SimpleMercenaryHireUIView.HireCompanyReferences
        CreateHireCompanyLayout(RectTransform[] pages)
    {
        SimpleMercenaryHireUIView.HireCompanyReferences layout =
            new SimpleMercenaryHireUIView.HireCompanyReferences();

        RectTransform hirePage =
            pages[(int)SimpleMercenaryHirePageSlot.Hire];
        layout.hireTitle = CreateText(
            "Hire Title",
            hirePage,
            "契約可能な傭兵",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -30f),
            Vector2.zero);
        layout.contractButton = CreateButton(
            "Contract Button",
            hirePage,
            "契約: 日雇い",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -4f),
            new Vector2(160f, 38f));
        layout.hireScrollRect = CreateScrollArea(
            "Hire",
            hirePage,
            new Vector2(0f, -52f),
            out RectTransform hireList);
        layout.hireList = hireList;
        Object.DestroyImmediate(
            hirePage.GetComponent<SimpleUIPage>());
        layout.hirePageUI =
            hirePage.gameObject.AddComponent<HirePageUI>();
        layout.hirePageUI.Initialize(
            layout.hireTitle,
            layout.contractButton,
            layout.hireScrollRect,
            layout.hireList);

        RectTransform companyPage =
            pages[(int)SimpleMercenaryHirePageSlot.Company];
        layout.companyTitle = CreateText(
            "Company Title",
            companyPage,
            "雇用済み傭兵",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -30f),
            Vector2.zero);
        layout.questButton = CreateButton(
            "Quest Button",
            companyPage,
            "依頼",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -4f),
            new Vector2(110f, 38f));
        layout.companyScrollRect = CreateScrollArea(
            "Company",
            companyPage,
            new Vector2(0f, -44f),
            out RectTransform companyList);
        layout.companyList = companyList;
        Object.DestroyImmediate(
            companyPage.GetComponent<SimpleUIPage>());
        layout.companyPageUI =
            companyPage.gameObject.AddComponent<CompanyPageUI>();
        layout.companyPageUI.Initialize(
            layout.companyTitle,
            layout.questButton,
            layout.companyScrollRect,
            layout.companyList);
        return layout;
    }

    private static void CreateTrainingGroundLayout(RectTransform page)
    {
        Text title = CreateText(
            "Training Ground Title",
            page,
            "Training Ground",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(24f, -48f),
            new Vector2(-24f, -12f));
        Text description = CreateText(
            "Training Ground Description",
            page,
            string.Empty,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(24f, -80f),
            new Vector2(-24f, -52f));
        ScrollRect scrollRect = CreateScrollArea(
            "Training Ground",
            page,
            new Vector2(0f, -86f),
            out RectTransform listRoot);
        TrainingGroundPageUI pageUI =
            page.gameObject.AddComponent<TrainingGroundPageUI>();
        pageUI.Initialize(title, description, scrollRect, listRoot);
    }

    private static ScrollRect CreateScrollArea(
        string prefix,
        RectTransform parent,
        Vector2 viewportOffsetMax,
        out RectTransform content)
    {
        GameObject viewportObject = new GameObject(
            prefix + " Viewport",
            typeof(RectTransform),
            typeof(Image),
            typeof(Mask),
            typeof(ScrollRect));
        RectTransform viewport =
            viewportObject.GetComponent<RectTransform>();
        viewport.SetParent(parent, false);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = viewportOffsetMax;

        Image image = viewportObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.01f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject(
            prefix + " List",
            typeof(RectTransform));
        content = contentObject.GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect =
            viewportObject.GetComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType =
            ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        return scrollRect;
    }

    private static RectTransform CreateOverlayRoot(
        RectTransform parent)
    {
        GameObject rootObject = new GameObject(
            "Overlay Root",
            typeof(RectTransform));
        RectTransform root =
            rootObject.GetComponent<RectTransform>();
        root.SetParent(parent, false);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.pivot = new Vector2(0.5f, 0.5f);
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        return root;
    }

    private static RectTransform CreateOverlaySlot(
        string objectName,
        RectTransform parent)
    {
        GameObject slotObject = new GameObject(
            objectName,
            typeof(RectTransform));
        RectTransform slot =
            slotObject.GetComponent<RectTransform>();
        slot.SetParent(parent, false);
        slot.anchorMin = Vector2.zero;
        slot.anchorMax = Vector2.one;
        slot.pivot = new Vector2(0.5f, 0.5f);
        slot.offsetMin = Vector2.zero;
        slot.offsetMax = Vector2.zero;
        return slot;
    }

    private static RectTransform CreateBox(
        string objectName,
        RectTransform parent,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 position,
        Vector2 size)
    {
        GameObject boxObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Outline));
        RectTransform box = boxObject.GetComponent<RectTransform>();
        box.SetParent(parent, false);
        box.anchorMin = anchor;
        box.anchorMax = anchor;
        box.pivot = pivot;
        box.anchoredPosition = position;
        box.sizeDelta = size;

        Image image = boxObject.GetComponent<Image>();
        image.color = new Color(0.27f, 0.16f, 0.09f, 0.94f);
        Outline outline = boxObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.72f, 0.52f, 0.27f, 0.9f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        return box;
    }

    private static Button CreateButton(
        string objectName,
        RectTransform parent,
        string label,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 position,
        Vector2 size)
    {
        RectTransform buttonRect = CreateBox(
            objectName, parent, anchor, pivot, position, size);
        buttonRect.GetComponent<Image>().color =
            new Color(0.35f, 0.22f, 0.13f, 1f);
        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonRect.GetComponent<Image>();
        CreateStretchText("Label", buttonRect, label);
        return button;
    }

    private static Text CreateStretchText(
        string objectName,
        RectTransform parent,
        string content)
    {
        return CreateText(
            objectName,
            parent,
            content,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero);
    }

    private static Text CreateText(
        string objectName,
        RectTransform parent,
        string content,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.raycastTarget = false;
        return text;
    }

    private static string GetPageName(
        SimpleMercenaryHirePageSlot slot)
    {
        switch (slot)
        {
            case SimpleMercenaryHirePageSlot.Hire:
                return "Hire Page";
            case SimpleMercenaryHirePageSlot.GlobalMap:
                return "Global Map Page";
            case SimpleMercenaryHirePageSlot.WorldMap:
                return "World Map Page";
            case SimpleMercenaryHirePageSlot.TownMap:
                return "Town Map Page";
            case SimpleMercenaryHirePageSlot.Company:
                return "Company Page";
            case SimpleMercenaryHirePageSlot.Party:
                return "Party Page";
            case SimpleMercenaryHirePageSlot.Heal:
                return "Heal Page";
            case SimpleMercenaryHirePageSlot.Battle:
                return "Battle Page";
            case SimpleMercenaryHirePageSlot.RoadBattle:
                return "Road Battle Page";
            case SimpleMercenaryHirePageSlot.Dungeon:
                return "Dungeon Page";
            case SimpleMercenaryHirePageSlot.Market:
                return "Market Page";
            case SimpleMercenaryHirePageSlot.Blacksmith:
                return "Blacksmith Page";
            case SimpleMercenaryHirePageSlot.Inventory:
                return "Inventory Page";
            case SimpleMercenaryHirePageSlot.JobChange:
                return "Job Change Page";
            case SimpleMercenaryHirePageSlot.TrainingGround:
                return "Training Ground Page";
            default:
                return slot + " Page";
        }
    }

    private static string GetOverlayName(
        SimpleMercenaryHireOverlaySlot slot)
    {
        switch (slot)
        {
            case SimpleMercenaryHireOverlaySlot.CharacterDetail:
                return "Character Detail Overlay";
            case SimpleMercenaryHireOverlaySlot.EquipmentDetail:
                return "Equipment Detail Overlay";
            case SimpleMercenaryHireOverlaySlot.EquipmentCollection:
                return "Equipment Collection Overlay";
            case SimpleMercenaryHireOverlaySlot.MerchantStatus:
                return "Merchant Status Overlay";
            case SimpleMercenaryHireOverlaySlot.Quest:
                return "Quest Overlay";
            case SimpleMercenaryHireOverlaySlot.TravelConfirmation:
                return "Travel Confirmation Overlay";
            case SimpleMercenaryHireOverlaySlot.GlobalMenu:
                return "Global Menu Overlay";
            case SimpleMercenaryHireOverlaySlot.DailyResult:
                return "Daily Result Overlay";
            default:
                return slot + " Overlay";
        }
    }
}
