using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI : MonoBehaviour
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
    [SerializeField] private DebtManager debtManager;
    [SerializeField] private RoadEncounterService roadEncounterService;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private StoryProgressManager storyProgressManager;
    [SerializeField] private TransportManager transportManager;
    [SerializeField] private DungeonExpeditionManager dungeonExpeditionManager;
    [SerializeField] private RemoteSaleManager remoteSaleManager;
    [SerializeField] private TrainingGroundManager trainingGroundManager;

    [Header("UI Prefab")]
    [SerializeField] private SimpleMercenaryHireUIView uiViewPrefab;

    [Header("Hire Candidates")]
    [SerializeField] private List<MercenaryDataSO> candidates = new List<MercenaryDataSO>();

    private readonly List<Button> townMapButtons = new List<Button>();
    private readonly List<RectTransform> regionMapPages =
        new List<RectTransform>();
    private readonly List<Button> standardTownFacilityButtons =
        new List<Button>();

    private RectTransform guildPanel;
    private RectTransform overlayRoot;
    private SimpleMercenaryHireUIView activeView;
    private UIPageRouter pageRouter;
    private RectTransform characterDetailOverlay;
    private Text characterDetailTitle;
    private Text characterDetailText;
    private RectTransform characterStatusPage;
    private RectTransform characterEquipmentPage;
    private RectTransform characterSkillList;
    private Text characterSkillDetailText;
    private Button characterStatusTabButton;
    private Button characterEquipmentTabButton;
    private RectTransform characterEquipmentList;
    private ScrollRect characterEquipmentScrollRect;
    private bool showingCharacterStatusPage = true;
    private RectTransform equipmentDetailOverlay;
    private Text equipmentDetailTitle;
    private Text equipmentDetailText;
    private Button equipmentEnhanceButton;
    private Button equipmentSellButton;
    private Button equipmentLockButton;
    private RectTransform questOverlay;
    private RectTransform questList;
    private RectTransform questDetailWindow;
    private RectTransform merchantStatusOverlay;
    private RectTransform merchantSkillList;
    private RectTransform equipmentCollectionOverlay;
    private BookPageUI equipmentCodexBook;
    private EquipmentSpecialCodexPageUI equipmentSpecialCodexPage;
    private RectTransform equipmentCodexNormalRoot;
    private RectTransform equipmentCodexSpecialRoot;
    private Button equipmentCodexNormalTabButton;
    private Button equipmentCodexSpecialTabButton;
    private RectTransform monsterCollectionOverlay;
    private BookPageUI monsterCodexBook;
    private RectTransform travelConfirmationOverlay;
    private RectTransform globalMenuOverlay;
    private RectTransform dailyResultOverlay;
    private RectTransform dailyResultContent;
    private Text dailyResultText;
    private RectTransform tutorialOverlay;
    private RectTransform remoteSaleOverlay;
    private RectTransform remoteSaleContent;
    private Text tutorialStepText;
    private Text tutorialTitleText;
    private Text tutorialBodyText;
    private Button tutorialBackButton;
    private Button tutorialNextButton;
    private Button tutorialCloseButton;
    private Button globalMenuButton;
    private Text travelConfirmationText;
    private RectTransform hirePage;
    private RectTransform globalMapPage;
    private RectTransform worldMapPage;
    private RectTransform townMapPage;
    private RectTransform hireList;
    private RectTransform companyPage;
    private RectTransform partyPage;
    private RectTransform healPage;
    private RectTransform battlePage;
    private RectTransform roadBattlePage;
    private Text roadBattleRouteText;
    private Button roadContinueButton;
    private Button roadRetreatButton;
    private RectTransform battleLogPanel;
    private RectTransform dungeonPage;
    private RectTransform marketPage;
    private RectTransform blacksmithPage;
    private RectTransform inventoryPage;
    private RectTransform jobChangePage;
    private RectTransform trainingGroundPage;
    private RectTransform jobChangeList;
    private Button jobFacilityButton;
    private Button trainingGroundFacilityButton;
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
    private Button hireFacilityButton;
    private Button hiddenIslandRegionButton;
    private Button startBattleButton;
    private Button battleSpeedButton;
    private Button battlePauseButton;
    private Button battleSkipButton;
    private Button roadSpeedButton;
    private Button roadPauseButton;
    private Button roadSkipButton;
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
    private Text battlePageTitleText;
    private Text battleEncounterText;
    private Text dungeonStatusText;
    private Text dungeonEventTitleText;
    private Text dungeonEventDescriptionText;
    private Text dungeonEventPreviewText;
    private RectTransform dungeonEventPanel;
    private RectTransform dungeonResultPanel;
    private Text dungeonResultText;
    private Button dungeonNextFloorButton;
    private Text marketInfoText;
    private Text storageCapacityText;
    private RectTransform storageUpgradeConfirmationOverlay;
    private Text storageUpgradeConfirmationText;
    private Text storageUpgradeConfirmationReasonText;
    private Button storageUpgradeConfirmButton;
    private RectTransform itemDetailOverlay;
    private Image itemDetailImage;
    private Text itemDetailImagePlaceholder;
    private Text itemDetailTitle;
    private Text itemDetailText;
    private Text itemDetailTransactionText;
    private Button itemDetailActionButton;
    private System.Action itemDetailAction;
    private RectTransform promotionPreviewOverlay;
    private Text promotionPreviewText;
    private Text promotionPreviewReasonText;
    private Button promotionPreviewConfirmButton;
    private MercenaryInstance promotionPreviewMercenary;
    private MercenaryClass promotionPreviewTarget;
    private RemoteSaleController remoteSaleController;
    private Font uiFont;
    private Font uiBodyFont;
    private SimpleMercenaryHireUIFactory uiFactory;
    private RectTransform battleLogContent;
    private RectTransform battleLogViewport;
    private ScrollRect battleLogScrollRect;
    private Coroutine battleLogScrollCoroutine;
    private BattleVisualController battleVisualController;
    private bool hasPendingDungeonCompletion;
    private bool pendingDungeonCompletionCleared;
    private Coroutine pendingDungeonCompletionCoroutine;
    private Coroutine dungeonEventPresentationCoroutine;
    private bool hasPendingDailyResult;
    private int pendingDailyResultDay;
    private DailyResultController dailyResultController;
    private HireAndPartyController hireAndPartyController;
    private EconomyController economyController;
    private CharacterEquipmentController characterEquipmentController;
    private MerchantStatusAndQuestController merchantStatusAndQuestController;
    private TownTravelController townTravelController;
    private DungeonBattleController dungeonBattleController;
    private TutorialController tutorialController;
    private AudioFeedbackService audioFeedbackService;
    private TransportController transportController;
    private FacilityGreetingController facilityGreetingController;
    private RectTransform facilityGreetingOverlay;
    private Text facilityGreetingTitle;
    private Text facilityGreetingDialogue;
    private Image facilityGreetingPortrait;
    private string pendingFacilityKey;
    private System.Action pendingFacilityDestination;
    private RectTransform transportOverlay;
    private RectTransform transportContent;
    private Text transportFooterText;
    private ExpeditionController expeditionController;
    private RectTransform expeditionOverlay;
    private RectTransform expeditionContent;

    // Aliases into the shared palette (UITheme) so the many partial files
    // of this class can keep their existing short references.
    private static readonly Color BackgroundColor = UITheme.BackgroundColor;
    private static readonly Color PanelColor = UITheme.PanelColor;
    private static readonly Color RowColor = UITheme.RowColor;
    private static readonly Color AccentColor = UITheme.AccentColor;
    private static readonly Color InactiveColor = UITheme.InactiveColor;
    private static readonly Color WoodButtonColor = UITheme.WoodButtonColor;
    private static readonly Color ImportantButtonColor = UITheme.ImportantButtonColor;
    private static readonly Color FrameColor = UITheme.FrameColor;
    private static readonly Color ButtonTextColor = UITheme.ButtonTextColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;

    private void Start()
    {
        ResolveReferences();
        SyncDungeonUnlocks();

        if (!HasRequiredReferences())
        {
            return;
        }

        uiFont = LoadUIFont();
        uiBodyFont = LoadBodyFont();
        uiFactory = new SimpleMercenaryHireUIFactory(uiFont, uiBodyFont);
        dailyResultController = new DailyResultController(
            merchantData,
            hireManager,
            partyManager,
            merchantInventory,
            progressionManager,
            CharacterEquipmentController.GetEquipmentDisplayName);
        hireAndPartyController = new HireAndPartyController(
            hireManager,
            partyManager,
            mercenaryGenerator,
            merchantInventory,
            healingManager,
            townProgressState,
            saveManager,
            transportManager,
            message => statusText.text = message,
            () => RefreshPage(hirePage),
            () => RefreshPage(companyPage),
            () => RefreshPage(partyPage),
            () => RefreshPage(healPage),
            () => RefreshPage(jobChangePage),
            RefreshUI,
            label => contractSelectButton.GetComponentInChildren<Text>().text = label);
        economyController = new EconomyController(
            merchantInventory,
            marketStockManager,
            blacksmithManager,
            message => statusText.text = message,
            () => RefreshPage(inventoryPage),
            () => RefreshPage(marketPage),
            () => RefreshPage(blacksmithPage),
            RefreshUI,
            label => inventoryFilterButton.GetComponentInChildren<Text>().text = label,
            label => equipmentSortButton.GetComponentInChildren<Text>().text = label);
        transportController = new TransportController(
            transportManager, merchantInventory, hireManager, partyManager,
            townProgressState, marketPriceManager,
            message => statusText.text = message,
            RefreshTransportOverlay);
        remoteSaleController = new RemoteSaleController(
            remoteSaleManager,
            merchantInventory,
            townProgressState,
            message => statusText.text = message,
            RefreshRemoteSaleOverlay);
        facilityGreetingController = new FacilityGreetingController();
        expeditionController = new ExpeditionController(
            dungeonExpeditionManager, dungeonRunManager, hireManager,
            partyManager, transportManager, message => statusText.text = message,
            RefreshExpeditionOverlay);
        characterEquipmentController = new CharacterEquipmentController(
            merchantData,
            merchantInventory,
            hireManager,
            battleManager,
            economyController,
            this,
            message => statusText.text = message,
            (title, body) =>
            {
                if (characterDetailText == null)
                {
                    return;
                }
                characterDetailTitle.text = title;
                characterDetailText.text = body;
            },
            ShowCharacterDetails,
            () => RefreshPage(companyPage),
            () => RefreshPage(partyPage),
            () => RefreshPage(inventoryPage),
            RefreshUI,
            SaveEquipmentChanges,
            () => saveManager?.SaveGame());
        merchantStatusAndQuestController = new MerchantStatusAndQuestController(
            merchantData,
            progressionManager,
            debtManager,
            hireManager,
            message => statusText.text = message,
            RebuildMerchantStatus,
            RebuildQuestList,
            () => RefreshPage(companyPage),
            RefreshUI);
        dungeonBattleController = new DungeonBattleController(
            battleManager,
            dungeonRunManager,
            partyManager,
            townProgressState,
            message => statusText.text = message,
            ResetBattleLog,
            ShowBattlePage,
            ShowDungeonPage,
            interactable => startBattleButton.interactable = interactable,
            active => startBattleButton.gameObject.SetActive(active),
            title => battlePageTitleText.text = title,
            encounter => battleEncounterText.text = encounter,
            () =>
            {
                RefreshPage(companyPage);
                RefreshPage(partyPage);
                RefreshPage(healPage);
            },
            UpdateDungeonEventUI,
            label =>
            {
                if (battleSpeedButton != null)
                {
                    SetButtonLabel(battleSpeedButton, label);
                }
                if (roadSpeedButton != null)
                {
                    SetButtonLabel(roadSpeedButton, label);
                }
            },
            label =>
            {
                if (battlePauseButton != null)
                {
                    SetButtonLabel(battlePauseButton, label);
                }
                if (roadPauseButton != null)
                {
                    SetButtonLabel(roadPauseButton, label);
                }
            },
            RefreshUI);
        townTravelController = new TownTravelController(
            townProgressState,
            partyManager,
            battleManager,
            roadEncounterService,
            dungeonRunManager,
            dayManager,
            mercenaryGenerator,
            marketStockManager,
            blacksmithManager,
            saveManager,
            message => statusText.text = message,
            ShowTownMap,
            ShowWorldMap,
            message =>
            {
                travelConfirmationText.text = message;
                travelConfirmationOverlay.SetAsLastSibling();
                travelConfirmationOverlay.gameObject.SetActive(true);
            },
            HideTravelConfirmation,
            ResetBattleLog,
            ShowRoadBattlePage,
            active =>
            {
                roadContinueButton.gameObject.SetActive(active);
                roadRetreatButton.gameObject.SetActive(active);
            },
            text => roadBattleRouteText.text = text,
            () => StartCoroutine(ContinueTownTravelBattleRoutine()),
            dungeonBattleController.OpenNearbyDungeon,
            SyncDungeonUnlocks,
            RefreshTownMapButtons);
        tutorialController = new TutorialController(
            message => statusText.text = message,
            () =>
            {
                tutorialOverlay.SetAsLastSibling();
                tutorialOverlay.gameObject.SetActive(true);
            },
            HideTutorialOverlay,
            text => tutorialStepText.text = text,
            text => tutorialTitleText.text = text,
            text => tutorialBodyText.text = text,
            interactable => tutorialBackButton.interactable = interactable,
            label => SetButtonLabel(tutorialNextButton, label),
            () => tutorialTitleText != null &&
                  tutorialBodyText != null &&
                  tutorialStepText != null &&
                  tutorialBackButton != null &&
                  tutorialNextButton != null);
        townTravelController.ApplyTownServiceSettings(true, true);
        PopulateUniqueCandidatesIfNeeded();
        hireAndPartyController.CacheAlreadyHiredCandidates();
        EnsureEventSystem();
        BuildUI();
        audioFeedbackService = GetComponent<AudioFeedbackService>() ??
                               FindObjectOfType<AudioFeedbackService>();
        audioFeedbackService?.RegisterButtonsUnder(activeView.transform);
        storyProgressManager = storyProgressManager ??
                               GetComponent<StoryProgressManager>() ??
                               FindObjectOfType<StoryProgressManager>();
        if (storyProgressManager != null)
        {
            storyProgressManager.MilestoneCompleted += HandleStoryMilestoneCompleted;
        }
        merchantData.GoldChanged += HandleGoldChanged;
        merchantData.ProgressionChanged += HandleProgressionChanged;
        hireManager.MercenaryHired += HandleMercenaryHired;
        hireManager.MercenaryDismissed += HandleMercenaryDismissed;
        partyManager.PartyChanged += HandlePartyChanged;
        mercenaryGenerator.CandidatesChanged += HandleCandidatesChanged;
        battleManager.BattleMessageTyped += HandleBattleMessage;
        battleManager.BattleCompleted += HandleBattleCompleted;
        dungeonRunManager.DungeonMessage += HandleDungeonMessage;
        dungeonRunManager.DungeonStateChanged += HandleDungeonStateChanged;
        dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
        healingManager.HealingChanged += HandleHealingChanged;
        trainingGroundManager.TrainingChanged += HandleTrainingGroundChanged;
        trainingGroundManager.TrainingCompleted += HandleTrainingCompleted;
        merchantInventory.InventoryChanged += HandleInventoryChanged;
        dayManager.DayChanged += HandleDayChanged;
        marketPriceManager.PricesChanged += HandlePricesChanged;
        marketStockManager.StockChanged += HandleMarketStockChanged;
        blacksmithManager.CraftingChanged += HandleCraftingChanged;
        transportManager.TransportChanged += HandleTransportChanged;
        transportManager.TransportEventOccurred += HandleTransportEvent;
        remoteSaleManager.RemoteSaleChanged += HandleRemoteSaleChanged;
        remoteSaleManager.RemoteSaleEventOccurred += HandleRemoteSaleEvent;
        dungeonExpeditionManager.ExpeditionChanged += HandleExpeditionChanged;
        dungeonExpeditionManager.ExpeditionEventOccurred += HandleExpeditionEvent;
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged += HandleProgressionChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged += HandleProgressionChanged;
        }
        dailyResultController.CaptureDailySnapshot(dayManager.CurrentDay);
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

        if (trainingGroundManager == null)
        {
            trainingGroundManager = GetComponent<TrainingGroundManager>() ??
                                  FindObjectOfType<TrainingGroundManager>();
        }

        if (trainingGroundManager == null)
        {
            trainingGroundManager = gameObject.AddComponent<TrainingGroundManager>();
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

        if (debtManager == null)
        {
            debtManager = GetComponent<DebtManager>() ??
                          FindObjectOfType<DebtManager>();
        }

        if (roadEncounterService == null)
        {
            roadEncounterService =
                GetComponent<RoadEncounterService>() ??
                gameObject.AddComponent<RoadEncounterService>();
        }
        roadEncounterService.Initialize(dungeonRunManager, battleManager);

        if (townProgressState == null)
        {
            townProgressState =
                GetComponent<TownProgressState>() ??
                FindObjectOfType<TownProgressState>();
        }

        if (transportManager == null)
        {
            transportManager = GetComponent<TransportManager>() ??
                               FindObjectOfType<TransportManager>();
        }
        if (remoteSaleManager == null)
        {
            remoteSaleManager = GetComponent<RemoteSaleManager>() ??
                                FindObjectOfType<RemoteSaleManager>();
        }
        if (dungeonExpeditionManager == null)
        {
            dungeonExpeditionManager = GetComponent<DungeonExpeditionManager>() ??
                                       FindObjectOfType<DungeonExpeditionManager>();
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

        if (transportManager == null)
        {
            Debug.LogError("Simple hire UI is missing TransportManager.", this);
            hasAllReferences = false;
        }
        if (remoteSaleManager == null)
        {
            Debug.LogError("Simple hire UI is missing RemoteSaleManager.", this);
            hasAllReferences = false;
        }
        if (dungeonExpeditionManager == null)
        {
            Debug.LogError("Simple hire UI is missing DungeonExpeditionManager.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private Font LoadUIFont()
    {
        Font font = Resources.Load<Font>("Fonts/ZenKurenaido-Regular");
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                "游明朝 Demibold",
                "Yu Mincho Demibold",
                "UD デジタル 教科書体 N",
                "UD Digi Kyokasho N",
                "游明朝",
                "Yu Mincho",
                "Yu Gothic UI",
                "Meiryo",
                "MS Gothic"
                },
                16);
        }

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

    private Font LoadBodyFont()
    {
        Font font = Resources.Load<Font>("Fonts/ZenKurenaido-Regular");
        return font != null ? font : uiFont;
    }

    private void PopulateUniqueCandidatesIfNeeded()
    {
        RemoveMissingCandidates();

        foreach (MercenaryDataSO candidate in
                 GameAssetRepository.LoadAll<MercenaryDataSO>())
        {
            AddUniqueCandidate(candidate);
        }

        mercenaryGenerator.SetUniqueCandidatePool(candidates);

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
            hireManager.MercenaryDismissed -= HandleMercenaryDismissed;
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

        if (battleVisualController != null)
        {
            battleVisualController.PresentationCompleted -=
                HandleBattleVisualPresentationCompleted;
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

        if (trainingGroundManager != null)
        {
            trainingGroundManager.TrainingChanged -= HandleTrainingGroundChanged;
            trainingGroundManager.TrainingCompleted -= HandleTrainingCompleted;
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
        if (transportManager != null)
        {
            transportManager.TransportChanged -= HandleTransportChanged;
            transportManager.TransportEventOccurred -= HandleTransportEvent;
        }
        if (remoteSaleManager != null)
        {
            remoteSaleManager.RemoteSaleChanged -= HandleRemoteSaleChanged;
            remoteSaleManager.RemoteSaleEventOccurred -= HandleRemoteSaleEvent;
        }
        if (dungeonExpeditionManager != null)
        {
            dungeonExpeditionManager.ExpeditionChanged -= HandleExpeditionChanged;
            dungeonExpeditionManager.ExpeditionEventOccurred -= HandleExpeditionEvent;
        }
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged -= HandleProgressionChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged -= HandleProgressionChanged;
        }
        if (storyProgressManager != null)
        {
            storyProgressManager.MilestoneCompleted -= HandleStoryMilestoneCompleted;
        }
    }

    private void BuildUI()
    {
        SimpleMercenaryHireUIView view = CreateView();
        activeView = view;
        pageRouter = view.GetComponent<UIPageRouter>() ??
                     view.gameObject.AddComponent<UIPageRouter>();
        Canvas canvas = view.Canvas;
        RectTransform panel = view.GuildPanel;
        guildPanel = panel;
        overlayRoot = view.OverlayRoot;

        if (!view.HasChromeLayout)
        {
            CreateText(panel, "傭兵商会", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -62f), new Vector2(-28f, -18f),
            ParchmentTextColor);

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
        Image dayDisplayImage = dayDisplayRect.gameObject.AddComponent<Image>();
        dayDisplayImage.color = RowColor;
        AddFantasyFrame(dayDisplayImage, 1.5f);

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
        merchantStatusButtonImage.color = RowColor;
        AddFantasyFrame(merchantStatusButtonImage, 1.5f);
        Button merchantStatusButton =
            merchantStatusButtonRect.gameObject.AddComponent<Button>();
        merchantStatusButton.targetGraphic = merchantStatusButtonImage;
        merchantStatusButton.onClick.AddListener(ShowMerchantStatusOverlay);
        ApplyButtonTransitions(merchantStatusButton);
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

        globalMenuButton =
            CreateActionButton(panel, "メニュー", ShowGlobalMenu);
        RectTransform menuRect =
            globalMenuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = menuRect.anchorMax = new Vector2(1f, 1f);
        menuRect.pivot = new Vector2(1f, 1f);
        menuRect.sizeDelta = new Vector2(110f, 40f);
        menuRect.anchoredPosition = new Vector2(-20f, -68f);
        }
        else
        {
            BindChromeLayout(view);
        }

        BindPageLayout(view, panel);

        BuildHirePage();
        BuildGlobalMapPage();
        BuildWorldMapPage();
        BuildTownMapPage();
        BuildCompanyPage();
        BuildPartyPage();
        BuildHealPage();
        BuildBattlePage();
        BuildRoadBattlePage();
        BuildDungeonPage();
        BuildMarketPage();
        BuildBlacksmithPage();
        BuildInventoryPage();
        BuildJobChangePage();
        BuildTrainingGroundPage();

        if (!view.HasChromeLayout)
        {
            statusText = CreateText(panel, "雇用する傭兵を選択してください。", 15, FontStyle.Normal,
                TextAnchor.MiddleLeft, new Vector2(28f, 22f),
                new Vector2(-28f, 54f), ParchmentMutedColor);
            statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
            statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
            statusText.rectTransform.pivot = new Vector2(0.5f, 0f);
        }

        BuildCharacterDetailOverlay();
        BuildEquipmentDetailOverlay();
        BuildEquipmentCollectionOverlay();
        BuildMonsterCollectionOverlay();
        BuildQuestOverlay();
        BuildMerchantStatusOverlay();
        BuildTravelConfirmationOverlay();
        BuildStorageUpgradeConfirmationOverlay();
        BuildItemDetailOverlay();
        BuildPromotionPreviewOverlay();
        BuildGlobalMenuOverlay();
        BuildDailyResultOverlay();
        BuildFacilityGreetingOverlay();
        BuildTransportOverlay();
        BuildRemoteSaleOverlay();
        BuildExpeditionOverlay();
        BuildTutorialOverlay();
    }

    private void ShowWorldMap(int worldMapIndex)
    {
        worldMapIndex = Mathf.Clamp(
            worldMapIndex, 0, WorldMapService.WorldRegionNames.Length - 1);
        if (!townTravelController.CanEnterWorldRegion(worldMapIndex))
        {
            int gateTownIndex =
                WorldMapService.GetGateTownIndexForWorldRegion(worldMapIndex);
            DungeonDataSO gateDungeon =
                dungeonRunManager.GetHighestGradeDungeonNearTown(
                    gateTownIndex);
            statusText.text = gateDungeon != null
                ? $"{WorldMapService.WorldRegionNames[worldMapIndex]}へ進むには、" +
                  $"「{gateDungeon.dungeonName}」の完全攻略が必要です。"
                : $"{WorldMapService.WorldRegionNames[worldMapIndex]}はまだ解放されていません。";
            return;
        }

        townProgressState.ViewedWorldMapIndex = worldMapIndex;
        dungeonRunManager.SetCurrentWorldMapIndex(worldMapIndex);
        SwitchToMapPage(worldMapPage, false);
    }

    private void RefreshWorldMapPage()
    {
        int worldMapIndex = townProgressState.ViewedWorldMapIndex;
        SetVisibleRegionMap(worldMapIndex);
        RefreshTownMapButtons();
        statusText.text =
            $"現在地: {WorldMapService.TownNames[townProgressState.CurrentTownIndex]}  |  " +
            $"{WorldMapService.WorldRegionNames[worldMapIndex]}";
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
