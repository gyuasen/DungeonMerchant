using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void HandleTrainingGroundChanged()
    {
        RefreshPage(trainingGroundPage);
        RefreshUI();
    }

    private void BuildTrainingGroundPage()
    {
        TrainingGroundPageUI pageUI =
            trainingGroundPage.GetComponent<TrainingGroundPageUI>() ??
            trainingGroundPage.gameObject.AddComponent<TrainingGroundPageUI>();
        if (pageUI.HasLayout)
        {
            ConfigureTrainingGroundPage(pageUI);
            pageRouter.Register(trainingGroundPage);
            return;
        }
        Text title = CreateText(trainingGroundPage, "修練場", 24,
            FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -48f),
            new Vector2(-24f, -12f), ParchmentTextColor);
        Text description = CreateText(trainingGroundPage, string.Empty, 15,
            FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(24f, -80f),
            new Vector2(-24f, -52f), ParchmentMutedColor);
        RectTransform viewport = CreateUIObject(
            "Training Ground Viewport", trainingGroundPage);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -86f);
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform listRoot = CreateUIObject("Training Ground List", viewport);
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
        ConfigureTrainingGroundPage(pageUI);
        pageRouter.Register(trainingGroundPage);
    }

    private void ConfigureTrainingGroundPage(TrainingGroundPageUI pageUI)
    {
        pageUI.Configure(uiBodyFont, ParchmentTextColor, MutedTextColor,
            ButtonTextColor, RowColor, WoodButtonColor, FrameColor, null, 24);
        pageUI.ConfigureTrainingGround(
            () => hireManager.HiredMercenaries,
            BuildTrainingDetails,
            BuildTrainingState,
            CanStartTraining,
            TryStartTrainingFromPage,
            () => $"修練中 {trainingGroundManager.ActiveTrainingCount} / {TrainingGroundManager.MaximumConcurrentTrainings}");
    }

    private void ShowTrainingGroundPage()
    {
        if (!TownServicePolicy.IsTrainingGroundAvailable(
                townProgressState.CurrentTownIndex))
        {
            statusText.text = "この町には修練場がありません。";
            return;
        }

        SwitchToPage(trainingGroundPage);
        RefreshPage(trainingGroundPage);
    }

    private string BuildTrainingDetails(MercenaryInstance mercenary)
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

    private void TryStartTrainingFromPage(MercenaryInstance mercenary)
    {
        if (trainingGroundManager.TryStartTraining(mercenary))
        {
            statusText.text = $"{mercenary.MercenaryName}を修練に預けました。";
        }
        else
        {
            statusText.text = GetTrainingUnavailableReason(
                trainingGroundManager.GetUnavailableReason(mercenary),
                TrainingCostService.GetCost(mercenary.Level + 1));
        }

        RefreshPage(trainingGroundPage);
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
                return "この傭兵は確認できません。";
            case TrainingUnavailableReason.AtLevelCap:
                return "レベル上限に到達しています。";
            case TrainingUnavailableReason.ContractExpired:
                return "契約が切れています。";
            case TrainingUnavailableReason.Incapacitated:
                return "戦闘不能の傭兵は修練できません。";
            case TrainingUnavailableReason.DifferentTown:
                return "別の町にいます。";
            case TrainingUnavailableReason.NoFacilityInTown:
                return "この町には修練場がありません。";
            case TrainingUnavailableReason.InParty:
                return "先に編成から外してください。";
            case TrainingUnavailableReason.OnTransport:
            case TrainingUnavailableReason.OnExpedition:
                return "他の任務に就いています。";
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
