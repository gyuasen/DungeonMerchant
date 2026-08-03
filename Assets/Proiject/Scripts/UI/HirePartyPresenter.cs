using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HirePartyPresenter
{
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;
    private static readonly Color ButtonTextColor = UITheme.ButtonTextColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color RowColor = UITheme.RowColor;
    private static readonly Color WoodButtonColor = UITheme.WoodButtonColor;
    private static readonly Color FrameColor = UITheme.FrameColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView activeView;
    private readonly UIPageRouter pageRouter;
    private readonly HireAndPartyController hireAndPartyController;
    private readonly MercenaryHireManager hireManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly MercenaryGenerator mercenaryGenerator;
    private readonly MerchantStatusAndQuestController merchantStatusAndQuestController;
    private readonly RectTransform hirePage;
    private readonly RectTransform companyPage;
    private readonly RectTransform partyPage;
    private readonly Font uiFont;
    private readonly Font uiBodyFont;
    private readonly Action<MercenaryDataSO> showFixedContractDetails;
    private readonly Action<MercenaryInstance> showGeneratedContractDetails;
    private readonly Action<MercenaryInstance> showCharacterDetails;
    private readonly UnityAction showQuestOverlay;
    private readonly UnityAction showTransportOverlay;
    private readonly UnityAction showExpeditionOverlay;
    private readonly UnityAction showRemoteSaleOverlay;
    private readonly Action<MercenaryInstance> showContractChangeConfirmation;
    private readonly Func<MercenaryInstance, bool> canOpenContractChangeConfirmation;
    private readonly Action<MercenaryInstance> showReleaseConfirmation;
    private readonly Action<Button> setContractSelectButton;
    private readonly Action<RectTransform> refreshPage;

    public HirePartyPresenter(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView activeView,
        UIPageRouter pageRouter,
        HireAndPartyController hireAndPartyController,
        MercenaryHireManager hireManager,
        MercenaryPartyManager partyManager,
        MercenaryGenerator mercenaryGenerator,
        MerchantStatusAndQuestController merchantStatusAndQuestController,
        RectTransform hirePage,
        RectTransform companyPage,
        RectTransform partyPage,
        Font uiFont,
        Font uiBodyFont,
        Func<Text> statusTextProvider,
        Func<Button> hireTabButtonProvider,
        Func<Button> companyTabButtonProvider,
        Func<Button> partyTabButtonProvider,
        Func<Button> healTabButtonProvider,
        Func<Button> startBattleButtonProvider,
        Action<MercenaryDataSO> showFixedContractDetails,
        Action<MercenaryInstance> showGeneratedContractDetails,
        Action<MercenaryInstance> showCharacterDetails,
        UnityAction showQuestOverlay,
        UnityAction showTransportOverlay,
        UnityAction showExpeditionOverlay,
        UnityAction showRemoteSaleOverlay,
        Action<MercenaryInstance> showContractChangeConfirmation,
        Func<MercenaryInstance, bool> canOpenContractChangeConfirmation,
        Action<MercenaryInstance> showReleaseConfirmation,
        Action<Button> setContractSelectButton,
        Action<RectTransform> refreshPage)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        if (activeView == null) throw new ArgumentNullException(nameof(activeView));
        if (pageRouter == null) throw new ArgumentNullException(nameof(pageRouter));
        this.hireAndPartyController = hireAndPartyController ?? throw new ArgumentNullException(nameof(hireAndPartyController));
        if (hireManager == null) throw new ArgumentNullException(nameof(hireManager));
        if (partyManager == null) throw new ArgumentNullException(nameof(partyManager));
        if (mercenaryGenerator == null) throw new ArgumentNullException(nameof(mercenaryGenerator));
        this.merchantStatusAndQuestController = merchantStatusAndQuestController ?? throw new ArgumentNullException(nameof(merchantStatusAndQuestController));
        if (hirePage == null) throw new ArgumentNullException(nameof(hirePage));
        if (companyPage == null) throw new ArgumentNullException(nameof(companyPage));
        if (partyPage == null) throw new ArgumentNullException(nameof(partyPage));
        if (uiFont == null) throw new ArgumentNullException(nameof(uiFont));
        if (uiBodyFont == null) throw new ArgumentNullException(nameof(uiBodyFont));
        // These UI elements are created later in BuildUI. Validate providers,
        // but never invoke them during presenter construction.
        if (statusTextProvider == null) throw new ArgumentNullException(nameof(statusTextProvider));
        if (hireTabButtonProvider == null) throw new ArgumentNullException(nameof(hireTabButtonProvider));
        if (companyTabButtonProvider == null) throw new ArgumentNullException(nameof(companyTabButtonProvider));
        if (partyTabButtonProvider == null) throw new ArgumentNullException(nameof(partyTabButtonProvider));
        if (healTabButtonProvider == null) throw new ArgumentNullException(nameof(healTabButtonProvider));
        if (startBattleButtonProvider == null) throw new ArgumentNullException(nameof(startBattleButtonProvider));
        this.activeView = activeView;
        this.pageRouter = pageRouter;
        this.hireManager = hireManager;
        this.partyManager = partyManager;
        this.mercenaryGenerator = mercenaryGenerator;
        this.hirePage = hirePage;
        this.companyPage = companyPage;
        this.partyPage = partyPage;
        this.uiFont = uiFont;
        this.uiBodyFont = uiBodyFont;
        this.showFixedContractDetails = showFixedContractDetails ?? throw new ArgumentNullException(nameof(showFixedContractDetails));
        this.showGeneratedContractDetails = showGeneratedContractDetails ?? throw new ArgumentNullException(nameof(showGeneratedContractDetails));
        this.showCharacterDetails = showCharacterDetails ?? throw new ArgumentNullException(nameof(showCharacterDetails));
        this.showQuestOverlay = showQuestOverlay ?? throw new ArgumentNullException(nameof(showQuestOverlay));
        this.showTransportOverlay = showTransportOverlay ?? throw new ArgumentNullException(nameof(showTransportOverlay));
        this.showExpeditionOverlay = showExpeditionOverlay ?? throw new ArgumentNullException(nameof(showExpeditionOverlay));
        this.showRemoteSaleOverlay = showRemoteSaleOverlay ?? throw new ArgumentNullException(nameof(showRemoteSaleOverlay));
        this.showContractChangeConfirmation = showContractChangeConfirmation ?? throw new ArgumentNullException(nameof(showContractChangeConfirmation));
        this.canOpenContractChangeConfirmation = canOpenContractChangeConfirmation ?? throw new ArgumentNullException(nameof(canOpenContractChangeConfirmation));
        this.showReleaseConfirmation = showReleaseConfirmation ?? throw new ArgumentNullException(nameof(showReleaseConfirmation));
        this.setContractSelectButton = setContractSelectButton ?? throw new ArgumentNullException(nameof(setContractSelectButton));
        this.refreshPage = refreshPage ?? throw new ArgumentNullException(nameof(refreshPage));
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
    private void ConfigureCompanyListPage(CompanyPageUI pageUI) => pageUI.ConfigureCompanyList(hireAndPartyController.GetCompanyMercenaries, mercenary => partyManager.Contains(mercenary), mercenary => hireManager.GetRenewalCost(mercenary), hireAndPartyController.TogglePartyMember, showCharacterDetails, merchantStatusAndQuestController.RenewContract, showContractChangeConfirmation, canOpenContractChangeConfirmation, showReleaseConfirmation, mercenary => MercenaryDutyService.GetDuty(mercenary.InstanceId) == MercenaryDuty.RoadTransit, mercenary => MercenaryDutyService.GetDuty(mercenary.InstanceId) == MercenaryDuty.Expedition);
    private void ConfigurePartyListPage(PartyPageUI pageUI) => pageUI.ConfigurePartyList(() => partyManager.MaxPartySize, () => partyManager.Members, hireAndPartyController.RemovePartyMember);
    private static void SetTopRight(Button button, float x) { RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f); rect.sizeDelta = new Vector2(110f, 38f); rect.anchoredPosition = new Vector2(x, -4f); }
    private static RectTransform CreateViewport(string name, RectTransform page, Vector2 offsetMax) { RectTransform viewport = CreateUIObject(name, page); viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = Vector2.zero; viewport.offsetMax = offsetMax; Image image = viewport.gameObject.AddComponent<Image>(); image.color = new Color(0f, 0f, 0f, 0.01f); Mask mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false; return viewport; }
    private static RectTransform CreateList(string name, RectTransform viewport) { RectTransform list = CreateUIObject(name, viewport); list.anchorMin = new Vector2(0f, 1f); list.anchorMax = new Vector2(1f, 1f); list.pivot = new Vector2(0.5f, 1f); list.anchoredPosition = Vector2.zero; return list; }
    private static ScrollRect ConfigureScrollRect(RectTransform viewport, RectTransform content) { ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>(); scrollRect.content = content; scrollRect.viewport = viewport; scrollRect.horizontal = false; scrollRect.vertical = true; scrollRect.movementType = ScrollRect.MovementType.Clamped; scrollRect.scrollSensitivity = 28f; return scrollRect; }
    private Text CreateText(RectTransform parent, string content, int size, FontStyle style, TextAnchor alignment, Vector2 min, Vector2 max, Color color) => factory.CreateText(parent, content, size, style, alignment, min, max, color);
    private Button CreateActionButton(RectTransform parent, string label, UnityAction action) => factory.CreateActionButton(parent, label, action);
    private static RectTransform CreateUIObject(string name, Transform parent) => SimpleMercenaryHireUIFactory.CreateUIObject(name, parent);
}
