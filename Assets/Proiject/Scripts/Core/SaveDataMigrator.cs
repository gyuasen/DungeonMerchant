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

        int sourceVersion = Mathf.Max(0, data.version);
        MigrateMerchantProgression(data, sourceVersion);
        MigrateDebt(data, sourceVersion);
        PreserveLegacyCollectionSemantics(data, sourceVersion);
        EnsureCollections(data);
        PopulatePersistentIds(data);
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

    private static void EnsureCollections(GameSaveData data)
    {
        data.inventory ??= new List<SavedInventoryItem>();
        data.equipmentInventory ??= new List<SavedEquipmentInstance>();
        data.hiredMercenaries ??= new List<SavedMercenary>();
        data.partyMemberIds ??= new List<string>();
        data.discoveredEquipmentAssetNames ??= new List<string>();
        data.discoveredEquipmentPersistentIds ??= new List<string>();
        data.progression ??= new ProgressionSaveData();
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
}
