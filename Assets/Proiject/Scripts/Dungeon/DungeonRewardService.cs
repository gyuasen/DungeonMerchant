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

            merchantInventory.AddItem(reward.item, reward.amount);
            SendMessage(
                $"踏破報酬: {JapaneseDisplayText.GetItemName(reward.item)} x{reward.amount}");
        }
    }

    public void TryGrantLimitedEquipment(
        DungeonDataSO dungeonData,
        float chance,
        string sourceLabel)
    {
        if (merchantInventory == null ||
            dungeonData?.limitedEquipmentDrops == null ||
            dungeonData.limitedEquipmentDrops.Length == 0 ||
            chance <= 0f ||
            UnityEngine.Random.value > chance)
        {
            return;
        }

        List<ItemDataSO> validDrops = new List<ItemDataSO>();
        foreach (ItemDataSO item in dungeonData.limitedEquipmentDrops)
        {
            if (item != null && item.IsEquipment)
            {
                validDrops.Add(item);
            }
        }

        if (validDrops.Count == 0)
        {
            return;
        }

        ItemDataSO drop = validDrops[UnityEngine.Random.Range(0, validDrops.Count)];
        EquipmentInstance equipment = EquipmentInstance.CreateRandom(drop);
        merchantInventory.AddEquipmentInstance(equipment);
        SendMessage(
            $"{sourceLabel}: [{JapaneseDisplayText.GetEquipmentQuality(equipment.Quality)}] " +
            $"{JapaneseDisplayText.GetItemName(drop)}");
    }

    private void SendMessage(string message)
    {
        messageHandler?.Invoke(message);
    }
}
