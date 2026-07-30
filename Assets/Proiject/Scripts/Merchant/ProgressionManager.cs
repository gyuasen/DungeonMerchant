using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    private static readonly int[] StorageCapacities =
        { 30, 60, 100, 160, 230, 310, 400, 500, 610, 720, 830, 900, 950, 980, 1000 };
    private static readonly int[] StorageUpgradeCosts =
        { 1500, 5000, 12000, 25000, 45000, 75000, 120000, 180000, 260000,
          360000, 480000, 620000, 780000, 950000, 0 };
    private static readonly int[] StorageRequiredLevels =
        { 1, 4, 8, 12, 18, 24, 32, 40, 50, 60, 70, 80, 90, 95, 100 };
    private static readonly int[] StorageMaintenanceCosts =
        { 0, 0, 100, 150, 200, 260, 330, 410, 500, 600, 700, 800, 900, 950, 1000 };

    [SerializeField] private MerchantData merchantData;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private RoadEncounterService roadEncounterService;
    [SerializeField] private DebtManager debtManager;
    [SerializeField] private List<SpecialQuestSO> specialQuestDefinitions =
        new List<SpecialQuestSO>();
    [SerializeField] private List<QuestRecord> quests = new List<QuestRecord>();
    [SerializeField, Range(0, 3)] private int storageTier;
    [SerializeField] private int totalDungeonClears;
    [SerializeField] private int profitableDungeonClears;
    [SerializeField] private int explorationExtraDays;
    [SerializeField] private int explorationStartGold;

    public IReadOnlyList<QuestRecord> Quests => quests;
    public int StorageTier => storageTier;
    public int StorageCapacity =>
        StorageCapacities[storageTier] +
        (merchantData != null ? merchantData.GetStorageCapacityBonus() : 0);
    public int StorageUpgradeCost => StorageUpgradeCosts[storageTier];
    public bool IsStorageAtMaximumTier => storageTier >= StorageCapacities.Length - 1;
    public int NextStorageCapacity => IsStorageAtMaximumTier
        ? StorageCapacity
        : StorageCapacities[storageTier + 1] +
          (merchantData != null ? merchantData.GetStorageCapacityBonus() : 0);
    public int NextStorageRequiredMerchantLevel => IsStorageAtMaximumTier
        ? 0
        : StorageRequiredLevels[storageTier + 1];
    public int StorageMaintenanceCost =>
        StorageMaintenanceCosts[
            Mathf.Clamp(storageTier, 0, StorageMaintenanceCosts.Length - 1)];
    public int TotalDungeonClears => totalDungeonClears;
    public int TotalGoldEarned => merchantData != null
        ? merchantData.LifetimeGoldEarned
        : 0;
    public int ProfitableDungeonClears => profitableDungeonClears;
    public string LastExplorationResult { get; private set; } = string.Empty;

    public event Action ProgressionChanged;
    public event Action<QuestCompletionInfo> QuestCompleted;

    private void OnEnable()
    {
        ResolveReferences();
        MigrateQuestRecords();
        PopulateSpecialQuests();
        GenerateNormalQuestsIfNeeded();
        if (dayManager != null) dayManager.DayChanged += HandleDayChanged;
        if (battleManager != null)
        {
            battleManager.EnemiesDefeated += HandleEnemiesDefeated;
        }
        if (inventory != null)
        {
            inventory.InventoryChanged += HandleInventoryChanged;
        }
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
        }
    }

    private void OnDisable()
    {
        if (dayManager != null) dayManager.DayChanged -= HandleDayChanged;
        if (battleManager != null)
        {
            battleManager.EnemiesDefeated -= HandleEnemiesDefeated;
        }
        if (inventory != null)
        {
            inventory.InventoryChanged -= HandleInventoryChanged;
        }
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
        }
    }

    public bool CanStore(int amount = 1)
    {
        ResolveReferences();
        return inventory == null ||
               inventory.GetUsedStorageSlots() + Mathf.Max(0, amount) <=
               StorageCapacity;
    }

    public bool CanStoreIn(int townIndex, int amount = 1)
    {
        ResolveReferences();
        return inventory == null ||
               inventory.GetUsedStorageSlotsIn(townIndex) +
               Mathf.Max(0, amount) <= StorageCapacity;
    }

    public bool TryUpgradeStorage()
    {
        ResolveReferences();
        if (!CanUpgradeStorage())
        {
            return false;
        }

        merchantData.TryPayGold(
            StorageUpgradeCost,
            GoldTransactionReason.StorageUpgrade);
        storageTier++;
        ProgressionChanged?.Invoke();
        return true;
    }

    public bool CanUpgradeStorage()
    {
        ResolveReferences();
        return !IsStorageAtMaximumTier &&
               merchantData != null &&
               merchantData.MerchantLevel >= NextStorageRequiredMerchantLevel &&
               merchantData.CanPay(StorageUpgradeCost);
    }

    public bool AcceptQuest(int index)
    {
        return index >= 0 && index < quests.Count &&
               AcceptQuest(quests[index].questId);
    }

    public bool AcceptQuest(string questId)
    {
        ResolveReferences();
        QuestRecord quest = quests.Find(value => value != null &&
            value.questId == questId);
        if (quest == null || !CanAcceptQuestHere(quest))
        {
            return false;
        }
        quest.accepted = true;
        TryCompleteQuest(quest);
        ProgressionChanged?.Invoke();
        return true;
    }

    public IReadOnlyList<QuestRecord> GetAvailableQuestsForCurrentTown()
    {
        ResolveReferences();
        return quests.FindAll(IsQuestAvailableHere);
    }

    public bool CanAcceptQuestHere(QuestRecord quest)
    {
        return !quest.accepted && IsQuestAvailableHere(quest);
    }

    public int GetQuestGoldReward(QuestRecord quest)
    {
        if (quest == null)
        {
            return 0;
        }
        float multiplier = merchantData != null
            ? merchantData.GetQuestGoldMultiplier()
            : 1f;
        return Mathf.RoundToInt(quest.goldReward * multiplier);
    }

    public int GetQuestExperienceReward(QuestRecord quest)
    {
        if (quest == null)
        {
            return 0;
        }
        float multiplier = merchantData != null
            ? merchantData.GetQuestExperienceMultiplier()
            : 1f;
        return Mathf.RoundToInt(quest.experienceReward * multiplier);
    }

    public void StartExploration()
    {
        explorationExtraDays = 0;
        explorationStartGold = merchantData != null ? merchantData.Gold : 0;
    }

    public void AddExplorationDelay(int days)
    {
        explorationExtraDays += Mathf.Max(0, days);
    }

    public string GetAchievementSummary()
    {
        ResolveReferences();
        string debtGoal = debtManager == null
            ? "借金情報なし"
            : debtManager.IsDebtCleared
                ? "達成 借金1億Gを完済（ゲームクリア）"
                : $"未達 借金残高 {debtManager.RemainingDebt:N0}G";
        return
            $"{debtGoal}\n" +
            $"累計獲得 {TotalGoldEarned:N0}G\n" +
            $"{(merchantData != null && merchantData.MerchantLevel >= 10 ? "達成" : "未達")} 商人Lv10\n" +
            $"{(profitableDungeonClears >= 10 ? "達成" : "未達")} 黒字探索10回\n" +
            $"{(merchantData != null && merchantData.Gold >= 50000 ? "達成" : "未達")} 資産50000G\n" +
            $"{(totalDungeonClears >= 20 ? "達成" : "未達")} ダンジョン踏破20回";
    }

    public ProgressionSaveData CreateSaveData()
    {
        return new ProgressionSaveData
        {
            storageTier = storageTier,
            totalDungeonClears = totalDungeonClears,
            profitableDungeonClears = profitableDungeonClears,
            quests = new List<QuestRecord>(quests)
        };
    }

    public void Restore(ProgressionSaveData data)
    {
        if (data == null)
        {
            return;
        }
        ResolveReferences();
        storageTier = Mathf.Clamp(
            data.storageTier, 0, StorageCapacities.Length - 1);
        totalDungeonClears = Mathf.Max(0, data.totalDungeonClears);
        profitableDungeonClears = Mathf.Max(0, data.profitableDungeonClears);
        quests = data.quests ?? new List<QuestRecord>();
        MigrateQuestRecords();
        GenerateNormalQuestsIfNeeded();
        ProgressionChanged?.Invoke();
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        ResolveReferences();
        int days = 1 + Mathf.Max(0, explorationExtraDays);
        int grade = dungeonRunManager?.SelectedDungeon != null
            ? (int)dungeonRunManager.SelectedDungeon.grade
            : 0;
        float expenseMultiplier = merchantData != null
            ? merchantData.GetExplorationExpenseMultiplier()
            : 1f;
        int expense = Mathf.Max(
            0,
            Mathf.RoundToInt(
                days * (100 + grade * 75) * expenseMultiplier));
        int goldBefore = merchantData != null ? merchantData.Gold : 0;
        merchantData?.TryPayGold(
            Mathf.Min(goldBefore, expense),
            GoldTransactionReason.ExplorationExpense);
        dayManager?.AdvanceDays(days);
        if (cleared)
        {
            totalDungeonClears++;
            if (goldBefore - expense > explorationStartGold)
            {
                profitableDungeonClears++;
            }
        }
        LastExplorationResult =
            $"探索日数 {days}日 / 探索費用 {expense}G";
        explorationExtraDays = 0;
        ProgressionChanged?.Invoke();
    }

    private void HandleEnemiesDefeated(IReadOnlyList<EnemyDataSO> enemies)
    {
        foreach (EnemyDataSO enemy in enemies)
        {
            if (enemy == null) continue;
            foreach (QuestRecord quest in quests)
            {
                if (quest.accepted &&
                    !quest.completed &&
                    quest.questType == QuestType.MonsterHunt &&
                    MatchesEnemy(quest, enemy))
                {
                    quest.currentAmount++;
                    TryCompleteQuest(quest);
                }
            }
        }
        ProgressionChanged?.Invoke();
    }

    private void TryCompleteQuest(QuestRecord quest)
    {
        if (!quest.accepted || quest.completed || quest.expired)
        {
            return;
        }

        if (quest.questType == QuestType.ItemDelivery)
        {
            ItemDataSO item = FindItem(quest.targetPersistentId, quest.targetName);
            if (item == null ||
                !inventory.HasItem(item, quest.requiredAmount))
            {
                return;
            }
            quest.completed = true;
            if (!inventory.TryRemoveItem(item, quest.requiredAmount))
            {
                quest.completed = false;
                return;
            }
            quest.currentAmount = quest.requiredAmount;
            AwardQuestCompletion(quest);
            return;
        }

        if (quest.currentAmount < quest.requiredAmount)
        {
            return;
        }

        AwardQuestCompletion(quest);
    }

    private void AwardQuestCompletion(QuestRecord quest)
    {
        if (!quest.completed)
        {
            quest.completed = true;
        }
        int goldReward = GetQuestGoldReward(quest);
        merchantData?.AddGold(
            goldReward,
            GoldTransactionReason.QuestReward,
            quest.title);
        QuestCompleted?.Invoke(new QuestCompletionInfo
        {
            Quest = quest,
            DeliveredAmount = quest.questType == QuestType.ItemDelivery
                ? quest.requiredAmount
                : 0,
            GoldReward = goldReward,
            TownIndex = GetCurrentTownIndex()
        });
    }

    private void HandleInventoryChanged()
    {
        foreach (QuestRecord quest in quests)
        {
            if (quest.accepted &&
                !quest.completed &&
                !quest.expired &&
                quest.questType == QuestType.ItemDelivery)
            {
                TryCompleteQuest(quest);
            }
        }
    }

    private void HandleDayChanged(int currentDay)
    {
        if (StorageMaintenanceCost > 0 && merchantData != null)
        {
            merchantData.TryPayGold(
                Mathf.Min(merchantData.Gold, StorageMaintenanceCost),
                GoldTransactionReason.StorageMaintenance,
                "倉庫維持費",
                currentDay - 1);
        }

        foreach (QuestRecord quest in quests)
        {
            if (!quest.completed &&
                quest.deadlineDay > 0 &&
                currentDay > quest.deadlineDay)
            {
                quest.expired = true;
            }
        }
        GenerateNormalQuestsIfNeeded();
        ProgressionChanged?.Invoke();
    }

    private void GenerateNormalQuestsIfNeeded()
    {
        ResolveReferences();
        int currentTownIndex = GetCurrentTownIndex();
        int activeNormal = quests.FindAll(q =>
            q != null && !q.isSpecial && !q.completed && !q.expired &&
            q.issuedTownIndex == currentTownIndex).Count;
        while (activeNormal < 3)
        {
            QuestRecord quest = CreateRandomQuest(currentTownIndex);
            if (quest == null)
            {
                break;
            }
            quests.Add(quest);
            activeNormal++;
        }
    }

    private QuestRecord CreateRandomQuest(int townIndex)
    {
        List<ItemDataSO> materials = FindMaterials(townIndex);
        List<EnemyDataSO> enemies = FindHuntEnemies(townIndex);
        if (materials.Count == 0 && enemies.Count == 0)
        {
            return null;
        }
        bool delivery = materials.Count > 0 &&
            (enemies.Count == 0 || UnityEngine.Random.value < 0.5f);
        int day = dayManager != null ? dayManager.CurrentDay : 1;
        if (delivery)
        {
            ItemDataSO item = materials[UnityEngine.Random.Range(0, materials.Count)];
            return new QuestRecord
            {
                title = "商会への納品依頼",
                questType = QuestType.ItemDelivery,
                targetName = item.itemName,
                targetPersistentId = item.PersistentId,
                requiredAmount = UnityEngine.Random.Range(2, 6),
                deadlineDay = day + UnityEngine.Random.Range(3, 7),
                goldReward = 180,
                experienceReward = 35,
                issuedTownIndex = townIndex,
                questId = Guid.NewGuid().ToString("N")
            };
        }

        EnemyDataSO enemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        string enemyName = enemy.enemyName;
        return new QuestRecord
        {
            title = $"{JapaneseDisplayText.GetEnemyName(enemyName)}討伐依頼",
            questType = QuestType.MonsterHunt,
            targetName = enemyName,
            targetPersistentId = enemy.PersistentId,
            requiredAmount = UnityEngine.Random.Range(2, 5),
            deadlineDay = day + UnityEngine.Random.Range(4, 8),
            goldReward = 250,
            experienceReward = 50,
            issuedTownIndex = townIndex,
            questId = Guid.NewGuid().ToString("N")
        };
    }

    private void PopulateSpecialQuests()
    {
        foreach (SpecialQuestSO definition in
                 GameAssetRepository.LoadAll<SpecialQuestSO>())
        {
            if (!specialQuestDefinitions.Contains(definition))
            {
                specialQuestDefinitions.Add(definition);
            }
        }
        foreach (SpecialQuestSO definition in specialQuestDefinitions)
        {
            if (definition == null ||
                quests.Exists(q => q.specialQuestId == definition.name))
            {
                continue;
            }
            QuestRecord quest = definition.CreateRecord();
            quest.issuedTownIndex = GetCurrentTownIndex();
            quest.questId = Guid.NewGuid().ToString("N");
            quests.Add(quest);
        }
    }

    private List<ItemDataSO> FindMaterials(int townIndex)
    {
        List<ItemDataSO> result = new List<ItemDataSO>();
        foreach (ItemDataSO item in FindAllItems())
        {
            if (item != null &&
                item.itemType == ItemType.Material &&
                item.itemName.IndexOf("Enhancement", StringComparison.Ordinal) < 0 &&
                IsItemAvailableNearTown(item, townIndex))
            {
                result.Add(item);
            }
        }
        return result;
    }

    private ItemDataSO FindItem(string persistentId, string itemName)
    {
        ItemDataSO item = GameAssetRepository.FindByPersistentId<ItemDataSO>(
            persistentId,
            string.Empty);
        return item ?? FindAllItems().Find(value =>
            value != null && value.itemName == itemName);
    }

    private List<ItemDataSO> FindAllItems()
    {
        return new List<ItemDataSO>(
            GameAssetRepository.LoadAll<ItemDataSO>());
    }

    private bool IsQuestAvailableHere(QuestRecord quest)
    {
        if (quest == null || quest.completed || quest.expired ||
            quest.issuedTownIndex != GetCurrentTownIndex())
        {
            return false;
        }
        if (quest.deadlineDay > 0 && dayManager != null &&
            dayManager.CurrentDay > quest.deadlineDay)
        {
            quest.expired = true;
            return false;
        }
        return IsQuestAchievableInTown(quest, GetCurrentTownIndex());
    }

    private bool IsQuestAchievableInTown(QuestRecord quest, int townIndex)
    {
        if (quest.questType == QuestType.ItemDelivery)
        {
            return IsItemAvailableNearTown(
                FindItem(quest.targetPersistentId, quest.targetName), townIndex);
        }
        return FindHuntEnemies(townIndex).Exists(enemy => MatchesEnemy(quest, enemy));
    }

    private bool IsItemAvailableNearTown(ItemDataSO item, int townIndex)
    {
        if (item == null)
        {
            return false;
        }
        foreach (DungeonDataSO dungeon in ItemUsageTextBuilder.GetClearRewardDungeons(item))
        {
            if (IsUnlockedDungeonNearTown(dungeon, townIndex)) return true;
        }
        foreach (EnemyDataSO enemy in ItemUsageTextBuilder.GetDropEnemies(item))
        {
            foreach (DungeonDataSO dungeon in GetUnlockedDungeonsNearTown(townIndex))
            {
                if (DungeonContainsEnemy(dungeon, enemy)) return true;
            }
        }
        return false;
    }

    private List<EnemyDataSO> FindHuntEnemies(int townIndex)
    {
        List<EnemyDataSO> result = new List<EnemyDataSO>();
        foreach (DungeonDataSO dungeon in GetUnlockedDungeonsNearTown(townIndex))
        {
            AddHuntEnemies(result, dungeon.normalEnemies);
        }
        foreach (int adjacentTown in GetAdjacentTownIndices(townIndex))
        {
            if (roadEncounterService != null)
            {
                AddHuntEnemies(result, roadEncounterService.GetPotentialEnemiesForRoute(townIndex, adjacentTown));
            }
            else if (dungeonRunManager != null)
            {
                AddHuntEnemies(
                    result,
                    dungeonRunManager.GetHighestGradeDungeonNearTown(townIndex)?.normalEnemies);
                AddHuntEnemies(
                    result,
                    dungeonRunManager.GetHighestGradeDungeonNearTown(adjacentTown)?.normalEnemies);
            }
        }
        return result;
    }

    private List<DungeonDataSO> GetUnlockedDungeonsNearTown(int townIndex)
    {
        List<DungeonDataSO> result = new List<DungeonDataSO>();
        IEnumerable<DungeonDataSO> candidates = dungeonRunManager != null
            ? dungeonRunManager.AvailableDungeons
            : GameAssetRepository.LoadAll<DungeonDataSO>();
        foreach (DungeonDataSO dungeon in candidates)
        {
            if (IsUnlockedDungeonNearTown(dungeon, townIndex)) result.Add(dungeon);
        }
        return result;
    }

    private bool IsUnlockedDungeonNearTown(DungeonDataSO dungeon, int townIndex)
    {
        return dungeon != null && dungeon.nearbyTownIndex == townIndex &&
               (dungeonRunManager == null || dungeonRunManager.IsDungeonUnlocked(dungeon));
    }

    private static bool DungeonContainsEnemy(
        DungeonDataSO dungeon,
        EnemyDataSO targetEnemy)
    {
        if (dungeon?.normalEnemies == null || targetEnemy == null)
        {
            return false;
        }
        foreach (EnemyDataSO enemy in dungeon.normalEnemies)
        {
            if (enemy == targetEnemy)
            {
                return true;
            }
        }
        return false;
    }

    private static void AddHuntEnemies(List<EnemyDataSO> destination, IEnumerable<EnemyDataSO> source)
    {
        if (source == null) return;
        foreach (EnemyDataSO enemy in source)
        {
            if (enemy != null && !enemy.isBoss && !destination.Contains(enemy)) destination.Add(enemy);
        }
    }

    private static List<int> GetAdjacentTownIndices(int townIndex)
    {
        List<int> result = new List<int>();
        for (int index = 0; index < WorldMapService.TownCount; index++)
        {
            if (townIndex != WorldMapService.HiddenIslandTownIndex &&
                index != WorldMapService.HiddenIslandTownIndex &&
                WorldMapService.AreTownsAdjacent(townIndex, index))
            {
                result.Add(index);
            }
        }
        return result;
    }

    private static bool MatchesEnemy(QuestRecord quest, EnemyDataSO enemy)
    {
        return enemy != null && (!string.IsNullOrWhiteSpace(quest.targetPersistentId)
            ? quest.targetPersistentId == enemy.PersistentId
            : quest.targetName == enemy.enemyName);
    }

    private int GetCurrentTownIndex()
    {
        return townProgressState != null ? townProgressState.CurrentTownIndex : 2;
    }

    private void MigrateQuestRecords()
    {
        foreach (QuestRecord quest in quests)
        {
            if (quest == null) continue;
            if (!WorldMapService.IsValidTownIndex(quest.issuedTownIndex)) quest.issuedTownIndex = GetCurrentTownIndex();
            if (string.IsNullOrWhiteSpace(quest.questId)) quest.questId = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(quest.targetPersistentId))
            {
                MigrateQuestTargetPersistentId(quest);
            }
        }
    }

    private void MigrateQuestTargetPersistentId(QuestRecord quest)
    {
        if (quest.questType == QuestType.ItemDelivery)
        {
            quest.targetPersistentId =
                FindItem(string.Empty, quest.targetName)?.PersistentId;
            return;
        }

        List<EnemyDataSO> matchingEnemies = FindHuntEnemies(
            quest.issuedTownIndex).FindAll(enemy =>
            enemy != null && enemy.enemyName == quest.targetName);
        if (matchingEnemies.Count == 1)
        {
            quest.targetPersistentId = matchingEnemies[0].PersistentId;
            return;
        }

        bool hasKnownEnemyName = new List<EnemyDataSO>(
            GameAssetRepository.LoadAll<EnemyDataSO>()).Exists(enemy =>
            enemy != null && enemy.enemyName == quest.targetName);
        if (matchingEnemies.Count == 0 && !hasKnownEnemyName)
        {
            quest.expired = true;
        }
    }

    /// <summary>
    /// 参照を一度だけ解決する。CanStore など高頻度で呼ばれる API から毎回
    /// 呼ばれるため、解決済みの参照は再解決しない。
    ///
    /// 判定に ?? を使わないのは、Unity の未設定 SerializeField や破棄済み
    /// コンポーネントが C# 参照としては "fake null" になり得るため。?? では
    /// 未解決のまま素通りし、呼ばれるたびに FindObjectOfType でシーン全体を
    /// 走査し続けることになる。
    /// </summary>
    private void ResolveReferences()
    {
        Resolve(ref merchantData);
        Resolve(ref dayManager);
        Resolve(ref inventory);
        Resolve(ref battleManager);
        Resolve(ref dungeonRunManager);
        Resolve(ref townProgressState);
        Resolve(ref roadEncounterService);
        Resolve(ref debtManager);
    }

    /// <summary>
    /// 既に解決済みなら何もしない。未解決のときだけ、同一 GameObject →
    /// シーン全体の順に探す。
    /// </summary>
    private void Resolve<T>(ref T reference) where T : Component
    {
        if (reference != null)
        {
            return;
        }

        reference = GetComponent<T>();
        if (reference == null)
        {
            reference = FindObjectOfType<T>();
        }
    }
}

[Serializable]
public class QuestRecord
{
    public string questId;
    public string title;
    public QuestType questType;
    public string targetName;
    public string targetPersistentId;
    public int issuedTownIndex = -1;
    public int requiredAmount;
    public int currentAmount;
    public int deadlineDay;
    public int goldReward;
    public int experienceReward;
    public bool accepted;
    public bool completed;
    public bool expired;
    public bool isSpecial;
    public string specialQuestId;
}

public sealed class QuestCompletionInfo
{
    public QuestRecord Quest;
    public int DeliveredAmount;
    public int GoldReward;
    public int TownIndex;
}

public enum QuestType
{
    ItemDelivery,
    MonsterHunt
}

[Serializable]
public class ProgressionSaveData
{
    public int storageTier;
    public int totalDungeonClears;
    public int profitableDungeonClears;
    public List<QuestRecord> quests = new List<QuestRecord>();
}
