using System;
using System.Collections.Generic;

public sealed class RemoteSaleController
{
    private readonly RemoteSaleManager manager;
    private readonly MerchantInventory inventory;
    private readonly TownProgressState towns;
    private readonly Action<string> setStatus;
    private readonly Action redraw;
    private int selectedTownIndex = -1;
    private readonly Dictionary<ItemDataSO, int> selectedAmounts =
        new Dictionary<ItemDataSO, int>();

    public RemoteSaleController(RemoteSaleManager manager, MerchantInventory inventory,
        TownProgressState towns, Action<string> setStatus, Action redraw)
    {
        this.manager = manager;
        this.inventory = inventory;
        this.towns = towns;
        this.setStatus = setStatus;
        this.redraw = redraw;
    }

    public int SelectedTownIndex => selectedTownIndex;
    public IReadOnlyList<RemoteSaleOrder> ActiveOrders => manager != null
        ? manager.ActiveOrders : Array.Empty<RemoteSaleOrder>();

    public bool IsTownAvailable(int townIndex)
    {
        return towns != null && townIndex != towns.CurrentTownIndex &&
            WorldMapService.GetTownProgressionPosition(townIndex) >= 0;
    }

    public void SelectTown(int townIndex)
    {
        if (!IsTownAvailable(townIndex))
        {
            return;
        }
        selectedTownIndex = townIndex;
        selectedAmounts.Clear();
        redraw?.Invoke();
    }

    public IEnumerable<InventoryItemStack> GetItems()
    {
        return selectedTownIndex >= 0 && inventory != null
            ? inventory.GetItemsIn(selectedTownIndex) : Array.Empty<InventoryItemStack>();
    }

    public IEnumerable<EquipmentInstance> GetEquipment()
    {
        return selectedTownIndex >= 0 && inventory != null
            ? inventory.GetEquipmentInstancesIn(selectedTownIndex) : Array.Empty<EquipmentInstance>();
    }

    public int GetSelectedAmount(ItemDataSO item)
    {
        return item != null && selectedAmounts.TryGetValue(item, out int amount)
            ? amount : 0;
    }

    public void ChangeAmount(ItemDataSO item, int available, int delta)
    {
        if (item == null)
        {
            return;
        }
        int amount = Math.Max(0, Math.Min(available, GetSelectedAmount(item) + delta));
        if (amount == 0)
        {
            selectedAmounts.Remove(item);
        }
        else
        {
            selectedAmounts[item] = amount;
        }
        redraw?.Invoke();
    }

    public int GetSelectedEstimatedGold()
    {
        int total = 0;
        foreach (KeyValuePair<ItemDataSO, int> entry in selectedAmounts)
        {
            total += manager.GetSellPriceAt(selectedTownIndex, entry.Key) * entry.Value;
        }
        return total;
    }

    public int GetSettlementDays() => manager != null && selectedTownIndex >= 0
        ? manager.CalculateSettlementDays(selectedTownIndex) : 0;

    public void ConfirmItems()
    {
        foreach (KeyValuePair<ItemDataSO, int> entry in selectedAmounts)
        {
            RemoteSaleOrderResult result = manager.TryCreateItemOrder(
                selectedTownIndex, entry.Key, entry.Value);
            if (result != RemoteSaleOrderResult.Succeeded)
            {
                setStatus?.Invoke(GetMessage(result));
                redraw?.Invoke();
                return;
            }
        }
        selectedAmounts.Clear();
        setStatus?.Invoke("売却指示を受け付けました");
        redraw?.Invoke();
    }

    public void SellEquipment(EquipmentInstance equipment)
    {
        RemoteSaleOrderResult result = manager.TryCreateEquipmentOrder(selectedTownIndex, equipment);
        setStatus?.Invoke(result == RemoteSaleOrderResult.Succeeded
            ? "売却指示を受け付けました" : GetMessage(result));
        redraw?.Invoke();
    }

    public void Cancel(RemoteSaleOrder order)
    {
        if (manager.CancelOrder(order))
        {
            setStatus?.Invoke("売却指示を取り消しました");
        }
        redraw?.Invoke();
    }

    private static string GetMessage(RemoteSaleOrderResult result)
    {
        switch (result)
        {
            case RemoteSaleOrderResult.CurrentTown: return "現在町の倉庫は即時売却を利用してください";
            case RemoteSaleOrderResult.InsufficientInventory: return "倉庫の在庫が不足しています";
            case RemoteSaleOrderResult.InvalidAmount: return "数量を選択してください";
            default: return "売却指示を作成できません";
        }
    }
}
