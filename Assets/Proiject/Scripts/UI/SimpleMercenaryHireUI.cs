using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI : MonoBehaviour, IEquipmentDetailView
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
    [SerializeField] private RoadCargoSession roadCargoSession;
    [SerializeField] private MarketStockManager marketStockManager;
    [SerializeField] private BlacksmithManager blacksmithManager;
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private DungeonExpeditionManager dungeonExpeditionManager;
    [SerializeField] private HealingManager healingManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private DebtManager debtManager;
    [SerializeField] private RoadEncounterService roadEncounterService;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private StoryProgressManager storyProgressManager;
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
    private readonly HashSet<RectTransform> dirtyPages =
        new HashSet<RectTransform>();

    private RectTransform guildPanel;
    private RectTransform overlayRoot;
    private readonly SimpleMercenaryHireUIView.ExpeditionReferences expeditionView =
        new SimpleMercenaryHireUIView.ExpeditionReferences();
    private ExpeditionOverlayPresenter expeditionOverlayPresenter;
    private SimpleMercenaryHireUIView activeView;
    private UIPageRouter pageRouter;
    private readonly SimpleMercenaryHireUIView.CharacterDetailReferences
        characterDetail =
            new SimpleMercenaryHireUIView.CharacterDetailReferences();
    private readonly SimpleMercenaryHireUIView.EquipmentDetailReferences
        equipmentDetail =
            new SimpleMercenaryHireUIView.EquipmentDetailReferences();
    private readonly SimpleMercenaryHireUIView.EquipmentSlotSelectionReferences
        slotSelection =
            new SimpleMercenaryHireUIView.EquipmentSlotSelectionReferences();
    private MerchantQuestOverlayPresenter merchantQuestOverlayPresenter;
    private readonly SimpleMercenaryHireUIView.EquipmentCodexReferences
        equipmentCodex =
            new SimpleMercenaryHireUIView.EquipmentCodexReferences();
    private MonsterCodexManager monsterCodexManager;
    private MonsterCodexOverlayView monsterCodexOverlayView;
    private ItemCodexOverlayView itemCodexOverlayView;
    private RectTransform globalMenuOverlay;
    private readonly SimpleMercenaryHireUIView.DailyResultReferences
        dailyResult =
            new SimpleMercenaryHireUIView.DailyResultReferences();
    private DailyResultOverlayView dailyResultOverlayView;
    private bool hasPendingDailyResult;
    private readonly Queue<string> pendingDailyResultTexts =
        new Queue<string>();
    private TutorialOverlayView tutorialOverlayView;
    private RemoteSaleOverlayView remoteSaleOverlayView;
    private Button globalMenuButton;
    private readonly SimpleMercenaryHireUIView.RoadBattleReferences roadBattle =
        new SimpleMercenaryHireUIView.RoadBattleReferences();
    private readonly SimpleMercenaryHireUIView.TravelConfirmationReferences
        travelConfirmation =
            new SimpleMercenaryHireUIView.TravelConfirmationReferences();
    private RectTransform hirePage;
    private RectTransform globalMapPage;
    private RectTransform worldMapPage;
    private RectTransform townMapPage;
    private RawImage townMapBackgroundImage;
    private RectTransform hireList;
    private RectTransform companyPage;
    private RectTransform partyPage;
    private RectTransform healPage;
    private RectTransform battlePage;
    private readonly SimpleMercenaryHireUIView.BattleViewReferences battleView =
        new SimpleMercenaryHireUIView.BattleViewReferences();
    private readonly SimpleMercenaryHireUIView.DungeonViewReferences dungeonView =
        new SimpleMercenaryHireUIView.DungeonViewReferences();
    private RectTransform roadBattlePage;
    private RectTransform dungeonPage;
    private RectTransform marketPage;
    private RectTransform blacksmithPage;
    private RectTransform inventoryPage;
    private RectTransform jobChangePage;
    private RectTransform trainingGroundPage;
    private Button jobFacilityButton;
    private Button trainingGroundFacilityButton;
    private RectTransform companyScrollContent;
    private RectTransform companyList;
    private RectTransform partyList;
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
    private Button startDungeonButton;
    private Button firstDungeonEventButton;
    private Button secondDungeonEventButton;
    private Button thirdDungeonEventButton;
    private Button contractSelectButton;
    private Text goldText;
    private Text dayText;
    private Text statusText;
    private readonly SimpleMercenaryHireUIView.ContractDetailsReferences
        contractDetails =
            new SimpleMercenaryHireUIView.ContractDetailsReferences();
    private ContractDetailsOverlayView contractDetailsOverlayView;
    private readonly SimpleMercenaryHireUIView.FacilityGreetingReferences
        facilityGreeting =
            new SimpleMercenaryHireUIView.FacilityGreetingReferences();
    private FacilityGreetingOverlayView facilityGreetingOverlayView;
    private RemoteSaleController remoteSaleController;
    private Font uiFont;
    private Font uiBodyFont;
    private SimpleMercenaryHireUIFactory uiFactory;
    private const string EndingSceneName = "Ending";
    private StoryOverlayView storyOverlayView;
    private Coroutine storyEntryCoroutine;
    private StoryPresentation activeStoryPresentation;
    private Coroutine battleLogScrollCoroutine;
    private BattleVisualController battleVisualController;
    private bool hasPendingDungeonCompletion;
    private bool pendingDungeonCompletionCleared;
    private Coroutine pendingDungeonCompletionCoroutine;
    private Coroutine dungeonEventPresentationCoroutine;
    private bool hasPendingRoadBattleOutcome;
    private bool pendingRoadBattleVictory;
    private Coroutine pendingRoadBattleOutcomeCoroutine;
    private int displayedRoadOriginTownIndex = -1;
    private int displayedRoadDestinationTownIndex = -1;
    private bool IsProgressionLocked =>
        (battleManager != null && battleManager.IsBattling) ||
        (battleVisualController != null && battleVisualController.IsPresentationBusy) ||
        hasPendingRoadBattleOutcome;
    private DailyResultController dailyResultController;
    private HireAndPartyController hireAndPartyController;
    private EconomyController economyController;
    private EconomyPresenter economyPresenter;
    private HirePartyPresenter hirePartyPresenter;
    private BattleDungeonPresenter battleDungeonPresenter;
    private CharacterEquipmentController characterEquipmentController;
    private CharacterEquipmentOverlayPresenter characterEquipmentOverlayPresenter;
    private MerchantStatusAndQuestController merchantStatusAndQuestController;
    private TownTravelController townTravelController;
    private DungeonBattleController dungeonBattleController;
    private TutorialController tutorialController;
    private OnboardingGuideController onboardingGuideController;
    private OnboardingGuideBannerView onboardingGuideBannerView;
    private TrainingGroundPagePresenter trainingGroundPagePresenter;
    private AudioFeedbackService audioFeedbackService;
    private FacilityGreetingController facilityGreetingController;
    public event System.Action<string> FacilityEntered;
    private string pendingFacilityKey;
    private System.Action pendingFacilityDestination;

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

    private void BuildHealPage() => hirePartyPresenter.BuildHealPage();

    private void BuildReleaseConfirmationOverlay() =>
        hirePartyPresenter.BuildReleaseConfirmationOverlay();

    private void BuildContractChangeConfirmationOverlay() =>
        hirePartyPresenter.BuildContractChangeConfirmationOverlay();

    private void ShowContractChangeConfirmation(MercenaryInstance mercenary) =>
        hirePartyPresenter.ShowContractChangeConfirmation(mercenary);

    private bool CanOpenContractChangeConfirmation(MercenaryInstance mercenary) =>
        hirePartyPresenter.CanOpenContractChangeConfirmation(mercenary);

    private void ShowReleaseConfirmation(MercenaryInstance mercenary) =>
        hirePartyPresenter.ShowReleaseConfirmation(mercenary);

    private void BuildCharacterDetailOverlay() =>
        characterEquipmentOverlayPresenter.BuildCharacterDetailOverlay();

    private void BuildItemDetailOverlay() => economyPresenter.BuildItemDetailOverlay();

    private void BuildSellOnlyConfirmationOverlay() =>
        economyPresenter.BuildSellOnlyConfirmationOverlay();

    private void BuildSellQuantityOverlay() =>
        economyPresenter.BuildSellQuantityOverlay();

    private void BuildInventoryPage() => economyPresenter.BuildInventoryPage();

    private void BuildMarketPage() => economyPresenter.BuildMarketPage();

    private void BuildBlacksmithPage() => economyPresenter.BuildBlacksmithPage();

    private void HandleInventoryChanged() => economyPresenter.HandleInventoryChanged();

    private void HandleMarketStockChanged() => economyPresenter.HandleMarketStockChanged();

    private void HandleCraftingChanged() => economyPresenter.HandleCraftingChanged();

    private void HandlePricesChanged() => economyPresenter.HandlePricesChanged();

    private void ShowMarketPage() => economyPresenter.ShowMarketPage();

    private void ShowBlacksmithPage() => economyPresenter.ShowBlacksmithPage();

    private void ShowInventoryPage() => economyPresenter.ShowInventoryPage();

    private void UpdateStorageCapacityText() => economyPresenter?.UpdateStorageCapacityText();

    private void BuildStorageUpgradeConfirmationOverlay() =>
        economyPresenter.BuildStorageUpgradeConfirmationOverlay();

    private void ShowSellQuantityOverlay(ItemDataSO item) =>
        economyPresenter.ShowSellQuantityOverlay(item);

    private void ShowSellOnlyConfirmation() =>
        economyPresenter.ShowSellOnlyConfirmation();

    private void ShowBlacksmithRecipeDetail(EquipmentRecipeSO recipe) =>
        economyPresenter.ShowBlacksmithRecipeDetail(recipe);

    private void ShowMarketItemDetail(MarketStockEntry entry) =>
        economyPresenter.ShowMarketItemDetail(entry);

    private void BuildEquipmentDetailOverlay() =>
        characterEquipmentOverlayPresenter.BuildEquipmentDetailOverlay();

    private void BuildEquipmentSlotSelectionOverlay() =>
        characterEquipmentOverlayPresenter.BuildEquipmentSlotSelectionOverlay();

    private void BuildEquipmentCollectionOverlay() =>
        characterEquipmentOverlayPresenter.BuildEquipmentCollectionOverlay();

    private void ShowCharacterDetails(MercenaryInstance mercenary) =>
        characterEquipmentOverlayPresenter.ShowCharacterDetails(mercenary);

    private void ShowEquipmentCollection() =>
        characterEquipmentOverlayPresenter.ShowEquipmentCollection();

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

    bool IEquipmentDetailView.HasOverlay =>
        characterEquipmentOverlayPresenter.HasEquipmentDetailOverlay;

    void IEquipmentDetailView.SetTitle(string title, Color color) =>
        characterEquipmentOverlayPresenter.SetEquipmentDetailTitle(title, color);

    void IEquipmentDetailView.SetDetailText(string text) =>
        characterEquipmentOverlayPresenter.SetEquipmentDetailText(text);

    void IEquipmentDetailView.SetEnhanceButton(bool interactable, string label) =>
        characterEquipmentOverlayPresenter.SetEnhanceButton(interactable, label);

    void IEquipmentDetailView.SetSellButton(bool interactable, string label) =>
        characterEquipmentOverlayPresenter.SetSellButton(interactable, label);

    void IEquipmentDetailView.SetLockButtonLabel(string label) =>
        characterEquipmentOverlayPresenter.SetLockButtonLabel(label);

    void IEquipmentDetailView.ShowOverlay() =>
        characterEquipmentOverlayPresenter.ShowEquipmentDetailOverlay();

    void IEquipmentDetailView.HideOverlay() =>
        characterEquipmentOverlayPresenter.HideEquipmentDetailOverlay();

    private void OnEnable()
    {
        if (storyEntryCoroutine == null)
        {
            storyEntryCoroutine = StartCoroutine(ShowInitialStoryWhenReady());
        }
    }

    private IEnumerator ShowInitialStoryWhenReady()
    {
        yield return null;
        while (overlayRoot == null || uiFactory == null)
        {
            yield return null;
        }

        ShowNextPendingStory();
        storyEntryCoroutine = null;
    }

    private void HandleStoryPresentationQueued()
    {
        if (storyOverlayView == null || !storyOverlayView.IsShowing)
        {
            ShowNextPendingStory();
        }
    }

    private void ShowNextPendingStory()
    {
        BuildStoryOverlay();
        if (storyProgressManager == null ||
            !storyProgressManager.TryDequeuePresentation(
                out StoryPresentation presentation))
        {
            return;
        }

        if (presentation.IsEnding)
        {
            // DebtCleared is queued only by TryComplete, never during restore.
            // Save again here so return from the ending always restores this state.
            saveManager?.SaveGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(EndingSceneName);
            return;
        }

        activeStoryPresentation = presentation;
        storyOverlayView.Show(presentation);
    }

    private void BuildStoryOverlay()
    {
        if (storyOverlayView == null)
        {
            storyOverlayView = new StoryOverlayView(
                uiFactory,
                overlayRoot,
                ParchmentTextColor,
                CloseStoryOverlay);
        }

        storyOverlayView.Build();
    }

    private void CloseStoryOverlay()
    {
        storyOverlayView.Hide();
        if (activeStoryPresentation.Milestone == StoryMilestone.OpeningDebtNotice)
        {
            onboardingGuideController?.TryComplete(OnboardingGuideStep.Opening);
        }
        activeStoryPresentation.OnClosed?.Invoke();
        activeStoryPresentation = default;
        ShowNextPendingStory();
    }

    private void HandleTrainingGroundChanged()
    {
        trainingGroundPagePresenter?.HandleTrainingGroundChanged();
    }

    private void BuildTrainingGroundPage()
    {
        TrainingGroundPageUI pageUI =
            trainingGroundPage.GetComponent<TrainingGroundPageUI>() ??
            trainingGroundPage.gameObject.AddComponent<TrainingGroundPageUI>();
        trainingGroundPagePresenter = new TrainingGroundPagePresenter(
            uiFactory,
            trainingGroundPage,
            pageUI,
            uiBodyFont,
            ParchmentTextColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            hireManager,
            trainingGroundManager,
            merchantData,
            dayManager,
            townProgressState,
            pageRouter.Register,
            targetPage => SwitchToPage(targetPage),
            RefreshPage,
            message => statusText.text = message,
            RefreshUI);
        trainingGroundPagePresenter.Build();
    }

    private void ShowTrainingGroundPage()
    {
        trainingGroundPagePresenter?.Show();
    }

    private bool CanShowExpeditionAction(DungeonDataSO dungeon) =>
        expeditionOverlayPresenter != null &&
        expeditionOverlayPresenter.CanShowAction(dungeon);

    private bool HasExpedition(DungeonDataSO dungeon) =>
        expeditionOverlayPresenter != null &&
        expeditionOverlayPresenter.HasExpedition(dungeon);

    private void BuildBattlePage() => battleDungeonPresenter.BuildBattlePage();

    private void BuildRoadBattlePage() => battleDungeonPresenter.BuildRoadBattlePage();

    private void BuildDungeonPage() => battleDungeonPresenter.BuildDungeonPage();

    private void BindBattleVisualController(BattleVisualController controller)
    {
        battleVisualController = controller;
        battleVisualController.Configure(
            battleManager,
            uiBodyFont != null ? uiBodyFont : uiFont);
        battleVisualController.PresentationLog += HandlePresentationLog;
        battleVisualController.PresentationSound += HandlePresentationSound;
        battleVisualController.PresentationCompleted +=
            HandleBattleVisualPresentationCompleted;
    }

    private void ShowExpeditionForDungeon(DungeonDataSO dungeon) =>
        expeditionOverlayPresenter?.ShowForDungeon(dungeon);

    private void ShowExpeditionManagementOverlay() =>
        expeditionOverlayPresenter?.ShowManagement();

    private void BuildQuestOverlay()
    {
        merchantQuestOverlayPresenter.BuildQuestOverlay(
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.Quest,
                "Quest Overlay"));
    }

    private void BuildMerchantStatusOverlay()
    {
        merchantQuestOverlayPresenter.BuildMerchantStatusOverlay(
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.MerchantStatus,
                "Merchant Status Overlay"));
    }

    private void HandleGoldChanged(int currentGold) =>
        merchantQuestOverlayPresenter.HandleGoldChanged(currentGold);

    private void HandleProgressionChanged() =>
        merchantQuestOverlayPresenter.HandleProgressionChanged();

    private void ShowQuestOverlay() =>
        merchantQuestOverlayPresenter.ShowQuestOverlay();

    private void HideQuestOverlay() =>
        merchantQuestOverlayPresenter.HideQuestOverlay();

    private void ShowMerchantStatusOverlay() =>
        merchantQuestOverlayPresenter.ShowMerchantStatusOverlay();

    private void HideMerchantStatusOverlay() =>
        merchantQuestOverlayPresenter.HideMerchantStatusOverlay();

    private void RebuildMerchantStatus() =>
        merchantQuestOverlayPresenter.RebuildMerchantStatus();

    private void RebuildQuestList() =>
        merchantQuestOverlayPresenter.RebuildQuestList();

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
            CharacterEquipmentController.GetEquipmentDisplayName,
            dungeonExpeditionManager);
        hireAndPartyController = new HireAndPartyController(
            hireManager,
            partyManager,
            mercenaryGenerator,
            merchantInventory,
            healingManager,
            townProgressState,
            saveManager,
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
            label => economyPresenter?.SetInventoryFilterLabel(label),
            label => economyPresenter?.SetEquipmentSortLabel(label));
        remoteSaleController = new RemoteSaleController(
            remoteSaleManager,
            merchantInventory,
            townProgressState,
            message => statusText.text = message,
            RefreshRemoteSaleOverlay);
        facilityGreetingController = new FacilityGreetingController();
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
                if (characterDetail.statusText == null)
                {
                    return;
                }
                characterDetail.title.text = title;
                characterDetail.statusText.text = body;
            },
            ShowCharacterDetails,
            () => RefreshPage(companyPage),
            () => RefreshPage(partyPage),
            () => RefreshPage(inventoryPage),
            RefreshUI,
            SaveEquipmentChanges,
            () => saveManager?.SaveGame());
        // overlayRoot は BuildUI() で初めて確定するため、ここでは値ではなく
        // 解決用のデリゲートを渡す。生成時点では未設定である。
        characterEquipmentOverlayPresenter =
            new CharacterEquipmentOverlayPresenter(
                uiFactory,
                characterDetail,
                equipmentDetail,
                slotSelection,
                equipmentCodex,
                () => overlayRoot,
                merchantInventory,
                characterEquipmentController,
                uiFont,
                uiBodyFont,
                GetOrCreateOverlay);
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
            title => battleView.pageTitleText.text = title,
            encounter => battleView.encounterText.text = encounter,
            () =>
            {
                RefreshPage(companyPage);
                RefreshPage(partyPage);
                RefreshPage(healPage);
            },
            UpdateDungeonEventUI,
            label =>
            {
                if (battleView.speedButton != null)
                {
                    SetButtonLabel(battleView.speedButton, label);
                }
                if (roadBattle.speedButton != null)
                {
                    SetButtonLabel(roadBattle.speedButton, label);
                }
            },
            label =>
            {
                if (battleView.pauseButton != null)
                {
                    SetButtonLabel(battleView.pauseButton, label);
                }
                if (roadBattle.pauseButton != null)
                {
                    SetButtonLabel(roadBattle.pauseButton, label);
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
            roadCargoSession,
            message => statusText.text = message,
            ShowTownMap,
            ShowWorldMap,
            message =>
            {
                travelConfirmation.text.text = message;
                travelConfirmation.selectedCargo.Clear();
                travelConfirmation.selectedCompanions.Clear();
                RefreshTravelCargoSelection();
                travelConfirmation.overlay.SetAsLastSibling();
                travelConfirmation.overlay.gameObject.SetActive(true);
            },
            HideTravelConfirmation,
            ResetBattleLog,
            ShowRoadBattlePage,
            active =>
            {
                roadBattle.continueButton.gameObject.SetActive(active);
                roadBattle.retreatButton.gameObject.SetActive(active);
            },
            text => roadBattle.routeText.text = text,
            () => StartCoroutine(ContinueTownTravelBattleRoutine()),
            dungeonBattleController.OpenNearbyDungeon,
            SyncDungeonUnlocks,
            RefreshTownMapButtons);
        tutorialController = new TutorialController(
            message => statusText.text = message,
            () => tutorialOverlayView?.Show(),
            () => tutorialOverlayView?.Hide(),
            text => tutorialOverlayView?.SetStepText(text),
            text => tutorialOverlayView?.SetTitleText(text),
            text => tutorialOverlayView?.SetBodyText(text),
            value => tutorialOverlayView?.SetBackInteractable(value),
            label => tutorialOverlayView?.SetNextButtonLabel(label),
            () => tutorialOverlayView != null && tutorialOverlayView.IsValid);
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
            storyProgressManager.PresentationQueued += HandleStoryPresentationQueued;
        }
        onboardingGuideController = GetComponent<OnboardingGuideController>() ??
            FindObjectOfType<OnboardingGuideController>();
        if (onboardingGuideController != null)
        {
            onboardingGuideController.StateChanged += HandleOnboardingGuideStateChanged;
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
        dayManager.DayChangeFinalized += HandleDayChangeFinalized;
        dayManager.DaysAdvanceCompleted += HandleDaysAdvanceCompleted;
        marketPriceManager.PricesChanged += HandlePricesChanged;
        marketStockManager.StockChanged += HandleMarketStockChanged;
        blacksmithManager.CraftingChanged += HandleCraftingChanged;
        remoteSaleManager.RemoteSaleChanged += HandleRemoteSaleChanged;
        remoteSaleManager.RemoteSaleEventOccurred += HandleRemoteSaleEvent;
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
        if (saveManager != null &&
            !saveManager.HasExistingSaveAtInitialization)
        {
            tutorialController.ShowTutorialIfNeeded();
        }
    }

    private void ResolveReferences()
    {
        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>();
        }

        if (monsterCodexManager == null)
        {
            monsterCodexManager = GetComponent<MonsterCodexManager>() ??
                FindObjectOfType<MonsterCodexManager>();
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

        if (dungeonExpeditionManager == null)
        {
            dungeonExpeditionManager = GetComponent<DungeonExpeditionManager>() ??
                                       FindObjectOfType<DungeonExpeditionManager>();
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

        if (roadCargoSession == null)
        {
            roadCargoSession = GetComponent<RoadCargoSession>() ??
                FindObjectOfType<RoadCargoSession>();
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

        if (remoteSaleManager == null)
        {
            remoteSaleManager = GetComponent<RemoteSaleManager>() ??
                                FindObjectOfType<RemoteSaleManager>();
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

        if (remoteSaleManager == null)
        {
            Debug.LogError("Simple hire UI is missing RemoteSaleManager.", this);
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

        CompletePendingRoadBattleOutcome();

        if (battleVisualController != null)
        {
            battleVisualController.PresentationLog -= HandlePresentationLog;
            battleVisualController.PresentationSound -= HandlePresentationSound;
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
            dayManager.DayChangeFinalized -= HandleDayChangeFinalized;
            dayManager.DaysAdvanceCompleted -= HandleDaysAdvanceCompleted;
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
        if (remoteSaleManager != null)
        {
            remoteSaleManager.RemoteSaleChanged -= HandleRemoteSaleChanged;
            remoteSaleManager.RemoteSaleEventOccurred -= HandleRemoteSaleEvent;
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
            storyProgressManager.PresentationQueued -= HandleStoryPresentationQueued;
        }
        if (onboardingGuideController != null)
        {
            onboardingGuideController.StateChanged -= HandleOnboardingGuideStateChanged;
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

        hirePartyPresenter = new HirePartyPresenter(
            uiFactory, activeView, pageRouter, hireAndPartyController, hireManager,
            partyManager, mercenaryGenerator, healingManager, merchantInventory, merchantData,
            merchantStatusAndQuestController, hirePage, companyPage, partyPage,
            healPage, jobChangePage, overlayRoot, uiFont, uiBodyFont,
            () => statusText, () => hireTabButton, () => companyTabButton,
            () => partyTabButton, () => healTabButton, () => startBattleButton,
            ShowContractDetails, ShowContractDetails, ShowCharacterDetails,
            ShowQuestOverlay, ShowTransportOverlay, ShowExpeditionOverlay,
            ShowRemoteSaleOverlay, ShowContractChangeConfirmation,
            CanOpenContractChangeConfirmation, ShowReleaseConfirmation,
            new SimpleMercenaryHireUIView.ReleaseConfirmationReferences(),
            new SimpleMercenaryHireUIView.ContractChangeReferences(),
            button => contractSelectButton = button, RefreshPage,
            townProgressState, dailyResultController, battleManager,
            new SimpleMercenaryHireUIView.PromotionPreviewReferences(),
            (targetPage, activeTab) => SwitchToPage(targetPage, activeTab),
            ShowTownMap, ShowExpeditionManagementOverlay, RefreshUI,
            () => TryUnlockHiddenIsland());

        battleDungeonPresenter = new BattleDungeonPresenter(
            uiFactory, activeView, pageRouter, battlePage, roadBattlePage,
            dungeonPage, uiFont, uiBodyFont, battleManager, dungeonRunManager,
            partyManager, townProgressState, progressionManager,
            dungeonBattleController, townTravelController,
            battleView, roadBattle, dungeonView, () => statusText,
            () => startBattleButton, () => startDungeonButton,
            () => firstDungeonEventButton, () => secondDungeonEventButton,
            () => thirdDungeonEventButton, () => battleVisualController,
            () => battleTabButton, () => dungeonTabButton,
            button => startBattleButton = button,
            button => startDungeonButton = button,
            button => firstDungeonEventButton = button,
            button => secondDungeonEventButton = button,
            button => thirdDungeonEventButton = button,
            BindBattleVisualController,
            RefreshBattlePage, RefreshRoadBattlePage, RefreshDungeonPage,
            ContinueToNextDungeonFloor,
            ReturnToTownAfterDungeon, CanShowExpeditionAction, HasExpedition,
            ShowExpeditionForDungeon, RefreshPage, () => audioFeedbackService);

        economyPresenter = new EconomyPresenter(
            uiFactory,
            overlayRoot,
            economyController,
            merchantInventory,
            merchantData,
            marketStockManager,
            blacksmithManager,
            inventoryPage,
            marketPage,
            blacksmithPage,
            uiBodyFont,
            marketPriceManager,
            townProgressState,
            dayManager,
            progressionManager,
            merchantStatusAndQuestController,
            dailyResultController,
            // タブボタンは本Presenterの生成後に作られるため遅延解決する。
            () => inventoryTabButton,
            () => marketTabButton,
            () => blacksmithTabButton,
            (targetPage, activeTab) => SwitchToPage(targetPage, activeTab),
            RefreshPage,
            RefreshUI,
            // TryUnlockHiddenIsland は bool を返すため Action へ直接渡せない。
            // 戻り値は既存経路でも使っていないので破棄する。
            () => TryUnlockHiddenIsland(),
            ShowEquipmentCollection,
            characterEquipmentController.UseConsumable,
            characterEquipmentController.ShowEquipmentDetails,
            pageRouter.Register,
            message => statusText.text = message);

        expeditionOverlayPresenter = new ExpeditionOverlayPresenter(
            uiFactory, expeditionView, overlayRoot,
            hireManager, dungeonRunManager, dungeonExpeditionManager,
            () => dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection());
        merchantQuestOverlayPresenter = new MerchantQuestOverlayPresenter(
            uiFactory,
            new SimpleMercenaryHireUIView.QuestReferences(),
            new SimpleMercenaryHireUIView.MerchantStatusReferences(),
            merchantData,
            progressionManager,
            merchantStatusAndQuestController,
            () => RefreshPage(healPage),
            () => RefreshPage(blacksmithPage),
            () => RefreshPage(companyPage),
            () => RefreshPage(inventoryPage),
            RefreshUI);

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
        BuildEquipmentSlotSelectionOverlay();
        BuildEquipmentCollectionOverlay();
        BuildMonsterCollectionOverlay();
        BuildItemCollectionOverlay();
        BuildQuestOverlay();
        BuildMerchantStatusOverlay();
        BuildTravelConfirmationOverlay();
        BuildReleaseConfirmationOverlay();
        BuildContractChangeConfirmationOverlay();
        BuildStorageUpgradeConfirmationOverlay();
        BuildSellOnlyConfirmationOverlay();
        BuildSellQuantityOverlay();
        BuildItemDetailOverlay();
        BuildPromotionPreviewOverlay();
        BuildGlobalMenuOverlay();
        BuildDailyResultOverlay();
        facilityGreetingOverlayView = new FacilityGreetingOverlayView(
            uiFactory,
            facilityGreeting,
            overlayRoot,
            EnterFacilityFromGreeting,
            HideFacilityGreeting);
        BuildFacilityGreetingOverlay();
        BuildRemoteSaleOverlay();
        contractDetailsOverlayView = new ContractDetailsOverlayView(
            uiFactory,
            contractDetails,
            overlayRoot,
            hireManager,
            merchantData,
            HideContractDetails);
        tutorialOverlayView = new TutorialOverlayView(
            uiFactory,
            new SimpleMercenaryHireUIView.TutorialReferences(),
            overlayRoot,
            () => tutorialController.ShowPreviousStep(),
            () => tutorialController.ShowNextStep(),
            HideTutorialOverlay);
        tutorialOverlayView.Build();
        tutorialController.Refresh();
        BuildOnboardingGuideBanner();
    }

    private void BuildMonsterCollectionOverlay()
    {
        RectTransform prefabOverlay = activeView != null
            ? activeView.GetOverlay(SimpleMercenaryHireOverlaySlot.MonsterCollection)
            : null;
        monsterCodexOverlayView = new MonsterCodexOverlayView(
            uiFactory,
            new SimpleMercenaryHireUIView.MonsterCodexReferences
            {
                overlay = prefabOverlay
            },
            overlayRoot,
            uiFont,
            uiBodyFont,
            monsterCodexManager,
            HideMonsterCollection);
        monsterCodexOverlayView.Build();
    }

    private void ShowMonsterCollection()
    {
        monsterCodexOverlayView?.Show();
    }

    private void HideMonsterCollection()
    {
        monsterCodexOverlayView?.Hide();
    }

    private void BuildItemCollectionOverlay()
    {
        RectTransform prefabOverlay = activeView != null
            ? activeView.GetOverlay(SimpleMercenaryHireOverlaySlot.ItemCollection)
            : null;
        itemCodexOverlayView = new ItemCodexOverlayView(
            uiFactory,
            new SimpleMercenaryHireUIView.ItemCodexReferences
            {
                overlay = prefabOverlay
            },
            overlayRoot,
            uiFont,
            uiBodyFont,
            merchantInventory,
            HideItemCollection);
        itemCodexOverlayView.Build();
    }

    private void ShowItemCollection()
    {
        itemCodexOverlayView?.Show();
    }

    private void HideItemCollection()
    {
        itemCodexOverlayView?.Hide();
    }

    private void ShowTutorialOverlay()
    {
        tutorialController.ShowTutorial();
    }

    private void HideTutorialOverlay()
    {
        tutorialOverlayView?.Hide();
    }

    private void BuildOnboardingGuideBanner()
    {
        if (onboardingGuideBannerView == null)
        {
            onboardingGuideBannerView = new OnboardingGuideBannerView(
                uiFactory,
                activeView != null ? activeView.Chrome : null,
                guildPanel,
                overlayRoot,
                () => onboardingGuideController != null &&
                    onboardingGuideController.IsEnabled &&
                    !onboardingGuideController.IsComplete,
                () => onboardingGuideController != null
                    ? onboardingGuideController.CurrentObjectiveText
                    : string.Empty,
                () => onboardingGuideController?.Skip());
        }

        onboardingGuideBannerView.Build();
    }

    private void HandleOnboardingGuideStateChanged(OnboardingGuideStep step)
    {
        onboardingGuideBannerView?.Refresh();
    }

    private void BuildFacilityGreetingOverlay()
    {
        facilityGreetingOverlayView?.Build();
    }

    private void OpenFacilityWithGreeting(string facilityKey, System.Action destination)
    {
        int currentDay = dayManager != null ? dayManager.CurrentDay : 1;
        int townIndex = townProgressState != null ? townProgressState.CurrentTownIndex : 0;
        if (!facilityGreetingController.ShouldShowGreeting(currentDay, townIndex, facilityKey))
        {
            EnterFacility(facilityKey, destination);
            return;
        }

        string townName = townIndex >= 0 && townIndex < WorldMapService.TownNames.Length
            ? WorldMapService.TownNames[townIndex]
            : "この町";
        FacilityGreeting greeting = facilityGreetingController.GetGreeting(
            currentDay, townIndex, townName, facilityKey);
        Sprite portrait = Resources.Load<Sprite>("UI/Staff/" + facilityKey);
        facilityGreetingOverlayView?.SetTitle(greeting.Title);
        facilityGreetingOverlayView?.SetDialogue(greeting.Dialogue);
        facilityGreetingOverlayView?.SetPortrait(portrait);
        pendingFacilityKey = facilityKey;
        pendingFacilityDestination = destination;
        facilityGreetingOverlayView?.Show();
    }

    private void EnterFacilityFromGreeting()
    {
        int currentDay = dayManager != null ? dayManager.CurrentDay : 1;
        int townIndex = townProgressState != null ? townProgressState.CurrentTownIndex : 0;
        facilityGreetingController.MarkEntered(currentDay, townIndex, pendingFacilityKey);
        string facilityKey = pendingFacilityKey;
        System.Action destination = pendingFacilityDestination;
        HideFacilityGreeting();
        EnterFacility(facilityKey, destination);
    }

    private void EnterFacility(string facilityKey, System.Action destination)
    {
        destination?.Invoke();
        FacilityEntered?.Invoke(facilityKey);
    }

    private void HideFacilityGreeting()
    {
        pendingFacilityKey = null;
        pendingFacilityDestination = null;
        facilityGreetingOverlayView?.Hide();
    }

    private void ShowContractDetails(MercenaryDataSO candidate)
    {
        contractDetailsOverlayView?.Show(candidate);
    }

    private void ShowContractDetails(MercenaryInstance candidate)
    {
        contractDetailsOverlayView?.Show(candidate);
    }

    private void HideContractDetails()
    {
        contractDetailsOverlayView?.Hide();
    }

    private void BuildRemoteSaleOverlay()
    {
        remoteSaleOverlayView = new RemoteSaleOverlayView(
            uiFactory,
            overlayRoot,
            remoteSaleController,
            remoteSaleManager,
            merchantInventory,
            HideRemoteSaleOverlay);
        remoteSaleOverlayView.Build();
    }

    private void ShowRemoteSaleOverlay()
    {
        remoteSaleOverlayView?.Show();
    }

    private void HideRemoteSaleOverlay()
    {
        remoteSaleOverlayView?.Hide();
    }

    private void HandleTrainingCompleted(TrainingReservation reservation)
    {
        string line = dailyResultController.RecordTrainingCompleted(reservation);
        if (!string.IsNullOrEmpty(line) &&
            dailyResultOverlayView != null &&
            dailyResultOverlayView.IsShowing)
        {
            dailyResultOverlayView.AppendText(line);
            dailyResultController.ConsumeRecordedTrainingCompletion(line);
        }
    }

    private void BuildDailyResultOverlay()
    {
        dailyResult.overlay = GetOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.DailyResult,
            "Daily Result Overlay");
        dailyResultOverlayView = new DailyResultOverlayView(
            uiFactory, dailyResult, overlayRoot, HideDailyResult);
        dailyResultOverlayView.Build();
    }

    private void HideDailyResult()
    {
        dailyResultOverlayView?.Hide();
        ShowPendingDailyResultIfReady();
    }

    private void HandleDayChanged(int currentDay)
    {
        if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            mercenaryGenerator.ClearCandidates();
        }
        RefreshPage(marketPage);
        RefreshPage(inventoryPage);
        RefreshPage(healPage);
        RefreshPage(companyPage);
        RefreshUI();
        string debtNotice = debtManager != null &&
                            (currentDay - 1) % DebtManager.DaysPerMonth == 0 &&
                            currentDay > 1
            ? debtManager.PaymentArrears > 0
                ? $" 月次返済後の滞納額：{debtManager.PaymentArrears:N0}Gです。"
                : $" 月次最低返済を完了しました。"
            : string.Empty;
        statusText.text =
            $"{currentDay}日目になりました。市場価格が更新されました。{debtNotice}";
    }

    private void HandleDayChangeFinalized(int currentDay)
    {
        QueueDailyResult(currentDay);
    }

    private void HandleDaysAdvanceCompleted(int advancedDays)
    {
        ShowPendingDailyResultIfReady();
    }

    private void ShowPendingDailyResultIfReady()
    {
        if (!hasPendingDailyResult ||
            (dailyResultOverlayView != null &&
             dailyResultOverlayView.IsShowing))
        {
            return;
        }

        if (battleVisualController != null &&
            battleVisualController.IsPresentationBusy)
        {
            return;
        }

        StringBuilder combined = new StringBuilder();
        bool first = true;
        while (pendingDailyResultTexts.Count > 0)
        {
            if (!first)
            {
                combined.AppendLine();
                combined.AppendLine("────────────");
                combined.AppendLine();
            }

            combined.Append(pendingDailyResultTexts.Dequeue());
            first = false;
        }

        hasPendingDailyResult = false;
        ShowDailyResult(combined.ToString());
    }

    private void QueueDailyResult(int currentDay)
    {
        string resultText = dailyResultOverlayView == null ||
                            !dailyResultOverlayView.IsValid
            ? null
            : dailyResultController.BuildDailyResultText(currentDay);
        if (resultText == null)
        {
            dailyResultController.CaptureDailySnapshot(currentDay);
            return;
        }

        pendingDailyResultTexts.Enqueue(resultText);
        hasPendingDailyResult = true;
        dailyResultController.CaptureDailySnapshot(currentDay);
    }

    private void ShowDailyResult(string resultText)
    {
        dailyResultOverlayView?.Show(resultText);
    }

    private void RefreshRemoteSaleOverlay()
    {
        remoteSaleOverlayView?.Refresh();
    }

    private void HandleRemoteSaleChanged()
    {
        RefreshPage(companyPage);
        if (remoteSaleOverlayView != null && remoteSaleOverlayView.IsShowing)
        {
            RefreshRemoteSaleOverlay();
        }
    }

    private void HandleRemoteSaleEvent(RemoteSaleEvent remoteSaleEvent)
    {
        dailyResultController.RecordRemoteSaleEvent(remoteSaleEvent);
        HandleRemoteSaleChanged();
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

    private void BuildHirePage() => hirePartyPresenter.BuildHirePage();
    private void BuildCompanyPage() => hirePartyPresenter.BuildCompanyPage();
    private void BuildPartyPage() => hirePartyPresenter.BuildPartyPage();
    private void BuildJobChangePage() => hirePartyPresenter.BuildJobChangePage();
    private void BuildPromotionPreviewOverlay() => hirePartyPresenter.BuildPromotionPreviewOverlay();

    // These lifecycle callback method groups intentionally remain on the MonoBehaviour.
    private void HandleMercenaryHired(MercenaryInstance mercenary) => hirePartyPresenter.HandleMercenaryHired(mercenary);
    private void HandleMercenaryDismissed(MercenaryInstance mercenary) => hirePartyPresenter.HandleMercenaryDismissed(mercenary);
    private void HandlePartyChanged() => hirePartyPresenter.HandlePartyChanged();
    private void HandleCandidatesChanged() => hirePartyPresenter.HandleCandidatesChanged();
    private void HandleHealingChanged() => hirePartyPresenter.HandleHealingChanged();

    private void ShowHirePage() => hirePartyPresenter.ShowHirePage();
    private void ShowCompanyPage() => hirePartyPresenter.ShowCompanyPage();
    private void ShowPartyPage() => hirePartyPresenter.ShowPartyPage();
    private void ShowHealPage() => hirePartyPresenter.ShowHealPage();
    private void ShowJobChangePage() => hirePartyPresenter.ShowJobChangePage();
    private void ShowTransportOverlay() => hirePartyPresenter.ShowTransportOverlay();
    private void ShowExpeditionOverlay() => hirePartyPresenter.ShowExpeditionOverlay();

    private void ResetBattleLog()
    {
        dungeonBattleController.ClearBattleLog();
        if (battleLogScrollCoroutine != null)
        {
            StopCoroutine(battleLogScrollCoroutine);
            battleLogScrollCoroutine = null;
        }
        battleDungeonPresenter.ResetBattleLogView();
    }

    private void AppendBattleMessage(string message, BattleLogType logType)
    {
        string text = dungeonBattleController.AppendBattleMessage(message, logType);
        battleDungeonPresenter.SetBattleLogText(text);
        UpdateBattleLogContentHeight();
        ScrollBattleLogToLatest();
    }

    private void UpdateBattleLogContentHeight()
    {
        battleDungeonPresenter.UpdateBattleLogContentHeight();
    }

    private void UpdateDungeonEventUI()
    {
        battleDungeonPresenter.UpdateDungeonEventUI();
    }

    private static void SetButtonLabel(Button button, string label)
    {
        BattleDungeonPresenter.SetButtonLabel(button, label);
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
