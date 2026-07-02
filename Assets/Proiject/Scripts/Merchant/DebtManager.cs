using System;
using UnityEngine;

public class DebtManager : MonoBehaviour
{
    public const int InitialDebt = 100000000;
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
    public int NextMinimumPayment =>
        Mathf.Min(remainingDebt, MonthlyMinimumPayment + paymentArrears);
    public bool IsDebtCleared => remainingDebt <= 0;

    public event Action DebtChanged;
    public event Action<int, int, int> MonthlyPaymentProcessed;

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

    public int Repay(int requestedAmount)
    {
        ResolveReferences();
        if (merchantData == null || requestedAmount <= 0 || IsDebtCleared)
        {
            return 0;
        }

        int payment = Mathf.Min(requestedAmount, remainingDebt, merchantData.Gold);
        if (payment <= 0 || !merchantData.TryPayGold(payment))
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
        int due = NextMinimumPayment;
        int paid = Repay(due);
        paymentArrears = Mathf.Max(0, due - paid);
        MonthlyPaymentProcessed?.Invoke(due, paid, paymentArrears);
        DebtChanged?.Invoke();
    }

    private void ResolveReferences()
    {
        merchantData = merchantData ?? GetComponent<MerchantData>() ??
            FindObjectOfType<MerchantData>();
        dayManager = dayManager ?? GetComponent<DayManager>() ??
            FindObjectOfType<DayManager>();
    }
}
