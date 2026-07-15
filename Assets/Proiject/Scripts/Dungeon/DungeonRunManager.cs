using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRunManager : MonoBehaviour
{
    private const int DefaultMaxEnemyCountPerEncounter = 5;

    [Header("References")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private DungeonDataSO dungeonData;
    [SerializeField] private List<DungeonDataSO> availableDungeons =
        new List<DungeonDataSO>();
    private readonly HashSet<int> unlockedTownIndices = new HashSet<int> { 2 };
    private readonly DungeonProgressStore progressStore =
        new DungeonProgressStore();
    private DungeonRewardService rewardService;
    [SerializeField, Min(0)] private int currentWorldMapIndex;

    [Header("Run Settings")]
    [SerializeField, Min(1)] private int encounterCount = 3;
    [SerializeField, Min(1)] private int firstEncounterEnemyCount = 2;
    [SerializeField, Min(0)] private int enemyCountIncreasePerEncounter = 1;

    [Header("Event Settings")]
    [SerializeField, Min(0)] private int restHealAmount = 15;
    [SerializeField, Min(0)] private int treasureGoldReward = 40;
    [SerializeField, Min(0)] private int hazardDamage = 5;

    public bool IsRunning { get; private set; }
    public bool IsAwaitingEventChoice { get; private set; }
    public int CurrentEncounter { get; private set; }
    private int currentRunEncounterCount;
    public int EncounterCount => IsRunning
        ? Mathf.Max(1, currentRunEncounterCount)
        : dungeonData != null
            ? Mathf.Max(1, dungeonData.encounterCount)
            : encounterCount;
    public string DungeonName => dungeonData != null
        ? dungeonData.dungeonName
        : "ダンジョン";
    public string DungeonDescription => dungeonData != null
        ? dungeonData.description
        : string.Empty;
    public int ClearGoldReward => dungeonData != null
        ? Mathf.Max(0, dungeonData.clearGoldReward)
        : 0;
    public int CurrentFloor => GetCurrentFloor(dungeonData);
    public int TotalFloors => dungeonData != null
        ? Mathf.Max(1, dungeonData.totalFloors)
        : 1;
    public bool IsSelectedDungeonFullyCleared =>
        dungeonData != null &&
        GetClearedFloors(dungeonData) >= TotalFloors;
    public IReadOnlyList<DungeonDataSO> AvailableDungeons => availableDungeons;
    public DungeonDataSO SelectedDungeon => dungeonData;
    public DungeonGrade HighestUnlockedGrade => progressStore.HighestUnlockedGrade;
    public int CurrentWorldMapIndex => currentWorldMapIndex;
    public string EventTitle => eventState.Presentation.Title;
    public string EventDescription => eventState.Presentation.Description;
    public string FirstOptionLabel => eventState.Presentation.FirstOptionLabel;
    public string SecondOptionLabel => eventState.Presentation.SecondOptionLabel;
    public string ThirdOptionLabel => eventState.Presentation.ThirdOptionLabel;

    public string GetEventOptionPreview(int optionIndex)
    {
        return DungeonEventService.CreateChoicePreview(
            eventState.Type,
            optionIndex,
            restHealAmount,
            treasureGoldReward,
            hazardDamage);
    }

    public string GetEventOptionImageKey(int optionIndex)
    {
        return DungeonEventService.GetChoiceImageKey(
            eventState.Type,
            optionIndex);
    }

    public event Action<string> DungeonMessage;
    public event Action DungeonStateChanged;
    public event Action<bool> DungeonCompleted;

    private void OnEnable()
    {
        progressStore.Load();
        ResolveReferences();
        PopulateDungeonDataIfNeeded();
    }

    private void OnDestroy()
    {
        if (battleManager != null)
        {
            battleManager.BattleCompleted -= HandleBattleCompleted;
        }
    }

    public bool StartRun()
    {
        ResolveReferences();
        PopulateDungeonDataIfNeeded();

        if (IsRunning)
        {
            SendDungeonMessage("すでにダンジョン探索中です。");
            return false;
        }

        if (battleManager == null || partyManager == null)
        {
            SendDungeonMessage("ダンジョン探索に必要な参照が不足しています。");
            return false;
        }

        if (partyManager.Members.Count == 0)
        {
            SendDungeonMessage("ダンジョンへ入る前に傭兵を編成してください。");
            return false;
        }

        IsRunning = true;
        progressionManager?.StartExploration();
        IsAwaitingEventChoice = false;
        CurrentEncounter = 0;
        currentRunEncounterCount = UnityEngine.Random.Range(3, 6);
        ClearEvent();
        SubscribeToBattle();
        SendDungeonMessage(
            $"{DungeonName} 第{CurrentFloor}/{TotalFloors}フロアの探索開始。" +
            $"遭遇回数: {EncounterCount}");
        DungeonStateChanged?.Invoke();
        return StartNextEncounter();
    }

    public void AbandonRun()
    {
        if (!IsRunning || (battleManager != null && battleManager.IsBattling))
        {
            return;
        }

        CompleteRun(false, "ダンジョン探索を中断しました。");
    }

    public bool IsDungeonUnlocked(DungeonDataSO data)
    {
        return data != null &&
               data.worldMapIndex == currentWorldMapIndex &&
               unlockedTownIndices.Contains(data.nearbyTownIndex);
    }

    public DungeonDataSO GetDungeonNearTown(int townIndex)
    {
        PopulateDungeonDataIfNeeded();
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (data != null && data.nearbyTownIndex == townIndex)
            {
                return data;
            }
        }

        return null;
    }

    public DungeonDataSO GetHighestGradeDungeonNearTown(int townIndex)
    {
        PopulateDungeonDataIfNeeded();
        DungeonDataSO result = null;
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (data == null || data.nearbyTownIndex != townIndex)
            {
                continue;
            }

            if (result == null || data.grade > result.grade)
            {
                result = data;
            }
        }
        return result;
    }

    public void SetCurrentWorldMapIndex(int worldMapIndex)
    {
        currentWorldMapIndex = Mathf.Max(0, worldMapIndex);
        PopulateDungeonDataIfNeeded();
        if (dungeonData == null ||
            dungeonData.worldMapIndex != currentWorldMapIndex ||
            !IsDungeonUnlocked(dungeonData))
        {
            dungeonData = FindFirstUnlockedDungeon();
        }
        DungeonStateChanged?.Invoke();
    }

    public int GetClearedFloors(DungeonDataSO data)
    {
        return progressStore.GetClearedFloors(data);
    }

    public int GetCurrentFloor(DungeonDataSO data)
    {
        if (data == null)
        {
            return 1;
        }

        int totalFloors = Mathf.Max(1, data.totalFloors);
        return Mathf.Min(GetClearedFloors(data) + 1, totalFloors);
    }

    public List<SavedDungeonFloorProgress> CreateFloorProgressSaveData()
    {
        return progressStore.CreateFloorProgressSaveData(availableDungeons);
    }

    public void SetUnlockedTownIndices(IReadOnlyList<int> townIndices)
    {
        unlockedTownIndices.Clear();
        unlockedTownIndices.Add(2);

        if (townIndices != null)
        {
            foreach (int townIndex in townIndices)
            {
                if (WorldMapService.IsValidTownIndex(townIndex))
                {
                    unlockedTownIndices.Add(townIndex);
                }
            }
        }

        PopulateDungeonDataIfNeeded();
        if (dungeonData == null || !IsDungeonUnlocked(dungeonData))
        {
            dungeonData = FindFirstUnlockedDungeon();
        }
        DungeonStateChanged?.Invoke();
    }

    public bool TrySelectDungeon(DungeonDataSO data)
    {
        if (IsRunning || data == null || !IsDungeonUnlocked(data))
        {
            return false;
        }

        dungeonData = data;
        SendDungeonMessage(
            $"{JapaneseDisplayText.GetDungeonGrade(data.grade)}「{data.dungeonName}」を選択しました。");
        DungeonStateChanged?.Invoke();
        return true;
    }

    public void RestoreProgress(
        DungeonGrade restoredHighestGrade,
        string selectedDungeonAssetName,
        string selectedDungeonPersistentId,
        IReadOnlyList<SavedDungeonFloorProgress> savedFloorProgress = null)
    {
        PopulateDungeonDataIfNeeded();
        progressStore.RestoreProgress(
            restoredHighestGrade,
            savedFloorProgress,
            availableDungeons);

        DungeonDataSO restoredSelection = progressStore.ResolveDungeon(
            selectedDungeonPersistentId,
            selectedDungeonAssetName,
            availableDungeons);
        if (restoredSelection != null &&
            !IsDungeonUnlocked(restoredSelection))
        {
            restoredSelection = null;
        }

        dungeonData = restoredSelection ?? FindFirstUnlockedDungeon();
        DungeonStateChanged?.Invoke();
    }

    [ContextMenu("ダンジョン開放状態を初期化")]
    public void ResetDungeonProgress()
    {
        progressStore.Reset();

        PopulateDungeonDataIfNeeded();
        dungeonData = FindFirstUnlockedDungeon();
        SendDungeonMessage("ダンジョンのフロア進行を初期化しました。");
        DungeonStateChanged?.Invoke();
    }

    public bool ChooseEventOption(int optionIndex)
    {
        if (!IsRunning || !IsAwaitingEventChoice || optionIndex < 0 || optionIndex > 2)
        {
            return false;
        }

        DungeonEventType eventType = eventState.Type;
        IsAwaitingEventChoice = false;

        if (optionIndex == 2)
        {
            CompleteRun(false, "パーティーは安全に撤退しました。");
            return true;
        }

        ResolveEventChoice(eventType, optionIndex);
        ClearEvent();
        DungeonStateChanged?.Invoke();
        return StartNextEncounter();
    }

    private bool StartNextEncounter()
    {
        if (!IsRunning)
        {
            return false;
        }

        CurrentEncounter++;
        if (CurrentEncounter > EncounterCount)
        {
            CompleteRun(true, "ダンジョンを踏破しました。");
            return true;
        }

        int firstEnemyCount = dungeonData != null
            ? Mathf.Max(1, dungeonData.firstEncounterEnemyCount)
            : firstEncounterEnemyCount;
        int enemyIncrease = dungeonData != null
            ? Mathf.Max(0, dungeonData.enemyCountIncreasePerEncounter)
            : enemyCountIncreasePerEncounter;
        int floorIncrease = dungeonData != null
            ? Mathf.Max(0, dungeonData.enemyCountIncreasePerFloor)
            : 0;
        int enemyCount =
            firstEnemyCount +
            ((CurrentEncounter - 1) * enemyIncrease) +
            ((CurrentFloor - 1) * floorIncrease);
        enemyCount = Mathf.Min(enemyCount, GetMaxEnemyCountPerEncounter());
        List<EnemyDataSO> enemies = CreateDungeonEncounter(enemyCount);

        SendDungeonMessage(
            $"遭遇 {CurrentEncounter}/{EncounterCount}: " +
            $"敵が{enemyCount}体出現しました。");
        DungeonStateChanged?.Invoke();

        battleManager.SetNextBattleBackground(
            dungeonData != null ? dungeonData.battleBackground : null,
            dungeonData != null &&
            !string.IsNullOrWhiteSpace(dungeonData.battleBackgroundKey)
                ? dungeonData.battleBackgroundKey
                : dungeonData != null
                    ? dungeonData.name
                    : null);
        bool started = battleManager.StartBattle(partyManager.Members, enemies);
        if (!started)
        {
            CompleteRun(false, "戦闘を開始できないため探索を終了しました。");
        }

        return started;
    }

    private int GetMaxEnemyCountPerEncounter()
    {
        return dungeonData != null
            ? Mathf.Max(1, dungeonData.maxEnemyCountPerEncounter)
            : DefaultMaxEnemyCountPerEncounter;
    }

    private List<EnemyDataSO> CreateDungeonEncounter(int enemyCount)
    {
        List<EnemyDataSO> enemies = new List<EnemyDataSO>();
        bool isFinalFloor = CurrentFloor >= TotalFloors;
        bool isBossEncounter =
            isFinalFloor && CurrentEncounter >= EncounterCount;

        if (dungeonData == null ||
            dungeonData.normalEnemies == null ||
            dungeonData.normalEnemies.Length == 0)
        {
            return battleManager.CreateDefaultEnemyEncounter(enemyCount);
        }

        int normalEnemyCount = isBossEncounter && dungeonData.bossEnemy != null
            ? Mathf.Max(0, enemyCount - 1)
            : enemyCount;
        bool specialVariantAdded = false;

        for (int i = 0; i < normalEnemyCount; i++)
        {
            EnemyDataSO enemy = GetRandomNormalEnemy();
            if (enemy != null)
            {
                if (!specialVariantAdded &&
                    enemy.category == EnemyCategory.Normal &&
                    UnityEngine.Random.value <
                    dungeonData.specialVariantChance)
                {
                    enemy = DungeonEnemyVariantService.CreateSpecialVariant(
                        enemy,
                        dungeonData.specialVariantSkillPool,
                        dungeonData.grade,
                        false);
                    specialVariantAdded = enemy != null &&
                                          enemy.isSpecialVariant;
                }
                enemies.Add(enemy);
            }
        }

        if (isBossEncounter && dungeonData.bossEnemy != null)
        {
            EnemyDataSO boss = dungeonData.bossEnemy;
            bool hasPreviouslyFullyCleared =
                GetClearedFloors(dungeonData) >= TotalFloors;
            if (hasPreviouslyFullyCleared &&
                UnityEngine.Random.value < dungeonData.specialBossChance)
            {
                boss = DungeonEnemyVariantService.CreateSpecialVariant(
                    boss,
                    dungeonData.specialVariantSkillPool,
                    dungeonData.grade,
                    true);
            }
            enemies.Add(boss);
        }

        return enemies.Count > 0
            ? enemies
            : battleManager.CreateDefaultEnemyEncounter(enemyCount);
    }

    private EnemyDataSO GetRandomNormalEnemy()
    {
        if (dungeonData?.normalEnemies == null)
        {
            return null;
        }

        List<EnemyDataSO> validEnemies = new List<EnemyDataSO>();
        foreach (EnemyDataSO enemy in dungeonData.normalEnemies)
        {
            if (enemy != null && !enemy.isBoss)
            {
                validEnemies.Add(enemy);
            }
        }

        return validEnemies.Count > 0
            ? validEnemies[UnityEngine.Random.Range(0, validEnemies.Count)]
            : null;
    }

    private void HandleBattleCompleted(bool victory)
    {
        if (!IsRunning)
        {
            return;
        }

        if (!victory)
        {
            CompleteRun(false, "ダンジョン探索に失敗しました。");
            return;
        }

        SendDungeonMessage(
            $"遭遇 {CurrentEncounter}/{EncounterCount} を突破しました。");

        if (CurrentEncounter >= EncounterCount)
        {
            CompleteCurrentFloor();
            return;
        }

        PresentRandomEvent();
    }

    private void CompleteRun(bool cleared, string message)
    {
        IsRunning = false;
        IsAwaitingEventChoice = false;
        ClearEvent();
        SendDungeonMessage(message);
        DungeonStateChanged?.Invoke();
        DungeonCompleted?.Invoke(cleared);
    }

    private void CompleteCurrentFloor()
    {
        if (dungeonData == null)
        {
            CompleteRun(false, "ダンジョン情報が見つかりません。");
            return;
        }

        int completedFloor = CurrentFloor;
        int totalFloors = TotalFloors;
        progressStore.RecordClearedFloor(dungeonData, completedFloor);

        int floorReward = Mathf.Max(0, dungeonData.floorClearGoldReward);
        if (floorReward > 0)
        {
            GrantGold(floorReward);
        }

        if (completedFloor >= totalFloors)
        {
            TryGrantLimitedEquipment(
                dungeonData.bossLimitedDropChance,
                "最終フロア限定ドロップ");
            GrantClearRewards();
            CompleteRun(
                true,
                $"{DungeonName}の全{totalFloors}フロアを完全攻略しました。");
            return;
        }

        CompleteRun(
            true,
            $"{DungeonName} 第{completedFloor}フロアを攻略しました。" +
            $"次回は第{completedFloor + 1}フロアから開始します。");
    }

    private DungeonEventState eventState = DungeonEventState.Empty;

    private void PresentRandomEvent()
    {
        DungeonEventType eventType = DungeonEventService.RollRandomEvent();
        IsAwaitingEventChoice = true;

        DungeonEventPresentation presentation =
            DungeonEventService.CreatePresentation(
                eventType,
                restHealAmount,
                treasureGoldReward,
                hazardDamage);
        eventState = new DungeonEventState(eventType, presentation);
        SendDungeonMessage($"ダンジョンイベント: {EventTitle}");
        DungeonStateChanged?.Invoke();
    }

    private void ResolveEventChoice(DungeonEventType eventType, int optionIndex)
    {
        DungeonEventChoiceResult result =
            DungeonEventService.ResolveChoice(
                eventType,
                optionIndex,
                restHealAmount,
                treasureGoldReward,
                hazardDamage);

        if (result.HealAmount > 0)
        {
            HealParty(result.HealAmount);
        }

        if (result.DamageAmount > 0)
        {
            DamageParty(result.DamageAmount);
        }

        if (result.GoldAmount > 0)
        {
            GrantGold(result.GoldAmount);
        }

        if (!string.IsNullOrEmpty(result.LimitedDropSourceLabel))
        {
            TryGrantLimitedEquipment(
                dungeonData != null
                    ? dungeonData.eventLimitedDropChance
                    : 0f,
                result.LimitedDropSourceLabel);
        }

        if (result.AddExplorationDelay)
        {
            progressionManager?.AddExplorationDelay(1);
        }
    }

    private void HealParty(int amount)
    {
        foreach (MercenaryInstance mercenary in partyManager.Members)
        {
            mercenary?.Heal(amount);
        }

        SendDungeonMessage($"パーティー全員が{amount} HP回復しました。");
    }

    private void DamageParty(int amount)
    {
        foreach (MercenaryInstance mercenary in partyManager.Members)
        {
            mercenary?.TakeDamage(amount);
        }

        SendDungeonMessage($"強行中にパーティー全員が{amount}ダメージを受けました。");
    }

    private void GrantGold(int amount)
    {
        ResolveReferences();
        rewardService?.GrantGold(amount);
    }

    private void GrantClearRewards()
    {
        ResolveReferences();
        rewardService?.GrantClearRewards(dungeonData);
    }

    private void TryGrantLimitedEquipment(float chance, string sourceLabel)
    {
        ResolveReferences();
        rewardService?.TryGrantLimitedEquipment(
            dungeonData,
            chance,
            sourceLabel);
    }

    private void PopulateDungeonDataIfNeeded()
    {
        RemoveMissingDungeons();

        foreach (DungeonDataSO data in
                 GameAssetRepository.LoadAll<DungeonDataSO>())
        {
            AddDungeon(data);
        }

        availableDungeons.Sort((left, right) =>
        {
            int townComparison =
                left.nearbyTownIndex.CompareTo(right.nearbyTownIndex);
            return townComparison != 0
                ? townComparison
                : left.grade.CompareTo(right.grade);
        });

        if (dungeonData == null || !IsDungeonUnlocked(dungeonData))
        {
            dungeonData = FindFirstUnlockedDungeon();
        }
    }

    private void AddDungeon(DungeonDataSO data)
    {
        if (data != null && !availableDungeons.Contains(data))
        {
            availableDungeons.Add(data);
        }
    }

    private void RemoveMissingDungeons()
    {
        for (int i = availableDungeons.Count - 1; i >= 0; i--)
        {
            if (availableDungeons[i] == null)
            {
                availableDungeons.RemoveAt(i);
            }
        }
    }

    private DungeonDataSO FindFirstUnlockedDungeon()
    {
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (data != null &&
                data.worldMapIndex == currentWorldMapIndex &&
                IsDungeonUnlocked(data))
            {
                return data;
            }
        }

        return null;
    }

    private void ClearEvent()
    {
        eventState = DungeonEventState.Empty;
    }

    private void SubscribeToBattle()
    {
        battleManager.BattleCompleted -= HandleBattleCompleted;
        battleManager.BattleCompleted += HandleBattleCompleted;
    }

    private void ResolveReferences()
    {
        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>();
        }

        if (partyManager == null)
        {
            partyManager = FindObjectOfType<MercenaryPartyManager>();
        }

        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>();
        }

        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = GetComponent<MerchantInventory>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = FindObjectOfType<MerchantInventory>();
        }

        if (progressionManager == null)
        {
            progressionManager = GetComponent<ProgressionManager>() ??
                                 FindObjectOfType<ProgressionManager>();
        }

        PopulateDungeonDataIfNeeded();
        rewardService = new DungeonRewardService(
            merchantData,
            merchantInventory,
            SendDungeonMessage);
    }

    private void SendDungeonMessage(string message)
    {
        Debug.Log(message);
        DungeonMessage?.Invoke(message);
    }
}

public enum DungeonEventType
{
    None,
    AbandonedCamp,
    TreasureCache,
    CollapsedPassage
}
