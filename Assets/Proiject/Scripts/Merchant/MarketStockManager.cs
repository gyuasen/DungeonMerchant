using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketStockManager : MonoBehaviour
{
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private DayManager dayManager;
    [SerializeField, Min(1)] private int stockSlots = 4;
    [SerializeField, Range(0.4f, 1.5f)] private float minimumBuyMultiplier = 0.65f;
    [SerializeField, Range(0.4f, 1.8f)] private float maximumBuyMultiplier = 1.15f;
    [SerializeField] private List<ItemDataSO> purchasableItems = new List<ItemDataSO>();
    [SerializeField] private List<MarketStockEntry> stock = new List<MarketStockEntry>();
    [SerializeField, Range(0, WorldMapService.HiddenIslandTownIndex)]
    private int currentTownIndex = 2;

    public IReadOnlyList<MarketStockEntry> Stock => stock;
    public int CurrentDay => dayManager != null ? dayManager.CurrentDay : 1;

    public event Action StockChanged;

    public void SetTownIndex(int townIndex, bool regenerate = true)
    {
        currentTownIndex = Mathf.Clamp(
            townIndex,
            0,
            WorldMapService.HiddenIslandTownIndex);
        if (regenerate)
        {
            GenerateDailyStock();
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
        PopulatePurchasableItemsIfNeeded();
        GenerateDailyStock();

        if (dayManager != null)
        {
            dayManager.DayChanged += HandleDayChanged;
        }
        if (merchantData != null)
        {
            merchantData.ProgressionChanged += HandleMerchantProgressionChanged;
        }
    }

    private void OnDisable()
    {
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
        }
        if (merchantData != null)
        {
            merchantData.ProgressionChanged -= HandleMerchantProgressionChanged;
        }
    }

    public bool CanBuy(MarketStockEntry entry, int amount = 1)
    {
        ResolveReferences();
        return entry != null
            && entry.Item != null
            && amount > 0
            && entry.Quantity >= amount
            && merchantData != null
            && merchantData.CanPay(entry.BuyPrice * amount)
            && CanStorePurchase(entry, amount);
    }

    public bool CanStorePurchase(MarketStockEntry entry, int amount = 1)
    {
        ResolveReferences();
        return entry != null &&
               entry.Item != null &&
               amount > 0 &&
               (entry.Item.IsEquipment ||
                (merchantInventory != null &&
                 merchantInventory.CanAddItem(entry.Item, amount)));
    }

    public bool TryBuy(MarketStockEntry entry, int amount = 1)
    {
        ResolveReferences();

        if (!CanBuy(entry, amount) || merchantInventory == null)
        {
            return false;
        }

        if (!entry.Item.IsEquipment &&
            !merchantInventory.TryAddItem(entry.Item, amount))
        {
            return false;
        }

        int totalPrice = entry.BuyPrice * amount;
        if (!merchantData.TryPayGold(totalPrice))
        {
            RollbackAddedItems(entry.Item, amount);
            return false;
        }

        if (!entry.Remove(amount))
        {
            merchantData.AddGold(totalPrice);
            RollbackAddedItems(entry.Item, amount);
            return false;
        }

        if (entry.Item.IsEquipment)
        {
            for (int i = 0; i < amount; i++)
            {
                merchantInventory.AddEquipmentInstance(
                    EquipmentInstance.CreateFixed(entry.Item));
            }
        }
        StockChanged?.Invoke();
        return true;
    }

    private void RollbackAddedItems(ItemDataSO item, int amount)
    {
        if (item != null &&
            !item.IsEquipment &&
            amount > 0 &&
            !merchantInventory.TryRemoveItem(item, amount))
        {
            Debug.LogError("Failed to roll back a market purchase inventory addition.", this);
        }
    }

    public void GenerateDailyStock()
    {
        PopulatePurchasableItemsIfNeeded();
        stock.Clear();
        List<ItemDataSO> townItems =
            purchasableItems.FindAll(IsAvailableInCurrentTown);

        if (townItems.Count == 0)
        {
            ItemDataSO fallbackWeapon = CreateRuntimeFallbackItem();
            int fallbackPrice = Mathf.Max(
                1,
                Mathf.RoundToInt(fallbackWeapon.basePrice * minimumBuyMultiplier));
            stock.Add(new MarketStockEntry(fallbackWeapon, 1, fallbackPrice));
            StockChanged?.Invoke();
            return;
        }

        int slotCount = Mathf.Min(stockSlots, townItems.Count);
        for (int i = 0; i < slotCount; i++)
        {
            ItemDataSO item = townItems[GetStableIndex(i, townItems.Count)];
            int quantity = 1 + Mathf.Abs(GetStableHash(item, i, 23)) % 2;
            float merchantMultiplier = merchantData != null
                ? merchantData.GetMarketBuyMultiplier()
                : 1f;
            int buyPrice = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    item.basePrice *
                    GetBuyMultiplier(item, i) *
                    merchantMultiplier));
            stock.Add(new MarketStockEntry(item, quantity, buyPrice));
        }

        AddUndeadPurificationWardFixedStock();

        StockChanged?.Invoke();
    }

    public float GetBuyMultiplier(ItemDataSO item, int salt = 0)
    {
        if (item == null)
        {
            return 1f;
        }

        int hash = GetStableHash(item, salt, 71);
        float normalized = (hash & 0x7fffffff) / (float)int.MaxValue;
        return Mathf.Lerp(minimumBuyMultiplier, maximumBuyMultiplier, normalized);
    }

    private void AddUndeadPurificationWardFixedStock()
    {
        if (currentTownIndex != 0)
        {
            return;
        }

        ItemDataSO ward = purchasableItems.Find(item =>
            item != null && item.PersistentId == "item.expansion.undeadbane");
        if (ward == null)
        {
            return;
        }

        float merchantMultiplier = merchantData != null
            ? merchantData.GetMarketBuyMultiplier()
            : 1f;
        int buyPrice = Mathf.Max(1, Mathf.RoundToInt(
            ward.basePrice * GetBuyMultiplier(ward, 97) * merchantMultiplier));
        stock.Add(new MarketStockEntry(ward, 1, buyPrice));
    }

    private int GetStableIndex(int slot, int itemCount)
    {
        return MarketHashUtility.ComputeStableIndex(
            CurrentDay, slot, currentTownIndex, itemCount);
    }

    private int GetStableHash(ItemDataSO item, int salt, int seed)
    {
        return MarketHashUtility.ComputeItemHash(
            seed, CurrentDay, currentTownIndex, salt, item);
    }

    private void HandleDayChanged(int day)
    {
        GenerateDailyStock();
    }

    private void HandleMerchantProgressionChanged()
    {
        GenerateDailyStock();
    }

    private void PopulatePurchasableItemsIfNeeded()
    {
        RemoveInvalidItems();

        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            AddPurchasableItem(item);
        }
    }

    private void AddPurchasableItem(ItemDataSO item)
    {
        if (IsPurchasableItem(item) && !purchasableItems.Contains(item))
        {
            purchasableItems.Add(item);
        }
    }

    private void RemoveInvalidItems()
    {
        for (int i = purchasableItems.Count - 1; i >= 0; i--)
        {
            if (!IsPurchasableItem(purchasableItems[i]))
            {
                purchasableItems.RemoveAt(i);
            }
        }
    }

    private static bool IsPurchasableItem(ItemDataSO item)
    {
        return item != null &&
               (item.IsEquipment || item.itemType == ItemType.Consumable) &&
               item.acquisitionType == ItemAcquisitionType.Market;
    }

    private bool IsAvailableInCurrentTown(ItemDataSO item)
    {
        if (!IsPurchasableItem(item))
        {
            return false;
        }
        if (item.itemType == ItemType.Consumable)
        {
            return true;
        }

        MercenaryClass itemClass =
            MercenaryClassProgression.GetBaseClass(item.requiredClass);
        return WorldMapService.IsMarketEquipmentAllowedInTown(
            currentTownIndex,
            itemClass,
            item.equipmentRank,
            item.equipmentSlot);
    }

    private ItemDataSO CreateRuntimeFallbackItem()
    {
        WorldMapService.EquipmentRankRange rankRange =
            WorldMapService.GetMarketEquipmentRankRange(currentTownIndex);
        int rank = rankRange.Minimum;
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = $"Standard Equipment Rank {rank}";
        item.itemType = ItemType.Equipment;
        item.rarity = ItemRarity.Common;
        item.description = "Runtime fallback weapon.";
        item.basePrice = 80 + rank * 70;
        item.equipmentSlot = EquipmentSlot.Weapon;
        item.requiredClass = MercenaryClass.Warrior;
        item.allClassesCanEquip = true;
        item.equipmentRank = rank;
        item.bonusMaxHP = rank * 3;
        item.bonusAttack = rank * 2 + 2;
        item.bonusDefense = rank;
        return item;
    }

    private void ResolveReferences()
    {
        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>();
        }

        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = GetComponent<MerchantInventory>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = FindObjectOfType<MerchantInventory>();
        }

        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>();
        }

        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }
    }
}
