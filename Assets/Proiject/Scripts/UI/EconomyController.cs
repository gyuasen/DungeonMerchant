using System;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Owns the market/blacksmith/inventory actions, the buy/craft-button
/// tracking state and the inventory filter/sort state. Extracted from
/// SimpleMercenaryHireUI (step 3.7). Page construction, routing and
/// delegate wiring stay in SimpleMercenaryHireUI.Economy.cs; only the
/// feature state and business actions live here.
/// </summary>
public sealed class EconomyController
{
    private readonly MerchantInventory merchantInventory;
    private readonly MarketStockManager marketStockManager;
    private readonly BlacksmithManager blacksmithManager;
    private readonly Action<string> setStatus;
    private readonly Action refreshInventoryPage;
    private readonly Action refreshMarketPage;
    private readonly Action refreshBlacksmithPage;
    private readonly Action refreshUI;
    private readonly Action<string> setFilterButtonLabel;
    private readonly Action<string> setSortButtonLabel;

    private readonly List<Button> marketBuyButtons = new List<Button>();
    private readonly List<MarketStockEntry> displayedMarketEntries =
        new List<MarketStockEntry>();
    private readonly List<Button> blacksmithCraftButtons = new List<Button>();
    private readonly List<EquipmentRecipeSO> displayedBlacksmithRecipes =
        new List<EquipmentRecipeSO>();

    private InventoryFilter inventoryFilter = InventoryFilter.All;
    private EquipmentSort equipmentSort = EquipmentSort.Name;
    private InventorySidebarCategory inventorySidebarCategory = InventorySidebarCategory.All;
    private MarketSidebarCategory marketSidebarCategory = MarketSidebarCategory.All;
    private BlacksmithSidebarCategory blacksmithSidebarCategory = BlacksmithSidebarCategory.All;
    private bool blacksmithCraftableOnly;
    private bool blacksmithRankAscending = true;

    public EconomyController(
        MerchantInventory merchantInventory,
        MarketStockManager marketStockManager,
        BlacksmithManager blacksmithManager,
        Action<string> setStatus,
        Action refreshInventoryPage,
        Action refreshMarketPage,
        Action refreshBlacksmithPage,
        Action refreshUI,
        Action<string> setFilterButtonLabel,
        Action<string> setSortButtonLabel)
    {
        this.merchantInventory = merchantInventory;
        this.marketStockManager = marketStockManager;
        this.blacksmithManager = blacksmithManager;
        this.setStatus = setStatus;
        this.refreshInventoryPage = refreshInventoryPage;
        this.refreshMarketPage = refreshMarketPage;
        this.refreshBlacksmithPage = refreshBlacksmithPage;
        this.refreshUI = refreshUI;
        this.setFilterButtonLabel = setFilterButtonLabel;
        this.setSortButtonLabel = setSortButtonLabel;
    }

    public IEnumerable<EquipmentInstance> GetSortedInventoryEquipment()
    {
        List<EquipmentInstance> sortedEquipment =
            new List<EquipmentInstance>(merchantInventory.EquipmentInstances);
        sortedEquipment.Sort(CompareEquipment);
        return sortedEquipment;
    }

    public bool ShouldShowInventoryItem(InventoryItemStack stack)
    {
        return stack != null &&
               stack.Item != null &&
               stack.Amount > 0 &&
               MatchesInventoryFilter(stack.Item) &&
               MatchesInventorySidebar(stack.Item) &&
               inventoryFilter != InventoryFilter.Locked;
    }

    public bool ShouldShowInventoryEquipment(EquipmentInstance equipment)
    {
        return equipment?.BaseItem != null &&
               MatchesInventoryFilter(equipment.BaseItem) &&
               MatchesInventorySidebar(equipment.BaseItem) &&
               (inventoryFilter != InventoryFilter.Locked ||
                equipment.IsLocked);
    }

    public IEnumerable<MarketStockEntry> GetMarketRows()
    {
        marketBuyButtons.Clear();
        displayedMarketEntries.Clear();
        return marketStockManager.Stock;
    }

    public static bool ShouldShowMarketEntry(MarketStockEntry entry)
    {
        return entry != null &&
               entry.Item != null &&
               entry.Quantity > 0;
    }

    public bool ShouldShowMarketEntryForSidebar(MarketStockEntry entry)
    {
        return ShouldShowMarketEntry(entry) && MatchesMarketSidebar(entry.Item);
    }

    public IEnumerable<EquipmentRecipeSO> GetBlacksmithRows()
    {
        blacksmithCraftButtons.Clear();
        displayedBlacksmithRecipes.Clear();
        return blacksmithManager.Recipes;
    }

    public static bool ShouldShowBlacksmithRecipe(EquipmentRecipeSO recipe)
    {
        return recipe != null && recipe.resultItem != null;
    }

    public bool ShouldShowBlacksmithRecipeForSidebar(EquipmentRecipeSO recipe)
    {
        return ShouldShowBlacksmithRecipe(recipe) &&
               MatchesBlacksmithSidebar(recipe) &&
               (!blacksmithCraftableOnly || blacksmithManager.CanCraft(recipe));
    }

    public IEnumerable<EquipmentRecipeSO> GetSortedBlacksmithRows()
    {
        List<EquipmentRecipeSO> recipes = new List<EquipmentRecipeSO>(
            GetBlacksmithRows());
        recipes.Sort((left, right) =>
        {
            int leftRank = left?.resultItem != null ? left.resultItem.equipmentRank : 0;
            int rightRank = right?.resultItem != null ? right.resultItem.equipmentRank : 0;
            int result = leftRank.CompareTo(rightRank);
            return blacksmithRankAscending ? result : -result;
        });
        return recipes;
    }

    public void RegisterMarketBuyButton(
        Button buyButton,
        MarketStockEntry entry)
    {
        marketBuyButtons.Add(buyButton);
        displayedMarketEntries.Add(entry);
    }

    public void RegisterBlacksmithCraftButton(
        Button craftButton,
        EquipmentRecipeSO recipe)
    {
        blacksmithCraftButtons.Add(craftButton);
        displayedBlacksmithRecipes.Add(recipe);
    }

    public void UpdateEconomyButtonInteractability()
    {
        for (int i = 0; i < marketBuyButtons.Count; i++)
        {
            marketBuyButtons[i].interactable =
                marketStockManager.CanBuy(displayedMarketEntries[i]);
        }

        for (int i = 0; i < blacksmithCraftButtons.Count; i++)
        {
            blacksmithCraftButtons[i].interactable =
                blacksmithManager.CanCraft(displayedBlacksmithRecipes[i]);
        }
    }

    public void SellItem(ItemDataSO item)
    {
        int sellPrice = merchantInventory.GetSellPrice(item);
        if (!merchantInventory.SellItem(item, 1))
        {
            setStatus($"{JapaneseDisplayText.GetItemName(item)}を売却できませんでした。");
            refreshUI();
            return;
        }

        setStatus($"{JapaneseDisplayText.GetItemName(item)}を{sellPrice} Gで売却しました。");
        refreshUI();
    }

    public List<InventoryItemStack> GetSellOnlyStacks()
    {
        List<InventoryItemStack> stacks = new List<InventoryItemStack>();
        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            if (stack?.Item != null && stack.Amount > 0 &&
                stack.Item.materialClassification == MaterialClassification.SellOnly)
            {
                stacks.Add(stack);
            }
        }
        return stacks;
    }

    public int GetSellOnlyTotalGold()
    {
        int total = 0;
        foreach (InventoryItemStack stack in GetSellOnlyStacks())
        {
            total += merchantInventory.GetSellPrice(stack.Item) * stack.Amount;
        }
        return total;
    }

    public int SellAllSellOnlyMaterials(out int soldCount, out bool stoppedEarly)
    {
        soldCount = 0;
        stoppedEarly = false;
        int earnedGold = 0;
        List<InventoryItemStack> stacks = GetSellOnlyStacks();
        foreach (InventoryItemStack stack in stacks)
        {
            int amount = merchantInventory.GetItemAmount(stack.Item);
            if (amount <= 0)
            {
                continue;
            }
            int price = merchantInventory.GetSellPrice(stack.Item) * amount;
            if (!merchantInventory.SellItem(stack.Item, amount))
            {
                stoppedEarly = true;
                setStatus("一括売却中に所持数が変化したため、残りの売却を中止しました。");
                break;
            }
            soldCount += amount;
            earnedGold += price;
        }
        refreshInventoryPage();
        refreshUI();
        return earnedGold;
    }

    public void SetInventorySidebarCategory(InventorySidebarCategory category)
    {
        inventorySidebarCategory = category;
        refreshInventoryPage();
    }

    public void SetMarketSidebarCategory(MarketSidebarCategory category)
    {
        marketSidebarCategory = category;
        refreshMarketPage();
    }

    public void SetBlacksmithSidebarCategory(BlacksmithSidebarCategory category)
    {
        blacksmithSidebarCategory = category;
        refreshBlacksmithPage();
    }

    public void ToggleBlacksmithCraftableOnly()
    {
        blacksmithCraftableOnly = !blacksmithCraftableOnly;
        refreshBlacksmithPage();
    }

    public void ToggleBlacksmithRankSort()
    {
        blacksmithRankAscending = !blacksmithRankAscending;
        refreshBlacksmithPage();
    }

    public bool IsBlacksmithCraftableOnly => blacksmithCraftableOnly;
    public bool IsBlacksmithRankAscending => blacksmithRankAscending;

    public void SellEquipment(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        int sellPrice = merchantInventory.GetSellPrice(equipment);
        string itemName = JapaneseDisplayText.GetItemName(equipment.BaseItem);
        if (!merchantInventory.SellEquipmentInstance(equipment))
        {
            setStatus($"{itemName}を売却できませんでした。");
            return;
        }

        setStatus($"{itemName}を{sellPrice} Gで売却しました。");
        refreshInventoryPage();
        refreshUI();
    }

    public void BuyMarketItem(MarketStockEntry entry)
    {
        if (entry == null || entry.Item == null)
        {
            return;
        }

        int buyPrice = entry.BuyPrice;
        if (!marketStockManager.TryBuy(entry, 1))
        {
            setStatus(!entry.Item.IsEquipment &&
                      !marketStockManager.CanStorePurchase(entry)
                ? "倉庫が満杯です。"
                : $"{JapaneseDisplayText.GetItemName(entry.Item)}を購入できませんでした。");
            refreshUI();
            return;
        }

        setStatus($"{JapaneseDisplayText.GetItemName(entry.Item)}を{buyPrice} Gで購入しました。");
        refreshMarketPage();
        refreshInventoryPage();
        refreshUI();
    }

    public void CraftEquipment(EquipmentRecipeSO recipe)
    {
        if (recipe == null || recipe.resultItem == null)
        {
            return;
        }

        string itemName = JapaneseDisplayText.GetItemName(recipe.resultItem);
        if (!blacksmithManager.TryCraft(recipe))
        {
            setStatus($"{itemName}を制作できませんでした。");
            refreshBlacksmithPage();
            refreshUI();
            return;
        }

        EquipmentInstance crafted = blacksmithManager.LastCraftedEquipment;
        setStatus(crafted != null
            ? $"[{JapaneseDisplayText.GetEquipmentQuality(crafted.Quality)}] " +
              $"{itemName}を制作しました。"
            : $"{itemName}を制作しました。");
        refreshBlacksmithPage();
        refreshInventoryPage();
        refreshUI();
    }

    public void CycleInventoryFilter()
    {
        inventoryFilter = (InventoryFilter)(
            ((int)inventoryFilter + 1) %
            System.Enum.GetValues(typeof(InventoryFilter)).Length);
        setFilterButtonLabel(
            $"絞込: {GetInventoryFilterLabel(inventoryFilter)}");
        refreshInventoryPage();
    }

    public void CycleEquipmentSort()
    {
        equipmentSort = (EquipmentSort)(
            ((int)equipmentSort + 1) %
            System.Enum.GetValues(typeof(EquipmentSort)).Length);
        setSortButtonLabel(
            $"並替: {GetEquipmentSortLabel(equipmentSort)}");
        refreshInventoryPage();
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

    private bool MatchesInventorySidebar(ItemDataSO item)
    {
        switch (inventorySidebarCategory)
        {
            case InventorySidebarCategory.Material:
                return item.itemType == ItemType.Material &&
                       item.materialClassification != MaterialClassification.SellOnly;
            case InventorySidebarCategory.Consumable:
                return item.itemType == ItemType.Consumable;
            case InventorySidebarCategory.Equipment:
                return item.IsEquipment;
            case InventorySidebarCategory.SellOnly:
                return item.materialClassification == MaterialClassification.SellOnly;
            default:
                return true;
        }
    }

    private bool MatchesMarketSidebar(ItemDataSO item)
    {
        switch (marketSidebarCategory)
        {
            case MarketSidebarCategory.Equipment: return item.IsEquipment;
            case MarketSidebarCategory.Consumable: return item.itemType == ItemType.Consumable;
            case MarketSidebarCategory.Material: return item.itemType == ItemType.Material;
            default: return true;
        }
    }

    private bool MatchesBlacksmithSidebar(EquipmentRecipeSO recipe)
    {
        if (blacksmithSidebarCategory == BlacksmithSidebarCategory.All)
        {
            return true;
        }
        return recipe.resultItem != null &&
               MercenaryClassProgression.GetBaseClass(recipe.resultItem.requiredClass) ==
               (MercenaryClass)((int)blacksmithSidebarCategory - 1);
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

public enum InventorySidebarCategory
{
    All,
    Material,
    Consumable,
    Equipment,
    SellOnly
}

public enum MarketSidebarCategory
{
    All,
    Equipment,
    Consumable,
    Material
}

public enum BlacksmithSidebarCategory
{
    All,
    Warrior,
    Archer,
    Mage,
    Priest,
    Rogue,
    Lancer
}
