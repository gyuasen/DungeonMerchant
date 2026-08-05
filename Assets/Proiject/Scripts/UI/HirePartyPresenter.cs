using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HirePartyViewDependencies
{
    public SimpleMercenaryHireUIFactory factory;
    public SimpleMercenaryHireUIView activeView;
    public UIPageRouter pageRouter;
    public RectTransform hirePage;
    public RectTransform companyPage;
    public RectTransform partyPage;
    public RectTransform healPage;
    public RectTransform jobChangePage;
    public RectTransform overlayRoot;
    public Font uiFont;
    public Font uiBodyFont;
    public Func<Text> statusTextProvider;
    public Func<Button> hireTabButtonProvider;
    public Func<Button> companyTabButtonProvider;
    public Func<Button> partyTabButtonProvider;
    public Func<Button> healTabButtonProvider;
    public Func<Button> startBattleButtonProvider;
    public SimpleMercenaryHireUIView.ReleaseConfirmationReferences releaseConfirmation;
    public SimpleMercenaryHireUIView.ContractChangeReferences contractChange;
    public SimpleMercenaryHireUIView.PromotionPreviewReferences promotionPreview;
}

public sealed class HirePartyDomainDependencies
{
    public HireAndPartyController hireAndPartyController;
    public MercenaryHireManager hireManager;
    public MercenaryPartyManager partyManager;
    public MercenaryGenerator mercenaryGenerator;
    public HealingManager healingManager;
    public MerchantInventory merchantInventory;
    public MerchantData merchantData;
    public MerchantStatusAndQuestController merchantStatusAndQuestController;
    public TownProgressState townProgressState;
    public DailyResultController dailyResultController;
    public BattleManager battleManager;
}

public sealed class HirePartyCallbacks
{
    public Action<Button> setContractSelectButton;
}

public sealed class HirePartyNavigation
{
    public Action<MercenaryDataSO> showFixedContractDetails;
    public Action<MercenaryInstance> showGeneratedContractDetails;
    public Action<MercenaryInstance> showCharacterDetails;
    public UnityAction showQuestOverlay;
    public UnityAction showTransportOverlay;
    public UnityAction showExpeditionOverlay;
    public UnityAction showRemoteSaleOverlay;
    public Action<MercenaryInstance> requestShowContractChangeConfirmation;
    public Func<MercenaryInstance, bool> requestCanOpenContractChangeConfirmation;
    public Action<MercenaryInstance> requestShowReleaseConfirmation;
    public Action<RectTransform> refreshPage;
    public Action<RectTransform, Button> switchToPage;
    public Action showTownMap;
    public Action showExpeditionManagementOverlay;
    public Action refreshUI;
    public Action tryUnlockHiddenIsland;
}

public sealed class HirePartyPresenter
{
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;
    private static readonly Color ButtonTextColor = UITheme.ButtonTextColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color RowColor = UITheme.RowColor;
    private static readonly Color WoodButtonColor = UITheme.WoodButtonColor;
    private static readonly Color FrameColor = UITheme.FrameColor;
    private static readonly Color ImportantButtonColor = UITheme.ImportantButtonColor;
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView activeView;
    private readonly UIPageRouter pageRouter;
    private readonly HireAndPartyController hireAndPartyController;
    private readonly MercenaryHireManager hireManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly MercenaryGenerator mercenaryGenerator;
    private readonly HealingManager healingManager;
    private readonly MerchantInventory merchantInventory;
    private readonly MerchantData merchantData;
    private readonly MerchantStatusAndQuestController merchantStatusAndQuestController;
    private readonly RectTransform hirePage;
    private readonly RectTransform companyPage;
    private readonly RectTransform partyPage;
    private readonly RectTransform healPage;
    private readonly RectTransform jobChangePage;
    private readonly RectTransform overlayRoot;
    private readonly Font uiFont;
    private readonly Font uiBodyFont;
    private readonly Func<Text> statusTextProvider;
    private readonly Func<Button> hireTabButtonProvider;
    private readonly Func<Button> companyTabButtonProvider;
    private readonly Func<Button> partyTabButtonProvider;
    private readonly Func<Button> healTabButtonProvider;
    private readonly Func<Button> startBattleButtonProvider;
    private readonly Action<MercenaryDataSO> showFixedContractDetails;
    private readonly Action<MercenaryInstance> showGeneratedContractDetails;
    private readonly Action<MercenaryInstance> showCharacterDetails;
    private readonly UnityAction showQuestOverlay;
    private readonly UnityAction showTransportOverlay;
    private readonly UnityAction showExpeditionOverlay;
    private readonly UnityAction showRemoteSaleOverlay;
    private readonly Action<MercenaryInstance> requestShowContractChangeConfirmation;
    private readonly Func<MercenaryInstance, bool> requestCanOpenContractChangeConfirmation;
    private readonly Action<MercenaryInstance> requestShowReleaseConfirmation;
    private readonly SimpleMercenaryHireUIView.ReleaseConfirmationReferences releaseConfirmation;
    private readonly SimpleMercenaryHireUIView.ContractChangeReferences contractChange;
    private readonly Action<Button> setContractSelectButton;
    private readonly Action<RectTransform> refreshPage;
    private readonly TownProgressState townProgressState;
    private readonly DailyResultController dailyResultController;
    private readonly BattleManager battleManager;
    private readonly SimpleMercenaryHireUIView.PromotionPreviewReferences promotionPreview;
    private readonly Action<RectTransform, Button> switchToPage;
    private readonly Action showTownMap;
    private readonly Action showExpeditionManagementOverlay;
    private readonly Action refreshUI;
    private readonly Action tryUnlockHiddenIsland;

    public HirePartyPresenter(
        HirePartyViewDependencies view,
        HirePartyDomainDependencies domain,
        HirePartyCallbacks callbacks,
        HirePartyNavigation navigation)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));
        if (domain == null) throw new ArgumentNullException(nameof(domain));
        if (callbacks == null) throw new ArgumentNullException(nameof(callbacks));
        if (navigation == null) throw new ArgumentNullException(nameof(navigation));
        this.factory = view.factory ?? throw new ArgumentNullException(nameof(view.factory));
        if (view.activeView == null) throw new ArgumentNullException(nameof(view.activeView));
        if (view.pageRouter == null) throw new ArgumentNullException(nameof(view.pageRouter));
        this.hireAndPartyController = domain.hireAndPartyController ?? throw new ArgumentNullException(nameof(domain.hireAndPartyController));
        if (domain.hireManager == null) throw new ArgumentNullException(nameof(domain.hireManager));
        if (domain.partyManager == null) throw new ArgumentNullException(nameof(domain.partyManager));
        if (domain.mercenaryGenerator == null) throw new ArgumentNullException(nameof(domain.mercenaryGenerator));
        if (domain.healingManager == null) throw new ArgumentNullException(nameof(domain.healingManager));
        if (domain.merchantInventory == null) throw new ArgumentNullException(nameof(domain.merchantInventory));
        if (domain.merchantData == null) throw new ArgumentNullException(nameof(domain.merchantData));
        this.merchantStatusAndQuestController = domain.merchantStatusAndQuestController ?? throw new ArgumentNullException(nameof(domain.merchantStatusAndQuestController));
        if (view.hirePage == null) throw new ArgumentNullException(nameof(view.hirePage));
        if (view.companyPage == null) throw new ArgumentNullException(nameof(view.companyPage));
        if (view.partyPage == null) throw new ArgumentNullException(nameof(view.partyPage));
        if (view.healPage == null) throw new ArgumentNullException(nameof(view.healPage));
        if (view.jobChangePage == null) throw new ArgumentNullException(nameof(view.jobChangePage));
        if (view.overlayRoot == null) throw new ArgumentNullException(nameof(view.overlayRoot));
        if (view.uiFont == null) throw new ArgumentNullException(nameof(view.uiFont));
        if (view.uiBodyFont == null) throw new ArgumentNullException(nameof(view.uiBodyFont));
        // These UI elements are created later in BuildUI. Validate providers,
        // but never invoke them during presenter construction.
        if (view.statusTextProvider == null) throw new ArgumentNullException(nameof(view.statusTextProvider));
        if (view.hireTabButtonProvider == null) throw new ArgumentNullException(nameof(view.hireTabButtonProvider));
        if (view.companyTabButtonProvider == null) throw new ArgumentNullException(nameof(view.companyTabButtonProvider));
        if (view.partyTabButtonProvider == null) throw new ArgumentNullException(nameof(view.partyTabButtonProvider));
        if (view.healTabButtonProvider == null) throw new ArgumentNullException(nameof(view.healTabButtonProvider));
        if (view.startBattleButtonProvider == null) throw new ArgumentNullException(nameof(view.startBattleButtonProvider));
        this.activeView = view.activeView;
        this.pageRouter = view.pageRouter;
        this.hireManager = domain.hireManager;
        this.partyManager = domain.partyManager;
        this.mercenaryGenerator = domain.mercenaryGenerator;
        this.healingManager = domain.healingManager;
        this.merchantInventory = domain.merchantInventory;
        this.merchantData = domain.merchantData;
        this.hirePage = view.hirePage;
        this.companyPage = view.companyPage;
        this.partyPage = view.partyPage;
        this.healPage = view.healPage;
        this.jobChangePage = view.jobChangePage;
        this.overlayRoot = view.overlayRoot;
        this.uiFont = view.uiFont;
        this.uiBodyFont = view.uiBodyFont;
        this.statusTextProvider = view.statusTextProvider;
        this.hireTabButtonProvider = view.hireTabButtonProvider;
        this.companyTabButtonProvider = view.companyTabButtonProvider;
        this.partyTabButtonProvider = view.partyTabButtonProvider;
        this.healTabButtonProvider = view.healTabButtonProvider;
        this.startBattleButtonProvider = view.startBattleButtonProvider;
        this.showFixedContractDetails = navigation.showFixedContractDetails ?? throw new ArgumentNullException(nameof(navigation.showFixedContractDetails));
        this.showGeneratedContractDetails = navigation.showGeneratedContractDetails ?? throw new ArgumentNullException(nameof(navigation.showGeneratedContractDetails));
        this.showCharacterDetails = navigation.showCharacterDetails ?? throw new ArgumentNullException(nameof(navigation.showCharacterDetails));
        this.showQuestOverlay = navigation.showQuestOverlay ?? throw new ArgumentNullException(nameof(navigation.showQuestOverlay));
        this.showTransportOverlay = navigation.showTransportOverlay ?? throw new ArgumentNullException(nameof(navigation.showTransportOverlay));
        this.showExpeditionOverlay = navigation.showExpeditionOverlay ?? throw new ArgumentNullException(nameof(navigation.showExpeditionOverlay));
        this.showRemoteSaleOverlay = navigation.showRemoteSaleOverlay ?? throw new ArgumentNullException(nameof(navigation.showRemoteSaleOverlay));
        this.requestShowContractChangeConfirmation = navigation.requestShowContractChangeConfirmation ?? throw new ArgumentNullException(nameof(navigation.requestShowContractChangeConfirmation));
        this.requestCanOpenContractChangeConfirmation = navigation.requestCanOpenContractChangeConfirmation ?? throw new ArgumentNullException(nameof(navigation.requestCanOpenContractChangeConfirmation));
        this.requestShowReleaseConfirmation = navigation.requestShowReleaseConfirmation ?? throw new ArgumentNullException(nameof(navigation.requestShowReleaseConfirmation));
        this.releaseConfirmation = view.releaseConfirmation ?? throw new ArgumentNullException(nameof(view.releaseConfirmation));
        this.contractChange = view.contractChange ?? throw new ArgumentNullException(nameof(view.contractChange));
        this.setContractSelectButton = callbacks.setContractSelectButton ?? throw new ArgumentNullException(nameof(callbacks.setContractSelectButton));
        this.refreshPage = navigation.refreshPage ?? throw new ArgumentNullException(nameof(navigation.refreshPage));
        if (domain.townProgressState == null) throw new ArgumentNullException(nameof(domain.townProgressState));
        if (domain.dailyResultController == null) throw new ArgumentNullException(nameof(domain.dailyResultController));
        if (domain.battleManager == null) throw new ArgumentNullException(nameof(domain.battleManager));
        this.townProgressState = domain.townProgressState;
        this.dailyResultController = domain.dailyResultController;
        this.battleManager = domain.battleManager;
        this.promotionPreview = view.promotionPreview ?? throw new ArgumentNullException(nameof(view.promotionPreview));
        this.switchToPage = navigation.switchToPage ?? throw new ArgumentNullException(nameof(navigation.switchToPage));
        this.showTownMap = navigation.showTownMap ?? throw new ArgumentNullException(nameof(navigation.showTownMap));
        this.showExpeditionManagementOverlay = navigation.showExpeditionManagementOverlay ?? throw new ArgumentNullException(nameof(navigation.showExpeditionManagementOverlay));
        this.refreshUI = navigation.refreshUI ?? throw new ArgumentNullException(nameof(navigation.refreshUI));
        this.tryUnlockHiddenIsland = navigation.tryUnlockHiddenIsland ?? throw new ArgumentNullException(nameof(navigation.tryUnlockHiddenIsland));
    }

    public void BuildHirePage()
    {
        if (activeView.HasHireCompanyLayout) { BindHirePageLayout(activeView.HireCompany); pageRouter.Register(hirePage); refreshPage(hirePage); return; }
        CreateText(hirePage, "螂醍ｴ・庄閭ｽ縺ｪ蛯ｭ蜈ｵ", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), Vector2.zero, ParchmentMutedColor);
        Button contractSelectButton = CreateActionButton(hirePage, "螂醍ｴ・ 譌･髮・＞", hireAndPartyController.CycleHireContract);
        RectTransform contractRect = contractSelectButton.GetComponent<RectTransform>(); contractRect.anchorMin = contractRect.anchorMax = new Vector2(1f, 1f); contractRect.pivot = new Vector2(1f, 1f); contractRect.sizeDelta = new Vector2(160f, 38f); contractRect.anchoredPosition = new Vector2(0f, -4f);
        RectTransform viewport = CreateViewport("Hire Viewport", hirePage, new Vector2(0f, -52f));
        RectTransform hireList = CreateList("Hire List", viewport);
        ScrollRect scrollRect = ConfigureScrollRect(viewport, hireList);
        HirePageUI pageUI = hirePage.GetComponent<HirePageUI>() ?? hirePage.gameObject.AddComponent<HirePageUI>();
        pageUI.Initialize(hirePage.GetComponentInChildren<Text>(), contractSelectButton, scrollRect, hireList);
        ConfigureHirePage(pageUI); ConfigureHireListPage(pageUI); setContractSelectButton(contractSelectButton);
        pageRouter.Register(hirePage); refreshPage(hirePage);
    }

    public void BuildCompanyPage()
    {
        if (activeView.HasHireCompanyLayout) { BindCompanyPageLayout(activeView.HireCompany); pageRouter.Register(companyPage); return; }
        CreateText(companyPage, "髮・畑貂医∩蛯ｭ蜈ｵ", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), Vector2.zero, ParchmentMutedColor);
        Button questButton = CreateActionButton(companyPage, "萓晞ｼ", showQuestOverlay); SetTopRight(questButton, 0f);
        Button transportButton = CreateActionButton(companyPage, "輸送管理", showTransportOverlay); SetTopRight(transportButton, -118f); transportButton.gameObject.SetActive(false);
        Button expeditionButton = CreateActionButton(companyPage, "遠征管理", showExpeditionOverlay); expeditionButton.name = "Expedition Button"; SetTopRight(expeditionButton, -236f); expeditionButton.transform.SetAsLastSibling(); expeditionButton.gameObject.SetActive(true);
        Button remoteSaleButton = CreateActionButton(companyPage, "蜈ｨ逕ｺ蛟牙ｺｫ", showRemoteSaleOverlay); remoteSaleButton.name = "Remote Sale Button"; SetTopRight(remoteSaleButton, -354f);
        RectTransform viewport = CreateViewport("Company Viewport", companyPage, new Vector2(0f, -44f));
        RectTransform companyList = CreateList("Company Scroll Content", viewport);
        ScrollRect scrollRect = ConfigureScrollRect(viewport, companyList);
        CompanyPageUI pageUI = companyPage.GetComponent<CompanyPageUI>() ?? companyPage.gameObject.AddComponent<CompanyPageUI>();
        pageUI.Initialize(companyPage.GetComponentInChildren<Text>(), questButton, scrollRect, companyList);
        ConfigureCompanyPage(pageUI); ConfigureCompanyListPage(pageUI); pageRouter.Register(companyPage);
    }

    public void BuildPartyPage()
    {
        Text title = CreateText(partyPage, "謗｢邏｢繝代・繝・ぅ繝ｼ", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), Vector2.zero, ParchmentMutedColor);
        RectTransform partyList = CreateUIObject("Party List", partyPage); partyList.anchorMin = Vector2.zero; partyList.anchorMax = Vector2.one; partyList.offsetMin = Vector2.zero; partyList.offsetMax = new Vector2(0f, -44f);
        PartyPageUI pageUI = partyPage.GetComponent<PartyPageUI>() ?? partyPage.gameObject.AddComponent<PartyPageUI>();
        pageUI.Initialize(title, partyList); pageUI.Configure(uiBodyFont, ParchmentMutedColor, MutedTextColor, ButtonTextColor, RowColor, WoodButtonColor, FrameColor, null); ConfigurePartyListPage(pageUI); pageRouter.Register(partyPage);
    }

    public void BuildHealPage()
    {
        Text title = CreateText(healPage, "治療所", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);
        Text description = CreateText(healPage, $"全回復費用: 失ったHP 1につき {healingManager.HealCostPerHP} G。" + $"戦闘不能の再活性治療は{healingManager.IncapacitatedCostMultiplier}倍+" + $"{healingManager.RevivalBaseCost} G。日送りで毎日 " + $"{healingManager.NaturalHealPerDay} HP回復します。", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -72f), new Vector2(0f, -42f), ParchmentMutedColor);
        RectTransform viewport = CreateViewport("Heal Viewport", healPage, new Vector2(0f, -86f));
        RectTransform healList = CreateList("Heal List", viewport);
        ConfigureScrollRect(viewport, healList);
        HealPageUI pageUI = healPage.GetComponent<HealPageUI>() ?? healPage.gameObject.AddComponent<HealPageUI>();
        pageUI.Initialize(title, description, healList);
        pageUI.Configure(uiBodyFont, ParchmentMutedColor, MutedTextColor, ButtonTextColor, RowColor, WoodButtonColor, FrameColor, null);
        ConfigureHealListPage(pageUI);
        pageRouter.Register(healPage);
    }

    private void BindHirePageLayout(SimpleMercenaryHireUIView.HireCompanyReferences layout)
    {
        HirePageUI pageUI = layout.GetOrCreateHirePageUI(); ConfigureHirePage(pageUI); ConfigureHireListPage(pageUI); setContractSelectButton(pageUI.ContractButton);
    }

    private void BindCompanyPageLayout(SimpleMercenaryHireUIView.HireCompanyReferences layout)
    {
        CompanyPageUI pageUI = layout.GetOrCreateCompanyPageUI(); ConfigureCompanyPage(pageUI); ConfigureCompanyListPage(pageUI);
        if (companyPage.Find("Remote Sale Button") == null) { Button remoteSaleButton = CreateActionButton(companyPage, "蜈ｨ逕ｺ蛟牙ｺｫ", showRemoteSaleOverlay); remoteSaleButton.name = "Remote Sale Button"; SetTopRight(remoteSaleButton, -354f); }
    }

    private void ConfigureHirePage(HirePageUI pageUI) => pageUI.Configure(uiBodyFont, uiFont, ParchmentMutedColor, ButtonTextColor, MutedTextColor, RowColor, WoodButtonColor, FrameColor, hireAndPartyController.CycleHireContract, null);
    private void ConfigureCompanyPage(CompanyPageUI pageUI) => pageUI.Configure(uiBodyFont, uiFont, ParchmentMutedColor, ButtonTextColor, MutedTextColor, RowColor, WoodButtonColor, FrameColor, showQuestOverlay, null);
    private void ConfigureHireListPage(HirePageUI pageUI) => pageUI.ConfigureHireList(hireAndPartyController.ResetHireListTracking, () => mercenaryGenerator.UniqueCandidates, hireAndPartyController.ShouldShowFixedHireCandidate, () => mercenaryGenerator.Candidates, candidate => candidate != null, hireAndPartyController.GetUnlockedContractType, () => hireManager.GetSelectedContractSuccessRate(), candidate => hireManager.GetInitialContractCost(candidate, hireManager.SelectedContract), candidate => hireManager.GetInitialContractCost(candidate, hireManager.SelectedContract), hireAndPartyController.CanHireFixedCandidate, candidate => hireManager.CanAfford(candidate), hireAndPartyController.Hire, hireAndPartyController.HireGeneratedCandidate, hireAndPartyController.RegisterFixedHireButton, hireAndPartyController.RegisterGeneratedHireButton, showFixedContractDetails, showGeneratedContractDetails);
    private void ConfigureCompanyListPage(CompanyPageUI pageUI) => pageUI.ConfigureCompanyList(hireAndPartyController.GetCompanyMercenaries, mercenary => partyManager.Contains(mercenary), mercenary => hireManager.GetRenewalCost(mercenary), hireAndPartyController.TogglePartyMember, showCharacterDetails, merchantStatusAndQuestController.RenewContract, requestShowContractChangeConfirmation, requestCanOpenContractChangeConfirmation, requestShowReleaseConfirmation, mercenary => MercenaryDutyService.GetDuty(mercenary.InstanceId) == MercenaryDuty.RoadTransit, mercenary => MercenaryDutyService.GetDuty(mercenary.InstanceId) == MercenaryDuty.Expedition);
    private void ConfigurePartyListPage(PartyPageUI pageUI) => pageUI.ConfigurePartyList(() => partyManager.MaxPartySize, () => partyManager.Members, hireAndPartyController.RemovePartyMember);
    private void ConfigureHealListPage(HealPageUI pageUI) => pageUI.ConfigureHealList(() => healingManager.GetMercenariesAtCurrentTown(), mercenary => healingManager.GetMissingHP(mercenary), mercenary => healingManager.GetFullHealCost(mercenary), mercenary => healingManager.CanHeal(mercenary), hireAndPartyController.HealMercenary, showCharacterDetails, mercenary => healingManager.GetMissingHP(mercenary) > 0 || mercenary.IsIncapacitated, GetHealingUnavailableReason);

    public void BuildReleaseConfirmationOverlay()
    {
        releaseConfirmation.overlay = CreateOverlay("Release Confirmation Overlay");
        RectTransform window = CreateConfirmationWindow("Release Confirmation Window", releaseConfirmation.overlay, "契約を解除しますか？");
        releaseConfirmation.text = CreateText(window, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(36f, -285f), new Vector2(-36f, -80f), ParchmentTextColor);
        Button confirmButton = CreateActionButton(window, "契約を解除する", ConfirmRelease);
        SetConfirmationButtonPosition(confirmButton, new Vector2(-110f, 26f), new Vector2(200f, 48f));
        confirmButton.targetGraphic.color = ImportantButtonColor;
        Button cancelButton = CreateActionButton(window, "やめる", HideReleaseConfirmation);
        SetConfirmationButtonPosition(cancelButton, new Vector2(110f, 26f), new Vector2(180f, 48f));
        releaseConfirmation.overlay.gameObject.SetActive(false);
    }

    public void BuildContractChangeConfirmationOverlay()
    {
        contractChange.confirmationOverlay = CreateOverlay("Contract Change Confirmation Overlay");
        RectTransform window = CreateConfirmationWindow("Contract Change Confirmation Window", contractChange.confirmationOverlay, "契約を変更しますか？");
        contractChange.confirmationText = CreateText(window, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(36f, -285f), new Vector2(-36f, -80f), ParchmentTextColor);
        contractChange.confirmButton = CreateActionButton(window, "契約を変更する", ConfirmContractChange);
        SetConfirmationButtonPosition(contractChange.confirmButton, new Vector2(-110f, 26f), new Vector2(200f, 48f));
        contractChange.confirmButton.targetGraphic.color = ImportantButtonColor;
        Button cancelButton = CreateActionButton(window, "やめる", HideContractChangeConfirmation);
        SetConfirmationButtonPosition(cancelButton, new Vector2(110f, 26f), new Vector2(180f, 48f));
        contractChange.confirmationOverlay.gameObject.SetActive(false);
    }

    public void ShowContractChangeConfirmation(MercenaryInstance mercenary)
    {
        if (!TryGetNextContractChangeTarget(mercenary, out MercenaryContractType target)) { SetStatus("専属契約は最上位です。"); return; }
        int cost = hireManager.GetInitialContractCost(mercenary, target);
        MercenaryHireManager.ContractChangeUnavailableReason reason = hireManager.GetContractChangeUnavailableReason(mercenary, target);
        contractChange.mercenary = mercenary; contractChange.target = target;
        contractChange.confirmationText.text = mercenary.MercenaryName + "\n" + JapaneseDisplayText.GetContractType(mercenary.ContractType) + " → " + JapaneseDisplayText.GetContractType(target) + "\n" + "新しい期限: " + GetContractEndText(target) + "\n" + "必要費用: " + cost + "G\n" + "新契約金を全額支払います。" + (reason == MercenaryHireManager.ContractChangeUnavailableReason.None ? string.Empty : "\n" + GetContractChangeUnavailableMessage(reason, cost));
        contractChange.confirmButton.interactable = reason == MercenaryHireManager.ContractChangeUnavailableReason.None;
        contractChange.confirmationOverlay.SetAsLastSibling(); contractChange.confirmationOverlay.gameObject.SetActive(true);
    }

    public bool CanOpenContractChangeConfirmation(MercenaryInstance mercenary) => TryGetNextContractChangeTarget(mercenary, out MercenaryContractType target) && hireManager.GetContractChangeUnavailableReason(mercenary, target) == MercenaryHireManager.ContractChangeUnavailableReason.None;

    public void ShowReleaseConfirmation(MercenaryInstance mercenary)
    {
        string unavailableReason = GetReleaseUnavailableReason(mercenary);
        if (!string.IsNullOrEmpty(unavailableReason)) { SetStatus(unavailableReason); return; }
        releaseConfirmation.mercenary = mercenary; releaseConfirmation.text.text = BuildReleaseConfirmationText(mercenary);
        releaseConfirmation.overlay.SetAsLastSibling(); releaseConfirmation.overlay.gameObject.SetActive(true);
    }

    private void ConfirmContractChange()
    {
        MercenaryInstance mercenary = contractChange.mercenary; MercenaryContractType target = contractChange.target;
        MercenaryHireManager.ContractChangeUnavailableReason reason = hireManager.GetContractChangeUnavailableReason(mercenary, target);
        if (reason != MercenaryHireManager.ContractChangeUnavailableReason.None) { SetStatus(GetContractChangeUnavailableMessage(reason, hireManager.GetInitialContractCost(mercenary, target))); return; }
        HideContractChangeConfirmation();
        if (hireManager.TryChangeContract(mercenary, target)) { SetStatus(mercenary.MercenaryName + "の契約を変更しました。"); refreshPage(companyPage); return; }
        SetStatus("契約を変更できませんでした。");
    }

    private void HideContractChangeConfirmation() { contractChange.confirmationOverlay?.gameObject.SetActive(false); contractChange.mercenary = null; }
    private void ConfirmRelease()
    {
        MercenaryInstance mercenary = releaseConfirmation.mercenary; HideReleaseConfirmation();
        string unavailableReason = GetReleaseUnavailableReason(mercenary);
        if (string.IsNullOrEmpty(unavailableReason)) hireAndPartyController.ReleaseMercenary(mercenary); else SetStatus(unavailableReason);
    }
    private void HideReleaseConfirmation() { releaseConfirmation.overlay?.gameObject.SetActive(false); releaseConfirmation.mercenary = null; }
    private string GetHealingUnavailableReason(MercenaryInstance mercenary) { int cost = healingManager.GetFullHealCost(mercenary); return merchantData.Gold >= cost ? string.Empty : $"資金不足: あと {cost - merchantData.Gold}G"; }
    private string GetReleaseUnavailableReason(MercenaryInstance mercenary)
    {
        if (mercenary == null) return "契約を解除できる傭兵が選択されていません";
        MercenaryDuty duty = MercenaryDutyService.GetDuty(mercenary.InstanceId);
        if (duty == MercenaryDuty.Training) return "修練中の傭兵は契約を解除できません";
        if (duty == MercenaryDuty.RoadTransit) return "街道移動中の傭兵は契約を解除できません";
        return duty == MercenaryDuty.Expedition ? "別動隊中の傭兵は契約を解除できません" : string.Empty;
    }
    private static string BuildReleaseConfirmationText(MercenaryInstance mercenary)
    {
        bool hasEquipment = mercenary.GetEquippedItem(EquipmentSlot.Weapon) != null || mercenary.GetEquippedItem(EquipmentSlot.Armor) != null || mercenary.GetEquippedItem(EquipmentSlot.Accessory) != null;
        return mercenary.MercenaryName + "\n" + "レベル " + mercenary.Level + "  |  " + JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass) + (hasEquipment ? "\n装備していた武具は傭兵の所在町の倉庫へ戻ります。" : string.Empty) + "\n契約を解除すると元に戻せません。";
    }
    private static bool TryGetNextContractChangeTarget(MercenaryInstance mercenary, out MercenaryContractType target) { target = MercenaryContractType.Local; if (mercenary == null || mercenary.ContractType == MercenaryContractType.Exclusive) return false; target = mercenary.ContractType == MercenaryContractType.Local ? MercenaryContractType.Temporary : MercenaryContractType.Exclusive; return true; }
    private static string GetContractEndText(MercenaryContractType contractType) { switch (contractType) { case MercenaryContractType.Local: return "当日"; case MercenaryContractType.Temporary: return "7日間"; default: return "無期限"; } }
    private static string GetContractChangeUnavailableMessage(MercenaryHireManager.ContractChangeUnavailableReason reason, int cost) { switch (reason) { case MercenaryHireManager.ContractChangeUnavailableReason.ContractLocked: return "変更先の契約が未解放です。"; case MercenaryHireManager.ContractChangeUnavailableReason.InTraining: return "修練中の傭兵は契約を変更できません。"; case MercenaryHireManager.ContractChangeUnavailableReason.InTransit: return "街道移動中の傭兵は契約を変更できません。"; case MercenaryHireManager.ContractChangeUnavailableReason.OnExpedition: return "別動隊中の傭兵は契約を変更できません。"; case MercenaryHireManager.ContractChangeUnavailableReason.InsufficientGold: return "資金不足: " + cost + "G必要です。"; default: return "現在、この契約へ変更できません。"; } }
    private RectTransform CreateOverlay(string name) { RectTransform overlay = CreateUIObject(name, overlayRoot); overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = Vector2.zero; overlay.offsetMax = Vector2.zero; overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f); return overlay; }
    private RectTransform CreateConfirmationWindow(string name, RectTransform overlay, string title) { RectTransform window = CreateUIObject(name, overlay); window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f); window.sizeDelta = new Vector2(600f, 390f); SimpleMercenaryHireUIFactory.ApplyParchmentPanel(window.gameObject.AddComponent<Image>()); CreateText(window, title, 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(28f, -72f), new Vector2(-28f, -22f), ParchmentTextColor); return window; }
    private static void SetConfirmationButtonPosition(Button button, Vector2 position, Vector2 size) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f); rect.sizeDelta = size; rect.anchoredPosition = position; }
    private void SetStatus(string message) { Text statusText = statusTextProvider(); if (statusText != null) statusText.text = message; }
    private static void SetTopRight(Button button, float x) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f); rect.sizeDelta = new Vector2(110f, 38f); rect.anchoredPosition = new Vector2(x, -4f); }
    private static RectTransform CreateViewport(string name, RectTransform page, Vector2 offsetMax) { RectTransform viewport = CreateUIObject(name, page); viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = Vector2.zero; viewport.offsetMax = offsetMax; Image image = viewport.gameObject.AddComponent<Image>(); image.color = new Color(0f, 0f, 0f, 0.01f); Mask mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false; return viewport; }
    private static RectTransform CreateList(string name, RectTransform viewport) { RectTransform list = CreateUIObject(name, viewport); list.anchorMin = new Vector2(0f, 1f); list.anchorMax = new Vector2(1f, 1f); list.pivot = new Vector2(0.5f, 1f); list.anchoredPosition = Vector2.zero; return list; }
    private static ScrollRect ConfigureScrollRect(RectTransform viewport, RectTransform content) { ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>(); scrollRect.content = content; scrollRect.viewport = viewport; scrollRect.horizontal = false; scrollRect.vertical = true; scrollRect.movementType = ScrollRect.MovementType.Clamped; scrollRect.scrollSensitivity = 28f; return scrollRect; }
    public void BuildJobChangePage()
    {
        Text title = CreateText(jobChangePage, $"転職神殿（転職可能 Lv{MercenaryClassProgression.PromotionLevel}）", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, -34f), Vector2.zero, ParchmentTextColor);
        RectTransform viewport = CreateUIObject("Job Change Viewport", jobChangePage);
        viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = Vector2.zero; viewport.offsetMax = new Vector2(0f, -48f);
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false;
        RectTransform jobChangeList = CreateUIObject("Job Change List", viewport);
        jobChangeList.anchorMin = new Vector2(0f, 1f); jobChangeList.anchorMax = new Vector2(1f, 1f); jobChangeList.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = jobChangeList; scroll.viewport = viewport; scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 28f;
        JobChangePageUI pageUI = jobChangePage.GetComponent<JobChangePageUI>() ?? jobChangePage.gameObject.AddComponent<JobChangePageUI>();
        pageUI.Initialize(title, scroll, jobChangeList);
        pageUI.Configure(uiFont, ParchmentTextColor, MutedTextColor, ButtonTextColor, RowColor, WoodButtonColor, FrameColor, null, 17);
        pageUI.ConfigureJobChangeList(hireAndPartyController.GetPromotionCandidates, hireAndPartyController.ShouldShowSpecialPromotion, hireAndPartyController.PromoteMercenary, ShowPromotionPreview);
        pageRouter.Register(jobChangePage);
    }

    public void BuildPromotionPreviewOverlay()
    {
        promotionPreview.overlay = CreateUIObject("Promotion Preview Overlay", overlayRoot);
        promotionPreview.overlay.gameObject.SetActive(false); promotionPreview.overlay.anchorMin = Vector2.zero; promotionPreview.overlay.anchorMax = Vector2.one; promotionPreview.overlay.offsetMin = Vector2.zero; promotionPreview.overlay.offsetMax = Vector2.zero;
        promotionPreview.overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Promotion Preview Window", promotionPreview.overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f); window.sizeDelta = new Vector2(700f, 520f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "転職確認", 26, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(28f, -68f), new Vector2(-28f, -18f), ParchmentTextColor);
        promotionPreview.text = CreateText(window, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(34f, -360f), new Vector2(-34f, -82f), ParchmentTextColor);
        promotionPreview.reasonText = CreateText(window, string.Empty, 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(34f, -414f), new Vector2(-34f, -362f), MutedTextColor);
        promotionPreview.confirmButton = CreateActionButton(window, "転職する", ConfirmPromotionPreview);
        RectTransform confirmRect = promotionPreview.confirmButton.GetComponent<RectTransform>(); confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot = new Vector2(0.5f, 0f); confirmRect.sizeDelta = new Vector2(180f, 48f); confirmRect.anchoredPosition = new Vector2(-105f, 25f);
        Button cancel = CreateActionButton(window, "キャンセル", HidePromotionPreview);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>(); cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot = new Vector2(0.5f, 0f); cancelRect.sizeDelta = new Vector2(180f, 48f); cancelRect.anchoredPosition = new Vector2(105f, 25f);
    }

    private void ShowPromotionPreview(MercenaryInstance mercenary, MercenaryClass target)
    {
        promotionPreview.mercenary = mercenary; promotionPreview.target = target;
        PromotionPreview preview = new PromotionPreview(mercenary, target);
        bool special = target == MercenaryClassProgression.GetSpecialClass(mercenary.MercenaryClass);
        ItemDataSO certificate = special && !mercenary.IsUnique ? hireAndPartyController.GetSpecialJobCertificate() : null;
        int certificateCount = certificate != null ? merchantInventory.GetItemAmount(certificate) : 0;
        bool canPromote = mercenary.CanPromote && (!special || mercenary.IsUnique || certificateCount > 0);
        promotionPreview.text.text = BuildPromotionPreviewText(mercenary, preview, certificate, certificateCount);
        promotionPreview.reasonText.text = canPromote ? string.Empty : certificate != null ? "転職証が不足しています。" : "転職条件を満たしていません。";
        promotionPreview.confirmButton.interactable = canPromote; promotionPreview.overlay.SetAsLastSibling(); promotionPreview.overlay.gameObject.SetActive(true);
    }

    private string BuildPromotionPreviewText(MercenaryInstance mercenary, PromotionPreview preview, ItemDataSO certificate, int certificateCount)
    {
        string equipmentWarning = BuildPromotionEquipmentWarning(mercenary, preview.TargetClass);
        string certificateText = certificate == null ? "消費する証: なし" : $"消費する証: {JapaneseDisplayText.GetItemName(certificate)} {certificateCount}/1";
        System.Collections.Generic.List<MercenarySkillDefinition> skills = MercenaryClassProgression.GetCombatSkills(preview.TargetClass);
        string skillText = "解禁予定スキル: " + string.Join("、", skills.ConvertAll(skill => skill.Name));
        return $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)} → {JapaneseDisplayText.GetMercenaryClass(preview.TargetClass)}\n" + $"HP {mercenary.MaxHP} → {preview.MaxHP} ({preview.MaxHP - mercenary.MaxHP:+#;-#;0})\n" + $"攻撃 {mercenary.Attack} → {preview.Attack} ({preview.Attack - mercenary.Attack:+#;-#;0})\n" + $"防御 {mercenary.Defense} → {preview.Defense} ({preview.Defense - mercenary.Defense:+#;-#;0})\n" + $"魔力 {mercenary.MaxMagicPower} → {preview.MaxMagicPower} ({preview.MaxMagicPower - mercenary.MaxMagicPower:+#;-#;0})\n" + $"速度 {mercenary.AttackSpeed:0.00} → {preview.AttackSpeed:0.00} ({preview.AttackSpeed - mercenary.AttackSpeed:+0.00;-0.00;0})\n" + $"レベル上限: {preview.LevelCap}  |  クリティカル {preview.CriticalRate * 100f:0}%  |  回避 {preview.EvasionRate * 100f:0}%\n" + certificateText + "\n" + skillText + "\n" + equipmentWarning;
    }

    private static string BuildPromotionEquipmentWarning(MercenaryInstance mercenary, MercenaryClass target) { System.Collections.Generic.List<string> names = new System.Collections.Generic.List<string>(); foreach (EquipmentSlot slot in new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Accessory }) { ItemDataSO item = mercenary.GetEquippedItem(slot); if (item != null && !item.CanEquip(target)) names.Add(JapaneseDisplayText.GetItemName(item)); } return names.Count == 0 ? "装備適合: 問題なし" : "装備不可になる装備: " + string.Join("、", names); }
    private void ConfirmPromotionPreview() { if (promotionPreview.mercenary == null || !promotionPreview.mercenary.CanPromote || MercenaryClassProgression.GetBaseClass(promotionPreview.target) != promotionPreview.mercenary.OriginalClass || MercenaryClassProgression.IsBaseClass(promotionPreview.target)) { HidePromotionPreview(); return; } hireAndPartyController.PromoteMercenary(promotionPreview.mercenary, promotionPreview.target); HidePromotionPreview(); }
    private void HidePromotionPreview() { if (promotionPreview.overlay != null) promotionPreview.overlay.gameObject.SetActive(false); promotionPreview.mercenary = null; }

    public void HandleMercenaryHired(MercenaryInstance mercenary) { dailyResultController.RecordMercenaryHired(mercenary); dailyResultController.CaptureMercenarySnapshot(mercenary); tryUnlockHiddenIsland(); refreshPage(companyPage); }
    public void HandleMercenaryDismissed(MercenaryInstance mercenary) { refreshPage(companyPage); refreshPage(partyPage); refreshPage(healPage); refreshPage(jobChangePage); }
    public void HandlePartyChanged() { dailyResultController.RememberDailyPartyMembers(); refreshPage(companyPage); refreshPage(partyPage); Button startBattleButton = startBattleButtonProvider(); if (startBattleButton != null && !battleManager.IsBattling) startBattleButton.interactable = partyManager.Members.Count > 0; SetStatus($"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}"); }
    public void HandleCandidatesChanged() { refreshPage(hirePage); refreshUI(); }
    public void HandleHealingChanged() { refreshPage(companyPage); refreshPage(partyPage); refreshPage(healPage); refreshUI(); }

    public void ShowHirePage() { if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex)) { showTownMap(); SetStatus($"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}には傭兵を雇用できる酒場がありません。"); return; } switchToPage(hirePage, hireTabButtonProvider()); SetStatus($"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}の雇用候補  |  Lv{mercenaryGenerator.CurrentMinimumLevel}～Lv{mercenaryGenerator.CurrentMaximumLevel}  |  雇用する傭兵を選択してください。"); }
    public void ShowCompanyPage() { switchToPage(companyPage, companyTabButtonProvider()); SetStatus($"商人Lv{merchantData.MerchantLevel} 獲得G進行 {merchantData.MerchantExperience:N0}/{merchantData.ExperienceToNextLevel:N0}  |  技能ポイント {merchantData.MerchantSkillPoints}  |  傭兵 {hireManager.HiredMercenaries.Count}人  |  雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}%"); }
    public void ShowTransportOverlay() { }
    public void ShowExpeditionOverlay() { showExpeditionManagementOverlay(); }
    public void ShowPartyPage() { switchToPage(partyPage, partyTabButtonProvider()); SetStatus($"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}"); }
    public void ShowHealPage() { switchToPage(healPage, healTabButtonProvider()); SetStatus($"治療費: 失ったHP 1につき {healingManager.HealCostPerHP} G"); }
    public void ShowJobChangePage() { if (!TownServicePolicy.IsJobChangeAvailable(townProgressState.CurrentTownIndex)) { SetStatus("転職神殿はエルド交易都市以降の町で利用できます。"); return; } switchToPage(jobChangePage, null); SetStatus($"Lv{MercenaryClassProgression.PromotionLevel}以上の基本職が転職できます。"); }
    private Text CreateText(RectTransform parent, string content, int size, FontStyle style, TextAnchor alignment, Vector2 min, Vector2 max, Color color) => factory.CreateText(parent, content, size, style, alignment, min, max, color);
    private Button CreateActionButton(RectTransform parent, string label, UnityAction action) => factory.CreateActionButton(parent, label, action);
    private static RectTransform CreateUIObject(string name, Transform parent) => SimpleMercenaryHireUIFactory.CreateUIObject(name, parent);
}
