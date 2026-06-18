using System;
using System.Collections.Generic;
using UnityEngine;

public class MerchantInventory : MonoBehaviour
{
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MarketPriceManager marketPriceManager;
    [SerializeField] private List<InventoryItemStack> items =
        new List<InventoryItemStack>();

    public IReadOnlyList<InventoryItemStack> Items => items;

    public event Action InventoryChanged;

    public void AddItem(ItemDataSO item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return;
        }

        InventoryItemStack stack = FindStack(item);
        if (stack == null)
        {
            items.Add(new InventoryItemStack(item, amount));
        }
        else
        {
            stack.Add(amount);
        }

        Debug.Log($"Added item: {item.itemName} x{amount}");
        InventoryChanged?.Invoke();
    }

    public bool SellItem(ItemDataSO item, int amount = 1)
    {
        ResolveReferences();

        if (merchantData == null || item == null || amount <= 0)
        {
            return false;
        }

        InventoryItemStack stack = FindStack(item);
        if (stack == null || !stack.Remove(amount))
        {
            return false;
        }

        merchantData.AddGold(GetSellPrice(item) * amount);

        if (stack.Amount <= 0)
        {
            items.Remove(stack);
        }

        Debug.Log($"Sold item: {item.itemName} x{amount}");
        InventoryChanged?.Invoke();
        return true;
    }

    public int GetSellPrice(ItemDataSO item)
    {
        ResolveReferences();

        if (item == null)
        {
            return 0;
        }

        return marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item)
            : item.basePrice;
    }

    public void RestoreItems(IEnumerable<InventoryItemStack> restoredItems)
    {
        items.Clear();
        if (restoredItems != null)
        {
            foreach (InventoryItemStack stack in restoredItems)
            {
                if (stack?.Item != null && stack.Amount > 0)
                {
                    items.Add(stack);
                }
            }
        }

        InventoryChanged?.Invoke();
    }

    private InventoryItemStack FindStack(ItemDataSO item)
    {
        foreach (InventoryItemStack stack in items)
        {
            if (stack.Item == item)
            {
                return stack;
            }
        }

        return null;
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

        if (marketPriceManager == null)
        {
            marketPriceManager = GetComponent<MarketPriceManager>();
        }

        if (marketPriceManager == null)
        {
            marketPriceManager = FindObjectOfType<MarketPriceManager>();
        }
    }
}
