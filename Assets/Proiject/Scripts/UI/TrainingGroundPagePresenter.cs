using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class TrainingGroundPagePresenter
{
    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly RectTransform page;
    private readonly TrainingGroundPageUI pageUI;
    private readonly Font bodyFont;
    private readonly Color parchmentTextColor;
    private readonly Color mutedTextColor;
    private readonly Color buttonTextColor;
    private readonly Color rowColor;
    private readonly Color woodButtonColor;
    private readonly Color frameColor;
    private readonly MercenaryHireManager hireManager;
    private readonly TrainingGroundManager trainingGroundManager;
    private readonly MerchantData merchantData;
    private readonly DayManager dayManager;
    private readonly TownProgressState townProgressState;
    private readonly Action<RectTransform> registerPage;
    private readonly Action<RectTransform> switchToPage;
    private readonly Action<RectTransform> refreshPage;
    private readonly Action<string> setStatus;
    private readonly Action refreshUI;

    public TrainingGroundPagePresenter(
        SimpleMercenaryHireUIFactory factory,
        RectTransform page,
        TrainingGroundPageUI pageUI,
        Font bodyFont,
        Color parchmentTextColor,
        Color mutedTextColor,
        Color buttonTextColor,
        Color rowColor,
        Color woodButtonColor,
        Color frameColor,
        MercenaryHireManager hireManager,
        TrainingGroundManager trainingGroundManager,
        MerchantData merchantData,
        DayManager dayManager,
        TownProgressState townProgressState,
        Action<RectTransform> registerPage,
        Action<RectTransform> switchToPage,
        Action<RectTransform> refreshPage,
        Action<string> setStatus,
        Action refreshUI)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.page = page ?? throw new ArgumentNullException(nameof(page));
        this.pageUI = pageUI ?? throw new ArgumentNullException(nameof(pageUI));
        this.bodyFont = bodyFont;
        this.parchmentTextColor = parchmentTextColor;
        this.mutedTextColor = mutedTextColor;
        this.buttonTextColor = buttonTextColor;
        this.rowColor = rowColor;
        this.woodButtonColor = woodButtonColor;
        this.frameColor = frameColor;
        this.hireManager = hireManager;
        this.trainingGroundManager = trainingGroundManager;
        this.merchantData = merchantData;
        this.dayManager = dayManager;
        this.townProgressState = townProgressState;
        this.registerPage = registerPage;
        this.switchToPage = switchToPage;
        this.refreshPage = refreshPage;
        this.setStatus = setStatus;
        this.refreshUI = refreshUI;
    }

    public void Build()
    {
        if (pageUI.HasLayout)
        {
            Configure(pageUI);
            registerPage?.Invoke(page);
            return;
        }

        Text title = factory.CreateText(page, "修練場", 24,
            FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -48f),
            new Vector2(-24f, -12f), parchmentTextColor);
        Text description = factory.CreateText(page, string.Empty, 15,
            FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(24f, -80f),
            new Vector2(-24f, -52f), mutedTextColor);
        RectTransform viewport = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Training Ground Viewport", page);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -86f);
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform listRoot = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Training Ground List", viewport);
        listRoot.anchorMin = new Vector2(0f, 1f);
        listRoot.anchorMax = new Vector2(1f, 1f);
        listRoot.pivot = new Vector2(0.5f, 1f);
        listRoot.anchoredPosition = Vector2.zero;
        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = listRoot;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        pageUI.Initialize(title, description, scrollRect, listRoot);
        Configure(pageUI);
        registerPage?.Invoke(page);
    }

    public void HandleTrainingGroundChanged()
    {
        refreshPage?.Invoke(page);
        refreshUI?.Invoke();
    }

    public void Show()
    {
        if (!TownServicePolicy.IsTrainingGroundAvailable(
                townProgressState.CurrentTownIndex))
        {
            setStatus?.Invoke("この町には修練場がありません。");
            return;
        }

        switchToPage?.Invoke(page);
        refreshPage?.Invoke(page);
    }

    private void Configure(TrainingGroundPageUI pageUI)
    {
        pageUI.Configure(bodyFont, parchmentTextColor, mutedTextColor,
            buttonTextColor, rowColor, woodButtonColor, frameColor, null, 24);
        pageUI.ConfigureTrainingGround(
            () => hireManager.HiredMercenaries,
            BuildTrainingDetails,
            BuildTrainingState,
            CanStartTraining,
            TryStartTraining,
            () => $"修練中 {trainingGroundManager.ActiveTrainingCount} / {TrainingGroundManager.MaximumConcurrentTrainings}");
    }

    private static string BuildTrainingDetails(MercenaryInstance mercenary)
    {
        int targetLevel = mercenary.Level + 1;
        int cost = TrainingCostService.GetCost(targetLevel);
        return $"{mercenary.MercenaryName}  Lv{mercenary.Level} → Lv{targetLevel}  |  {cost} G";
    }

    private string BuildTrainingState(MercenaryInstance mercenary)
    {
        int cost = TrainingCostService.GetCost(mercenary.Level + 1);
        TrainingUnavailableReason unavailableReason =
            trainingGroundManager.GetUnavailableReason(mercenary);
        return unavailableReason == TrainingUnavailableReason.AlreadyTraining
            ? GetTrainingState(mercenary)
            : GetTrainingUnavailableReason(unavailableReason, cost);
    }

    private bool CanStartTraining(MercenaryInstance mercenary)
    {
        return trainingGroundManager.GetUnavailableReason(mercenary) ==
               TrainingUnavailableReason.None;
    }

    private void TryStartTraining(MercenaryInstance mercenary)
    {
        if (trainingGroundManager.TryStartTraining(mercenary))
        {
            setStatus?.Invoke($"{mercenary.MercenaryName}を修練に預けました。");
        }
        else
        {
            setStatus?.Invoke(GetTrainingUnavailableReason(
                trainingGroundManager.GetUnavailableReason(mercenary),
                TrainingCostService.GetCost(mercenary.Level + 1)));
        }

        refreshPage?.Invoke(page);
    }

    private string GetTrainingState(MercenaryInstance mercenary)
    {
        foreach (TrainingReservation reservation in
                 trainingGroundManager.ActiveReservations)
        {
            if (reservation != null &&
                reservation.MercenaryInstanceId == mercenary.InstanceId)
            {
                return $"修練中（あと{Mathf.Max(0, reservation.CompletionDay - dayManager.CurrentDay)}日）";
            }
        }

        return "修練中";
    }

    private string GetTrainingUnavailableReason(
        TrainingUnavailableReason reason,
        int cost)
    {
        switch (reason)
        {
            case TrainingUnavailableReason.None:
                return string.Empty;
            case TrainingUnavailableReason.MissingManagerReference:
            case TrainingUnavailableReason.InvalidMercenary:
            case TrainingUnavailableReason.NotHired:
                return "この傭兵は選択できません。";
            case TrainingUnavailableReason.AtLevelCap:
                return "レベル上限に到達しています。";
            case TrainingUnavailableReason.ContractExpired:
                return "契約が切れています。";
            case TrainingUnavailableReason.Incapacitated:
                return "戦闘不能中の傭兵は修練できません。";
            case TrainingUnavailableReason.DifferentTown:
                return "別の町にいます。";
            case TrainingUnavailableReason.NoFacilityInTown:
                return "この町には修練場がありません。";
            case TrainingUnavailableReason.InParty:
                return "先に隊列から外してください。";
            case TrainingUnavailableReason.OnTransport:
            case TrainingUnavailableReason.OnExpedition:
                return "移動・遠征中のため利用できません。";
            case TrainingUnavailableReason.AlreadyTraining:
                return "修練中です。";
            case TrainingUnavailableReason.SlotsFull:
                return "同時修練枠が埋まっています。";
            case TrainingUnavailableReason.LevelLimit:
                return $"他の傭兵より2レベル以上低い必要があります。（現在の上限 Lv{trainingGroundManager.GetMaximumTrainableLevel()}）";
            case TrainingUnavailableReason.InsufficientGold:
                return $"資金不足（あと{cost - merchantData.Gold} G）。";
            default:
                return "修練を開始できません。";
        }
    }
}
