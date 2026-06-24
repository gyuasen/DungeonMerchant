using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DungeonRunManager : MonoBehaviour
{
    private const string UnlockedGradeSaveKey =
        "DungeonMerchant.Dungeon.HighestUnlockedGrade";

    [Header("References")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private DungeonDataSO dungeonData;
    [SerializeField] private List<DungeonDataSO> availableDungeons =
        new List<DungeonDataSO>();
    [SerializeField] private DungeonGrade highestUnlockedGrade = DungeonGrade.Low;
    private readonly HashSet<int> unlockedTownIndices = new HashSet<int> { 2 };
    private readonly Dictionary<string, int> clearedFloorsByDungeon =
        new Dictionary<string, int>();
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
    public int EncounterCount => dungeonData != null
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
    public DungeonGrade HighestUnlockedGrade => highestUnlockedGrade;
    public string EventTitle { get; private set; } = string.Empty;
    public string EventDescription { get; private set; } = string.Empty;
    public string FirstOptionLabel { get; private set; } = string.Empty;
    public string SecondOptionLabel { get; private set; } = string.Empty;
    public string ThirdOptionLabel { get; private set; } = string.Empty;

    public event Action<string> DungeonMessage;
    public event Action DungeonStateChanged;
    public event Action<bool> DungeonCompleted;

    private void OnEnable()
    {
        LoadDungeonProgress();
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

    public int GetClearedFloors(DungeonDataSO data)
    {
        if (data == null ||
            !clearedFloorsByDungeon.TryGetValue(data.name, out int clearedFloors))
        {
            return 0;
        }

        return Mathf.Clamp(clearedFloors, 0, Mathf.Max(1, data.totalFloors));
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
        List<SavedDungeonFloorProgress> result =
            new List<SavedDungeonFloorProgress>();
        foreach (KeyValuePair<string, int> pair in clearedFloorsByDungeon)
        {
            result.Add(new SavedDungeonFloorProgress
            {
                dungeonAssetName = pair.Key,
                clearedFloors = pair.Value
            });
        }

        return result;
    }

    public void SetUnlockedTownIndices(IReadOnlyList<int> townIndices)
    {
        unlockedTownIndices.Clear();
        unlockedTownIndices.Add(2);

        if (townIndices != null)
        {
            foreach (int townIndex in townIndices)
            {
                if (townIndex >= 0)
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
        IReadOnlyList<SavedDungeonFloorProgress> savedFloorProgress = null)
    {
        highestUnlockedGrade = (DungeonGrade)Mathf.Clamp(
            (int)restoredHighestGrade,
            (int)DungeonGrade.Low,
            (int)DungeonGrade.Highest);
        SaveDungeonProgress();
        PopulateDungeonDataIfNeeded();
        clearedFloorsByDungeon.Clear();
        if (savedFloorProgress != null)
        {
            foreach (SavedDungeonFloorProgress progress in savedFloorProgress)
            {
                if (progress == null ||
                    string.IsNullOrWhiteSpace(progress.dungeonAssetName))
                {
                    continue;
                }

                clearedFloorsByDungeon[progress.dungeonAssetName] =
                    Mathf.Max(0, progress.clearedFloors);
            }
        }

        DungeonDataSO restoredSelection = null;
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (data != null &&
                data.name == selectedDungeonAssetName &&
                IsDungeonUnlocked(data))
            {
                restoredSelection = data;
                break;
            }
        }

        dungeonData = restoredSelection ?? FindFirstUnlockedDungeon();
        DungeonStateChanged?.Invoke();
    }

    [ContextMenu("ダンジョン開放状態を初期化")]
    public void ResetDungeonProgress()
    {
        highestUnlockedGrade = DungeonGrade.Low;
        clearedFloorsByDungeon.Clear();
        PlayerPrefs.DeleteKey(UnlockedGradeSaveKey);
        PlayerPrefs.Save();

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

        DungeonEventType eventType = currentEventType;
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
        List<EnemyDataSO> enemies = CreateDungeonEncounter(enemyCount);

        SendDungeonMessage(
            $"遭遇 {CurrentEncounter}/{EncounterCount}: " +
            $"敵が{enemyCount}体出現しました。");
        DungeonStateChanged?.Invoke();

        bool started = battleManager.StartBattle(partyManager.Members, enemies);
        if (!started)
        {
            CompleteRun(false, "戦闘を開始できないため探索を終了しました。");
        }

        return started;
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

        for (int i = 0; i < normalEnemyCount; i++)
        {
            EnemyDataSO enemy = GetRandomNormalEnemy();
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }

        if (isBossEncounter && dungeonData.bossEnemy != null)
        {
            enemies.Add(dungeonData.bossEnemy);
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
        int previousClearedFloors = GetClearedFloors(dungeonData);
        clearedFloorsByDungeon[dungeonData.name] =
            Mathf.Max(previousClearedFloors, completedFloor);

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

    private DungeonEventType currentEventType = DungeonEventType.None;

    private void PresentRandomEvent()
    {
        currentEventType = (DungeonEventType)UnityEngine.Random.Range(
            (int)DungeonEventType.AbandonedCamp,
            (int)DungeonEventType.CollapsedPassage + 1);
        IsAwaitingEventChoice = true;

        switch (currentEventType)
        {
            case DungeonEventType.AbandonedCamp:
                EventTitle = "放棄された野営地";
                EventDescription =
                    "静かな野営地を発見しました。休息は時間を使いますが、今回のフロア探索は合計1日として処理されます。";
                FirstOptionLabel = $"休息 HP+{restHealAmount}（時間消費）";
                SecondOptionLabel = $"物資探索 +{treasureGoldReward / 2} G";
                break;
            case DungeonEventType.TreasureCache:
                EventTitle = "隠された宝箱";
                EventDescription =
                    "商人が隠したと思われる施錠された箱を発見しました。回収してもフロア探索日数は1日のままです。";
                FirstOptionLabel = $"回収 +{treasureGoldReward} G";
                SecondOptionLabel = $"休息 HP+{Mathf.Max(1, restHealAmount / 2)}（短時間）";
                break;
            case DungeonEventType.CollapsedPassage:
                EventTitle = "崩れた通路";
                EventDescription =
                    "近道は崩れかけています。強行突破は時間を使いますが、今回のフロア探索は合計1日として処理されます。";
                FirstOptionLabel = $"強行突破 HP-{hazardDamage}（時間消費）";
                SecondOptionLabel = $"休息 HP+{Mathf.Max(1, restHealAmount / 2)}（短時間）";
                break;
        }

        ThirdOptionLabel = "撤退";
        SendDungeonMessage($"ダンジョンイベント: {EventTitle}");
        DungeonStateChanged?.Invoke();
    }

    private void ResolveEventChoice(DungeonEventType eventType, int optionIndex)
    {
        switch (eventType)
        {
            case DungeonEventType.AbandonedCamp:
                if (optionIndex == 0)
                {
                    HealParty(restHealAmount);
                    progressionManager?.AddExplorationDelay(1);
                }
                else
                {
                    GrantGold(treasureGoldReward / 2);
                    TryGrantLimitedEquipment(
                        dungeonData != null
                            ? dungeonData.eventLimitedDropChance
                            : 0f,
                        "探索イベント限定ドロップ");
                }
                break;
            case DungeonEventType.TreasureCache:
                if (optionIndex == 0)
                {
                    GrantGold(treasureGoldReward);
                    TryGrantLimitedEquipment(
                        dungeonData != null
                            ? dungeonData.eventLimitedDropChance
                            : 0f,
                        "宝箱限定ドロップ");
                }
                else
                {
                    HealParty(Mathf.Max(1, restHealAmount / 2));
                }
                break;
            case DungeonEventType.CollapsedPassage:
                if (optionIndex == 0)
                {
                    DamageParty(hazardDamage);
                    progressionManager?.AddExplorationDelay(1);
                }
                else
                {
                    HealParty(Mathf.Max(1, restHealAmount / 2));
                }
                break;
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
        merchantData?.AddGold(amount);
        SendDungeonMessage($"{amount} Gを獲得しました。");
    }

    private void GrantClearRewards()
    {
        ResolveReferences();

        if (dungeonData == null)
        {
            return;
        }

        int goldReward = Mathf.Max(0, dungeonData.clearGoldReward);
        if (goldReward > 0)
        {
            merchantData?.AddGold(goldReward);
            SendDungeonMessage($"踏破報酬: {goldReward} G");
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
            SendDungeonMessage(
                $"踏破報酬: {JapaneseDisplayText.GetItemName(reward.item)} x{reward.amount}");
        }
    }

    private void TryGrantLimitedEquipment(float chance, string sourceLabel)
    {
        ResolveReferences();
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
        SendDungeonMessage(
            $"{sourceLabel}: [{JapaneseDisplayText.GetEquipmentQuality(equipment.Quality)}] " +
            $"{JapaneseDisplayText.GetItemName(drop)}");
    }

    private void PopulateDungeonDataIfNeeded()
    {
        RemoveMissingDungeons();

        foreach (DungeonDataSO data in Resources.LoadAll<DungeonDataSO>(string.Empty))
        {
            if (data.worldMapIndex == currentWorldMapIndex)
            {
                AddDungeon(data);
            }
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:DungeonDataSO",
            new[] { "Assets/Proiject/ScriptableObjects/Dungeons" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DungeonDataSO data =
                AssetDatabase.LoadAssetAtPath<DungeonDataSO>(path);
            if (data != null && data.worldMapIndex == currentWorldMapIndex)
            {
                AddDungeon(data);
            }
        }
#endif

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
            if (availableDungeons[i] == null ||
                availableDungeons[i].worldMapIndex != currentWorldMapIndex)
            {
                availableDungeons.RemoveAt(i);
            }
        }
    }

    private void LoadDungeonProgress()
    {
        int savedGrade = PlayerPrefs.GetInt(
            UnlockedGradeSaveKey,
            (int)DungeonGrade.Low);
        highestUnlockedGrade = (DungeonGrade)Mathf.Clamp(
            savedGrade,
            (int)DungeonGrade.Low,
            (int)DungeonGrade.Highest);
    }

    private void SaveDungeonProgress()
    {
        PlayerPrefs.SetInt(UnlockedGradeSaveKey, (int)highestUnlockedGrade);
        PlayerPrefs.Save();
    }

    private DungeonDataSO FindFirstUnlockedDungeon()
    {
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (IsDungeonUnlocked(data))
            {
                return data;
            }
        }

        return null;
    }

    private void ClearEvent()
    {
        currentEventType = DungeonEventType.None;
        EventTitle = string.Empty;
        EventDescription = string.Empty;
        FirstOptionLabel = string.Empty;
        SecondOptionLabel = string.Empty;
        ThirdOptionLabel = string.Empty;
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
