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
    private readonly MerchantData merchantData;
    private readonly MerchantStatusAndQuestController merchantStatusAndQuestController;
    private readonly RectTransform hirePage;
    private readonly RectTransform companyPage;
    private readonly RectTransform partyPage;
    private readonly RectTransform healPage;
    private readonly RectTransform overlayRoot;
    private readonly Font uiFont;
    private readonly Font uiBodyFont;
    private readonly Func<Text> statusTextProvider;
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

    public HirePartyPresenter(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView activeView,
        UIPageRouter pageRouter,
        HireAndPartyController hireAndPartyController,
        MercenaryHireManager hireManager,
        MercenaryPartyManager partyManager,
        MercenaryGenerator mercenaryGenerator,
        HealingManager healingManager,
        MerchantData merchantData,
        MerchantStatusAndQuestController merchantStatusAndQuestController,
        RectTransform hirePage,
        RectTransform companyPage,
        RectTransform partyPage,
        RectTransform healPage,
        RectTransform overlayRoot,
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
        Action<MercenaryInstance> requestShowContractChangeConfirmation,
        Func<MercenaryInstance, bool> requestCanOpenContractChangeConfirmation,
        Action<MercenaryInstance> requestShowReleaseConfirmation,
        SimpleMercenaryHireUIView.ReleaseConfirmationReferences releaseConfirmation,
        SimpleMercenaryHireUIView.ContractChangeReferences contractChange,
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
        if (healingManager == null) throw new ArgumentNullException(nameof(healingManager));
        if (merchantData == null) throw new ArgumentNullException(nameof(merchantData));
        this.merchantStatusAndQuestController = merchantStatusAndQuestController ?? throw new ArgumentNullException(nameof(merchantStatusAndQuestController));
        if (hirePage == null) throw new ArgumentNullException(nameof(hirePage));
        if (companyPage == null) throw new ArgumentNullException(nameof(companyPage));
        if (partyPage == null) throw new ArgumentNullException(nameof(partyPage));
        if (healPage == null) throw new ArgumentNullException(nameof(healPage));
        if (overlayRoot == null) throw new ArgumentNullException(nameof(overlayRoot));
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
        this.healingManager = healingManager;
        this.merchantData = merchantData;
        this.hirePage = hirePage;
        this.companyPage = companyPage;
        this.partyPage = partyPage;
        this.healPage = healPage;
        this.overlayRoot = overlayRoot;
        this.uiFont = uiFont;
        this.uiBodyFont = uiBodyFont;
        this.statusTextProvider = statusTextProvider;
        this.showFixedContractDetails = showFixedContractDetails ?? throw new ArgumentNullException(nameof(showFixedContractDetails));
        this.showGeneratedContractDetails = showGeneratedContractDetails ?? throw new ArgumentNullException(nameof(showGeneratedContractDetails));
        this.showCharacterDetails = showCharacterDetails ?? throw new ArgumentNullException(nameof(showCharacterDetails));
        this.showQuestOverlay = showQuestOverlay ?? throw new ArgumentNullException(nameof(showQuestOverlay));
        this.showTransportOverlay = showTransportOverlay ?? throw new ArgumentNullException(nameof(showTransportOverlay));
        this.showExpeditionOverlay = showExpeditionOverlay ?? throw new ArgumentNullException(nameof(showExpeditionOverlay));
        this.showRemoteSaleOverlay = showRemoteSaleOverlay ?? throw new ArgumentNullException(nameof(showRemoteSaleOverlay));
        this.requestShowContractChangeConfirmation = requestShowContractChangeConfirmation ?? throw new ArgumentNullException(nameof(requestShowContractChangeConfirmation));
        this.requestCanOpenContractChangeConfirmation = requestCanOpenContractChangeConfirmation ?? throw new ArgumentNullException(nameof(requestCanOpenContractChangeConfirmation));
        this.requestShowReleaseConfirmation = requestShowReleaseConfirmation ?? throw new ArgumentNullException(nameof(requestShowReleaseConfirmation));
        this.releaseConfirmation = releaseConfirmation ?? throw new ArgumentNullException(nameof(releaseConfirmation));
        this.contractChange = contractChange ?? throw new ArgumentNullException(nameof(contractChange));
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
    private Text CreateText(RectTransform parent, string content, int size, FontStyle style, TextAnchor alignment, Vector2 min, Vector2 max, Color color) => factory.CreateText(parent, content, size, style, alignment, min, max, color);
    private Button CreateActionButton(RectTransform parent, string label, UnityAction action) => factory.CreateActionButton(parent, label, action);
    private static RectTransform CreateUIObject(string name, Transform parent) => SimpleMercenaryHireUIFactory.CreateUIObject(name, parent);
}
