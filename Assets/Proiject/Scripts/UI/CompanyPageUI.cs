using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class CompanyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button questButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, bool> isInParty;
    private Func<MercenaryInstance, int> renewalCostProvider;
    private Action<MercenaryInstance> togglePartyAction;
    private Action<MercenaryInstance> showDetailsAction;
    private Action<MercenaryInstance> renewContractAction;
    private Font rowFont;
    private Color rowTextColor = Color.white;
    private Color mutedTextColor = Color.gray;
    private Color buttonTextColor = Color.white;
    private Color rowColor = new Color(0.27f, 0.16f, 0.09f, 0.94f);
    private Color buttonColor = new Color(0.35f, 0.22f, 0.13f, 1f);
    private Color frameColor = new Color(0.72f, 0.52f, 0.27f, 0.9f);

    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text title,
        Button quest,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = title;
        questButton = quest;
        scrollRect = targetScrollRect;
        listRoot = targetListRoot;
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
        rowFont = titleFont;
        buttonTextColor = targetButtonTextColor;
        mutedTextColor = targetMutedTextColor;
        rowColor = targetRowColor;
        buttonColor = targetButtonColor;
        frameColor = targetFrameColor;

        ConfigureText(
            titleText, titleFont, 15,
            TextAnchor.MiddleLeft, titleColor);
        ConfigureButton(
            questButton, buttonFont, buttonTextColor,
            "依頼", showQuests);
        scrollRect.content = listRoot;
        refreshAction = refresh;
    }

    public void ConfigureCompanyList(
        Func<IEnumerable<MercenaryInstance>> mercenaries,
        Func<MercenaryInstance, bool> targetIsInParty,
        Func<MercenaryInstance, int> targetRenewalCostProvider,
        Action<MercenaryInstance> targetTogglePartyAction,
        Action<MercenaryInstance> targetShowDetailsAction,
        Action<MercenaryInstance> targetRenewContractAction)
    {
        mercenaryProvider = mercenaries;
        isInParty = targetIsInParty;
        renewalCostProvider = targetRenewalCostProvider;
        togglePartyAction = targetTogglePartyAction;
        showDetailsAction = targetShowDetailsAction;
        renewContractAction = targetRenewContractAction;
    }

    public override void Refresh()
    {
        if (mercenaryProvider == null)
        {
            refreshAction?.Invoke();
            return;
        }

        ClearChildren(listRoot);

        float rowTop = 0f;
        bool createdAnyRow = false;
        foreach (MercenaryInstance mercenary in
                 mercenaryProvider.Invoke() ??
                 Array.Empty<MercenaryInstance>())
        {
            if (mercenary == null)
            {
                continue;
            }

            CreateCompanyRow(mercenary, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        if (!createdAnyRow)
        {
            CreateEmptyMessage("雇用済みの傭兵はいません。");
            return;
        }

        listRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void CreateCompanyRow(
        MercenaryInstance mercenary,
        float top)
    {
        RectTransform row =
            CreateRow(
                mercenary.MercenaryName,
                listRoot,
                top,
                rowColor,
                frameColor);
        CreateText(
            row,
            mercenary.MercenaryName,
            rowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-300f, -12f),
            rowTextColor);

        CreateText(
            row,
            BuildMercenaryDetails(mercenary),
            rowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-300f, -48f),
            mutedTextColor);

        CreateText(
            row,
            $"ID {GetShortId(mercenary)}",
            rowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleRight,
            new Vector2(18f, -64f),
            new Vector2(-300f, -30f),
            mutedTextColor);

        string actionLabel =
            isInParty?.Invoke(mercenary) == true ? "外す" : "加える";
        Button partyButton = CreateActionButton(
            row,
            actionLabel,
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => togglePartyAction?.Invoke(mercenary));
        partyButton.GetComponent<RectTransform>().sizeDelta =
            new Vector2(112f, 52f);

        Button detailsButton = CreateActionButton(
            row,
            "詳細",
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => showDetailsAction?.Invoke(mercenary));
        RectTransform detailsRect = detailsButton.GetComponent<RectTransform>();
        detailsRect.sizeDelta = new Vector2(112f, 52f);
        detailsRect.anchoredPosition = new Vector2(-142f, 0f);

        if (!mercenary.ContractNeedsRenewal)
        {
            return;
        }

        int renewalCost = renewalCostProvider?.Invoke(mercenary) ?? 0;
        Button renewButton = CreateActionButton(
            row,
            $"更新 {renewalCost}G",
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => renewContractAction?.Invoke(mercenary));
        RectTransform renewRect = renewButton.GetComponent<RectTransform>();
        renewRect.sizeDelta = new Vector2(112f, 52f);
        renewRect.anchoredPosition = new Vector2(-266f, 0f);
    }

    private static string BuildMercenaryDetails(MercenaryInstance mercenary)
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
            contractStatus;
    }

    private static string GetShortId(MercenaryInstance mercenary)
    {
        return string.IsNullOrEmpty(mercenary.InstanceId)
            ? "--------"
            : mercenary.InstanceId.Substring(
                0,
                Mathf.Min(8, mercenary.InstanceId.Length))
                .ToUpperInvariant();
    }

    private void CreateEmptyMessage(string message)
    {
        CreateText(
            listRoot,
            message,
            rowFont,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -180f),
            new Vector2(0f, -80f),
            mutedTextColor);
    }
}
