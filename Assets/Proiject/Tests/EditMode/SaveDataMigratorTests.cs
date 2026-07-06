using NUnit.Framework;

public sealed class SaveDataMigratorTests
{
    [Test]
    public void Migrate_PreVersion16_PopulatesProgressionAndDebt()
    {
        GameSaveData data = new GameSaveData
        {
            version = 8,
            merchantLevel = 3,
            merchantExperience = 125,
            merchantSkillPoints = 0,
            currentDay = 61,
            remainingDebt = 1,
            debtPaymentArrears = 999,
            processedDebtMonths = 0
        };

        SaveDataMigrator.Migrate(data);

        Assert.That(data.version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(data.merchantSkillPoints, Is.EqualTo(4));
        Assert.That(
            data.lifetimeGoldEarned,
            Is.EqualTo(
                MerchantData.EstimateLifetimeEarningsForMigration(3, 125)));
        Assert.That(data.remainingDebt, Is.EqualTo(DebtManager.InitialDebt));
        Assert.That(data.debtPaymentArrears, Is.Zero);
        Assert.That(data.processedDebtMonths, Is.EqualTo(2));
        Assert.That(data.unlockedTownIndices, Is.Null);
        Assert.That(data.dungeonFloorProgress, Is.Null);
    }

    [Test]
    public void Migrate_LegacyAssetNames_PopulatesPersistentIds()
    {
        ItemDataSO item = FirstAsset<ItemDataSO>();
        DungeonDataSO dungeon = FirstAsset<DungeonDataSO>();
        Assert.That(item, Is.Not.Null);
        Assert.That(dungeon, Is.Not.Null);

        GameSaveData data = new GameSaveData { version = 17 };
        data.selectedDungeonAssetName = dungeon.name;
        data.inventory.Add(new SavedInventoryItem
        {
            itemAssetName = item.name,
            itemName = item.itemName,
            amount = 1
        });
        data.discoveredEquipmentAssetNames.Add(item.name);

        SaveDataMigrator.Migrate(data);

        Assert.That(
            data.selectedDungeonPersistentId,
            Is.EqualTo(dungeon.PersistentId));
        Assert.That(
            data.inventory[0].itemPersistentId,
            Is.EqualTo(item.PersistentId));
        Assert.That(
            data.discoveredEquipmentPersistentIds,
            Does.Contain(item.PersistentId));
    }

    [Test]
    public void Migrate_CurrentData_IsIdempotent()
    {
        GameSaveData data = new GameSaveData();

        SaveDataMigrator.Migrate(data);
        SaveDataMigrator.Migrate(data);

        Assert.That(data.version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(data.inventory, Is.Not.Null);
        Assert.That(data.equipmentInventory, Is.Not.Null);
        Assert.That(data.hiredMercenaries, Is.Not.Null);
        Assert.That(data.partyMemberIds, Is.Not.Null);
    }

    private static T FirstAsset<T>()
        where T : UnityEngine.Object
    {
        foreach (T asset in GameAssetRepository.LoadAll<T>())
        {
            if (asset != null)
            {
                return asset;
            }
        }

        return null;
    }
}
