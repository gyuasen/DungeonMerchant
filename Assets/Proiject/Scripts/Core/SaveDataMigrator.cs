using System.Collections.Generic;
using UnityEngine;

public static class SaveDataMigrator
{
    public static GameSaveData Migrate(GameSaveData data)
    {
        if (data == null)
        {
            return null;
        }

        if (data.version > GameSaveData.CurrentVersion)
        {
            return data;
        }

        int sourceVersion = Mathf.Max(0, data.version);
        MigrateMerchantProgression(data, sourceVersion);
        MigrateDebt(data, sourceVersion);
        PreserveLegacyCollectionSemantics(data, sourceVersion);
        EnsureCollections(data);
        MigrateTownInventories(data, sourceVersion);
        MigrateMercenaryConsumables(data, sourceVersion);
        MigrateMercenaryLocations(data, sourceVersion);
        PopulatePersistentIds(data);
        MigrateStoryProgress(data, sourceVersion);
        data.version = GameSaveData.CurrentVersion;
        return data;
    }

    private static void MigrateMerchantProgression(
        GameSaveData data,
        int sourceVersion)
    {
        if (sourceVersion < 9)
        {
            data.merchantSkillPoints = Mathf.Max(
                2,
                data.merchantLevel + 1);
        }

        if (sourceVersion < 16)
        {
            data.lifetimeGoldEarned =
                MerchantData.EstimateLifetimeEarningsForMigration(
                    Mathf.Max(1, data.merchantLevel),
                    data.merchantExperience);
        }
    }

    private static void MigrateDebt(GameSaveData data, int sourceVersion)
    {
        if (sourceVersion >= 16)
        {
            return;
        }

        data.remainingDebt = DebtManager.InitialDebt;
        data.debtPaymentArrears = 0;
        data.processedDebtMonths =
            (Mathf.Max(1, data.currentDay) - 1) /
            DebtManager.DaysPerMonth;
    }

    private static void PreserveLegacyCollectionSemantics(
        GameSaveData data,
        int sourceVersion)
    {
        if (sourceVersion < 11)
        {
            data.unlockedTownIndices = null;
        }

        if (sourceVersion < 12)
        {
            data.dungeonFloorProgress = null;
        }
    }

    private static void MigrateStoryProgress(GameSaveData data, int sourceVersion)
    {
        if (sourceVersion >= 21)
        {
            return;
        }

        data.completedStoryMilestones = new List<StoryMilestone>
        {
            StoryMilestone.OpeningDebtNotice
        };
        if (data.hiredMercenaries != null && data.hiredMercenaries.Count > 0)
        {
            AddStoryMilestone(data, StoryMilestone.FirstMercenary);
        }
        if (HasFullyClearedDungeon(data.dungeonFloorProgress))
        {
            AddStoryMilestone(data, StoryMilestone.FirstDungeonClear);
        }
        if (data.unlockedTownIndices != null)
        {
            if (data.unlockedTownIndices.Contains(1)) AddStoryMilestone(data, StoryMilestone.LeafUnlocked);
            if (WorldMapService.HasUnlockedTownInWorld(data.unlockedTownIndices, 1)) AddStoryMilestone(data, StoryMilestone.RegionGateCleared);
            if (data.unlockedTownIndices.Contains(6)) AddStoryMilestone(data, StoryMilestone.AbyssReached);
            if (data.unlockedTownIndices.Contains(WorldMapService.HiddenIslandTownIndex)) AddStoryMilestone(data, StoryMilestone.HiddenIslandReached);
        }
        if (data.remainingDebt <= 0) AddStoryMilestone(data, StoryMilestone.DebtCleared);
    }

    private static void MigrateTownInventories(
        GameSaveData data,
        int sourceVersion)
    {
        if (sourceVersion >= 23)
        {
            return;
        }

        foreach (SavedInventoryItem item in data.inventory)
        {
            if (item != null)
            {
                item.townIndex = data.currentTownIndex;
            }
        }

        foreach (SavedEquipmentInstance equipment in data.equipmentInventory)
        {
            if (equipment != null)
            {
                equipment.townIndex = data.currentTownIndex;
            }
        }
    }

    private static void MigrateMercenaryConsumables(
        GameSaveData data,
        int sourceVersion)
    {
        if (sourceVersion >= 26 || data.hiredMercenaries == null)
        {
            return;
        }

        foreach (SavedMercenary mercenary in data.hiredMercenaries)
        {
            if (mercenary != null)
            {
                mercenary.consumableSlots = new SavedMercenaryConsumableSlot[2]
                {
                    new SavedMercenaryConsumableSlot(),
                    new SavedMercenaryConsumableSlot()
                };
            }
        }
    }

    private static void MigrateMercenaryLocations(
        GameSaveData data,
        int sourceVersion)
    {
        if (sourceVersion >= 28 || data.hiredMercenaries == null)
        {
            return;
        }

        foreach (SavedMercenary mercenary in data.hiredMercenaries)
        {
            if (mercenary != null)
            {
                mercenary.townIndex = data.currentTownIndex;
            }
        }
    }

    private static bool HasFullyClearedDungeon(
        List<SavedDungeonFloorProgress> progressEntries)
    {
        if (progressEntries == null)
        {
            return false;
        }

        foreach (SavedDungeonFloorProgress progress in progressEntries)
        {
            if (progress == null || progress.clearedFloors <= 0)
            {
                continue;
            }

            DungeonDataSO dungeon =
                GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                    progress.dungeonPersistentId,
                    progress.dungeonAssetName);
            if (dungeon != null &&
                progress.clearedFloors >= Mathf.Max(1, dungeon.totalFloors))
            {
                return true;
            }
        }

        return false;
    }

    private static void EnsureCollections(GameSaveData data)
    {
        if (data.inventory == null) data.inventory = new List<SavedInventoryItem>();
        if (data.equipmentInventory == null) data.equipmentInventory = new List<SavedEquipmentInstance>();
        if (data.hiredMercenaries == null) data.hiredMercenaries = new List<SavedMercenary>();
        if (data.partyMemberIds == null) data.partyMemberIds = new List<string>();
        if (data.transportConvoys == null) data.transportConvoys = new List<SavedTransportConvoy>();
        if (data.remoteSaleOrders == null) data.remoteSaleOrders = new List<SavedRemoteSaleOrder>();
        if (data.dungeonExpeditions == null)
        {
            data.dungeonExpeditions = new List<SavedDungeonExpedition>();
        }
        if (data.discoveredEquipmentAssetNames == null) data.discoveredEquipmentAssetNames = new List<string>();
        if (data.discoveredEquipmentPersistentIds == null) data.discoveredEquipmentPersistentIds = new List<string>();
        if (data.encounteredEnemyIds == null) data.encounteredEnemyIds = new List<string>();
        if (data.completedStoryMilestones == null) data.completedStoryMilestones = new List<StoryMilestone>();
        if (data.progression == null) data.progression = new ProgressionSaveData();
    }

    private static void PopulatePersistentIds(GameSaveData data)
    {
        data.selectedDungeonPersistentId = ResolvePersistentId<DungeonDataSO>(
            data.selectedDungeonPersistentId,
            data.selectedDungeonAssetName);

        if (data.dungeonFloorProgress != null)
        {
            foreach (SavedDungeonFloorProgress progress in
                     data.dungeonFloorProgress)
            {
                if (progress != null)
                {
                    progress.dungeonPersistentId =
                        ResolvePersistentId<DungeonDataSO>(
                            progress.dungeonPersistentId,
                            progress.dungeonAssetName);
                }
            }
        }

        foreach (SavedInventoryItem item in data.inventory)
        {
            if (item != null)
            {
                item.itemPersistentId = ResolveItemId(
                    item.itemPersistentId,
                    item.itemAssetName,
                    item.itemName);
            }
        }

        foreach (SavedEquipmentInstance equipment in data.equipmentInventory)
        {
            PopulateEquipmentId(equipment);
        }

        foreach (SavedMercenary mercenary in data.hiredMercenaries)
        {
            PopulateMercenaryIds(mercenary);
        }

        foreach (string assetName in data.discoveredEquipmentAssetNames)
        {
            string persistentId =
                ResolvePersistentId<ItemDataSO>(string.Empty, assetName);
            AddUnique(data.discoveredEquipmentPersistentIds, persistentId);
        }
    }

    private static void PopulateMercenaryIds(SavedMercenary mercenary)
    {
        if (mercenary == null)
        {
            return;
        }

        mercenary.baseDataPersistentId =
            ResolvePersistentId<MercenaryDataSO>(
                mercenary.baseDataPersistentId,
                mercenary.baseDataAssetName);
        mercenary.archetypePersistentId =
            ResolvePersistentId<MercenaryArchetypeSO>(
                mercenary.archetypePersistentId,
                mercenary.archetypeAssetName);
        mercenary.equippedWeaponPersistentId =
            ResolvePersistentId<ItemDataSO>(
                mercenary.equippedWeaponPersistentId,
                mercenary.equippedWeaponAssetName);
        mercenary.equippedArmorPersistentId =
            ResolvePersistentId<ItemDataSO>(
                mercenary.equippedArmorPersistentId,
                mercenary.equippedArmorAssetName);
        mercenary.equippedAccessoryPersistentId =
            ResolvePersistentId<ItemDataSO>(
                mercenary.equippedAccessoryPersistentId,
                mercenary.equippedAccessoryAssetName);
        PopulateEquipmentId(mercenary.equippedWeaponInstance);
        PopulateEquipmentId(mercenary.equippedArmorInstance);
        PopulateEquipmentId(mercenary.equippedAccessoryInstance);
        if (mercenary.consumableSlots == null)
        {
            mercenary.consumableSlots = new SavedMercenaryConsumableSlot[2]
            {
                new SavedMercenaryConsumableSlot(),
                new SavedMercenaryConsumableSlot()
            };
        }

        foreach (SavedMercenaryConsumableSlot slot in mercenary.consumableSlots)
        {
            if (slot != null)
            {
                slot.itemPersistentId = ResolvePersistentId<ItemDataSO>(
                    slot.itemPersistentId,
                    string.Empty);
                slot.count = Mathf.Clamp(slot.count, 0, MercenaryConsumableSlot.MaxCount);
            }
        }
    }

    private static void PopulateEquipmentId(SavedEquipmentInstance equipment)
    {
        if (equipment != null)
        {
            equipment.baseItemPersistentId =
                ResolvePersistentId<ItemDataSO>(
                    equipment.baseItemPersistentId,
                    equipment.baseItemAssetName);
        }
    }

    private static string ResolveItemId(
        string persistentId,
        string assetName,
        string itemName)
    {
        ItemDataSO item = GameAssetRepository.FindByPersistentId<ItemDataSO>(
            persistentId,
            assetName);
        if (item == null && !string.IsNullOrWhiteSpace(itemName))
        {
            foreach (ItemDataSO candidate in
                     GameAssetRepository.LoadAll<ItemDataSO>())
            {
                if (candidate != null && candidate.itemName == itemName)
                {
                    item = candidate;
                    break;
                }
            }
        }

        return item != null ? item.PersistentId : persistentId;
    }

    private static string ResolvePersistentId<T>(
        string persistentId,
        string legacyAssetName)
        where T : Object
    {
        T asset = GameAssetRepository.FindByPersistentId<T>(
            persistentId,
            legacyAssetName);
        return asset != null
            ? GameAssetRepository.GetPersistentId(asset)
            : persistentId;
    }

    private static void AddUnique(List<string> values, string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !values.Contains(value))
        {
            values.Add(value);
        }
    }

    private static void AddStoryMilestone(GameSaveData data, StoryMilestone milestone)
    {
        if (!data.completedStoryMilestones.Contains(milestone))
        {
            data.completedStoryMilestones.Add(milestone);
        }
    }
}
