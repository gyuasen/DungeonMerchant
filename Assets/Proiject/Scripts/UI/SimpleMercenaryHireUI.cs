using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleMercenaryHireUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MercenaryGenerator mercenaryGenerator;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MarketPriceManager marketPriceManager;
    [SerializeField] private MarketStockManager marketStockManager;
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private HealingManager healingManager;

    [Header("Hire Candidates")]
    [SerializeField] private List<MercenaryDataSO> candidates = new List<MercenaryDataSO>();

    private readonly List<Button> hireButtons = new List<Button>();
    private readonly List<MercenaryDataSO> displayedCandidates = new List<MercenaryDataSO>();
    private readonly List<Button> generatedHireButtons = new List<Button>();
    private readonly List<MercenaryInstance> displayedGeneratedCandidates =
        new List<MercenaryInstance>();
    private readonly List<Button> marketBuyButtons = new List<Button>();
    private readonly List<MarketStockEntry> displayedMarketEntries =
        new List<MarketStockEntry>();
    private readonly HashSet<MercenaryDataSO> hiredCandidates = new HashSet<MercenaryDataSO>();
    private readonly List<string> battleLogLines = new List<string>();

    private RectTransform hirePage;
    private RectTransform hireList;
    private RectTransform companyPage;
    private RectTransform partyPage;
    private RectTransform healPage;
    private RectTransform battlePage;
    private RectTransform dungeonPage;
    private RectTransform marketPage;
    private RectTransform inventoryPage;
    private RectTransform companyScrollContent;
    private RectTransform companyList;
    private RectTransform partyList;
    private RectTransform healList;
    private RectTransform inventoryList;
    private RectTransform marketList;
    private Button hireTabButton;
    private Button companyTabButton;
    private Button partyTabButton;
    private Button healTabButton;
    private Button battleTabButton;
    private Button dungeonTabButton;
    private Button marketTabButton;
    private Button inventoryTabButton;
    private Button startBattleButton;
    private Button startDungeonButton;
    private Button nextDayButton;
    private Text goldText;
    private Text statusText;
    private Text battleLogText;
    private Text dungeonStatusText;
    private Text marketInfoText;
    private Font uiFont;
    private RectTransform battleLogContent;
    private ScrollRect battleLogScrollRect;
    private Coroutine battleLogScrollCoroutine;

    private static readonly Color BackgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
    private static readonly Color PanelColor = new Color(0.13f, 0.15f, 0.18f, 1f);
    private static readonly Color RowColor = new Color(0.19f, 0.21f, 0.24f, 1f);
    private static readonly Color AccentColor = new Color(0.2f, 0.65f, 0.48f, 1f);
    private static readonly Color InactiveColor = new Color(0.25f, 0.28f, 0.32f, 1f);
    private static readonly Color MutedTextColor = new Color(0.7f, 0.74f, 0.78f, 1f);

    private void Start()
    {
        ResolveReferences();

        if (!HasRequiredReferences())
        {
            return;
        }

        uiFont = LoadUIFont();
        PopulateUniqueCandidatesIfNeeded();
        CacheAlreadyHiredCandidates();
        EnsureEventSystem();
        BuildUI();
        merchantData.GoldChanged += HandleGoldChanged;
        hireManager.MercenaryHired += HandleMercenaryHired;
        partyManager.PartyChanged += HandlePartyChanged;
        mercenaryGenerator.CandidatesChanged += HandleCandidatesChanged;
        battleManager.BattleMessageTyped += HandleBattleMessage;
        battleManager.BattleCompleted += HandleBattleCompleted;
        dungeonRunManager.DungeonMessage += HandleDungeonMessage;
        dungeonRunManager.DungeonStateChanged += HandleDungeonStateChanged;
        dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
        healingManager.HealingChanged += HandleHealingChanged;
        merchantInventory.InventoryChanged += HandleInventoryChanged;
        dayManager.DayChanged += HandleDayChanged;
        marketPriceManager.PricesChanged += HandlePricesChanged;
        marketStockManager.StockChanged += HandleMarketStockChanged;
        ShowHirePage();
        RefreshUI();
    }

    private void ResolveReferences()
    {
        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>();
        }

        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>();
        }

        if (mercenaryGenerator == null)
        {
            mercenaryGenerator = GetComponent<MercenaryGenerator>();
        }

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = GetComponent<MerchantInventory>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = FindObjectOfType<MerchantInventory>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = gameObject.AddComponent<MerchantInventory>();
        }

        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>();
        }

        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }

        if (dayManager == null)
        {
            dayManager = gameObject.AddComponent<DayManager>();
        }

        if (marketPriceManager == null)
        {
            marketPriceManager = GetComponent<MarketPriceManager>();
        }

        if (marketPriceManager == null)
        {
            marketPriceManager = FindObjectOfType<MarketPriceManager>();
        }

        if (marketPriceManager == null)
        {
            marketPriceManager = gameObject.AddComponent<MarketPriceManager>();
        }

        if (marketStockManager == null)
        {
            marketStockManager = GetComponent<MarketStockManager>();
        }

        if (marketStockManager == null)
        {
            marketStockManager = FindObjectOfType<MarketStockManager>();
        }

        if (marketStockManager == null)
        {
            marketStockManager = gameObject.AddComponent<MarketStockManager>();
        }

        if (dungeonRunManager == null)
        {
            dungeonRunManager = GetComponent<DungeonRunManager>();
        }

        if (dungeonRunManager == null)
        {
            dungeonRunManager = FindObjectOfType<DungeonRunManager>();
        }

        if (dungeonRunManager == null)
        {
            dungeonRunManager = gameObject.AddComponent<DungeonRunManager>();
        }

        if (healingManager == null)
        {
            healingManager = GetComponent<HealingManager>();
        }

        if (healingManager == null)
        {
            healingManager = FindObjectOfType<HealingManager>();
        }

        if (healingManager == null)
        {
            healingManager = gameObject.AddComponent<HealingManager>();
        }

        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>();
        }

        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }
    }

    private bool HasRequiredReferences()
    {
        bool hasAllReferences = true;

        if (merchantData == null)
        {
            Debug.LogError("Simple hire UI is missing MerchantData.", this);
            hasAllReferences = false;
        }

        if (hireManager == null)
        {
            Debug.LogError("Simple hire UI is missing MercenaryHireManager.", this);
            hasAllReferences = false;
        }

        if (partyManager == null)
        {
            Debug.LogError("Simple hire UI is missing MercenaryPartyManager.", this);
            hasAllReferences = false;
        }

        if (mercenaryGenerator == null)
        {
            Debug.LogError("Simple hire UI is missing MercenaryGenerator.", this);
            hasAllReferences = false;
        }

        if (battleManager == null)
        {
            Debug.LogError("Simple hire UI is missing BattleManager.", this);
            hasAllReferences = false;
        }

        if (merchantInventory == null)
        {
            Debug.LogError("Simple hire UI is missing MerchantInventory.", this);
            hasAllReferences = false;
        }

        if (dayManager == null)
        {
            Debug.LogError("Simple hire UI is missing DayManager.", this);
            hasAllReferences = false;
        }

        if (marketPriceManager == null)
        {
            Debug.LogError("Simple hire UI is missing MarketPriceManager.", this);
            hasAllReferences = false;
        }

        if (marketStockManager == null)
        {
            Debug.LogError("Simple hire UI is missing MarketStockManager.", this);
            hasAllReferences = false;
        }

        if (dungeonRunManager == null)
        {
            Debug.LogError("Simple hire UI is missing DungeonRunManager.", this);
            hasAllReferences = false;
        }

        if (healingManager == null)
        {
            Debug.LogError("Simple hire UI is missing HealingManager.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private Font LoadUIFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (font == null)
        {
            Debug.LogError("Simple hire UI could not load a built-in font.", this);
        }

        return font;
    }

    private void PopulateUniqueCandidatesIfNeeded()
    {
        RemoveMissingCandidates();
        if (candidates.Count > 0)
        {
            return;
        }

        foreach (MercenaryDataSO candidate in Resources.LoadAll<MercenaryDataSO>(string.Empty))
        {
            AddUniqueCandidate(candidate);
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:MercenaryDataSO",
            new[] { "Assets/Proiject/ScriptableObjects/Mercenaries" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MercenaryDataSO candidate =
                AssetDatabase.LoadAssetAtPath<MercenaryDataSO>(path);
            AddUniqueCandidate(candidate);
        }
#endif

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No unique mercenary data assets were found.", this);
        }
    }

    private void RemoveMissingCandidates()
    {
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (candidates[i] == null)
            {
                candidates.RemoveAt(i);
            }
        }
    }

    private void AddUniqueCandidate(MercenaryDataSO candidate)
    {
        if (candidate == null || candidates.Contains(candidate))
        {
            return;
        }

        candidates.Add(candidate);
    }

    private void OnDestroy()
    {
        if (merchantData != null)
        {
            merchantData.GoldChanged -= HandleGoldChanged;
        }

        if (hireManager != null)
        {
            hireManager.MercenaryHired -= HandleMercenaryHired;
        }

        if (partyManager != null)
        {
            partyManager.PartyChanged -= HandlePartyChanged;
        }

        if (mercenaryGenerator != null)
        {
            mercenaryGenerator.CandidatesChanged -= HandleCandidatesChanged;
        }

        if (battleManager != null)
        {
            battleManager.BattleMessageTyped -= HandleBattleMessage;
            battleManager.BattleCompleted -= HandleBattleCompleted;
        }

        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonMessage -= HandleDungeonMessage;
            dungeonRunManager.DungeonStateChanged -= HandleDungeonStateChanged;
            dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
        }

        if (healingManager != null)
        {
            healingManager.HealingChanged -= HandleHealingChanged;
        }

        if (merchantInventory != null)
        {
            merchantInventory.InventoryChanged -= HandleInventoryChanged;
        }

        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
        }

        if (marketPriceManager != null)
        {
            marketPriceManager.PricesChanged -= HandlePricesChanged;
        }

        if (marketStockManager != null)
        {
            marketStockManager.StockChanged -= HandleMarketStockChanged;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = CreateCanvas();
        RectTransform panel = CreatePanel(canvas.transform);

        CreateText(panel, "MERCENARY GUILD", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), Color.white);

        goldText = CreateText(panel, string.Empty, 20, FontStyle.Bold, TextAnchor.MiddleRight,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), AccentColor);

        hireTabButton = CreateNavigationButton(panel, "HIRE", new Vector2(28f, -78f), ShowHirePage);
        companyTabButton = CreateNavigationButton(
            panel,
            "COMPANY",
            new Vector2(124f, -78f),
            ShowCompanyPage);
        partyTabButton = CreateNavigationButton(
            panel,
            "PARTY",
            new Vector2(220f, -78f),
            ShowPartyPage);
        healTabButton = CreateNavigationButton(
            panel,
            "HEAL",
            new Vector2(316f, -78f),
            ShowHealPage);
        battleTabButton = CreateNavigationButton(
            panel,
            "BATTLE",
            new Vector2(412f, -78f),
            ShowBattlePage);
        dungeonTabButton = CreateNavigationButton(
            panel,
            "DUNGEON",
            new Vector2(508f, -78f),
            ShowDungeonPage);
        marketTabButton = CreateNavigationButton(
            panel,
            "MARKET",
            new Vector2(604f, -78f),
            ShowMarketPage);
        inventoryTabButton = CreateNavigationButton(
            panel,
            "INVENTORY",
            new Vector2(700f, -78f),
            ShowInventoryPage);

        hirePage = CreatePage("Hire Page", panel);
        companyPage = CreatePage("Company Page", panel);
        partyPage = CreatePage("Party Page", panel);
        healPage = CreatePage("Heal Page", panel);
        battlePage = CreatePage("Battle Page", panel);
        dungeonPage = CreatePage("Dungeon Page", panel);
        marketPage = CreatePage("Market Page", panel);
        inventoryPage = CreatePage("Inventory Page", panel);

        BuildHirePage();
        BuildCompanyPage();
        BuildPartyPage();
        BuildHealPage();
        BuildBattlePage();
        BuildDungeonPage();
        BuildMarketPage();
        BuildInventoryPage();

        statusText = CreateText(panel, "Select a mercenary to hire.", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(28f, 22f), new Vector2(-28f, 54f), MutedTextColor);
        statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        statusText.rectTransform.pivot = new Vector2(0.5f, 0f);
    }

    private void BuildHirePage()
    {
        CreateText(hirePage, "Available contracts", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        RectTransform viewport = CreateUIObject("Hire Viewport", hirePage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -44f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        hireList = CreateUIObject("Hire List", viewport);
        hireList.anchorMin = new Vector2(0f, 1f);
        hireList.anchorMax = new Vector2(1f, 1f);
        hireList.pivot = new Vector2(0.5f, 1f);
        hireList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = hireList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        RebuildHireList();
    }

    private void RebuildHireList()
    {
        ClearChildren(hireList);
        hireButtons.Clear();
        displayedCandidates.Clear();
        generatedHireButtons.Clear();
        displayedGeneratedCandidates.Clear();

        float rowTop = 0f;
        foreach (MercenaryDataSO candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            if (hiredCandidates.Contains(candidate))
            {
                continue;
            }

            CreateCandidateRow(hireList, candidate, rowTop);
            rowTop -= 112f;
        }

        foreach (MercenaryInstance candidate in mercenaryGenerator.Candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            CreateGeneratedCandidateRow(hireList, candidate, rowTop);
            rowTop -= 112f;
        }

        hireList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void BuildCompanyPage()
    {
        CreateText(companyPage, "Hired mercenaries", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        RectTransform viewport = CreateUIObject("Company Viewport", companyPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -44f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        companyScrollContent = CreateUIObject("Company Scroll Content", viewport);
        companyScrollContent.anchorMin = new Vector2(0f, 1f);
        companyScrollContent.anchorMax = new Vector2(1f, 1f);
        companyScrollContent.pivot = new Vector2(0.5f, 1f);
        companyScrollContent.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = companyScrollContent;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        companyList = companyScrollContent;
    }

    private void BuildPartyPage()
    {
        CreateText(partyPage, "Exploration party", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        partyList = CreateUIObject("Party List", partyPage);
        partyList.anchorMin = new Vector2(0f, 0f);
        partyList.anchorMax = new Vector2(1f, 1f);
        partyList.offsetMin = Vector2.zero;
        partyList.offsetMax = new Vector2(0f, -44f);
    }

    private void BuildHealPage()
    {
        CreateText(healPage, "Treatment ward", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        CreateText(
            healPage,
            $"Full treatment costs {healingManager.HealCostPerHP} G per missing HP. " +
            $"Rest restores {healingManager.NaturalHealPerDay} HP each day.",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -72f),
            new Vector2(0f, -42f),
            MutedTextColor);

        RectTransform viewport = CreateUIObject("Heal Viewport", healPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -86f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        healList = CreateUIObject("Heal List", viewport);
        healList.anchorMin = new Vector2(0f, 1f);
        healList.anchorMax = new Vector2(1f, 1f);
        healList.pivot = new Vector2(0.5f, 1f);
        healList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = healList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
    }

    private void BuildBattlePage()
    {
        string enemyDescription = battleManager.GetEncounterDescription();

        CreateText(battlePage, "Battle preparation", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        CreateText(battlePage, enemyDescription, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f), Color.white);

        startBattleButton = CreateActionButton(
            battlePage,
            "START",
            StartPartyBattle);
        RectTransform startRect = startBattleButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);

        RectTransform logPanel = CreateUIObject("Battle Log", battlePage);
        logPanel.anchorMin = new Vector2(0f, 0f);
        logPanel.anchorMax = new Vector2(1f, 1f);
        logPanel.offsetMin = Vector2.zero;
        logPanel.offsetMax = new Vector2(0f, -104f);

        Image logBackground = logPanel.gameObject.AddComponent<Image>();
        logBackground.color = RowColor;

        RectTransform viewport = CreateUIObject("Battle Log Viewport", logPanel);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(16f, 16f);
        viewport.offsetMax = new Vector2(-16f, -16f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        battleLogContent = CreateUIObject("Battle Log Content", viewport);
        battleLogContent.anchorMin = new Vector2(0f, 1f);
        battleLogContent.anchorMax = new Vector2(1f, 1f);
        battleLogContent.pivot = new Vector2(0.5f, 1f);
        battleLogContent.anchoredPosition = Vector2.zero;
        battleLogContent.sizeDelta = new Vector2(0f, 430f);

        battleLogScrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        battleLogScrollRect.content = battleLogContent;
        battleLogScrollRect.viewport = viewport;
        battleLogScrollRect.horizontal = false;
        battleLogScrollRect.vertical = true;
        battleLogScrollRect.movementType = ScrollRect.MovementType.Clamped;
        battleLogScrollRect.scrollSensitivity = 28f;

        battleLogText = CreateText(battleLogContent, "Ready for battle.", 14, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(16f, 16f), new Vector2(-16f, -16f),
            MutedTextColor);
        battleLogText.supportRichText = true;
        battleLogText.rectTransform.anchorMin = Vector2.zero;
        battleLogText.rectTransform.anchorMax = Vector2.one;
        battleLogText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        battleLogText.rectTransform.offsetMin = new Vector2(0f, 8f);
        battleLogText.rectTransform.offsetMax = new Vector2(0f, -8f);
    }

    private void BuildDungeonPage()
    {
        CreateText(dungeonPage, "Dungeon run", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        dungeonStatusText = CreateText(
            dungeonPage,
            "Prepare a party, then enter the dungeon.",
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -92f),
            new Vector2(-170f, -42f),
            Color.white);

        startDungeonButton = CreateActionButton(
            dungeonPage,
            "ENTER",
            StartDungeonRun);
        RectTransform startRect = startDungeonButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);

        CreateText(
            dungeonPage,
            "A dungeon run starts several battles in a row. Mercenary HP carries over between encounters.",
            15,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, -170f),
            new Vector2(0f, -112f),
            MutedTextColor);
    }

    private void BuildInventoryPage()
    {
        CreateText(inventoryPage, "Merchant inventory", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        marketInfoText = CreateText(inventoryPage, string.Empty, 16, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(-160f, -38f),
            Color.white);

        nextDayButton = CreateActionButton(inventoryPage, "NEXT DAY", AdvanceDay);
        RectTransform nextDayRect = nextDayButton.GetComponent<RectTransform>();
        nextDayRect.anchorMin = new Vector2(1f, 1f);
        nextDayRect.anchorMax = new Vector2(1f, 1f);
        nextDayRect.pivot = new Vector2(1f, 1f);
        nextDayRect.anchoredPosition = new Vector2(0f, -34f);

        inventoryList = CreateUIObject("Inventory List", inventoryPage);
        inventoryList.anchorMin = new Vector2(0f, 0f);
        inventoryList.anchorMax = new Vector2(1f, 1f);
        inventoryList.offsetMin = Vector2.zero;
        inventoryList.offsetMax = new Vector2(0f, -86f);
    }

    private void BuildMarketPage()
    {
        CreateText(marketPage, "Daily market stock", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        marketList = CreateUIObject("Market List", marketPage);
        marketList.anchorMin = new Vector2(0f, 0f);
        marketList.anchorMax = new Vector2(1f, 1f);
        marketList.offsetMin = Vector2.zero;
        marketList.offsetMax = new Vector2(0f, -44f);
    }

    private void RebuildCompanyList()
    {
        ClearChildren(companyList);

        if (hireManager.HiredMercenaries.Count == 0)
        {
            CreateText(companyList, "No mercenaries hired yet.", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        float rowTop = 0f;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            CreateCompanyRow(companyList, mercenary, rowTop);
            rowTop -= 112f;
        }

        companyList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void RebuildPartyList()
    {
        ClearChildren(partyList);

        float rowTop = 0f;
        for (int slotIndex = 0; slotIndex < partyManager.MaxPartySize; slotIndex++)
        {
            if (slotIndex < partyManager.Members.Count)
            {
                CreatePartyRow(partyList, partyManager.Members[slotIndex], slotIndex, rowTop);
            }
            else
            {
                CreateEmptyPartyRow(partyList, slotIndex, rowTop);
            }

            rowTop -= 112f;
        }
    }

    private void RebuildHealList()
    {
        ClearChildren(healList);

        if (hireManager.HiredMercenaries.Count == 0)
        {
            CreateText(healList, "No mercenaries hired yet.", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        float rowTop = 0f;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            CreateHealRow(healList, mercenary, rowTop);
            rowTop -= 112f;
        }

        healList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void RebuildInventoryList()
    {
        ClearChildren(inventoryList);

        if (merchantInventory.Items.Count == 0)
        {
            CreateText(inventoryList, "No items in stock.", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        float rowTop = 0f;
        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            if (stack == null || stack.Item == null || stack.Amount <= 0)
            {
                continue;
            }

            CreateInventoryRow(inventoryList, stack, rowTop);
            rowTop -= 112f;
        }
    }

    private void RebuildMarketList()
    {
        ClearChildren(marketList);
        marketBuyButtons.Clear();
        displayedMarketEntries.Clear();

        if (marketStockManager.Stock.Count == 0)
        {
            CreateText(marketList, "No goods are available today.", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        float rowTop = 0f;
        foreach (MarketStockEntry entry in marketStockManager.Stock)
        {
            if (entry == null || entry.Item == null || entry.Quantity <= 0)
            {
                continue;
            }

            CreateMarketRow(marketList, entry, rowTop);
            rowTop -= 112f;
        }
    }

    private void CreateCandidateRow(RectTransform parent, MercenaryDataSO candidate, float top)
    {
        RectTransform row = CreateRow(candidate.mercenaryName, parent, top);

        CreateText(row, candidate.mercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"{candidate.mercenaryClass}  |  {candidate.contractType}  |  " +
            $"HP {candidate.maxHP}  ATK {candidate.attack}  DEF {candidate.defense}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button hireButton = CreateActionButton(
            row,
            $"{candidate.hireCost} G",
            () => Hire(candidate));

        hireButtons.Add(hireButton);
        displayedCandidates.Add(candidate);
    }

    private void CreateCompanyRow(RectTransform parent, MercenaryInstance mercenary, float top)
    {
        RectTransform row = CreateRow(mercenary.MercenaryName, parent, top);
        CreateText(row, mercenary.MercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"LV {mercenary.Level}  |  {mercenary.MercenaryClass}  |  {mercenary.ContractType}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        string shortId = mercenary.InstanceId.Substring(0, 8).ToUpperInvariant();
        CreateText(row, $"ID {shortId}", 13, FontStyle.Normal, TextAnchor.MiddleRight,
            new Vector2(18f, -64f), new Vector2(-170f, -30f), MutedTextColor);

        string actionLabel = partyManager.Contains(mercenary) ? "REMOVE" : "ADD";
        CreateActionButton(row, actionLabel, () => TogglePartyMember(mercenary));
    }

    private void CreateGeneratedCandidateRow(
        RectTransform parent,
        MercenaryInstance candidate,
        float top)
    {
        RectTransform row = CreateRow(candidate.MercenaryName, parent, top);

        CreateText(row, candidate.MercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"{candidate.MercenaryClass}  |  {candidate.ContractType}  |  " +
            $"HP {candidate.MaxHP}  ATK {candidate.Attack}  DEF {candidate.Defense}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button hireButton = CreateActionButton(
            row,
            $"{candidate.HireCost} G",
            () => HireGeneratedCandidate(candidate));

        generatedHireButtons.Add(hireButton);
        displayedGeneratedCandidates.Add(candidate);
    }

    private void CreatePartyRow(
        RectTransform parent,
        MercenaryInstance mercenary,
        int slotIndex,
        float top)
    {
        RectTransform row = CreateRow($"Party Slot {slotIndex + 1}", parent, top);
        CreateText(row, $"{slotIndex + 1}. {mercenary.MercenaryName}", 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"LV {mercenary.Level}  |  {mercenary.MercenaryClass}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        CreateActionButton(row, "REMOVE", () => RemovePartyMember(mercenary));
    }

    private void CreateEmptyPartyRow(RectTransform parent, int slotIndex, float top)
    {
        RectTransform row = CreateRow($"Empty Party Slot {slotIndex + 1}", parent, top);

        CreateText(row, $"{slotIndex + 1}. EMPTY SLOT", 20, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -58f), new Vector2(-18f, -28f),
            MutedTextColor);
    }

    private void CreateHealRow(
        RectTransform parent,
        MercenaryInstance mercenary,
        float top)
    {
        RectTransform row = CreateRow($"{mercenary.MercenaryName} Treatment", parent, top);

        CreateText(row, mercenary.MercenaryName, 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f),
            Color.white);

        int missingHP = healingManager.GetMissingHP(mercenary);
        int healCost = healingManager.GetFullHealCost(mercenary);
        string details =
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
            $"Missing {missingHP}  |  Full treatment {healCost} G";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button healButton = CreateActionButton(row, "HEAL", () => HealMercenary(mercenary));
        healButton.interactable = healingManager.CanHeal(mercenary);
    }

    private void CreateInventoryRow(
        RectTransform parent,
        InventoryItemStack stack,
        float top)
    {
        ItemDataSO item = stack.Item;
        RectTransform row = CreateRow(item.itemName, parent, top);

        CreateText(row, $"{item.itemName} x{stack.Amount}", 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f),
            Color.white);

        int sellPrice = merchantInventory.GetSellPrice(item);
        int percent = Mathf.RoundToInt(marketPriceManager.GetSellMultiplier(item) * 100f);
        string details =
            $"{item.rarity}  |  {item.itemType}  |  Base {item.basePrice} G  |  " +
            $"Today {sellPrice} G ({percent}%)";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        CreateActionButton(row, "SELL", () => SellItem(item));
    }

    private void CreateMarketRow(
        RectTransform parent,
        MarketStockEntry entry,
        float top)
    {
        ItemDataSO item = entry.Item;
        RectTransform row = CreateRow(item.itemName, parent, top);

        CreateText(row, $"{item.itemName} x{entry.Quantity}", 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f),
            Color.white);

        int sellPrice = merchantInventory.GetSellPrice(item);
        string details =
            $"{item.rarity}  |  {item.itemType}  |  Buy {entry.BuyPrice} G  |  " +
            $"Today sell {sellPrice} G";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button buyButton = CreateActionButton(row, "BUY", () => BuyMarketItem(entry));
        marketBuyButtons.Add(buyButton);
        displayedMarketEntries.Add(entry);
    }

    private RectTransform CreateRow(string rowName, RectTransform parent, float top)
    {
        RectTransform row = CreateUIObject(rowName, parent);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 96f);
        row.offsetMax = new Vector2(0f, top);

        Image rowImage = row.gameObject.AddComponent<Image>();
        rowImage.color = RowColor;
        return row;
    }

    private Button CreateNavigationButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateUIObject($"{label} Tab", parent);
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.sizeDelta = new Vector2(104f, 38f);
        buttonRect.anchoredPosition = position;

        Image image = buttonRect.gameObject.AddComponent<Image>();
        image.color = InactiveColor;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateButtonLabel(buttonRect, label, 15);
        return button;
    }

    private Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateUIObject("Action Button", parent);
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.sizeDelta = new Vector2(130f, 52f);
        buttonRect.anchoredPosition = new Vector2(-18f, 0f);

        Image image = buttonRect.gameObject.AddComponent<Image>();
        image.color = AccentColor;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateButtonLabel(buttonRect, label, 17);
        return button;
    }

    private void CreateButtonLabel(RectTransform parent, string label, int fontSize)
    {
        Text buttonText = CreateText(parent, label, fontSize, FontStyle.Bold,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Color.white);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;
    }

    private void Hire(MercenaryDataSO candidate)
    {
        if (!hireManager.TryHireMercenary(candidate))
        {
            statusText.text = $"Could not hire {candidate.mercenaryName}.";
            RefreshUI();
            return;
        }

        hiredCandidates.Add(candidate);
        statusText.text = $"{candidate.mercenaryName} joined your company.";
        RebuildHireList();
        RefreshUI();
    }

    private void HireGeneratedCandidate(MercenaryInstance candidate)
    {
        if (!hireManager.TryHireMercenary(candidate))
        {
            statusText.text = $"Could not hire {candidate.MercenaryName}.";
            RefreshUI();
            return;
        }

        statusText.text = $"{candidate.MercenaryName} joined your company.";
        mercenaryGenerator.RemoveCandidate(candidate);
        RefreshUI();
    }

    private void HandleMercenaryHired(MercenaryInstance mercenary)
    {
        RebuildCompanyList();
    }

    private void HandlePartyChanged()
    {
        RebuildCompanyList();
        RebuildPartyList();
        if (startBattleButton != null && !battleManager.IsBattling)
        {
            startBattleButton.interactable = partyManager.Members.Count > 0;
        }
        statusText.text = $"Party members: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void HandleCandidatesChanged()
    {
        RebuildHireList();
        RefreshUI();
    }

    private void HandleInventoryChanged()
    {
        RebuildInventoryList();
        RefreshUI();
    }

    private void HandleMarketStockChanged()
    {
        RebuildMarketList();
        RefreshUI();
    }

    private void HandleDayChanged(int currentDay)
    {
        RebuildMarketList();
        RebuildInventoryList();
        RebuildHealList();
        RefreshUI();
        statusText.text = $"Day {currentDay} started. Market prices changed.";
    }

    private void HandlePricesChanged()
    {
        RebuildInventoryList();
        RefreshUI();
    }

    private void HandleGoldChanged(int currentGold)
    {
        RebuildHealList();
        RefreshUI();
    }

    private void StartPartyBattle()
    {
        battleLogLines.Clear();
        battleLogText.text = string.Empty;
        battleLogContent.sizeDelta = new Vector2(0f, 430f);
        startBattleButton.interactable = false;

        if (!battleManager.StartBattle(partyManager.Members))
        {
            startBattleButton.interactable = true;
        }
    }

    private void StartDungeonRun()
    {
        ShowBattlePage();
        battleLogLines.Clear();
        battleLogText.text = string.Empty;
        battleLogContent.sizeDelta = new Vector2(0f, 430f);

        if (!dungeonRunManager.StartRun())
        {
            ShowDungeonPage();
        }

        RefreshUI();
    }

    private void HandleBattleMessage(string message, BattleLogType logType)
    {
        string coloredMessage = ColorizeBattleMessage(message, logType);
        battleLogLines.Add(coloredMessage);
        battleLogText.text = string.Join("\n", battleLogLines);

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
        float height = Mathf.Max(430f, battleLogText.preferredHeight + 32f);
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

    private static string ColorizeBattleMessage(string message, BattleLogType logType)
    {
        string escapedMessage = EscapeRichText(message);
        switch (logType)
        {
            case BattleLogType.Player:
                return $"<color=#5CA8FF>{escapedMessage}</color>";
            case BattleLogType.Enemy:
                return $"<color=#FF6B6B>{escapedMessage}</color>";
            case BattleLogType.Reward:
                return $"<color=#6FE3A0>{escapedMessage}</color>";
            default:
                return escapedMessage;
        }
    }

    private static string EscapeRichText(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private void HandleBattleCompleted(bool victory)
    {
        startBattleButton.interactable = partyManager.Members.Count > 0;
        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();
        RefreshUI();
        statusText.text = victory ? "Battle won." : "Battle lost.";
    }

    private void HandleHealingChanged()
    {
        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();
        RefreshUI();
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
        RefreshUI();
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        statusText.text = cleared ? "Dungeon cleared." : "Dungeon run ended.";
        RefreshUI();
    }

    private void TogglePartyMember(MercenaryInstance mercenary)
    {
        if (partyManager.Contains(mercenary))
        {
            RemovePartyMember(mercenary);
            return;
        }

        if (!partyManager.TryAdd(mercenary))
        {
            statusText.text = "The party is full.";
        }
    }

    private void RemovePartyMember(MercenaryInstance mercenary)
    {
        partyManager.Remove(mercenary);
    }

    private void HealMercenary(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return;
        }

        int cost = healingManager.GetFullHealCost(mercenary);
        if (!healingManager.TryHealFull(mercenary))
        {
            statusText.text = $"Could not heal {mercenary.MercenaryName}.";
            RefreshUI();
            return;
        }

        statusText.text = $"Healed {mercenary.MercenaryName} for {cost} G.";
        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();
        RefreshUI();
    }

    private void SellItem(ItemDataSO item)
    {
        int sellPrice = merchantInventory.GetSellPrice(item);
        if (!merchantInventory.SellItem(item, 1))
        {
            statusText.text = $"Could not sell {item.itemName}.";
            RefreshUI();
            return;
        }

        statusText.text = $"Sold {item.itemName} for {sellPrice} G.";
        RefreshUI();
    }

    private void BuyMarketItem(MarketStockEntry entry)
    {
        if (entry == null || entry.Item == null)
        {
            return;
        }

        int buyPrice = entry.BuyPrice;
        string itemName = entry.Item.itemName;
        if (!marketStockManager.TryBuy(entry, 1))
        {
            statusText.text = $"Could not buy {itemName}.";
            RefreshUI();
            return;
        }

        statusText.text = $"Bought {itemName} for {buyPrice} G.";
        RebuildMarketList();
        RebuildInventoryList();
        RefreshUI();
    }

    private void AdvanceDay()
    {
        dayManager.AdvanceDay();
    }

    private void CacheAlreadyHiredCandidates()
    {
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary.BaseData != null)
            {
                hiredCandidates.Add(mercenary.BaseData);
            }
        }
    }

    private void ShowHirePage()
    {
        hirePage.gameObject.SetActive(true);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, true);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, false);
        statusText.text = "Select a mercenary to hire.";
    }

    private void ShowCompanyPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(true);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, true);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildCompanyList();
        statusText.text = $"Company mercenaries: {hireManager.HiredMercenaries.Count}";
    }

    private void ShowPartyPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(true);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, true);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildPartyList();
        statusText.text = $"Party members: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void ShowHealPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(true);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, true);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildHealList();
        statusText.text =
            $"Treatment: {healingManager.HealCostPerHP} G per missing HP";
    }

    private void ShowBattlePage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(true);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, true);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, false);
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !battleManager.IsBattling;
        statusText.text = $"Battle party: {partyManager.Members.Count} mercenaries";
    }

    private void ShowDungeonPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(true);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, true);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, false);

        dungeonStatusText.text = dungeonRunManager.IsRunning
            ? $"Dungeon running: {dungeonRunManager.CurrentEncounter}/{dungeonRunManager.EncounterCount}"
            : $"Ready for {dungeonRunManager.EncounterCount} encounters.";
        statusText.text = $"Dungeon party: {partyManager.Members.Count} mercenaries";
        RefreshUI();
    }

    private void ShowMarketPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(true);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, true);
        SetTabActive(inventoryTabButton, false);
        RebuildMarketList();
        statusText.text =
            $"Market stock: {marketStockManager.Stock.Count} / {marketPriceManager.GetMarketSummary()}";
    }

    private void ShowInventoryPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(true);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(inventoryTabButton, true);
        RebuildInventoryList();
        statusText.text =
            $"Inventory items: {merchantInventory.Items.Count} / {marketPriceManager.GetMarketSummary()}";
    }

    private void RefreshUI()
    {
        goldText.text = $"GOLD  {merchantData.Gold}";

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

        if (marketInfoText != null)
        {
            marketInfoText.text =
                $"{marketPriceManager.GetMarketSummary()}  |  Sell prices refresh every day";
        }

        if (startDungeonButton != null)
        {
            startDungeonButton.interactable =
                partyManager.Members.Count > 0 &&
                !battleManager.IsBattling &&
                !dungeonRunManager.IsRunning;
        }
    }

    private static void SetTabActive(Button button, bool isActive)
    {
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

    private RectTransform CreatePanel(Transform parent)
    {
        RectTransform panel = CreateUIObject("Guild Panel", parent);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820f, 620f);
        panel.anchoredPosition = Vector2.zero;

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColor;
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
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
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
