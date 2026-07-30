using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "game-save.json";
    private static string automatedTestSavePath;

    private MerchantData merchantData;
    private DayManager dayManager;
    private MerchantInventory merchantInventory;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private HealingManager healingManager;
    private TrainingGroundManager trainingGroundManager;
    private BattleManager battleManager;
    private DungeonRunManager dungeonRunManager;
    private DungeonExpeditionManager dungeonExpeditionManager;
    private ProgressionManager progressionManager;
    private DebtManager debtManager;
    private TownProgressState townProgressState;
    private StoryProgressManager storyProgressManager;
    private RoadCargoSession roadCargoSession;
    private RemoteSaleManager remoteSaleManager;
    private MonsterCodexManager monsterCodexManager;
    private OnboardingGuideController onboardingGuideController;
    private bool initialized;
    private bool isLoading;
    private bool suppressAutoSaveAfterDelete;
    // 既存セーブのロードに失敗した(バージョン超過/破損)場合に立てる。
    // 失敗したセーブを初期状態や部分ロード状態で上書きしないよう、以降の
    // 保存を全面的に抑止する。
    private bool saveBlockedByLoadFailure;
    private string savePathOverride = string.Empty;
    private Coroutine pendingAutoSaveCoroutine;

    public static string DefaultSavePath => IsAutomatedTestRun()
        ? GetAutomatedTestSavePath()
        : Path.Combine(Application.persistentDataPath, SaveFileName);
    public static bool SaveFileExists() => File.Exists(DefaultSavePath);

    public static void DeleteSaveFileAndProgress()
    {
        if (File.Exists(DefaultSavePath))
        {
            File.Delete(DefaultSavePath);
        }

        PlayerPrefs.DeleteKey(DungeonProgressStore.UnlockedGradeSaveKey);
        PlayerPrefs.Save();
    }

    public string SavePath => !string.IsNullOrEmpty(savePathOverride)
        ? savePathOverride
        : DefaultSavePath;
    public bool HasSaveData => File.Exists(SavePath);
    public bool HasExistingSaveAtInitialization { get; private set; }

    public void InitializeAndLoad()
    {
        if (initialized)
        {
            return;
        }

        ResolveReferences();
        HasExistingSaveAtInitialization = File.Exists(SavePath);
        bool loadedExistingSave = false;
        if (HasExistingSaveAtInitialization)
        {
            loadedExistingSave = LoadGame();
        }

        // 既存セーブが無い場合のみ新規状態を適用する。既存セーブのロードに
        // 失敗した場合は、新規状態で上書きしないよう ApplySaveData を呼ばず、
        // 保存も saveBlockedByLoadFailure により抑止される。
        if (!HasExistingSaveAtInitialization)
        {
            ApplySaveData(new GameSaveData
            {
                onboardingEnabled = true,
                onboardingStep = OnboardingGuideStep.Opening
            });
        }
        Subscribe();
        if (loadedExistingSave)
        {
            onboardingGuideController?.CompleteOpeningAfterExistingSaveLoad();
        }
        if (!HasExistingSaveAtInitialization)
        {
            storyProgressManager?.BeginNewGame();
        }
        initialized = true;
    }

    [ContextMenu("ゲームを保存")]
    public void SaveGame()
    {
        CancelPendingAutoSave();
        if (isLoading || IsAutomatedTestRun())
        {
            return;
        }

        if (saveBlockedByLoadFailure)
        {
            Debug.LogWarning(
                "既存セーブのロードに失敗しているため、保存を行いません。");
            return;
        }

        ResolveReferences();
        GameSaveData data = CreateSaveData();
        WriteSaveFileAtomically(JsonUtility.ToJson(data, true));
        Debug.Log($"ゲームを保存しました: {SavePath}");
    }

    // 一時ファイルへ書き込んでから本番ファイルへ置換する。書き込み途中に
    // 中断・失敗しても、既存の完全なセーブが破損しないようにする。
    private void WriteSaveFileAtomically(string json)
    {
        string directory = Path.GetDirectoryName(SavePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempPath = SavePath + ".tmp";
        File.WriteAllText(tempPath, json);
        if (File.Exists(SavePath))
        {
            // File.Replace は元ファイルを一時退避しつつ原子的に置換する。
            File.Replace(tempPath, SavePath, SavePath + ".bak");
        }
        else
        {
            File.Move(tempPath, SavePath);
        }
    }

    [ContextMenu("ゲームを読込")]
    public bool LoadGame()
    {
        ResolveReferences();
        if (!File.Exists(SavePath))
        {
            return false;
        }

        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(
                File.ReadAllText(SavePath));
            if (data == null)
            {
                BlockSaveAfterLoadFailure(
                    "セーブデータを解釈できませんでした。");
                return false;
            }

            if (data.version > GameSaveData.CurrentVersion)
            {
                Debug.LogError(
                    $"Save data version {data.version} is newer than the " +
                    $"supported version {GameSaveData.CurrentVersion}. " +
                    "The save was not loaded or modified.");
                BlockSaveAfterLoadFailure(
                    "対応していない新しいバージョンのセーブデータです。");
                return false;
            }

            bool requiresMigration =
                data.version != GameSaveData.CurrentVersion;
            data = SaveDataMigrator.Migrate(data);
            isLoading = true;
            storyProgressManager?.BeginRestore();
            ApplySaveData(data);
            if (requiresMigration)
            {
                WriteSaveFileAtomically(JsonUtility.ToJson(data, true));
                Debug.Log(
                    $"Save data migrated to version " +
                    $"{GameSaveData.CurrentVersion}.");
            }
            Debug.Log($"ゲームを読み込みました: {SavePath}");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"セーブデータの読込に失敗しました: {exception.Message}");
            BlockSaveAfterLoadFailure(
                "セーブデータの読込中にエラーが発生しました。");
            return false;
        }
        finally
        {
            isLoading = false;
            storyProgressManager?.EndRestore();
        }
    }

    // ロード失敗時に呼ぶ。以降の保存を抑止し、失敗したセーブファイルを
    // 上書きしないようにする。
    private void BlockSaveAfterLoadFailure(string reason)
    {
        saveBlockedByLoadFailure = true;
        Debug.LogWarning(
            $"既存セーブの保護のため以降の自動保存を停止します: {reason}");
    }

    [ContextMenu("セーブデータを削除")]
    public void DeleteSaveData()
    {
        CancelPendingAutoSave();
        suppressAutoSaveAfterDelete = true;
        if (string.IsNullOrEmpty(savePathOverride))
        {
            DeleteSaveFileAndProgress();
        }
        else if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        dungeonRunManager?.ResetDungeonProgress();
        Debug.Log("セーブデータを削除しました。");
    }

    private void OnApplicationQuit()
    {
        if (initialized && !suppressAutoSaveAfterDelete)
        {
            SaveGame();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized && !suppressAutoSaveAfterDelete)
        {
            SaveGame();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private static bool IsAutomatedTestRun()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length; i++)
        {
            if (string.Equals(
                    arguments[i],
                    "-runTests",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetAutomatedTestSavePath()
    {
        if (string.IsNullOrEmpty(automatedTestSavePath))
        {
            automatedTestSavePath = Path.Combine(
                Application.temporaryCachePath,
                $"dungeon-merchant-test-{Guid.NewGuid():N}.json");
        }

        return automatedTestSavePath;
    }

    private GameSaveData CreateSaveData()
    {
        GameSaveData data = new GameSaveData
        {
            gold = merchantData != null ? merchantData.Gold : 500,
            merchantLevel = merchantData != null
                ? merchantData.MerchantLevel
                : 1,
            merchantExperience = merchantData != null
                ? merchantData.MerchantExperience
                : 0,
            lifetimeGoldEarned = merchantData != null
                ? merchantData.LifetimeGoldEarned
                : 0,
            merchantSkillPoints = merchantData != null
                ? merchantData.MerchantSkillPoints
                : 2,
            merchantNegotiation = merchantData != null
                ? merchantData.Negotiation
                : 0,
            merchantLeadership = merchantData != null
                ? merchantData.Leadership
                : 0,
            merchantAppraisal = merchantData != null
                ? merchantData.Appraisal
                : 0,
            merchantLogistics = merchantData != null
                ? merchantData.Logistics
                : 0,
            currentDay = dayManager != null ? dayManager.CurrentDay : 1,
            remainingDebt = debtManager != null
                ? debtManager.RemainingDebt
                : DebtManager.InitialDebt,
            debtPaymentArrears = debtManager != null
                ? debtManager.PaymentArrears
                : 0,
            processedDebtMonths = debtManager != null
                ? Mathf.Max(0, debtManager.CurrentMonth - 1)
                : 0,
            currentTownIndex = townProgressState != null
                ? townProgressState.CurrentTownIndex
                : 2,
            highestUnlockedDungeonGrade = dungeonRunManager != null
                ? (int)dungeonRunManager.HighestUnlockedGrade
                : 0,
            selectedDungeonAssetName =
                dungeonRunManager != null && dungeonRunManager.SelectedDungeon != null
                    ? dungeonRunManager.SelectedDungeon.name
                    : string.Empty,
            selectedDungeonPersistentId =
                dungeonRunManager != null &&
                dungeonRunManager.SelectedDungeon != null
                    ? dungeonRunManager.SelectedDungeon.PersistentId
                    : string.Empty
        };

        if (storyProgressManager != null)
        {
            data.completedStoryMilestones.AddRange(
                storyProgressManager.CompletedMilestones);
        }

        if (townProgressState != null)
        {
            data.unlockedTownIndices.Clear();
            data.unlockedTownIndices.AddRange(townProgressState.GetUnlockedTownIndices());
        }

        if (dungeonRunManager != null)
        {
            data.dungeonFloorProgress.AddRange(
                dungeonRunManager.CreateFloorProgressSaveData());
        }

        if (merchantInventory != null)
        {
            data.discoveredEquipmentPersistentIds.AddRange(
                merchantInventory.DiscoveredEquipmentPersistentIds);
            for (int townIndex = 0;
                 townIndex < WorldMapService.TownNames.Length;
                 townIndex++)
            {
                foreach (InventoryItemStack stack in
                         merchantInventory.GetItemsIn(townIndex))
                {
                    if (stack?.Item == null || stack.Amount <= 0)
                    {
                        continue;
                    }

                    data.inventory.Add(new SavedInventoryItem
                    {
                        townIndex = townIndex,
                        itemPersistentId = stack.Item.PersistentId,
                        itemAssetName = stack.Item.name,
                        itemName = stack.Item.itemName,
                        amount = stack.Amount
                    });
                }

                foreach (EquipmentInstance equipment in
                         merchantInventory.GetEquipmentInstancesIn(townIndex))
                {
                    if (equipment?.BaseItem != null)
                    {
                        SavedEquipmentInstance savedEquipment =
                            CreateSavedEquipment(equipment);
                        if (savedEquipment != null)
                        {
                            savedEquipment.townIndex = townIndex;
                            data.equipmentInventory.Add(savedEquipment);
                        }
                    }
                }
            }
        }

        if (hireManager != null)
        {
            foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
            {
                if (mercenary == null)
                {
                    continue;
                }

                SavedMercenary savedMercenary = new SavedMercenary
                {
                    instanceId = mercenary.InstanceId,
                    townIndex = mercenary.CurrentTownIndex,
                    baseDataAssetName = mercenary.BaseData != null
                        ? mercenary.BaseData.name
                        : string.Empty,
                    baseDataPersistentId = mercenary.BaseData != null
                        ? mercenary.BaseData.PersistentId
                        : string.Empty,
                    archetypeAssetName = mercenary.Archetype != null
                        ? mercenary.Archetype.name
                        : string.Empty,
                    archetypePersistentId = mercenary.Archetype != null
                        ? mercenary.Archetype.PersistentId
                        : string.Empty,
                    mercenaryName = mercenary.MercenaryName,
                    mercenaryClass = mercenary.MercenaryClass,
                    contractType = mercenary.ContractType,
                    level = mercenary.Level,
                    currentExperience = mercenary.CurrentExperience,
                    maxHP = mercenary.BaseMaxHP,
                    currentHP = mercenary.CurrentHP,
                    attack = mercenary.BaseAttack,
                    defense = mercenary.BaseDefense,
                    maxMagicPower = mercenary.BaseMaxMagicPower,
                    attackSpeed = mercenary.BaseAttackSpeed,
                    statusEffect = mercenary.StatusEffect,
                    hireCost = mercenary.HireCost,
                    contractEndDay = mercenary.ContractEndDay,
                    contractNeedsRenewal = mercenary.ContractNeedsRenewal,
                    equippedWeaponAssetName = mercenary.EquippedWeapon != null
                        ? mercenary.EquippedWeapon.name
                        : string.Empty,
                    equippedWeaponPersistentId =
                        mercenary.EquippedWeapon != null
                            ? mercenary.EquippedWeapon.PersistentId
                            : string.Empty,
                    equippedWeaponInstance = mercenary.EquippedWeaponInstance != null
                        ? CreateSavedEquipment(mercenary.EquippedWeaponInstance)
                        : null,
                    equippedArmorAssetName = mercenary.EquippedArmor != null
                        ? mercenary.EquippedArmor.name
                        : string.Empty,
                    equippedArmorPersistentId =
                        mercenary.EquippedArmor != null
                            ? mercenary.EquippedArmor.PersistentId
                            : string.Empty,
                    equippedArmorInstance = mercenary.EquippedArmorInstance != null
                        ? CreateSavedEquipment(mercenary.EquippedArmorInstance)
                        : null,
                    equippedAccessoryAssetName = mercenary.EquippedAccessory != null
                        ? mercenary.EquippedAccessory.name
                        : string.Empty,
                    equippedAccessoryPersistentId =
                        mercenary.EquippedAccessory != null
                            ? mercenary.EquippedAccessory.PersistentId
                            : string.Empty,
                    equippedAccessoryInstance =
                        mercenary.EquippedAccessoryInstance != null
                            ? CreateSavedEquipment(
                                mercenary.EquippedAccessoryInstance)
                            : null,
                    consumableSlots = CreateSavedConsumableSlots(mercenary)
                };
                NormalizeSavedMercenaryEquipment(
                    savedMercenary,
                    EquipmentSlot.Weapon,
                    mercenary.EquippedWeapon,
                    mercenary.EquippedWeaponInstance);
                NormalizeSavedMercenaryEquipment(
                    savedMercenary,
                    EquipmentSlot.Armor,
                    mercenary.EquippedArmor,
                    mercenary.EquippedArmorInstance);
                NormalizeSavedMercenaryEquipment(
                    savedMercenary,
                    EquipmentSlot.Accessory,
                    mercenary.EquippedAccessory,
                    mercenary.EquippedAccessoryInstance);
                data.hiredMercenaries.Add(savedMercenary);
            }
        }

        if (partyManager != null)
        {
            foreach (MercenaryInstance mercenary in partyManager.Members)
            {
                if (mercenary != null)
                {
                    data.partyMemberIds.Add(mercenary.InstanceId);
                }
            }
        }

        if (progressionManager != null)
        {
            data.progression = progressionManager.CreateSaveData();
        }

        if (roadCargoSession != null)
        {
            data.roadCargoSession = roadCargoSession.CreateSaveData();
        }
        if (trainingGroundManager != null)
        {
            data.trainingAssignments = trainingGroundManager.CreateSaveData();
        }
        if (remoteSaleManager != null)
        {
            data.remoteSaleOrders = remoteSaleManager.CreateSaveData();
        }
        if (dungeonExpeditionManager != null)
        {
            data.dungeonExpeditions = dungeonExpeditionManager.CreateSaveData();
        }
        if (monsterCodexManager != null)
        {
            data.encounteredEnemyIds.AddRange(
                monsterCodexManager.EncounteredEnemyIds);
        }

        if (onboardingGuideController != null)
        {
            data.onboardingEnabled = onboardingGuideController.IsEnabled;
            data.onboardingStep = onboardingGuideController.CurrentStep;
            data.onboardingShownCards.Clear();
            data.onboardingShownCards.AddRange(
                onboardingGuideController.ShownCards);
        }

        return data;
    }

    private void ApplySaveData(GameSaveData data)
    {
        merchantData?.SetGold(data.gold);
        merchantData?.RestoreProgression(
            Mathf.Max(1, data.merchantLevel),
            data.merchantExperience,
            data.lifetimeGoldEarned);
        merchantData?.RestoreSkills(
            data.merchantSkillPoints,
            data.merchantNegotiation,
            data.merchantLeadership,
            data.merchantAppraisal,
            data.merchantLogistics);
        debtManager?.Restore(
            data.remainingDebt,
            data.debtPaymentArrears,
            data.processedDebtMonths);
        dayManager?.SetCurrentDay(data.currentDay);
        townProgressState?.RestoreTownProgress(
            data.currentTownIndex,
            data.unlockedTownIndices);

        Dictionary<int, List<InventoryItemStack>> restoredItems =
            new Dictionary<int, List<InventoryItemStack>>();
        if (data.inventory != null)
        {
            foreach (SavedInventoryItem savedItem in data.inventory)
            {
                if (savedItem == null)
                {
                    continue;
                }

                ItemDataSO item = FindItem(
                    savedItem.itemPersistentId,
                    savedItem.itemAssetName,
                    savedItem.itemName);
                if (item != null && savedItem.amount > 0)
                {
                    if (!restoredItems.TryGetValue(
                            savedItem.townIndex,
                            out List<InventoryItemStack> townItems))
                    {
                        townItems = new List<InventoryItemStack>();
                        restoredItems[savedItem.townIndex] = townItems;
                    }
                    townItems.Add(new InventoryItemStack(item, savedItem.amount));
                }
            }
        }
        if (merchantInventory != null)
        {
            for (int townIndex = 0;
                 townIndex < WorldMapService.TownNames.Length;
                 townIndex++)
            {
                restoredItems.TryGetValue(
                    townIndex,
                    out List<InventoryItemStack> townItems);
                merchantInventory.RestoreItemsIn(townIndex, townItems);
            }
        }

        Dictionary<int, List<EquipmentInstance>> restoredEquipment =
            new Dictionary<int, List<EquipmentInstance>>();
        if (data.equipmentInventory != null)
        {
            foreach (SavedEquipmentInstance savedEquipment in data.equipmentInventory)
            {
                EquipmentInstance equipment = RestoreEquipment(savedEquipment);
                if (equipment != null)
                {
                    if (!restoredEquipment.TryGetValue(
                            savedEquipment.townIndex,
                            out List<EquipmentInstance> townEquipment))
                    {
                        townEquipment = new List<EquipmentInstance>();
                        restoredEquipment[savedEquipment.townIndex] =
                            townEquipment;
                    }
                    townEquipment.Add(equipment);
                }
            }
        }
        if (merchantInventory != null)
        {
            for (int townIndex = 0;
                 townIndex < WorldMapService.TownNames.Length;
                 townIndex++)
            {
                restoredEquipment.TryGetValue(
                    townIndex,
                    out List<EquipmentInstance> townEquipment);
                merchantInventory.RestoreEquipmentInstancesIn(
                    townIndex,
                    townEquipment);
            }
        }
        merchantInventory?.RestoreDiscoveredEquipment(
            data.discoveredEquipmentPersistentIds,
            data.discoveredEquipmentAssetNames);
        monsterCodexManager?.RestoreEncounteredEnemies(data.encounteredEnemyIds);

        List<MercenaryInstance> restoredMercenaries = new List<MercenaryInstance>();
        if (data.hiredMercenaries != null)
        {
            foreach (SavedMercenary savedMercenary in data.hiredMercenaries)
            {
                if (savedMercenary != null)
                {
                    restoredMercenaries.Add(RestoreMercenary(savedMercenary));
                }
            }
        }
        hireManager?.RestoreHiredMercenaries(restoredMercenaries);
        partyManager?.RestoreParty(null);
        trainingGroundManager?.Restore(null);
        roadCargoSession?.Restore(null);
        dungeonExpeditionManager?.ClearActiveExpeditions();
        trainingGroundManager?.Restore(data.trainingAssignments);
        if (merchantInventory != null)
        {
            foreach (MercenaryInstance mercenary in restoredMercenaries)
            {
                merchantInventory.RegisterEquipmentDiscovery(
                    mercenary.GetEquippedItem(EquipmentSlot.Weapon));
                merchantInventory.RegisterEquipmentDiscovery(
                    mercenary.GetEquippedItem(EquipmentSlot.Armor));
                merchantInventory.RegisterEquipmentDiscovery(
                    mercenary.GetEquippedItem(EquipmentSlot.Accessory));
            }
        }

        Dictionary<string, MercenaryInstance> mercenaryById =
            new Dictionary<string, MercenaryInstance>();
        foreach (MercenaryInstance mercenary in restoredMercenaries)
        {
            mercenaryById[mercenary.InstanceId] = mercenary;
        }

        List<MercenaryInstance> restoredParty = new List<MercenaryInstance>();
        if (data.partyMemberIds != null)
        {
            foreach (string instanceId in data.partyMemberIds)
            {
                if (!string.IsNullOrWhiteSpace(instanceId) &&
                    mercenaryById.TryGetValue(instanceId, out MercenaryInstance mercenary))
                {
                    restoredParty.Add(mercenary);
                }
            }
        }
        partyManager?.RestoreParty(restoredParty);
        roadCargoSession?.Restore(data.roadCargoSession);
        RollBackInterruptedRoadCargoSession();
        remoteSaleManager?.Restore(data.remoteSaleOrders);
        progressionManager?.Restore(data.progression);

        dungeonRunManager?.SetCurrentWorldMapIndex(townProgressState.CurrentWorldMapIndex);
        dungeonRunManager?.RestoreProgress(
            (DungeonGrade)Mathf.Clamp(
                data.highestUnlockedDungeonGrade,
                (int)DungeonGrade.Low,
                (int)DungeonGrade.Highest),
            data.selectedDungeonAssetName,
            data.selectedDungeonPersistentId,
            data.dungeonFloorProgress);
        dungeonExpeditionManager?.Restore(
            data.dungeonExpeditions,
            mercenaryById);
        storyProgressManager?.RestoreCompletedMilestones(
            data.completedStoryMilestones);
        onboardingGuideController?.Restore(
            data.onboardingEnabled,
            data.onboardingStep,
            data.onboardingShownCards);
    }

    private void RollBackInterruptedRoadCargoSession()
    {
        if (roadCargoSession == null || !roadCargoSession.IsActive ||
            townProgressState == null ||
            roadCargoSession.ActiveSession.originTownIndex !=
            townProgressState.CurrentTownIndex)
        {
            return;
        }

        RoadCargoResolutionResult result = roadCargoSession.Retreat();
        if (result == RoadCargoResolutionResult.StorageFull)
        {
            Debug.LogWarning(
                "Interrupted road cargo remains pending at the origin because " +
                "storage is full. Free storage and receive it before travelling.");
        }
    }

    private MercenaryInstance RestoreMercenary(SavedMercenary saved)
    {
        MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
            saved.instanceId,
            FindMercenaryData(
                saved.baseDataPersistentId,
                saved.baseDataAssetName),
            FindArchetype(
                saved.archetypePersistentId,
                saved.archetypeAssetName),
            saved.mercenaryName,
            saved.mercenaryClass,
            saved.contractType,
            saved.level,
            saved.currentExperience,
            saved.maxHP,
            saved.currentHP,
            saved.attack,
            saved.defense,
            saved.maxMagicPower,
            saved.attackSpeed,
            saved.hireCost);
        RestoreMercenaryEquipment(
            mercenary,
            EquipmentSlot.Weapon,
            saved.equippedWeaponPersistentId,
            saved.equippedWeaponAssetName,
            saved.equippedWeaponInstance);
        RestoreMercenaryEquipment(
            mercenary,
            EquipmentSlot.Armor,
            saved.equippedArmorPersistentId,
            saved.equippedArmorAssetName,
            saved.equippedArmorInstance);
        RestoreMercenaryEquipment(
            mercenary,
            EquipmentSlot.Accessory,
            saved.equippedAccessoryPersistentId,
            saved.equippedAccessoryAssetName,
            saved.equippedAccessoryInstance);
        mercenary.SetCurrentHP(saved.currentHP);
        if (saved.statusEffect != BattleStatusEffect.None)
        {
            mercenary.RestoreStatusEffect(saved.statusEffect);
        }
        mercenary.RestoreContractState(
            saved.contractEndDay,
            saved.contractNeedsRenewal);
        mercenary.SetCurrentTownIndex(saved.townIndex);
        RestoreMercenaryConsumableSlots(mercenary, saved.consumableSlots);
        return mercenary;
    }

    private static SavedMercenaryConsumableSlot[] CreateSavedConsumableSlots(
        MercenaryInstance mercenary)
    {
        SavedMercenaryConsumableSlot[] savedSlots =
            new SavedMercenaryConsumableSlot[2];
        for (int i = 0; i < savedSlots.Length; i++)
        {
            MercenaryConsumableSlot slot = mercenary.ConsumableSlots[i];
            savedSlots[i] = new SavedMercenaryConsumableSlot
            {
                itemPersistentId = slot.Item != null ? slot.Item.PersistentId : string.Empty,
                count = slot.Count
            };
        }

        return savedSlots;
    }

    private static void NormalizeSavedMercenaryEquipment(
        SavedMercenary saved,
        EquipmentSlot slot,
        ItemDataSO item,
        EquipmentInstance instance)
    {
        if (item == null || !item.IsEquipment || item.equipmentSlot != slot)
        {
            return;
        }

        SavedEquipmentInstance savedInstance = CreateSavedEquipment(
            instance ?? EquipmentInstance.CreateFixed(item));
        if (slot == EquipmentSlot.Weapon)
        {
            saved.equippedWeaponInstance = savedInstance;
        }
        else if (slot == EquipmentSlot.Armor)
        {
            saved.equippedArmorInstance = savedInstance;
        }
        else
        {
            saved.equippedAccessoryInstance = savedInstance;
        }
    }

    private static void RestoreMercenaryConsumableSlots(
        MercenaryInstance mercenary,
        SavedMercenaryConsumableSlot[] savedSlots)
    {
        if (savedSlots == null)
        {
            return;
        }

        for (int i = 0; i < Mathf.Min(2, savedSlots.Length); i++)
        {
            SavedMercenaryConsumableSlot savedSlot = savedSlots[i];
            ItemDataSO item = savedSlot == null
                ? null
                : GameAssetRepository.FindByPersistentId<ItemDataSO>(
                    savedSlot.itemPersistentId,
                    string.Empty);
            mercenary.RestoreConsumableSlot(
                i,
                item != null && item.itemType == ItemType.Consumable
                    ? item
                    : null,
                savedSlot != null ? savedSlot.count : 0);
        }
    }

    private void RestoreMercenaryEquipment(
        MercenaryInstance mercenary,
        EquipmentSlot slot,
        string itemPersistentId,
        string itemAssetName,
        SavedEquipmentInstance savedInstance)
    {
        EquipmentInstance equipment = RestoreEquipment(savedInstance);
        if (equipment != null)
        {
            LogMismatchedEquipmentReference(
                slot,
                itemPersistentId,
                itemAssetName,
                equipment.BaseItem);
            mercenary.RestoreEquippedEquipment(slot, equipment);
            return;
        }

        ItemDataSO legacyItem = FindItem(
            itemPersistentId,
            itemAssetName,
            string.Empty);
        if (legacyItem == null || !legacyItem.IsEquipment ||
            legacyItem.equipmentSlot != slot)
        {
            if (!string.IsNullOrWhiteSpace(itemPersistentId) ||
                !string.IsNullOrWhiteSpace(itemAssetName))
            {
                Debug.LogWarning("Skipped an invalid legacy equipped item reference.");
            }
            return;
        }

        mercenary.RestoreEquippedEquipment(
            slot,
            EquipmentInstance.CreateFixed(legacyItem));
    }

    private void LogMismatchedEquipmentReference(
        EquipmentSlot slot,
        string itemPersistentId,
        string itemAssetName,
        ItemDataSO instanceItem)
    {
        if (instanceItem == null ||
            (string.IsNullOrWhiteSpace(itemPersistentId) &&
             string.IsNullOrWhiteSpace(itemAssetName)))
        {
            return;
        }

        ItemDataSO outerItem = FindItem(
            itemPersistentId,
            itemAssetName,
            string.Empty);
        if (outerItem != null && outerItem != instanceItem)
        {
            Debug.LogWarning(
                $"Equipped {slot} reference disagreed with its instance. The instance was used.");
        }
    }

    private static SavedEquipmentInstance CreateSavedEquipment(
        EquipmentInstance equipment)
    {
        // Broken or legacy equipment references must not prevent the entire
        // game from being saved when play mode or the application ends.
        if (equipment?.BaseItem == null)
        {
            return null;
        }

        SavedEquipmentInstance saved = new SavedEquipmentInstance
        {
            instanceId = equipment.InstanceId,
            baseItemAssetName = equipment.BaseItem.name,
            baseItemPersistentId = equipment.BaseItem.PersistentId,
            quality = equipment.Quality,
            enhancementLevel = equipment.EnhancementLevel,
            isLocked = equipment.IsLocked
        };
        if (equipment.Modifiers == null)
        {
            return saved;
        }

        foreach (EquipmentModifier modifier in equipment.Modifiers)
        {
            if (modifier != null)
            {
                saved.modifiers.Add(new SavedEquipmentModifier
                {
                    type = modifier.type,
                    value = modifier.value
                });
            }
        }
        return saved;
    }

    private EquipmentInstance RestoreEquipment(SavedEquipmentInstance saved)
    {
        if (saved == null)
        {
            return null;
        }

        ItemDataSO baseItem = FindItem(
            saved.baseItemPersistentId,
            saved.baseItemAssetName,
            string.Empty);
        if (baseItem == null || !baseItem.IsEquipment)
        {
            return null;
        }

        List<EquipmentModifier> modifiers = new List<EquipmentModifier>();
        if (saved.modifiers != null)
        {
            foreach (SavedEquipmentModifier modifier in saved.modifiers)
            {
                if (modifier != null)
                {
                    modifiers.Add(new EquipmentModifier(modifier.type, modifier.value));
                }
            }
        }
        return EquipmentInstance.CreateRestored(
            saved.instanceId,
            baseItem,
            saved.quality,
            modifiers,
            saved.enhancementLevel,
            saved.isLocked);
    }

    private void Subscribe()
    {
        if (merchantData != null) merchantData.GoldChanged += HandleChanged;
        if (merchantData != null) merchantData.ProgressionChanged += HandleChanged;
        if (dayManager != null) dayManager.DayChanged += HandleChanged;
        if (merchantInventory != null) merchantInventory.InventoryChanged += HandleChanged;
        if (hireManager != null) hireManager.MercenaryHired += HandleMercenaryChanged;
        if (hireManager != null) hireManager.MercenaryDismissed += HandleMercenaryChanged;
        if (hireManager != null) hireManager.ContractsChanged += HandleChanged;
        if (storyProgressManager != null) storyProgressManager.MilestoneCompleted += HandleStoryMilestoneCompleted;
        if (roadCargoSession != null) roadCargoSession.CargoChanged += HandleChanged;
        if (remoteSaleManager != null) remoteSaleManager.RemoteSaleChanged += HandleChanged;
        if (partyManager != null) partyManager.PartyChanged += HandleChanged;
        if (healingManager != null) healingManager.HealingChanged += HandleChanged;
        if (trainingGroundManager != null) trainingGroundManager.TrainingChanged += HandleChanged;
        if (battleManager != null) battleManager.BattleCompleted += HandleBattleCompleted;
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
            dungeonRunManager.DungeonStateChanged += HandleChanged;
        }
        if (dungeonExpeditionManager != null)
        {
            dungeonExpeditionManager.ExpeditionChanged += HandleChanged;
        }
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged += HandleChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged += HandleChanged;
        }
        if (onboardingGuideController != null)
        {
            onboardingGuideController.StateChanged += HandleOnboardingGuideChanged;
        }
    }

    private void Unsubscribe()
    {
        if (merchantData != null) merchantData.GoldChanged -= HandleChanged;
        if (merchantData != null) merchantData.ProgressionChanged -= HandleChanged;
        if (dayManager != null) dayManager.DayChanged -= HandleChanged;
        if (merchantInventory != null) merchantInventory.InventoryChanged -= HandleChanged;
        if (hireManager != null) hireManager.MercenaryHired -= HandleMercenaryChanged;
        if (hireManager != null) hireManager.MercenaryDismissed -= HandleMercenaryChanged;
        if (hireManager != null) hireManager.ContractsChanged -= HandleChanged;
        if (storyProgressManager != null) storyProgressManager.MilestoneCompleted -= HandleStoryMilestoneCompleted;
        if (roadCargoSession != null) roadCargoSession.CargoChanged -= HandleChanged;
        if (remoteSaleManager != null) remoteSaleManager.RemoteSaleChanged -= HandleChanged;
        if (partyManager != null) partyManager.PartyChanged -= HandleChanged;
        if (healingManager != null) healingManager.HealingChanged -= HandleChanged;
        if (trainingGroundManager != null) trainingGroundManager.TrainingChanged -= HandleChanged;
        if (battleManager != null) battleManager.BattleCompleted -= HandleBattleCompleted;
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
            dungeonRunManager.DungeonStateChanged -= HandleChanged;
        }
        if (dungeonExpeditionManager != null)
        {
            dungeonExpeditionManager.ExpeditionChanged -= HandleChanged;
        }
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged -= HandleChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged -= HandleChanged;
        }
        if (onboardingGuideController != null)
        {
            onboardingGuideController.StateChanged -= HandleOnboardingGuideChanged;
        }
    }

    private void HandleChanged() => RequestAutoSave();
    private void HandleChanged(int value) => RequestAutoSave();
    private void HandleMercenaryChanged(MercenaryInstance mercenary) => RequestAutoSave();
    private void HandleBattleCompleted(bool victory) => RequestAutoSave();
    private void HandleDungeonCompleted(bool cleared) => RequestAutoSave();
    private void HandleStoryMilestoneCompleted(StoryMilestone milestone)
    {
        if (isLoading || suppressAutoSaveAfterDelete)
        {
            return;
        }

        CancelPendingAutoSave();
        SaveGame();
    }
    private void HandleOnboardingGuideChanged(OnboardingGuideStep step) => RequestAutoSave();

    private void RequestAutoSave()
    {
        // 破棄済みインスタンスへのゾンビ購読から呼ばれても例外にしない。
        // Unity の == null は破棄済みオブジェクトを検出する。
        if (this == null)
        {
            return;
        }

        if (isLoading || suppressAutoSaveAfterDelete ||
            pendingAutoSaveCoroutine != null)
        {
            return;
        }

        if (!isActiveAndEnabled)
        {
            SaveGame();
            return;
        }

        pendingAutoSaveCoroutine = StartCoroutine(SaveAfterUiTransition());
    }

    private IEnumerator SaveAfterUiTransition()
    {
        const float delaySeconds = 0.25f;
        float saveAt = Time.realtimeSinceStartup + delaySeconds;
        while (Time.realtimeSinceStartup < saveAt)
        {
            yield return null;
        }

        pendingAutoSaveCoroutine = null;
        SaveGame();
    }

    private void CancelPendingAutoSave()
    {
        if (pendingAutoSaveCoroutine == null)
        {
            return;
        }

        StopCoroutine(pendingAutoSaveCoroutine);
        pendingAutoSaveCoroutine = null;
    }

    private ItemDataSO FindItem(
        string persistentId,
        string assetName,
        string itemName)
    {
        ItemDataSO persistentMatch =
            GameAssetRepository.FindByPersistentId<ItemDataSO>(
                persistentId,
                assetName);
        if (persistentMatch != null)
        {
            return persistentMatch;
        }

        foreach (ItemDataSO item in
                 GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }

    private MercenaryDataSO FindMercenaryData(
        string persistentId,
        string assetName)
    {
        return GameAssetRepository.FindByPersistentId<MercenaryDataSO>(
            persistentId,
            assetName);
    }

    private MercenaryArchetypeSO FindArchetype(
        string persistentId,
        string assetName)
    {
        return GameAssetRepository.FindByPersistentId<MercenaryArchetypeSO>(
            persistentId,
            assetName);
    }

    /// <summary>
    /// 参照を一度だけ解決する。SaveGame / LoadGame から毎回呼ばれるため、
    /// 解決済みの参照は再解決しない。未解決のものだけ GetComponent を試し、
    /// それでも見つからなければシーン全体を走査する。
    ///
    /// Unity の未設定 SerializeField や破棄済みコンポーネントは C# 参照と
    /// しては "fake null" になり得るため、?? ではなく Unity の == null 判定
    /// で解決する。?? だと破棄済み参照が残り、以降の解決が行われない。
    /// </summary>
    private void ResolveReferences()
    {
        Resolve(ref merchantData);
        Resolve(ref dayManager);
        Resolve(ref merchantInventory);
        Resolve(ref hireManager);
        Resolve(ref partyManager);
        Resolve(ref healingManager);
        Resolve(ref trainingGroundManager);
        Resolve(ref battleManager);
        Resolve(ref dungeonRunManager);
        Resolve(ref dungeonExpeditionManager);
        Resolve(ref progressionManager);
        Resolve(ref debtManager);
        Resolve(ref townProgressState);
        Resolve(ref storyProgressManager);
        Resolve(ref roadCargoSession);
        Resolve(ref remoteSaleManager);
        Resolve(ref monsterCodexManager);
        Resolve(ref onboardingGuideController);
    }

    /// <summary>
    /// 既に解決済みなら何もしない。未解決のときだけ、同一 GameObject →
    /// シーン全体の順に探す。FindObjectOfType はシーン全体を走査するため、
    /// 保存のたびに呼ばれないようこのガードを挟む。
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
