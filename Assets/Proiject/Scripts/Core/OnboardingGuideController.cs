using System;
using System.Collections.Generic;
using UnityEngine;

public enum OnboardingGuideStep
{
    Opening = 0,
    HireMercenary = 1,
    FormParty = 2,
    DepartDungeon = 3,
    ReturnFromDungeon = 4,
    OpenWarehouse = 5,
    SellItem = 6,
    OpenMarket = 7,
    OpenBlacksmith = 8,
    Completed = 9,
    Skipped = 10
}

public enum OnboardingGuideCard
{
    Warehouse,
    Market,
    Blacksmith
}

public sealed class OnboardingGuideController : MonoBehaviour
{
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private DungeonRunManager dungeonRunManager;
    private MerchantInventory merchantInventory;
    private TownProgressState townProgressState;
    private SimpleMercenaryHireUI gameUI;
    private StoryProgressManager storyProgressManager;
    private readonly HashSet<OnboardingGuideCard> shownCards =
        new HashSet<OnboardingGuideCard>();

    public OnboardingGuideStep CurrentStep { get; private set; } =
        OnboardingGuideStep.Completed;
    public bool IsEnabled { get; private set; }
    public bool IsComplete =>
        CurrentStep == OnboardingGuideStep.Completed ||
        CurrentStep == OnboardingGuideStep.Skipped;
    public string CurrentObjectiveText => GetObjectiveText(CurrentStep);
    public IReadOnlyCollection<OnboardingGuideCard> ShownCards => shownCards;

    public event Action<OnboardingGuideStep> StateChanged;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();
        CorrectFromGameState();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Restore(
        bool enabled,
        OnboardingGuideStep step,
        IEnumerable<OnboardingGuideCard> restoredCards = null)
    {
        ResolveReferences();
        shownCards.Clear();
        if (restoredCards != null)
        {
            foreach (OnboardingGuideCard card in restoredCards)
            {
                if (Enum.IsDefined(typeof(OnboardingGuideCard), card))
                {
                    shownCards.Add(card);
                }
            }
        }
        IsEnabled = enabled;
        CurrentStep = enabled || step == OnboardingGuideStep.Skipped
            ? SanitizeStep(step)
            : OnboardingGuideStep.Completed;
        CorrectFromGameState();
        StateChanged?.Invoke(CurrentStep);
    }

    public bool TryComplete(OnboardingGuideStep step)
    {
        if (!IsEnabled || IsComplete || step != CurrentStep)
        {
            return false;
        }

        Advance();
        if (step == OnboardingGuideStep.OpenWarehouse &&
            !HasSellableInventory())
        {
            CurrentStep = OnboardingGuideStep.DepartDungeon;
            StateChanged?.Invoke(CurrentStep);
        }
        return true;
    }

    public bool Advance()
    {
        if (!IsEnabled || IsComplete)
        {
            return false;
        }

        CurrentStep = GetNextStep(CurrentStep);
        if (CurrentStep == OnboardingGuideStep.Completed)
        {
            IsEnabled = false;
        }
        StateChanged?.Invoke(CurrentStep);
        return true;
    }

    public bool Skip()
    {
        if (!IsEnabled || IsComplete)
        {
            return false;
        }

        IsEnabled = false;
        CurrentStep = OnboardingGuideStep.Skipped;
        StateChanged?.Invoke(CurrentStep);
        return true;
    }

    public bool CompleteOpeningAfterExistingSaveLoad()
    {
        return TryComplete(OnboardingGuideStep.Opening);
    }

    public bool CorrectFromGameState()
    {
        if (!IsEnabled || IsComplete)
        {
            return false;
        }

        bool corrected = false;
        while (CanCorrectCurrentStep())
        {
            Advance();
            corrected = true;
        }

        return corrected;
    }

    private bool CanCorrectCurrentStep()
    {
        if (CurrentStep == OnboardingGuideStep.HireMercenary)
        {
            return hireManager != null && hireManager.HiredMercenaries.Count > 0;
        }

        if (CurrentStep == OnboardingGuideStep.FormParty)
        {
            return partyManager != null && partyManager.Members.Count > 0;
        }

        return false;
    }

    private bool HasSellableInventory()
    {
        if (merchantInventory == null)
        {
            return false;
        }

        if (merchantInventory.GetUsedStorageSlots() > 0)
        {
            return true;
        }

        foreach (EquipmentInstance equipment in merchantInventory.EquipmentInstances)
        {
            if (equipment != null && !equipment.IsLocked)
            {
                return true;
            }
        }

        return false;
    }

    public static string GetObjectiveText(OnboardingGuideStep step)
    {
        switch (step)
        {
            case OnboardingGuideStep.Opening:
                return "第一章を始める";
            case OnboardingGuideStep.HireMercenary:
                return "傭兵を雇う";
            case OnboardingGuideStep.FormParty:
                return "パーティを編成する";
            case OnboardingGuideStep.DepartDungeon:
                return "ダンジョンへ出撃して品物を持ち帰る";
            case OnboardingGuideStep.ReturnFromDungeon:
                return "ダンジョンから帰還する";
            case OnboardingGuideStep.OpenWarehouse:
                return "倉庫を開く";
            case OnboardingGuideStep.SellItem:
                return "アイテムを1つ売却する（売値は日によって変わります）";
            case OnboardingGuideStep.OpenMarket:
                return "市場を開く";
            case OnboardingGuideStep.OpenBlacksmith:
                return "鍛冶屋を開く";
            default:
                return string.Empty;
        }
    }

    private static OnboardingGuideStep GetNextStep(OnboardingGuideStep step)
    {
        return step < OnboardingGuideStep.OpenBlacksmith
            ? step + 1
            : OnboardingGuideStep.Completed;
    }

    private static OnboardingGuideStep SanitizeStep(OnboardingGuideStep step)
    {
        return step >= OnboardingGuideStep.Opening &&
               step <= OnboardingGuideStep.Skipped
            ? step
            : OnboardingGuideStep.Completed;
    }

    private void ResolveReferences()
    {
        hireManager = hireManager ?? GetComponent<MercenaryHireManager>() ??
            FindObjectOfType<MercenaryHireManager>();
        partyManager = partyManager ?? GetComponent<MercenaryPartyManager>() ??
            FindObjectOfType<MercenaryPartyManager>();
        dungeonRunManager = dungeonRunManager ?? GetComponent<DungeonRunManager>() ??
            FindObjectOfType<DungeonRunManager>();
        merchantInventory = merchantInventory ?? GetComponent<MerchantInventory>() ??
            FindObjectOfType<MerchantInventory>();
        townProgressState = townProgressState ?? GetComponent<TownProgressState>() ??
            FindObjectOfType<TownProgressState>();
        gameUI = gameUI ?? GetComponent<SimpleMercenaryHireUI>() ??
            FindObjectOfType<SimpleMercenaryHireUI>();
        storyProgressManager = storyProgressManager ??
            GetComponent<StoryProgressManager>() ??
            FindObjectOfType<StoryProgressManager>();
    }

    private void Subscribe()
    {
        Unsubscribe();
        ResolveReferences();
        if (hireManager != null) hireManager.MercenaryHired += HandleMercenaryHired;
        if (partyManager != null) partyManager.PartyChanged += HandlePartyChanged;
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonStateChanged += HandleDungeonStateChanged;
            dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
        }
        if (merchantInventory != null) merchantInventory.ItemSold += HandleItemSold;
        if (townProgressState != null)
        {
            townProgressState.TownProgressChanged += HandleTownProgressChanged;
        }
        if (gameUI != null) gameUI.FacilityEntered += HandleFacilityEntered;
    }

    private void Unsubscribe()
    {
        if (hireManager != null) hireManager.MercenaryHired -= HandleMercenaryHired;
        if (partyManager != null) partyManager.PartyChanged -= HandlePartyChanged;
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonStateChanged -= HandleDungeonStateChanged;
            dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
        }
        if (merchantInventory != null) merchantInventory.ItemSold -= HandleItemSold;
        if (townProgressState != null)
        {
            townProgressState.TownProgressChanged -= HandleTownProgressChanged;
        }
        if (gameUI != null) gameUI.FacilityEntered -= HandleFacilityEntered;
    }

    private void HandleMercenaryHired(MercenaryInstance mercenary)
    {
        if (IsEnabled && CurrentStep == OnboardingGuideStep.HireMercenary &&
            hireManager.HiredMercenaries.Count > 0)
        {
            TryComplete(OnboardingGuideStep.HireMercenary);
        }
    }

    private void HandlePartyChanged()
    {
        if (IsEnabled && CurrentStep == OnboardingGuideStep.FormParty &&
            partyManager.Members.Count > 0)
        {
            TryComplete(OnboardingGuideStep.FormParty);
        }
    }

    private void HandleDungeonStateChanged()
    {
        if (IsEnabled && CurrentStep == OnboardingGuideStep.DepartDungeon &&
            dungeonRunManager.IsRunning)
        {
            TryComplete(OnboardingGuideStep.DepartDungeon);
        }
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        if (IsEnabled && CurrentStep == OnboardingGuideStep.ReturnFromDungeon)
        {
            TryComplete(OnboardingGuideStep.ReturnFromDungeon);
        }
    }

    private void HandleItemSold(MerchantInventorySale sale)
    {
        TryComplete(OnboardingGuideStep.SellItem);
    }

    private void HandleTownProgressChanged()
    {
        CorrectFromGameState();
    }

    private void HandleFacilityEntered(string facilityKey)
    {
        if (CurrentStep == OnboardingGuideStep.OpenWarehouse &&
            facilityKey == FacilityGreetingController.WarehouseKey)
        {
            TryComplete(OnboardingGuideStep.OpenWarehouse);
            ShowSupplementalCard(OnboardingGuideCard.Warehouse);
        }
        else if (CurrentStep == OnboardingGuideStep.OpenMarket &&
                 facilityKey == FacilityGreetingController.MarketKey)
        {
            TryComplete(OnboardingGuideStep.OpenMarket);
            ShowSupplementalCard(OnboardingGuideCard.Market);
        }
        else if (CurrentStep == OnboardingGuideStep.OpenBlacksmith &&
                 facilityKey == FacilityGreetingController.BlacksmithKey)
        {
            ShowSupplementalCard(OnboardingGuideCard.Blacksmith);
        }

        CorrectFromGameState();
    }

    private void ShowSupplementalCard(OnboardingGuideCard card)
    {
        if (!shownCards.Add(card))
        {
            if (card == OnboardingGuideCard.Blacksmith)
            {
                TryComplete(OnboardingGuideStep.OpenBlacksmith);
            }
            return;
        }

        StateChanged?.Invoke(CurrentStep);
        storyProgressManager?.EnqueueOnboardingPresentation(
            card,
            () => CompleteSupplementalCard(card));
    }

    private void CompleteSupplementalCard(OnboardingGuideCard card)
    {
        if (card == OnboardingGuideCard.Blacksmith)
        {
            TryComplete(OnboardingGuideStep.OpenBlacksmith);
        }
    }
}
