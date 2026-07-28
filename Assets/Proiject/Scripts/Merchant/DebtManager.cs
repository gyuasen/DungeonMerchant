using System;
using UnityEngine;

public class DebtManager : MonoBehaviour
{
    public const int InitialDebt = 50000000;
    public const int MonthlyMinimumPayment = 10000;
    public const int DaysPerMonth = 30;

    [SerializeField] private MerchantData merchantData;
    [SerializeField] private DayManager dayManager;
    [SerializeField, Min(0)] private int remainingDebt = InitialDebt;
    [SerializeField, Min(0)] private int paymentArrears;
    [SerializeField, Min(0)] private int processedMonths;

    public int RemainingDebt => remainingDebt;
    public int PaymentArrears => paymentArrears;
    public int CurrentMonth => dayManager != null
        ? ((Mathf.Max(1, dayManager.CurrentDay) - 1) / DaysPerMonth) + 1
        : 1;
    public int DaysUntilPayment => DaysPerMonth -
        ((Mathf.Max(1, dayManager != null ? dayManager.CurrentDay : 1) - 1)
         % DaysPerMonth);
    // 延滞金は廃止。月次徴収は所持金不足でも強制で、不足分は所持金が負に沈む。
    public int NextMinimumPayment =>
        Mathf.Min(remainingDebt, MonthlyMinimumPayment);
    public bool IsDebtCleared => remainingDebt <= 0;

    public event Action DebtChanged;
    public event Action<int, int, int> MonthlyPaymentProcessed;

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToDayChanges();
    }

    private void OnDisable()
    {
        UnsubscribeFromDayChanges();
    }

    public void Initialize(
        MerchantData targetMerchantData,
        DayManager targetDayManager)
    {
        UnsubscribeFromDayChanges();
        merchantData = targetMerchantData;
        dayManager = targetDayManager;

        if (isActiveAndEnabled)
        {
            SubscribeToDayChanges();
        }
    }

    public int Repay(int requestedAmount)
    {
        ResolveReferences();
        if (merchantData == null || requestedAmount <= 0 || IsDebtCleared)
        {
            return 0;
        }

        int payment = Mathf.Min(requestedAmount, remainingDebt, merchantData.Gold);
        if (payment <= 0 || !merchantData.TryPayGold(
                payment,
                GoldTransactionReason.DebtRepayment))
        {
            return 0;
        }

        remainingDebt -= payment;
        paymentArrears = Mathf.Max(0, paymentArrears - payment);
        DebtChanged?.Invoke();
        return payment;
    }

    public void Restore(int debt, int arrears, int savedProcessedMonths)
    {
        remainingDebt = Mathf.Clamp(debt, 0, InitialDebt);
        paymentArrears = Mathf.Max(0, arrears);
        processedMonths = Mathf.Max(0, savedProcessedMonths);
        DebtChanged?.Invoke();
    }

    private void HandleDayChanged(int currentDay)
    {
        int completedMonths = (Mathf.Max(1, currentDay) - 1) / DaysPerMonth;
        while (processedMonths < completedMonths && !IsDebtCleared)
        {
            ProcessMonthlyPayment();
            processedMonths++;
        }
    }

    private void ProcessMonthlyPayment()
    {
        ResolveReferences();
        if (merchantData == null || IsDebtCleared)
        {
            return;
        }

        // 月次最低額を強制徴収する。所持金が足りなくても引き、残高は負に沈む。
        // 残債もその分だけ確実に減る。延滞金の繰越は行わない。
        int due = NextMinimumPayment;
        if (due <= 0)
        {
            return;
        }

        merchantData.ForceDeductGold(
            due,
            GoldTransactionReason.DebtRepayment,
            accountingDay: dayManager != null ? dayManager.CurrentDay : (int?)null);
        remainingDebt = Mathf.Max(0, remainingDebt - due);
        paymentArrears = 0;
        MonthlyPaymentProcessed?.Invoke(due, due, 0);
        DebtChanged?.Invoke();
    }

    private void ResolveReferences()
    {
        merchantData = merchantData ?? GetComponent<MerchantData>() ??
            FindObjectOfType<MerchantData>();
        dayManager = dayManager ?? GetComponent<DayManager>() ??
            FindObjectOfType<DayManager>();
    }

    private void SubscribeToDayChanges()
    {
        if (dayManager == null)
        {
            return;
        }

        dayManager.DayChanged -= HandleDayChanged;
        dayManager.DayChanged += HandleDayChanged;
    }

    private void UnsubscribeFromDayChanges()
    {
        if (dayManager != null)
        {
            dayManager.DayChanged -= HandleDayChanged;
        }
    }
}
