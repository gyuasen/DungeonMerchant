using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class TownInventoryBucket
{
    public int townIndex;
    public List<InventoryItemStack> items = new List<InventoryItemStack>();
    public List<EquipmentInstance> equipmentInstances =
        new List<EquipmentInstance>();
}

public class MerchantInventory : MonoBehaviour
{
    private const int FallbackTownIndex = 2;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MarketPriceManager marketPriceManager;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private List<ItemDataSO> enhancementMaterials =
        new List<ItemDataSO>();
    [SerializeField] private List<InventoryItemStack> items =
        new List<InventoryItemStack>();
    [SerializeField] private List<EquipmentInstance> equipmentInstances =
        new List<EquipmentInstance>();
    [SerializeField] private List<TownInventoryBucket> townBuckets =
        new List<TownInventoryBucket>();
    [FormerlySerializedAs("discoveredEquipmentAssetNames")]
    [SerializeField] private List<string> discoveredEquipmentPersistentIds =
        new List<string>();

    public IReadOnlyList<InventoryItemStack> Items => CurrentBucket.items;
    public IReadOnlyList<EquipmentInstance> EquipmentInstances =>
        CurrentBucket.equipmentInstances;
    public IReadOnlyList<string> DiscoveredEquipmentPersistentIds =>
        discoveredEquipmentPersistentIds;

    public event Action InventoryChanged;

    public int GetUsedStorageSlots()
    {
        return GetUsedStorageSlotsIn(CurrentTownIndex);
    }

    public int GetUsedStorageSlotsIn(int townIndex)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, false);
        if (bucket == null)
        {
            return 0;
        }

        int amount = bucket.equipmentInstances.Count;
        foreach (InventoryItemStack stack in bucket.items)
        {
            if (stack != null) amount += stack.Amount;
        }
        return amount;
    }

    public void AddItem(ItemDataSO item, int amount = 1)
    {
        if (!TryAddItem(item, amount) && item != null && amount > 0)
        {
            Debug.LogWarning("Storage capacity exceeded.");
        }
    }

    public bool TryAddItem(ItemDataSO item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        ResolveReferences();
        if (progressionManager != null &&
            !progressionManager.CanStore(amount))
        {
            return false;
        }

        InventoryItemStack stack = FindStack(CurrentBucket, item);
        if (stack == null)
        {
            CurrentBucket.items.Add(new InventoryItemStack(item, amount));
        }
        else
        {
            stack.Add(amount);
        }

        Debug.Log($"Added item: {item.itemName} x{amount}");
        RegisterEquipmentDiscovery(item);
        InventoryChanged?.Invoke();
        return true;
    }

    public void AddEquipmentInstance(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        ResolveReferences();
        if (progressionManager != null &&
            !progressionManager.CanStore())
        {
            Debug.LogWarning("Storage capacity exceeded.");
            return;
        }

        CurrentBucket.equipmentInstances.Add(equipment);
        RegisterEquipmentDiscovery(equipment.BaseItem);
        InventoryChanged?.Invoke();
    }

    public bool TryRemoveEquipmentInstance(EquipmentInstance equipment)
    {
        if (equipment == null || !CurrentBucket.equipmentInstances.Remove(equipment))
        {
            return false;
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public EquipmentEnhancementResult TryEnhanceEquipment(
        EquipmentInstance equipment)
    {
        ResolveReferences();
        if (equipment?.BaseItem == null ||
            merchantData == null ||
            equipment.EnhancementLevel >= 10)
        {
            return EquipmentEnhancementResult.Invalid;
        }

        int cost = equipment.GetEnhancementCost();
        int materialAmount = equipment.GetEnhancementMaterialAmount();
        ItemDataSO enhancementMaterial =
            GetEnhancementMaterial(equipment);
        if (enhancementMaterial == null ||
            !HasItem(enhancementMaterial, materialAmount))
        {
            return EquipmentEnhancementResult.NotEnoughMaterial;
        }

        if (!merchantData.TryPayGold(cost))
        {
            return EquipmentEnhancementResult.NotEnoughGold;
        }

        if (!TryRemoveItem(enhancementMaterial, materialAmount))
        {
            return EquipmentEnhancementResult.NotEnoughMaterial;
        }

        if (UnityEngine.Random.value > equipment.GetEnhancementSuccessRate())
        {
            InventoryChanged?.Invoke();
            return EquipmentEnhancementResult.Failed;
        }

        if (!equipment.TryEnhance())
        {
            return EquipmentEnhancementResult.Invalid;
        }

        InventoryChanged?.Invoke();
        return EquipmentEnhancementResult.Succeeded;
    }

    public bool SellItem(ItemDataSO item, int amount = 1)
    {
        ResolveReferences();

        if (merchantData == null || item == null || amount <= 0)
        {
            return false;
        }

        InventoryItemStack stack = FindStack(CurrentBucket, item);
        if (stack == null || !stack.Remove(amount))
        {
            return false;
        }

        merchantData.AddGold(GetSellPrice(item) * amount);

        if (stack.Amount <= 0)
        {
            CurrentBucket.items.Remove(stack);
        }

        Debug.Log($"Sold item: {item.itemName} x{amount}");
        InventoryChanged?.Invoke();
        return true;
    }

    public int GetSellPrice(ItemDataSO item)
    {
        ResolveReferences();

        if (item == null)
        {
            return 0;
        }

        int baseSellPrice = marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item)
            : item.basePrice;
        return Mathf.Max(
            1,
            Mathf.RoundToInt(baseSellPrice * GetTownDemandMultiplier(item)));
    }

    public int GetSellPrice(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return 0;
        }

        ResolveReferences();
        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                GetBaseSellPrice(equipment.BaseItem) *
                equipment.GetSellPriceQualityMultiplier() *
                (1f + equipment.EnhancementLevel * 0.12f) *
                GetTownDemandMultiplier(equipment.BaseItem)));
    }

    public bool SellEquipmentInstance(EquipmentInstance equipment)
    {
        ResolveReferences();
        if (merchantData == null ||
            equipment == null ||
            equipment.IsLocked ||
            !CurrentBucket.equipmentInstances.Remove(equipment))
        {
            return false;
        }

        merchantData.AddGold(GetSellPrice(equipment));
        InventoryChanged?.Invoke();
        return true;
    }

    public int GetItemAmount(ItemDataSO item)
    {
        InventoryItemStack stack = FindStack(CurrentBucket, item);
        return stack != null ? stack.Amount : 0;
    }

    public bool HasItem(ItemDataSO item, int amount = 1)
    {
        return item != null && amount > 0 && GetItemAmount(item) >= amount;
    }

    public bool TryRemoveItem(ItemDataSO item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        InventoryItemStack stack = FindStack(CurrentBucket, item);
        if (stack == null || !stack.Remove(amount))
        {
            return false;
        }

        if (stack.Amount <= 0)
        {
            CurrentBucket.items.Remove(stack);
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public bool TryConsumeMaterials(CraftingMaterialRequirement[] requirements)
    {
        if (requirements == null)
        {
            return true;
        }

        foreach (CraftingMaterialRequirement requirement in requirements)
        {
            if (requirement == null ||
                requirement.item == null ||
                requirement.amount <= 0 ||
                !HasItem(requirement.item, requirement.amount))
            {
                return false;
            }
        }

        foreach (CraftingMaterialRequirement requirement in requirements)
        {
            InventoryItemStack stack = FindStack(CurrentBucket, requirement.item);
            stack.Remove(requirement.amount);
            if (stack.Amount <= 0)
            {
                CurrentBucket.items.Remove(stack);
            }
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public void RestoreItems(IEnumerable<InventoryItemStack> restoredItems)
    {
        RestoreItemsIn(CurrentTownIndex, restoredItems);
    }

    public void RestoreItemsIn(
        int townIndex,
        IEnumerable<InventoryItemStack> restoredItems)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, true);
        bucket.items.Clear();
        if (restoredItems != null)
        {
            foreach (InventoryItemStack stack in restoredItems)
            {
                if (stack?.Item != null && stack.Amount > 0)
                {
                    bucket.items.Add(stack);
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    public void RestoreEquipmentInstances(
        IEnumerable<EquipmentInstance> restoredEquipment)
    {
        RestoreEquipmentInstancesIn(CurrentTownIndex, restoredEquipment);
    }

    public void RestoreEquipmentInstancesIn(
        int townIndex,
        IEnumerable<EquipmentInstance> restoredEquipment)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, true);
        bucket.equipmentInstances.Clear();
        if (restoredEquipment != null)
        {
            foreach (EquipmentInstance equipment in restoredEquipment)
            {
                if (equipment?.BaseItem != null)
                {
                    bucket.equipmentInstances.Add(equipment);
                    RegisterEquipmentDiscovery(equipment.BaseItem);
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    public void RestoreDiscoveredEquipment(
        IEnumerable<string> persistentIds,
        IEnumerable<string> legacyAssetNames)
    {
        discoveredEquipmentPersistentIds.Clear();
        AddDiscoveredEquipmentIdentifiers(persistentIds, false);
        AddDiscoveredEquipmentIdentifiers(legacyAssetNames, true);

        foreach (TownInventoryBucket bucket in townBuckets)
        {
            foreach (EquipmentInstance equipment in bucket.equipmentInstances)
            {
                RegisterEquipmentDiscovery(equipment?.BaseItem);
            }
        }
    }

    private void AddDiscoveredEquipmentIdentifiers(
        IEnumerable<string> identifiers,
        bool resolveLegacyName)
    {
        if (identifiers == null)
        {
            return;
        }

        foreach (string identifier in identifiers)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                continue;
            }

            string persistentId = identifier;
            if (resolveLegacyName)
            {
                ItemDataSO item =
                    GameAssetRepository.FindByName<ItemDataSO>(identifier);
                persistentId = item != null ? item.PersistentId : identifier;
            }

            if (!discoveredEquipmentPersistentIds.Contains(persistentId))
            {
                discoveredEquipmentPersistentIds.Add(persistentId);
            }
        }
    }

    public bool HasDiscoveredEquipment(ItemDataSO item)
    {
        return item != null &&
               (discoveredEquipmentPersistentIds.Contains(item.PersistentId) ||
                discoveredEquipmentPersistentIds.Contains(item.name));
    }

    public void ToggleEquipmentLock(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        ResolveReferences();
        if (progressionManager != null &&
            !progressionManager.CanStore())
        {
            Debug.LogWarning("Storage capacity exceeded.");
            return;
        }

        equipment.ToggleLock();
        InventoryChanged?.Invoke();
    }

    public ItemDataSO GetEnhancementMaterial(EquipmentInstance equipment)
    {
        ResolveReferences();
        if (equipment == null || enhancementMaterials.Count < 5)
        {
            return null;
        }

        int targetLevel = equipment.EnhancementLevel + 1;
        int materialIndex = Mathf.Clamp((targetLevel - 1) / 2, 0, 4);
        return enhancementMaterials[materialIndex];
    }

    public int GetItemAmountIn(int townIndex, ItemDataSO item)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, false);
        InventoryItemStack stack = bucket == null ? null : FindStack(bucket, item);
        return stack != null ? stack.Amount : 0;
    }

    public IReadOnlyList<InventoryItemStack> GetItemsIn(int townIndex)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, false);
        return bucket != null ? bucket.items : Array.Empty<InventoryItemStack>();
    }

    public IReadOnlyList<EquipmentInstance> GetEquipmentInstancesIn(int townIndex)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, false);
        return bucket != null
            ? bucket.equipmentInstances
            : Array.Empty<EquipmentInstance>();
    }

    public bool DepositItemTo(int townIndex, ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        TownInventoryBucket bucket = GetBucket(townIndex, true);
        InventoryItemStack stack = FindStack(bucket, item);
        if (stack == null)
        {
            bucket.items.Add(new InventoryItemStack(item, amount));
        }
        else
        {
            stack.Add(amount);
        }

        RegisterEquipmentDiscovery(item);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool TryRemoveItemFrom(int townIndex, ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        TownInventoryBucket bucket = GetBucket(townIndex, false);
        InventoryItemStack stack = bucket == null ? null : FindStack(bucket, item);
        if (stack == null || !stack.Remove(amount))
        {
            return false;
        }

        if (stack.Amount <= 0)
        {
            bucket.items.Remove(stack);
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public void DepositEquipmentTo(int townIndex, EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        TownInventoryBucket bucket = GetBucket(townIndex, true);
        bucket.equipmentInstances.Add(equipment);
        RegisterEquipmentDiscovery(equipment.BaseItem);
        InventoryChanged?.Invoke();
    }

    public bool TryRemoveEquipmentInstanceFrom(
        int townIndex,
        EquipmentInstance equipment)
    {
        TownInventoryBucket bucket = GetBucket(townIndex, false);
        if (equipment == null || bucket == null ||
            !bucket.equipmentInstances.Remove(equipment))
        {
            return false;
        }

        InventoryChanged?.Invoke();
        return true;
    }

    private InventoryItemStack FindStack(
        TownInventoryBucket bucket,
        ItemDataSO item)
    {
        if (bucket == null)
        {
            return null;
        }

        foreach (InventoryItemStack stack in bucket.items)
        {
            if (stack.Item == item)
            {
                return stack;
            }
        }

        return null;
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

        if (marketPriceManager == null)
        {
            marketPriceManager = GetComponent<MarketPriceManager>();
        }

        if (marketPriceManager == null)
        {
            marketPriceManager = FindObjectOfType<MarketPriceManager>();
        }

        if (progressionManager == null)
        {
            progressionManager = GetComponent<ProgressionManager>() ??
                                 FindObjectOfType<ProgressionManager>();
        }

        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>() ??
                                FindObjectOfType<TownProgressState>();
        }

        if (enhancementMaterials.Count < 5 ||
            enhancementMaterials.Exists(item => item == null))
        {
            PopulateEnhancementMaterials();
        }
    }

    private int CurrentTownIndex
    {
        get
        {
            ResolveReferences();
            return townProgressState != null
                ? townProgressState.CurrentTownIndex
                : FallbackTownIndex;
        }
    }

    private TownInventoryBucket CurrentBucket => GetBucket(CurrentTownIndex, true);

    private TownInventoryBucket GetBucket(int townIndex, bool create)
    {
        MigrateLegacyStorage();
        foreach (TownInventoryBucket bucket in townBuckets)
        {
            if (bucket != null && bucket.townIndex == townIndex)
            {
                return bucket;
            }
        }

        if (!create)
        {
            return null;
        }

        TownInventoryBucket created = new TownInventoryBucket
        {
            townIndex = townIndex
        };
        townBuckets.Add(created);
        return created;
    }

    private void MigrateLegacyStorage()
    {
        if ((items == null || items.Count == 0) &&
            (equipmentInstances == null || equipmentInstances.Count == 0))
        {
            return;
        }

        TownInventoryBucket bucket = townBuckets.Find(value =>
            value != null && value.townIndex == FallbackTownIndex);
        if (bucket == null)
        {
            bucket = new TownInventoryBucket { townIndex = FallbackTownIndex };
            townBuckets.Add(bucket);
        }
        bucket.items.AddRange(items);
        bucket.equipmentInstances.AddRange(equipmentInstances);
        items.Clear();
        equipmentInstances.Clear();
    }

    private int GetBaseSellPrice(ItemDataSO item)
    {
        return marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item)
            : item.basePrice;
    }

    private float GetTownDemandMultiplier(ItemDataSO item)
    {
        return townProgressState != null
            ? WorldMapService.GetTownDemandMultiplier(
                townProgressState.CurrentTownIndex,
                item)
            : 1f;
    }

    public void RegisterEquipmentDiscovery(ItemDataSO item)
    {
        if (item == null ||
            !item.IsEquipment ||
            HasDiscoveredEquipment(item))
        {
            return;
        }

        discoveredEquipmentPersistentIds.Add(item.PersistentId);
    }

#if UNITY_EDITOR
    public void ClearEquipmentDiscoveryForEditor()
    {
        discoveredEquipmentPersistentIds.Clear();
        InventoryChanged?.Invoke();
    }
#endif

    private void PopulateEnhancementMaterials()
    {
        string[] names =
        {
            "Low Grade Enhancement Ore",
            "Lower Grade Enhancement Ore",
            "Middle Grade Enhancement Ore",
            "Upper Grade Enhancement Ore",
            "Highest Grade Enhancement Ore"
        };
        enhancementMaterials.Clear();
        List<ItemDataSO> allItems =
            new List<ItemDataSO>(
                GameAssetRepository.LoadAll<ItemDataSO>());

        foreach (string itemName in names)
        {
            ItemDataSO found = allItems.Find(item =>
                item != null &&
                (item.itemName == itemName ||
                 (itemName == "Low Grade Enhancement Ore" &&
                  item.itemName == "Enhancement Ore")));
            enhancementMaterials.Add(found);
        }
    }
}

public enum EquipmentEnhancementResult
{
    Succeeded,
    Failed,
    NotEnoughGold,
    NotEnoughMaterial,
    Invalid
}
