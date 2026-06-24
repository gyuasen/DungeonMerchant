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
    [SerializeField] private BlacksmithManager blacksmithManager;
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private HealingManager healingManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private ProgressionManager progressionManager;

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
    private readonly List<Button> blacksmithCraftButtons = new List<Button>();
    private readonly List<EquipmentRecipeSO> displayedBlacksmithRecipes =
        new List<EquipmentRecipeSO>();
    private readonly List<Button> dungeonSelectButtons = new List<Button>();
    private readonly List<DungeonDataSO> displayedDungeons = new List<DungeonDataSO>();
    private readonly HashSet<MercenaryDataSO> hiredCandidates = new HashSet<MercenaryDataSO>();
    private readonly List<string> battleLogLines = new List<string>();
    private readonly List<Button> townMapButtons = new List<Button>();
    private readonly HashSet<int> unlockedTownIndices = new HashSet<int> { 2 };

    private RectTransform guildPanel;
    private RectTransform characterDetailOverlay;
    private Text characterDetailTitle;
    private Text characterDetailText;
    private RectTransform characterEquipmentList;
    private ScrollRect characterEquipmentScrollRect;
    private MercenaryInstance selectedDetailMercenary;
    private RectTransform equipmentDetailOverlay;
    private Text equipmentDetailTitle;
    private Text equipmentDetailText;
    private Button equipmentEnhanceButton;
    private Button equipmentSellButton;
    private Button equipmentLockButton;
    private EquipmentInstance selectedEquipmentDetail;
    private RectTransform questOverlay;
    private RectTransform questList;
    private RectTransform merchantStatusOverlay;
    private RectTransform merchantSkillList;
    private RectTransform equipmentCollectionOverlay;
    private RectTransform equipmentCollectionContent;
    private Text equipmentCollectionText;
    private RectTransform travelConfirmationOverlay;
    private Text travelConfirmationText;
    private InventoryFilter inventoryFilter = InventoryFilter.All;
    private EquipmentSort equipmentSort = EquipmentSort.Name;
    private RectTransform hirePage;
    private RectTransform globalMapPage;
    private RectTransform worldMapPage;
    private RectTransform townMapPage;
    private RectTransform hireList;
    private RectTransform companyPage;
    private RectTransform partyPage;
    private RectTransform healPage;
    private RectTransform battlePage;
    private RectTransform dungeonPage;
    private RectTransform marketPage;
    private RectTransform blacksmithPage;
    private RectTransform inventoryPage;
    private RectTransform companyScrollContent;
    private RectTransform companyList;
    private RectTransform partyList;
    private RectTransform healList;
    private RectTransform inventoryList;
    private RectTransform marketList;
    private RectTransform blacksmithList;
    private RectTransform dungeonSelectionList;
    private Button hireTabButton = null;
    private Button mapButton;
    private Button townMapButton;
    private Button companyTabButton = null;
    private Button partyTabButton = null;
    private Button healTabButton = null;
    private Button battleTabButton = null;
    private Button dungeonTabButton = null;
    private Button marketTabButton = null;
    private Button blacksmithTabButton = null;
    private Button inventoryTabButton = null;
    private Button startBattleButton;
    private Button startDungeonButton;
    private Button firstDungeonEventButton;
    private Button secondDungeonEventButton;
    private Button thirdDungeonEventButton;
    private Button nextDayButton;
    private Button contractSelectButton;
    private Button inventoryFilterButton;
    private Button equipmentSortButton;
    private Text goldText;
    private Text dayText;
    private Text statusText;
    private Text battleLogText;
    private Text dungeonStatusText;
    private Text dungeonEventTitleText;
    private Text dungeonEventDescriptionText;
    private Text marketInfoText;
    private Font uiFont;
    private RectTransform battleLogContent;
    private RectTransform battleLogViewport;
    private ScrollRect battleLogScrollRect;
    private Coroutine battleLogScrollCoroutine;
    private int currentTownIndex = 2;
    private int pendingTravelTownIndex = -1;
    private int confirmationTravelTownIndex = -1;
    private bool confirmationOpenDungeonAfterTravel;
    private bool pendingTravelWasUnlock;
    private bool pendingOpenDungeonAfterTravel;

    private static readonly string[] TownNames =
    {
        "エルド交易都市",
        "リーフ森林都市",
        "セイル港湾都市"
    };

    public int CurrentTownIndex => currentTownIndex;

    public List<int> GetUnlockedTownIndices()
    {
        List<int> result = new List<int>(unlockedTownIndices);
        result.Sort();
        return result;
    }

    public void RestoreTownProgress(
        int townIndex,
        IReadOnlyList<int> savedUnlockedTownIndices)
    {
        unlockedTownIndices.Clear();
        unlockedTownIndices.Add(2);

        int leftmostUnlockedTownIndex = 2;
        if (savedUnlockedTownIndices != null)
        {
            foreach (int unlockedTownIndex in savedUnlockedTownIndices)
            {
                if (unlockedTownIndex >= 0 &&
                    unlockedTownIndex < TownNames.Length)
                {
                    leftmostUnlockedTownIndex =
                        Mathf.Min(leftmostUnlockedTownIndex, unlockedTownIndex);
                }
            }
        }

        currentTownIndex = Mathf.Clamp(townIndex, 0, TownNames.Length - 1);
        leftmostUnlockedTownIndex =
            Mathf.Min(leftmostUnlockedTownIndex, currentTownIndex);
        for (int i = leftmostUnlockedTownIndex; i < TownNames.Length; i++)
        {
            unlockedTownIndices.Add(i);
        }

        SyncDungeonUnlocks();
        RefreshTownMapButtons();
    }

    private static readonly Color BackgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
    private static readonly Color PanelColor = new Color(0.13f, 0.15f, 0.18f, 1f);
    private static readonly Color RowColor = new Color(0.19f, 0.21f, 0.24f, 1f);
    private static readonly Color AccentColor = new Color(0.2f, 0.65f, 0.48f, 1f);
    private static readonly Color InactiveColor = new Color(0.25f, 0.28f, 0.32f, 1f);
    private static readonly Color MutedTextColor = new Color(0.7f, 0.74f, 0.78f, 1f);

    private void Start()
    {
        ResolveReferences();
        SyncDungeonUnlocks();

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
        merchantData.ProgressionChanged += HandleProgressionChanged;
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
        blacksmithManager.CraftingChanged += HandleCraftingChanged;
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged += HandleProgressionChanged;
        }
        ShowGlobalMap();
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

        if (blacksmithManager == null)
        {
            blacksmithManager = GetComponent<BlacksmithManager>();
        }

        if (blacksmithManager == null)
        {
            blacksmithManager = FindObjectOfType<BlacksmithManager>();
        }

        if (blacksmithManager == null)
        {
            blacksmithManager = gameObject.AddComponent<BlacksmithManager>();
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

        if (saveManager == null)
        {
            saveManager = GetComponent<SaveManager>();
        }

        if (saveManager == null)
        {
            saveManager = FindObjectOfType<SaveManager>();
        }

        if (progressionManager == null)
        {
            progressionManager = GetComponent<ProgressionManager>() ??
                                 FindObjectOfType<ProgressionManager>();
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

        if (blacksmithManager == null)
        {
            Debug.LogError("Simple hire UI is missing BlacksmithManager.", this);
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
            merchantData.ProgressionChanged -= HandleProgressionChanged;
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

        if (blacksmithManager != null)
        {
            blacksmithManager.CraftingChanged -= HandleCraftingChanged;
        }
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged -= HandleProgressionChanged;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = CreateCanvas();
        RectTransform panel = CreatePanel(canvas.transform);
        guildPanel = panel;

        CreateText(panel, "傭兵商会", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), Color.white);

        mapButton = CreateActionButton(panel, "全体マップ", ShowGlobalMap);
        RectTransform mapRect = mapButton.GetComponent<RectTransform>();
        mapRect.anchorMin = mapRect.anchorMax = new Vector2(0f, 1f);
        mapRect.pivot = new Vector2(0f, 1f);
        mapRect.sizeDelta = new Vector2(120f, 40f);
        mapRect.anchoredPosition = new Vector2(172f, -18f);

        townMapButton = CreateActionButton(panel, "町マップ", ShowTownMap);
        RectTransform townMapRect = townMapButton.GetComponent<RectTransform>();
        townMapRect.anchorMin = townMapRect.anchorMax = new Vector2(0f, 1f);
        townMapRect.pivot = new Vector2(0f, 1f);
        townMapRect.sizeDelta = new Vector2(100f, 40f);
        townMapRect.anchoredPosition = new Vector2(296f, -18f);

        RectTransform dayDisplayRect =
            CreateUIObject("Day Display", panel);
        dayDisplayRect.anchorMin = dayDisplayRect.anchorMax =
            new Vector2(0f, 1f);
        dayDisplayRect.pivot = new Vector2(0f, 1f);
        dayDisplayRect.sizeDelta = new Vector2(78f, 44f);
        dayDisplayRect.anchoredPosition = new Vector2(404f, -16f);
        dayDisplayRect.gameObject.AddComponent<Image>().color =
            new Color(0.11f, 0.13f, 0.16f, 1f);

        dayText = CreateText(
            dayDisplayRect,
            string.Empty,
            18,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            Color.white);
        dayText.rectTransform.anchorMin = Vector2.zero;
        dayText.rectTransform.anchorMax = Vector2.one;
        dayText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        dayText.rectTransform.offsetMin = Vector2.zero;
        dayText.rectTransform.offsetMax = Vector2.zero;

        RectTransform merchantStatusButtonRect =
            CreateUIObject("Merchant Status Button", panel);
        merchantStatusButtonRect.anchorMin =
            merchantStatusButtonRect.anchorMax = new Vector2(1f, 1f);
        merchantStatusButtonRect.pivot = new Vector2(1f, 1f);
        merchantStatusButtonRect.sizeDelta = new Vector2(310f, 44f);
        merchantStatusButtonRect.anchoredPosition = new Vector2(-20f, -16f);
        Image merchantStatusButtonImage =
            merchantStatusButtonRect.gameObject.AddComponent<Image>();
        merchantStatusButtonImage.color = new Color(0.11f, 0.13f, 0.16f, 1f);
        Button merchantStatusButton =
            merchantStatusButtonRect.gameObject.AddComponent<Button>();
        merchantStatusButton.targetGraphic = merchantStatusButtonImage;
        merchantStatusButton.onClick.AddListener(ShowMerchantStatusOverlay);
        goldText = CreateText(
            merchantStatusButtonRect,
            string.Empty,
            18,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero,
            AccentColor);
        goldText.rectTransform.anchorMin = Vector2.zero;
        goldText.rectTransform.anchorMax = Vector2.one;
        goldText.rectTransform.offsetMin = new Vector2(12f, 0f);
        goldText.rectTransform.offsetMax = new Vector2(-12f, 0f);

        hirePage = CreatePage("Hire Page", panel);
        globalMapPage = CreatePage("Global Map Page", panel);
        worldMapPage = CreatePage("World Map Page", panel);
        townMapPage = CreatePage("Town Map Page", panel);
        companyPage = CreatePage("Company Page", panel);
        partyPage = CreatePage("Party Page", panel);
        healPage = CreatePage("Heal Page", panel);
        battlePage = CreatePage("Battle Page", panel);
        dungeonPage = CreatePage("Dungeon Page", panel);
        marketPage = CreatePage("Market Page", panel);
        blacksmithPage = CreatePage("Blacksmith Page", panel);
        inventoryPage = CreatePage("Inventory Page", panel);

        BuildHirePage();
        BuildGlobalMapPage();
        BuildWorldMapPage();
        BuildTownMapPage();
        BuildCompanyPage();
        BuildPartyPage();
        BuildHealPage();
        BuildBattlePage();
        BuildDungeonPage();
        BuildMarketPage();
        BuildBlacksmithPage();
        BuildInventoryPage();

        statusText = CreateText(panel, "雇用する傭兵を選択してください。", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(28f, 22f), new Vector2(-28f, 54f), MutedTextColor);
        statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        statusText.rectTransform.pivot = new Vector2(0.5f, 0f);

        BuildCharacterDetailOverlay();
        BuildEquipmentDetailOverlay();
        BuildEquipmentCollectionOverlay();
        BuildQuestOverlay();
        BuildMerchantStatusOverlay();
        BuildTravelConfirmationOverlay();
    }

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
            () => ShowUnavailableWorldMap("北西山岳森林地域"));
        StyleUnavailableWorldMapButton(secondContinentButton);

        Button thirdContinentButton = CreateWorldRegionButton(
            globalMapPage,
            "南西黒土地域\n最高級",
            new Vector2(-225f, -125f),
            new Vector2(360f, 245f),
            () => ShowUnavailableWorldMap("南西黒土地域"));
        StyleUnavailableWorldMapButton(thirdContinentButton);

        CreateText(
            globalMapPage,
            "探索する大陸を選択してください。",
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(14f, -34f),
            new Vector2(-14f, -4f),
            Color.white);
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
            CreateUIObject("Travel Confirmation Overlay", guildPanel);
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
        window.gameObject.AddComponent<Image>().color = PanelColor;

        CreateText(
            window,
            "町を移動しますか？",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(24f, -74f),
            new Vector2(-24f, -22f),
            Color.white);

        travelConfirmationText = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -190f),
            new Vector2(-36f, -82f),
            MutedTextColor);

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
        AddMapBackground(
            worldMapPage,
            "Maps/Map",
            "Maps/EasternRegionMap");

        townMapButtons.Add(CreateTownMapButton(
            worldMapPage,
            TownNames[0],
            new Vector2(-330f, -15f),
            () => TravelToTown(0)));
        townMapButtons.Add(CreateTownMapButton(
            worldMapPage,
            TownNames[1],
            new Vector2(0f, 105f),
            () => TravelToTown(1)));
        townMapButtons.Add(CreateTownMapButton(
            worldMapPage,
            TownNames[2],
            new Vector2(300f, 35f),
            () => TravelToTown(2)));
        CreateMapButton(
            worldMapPage,
            "低級洞窟",
            new Vector2(-225f, -85f),
            new Vector2(96f, 42f),
            () => TravelToDungeon(0));
        CreateMapButton(
            worldMapPage,
            "森林遺跡",
            new Vector2(115f, 100f),
            new Vector2(96f, 42f),
            () => TravelToDungeon(1));
        CreateMapButton(
            worldMapPage,
            "海蝕迷宮",
            new Vector2(205f, -35f),
            new Vector2(96f, 42f),
            () => TravelToDungeon(2));

        Button globalMapButton = CreateMapButton(
            worldMapPage,
            "← 全体マップへ",
            new Vector2(-315f, -185f),
            new Vector2(150f, 46f),
            ShowGlobalMap);
        globalMapButton.targetGraphic.color =
            new Color(0.12f, 0.32f, 0.52f, 0.96f);

        CreateText(
            worldMapPage,
            "未解放の町は移動クエストに勝利すると解放されます。町の移動で1日経過します。",
            14,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(14f, -34f),
            new Vector2(-14f, -4f),
            Color.white);

        RefreshTownMapButtons();
    }

    private void BuildTownMapPage()
    {
        AddMapBackground(townMapPage, "Maps/TownMap");

        CreateMapButton(
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
            townMapPage, "訓練場", new Vector2(105f, -105f),
            new Vector2(100f, 48f), ShowBattlePage);
        CreateMapButton(
            townMapPage, "近隣ダンジョン", new Vector2(0f, -172f),
            new Vector2(150f, 52f), OpenNearbyDungeon);
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

    private void BuildMerchantStatusOverlay()
    {
        merchantStatusOverlay =
            CreateUIObject("Merchant Status Overlay", guildPanel);
        merchantStatusOverlay.anchorMin = Vector2.zero;
        merchantStatusOverlay.anchorMax = Vector2.one;
        merchantStatusOverlay.offsetMin = Vector2.zero;
        merchantStatusOverlay.offsetMax = Vector2.zero;
        merchantStatusOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Merchant Status Window", merchantStatusOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 580f);
        window.gameObject.AddComponent<Image>().color = PanelColor;

        CreateText(
            window,
            "商人ステータス",
            26,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -64f),
            new Vector2(-120f, -20f),
            Color.white);

        RectTransform viewport =
            CreateUIObject("Merchant Status Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 28f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        merchantSkillList = CreateUIObject("Merchant Skill List", viewport);
        merchantSkillList.anchorMin = new Vector2(0f, 1f);
        merchantSkillList.anchorMax = new Vector2(1f, 1f);
        merchantSkillList.pivot = new Vector2(0.5f, 1f);

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = merchantSkillList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        Button closeButton =
            CreateActionButton(window, "閉じる", HideMerchantStatusOverlay);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        merchantStatusOverlay.gameObject.SetActive(false);
    }

    private void BuildEquipmentDetailOverlay()
    {
        equipmentDetailOverlay = CreateUIObject("Equipment Detail Overlay", guildPanel);
        equipmentDetailOverlay.anchorMin = Vector2.zero;
        equipmentDetailOverlay.anchorMax = Vector2.one;
        equipmentDetailOverlay.offsetMin = Vector2.zero;
        equipmentDetailOverlay.offsetMax = Vector2.zero;
        equipmentDetailOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.78f);

        RectTransform window = CreateUIObject("Equipment Detail Window", equipmentDetailOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(600f, 470f);
        window.gameObject.AddComponent<Image>().color = PanelColor;

        equipmentDetailTitle = CreateText(
            window, string.Empty, 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -66f), new Vector2(-28f, -20f), Color.white);
        equipmentDetailText = CreateText(
            window, string.Empty, 17, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(28f, 92f), new Vector2(-28f, -82f), Color.white);
        equipmentDetailText.rectTransform.anchorMin = Vector2.zero;
        equipmentDetailText.rectTransform.anchorMax = Vector2.one;
        equipmentDetailText.rectTransform.offsetMin = new Vector2(28f, 92f);
        equipmentDetailText.rectTransform.offsetMax = new Vector2(-28f, -82f);

        equipmentEnhanceButton =
            CreateActionButton(window, "強化", EnhanceSelectedEquipment);
        RectTransform enhanceRect = equipmentEnhanceButton.GetComponent<RectTransform>();
        enhanceRect.anchorMin = enhanceRect.anchorMax = new Vector2(1f, 0f);
        enhanceRect.pivot = new Vector2(1f, 0f);
        enhanceRect.anchoredPosition = new Vector2(-174f, 24f);

        equipmentSellButton =
            CreateActionButton(window, "売却", SellSelectedEquipment);
        RectTransform sellRect = equipmentSellButton.GetComponent<RectTransform>();
        sellRect.anchorMin = sellRect.anchorMax = new Vector2(1f, 0f);
        sellRect.pivot = new Vector2(1f, 0f);
        sellRect.anchoredPosition = new Vector2(-28f, 24f);

        equipmentLockButton =
            CreateActionButton(window, "ロック", ToggleSelectedEquipmentLock);
        RectTransform lockRect = equipmentLockButton.GetComponent<RectTransform>();
        lockRect.anchorMin = lockRect.anchorMax = new Vector2(0f, 0f);
        lockRect.pivot = new Vector2(0f, 0f);
        lockRect.anchoredPosition = new Vector2(28f, 24f);

        Button closeButton = CreateActionButton(window, "閉じる", HideEquipmentDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        equipmentDetailOverlay.gameObject.SetActive(false);
    }

    private void BuildEquipmentCollectionOverlay()
    {
        equipmentCollectionOverlay =
            CreateUIObject("Equipment Collection Overlay", guildPanel);
        equipmentCollectionOverlay.anchorMin = Vector2.zero;
        equipmentCollectionOverlay.anchorMax = Vector2.one;
        equipmentCollectionOverlay.offsetMin = Vector2.zero;
        equipmentCollectionOverlay.offsetMax = Vector2.zero;
        equipmentCollectionOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Equipment Collection Window", equipmentCollectionOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 560f);
        window.gameObject.AddComponent<Image>().color = PanelColor;

        CreateText(
            window, "装備図鑑", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f), Color.white);

        RectTransform viewport = CreateUIObject("Collection Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 28f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        equipmentCollectionContent =
            CreateUIObject("Collection Content", viewport);
        equipmentCollectionContent.anchorMin = new Vector2(0f, 1f);
        equipmentCollectionContent.anchorMax = new Vector2(1f, 1f);
        equipmentCollectionContent.pivot = new Vector2(0.5f, 1f);
        equipmentCollectionText = CreateText(
            equipmentCollectionContent, string.Empty, 16, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(12f, 12f),
            new Vector2(-12f, -12f), Color.white);
        equipmentCollectionText.supportRichText = true;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = equipmentCollectionContent;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        Button closeButton =
            CreateActionButton(window, "閉じる", HideEquipmentCollection);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        equipmentCollectionOverlay.gameObject.SetActive(false);
    }

    private void BuildQuestOverlay()
    {
        questOverlay = CreateUIObject("Quest Overlay", guildPanel);
        questOverlay.anchorMin = Vector2.zero;
        questOverlay.anchorMax = Vector2.one;
        questOverlay.offsetMin = Vector2.zero;
        questOverlay.offsetMax = Vector2.zero;
        questOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject("Quest Window", questOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 560f);
        window.gameObject.AddComponent<Image>().color = PanelColor;
        CreateText(
            window, "依頼", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f), Color.white);

        RectTransform viewport = CreateUIObject("Quest Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 28f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        questList = CreateUIObject("Quest List", viewport);
        questList.anchorMin = new Vector2(0f, 1f);
        questList.anchorMax = new Vector2(1f, 1f);
        questList.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = questList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        Button closeButton = CreateActionButton(window, "閉じる", HideQuestOverlay);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        questOverlay.gameObject.SetActive(false);
    }

    private void BuildCharacterDetailOverlay()
    {
        characterDetailOverlay = CreateUIObject("Character Detail Overlay", guildPanel);
        characterDetailOverlay.anchorMin = Vector2.zero;
        characterDetailOverlay.anchorMax = Vector2.one;
        characterDetailOverlay.offsetMin = Vector2.zero;
        characterDetailOverlay.offsetMax = Vector2.zero;

        Image overlayImage = characterDetailOverlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.78f);

        RectTransform window = CreateUIObject("Character Detail Window", characterDetailOverlay);
        window.anchorMin = new Vector2(0.5f, 0.5f);
        window.anchorMax = new Vector2(0.5f, 0.5f);
        window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(780f, 540f);
        window.anchoredPosition = Vector2.zero;

        Image windowImage = window.gameObject.AddComponent<Image>();
        windowImage.color = PanelColor;

        characterDetailTitle = CreateText(
            window,
            string.Empty,
            26,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -64f),
            new Vector2(-120f, -20f),
            Color.white);

        characterDetailText = CreateText(
            window,
            string.Empty,
            17,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(28f, 28f),
            new Vector2(-414f, -86f),
            Color.white);
        characterDetailText.rectTransform.anchorMin = Vector2.zero;
        characterDetailText.rectTransform.anchorMax = Vector2.one;
        characterDetailText.rectTransform.offsetMin = new Vector2(28f, 28f);
        characterDetailText.rectTransform.offsetMax = new Vector2(-414f, -86f);

        CreateText(
            window,
            "装備変更",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(386f, -104f),
            new Vector2(-28f, -68f),
            Color.white);

        RectTransform equipmentViewport =
            CreateUIObject("Equipment Viewport", window);
        equipmentViewport.anchorMin = new Vector2(1f, 1f);
        equipmentViewport.anchorMax = new Vector2(1f, 1f);
        equipmentViewport.pivot = new Vector2(1f, 1f);
        equipmentViewport.sizeDelta = new Vector2(366f, 398f);
        equipmentViewport.anchoredPosition = new Vector2(-28f, -112f);

        Image equipmentViewportImage =
            equipmentViewport.gameObject.AddComponent<Image>();
        equipmentViewportImage.color = new Color(0f, 0f, 0f, 0.12f);
        Mask equipmentMask = equipmentViewport.gameObject.AddComponent<Mask>();
        equipmentMask.showMaskGraphic = false;

        characterEquipmentList =
            CreateUIObject("Equipment Scroll Content", equipmentViewport);
        characterEquipmentList.anchorMin = new Vector2(0f, 1f);
        characterEquipmentList.anchorMax = new Vector2(1f, 1f);
        characterEquipmentList.pivot = new Vector2(0.5f, 1f);
        characterEquipmentList.anchoredPosition = Vector2.zero;

        characterEquipmentScrollRect =
            equipmentViewport.gameObject.AddComponent<ScrollRect>();
        characterEquipmentScrollRect.content = characterEquipmentList;
        characterEquipmentScrollRect.viewport = equipmentViewport;
        characterEquipmentScrollRect.horizontal = false;
        characterEquipmentScrollRect.vertical = true;
        characterEquipmentScrollRect.movementType = ScrollRect.MovementType.Clamped;
        characterEquipmentScrollRect.scrollSensitivity = 30f;

        Button closeButton = CreateActionButton(window, "閉じる", HideCharacterDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        characterDetailOverlay.gameObject.SetActive(false);
    }

    private void BuildHirePage()
    {
        CreateText(hirePage, "契約可能な傭兵", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        contractSelectButton = CreateActionButton(
            hirePage,
            "契約: 日雇い",
            CycleHireContract);
        RectTransform contractRect =
            contractSelectButton.GetComponent<RectTransform>();
        contractRect.anchorMin = contractRect.anchorMax = new Vector2(1f, 1f);
        contractRect.pivot = new Vector2(1f, 1f);
        contractRect.sizeDelta = new Vector2(160f, 38f);
        contractRect.anchoredPosition = new Vector2(0f, -4f);

        RectTransform viewport = CreateUIObject("Hire Viewport", hirePage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -52f);

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

        Button questButton =
            CreateActionButton(companyPage, "依頼", ShowQuestOverlay);
        RectTransform questRect = questButton.GetComponent<RectTransform>();
        questRect.anchorMin = questRect.anchorMax = new Vector2(1f, 1f);
        questRect.pivot = new Vector2(1f, 1f);
        questRect.sizeDelta = new Vector2(110f, 38f);
        questRect.anchoredPosition = new Vector2(0f, -4f);

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
            $"戦闘不能は{healingManager.IncapacitatedCostMultiplier}倍+" +
            $"{healingManager.RevivalBaseCost} G。日送りで毎日 " +
            $"{healingManager.NaturalHealPerDay} HP回復します。",
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

        battleLogViewport = CreateUIObject("Battle Log Viewport", logPanel);
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
    }

    private void BuildDungeonPage()
    {
        CreateText(dungeonPage, "ダンジョン探索", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        dungeonStatusText = CreateText(
            dungeonPage,
            "パーティーを編成してダンジョンへ向かいましょう。",
            14,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -154f),
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

        inventoryFilterButton =
            CreateActionButton(inventoryPage, "絞込: 全て", CycleInventoryFilter);
        inventoryFilterButton.name = "Inventory Filter Button";
        RectTransform filterRect = inventoryFilterButton.GetComponent<RectTransform>();
        filterRect.anchorMin = filterRect.anchorMax = new Vector2(0f, 1f);
        filterRect.pivot = new Vector2(0f, 1f);
        filterRect.sizeDelta = new Vector2(150f, 38f);
        filterRect.anchoredPosition = new Vector2(0f, -78f);

        equipmentSortButton =
            CreateActionButton(inventoryPage, "並替: 名前", CycleEquipmentSort);
        equipmentSortButton.name = "Equipment Sort Button";
        RectTransform sortRect = equipmentSortButton.GetComponent<RectTransform>();
        sortRect.anchorMin = sortRect.anchorMax = new Vector2(0f, 1f);
        sortRect.pivot = new Vector2(0f, 1f);
        sortRect.sizeDelta = new Vector2(150f, 38f);
        sortRect.anchoredPosition = new Vector2(166f, -78f);

        Button collectionButton =
            CreateActionButton(inventoryPage, "装備図鑑", ShowEquipmentCollection);
        RectTransform collectionRect = collectionButton.GetComponent<RectTransform>();
        collectionRect.anchorMin = collectionRect.anchorMax = new Vector2(0f, 1f);
        collectionRect.pivot = new Vector2(0f, 1f);
        collectionRect.sizeDelta = new Vector2(130f, 38f);
        collectionRect.anchoredPosition = new Vector2(332f, -78f);

        Button storageButton =
            CreateActionButton(inventoryPage, "倉庫拡張", UpgradeStorage);
        RectTransform storageRect = storageButton.GetComponent<RectTransform>();
        storageRect.anchorMin = storageRect.anchorMax = new Vector2(0f, 1f);
        storageRect.pivot = new Vector2(0f, 1f);
        storageRect.sizeDelta = new Vector2(130f, 38f);
        storageRect.anchoredPosition = new Vector2(478f, -78f);

        RectTransform viewport = CreateUIObject("Inventory Viewport", inventoryPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -126f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        inventoryList = CreateUIObject("Inventory List", viewport);
        inventoryList.anchorMin = new Vector2(0f, 1f);
        inventoryList.anchorMax = new Vector2(1f, 1f);
        inventoryList.pivot = new Vector2(0.5f, 1f);
        inventoryList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = inventoryList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
    }

    private void BuildMarketPage()
    {
        CreateText(marketPage, "本日の仕入れ商品", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        RectTransform viewport = CreateUIObject("Market Viewport", marketPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -52f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        marketList = CreateUIObject("Market List", viewport);
        marketList.anchorMin = new Vector2(0f, 1f);
        marketList.anchorMax = new Vector2(1f, 1f);
        marketList.pivot = new Vector2(0.5f, 1f);
        marketList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = marketList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
    }

    private void BuildBlacksmithPage()
    {
        CreateText(blacksmithPage, "鍛冶屋", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            MutedTextColor);

        CreateText(
            blacksmithPage,
            "モンスター素材とゴールドを使い、市場では買えない武器を制作します。",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -70f),
            new Vector2(0f, -38f),
            MutedTextColor);

        RectTransform viewport = CreateUIObject("Blacksmith Viewport", blacksmithPage);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -84f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        blacksmithList = CreateUIObject("Blacksmith List", viewport);
        blacksmithList.anchorMin = new Vector2(0f, 1f);
        blacksmithList.anchorMax = new Vector2(1f, 1f);
        blacksmithList.pivot = new Vector2(0.5f, 1f);
        blacksmithList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = blacksmithList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
    }

    private void RebuildCompanyList()
    {
        ClearChildren(companyList);

        float rowTop = 0f;
        if (hireManager.HiredMercenaries.Count == 0)
        {
            CreateText(companyList, "雇用済みの傭兵はいません。", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f),
                new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            CreateCompanyRow(companyList, mercenary, rowTop);
            rowTop -= 112f;
        }

        companyList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void CreateMerchantSkillRow(
        RectTransform parent,
        MerchantSkillType skill,
        string label,
        string description,
        float top)
    {
        int rank = merchantData.GetSkillRank(skill);
        RectTransform row = CreateRow($"Merchant Skill {skill}", parent, top);
        CreateText(
            row,
            $"{label}  Lv{rank}/10",
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -45f),
            new Vector2(-160f, -10f),
            Color.white);
        CreateText(
            row,
            description,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -78f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        Button increaseButton = CreateActionButton(
            row,
            rank >= 10 ? "最大" : "+1",
            () => IncreaseMerchantSkill(skill));
        increaseButton.interactable =
            merchantData.MerchantSkillPoints > 0 && rank < 10;
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
        inventoryList.sizeDelta = new Vector2(0f, 430f);

        if (merchantInventory.Items.Count == 0 &&
            merchantInventory.EquipmentInstances.Count == 0)
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

            if (!MatchesInventoryFilter(stack.Item))
            {
                continue;
            }
            if (inventoryFilter == InventoryFilter.Locked)
            {
                continue;
            }

            CreateInventoryRow(inventoryList, stack, rowTop);
            rowTop -= 112f;
        }

        List<EquipmentInstance> sortedEquipment =
            new List<EquipmentInstance>(merchantInventory.EquipmentInstances);
        sortedEquipment.Sort(CompareEquipment);
        foreach (EquipmentInstance equipment in sortedEquipment)
        {
            if (equipment?.BaseItem == null)
            {
                continue;
            }

            if (!MatchesInventoryFilter(equipment.BaseItem))
            {
                continue;
            }
            if (inventoryFilter == InventoryFilter.Locked &&
                !equipment.IsLocked)
            {
                continue;
            }

            CreateEquipmentInventoryRow(inventoryList, equipment, rowTop);
            rowTop -= 112f;
        }

        inventoryList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void RebuildMarketList()
    {
        ClearChildren(marketList);
        marketList.sizeDelta = new Vector2(0f, 430f);
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

        marketList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void RebuildBlacksmithList()
    {
        ClearChildren(blacksmithList);
        blacksmithCraftButtons.Clear();
        displayedBlacksmithRecipes.Clear();

        if (blacksmithManager.Recipes.Count == 0)
        {
            CreateText(blacksmithList, "制作可能なレシピはありません。", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        float rowTop = 0f;
        foreach (EquipmentRecipeSO recipe in blacksmithManager.Recipes)
        {
            if (recipe == null || recipe.resultItem == null)
            {
                continue;
            }

            CreateBlacksmithRow(recipe, rowTop);
            rowTop -= 140f;
        }

        blacksmithList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void CreateCandidateRow(RectTransform parent, MercenaryDataSO candidate, float top)
    {
        RectTransform row = CreateRow(candidate.mercenaryName, parent, top);

        CreateText(row, candidate.mercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"{JapaneseDisplayText.GetMercenaryClass(candidate.mercenaryClass)}  |  " +
            $"{JapaneseDisplayText.GetContractType(GetUnlockedContractType())}  |  " +
            $"成功率 {merchantData.GetHireSuccessRate() * 100f:0}%  |  " +
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
            new Vector2(18f, -42f), new Vector2(-300f, -12f), Color.white);

        string contractStatus = mercenary.ContractNeedsRenewal
            ? "更新待ち"
            : mercenary.ContractEndDay > 0
                ? $"期限 {mercenary.ContractEndDay}日"
                : "期限なし";
        string details =
            $"レベル {mercenary.Level}  経験値 " +
            $"{mercenary.CurrentExperience}/{mercenary.ExperienceToNextLevel}  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
            $"{JapaneseDisplayText.GetContractType(mercenary.ContractType)} " +
            contractStatus;

        CreateText(row, details, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-300f, -48f), MutedTextColor);

        string shortId = mercenary.InstanceId.Substring(0, 8).ToUpperInvariant();
        CreateText(row, $"ID {shortId}", 13, FontStyle.Normal, TextAnchor.MiddleRight,
            new Vector2(18f, -64f), new Vector2(-300f, -30f), MutedTextColor);

        string actionLabel = partyManager.Contains(mercenary) ? "外す" : "加える";
        Button partyButton =
            CreateActionButton(row, actionLabel, () => TogglePartyMember(mercenary));
        partyButton.GetComponent<RectTransform>().sizeDelta = new Vector2(112f, 52f);

        Button detailsButton =
            CreateActionButton(row, "詳細", () => ShowCharacterDetails(mercenary));
        RectTransform detailsRect = detailsButton.GetComponent<RectTransform>();
        detailsRect.sizeDelta = new Vector2(112f, 52f);
        detailsRect.anchoredPosition = new Vector2(-142f, 0f);

        if (mercenary.ContractNeedsRenewal)
        {
            Button renewButton = CreateActionButton(
                row,
                $"更新 {hireManager.GetRenewalCost(mercenary)}G",
                () => RenewContract(mercenary));
            RectTransform renewRect = renewButton.GetComponent<RectTransform>();
            renewRect.sizeDelta = new Vector2(112f, 52f);
            renewRect.anchoredPosition = new Vector2(-266f, 0f);
        }
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
            $"{JapaneseDisplayText.GetContractType(GetUnlockedContractType())}  |  " +
            $"成功率 {merchantData.GetHireSuccessRate() * 100f:0}%  |  " +
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
            $"レベル {mercenary.Level}  経験値 " +
            $"{mercenary.CurrentExperience}/{mercenary.ExperienceToNextLevel}  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
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
        string condition = mercenary.IsIncapacitated ? "戦闘不能  |  " : string.Empty;
        string details =
            $"{condition}HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
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
        int percent = Mathf.RoundToInt(
            marketPriceManager.GetEffectiveSellMultiplier(item) * 100f);
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

        string details =
            $"{JapaneseDisplayText.GetMercenaryClass(item.requiredClass)}用  |  " +
            $"{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}ランク" +
            $"{item.equipmentRank}  |  攻撃+{item.bonusAttack}  " +
            $"防御+{item.bonusDefense}  HP+{item.bonusMaxHP}  |  " +
            $"仕入れ {entry.BuyPrice} G";

        CreateText(row, details, 13, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button buyButton = CreateActionButton(row, "購入", () => BuyMarketItem(entry));
        marketBuyButtons.Add(buyButton);
        displayedMarketEntries.Add(entry);
    }

    private void CreateEquipmentInventoryRow(
        RectTransform parent,
        EquipmentInstance equipment,
        float top)
    {
        ItemDataSO item = equipment.BaseItem;
        RectTransform row = CreateRow(equipment.InstanceId, parent, top);
        string quality = JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        Color qualityColor = GetEquipmentQualityColor(equipment.Quality);

        CreateText(
            row,
            $"{(equipment.IsLocked ? "[LOCK] " : string.Empty)}" +
            $"[{quality}] {GetEquipmentDisplayName(equipment)}",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            qualityColor);

        string details =
            $"HP {FormatSigned(equipment.BonusMaxHP)}  " +
            $"攻撃 {FormatSigned(equipment.BonusAttack)}  " +
            $"防御 {FormatSigned(equipment.BonusDefense)}  " +
            $"速度 {FormatSigned(equipment.BonusAttackSpeed)}";
        CreateText(
            row,
            details,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        CreateActionButton(
            row,
            "詳細",
            () => ShowEquipmentDetails(equipment));
    }

    private void CreateBlacksmithRow(EquipmentRecipeSO recipe, float top)
    {
        ItemDataSO item = recipe.resultItem;
        RectTransform row = CreateUIObject(recipe.name, blacksmithList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 124f);
        row.offsetMax = new Vector2(0f, top);

        Image rowImage = row.gameObject.AddComponent<Image>();
        rowImage.color = RowColor;

        CreateText(
            row,
            JapaneseDisplayText.GetItemName(item),
            21,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -38f),
            new Vector2(-160f, -8f),
            Color.white);

        string stats =
            $"{JapaneseDisplayText.GetMercenaryClass(item.requiredClass)}用  |  " +
            $"ランク{item.equipmentRank}  |  攻撃+{item.bonusAttack}  " +
            $"防御+{item.bonusDefense}  HP+{item.bonusMaxHP}";
        CreateText(
            row,
            stats,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -70f),
            new Vector2(-160f, -42f),
            MutedTextColor);

        string materials = BuildRecipeMaterialText(recipe);
        CreateText(
            row,
            $"{materials}  |  費用 {recipe.goldCost} G",
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -104f),
            new Vector2(-160f, -76f),
            MutedTextColor);

        Button craftButton =
            CreateActionButton(row, "制作", () => CraftEquipment(recipe));
        craftButton.interactable = blacksmithManager.CanCraft(recipe);
        blacksmithCraftButtons.Add(craftButton);
        displayedBlacksmithRecipes.Add(recipe);
    }

    private string BuildRecipeMaterialText(EquipmentRecipeSO recipe)
    {
        if (recipe.materials == null || recipe.materials.Length == 0)
        {
            return "素材なし";
        }

        List<string> materialTexts = new List<string>();
        foreach (CraftingMaterialRequirement requirement in recipe.materials)
        {
            if (requirement == null || requirement.item == null)
            {
                continue;
            }

            int owned = merchantInventory.GetItemAmount(requirement.item);
            materialTexts.Add(
                $"{JapaneseDisplayText.GetItemName(requirement.item)} " +
                $"{owned}/{requirement.amount}");
        }

        return materialTexts.Count > 0
            ? string.Join("、", materialTexts)
            : "素材なし";
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
        buttonRect.sizeDelta = new Vector2(84f, 38f);
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
        RebuildBlacksmithList();
        RefreshUI();
    }

    private void HandleMarketStockChanged()
    {
        RebuildMarketList();
        RefreshUI();
    }

    private void HandleCraftingChanged()
    {
        RebuildInventoryList();
        RebuildBlacksmithList();
        RefreshUI();
    }

    private void HandleDayChanged(int currentDay)
    {
        RebuildMarketList();
        RebuildInventoryList();
        RebuildHealList();
        RebuildCompanyList();
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
        RebuildBlacksmithList();
        RefreshUI();
    }

    private void StartPartyBattle()
    {
        ResetBattleLog();
        startBattleButton.interactable = false;

        if (!battleManager.StartBattle(partyManager.Members))
        {
            startBattleButton.interactable = true;
        }
    }

    private void StartDungeonRun()
    {
        ShowBattlePage();
        ResetBattleLog();

        if (!dungeonRunManager.StartRun())
        {
            ShowDungeonPage();
        }

        RefreshUI();
    }

    private void ResetBattleLog()
    {
        battleLogLines.Clear();
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
        string nearbyTown = data.nearbyTownIndex >= 0 &&
                            data.nearbyTownIndex < TownNames.Length
            ? TownNames[data.nearbyTownIndex]
            : "町未設定";
        int clearedFloors = dungeonRunManager.GetClearedFloors(data);
        int totalFloors = Mathf.Max(1, data.totalFloors);
        string floorProgress = clearedFloors >= totalFloors
            ? $"完全攻略 {totalFloors}/{totalFloors}F"
            : $"次回 {clearedFloors + 1}/{totalFloors}F";
        string details =
            $"{nearbyTown}近隣  |  {grade}  |  {data.dungeonName}  |  " +
            $"{floorProgress}  |  {GetDungeonEnemyGradeSummary(data)}";
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

    private static string BuildDungeonRewardPreview(DungeonDataSO data)
    {
        if (data == null)
        {
            return "報酬情報なし";
        }

        List<string> guaranteed = new List<string>();
        if (data.clearItemRewards != null)
        {
            foreach (DungeonItemReward reward in data.clearItemRewards)
            {
                if (reward?.item != null && reward.amount > 0)
                {
                    guaranteed.Add(
                        $"{JapaneseDisplayText.GetItemName(reward.item)}×{reward.amount}");
                }
            }
        }

        List<string> limited = new List<string>();
        Dictionary<EquipmentSetId, int> setCounts =
            new Dictionary<EquipmentSetId, int>();
        if (data.limitedEquipmentDrops != null)
        {
            foreach (ItemDataSO item in data.limitedEquipmentDrops)
            {
                if (item != null)
                {
                    if (item.equipmentSet != EquipmentSetId.None)
                    {
                        if (!setCounts.ContainsKey(item.equipmentSet))
                        {
                            setCounts[item.equipmentSet] = 0;
                        }
                        setCounts[item.equipmentSet]++;
                    }
                    else
                    {
                        limited.Add(JapaneseDisplayText.GetItemName(item));
                    }
                }
            }
        }

        foreach (KeyValuePair<EquipmentSetId, int> entry in setCounts)
        {
            limited.Add(
                $"{JapaneseDisplayText.GetEquipmentSet(entry.Key)}セット" +
                $"（{entry.Value}種）");
        }

        return $"確定: {(guaranteed.Count > 0 ? string.Join("、", guaranteed) : "なし")}\n" +
               $"限定: {(limited.Count > 0 ? string.Join("、", limited) : "なし")} / " +
               $"イベント{data.eventLimitedDropChance * 100f:0.#}%・" +
               $"ボス{data.bossLimitedDropChance * 100f:0.#}%";
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

        if (pendingTravelTownIndex >= 0)
        {
            int destinationTownIndex = pendingTravelTownIndex;
            bool wasUnlock = pendingTravelWasUnlock;
            bool openDungeonAfterTravel = pendingOpenDungeonAfterTravel;
            pendingTravelTownIndex = -1;
            pendingTravelWasUnlock = false;
            pendingOpenDungeonAfterTravel = false;

            if (victory)
            {
                unlockedTownIndices.Add(destinationTownIndex);
                currentTownIndex = destinationTownIndex;
                dayManager.AdvanceDay();
                SyncDungeonUnlocks();
                RefreshTownMapButtons();
                if (openDungeonAfterTravel)
                {
                    OpenNearbyDungeon();
                }
                else
                {
                    ShowTownMap();
                }
                statusText.text = wasUnlock
                    ? $"街道戦闘に勝利し、{TownNames[destinationTownIndex]}を解放しました。"
                    : $"街道戦闘に勝利し、{TownNames[destinationTownIndex]}へ到着しました。";
                saveManager?.SaveGame();
            }
            else
            {
                ShowWorldMap();
                statusText.text = wasUnlock
                    ? $"街道戦闘に敗北しました。{TownNames[destinationTownIndex]}は未解放のままです。"
                    : $"街道戦闘に敗北したため、{TownNames[destinationTownIndex]}へ移動できませんでした。";
            }
            return;
        }

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

    private void ShowCharacterDetails(MercenaryInstance mercenary)
    {
        if (mercenary == null || characterDetailOverlay == null)
        {
            return;
        }

        selectedDetailMercenary = mercenary;
        string source = mercenary.IsUnique ? "固有傭兵" : "量産型傭兵";
        string condition = mercenary.IsIncapacitated ? "戦闘不能" : "行動可能";
        string shortId = mercenary.InstanceId.Substring(0, 8).ToUpperInvariant();

        characterDetailTitle.text = mercenary.MercenaryName;
        characterDetailText.text =
            $"種別: {source}\n" +
            $"ID: {shortId}\n" +
            $"職業: {JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}\n" +
            $"契約: {JapaneseDisplayText.GetContractType(mercenary.ContractType)}\n" +
            $"契約期限: {(mercenary.ContractEndDay > 0 ? mercenary.ContractEndDay + "日" : "無期限")}" +
            $"{(mercenary.ContractNeedsRenewal ? "（更新待ち）" : string.Empty)}\n" +
            $"状態: {condition}\n\n" +
            $"レベル: {mercenary.Level}\n" +
            $"経験値: {mercenary.CurrentExperience} / " +
            $"{mercenary.ExperienceToNextLevel}\n\n" +
            $"HP: {mercenary.CurrentHP} / {mercenary.MaxHP}\n" +
            $"攻撃: {mercenary.Attack}\n" +
            $"防御: {mercenary.Defense}\n" +
            $"行動速度: {mercenary.AttackSpeed:0.00}\n" +
            $"最大魔力: {mercenary.MaxMagicPower}\n" +
            $"武器: {GetEquippedEquipmentName(mercenary, EquipmentSlot.Weapon)}\n" +
            $"防具: {GetEquippedEquipmentName(mercenary, EquipmentSlot.Armor)}\n" +
            $"装飾品: {GetEquippedEquipmentName(mercenary, EquipmentSlot.Accessory)}\n" +
            $"セット: {BuildActiveSetSummary(mercenary)}\n" +
            $"スキル: {mercenary.SkillBoardName}\n" +
            $"{BuildMercenarySkillSummary(mercenary)}\n" +
            $"雇用費: {mercenary.HireCost} G";

        RebuildCharacterEquipmentList();
        characterDetailOverlay.SetAsLastSibling();
        characterDetailOverlay.gameObject.SetActive(true);
    }

    private void HideCharacterDetails()
    {
        if (characterDetailOverlay != null)
        {
            characterDetailOverlay.gameObject.SetActive(false);
        }

        selectedDetailMercenary = null;
    }

    private void RebuildCharacterEquipmentList()
    {
        if (characterEquipmentList == null || selectedDetailMercenary == null)
        {
            return;
        }

        ClearChildren(characterEquipmentList);
        float top = 0f;
        foreach (EquipmentSlot slot in
                 System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            ItemDataSO equipped =
                selectedDetailMercenary.GetEquippedItem(slot);
            EquipmentInstance equippedInstance =
                selectedDetailMercenary.GetEquippedInstance(slot);
            if (equippedInstance != null)
            {
                CreateEquipmentInstanceOptionRow(
                    equippedInstance,
                    true,
                    top);
                top -= 116f;
            }
            else if (equipped != null)
            {
                CreateEquipmentOptionRow(equipped, true, top);
                top -= 116f;
            }
        }

        foreach (EquipmentInstance equipment in merchantInventory.EquipmentInstances)
        {
            if (equipment?.BaseItem == null ||
                !equipment.BaseItem.CanEquip(selectedDetailMercenary.MercenaryClass))
            {
                continue;
            }

            CreateEquipmentInstanceOptionRow(equipment, false, top);
            top -= 116f;
        }

        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            ItemDataSO item = stack?.Item;
            if (item == null ||
                stack.Amount <= 0 ||
                !item.CanEquip(selectedDetailMercenary.MercenaryClass))
            {
                continue;
            }

            CreateEquipmentOptionRow(item, false, top);
            top -= 116f;
        }

        if (top == 0f)
        {
            CreateText(
                characterEquipmentList,
                "装備できる武器を所持していません",
                14,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Vector2(0f, -50f),
                new Vector2(0f, 0f),
                MutedTextColor);
        }

        characterEquipmentList.sizeDelta =
            new Vector2(0f, Mathf.Max(398f, -top));
        Canvas.ForceUpdateCanvases();
        if (characterEquipmentScrollRect != null)
        {
            characterEquipmentScrollRect.StopMovement();
            characterEquipmentScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void CreateEquipmentOptionRow(ItemDataSO item, bool isEquipped, float top)
    {
        RectTransform row = CreateUIObject(
            isEquipped ? $"Equipped {item.equipmentSlot}" : item.itemName,
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 106f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;

        string owned = isEquipped
            ? "装備中"
            : $"所持 {merchantInventory.GetItemAmount(item)}";
        string stats = isEquipped
            ? BuildEquipmentBonusText(item)
            : BuildEquipmentComparisonText(
                item,
                selectedDetailMercenary.GetEquippedItem(item.equipmentSlot),
                selectedDetailMercenary.GetEquippedInstance(
                    item.equipmentSlot));
        CreateText(
            row,
            $"<b>[{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}] " +
            $"{JapaneseDisplayText.GetItemName(item)}</b>  " +
            $"R{item.equipmentRank}  {owned}\n{stats}",
            15,
            isEquipped ? FontStyle.Bold : FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(12f, -96f),
            new Vector2(-96f, -10f),
            Color.white);

        Button button = CreateActionButton(
            row,
            isEquipped ? "解除" : "装備",
            isEquipped
                ? () => UnequipSelectedEquipment(item.equipmentSlot)
                : () => EquipSelectedEquipment(item));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);
    }

    private void CreateEquipmentInstanceOptionRow(
        EquipmentInstance equipment,
        bool isEquipped,
        float top)
    {
        ItemDataSO item = equipment.BaseItem;
        RectTransform row = CreateUIObject(
            isEquipped
                ? $"Equipped Quality {item.equipmentSlot}"
                : equipment.InstanceId,
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 106f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;

        string quality = JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        Color qualityColor = GetEquipmentQualityColor(equipment.Quality);
        string stats = BuildEquipmentInstanceComparisonText(
            equipment,
            selectedDetailMercenary.GetEquippedInstance(item.equipmentSlot),
            selectedDetailMercenary.GetEquippedItem(item.equipmentSlot));
        CreateText(
            row,
            $"<b>[{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}・" +
            $"{quality}] {GetEquipmentDisplayName(equipment)}</b>  " +
            $"R{item.equipmentRank}  {(isEquipped ? "装備中" : "個体装備")}\n{stats}",
            15,
            isEquipped ? FontStyle.Bold : FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(12f, -96f),
            new Vector2(-170f, -10f),
            qualityColor);

        Button button = CreateActionButton(
            row,
            isEquipped ? "解除" : "装備",
            isEquipped
                ? () => UnequipSelectedEquipment(item.equipmentSlot)
                : () => EquipSelectedEquipment(equipment));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);

        Button detailButton = CreateActionButton(
            row,
            "詳細",
            () => ShowEquipmentDetails(equipment));
        RectTransform detailRect = detailButton.GetComponent<RectTransform>();
        detailRect.sizeDelta = new Vector2(64f, 40f);
        detailRect.anchoredPosition = new Vector2(-92f, 0f);
    }

    private static string GetEquippedEquipmentName(
        MercenaryInstance mercenary,
        EquipmentSlot slot)
    {
        ItemDataSO item = mercenary?.GetEquippedItem(slot);
        if (item == null)
        {
            return "なし";
        }

        EquipmentInstance instance = mercenary.GetEquippedInstance(slot);
        string name = JapaneseDisplayText.GetItemName(item);
        return instance != null
            ? $"[{JapaneseDisplayText.GetEquipmentQuality(instance.Quality)}] " +
              $"{GetEquipmentDisplayName(instance)}"
            : name;
    }

    private static string GetEquipmentDisplayName(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return "不明な装備";
        }

        string enhancement = equipment.EnhancementLevel > 0
            ? $" +{equipment.EnhancementLevel}"
            : string.Empty;
        return JapaneseDisplayText.GetItemName(equipment.BaseItem) + enhancement;
    }

    private static string BuildActiveSetSummary(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return "なし";
        }

        List<string> summaries = new List<string>();
        foreach (EquipmentSetId setId in
                 (EquipmentSetId[])System.Enum.GetValues(typeof(EquipmentSetId)))
        {
            if (setId == EquipmentSetId.None)
            {
                continue;
            }

            int count = mercenary.GetEquippedSetCount(setId);
            if (count <= 0)
            {
                continue;
            }

            string active = count >= 3
                ? "全効果"
                : count >= 2 ? "2部位効果" : "未発動";
            summaries.Add(
                $"{JapaneseDisplayText.GetEquipmentSet(setId)} {count}/3 {active}");
        }
        return summaries.Count > 0 ? string.Join(", ", summaries) : "なし";
    }

    private static Color GetEquipmentQualityColor(EquipmentQuality quality)
    {
        switch (quality)
        {
            case EquipmentQuality.Poor: return new Color(0.62f, 0.62f, 0.62f);
            case EquipmentQuality.Fine: return new Color(0.38f, 0.82f, 0.48f);
            case EquipmentQuality.Rare: return new Color(0.35f, 0.62f, 1f);
            case EquipmentQuality.Legendary: return new Color(1f, 0.68f, 0.18f);
            default: return Color.white;
        }
    }

    private static string BuildEquipmentInstanceComparisonText(
        EquipmentInstance candidate,
        EquipmentInstance equippedInstance,
        ItemDataSO equippedItem)
    {
        int currentHP = equippedInstance != null
            ? equippedInstance.BonusMaxHP
            : equippedItem != null ? equippedItem.bonusMaxHP : 0;
        int currentAttack = equippedInstance != null
            ? equippedInstance.BonusAttack
            : equippedItem != null ? equippedItem.bonusAttack : 0;
        int currentDefense = equippedInstance != null
            ? equippedInstance.BonusDefense
            : equippedItem != null ? equippedItem.bonusDefense : 0;
        float currentSpeed = equippedInstance != null
            ? equippedInstance.BonusAttackSpeed
            : equippedItem != null ? equippedItem.bonusAttackSpeed : 0f;

        return $"HP {FormatSigned(candidate.BonusMaxHP)} " +
               $"{FormatComparison(candidate.BonusMaxHP - currentHP)}  " +
               $"攻撃 {FormatSigned(candidate.BonusAttack)} " +
               $"{FormatComparison(candidate.BonusAttack - currentAttack)}\n" +
               $"防御 {FormatSigned(candidate.BonusDefense)} " +
               $"{FormatComparison(candidate.BonusDefense - currentDefense)}  " +
               $"速度 {FormatSigned(candidate.BonusAttackSpeed)} " +
               $"{FormatComparison(candidate.BonusAttackSpeed - currentSpeed)}";
    }

    private static string BuildEquipmentBonusText(ItemDataSO item)
    {
        return $"HP {FormatSigned(item.bonusMaxHP)}  " +
               $"攻撃 {FormatSigned(item.bonusAttack)}\n" +
               $"防御 {FormatSigned(item.bonusDefense)}  " +
               $"速度 {FormatSigned(item.bonusAttackSpeed)}";
    }

    private static string BuildEquipmentComparisonText(
        ItemDataSO candidate,
        ItemDataSO equipped,
        EquipmentInstance equippedInstance)
    {
        int currentHP = equippedInstance != null
            ? equippedInstance.BonusMaxHP
            : equipped != null ? equipped.bonusMaxHP : 0;
        int currentAttack = equippedInstance != null
            ? equippedInstance.BonusAttack
            : equipped != null ? equipped.bonusAttack : 0;
        int currentDefense = equippedInstance != null
            ? equippedInstance.BonusDefense
            : equipped != null ? equipped.bonusDefense : 0;
        float currentSpeed = equippedInstance != null
            ? equippedInstance.BonusAttackSpeed
            : equipped != null ? equipped.bonusAttackSpeed : 0f;

        return $"HP {FormatSigned(candidate.bonusMaxHP)} " +
               $"{FormatComparison(candidate.bonusMaxHP - currentHP)}  " +
               $"攻撃 {FormatSigned(candidate.bonusAttack)} " +
               $"{FormatComparison(candidate.bonusAttack - currentAttack)}\n" +
               $"防御 {FormatSigned(candidate.bonusDefense)} " +
               $"{FormatComparison(candidate.bonusDefense - currentDefense)}  " +
               $"速度 {FormatSigned(candidate.bonusAttackSpeed)} " +
               $"{FormatComparison(candidate.bonusAttackSpeed - currentSpeed)}";
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private static string FormatSigned(float value)
    {
        return value >= 0f ? $"+{value:0.00}" : value.ToString("0.00");
    }

    private static string FormatComparison(int difference)
    {
        return FormatComparison((float)difference, "0");
    }

    private static string FormatComparison(float difference)
    {
        return FormatComparison(difference, "0.00");
    }

    private static string FormatComparison(float difference, string format)
    {
        const string IncreaseColor = "#65D88A";
        const string DecreaseColor = "#FF7474";
        const string EqualColor = "#AEB6BE";
        string color = difference > 0f
            ? IncreaseColor
            : difference < 0f
                ? DecreaseColor
                : EqualColor;
        string sign = difference > 0f ? "+" : string.Empty;
        return $"<color={color}>({sign}{difference.ToString(format)})</color>";
    }

    private void EquipSelectedEquipment(ItemDataSO equipment)
    {
        if (selectedDetailMercenary == null ||
            equipment == null ||
            !merchantInventory.HasItem(equipment))
        {
            return;
        }

        EquipmentSlot slot = equipment.equipmentSlot;
        ItemDataSO previousItem =
            selectedDetailMercenary.GetEquippedItem(slot);
        EquipmentInstance previousInstance =
            selectedDetailMercenary.GetEquippedInstance(slot);
        if (!selectedDetailMercenary.EquipEquipment(equipment) ||
            !merchantInventory.TryRemoveItem(equipment))
        {
            if (previousInstance != null)
            {
                selectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousInstance);
            }
            else
            {
                selectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousItem);
            }
            return;
        }

        if (previousInstance != null)
        {
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else if (previousItem != null)
        {
            merchantInventory.AddItem(previousItem);
        }

        statusText.text =
            $"{selectedDetailMercenary.MercenaryName}に" +
            $"{JapaneseDisplayText.GetItemName(equipment)}を装備しました。";
        ShowCharacterDetails(selectedDetailMercenary);
        RebuildCompanyList();
        RebuildPartyList();
        SaveEquipmentChanges();
    }

    private void EquipSelectedEquipment(EquipmentInstance equipment)
    {
        if (selectedDetailMercenary == null ||
            equipment?.BaseItem == null)
        {
            return;
        }

        EquipmentSlot slot = equipment.BaseItem.equipmentSlot;
        ItemDataSO previousItem =
            selectedDetailMercenary.GetEquippedItem(slot);
        EquipmentInstance previousInstance =
            selectedDetailMercenary.GetEquippedInstance(slot);
        if (!selectedDetailMercenary.EquipEquipment(equipment) ||
            !merchantInventory.TryRemoveEquipmentInstance(equipment))
        {
            if (previousInstance != null)
            {
                selectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousInstance);
            }
            else
            {
                selectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousItem);
            }
            return;
        }

        if (previousInstance != null)
        {
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else if (previousItem != null)
        {
            merchantInventory.AddItem(previousItem);
        }

        statusText.text =
            $"{selectedDetailMercenary.MercenaryName}に" +
            $"[{JapaneseDisplayText.GetEquipmentQuality(equipment.Quality)}] " +
            $"{JapaneseDisplayText.GetItemName(equipment.BaseItem)}を装備しました。";
        ShowCharacterDetails(selectedDetailMercenary);
        RebuildCompanyList();
        RebuildPartyList();
        SaveEquipmentChanges();
    }

    private void UnequipSelectedEquipment(EquipmentSlot slot)
    {
        if (selectedDetailMercenary == null ||
            selectedDetailMercenary.GetEquippedItem(slot) == null)
        {
            return;
        }

        EquipmentInstance previousInstance =
            selectedDetailMercenary.GetEquippedInstance(slot);
        if (previousInstance != null)
        {
            selectedDetailMercenary.UnequipEquipmentInstance(slot);
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else
        {
            ItemDataSO previousItem =
                selectedDetailMercenary.UnequipEquipment(slot);
            merchantInventory.AddItem(previousItem);
        }
        statusText.text =
            $"{selectedDetailMercenary.MercenaryName}の" +
            $"{JapaneseDisplayText.GetEquipmentSlot(slot)}を解除しました。";
        ShowCharacterDetails(selectedDetailMercenary);
        RebuildCompanyList();
        RebuildPartyList();
        SaveEquipmentChanges();
    }

    private void SaveEquipmentChanges()
    {
        if (saveManager == null)
        {
            saveManager = GetComponent<SaveManager>() ??
                          FindObjectOfType<SaveManager>();
        }

        if (saveManager == null)
        {
            Debug.LogWarning("装備変更を保存するSaveManagerが見つかりません。", this);
            return;
        }

        saveManager.SaveGame();
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

    private void HandleProgressionChanged()
    {
        RebuildCompanyList();
        RebuildInventoryList();
        if (merchantStatusOverlay != null &&
            merchantStatusOverlay.gameObject.activeSelf)
        {
            RebuildMerchantStatus();
        }
        if (questOverlay != null && questOverlay.gameObject.activeSelf)
        {
            RebuildQuestList();
        }
        RefreshUI();
    }

    private void IncreaseMerchantSkill(MerchantSkillType skill)
    {
        if (merchantData.TryIncreaseSkill(skill))
        {
            statusText.text =
                $"商人技能を強化しました。残りポイント " +
                $"{merchantData.MerchantSkillPoints}";
        }
        RebuildMerchantStatus();
        RefreshUI();
    }

    private void SellEquipment(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        int sellPrice = merchantInventory.GetSellPrice(equipment);
        string itemName = JapaneseDisplayText.GetItemName(equipment.BaseItem);
        if (!merchantInventory.SellEquipmentInstance(equipment))
        {
            statusText.text = $"{itemName}を売却できませんでした。";
            return;
        }

        statusText.text = $"{itemName}を{sellPrice} Gで売却しました。";
        RebuildInventoryList();
        RefreshUI();
    }

    private void ShowEquipmentDetails(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null || equipmentDetailOverlay == null)
        {
            return;
        }

        selectedEquipmentDetail = equipment;
        ItemDataSO item = equipment.BaseItem;
        string quality = JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        equipmentDetailTitle.text =
            $"[{quality}] {GetEquipmentDisplayName(equipment)}";
        equipmentDetailTitle.color = GetEquipmentQualityColor(equipment.Quality);

        List<string> modifierLines = new List<string>();
        foreach (EquipmentModifier modifier in equipment.Modifiers)
        {
            if (modifier != null)
            {
                modifierLines.Add(
                    $"{JapaneseDisplayText.GetEquipmentModifier(modifier.type)} " +
                    $"{FormatSigned(modifier.value)}");
            }
        }

        string modifiers = modifierLines.Count > 0
            ? string.Join("\n", modifierLines)
            : "追加効果なし";
        string setText = BuildEquipmentSetDetail(item.equipmentSet);
        string target = item.allClassesCanEquip
            ? "全職業"
            : JapaneseDisplayText.GetMercenaryClass(item.requiredClass);
        ItemDataSO enhancementMaterial =
            merchantInventory.GetEnhancementMaterial(equipment);
        string enhancementMaterialName = enhancementMaterial != null
            ? JapaneseDisplayText.GetItemName(enhancementMaterial)
            : "対応する強化鉱石";

        equipmentDetailText.text =
            $"種類: {JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}\n" +
            $"装備対象: {target}  ランク: {item.equipmentRank}\n" +
            $"品質: {quality}  強化: +{equipment.EnhancementLevel} / +10\n\n" +
            $"最終性能\n" +
            $"HP {FormatSigned(equipment.BonusMaxHP)}  " +
            $"攻撃 {FormatSigned(equipment.BonusAttack)}\n" +
            $"防御 {FormatSigned(equipment.BonusDefense)}  " +
            $"攻撃速度 {FormatSigned(equipment.BonusAttackSpeed)}\n\n" +
            $"追加効果\n{modifiers}\n\n{setText}\n\n" +
            $"次回強化: 成功率 " +
            $"{equipment.GetEnhancementSuccessRate() * 100f:0}%  " +
            $"{enhancementMaterialName} " +
            $"{equipment.GetEnhancementMaterialAmount()}個";

        bool canEnhance = equipment.EnhancementLevel < 10;
        equipmentEnhanceButton.interactable =
            canEnhance &&
            merchantData.CanPay(equipment.GetEnhancementCost()) &&
            enhancementMaterial != null &&
            merchantInventory.HasItem(
                enhancementMaterial,
                equipment.GetEnhancementMaterialAmount());
        equipmentEnhanceButton.GetComponentInChildren<Text>().text =
            canEnhance
                ? $"強化 {equipment.GetEnhancementCost()}G"
                : "強化完了";
        equipmentSellButton.interactable =
            IsEquipmentInInventory(equipment) && !equipment.IsLocked;
        equipmentSellButton.GetComponentInChildren<Text>().text =
            $"売却 {merchantInventory.GetSellPrice(equipment)}G";
        equipmentLockButton.GetComponentInChildren<Text>().text =
            equipment.IsLocked ? "ロック解除" : "ロック";

        equipmentDetailOverlay.SetAsLastSibling();
        equipmentDetailOverlay.gameObject.SetActive(true);
    }

    private static string BuildEquipmentSetDetail(EquipmentSetId setId)
    {
        if (setId == EquipmentSetId.None)
        {
            return "セット効果: なし";
        }

        switch (setId)
        {
            case EquipmentSetId.Vanguard:
                return "セット: 不屈の前衛\n" +
                       "2部位: 最大HP+20、防御+10\n" +
                       "3部位: 攻撃+8";
            case EquipmentSetId.Windstalker:
                return "セット: 風狩り\n" +
                       "2部位: 攻撃+5、攻撃速度+0.08\n" +
                       "3部位: 攻撃+10、攻撃速度+0.06";
            case EquipmentSetId.ArcaneSage:
                return "セット: 秘術賢者\n" +
                       "2部位: 攻撃+10\n" +
                       "3部位: 攻撃+15、攻撃速度+0.04";
            default:
                return "セット: 古代守護者\n" +
                       "2部位: 最大HP+30、防御+8\n" +
                       "3部位: 攻撃+12、攻撃速度+0.08";
        }
    }

    private bool IsEquipmentInInventory(EquipmentInstance equipment)
    {
        foreach (EquipmentInstance owned in merchantInventory.EquipmentInstances)
        {
            if (ReferenceEquals(owned, equipment))
            {
                return true;
            }
        }
        return false;
    }

    private void HideEquipmentDetails()
    {
        equipmentDetailOverlay?.gameObject.SetActive(false);
        selectedEquipmentDetail = null;
    }

    private void EnhanceSelectedEquipment()
    {
        EquipmentInstance equipment = selectedEquipmentDetail;
        if (equipment == null)
        {
            return;
        }

        EquipmentEnhancementResult result =
            merchantInventory.TryEnhanceEquipment(equipment);
        switch (result)
        {
            case EquipmentEnhancementResult.Succeeded:
                statusText.text =
                    $"{GetEquipmentDisplayName(equipment)}の強化に成功しました。";
                break;
            case EquipmentEnhancementResult.Failed:
                statusText.text =
                    "強化に失敗しました。装備と強化値は維持されます。";
                break;
            case EquipmentEnhancementResult.NotEnoughMaterial:
                statusText.text = "強化鉱石が不足しています。";
                break;
            case EquipmentEnhancementResult.NotEnoughGold:
                statusText.text = "ゴールドが不足しています。";
                break;
            default:
                statusText.text = "装備を強化できませんでした。";
                break;
        }
        RebuildInventoryList();
        if (selectedDetailMercenary != null)
        {
            ShowCharacterDetails(selectedDetailMercenary);
        }
        ShowEquipmentDetails(equipment);
        RefreshUI();
        SaveEquipmentChanges();
    }

    private void ToggleSelectedEquipmentLock()
    {
        if (selectedEquipmentDetail == null)
        {
            return;
        }

        merchantInventory.ToggleEquipmentLock(selectedEquipmentDetail);
        statusText.text = selectedEquipmentDetail.IsLocked
            ? "装備をロックしました。"
            : "装備のロックを解除しました。";
        RebuildInventoryList();
        ShowEquipmentDetails(selectedEquipmentDetail);
        SaveEquipmentChanges();
    }

    private void SellSelectedEquipment()
    {
        EquipmentInstance equipment = selectedEquipmentDetail;
        if (equipment == null || !IsEquipmentInInventory(equipment))
        {
            return;
        }

        HideEquipmentDetails();
        SellEquipment(equipment);
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

    private void CraftEquipment(EquipmentRecipeSO recipe)
    {
        if (recipe == null || recipe.resultItem == null)
        {
            return;
        }

        string itemName = JapaneseDisplayText.GetItemName(recipe.resultItem);
        if (!blacksmithManager.TryCraft(recipe))
        {
            statusText.text = $"{itemName}を制作できませんでした。";
            RebuildBlacksmithList();
            RefreshUI();
            return;
        }

        EquipmentInstance crafted = blacksmithManager.LastCraftedEquipment;
        statusText.text = crafted != null
            ? $"[{JapaneseDisplayText.GetEquipmentQuality(crafted.Quality)}] " +
              $"{itemName}を制作しました。"
            : $"{itemName}を制作しました。";
        RebuildBlacksmithList();
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
        HideMapPages();
        hirePage.gameObject.SetActive(true);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, true);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
        statusText.text = "雇用する傭兵を選択してください。";
    }

    private void ShowCompanyPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(true);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, true);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildCompanyList();
        statusText.text =
            $"商人Lv{merchantData.MerchantLevel} " +
            $"EXP {merchantData.MerchantExperience}/{merchantData.ExperienceToNextLevel}  |  " +
            $"技能ポイント {merchantData.MerchantSkillPoints}  |  " +
            $"傭兵 {hireManager.HiredMercenaries.Count}人  |  " +
            $"雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}%";
    }

    private void ShowPartyPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(true);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, true);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildPartyList();
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void ShowHealPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(true);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, true);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildHealList();
        statusText.text =
            $"治療費: 失ったHP 1につき {healingManager.HealCostPerHP} G";
    }

    private void ShowBattlePage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(true);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, true);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !battleManager.IsBattling;
        statusText.text = $"戦闘参加: 傭兵{partyManager.Members.Count}人";
    }

    private void ShowDungeonPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(true);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, true);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
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
                  BuildDungeonRewardPreview(
                      dungeonRunManager.SelectedDungeon);
        }

        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();
        statusText.text = $"探索パーティー: 傭兵{partyManager.Members.Count}人";
        RefreshUI();
    }

    private void ShowMarketPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(true);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, true);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, false);
        RebuildMarketList();
        statusText.text =
            $"仕入れ商品: {marketStockManager.Stock.Count}種類 / {marketPriceManager.GetMarketSummary()}";
    }

    private void ShowBlacksmithPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(true);
        inventoryPage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, true);
        SetTabActive(inventoryTabButton, false);
        RebuildBlacksmithList();
        statusText.text = $"鍛冶レシピ: {blacksmithManager.Recipes.Count}種類";
        RefreshUI();
    }

    private void ShowInventoryPage()
    {
        HideMapPages();
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(true);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(healTabButton, false);
        SetTabActive(battleTabButton, false);
        SetTabActive(dungeonTabButton, false);
        SetTabActive(marketTabButton, false);
        SetTabActive(blacksmithTabButton, false);
        SetTabActive(inventoryTabButton, true);
        RebuildInventoryList();
        statusText.text =
            $"倉庫 {merchantInventory.GetUsedStorageSlots()}/" +
            $"{(progressionManager != null ? progressionManager.StorageCapacity : 0)}  |  " +
            $"{marketPriceManager.GetMarketSummary()}  |  " +
            $"維持費 {(progressionManager != null ? progressionManager.StorageMaintenanceCost : 0)}G/日";
    }

    private void ShowGlobalMap()
    {
        HideStandardPages();
        globalMapPage.gameObject.SetActive(true);
        worldMapPage.gameObject.SetActive(false);
        townMapPage.gameObject.SetActive(false);
        SetAllTabsInactive();
        SetMapHeaderButtons(false);
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
        HideStandardPages();
        globalMapPage.gameObject.SetActive(false);
        worldMapPage.gameObject.SetActive(true);
        townMapPage.gameObject.SetActive(false);
        SetAllTabsInactive();
        SetMapHeaderButtons(false);
        RefreshTownMapButtons();
        statusText.text =
            $"現在地: {TownNames[currentTownIndex]}  |  移動先の町を選択";
    }

    private void ShowTownMap()
    {
        HideStandardPages();
        globalMapPage.gameObject.SetActive(false);
        worldMapPage.gameObject.SetActive(false);
        townMapPage.gameObject.SetActive(true);
        SetAllTabsInactive();
        SetMapHeaderButtons(false);
        statusText.text =
            $"{TownNames[currentTownIndex]}  |  利用する施設を選択";
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
            $"・街道戦闘が発生します\n・勝利すると1日経過します" +
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

        ResetBattleLog();
        pendingTravelTownIndex = destinationTownIndex;
        pendingTravelWasUnlock =
            !unlockedTownIndices.Contains(destinationTownIndex);
        pendingOpenDungeonAfterTravel = openDungeonAfterTravel;
        int enemyCount = 2 + Mathf.Abs(destinationTownIndex - currentTownIndex);
        List<EnemyDataSO> enemies =
            battleManager.CreateDefaultEnemyEncounter(enemyCount);

        if (!battleManager.StartBattle(partyManager.Members, enemies))
        {
            pendingTravelTownIndex = -1;
            pendingTravelWasUnlock = false;
            pendingOpenDungeonAfterTravel = false;
            statusText.text = "街道戦闘を開始できませんでした。";
            return;
        }

        ShowBattlePage();
        statusText.text =
            $"町の移動: {TownNames[destinationTownIndex]}への街道を突破してください。";
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

    private void HideMapPages()
    {
        globalMapPage?.gameObject.SetActive(false);
        worldMapPage?.gameObject.SetActive(false);
        townMapPage?.gameObject.SetActive(false);
        SetMapHeaderButtons(true);
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

    private void HideStandardPages()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        healPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        dungeonPage.gameObject.SetActive(false);
        marketPage.gameObject.SetActive(false);
        blacksmithPage.gameObject.SetActive(false);
        inventoryPage.gameObject.SetActive(false);
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
        goldText.text =
            $"商人Lv{merchantData.MerchantLevel}  所持金 {merchantData.Gold} G";
        if (dayText != null)
        {
            dayText.text = $"{dayManager.CurrentDay}日目";
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
            startDungeonButton.gameObject.SetActive(!dungeonRunManager.IsRunning);
            startDungeonButton.interactable =
                partyManager.Members.Count > 0 &&
                !battleManager.IsBattling &&
                !dungeonRunManager.IsRunning;
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

    private void CycleInventoryFilter()
    {
        inventoryFilter = (InventoryFilter)(
            ((int)inventoryFilter + 1) %
            System.Enum.GetValues(typeof(InventoryFilter)).Length);
        inventoryFilterButton.GetComponentInChildren<Text>().text =
            $"絞込: {GetInventoryFilterLabel(inventoryFilter)}";
        RebuildInventoryList();
    }

    private void CycleEquipmentSort()
    {
        equipmentSort = (EquipmentSort)(
            ((int)equipmentSort + 1) %
            System.Enum.GetValues(typeof(EquipmentSort)).Length);
        equipmentSortButton.GetComponentInChildren<Text>().text =
            $"並替: {GetEquipmentSortLabel(equipmentSort)}";
        RebuildInventoryList();
    }

    private bool MatchesInventoryFilter(ItemDataSO item)
    {
        if (item == null)
        {
            return false;
        }

        switch (inventoryFilter)
        {
            case InventoryFilter.Material:
                return !item.IsEquipment;
            case InventoryFilter.Weapon:
                return item.IsEquipment &&
                       item.equipmentSlot == EquipmentSlot.Weapon;
            case InventoryFilter.Armor:
                return item.IsEquipment &&
                       item.equipmentSlot == EquipmentSlot.Armor;
            case InventoryFilter.Accessory:
                return item.IsEquipment &&
                       item.equipmentSlot == EquipmentSlot.Accessory;
            case InventoryFilter.SetEquipment:
                return item.IsEquipment &&
                       item.equipmentSet != EquipmentSetId.None;
            default:
                return true;
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
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                string state = i == currentTownIndex
                    ? "\n【現在地】"
                    : unlocked
                        ? string.Empty
                        : "\n【未解放】";
                label.text = TownNames[i] + state;
                label.color = unlocked
                    ? Color.white
                    : new Color(0.38f, 0.4f, 0.42f, 1f);
            }

            button.targetGraphic.color = unlocked
                ? new Color(0.04f, 0.05f, 0.06f, 0.76f)
                : new Color(0.005f, 0.005f, 0.008f, 0.97f);

            RawImage[] markerImages = button.GetComponentsInChildren<RawImage>();
            foreach (RawImage markerImage in markerImages)
            {
                markerImage.color = unlocked
                    ? Color.white
                    : new Color(0.035f, 0.035f, 0.04f, 1f);
            }
        }
    }

    private int GetNextUnlockableTownIndex()
    {
        int leftmostUnlockedTownIndex = TownNames.Length - 1;
        foreach (int unlockedTownIndex in unlockedTownIndices)
        {
            leftmostUnlockedTownIndex =
                Mathf.Min(leftmostUnlockedTownIndex, unlockedTownIndex);
        }

        return leftmostUnlockedTownIndex > 0
            ? leftmostUnlockedTownIndex - 1
            : -1;
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

    private int CompareEquipment(
        EquipmentInstance left,
        EquipmentInstance right)
    {
        if (left?.BaseItem == null) return 1;
        if (right?.BaseItem == null) return -1;

        switch (equipmentSort)
        {
            case EquipmentSort.Quality:
                return right.Quality.CompareTo(left.Quality);
            case EquipmentSort.Enhancement:
                return right.EnhancementLevel.CompareTo(left.EnhancementLevel);
            case EquipmentSort.Set:
                return left.BaseItem.equipmentSet.CompareTo(
                    right.BaseItem.equipmentSet);
            default:
                return string.Compare(
                    JapaneseDisplayText.GetItemName(left.BaseItem),
                    JapaneseDisplayText.GetItemName(right.BaseItem),
                    System.StringComparison.Ordinal);
        }
    }

    private static string GetInventoryFilterLabel(InventoryFilter filter)
    {
        switch (filter)
        {
            case InventoryFilter.Material: return "素材";
            case InventoryFilter.Weapon: return "武器";
            case InventoryFilter.Armor: return "防具";
            case InventoryFilter.Accessory: return "装飾品";
            case InventoryFilter.SetEquipment: return "セット";
            case InventoryFilter.Locked: return "ロック";
            default: return "全て";
        }
    }

    private static string GetEquipmentSortLabel(EquipmentSort sort)
    {
        switch (sort)
        {
            case EquipmentSort.Quality: return "品質";
            case EquipmentSort.Enhancement: return "強化";
            case EquipmentSort.Set: return "セット";
            default: return "名前";
        }
    }

    private void ShowEquipmentCollection()
    {
        List<ItemDataSO> equipmentItems = FindAllEquipmentAssets();
        equipmentItems.Sort((left, right) =>
            string.Compare(
                JapaneseDisplayText.GetItemName(left),
                JapaneseDisplayText.GetItemName(right),
                System.StringComparison.Ordinal));

        List<string> lines = new List<string>();
        int discoveredCount = 0;
        foreach (ItemDataSO item in equipmentItems)
        {
            bool discovered = merchantInventory.HasDiscoveredEquipment(item);
            if (discovered)
            {
                discoveredCount++;
            }

            string name = discovered
                ? JapaneseDisplayText.GetItemName(item)
                : "？？？？？？";
            string set = item.equipmentSet != EquipmentSetId.None
                ? $" / {JapaneseDisplayText.GetEquipmentSet(item.equipmentSet)}"
                : string.Empty;
            string source = item.acquisitionType == ItemAcquisitionType.Dungeon
                ? "ダンジョン限定"
                : item.acquisitionType == ItemAcquisitionType.Blacksmith
                    ? "鍛冶屋"
                    : "市場";
            lines.Add(
                $"{(discovered ? "●" : "○")} [{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}] " +
                $"{name}{set} / {source}");
        }

        equipmentCollectionText.text =
            $"収集率 {discoveredCount}/{equipmentItems.Count}\n\n" +
            string.Join("\n", lines);
        float height = Mathf.Max(430f, 76f + lines.Count * 28f);
        equipmentCollectionContent.sizeDelta = new Vector2(0f, height);
        equipmentCollectionText.rectTransform.anchorMin = Vector2.zero;
        equipmentCollectionText.rectTransform.anchorMax = Vector2.one;
        equipmentCollectionText.rectTransform.offsetMin = new Vector2(12f, 12f);
        equipmentCollectionText.rectTransform.offsetMax = new Vector2(-12f, -12f);
        equipmentCollectionOverlay.SetAsLastSibling();
        equipmentCollectionOverlay.gameObject.SetActive(true);
    }

    private void HideEquipmentCollection()
    {
        equipmentCollectionOverlay?.gameObject.SetActive(false);
    }

    private void ShowQuestOverlay()
    {
        RebuildQuestList();
        questOverlay.SetAsLastSibling();
        questOverlay.gameObject.SetActive(true);
    }

    private void HideQuestOverlay()
    {
        questOverlay?.gameObject.SetActive(false);
    }

    private void ShowMerchantStatusOverlay()
    {
        RebuildMerchantStatus();
        merchantStatusOverlay.SetAsLastSibling();
        merchantStatusOverlay.gameObject.SetActive(true);
    }

    private void HideMerchantStatusOverlay()
    {
        merchantStatusOverlay?.gameObject.SetActive(false);
    }

    private void RebuildMerchantStatus()
    {
        if (merchantSkillList == null || merchantData == null)
        {
            return;
        }

        ClearChildren(merchantSkillList);
        float top = 0f;

        RectTransform summaryRow =
            CreateRow("Merchant Summary", merchantSkillList, top);
        CreateText(
            summaryRow,
            $"商人Lv {merchantData.MerchantLevel}  " +
            $"EXP {merchantData.MerchantExperience}/" +
            $"{merchantData.ExperienceToNextLevel}  " +
            $"所持金 {merchantData.Gold}G\n" +
            $"未使用技能ポイント {merchantData.MerchantSkillPoints}  |  " +
            $"習得技能: {merchantData.GetUnlockedMerchantSkills()}",
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -78f),
            new Vector2(-16f, -12f),
            Color.white);
        top -= 112f;

        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Negotiation,
            "交渉",
            $"仕入れ {merchantData.GetMarketBuyMultiplier() * 100f:0}% / " +
            $"売却 {merchantData.GetMarketSellMultiplier() * 100f:0}%\n" +
            "Lv3 値切り術 / Lv7 商談の達人",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Leadership,
            "統率",
            $"雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}% / " +
            $"契約更新費 {merchantData.GetRenewalCostMultiplier() * 100f:0}%\n" +
            "Lv3 人を見る目 / Lv7 契約管理",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Appraisal,
            "鑑定",
            $"依頼ゴールド {merchantData.GetQuestGoldMultiplier() * 100f:0}% / " +
            $"依頼EXP {merchantData.GetQuestExperienceMultiplier() * 100f:0}%\n" +
            "Lv3 目利き / Lv7 慧眼",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Logistics,
            "兵站",
            $"倉庫容量 +{merchantData.GetStorageCapacityBonus()} / " +
            $"探索費用 {merchantData.GetExplorationExpenseMultiplier() * 100f:0}%\n" +
            "Lv3 荷役整理 / Lv7 遠征計画",
            top);
        top -= 112f;

        merchantSkillList.sizeDelta =
            new Vector2(0f, Mathf.Max(470f, -top));
    }

    private void RebuildQuestList()
    {
        if (questList == null || progressionManager == null)
        {
            return;
        }

        ClearChildren(questList);
        CreateText(
            questList,
            "長期目標\n" + progressionManager.GetAchievementSummary(),
            14,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(12f, -120f),
            new Vector2(-12f, 0f),
            MutedTextColor);
        float top = -132f;
        for (int i = 0; i < progressionManager.Quests.Count; i++)
        {
            int index = i;
            QuestRecord quest = progressionManager.Quests[i];
            RectTransform row = CreateRow($"Quest {i}", questList, top);
            string type = quest.isSpecial ? "特殊" : "通常";
            string state = quest.completed
                ? "達成済み"
                : quest.expired
                    ? "期限切れ"
                    : quest.accepted ? "進行中" : "未受注";
            CreateText(
                row,
                $"[{type}] {quest.title}  {state}",
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -40f),
                new Vector2(-150f, -10f),
                Color.white);
            string target = quest.questType == QuestType.ItemDelivery
                ? JapaneseDisplayText.GetItemNameByRawName(quest.targetName)
                : JapaneseDisplayText.GetEnemyName(quest.targetName);
            CreateText(
                row,
                $"{target} {quest.currentAmount}/{quest.requiredAmount}  " +
                $"期限 {quest.deadlineDay}日  報酬 " +
                $"{progressionManager.GetQuestGoldReward(quest)}G / " +
                $"商人EXP {progressionManager.GetQuestExperienceReward(quest)}",
                13,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -76f),
                new Vector2(-150f, -48f),
                MutedTextColor);
            Button button = CreateActionButton(
                row,
                quest.accepted ? state : "受注",
                () => AcceptQuest(index));
            button.interactable =
                !quest.accepted && !quest.completed && !quest.expired;
            top -= 112f;
        }
        questList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -top));
    }

    private void AcceptQuest(int index)
    {
        if (progressionManager.AcceptQuest(index))
        {
            statusText.text = "依頼を受注しました。";
        }
        RebuildQuestList();
        RefreshUI();
    }

    private void UpgradeStorage()
    {
        if (progressionManager != null &&
            progressionManager.TryUpgradeStorage())
        {
            statusText.text = "倉庫を拡張しました。";
        }
        else
        {
            statusText.text = "商人レベルまたはゴールドが不足しています。";
        }
        RefreshUI();
    }

    private void RenewContract(MercenaryInstance mercenary)
    {
        if (hireManager.TryRenewContract(mercenary))
        {
            statusText.text = $"{mercenary.MercenaryName}の契約を更新しました。";
        }
        else
        {
            statusText.text = "契約を更新できませんでした。";
        }
        RebuildCompanyList();
        RefreshUI();
    }

    private static string BuildMercenarySkillSummary(MercenaryInstance mercenary)
    {
        List<string> skills = new List<string>();
        switch (mercenary.MercenaryClass)
        {
            case MercenaryClass.Warrior:
                skills.Add("戦闘スキル: 挑発の一撃（魔力35）");
                break;
            case MercenaryClass.Archer:
                skills.Add("戦闘スキル: 連射（魔力45）");
                break;
            case MercenaryClass.Mage:
                skills.Add("戦闘スキル: 火球（魔力50）");
                break;
        }

        if (mercenary.Level >= 2)
        {
            switch (mercenary.MercenaryClass)
            {
                case MercenaryClass.Warrior:
                    skills.Add("基礎体力: HP+10、防御+3");
                    break;
                case MercenaryClass.Archer:
                    skills.Add("速射訓練: 攻撃速度+0.05");
                    break;
                case MercenaryClass.Mage:
                    skills.Add("魔力集中: 攻撃+4");
                    break;
            }
        }
        if (mercenary.IsUnique &&
            mercenary.Level >=
            Mathf.Max(1, mercenary.BaseData.uniqueSkillUnlockLevel))
        {
            MercenaryDataSO data = mercenary.BaseData;
            skills.Add(
                $"{data.uniqueSkillName}: HP+{data.uniqueSkillBonusMaxHP}、" +
                $"攻撃+{data.uniqueSkillBonusAttack}、" +
                $"防御+{data.uniqueSkillBonusDefense}、" +
                $"魔力+{data.uniqueSkillBonusMaxMagicPower}、" +
                $"速度+{data.uniqueSkillBonusAttackSpeed:0.00}");
        }
        return skills.Count > 0
            ? string.Join(" / ", skills)
            : "スキル未設定";
    }

    private MercenaryContractType GetUnlockedContractType()
    {
        return hireManager.SelectedContract;
    }

    private void CycleHireContract()
    {
        MercenaryContractType selected =
            hireManager.CycleSelectedContract();
        contractSelectButton.GetComponentInChildren<Text>().text =
            $"契約: {JapaneseDisplayText.GetContractType(selected)}";
        RebuildHireList();
    }

    private static List<ItemDataSO> FindAllEquipmentAssets()
    {
        List<ItemDataSO> results =
            new List<ItemDataSO>(Resources.LoadAll<ItemDataSO>(string.Empty));
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:ItemDataSO",
            new[] { "Assets/Proiject/ScriptableObjects/Items" });
        foreach (string guid in guids)
        {
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (item != null && !results.Contains(item))
            {
                results.Add(item);
            }
        }
#endif
        results.RemoveAll(item => item == null || !item.IsEquipment);
        return results;
    }
}

public enum InventoryFilter
{
    All,
    Material,
    Weapon,
    Armor,
    Accessory,
    SetEquipment,
    Locked
}

public enum EquipmentSort
{
    Name,
    Quality,
    Enhancement,
    Set
}
