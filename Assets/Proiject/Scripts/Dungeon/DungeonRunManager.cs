using System;
using System.Collections.Generic;
using UnityEngine;

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
    public DungeonGrade HighestUnlockedGrade => highestUnlockedGrade;
    public int CurrentWorldMapIndex => currentWorldMapIndex;
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
        if (data == null ||
            !clearedFloorsByDungeon.TryGetValue(
                GetDungeonProgressKey(data),
                out int clearedFloors))
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
            DungeonDataSO dungeon = FindDungeonByProgressKey(pair.Key);
            result.Add(new SavedDungeonFloorProgress
            {
                dungeonPersistentId =
                    dungeon != null ? dungeon.PersistentId : pair.Key,
                dungeonAssetName =
                    dungeon != null ? dungeon.name : pair.Key,
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
                    (string.IsNullOrWhiteSpace(progress.dungeonPersistentId) &&
                     string.IsNullOrWhiteSpace(progress.dungeonAssetName)))
                {
                    continue;
                }

                DungeonDataSO restoredDungeon =
                    FindDungeon(
                        progress.dungeonPersistentId,
                        progress.dungeonAssetName);
                string progressKey = restoredDungeon != null
                    ? GetDungeonProgressKey(restoredDungeon)
                    : !string.IsNullOrWhiteSpace(progress.dungeonPersistentId)
                        ? progress.dungeonPersistentId
                        : progress.dungeonAssetName;
                clearedFloorsByDungeon[progressKey] =
                    Mathf.Max(0, progress.clearedFloors);
            }
        }

        DungeonDataSO restoredSelection = FindDungeon(
            selectedDungeonPersistentId,
            selectedDungeonAssetName);
        if (restoredSelection != null &&
            !IsDungeonUnlocked(restoredSelection))
        {
            restoredSelection = null;
        }

        dungeonData = restoredSelection ?? FindFirstUnlockedDungeon();
        DungeonStateChanged?.Invoke();
    }

    private static string GetDungeonProgressKey(DungeonDataSO data)
    {
        return data != null ? data.PersistentId : string.Empty;
    }

    private DungeonDataSO FindDungeon(
        string persistentId,
        string legacyAssetName)
    {
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (data == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(persistentId) &&
                data.PersistentId == persistentId)
            {
                return data;
            }

            if (string.IsNullOrWhiteSpace(persistentId) &&
                data.name == legacyAssetName)
            {
                return data;
            }
        }

        return GameAssetRepository.FindByPersistentId<DungeonDataSO>(
            persistentId,
            legacyAssetName);
    }

    private DungeonDataSO FindDungeonByProgressKey(string progressKey)
    {
        foreach (DungeonDataSO data in availableDungeons)
        {
            if (data != null &&
                (data.PersistentId == progressKey || data.name == progressKey))
            {
                return data;
            }
        }

        return GameAssetRepository.FindByPersistentId<DungeonDataSO>(
            progressKey,
            progressKey);
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
                    enemy = CreateSpecialVariant(enemy, false);
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
                boss = CreateSpecialVariant(boss, true);
            }
            enemies.Add(boss);
        }

        return enemies.Count > 0
            ? enemies
            : battleManager.CreateDefaultEnemyEncounter(enemyCount);
    }

    private EnemyDataSO CreateSpecialVariant(
        EnemyDataSO source,
        bool isBossVariant)
    {
        if (source == null ||
            dungeonData?.specialVariantSkillPool == null ||
            dungeonData.specialVariantSkillPool.Length == 0)
        {
            return source;
        }

        List<EnemySkillType> skills = new List<EnemySkillType>();
        foreach (EnemySkillType skill in dungeonData.specialVariantSkillPool)
        {
            if (skill != EnemySkillType.None)
            {
                skills.Add(skill);
            }
        }
        if (skills.Count == 0)
        {
            return source;
        }

        EnemyDataSO variant = Instantiate(source);
        variant.name = $"{source.name} Special Variant";
        variant.hideFlags = HideFlags.DontSave;
        variant.isSpecialVariant = true;
        variant.enemySkill =
            skills[UnityEngine.Random.Range(0, skills.Count)];
        variant.specialVariantTitle =
            GetSpecialVariantTitle(variant.enemySkill, isBossVariant);
        variant.maxHP = Mathf.RoundToInt(
            source.maxHP * (isBossVariant ? 1.4f : 1.18f));
        variant.attack = Mathf.RoundToInt(
            source.attack * (isBossVariant ? 1.25f : 1.12f));
        variant.defense = Mathf.RoundToInt(
            source.defense * (isBossVariant ? 1.2f : 1.1f));
        variant.goldReward = Mathf.RoundToInt(
            source.goldReward * (isBossVariant ? 3f : 1.75f));
        variant.experienceMultiplier =
            Mathf.Max(1f, source.experienceMultiplier) *
            (isBossVariant ? 2.5f : 2f);

        ApplySpecialSkillStatBonus(variant);
        AddSpecialVariantMaterialDrop(variant, isBossVariant);
        if (isBossVariant)
        {
            AddSpecialJobCertificateDrop(variant);
        }
        return variant;
    }

    private static void AddSpecialVariantMaterialDrop(
        EnemyDataSO variant,
        bool isBossVariant)
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            "Items/Special/MutantCore");
        if (variant == null || material == null)
        {
            return;
        }

        List<ItemDropEntry> drops = new List<ItemDropEntry>();
        if (variant.itemDrops != null)
        {
            drops.AddRange(variant.itemDrops);
        }
        drops.Add(new ItemDropEntry
        {
            item = material,
            amount = isBossVariant ? 2 : 1,
            dropChance = isBossVariant ? 1f : 0.5f
        });
        variant.itemDrops = drops.ToArray();
    }

    private void AddSpecialJobCertificateDrop(EnemyDataSO variant)
    {
        ItemDataSO certificate = Resources.Load<ItemDataSO>(
            "Items/JobChange/SecretJobCertificate");
        if (certificate == null)
        {
            return;
        }

        List<ItemDropEntry> drops = new List<ItemDropEntry>();
        if (variant.itemDrops != null)
        {
            drops.AddRange(variant.itemDrops);
        }
        float chance = Mathf.Clamp01(
            0.5f + ((int)dungeonData.grade * 0.125f));
        drops.Add(new ItemDropEntry
        {
            item = certificate,
            amount = 1,
            dropChance = chance
        });
        variant.itemDrops = drops.ToArray();
    }

    private static string GetSpecialVariantTitle(
        EnemySkillType skill,
        bool isBossVariant)
    {
        if (isBossVariant)
        {
            return "異形の";
        }

        switch (skill)
        {
            case EnemySkillType.PowerStrike: return "凶暴な";
            case EnemySkillType.VenomStrike: return "猛毒の";
            case EnemySkillType.ParalyzingRoar: return "震声の";
            case EnemySkillType.CriticalFocus: return "鋭眼の";
            case EnemySkillType.DoubleStrike: return "迅撃の";
            case EnemySkillType.LifeDrain: return "吸命の";
            case EnemySkillType.ArmorPierce: return "破甲の";
            case EnemySkillType.FlameBreath: return "炎息の";
            case EnemySkillType.FrostBite: return "氷牙の";
            case EnemySkillType.TripleStrike: return "裂爪の";
            case EnemySkillType.BattleHeal: return "再生する";
            case EnemySkillType.SacrificialStrike: return "狂戦の";
            case EnemySkillType.Execute: return "処刑者の";
            case EnemySkillType.ToxicCloud: return "瘴気の";
            default: return "変異した";
        }
    }

    private static void ApplySpecialSkillStatBonus(EnemyDataSO variant)
    {
        switch (variant.enemySkill)
        {
            case EnemySkillType.PowerStrike:
                variant.attack = Mathf.RoundToInt(variant.attack * 1.15f);
                break;
            case EnemySkillType.VenomStrike:
                variant.attackSpeed *= 1.1f;
                break;
            case EnemySkillType.ParalyzingRoar:
                variant.defense = Mathf.RoundToInt(variant.defense * 1.15f);
                break;
            case EnemySkillType.CriticalFocus:
                variant.criticalRate += 0.2f;
                break;
            case EnemySkillType.DoubleStrike:
                variant.attackSpeed *= 1.2f;
                break;
            case EnemySkillType.LifeDrain:
                variant.maxHP = Mathf.RoundToInt(variant.maxHP * 1.2f);
                break;
            case EnemySkillType.ArmorPierce:
                variant.attack = Mathf.RoundToInt(variant.attack * 1.1f);
                break;
            case EnemySkillType.FlameBreath:
                variant.maxMagicPower += 25;
                break;
            case EnemySkillType.FrostBite:
                variant.defense = Mathf.RoundToInt(variant.defense * 1.12f);
                break;
            case EnemySkillType.TripleStrike:
                variant.attackSpeed *= 1.15f;
                break;
            case EnemySkillType.BattleHeal:
                variant.maxHP = Mathf.RoundToInt(variant.maxHP * 1.25f);
                break;
            case EnemySkillType.SacrificialStrike:
                variant.attack = Mathf.RoundToInt(variant.attack * 1.18f);
                break;
            case EnemySkillType.Execute:
                variant.criticalRate += 0.12f;
                break;
            case EnemySkillType.ToxicCloud:
                variant.maxMagicPower += 20;
                variant.attackSpeed *= 1.08f;
                break;
        }
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
        clearedFloorsByDungeon[GetDungeonProgressKey(dungeonData)] =
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
