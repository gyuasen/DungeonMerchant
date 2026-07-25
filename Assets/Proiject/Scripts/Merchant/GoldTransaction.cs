using System;

public enum GoldTransactionReason
{
    Unclassified,
    ItemSale,
    QuestReward,
    BattleReward,
    DungeonReward,
    RemoteSale,
    MarketPurchase,
    Blacksmith,
    MercenaryHire,
    ContractRenewal,
    Healing,
    Training,
    DebtRepayment,
    StorageUpgrade,
    StorageMaintenance,
    ExplorationExpense,
    Refund,
    Other,
    ContractChange
}

public sealed class GoldTransaction
{
    public string TransactionId { get; }
    public int SignedAmount { get; }
    public GoldTransactionReason Reason { get; }
    public string Detail { get; }
    public int AccountingDay { get; }
    public string RelatedTransactionId { get; }

    public GoldTransaction(
        string transactionId,
        int signedAmount,
        GoldTransactionReason reason,
        string detail,
        int accountingDay,
        string relatedTransactionId = null)
    {
        TransactionId = transactionId;
        SignedAmount = signedAmount;
        Reason = reason;
        Detail = detail;
        AccountingDay = accountingDay;
        RelatedTransactionId = relatedTransactionId;
    }
}
