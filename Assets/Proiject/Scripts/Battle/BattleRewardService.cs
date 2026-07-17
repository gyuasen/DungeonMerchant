using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies battle rewards and persists each participant's post-battle state.
/// This is deliberately a plain C# service; BattleManager owns battle flow and events.
/// </summary>
public sealed class BattleRewardService
{
    // Keep low-grade encounters meaningful while flattening high-grade rewards.
    // strengthOffset is 0 for grade 10 and 9 for grade 1.
    private const int BaseExperienceReward = 50;
    private const int LinearExperienceGrowth = 20;
    private const int QuadraticExperienceGrowth = 5;

    private readonly MerchantData merchantData;
    private readonly MerchantInventory merchantInventory;
    private readonly Action<string, BattleLogType> messageHandler;
    private readonly Func<float> randomValueProvider;
    private ItemDataSO fallbackDropItem;

    public sealed class VictoryRewardCalculation
    {
        public int Gold { get; }
        public int ExperiencePerMercenary { get; }
        public IReadOnlyList<ItemDropEntry> ItemDrops { get; }

        public VictoryRewardCalculation(int gold, int experiencePerMercenary, IReadOnlyList<ItemDropEntry> itemDrops)
        {
            Gold = gold;
            ExperiencePerMercenary = experiencePerMercenary;
            ItemDrops = itemDrops;
        }
    }

    public BattleRewardService(
        MerchantData targetMerchantData,
        MerchantInventory targetMerchantInventory,
        Action<string, BattleLogType> targetMessageHandler,
        Func<float> targetRandomValueProvider)
    {
        merchantData = targetMerchantData;
        merchantInventory = targetMerchantInventory;
        messageHandler = targetMessageHandler;
        randomValueProvider = targetRandomValueProvider ?? (() => UnityEngine.Random.value);
    }

    public bool MatchesDependencies(
        MerchantData targetMerchantData,
        MerchantInventory targetMerchantInventory)
    {
        return merchantData == targetMerchantData &&
               merchantInventory == targetMerchantInventory;
    }

    public void ApplyBattleResultsToMercenaries(
        IReadOnlyList<MercenaryInstance> battleMercenaries,
        IReadOnlyList<BattleUnit> playerUnits)
    {
        if (battleMercenaries == null || playerUnits == null)
        {
            return;
        }

        int count = Mathf.Min(battleMercenaries.Count, playerUnits.Count);
        for (int i = 0; i < count; i++)
        {
            MercenaryInstance mercenary = battleMercenaries[i];
            BattleUnit unit = playerUnits[i];
            mercenary.SetCurrentHP(unit.CurrentHP);
            mercenary.RestoreStatusEffect(unit.StatusEffect);
            SendMessage(
                BattleLogFormatter.FormatPostBattleHp(
                    mercenary.MercenaryName,
                    mercenary.CurrentHP,
                    mercenary.MaxHP,
                    unit.StatusSummary),
                BattleLogType.System);
        }
    }

    public void GrantVictoryRewards(
        IReadOnlyList<EnemyDataSO> defeatedEnemies,
        IReadOnlyList<MercenaryInstance> battleMercenaries)
    {
        VictoryRewardCalculation calculation = CalculateVictoryRewards(
            defeatedEnemies,
            battleMercenaries == null ? 0 : battleMercenaries.Count,
            randomValueProvider,
            GetFallbackDropItem());
        merchantData?.AddGold(calculation.Gold);
        SendMessage(
            BattleLogFormatter.FormatVictoryGold(calculation.Gold),
            BattleLogType.Reward);
        GrantExperienceRewards(calculation.ExperiencePerMercenary, battleMercenaries);
        GrantItemRewards(calculation.ItemDrops);
    }

    public static VictoryRewardCalculation CalculateVictoryRewards(
        IReadOnlyList<EnemyDataSO> defeatedEnemies,
        int mercenaryCount,
        Func<float> randomValueProvider,
        ItemDataSO fallbackItem)
    {
        List<ItemDropEntry> drops = CalculateItemDrops(
            defeatedEnemies,
            randomValueProvider,
            fallbackItem);
        return new VictoryRewardCalculation(
            CalculateGoldReward(defeatedEnemies),
            CalculateExperiencePerMercenary(
                CalculateExperienceReward(defeatedEnemies),
                mercenaryCount),
            drops);
    }

    public static List<ItemDropEntry> CalculateItemDrops(
        IReadOnlyList<EnemyDataSO> defeatedEnemies,
        Func<float> randomValueProvider,
        ItemDataSO fallbackItem)
    {
        Func<float> random = randomValueProvider ?? (() => UnityEngine.Random.value);
        List<ItemDropEntry> result = new List<ItemDropEntry>();
        if (defeatedEnemies != null)
        {
            foreach (EnemyDataSO enemy in defeatedEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }
                if (enemy.itemDrops != null)
                {
                    foreach (ItemDropEntry drop in enemy.itemDrops)
                    {
                        if (drop != null && drop.item != null && drop.amount > 0 && random() <= drop.dropChance)
                        {
                            result.Add(drop);
                        }
                    }
                }
                ItemDataSO magicStone = MaterialCatalog.GetMagicStoneForEnemyGrade(enemy.monsterGrade);
                if (magicStone != null && (enemy.isBoss || random() <= MaterialCatalog.MagicStoneDropChance))
                {
                    result.Add(new ItemDropEntry { item = magicStone, amount = enemy.isBoss ? 2 : 1, dropChance = 1f });
                }
            }
        }
        if (result.Count == 0 && fallbackItem != null)
        {
            result.Add(new ItemDropEntry { item = fallbackItem, amount = 1, dropChance = 1f });
        }
        return result;
    }

    public static int CalculateGoldReward(IReadOnlyList<EnemyDataSO> defeatedEnemies)
    {
        if (defeatedEnemies == null)
        {
            return 0;
        }

        int totalGoldReward = 0;
        foreach (EnemyDataSO enemy in defeatedEnemies)
        {
            if (enemy != null)
            {
                totalGoldReward += enemy.goldReward;
            }
        }

        return totalGoldReward;
    }

    public static int CalculateExperienceReward(
        IReadOnlyList<EnemyDataSO> defeatedEnemies)
    {
        if (defeatedEnemies == null)
        {
            return 1;
        }

        int totalExperience = 0;
        foreach (EnemyDataSO enemy in defeatedEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            int enemyExperience = CalculateBaseExperienceReward(
                enemy.monsterGrade);
            if (enemy.isBoss)
            {
                enemyExperience *= 2;
            }

            enemyExperience = Mathf.RoundToInt(
                enemyExperience * Mathf.Max(1f, enemy.experienceMultiplier));
            totalExperience += enemyExperience;
        }

        return Mathf.Max(1, totalExperience);
    }

    public static int CalculateBaseExperienceReward(int monsterGrade)
    {
        int grade = Mathf.Clamp(monsterGrade, 1, 10);
        int strengthOffset = 10 - grade;
        return BaseExperienceReward +
               (LinearExperienceGrowth * strengthOffset) +
               (QuadraticExperienceGrowth * strengthOffset * strengthOffset);
    }

    public static int CalculateExperiencePerMercenary(
        int totalExperience,
        int mercenaryCount)
    {
        return mercenaryCount <= 0
            ? 0
            : Mathf.Max(1, totalExperience / mercenaryCount);
    }

    private void GrantExperienceRewards(
        int experiencePerMercenary,
        IReadOnlyList<MercenaryInstance> battleMercenaries)
    {
        if (battleMercenaries == null || battleMercenaries.Count == 0)
        {
            return;
        }

        foreach (MercenaryInstance mercenary in battleMercenaries)
        {
            if (mercenary.IsAtLevelCap)
            {
                SendMessage(
                    BattleLogFormatter.FormatLevelCap(
                        mercenary.MercenaryName,
                        mercenary.LevelCap),
                    BattleLogType.Reward);
                continue;
            }

            int previousLevel = mercenary.Level;
            int levelsGained = mercenary.AddExperience(experiencePerMercenary);
            SendMessage(
                BattleLogFormatter.FormatExperience(
                    mercenary.MercenaryName,
                    experiencePerMercenary,
                    mercenary.CurrentExperience,
                    mercenary.ExperienceToNextLevel),
                BattleLogType.Reward);

            if (levelsGained > 0)
            {
                SendMessage(
                    BattleLogFormatter.FormatLevelUp(
                        mercenary.MercenaryName,
                        previousLevel,
                        mercenary.Level),
                    BattleLogType.Reward);
            }
        }
    }

    private void GrantItemRewards(IReadOnlyList<ItemDropEntry> drops)
    {
        if (merchantInventory == null)
        {
            return;
        }
        if (drops == null)
        {
            return;
        }
        foreach (ItemDropEntry drop in drops)
        {
            if (drop != null && drop.item != null && drop.amount > 0)
            {
                if (!merchantInventory.TryAddItem(drop.item, drop.amount))
                {
                    SendMessage(
                        "倉庫が満杯のため、戦利品を受け取れませんでした。",
                        BattleLogType.System);
                    continue;
                }

                SendMessage(
                    BattleLogFormatter.FormatItemDrop(
                        JapaneseDisplayText.GetItemName(drop.item),
                        drop.amount),
                    BattleLogType.Reward);
            }
        }
    }

    private void GrantItemRewardsLegacy(IReadOnlyList<EnemyDataSO> defeatedEnemies)
    {
        if (merchantInventory == null)
        {
            SendMessage("商人在庫が設定されていません。", BattleLogType.System);
            return;
        }

        bool droppedAnyItem = false;
        bool rolledAnyItem = false;
        if (defeatedEnemies != null)
        {
            foreach (EnemyDataSO defeatedEnemy in defeatedEnemies)
            {
                if (defeatedEnemy == null || defeatedEnemy.itemDrops == null)
                {
                    continue;
                }

                foreach (ItemDropEntry drop in defeatedEnemy.itemDrops)
                {
                    if (drop == null || drop.item == null || drop.amount <= 0 ||
                        randomValueProvider() > drop.dropChance)
                    {
                        continue;
                    }

                    rolledAnyItem = true;
                    if (!merchantInventory.TryAddItem(drop.item, drop.amount))
                    {
                        SendMessage(
                            "倉庫が満杯のため、戦利品を受け取れませんでした。",
                            BattleLogType.System);
                        continue;
                    }
                    SendMessage(
                        BattleLogFormatter.FormatItemDrop(
                            JapaneseDisplayText.GetItemName(drop.item),
                            drop.amount),
                        BattleLogType.Reward);
                    droppedAnyItem = true;
                }
            }
        }

        if (!rolledAnyItem && !droppedAnyItem)
        {
            ItemDataSO fallbackItem = GetFallbackDropItem();
            if (!merchantInventory.TryAddItem(fallbackItem, 1))
            {
                SendMessage(
                    "倉庫が満杯のため、戦利品を受け取れませんでした。",
                    BattleLogType.System);
                return;
            }
            SendMessage(
                BattleLogFormatter.FormatItemDrop(
                    JapaneseDisplayText.GetItemName(fallbackItem),
                    1),
                BattleLogType.Reward);
        }
    }

    private ItemDataSO GetFallbackDropItem()
    {
        if (fallbackDropItem != null)
        {
            return fallbackDropItem;
        }

        IReadOnlyList<ItemDataSO> resourceItems =
            GameAssetRepository.LoadAll<ItemDataSO>();
        if (resourceItems.Count > 0)
        {
            fallbackDropItem = resourceItems[0];
            return fallbackDropItem;
        }

        fallbackDropItem = ScriptableObject.CreateInstance<ItemDataSO>();
        fallbackDropItem.name = "Runtime Monster Fang";
        fallbackDropItem.itemName = "Monster Fang";
        fallbackDropItem.itemType = ItemType.Material;
        fallbackDropItem.rarity = ItemRarity.Common;
        fallbackDropItem.description = "A common monster material for testing trade flow.";
        fallbackDropItem.basePrice = 25;
        return fallbackDropItem;
    }

    private void SendMessage(string message, BattleLogType logType)
    {
        messageHandler?.Invoke(message, logType);
    }
}
