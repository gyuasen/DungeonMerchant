using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private DungeonRunManager dungeonRunManager;
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
        new[] { 30, 60, 100, 160 }[storageTier] +
        (merchantData != null ? merchantData.GetStorageCapacityBonus() : 0);
    public int StorageUpgradeCost => new[] { 1500, 5000, 12000, 0 }[storageTier];
    public int StorageMaintenanceCost => storageTier >= 2
        ? 100 * (storageTier - 1)
        : 0;
    public int TotalDungeonClears => totalDungeonClears;
    public int TotalGoldEarned => merchantData != null
        ? merchantData.LifetimeGoldEarned
        : 0;
    public int ProfitableDungeonClears => profitableDungeonClears;
    public string LastExplorationResult { get; private set; } = string.Empty;

    public event Action ProgressionChanged;

    private void OnEnable()
    {
        ResolveReferences();
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

    public bool TryUpgradeStorage()
    {
        ResolveReferences();
        if (storageTier >= 3)
        {
            return false;
        }

        int requiredLevel = new[] { 1, 4, 8, 12 }[storageTier + 1];
        int cost = StorageUpgradeCost;
        if (merchantData == null ||
            merchantData.MerchantLevel < requiredLevel ||
            !merchantData.TryPayGold(cost))
        {
            return false;
        }

        storageTier++;
        ProgressionChanged?.Invoke();
        return true;
    }

    public bool AcceptQuest(int index)
    {
        if (index < 0 ||
            index >= quests.Count ||
            quests[index].accepted ||
            quests[index].expired ||
            quests[index].completed)
        {
            return false;
        }
        quests[index].accepted = true;
        TryCompleteQuest(quests[index]);
        ProgressionChanged?.Invoke();
        return true;
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
        storageTier = Mathf.Clamp(data.storageTier, 0, 3);
        totalDungeonClears = Mathf.Max(0, data.totalDungeonClears);
        profitableDungeonClears = Mathf.Max(0, data.profitableDungeonClears);
        quests = data.quests ?? new List<QuestRecord>();
        GenerateNormalQuestsIfNeeded();
        ProgressionChanged?.Invoke();
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        ResolveReferences();
        int days = 1;
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
        merchantData?.TryPayGold(Mathf.Min(goldBefore, expense));
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
                    quest.targetName == enemy.enemyName)
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
            ItemDataSO item = FindItem(quest.targetName);
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
        }

        if (quest.currentAmount < quest.requiredAmount)
        {
            return;
        }

        quest.completed = true;
        merchantData?.AddGold(GetQuestGoldReward(quest));
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
                Mathf.Min(merchantData.Gold, StorageMaintenanceCost));
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
        int activeNormal = quests.FindAll(q =>
            !q.isSpecial && !q.completed && !q.expired).Count;
        while (activeNormal < 3)
        {
            quests.Add(CreateRandomQuest());
            activeNormal++;
        }
    }

    private QuestRecord CreateRandomQuest()
    {
        bool delivery = UnityEngine.Random.value < 0.5f;
        int day = dayManager != null ? dayManager.CurrentDay : 1;
        if (delivery)
        {
            List<ItemDataSO> materials = FindMaterials();
            ItemDataSO item = materials.Count > 0
                ? materials[UnityEngine.Random.Range(0, materials.Count)]
                : null;
            return new QuestRecord
            {
                title = "商会への納品依頼",
                questType = QuestType.ItemDelivery,
                targetName = item != null ? item.itemName : "Monster Fang",
                requiredAmount = UnityEngine.Random.Range(2, 6),
                deadlineDay = day + UnityEngine.Random.Range(3, 7),
                goldReward = 180,
                experienceReward = 35
            };
        }

        string[] enemies = { "Slime", "Goblin", "Orc", "Skeleton" };
        string enemyName = enemies[UnityEngine.Random.Range(0, enemies.Length)];
        return new QuestRecord
        {
            title = $"{JapaneseDisplayText.GetEnemyName(enemyName)}討伐依頼",
            questType = QuestType.MonsterHunt,
            targetName = enemyName,
            requiredAmount = UnityEngine.Random.Range(2, 5),
            deadlineDay = day + UnityEngine.Random.Range(4, 8),
            goldReward = 250,
            experienceReward = 50
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
            quests.Add(definition.CreateRecord());
        }
    }

    private List<ItemDataSO> FindMaterials()
    {
        List<ItemDataSO> result = new List<ItemDataSO>();
        foreach (ItemDataSO item in FindAllItems())
        {
            if (item != null &&
                item.itemType == ItemType.Material &&
                item.itemName.IndexOf("Enhancement", StringComparison.Ordinal) < 0)
            {
                result.Add(item);
            }
        }
        return result;
    }

    private ItemDataSO FindItem(string itemName)
    {
        return FindAllItems().Find(item =>
            item != null && item.itemName == itemName);
    }

    private List<ItemDataSO> FindAllItems()
    {
        return new List<ItemDataSO>(
            GameAssetRepository.LoadAll<ItemDataSO>());
    }

    private void ResolveReferences()
    {
        merchantData = merchantData ?? GetComponent<MerchantData>() ??
            FindObjectOfType<MerchantData>();
        dayManager = dayManager ?? GetComponent<DayManager>() ??
            FindObjectOfType<DayManager>();
        inventory = inventory ?? GetComponent<MerchantInventory>() ??
            FindObjectOfType<MerchantInventory>();
        battleManager = battleManager ?? GetComponent<BattleManager>() ??
            FindObjectOfType<BattleManager>();
        dungeonRunManager = dungeonRunManager ?? GetComponent<DungeonRunManager>() ??
            FindObjectOfType<DungeonRunManager>();
        debtManager = debtManager ?? GetComponent<DebtManager>() ??
            FindObjectOfType<DebtManager>();
    }
}

[Serializable]
public class QuestRecord
{
    public string title;
    public QuestType questType;
    public string targetName;
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
