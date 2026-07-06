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
    [SerializeField, Range(0, 6)] private int currentTownIndex = 2;

    public IReadOnlyList<MarketStockEntry> Stock => stock;
    public int CurrentDay => dayManager != null ? dayManager.CurrentDay : 1;

    public event Action StockChanged;

    public void SetTownIndex(int townIndex, bool regenerate = true)
    {
        currentTownIndex = Mathf.Clamp(townIndex, 0, 6);
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
            && merchantData.CanPay(entry.BuyPrice * amount);
    }

    public bool TryBuy(MarketStockEntry entry, int amount = 1)
    {
        ResolveReferences();

        if (!CanBuy(entry, amount) || merchantInventory == null)
        {
            return false;
        }

        int totalPrice = entry.BuyPrice * amount;
        if (!merchantData.TryPayGold(totalPrice))
        {
            return false;
        }

        if (!entry.Remove(amount))
        {
            return false;
        }

        for (int i = 0; i < amount; i++)
        {
            if (entry.Item.IsEquipment)
            {
                merchantInventory.AddEquipmentInstance(
                    EquipmentInstance.CreateFixed(entry.Item));
            }
            else
            {
                merchantInventory.AddItem(entry.Item);
            }
        }
        StockChanged?.Invoke();
        return true;
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

    private int GetStableIndex(int slot, int itemCount)
    {
        unchecked
        {
            int hash = CurrentDay * 73856093;
            hash ^= slot * 19349663;
            hash ^= currentTownIndex * 83492791;
            return Mathf.Abs(hash) % Mathf.Max(1, itemCount);
        }
    }

    private int GetStableHash(ItemDataSO item, int salt, int seed)
    {
        unchecked
        {
            int hash = seed;
            hash = hash * 31 + CurrentDay;
            hash = hash * 31 + currentTownIndex;
            hash = hash * 31 + salt;
            hash = hash * 31 + (int)item.itemType;
            hash = hash * 31 + (int)item.rarity;

            string key = string.IsNullOrWhiteSpace(item.itemName) ? item.name : item.itemName;
            foreach (char character in key)
            {
                hash = hash * 31 + character;
            }

            return hash;
        }
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
        switch (currentTownIndex)
        {
            case 2:
                return item.equipmentRank <= 1;
            case 1:
                return item.equipmentRank <= 1 ||
                       itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Rogue;
            case 0:
                return true;
            case 3:
                return itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Priest ||
                       item.equipmentSlot == EquipmentSlot.Accessory;
            case 4:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Lancer ||
                       itemClass == MercenaryClass.Priest;
            case 5:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Lancer ||
                       item.equipmentRank >= 2;
            default:
                return item.equipmentRank >= 2 ||
                       item.equipmentSlot == EquipmentSlot.Accessory;
        }
    }

    private ItemDataSO CreateRuntimeFallbackItem()
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = "Iron Sword";
        item.itemType = ItemType.Equipment;
        item.rarity = ItemRarity.Common;
        item.description = "Runtime fallback weapon.";
        item.basePrice = 120;
        item.equipmentSlot = EquipmentSlot.Weapon;
        item.requiredClass = MercenaryClass.Warrior;
        item.equipmentRank = 1;
        item.bonusMaxHP = 5;
        item.bonusAttack = 4;
        item.bonusDefense = 1;
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
