using System;
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
    private BattleManager battleManager;
    private DungeonRunManager dungeonRunManager;
    private ProgressionManager progressionManager;
    private DebtManager debtManager;
    private TownProgressState townProgressState;
    private StoryProgressManager storyProgressManager;
    private bool initialized;
    private bool isLoading;
    private bool suppressAutoSaveAfterDelete;
    private string savePathOverride = string.Empty;

    public string SavePath => !string.IsNullOrEmpty(savePathOverride)
        ? savePathOverride
        : IsAutomatedTestRun()
            ? GetAutomatedTestSavePath()
            : Path.Combine(Application.persistentDataPath, SaveFileName);
    public bool HasSaveData => File.Exists(SavePath);

    public void InitializeAndLoad()
    {
        if (initialized)
        {
            return;
        }

        ResolveReferences();
        bool hasExistingSave = File.Exists(SavePath);
        LoadGame();
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
        suppressAutoSaveAfterDelete = true;
        if (File.Exists(SavePath))
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
            foreach (InventoryItemStack stack in merchantInventory.Items)
            {
                if (stack?.Item == null || stack.Amount <= 0)
                {
                    continue;
                }

                data.inventory.Add(new SavedInventoryItem
                {
                    itemPersistentId = stack.Item.PersistentId,
                    itemAssetName = stack.Item.name,
                    itemName = stack.Item.itemName,
                    amount = stack.Amount
                });
            }

            foreach (EquipmentInstance equipment in merchantInventory.EquipmentInstances)
            {
                if (equipment?.BaseItem != null)
                {
                    SavedEquipmentInstance savedEquipment =
                        CreateSavedEquipment(equipment);
                    if (savedEquipment != null)
                    {
                        data.equipmentInventory.Add(savedEquipment);
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
                            : null
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

        List<InventoryItemStack> restoredItems = new List<InventoryItemStack>();
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
                    restoredItems.Add(new InventoryItemStack(item, savedItem.amount));
                }
            }
        }
        merchantInventory?.RestoreItems(restoredItems);

        List<EquipmentInstance> restoredEquipment =
            new List<EquipmentInstance>();
        if (data.equipmentInventory != null)
        {
            foreach (SavedEquipmentInstance savedEquipment in data.equipmentInventory)
            {
                EquipmentInstance equipment = RestoreEquipment(savedEquipment);
                if (equipment != null)
                {
                    restoredEquipment.Add(equipment);
                }
            }
        }
        merchantInventory?.RestoreEquipmentInstances(restoredEquipment);
        merchantInventory?.RestoreDiscoveredEquipment(
            data.discoveredEquipmentPersistentIds,
            data.discoveredEquipmentAssetNames);

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
        return mercenary;
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
        if (partyManager != null) partyManager.PartyChanged += HandleChanged;
        if (healingManager != null) healingManager.HealingChanged += HandleChanged;
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
        if (partyManager != null) partyManager.PartyChanged -= HandleChanged;
        if (healingManager != null) healingManager.HealingChanged -= HandleChanged;
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

    private void HandleChanged() => SaveGame();
    private void HandleChanged(int value) => SaveGame();
    private void HandleMercenaryChanged(MercenaryInstance mercenary) => SaveGame();
    private void HandleBattleCompleted(bool victory) => SaveGame();
    private void HandleDungeonCompleted(bool cleared) => SaveGame();
    private void HandleStoryMilestoneCompleted(StoryMilestone milestone) => SaveGame();

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
    }
}
