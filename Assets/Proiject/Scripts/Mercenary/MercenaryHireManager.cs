using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryHireManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;

    [Header("Hire Target")]
    [SerializeField] private MercenaryDataSO targetMercenary;

    [Header("Hired Mercenaries")]
    [SerializeField] private List<MercenaryInstance> hiredMercenaries =
        new List<MercenaryInstance>();

    public IReadOnlyList<MercenaryInstance> HiredMercenaries => hiredMercenaries;

    public event Action<MercenaryInstance> MercenaryHired;

    public void HireMercenary()
    {
        TryHireMercenary(targetMercenary);
    }

    public bool TryHireMercenary(MercenaryDataSO mercenary)
    {
        return TryHireMercenary(mercenary, out _);
    }

    public bool TryHireMercenary(
        MercenaryDataSO mercenary,
        out MercenaryInstance hiredMercenary)
    {
        if (mercenary == null)
        {
            Debug.LogError("No mercenary is selected for hire.");
            hiredMercenary = null;
            return false;
        }

        hiredMercenary = new MercenaryInstance(mercenary);
        if (TryHireMercenary(hiredMercenary))
        {
            return true;
        }

        hiredMercenary = null;
        return false;
    }

    public bool TryHireMercenary(MercenaryInstance mercenary)
    {
        ResolveReferences();

        if (merchantData == null)
        {
            Debug.LogError("MerchantData is not assigned.");
            return false;
        }

        if (mercenary == null)
        {
            Debug.LogError("No mercenary is selected for hire.");
            return false;
        }

        if (hiredMercenaries.Contains(mercenary))
        {
            Debug.LogError($"{mercenary.MercenaryName} is already hired.");
            return false;
        }

        if (mercenary.HireCost < 0)
        {
            Debug.LogError($"{mercenary.MercenaryName} has an invalid hire cost.");
            return false;
        }

        if (!merchantData.TryPayGold(mercenary.HireCost))
        {
            Debug.Log($"Not enough gold to hire {mercenary.MercenaryName}.");
            return false;
        }

        hiredMercenaries.Add(mercenary);
        MercenaryHired?.Invoke(mercenary);

        Debug.Log(
            $"Hired {mercenary.MercenaryName}. " +
            $"Company mercenaries: {hiredMercenaries.Count}");
        return true;
    }

    public bool CanAfford(MercenaryDataSO mercenary)
    {
        ResolveReferences();

        return merchantData != null &&
               mercenary != null &&
               mercenary.hireCost >= 0 &&
               merchantData.CanPay(mercenary.hireCost);
    }

    public bool CanAfford(MercenaryInstance mercenary)
    {
        ResolveReferences();

        return merchantData != null &&
               mercenary != null &&
               mercenary.HireCost >= 0 &&
               merchantData.CanPay(mercenary.HireCost);
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
    }
}
