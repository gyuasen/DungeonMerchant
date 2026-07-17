using UnityEngine;
using UnityEngine.UI;

// Overlay creation/show/hide routing for the daily result feature.
// The snapshot data and text building live in DailyResultController.
public partial class SimpleMercenaryHireUI
{
    private void BuildDailyResultOverlay()
    {
        dailyResultOverlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.DailyResult,
                "Daily Result Overlay");
        dailyResultOverlay.gameObject.SetActive(false);
        dailyResultOverlay.anchorMin = Vector2.zero;
        dailyResultOverlay.anchorMax = Vector2.one;
        dailyResultOverlay.offsetMin = Vector2.zero;
        dailyResultOverlay.offsetMax = Vector2.zero;
        dailyResultOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.84f);

        RectTransform window =
            CreateUIObject("Daily Result Window", dailyResultOverlay);
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

        dailyResultContent =
            CreateUIObject("Daily Result Content", viewport);
        dailyResultContent.anchorMin = new Vector2(0f, 1f);
        dailyResultContent.anchorMax = new Vector2(1f, 1f);
        dailyResultContent.pivot = new Vector2(0.5f, 1f);
        dailyResultText = CreateText(
            dailyResultContent,
            string.Empty,
            17,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(16f, 16f),
            new Vector2(-16f, -16f),
            ParchmentTextColor);
        dailyResultText.rectTransform.anchorMin = Vector2.zero;
        dailyResultText.rectTransform.anchorMax = Vector2.one;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = dailyResultContent;
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

        dailyResultOverlay.gameObject.SetActive(false);
    }

    private void HideDailyResult()
    {
        dailyResultOverlay?.gameObject.SetActive(false);
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
        ShowDailyResult(currentDay);
    }

    private void ShowDailyResult(int currentDay)
    {
        string resultText =
            dailyResultOverlay == null || dailyResultText == null
                ? null
                : dailyResultController.BuildDailyResultText(currentDay);
        if (resultText == null)
        {
            dailyResultController.CaptureDailySnapshot(currentDay);
            return;
        }

        dailyResultText.text = resultText;
        int lineCount = resultText.Split('\n').Length;
        dailyResultContent.sizeDelta =
            new Vector2(0f, Mathf.Max(420f, 40f + lineCount * 34f));
        dailyResultOverlay.SetAsLastSibling();
        dailyResultOverlay.gameObject.SetActive(true);
        dailyResultController.CaptureDailySnapshot(currentDay);
    }
}
