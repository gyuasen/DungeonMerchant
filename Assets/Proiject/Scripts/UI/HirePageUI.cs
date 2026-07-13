using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HirePageUI : ListPageUIBase
{
    private const float CardWidth = 520f;
    private const float CardHeight = 330f;
    private const float CardSpacing = 16f;

    [SerializeField] private Button contractButton;
    [SerializeField] private ScrollRect scrollRect;
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
    private Button previousButton;
    private Button nextButton;
    private Text pageIndicator;
    private int selectedIndex;
    private int candidateCount;
    private Coroutine slideRoutine;

    public Button ContractButton => contractButton;

    public void Initialize(
        Text targetTitle,
        Button targetContractButton,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        base.Initialize(targetTitle, null, targetListRoot);
        contractButton = targetContractButton;
        scrollRect = targetScrollRect;
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
        base.Configure(
            titleFont,
            titleColor,
            targetMutedTextColor,
            targetButtonTextColor,
            targetRowColor,
            targetButtonColor,
            targetFrameColor,
            onRefresh);

        ConfigureButton(
            contractButton,
            buttonFont,
            targetButtonTextColor,
            "契約を変更",
            onCycleContract);

        scrollRect.content = ListRoot;
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.12f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;
        scrollRect.scrollSensitivity = 42f;
        ConfigureCarouselContent();
        EnsureNavigation(buttonFont, targetButtonTextColor);
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
        UpdateContractButtonLabel();
    }

    public override void Refresh()
    {
        if (fixedCandidateProvider == null &&
            generatedCandidateProvider == null)
        {
            base.Refresh();
            return;
        }

        ClearChildren(ListRoot);
        beforeRebuild?.Invoke();
        ConfigureCarouselContent();
        UpdateContractButtonLabel();

        int cardIndex = 0;
        foreach (MercenaryDataSO candidate in
                 fixedCandidateProvider?.Invoke() ??
                 Array.Empty<MercenaryDataSO>())
        {
            if (candidate == null ||
                shouldShowFixedCandidate != null &&
                !shouldShowFixedCandidate(candidate))
            {
                continue;
            }

            CreateFixedCandidateCard(candidate, cardIndex++);
        }

        foreach (MercenaryInstance candidate in
                 generatedCandidateProvider?.Invoke() ??
                 Array.Empty<MercenaryInstance>())
        {
            if (candidate == null ||
                shouldShowGeneratedCandidate != null &&
                !shouldShowGeneratedCandidate(candidate))
            {
                continue;
            }

            CreateGeneratedCandidateCard(candidate, cardIndex++);
        }

        candidateCount = cardIndex;
        selectedIndex = Mathf.Clamp(
            selectedIndex, 0, Mathf.Max(0, candidateCount - 1));
        float contentWidth = candidateCount > 0
            ? candidateCount * (CardWidth + CardSpacing) + CardSpacing
            : CardWidth;
        ListRoot.sizeDelta = new Vector2(contentWidth, 0f);

        if (candidateCount == 0)
        {
            CreateEmptyMessage("現在、紹介できる傭兵はいません。翌日に再度ご確認ください。");
        }

        Canvas.ForceUpdateCanvases();
        SetCarouselPosition(false);
        UpdateNavigation();
    }

    private void CreateFixedCandidateCard(
        MercenaryDataSO candidate,
        int index)
    {
        RectTransform card = CreateResumeCard(
            candidate.mercenaryName,
            candidate.mercenaryClass,
            1,
            candidate.maxHP,
            candidate.attack,
            candidate.defense,
            candidate.maxMagicPower,
            candidate.attackSpeed,
            candidate.hireCost,
            true,
            index);

        Button hireButton = CreateHireButton(
            card,
            candidate.hireCost,
            () => hireFixedAction?.Invoke(candidate));
        hireButton.interactable =
            canHireFixedCandidate?.Invoke(candidate) == true;
        registerFixedButton?.Invoke(hireButton, candidate);
    }

    private void CreateGeneratedCandidateCard(
        MercenaryInstance candidate,
        int index)
    {
        RectTransform card = CreateResumeCard(
            candidate.MercenaryName,
            candidate.MercenaryClass,
            candidate.Level,
            candidate.MaxHP,
            candidate.Attack,
            candidate.Defense,
            candidate.MaxMagicPower,
            candidate.AttackSpeed,
            candidate.HireCost,
            false,
            index);

        Button hireButton = CreateHireButton(
            card,
            candidate.HireCost,
            () => hireGeneratedAction?.Invoke(candidate));
        hireButton.interactable =
            canHireGeneratedCandidate?.Invoke(candidate) == true;
        registerGeneratedButton?.Invoke(hireButton, candidate);
    }

    private RectTransform CreateResumeCard(
        string candidateName,
        MercenaryClass mercenaryClass,
        int level,
        int maxHP,
        int attack,
        int defense,
        int magicPower,
        float attackSpeed,
        int hireCost,
        bool isUnique,
        int index)
    {
        RectTransform card = CreateUIObject(
            $"Resume {candidateName}", ListRoot);
        card.anchorMin = card.anchorMax = new Vector2(0f, 0.5f);
        card.pivot = new Vector2(0f, 0.5f);
        card.sizeDelta = new Vector2(CardWidth, CardHeight);
        card.anchoredPosition = new Vector2(
            CardSpacing + index * (CardWidth + CardSpacing), 0f);

        Image paper = card.gameObject.AddComponent<Image>();
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(paper);
        Outline outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = FrameColor;
        outline.effectDistance = new Vector2(2f, -2f);

        CreateCardText(
            card,
            "傭兵登録票",
            18,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(16f, -40f),
            new Vector2(-16f, -10f),
            UITheme.ParchmentTextColor);

        CreatePortrait(card, mercenaryClass);

        string className =
            JapaneseDisplayText.GetMercenaryClass(mercenaryClass);
        CreateCardText(
            card,
            $"{candidateName}\n【{className}】  Lv{level}\n{GetClassRole(mercenaryClass)}",
            18,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Vector2(184f, -116f),
            new Vector2(-18f, -52f),
            UITheme.ParchmentTextColor);

        string contract =
            JapaneseDisplayText.GetContractType(GetContractType());
        string success = $"{GetSuccessRate() * 100f:0}%";
        string details =
            "能力\n" +
            $"HP {maxHP}　攻撃 {attack}　防御 {defense}\n" +
            $"魔力 {magicPower}　速度 {attackSpeed:0.00}\n\n" +
            "略歴・得意分野\n" +
            $"{GetCareerSummary(mercenaryClass, isUnique)}\n\n" +
            $"希望契約: {contract}　成立率: {success}\n" +
            $"契約金: {hireCost} G";
        CreateCardText(
            card,
            details,
            14,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(184f, -280f),
            new Vector2(-18f, -120f),
            UITheme.ParchmentTextColor);
        return card;
    }

    private void CreatePortrait(
        RectTransform card,
        MercenaryClass mercenaryClass)
    {
        RectTransform portraitRect = CreateUIObject("Portrait", card);
        portraitRect.anchorMin = portraitRect.anchorMax =
            new Vector2(0f, 1f);
        portraitRect.pivot = new Vector2(0f, 1f);
        portraitRect.sizeDelta = new Vector2(148f, 238f);
        portraitRect.anchoredPosition = new Vector2(20f, -56f);

        RawImage portrait = portraitRect.gameObject.AddComponent<RawImage>();
        if (!MercenaryPortraitProvider.TryApply(
                portrait, mercenaryClass))
        {
            portrait.color = GetClassColor(mercenaryClass);
        }

        Outline frame = portraitRect.gameObject.AddComponent<Outline>();
        frame.effectColor = FrameColor;
        frame.effectDistance = new Vector2(2f, -2f);
    }

    private Button CreateHireButton(
        RectTransform card,
        int hireCost,
        UnityAction action)
    {
        Button button = CreateActionButton(
            card,
            $"この傭兵と契約\n{hireCost} G",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(166f, 48f);
        rect.anchoredPosition = new Vector2(-18f, 16f);
        return button;
    }

    private void EnsureNavigation(Font buttonFont, Color textColor)
    {
        if (previousButton == null)
        {
            previousButton = CreateActionButton(
                transform as RectTransform,
                "＜",
                buttonFont,
                ButtonColor,
                FrameColor,
                textColor,
                () => MoveSelection(-1));
            ConfigureNavigationButton(previousButton, false);
        }

        if (nextButton == null)
        {
            nextButton = CreateActionButton(
                transform as RectTransform,
                "＞",
                buttonFont,
                ButtonColor,
                FrameColor,
                textColor,
                () => MoveSelection(1));
            ConfigureNavigationButton(nextButton, true);
        }

        if (pageIndicator == null)
        {
            pageIndicator = CreateCardText(
                transform as RectTransform,
                string.Empty,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Vector2(270f, -38f),
                new Vector2(-270f, -8f),
                UITheme.ParchmentTextColor);
        }
    }

    private static void ConfigureNavigationButton(Button button, bool right)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax =
            new Vector2(right ? 1f : 0f, 0.5f);
        rect.pivot = new Vector2(right ? 1f : 0f, 0.5f);
        rect.sizeDelta = new Vector2(42f, 72f);
        rect.anchoredPosition = new Vector2(right ? -4f : 4f, -18f);
        rect.SetAsLastSibling();
    }

    private void MoveSelection(int direction)
    {
        if (candidateCount <= 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(
            selectedIndex + direction, 0, candidateCount - 1);
        SetCarouselPosition(true);
        UpdateNavigation();
    }

    private void SetCarouselPosition(bool animate)
    {
        float target = candidateCount <= 1
            ? 0f
            : selectedIndex / (float)(candidateCount - 1);
        if (!animate || !isActiveAndEnabled)
        {
            scrollRect.horizontalNormalizedPosition = target;
            return;
        }

        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
        }
        slideRoutine = StartCoroutine(SlideTo(target));
    }

    private IEnumerator SlideTo(float target)
    {
        float start = scrollRect.horizontalNormalizedPosition;
        float elapsed = 0f;
        const float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / duration));
            scrollRect.horizontalNormalizedPosition =
                Mathf.Lerp(start, target, t);
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = target;
        slideRoutine = null;
    }

    private void UpdateNavigation()
    {
        if (previousButton != null)
        {
            previousButton.interactable = selectedIndex > 0;
        }
        if (nextButton != null)
        {
            nextButton.interactable =
                candidateCount > 0 && selectedIndex < candidateCount - 1;
        }
        if (pageIndicator != null)
        {
            pageIndicator.text = candidateCount > 0
                ? $"候補 {selectedIndex + 1} / {candidateCount}"
                : "候補なし";
        }
    }

    private void ConfigureCarouselContent()
    {
        ListRoot.anchorMin = new Vector2(0f, 0f);
        ListRoot.anchorMax = new Vector2(0f, 1f);
        ListRoot.pivot = new Vector2(0f, 0.5f);
        ListRoot.anchoredPosition = Vector2.zero;
    }

    private Text CreateCardText(
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        Text text = CreateText(
            parent,
            content,
            RowFont,
            fontSize,
            fontStyle,
            alignment,
            offsetMin,
            offsetMax,
            color);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
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

    private void UpdateContractButtonLabel()
    {
        if (contractButton == null)
        {
            return;
        }
        Text label = contractButton.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text =
                $"契約: {JapaneseDisplayText.GetContractType(GetContractType())}";
        }
    }

    private static string GetClassRole(MercenaryClass mercenaryClass)
    {
        switch (MercenaryClassProgression.GetBaseClass(mercenaryClass))
        {
            case MercenaryClass.Warrior: return "前衛／防御・敵の引きつけ";
            case MercenaryClass.Archer: return "後衛／連続攻撃・狙撃";
            case MercenaryClass.Mage: return "後衛／魔法火力・範囲攻撃";
            case MercenaryClass.Priest: return "後衛／治療・支援";
            case MercenaryClass.Rogue: return "前衛／高速攻撃・急所狙い";
            default: return "前衛／貫通攻撃・隊列防護";
        }
    }

    private static string GetCareerSummary(
        MercenaryClass mercenaryClass,
        bool isUnique)
    {
        string prefix = isUnique
            ? "各地で名を知られた経験豊かな傭兵。"
            : "街道護衛と魔物討伐の経験を持つ。";
        switch (MercenaryClassProgression.GetBaseClass(mercenaryClass))
        {
            case MercenaryClass.Warrior: return prefix + " 持久戦と味方の護衛を得意とする。";
            case MercenaryClass.Archer: return prefix + " 遠距離からの援護射撃を得意とする。";
            case MercenaryClass.Mage: return prefix + " 魔力を用いた集団戦を得意とする。";
            case MercenaryClass.Priest: return prefix + " 負傷者の治療と戦線維持を担う。";
            case MercenaryClass.Rogue: return prefix + " 索敵と素早い奇襲を得意とする。";
            default: return prefix + " 長い間合いで前線を支える。";
        }
    }

    private static Color GetClassColor(MercenaryClass mercenaryClass)
    {
        switch (MercenaryClassProgression.GetBaseClass(mercenaryClass))
        {
            case MercenaryClass.Warrior: return new Color(0.45f, 0.25f, 0.18f, 1f);
            case MercenaryClass.Archer: return new Color(0.22f, 0.42f, 0.22f, 1f);
            case MercenaryClass.Mage: return new Color(0.28f, 0.25f, 0.5f, 1f);
            case MercenaryClass.Priest: return new Color(0.72f, 0.64f, 0.42f, 1f);
            case MercenaryClass.Rogue: return new Color(0.28f, 0.28f, 0.31f, 1f);
            default: return new Color(0.34f, 0.38f, 0.46f, 1f);
        }
    }
}
