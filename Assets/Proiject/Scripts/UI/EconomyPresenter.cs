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
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;
    private static readonly Color ButtonTextColor = UITheme.ButtonTextColor;
    private static readonly Color RowColor = UITheme.RowColor;
    private static readonly Color WoodButtonColor = UITheme.WoodButtonColor;
    private static readonly Color FrameColor = UITheme.FrameColor;
    private static readonly Color AccentColor = UITheme.AccentColor;
    private static readonly Color WhiteColor = Color.white;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly RectTransform overlayRoot;
    private readonly EconomyController economyController;
    private readonly MerchantInventory merchantInventory;
    private readonly MerchantData merchantData;
    private readonly MarketStockManager marketStockManager;
    private readonly BlacksmithManager blacksmithManager;
    private readonly Action<string> setStatusText;
    private readonly RectTransform inventoryPage;
    private readonly RectTransform marketPage;
    private readonly RectTransform blacksmithPage;
    private readonly Font uiBodyFont;
    private readonly MarketPriceManager marketPriceManager;
    private readonly TownProgressState townProgressState;
    private readonly DayManager dayManager;
    private readonly ProgressionManager progressionManager;
    private readonly MerchantStatusAndQuestController merchantStatusAndQuestController;
    private readonly DailyResultController dailyResultController;
    private readonly Func<Button> inventoryTabButtonProvider;
    private readonly Func<Button> marketTabButtonProvider;
    private readonly Func<Button> blacksmithTabButtonProvider;
    private readonly Action<RectTransform, Button> switchToPage;
    private readonly Action<RectTransform> refreshPage;
    private readonly Action refreshUI;
    private readonly Action tryUnlockHiddenIsland;
    private readonly Action showEquipmentCollection;
    private readonly Action<ItemDataSO> useConsumable;
    private readonly Action<EquipmentInstance> showEquipmentDetails;
    private readonly Action<RectTransform> registerPage;
    private RectTransform inventoryList;
    private RectTransform marketList;
    private RectTransform blacksmithList;
    private Button nextDayButton;
    private Button inventoryFilterButton;
    private Button equipmentSortButton;
    private readonly List<Button> inventorySidebarButtons = new List<Button>();
    private readonly List<Button> marketSidebarButtons = new List<Button>();
    private readonly List<Button> blacksmithSidebarButtons = new List<Button>();
    private Text marketInfoText;
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
    private readonly SimpleMercenaryHireUIView.StorageUpgradeReferences storageUpgrade =
        new SimpleMercenaryHireUIView.StorageUpgradeReferences();

    public EconomyPresenter(
        SimpleMercenaryHireUIFactory factory,
        RectTransform overlayRoot,
        EconomyController economyController,
        MerchantInventory merchantInventory,
        MerchantData merchantData,
        MarketStockManager marketStockManager,
        BlacksmithManager blacksmithManager,
        RectTransform inventoryPage,
        RectTransform marketPage,
        RectTransform blacksmithPage,
        Font uiBodyFont,
        MarketPriceManager marketPriceManager,
        TownProgressState townProgressState,
        DayManager dayManager,
        ProgressionManager progressionManager,
        MerchantStatusAndQuestController merchantStatusAndQuestController,
        DailyResultController dailyResultController,
        Func<Button> inventoryTabButtonProvider,
        Func<Button> marketTabButtonProvider,
        Func<Button> blacksmithTabButtonProvider,
        Action<RectTransform, Button> switchToPage,
        Action<RectTransform> refreshPage,
        Action refreshUI,
        Action tryUnlockHiddenIsland,
        Action showEquipmentCollection,
        Action<ItemDataSO> useConsumable,
        Action<EquipmentInstance> showEquipmentDetails,
        Action<RectTransform> registerPage,
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
        if (inventoryPage == null) throw new ArgumentNullException(nameof(inventoryPage));
        if (marketPage == null) throw new ArgumentNullException(nameof(marketPage));
        if (blacksmithPage == null) throw new ArgumentNullException(nameof(blacksmithPage));
        if (uiBodyFont == null) throw new ArgumentNullException(nameof(uiBodyFont));
        if (marketPriceManager == null) throw new ArgumentNullException(nameof(marketPriceManager));
        if (townProgressState == null) throw new ArgumentNullException(nameof(townProgressState));
        if (dayManager == null) throw new ArgumentNullException(nameof(dayManager));
        if (merchantStatusAndQuestController == null) throw new ArgumentNullException(nameof(merchantStatusAndQuestController));
        if (dailyResultController == null) throw new ArgumentNullException(nameof(dailyResultController));
        if (inventoryTabButtonProvider == null) throw new ArgumentNullException(nameof(inventoryTabButtonProvider));
        if (marketTabButtonProvider == null) throw new ArgumentNullException(nameof(marketTabButtonProvider));
        if (blacksmithTabButtonProvider == null) throw new ArgumentNullException(nameof(blacksmithTabButtonProvider));
        this.overlayRoot = overlayRoot;
        this.merchantInventory = merchantInventory;
        this.merchantData = merchantData;
        this.marketStockManager = marketStockManager;
        this.blacksmithManager = blacksmithManager;
        this.inventoryPage = inventoryPage;
        this.marketPage = marketPage;
        this.blacksmithPage = blacksmithPage;
        this.uiBodyFont = uiBodyFont;
        this.marketPriceManager = marketPriceManager;
        this.townProgressState = townProgressState;
        this.dayManager = dayManager;
        this.progressionManager = progressionManager;
        this.merchantStatusAndQuestController = merchantStatusAndQuestController;
        this.dailyResultController = dailyResultController;
        this.inventoryTabButtonProvider = inventoryTabButtonProvider;
        this.marketTabButtonProvider = marketTabButtonProvider;
        this.blacksmithTabButtonProvider = blacksmithTabButtonProvider;
        this.switchToPage = switchToPage ?? throw new ArgumentNullException(nameof(switchToPage));
        this.refreshPage = refreshPage ?? throw new ArgumentNullException(nameof(refreshPage));
        this.refreshUI = refreshUI ?? throw new ArgumentNullException(nameof(refreshUI));
        this.tryUnlockHiddenIsland = tryUnlockHiddenIsland ?? throw new ArgumentNullException(nameof(tryUnlockHiddenIsland));
        this.showEquipmentCollection = showEquipmentCollection ?? throw new ArgumentNullException(nameof(showEquipmentCollection));
        this.useConsumable = useConsumable ?? throw new ArgumentNullException(nameof(useConsumable));
        this.showEquipmentDetails = showEquipmentDetails ?? throw new ArgumentNullException(nameof(showEquipmentDetails));
        this.registerPage = registerPage ?? throw new ArgumentNullException(nameof(registerPage));
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

    public void BuildInventoryPage()
    {
        Text title = CreateText(inventoryPage, "商人在庫", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);
        Text capacityText = CreateText(inventoryPage, string.Empty, 16, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(180f, -30f), new Vector2(-150f, 0f), ParchmentTextColor);
        storageUpgrade.capacityText = capacityText;
        marketInfoText = CreateText(inventoryPage, string.Empty, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(-160f, -38f), ParchmentTextColor);
        nextDayButton = CreateActionButton(inventoryPage, "翌日へ", () => AdvanceDay());
        RectTransform nextDayRect = nextDayButton.GetComponent<RectTransform>(); nextDayRect.anchorMin = nextDayRect.anchorMax = new Vector2(1f, 1f); nextDayRect.pivot = new Vector2(1f, 1f); nextDayRect.anchoredPosition = new Vector2(0f, -34f);
        const float topRowY = -78f; const float bottomRowY = -120f;
        inventoryFilterButton = CreateActionButton(inventoryPage, "絞込: 全て", economyController.CycleInventoryFilter); inventoryFilterButton.name = "Inventory Filter Button";
        RectTransform filterRect = inventoryFilterButton.GetComponent<RectTransform>(); filterRect.anchorMin = filterRect.anchorMax = new Vector2(0f, 1f); filterRect.pivot = new Vector2(0f, 1f); filterRect.sizeDelta = new Vector2(150f, 38f); filterRect.anchoredPosition = new Vector2(142f, topRowY);
        equipmentSortButton = CreateActionButton(inventoryPage, "並替: 名前", economyController.CycleEquipmentSort); equipmentSortButton.name = "Equipment Sort Button";
        RectTransform sortRect = equipmentSortButton.GetComponent<RectTransform>(); sortRect.anchorMin = sortRect.anchorMax = new Vector2(0f, 1f); sortRect.pivot = new Vector2(0f, 1f); sortRect.sizeDelta = new Vector2(150f, 38f); sortRect.anchoredPosition = new Vector2(308f, topRowY);
        Button collectionButton = CreateActionButton(inventoryPage, "装備図鑑", () => showEquipmentCollection());
        RectTransform collectionRect = collectionButton.GetComponent<RectTransform>(); collectionRect.anchorMin = collectionRect.anchorMax = new Vector2(0f, 1f); collectionRect.pivot = new Vector2(0f, 1f); collectionRect.sizeDelta = new Vector2(150f, 38f); collectionRect.anchoredPosition = new Vector2(142f, bottomRowY);
        Button storageButton = CreateActionButton(inventoryPage, "倉庫拡張", () => ShowStorageUpgradeConfirmation());
        RectTransform storageRect = storageButton.GetComponent<RectTransform>(); storageRect.anchorMin = storageRect.anchorMax = new Vector2(0f, 1f); storageRect.pivot = new Vector2(0f, 1f); storageRect.sizeDelta = new Vector2(150f, 38f); storageRect.anchoredPosition = new Vector2(308f, bottomRowY);
        Button sellOnlyButton = CreateActionButton(inventoryPage, "売却用素材を一括売却", ShowSellOnlyConfirmation);
        RectTransform sellOnlyRect = sellOnlyButton.GetComponent<RectTransform>(); sellOnlyRect.anchorMin = sellOnlyRect.anchorMax = new Vector2(0f, 1f); sellOnlyRect.pivot = new Vector2(0f, 1f); sellOnlyRect.sizeDelta = new Vector2(210f, 38f); sellOnlyRect.anchoredPosition = new Vector2(474f, bottomRowY);
        CreateInventorySidebar();
        RectTransform viewport = CreateViewport("Inventory Viewport", inventoryPage, new Vector2(142f, 0f), new Vector2(0f, -166f));
        inventoryList = CreateList("Inventory List", viewport);
        ConfigureScrollRect(viewport, inventoryList);
        InventoryPageUI pageUI = inventoryPage.GetComponent<InventoryPageUI>() ?? inventoryPage.gameObject.AddComponent<InventoryPageUI>();
        pageUI.Initialize(title, null, inventoryList);
        pageUI.ConfigureInventory(uiBodyFont, ParchmentMutedColor, MutedTextColor, ButtonTextColor, RowColor, WoodButtonColor, FrameColor, () => merchantInventory.Items, economyController.GetSortedInventoryEquipment, economyController.ShouldShowInventoryItem, economyController.ShouldShowInventoryEquipment, item => merchantInventory.GetSellPrice(item), item => marketPriceManager.GetEffectiveSellMultiplier(item), item => WorldMapService.GetTownDemandMultiplier(townProgressState.CurrentTownIndex, item), CharacterEquipmentController.GetEquipmentDisplayName, CharacterEquipmentController.GetEquipmentQualityColor, ShowSellQuantityOverlay, useConsumable, showEquipmentDetails);
        registerPage(inventoryPage); UpdateStorageCapacityText();
    }

    public void BuildMarketPage()
    {
        Text title = CreateText(marketPage, "本日の仕入れ商品", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);
        Text demandSummary = CreateText(marketPage, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(0f, -38f), ParchmentMutedColor);
        CreateMarketSidebar(); RectTransform viewport = CreateViewport("Market Viewport", marketPage, new Vector2(142f, 0f), new Vector2(0f, -84f));
        marketList = CreateList("Market List", viewport); ConfigureScrollRect(viewport, marketList);
        MarketPageUI pageUI = marketPage.GetComponent<MarketPageUI>() ?? marketPage.gameObject.AddComponent<MarketPageUI>();
        pageUI.Initialize(title, demandSummary, marketList);
        pageUI.ConfigureMarket(uiBodyFont, ParchmentMutedColor, MutedTextColor, ButtonTextColor, RowColor, WoodButtonColor, FrameColor, economyController.GetMarketRows, economyController.ShouldShowMarketEntryForSidebar, entry => marketStockManager.CanBuy(entry), economyController.BuyMarketItem, economyController.RegisterMarketBuyButton, demandSummary, GetCurrentTownDemandSummary, ShowMarketItemDetail);
        registerPage(marketPage);
    }

    public void BuildBlacksmithPage()
    {
        Text title = CreateText(blacksmithPage, "鍛冶屋", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);
        Text description = CreateText(blacksmithPage, "モンスター素材とゴールドを使い、市場では買えない武器を制作します。", 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(0f, -38f), ParchmentMutedColor);
        CreateBlacksmithSidebar();
        Button craftableButton = CreateActionButton(blacksmithPage, "製作可能のみ: OFF", ToggleBlacksmithCraftableOnly);
        RectTransform craftableRect = craftableButton.GetComponent<RectTransform>(); craftableRect.anchorMin = craftableRect.anchorMax = new Vector2(1f, 1f); craftableRect.pivot = new Vector2(1f, 1f); craftableRect.sizeDelta = new Vector2(150f, 32f); craftableRect.anchoredPosition = new Vector2(0f, -34f);
        Button rankSortButton = CreateActionButton(blacksmithPage, "ランク順: 昇順", ToggleBlacksmithRankSort);
        RectTransform rankSortRect = rankSortButton.GetComponent<RectTransform>(); rankSortRect.anchorMin = rankSortRect.anchorMax = new Vector2(1f, 1f); rankSortRect.pivot = new Vector2(1f, 1f); rankSortRect.sizeDelta = new Vector2(150f, 32f); rankSortRect.anchoredPosition = new Vector2(-160f, -34f);
        RectTransform viewport = CreateViewport("Blacksmith Viewport", blacksmithPage, new Vector2(142f, 0f), new Vector2(0f, -84f));
        blacksmithList = CreateList("Blacksmith List", viewport); ConfigureScrollRect(viewport, blacksmithList);
        BlacksmithPageUI pageUI = blacksmithPage.GetComponent<BlacksmithPageUI>() ?? blacksmithPage.gameObject.AddComponent<BlacksmithPageUI>();
        pageUI.Initialize(title, description, blacksmithList);
        pageUI.ConfigureBlacksmith(uiBodyFont, ParchmentMutedColor, MutedTextColor, ButtonTextColor, RowColor, WoodButtonColor, FrameColor, economyController.GetSortedBlacksmithRows, economyController.ShouldShowBlacksmithRecipeForSidebar, item => merchantInventory.GetItemAmount(item), recipe => blacksmithManager.CanCraft(recipe), economyController.CraftEquipment, economyController.RegisterBlacksmithCraftButton, ShowBlacksmithRecipeDetail);
        registerPage(blacksmithPage);
    }

    public void SetInventoryFilterLabel(string label) { if (inventoryFilterButton != null) inventoryFilterButton.GetComponentInChildren<Text>().text = label; }
    public void SetEquipmentSortLabel(string label) { if (equipmentSortButton != null) equipmentSortButton.GetComponentInChildren<Text>().text = label; }
    public void SetMarketInfoText(string text) { if (marketInfoText != null) marketInfoText.text = text; }

    public void HandleInventoryChanged()
    {
        dailyResultController.RecordDailyInventoryGains();
        tryUnlockHiddenIsland();
        refreshPage(inventoryPage);
        refreshPage(blacksmithPage);
        refreshUI();
    }

    public void HandleMarketStockChanged()
    {
        refreshPage(marketPage);
        refreshUI();
    }

    public void HandleCraftingChanged()
    {
        refreshPage(inventoryPage);
        refreshPage(blacksmithPage);
        refreshUI();
    }

    public void HandlePricesChanged()
    {
        refreshPage(inventoryPage);
        refreshUI();
    }

    public void ShowMarketPage()
    {
        switchToPage(marketPage, marketTabButtonProvider());
        setStatusText(
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}市場  |  " +
            $"仕入れ商品: {marketStockManager.Stock.Count}種類 / " +
            marketPriceManager.GetMarketSummary());
    }

    public void ShowBlacksmithPage()
    {
        switchToPage(blacksmithPage, blacksmithTabButtonProvider());
        setStatusText(
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}鍛冶屋  |  " +
            $"レシピ: {blacksmithManager.Recipes.Count}種類");
        refreshUI();
    }

    public void ShowInventoryPage()
    {
        UpdateStorageCapacityText();
        switchToPage(inventoryPage, inventoryTabButtonProvider());
        setStatusText(
            $"倉庫 {merchantInventory.GetUsedStorageSlots()}/" +
            $"{(progressionManager != null ? progressionManager.StorageCapacity : 0)}  |  " +
            $"{marketPriceManager.GetMarketSummary()}  |  " +
            $"維持費 {(progressionManager != null ? progressionManager.StorageMaintenanceCost : 0)}G/日");
    }

    public void UpdateStorageCapacityText()
    {
        if (storageUpgrade.capacityText == null)
        {
            return;
        }

        int used = merchantInventory != null
            ? merchantInventory.GetUsedStorageSlots()
            : 0;
        int capacity = progressionManager != null
            ? progressionManager.StorageCapacity
            : 0;
        int remaining = Mathf.Max(0, capacity - used);
        string expansion = progressionManager == null
            ? string.Empty
            : progressionManager.IsStorageAtMaximumTier
                ? "最大拡張済み"
                : $"次回 {progressionManager.NextStorageCapacity}枠 / " +
                  $"{progressionManager.StorageUpgradeCost:N0}G / " +
                  $"商人Lv{progressionManager.NextStorageRequiredMerchantLevel}";

        storageUpgrade.capacityText.text =
            $"倉庫 {used}/{capacity}（空き {remaining}）  |  {expansion}";
        storageUpgrade.capacityText.color = capacity > 0 && remaining == 0
            ? new Color(0.65f, 0.08f, 0.04f)
            : remaining <= Mathf.Max(3, Mathf.CeilToInt(capacity * 0.1f))
                ? new Color(0.72f, 0.35f, 0.04f)
                : ParchmentTextColor;
    }

    public void BuildStorageUpgradeConfirmationOverlay()
    {
        storageUpgrade.confirmationOverlay = CreateUIObject(
            "Storage Upgrade Confirmation Overlay", overlayRoot);
        storageUpgrade.confirmationOverlay.gameObject.SetActive(false);
        storageUpgrade.confirmationOverlay.anchorMin = Vector2.zero;
        storageUpgrade.confirmationOverlay.anchorMax = Vector2.one;
        storageUpgrade.confirmationOverlay.offsetMin = Vector2.zero;
        storageUpgrade.confirmationOverlay.offsetMax = Vector2.zero;
        storageUpgrade.confirmationOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject(
            "Storage Upgrade Confirmation Window", storageUpgrade.confirmationOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(560f, 340f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "倉庫を拡張しますか？", 26, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(28f, -72f),
            new Vector2(-28f, -22f), ParchmentTextColor);
        storageUpgrade.confirmationText = CreateText(window, string.Empty, 18,
            FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(36f, -190f),
            new Vector2(-36f, -82f), ParchmentTextColor);
        storageUpgrade.confirmationReasonText = CreateText(window, string.Empty, 15,
            FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(36f, -238f),
            new Vector2(-36f, -190f), MutedTextColor);
        storageUpgrade.confirmButton = CreateActionButton(
            window, "拡張する", ConfirmStorageUpgrade);
        RectTransform confirmRect =
            storageUpgrade.confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot =
            new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 26f);
        storageUpgrade.confirmButton.targetGraphic.color = AccentColor;
        Button cancelButton = CreateActionButton(
            window, "キャンセル", HideStorageUpgradeConfirmation);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot =
            new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 26f);
    }

    private void AdvanceDay() => dayManager.AdvanceDay();

    private void ShowStorageUpgradeConfirmation()
    {
        RefreshStorageUpgradeConfirmation();
        storageUpgrade.confirmationOverlay.SetAsLastSibling();
        storageUpgrade.confirmationOverlay.gameObject.SetActive(true);
    }

    private void HideStorageUpgradeConfirmation() =>
        storageUpgrade.confirmationOverlay?.gameObject.SetActive(false);

    private void ConfirmStorageUpgrade()
    {
        if (merchantStatusAndQuestController.TryUpgradeStorage())
        {
            HideStorageUpgradeConfirmation();
            return;
        }

        RefreshStorageUpgradeConfirmation();
    }

    private void RefreshStorageUpgradeConfirmation()
    {
        if (storageUpgrade.confirmationText == null ||
            storageUpgrade.confirmationReasonText == null ||
            storageUpgrade.confirmButton == null)
        {
            return;
        }

        if (progressionManager == null || merchantData == null)
        {
            storageUpgrade.confirmationText.text = "倉庫情報を取得できません。";
            storageUpgrade.confirmationReasonText.text = string.Empty;
            storageUpgrade.confirmButton.interactable = false;
            return;
        }

        if (progressionManager.IsStorageAtMaximumTier)
        {
            storageUpgrade.confirmationText.text =
                $"現在の容量: {progressionManager.StorageCapacity}枠\n倉庫は最大まで拡張済みです。";
            storageUpgrade.confirmationReasonText.text = "これ以上拡張できません。";
            storageUpgrade.confirmButton.interactable = false;
            return;
        }

        int cost = progressionManager.StorageUpgradeCost;
        int requiredLevel = progressionManager.NextStorageRequiredMerchantLevel;
        int missingGold = Mathf.Max(0, cost - merchantData.Gold);
        storageUpgrade.confirmationText.text =
            $"容量: {progressionManager.StorageCapacity}枠 → " +
            $"{progressionManager.NextStorageCapacity}枠\n" +
            $"必要金額: {cost:N0}G  |  所持金: {merchantData.Gold:N0}G\n" +
            $"必要商人レベル: Lv{requiredLevel}（現在 Lv{merchantData.MerchantLevel}）";
        if (merchantData.MerchantLevel < requiredLevel)
        {
            storageUpgrade.confirmationReasonText.text =
                $"商人レベルが不足しています。（あと {requiredLevel - merchantData.MerchantLevel}）";
        }
        else if (missingGold > 0)
        {
            storageUpgrade.confirmationReasonText.text =
                $"資金が不足しています。（あと {missingGold:N0}G）";
        }
        else
        {
            storageUpgrade.confirmationReasonText.text = "拡張できます。";
        }

        storageUpgrade.confirmButton.interactable =
            progressionManager.CanUpgradeStorage();
    }

    private void CreateInventorySidebar()
    {
        inventorySidebarButtons.Clear();
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "全て", 0, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.All));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "素材", 1, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.Material));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "消耗品", 2, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.Consumable));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "装備", 3, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.Equipment));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "売却用", 4, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.SellOnly)); SetSidebarSelection(inventorySidebarButtons, 0);
    }

    private void CreateMarketSidebar()
    {
        marketSidebarButtons.Clear();
        CreateSidebarButton(marketPage, marketSidebarButtons, "全て", 0, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.All));
        CreateSidebarButton(marketPage, marketSidebarButtons, "装備", 1, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.Equipment));
        CreateSidebarButton(marketPage, marketSidebarButtons, "消耗品", 2, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.Consumable));
        CreateSidebarButton(marketPage, marketSidebarButtons, "素材", 3, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.Material)); SetSidebarSelection(marketSidebarButtons, 0);
    }

    private void CreateBlacksmithSidebar()
    {
        blacksmithSidebarButtons.Clear();
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "全職種", 0, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.All));
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "戦士", 1, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Warrior));
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "弓使い", 2, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Archer));
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "魔術師", 3, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Mage));
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "僧侶", 4, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Priest));
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "盗賊", 5, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Rogue));
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "槍使い", 6, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Lancer)); SetSidebarSelection(blacksmithSidebarButtons, 0);
    }

    private void CreateSidebarButton(RectTransform page, List<Button> buttons, string label, int index, Action action)
    {
        Button button = CreateActionButton(page, label, () => { action(); SetSidebarSelection(buttons, index); });
        RectTransform rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f); rect.sizeDelta = new Vector2(126f, 36f); rect.anchoredPosition = new Vector2(0f, -94f - index * 42f); buttons.Add(button);
    }

    private static void SetSidebarSelection(List<Button> buttons, int selectedIndex)
    {
        for (int i = 0; i < buttons.Count; i++) { Image image = buttons[i].targetGraphic as Image; if (image != null) image.color = i == selectedIndex ? AccentColor : WoodButtonColor; }
    }

    private void ToggleBlacksmithCraftableOnly() { economyController.ToggleBlacksmithCraftableOnly(); RefreshBlacksmithFilterLabels(); }
    private void ToggleBlacksmithRankSort() { economyController.ToggleBlacksmithRankSort(); RefreshBlacksmithFilterLabels(); }
    private void RefreshBlacksmithFilterLabels()
    {
        foreach (Button button in blacksmithPage.GetComponentsInChildren<Button>())
        {
            Text label = button.GetComponentInChildren<Text>(); if (label == null) continue;
            if (label.text.StartsWith("製作可能のみ:")) label.text = "製作可能のみ: " + (economyController.IsBlacksmithCraftableOnly ? "ON" : "OFF");
            else if (label.text.StartsWith("ランク順:")) label.text = "ランク順: " + (economyController.IsBlacksmithRankAscending ? "昇順" : "降順");
        }
    }

    private string GetCurrentTownDemandSummary() => $"この町の需要:  素材{GetDemandMarker(townProgressState.CurrentTownIndex, ItemType.Material)}  装備{GetDemandMarker(townProgressState.CurrentTownIndex, ItemType.Equipment)}  消耗品{GetDemandMarker(townProgressState.CurrentTownIndex, ItemType.Consumable)}";
    private static string GetDemandMarker(int townIndex, ItemType itemType) { float multiplier = WorldMapService.GetTownDemandMultiplier(townIndex, itemType); return multiplier > 1.05f ? "▲" : multiplier < 0.95f ? "▼" : "─"; }
    private static RectTransform CreateViewport(string name, RectTransform page, Vector2 offsetMin, Vector2 offsetMax) { RectTransform viewport = CreateUIObject(name, page); viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = offsetMin; viewport.offsetMax = offsetMax; Image image = viewport.gameObject.AddComponent<Image>(); image.color = new Color(0f, 0f, 0f, 0.01f); Mask mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false; return viewport; }
    private static RectTransform CreateList(string name, RectTransform viewport) { RectTransform list = CreateUIObject(name, viewport); list.anchorMin = new Vector2(0f, 1f); list.anchorMax = new Vector2(1f, 1f); list.pivot = new Vector2(0.5f, 1f); list.anchoredPosition = Vector2.zero; return list; }
    private static void ConfigureScrollRect(RectTransform viewport, RectTransform content) { ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>(); scrollRect.content = content; scrollRect.viewport = viewport; scrollRect.horizontal = false; scrollRect.vertical = true; scrollRect.movementType = ScrollRect.MovementType.Clamped; scrollRect.scrollSensitivity = 28f; }

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
