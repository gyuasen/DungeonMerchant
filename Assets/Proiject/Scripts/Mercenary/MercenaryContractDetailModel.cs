using System.Collections.Generic;

/// <summary>
/// Pure, UI-agnostic view model for the hire-screen contract comparison
/// overlay. Builds one column per contract type for a single candidate,
/// exposing the resolved real amounts (initial cost, renewal cost) and the
/// descriptive text (term, renewal method, unlock status) together with the
/// "selected" and "unlocked" flags the overlay uses for emphasis.
/// Keeping this logic out of the MonoBehaviour UI makes it unit-testable and
/// avoids re-implementing the /2 . /3 renewal math in the presentation layer.
/// </summary>
public readonly struct MercenaryContractDetailColumn
{
    public MercenaryContractDetailColumn(
        MercenaryContractType contractType,
        bool isSelected,
        bool isUnlocked,
        int initialCost,
        int renewalCost)
    {
        ContractType = contractType;
        IsSelected = isSelected;
        IsUnlocked = isUnlocked;
        InitialCost = initialCost;
        RenewalCost = renewalCost;
    }

    public MercenaryContractType ContractType { get; }
    public bool IsSelected { get; }
    public bool IsUnlocked { get; }
    public int InitialCost { get; }
    public int RenewalCost { get; }

    public int RequiredMerchantLevel =>
        MercenaryContractRules.GetRequiredMerchantLevel(ContractType);

    public string ContractName =>
        JapaneseDisplayText.GetContractType(ContractType);

    public string TermText =>
        MercenaryContractDetailModel.GetTermText(ContractType);

    public string RenewalMethodText =>
        MercenaryContractDetailModel.GetRenewalMethodText(ContractType);

    public string RenewalCostText =>
        RenewalCost > 0 ? RenewalCost + " G" : "更新なし";

    public string UnlockStatusText =>
        IsUnlocked
            ? "解放済み"
            : "未解放\n商人Lv" + RequiredMerchantLevel + "で解放";
}

public static class MercenaryContractDetailModel
{
    public static readonly MercenaryContractType[] DisplayOrder =
    {
        MercenaryContractType.Local,
        MercenaryContractType.Temporary,
        MercenaryContractType.Exclusive
    };

    /// <summary>
    /// Builds the three comparison columns for a candidate. The caller
    /// supplies the resolved real amounts through the two selectors so the
    /// existing GetInitialContractCost / GetRenewalCost APIs stay the single
    /// source of truth for money.
    /// </summary>
    public static IReadOnlyList<MercenaryContractDetailColumn> BuildColumns(
        MercenaryContractType selectedContract,
        System.Func<MercenaryContractType, bool> isContractUnlocked,
        System.Func<MercenaryContractType, int> getInitialCost,
        System.Func<MercenaryContractType, int> getRenewalCost)
    {
        List<MercenaryContractDetailColumn> columns =
            new List<MercenaryContractDetailColumn>(DisplayOrder.Length);
        foreach (MercenaryContractType contractType in DisplayOrder)
        {
            bool isUnlocked = isContractUnlocked != null &&
                isContractUnlocked(contractType);
            int initialCost = getInitialCost != null
                ? getInitialCost(contractType)
                : 0;
            int renewalCost = getRenewalCost != null
                ? getRenewalCost(contractType)
                : 0;
            columns.Add(new MercenaryContractDetailColumn(
                contractType,
                selectedContract == contractType,
                isUnlocked,
                initialCost,
                renewalCost));
        }

        return columns;
    }

    public static string BuildColumnText(MercenaryContractDetailColumn column)
    {
        string selectedLabel = column.IsSelected ? "選択中\n" : string.Empty;
        return selectedLabel +
               column.ContractName + "\n\n" +
               "契約金: " + column.InitialCost + " G\n" +
               "期限: " + column.TermText + "\n" +
               "更新費: " + column.RenewalCostText + "\n" +
               "更新方法:\n" + column.RenewalMethodText +
               "\n\n解放状況:\n" + column.UnlockStatusText;
    }

    public static string GetTermText(MercenaryContractType contractType)
    {
        switch (contractType)
        {
            case MercenaryContractType.Temporary: return "7日間";
            case MercenaryContractType.Exclusive: return "無期限";
            default: return "当日";
        }
    }

    public static string GetRenewalMethodText(MercenaryContractType contractType)
    {
        switch (contractType)
        {
            case MercenaryContractType.Temporary: return "7日ごとに自動更新";
            case MercenaryContractType.Exclusive: return "更新不要";
            default: return "毎日自動更新";
        }
    }
}
