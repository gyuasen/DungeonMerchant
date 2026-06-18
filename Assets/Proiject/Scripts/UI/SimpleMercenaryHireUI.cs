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
    private readonly List<Button> dungeonSelectButtons = new List<Button>();
    private readonly List<DungeonDataSO> displayedDungeons = new List<DungeonDataSO>();
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
    private RectTransform dungeonSelectionList;
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
    private Button firstDungeonEventButton;
    private Button secondDungeonEventButton;
    private Button thirdDungeonEventButton;
    private Button nextDayButton;
    private Text goldText;
    private Text statusText;
    private Text battleLogText;
    private Text dungeonStatusText;
    private Text dungeonEventTitleText;
    private Text dungeonEventDescriptionText;
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
        Font font = Font.CreateDynamicFontFromOSFont(
            new[] { "Yu Gothic UI", "Yu Gothic", "Meiryo", "MS Gothic" },
            16);

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        CreateText(panel, "傭兵商会", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), Color.white);

        goldText = CreateText(panel, string.Empty, 20, FontStyle.Bold, TextAnchor.MiddleRight,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), AccentColor);

        hireTabButton = CreateNavigationButton(panel, "雇用", new Vector2(28f, -78f), ShowHirePage);
        companyTabButton = CreateNavigationButton(
            panel,
            "商会",
            new Vector2(124f, -78f),
            ShowCompanyPage);
        partyTabButton = CreateNavigationButton(
            panel,
            "編成",
            new Vector2(220f, -78f),
            ShowPartyPage);
        healTabButton = CreateNavigationButton(
            panel,
            "治療",
            new Vector2(316f, -78f),
            ShowHealPage);
        battleTabButton = CreateNavigationButton(
            panel,
            "戦闘",
            new Vector2(412f, -78f),
            ShowBattlePage);
        dungeonTabButton = CreateNavigationButton(
            panel,
            "探索",
            new Vector2(508f, -78f),
            ShowDungeonPage);
        marketTabButton = CreateNavigationButton(
            panel,
            "市場",
            new Vector2(604f, -78f),
            ShowMarketPage);
        inventoryTabButton = CreateNavigationButton(
            panel,
            "在庫",
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

        statusText = CreateText(panel, "雇用する傭兵を選択してください。", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(28f, 22f), new Vector2(-28f, 54f), MutedTextColor);
        statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        statusText.rectTransform.pivot = new Vector2(0.5f, 0f);
    }

    private void BuildHirePage()
    {
        CreateText(hirePage, "契約可能な傭兵", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
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
        CreateText(companyPage, "雇用済み傭兵", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
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
        CreateText(partyPage, "探索パーティー", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        partyList = CreateUIObject("Party List", partyPage);
        partyList.anchorMin = new Vector2(0f, 0f);
        partyList.anchorMax = new Vector2(1f, 1f);
        partyList.offsetMin = Vector2.zero;
        partyList.offsetMax = new Vector2(0f, -44f);
    }

    private void BuildHealPage()
    {
        CreateText(healPage, "治療所", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        CreateText(
            healPage,
            $"全回復費用: 失ったHP 1につき {healingManager.HealCostPerHP} G。" +
            $"日送りで毎日 {healingManager.NaturalHealPerDay} HP回復します。",
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

        CreateText(battlePage, "戦闘準備", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        CreateText(battlePage, enemyDescription, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f), Color.white);

        startBattleButton = CreateActionButton(
            battlePage,
            "開始",
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

        battleLogText = CreateText(battleLogContent, "戦闘準備完了。", 14, FontStyle.Normal,
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
        CreateText(dungeonPage, "ダンジョン探索", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        dungeonStatusText = CreateText(
            dungeonPage,
            "パーティーを編成してダンジョンへ向かいましょう。",
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -92f),
            new Vector2(-170f, -42f),
            Color.white);

        startDungeonButton = CreateActionButton(
            dungeonPage,
            "探索開始",
            StartDungeonRun);
        RectTransform startRect = startDungeonButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);

        CreateText(
            dungeonPage,
            "戦闘の間にイベントが発生します。報酬、回復、危険行動、撤退を選択できます。",
            15,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, -170f),
            new Vector2(0f, -112f),
            MutedTextColor);

        dungeonSelectionList = CreateUIObject("Dungeon Selection List", dungeonPage);
        dungeonSelectionList.anchorMin = new Vector2(0f, 0f);
        dungeonSelectionList.anchorMax = new Vector2(1f, 1f);
        dungeonSelectionList.offsetMin = Vector2.zero;
        dungeonSelectionList.offsetMax = new Vector2(0f, -174f);

        dungeonEventTitleText = CreateText(
            dungeonPage,
            string.Empty,
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -230f),
            new Vector2(0f, -184f),
            AccentColor);

        dungeonEventDescriptionText = CreateText(
            dungeonPage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, -300f),
            new Vector2(0f, -238f),
            Color.white);

        firstDungeonEventButton = CreateActionButton(
            dungeonPage,
            "選択肢1",
            () => ChooseDungeonEventOption(0));
        PositionDungeonEventButton(firstDungeonEventButton, 0f);

        secondDungeonEventButton = CreateActionButton(
            dungeonPage,
            "選択肢2",
            () => ChooseDungeonEventOption(1));
        PositionDungeonEventButton(secondDungeonEventButton, 248f);

        thirdDungeonEventButton = CreateActionButton(
            dungeonPage,
            "撤退",
            () => ChooseDungeonEventOption(2));
        PositionDungeonEventButton(thirdDungeonEventButton, 496f);

        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();
    }

    private static void PositionDungeonEventButton(Button button, float x)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(228f, 52f);
        rect.anchoredPosition = new Vector2(x, 18f);
    }

    private void BuildInventoryPage()
    {
        CreateText(inventoryPage, "商人在庫", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        marketInfoText = CreateText(inventoryPage, string.Empty, 16, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(-160f, -38f),
            Color.white);

        nextDayButton = CreateActionButton(inventoryPage, "翌日へ", AdvanceDay);
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
        CreateText(marketPage, "本日の仕入れ商品", 15, FontStyle.Normal,
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
            CreateText(companyList, "雇用済みの傭兵はいません。", 18, FontStyle.Normal,
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
            CreateText(healList, "治療できる傭兵はいません。", 18, FontStyle.Normal,
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
            CreateText(inventoryList, "在庫はありません。", 18, FontStyle.Normal,
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
            CreateText(marketList, "本日仕入れ可能な商品はありません。", 18, FontStyle.Normal,
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
            $"{JapaneseDisplayText.GetMercenaryClass(candidate.mercenaryClass)}  |  " +
            $"{JapaneseDisplayText.GetContractType(candidate.contractType)}  |  " +
            $"HP {candidate.maxHP}  攻撃 {candidate.attack}  防御 {candidate.defense}";

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
            $"レベル {mercenary.Level}  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}  |  " +
            $"{JapaneseDisplayText.GetContractType(mercenary.ContractType)}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        string shortId = mercenary.InstanceId.Substring(0, 8).ToUpperInvariant();
        CreateText(row, $"ID {shortId}", 13, FontStyle.Normal, TextAnchor.MiddleRight,
            new Vector2(18f, -64f), new Vector2(-170f, -30f), MutedTextColor);

        string actionLabel = partyManager.Contains(mercenary) ? "外す" : "加える";
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
            $"{JapaneseDisplayText.GetMercenaryClass(candidate.MercenaryClass)}  |  " +
            $"{JapaneseDisplayText.GetContractType(candidate.ContractType)}  |  " +
            $"HP {candidate.MaxHP}  攻撃 {candidate.Attack}  防御 {candidate.Defense}";

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
            $"レベル {mercenary.Level}  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        CreateActionButton(row, "外す", () => RemovePartyMember(mercenary));
    }

    private void CreateEmptyPartyRow(RectTransform parent, int slotIndex, float top)
    {
        RectTransform row = CreateRow($"Empty Party Slot {slotIndex + 1}", parent, top);

        CreateText(row, $"{slotIndex + 1}. 空き枠", 20, FontStyle.Bold,
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
            $"不足 {missingHP}  |  全回復 {healCost} G";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button healButton = CreateActionButton(row, "治療", () => HealMercenary(mercenary));
        healButton.interactable = healingManager.CanHeal(mercenary);
    }

    private void CreateInventoryRow(
        RectTransform parent,
        InventoryItemStack stack,
        float top)
    {
        ItemDataSO item = stack.Item;
        RectTransform row = CreateRow(item.itemName, parent, top);

        CreateText(row, $"{JapaneseDisplayText.GetItemName(item)} x{stack.Amount}", 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f),
            Color.white);

        int sellPrice = merchantInventory.GetSellPrice(item);
        int percent = Mathf.RoundToInt(marketPriceManager.GetSellMultiplier(item) * 100f);
        string details =
            $"{JapaneseDisplayText.GetItemRarity(item.rarity)}  |  " +
            $"{JapaneseDisplayText.GetItemType(item.itemType)}  |  基準 {item.basePrice} G  |  " +
            $"本日 {sellPrice} G ({percent}%)";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        CreateActionButton(row, "売却", () => SellItem(item));
    }

    private void CreateMarketRow(
        RectTransform parent,
        MarketStockEntry entry,
        float top)
    {
        ItemDataSO item = entry.Item;
        RectTransform row = CreateRow(item.itemName, parent, top);

        CreateText(row, $"{JapaneseDisplayText.GetItemName(item)} x{entry.Quantity}", 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f),
            Color.white);

        int sellPrice = merchantInventory.GetSellPrice(item);
        string details =
            $"{JapaneseDisplayText.GetItemRarity(item.rarity)}  |  " +
            $"{JapaneseDisplayText.GetItemType(item.itemType)}  |  仕入れ {entry.BuyPrice} G  |  " +
            $"本日売値 {sellPrice} G";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button buyButton = CreateActionButton(row, "購入", () => BuyMarketItem(entry));
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
            statusText.text = $"{candidate.mercenaryName}を雇用できませんでした。";
            RefreshUI();
            return;
        }

        hiredCandidates.Add(candidate);
        statusText.text = $"{candidate.mercenaryName}が商会に加わりました。";
        RebuildHireList();
        RefreshUI();
    }

    private void HireGeneratedCandidate(MercenaryInstance candidate)
    {
        if (!hireManager.TryHireMercenary(candidate))
        {
            statusText.text = $"{candidate.MercenaryName}を雇用できませんでした。";
            RefreshUI();
            return;
        }

        statusText.text = $"{candidate.MercenaryName}が商会に加わりました。";
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
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
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
        statusText.text = $"{currentDay}日目になりました。市場価格が更新されました。";
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

    private void SelectDungeon(DungeonDataSO data)
    {
        if (!dungeonRunManager.TrySelectDungeon(data))
        {
            statusText.text = "このダンジョンはまだ選択できません。";
            return;
        }

        RebuildDungeonSelectionList();
        ShowDungeonPage();
    }

    private void RebuildDungeonSelectionList()
    {
        if (dungeonSelectionList == null)
        {
            return;
        }

        ClearChildren(dungeonSelectionList);
        dungeonSelectButtons.Clear();
        displayedDungeons.Clear();

        float rowTop = 0f;
        foreach (DungeonDataSO data in dungeonRunManager.AvailableDungeons)
        {
            if (data == null)
            {
                continue;
            }

            CreateDungeonSelectionRow(data, rowTop);
            rowTop -= 50f;
        }
    }

    private void CreateDungeonSelectionRow(DungeonDataSO data, float top)
    {
        RectTransform row = CreateUIObject(data.dungeonName, dungeonSelectionList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 44f);
        row.offsetMax = new Vector2(0f, top);

        Image image = row.gameObject.AddComponent<Image>();
        image.color = RowColor;

        string grade = JapaneseDisplayText.GetDungeonGrade(data.grade);
        string details =
            $"{grade}  |  {data.dungeonName}  |  {GetDungeonEnemyGradeSummary(data)}  |  " +
            $"踏破{data.clearGoldReward} G";
        CreateText(row, details, 14, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(14f, -44f), new Vector2(-130f, -8f), Color.white);

        bool unlocked = dungeonRunManager.IsDungeonUnlocked(data);
        bool selected = dungeonRunManager.SelectedDungeon == data;
        string label = selected ? "選択中" : unlocked ? "選択" : "未開放";
        Button button = CreateActionButton(row, label, () => SelectDungeon(data));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(108f, 34f);
        button.interactable = unlocked && !selected;
        dungeonSelectButtons.Add(button);
        displayedDungeons.Add(data);
    }

    private static string GetDungeonEnemyGradeSummary(DungeonDataSO data)
    {
        List<int> grades = new List<int>();
        if (data.normalEnemies != null)
        {
            foreach (EnemyDataSO enemy in data.normalEnemies)
            {
                if (enemy != null && !grades.Contains(enemy.monsterGrade))
                {
                    grades.Add(enemy.monsterGrade);
                }
            }
        }

        grades.Sort((left, right) => right.CompareTo(left));
        string normalGrades = grades.Count > 0
            ? string.Join("・", grades)
            : "未設定";
        string bossGrade = data.bossEnemy != null
            ? data.bossEnemy.monsterGrade.ToString()
            : "なし";
        return $"通常{normalGrades}等級 / ボス{bossGrade}等級";
    }

    private void ChooseDungeonEventOption(int optionIndex)
    {
        if (!dungeonRunManager.ChooseEventOption(optionIndex))
        {
            statusText.text = "その選択肢は現在選べません。";
            UpdateDungeonEventUI();
            return;
        }

        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();

        if (dungeonRunManager.IsRunning && battleManager.IsBattling)
        {
            ShowBattlePage();
        }
        else
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
        statusText.text = victory ? "戦闘に勝利しました。" : "戦闘に敗北しました。";
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
        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            ShowDungeonPage();
        }

        RefreshUI();
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        statusText.text = cleared ? "ダンジョンを踏破しました。" : "ダンジョン探索を終了しました。";
        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();
        RefreshUI();
    }

    private void UpdateDungeonEventUI()
    {
        if (dungeonEventTitleText == null ||
            dungeonEventDescriptionText == null ||
            firstDungeonEventButton == null ||
            secondDungeonEventButton == null ||
            thirdDungeonEventButton == null)
        {
            return;
        }

        bool showEvent = dungeonRunManager.IsAwaitingEventChoice;
        if (dungeonSelectionList != null)
        {
            dungeonSelectionList.gameObject.SetActive(!dungeonRunManager.IsRunning);
        }

        dungeonEventTitleText.gameObject.SetActive(showEvent);
        dungeonEventDescriptionText.gameObject.SetActive(showEvent);
        firstDungeonEventButton.gameObject.SetActive(showEvent);
        secondDungeonEventButton.gameObject.SetActive(showEvent);
        thirdDungeonEventButton.gameObject.SetActive(showEvent);

        if (!showEvent)
        {
            return;
        }

        dungeonEventTitleText.text = dungeonRunManager.EventTitle;
        dungeonEventDescriptionText.text = dungeonRunManager.EventDescription;
        SetButtonLabel(firstDungeonEventButton, dungeonRunManager.FirstOptionLabel);
        SetButtonLabel(secondDungeonEventButton, dungeonRunManager.SecondOptionLabel);
        SetButtonLabel(thirdDungeonEventButton, dungeonRunManager.ThirdOptionLabel);
    }

    private static void SetButtonLabel(Button button, string label)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = label;
        }
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
            statusText.text = "パーティーは満員です。";
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
            statusText.text = $"{mercenary.MercenaryName}を治療できませんでした。";
            RefreshUI();
            return;
        }

        statusText.text = $"{mercenary.MercenaryName}を{cost} Gで治療しました。";
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
            statusText.text = $"{JapaneseDisplayText.GetItemName(item)}を売却できませんでした。";
            RefreshUI();
            return;
        }

        statusText.text = $"{JapaneseDisplayText.GetItemName(item)}を{sellPrice} Gで売却しました。";
        RefreshUI();
    }

    private void BuyMarketItem(MarketStockEntry entry)
    {
        if (entry == null || entry.Item == null)
        {
            return;
        }

        int buyPrice = entry.BuyPrice;
        if (!marketStockManager.TryBuy(entry, 1))
        {
            statusText.text = $"{JapaneseDisplayText.GetItemName(entry.Item)}を購入できませんでした。";
            RefreshUI();
            return;
        }

        statusText.text = $"{JapaneseDisplayText.GetItemName(entry.Item)}を{buyPrice} Gで購入しました。";
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
        statusText.text = "雇用する傭兵を選択してください。";
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
        statusText.text = $"商会所属の傭兵: {hireManager.HiredMercenaries.Count}人";
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
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
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
            $"治療費: 失ったHP 1につき {healingManager.HealCostPerHP} G";
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
        statusText.text = $"戦闘参加: 傭兵{partyManager.Members.Count}人";
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

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            dungeonStatusText.text =
                $"遭遇 {dungeonRunManager.CurrentEncounter}/" +
                $"{dungeonRunManager.EncounterCount} を突破。次の行動を選んでください。";
        }
        else
        {
            dungeonStatusText.text = dungeonRunManager.IsRunning
                ? $"探索中: {dungeonRunManager.CurrentEncounter}/{dungeonRunManager.EncounterCount}"
                : $"{dungeonRunManager.DungeonName}  |  " +
                  $"遭遇{dungeonRunManager.EncounterCount}回  |  " +
                  $"踏破報酬 {dungeonRunManager.ClearGoldReward} G";
        }

        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();
        statusText.text = $"探索パーティー: 傭兵{partyManager.Members.Count}人";
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
            $"仕入れ商品: {marketStockManager.Stock.Count}種類 / {marketPriceManager.GetMarketSummary()}";
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
            $"在庫品目: {merchantInventory.Items.Count}種類 / {marketPriceManager.GetMarketSummary()}";
    }

    private void RefreshUI()
    {
        goldText.text = $"所持金  {merchantData.Gold} G";

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
                $"{marketPriceManager.GetMarketSummary()}  |  売却価格は日ごとに更新";
        }

        if (startDungeonButton != null)
        {
            startDungeonButton.gameObject.SetActive(!dungeonRunManager.IsRunning);
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
