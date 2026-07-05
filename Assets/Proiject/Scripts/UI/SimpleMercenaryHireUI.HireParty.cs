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
            RebuildHireList();
            return;
        }

        CreateText(hirePage, "契約可能な傭兵", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);

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
        if (activeView != null && activeView.HasHireCompanyLayout)
        {
            BindCompanyPageLayout(activeView.HireCompany);
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

    private void BindHirePageLayout(
        SimpleMercenaryHireUIView.HireCompanyReferences layout)
    {
        HirePageUI pageUI = layout.GetOrCreateHirePageUI();
        pageUI.Configure(
            uiBodyFont,
            uiFont,
            ParchmentMutedColor,
            ButtonTextColor,
            CycleHireContract,
            RebuildHireList);
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
            ShowQuestOverlay,
            RebuildCompanyList);
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
            uiBodyFont, ParchmentMutedColor, RebuildPartyList);
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
            $"戦闘不能は{healingManager.IncapacitatedCostMultiplier}倍+" +
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
            uiBodyFont, ParchmentMutedColor, RebuildHealList);
        pageRouter.Register(healPage);
    }

    private void BuildJobChangePage()
    {
        CreateText(
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
                ParchmentMutedColor);
            return;
        }

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
                ParchmentMutedColor);
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

    private void Hire(MercenaryDataSO candidate)
    {
        if (!TownServicePolicy.IsHiringAvailable(currentTownIndex))
        {
            statusText.text =
                $"{TownNames[currentTownIndex]}では傭兵を雇用できません。";
            return;
        }
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
        if (!TownServicePolicy.IsHiringAvailable(currentTownIndex))
        {
            statusText.text =
                $"{TownNames[currentTownIndex]}では傭兵を雇用できません。";
            return;
        }
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
        CaptureMercenarySnapshot(mercenary);
        RebuildCompanyList();
    }

    private void HandlePartyChanged()
    {
        RememberDailyPartyMembers();
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

    private void HandleHealingChanged()
    {
        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();
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
        if (!TownServicePolicy.IsHiringAvailable(currentTownIndex))
        {
            ShowTownMap();
            statusText.text =
                $"{TownNames[currentTownIndex]}には傭兵を雇用できる酒場がありません。";
            return;
        }
        SwitchToPage(hirePage, hireTabButton);
        statusText.text =
            $"{TownNames[currentTownIndex]}の雇用候補  |  " +
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
        if (currentTownIndex != 0)
        {
            statusText.text =
                "転職神殿はエルド交易都市でのみ利用できます。";
            return;
        }

        SwitchToPage(jobChangePage);
        RebuildJobChangeList();
        statusText.text =
            $"Lv{MercenaryClassProgression.PromotionLevel}以上の基本職が転職できます。";
    }

    private void RebuildJobChangeList()
    {
        if (jobChangeList == null)
        {
            return;
        }

        ClearChildren(jobChangeList);
        float top = 0f;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            RectTransform row =
                CreateRow($"Job Change {mercenary.InstanceId}", jobChangeList, top);
            CreateText(
                row,
                $"{mercenary.MercenaryName}  Lv{mercenary.Level}  " +
                $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}",
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -44f),
                new Vector2(-370f, -8f),
                Color.white);

            if (mercenary.CanPromote)
            {
                MercenaryClass[] advanced =
                    MercenaryClassProgression.GetAdvancedClasses(
                        mercenary.MercenaryClass);
                CreatePromotionButton(row, mercenary, advanced[0], -18f);
                CreatePromotionButton(row, mercenary, advanced[1], -130f);

                bool showSpecial =
                    mercenary.IsUnique || HasSpecialJobCertificate();
                if (showSpecial)
                {
                    CreatePromotionButton(
                        row,
                        mercenary,
                        MercenaryClassProgression.GetSpecialClass(
                            mercenary.MercenaryClass),
                        -242f);
                }
            }
            else
            {
                string message = MercenaryClassProgression.IsBaseClass(
                    mercenary.MercenaryClass)
                    ? $"Lv{MercenaryClassProgression.PromotionLevel}で転職可能"
                    : "転職済み";
                CreateText(
                    row,
                    message,
                    14,
                    FontStyle.Normal,
                    TextAnchor.MiddleRight,
                    new Vector2(16f, -72f),
                    new Vector2(-18f, -42f),
                    MutedTextColor);
            }
            top -= 112f;
        }
        jobChangeList.sizeDelta =
            new Vector2(0f, Mathf.Max(430f, -top));
    }

    private void CreatePromotionButton(
        RectTransform row,
        MercenaryInstance mercenary,
        MercenaryClass target,
        float x)
    {
        Button button = CreateActionButton(
            row,
            JapaneseDisplayText.GetMercenaryClass(target),
            () => PromoteMercenary(mercenary, target));
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(104f, 44f);
        rect.anchoredPosition = new Vector2(x, 0f);
    }

    private void PromoteMercenary(
        MercenaryInstance mercenary,
        MercenaryClass target)
    {
        bool isSpecial =
            target == MercenaryClassProgression.GetSpecialClass(
                mercenary.MercenaryClass);
        ItemDataSO certificate = GetSpecialJobCertificate();
        if (isSpecial &&
            !mercenary.IsUnique &&
            (certificate == null ||
             !merchantInventory.TryRemoveItem(certificate)))
        {
            statusText.text = "特殊転職に必要な秘伝の転職証がありません。";
            return;
        }

        if (!mercenary.PromoteTo(target))
        {
            if (isSpecial && !mercenary.IsUnique && certificate != null)
            {
                merchantInventory.AddItem(certificate);
            }
            statusText.text = "転職条件を満たしていません。";
            return;
        }

        statusText.text =
            $"{mercenary.MercenaryName}は" +
            $"{JapaneseDisplayText.GetMercenaryClass(target)}へ転職しました。";
        RebuildJobChangeList();
        RebuildCompanyList();
        saveManager?.SaveGame();
    }

    private ItemDataSO GetSpecialJobCertificate()
    {
        return Resources.Load<ItemDataSO>(
            "Items/JobChange/SecretJobCertificate");
    }

    private bool HasSpecialJobCertificate()
    {
        ItemDataSO item = GetSpecialJobCertificate();
        return item != null && merchantInventory.HasItem(item);
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

}
