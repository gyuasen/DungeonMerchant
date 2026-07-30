using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class RemoteSaleOverlayView
{
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.82f);
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly Transform parent;
    private readonly RemoteSaleController controller;
    private readonly RemoteSaleManager manager;
    private readonly MerchantInventory inventory;
    private readonly UnityAction onClose;
    private RectTransform overlay;
    private RectTransform content;

    public RemoteSaleOverlayView(
        SimpleMercenaryHireUIFactory factory,
        Transform parent,
        RemoteSaleController controller,
        RemoteSaleManager manager,
        MerchantInventory inventory,
        UnityAction onClose)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.controller = controller;
        this.manager = manager;
        this.inventory = inventory;
        this.onClose = onClose;
    }

    public void Build()
    {
        overlay = SimpleMercenaryHireUIFactory.CreateUIObject("Remote Sale Overlay", parent);
        overlay.gameObject.SetActive(false);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.gameObject.AddComponent<Image>().color = OverlayColor;
        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject("Remote Sale Window", overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(900f, 650f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        factory.CreateText(window, "全町倉庫", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-28f, -20f), ParchmentTextColor);
        content = CreateScrollableContent(window, "Remote Sale Viewport", "Remote Sale Content",
            new Vector2(28f, 86f), new Vector2(-28f, -82f));
        Button confirm = factory.CreateActionButton(window, "売却指示", controller.ConfirmItems);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(1f, 0f);
        confirmRect.pivot = new Vector2(1f, 0f);
        confirmRect.sizeDelta = new Vector2(120f, 42f);
        confirmRect.anchoredPosition = new Vector2(-140f, 24f);
        Button close = factory.CreateActionButton(window, "閉じる", onClose);
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 0f);
        closeRect.pivot = new Vector2(1f, 0f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-28f, 24f);
    }

    public void Show()
    {
        Refresh();
        overlay.SetAsLastSibling();
        overlay.gameObject.SetActive(true);
    }

    public void Hide()
    {
        overlay?.gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (content == null || controller == null)
        {
            return;
        }
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(content.GetChild(i).gameObject);
        }
        float top = -12f;
        CreateScrollSection(content, "町を選択", ref top);
        for (int i = 0; i < WorldMapService.TownNames.Length; i++)
        {
            if (!controller.IsTownAvailable(i))
            {
                continue;
            }
            int town = i;
            CreateScrollButton(content,
                (controller.SelectedTownIndex == town ? "● " : "○ ") + WorldMapService.TownNames[town],
                () => controller.SelectTown(town), ref top);
        }
        if (controller.SelectedTownIndex >= 0)
        {
            int town = controller.SelectedTownIndex;
            CreateScrollSection(content, WorldMapService.TownNames[town] + "の倉庫", ref top);
            CreateScrollLabel(content, "使用容量 " + inventory.GetUsedStorageSlotsIn(town), ref top);
            foreach (InventoryItemStack stack in controller.GetItems())
            {
                CreateRemoteItemRow(stack, ref top);
            }
            foreach (EquipmentInstance equipment in controller.GetEquipment())
            {
                EquipmentInstance selected = equipment;
                CreateScrollButton(content, JapaneseDisplayText.GetItemName(equipment.BaseItem) +
                    " / 売却指示", () => controller.SellEquipment(selected), ref top);
            }
            CreateScrollLabel(content, "予想 " + controller.GetSelectedEstimatedGold() +
                "G / 約定まで" + controller.GetSettlementDays() + "日", ref top);
        }
        CreateScrollSection(content, "進行中の売却指示", ref top);
        foreach (RemoteSaleOrder order in controller.ActiveOrders)
        {
            RemoteSaleOrder selected = order;
            string name = order.IsEquipment ? JapaneseDisplayText.GetItemName(order.Equipment.BaseItem) :
                JapaneseDisplayText.GetItemName(order.Item) + "×" + order.Amount;
            CreateScrollButton(content, WorldMapService.TownNames[order.TownIndex] + " / " + name +
                " / 残り" + order.RemainingDays + "日 / 約" + manager.GetEstimatedGold(order) + "G / 取消",
                () => controller.Cancel(selected), ref top);
        }
        content.sizeDelta = new Vector2(0f, Mathf.Max(420f, -top + 12f));
    }

    public bool IsShowing => overlay != null && overlay.gameObject.activeSelf;

    private RectTransform CreateScrollableContent(
        RectTransform parent, string viewportName, string contentName,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform viewport = SimpleMercenaryHireUIFactory.CreateUIObject(viewportName, parent);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = offsetMin;
        viewport.offsetMax = offsetMax;
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform scrollContent = SimpleMercenaryHireUIFactory.CreateUIObject(contentName, viewport);
        scrollContent.anchorMin = new Vector2(0f, 1f);
        scrollContent.anchorMax = new Vector2(1f, 1f);
        scrollContent.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = scrollContent;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        return scrollContent;
    }

    private void CreateScrollSection(RectTransform scrollContent, string text, ref float top)
    {
        factory.CreateText(scrollContent, text, 19, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(14f, top - 30f), new Vector2(-14f, top), ParchmentTextColor);
        top -= 38f;
    }

    private void CreateScrollLabel(RectTransform scrollContent, string text, ref float top)
    {
        factory.CreateText(scrollContent, text, 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(22f, top - 28f), new Vector2(-22f, top), ParchmentMutedColor);
        top -= 32f;
    }

    private void CreateScrollButton(
        RectTransform scrollContent, string text, UnityAction action, ref float top)
    {
        Button button = factory.CreateActionButton(scrollContent, text, action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(360f, 30f);
        rect.anchoredPosition = new Vector2(18f, top);
        top -= 34f;
    }

    private void CreateRemoteItemRow(InventoryItemStack stack, ref float top)
    {
        ItemDataSO item = stack.Item;
        int amount = controller.GetSelectedAmount(item);
        CreateScrollLabel(content, JapaneseDisplayText.GetItemName(item) + "×" + stack.Amount +
            " / 指示 " + amount, ref top);
        float rowTop = top + 30f;
        Button minus = factory.CreateActionButton(content, "－", () => controller.ChangeAmount(item, stack.Amount, -1));
        Button plus = factory.CreateActionButton(content, "＋", () => controller.ChangeAmount(item, stack.Amount, 1));
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
}
