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

    [Test]
    public void Migrate_Version28_InitializesEmptyTrainingAssignments()
    {
        GameSaveData data = new GameSaveData
        {
            version = 28,
            trainingAssignments = null
        };

        SaveDataMigrator.Migrate(data);

        Assert.That(data.version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(data.trainingAssignments, Is.Not.Null);
        Assert.That(data.trainingAssignments, Is.Empty);
    }

    [Test]
    public void Migrate_PreStorySave_InfersCompletedMilestones()
    {
        DungeonDataSO dungeon = FirstAsset<DungeonDataSO>();
        Assert.That(dungeon, Is.Not.Null);
        GameSaveData data = new GameSaveData
        {
            version = 20,
            remainingDebt = 0
        };
        data.hiredMercenaries.Add(new SavedMercenary());
        data.dungeonFloorProgress.Add(new SavedDungeonFloorProgress
        {
            dungeonPersistentId = dungeon.PersistentId,
            dungeonAssetName = dungeon.name,
            clearedFloors = dungeon.totalFloors
        });
        data.unlockedTownIndices.Add(1);
        data.unlockedTownIndices.Add(3);
        data.unlockedTownIndices.Add(6);
        data.unlockedTownIndices.Add(WorldMapService.HiddenIslandTownIndex);

        SaveDataMigrator.Migrate(data);

        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.OpeningDebtNotice));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.FirstMercenary));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.FirstDungeonClear));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.LeafUnlocked));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.RegionGateCleared));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.AbyssReached));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.HiddenIslandReached));
        Assert.That(data.completedStoryMilestones,
            Does.Contain(StoryMilestone.DebtCleared));
    }

    [Test]
    public void Migrate_Version22_AssignsAllInventoryToSavedCurrentTown()
    {
        GameSaveData data = new GameSaveData
        {
            version = 22,
            currentTownIndex = 5
        };
        data.inventory.Add(new SavedInventoryItem { amount = 2 });
        data.equipmentInventory.Add(new SavedEquipmentInstance());

        SaveDataMigrator.Migrate(data);

        Assert.That(data.inventory[0].townIndex, Is.EqualTo(5));
        Assert.That(data.equipmentInventory[0].townIndex, Is.EqualTo(5));
    }

    [Test]
    public void Migrate_PreStorySave_DoesNotTreatPartialFloorAsDungeonClear()
    {
        DungeonDataSO dungeon = FirstAsset<DungeonDataSO>();
        Assert.That(dungeon, Is.Not.Null);
        GameSaveData data = new GameSaveData { version = 20 };
        data.dungeonFloorProgress.Add(new SavedDungeonFloorProgress
        {
            dungeonPersistentId = dungeon.PersistentId,
            dungeonAssetName = dungeon.name,
            clearedFloors = dungeon.totalFloors > 1
                ? dungeon.totalFloors - 1
                : 0
        });

        SaveDataMigrator.Migrate(data);

        Assert.That(
            data.completedStoryMilestones.Contains(
                StoryMilestone.FirstDungeonClear),
            Is.False);
    }

    [Test]
    public void Migrate_FutureVersion_DoesNotDowngradeOrMutateData()
    {
        int futureVersion = GameSaveData.CurrentVersion + 1;
        GameSaveData data = new GameSaveData
        {
            version = futureVersion,
            gold = 1234
        };

        SaveDataMigrator.Migrate(data);

        Assert.That(data.version, Is.EqualTo(futureVersion));
        Assert.That(data.gold, Is.EqualTo(1234));
    }

    [Test]
    public void Migrate_Version31_ReturnsConvoyCargoAndClearsLegacyDuties()
    {
        ItemDataSO item = FirstAsset<ItemDataSO>();
        Assert.That(item, Is.Not.Null);
        GameSaveData data = new GameSaveData { version = 31 };
        data.hiredMercenaries.Add(new SavedMercenary
        {
            instanceId = "escort",
            townIndex = 4
        });
        data.transportConvoys.Add(new SavedTransportConvoy
        {
            originTownIndex = 1,
            escortInstanceIds = new System.Collections.Generic.List<string>
            {
                "escort"
            },
            cargo = new System.Collections.Generic.List<SavedTransportCargo>
            {
                new SavedTransportCargo
                {
                    itemPersistentId = item.PersistentId,
                    itemAssetName = item.name,
                    amount = 3
                }
            }
        });
        data.dungeonExpeditions.Add(new SavedDungeonExpedition());

        SaveDataMigrator.Migrate(data);

        Assert.That(data.transportConvoys, Is.Empty);
        Assert.That(data.dungeonExpeditions, Is.Empty);
        Assert.That(data.hiredMercenaries[0].townIndex, Is.EqualTo(1));
        Assert.That(data.inventory.Exists(value =>
            value.townIndex == 1 &&
            value.itemPersistentId == item.PersistentId &&
            value.amount == 3), Is.True);

        SaveDataMigrator.Migrate(data);

        Assert.That(data.inventory.FindAll(value =>
            value.townIndex == 1 &&
            value.itemPersistentId == item.PersistentId).Count, Is.EqualTo(1));
        Assert.That(data.inventory.Find(value =>
            value.townIndex == 1 &&
            value.itemPersistentId == item.PersistentId).amount, Is.EqualTo(3));
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
