using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "game-save.json";

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
    private SimpleMercenaryHireUI simpleUI;
    private bool initialized;
    private bool isLoading;

    public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public void InitializeAndLoad()
    {
        if (initialized)
        {
            return;
        }

        ResolveReferences();
        LoadGame();
        Subscribe();
        initialized = true;
    }

    [ContextMenu("ゲームを保存")]
    public void SaveGame()
    {
        if (isLoading)
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

            isLoading = true;
            ApplySaveData(data);
            Debug.Log($"ゲームを読み込みました: {SavePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"セーブデータの読込に失敗しました: {exception.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    [ContextMenu("セーブデータを削除")]
    public void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        dungeonRunManager?.ResetDungeonProgress();
        Debug.Log("セーブデータを削除しました。");
    }

    private void OnApplicationQuit()
    {
        if (initialized)
        {
            SaveGame();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized)
        {
            SaveGame();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
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
            currentTownIndex = simpleUI != null ? simpleUI.CurrentTownIndex : 2,
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

        if (simpleUI != null)
        {
            data.unlockedTownIndices.Clear();
            data.unlockedTownIndices.AddRange(simpleUI.GetUnlockedTownIndices());
        }

        if (dungeonRunManager != null)
        {
            data.dungeonFloorProgress.AddRange(
                dungeonRunManager.CreateFloorProgressSaveData());
        }

        if (merchantInventory != null)
        {
            data.discoveredEquipmentAssetNames.AddRange(
                merchantInventory.DiscoveredEquipmentAssetNames);
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
                    data.equipmentInventory.Add(CreateSavedEquipment(equipment));
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
            data.version >= 16
                ? data.lifetimeGoldEarned
                : -1);
        int skillPoints = data.version >= 9
            ? data.merchantSkillPoints
            : Mathf.Max(2, data.merchantLevel + 1);
        merchantData?.RestoreSkills(
            skillPoints,
            data.merchantNegotiation,
            data.merchantLeadership,
            data.merchantAppraisal,
            data.merchantLogistics);
        debtManager?.Restore(
            data.version >= 16
                ? data.remainingDebt
                : DebtManager.InitialDebt,
            data.version >= 16 ? data.debtPaymentArrears : 0,
            data.version >= 16
                ? data.processedDebtMonths
                : (Mathf.Max(1, data.currentDay) - 1) /
                  DebtManager.DaysPerMonth);
        dayManager?.SetCurrentDay(data.currentDay);
        simpleUI?.RestoreTownProgress(
            data.currentTownIndex,
            data.version >= 11 ? data.unlockedTownIndices : null);

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
            data.version >= 12 ? data.dungeonFloorProgress : null);
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
        SavedEquipmentInstance saved = new SavedEquipmentInstance
        {
            instanceId = equipment.InstanceId,
            baseItemAssetName = equipment.BaseItem.name,
            baseItemPersistentId = equipment.BaseItem.PersistentId,
            quality = equipment.Quality,
            enhancementLevel = equipment.EnhancementLevel,
            isLocked = equipment.IsLocked
        };
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
        simpleUI =
            GetComponent<SimpleMercenaryHireUI>() ??
            FindObjectOfType<SimpleMercenaryHireUI>();
    }
}
