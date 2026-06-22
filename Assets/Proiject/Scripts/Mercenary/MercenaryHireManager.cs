using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryHireManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MercenaryContractType selectedContract =
        MercenaryContractType.Local;

    [Header("Hire Target")]
    [SerializeField] private MercenaryDataSO targetMercenary;

    [Header("Hired Mercenaries")]
    [SerializeField] private List<MercenaryInstance> hiredMercenaries =
        new List<MercenaryInstance>();

    public IReadOnlyList<MercenaryInstance> HiredMercenaries => hiredMercenaries;
    public MercenaryContractType SelectedContract => selectedContract;

    public event Action<MercenaryInstance> MercenaryHired;
    public event Action ContractsChanged;

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

        MercenaryContractType contractType = merchantData.IsContractUnlocked(
            selectedContract)
            ? selectedContract
            : MercenaryContractType.Local;
        if (UnityEngine.Random.value > merchantData.GetHireSuccessRate())
        {
            merchantData.AddGold(mercenary.HireCost);
            return false;
        }
        mercenary.SetContract(
            contractType,
            dayManager != null ? dayManager.CurrentDay : 1);

        hiredMercenaries.Add(mercenary);
        MercenaryHired?.Invoke(mercenary);

        Debug.Log(
            $"Hired {mercenary.MercenaryName}. " +
            $"Company mercenaries: {hiredMercenaries.Count}");
        return true;
    }

    public bool TryRenewContract(MercenaryInstance mercenary)
    {
        ResolveReferences();
        if (mercenary == null ||
            !hiredMercenaries.Contains(mercenary) ||
            !mercenary.ContractNeedsRenewal ||
            !merchantData.TryPayGold(GetRenewalCost(mercenary)))
        {
            return false;
        }

        mercenary.RenewContract(
            dayManager != null ? dayManager.CurrentDay : 1);
        ContractsChanged?.Invoke();
        return true;
    }

    public int GetRenewalCost(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return 0;
        }

        ResolveReferences();
        float multiplier = merchantData != null
            ? merchantData.GetRenewalCostMultiplier()
            : 1f;
        return Mathf.Max(
            1,
            Mathf.RoundToInt(mercenary.GetRenewalCost() * multiplier));
    }

    private MercenaryContractType GetBestUnlockedContract()
    {
        if (merchantData.IsContractUnlocked(MercenaryContractType.Exclusive))
        {
            return MercenaryContractType.Exclusive;
        }
        if (merchantData.IsContractUnlocked(MercenaryContractType.Temporary))
        {
            return MercenaryContractType.Temporary;
        }
        return MercenaryContractType.Local;
    }

    public MercenaryContractType CycleSelectedContract()
    {
        MercenaryContractType[] order =
        {
            MercenaryContractType.Local,
            MercenaryContractType.Temporary,
            MercenaryContractType.Exclusive
        };
        int currentIndex = Array.IndexOf(order, selectedContract);
        for (int offset = 1; offset <= order.Length; offset++)
        {
            MercenaryContractType candidate =
                order[(currentIndex + offset) % order.Length];
            if (merchantData.IsContractUnlocked(candidate))
            {
                selectedContract = candidate;
                return selectedContract;
            }
        }
        selectedContract = MercenaryContractType.Local;
        return selectedContract;
    }

    private void HandleDayChanged(int currentDay)
    {
        foreach (MercenaryInstance mercenary in hiredMercenaries)
        {
            mercenary?.UpdateContractForDay(currentDay);
        }
        ContractsChanged?.Invoke();
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

    public void RestoreHiredMercenaries(
        IEnumerable<MercenaryInstance> restoredMercenaries)
    {
        hiredMercenaries.Clear();
        if (restoredMercenaries == null)
        {
            return;
        }

        foreach (MercenaryInstance mercenary in restoredMercenaries)
        {
            if (mercenary != null)
            {
                hiredMercenaries.Add(mercenary);
                MercenaryHired?.Invoke(mercenary);
            }
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

        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>() ??
                         FindObjectOfType<DayManager>();
        }
    }
}
