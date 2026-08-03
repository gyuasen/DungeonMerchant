using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildInventoryPage()
    {
        Text title = CreateText(inventoryPage, "商人在庫", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        storageUpgrade.capacityText = CreateText(
            inventoryPage,
            string.Empty,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleRight,
            new Vector2(180f, -30f),
            new Vector2(-150f, 0f),
            ParchmentTextColor);

        marketInfoText = CreateText(inventoryPage, string.Empty, 16, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(-160f, -38f),
            ParchmentTextColor);

        nextDayButton = CreateActionButton(inventoryPage, "翌日へ", AdvanceDay);
        RectTransform nextDayRect = nextDayButton.GetComponent<RectTransform>();
        nextDayRect.anchorMin = new Vector2(1f, 1f);
        nextDayRect.anchorMax = new Vector2(1f, 1f);
        nextDayRect.pivot = new Vector2(1f, 1f);
        nextDayRect.anchoredPosition = new Vector2(0f, -34f);

        // 上部ボタンは2段に整列する。
        // 1段目(y=-78): 操作系(絞込 / 並替)。2段目(y=-120): 機能系
        // (装備図鑑 / 倉庫拡張 / 売却用素材を一括売却)。いずれも左アンカーで
        // 等間隔に並べ、右上の「翌日へ」やビューポートと重ならないようにする。
        const float topRowY = -78f;
        const float bottomRowY = -120f;

        inventoryFilterButton =
            CreateActionButton(inventoryPage, "絞込: 全て",
                economyController.CycleInventoryFilter);
        inventoryFilterButton.name = "Inventory Filter Button";
        RectTransform filterRect = inventoryFilterButton.GetComponent<RectTransform>();
        filterRect.anchorMin = filterRect.anchorMax = new Vector2(0f, 1f);
        filterRect.pivot = new Vector2(0f, 1f);
        filterRect.sizeDelta = new Vector2(150f, 38f);
        filterRect.anchoredPosition = new Vector2(142f, topRowY);

        equipmentSortButton =
            CreateActionButton(inventoryPage, "並替: 名前",
                economyController.CycleEquipmentSort);
        equipmentSortButton.name = "Equipment Sort Button";
        RectTransform sortRect = equipmentSortButton.GetComponent<RectTransform>();
        sortRect.anchorMin = sortRect.anchorMax = new Vector2(0f, 1f);
        sortRect.pivot = new Vector2(0f, 1f);
        sortRect.sizeDelta = new Vector2(150f, 38f);
        sortRect.anchoredPosition = new Vector2(308f, topRowY);

        Button collectionButton =
            CreateActionButton(inventoryPage, "装備図鑑", ShowEquipmentCollection);
        RectTransform collectionRect = collectionButton.GetComponent<RectTransform>();
        collectionRect.anchorMin = collectionRect.anchorMax = new Vector2(0f, 1f);
        collectionRect.pivot = new Vector2(0f, 1f);
        collectionRect.sizeDelta = new Vector2(150f, 38f);
        collectionRect.anchoredPosition = new Vector2(142f, bottomRowY);

        Button storageButton =
            CreateActionButton(
                inventoryPage,
                "倉庫拡張",
                ShowStorageUpgradeConfirmation);
        RectTransform storageRect = storageButton.GetComponent<RectTransform>();
        storageRect.anchorMin = storageRect.anchorMax = new Vector2(0f, 1f);
        storageRect.pivot = new Vector2(0f, 1f);
        storageRect.sizeDelta = new Vector2(150f, 38f);
        storageRect.anchoredPosition = new Vector2(308f, bottomRowY);

        Button sellOnlyButton = CreateActionButton(
            inventoryPage,
            "売却用素材を一括売却",
            ShowSellOnlyConfirmation);
        RectTransform sellOnlyRect = sellOnlyButton.GetComponent<RectTransform>();
        sellOnlyRect.anchorMin = sellOnlyRect.anchorMax = new Vector2(0f, 1f);
        sellOnlyRect.pivot = new Vector2(0f, 1f);
        sellOnlyRect.sizeDelta = new Vector2(210f, 38f);
        sellOnlyRect.anchoredPosition = new Vector2(474f, bottomRowY);

        CreateInventorySidebar();

        RectTransform viewport = CreateUIObject("Inventory Viewport", inventoryPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = new Vector2(142f, 0f);
        viewport.offsetMax = new Vector2(0f, -166f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        inventoryList = CreateUIObject("Inventory List", viewport);
        inventoryList.anchorMin = new Vector2(0f, 1f);
        inventoryList.anchorMax = new Vector2(1f, 1f);
        inventoryList.pivot = new Vector2(0.5f, 1f);
        inventoryList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = inventoryList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        InventoryPageUI pageUI =
            inventoryPage.GetComponent<InventoryPageUI>() ??
            inventoryPage.gameObject.AddComponent<InventoryPageUI>();
        pageUI.Initialize(title, null, inventoryList);
        pageUI.ConfigureInventory(
            uiBodyFont,
            ParchmentMutedColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            () => merchantInventory.Items,
            economyController.GetSortedInventoryEquipment,
            economyController.ShouldShowInventoryItem,
            economyController.ShouldShowInventoryEquipment,
            item => merchantInventory.GetSellPrice(item),
            item => marketPriceManager.GetEffectiveSellMultiplier(item),
            item => townProgressState != null
                ? WorldMapService.GetTownDemandMultiplier(
                    townProgressState.CurrentTownIndex, item)
                : 1f,
            CharacterEquipmentController.GetEquipmentDisplayName,
            CharacterEquipmentController.GetEquipmentQualityColor,
            ShowSellQuantityOverlay,
            characterEquipmentController.UseConsumable,
            characterEquipmentController.ShowEquipmentDetails);
        pageRouter.Register(inventoryPage);
        UpdateStorageCapacityText();
    }

    private void BuildMarketPage()
    {
        Text title = CreateText(marketPage, "本日の仕入れ商品", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        Text demandSummary = CreateText(
            marketPage,
            string.Empty,
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -70f),
            new Vector2(0f, -38f),
            ParchmentMutedColor);

        CreateMarketSidebar();

        RectTransform viewport = CreateUIObject("Market Viewport", marketPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = new Vector2(142f, 0f);
        viewport.offsetMax = new Vector2(0f, -84f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        marketList = CreateUIObject("Market List", viewport);
        marketList.anchorMin = new Vector2(0f, 1f);
        marketList.anchorMax = new Vector2(1f, 1f);
        marketList.pivot = new Vector2(0.5f, 1f);
        marketList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = marketList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        MarketPageUI pageUI =
            marketPage.GetComponent<MarketPageUI>() ??
            marketPage.gameObject.AddComponent<MarketPageUI>();
        pageUI.Initialize(title, demandSummary, marketList);
        pageUI.ConfigureMarket(
            uiBodyFont,
            ParchmentMutedColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            economyController.GetMarketRows,
            economyController.ShouldShowMarketEntryForSidebar,
            entry => marketStockManager.CanBuy(entry),
            economyController.BuyMarketItem,
            economyController.RegisterMarketBuyButton,
            demandSummary,
            GetCurrentTownDemandSummary,
            ShowMarketItemDetail);
        pageRouter.Register(marketPage);
    }

    private void BuildBlacksmithPage()
    {
        Text title = CreateText(blacksmithPage, "鍛冶屋", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        Text description = CreateText(
            blacksmithPage,
            "モンスター素材とゴールドを使い、市場では買えない武器を制作します。",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -70f),
            new Vector2(0f, -38f),
            ParchmentMutedColor);

        CreateBlacksmithSidebar();

        Button craftableButton = CreateActionButton(
            blacksmithPage,
            "製作可能のみ: OFF",
            ToggleBlacksmithCraftableOnly);
        RectTransform craftableRect = craftableButton.GetComponent<RectTransform>();
        craftableRect.anchorMin = craftableRect.anchorMax = new Vector2(1f, 1f);
        craftableRect.pivot = new Vector2(1f, 1f);
        craftableRect.sizeDelta = new Vector2(150f, 32f);
        craftableRect.anchoredPosition = new Vector2(0f, -34f);

        Button rankSortButton = CreateActionButton(
            blacksmithPage,
            "ランク順: 昇順",
            ToggleBlacksmithRankSort);
        RectTransform rankSortRect = rankSortButton.GetComponent<RectTransform>();
        rankSortRect.anchorMin = rankSortRect.anchorMax = new Vector2(1f, 1f);
        rankSortRect.pivot = new Vector2(1f, 1f);
        rankSortRect.sizeDelta = new Vector2(150f, 32f);
        rankSortRect.anchoredPosition = new Vector2(-160f, -34f);

        RectTransform viewport = CreateUIObject("Blacksmith Viewport", blacksmithPage);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(142f, 0f);
        viewport.offsetMax = new Vector2(0f, -84f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        blacksmithList = CreateUIObject("Blacksmith List", viewport);
        blacksmithList.anchorMin = new Vector2(0f, 1f);
        blacksmithList.anchorMax = new Vector2(1f, 1f);
        blacksmithList.pivot = new Vector2(0.5f, 1f);
        blacksmithList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = blacksmithList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        BlacksmithPageUI pageUI =
            blacksmithPage.GetComponent<BlacksmithPageUI>() ??
            blacksmithPage.gameObject.AddComponent<BlacksmithPageUI>();
        pageUI.Initialize(title, description, blacksmithList);
        pageUI.ConfigureBlacksmith(
            uiBodyFont,
            ParchmentMutedColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            economyController.GetSortedBlacksmithRows,
            economyController.ShouldShowBlacksmithRecipeForSidebar,
            item => merchantInventory.GetItemAmount(item),
            recipe => blacksmithManager.CanCraft(recipe),
            economyController.CraftEquipment,
            economyController.RegisterBlacksmithCraftButton,
            ShowBlacksmithRecipeDetail);
        pageRouter.Register(blacksmithPage);
    }

    private void HandleInventoryChanged()
    {
        dailyResultController.RecordDailyInventoryGains();
        TryUnlockHiddenIsland();
        RefreshPage(inventoryPage);
        RefreshPage(blacksmithPage);
        RefreshUI();
    }

    private void HandleMarketStockChanged()
    {
        RefreshPage(marketPage);
        RefreshUI();
    }

    private void HandleCraftingChanged()
    {
        RefreshPage(inventoryPage);
        RefreshPage(blacksmithPage);
        RefreshUI();
    }

    private void HandlePricesChanged()
    {
        RefreshPage(inventoryPage);
        RefreshUI();
    }

    private void AdvanceDay()
    {
        dayManager.AdvanceDay();
    }

    private void ShowMarketPage()
    {
        SwitchToPage(marketPage, marketTabButton);
        statusText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}市場  |  " +
            $"仕入れ商品: {marketStockManager.Stock.Count}種類 / " +
            marketPriceManager.GetMarketSummary();
    }

    private void ShowBlacksmithPage()
    {
        SwitchToPage(blacksmithPage, blacksmithTabButton);
        statusText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}鍛冶屋  |  " +
            $"レシピ: {blacksmithManager.Recipes.Count}種類";
        RefreshUI();
    }

    private void ShowInventoryPage()
    {
        UpdateStorageCapacityText();
        SwitchToPage(inventoryPage, inventoryTabButton);
        statusText.text =
            $"倉庫 {merchantInventory.GetUsedStorageSlots()}/" +
            $"{(progressionManager != null ? progressionManager.StorageCapacity : 0)}  |  " +
            $"{marketPriceManager.GetMarketSummary()}  |  " +
            $"維持費 {(progressionManager != null ? progressionManager.StorageMaintenanceCost : 0)}G/日";
    }

    private void UpdateStorageCapacityText()
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

    private void BuildStorageUpgradeConfirmationOverlay()
    {
        storageUpgrade.confirmationOverlay = CreateUIObject(
            "Storage Upgrade Confirmation Overlay",
            overlayRoot);
        storageUpgrade.confirmationOverlay.gameObject.SetActive(false);
        storageUpgrade.confirmationOverlay.anchorMin = Vector2.zero;
        storageUpgrade.confirmationOverlay.anchorMax = Vector2.one;
        storageUpgrade.confirmationOverlay.offsetMin = Vector2.zero;
        storageUpgrade.confirmationOverlay.offsetMax = Vector2.zero;
        storageUpgrade.confirmationOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject(
            "Storage Upgrade Confirmation Window",
            storageUpgrade.confirmationOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(560f, 340f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "倉庫を拡張しますか？",
            26,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(28f, -72f),
            new Vector2(-28f, -22f),
            ParchmentTextColor);

        storageUpgrade.confirmationText = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -190f),
            new Vector2(-36f, -82f),
            ParchmentTextColor);
        storageUpgrade.confirmationReasonText = CreateText(
            window,
            string.Empty,
            15,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -238f),
            new Vector2(-36f, -190f),
            MutedTextColor);

        storageUpgrade.confirmButton = CreateActionButton(
            window,
            "拡張する",
            ConfirmStorageUpgrade);
        RectTransform confirmRect =
            storageUpgrade.confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot =
            new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 26f);
        storageUpgrade.confirmButton.targetGraphic.color = AccentColor;

        Button cancelButton = CreateActionButton(
            window,
            "キャンセル",
            HideStorageUpgradeConfirmation);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot =
            new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 26f);

    }

    private void CreateInventorySidebar()
    {
        inventorySidebarButtons.Clear();
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "全て", 0, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.All));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "素材", 1, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.Material));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "消耗品", 2, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.Consumable));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "装備", 3, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.Equipment));
        CreateSidebarButton(inventoryPage, inventorySidebarButtons, "売却用", 4, () => economyController.SetInventorySidebarCategory(InventorySidebarCategory.SellOnly));
        SetSidebarSelection(inventorySidebarButtons, 0);
    }

    private void CreateMarketSidebar()
    {
        marketSidebarButtons.Clear();
        CreateSidebarButton(marketPage, marketSidebarButtons, "全て", 0, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.All));
        CreateSidebarButton(marketPage, marketSidebarButtons, "装備", 1, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.Equipment));
        CreateSidebarButton(marketPage, marketSidebarButtons, "消耗品", 2, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.Consumable));
        CreateSidebarButton(marketPage, marketSidebarButtons, "素材", 3, () => economyController.SetMarketSidebarCategory(MarketSidebarCategory.Material));
        SetSidebarSelection(marketSidebarButtons, 0);
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
        CreateSidebarButton(blacksmithPage, blacksmithSidebarButtons, "槍使い", 6, () => economyController.SetBlacksmithSidebarCategory(BlacksmithSidebarCategory.Lancer));
        SetSidebarSelection(blacksmithSidebarButtons, 0);
    }

    private void CreateSidebarButton(RectTransform page, List<Button> buttons, string label, int index, System.Action action)
    {
        Button button = CreateActionButton(page, label, () =>
        {
            action();
            SetSidebarSelection(buttons, index);
        });
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(126f, 36f);
        rect.anchoredPosition = new Vector2(0f, -94f - index * 42f);
        buttons.Add(button);
    }

    private void SetSidebarSelection(List<Button> buttons, int selectedIndex)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Image image = buttons[i].targetGraphic as Image;
            if (image != null)
            {
                image.color = i == selectedIndex ? AccentColor : WoodButtonColor;
            }
        }
    }

    private void ToggleBlacksmithCraftableOnly()
    {
        economyController.ToggleBlacksmithCraftableOnly();
        RefreshBlacksmithFilterLabels();
    }

    private void ToggleBlacksmithRankSort()
    {
        economyController.ToggleBlacksmithRankSort();
        RefreshBlacksmithFilterLabels();
    }

    private void RefreshBlacksmithFilterLabels()
    {
        foreach (Button button in blacksmithPage.GetComponentsInChildren<Button>())
        {
            Text label = button.GetComponentInChildren<Text>();
            if (label == null)
            {
                continue;
            }
            if (label.text.StartsWith("製作可能のみ:"))
            {
                label.text = "製作可能のみ: " + (economyController.IsBlacksmithCraftableOnly ? "ON" : "OFF");
            }
            else if (label.text.StartsWith("ランク順:"))
            {
                label.text = "ランク順: " + (economyController.IsBlacksmithRankAscending ? "昇順" : "降順");
            }
        }
    }

    private void ShowStorageUpgradeConfirmation()
    {
        RefreshStorageUpgradeConfirmation();
        storageUpgrade.confirmationOverlay.SetAsLastSibling();
        storageUpgrade.confirmationOverlay.gameObject.SetActive(true);
    }

    private void HideStorageUpgradeConfirmation()
    {
        storageUpgrade.confirmationOverlay?.gameObject.SetActive(false);
    }

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
            storageUpgrade.confirmationReasonText.text =
                "これ以上拡張できません。";
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

    private string GetCurrentTownDemandSummary()
    {
        int townIndex = townProgressState != null
            ? townProgressState.CurrentTownIndex
            : 2;
        return $"この町の需要:  素材{GetDemandMarker(townIndex, ItemType.Material)}  " +
               $"装備{GetDemandMarker(townIndex, ItemType.Equipment)}  " +
               $"消耗品{GetDemandMarker(townIndex, ItemType.Consumable)}";
    }

    private static string GetDemandMarker(int townIndex, ItemType itemType)
    {
        float multiplier =
            WorldMapService.GetTownDemandMultiplier(townIndex, itemType);
        return multiplier > 1.05f ? "▲" :
            multiplier < 0.95f ? "▼" : "─";
    }

}
