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
    private Action<RectTransform, MercenaryDataSO, float> createFixedCandidateRow;
    private Func<IEnumerable<MercenaryInstance>> generatedCandidateProvider;
    private Func<MercenaryInstance, bool> shouldShowGeneratedCandidate;
    private Action<RectTransform, MercenaryInstance, float>
        createGeneratedCandidateRow;

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
        Color buttonTextColor,
        UnityAction onCycleContract,
        UnityAction onRefresh)
    {
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
        Action<RectTransform, MercenaryDataSO, float> fixedRowFactory,
        Func<IEnumerable<MercenaryInstance>> generatedCandidates,
        Func<MercenaryInstance, bool> generatedFilter,
        Action<RectTransform, MercenaryInstance, float> generatedRowFactory)
    {
        beforeRebuild = resetLists;
        fixedCandidateProvider = fixedCandidates;
        shouldShowFixedCandidate = fixedFilter;
        createFixedCandidateRow = fixedRowFactory;
        generatedCandidateProvider = generatedCandidates;
        shouldShowGeneratedCandidate = generatedFilter;
        createGeneratedCandidateRow = generatedRowFactory;
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

            createFixedCandidateRow?.Invoke(listRoot, candidate, rowTop);
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

            createGeneratedCandidateRow?.Invoke(listRoot, candidate, rowTop);
            rowTop -= 112f;
        }

        listRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }
}
