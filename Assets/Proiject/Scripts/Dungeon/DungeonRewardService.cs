using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DungeonRewardService
{
    private readonly MerchantData merchantData;
    private readonly MerchantInventory merchantInventory;
    private readonly Action<string> messageHandler;

    public DungeonRewardService(
        MerchantData targetMerchantData,
        MerchantInventory targetMerchantInventory,
        Action<string> targetMessageHandler)
    {
        merchantData = targetMerchantData;
        merchantInventory = targetMerchantInventory;
        messageHandler = targetMessageHandler;
    }

    public void GrantGold(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount <= 0)
        {
            return;
        }

        merchantData?.AddGold(safeAmount);
        SendMessage($"{safeAmount} Gを獲得しました。");
    }

    public void GrantClearRewards(DungeonDataSO dungeonData)
    {
        if (dungeonData == null)
        {
            return;
        }

        int goldReward = Mathf.Max(0, dungeonData.clearGoldReward);
        if (goldReward > 0)
        {
            merchantData?.AddGold(goldReward);
            SendMessage($"踏破報酬: {goldReward} G");
        }

        if (merchantInventory == null || dungeonData.clearItemRewards == null)
        {
            return;
        }

        foreach (DungeonItemReward reward in dungeonData.clearItemRewards)
        {
            if (reward == null || reward.item == null || reward.amount <= 0)
            {
                continue;
            }

            if (merchantInventory.TryAddItem(reward.item, reward.amount))
            {
                SendMessage(
                    $"踏破報酬: {JapaneseDisplayText.GetItemName(reward.item)} x{reward.amount}");
            }
            else
            {
                SendMessage(
                    $"倉庫が満杯で受け取れませんでした: " +
                    $"{JapaneseDisplayText.GetItemName(reward.item)} x{reward.amount}");
            }
        }
    }

    public void TryGrantLimitedEquipment(
        DungeonDataSO dungeonData,
        float chance,
        string sourceLabel)
    {
        if (merchantInventory == null ||
            chance <= 0f ||
            UnityEngine.Random.value > chance)
        {
            return;
        }

        EquipmentInstance equipment = TryCreateLimitedEquipment(
            dungeonData,
            () => UnityEngine.Random.value);
        if (equipment == null)
        {
            return;
        }

        merchantInventory.AddEquipmentInstance(equipment);
        SendMessage(
            $"{sourceLabel}: [{JapaneseDisplayText.GetEquipmentQuality(equipment.Quality)}] " +
            $"{JapaneseDisplayText.GetItemName(equipment.BaseItem)}");
    }

    public static EquipmentInstance TryCreateLimitedEquipment(
        DungeonDataSO dungeonData,
        Func<float> randomValue)
    {
        if (dungeonData?.limitedEquipmentDrops == null ||
            dungeonData.limitedEquipmentDrops.Length == 0)
        {
            return null;
        }

        Func<float> provider = randomValue ?? (() => UnityEngine.Random.value);
        List<ItemDataSO> validDrops = new List<ItemDataSO>();
        foreach (ItemDataSO item in dungeonData.limitedEquipmentDrops)
        {
            if (item != null &&
                item.IsEquipment &&
                WorldMapService.IsDungeonEquipmentRankAllowed(
                    dungeonData.nearbyTownIndex,
                    item.equipmentRank))
            {
                validDrops.Add(item);
            }
        }

        if (validDrops.Count == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(
            Mathf.FloorToInt(provider() * validDrops.Count),
            0,
            validDrops.Count - 1);
        return EquipmentInstance.CreateRandom(validDrops[index], provider);
    }

    private void SendMessage(string message)
    {
        messageHandler?.Invoke(message);
    }
}
