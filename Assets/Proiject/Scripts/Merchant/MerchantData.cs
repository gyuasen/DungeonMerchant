using System;
using UnityEngine;

public class MerchantData : MonoBehaviour
{
    [Header("Merchant Status")]
    [SerializeField, Min(0)] private int gold = 500;

    public int Gold => gold;

    public event Action<int> GoldChanged;

    private void OnValidate()
    {
        gold = Mathf.Max(0, gold);
    }

    public bool CanPay(int amount)
    {
        return amount >= 0 && gold >= amount;
    }

    public bool TryPayGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Invalid payment amount.");
            return false;
        }

        if (!CanPay(amount))
        {
            Debug.Log("Not enough gold.");
            return false;
        }

        gold -= amount;
        GoldChanged?.Invoke(gold);
        Debug.Log($"Paid {amount} G. Current gold: {gold} G");
        return true;
    }

    public void PayGold(int amount)
    {
        TryPayGold(amount);
    }

    public void AddGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Invalid gold reward amount.");
            return;
        }

        gold += amount;
        GoldChanged?.Invoke(gold);
        Debug.Log($"Gained {amount} G. Current gold: {gold} G");
    }

    public void SetGold(int value)
    {
        gold = Mathf.Max(0, value);
        GoldChanged?.Invoke(gold);
    }
}
