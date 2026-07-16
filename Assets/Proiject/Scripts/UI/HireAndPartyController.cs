using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the hire/party/heal/job-change actions and the hire-button
/// tracking state. Extracted from SimpleMercenaryHireUI (step 3.6).
/// Page construction, routing and delegate wiring stay in
/// SimpleMercenaryHireUI.HireParty.cs; only the feature state and
/// business actions live here.
/// </summary>
public sealed class HireAndPartyController
{
    private readonly MercenaryHireManager hireManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly MercenaryGenerator mercenaryGenerator;
    private readonly MerchantInventory merchantInventory;
    private readonly HealingManager healingManager;
    private readonly TownProgressState townProgressState;
    private readonly SaveManager saveManager;
    private readonly TransportManager transportManager;
    private readonly Action<string> setStatus;
    private readonly Action refreshHirePage;
    private readonly Action refreshCompanyPage;
    private readonly Action refreshPartyPage;
    private readonly Action refreshHealPage;
    private readonly Action refreshJobChangePage;
    private readonly Action refreshUI;
    private readonly Action<string> setContractButtonLabel;

    private readonly List<Button> hireButtons = new List<Button>();
    private readonly List<MercenaryDataSO> displayedCandidates = new List<MercenaryDataSO>();
    private readonly List<Button> generatedHireButtons = new List<Button>();
    private readonly List<MercenaryInstance> displayedGeneratedCandidates =
        new List<MercenaryInstance>();
    private readonly HashSet<MercenaryDataSO> hiredCandidates = new HashSet<MercenaryDataSO>();

    public HireAndPartyController(
        MercenaryHireManager hireManager,
        MercenaryPartyManager partyManager,
        MercenaryGenerator mercenaryGenerator,
        MerchantInventory merchantInventory,
        HealingManager healingManager,
        TownProgressState townProgressState,
        SaveManager saveManager,
        TransportManager transportManager,
        Action<string> setStatus,
        Action refreshHirePage,
        Action refreshCompanyPage,
        Action refreshPartyPage,
        Action refreshHealPage,
        Action refreshJobChangePage,
        Action refreshUI,
        Action<string> setContractButtonLabel)
    {
        this.hireManager = hireManager;
        this.partyManager = partyManager;
        this.mercenaryGenerator = mercenaryGenerator;
        this.merchantInventory = merchantInventory;
        this.healingManager = healingManager;
        this.townProgressState = townProgressState;
        this.saveManager = saveManager;
        this.transportManager = transportManager;
        this.setStatus = setStatus;
        this.refreshHirePage = refreshHirePage;
        this.refreshCompanyPage = refreshCompanyPage;
        this.refreshPartyPage = refreshPartyPage;
        this.refreshHealPage = refreshHealPage;
        this.refreshJobChangePage = refreshJobChangePage;
        this.refreshUI = refreshUI;
        this.setContractButtonLabel = setContractButtonLabel;
    }

    public void ResetHireListTracking()
    {
        hireButtons.Clear();
        displayedCandidates.Clear();
        generatedHireButtons.Clear();
        displayedGeneratedCandidates.Clear();
    }

    public bool ShouldShowFixedHireCandidate(MercenaryDataSO candidate)
    {
        return candidate != null && !hiredCandidates.Contains(candidate);
    }

    public bool CanHireFixedCandidate(MercenaryDataSO candidate)
    {
        return !hiredCandidates.Contains(candidate) &&
               hireManager.CanAfford(candidate);
    }

    public void RegisterFixedHireButton(
        Button hireButton,
        MercenaryDataSO candidate)
    {
        hireButtons.Add(hireButton);
        displayedCandidates.Add(candidate);
    }

    public void RegisterGeneratedHireButton(
        Button hireButton,
        MercenaryInstance candidate)
    {
        generatedHireButtons.Add(hireButton);
        displayedGeneratedCandidates.Add(candidate);
    }

    public void UpdateHireButtonInteractability()
    {
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
    }

    public void Hire(MercenaryDataSO candidate)
    {
        if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            setStatus(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}では傭兵を雇用できません。");
            return;
        }
        if (!hireManager.TryHireMercenary(candidate))
        {
            setStatus($"{candidate.mercenaryName}を雇用できませんでした。");
            refreshUI();
            return;
        }

        hiredCandidates.Add(candidate);
        setStatus($"{candidate.mercenaryName}が商会に加わりました。");
        refreshHirePage();
        refreshUI();
    }

    public void HireGeneratedCandidate(MercenaryInstance candidate)
    {
        if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            setStatus(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}では傭兵を雇用できません。");
            return;
        }
        if (!hireManager.TryHireMercenary(candidate))
        {
            setStatus($"{candidate.MercenaryName}を雇用できませんでした。");
            refreshUI();
            return;
        }

        setStatus($"{candidate.MercenaryName}が商会に加わりました。");
        mercenaryGenerator.RemoveCandidate(candidate);
        refreshUI();
    }

    public void CacheAlreadyHiredCandidates()
    {
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary.BaseData != null)
            {
                hiredCandidates.Add(mercenary.BaseData);
            }
        }
    }

    public void TogglePartyMember(MercenaryInstance mercenary)
    {
        if (partyManager.Contains(mercenary))
        {
            RemovePartyMember(mercenary);
            return;
        }

        if (!partyManager.TryAdd(mercenary))
        {
            setStatus(transportManager != null &&
                      transportManager.IsMercenaryOnTransportDuty(mercenary.InstanceId)
                ? "輸送任務中の傭兵は編成できません"
                : "パーティーは満員です。");
        }
    }

    public void RemovePartyMember(MercenaryInstance mercenary)
    {
        partyManager.Remove(mercenary);
    }

    public void ReleaseMercenary(MercenaryInstance mercenary)
    {
        if (mercenary == null || !hireManager.TryReleaseMercenary(mercenary))
        {
            setStatus("契約を解除できませんでした。");
            return;
        }

        if (mercenary.BaseData != null)
        {
            hiredCandidates.Remove(mercenary.BaseData);
        }

        setStatus($"{mercenary.MercenaryName}との契約を解除しました。");
        refreshCompanyPage();
        refreshPartyPage();
        refreshHealPage();
        refreshJobChangePage();
        refreshUI();
        saveManager?.SaveGame();
    }

    public void HealMercenary(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return;
        }

        int cost = healingManager.GetFullHealCost(mercenary);
        if (!healingManager.TryHealFull(mercenary))
        {
            setStatus($"{mercenary.MercenaryName}を治療できませんでした。");
            refreshUI();
            return;
        }

        setStatus($"{mercenary.MercenaryName}を{cost} Gで治療しました。");
        refreshCompanyPage();
        refreshPartyPage();
        refreshHealPage();
        refreshUI();
    }

    public bool ShouldShowSpecialPromotion(MercenaryInstance mercenary)
    {
        return mercenary != null &&
               (mercenary.IsUnique || HasSpecialJobCertificate());
    }

    public void PromoteMercenary(
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
            setStatus("特殊転職に必要な秘伝の転職証がありません。");
            return;
        }

        if (!mercenary.PromoteTo(target))
        {
            if (isSpecial && !mercenary.IsUnique && certificate != null)
            {
                merchantInventory.AddItem(certificate);
            }
            setStatus("転職条件を満たしていません。");
            return;
        }

        setStatus(
            $"{mercenary.MercenaryName}は" +
            $"{JapaneseDisplayText.GetMercenaryClass(target)}へ転職しました。");
        refreshJobChangePage();
        refreshCompanyPage();
        saveManager?.SaveGame();
    }

    public ItemDataSO GetSpecialJobCertificate()
    {
        return Resources.Load<ItemDataSO>(
            "Items/JobChange/SecretJobCertificate");
    }

    public bool HasSpecialJobCertificate()
    {
        ItemDataSO item = GetSpecialJobCertificate();
        return item != null && merchantInventory.HasItem(item);
    }

    public MercenaryContractType GetUnlockedContractType()
    {
        return hireManager.SelectedContract;
    }

    public void CycleHireContract()
    {
        MercenaryContractType selected =
            hireManager.CycleSelectedContract();
        setContractButtonLabel(
            $"契約: {JapaneseDisplayText.GetContractType(selected)}");
        refreshHirePage();
    }
}
