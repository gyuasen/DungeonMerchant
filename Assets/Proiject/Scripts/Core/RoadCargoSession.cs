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
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private TrainingGroundManager trainingGroundManager;
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

    public bool IsCompanionInTransit(string instanceId)
    {
        return !string.IsNullOrWhiteSpace(instanceId) &&
               activeSession?.companionInstanceIds != null &&
               activeSession.companionInstanceIds.Contains(instanceId);
    }

    public IReadOnlyList<string> GetCompanionInstanceIds()
    {
        return activeSession?.companionInstanceIds ??
            (IReadOnlyList<string>)Array.Empty<string>();
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
        return TryBegin(originTownIndex, destinationTownIndex, cargo, null);
    }

    public RoadCargoDepartureResult TryBegin(
        int originTownIndex,
        int destinationTownIndex,
        IReadOnlyList<RoadCargoEntry> cargo,
        IReadOnlyList<string> companionInstanceIds)
    {
        ResolveReferences();
        if (IsActive)
        {
            return RoadCargoDepartureResult.AlreadyActive;
        }

        if (originTownIndex < 0 ||
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

        List<string> companions = NormalizeCompanions(
            companionInstanceIds,
            originTownIndex);
        if (companions == null)
        {
            return RoadCargoDepartureResult.InvalidCargo;
        }

        if (normalized.Count == 0 && companions.Count == 0)
        {
            return RoadCargoDepartureResult.InvalidCargo;
        }

        if (normalized.Count > 0 && inventory == null)
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
        activeSession.companionInstanceIds.AddRange(companions);
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

    public void CompleteCompanionsVictory()
    {
        if (!IsActive || activeSession.companionInstanceIds == null)
        {
            return;
        }

        foreach (string instanceId in activeSession.companionInstanceIds)
        {
            MercenaryInstance mercenary = FindHiredMercenary(instanceId);
            if (mercenary != null)
            {
                mercenary.SetCurrentTownIndex(activeSession.destinationTownIndex);
            }
        }
        activeSession.companionInstanceIds.Clear();
        ClearSessionIfEmpty();
        CargoChanged?.Invoke();
    }

    public void ReleaseCompanionReservations()
    {
        if (!IsActive || activeSession.companionInstanceIds == null)
        {
            return;
        }
        activeSession.companionInstanceIds.Clear();
        ClearSessionIfEmpty();
        CargoChanged?.Invoke();
    }

    public RoadCargoResolutionResult Retreat()
    {
        RoadCargoResolutionResult result = TryDepositActiveCargo(activeSession != null
            ? activeSession.originTownIndex
            : -1);
        ReleaseCompanionReservations();
        return result;
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
        foreach (string instanceId in activeSession.companionInstanceIds)
        {
            saved.companionInstanceIds.Add(instanceId);
        }
        return saved;
    }

    public void Restore(SavedRoadCargoSession saved)
    {
        ResolveReferences();
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
        if (total == 0)
        {
            activeSession.cargo.Clear();
            ClearSessionIfEmpty();
            CargoChanged?.Invoke();
            return RoadCargoResolutionResult.Succeeded;
        }
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
        activeSession.cargo.Clear();
        ClearSessionIfEmpty();
        CargoChanged?.Invoke();
        return RoadCargoResolutionResult.Succeeded;
    }

    private static Dictionary<ItemDataSO, int> NormalizeCargo(
        IReadOnlyList<RoadCargoEntry> cargo)
    {
        Dictionary<ItemDataSO, int> normalized =
            new Dictionary<ItemDataSO, int>();
        if (cargo == null)
        {
            return normalized;
        }
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
        if (!WorldMapService.IsValidTownIndex(saved.originTownIndex) ||
            !WorldMapService.IsValidTownIndex(saved.destinationTownIndex) ||
            saved.originTownIndex == saved.destinationTownIndex)
        {
            Debug.LogWarning("Discarded invalid saved road cargo session.");
            return null;
        }
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
        if (saved.companionInstanceIds != null)
        {
            foreach (string instanceId in saved.companionInstanceIds)
            {
                if (copy.companionInstanceIds.Contains(instanceId) ||
                    !IsValidCompanion(instanceId, copy.originTownIndex))
                {
                    Debug.LogWarning("Discarded invalid saved road companion.");
                    continue;
                }
                copy.companionInstanceIds.Add(instanceId);
            }
        }
        return copy.cargo.Count > 0 || copy.companionInstanceIds.Count > 0
            ? copy
            : null;
    }

    private List<string> NormalizeCompanions(
        IReadOnlyList<string> companionInstanceIds,
        int originTownIndex)
    {
        List<string> normalized = new List<string>();
        if (companionInstanceIds == null)
        {
            return normalized;
        }
        foreach (string instanceId in companionInstanceIds)
        {
            if (string.IsNullOrWhiteSpace(instanceId) ||
                normalized.Contains(instanceId) ||
                !IsValidCompanion(instanceId, originTownIndex))
            {
                return null;
            }
            normalized.Add(instanceId);
        }
        return normalized;
    }

    private bool IsValidCompanion(string instanceId, int originTownIndex)
    {
        MercenaryInstance mercenary = FindHiredMercenary(instanceId);
        return mercenary != null &&
               mercenary.CurrentTownIndex == originTownIndex &&
               mercenary.IsContractActive &&
               !MercenaryDutyService.IsOnDuty(instanceId);
    }

    private MercenaryInstance FindHiredMercenary(string instanceId)
    {
        if (hireManager == null || string.IsNullOrWhiteSpace(instanceId))
        {
            return null;
        }
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && mercenary.InstanceId == instanceId)
            {
                return mercenary;
            }
        }
        return null;
    }

    private void ClearSessionIfEmpty()
    {
        if (activeSession != null && activeSession.cargo.Count == 0 &&
            activeSession.companionInstanceIds.Count == 0)
        {
            activeSession = null;
        }
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
        hireManager = hireManager ?? GetComponent<MercenaryHireManager>() ??
            FindObjectOfType<MercenaryHireManager>();
        partyManager = partyManager ?? GetComponent<MercenaryPartyManager>() ??
            FindObjectOfType<MercenaryPartyManager>();
        trainingGroundManager = trainingGroundManager ??
            GetComponent<TrainingGroundManager>() ??
            FindObjectOfType<TrainingGroundManager>();
    }
}
