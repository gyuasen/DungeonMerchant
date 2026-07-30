using UnityEngine;
using UnityEngine.UI;

// Overlay creation/show/hide routing for the daily result feature.
// The snapshot data and text building live in DailyResultController.
public partial class SimpleMercenaryHireUI
{
    private void HandleTrainingCompleted(TrainingReservation reservation)
    {
        string line = dailyResultController.RecordTrainingCompleted(reservation);
        if (!string.IsNullOrEmpty(line) &&
            dailyResult.overlay != null &&
            dailyResult.overlay.gameObject.activeSelf &&
            dailyResult.text != null)
        {
            dailyResult.text.text += "\n" + line;
            dailyResultController.ConsumeRecordedTrainingCompletion(line);
        }
    }

    private void BuildDailyResultOverlay()
    {
        dailyResult.overlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.DailyResult,
                "Daily Result Overlay");
        dailyResult.overlay.gameObject.SetActive(false);
        dailyResult.overlay.anchorMin = Vector2.zero;
        dailyResult.overlay.anchorMax = Vector2.one;
        dailyResult.overlay.offsetMin = Vector2.zero;
        dailyResult.overlay.offsetMax = Vector2.zero;
        dailyResult.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.84f);

        RectTransform window =
            CreateUIObject("Daily Result Window", dailyResult.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 580f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "一日のリザルト",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(130f, -66f),
            new Vector2(-130f, -18f),
            ParchmentTextColor);

        RectTransform viewport =
            CreateUIObject("Daily Result Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 76f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        dailyResult.content =
            CreateUIObject("Daily Result Content", viewport);
        dailyResult.content.anchorMin = new Vector2(0f, 1f);
        dailyResult.content.anchorMax = new Vector2(1f, 1f);
        dailyResult.content.pivot = new Vector2(0.5f, 1f);
        dailyResult.text = CreateText(
            dailyResult.content,
            string.Empty,
            17,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(16f, 16f),
            new Vector2(-16f, -16f),
            ParchmentTextColor);
        dailyResult.text.supportRichText = true;
        dailyResult.text.rectTransform.anchorMin = Vector2.zero;
        dailyResult.text.rectTransform.anchorMax = Vector2.one;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = dailyResult.content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        Button closeButton =
            CreateActionButton(window, "確認", HideDailyResult);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax =
            new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 18f);

        dailyResult.overlay.gameObject.SetActive(false);
    }

    private void HideDailyResult()
    {
        dailyResult.overlay?.gameObject.SetActive(false);
        ShowPendingDailyResultIfReady();
    }

    private void HandleDayChanged(int currentDay)
    {
        if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            mercenaryGenerator.ClearCandidates();
        }
        RefreshPage(marketPage);
        RefreshPage(inventoryPage);
        RefreshPage(healPage);
        RefreshPage(companyPage);
        RefreshUI();
        string debtNotice = debtManager != null &&
                            (currentDay - 1) % DebtManager.DaysPerMonth == 0 &&
                            currentDay > 1
            ? debtManager.PaymentArrears > 0
                ? $" 月次返済後の滞納額は{debtManager.PaymentArrears:N0}Gです。"
                : $" 月次最低返済を完了しました。"
            : string.Empty;
        statusText.text =
            $"{currentDay}日目になりました。市場価格が更新されました。{debtNotice}";
    }

    private void HandleDayChangeFinalized(int currentDay)
    {
        // 各日のリザルトをキューへ積むだけにとどめ、表示は複数日の進行が
        // すべて終わってから(HandleDaysAdvanceCompleted)まとめて行う。
        // これにより複数日一気に進んでも1画面に連結して表示できる。
        QueueDailyResult(currentDay);
    }

    private void HandleDaysAdvanceCompleted(int advancedDays)
    {
        ShowPendingDailyResultIfReady();
    }

    private void ShowPendingDailyResultIfReady()
    {
        if (!dailyResult.hasPending ||
            (dailyResult.overlay != null &&
             dailyResult.overlay.gameObject.activeSelf))
        {
            return;
        }

        if (battleVisualController != null &&
            battleVisualController.IsPresentationBusy)
        {
            return;
        }

        // キューに溜まった複数日分を1画面へ連結して表示する。
        System.Text.StringBuilder combined = new System.Text.StringBuilder();
        bool first = true;
        while (dailyResult.pendingTexts.Count > 0)
        {
            if (!first)
            {
                combined.AppendLine();
                combined.AppendLine("────────────");
                combined.AppendLine();
            }

            combined.Append(dailyResult.pendingTexts.Dequeue());
            first = false;
        }

        dailyResult.hasPending = false;
        ShowDailyResult(combined.ToString());
    }

    private void QueueDailyResult(int currentDay)
    {
        string resultText =
            dailyResult.overlay == null || dailyResult.text == null
                ? null
                : dailyResultController.BuildDailyResultText(currentDay);
        if (resultText == null)
        {
            dailyResultController.CaptureDailySnapshot(currentDay);
            return;
        }

        dailyResult.pendingTexts.Enqueue(resultText);
        dailyResult.hasPending = true;
        dailyResultController.CaptureDailySnapshot(currentDay);
    }

    private void ShowDailyResult(string resultText)
    {
        dailyResult.text.text = resultText;
        int lineCount = resultText.Split('\n').Length;
        dailyResult.content.sizeDelta =
            new Vector2(0f, Mathf.Max(420f, 40f + lineCount * 34f));
        dailyResult.overlay.SetAsLastSibling();
        dailyResult.overlay.gameObject.SetActive(true);
    }
}
