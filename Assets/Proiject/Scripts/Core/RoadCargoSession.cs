using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RoadCargoEntry
{
    public ItemDataSO item;
    public int amount;

    public RoadCargoEntry(ItemDataSO item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

public enum RoadCargoDepartureResult
{
    Succeeded,
    AlreadyActive,
    InvalidTown,
    InvalidCargo,
    InsufficientCargo,
    OverCapacity
}

public enum RoadCargoResolutionResult
{
    NoActiveSession,
    Succeeded,
    StorageFull
}

public sealed class RoadCargoSession : MonoBehaviour
{
    public const int BaseCapacity = 20;
    public const int MaximumCapacity = 50;

    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private SavedRoadCargoSession activeSession;

    public SavedRoadCargoSession ActiveSession => activeSession;
    public bool IsActive => activeSession != null;
    public event Action CargoChanged;

    public static int CalculateCapacity(MerchantData data)
    {
        int logistics = data != null ? data.Logistics : 0;
        return Mathf.Clamp(BaseCapacity + logistics, BaseCapacity, MaximumCapacity);
    }

    public int Capacity => CalculateCapacity(merchantData);

    public int UsedCapacity
    {
        get
        {
            int total = 0;
            if (activeSession?.cargo != null)
            {
                foreach (SavedTransportCargo cargo in activeSession.cargo)
                {
                    if (cargo != null)
                    {
                        total += Mathf.Max(0, cargo.amount);
                    }
                }
            }
            return total;
        }
    }

    public List<RoadCargoEntry> GetCargoManifest()
    {
        List<RoadCargoEntry> manifest = new List<RoadCargoEntry>();
        if (activeSession?.cargo == null)
        {
            return manifest;
        }

        foreach (SavedTransportCargo cargo in activeSession.cargo)
        {
            ItemDataSO item = ResolveItem(cargo);
            if (item != null && cargo.amount > 0)
            {
                manifest.Add(new RoadCargoEntry(item, cargo.amount));
            }
        }
        return manifest;
    }

    private void OnEnable()
    {
        ResolveReferences();
    }

    public RoadCargoDepartureResult TryBegin(
        int originTownIndex,
        int destinationTownIndex,
        IReadOnlyList<RoadCargoEntry> cargo)
    {
        ResolveReferences();
        if (IsActive)
        {
            return RoadCargoDepartureResult.AlreadyActive;
        }

        if (inventory == null ||
            originTownIndex < 0 ||
            destinationTownIndex < 0 ||
            originTownIndex == destinationTownIndex)
        {
            return RoadCargoDepartureResult.InvalidTown;
        }

        Dictionary<ItemDataSO, int> normalized = NormalizeCargo(cargo);
        if (normalized == null)
        {
            return RoadCargoDepartureResult.InvalidCargo;
        }

        int total = 0;
        foreach (KeyValuePair<ItemDataSO, int> entry in normalized)
        {
            total += entry.Value;
            if (inventory.GetItemAmountIn(originTownIndex, entry.Key) < entry.Value)
            {
                return RoadCargoDepartureResult.InsufficientCargo;
            }
        }

        if (total > Capacity)
        {
            return RoadCargoDepartureResult.OverCapacity;
        }

        foreach (KeyValuePair<ItemDataSO, int> entry in normalized)
        {
            if (!inventory.TryRemoveItemFrom(originTownIndex, entry.Key, entry.Value))
            {
                Debug.LogError("Road cargo departure failed after validation.");
                return RoadCargoDepartureResult.InsufficientCargo;
            }
        }

        activeSession = new SavedRoadCargoSession
        {
            originTownIndex = originTownIndex,
            destinationTownIndex = destinationTownIndex
        };
        foreach (KeyValuePair<ItemDataSO, int> entry in normalized)
        {
            activeSession.cargo.Add(new SavedTransportCargo
            {
                itemPersistentId = entry.Key.PersistentId,
                itemAssetName = entry.Key.name,
                amount = entry.Value
            });
        }
        CargoChanged?.Invoke();
        return RoadCargoDepartureResult.Succeeded;
    }

    public RoadCargoResolutionResult CompleteVictory()
    {
        return TryDepositActiveCargo(activeSession != null
            ? activeSession.destinationTownIndex
            : -1);
    }

    public RoadCargoResolutionResult Retreat()
    {
        return TryDepositActiveCargo(activeSession != null
            ? activeSession.originTownIndex
            : -1);
    }

    public RoadCargoResolutionResult CompleteDefeat()
    {
        if (!IsActive)
        {
            return RoadCargoResolutionResult.NoActiveSession;
        }

        if (!activeSession.defeatLossApplied)
        {
            foreach (SavedTransportCargo cargo in activeSession.cargo)
            {
                if (cargo != null)
                {
                    int loss = Mathf.CeilToInt(Mathf.Max(0, cargo.amount) * .25f);
                    cargo.amount = Mathf.Max(0, cargo.amount - loss);
                }
            }
            activeSession.defeatLossApplied = true;
        }
        activeSession.cargo.RemoveAll(cargo => cargo == null || cargo.amount <= 0);
        CargoChanged?.Invoke();
        if (activeSession.cargo.Count == 0)
        {
            activeSession = null;
            CargoChanged?.Invoke();
            return RoadCargoResolutionResult.Succeeded;
        }
        return Retreat();
    }

    public SavedRoadCargoSession CreateSaveData()
    {
        if (!IsActive)
        {
            return null;
        }

        SavedRoadCargoSession saved = new SavedRoadCargoSession
        {
            originTownIndex = activeSession.originTownIndex,
            destinationTownIndex = activeSession.destinationTownIndex,
            defeatLossApplied = activeSession.defeatLossApplied
        };
        foreach (SavedTransportCargo cargo in activeSession.cargo)
        {
            if (cargo != null && cargo.amount > 0)
            {
                saved.cargo.Add(new SavedTransportCargo
                {
                    itemPersistentId = cargo.itemPersistentId,
                    itemAssetName = cargo.itemAssetName,
                    amount = cargo.amount
                });
            }
        }
        return saved;
    }

    public void Restore(SavedRoadCargoSession saved)
    {
        activeSession = saved == null ? null : CreateValidatedCopy(saved);
        CargoChanged?.Invoke();
    }

    private RoadCargoResolutionResult TryDepositActiveCargo(int townIndex)
    {
        ResolveReferences();
        if (!IsActive)
        {
            return RoadCargoResolutionResult.NoActiveSession;
        }

        int total = UsedCapacity;
        if (inventory == null || !inventory.CanDepositItemsTo(townIndex, total))
        {
            return RoadCargoResolutionResult.StorageFull;
        }

        foreach (SavedTransportCargo cargo in activeSession.cargo)
        {
            ItemDataSO item = ResolveItem(cargo);
            if (item == null || !inventory.DepositItemTo(townIndex, item, cargo.amount))
            {
                Debug.LogError("Road cargo deposit failed after capacity validation.");
                return RoadCargoResolutionResult.StorageFull;
            }
        }
        activeSession = null;
        CargoChanged?.Invoke();
        return RoadCargoResolutionResult.Succeeded;
    }

    private static Dictionary<ItemDataSO, int> NormalizeCargo(
        IReadOnlyList<RoadCargoEntry> cargo)
    {
        if (cargo == null || cargo.Count == 0)
        {
            return null;
        }

        Dictionary<ItemDataSO, int> normalized =
            new Dictionary<ItemDataSO, int>();
        foreach (RoadCargoEntry entry in cargo)
        {
            if (entry?.item == null ||
                (entry.item.itemType != ItemType.Material &&
                 entry.item.itemType != ItemType.Consumable) ||
                entry.amount <= 0)
            {
                return null;
            }
            normalized.TryGetValue(entry.item, out int amount);
            normalized[entry.item] = amount + entry.amount;
        }
        return normalized;
    }

    private SavedRoadCargoSession CreateValidatedCopy(SavedRoadCargoSession saved)
    {
        SavedRoadCargoSession copy = new SavedRoadCargoSession
        {
            originTownIndex = saved.originTownIndex,
            destinationTownIndex = saved.destinationTownIndex,
            defeatLossApplied = saved.defeatLossApplied
        };
        if (saved.cargo != null)
        {
            foreach (SavedTransportCargo cargo in saved.cargo)
            {
                ItemDataSO item = ResolveItem(cargo);
                if (item == null ||
                    (item.itemType != ItemType.Material &&
                     item.itemType != ItemType.Consumable) ||
                    cargo.amount <= 0)
                {
                    Debug.LogWarning("Discarded invalid saved road cargo entry.");
                    continue;
                }
                copy.cargo.Add(new SavedTransportCargo
                {
                    itemPersistentId = item.PersistentId,
                    itemAssetName = item.name,
                    amount = cargo.amount
                });
            }
        }
        return copy.cargo.Count > 0 ? copy : null;
    }

    private static ItemDataSO ResolveItem(SavedTransportCargo cargo)
    {
        return cargo == null
            ? null
            : GameAssetRepository.FindByPersistentId<ItemDataSO>(
                cargo.itemPersistentId,
                cargo.itemAssetName);
    }

    private void ResolveReferences()
    {
        merchantData = merchantData ?? GetComponent<MerchantData>() ??
            FindObjectOfType<MerchantData>();
        inventory = inventory ?? GetComponent<MerchantInventory>() ??
            FindObjectOfType<MerchantInventory>();
    }
}
