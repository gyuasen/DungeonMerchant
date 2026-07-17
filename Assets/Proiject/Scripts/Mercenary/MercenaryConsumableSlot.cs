using System;
using UnityEngine;

[Serializable]
public sealed class MercenaryConsumableSlot
{
    public const int MaxCount = 5;

    [SerializeField] private ItemDataSO item;
    [SerializeField] private int count;

    public ItemDataSO Item => item;
    public int Count => count;
    public bool IsEmpty => item == null || count <= 0;

    public void Set(ItemDataSO value, int amount)
    {
        item = value;
        count = value == null ? 0 : Mathf.Clamp(amount, 0, MaxCount);
        if (count == 0)
        {
            item = null;
        }
    }

    public bool TryConsume()
    {
        if (IsEmpty)
        {
            return false;
        }

        count--;
        if (count <= 0)
        {
            Clear();
        }
        return true;
    }

    public void Clear()
    {
        item = null;
        count = 0;
    }
}
