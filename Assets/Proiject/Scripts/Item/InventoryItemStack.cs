using System;
using UnityEngine;

[Serializable]
public class InventoryItemStack
{
    [SerializeField] private ItemDataSO item;
    [SerializeField, Min(0)] private int amount;

    public ItemDataSO Item => item;
    public int Amount => amount;

    public InventoryItemStack(ItemDataSO item, int amount)
    {
        this.item = item;
        this.amount = Mathf.Max(0, amount);
    }

    public void Add(int value)
    {
        amount += Mathf.Max(0, value);
    }

    public bool Remove(int value)
    {
        if (value <= 0 || amount < value)
        {
            return false;
        }

        amount -= value;
        return true;
    }
}
