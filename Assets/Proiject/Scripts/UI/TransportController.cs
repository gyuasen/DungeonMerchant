using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Owns transport convoy selection state, validation calls and display text.
/// UI construction and event subscriptions remain in SimpleMercenaryHireUI.Transport.
/// </summary>
public sealed class TransportController
{
    private readonly TransportManager transportManager;
    private readonly MerchantInventory inventory;
    private readonly MercenaryHireManager hireManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly TownProgressState townProgressState;
    private readonly MarketPriceManager marketPriceManager;
    private readonly Action<string> setStatus;
    private readonly Action redraw;
    private readonly Dictionary<ItemDataSO, int> selectedCargo =
        new Dictionary<ItemDataSO, int>();
    private readonly List<MercenaryInstance> selectedEscorts =
        new List<MercenaryInstance>();

    private int destinationTownIndex = -1;

    public TransportController(
        TransportManager transportManager,
        MerchantInventory inventory,
        MercenaryHireManager hireManager,
        MercenaryPartyManager partyManager,
        TownProgressState townProgressState,
        MarketPriceManager marketPriceManager,
        Action<string> setStatus,
        Action redraw)
    {
        this.transportManager = transportManager;
        this.inventory = inventory;
        this.hireManager = hireManager;
        this.partyManager = partyManager;
        this.townProgressState = townProgressState;
        this.marketPriceManager = marketPriceManager;
        this.setStatus = setStatus;
        this.redraw = redraw;
    }

    public int DestinationTownIndex => destinationTownIndex;
    public IReadOnlyList<TransportConvoy> ActiveConvoys =>
        transportManager != null ? transportManager.ActiveConvoys :
        Array.Empty<TransportConvoy>();

    public IEnumerable<InventoryItemStack> GetCargoCandidates()
    {
        if (inventory == null)
        {
            yield break;
        }
        foreach (InventoryItemStack stack in inventory.Items)
        {
            if (stack?.Item != null && !stack.Item.IsEquipment && stack.Amount > 0)
            {
                yield return stack;
            }
        }
    }

    public IEnumerable<MercenaryInstance> GetAvailableEscorts()
    {
        if (hireManager == null)
        {
            yield break;
        }
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && mercenary.IsContractActive &&
                (partyManager == null || !partyManager.Contains(mercenary)) &&
                (transportManager == null ||
                 !transportManager.IsMercenaryOnTransportDuty(mercenary.InstanceId)))
            {
                yield return mercenary;
            }
        }
    }

    public bool IsDestinationAvailable(int townIndex)
    {
        return townProgressState != null && townIndex != townProgressState.CurrentTownIndex &&
               townProgressState.IsTownUnlocked(townIndex) &&
               WorldMapService.GetTownProgressionPosition(townIndex) >= 0;
    }

    public void SelectDestination(int townIndex)
    {
        if (!IsDestinationAvailable(townIndex))
        {
            return;
        }
        destinationTownIndex = townIndex;
        redraw?.Invoke();
    }

    public int GetSelectedCargoAmount(ItemDataSO item)
    {
        return item != null && selectedCargo.TryGetValue(item, out int amount)
            ? amount : 0;
    }

    public void ChangeCargo(ItemDataSO item, int available, int delta)
    {
        if (item == null)
        {
            return;
        }
        int amount = Math.Max(0, Math.Min(available, GetSelectedCargoAmount(item) + delta));
        if (amount == 0)
        {
            selectedCargo.Remove(item);
        }
        else
        {
            selectedCargo[item] = amount;
        }
        redraw?.Invoke();
    }

    public bool IsEscortSelected(MercenaryInstance mercenary)
    {
        return mercenary != null && selectedEscorts.Contains(mercenary);
    }

    public void ToggleEscort(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return;
        }
        if (selectedEscorts.Remove(mercenary))
        {
            redraw?.Invoke();
            return;
        }
        if (selectedEscorts.Count >= 3)
        {
            setStatus?.Invoke("護衛は3人まで選択できます");
            return;
        }
        bool isAvailable = false;
        foreach (MercenaryInstance candidate in GetAvailableEscorts())
        {
            if (ReferenceEquals(candidate, mercenary))
            {
                isAvailable = true;
                break;
            }
        }
        if (!isAvailable)
        {
            setStatus?.Invoke("その傭兵は護衛にできません");
            return;
        }
        selectedEscorts.Add(mercenary);
        redraw?.Invoke();
    }

    public int GetCargoUnits()
    {
        int units = 0;
        foreach (KeyValuePair<ItemDataSO, int> entry in selectedCargo)
        {
            units += entry.Value;
        }
        return units;
    }

    public int GetTransportCost() => destinationTownIndex < 0 || transportManager == null
        ? 0 : transportManager.CalculateTransportCost(destinationTownIndex, GetCargoUnits());

    public int GetEstimatedSaleGold()
    {
        if (destinationTownIndex < 0)
        {
            return 0;
        }
        int total = 0;
        foreach (KeyValuePair<ItemDataSO, int> entry in selectedCargo)
        {
            int price = marketPriceManager != null
                ? marketPriceManager.GetSellPrice(entry.Key)
                : entry.Key.basePrice;
            total += Math.Max(1, (int)Math.Round(price *
                WorldMapService.GetTownDemandMultiplier(destinationTownIndex, entry.Key))) * entry.Value;
        }
        return total;
    }

    public string BuildConvoyText(TransportConvoy convoy)
    {
        int cargoUnits = 0;
        if (convoy != null)
        {
            foreach (TransportCargo cargo in convoy.cargo)
            {
                cargoUnits += cargo != null ? cargo.amount : 0;
            }
        }
        return convoy == null ? string.Empty :
            $"{GetTownName(convoy.originTownIndex)}→{GetTownName(convoy.destinationTownIndex)} / " +
            $"残り{convoy.remainingDays}日 / 積荷{cargoUnits}個 / 護衛{convoy.escortInstanceIds.Count}人";
    }

    public void Depart()
    {
        if (transportManager == null)
        {
            return;
        }
        List<(ItemDataSO item, int amount)> cargo =
            new List<(ItemDataSO item, int amount)>();
        foreach (KeyValuePair<ItemDataSO, int> entry in selectedCargo)
        {
            if (entry.Key != null && entry.Value > 0)
            {
                cargo.Add((entry.Key, entry.Value));
            }
        }
        TransportDepartureResult result = transportManager.TryDepartConvoy(
            destinationTownIndex, cargo, selectedEscorts);
        setStatus?.Invoke(GetDepartureMessage(result));
        if (result == TransportDepartureResult.Succeeded)
        {
            selectedCargo.Clear();
            selectedEscorts.Clear();
            destinationTownIndex = -1;
        }
        redraw?.Invoke();
    }

    public static string GetDepartureMessage(TransportDepartureResult result)
    {
        switch (result)
        {
            case TransportDepartureResult.Succeeded: return "輸送部隊が出発しました";
            case TransportDepartureResult.InvalidDestination: return "目的地を選択してください";
            case TransportDepartureResult.InvalidCargo: return "積荷を選択してください";
            case TransportDepartureResult.InsufficientCargo: return "積荷の在庫が不足しています";
            case TransportDepartureResult.InvalidEscort: return "その傭兵は護衛にできません";
            case TransportDepartureResult.InsufficientGold: return "輸送費が不足しています";
            default: return "輸送部隊を出発できません";
        }
    }

    private static string GetTownName(int townIndex)
    {
        return townIndex >= 0 && townIndex < WorldMapService.TownNames.Length
            ? WorldMapService.TownNames[townIndex] : "不明な町";
    }
}
