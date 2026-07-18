using System;
using System.Collections.Generic;
using UnityEngine;

public enum RemoteSaleOrderResult
{
    Succeeded,
    InvalidTown,
    CurrentTown,
    InvalidItem,
    InvalidAmount,
    InsufficientInventory,
    InvalidEquipment
}

[Serializable]
public sealed class RemoteSaleOrder
{
    public int TownIndex { get; }
    public int RemainingDays { get; internal set; }
    public ItemDataSO Item { get; }
    public int Amount { get; }
    public EquipmentInstance Equipment { get; }

    public bool IsEquipment => Equipment != null;

    public RemoteSaleOrder(
        int townIndex,
        int remainingDays,
        ItemDataSO item,
        int amount,
        EquipmentInstance equipment)
    {
        TownIndex = townIndex;
        RemainingDays = remainingDays;
        Item = item;
        Amount = amount;
        Equipment = equipment;
    }
}

public enum RemoteSaleEventType
{
    Settled
}

public sealed class RemoteSaleEvent
{
    public RemoteSaleEventType Type { get; }
    public RemoteSaleOrder Order { get; }
    public int Gold { get; }

    public RemoteSaleEvent(RemoteSaleEventType type, RemoteSaleOrder order, int gold)
    {
        Type = type;
        Order = order;
        Gold = gold;
    }
}

public class RemoteSaleManager : MonoBehaviour
{
    [SerializeField] private List<RemoteSaleOrder> activeOrders =
        new List<RemoteSaleOrder>();
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private MarketPriceManager marketPriceManager;
    [SerializeField] private DayManager dayManager;

    public IReadOnlyList<RemoteSaleOrder> ActiveOrders => activeOrders;
    public event Action<RemoteSaleEvent> RemoteSaleEventOccurred;
    public event Action RemoteSaleChanged;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnDestroy()
    {
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
        }
    }

    public void ResolveReferences()
    {
        if (inventory == null)
        {
            inventory = GetComponent<MerchantInventory>() ??
                FindObjectOfType<MerchantInventory>();
        }
        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>() ??
                FindObjectOfType<MerchantData>();
        }
        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>() ??
                FindObjectOfType<TownProgressState>();
        }
        if (marketPriceManager == null)
        {
            marketPriceManager = GetComponent<MarketPriceManager>() ??
                FindObjectOfType<MarketPriceManager>();
        }
        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>() ?? FindObjectOfType<DayManager>();
        }
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
            dayManager.DayChanged += HandleDayChanged;
        }
    }

    public RemoteSaleOrderResult TryCreateItemOrder(
        int townIndex,
        ItemDataSO item,
        int amount)
    {
        ResolveReferences();
        RemoteSaleOrderResult validation = ValidateTown(townIndex);
        if (validation != RemoteSaleOrderResult.Succeeded)
        {
            return validation;
        }
        if (item == null || item.IsEquipment)
        {
            return RemoteSaleOrderResult.InvalidItem;
        }
        if (amount <= 0)
        {
            return RemoteSaleOrderResult.InvalidAmount;
        }
        if (inventory == null || !inventory.TryRemoveItemFrom(townIndex, item, amount))
        {
            return RemoteSaleOrderResult.InsufficientInventory;
        }

        activeOrders.Add(new RemoteSaleOrder(
            townIndex,
            CalculateSettlementDays(townIndex),
            item,
            amount,
            null));
        RemoteSaleChanged?.Invoke();
        return RemoteSaleOrderResult.Succeeded;
    }

    public RemoteSaleOrderResult TryCreateEquipmentOrder(
        int townIndex,
        EquipmentInstance equipment)
    {
        ResolveReferences();
        RemoteSaleOrderResult validation = ValidateTown(townIndex);
        if (validation != RemoteSaleOrderResult.Succeeded)
        {
            return validation;
        }
        if (equipment == null || equipment.BaseItem == null || equipment.IsLocked ||
            inventory == null || !inventory.TryRemoveEquipmentInstanceFrom(townIndex, equipment))
        {
            return RemoteSaleOrderResult.InvalidEquipment;
        }

        activeOrders.Add(new RemoteSaleOrder(
            townIndex,
            CalculateSettlementDays(townIndex),
            null,
            0,
            equipment));
        RemoteSaleChanged?.Invoke();
        return RemoteSaleOrderResult.Succeeded;
    }

    public bool CancelOrder(RemoteSaleOrder order)
    {
        ResolveReferences();
        if (order == null || !activeOrders.Remove(order) || inventory == null)
        {
            return false;
        }
        if (order.IsEquipment)
        {
            inventory.DepositEquipmentTo(order.TownIndex, order.Equipment);
        }
        else
        {
            inventory.DepositItemTo(order.TownIndex, order.Item, order.Amount);
        }
        RemoteSaleChanged?.Invoke();
        return true;
    }

    public int CalculateSettlementDays(int townIndex)
    {
        int currentTown = townProgressState != null
            ? townProgressState.CurrentTownIndex : -1;
        int currentPosition = WorldMapService.GetTownProgressionPosition(currentTown);
        int targetPosition = WorldMapService.GetTownProgressionPosition(townIndex);
        if (currentPosition < 0 || targetPosition < 0)
        {
            return 1;
        }
        return Mathf.Max(1, Mathf.Abs(currentPosition - targetPosition));
    }

    public int GetEstimatedGold(RemoteSaleOrder order)
    {
        if (order == null)
        {
            return 0;
        }
        return order.IsEquipment
            ? GetSellPriceAt(order.TownIndex, order.Equipment)
            : GetSellPriceAt(order.TownIndex, order.Item) * order.Amount;
    }

    public int GetSellPriceAt(int townIndex, ItemDataSO item)
    {
        if (item == null)
        {
            return 0;
        }
        int basePrice = marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item) : item.basePrice;
        return Mathf.Max(1, Mathf.RoundToInt(basePrice *
            WorldMapService.GetTownDemandMultiplier(townIndex, item)));
    }

    public int GetSellPriceAt(int townIndex, EquipmentInstance equipment)
    {
        if (equipment == null || equipment.BaseItem == null)
        {
            return 0;
        }
        int basePrice = marketPriceManager != null
            ? marketPriceManager.GetSellPrice(equipment.BaseItem)
            : equipment.BaseItem.basePrice;
        return Mathf.Max(1, Mathf.RoundToInt(basePrice *
            equipment.GetSellPriceQualityMultiplier() *
            (1f + equipment.EnhancementLevel * 0.12f) *
            WorldMapService.GetTownDemandMultiplier(townIndex, equipment.BaseItem)));
    }

    public List<SavedRemoteSaleOrder> CreateSaveData()
    {
        List<SavedRemoteSaleOrder> result = new List<SavedRemoteSaleOrder>();
        foreach (RemoteSaleOrder order in activeOrders)
        {
            if (order == null)
            {
                continue;
            }
            SavedRemoteSaleOrder saved = new SavedRemoteSaleOrder
            {
                townIndex = order.TownIndex,
                remainingDays = order.RemainingDays,
                itemPersistentId = order.Item != null ? order.Item.PersistentId : string.Empty,
                itemAssetName = order.Item != null ? order.Item.name : string.Empty,
                amount = order.Amount,
                equipment = CreateSavedEquipment(order.Equipment)
            };
            result.Add(saved);
        }
        return result;
    }

    public void Restore(List<SavedRemoteSaleOrder> savedOrders)
    {
        ResolveReferences();
        activeOrders.Clear();
        if (savedOrders != null)
        {
            foreach (SavedRemoteSaleOrder saved in savedOrders)
            {
                RemoteSaleOrder order = RestoreOrder(saved);
                if (order != null)
                {
                    activeOrders.Add(order);
                }
            }
        }
        RemoteSaleChanged?.Invoke();
    }

    private RemoteSaleOrderResult ValidateTown(int townIndex)
    {
        if (townIndex < 0 || townIndex >= WorldMapService.TownNames.Length ||
            WorldMapService.GetTownProgressionPosition(townIndex) < 0)
        {
            return RemoteSaleOrderResult.InvalidTown;
        }
        if (townProgressState == null || townIndex == townProgressState.CurrentTownIndex)
        {
            return RemoteSaleOrderResult.CurrentTown;
        }
        return RemoteSaleOrderResult.Succeeded;
    }

    private void HandleDayChanged(int day)
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            RemoteSaleOrder order = activeOrders[i];
            order.RemainingDays--;
            if (order.RemainingDays > 0)
            {
                continue;
            }
            int gold = GetEstimatedGold(order);
            merchantData?.AddGold(gold);
            activeOrders.RemoveAt(i);
            RemoteSaleEventOccurred?.Invoke(new RemoteSaleEvent(
                RemoteSaleEventType.Settled,
                order,
                gold));
        }
        RemoteSaleChanged?.Invoke();
    }

    private RemoteSaleOrder RestoreOrder(SavedRemoteSaleOrder saved)
    {
        if (saved == null || saved.remainingDays <= 0)
        {
            return null;
        }
        EquipmentInstance equipment = RestoreEquipment(saved.equipment);
        if (equipment != null)
        {
            return new RemoteSaleOrder(saved.townIndex, saved.remainingDays, null, 0, equipment);
        }
        ItemDataSO item = GameAssetRepository.FindByPersistentId<ItemDataSO>(
            saved.itemPersistentId,
            saved.itemAssetName);
        return item != null && saved.amount > 0
            ? new RemoteSaleOrder(saved.townIndex, saved.remainingDays, item, saved.amount, null)
            : null;
    }

    private static SavedEquipmentInstance CreateSavedEquipment(EquipmentInstance equipment)
    {
        if (equipment == null || equipment.BaseItem == null)
        {
            return null;
        }
        SavedEquipmentInstance saved = new SavedEquipmentInstance
        {
            instanceId = equipment.InstanceId,
            baseItemAssetName = equipment.BaseItem.name,
            baseItemPersistentId = equipment.BaseItem.PersistentId,
            quality = equipment.Quality,
            enhancementLevel = equipment.EnhancementLevel,
            isLocked = equipment.IsLocked
        };
        if (equipment.Modifiers == null)
        {
            return saved;
        }
        foreach (EquipmentModifier modifier in equipment.Modifiers)
        {
            if (modifier != null)
            {
                saved.modifiers.Add(new SavedEquipmentModifier
                {
                    type = modifier.type,
                    value = modifier.value
                });
            }
        }
        return saved;
    }

    private static EquipmentInstance RestoreEquipment(SavedEquipmentInstance saved)
    {
        if (saved == null)
        {
            return null;
        }
        ItemDataSO item = GameAssetRepository.FindByPersistentId<ItemDataSO>(
            saved.baseItemPersistentId,
            saved.baseItemAssetName);
        if (item == null)
        {
            return null;
        }
        List<EquipmentModifier> modifiers = new List<EquipmentModifier>();
        if (saved.modifiers != null)
        {
            foreach (SavedEquipmentModifier modifier in saved.modifiers)
            {
                if (modifier != null)
                {
                    modifiers.Add(new EquipmentModifier(modifier.type, modifier.value));
                }
            }
        }
        return EquipmentInstance.CreateRestored(saved.instanceId, item, saved.quality,
            modifiers, saved.enhancementLevel, saved.isLocked);
    }
}
