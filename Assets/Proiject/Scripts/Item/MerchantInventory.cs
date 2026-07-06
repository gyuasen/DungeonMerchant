using System;
using System.Collections.Generic;
using UnityEngine;

public class MerchantInventory : MonoBehaviour
{
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MarketPriceManager marketPriceManager;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private List<ItemDataSO> enhancementMaterials =
        new List<ItemDataSO>();
    [SerializeField] private List<InventoryItemStack> items =
        new List<InventoryItemStack>();
    [SerializeField] private List<EquipmentInstance> equipmentInstances =
        new List<EquipmentInstance>();
    [SerializeField] private List<string> discoveredEquipmentAssetNames =
        new List<string>();

    public IReadOnlyList<InventoryItemStack> Items => items;
    public IReadOnlyList<EquipmentInstance> EquipmentInstances => equipmentInstances;
    public IReadOnlyList<string> DiscoveredEquipmentAssetNames =>
        discoveredEquipmentAssetNames;

    public event Action InventoryChanged;

    public int GetUsedStorageSlots()
    {
        int amount = equipmentInstances.Count;
        foreach (InventoryItemStack stack in items)
        {
            if (stack != null) amount += stack.Amount;
        }
        return amount;
    }

    public void AddItem(ItemDataSO item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return;
        }

        ResolveReferences();
        if (progressionManager != null &&
            !progressionManager.CanStore(amount))
        {
            Debug.LogWarning("Storage capacity exceeded.");
            return;
        }

        InventoryItemStack stack = FindStack(item);
        if (stack == null)
        {
            items.Add(new InventoryItemStack(item, amount));
        }
        else
        {
            stack.Add(amount);
        }

        Debug.Log($"Added item: {item.itemName} x{amount}");
        RegisterEquipmentDiscovery(item);
        InventoryChanged?.Invoke();
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

        equipmentInstances.Add(equipment);
        RegisterEquipmentDiscovery(equipment.BaseItem);
        InventoryChanged?.Invoke();
    }

    public bool TryRemoveEquipmentInstance(EquipmentInstance equipment)
    {
        if (equipment == null || !equipmentInstances.Remove(equipment))
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

        InventoryItemStack stack = FindStack(item);
        if (stack == null || !stack.Remove(amount))
        {
            return false;
        }

        merchantData.AddGold(GetSellPrice(item) * amount);

        if (stack.Amount <= 0)
        {
            items.Remove(stack);
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

        return marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item)
            : item.basePrice;
    }

    public int GetSellPrice(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return 0;
        }

        float qualityMultiplier;
        switch (equipment.Quality)
        {
            case EquipmentQuality.Poor: qualityMultiplier = 0.65f; break;
            case EquipmentQuality.Fine: qualityMultiplier = 1.2f; break;
            case EquipmentQuality.Rare: qualityMultiplier = 1.55f; break;
            case EquipmentQuality.Legendary: qualityMultiplier = 2.2f; break;
            default: qualityMultiplier = 1f; break;
        }
        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                GetSellPrice(equipment.BaseItem) *
                qualityMultiplier *
                (1f + equipment.EnhancementLevel * 0.12f)));
    }

    public bool SellEquipmentInstance(EquipmentInstance equipment)
    {
        ResolveReferences();
        if (merchantData == null ||
            equipment == null ||
            equipment.IsLocked ||
            !equipmentInstances.Remove(equipment))
        {
            return false;
        }

        merchantData.AddGold(GetSellPrice(equipment));
        InventoryChanged?.Invoke();
        return true;
    }

    public int GetItemAmount(ItemDataSO item)
    {
        InventoryItemStack stack = FindStack(item);
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

        InventoryItemStack stack = FindStack(item);
        if (stack == null || !stack.Remove(amount))
        {
            return false;
        }

        if (stack.Amount <= 0)
        {
            items.Remove(stack);
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
            InventoryItemStack stack = FindStack(requirement.item);
            stack.Remove(requirement.amount);
            if (stack.Amount <= 0)
            {
                items.Remove(stack);
            }
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public void RestoreItems(IEnumerable<InventoryItemStack> restoredItems)
    {
        items.Clear();
        if (restoredItems != null)
        {
            foreach (InventoryItemStack stack in restoredItems)
            {
                if (stack?.Item != null && stack.Amount > 0)
                {
                    items.Add(stack);
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    public void RestoreEquipmentInstances(
        IEnumerable<EquipmentInstance> restoredEquipment)
    {
        equipmentInstances.Clear();
        if (restoredEquipment != null)
        {
            foreach (EquipmentInstance equipment in restoredEquipment)
            {
                if (equipment?.BaseItem != null)
                {
                    equipmentInstances.Add(equipment);
                    RegisterEquipmentDiscovery(equipment.BaseItem);
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    public void RestoreDiscoveredEquipment(IEnumerable<string> assetNames)
    {
        discoveredEquipmentAssetNames.Clear();
        if (assetNames != null)
        {
            foreach (string assetName in assetNames)
            {
                if (!string.IsNullOrWhiteSpace(assetName) &&
                    !discoveredEquipmentAssetNames.Contains(assetName))
                {
                    discoveredEquipmentAssetNames.Add(assetName);
                }
            }
        }

        foreach (EquipmentInstance equipment in equipmentInstances)
        {
            RegisterEquipmentDiscovery(equipment?.BaseItem);
        }
    }

    public bool HasDiscoveredEquipment(ItemDataSO item)
    {
        return item != null &&
               discoveredEquipmentAssetNames.Contains(item.name);
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

    private InventoryItemStack FindStack(ItemDataSO item)
    {
        foreach (InventoryItemStack stack in items)
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

        if (enhancementMaterials.Count < 5 ||
            enhancementMaterials.Exists(item => item == null))
        {
            PopulateEnhancementMaterials();
        }
    }

    public void RegisterEquipmentDiscovery(ItemDataSO item)
    {
        if (item == null ||
            !item.IsEquipment ||
            discoveredEquipmentAssetNames.Contains(item.name))
        {
            return;
        }

        discoveredEquipmentAssetNames.Add(item.name);
    }

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
