using System;
using UnityEngine;

public class MarketPriceManager : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField, Range(0.1f, 1f)] private float minimumSellMultiplier = 0.75f;
    [SerializeField, Range(1f, 3f)] private float maximumSellMultiplier = 1.35f;

    public event Action PricesChanged;

    public int CurrentDay => dayManager != null ? dayManager.CurrentDay : 1;

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

    public int GetSellPrice(ItemDataSO item)
    {
        if (item == null)
        {
            return 0;
        }

        return Mathf.Max(0, Mathf.RoundToInt(item.basePrice * GetSellMultiplier(item)));
    }

    public float GetSellMultiplier(ItemDataSO item)
    {
        if (item == null)
        {
            return 1f;
        }

        int hash = CalculateStableHash(item);
        float normalized = (hash & 0x7fffffff) / (float)int.MaxValue;
        return Mathf.Lerp(minimumSellMultiplier, maximumSellMultiplier, normalized);
    }

    public string GetMarketSummary()
    {
        return $"DAY {CurrentDay}";
    }

    private int CalculateStableHash(ItemDataSO item)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + CurrentDay;
            hash = hash * 31 + (int)item.itemType;
            hash = hash * 31 + (int)item.rarity;

            string key = string.IsNullOrWhiteSpace(item.itemName)
                ? item.name
                : item.itemName;

            foreach (char character in key)
            {
                hash = hash * 31 + character;
            }

            return hash;
        }
    }

    private void HandleDayChanged(int day)
    {
        PricesChanged?.Invoke();
    }

    private void ResolveReferences()
    {
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
