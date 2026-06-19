using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    public IReadOnlyList<MarketStockEntry> Stock => stock;
    public int CurrentDay => dayManager != null ? dayManager.CurrentDay : 1;

    public event Action StockChanged;

    private void OnEnable()
    {
        ResolveReferences();
        PopulatePurchasableItemsIfNeeded();
        GenerateDailyStock();

        if (dayManager != null)
        {
            dayManager.DayChanged += HandleDayChanged;
        }
    }

    private void OnDisable()
    {
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
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
            merchantData.AddGold(totalPrice);
            return false;
        }

        merchantInventory.AddItem(entry.Item, amount);
        StockChanged?.Invoke();
        return true;
    }

    public void GenerateDailyStock()
    {
        PopulatePurchasableItemsIfNeeded();
        stock.Clear();

        if (purchasableItems.Count == 0)
        {
            ItemDataSO fallbackWeapon = CreateRuntimeFallbackItem();
            int fallbackPrice = Mathf.Max(
                1,
                Mathf.RoundToInt(fallbackWeapon.basePrice * minimumBuyMultiplier));
            stock.Add(new MarketStockEntry(fallbackWeapon, 1, fallbackPrice));
            StockChanged?.Invoke();
            return;
        }

        int slotCount = Mathf.Min(stockSlots, purchasableItems.Count);
        for (int i = 0; i < slotCount; i++)
        {
            ItemDataSO item = purchasableItems[GetStableIndex(i)];
            int quantity = 1 + Mathf.Abs(GetStableHash(item, i, 23)) % 2;
            int buyPrice = Mathf.Max(1, Mathf.RoundToInt(item.basePrice * GetBuyMultiplier(item, i)));
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

    private int GetStableIndex(int slot)
    {
        unchecked
        {
            int hash = CurrentDay * 73856093;
            hash ^= slot * 19349663;
            return Mathf.Abs(hash) % purchasableItems.Count;
        }
    }

    private int GetStableHash(ItemDataSO item, int salt, int seed)
    {
        unchecked
        {
            int hash = seed;
            hash = hash * 31 + CurrentDay;
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

    private void PopulatePurchasableItemsIfNeeded()
    {
        RemoveInvalidItems();
        if (purchasableItems.Count > 0)
        {
            return;
        }

        foreach (ItemDataSO item in Resources.LoadAll<ItemDataSO>(string.Empty))
        {
            AddPurchasableItem(item);
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:ItemDataSO",
            new[] { "Assets/Proiject/ScriptableObjects/Items" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AddPurchasableItem(AssetDatabase.LoadAssetAtPath<ItemDataSO>(path));
        }
#endif
    }

    private void AddPurchasableItem(ItemDataSO item)
    {
        if (IsPurchasableWeapon(item) && !purchasableItems.Contains(item))
        {
            purchasableItems.Add(item);
        }
    }

    private void RemoveInvalidItems()
    {
        for (int i = purchasableItems.Count - 1; i >= 0; i--)
        {
            if (!IsPurchasableWeapon(purchasableItems[i]))
            {
                purchasableItems.RemoveAt(i);
            }
        }
    }

    private static bool IsPurchasableWeapon(ItemDataSO item)
    {
        return item != null &&
               item.IsEquipment &&
               item.acquisitionType == ItemAcquisitionType.Market &&
               item.equipmentSlot == EquipmentSlot.Weapon;
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
