using UnityEngine;
using UnityEngine.UI;

// Transport overlay construction and event routing. Feature state lives in
// TransportController so this partial only owns UI objects and subscriptions.
public partial class SimpleMercenaryHireUI
{
    private void BuildTransportOverlay()
    {
        transportOverlay = CreateUIObject("Transport Overlay", overlayRoot);
        transportOverlay.anchorMin = Vector2.zero;
        transportOverlay.anchorMax = Vector2.one;
        transportOverlay.offsetMin = Vector2.zero;
        transportOverlay.offsetMax = Vector2.zero;
        transportOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject("Transport Window", transportOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(900f, 650f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "輸送部隊", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-140f, -20f), ParchmentTextColor);

        transportContent = CreateScrollableContent(
            window, "Transport Viewport", "Transport Content",
            new Vector2(28f, 86f), new Vector2(-28f, -82f));
        transportFooterText = CreateText(window, string.Empty, 16, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(28f, 28f), new Vector2(-250f, 70f),
            ParchmentTextColor);
        Button departButton = CreateActionButton(window, "出発", transportController.Depart);
        RectTransform departRect = departButton.GetComponent<RectTransform>();
        departRect.anchorMin = departRect.anchorMax = new Vector2(1f, 0f);
        departRect.pivot = new Vector2(1f, 0f);
        departRect.sizeDelta = new Vector2(110f, 42f);
        departRect.anchoredPosition = new Vector2(-140f, 24f);
        Button closeButton = CreateActionButton(window, "閉じる", HideTransportOverlay);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 0f);
        closeRect.pivot = new Vector2(1f, 0f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-28f, 24f);
        transportOverlay.gameObject.SetActive(false);
    }

    private RectTransform CreateScrollableContent(
        RectTransform parent, string viewportName, string contentName,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform viewport = CreateUIObject(viewportName, parent);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = offsetMin;
        viewport.offsetMax = offsetMax;
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform content = CreateUIObject(contentName, viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        return content;
    }

    private void ShowTransportOverlay()
    {
        RefreshTransportOverlay();
        transportOverlay.SetAsLastSibling();
        transportOverlay.gameObject.SetActive(true);
    }

    private void HideTransportOverlay()
    {
        transportOverlay?.gameObject.SetActive(false);
    }

    private void RefreshTransportOverlay()
    {
        if (transportContent == null || transportController == null)
        {
            return;
        }
        for (int i = transportContent.childCount - 1; i >= 0; i--)
        {
            Destroy(transportContent.GetChild(i).gameObject);
        }
        float top = -12f;
        CreateTransportSection("進行中の輸送部隊", ref top);
        bool hasConvoy = false;
        foreach (TransportConvoy convoy in transportController.ActiveConvoys)
        {
            hasConvoy = true;
            CreateTransportLabel(transportController.BuildConvoyText(convoy), ref top);
        }
        if (!hasConvoy) CreateTransportLabel("進行中の部隊はいません", ref top);
        CreateTransportSection("新規編成", ref top);
        CreateTransportLabel("目的地", ref top);
        for (int i = 0; i < WorldMapService.TownNames.Length; i++)
        {
            if (!transportController.IsDestinationAvailable(i)) continue;
            int townIndex = i;
            CreateTransportButton(
                (transportController.DestinationTownIndex == i ? "● " : "○ ") +
                WorldMapService.TownNames[i], () => transportController.SelectDestination(townIndex), ref top);
        }
        CreateTransportLabel("積荷（通常アイテム）", ref top);
        bool hasCargo = false;
        foreach (InventoryItemStack stack in transportController.GetCargoCandidates())
        {
            hasCargo = true;
            CreateCargoRow(stack, ref top);
        }
        if (!hasCargo) CreateTransportLabel("輸送できる在庫がありません", ref top);
        CreateTransportLabel("護衛（最大3人）", ref top);
        bool hasEscort = false;
        foreach (MercenaryInstance mercenary in transportController.GetAvailableEscorts())
        {
            hasEscort = true;
            MercenaryInstance selected = mercenary;
            CreateTransportButton(
                (transportController.IsEscortSelected(mercenary) ? "● " : "○ ") +
                mercenary.MercenaryName + "  Lv" + mercenary.Level,
                () => transportController.ToggleEscort(selected), ref top);
        }
        if (!hasEscort) CreateTransportLabel("割り当て可能な傭兵はいません", ref top);
        transportContent.sizeDelta = new Vector2(0f, Mathf.Max(420f, -top + 12f));
        if (transportFooterText != null)
        {
            transportFooterText.text = "輸送費 " + transportController.GetTransportCost() +
                " G    目的地での想定価値 約" + transportController.GetEstimatedSaleGold() + " G";
        }
    }

    private void CreateTransportSection(string text, ref float top)
    {
        Text label = CreateText(transportContent, text, 19, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(14f, top - 30f), new Vector2(-14f, top),
            ParchmentTextColor);
        top -= 38f;
    }

    private void CreateTransportLabel(string text, ref float top)
    {
        CreateText(transportContent, text, 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(22f, top - 28f), new Vector2(-22f, top), ParchmentMutedColor);
        top -= 32f;
    }

    private void CreateTransportButton(string text, UnityEngine.Events.UnityAction action, ref float top)
    {
        Button button = CreateActionButton(transportContent, text, action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(360f, 30f);
        rect.anchoredPosition = new Vector2(18f, top);
        top -= 34f;
    }

    private void CreateCargoRow(InventoryItemStack stack, ref float top)
    {
        ItemDataSO item = stack.Item;
        int amount = transportController.GetSelectedCargoAmount(item);
        CreateTransportLabel(JapaneseDisplayText.GetItemName(item) + "  在庫" + stack.Amount + "  選択" + amount, ref top);
        float rowTop = top + 30f;
        Button minus = CreateActionButton(transportContent, "－", () => transportController.ChangeCargo(item, stack.Amount, -1));
        Button plus = CreateActionButton(transportContent, "＋", () => transportController.ChangeCargo(item, stack.Amount, 1));
        RectTransform minusRect = minus.GetComponent<RectTransform>();
        minusRect.anchorMin = minusRect.anchorMax = new Vector2(0f, 1f);
        minusRect.pivot = new Vector2(0f, 1f);
        minusRect.sizeDelta = new Vector2(45f, 28f);
        minusRect.anchoredPosition = new Vector2(390f, rowTop);
        RectTransform plusRect = plus.GetComponent<RectTransform>();
        plusRect.anchorMin = plusRect.anchorMax = new Vector2(0f, 1f);
        plusRect.pivot = new Vector2(0f, 1f);
        plusRect.sizeDelta = new Vector2(45f, 28f);
        plusRect.anchoredPosition = new Vector2(442f, rowTop);
    }

    private void HandleTransportChanged()
    {
        RefreshPage(companyPage);
        if (transportOverlay != null && transportOverlay.gameObject.activeSelf) RefreshTransportOverlay();
    }

    private void HandleTransportEvent(TransportEvent transportEvent)
    {
        dailyResultController.RecordTransportEvent(transportEvent);
        HandleTransportChanged();
    }
}
