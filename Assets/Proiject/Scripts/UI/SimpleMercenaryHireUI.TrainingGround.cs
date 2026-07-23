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
            bool training = trainingGroundManager.IsMercenaryTraining(
                mercenary.InstanceId);
            string state = training
                ? GetTrainingState(mercenary)
                : GetTrainingUnavailableReason(mercenary, cost);
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
            button.interactable = !training &&
                                  trainingGroundManager.CanStartTraining(
                                      mercenary);
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
                mercenary,
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
        MercenaryInstance mercenary,
        int cost)
    {
        if (!TownServicePolicy.IsTrainingGroundAvailable(
                townProgressState.CurrentTownIndex)) return "この町には修練場がありません。";
        if (mercenary.IsAtLevelCap) return "レベル上限に到達しています。";
        if (!mercenary.IsContractActive) return "契約が切れています。";
        if (mercenary.IsIncapacitated) return "戦闘不能の傭兵は利用できません。";
        if (mercenary.CurrentTownIndex != townProgressState.CurrentTownIndex) return "別の町にいます。";
        if (trainingGroundManager.IsMercenaryTraining(mercenary.InstanceId)) return "修練中です。";
        if (trainingGroundManager.ActiveTrainingCount >= TrainingGroundManager.MaximumConcurrentTrainings) return "同時修練枠が埋まっています。";
        if (merchantData.Gold < cost) return $"資金不足（あと{cost - merchantData.Gold} G）。";
        return trainingGroundManager.CanStartTraining(mercenary)
            ? string.Empty
            : "他任務中、または最高Lv-2の上限を超えています。";
    }
}
