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
        Text title = CreateText(trainingGroundPage, "Training Ground", 24,
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
            () => $"Training {trainingGroundManager.ActiveTrainingCount} / {TrainingGroundManager.MaximumConcurrentTrainings}");
    }

    private void ShowTrainingGroundPage()
    {
        if (!TownServicePolicy.IsTrainingGroundAvailable(
                townProgressState.CurrentTownIndex))
        {
            statusText.text = "A training ground is not available in this town.";
            return;
        }

        SwitchToPage(trainingGroundPage);
        RefreshPage(trainingGroundPage);
    }

    private string BuildTrainingDetails(MercenaryInstance mercenary)
    {
        int targetLevel = mercenary.Level + 1;
        int cost = TrainingCostService.GetCost(targetLevel);
        return $"{mercenary.MercenaryName}  Lv{mercenary.Level} -> Lv{targetLevel}  |  {cost} G";
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
            statusText.text = $"{mercenary.MercenaryName} started training.";
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
                return $"Training: {Mathf.Max(0, reservation.CompletionDay - dayManager.CurrentDay)} days remaining";
            }
        }

        return "Training";
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
                return "The mercenary cannot be confirmed.";
            case TrainingUnavailableReason.AtLevelCap:
                return "Already at the maximum level.";
            case TrainingUnavailableReason.ContractExpired:
                return "The contract has expired.";
            case TrainingUnavailableReason.Incapacitated:
                return "Incapacitated mercenaries cannot train.";
            case TrainingUnavailableReason.DifferentTown:
                return "The mercenary is in another town.";
            case TrainingUnavailableReason.NoFacilityInTown:
                return "A training ground is not available in this town.";
            case TrainingUnavailableReason.InParty:
                return "Remove the mercenary from the party first.";
            case TrainingUnavailableReason.OnTransport:
                return "The mercenary is on transport duty.";
            case TrainingUnavailableReason.OnExpedition:
                return "The mercenary is on an expedition.";
            case TrainingUnavailableReason.AlreadyTraining:
                return "Already training.";
            case TrainingUnavailableReason.SlotsFull:
                return "All training slots are occupied.";
            case TrainingUnavailableReason.LevelLimit:
                return $"Requires two levels below the town limit (Lv{trainingGroundManager.GetMaximumTrainableLevel()}).";
            case TrainingUnavailableReason.InsufficientGold:
                return $"Need {cost - merchantData.Gold} G more.";
            default:
                return "Unable to start training.";
        }
    }
}
