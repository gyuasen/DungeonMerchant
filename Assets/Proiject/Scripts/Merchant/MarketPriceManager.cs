using System;
using UnityEngine;

public class MarketPriceManager : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MerchantData merchantData;
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
        if (merchantData != null)
        {
            merchantData.ProgressionChanged += HandleMerchantProgressionChanged;
        }
    }

    private void OnDisable()
    {
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
        }
        if (merchantData != null)
        {
            merchantData.ProgressionChanged -= HandleMerchantProgressionChanged;
        }
    }

    public int GetSellPrice(ItemDataSO item)
    {
        if (item == null)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                item.basePrice * GetEffectiveSellMultiplier(item)));
    }

    public float GetSellMultiplier(ItemDataSO item)
    {
        if (item == null)
        {
            return 1f;
        }

        int hash = MarketHashUtility.ComputeItemHash(17, CurrentDay, item);
        float normalized = (hash & 0x7fffffff) / (float)int.MaxValue;
        return Mathf.Lerp(minimumSellMultiplier, maximumSellMultiplier, normalized);
    }

    public float GetEffectiveSellMultiplier(ItemDataSO item)
    {
        float merchantMultiplier = merchantData != null
            ? merchantData.GetMarketSellMultiplier()
            : 1f;
        return GetSellMultiplier(item) * merchantMultiplier;
    }

    public string GetMarketSummary()
    {
        return $"{CurrentDay}日目";
    }

    private void HandleDayChanged(int day)
    {
        PricesChanged?.Invoke();
    }

    private void HandleMerchantProgressionChanged()
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

        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>() ??
                           FindObjectOfType<MerchantData>();
        }
    }
}
