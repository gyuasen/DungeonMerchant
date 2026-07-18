using System;
using System.Collections.Generic;
using UnityEngine;

public class HealingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private TownProgressState townProgressState;

    [Header("Healing Settings")]
    [SerializeField, Min(0)] private int naturalHealPerDay = 10;

    public int HealCostPerHP => HealingCostService.LightInjuryRate;
    public int IncapacitatedCostMultiplier => 1;
    public int RevivalBaseCost => HealingCostService.RevivalCost;
    public int NaturalHealPerDay => naturalHealPerDay;

    public event Action HealingChanged;

    private void OnEnable()
    {
        ResolveReferences();
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
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

    public int GetMissingHP(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return 0;
        }

        return Mathf.Max(0, mercenary.MaxHP - mercenary.CurrentHP);
    }

    public int GetFullHealCost(MercenaryInstance mercenary)
    {
        return GetFullHealCostBreakdown(mercenary).TotalCost;
    }

    public HealingCostBreakdown GetFullHealCostBreakdown(
        MercenaryInstance mercenary)
    {
        return mercenary == null
            ? HealingCostService.CalculateFullHealCost(0, 0, false)
            : HealingCostService.CalculateFullHealCost(
                mercenary.MaxHP,
                mercenary.CurrentHP,
                mercenary.IsIncapacitated);
    }

    public bool CanHeal(MercenaryInstance mercenary)
    {
        ResolveReferences();

        int cost = GetFullHealCost(mercenary);
        return merchantData != null &&
               mercenary != null &&
               IsAtCurrentTown(mercenary) &&
               cost > 0 &&
               merchantData.CanPay(cost);
    }

    public bool TryHealFull(MercenaryInstance mercenary)
    {
        ResolveReferences();

        if (merchantData == null || mercenary == null)
        {
            return false;
        }

        if (!IsAtCurrentTown(mercenary))
        {
            return false;
        }

        int cost = GetFullHealCost(mercenary);
        if (cost <= 0)
        {
            return false;
        }

        if (!merchantData.TryPayGold(cost))
        {
            return false;
        }

        mercenary.RestoreFullHP();
        Debug.Log($"Healed {mercenary.MercenaryName} for {cost} G.");
        HealingChanged?.Invoke();
        return true;
    }

    public IEnumerable<MercenaryInstance> GetMercenariesAtCurrentTown()
    {
        ResolveReferences();
        if (hireManager == null)
        {
            yield break;
        }

        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && IsAtCurrentTown(mercenary))
            {
                yield return mercenary;
            }
        }
    }

    private bool IsAtCurrentTown(MercenaryInstance mercenary)
    {
        return townProgressState == null ||
               mercenary.CurrentTownIndex == townProgressState.CurrentTownIndex;
    }

    private void HandleDayChanged(int currentDay)
    {
        ResolveReferences();

        if (hireManager == null || naturalHealPerDay <= 0)
        {
            return;
        }

        bool healedAny = false;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary == null ||
                mercenary.IsIncapacitated ||
                mercenary.CurrentHP >= mercenary.MaxHP)
            {
                continue;
            }

            mercenary.Heal(naturalHealPerDay);
            healedAny = true;
        }

        if (healedAny)
        {
            Debug.Log($"Mercenaries recovered {naturalHealPerDay} HP after resting.");
            HealingChanged?.Invoke();
        }
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

        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>();
        }

        if (hireManager == null)
        {
            hireManager = FindObjectOfType<MercenaryHireManager>();
        }

        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>();
        }

        if (dayManager == null)
        {
            dayManager = FindObjectOfType<DayManager>();
        }

        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>() ??
                                FindObjectOfType<TownProgressState>();
        }
    }
}
