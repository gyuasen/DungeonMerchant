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

        marketInfoText = CreateText(inventoryPage, string.Empty, 16, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(0f, -70f), new Vector2(-160f, -38f),
            ParchmentTextColor);

        nextDayButton = CreateActionButton(inventoryPage, "翌日へ", AdvanceDay);
        RectTransform nextDayRect = nextDayButton.GetComponent<RectTransform>();
        nextDayRect.anchorMin = new Vector2(1f, 1f);
        nextDayRect.anchorMax = new Vector2(1f, 1f);
        nextDayRect.pivot = new Vector2(1f, 1f);
        nextDayRect.anchoredPosition = new Vector2(0f, -34f);

        inventoryFilterButton =
            CreateActionButton(inventoryPage, "絞込: 全て", CycleInventoryFilter);
        inventoryFilterButton.name = "Inventory Filter Button";
        RectTransform filterRect = inventoryFilterButton.GetComponent<RectTransform>();
        filterRect.anchorMin = filterRect.anchorMax = new Vector2(0f, 1f);
        filterRect.pivot = new Vector2(0f, 1f);
        filterRect.sizeDelta = new Vector2(150f, 38f);
        filterRect.anchoredPosition = new Vector2(0f, -78f);

        equipmentSortButton =
            CreateActionButton(inventoryPage, "並替: 名前", CycleEquipmentSort);
        equipmentSortButton.name = "Equipment Sort Button";
        RectTransform sortRect = equipmentSortButton.GetComponent<RectTransform>();
        sortRect.anchorMin = sortRect.anchorMax = new Vector2(0f, 1f);
        sortRect.pivot = new Vector2(0f, 1f);
        sortRect.sizeDelta = new Vector2(150f, 38f);
        sortRect.anchoredPosition = new Vector2(166f, -78f);

        Button collectionButton =
            CreateActionButton(inventoryPage, "装備図鑑", ShowEquipmentCollection);
        RectTransform collectionRect = collectionButton.GetComponent<RectTransform>();
        collectionRect.anchorMin = collectionRect.anchorMax = new Vector2(0f, 1f);
        collectionRect.pivot = new Vector2(0f, 1f);
        collectionRect.sizeDelta = new Vector2(130f, 38f);
        collectionRect.anchoredPosition = new Vector2(332f, -78f);

        Button storageButton =
            CreateActionButton(inventoryPage, "倉庫拡張", UpgradeStorage);
        RectTransform storageRect = storageButton.GetComponent<RectTransform>();
        storageRect.anchorMin = storageRect.anchorMax = new Vector2(0f, 1f);
        storageRect.pivot = new Vector2(0f, 1f);
        storageRect.sizeDelta = new Vector2(130f, 38f);
        storageRect.anchoredPosition = new Vector2(478f, -78f);

        RectTransform viewport = CreateUIObject("Inventory Viewport", inventoryPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -126f);

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
            GetSortedInventoryEquipment,
            ShouldShowInventoryItem,
            ShouldShowInventoryEquipment,
            item => merchantInventory.GetSellPrice(item),
            item => marketPriceManager.GetEffectiveSellMultiplier(item),
            GetEquipmentDisplayName,
            GetEquipmentQualityColor,
            SellItem,
            UseConsumable,
            ShowEquipmentDetails);
        pageRouter.Register(inventoryPage);
    }

    private void BuildMarketPage()
    {
        Text title = CreateText(marketPage, "本日の仕入れ商品", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        RectTransform viewport = CreateUIObject("Market Viewport", marketPage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -52f);

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
        pageUI.Initialize(title, null, marketList);
        pageUI.ConfigureMarket(
            uiBodyFont,
            ParchmentMutedColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            GetMarketRows,
            ShouldShowMarketEntry,
            entry => marketStockManager.CanBuy(entry),
            BuyMarketItem,
            RegisterMarketBuyButton);
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

        RectTransform viewport = CreateUIObject("Blacksmith Viewport", blacksmithPage);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
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
            GetBlacksmithRows,
            ShouldShowBlacksmithRecipe,
            item => merchantInventory.GetItemAmount(item),
            recipe => blacksmithManager.CanCraft(recipe),
            CraftEquipment,
            RegisterBlacksmithCraftButton);
        pageRouter.Register(blacksmithPage);
    }

    private IEnumerable<EquipmentInstance> GetSortedInventoryEquipment()
    {
        List<EquipmentInstance> sortedEquipment =
            new List<EquipmentInstance>(merchantInventory.EquipmentInstances);
        sortedEquipment.Sort(CompareEquipment);
        return sortedEquipment;
    }

    private bool ShouldShowInventoryItem(InventoryItemStack stack)
    {
        return stack != null &&
               stack.Item != null &&
               stack.Amount > 0 &&
               MatchesInventoryFilter(stack.Item) &&
               inventoryFilter != InventoryFilter.Locked;
    }

    private bool ShouldShowInventoryEquipment(EquipmentInstance equipment)
    {
        return equipment?.BaseItem != null &&
               MatchesInventoryFilter(equipment.BaseItem) &&
               (inventoryFilter != InventoryFilter.Locked ||
                equipment.IsLocked);
    }

    private IEnumerable<MarketStockEntry> GetMarketRows()
    {
        marketBuyButtons.Clear();
        displayedMarketEntries.Clear();
        return marketStockManager.Stock;
    }

    private static bool ShouldShowMarketEntry(MarketStockEntry entry)
    {
        return entry != null &&
               entry.Item != null &&
               entry.Quantity > 0;
    }

    private IEnumerable<EquipmentRecipeSO> GetBlacksmithRows()
    {
        blacksmithCraftButtons.Clear();
        displayedBlacksmithRecipes.Clear();
        return blacksmithManager.Recipes;
    }

    private static bool ShouldShowBlacksmithRecipe(EquipmentRecipeSO recipe)
    {
        return recipe != null && recipe.resultItem != null;
    }

    private void RegisterMarketBuyButton(
        Button buyButton,
        MarketStockEntry entry)
    {
        marketBuyButtons.Add(buyButton);
        displayedMarketEntries.Add(entry);
    }

    private void RegisterBlacksmithCraftButton(
        Button craftButton,
        EquipmentRecipeSO recipe)
    {
        blacksmithCraftButtons.Add(craftButton);
        displayedBlacksmithRecipes.Add(recipe);
    }

    private void HandleInventoryChanged()
    {
        RecordDailyInventoryGains();
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

    private void SellItem(ItemDataSO item)
    {
        int sellPrice = merchantInventory.GetSellPrice(item);
        if (!merchantInventory.SellItem(item, 1))
        {
            statusText.text = $"{JapaneseDisplayText.GetItemName(item)}を売却できませんでした。";
            RefreshUI();
            return;
        }

        statusText.text = $"{JapaneseDisplayText.GetItemName(item)}を{sellPrice} Gで売却しました。";
        RefreshUI();
    }

    private void SellEquipment(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        int sellPrice = merchantInventory.GetSellPrice(equipment);
        string itemName = JapaneseDisplayText.GetItemName(equipment.BaseItem);
        if (!merchantInventory.SellEquipmentInstance(equipment))
        {
            statusText.text = $"{itemName}を売却できませんでした。";
            return;
        }

        statusText.text = $"{itemName}を{sellPrice} Gで売却しました。";
        RefreshPage(inventoryPage);
        RefreshUI();
    }

    private void BuyMarketItem(MarketStockEntry entry)
    {
        if (entry == null || entry.Item == null)
        {
            return;
        }

        int buyPrice = entry.BuyPrice;
        if (!marketStockManager.TryBuy(entry, 1))
        {
            statusText.text = $"{JapaneseDisplayText.GetItemName(entry.Item)}を購入できませんでした。";
            RefreshUI();
            return;
        }

        statusText.text = $"{JapaneseDisplayText.GetItemName(entry.Item)}を{buyPrice} Gで購入しました。";
        RefreshPage(marketPage);
        RefreshPage(inventoryPage);
        RefreshUI();
    }

    private void CraftEquipment(EquipmentRecipeSO recipe)
    {
        if (recipe == null || recipe.resultItem == null)
        {
            return;
        }

        string itemName = JapaneseDisplayText.GetItemName(recipe.resultItem);
        if (!blacksmithManager.TryCraft(recipe))
        {
            statusText.text = $"{itemName}を制作できませんでした。";
            RefreshPage(blacksmithPage);
            RefreshUI();
            return;
        }

        EquipmentInstance crafted = blacksmithManager.LastCraftedEquipment;
        statusText.text = crafted != null
            ? $"[{JapaneseDisplayText.GetEquipmentQuality(crafted.Quality)}] " +
              $"{itemName}を制作しました。"
            : $"{itemName}を制作しました。";
        RefreshPage(blacksmithPage);
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
        SwitchToPage(inventoryPage, inventoryTabButton);
        statusText.text =
            $"倉庫 {merchantInventory.GetUsedStorageSlots()}/" +
            $"{(progressionManager != null ? progressionManager.StorageCapacity : 0)}  |  " +
            $"{marketPriceManager.GetMarketSummary()}  |  " +
            $"維持費 {(progressionManager != null ? progressionManager.StorageMaintenanceCost : 0)}G/日";
    }

    private void CycleInventoryFilter()
    {
        inventoryFilter = (InventoryFilter)(
            ((int)inventoryFilter + 1) %
            System.Enum.GetValues(typeof(InventoryFilter)).Length);
        inventoryFilterButton.GetComponentInChildren<Text>().text =
            $"絞込: {GetInventoryFilterLabel(inventoryFilter)}";
        RefreshPage(inventoryPage);
    }

    private void CycleEquipmentSort()
    {
        equipmentSort = (EquipmentSort)(
            ((int)equipmentSort + 1) %
            System.Enum.GetValues(typeof(EquipmentSort)).Length);
        equipmentSortButton.GetComponentInChildren<Text>().text =
            $"並替: {GetEquipmentSortLabel(equipmentSort)}";
        RefreshPage(inventoryPage);
    }

    private bool MatchesInventoryFilter(ItemDataSO item)
    {
        if (item == null)
        {
            return false;
        }

        switch (inventoryFilter)
        {
            case InventoryFilter.Material:
                return !item.IsEquipment;
            case InventoryFilter.Weapon:
                return item.IsEquipment &&
                       item.equipmentSlot == EquipmentSlot.Weapon;
            case InventoryFilter.Armor:
                return item.IsEquipment &&
                       item.equipmentSlot == EquipmentSlot.Armor;
            case InventoryFilter.Accessory:
                return item.IsEquipment &&
                       item.equipmentSlot == EquipmentSlot.Accessory;
            case InventoryFilter.SetEquipment:
                return item.IsEquipment &&
                       item.equipmentSet != EquipmentSetId.None;
            default:
                return true;
        }
    }

    private int CompareEquipment(
        EquipmentInstance left,
        EquipmentInstance right)
    {
        if (left?.BaseItem == null) return 1;
        if (right?.BaseItem == null) return -1;

        switch (equipmentSort)
        {
            case EquipmentSort.Quality:
                return right.Quality.CompareTo(left.Quality);
            case EquipmentSort.Enhancement:
                return right.EnhancementLevel.CompareTo(left.EnhancementLevel);
            case EquipmentSort.Set:
                return left.BaseItem.equipmentSet.CompareTo(
                    right.BaseItem.equipmentSet);
            default:
                return string.Compare(
                    JapaneseDisplayText.GetItemName(left.BaseItem),
                    JapaneseDisplayText.GetItemName(right.BaseItem),
                    System.StringComparison.Ordinal);
        }
    }

    private static string GetInventoryFilterLabel(InventoryFilter filter)
    {
        switch (filter)
        {
            case InventoryFilter.Material: return "素材";
            case InventoryFilter.Weapon: return "武器";
            case InventoryFilter.Armor: return "防具";
            case InventoryFilter.Accessory: return "装飾品";
            case InventoryFilter.SetEquipment: return "セット";
            case InventoryFilter.Locked: return "ロック";
            default: return "全て";
        }
    }

    private static string GetEquipmentSortLabel(EquipmentSort sort)
    {
        switch (sort)
        {
            case EquipmentSort.Quality: return "品質";
            case EquipmentSort.Enhancement: return "強化";
            case EquipmentSort.Set: return "セット";
            default: return "名前";
        }
    }

}
