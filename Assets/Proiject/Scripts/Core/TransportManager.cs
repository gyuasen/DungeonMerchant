using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransportCargo
{
    public ItemDataSO item;
    public int amount;

    public TransportCargo(ItemDataSO item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

[Serializable]
public class TransportConvoy
{
    public int originTownIndex;
    public int destinationTownIndex;
    public List<TransportCargo> cargo = new List<TransportCargo>();
    public List<string> escortInstanceIds = new List<string>();
    public int remainingDays;
    public int totalSegments;
}

public enum TransportDepartureResult
{
    Succeeded,
    InvalidDestination,
    InvalidCargo,
    InsufficientCargo,
    InvalidEscort,
    InsufficientGold
}

public enum TransportEventType
{
    RaidRepelled,
    RaidLoss,
    Arrived
}

public sealed class TransportEvent
{
    public TransportEventType Type { get; }
    public TransportConvoy Convoy { get; }
    public int Gold { get; }
    public int LostCargo { get; }

    public TransportEvent(
        TransportEventType type,
        TransportConvoy convoy,
        int gold = 0,
        int lostCargo = 0)
    {
        Type = type;
        Convoy = convoy;
        Gold = gold;
        LostCargo = lostCargo;
    }
}

public class TransportManager : MonoBehaviour
{
    public const int BaseCostPerSegment = 50;

    private const float RaidChance = .25f;

    [SerializeField] private List<TransportConvoy> activeConvoys =
        new List<TransportConvoy>();
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MarketPriceManager marketPriceManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private DungeonExpeditionManager dungeonExpeditionManager;
    [SerializeField] private TrainingGroundManager trainingGroundManager;

    private Func<float> randomValue = () => UnityEngine.Random.value;
    private bool isDayChangedSubscribed;

    public IReadOnlyList<TransportConvoy> ActiveConvoys => activeConvoys;

    public event Action TransportChanged;
    public event Action<TransportEvent> TransportEventOccurred;

    private void OnEnable()
    {
        ResolveReferences();
    }

    private void OnDisable()
    {
        if (dayManager != null && isDayChangedSubscribed)
        {
            dayManager.DayChanged -= HandleDayChanged;
            isDayChangedSubscribed = false;
        }
    }

    public void SetRandomProvider(Func<float> provider)
    {
        randomValue = provider ?? (() => UnityEngine.Random.value);
    }

    public bool CanCreateConvoy(
        int destinationTownIndex,
        IReadOnlyList<(ItemDataSO item, int amount)> cargo,
        IReadOnlyList<MercenaryInstance> escorts)
    {
        return Validate(destinationTownIndex, cargo, escorts) ==
               TransportDepartureResult.Succeeded;
    }

    public TransportDepartureResult TryDepartConvoy(
        int destinationTownIndex,
        IReadOnlyList<(ItemDataSO item, int amount)> cargo,
        IReadOnlyList<MercenaryInstance> escorts)
    {
        TransportDepartureResult result =
            Validate(destinationTownIndex, cargo, escorts);
        if (result != TransportDepartureResult.Succeeded)
        {
            return result;
        }

        int units = 0;
        foreach ((ItemDataSO item, int amount) entry in cargo)
        {
            units += entry.amount;
        }

        int cost = CalculateTransportCost(destinationTownIndex, units);
        if (!merchantData.TryPayGold(cost))
        {
            return TransportDepartureResult.InsufficientGold;
        }

        foreach ((ItemDataSO item, int amount) entry in cargo)
        {
            inventory.TryRemoveItem(entry.item, entry.amount);
        }

        TransportConvoy convoy = new TransportConvoy
        {
            originTownIndex = townProgressState.CurrentTownIndex,
            destinationTownIndex = destinationTownIndex
        };
        convoy.totalSegments = CalculateSegments(
            convoy.originTownIndex,
            destinationTownIndex);
        convoy.remainingDays = convoy.totalSegments;

        foreach ((ItemDataSO item, int amount) entry in cargo)
        {
            convoy.cargo.Add(new TransportCargo(entry.item, entry.amount));
        }

        if (escorts != null)
        {
            foreach (MercenaryInstance escort in escorts)
            {
                convoy.escortInstanceIds.Add(escort.InstanceId);
            }
        }

        activeConvoys.Add(convoy);
        TransportChanged?.Invoke();
        return TransportDepartureResult.Succeeded;
    }

    public bool IsMercenaryOnTransportDuty(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return false;
        }

        foreach (TransportConvoy convoy in activeConvoys)
        {
            if (convoy.escortInstanceIds.Contains(instanceId))
            {
                return true;
            }
        }

        return false;
    }

    public int CalculateTransportCost(int destinationTownIndex, int cargoUnits)
    {
        ResolveReferences();
        int segments = townProgressState == null
            ? 0
            : CalculateSegments(
                townProgressState.CurrentTownIndex,
                destinationTownIndex);
        float logisticsMultiplier = Mathf.Max(
            .5f,
            1f - (merchantData != null ? merchantData.Logistics : 0) * .01f);
        return Mathf.CeilToInt(
            segments * BaseCostPerSegment * Mathf.Max(0, cargoUnits) *
            logisticsMultiplier);
    }

    public List<SavedTransportConvoy> CreateSaveData()
    {
        List<SavedTransportConvoy> saved = new List<SavedTransportConvoy>();
        foreach (TransportConvoy convoy in activeConvoys)
        {
            SavedTransportConvoy value = new SavedTransportConvoy
            {
                originTownIndex = convoy.originTownIndex,
                destinationTownIndex = convoy.destinationTownIndex,
                remainingDays = convoy.remainingDays,
                totalSegments = convoy.totalSegments,
                escortInstanceIds = new List<string>(convoy.escortInstanceIds)
            };

            foreach (TransportCargo cargo in convoy.cargo)
            {
                if (cargo.item != null && cargo.amount > 0)
                {
                    value.cargo.Add(new SavedTransportCargo
                    {
                        itemPersistentId = cargo.item.PersistentId,
                        itemAssetName = cargo.item.name,
                        amount = cargo.amount
                    });
                }
            }

            saved.Add(value);
        }

        return saved;
    }

    public void Restore(
        List<SavedTransportConvoy> saved,
        IReadOnlyDictionary<string, MercenaryInstance> mercenaries)
    {
        ResolveReferences();
        activeConvoys.Clear();
        if (saved != null)
        {
            foreach (SavedTransportConvoy value in saved)
            {
                if (value == null)
                {
                    continue;
                }

                TransportConvoy convoy = new TransportConvoy
                {
                    originTownIndex = value.originTownIndex,
                    destinationTownIndex = value.destinationTownIndex,
                    remainingDays = Mathf.Max(0, value.remainingDays),
                    totalSegments = Mathf.Max(0, value.totalSegments)
                };
                RestoreCargo(value, convoy);
                RestoreEscorts(value, convoy, mercenaries);
                if (convoy.cargo.Count > 0)
                {
                    activeConvoys.Add(convoy);
                }
            }
        }

        TransportChanged?.Invoke();
    }

    private TransportDepartureResult Validate(
        int destination,
        IReadOnlyList<(ItemDataSO item, int amount)> cargo,
        IReadOnlyList<MercenaryInstance> escorts)
    {
        ResolveReferences();
        if (townProgressState == null ||
            !townProgressState.IsTownUnlocked(destination) ||
            destination == townProgressState.CurrentTownIndex ||
            CalculateSegments(townProgressState.CurrentTownIndex, destination) <= 0)
        {
            return TransportDepartureResult.InvalidDestination;
        }

        if (cargo == null || cargo.Count == 0)
        {
            return TransportDepartureResult.InvalidCargo;
        }

        int units = 0;
        Dictionary<ItemDataSO, int> requested =
            new Dictionary<ItemDataSO, int>();
        foreach ((ItemDataSO item, int amount) entry in cargo)
        {
            if (entry.item == null || entry.amount <= 0)
            {
                return TransportDepartureResult.InvalidCargo;
            }

            requested.TryGetValue(entry.item, out int amount);
            requested[entry.item] = amount + entry.amount;
            units += entry.amount;
        }

        foreach (KeyValuePair<ItemDataSO, int> entry in requested)
        {
            if (!inventory.HasItem(entry.Key, entry.Value))
            {
                return TransportDepartureResult.InsufficientCargo;
            }
        }

        if (escorts != null && escorts.Count > 3)
        {
            return TransportDepartureResult.InvalidEscort;
        }

        HashSet<string> escortIds = new HashSet<string>();
        if (escorts != null)
        {
            foreach (MercenaryInstance escort in escorts)
            {
                if (escort == null ||
                    !escortIds.Add(escort.InstanceId) ||
                    !IsHired(escort) ||
                    escort.CurrentTownIndex != townProgressState.CurrentTownIndex ||
                    (partyManager != null && partyManager.Contains(escort)) ||
                    IsMercenaryOnTransportDuty(escort.InstanceId) ||
                    (trainingGroundManager != null &&
                     trainingGroundManager.IsMercenaryTraining(
                         escort.InstanceId)) ||
                    (dungeonExpeditionManager != null &&
                     dungeonExpeditionManager.IsMercenaryOnExpeditionDuty(escort.InstanceId)))
                {
                    return TransportDepartureResult.InvalidEscort;
                }
            }
        }

        if (merchantData == null ||
            !merchantData.CanPay(CalculateTransportCost(destination, units)))
        {
            return TransportDepartureResult.InsufficientGold;
        }

        return TransportDepartureResult.Succeeded;
    }

    private void HandleDayChanged(int day)
    {
        ResolveReferences();
        for (int i = activeConvoys.Count - 1; i >= 0; i--)
        {
            TransportConvoy convoy = activeConvoys[i];
            ProcessRaid(convoy);
            convoy.remainingDays = Mathf.Max(0, convoy.remainingDays - 1);
            if (convoy.remainingDays == 0)
            {
                int depositedCargo = DepositCargo(convoy);
                UpdateEscortLocations(convoy);
                TransportEventOccurred?.Invoke(new TransportEvent(
                    TransportEventType.Arrived,
                    convoy,
                    depositedCargo));
                activeConvoys.RemoveAt(i);
            }
        }

        TransportChanged?.Invoke();
    }

    private void ProcessRaid(TransportConvoy convoy)
    {
        if (randomValue() >= RaidChance)
        {
            return;
        }

        int travelled = Mathf.Max(
            0,
            convoy.totalSegments - convoy.remainingDays);
        int town = GetTownAtSegment(
            convoy.originTownIndex,
            convoy.destinationTownIndex,
            travelled + 1);
        int required = GetRaidRequiredStrength(town);
        List<MercenaryInstance> escorts = GetEscorts(convoy);
        int strength = CombatPowerCalculator.Calculate(escorts);

        if (strength >= required)
        {
            foreach (MercenaryInstance escort in escorts)
            {
                escort.AddExperience(5);
            }

            TransportEventOccurred?.Invoke(new TransportEvent(
                TransportEventType.RaidRepelled,
                convoy));
            return;
        }

        float lack = required <= 0
            ? 1f
            : Mathf.Clamp01((required - strength) / (float)required);
        float lossRate = Mathf.Lerp(.1f, .5f, lack);
        int lost = 0;
        foreach (TransportCargo cargo in convoy.cargo)
        {
            int amount = Mathf.Clamp(
                Mathf.CeilToInt(cargo.amount * lossRate),
                0,
                cargo.amount);
            cargo.amount -= amount;
            lost += amount;
        }

        foreach (MercenaryInstance escort in escorts)
        {
            int damage = Mathf.Max(
                1,
                Mathf.CeilToInt(escort.MaxHP * (.1f + lack * .2f)));
            escort.SetCurrentHP(Mathf.Max(1, escort.CurrentHP - damage));
        }

        TransportEventOccurred?.Invoke(new TransportEvent(
            TransportEventType.RaidLoss,
            convoy,
            0,
            lost));
    }

    public int GetRaidRequiredStrength(int townIndex)
    {
        return 100 + Mathf.Max(
            0,
            WorldMapService.GetTownProgressionPosition(townIndex)) * 100;
    }

    private int DepositCargo(TransportConvoy convoy)
    {
        if (inventory == null)
        {
            return 0;
        }

        int deposited = 0;
        foreach (TransportCargo cargo in convoy.cargo)
        {
            if (cargo.item == null || cargo.amount <= 0)
            {
                continue;
            }

            if (inventory.DepositItemTo(
                    convoy.destinationTownIndex,
                    cargo.item,
                    cargo.amount))
            {
                deposited += cargo.amount;
            }
        }

        return deposited;
    }

    private void UpdateEscortLocations(TransportConvoy convoy)
    {
        foreach (MercenaryInstance escort in GetEscorts(convoy))
        {
            escort.SetCurrentTownIndex(convoy.destinationTownIndex);
        }
    }

    private int CalculateSegments(int origin, int destination)
    {
        int originPosition = WorldMapService.GetTownProgressionPosition(origin);
        int destinationPosition =
            WorldMapService.GetTownProgressionPosition(destination);
        if (originPosition < 0 || destinationPosition < 0)
        {
            return 0;
        }

        return Mathf.Abs(originPosition - destinationPosition);
    }

    private int GetTownAtSegment(int origin, int destination, int segment)
    {
        int town = origin;
        for (int i = 0; i < segment; i++)
        {
            town = WorldMapService.GetNextTownToward(town, destination);
            if (town < 0)
            {
                return destination;
            }
        }

        return town;
    }

    private List<MercenaryInstance> GetEscorts(TransportConvoy convoy)
    {
        List<MercenaryInstance> result = new List<MercenaryInstance>();
        if (hireManager != null)
        {
            foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
            {
                if (mercenary != null &&
                    convoy.escortInstanceIds.Contains(mercenary.InstanceId))
                {
                    result.Add(mercenary);
                }
            }
        }

        return result;
    }

    private bool IsHired(MercenaryInstance mercenary)
    {
        if (hireManager == null)
        {
            return false;
        }

        foreach (MercenaryInstance hired in hireManager.HiredMercenaries)
        {
            if (ReferenceEquals(hired, mercenary))
            {
                return true;
            }
        }

        return false;
    }

    private void RestoreCargo(
        SavedTransportConvoy saved,
        TransportConvoy convoy)
    {
        if (saved.cargo == null)
        {
            return;
        }

        foreach (SavedTransportCargo savedCargo in saved.cargo)
        {
            ItemDataSO item = savedCargo == null
                ? null
                : GameAssetRepository.FindByPersistentId<ItemDataSO>(
                    savedCargo.itemPersistentId,
                    savedCargo.itemAssetName);
            if (item != null && savedCargo.amount > 0)
            {
                convoy.cargo.Add(new TransportCargo(item, savedCargo.amount));
            }
        }
    }

    private void RestoreEscorts(
        SavedTransportConvoy saved,
        TransportConvoy convoy,
        IReadOnlyDictionary<string, MercenaryInstance> mercenaries)
    {
        if (saved.escortInstanceIds == null || mercenaries == null)
        {
            return;
        }

        foreach (string instanceId in saved.escortInstanceIds)
        {
            if (trainingGroundManager != null &&
                trainingGroundManager.IsMercenaryTraining(instanceId))
            {
                Debug.LogWarning(
                    "Discarded saved transport escort assigned to training.");
                continue;
            }

            if (mercenaries.ContainsKey(instanceId))
            {
                convoy.escortInstanceIds.Add(instanceId);
            }
        }
    }

    private void ResolveReferences()
    {
        if (inventory == null)
        {
            inventory = GetComponent<MerchantInventory>();
        }
        if (inventory == null)
        {
            inventory = FindObjectOfType<MerchantInventory>();
        }
        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>();
        }
        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }
        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>();
        }
        if (townProgressState == null)
        {
            townProgressState = FindObjectOfType<TownProgressState>();
        }
        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>();
        }
        if (hireManager == null)
        {
            hireManager = FindObjectOfType<MercenaryHireManager>();
        }
        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>();
        }
        if (partyManager == null)
        {
            partyManager = FindObjectOfType<MercenaryPartyManager>();
        }
        if (marketPriceManager == null)
        {
            marketPriceManager = GetComponent<MarketPriceManager>();
        }
        if (marketPriceManager == null)
        {
            marketPriceManager = FindObjectOfType<MarketPriceManager>();
        }
        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>();
        }
        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }
        if (dungeonExpeditionManager == null)
        {
            dungeonExpeditionManager = GetComponent<DungeonExpeditionManager>();
        }
        if (dungeonExpeditionManager == null)
        {
            dungeonExpeditionManager = FindObjectOfType<DungeonExpeditionManager>();
        }
        if (trainingGroundManager == null)
        {
            trainingGroundManager = GetComponent<TrainingGroundManager>();
        }
        if (trainingGroundManager == null)
        {
            trainingGroundManager = FindObjectOfType<TrainingGroundManager>();
        }
        EnsureDayChangedSubscription();
    }

    private void EnsureDayChangedSubscription()
    {
        if (dayManager == null)
        {
            return;
        }

        dayManager.DayChanged -= HandleDayChanged;
        dayManager.DayChanged += HandleDayChanged;
        isDayChangedSubscribed = true;
    }
}
