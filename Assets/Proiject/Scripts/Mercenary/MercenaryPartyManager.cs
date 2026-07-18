using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryPartyManager : MonoBehaviour
{
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private TransportManager transportManager;
    [SerializeField] private DungeonExpeditionManager dungeonExpeditionManager;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField, Min(1)] private int maxPartySize = 3;
    [SerializeField] private List<MercenaryInstance> members = new List<MercenaryInstance>();

    public int MaxPartySize => maxPartySize;
    public IReadOnlyList<MercenaryInstance> Members => members;
    public bool IsFull => members.Count >= maxPartySize;

    public event Action PartyChanged;

    private void OnEnable()
    {
        ResolveReferences();
        if (hireManager != null)
        {
            hireManager.ContractsChanged += RemoveInactiveContracts;
            hireManager.MercenaryDismissed += RemoveReleasedMercenary;
        }
    }

    private void OnDisable()
    {
        if (hireManager != null)
        {
            hireManager.ContractsChanged -= RemoveInactiveContracts;
            hireManager.MercenaryDismissed -= RemoveReleasedMercenary;
        }
    }

    public bool Contains(MercenaryInstance mercenary)
    {
        return mercenary != null && members.Contains(mercenary);
    }

    public bool TryAdd(MercenaryInstance mercenary)
    {
        if (mercenary == null ||
            !mercenary.IsContractActive ||
            !IsHired(mercenary) ||
            !IsAtCurrentTown(mercenary) ||
            (transportManager != null &&
             transportManager.IsMercenaryOnTransportDuty(mercenary.InstanceId)) ||
            (dungeonExpeditionManager != null &&
             dungeonExpeditionManager.IsMercenaryOnExpeditionDuty(mercenary.InstanceId)) ||
            Contains(mercenary) ||
            IsFull)
        {
            return false;
        }

        members.Add(mercenary);
        PartyChanged?.Invoke();
        return true;
    }

    public bool Remove(MercenaryInstance mercenary)
    {
        if (mercenary == null || !members.Remove(mercenary))
        {
            return false;
        }

        PartyChanged?.Invoke();
        return true;
    }

    public void RestoreParty(IEnumerable<MercenaryInstance> restoredMembers)
    {
        members.Clear();
        if (restoredMembers != null)
        {
            foreach (MercenaryInstance mercenary in restoredMembers)
            {
                if (mercenary != null &&
                    mercenary.IsContractActive &&
                    members.Count < maxPartySize &&
                    IsHired(mercenary))
                {
                    members.Add(mercenary);
                }
            }
        }

        PartyChanged?.Invoke();
    }

    private bool IsHired(MercenaryInstance mercenary)
    {
        ResolveReferences();

        if (hireManager == null)
        {
            return false;
        }

        if (transportManager == null)
        {
            transportManager = GetComponent<TransportManager>() ??
                               FindObjectOfType<TransportManager>();
        }
        if (dungeonExpeditionManager == null)
        {
            dungeonExpeditionManager = GetComponent<DungeonExpeditionManager>() ??
                                       FindObjectOfType<DungeonExpeditionManager>();
        }

        foreach (MercenaryInstance hiredMercenary in hireManager.HiredMercenaries)
        {
            if (ReferenceEquals(hiredMercenary, mercenary))
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateMemberLocations(int townIndex)
    {
        foreach (MercenaryInstance member in members)
        {
            member?.SetCurrentTownIndex(townIndex);
        }
    }

    private bool IsAtCurrentTown(MercenaryInstance mercenary)
    {
        ResolveReferences();
        return townProgressState == null ||
               mercenary.CurrentTownIndex == townProgressState.CurrentTownIndex;
    }

    private void RemoveInactiveContracts()
    {
        if (members.RemoveAll(member =>
                member == null || !member.IsContractActive) > 0)
        {
            PartyChanged?.Invoke();
        }
    }

    private void RemoveReleasedMercenary(MercenaryInstance mercenary)
    {
        Remove(mercenary);
    }

    private void ResolveReferences()
    {
        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>();
        }

        if (hireManager == null)
        {
            hireManager = FindObjectOfType<MercenaryHireManager>();
        }

        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>() ??
                                FindObjectOfType<TownProgressState>();
        }
    }
}
