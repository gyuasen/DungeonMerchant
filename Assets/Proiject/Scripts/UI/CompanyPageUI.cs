using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class CompanyPageUI : ListPageUIBase
{
    [SerializeField] private Button questButton;
    [SerializeField] private ScrollRect scrollRect;
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, bool> isInParty;
    private Func<MercenaryInstance, int> renewalCostProvider;
    private Action<MercenaryInstance> togglePartyAction;
    private Action<MercenaryInstance> showDetailsAction;
    private Action<MercenaryInstance> renewContractAction;
    private Action<MercenaryInstance> releaseContractAction;
    private Func<MercenaryInstance, bool> isOnTransportDuty;
    private Func<MercenaryInstance, bool> isOnExpeditionDuty;

    public void Initialize(
        Text title,
        Button quest,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        questButton = quest;
        scrollRect = targetScrollRect;
        base.Initialize(title, null, targetListRoot);
    }

    public void Configure(
        Font titleFont,
        Font buttonFont,
        Color titleColor,
        Color targetButtonTextColor,
        Color targetMutedTextColor,
        Color targetRowColor,
        Color targetButtonColor,
        Color targetFrameColor,
        UnityAction showQuests,
        UnityAction refresh)
    {
        Configure(
            titleFont,
            titleColor,
            targetMutedTextColor,
            targetButtonTextColor,
            targetRowColor,
            targetButtonColor,
            targetFrameColor,
            refresh);
        ConfigureButton(
            questButton, buttonFont, targetButtonTextColor,
            "依頼", showQuests);
        scrollRect.content = ListRoot;
    }

    public void ConfigureCompanyList(
        Func<IEnumerable<MercenaryInstance>> mercenaries,
        Func<MercenaryInstance, bool> targetIsInParty,
        Func<MercenaryInstance, int> targetRenewalCostProvider,
        Action<MercenaryInstance> targetTogglePartyAction,
        Action<MercenaryInstance> targetShowDetailsAction,
        Action<MercenaryInstance> targetRenewContractAction,
        Action<MercenaryInstance> targetReleaseContractAction,
        Func<MercenaryInstance, bool> targetIsOnTransportDuty = null,
        Func<MercenaryInstance, bool> targetIsOnExpeditionDuty = null)
    {
        mercenaryProvider = mercenaries;
        isInParty = targetIsInParty;
        renewalCostProvider = targetRenewalCostProvider;
        togglePartyAction = targetTogglePartyAction;
        showDetailsAction = targetShowDetailsAction;
        renewContractAction = targetRenewContractAction;
        releaseContractAction = targetReleaseContractAction;
        isOnTransportDuty = targetIsOnTransportDuty;
        isOnExpeditionDuty = targetIsOnExpeditionDuty;
    }

    public override void Refresh()
    {
        if (mercenaryProvider == null)
        {
            base.Refresh();
            return;
        }

        RebuildRows(
            mercenaryProvider.Invoke(),
            112f,
            430f,
            "雇用済みの傭兵はいません。",
            mercenary => mercenary != null,
            (_, mercenary, rowTop) => CreateCompanyRow(mercenary, rowTop),
            (_, message) => CreateEmptyMessage(message));
    }

    private void CreateCompanyRow(
        MercenaryInstance mercenary,
        float top)
    {
        RectTransform row =
            CreateRow(
                mercenary.MercenaryName,
                ListRoot,
                top,
                RowColor,
                FrameColor);
        CreateText(
            row,
            mercenary.MercenaryName,
            RowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-300f, -12f),
            RowTextColor);

        CreateText(
            row,
            BuildMercenaryDetails(mercenary),
            RowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-300f, -48f),
            MutedTextColor);

        string actionLabel =
            isInParty?.Invoke(mercenary) == true ? "外す" : "加える";
        Button partyButton = CreateActionButton(
            row,
            actionLabel,
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => togglePartyAction?.Invoke(mercenary));
        partyButton.GetComponent<RectTransform>().sizeDelta =
            new Vector2(80f, 44f);

        Button detailsButton = CreateActionButton(
            row,
            "詳細",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => showDetailsAction?.Invoke(mercenary));
        RectTransform detailsRect = detailsButton.GetComponent<RectTransform>();
        detailsRect.sizeDelta = new Vector2(80f, 44f);
        detailsRect.anchoredPosition = new Vector2(-104f, 0f);

        Button releaseButton = CreateActionButton(
            row,
            "契約解除",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => releaseContractAction?.Invoke(mercenary));
        RectTransform releaseRect = releaseButton.GetComponent<RectTransform>();
        releaseRect.sizeDelta = new Vector2(80f, 44f);
        releaseRect.anchoredPosition = new Vector2(-190f, 0f);

        if (!mercenary.ContractNeedsRenewal)
        {
            return;
        }

        int renewalCost = renewalCostProvider?.Invoke(mercenary) ?? 0;
        Button renewButton = CreateActionButton(
            row,
            $"更新 {renewalCost}G",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => renewContractAction?.Invoke(mercenary));
        RectTransform renewRect = renewButton.GetComponent<RectTransform>();
        renewRect.sizeDelta = new Vector2(80f, 44f);
        renewRect.anchoredPosition = new Vector2(-276f, 0f);
    }

    private string BuildMercenaryDetails(MercenaryInstance mercenary)
    {
        string contractStatus = mercenary.ContractNeedsRenewal
            ? "更新待ち"
            : mercenary.ContractEndDay > 0
                ? $"期限 {mercenary.ContractEndDay}日"
                : "期限なし";
        return
            $"レベル {mercenary.Level}  経験値 " +
            $"{mercenary.CurrentExperience}/{mercenary.ExperienceToNextLevel}  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
            $"{JapaneseDisplayText.GetContractType(mercenary.ContractType)} " +
            contractStatus +
            (isOnTransportDuty?.Invoke(mercenary) == true ? "  |  輸送任務中" : string.Empty);
    }

}
