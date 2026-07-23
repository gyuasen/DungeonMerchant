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
        pageUI.ConfigureTrainingGround(RefreshTrainingGroundPage);
        pageRouter.Register(trainingGroundPage);
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
        RefreshTrainingGroundPage();
    }

    private void RefreshTrainingGroundPage()
    {
        if (trainingGroundPage == null || trainingGroundManager == null)
        {
            return;
        }

        for (int index = trainingGroundPage.childCount - 1;
             index >= 0;
             index--)
        {
            Destroy(trainingGroundPage.GetChild(index).gameObject);
        }

        CreateText(trainingGroundPage, "修練場", 24, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(24f, -48f),
            new Vector2(-24f, -12f), ParchmentTextColor);
        CreateText(trainingGroundPage,
            $"修練中 {trainingGroundManager.ActiveTrainingCount} / {TrainingGroundManager.MaximumConcurrentTrainings}",
            15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(24f, -80f), new Vector2(-24f, -52f),
            ParchmentMutedColor);

        float top = -118f;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary == null)
            {
                continue;
            }

            RectTransform row = CreateRow(
                "Training " + mercenary.InstanceId,
                trainingGroundPage,
                top);
            top -= 82f;
            int targetLevel = mercenary.Level + 1;
            int cost = TrainingCostService.GetCost(targetLevel);
            TrainingUnavailableReason unavailableReason =
                trainingGroundManager.GetUnavailableReason(mercenary);
            bool training = unavailableReason ==
                TrainingUnavailableReason.AlreadyTraining;
            string state = training
                ? GetTrainingState(mercenary)
                : GetTrainingUnavailableReason(unavailableReason, cost);
            CreateText(row,
                $"{mercenary.MercenaryName}  Lv{mercenary.Level} → Lv{targetLevel}  |  {cost} G",
                16, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(16f, -40f), new Vector2(-170f, -8f),
                ParchmentTextColor);
            CreateText(row, state, 13, FontStyle.Normal,
                TextAnchor.MiddleLeft, new Vector2(16f, -72f),
                new Vector2(-170f, -42f), ParchmentMutedColor);
            Button button = CreateActionButton(row,
                training ? "修練中" : "修練させる",
                () => TryStartTrainingFromPage(mercenary));
            button.interactable = unavailableReason ==
                                  TrainingUnavailableReason.None;
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(130f, 46f);
            buttonRect.anchoredPosition = new Vector2(-18f, 0f);
        }
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

        RefreshTrainingGroundPage();
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
                return "傭兵情報を確認できません。";
            case TrainingUnavailableReason.AtLevelCap:
                return "レベル上限に到達しています。";
            case TrainingUnavailableReason.ContractExpired:
                return "契約が切れています。";
            case TrainingUnavailableReason.Incapacitated:
                return "戦闘不能の傭兵は利用できません。";
            case TrainingUnavailableReason.DifferentTown:
                return "別の町にいます。";
            case TrainingUnavailableReason.NoFacilityInTown:
                return "この町には修練場がありません。";
            case TrainingUnavailableReason.InParty:
                return "編成に加わっています。";
            case TrainingUnavailableReason.OnTransport:
                return "輸送任務中です。";
            case TrainingUnavailableReason.OnExpedition:
                return "遠征中です。";
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
