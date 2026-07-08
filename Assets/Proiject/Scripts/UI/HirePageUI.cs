using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HirePageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button contractButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    private Action beforeRebuild;
    private Func<IEnumerable<MercenaryDataSO>> fixedCandidateProvider;
    private Func<MercenaryDataSO, bool> shouldShowFixedCandidate;
    private Func<IEnumerable<MercenaryInstance>> generatedCandidateProvider;
    private Func<MercenaryInstance, bool> shouldShowGeneratedCandidate;
    private Func<MercenaryContractType> contractProvider;
    private Func<float> successRateProvider;
    private Func<MercenaryDataSO, bool> canHireFixedCandidate;
    private Func<MercenaryInstance, bool> canHireGeneratedCandidate;
    private Action<MercenaryDataSO> hireFixedAction;
    private Action<MercenaryInstance> hireGeneratedAction;
    private Action<Button, MercenaryDataSO> registerFixedButton;
    private Action<Button, MercenaryInstance> registerGeneratedButton;
    private Font rowFont;
    private Color rowTextColor = Color.white;
    private Color mutedTextColor = Color.gray;
    private Color buttonTextColor = Color.white;
    private Color rowColor = new Color(0.27f, 0.16f, 0.09f, 0.94f);
    private Color buttonColor = new Color(0.35f, 0.22f, 0.13f, 1f);
    private Color frameColor = new Color(0.72f, 0.52f, 0.27f, 0.9f);

    public Button ContractButton => contractButton;
    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text targetTitle,
        Button targetContractButton,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = targetTitle;
        contractButton = targetContractButton;
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
        UnityAction onCycleContract,
        UnityAction onRefresh)
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
            contractButton,
            buttonFont,
            buttonTextColor,
            "契約: 日雇い",
            onCycleContract);

        scrollRect.content = listRoot;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        refreshAction = onRefresh;
    }

    public void ConfigureHireList(
        Action resetLists,
        Func<IEnumerable<MercenaryDataSO>> fixedCandidates,
        Func<MercenaryDataSO, bool> fixedFilter,
        Func<IEnumerable<MercenaryInstance>> generatedCandidates,
        Func<MercenaryInstance, bool> generatedFilter,
        Func<MercenaryContractType> targetContractProvider,
        Func<float> targetSuccessRateProvider,
        Func<MercenaryDataSO, bool> targetCanHireFixedCandidate,
        Func<MercenaryInstance, bool> targetCanHireGeneratedCandidate,
        Action<MercenaryDataSO> targetHireFixedAction,
        Action<MercenaryInstance> targetHireGeneratedAction,
        Action<Button, MercenaryDataSO> targetRegisterFixedButton,
        Action<Button, MercenaryInstance> targetRegisterGeneratedButton)
    {
        beforeRebuild = resetLists;
        fixedCandidateProvider = fixedCandidates;
        shouldShowFixedCandidate = fixedFilter;
        generatedCandidateProvider = generatedCandidates;
        shouldShowGeneratedCandidate = generatedFilter;
        contractProvider = targetContractProvider;
        successRateProvider = targetSuccessRateProvider;
        canHireFixedCandidate = targetCanHireFixedCandidate;
        canHireGeneratedCandidate = targetCanHireGeneratedCandidate;
        hireFixedAction = targetHireFixedAction;
        hireGeneratedAction = targetHireGeneratedAction;
        registerFixedButton = targetRegisterFixedButton;
        registerGeneratedButton = targetRegisterGeneratedButton;
    }

    public override void Refresh()
    {
        if (fixedCandidateProvider == null &&
            generatedCandidateProvider == null)
        {
            refreshAction?.Invoke();
            return;
        }

        ClearChildren(listRoot);
        beforeRebuild?.Invoke();

        float rowTop = 0f;
        foreach (MercenaryDataSO candidate in
                 fixedCandidateProvider?.Invoke() ??
                 Array.Empty<MercenaryDataSO>())
        {
            if (shouldShowFixedCandidate != null &&
                !shouldShowFixedCandidate(candidate))
            {
                continue;
            }

            CreateFixedCandidateRow(candidate, rowTop);
            rowTop -= 112f;
        }

        foreach (MercenaryInstance candidate in
                 generatedCandidateProvider?.Invoke() ??
                 Array.Empty<MercenaryInstance>())
        {
            if (shouldShowGeneratedCandidate != null &&
                !shouldShowGeneratedCandidate(candidate))
            {
                continue;
            }

            CreateGeneratedCandidateRow(candidate, rowTop);
            rowTop -= 112f;
        }

        listRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void CreateFixedCandidateRow(
        MercenaryDataSO candidate,
        float top)
    {
        RectTransform row =
            CreateRow(
                candidate.mercenaryName,
                listRoot,
                top,
                rowColor,
                frameColor);
        CreateCandidateTexts(
            row,
            candidate.mercenaryName,
            JapaneseDisplayText.GetMercenaryClass(candidate.mercenaryClass),
            candidate.maxHP,
            candidate.attack,
            candidate.defense);

        Button hireButton = CreateActionButton(
            row,
            $"{candidate.hireCost} G",
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => hireFixedAction?.Invoke(candidate));
        hireButton.interactable =
            canHireFixedCandidate?.Invoke(candidate) == true;
        registerFixedButton?.Invoke(hireButton, candidate);
    }

    private void CreateGeneratedCandidateRow(
        MercenaryInstance candidate,
        float top)
    {
        RectTransform row =
            CreateRow(
                candidate.MercenaryName,
                listRoot,
                top,
                rowColor,
                frameColor);
        CreateCandidateTexts(
            row,
            candidate.MercenaryName,
            JapaneseDisplayText.GetMercenaryClass(candidate.MercenaryClass),
            candidate.MaxHP,
            candidate.Attack,
            candidate.Defense);

        Button hireButton = CreateActionButton(
            row,
            $"{candidate.HireCost} G",
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => hireGeneratedAction?.Invoke(candidate));
        hireButton.interactable =
            canHireGeneratedCandidate?.Invoke(candidate) == true;
        registerGeneratedButton?.Invoke(hireButton, candidate);
    }

    private void CreateCandidateTexts(
        RectTransform row,
        string candidateName,
        string className,
        int maxHP,
        int attack,
        int defense)
    {
        CreateText(
            row,
            candidateName,
            rowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            rowTextColor);

        string details =
            $"{className}  |  " +
            $"{JapaneseDisplayText.GetContractType(GetContractType())}  |  " +
            $"成功率 {GetSuccessRate() * 100f:0}%  |  " +
            $"HP {maxHP}  攻撃 {attack}  防御 {defense}";
        CreateText(
            row,
            details,
            rowFont,
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            mutedTextColor);
    }

    private MercenaryContractType GetContractType()
    {
        return contractProvider != null
            ? contractProvider.Invoke()
            : MercenaryContractType.Local;
    }

    private float GetSuccessRate()
    {
        return successRateProvider != null
            ? successRateProvider.Invoke()
            : 0f;
    }
}
