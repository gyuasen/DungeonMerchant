using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryHireManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private TrainingGroundManager trainingGroundManager;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private RoadCargoSession roadCargoSession;
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
    public event Action<MercenaryInstance> MercenaryDismissed;
    public event Action ContractsChanged;

    public enum ContractChangeUnavailableReason
    {
        None,
        SameOrLower,
        ContractLocked,
        InTraining,
        InTransit,
        InsufficientGold,
        AtMaxContract,
        NotHired
    }

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

        MercenaryContractType contractType = merchantData.IsContractUnlocked(
            selectedContract)
            ? selectedContract
            : MercenaryContractType.Local;
        int initialContractCost = GetInitialContractCost(mercenary, contractType);
        if (!merchantData.CanPay(initialContractCost))
        {
            Debug.Log($"Not enough gold to hire {mercenary.MercenaryName}.");
            return false;
        }
        if (UnityEngine.Random.value > GetSelectedContractSuccessRate())
        {
            return false;
        }
        if (!merchantData.TryPayGold(
                initialContractCost,
                GoldTransactionReason.MercenaryHire,
                mercenary.MercenaryName))
        {
            return false;
        }
        mercenary.SetContract(
            contractType,
            dayManager != null ? dayManager.CurrentDay : 1);
        if (townProgressState != null)
        {
            mercenary.SetCurrentTownIndex(townProgressState.CurrentTownIndex);
        }

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
            !merchantData.TryPayGold(
                GetRenewalCost(mercenary),
                GoldTransactionReason.ContractRenewal,
                mercenary.MercenaryName))
        {
            return false;
        }

        mercenary.RenewContract(
            dayManager != null ? dayManager.CurrentDay : 1);
        ContractsChanged?.Invoke();
        return true;
    }

    public bool TryReleaseMercenary(MercenaryInstance mercenary)
    {
        ResolveReferences();
        if (mercenary == null ||
            (trainingGroundManager != null &&
             trainingGroundManager.IsMercenaryTraining(mercenary.InstanceId)) ||
            (roadCargoSession != null &&
             roadCargoSession.IsCompanionInTransit(mercenary.InstanceId)) ||
            !hiredMercenaries.Contains(mercenary) ||
            !TryReturnEquippedEquipment(mercenary))
        {
            return false;
        }

        hiredMercenaries.Remove(mercenary);

        MercenaryDismissed?.Invoke(mercenary);
        ContractsChanged?.Invoke();
        return true;
    }

    public int GetRenewalCost(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return 0;
        }

        if (mercenary.ContractType == MercenaryContractType.Exclusive)
        {
            return 0;
        }

        ResolveReferences();
        float multiplier = merchantData != null
            ? merchantData.GetRenewalCostMultiplier()
            : 1f;
        return MercenaryContractRules.CalculateRenewalCost(
            mercenary.HireCost,
            mercenary.ContractType,
            multiplier);
    }

    public int GetRenewalCost(
        MercenaryDataSO mercenary,
        MercenaryContractType contractType)
    {
        if (mercenary == null)
        {
            return 0;
        }

        ResolveReferences();
        float multiplier = merchantData != null
            ? merchantData.GetRenewalCostMultiplier()
            : 1f;
        return MercenaryContractRules.CalculateRenewalCost(
            mercenary.hireCost,
            contractType,
            multiplier);
    }

    public int GetRenewalCost(
        MercenaryInstance mercenary,
        MercenaryContractType contractType)
    {
        if (mercenary == null)
        {
            return 0;
        }

        ResolveReferences();
        float multiplier = merchantData != null
            ? merchantData.GetRenewalCostMultiplier()
            : 1f;
        return MercenaryContractRules.CalculateRenewalCost(
            mercenary.HireCost,
            contractType,
            multiplier);
    }

    public int GetInitialContractCost(
        MercenaryDataSO mercenary,
        MercenaryContractType contractType)
    {
        return mercenary == null
            ? 0
            : MercenaryContractRules.CalculateInitialCost(
                mercenary.hireCost,
                contractType);
    }

    public int GetInitialContractCost(
        MercenaryInstance mercenary,
        MercenaryContractType contractType)
    {
        return mercenary == null
            ? 0
            : MercenaryContractRules.CalculateInitialCost(
                mercenary.HireCost,
                contractType);
    }

    public bool TryChangeContract(
        MercenaryInstance mercenary,
        MercenaryContractType newContractType)
    {
        if (!CanChangeContract(mercenary, newContractType))
        {
            return false;
        }

        int cost = GetInitialContractCost(mercenary, newContractType);
        if (!merchantData.TryPayGold(
                cost,
                GoldTransactionReason.ContractChange,
                mercenary.MercenaryName))
        {
            return false;
        }

        mercenary.SetContract(
            newContractType,
            dayManager != null ? dayManager.CurrentDay : 1);
        ContractsChanged?.Invoke();
        return true;
    }

    public bool CanChangeContract(
        MercenaryInstance mercenary,
        MercenaryContractType newContractType)
    {
        return GetContractChangeUnavailableReason(
            mercenary,
            newContractType) == ContractChangeUnavailableReason.None;
    }

    public ContractChangeUnavailableReason GetContractChangeUnavailableReason(
        MercenaryInstance mercenary,
        MercenaryContractType newContractType)
    {
        ResolveReferences();
        if (mercenary == null ||
            merchantData == null ||
            !hiredMercenaries.Contains(mercenary))
        {
            return ContractChangeUnavailableReason.NotHired;
        }
        if (mercenary.ContractType == MercenaryContractType.Exclusive)
        {
            return ContractChangeUnavailableReason.AtMaxContract;
        }
        if (!MercenaryContractRules.IsUpgrade(
                mercenary.ContractType,
                newContractType))
        {
            return ContractChangeUnavailableReason.SameOrLower;
        }
        if (!merchantData.IsContractUnlocked(newContractType))
        {
            return ContractChangeUnavailableReason.ContractLocked;
        }
        if (trainingGroundManager != null &&
            trainingGroundManager.IsMercenaryTraining(mercenary.InstanceId))
        {
            return ContractChangeUnavailableReason.InTraining;
        }
        if (roadCargoSession != null &&
            roadCargoSession.IsCompanionInTransit(mercenary.InstanceId))
        {
            return ContractChangeUnavailableReason.InTransit;
        }
        return merchantData.CanPay(GetInitialContractCost(
            mercenary,
            newContractType))
            ? ContractChangeUnavailableReason.None
            : ContractChangeUnavailableReason.InsufficientGold;
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

    public float GetSelectedContractSuccessRate()
    {
        ResolveReferences();
        return selectedContract == MercenaryContractType.Exclusive &&
               merchantData != null
            ? merchantData.GetHireSuccessRate()
            : 1f;
    }

    private void HandleDayChanged(int currentDay)
    {
        foreach (MercenaryInstance mercenary in hiredMercenaries)
        {
            if (mercenary == null)
            {
                continue;
            }

            mercenary.UpdateContractForDay(currentDay);
            if ((mercenary.ContractType == MercenaryContractType.Local ||
                 mercenary.ContractType == MercenaryContractType.Temporary) &&
                mercenary.ContractNeedsRenewal &&
                merchantData != null &&
                merchantData.TryPayGold(
                    GetRenewalCost(mercenary),
                    GoldTransactionReason.ContractRenewal,
                    mercenary.MercenaryName,
                    currentDay - 1))
            {
                mercenary.RenewContract(currentDay);
            }
        }
        ContractsChanged?.Invoke();
    }

    public bool CanAfford(MercenaryDataSO mercenary)
    {
        ResolveReferences();

        return merchantData != null &&
               mercenary != null &&
               mercenary.hireCost >= 0 &&
               merchantData.CanPay(GetInitialContractCost(
                   mercenary,
                   selectedContract));
    }

    public bool CanAfford(MercenaryInstance mercenary)
    {
        ResolveReferences();

        return merchantData != null &&
               mercenary != null &&
               mercenary.HireCost >= 0 &&
               merchantData.CanPay(GetInitialContractCost(
                   mercenary,
                   selectedContract));
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

        if (townProgressState == null)
        {
            townProgressState = GetComponent<TownProgressState>() ??
                                FindObjectOfType<TownProgressState>();
        }

        if (trainingGroundManager == null)
        {
            trainingGroundManager = GetComponent<TrainingGroundManager>() ??
                                  FindObjectOfType<TrainingGroundManager>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = GetComponent<MerchantInventory>() ??
                                FindObjectOfType<MerchantInventory>();
        }

        if (roadCargoSession == null)
        {
            roadCargoSession = GetComponent<RoadCargoSession>() ??
                               FindObjectOfType<RoadCargoSession>();
        }
    }

    private bool TryReturnEquippedEquipment(MercenaryInstance mercenary)
    {
        EquipmentSlot[] slots =
        {
            EquipmentSlot.Weapon,
            EquipmentSlot.Armor,
            EquipmentSlot.Accessory
        };
        List<EquipmentInstance> equipmentToReturn =
            new List<EquipmentInstance>();
        foreach (EquipmentSlot slot in slots)
        {
            ItemDataSO item = mercenary.GetEquippedItem(slot);
            if (item == null)
            {
                continue;
            }

            EquipmentInstance equipment = mercenary.GetEquippedInstance(slot) ??
                EquipmentInstance.CreateFixed(item);
            if (equipment.BaseItem == null)
            {
                Debug.LogWarning(
                    "Could not return equipped item while releasing " +
                    mercenary.MercenaryName + ".");
                return false;
            }

            equipmentToReturn.Add(equipment);
        }

        if (equipmentToReturn.Count == 0)
        {
            return true;
        }

        if (merchantInventory == null)
        {
            Debug.LogWarning(
                "Could not release " + mercenary.MercenaryName +
                " because equipped items cannot be returned without inventory.");
            return false;
        }

        try
        {
            foreach (EquipmentInstance equipment in equipmentToReturn)
            {
                merchantInventory.DepositEquipmentTo(
                    mercenary.CurrentTownIndex,
                    equipment);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Could not return equipped items while releasing " +
                mercenary.MercenaryName + ": " + exception.Message);
            return false;
        }

        foreach (EquipmentSlot slot in slots)
        {
            mercenary.UnequipEquipment(slot);
        }

        return true;
    }
}
