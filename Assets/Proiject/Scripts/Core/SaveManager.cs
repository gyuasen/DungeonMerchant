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
    private ProgressionManager progressionManager;
    private DebtManager debtManager;
    private TownProgressState townProgressState;
    private StoryProgressManager storyProgressManager;
    private TransportManager transportManager;
    private DungeonExpeditionManager dungeonExpeditionManager;
    private RemoteSaleManager remoteSaleManager;
    private MonsterCodexManager monsterCodexManager;
    private bool initialized;
    private bool isLoading;
    private bool suppressAutoSaveAfterDelete;
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

    public void InitializeAndLoad()
    {
        if (initialized)
        {
            return;
        }

        ResolveReferences();
        bool hasExistingSave = File.Exists(SavePath);
        if (hasExistingSave)
        {
            LoadGame();
        }
        else
        {
            ApplySaveData(new GameSaveData());
        }
        Subscribe();
        if (!hasExistingSave)
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

        ResolveReferences();
        GameSaveData data = CreateSaveData();
        string directory = Path.GetDirectoryName(SavePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"ゲームを保存しました: {SavePath}");
    }

    [ContextMenu("ゲームを読込")]
    public void LoadGame()
    {
        ResolveReferences();
        if (!File.Exists(SavePath))
        {
            return;
        }

        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(
                File.ReadAllText(SavePath));
            if (data == null)
            {
                return;
            }

            if (data.version > GameSaveData.CurrentVersion)
            {
                Debug.LogError(
                    $"Save data version {data.version} is newer than the " +
                    $"supported version {GameSaveData.CurrentVersion}. " +
                    "The save was not loaded or modified.");
                return;
            }

            bool requiresMigration =
                data.version != GameSaveData.CurrentVersion;
            data = SaveDataMigrator.Migrate(data);
            isLoading = true;
            storyProgressManager?.BeginRestore();
            ApplySaveData(data);
            if (requiresMigration)
            {
                File.WriteAllText(
                    SavePath,
                    JsonUtility.ToJson(data, true));
                Debug.Log(
                    $"Save data migrated to version " +
                    $"{GameSaveData.CurrentVersion}.");
            }
            Debug.Log($"ゲームを読み込みました: {SavePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"セーブデータの読込に失敗しました: {exception.Message}");
        }
        finally
        {
            isLoading = false;
            storyProgressManager?.EndRestore();
        }
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

                data.hiredMercenaries.Add(new SavedMercenary
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
                });
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

        if (transportManager != null)
        {
            data.transportConvoys = transportManager.CreateSaveData();
        }
        if (dungeonExpeditionManager != null)
        {
            data.dungeonExpeditions = dungeonExpeditionManager.CreateSaveData();
        }
        if (trainingGroundManager != null)
        {
            data.trainingAssignments = trainingGroundManager.CreateSaveData();
        }
        if (remoteSaleManager != null)
        {
            data.remoteSaleOrders = remoteSaleManager.CreateSaveData();
        }
        if (monsterCodexManager != null)
        {
            data.encounteredEnemyIds.AddRange(
                monsterCodexManager.EncounteredEnemyIds);
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
        transportManager?.Restore(data.transportConvoys, mercenaryById);
        dungeonExpeditionManager?.Restore(data.dungeonExpeditions, mercenaryById);
        remoteSaleManager?.Restore(data.remoteSaleOrders);
        progressionManager?.Restore(data.progression);

        dungeonRunManager?.RestoreProgress(
            (DungeonGrade)Mathf.Clamp(
                data.highestUnlockedDungeonGrade,
                (int)DungeonGrade.Low,
                (int)DungeonGrade.Highest),
            data.selectedDungeonAssetName,
            data.selectedDungeonPersistentId,
            data.dungeonFloorProgress);
        dungeonRunManager?.SetCurrentWorldMapIndex(townProgressState.CurrentWorldMapIndex);
        storyProgressManager?.RestoreCompletedMilestones(
            data.completedStoryMilestones);
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
            mercenary.RestoreEquippedEquipment(slot, equipment);
            return;
        }

        mercenary.RestoreEquippedEquipment(
            slot,
            FindItem(itemPersistentId, itemAssetName, string.Empty));
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
        if (baseItem == null)
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
        if (transportManager != null) transportManager.TransportChanged += HandleChanged;
        if (dungeonExpeditionManager != null) dungeonExpeditionManager.ExpeditionChanged += HandleChanged;
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
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged += HandleChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged += HandleChanged;
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
        if (transportManager != null) transportManager.TransportChanged -= HandleChanged;
        if (dungeonExpeditionManager != null) dungeonExpeditionManager.ExpeditionChanged -= HandleChanged;
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
        if (progressionManager != null)
        {
            progressionManager.ProgressionChanged -= HandleChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged -= HandleChanged;
        }
    }

    private void HandleChanged() => RequestAutoSave();
    private void HandleChanged(int value) => RequestAutoSave();
    private void HandleMercenaryChanged(MercenaryInstance mercenary) => RequestAutoSave();
    private void HandleBattleCompleted(bool victory) => RequestAutoSave();
    private void HandleDungeonCompleted(bool cleared) => RequestAutoSave();
    private void HandleStoryMilestoneCompleted(StoryMilestone milestone) => RequestAutoSave();

    private void RequestAutoSave()
    {
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

    private void ResolveReferences()
    {
        merchantData = GetComponent<MerchantData>() ?? FindObjectOfType<MerchantData>();
        dayManager = GetComponent<DayManager>() ?? FindObjectOfType<DayManager>();
        merchantInventory =
            GetComponent<MerchantInventory>() ?? FindObjectOfType<MerchantInventory>();
        hireManager =
            GetComponent<MercenaryHireManager>() ?? FindObjectOfType<MercenaryHireManager>();
        partyManager =
            GetComponent<MercenaryPartyManager>() ?? FindObjectOfType<MercenaryPartyManager>();
        healingManager =
            GetComponent<HealingManager>() ?? FindObjectOfType<HealingManager>();
        trainingGroundManager =
            GetComponent<TrainingGroundManager>() ??
            FindObjectOfType<TrainingGroundManager>();
        battleManager = GetComponent<BattleManager>() ?? FindObjectOfType<BattleManager>();
        dungeonRunManager =
            GetComponent<DungeonRunManager>() ?? FindObjectOfType<DungeonRunManager>();
        progressionManager =
            GetComponent<ProgressionManager>() ?? FindObjectOfType<ProgressionManager>();
        debtManager =
            GetComponent<DebtManager>() ?? FindObjectOfType<DebtManager>();
        townProgressState =
            GetComponent<TownProgressState>() ??
            FindObjectOfType<TownProgressState>();
        storyProgressManager =
            GetComponent<StoryProgressManager>() ??
            FindObjectOfType<StoryProgressManager>();
        transportManager =
            GetComponent<TransportManager>() ??
            FindObjectOfType<TransportManager>();
        dungeonExpeditionManager =
            GetComponent<DungeonExpeditionManager>() ??
            FindObjectOfType<DungeonExpeditionManager>();
        remoteSaleManager =
            GetComponent<RemoteSaleManager>() ??
            FindObjectOfType<RemoteSaleManager>();
        monsterCodexManager =
            GetComponent<MonsterCodexManager>() ??
            FindObjectOfType<MonsterCodexManager>();
    }
}
