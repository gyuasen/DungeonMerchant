using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryPartyManager : MonoBehaviour
{
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField, Min(1)] private int maxPartySize = 3;
    [SerializeField] private List<MercenaryInstance> members = new List<MercenaryInstance>();

    public int MaxPartySize => maxPartySize;
    public IReadOnlyList<MercenaryInstance> Members => members;
    public bool IsFull => members.Count >= maxPartySize;

    public event Action PartyChanged;

    public bool Contains(MercenaryInstance mercenary)
    {
        return mercenary != null && members.Contains(mercenary);
    }

    public bool TryAdd(MercenaryInstance mercenary)
    {
        if (mercenary == null || !IsHired(mercenary) || Contains(mercenary) || IsFull)
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

    private bool IsHired(MercenaryInstance mercenary)
    {
        if (hireManager == null)
        {
            return false;
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
}
