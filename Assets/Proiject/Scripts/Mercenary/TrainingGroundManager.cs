using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TrainingReservation
{
    [SerializeField] private string mercenaryInstanceId;
    [SerializeField] private int trainingTownIndex;
    [SerializeField] private int startDay;
    [SerializeField] private int targetLevel;
    [SerializeField] private int completionDay;
    [SerializeField] private int paidCost;
    [NonSerialized] private MercenaryInstance mercenary;

    public string MercenaryInstanceId => mercenaryInstanceId;
    public int TrainingTownIndex => trainingTownIndex;
    public int StartDay => startDay;
    public int TargetLevel => targetLevel;
    public int CompletionDay => completionDay;
    public int PaidCost => paidCost;

    public TrainingReservation(
        string instanceId,
        int trainingTownIndex,
        int startDay,
        int targetLevel,
        int completionDay,
        int paidCost,
        MercenaryInstance mercenary = null)
    {
        mercenaryInstanceId = instanceId;
        this.trainingTownIndex = trainingTownIndex;
        this.startDay = startDay;
        this.targetLevel = targetLevel;
        this.completionDay = completionDay;
        this.paidCost = paidCost;
        this.mercenary = mercenary;
    }

    internal MercenaryInstance GetMercenary()
    {
        return mercenary;
    }

    internal void SetMercenary(MercenaryInstance value)
    {
        mercenary = value;
    }
}

public enum TrainingUnavailableReason
{
    None,
    MissingManagerReference,
    InvalidMercenary,
    NotHired,
    AtLevelCap,
    ContractExpired,
    Incapacitated,
    DifferentTown,
    NoFacilityInTown,
    InParty,
    OnTransport,
    OnExpedition,
    AlreadyTraining,
    SlotsFull,
    LevelLimit,
    InsufficientGold
}

public class TrainingGroundManager : MonoBehaviour
{
    public const int MaximumConcurrentTrainings = 3;

    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private TransportManager transportManager;
    [SerializeField] private DungeonExpeditionManager dungeonExpeditionManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private List<TrainingReservation> reservations =
        new List<TrainingReservation>();

    private Func<int, bool> townAvailabilityValidator =
        TownServicePolicy.IsTrainingGroundAvailable;

    public IReadOnlyList<TrainingReservation> ActiveReservations => reservations;
    public int ActiveTrainingCount => reservations.Count;
    public event Action<TrainingReservation> TrainingCompleted;
    public event Action TrainingChanged;

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToDayChanged();
    }

    private void OnDisable()
    {
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
        }
    }

    public void SetTownAvailabilityValidator(Func<int, bool> validator)
    {
        townAvailabilityValidator = validator ??
            TownServicePolicy.IsTrainingGroundAvailable;
    }

    public bool IsTownAvailable(int townIndex)
    {
        Func<int, bool> validator = townAvailabilityValidator ??
            TownServicePolicy.IsTrainingGroundAvailable;
        return validator(townIndex);
    }

    public int GetTrainingCost(MercenaryInstance mercenary)
    {
        return mercenary == null
            ? 0
            : TrainingCostService.GetCost(mercenary.Level + 1);
    }

    public bool CanStartTraining(MercenaryInstance mercenary)
    {
        return GetUnavailableReason(mercenary) == TrainingUnavailableReason.None;
    }

    public bool CanStartTraining(
        MercenaryInstance mercenary,
        out int cost)
    {
        cost = GetTrainingCost(mercenary);
        return GetUnavailableReason(mercenary) == TrainingUnavailableReason.None;
    }

    public TrainingUnavailableReason GetUnavailableReason(
        MercenaryInstance mercenary)
    {
        ResolveReferences();
        if (merchantData == null || hireManager == null)
        {
            return TrainingUnavailableReason.MissingManagerReference;
        }

        if (mercenary == null)
        {
            return TrainingUnavailableReason.InvalidMercenary;
        }

        if (!IsHired(mercenary))
        {
            return TrainingUnavailableReason.NotHired;
        }

        if (mercenary.IsAtLevelCap)
        {
            return TrainingUnavailableReason.AtLevelCap;
        }

        if (!mercenary.IsContractActive)
        {
            return TrainingUnavailableReason.ContractExpired;
        }

        if (mercenary.IsIncapacitated)
        {
            return TrainingUnavailableReason.Incapacitated;
        }

        if (!IsAtCurrentTown(mercenary))
        {
            return TrainingUnavailableReason.DifferentTown;
        }

        if (!IsTownAvailable(mercenary.CurrentTownIndex))
        {
            return TrainingUnavailableReason.NoFacilityInTown;
        }

        if (partyManager != null && partyManager.Contains(mercenary))
        {
            return TrainingUnavailableReason.InParty;
        }

        if (transportManager != null &&
            transportManager.IsMercenaryOnTransportDuty(mercenary.InstanceId))
        {
            return TrainingUnavailableReason.OnTransport;
        }

        if (dungeonExpeditionManager != null &&
            dungeonExpeditionManager.IsMercenaryOnExpeditionDuty(
                mercenary.InstanceId))
        {
            return TrainingUnavailableReason.OnExpedition;
        }

        if (IsMercenaryTraining(mercenary.InstanceId))
        {
            return TrainingUnavailableReason.AlreadyTraining;
        }

        if (reservations.Count >= MaximumConcurrentTrainings)
        {
            return TrainingUnavailableReason.SlotsFull;
        }

        if (mercenary.Level + 1 > GetMaximumTrainableLevel())
        {
            return TrainingUnavailableReason.LevelLimit;
        }

        if (!merchantData.CanPay(GetTrainingCost(mercenary)))
        {
            return TrainingUnavailableReason.InsufficientGold;
        }

        return TrainingUnavailableReason.None;
    }

    public bool TryStartTraining(MercenaryInstance mercenary)
    {
        TrainingUnavailableReason reason = GetUnavailableReason(mercenary);
        if (reason != TrainingUnavailableReason.None)
        {
            return false;
        }

        int cost = GetTrainingCost(mercenary);
        if (!merchantData.TryPayGold(cost))
        {
            return false;
        }

        int currentDay = dayManager != null ? dayManager.CurrentDay : 1;
        reservations.Add(new TrainingReservation(
            mercenary.InstanceId,
            mercenary.CurrentTownIndex,
            currentDay,
            mercenary.Level + 1,
            currentDay + 1,
            cost,
            mercenary));
        TrainingChanged?.Invoke();
        return true;
    }

    public bool IsMercenaryTraining(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return false;
        }

        foreach (TrainingReservation reservation in reservations)
        {
            if (reservation != null &&
                reservation.MercenaryInstanceId == instanceId)
            {
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<TrainingReservation> CreateReservationSnapshot()
    {
        List<TrainingReservation> snapshot =
            new List<TrainingReservation>(reservations.Count);
        foreach (TrainingReservation reservation in reservations)
        {
            if (reservation != null)
            {
                snapshot.Add(new TrainingReservation(
                    reservation.MercenaryInstanceId,
                    reservation.TrainingTownIndex,
                    reservation.StartDay,
                    reservation.TargetLevel,
                    reservation.CompletionDay,
                    reservation.PaidCost));
            }
        }

        return snapshot;
    }

    public void RestoreReservations(
        IEnumerable<TrainingReservation> restoredReservations)
    {
        ResolveReferences();
        reservations.Clear();
        if (restoredReservations != null)
        {
            foreach (TrainingReservation reservation in restoredReservations)
            {
                if (reservation != null &&
                    !string.IsNullOrWhiteSpace(reservation.MercenaryInstanceId) &&
                    WorldMapService.IsValidTownIndex(
                        reservation.TrainingTownIndex) &&
                    IsTownAvailable(reservation.TrainingTownIndex) &&
                    reservations.Count < MaximumConcurrentTrainings &&
                    !IsMercenaryTraining(reservation.MercenaryInstanceId))
                {
                    reservations.Add(new TrainingReservation(
                        reservation.MercenaryInstanceId,
                        reservation.TrainingTownIndex,
                        reservation.StartDay,
                        reservation.TargetLevel,
                        reservation.CompletionDay,
                        reservation.PaidCost,
                        FindHiredMercenary(reservation.MercenaryInstanceId)));
                }
            }
        }

        TrainingChanged?.Invoke();
    }

    public List<SavedTrainingAssignment> CreateSaveData()
    {
        List<SavedTrainingAssignment> saved =
            new List<SavedTrainingAssignment>(reservations.Count);
        foreach (TrainingReservation reservation in reservations)
        {
            if (reservation != null)
            {
                saved.Add(new SavedTrainingAssignment
                {
                    mercenaryInstanceId = reservation.MercenaryInstanceId,
                    trainingTownIndex = reservation.TrainingTownIndex,
                    startDay = reservation.StartDay,
                    completionDay = reservation.CompletionDay,
                    targetLevel = reservation.TargetLevel,
                    paidCost = reservation.PaidCost
                });
            }
        }

        return saved;
    }

    public void Restore(
        IEnumerable<SavedTrainingAssignment> savedAssignments)
    {
        ResolveReferences();
        reservations.Clear();
        HashSet<string> restoredIds = new HashSet<string>();
        if (savedAssignments != null)
        {
            foreach (SavedTrainingAssignment saved in savedAssignments)
            {
                if (!TryRestoreAssignment(saved, restoredIds))
                {
                    continue;
                }
            }
        }

        CompleteDueReservations(dayManager != null ? dayManager.CurrentDay : 1);
        TrainingChanged?.Invoke();
    }

    private void HandleDayChanged(int currentDay)
    {
        CompleteDueReservations(currentDay);
    }

    private void CompleteDueReservations(int currentDay)
    {
        List<TrainingReservation> completed =
            new List<TrainingReservation>();
        foreach (TrainingReservation reservation in reservations)
        {
            if (reservation != null && currentDay >= reservation.CompletionDay)
            {
                completed.Add(reservation);
            }
        }

        if (completed.Count == 0)
        {
            return;
        }

        foreach (TrainingReservation reservation in completed)
        {
            reservations.Remove(reservation);
        }

        foreach (TrainingReservation reservation in completed)
        {
            MercenaryInstance mercenary = reservation.GetMercenary() ??
                FindHiredMercenary(reservation.MercenaryInstanceId);
            if (mercenary != null)
            {
                int missingExperience = mercenary.ExperienceToNextLevel -
                    mercenary.CurrentExperience;
                mercenary.AddExperience(missingExperience);
            }

            TrainingCompleted?.Invoke(reservation);
        }

        TrainingChanged?.Invoke();
    }

    private bool TryRestoreAssignment(
        SavedTrainingAssignment saved,
        HashSet<string> restoredIds)
    {
        if (saved == null ||
            string.IsNullOrWhiteSpace(saved.mercenaryInstanceId) ||
            !restoredIds.Add(saved.mercenaryInstanceId) ||
            saved.startDay <= 0 ||
            saved.completionDay != saved.startDay + 1 ||
            saved.targetLevel <= 1 ||
            saved.paidCost != TrainingCostService.GetCost(saved.targetLevel) ||
            !WorldMapService.IsValidTownIndex(saved.trainingTownIndex) ||
            !IsTownAvailable(saved.trainingTownIndex))
        {
            Debug.LogWarning("Discarded invalid saved training assignment.");
            return false;
        }

        MercenaryInstance mercenary = FindHiredMercenary(
            saved.mercenaryInstanceId);
        if (mercenary == null ||
            mercenary.CurrentTownIndex != saved.trainingTownIndex ||
            mercenary.IsAtLevelCap ||
            mercenary.Level + 1 != saved.targetLevel ||
            saved.targetLevel > mercenary.LevelCap ||
            reservations.Count >= MaximumConcurrentTrainings)
        {
            Debug.LogWarning("Discarded unresolved saved training assignment.");
            return false;
        }

        reservations.Add(new TrainingReservation(
            saved.mercenaryInstanceId,
            saved.trainingTownIndex,
            saved.startDay,
            saved.targetLevel,
            saved.completionDay,
            saved.paidCost,
            mercenary));
        return true;
    }

    public int GetMaximumTrainableLevel()
    {
        ResolveReferences();
        int highestLevel = 0;
        if (hireManager == null)
        {
            return -2;
        }

        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null)
            {
                highestLevel = Mathf.Max(highestLevel, mercenary.Level);
            }
        }

        return highestLevel - 2;
    }

    private bool IsHired(MercenaryInstance mercenary)
    {
        return mercenary != null &&
               FindHiredMercenary(mercenary.InstanceId) == mercenary;
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

    private bool IsAtCurrentTown(MercenaryInstance mercenary)
    {
        return townProgressState == null ||
               mercenary.CurrentTownIndex == townProgressState.CurrentTownIndex;
    }

    private void SubscribeToDayChanged()
    {
        if (dayManager == null)
        {
            return;
        }

        dayManager.DayChanged -= HandleDayChanged;
        dayManager.DayChanged += HandleDayChanged;
    }

    private void ResolveReferences()
    {
        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>() ??
                FindObjectOfType<MerchantData>();
        }

        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>() ??
                FindObjectOfType<MercenaryHireManager>();
        }

        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>() ??
                FindObjectOfType<MercenaryPartyManager>();
        }

        if (transportManager == null)
        {
            transportManager = GetComponent<TransportManager>() ??
                FindObjectOfType<TransportManager>();
        }

        if (dungeonExpeditionManager == null)
        {
            dungeonExpeditionManager =
                GetComponent<DungeonExpeditionManager>() ??
                FindObjectOfType<DungeonExpeditionManager>();
        }

        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>() ??
                FindObjectOfType<DayManager>();
        }

        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>() ??
                FindObjectOfType<TownProgressState>();
        }

        SubscribeToDayChanged();
    }
}
