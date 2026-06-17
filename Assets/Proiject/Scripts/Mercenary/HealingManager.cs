using System;
using UnityEngine;

public class HealingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private DayManager dayManager;

    [Header("Healing Settings")]
    [SerializeField, Min(0)] private int healCostPerHP = 2;
    [SerializeField, Min(0)] private int naturalHealPerDay = 10;

    public int HealCostPerHP => healCostPerHP;
    public int NaturalHealPerDay => naturalHealPerDay;

    public event Action HealingChanged;

    private void OnEnable()
    {
        ResolveReferences();
        if (dayManager != null)
        {
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
        return GetMissingHP(mercenary) * healCostPerHP;
    }

    public bool CanHeal(MercenaryInstance mercenary)
    {
        ResolveReferences();

        int cost = GetFullHealCost(mercenary);
        return merchantData != null &&
               mercenary != null &&
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
            if (mercenary == null || mercenary.CurrentHP >= mercenary.MaxHP)
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
    }
}
