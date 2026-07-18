using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildHirePage()
    {
        if (activeView != null && activeView.HasHireCompanyLayout)
        {
            BindHirePageLayout(activeView.HireCompany);
            pageRouter.Register(hirePage);
            RefreshPage(hirePage);
            return;
        }

        CreateText(hirePage, "契約可能な傭兵", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);

        contractSelectButton = CreateActionButton(
            hirePage,
            "契約: 日雇い",
            hireAndPartyController.CycleHireContract);
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

        HirePageUI pageUI =
            hirePage.GetComponent<HirePageUI>() ??
            hirePage.gameObject.AddComponent<HirePageUI>();
        Text title = hirePage.GetComponentInChildren<Text>();
        pageUI.Initialize(title, contractSelectButton, scrollRect, hireList);
        pageUI.Configure(
            uiBodyFont,
            uiFont,
            ParchmentMutedColor,
            ButtonTextColor,
            MutedTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            hireAndPartyController.CycleHireContract,
            null);
        ConfigureHireListPage(pageUI);
        pageRouter.Register(hirePage);
        RefreshPage(hirePage);
    }

    private void BuildCompanyPage()
    {
        if (activeView != null && activeView.HasHireCompanyLayout)
        {
            BindCompanyPageLayout(activeView.HireCompany);
            pageRouter.Register(companyPage);
            return;
        }

        CreateText(companyPage, "雇用済み傭兵", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);

        Button questButton =
            CreateActionButton(companyPage, "依頼", ShowQuestOverlay);
        RectTransform questRect = questButton.GetComponent<RectTransform>();
        questRect.anchorMin = questRect.anchorMax = new Vector2(1f, 1f);
        questRect.pivot = new Vector2(1f, 1f);
        questRect.sizeDelta = new Vector2(110f, 38f);
        questRect.anchoredPosition = new Vector2(0f, -4f);

        Button transportButton =
            CreateActionButton(companyPage, "輸送部隊", ShowTransportOverlay);
        RectTransform transportRect = transportButton.GetComponent<RectTransform>();
        transportRect.anchorMin = transportRect.anchorMax = new Vector2(1f, 1f);
        transportRect.pivot = new Vector2(1f, 1f);
        transportRect.sizeDelta = new Vector2(110f, 38f);
        transportRect.anchoredPosition = new Vector2(-118f, -4f);
        Button expeditionButton = CreateActionButton(companyPage, "遠征部隊", ShowExpeditionOverlay);
        expeditionButton.name = "Expedition Button";
        RectTransform expeditionRect = expeditionButton.GetComponent<RectTransform>();
        expeditionRect.anchorMin = expeditionRect.anchorMax = new Vector2(1f, 1f);
        expeditionRect.pivot = new Vector2(1f, 1f);
        expeditionRect.sizeDelta = new Vector2(110f, 38f);
        expeditionRect.anchoredPosition = new Vector2(-236f, -4f);
        expeditionButton.transform.SetAsLastSibling();
        Button remoteSaleButton = CreateActionButton(companyPage, "全町倉庫", ShowRemoteSaleOverlay);
        remoteSaleButton.name = "Remote Sale Button";
        RectTransform remoteSaleRect = remoteSaleButton.GetComponent<RectTransform>();
        remoteSaleRect.anchorMin = remoteSaleRect.anchorMax = new Vector2(1f, 1f);
        remoteSaleRect.pivot = new Vector2(1f, 1f);
        remoteSaleRect.sizeDelta = new Vector2(110f, 38f);
        remoteSaleRect.anchoredPosition = new Vector2(-354f, -4f);

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
        CompanyPageUI pageUI =
            companyPage.GetComponent<CompanyPageUI>() ??
            companyPage.gameObject.AddComponent<CompanyPageUI>();
        Text title = companyPage.GetComponentInChildren<Text>();
        pageUI.Initialize(
            title, questButton, scrollRect, companyList);
        pageUI.Configure(
            uiBodyFont,
            uiFont,
            ParchmentMutedColor,
            ButtonTextColor,
            MutedTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            ShowQuestOverlay,
            null);
        ConfigureCompanyListPage(pageUI);
        pageRouter.Register(companyPage);
    }

    private void BindHirePageLayout(
        SimpleMercenaryHireUIView.HireCompanyReferences layout)
    {
        HirePageUI pageUI = layout.GetOrCreateHirePageUI();
        pageUI.Configure(
            uiBodyFont,
            uiFont,
            ParchmentMutedColor,
            ButtonTextColor,
            MutedTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            hireAndPartyController.CycleHireContract,
            null);
        ConfigureHireListPage(pageUI);
        contractSelectButton = pageUI.ContractButton;
        hireList = pageUI.ListRoot;
    }

    private void BindCompanyPageLayout(
        SimpleMercenaryHireUIView.HireCompanyReferences layout)
    {
        CompanyPageUI pageUI = layout.GetOrCreateCompanyPageUI();
        pageUI.Configure(
            uiBodyFont,
            uiFont,
            ParchmentMutedColor,
            ButtonTextColor,
            MutedTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            ShowQuestOverlay,
            null);
        ConfigureCompanyListPage(pageUI);
        if (companyPage.Find("Transport Button") == null)
        {
            Button transportButton =
                CreateActionButton(companyPage, "輸送部隊", ShowTransportOverlay);
            transportButton.name = "Transport Button";
            RectTransform transportRect = transportButton.GetComponent<RectTransform>();
            transportRect.anchorMin = transportRect.anchorMax = new Vector2(1f, 1f);
            transportRect.pivot = new Vector2(1f, 1f);
            transportRect.sizeDelta = new Vector2(110f, 38f);
            transportRect.anchoredPosition = new Vector2(-118f, -4f);
        }
        if (companyPage.Find("Expedition Button") == null)
        {
            Button expeditionButton = CreateActionButton(companyPage, "遠征部隊", ShowExpeditionOverlay);
            expeditionButton.name = "Expedition Button";
            RectTransform expeditionRect = expeditionButton.GetComponent<RectTransform>();
            expeditionRect.anchorMin = expeditionRect.anchorMax = new Vector2(1f, 1f);
            expeditionRect.pivot = new Vector2(1f, 1f);
            expeditionRect.sizeDelta = new Vector2(110f, 38f);
            expeditionRect.anchoredPosition = new Vector2(-236f, -4f);
            expeditionButton.transform.SetAsLastSibling();
        }
        if (companyPage.Find("Remote Sale Button") == null)
        {
            Button remoteSaleButton = CreateActionButton(companyPage, "全町倉庫", ShowRemoteSaleOverlay);
            remoteSaleButton.name = "Remote Sale Button";
            RectTransform remoteSaleRect = remoteSaleButton.GetComponent<RectTransform>();
            remoteSaleRect.anchorMin = remoteSaleRect.anchorMax = new Vector2(1f, 1f);
            remoteSaleRect.pivot = new Vector2(1f, 1f);
            remoteSaleRect.sizeDelta = new Vector2(110f, 38f);
            remoteSaleRect.anchoredPosition = new Vector2(-354f, -4f);
        }
        companyScrollContent = pageUI.ListRoot;
        companyList = companyScrollContent;
    }

    private void BuildPartyPage()
    {
        Text title = CreateText(
            partyPage, "探索パーティー", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);

        partyList = CreateUIObject("Party List", partyPage);
        partyList.anchorMin = new Vector2(0f, 0f);
        partyList.anchorMax = new Vector2(1f, 1f);
        partyList.offsetMin = Vector2.zero;
        partyList.offsetMax = new Vector2(0f, -44f);

        PartyPageUI pageUI =
            partyPage.GetComponent<PartyPageUI>() ??
            partyPage.gameObject.AddComponent<PartyPageUI>();
        pageUI.Initialize(title, partyList);
        pageUI.Configure(
            uiBodyFont,
            ParchmentMutedColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            null);
        ConfigurePartyListPage(pageUI);
        pageRouter.Register(partyPage);
    }

    private void BuildHealPage()
    {
        Text title = CreateText(healPage, "治療所", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        Text description = CreateText(
            healPage,
            $"全回復費用: 失ったHP 1につき {healingManager.HealCostPerHP} G。" +
            $"戦闘不能の再活性治療は{healingManager.IncapacitatedCostMultiplier}倍+" +
            $"{healingManager.RevivalBaseCost} G。日送りで毎日 " +
            $"{healingManager.NaturalHealPerDay} HP回復します。",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -72f),
            new Vector2(0f, -42f),
            ParchmentMutedColor);

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

        HealPageUI pageUI =
            healPage.GetComponent<HealPageUI>() ??
            healPage.gameObject.AddComponent<HealPageUI>();
        pageUI.Initialize(title, description, healList);
        pageUI.Configure(
            uiBodyFont,
            ParchmentMutedColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            null);
        ConfigureHealListPage(pageUI);
        pageRouter.Register(healPage);
    }

    private void BuildJobChangePage()
    {
        Text title = CreateText(
            jobChangePage,
            $"転職神殿（転職可能 Lv{MercenaryClassProgression.PromotionLevel}）",
            17,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -34f),
            Vector2.zero,
            ParchmentTextColor);

        RectTransform viewport =
            CreateUIObject("Job Change Viewport", jobChangePage);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -48f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        jobChangeList = CreateUIObject("Job Change List", viewport);
        jobChangeList.anchorMin = new Vector2(0f, 1f);
        jobChangeList.anchorMax = new Vector2(1f, 1f);
        jobChangeList.pivot = new Vector2(0.5f, 1f);

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = jobChangeList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        JobChangePageUI pageUI =
            jobChangePage.GetComponent<JobChangePageUI>() ??
            jobChangePage.gameObject.AddComponent<JobChangePageUI>();
        pageUI.Initialize(title, scroll, jobChangeList);
        pageUI.Configure(
            uiFont,
            ParchmentTextColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            null,
            17);
        pageUI.ConfigureJobChangeList(
            () => hireManager.HiredMercenaries,
            hireAndPartyController.ShouldShowSpecialPromotion,
            hireAndPartyController.PromoteMercenary);
        pageRouter.Register(jobChangePage);
    }

    private void ConfigureHireListPage(HirePageUI pageUI)
    {
        pageUI.ConfigureHireList(
            hireAndPartyController.ResetHireListTracking,
            () => mercenaryGenerator.UniqueCandidates,
            hireAndPartyController.ShouldShowFixedHireCandidate,
            () => mercenaryGenerator.Candidates,
            candidate => candidate != null,
            hireAndPartyController.GetUnlockedContractType,
            () => hireManager.GetSelectedContractSuccessRate(),
            hireAndPartyController.CanHireFixedCandidate,
            candidate => hireManager.CanAfford(candidate),
            hireAndPartyController.Hire,
            hireAndPartyController.HireGeneratedCandidate,
            hireAndPartyController.RegisterFixedHireButton,
            hireAndPartyController.RegisterGeneratedHireButton);
    }

    private void ConfigureCompanyListPage(CompanyPageUI pageUI)
    {
        pageUI.ConfigureCompanyList(
            () => hireManager.HiredMercenaries,
            mercenary => partyManager.Contains(mercenary),
            mercenary => hireManager.GetRenewalCost(mercenary),
            hireAndPartyController.TogglePartyMember,
            ShowCharacterDetails,
            merchantStatusAndQuestController.RenewContract,
            hireAndPartyController.ReleaseMercenary,
            mercenary => transportManager.IsMercenaryOnTransportDuty(mercenary.InstanceId),
            mercenary => dungeonExpeditionManager.IsMercenaryOnExpeditionDuty(mercenary.InstanceId));
    }

    private void ConfigurePartyListPage(PartyPageUI pageUI)
    {
        pageUI.ConfigurePartyList(
            () => partyManager.MaxPartySize,
            () => partyManager.Members,
            hireAndPartyController.RemovePartyMember);
    }

    private void ConfigureHealListPage(HealPageUI pageUI)
    {
        pageUI.ConfigureHealList(
            () => healingManager.GetMercenariesAtCurrentTown(),
            mercenary => healingManager.GetMissingHP(mercenary),
            mercenary => healingManager.GetFullHealCost(mercenary),
            mercenary => healingManager.CanHeal(mercenary),
            hireAndPartyController.HealMercenary);
    }

    private void HandleMercenaryHired(MercenaryInstance mercenary)
    {
        dailyResultController.CaptureMercenarySnapshot(mercenary);
        TryUnlockHiddenIsland();
        RefreshPage(companyPage);
    }

    private void HandleMercenaryDismissed(MercenaryInstance mercenary)
    {
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        RefreshPage(healPage);
        RefreshPage(jobChangePage);
    }

    private void HandlePartyChanged()
    {
        dailyResultController.RememberDailyPartyMembers();
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        if (startBattleButton != null && !battleManager.IsBattling)
        {
            startBattleButton.interactable = partyManager.Members.Count > 0;
        }
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void HandleCandidatesChanged()
    {
        RefreshPage(hirePage);
        RefreshUI();
    }

    private void HandleHealingChanged()
    {
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        RefreshPage(healPage);
        RefreshUI();
    }

    private void ShowHirePage()
    {
        if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            ShowTownMap();
            statusText.text =
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}には傭兵を雇用できる酒場がありません。";
            return;
        }
        SwitchToPage(hirePage, hireTabButton);
        statusText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}の雇用候補  |  " +
            $"Lv{mercenaryGenerator.CurrentMinimumLevel}～" +
            $"Lv{mercenaryGenerator.CurrentMaximumLevel}  |  " +
            "雇用する傭兵を選択してください。";
    }

    private void ShowCompanyPage()
    {
        SwitchToPage(companyPage, companyTabButton);
        statusText.text =
            $"商人Lv{merchantData.MerchantLevel} " +
            $"獲得G進行 {merchantData.MerchantExperience:N0}/" +
            $"{merchantData.ExperienceToNextLevel:N0}  |  " +
            $"技能ポイント {merchantData.MerchantSkillPoints}  |  " +
            $"傭兵 {hireManager.HiredMercenaries.Count}人  |  " +
            $"雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}%";
    }

    private void ShowPartyPage()
    {
        SwitchToPage(partyPage, partyTabButton);
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void ShowHealPage()
    {
        SwitchToPage(healPage, healTabButton);
        statusText.text =
            $"治療費: 失ったHP 1につき {healingManager.HealCostPerHP} G";
    }

    private void ShowJobChangePage()
    {
        if (townProgressState.CurrentTownIndex != 0)
        {
            statusText.text =
                "転職神殿はエルド交易都市でのみ利用できます。";
            return;
        }

        SwitchToPage(jobChangePage);
        statusText.text =
            $"Lv{MercenaryClassProgression.PromotionLevel}以上の基本職が転職できます。";
    }

}
