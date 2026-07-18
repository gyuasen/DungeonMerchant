using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildRemoteSaleOverlay()
    {
        remoteSaleOverlay = CreateUIObject("Remote Sale Overlay", overlayRoot);
        remoteSaleOverlay.gameObject.SetActive(false);
        remoteSaleOverlay.anchorMin = Vector2.zero;
        remoteSaleOverlay.anchorMax = Vector2.one;
        remoteSaleOverlay.offsetMin = Vector2.zero;
        remoteSaleOverlay.offsetMax = Vector2.zero;
        remoteSaleOverlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Remote Sale Window", remoteSaleOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(900f, 650f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "全町倉庫", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-28f, -20f), ParchmentTextColor);
        remoteSaleContent = CreateScrollableContent(window, "Remote Sale Viewport", "Remote Sale Content",
            new Vector2(28f, 86f), new Vector2(-28f, -82f));
        Button confirm = CreateActionButton(window, "売却指示", remoteSaleController.ConfirmItems);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(1f, 0f);
        confirmRect.pivot = new Vector2(1f, 0f);
        confirmRect.sizeDelta = new Vector2(120f, 42f);
        confirmRect.anchoredPosition = new Vector2(-140f, 24f);
        Button close = CreateActionButton(window, "閉じる", HideRemoteSaleOverlay);
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 0f);
        closeRect.pivot = new Vector2(1f, 0f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-28f, 24f);
    }

    private void ShowRemoteSaleOverlay()
    {
        RefreshRemoteSaleOverlay();
        remoteSaleOverlay.SetAsLastSibling();
        remoteSaleOverlay.gameObject.SetActive(true);
    }

    private void HideRemoteSaleOverlay()
    {
        remoteSaleOverlay?.gameObject.SetActive(false);
    }

    private void RefreshRemoteSaleOverlay()
    {
        if (remoteSaleContent == null || remoteSaleController == null)
        {
            return;
        }
        for (int i = remoteSaleContent.childCount - 1; i >= 0; i--)
        {
            Destroy(remoteSaleContent.GetChild(i).gameObject);
        }
        float top = -12f;
        CreateScrollSection(remoteSaleContent, "町を選択", ref top);
        for (int i = 0; i < WorldMapService.TownNames.Length; i++)
        {
            if (!remoteSaleController.IsTownAvailable(i))
            {
                continue;
            }
            int town = i;
            CreateScrollButton(remoteSaleContent,
                (remoteSaleController.SelectedTownIndex == town ? "● " : "○ ") + WorldMapService.TownNames[town],
                () => remoteSaleController.SelectTown(town), ref top);
        }
        if (remoteSaleController.SelectedTownIndex >= 0)
        {
            int town = remoteSaleController.SelectedTownIndex;
            CreateScrollSection(remoteSaleContent, WorldMapService.TownNames[town] + "の倉庫", ref top);
            CreateScrollLabel(remoteSaleContent, "使用容量 " + merchantInventory.GetUsedStorageSlotsIn(town), ref top);
            foreach (InventoryItemStack stack in remoteSaleController.GetItems())
            {
                CreateRemoteItemRow(stack, ref top);
            }
            foreach (EquipmentInstance equipment in remoteSaleController.GetEquipment())
            {
                EquipmentInstance selected = equipment;
                CreateScrollButton(remoteSaleContent, JapaneseDisplayText.GetItemName(equipment.BaseItem) +
                    " / 売却指示", () => remoteSaleController.SellEquipment(selected), ref top);
            }
            CreateScrollLabel(remoteSaleContent, "予想 " + remoteSaleController.GetSelectedEstimatedGold() +
                "G / 約定まで" + remoteSaleController.GetSettlementDays() + "日", ref top);
        }
        CreateScrollSection(remoteSaleContent, "進行中の売却指示", ref top);
        foreach (RemoteSaleOrder order in remoteSaleController.ActiveOrders)
        {
            RemoteSaleOrder selected = order;
            string name = order.IsEquipment ? JapaneseDisplayText.GetItemName(order.Equipment.BaseItem) :
                JapaneseDisplayText.GetItemName(order.Item) + "×" + order.Amount;
            CreateScrollButton(remoteSaleContent, WorldMapService.TownNames[order.TownIndex] + " / " + name +
                " / 残り" + order.RemainingDays + "日 / 約" + remoteSaleManager.GetEstimatedGold(order) + "G / 取消",
                () => remoteSaleController.Cancel(selected), ref top);
        }
        remoteSaleContent.sizeDelta = new Vector2(0f, Mathf.Max(420f, -top + 12f));
    }

    private void CreateRemoteItemRow(InventoryItemStack stack, ref float top)
    {
        ItemDataSO item = stack.Item;
        int amount = remoteSaleController.GetSelectedAmount(item);
        CreateScrollLabel(remoteSaleContent, JapaneseDisplayText.GetItemName(item) + "×" + stack.Amount +
            " / 指示 " + amount, ref top);
        float rowTop = top + 30f;
        Button minus = CreateActionButton(remoteSaleContent, "－", () => remoteSaleController.ChangeAmount(item, stack.Amount, -1));
        Button plus = CreateActionButton(remoteSaleContent, "＋", () => remoteSaleController.ChangeAmount(item, stack.Amount, 1));
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

    private void HandleRemoteSaleChanged()
    {
        RefreshPage(companyPage);
        if (remoteSaleOverlay != null && remoteSaleOverlay.gameObject.activeSelf)
        {
            RefreshRemoteSaleOverlay();
        }
    }

    private void HandleRemoteSaleEvent(RemoteSaleEvent remoteSaleEvent)
    {
        dailyResultController.RecordRemoteSaleEvent(remoteSaleEvent);
        HandleRemoteSaleChanged();
    }
}
