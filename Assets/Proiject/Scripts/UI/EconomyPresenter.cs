using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public sealed class EconomyPresenter
{
    private static readonly Color ImportantButtonColor = UITheme.ImportantButtonColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color WhiteColor = Color.white;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly RectTransform overlayRoot;
    private readonly EconomyController economyController;
    private readonly MerchantInventory merchantInventory;
    private readonly MerchantData merchantData;
    private readonly MarketStockManager marketStockManager;
    private readonly BlacksmithManager blacksmithManager;
    private readonly Action<string> setStatusText;
    private readonly SimpleMercenaryHireUIView.SellQuantityReferences sellQuantity =
        new SimpleMercenaryHireUIView.SellQuantityReferences();
    private readonly SimpleMercenaryHireUIView.SellOnlyConfirmationReferences
        sellOnlyConfirmation =
            new SimpleMercenaryHireUIView.SellOnlyConfirmationReferences();
    private RectTransform itemDetailOverlay;
    private Image itemDetailImage;
    private Text itemDetailImagePlaceholder;
    private Text itemDetailTitle;
    private Text itemDetailText;
    private Text itemDetailTransactionText;
    private Button itemDetailActionButton;
    private Action itemDetailAction;

    public EconomyPresenter(
        SimpleMercenaryHireUIFactory factory,
        RectTransform overlayRoot,
        EconomyController economyController,
        MerchantInventory merchantInventory,
        MerchantData merchantData,
        MarketStockManager marketStockManager,
        BlacksmithManager blacksmithManager,
        Action<string> setStatusText)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        if (overlayRoot == null) throw new ArgumentNullException(nameof(overlayRoot));
        this.economyController = economyController ??
            throw new ArgumentNullException(nameof(economyController));
        if (merchantInventory == null) throw new ArgumentNullException(nameof(merchantInventory));
        if (merchantData == null) throw new ArgumentNullException(nameof(merchantData));
        if (marketStockManager == null) throw new ArgumentNullException(nameof(marketStockManager));
        if (blacksmithManager == null) throw new ArgumentNullException(nameof(blacksmithManager));
        this.overlayRoot = overlayRoot;
        this.merchantInventory = merchantInventory;
        this.merchantData = merchantData;
        this.marketStockManager = marketStockManager;
        this.blacksmithManager = blacksmithManager;
        this.setStatusText = setStatusText ?? throw new ArgumentNullException(nameof(setStatusText));
    }

    public void BuildItemDetailOverlay()
    {
        itemDetailOverlay = CreateUIObject("Item Detail Overlay", overlayRoot);
        itemDetailOverlay.anchorMin = Vector2.zero;
        itemDetailOverlay.anchorMax = Vector2.one;
        itemDetailOverlay.offsetMin = Vector2.zero;
        itemDetailOverlay.offsetMax = Vector2.zero;
        itemDetailOverlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Item Detail Window", itemDetailOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(680f, 600f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        itemDetailTitle = CreateText(window, string.Empty, 25, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(142f, -64f), new Vector2(-34f, -20f), ParchmentTextColor);
        RectTransform imageRect = CreateUIObject("Item Detail Image", window);
        imageRect.anchorMin = imageRect.anchorMax = new Vector2(0f, 1f);
        imageRect.pivot = new Vector2(0f, 1f);
        imageRect.sizeDelta = new Vector2(92f, 92f);
        imageRect.anchoredPosition = new Vector2(34f, -28f);
        itemDetailImage = imageRect.gameObject.AddComponent<Image>();
        itemDetailImagePlaceholder = CreateText(imageRect, "?", 42, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, WhiteColor);
        itemDetailImagePlaceholder.rectTransform.anchorMin = Vector2.zero;
        itemDetailImagePlaceholder.rectTransform.anchorMax = Vector2.one;
        itemDetailImagePlaceholder.rectTransform.offsetMin = Vector2.zero;
        itemDetailImagePlaceholder.rectTransform.offsetMax = Vector2.zero;
        itemDetailText = CreateText(window, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(34f, -230f), new Vector2(-34f, -92f), ParchmentTextColor);
        itemDetailTransactionText = CreateText(window, string.Empty, 15, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(34f, -490f), new Vector2(-34f, -232f), MutedTextColor);
        itemDetailActionButton = CreateActionButton(window, string.Empty, ExecuteItemDetailAction);
        RectTransform actionRect = itemDetailActionButton.GetComponent<RectTransform>();
        actionRect.anchorMin = actionRect.anchorMax = actionRect.pivot = new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(180f, 48f);
        actionRect.anchoredPosition = new Vector2(-105f, 26f);
        Button closeButton = CreateActionButton(window, "閉じる", HideItemDetail);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 48f);
        closeRect.anchoredPosition = new Vector2(105f, 26f);
        itemDetailOverlay.gameObject.SetActive(false);
    }

    public void BuildSellOnlyConfirmationOverlay()
    {
        sellOnlyConfirmation.overlay = CreateUIObject("Sell Only Confirmation Overlay", overlayRoot);
        sellOnlyConfirmation.overlay.gameObject.SetActive(false);
        sellOnlyConfirmation.overlay.anchorMin = Vector2.zero;
        sellOnlyConfirmation.overlay.anchorMax = Vector2.one;
        sellOnlyConfirmation.overlay.offsetMin = Vector2.zero;
        sellOnlyConfirmation.overlay.offsetMax = Vector2.zero;
        sellOnlyConfirmation.overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Sell Only Confirmation Window", sellOnlyConfirmation.overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(560f, 340f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "売却用・環境素材を一括売却しますか？", 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(28f, -72f), new Vector2(-28f, -22f), ParchmentTextColor);
        sellOnlyConfirmation.text = CreateText(window, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(36f, -194f), new Vector2(-36f, -82f), ParchmentTextColor);
        Button confirm = CreateActionButton(window, "すべて売却", ConfirmSellOnlyMaterials);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot = new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 26f);
        confirm.targetGraphic.color = ImportantButtonColor;
        Button cancel = CreateActionButton(window, "キャンセル", HideSellOnlyConfirmation);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 26f);
        sellOnlyConfirmation.overlay.gameObject.SetActive(false);
    }

    public void ShowSellOnlyConfirmation()
    {
        List<InventoryItemStack> stacks = economyController.GetSellOnlyStacks();
        int itemCount = 0;
        foreach (InventoryItemStack stack in stacks) itemCount += stack.Amount;
        sellOnlyConfirmation.text.text = itemCount > 0
            ? $"売却対象: {stacks.Count}種類 / {itemCount}個\n合計獲得: {economyController.GetSellOnlyTotalGold():N0}G\n制作素材は対象に含まれません。"
            : "売却できる売却用・環境素材はありません。\n制作素材は対象に含まれません。";
        sellOnlyConfirmation.overlay.SetAsLastSibling();
        sellOnlyConfirmation.overlay.gameObject.SetActive(true);
    }

    private void ConfirmSellOnlyMaterials()
    {
        int earnedGold = economyController.SellAllSellOnlyMaterials(out int soldCount, out bool stoppedEarly);
        HideSellOnlyConfirmation();
        setStatusText(stoppedEarly ? $"{soldCount}個を売却し、{earnedGold:N0}Gを獲得しました。残りは売却していません。" : soldCount > 0 ? $"売却用・環境素材を{soldCount}個まとめて売却し、{earnedGold:N0}Gを獲得しました。" : "売却できる売却用・環境素材はありません。");
    }

    private void HideSellOnlyConfirmation() => sellOnlyConfirmation.overlay?.gameObject.SetActive(false);

    public void BuildSellQuantityOverlay()
    {
        sellQuantity.overlay = CreateUIObject("Sell Quantity Overlay", overlayRoot);
        sellQuantity.overlay.anchorMin = Vector2.zero;
        sellQuantity.overlay.anchorMax = Vector2.one;
        sellQuantity.overlay.offsetMin = Vector2.zero;
        sellQuantity.overlay.offsetMax = Vector2.zero;
        sellQuantity.overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Sell Quantity Window", sellQuantity.overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(560f, 360f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        sellQuantity.titleText = CreateText(window, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(28f, -58f), new Vector2(-28f, -18f), ParchmentTextColor);
        RectTransform sellImageRect = CreateUIObject("Sell Item Image", window);
        sellImageRect.anchorMin = sellImageRect.anchorMax = sellImageRect.pivot = new Vector2(0.5f, 1f);
        sellImageRect.sizeDelta = new Vector2(72f, 72f);
        sellImageRect.anchoredPosition = new Vector2(0f, -64f);
        sellQuantity.image = sellImageRect.gameObject.AddComponent<Image>();
        sellQuantity.imagePlaceholder = CreateText(sellImageRect, "?", 32, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, ParchmentTextColor);
        sellQuantity.detailText = CreateText(window, string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(36f, -220f), new Vector2(-36f, -146f), ParchmentTextColor);
        Button minusButton = CreateActionButton(window, "－", () => AdjustSellQuantity(-1));
        RectTransform minusRect = minusButton.GetComponent<RectTransform>(); minusRect.anchorMin = minusRect.anchorMax = minusRect.pivot = new Vector2(0.5f, 0f); minusRect.sizeDelta = new Vector2(64f, 48f); minusRect.anchoredPosition = new Vector2(-150f, 96f);
        Button plusButton = CreateActionButton(window, "＋", () => AdjustSellQuantity(1));
        RectTransform plusRect = plusButton.GetComponent<RectTransform>(); plusRect.anchorMin = plusRect.anchorMax = plusRect.pivot = new Vector2(0.5f, 0f); plusRect.sizeDelta = new Vector2(64f, 48f); plusRect.anchoredPosition = new Vector2(-78f, 96f);
        Button allButton = CreateActionButton(window, "全部", SelectAllSellQuantity);
        RectTransform allRect = allButton.GetComponent<RectTransform>(); allRect.anchorMin = allRect.anchorMax = allRect.pivot = new Vector2(0.5f, 0f); allRect.sizeDelta = new Vector2(120f, 48f); allRect.anchoredPosition = new Vector2(120f, 96f);
        Button confirm = CreateActionButton(window, "売却する", ConfirmSellQuantity);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>(); confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot = new Vector2(0.5f, 0f); confirmRect.sizeDelta = new Vector2(180f, 48f); confirmRect.anchoredPosition = new Vector2(-105f, 26f); confirm.targetGraphic.color = ImportantButtonColor;
        Button cancel = CreateActionButton(window, "やめる", HideSellQuantityOverlay);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>(); cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot = new Vector2(0.5f, 0f); cancelRect.sizeDelta = new Vector2(180f, 48f); cancelRect.anchoredPosition = new Vector2(105f, 26f);
        sellQuantity.overlay.gameObject.SetActive(false);
    }

    public void ShowSellQuantityOverlay(ItemDataSO item)
    {
        if (item == null || economyController == null) return;
        int owned = economyController.GetItemAmount(item);
        if (owned <= 0) { setStatusText($"{JapaneseDisplayText.GetItemName(item)}は所持していません。"); return; }
        sellQuantity.item = item; sellQuantity.amount = 1; sellQuantity.titleText.text = JapaneseDisplayText.GetItemName(item);
        Sprite sprite = ItemPresentationService.ResolveSprite(item);
        sellQuantity.image.sprite = sprite; sellQuantity.image.color = sprite != null ? WhiteColor : new Color(1f, 1f, 1f, 0f);
        sellQuantity.imagePlaceholder.gameObject.SetActive(sprite == null);
        RefreshSellQuantityDetail(); sellQuantity.overlay.SetAsLastSibling(); sellQuantity.overlay.gameObject.SetActive(true);
    }

    private void AdjustSellQuantity(int delta)
    {
        if (sellQuantity.item == null) return;
        int owned = economyController.GetItemAmount(sellQuantity.item);
        if (owned <= 0) { setStatusText($"{JapaneseDisplayText.GetItemName(sellQuantity.item)}は所持していません。"); HideSellQuantityOverlay(); return; }
        sellQuantity.amount = Mathf.Clamp(sellQuantity.amount + delta, 1, Mathf.Max(1, owned)); RefreshSellQuantityDetail();
    }

    private void SelectAllSellQuantity()
    {
        if (sellQuantity.item == null) return;
        int owned = economyController.GetItemAmount(sellQuantity.item);
        if (owned <= 0) { setStatusText($"{JapaneseDisplayText.GetItemName(sellQuantity.item)}は所持していません。"); HideSellQuantityOverlay(); return; }
        sellQuantity.amount = owned; RefreshSellQuantityDetail();
    }

    private void RefreshSellQuantityDetail()
    {
        if (sellQuantity.item == null) return;
        int owned = economyController.GetItemAmount(sellQuantity.item);
        int unitPrice = economyController.GetSellPrice(sellQuantity.item);
        sellQuantity.detailText.text = $"数量: {sellQuantity.amount} / 所持 {owned}\n単価: {unitPrice:N0}G\n合計獲得: {unitPrice * sellQuantity.amount:N0}G";
    }

    private void ConfirmSellQuantity()
    {
        if (sellQuantity.item == null) { HideSellQuantityOverlay(); return; }
        ItemDataSO item = sellQuantity.item; int owned = economyController.GetItemAmount(item);
        if (owned <= 0) { HideSellQuantityOverlay(); setStatusText($"{JapaneseDisplayText.GetItemName(item)}は所持していません。"); return; }
        int amount = Mathf.Clamp(sellQuantity.amount, 1, owned); HideSellQuantityOverlay(); economyController.SellItem(item, amount);
    }

    private void HideSellQuantityOverlay() { sellQuantity.item = null; sellQuantity.overlay?.gameObject.SetActive(false); }

    public void ShowBlacksmithRecipeDetail(EquipmentRecipeSO recipe)
    {
        if (recipe == null || recipe.resultItem == null) return;
        string materials = BuildRecipeDetailText(recipe);
        string transaction = materials + "\n必要金額: " + recipe.goldCost + "G  |  所持金: " + merchantData.Gold + "G";
        ShowItemDetail(recipe.resultItem, transaction, "制作する", () => economyController.CraftEquipment(recipe), blacksmithManager.CanCraft(recipe));
    }

    public void ShowMarketItemDetail(MarketStockEntry entry)
    {
        if (entry == null || entry.Item == null) return;
        string transaction = "価格: " + entry.BuyPrice + "G  |  所持金: " + merchantData.Gold + "G\n在庫: " + entry.Quantity;
        ShowItemDetail(entry.Item, transaction, "購入する", () => economyController.BuyMarketItem(entry), marketStockManager.CanBuy(entry));
    }

    private string BuildRecipeDetailText(EquipmentRecipeSO recipe)
    {
        if (recipe.materials == null || recipe.materials.Length == 0) return "必要素材: なし";
        StringBuilder result = new StringBuilder("必要素材:");
        foreach (CraftingMaterialRequirement requirement in recipe.materials)
        {
            if (requirement == null || requirement.item == null) continue;
            int owned = merchantInventory.GetItemAmount(requirement.item);
            string color = owned >= requirement.amount ? "#3D8A45" : "#B43A2F";
            result.Append("\n<color=").Append(color).Append(">").Append(JapaneseDisplayText.GetItemName(requirement.item)).Append(" ").Append(owned).Append("/").Append(requirement.amount).Append("</color>").Append("\n  入手: ").Append(ItemUsageTextBuilder.BuildAcquisitionText(requirement.item));
        }
        return result.ToString();
    }

    private void ShowItemDetail(ItemDataSO item, string transactionText, string actionLabel, Action action, bool canExecute)
    {
        itemDetailTitle.text = JapaneseDisplayText.GetItemName(item);
        Sprite sprite = ItemPresentationService.ResolveSprite(item);
        itemDetailImage.sprite = sprite; itemDetailImage.color = sprite != null ? WhiteColor : new Color(0.2f, 0.2f, 0.2f, 1f);
        itemDetailImagePlaceholder.gameObject.SetActive(sprite == null);
        itemDetailText.text = ItemPresentationService.BuildDetailText(item); itemDetailTransactionText.text = transactionText;
        factory.SetOrCreateButtonLabel(itemDetailActionButton.GetComponent<RectTransform>(), actionLabel, 17);
        itemDetailAction = action; itemDetailActionButton.interactable = canExecute;
        itemDetailOverlay.SetAsLastSibling(); itemDetailOverlay.gameObject.SetActive(true);
    }

    private void ExecuteItemDetailAction() { itemDetailAction?.Invoke(); HideItemDetail(); }
    private void HideItemDetail() { itemDetailOverlay?.gameObject.SetActive(false); itemDetailAction = null; }

    private Text CreateText(RectTransform parent, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax, Color color) => factory.CreateText(parent, content, fontSize, fontStyle, alignment, offsetMin, offsetMax, color);
    private Button CreateActionButton(RectTransform parent, string label, UnityEngine.Events.UnityAction action) => factory.CreateActionButton(parent, label, action);
    private static RectTransform CreateUIObject(string objectName, Transform parent) => SimpleMercenaryHireUIFactory.CreateUIObject(objectName, parent);
    private static void ApplyParchmentPanel(Image image) => SimpleMercenaryHireUIFactory.ApplyParchmentPanel(image);
}
