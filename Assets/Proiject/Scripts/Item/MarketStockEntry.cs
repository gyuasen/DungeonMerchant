using System;
using UnityEngine;

[Serializable]
public class MarketStockEntry
{
    [SerializeField] private ItemDataSO item;
    [SerializeField, Min(0)] private int quantity;
    [SerializeField, Min(0)] private int buyPrice;

    public ItemDataSO Item => item;
    public int Quantity => quantity;
    public int BuyPrice => buyPrice;

    public MarketStockEntry(ItemDataSO item, int quantity, int buyPrice)
    {
        this.item = item;
        this.quantity = Mathf.Max(0, quantity);
        this.buyPrice = Mathf.Max(0, buyPrice);
    }

    public bool Remove(int amount)
    {
        if (amount <= 0 || quantity < amount)
        {
            return false;
        }

        quantity -= amount;
        return true;
    }
}
