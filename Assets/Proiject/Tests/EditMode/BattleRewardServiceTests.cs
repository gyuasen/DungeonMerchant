using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class BattleRewardServiceTests
{
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (Object created in createdObjects)
        {
            Object.DestroyImmediate(created);
        }

        createdObjects.Clear();
    }

    [Test]
    public void CalculateGoldReward_SumsNonNullEnemyRewards()
    {
        EnemyDataSO first = Track(CreateEnemy(3, false, 1f, 40));
        EnemyDataSO second = Track(CreateEnemy(8, false, 1f, 75));

        int result = BattleRewardService.CalculateGoldReward(
            new List<EnemyDataSO> { first, null, second });

        Assert.That(result, Is.EqualTo(115));
    }

    [Test]
    public void CalculateExperienceReward_AppliesGradeBossAndMultiplierRules()
    {
        EnemyDataSO normal = Track(CreateEnemy(8, false, 1.5f, 0));
        EnemyDataSO boss = Track(CreateEnemy(10, true, 1f, 0));

        int result = BattleRewardService.CalculateExperienceReward(
            new List<EnemyDataSO> { normal, boss });

        Assert.That(result, Is.EqualTo(390));
    }

    [TestCase(10, 50)]
[TestCase(9, 90)]
[TestCase(8, 160)]
[TestCase(7, 260)]
[TestCase(6, 390)]
[TestCase(5, 550)]
[TestCase(4, 740)]
[TestCase(3, 960)]
[TestCase(2, 1210)]
[TestCase(1, 1490)]
    public void CalculateBaseExperienceReward_UsesFlattenedGradeCurve(
        int grade,
        int expected)
    {
        Assert.That(
            BattleRewardService.CalculateBaseExperienceReward(grade),
            Is.EqualTo(expected));
    }

    [Test]
    public void LowGradeFirstFloor_GrantsAtLeastOneInitialLevelToFourMercenaries()
    {
        EnemyDataSO enemy = Track(CreateEnemy(10, false, 1f, 0));
        int experiencePerMercenary = 0;

        foreach (int enemyCount in new[] { 2, 3, 4 })
        {
            List<EnemyDataSO> encounter = new List<EnemyDataSO>();
            for (int i = 0; i < enemyCount; i++)
            {
                encounter.Add(enemy);
            }

            experiencePerMercenary +=
                BattleRewardService.CalculateExperiencePerMercenary(
                    BattleRewardService.CalculateExperienceReward(encounter),
                    4);
        }

        Assert.That(experiencePerMercenary, Is.EqualTo(112));
        Assert.That(
            experiencePerMercenary,
            Is.GreaterThanOrEqualTo(
                MercenaryInstance.CalculateExperienceToNextLevel(1)));
    }

    [Test]
    public void LowGradeEarlyRoad_GrantsVisibleProgressToFourMercenaries()
    {
        EnemyDataSO enemy = Track(CreateEnemy(10, false, 1f, 0));
        int totalExperience = BattleRewardService.CalculateExperienceReward(
            new List<EnemyDataSO> { enemy, enemy });

        int result = BattleRewardService.CalculateExperiencePerMercenary(
            totalExperience,
            4);

        Assert.That(result, Is.EqualTo(25));
    }

    [TestCase(100, 3, 33)]
    [TestCase(1, 4, 1)]
    [TestCase(100, 0, 0)]
    public void CalculateExperiencePerMercenary_UsesExistingMinimumAndDivisionRules(
        int totalExperience,
        int mercenaryCount,
        int expected)
    {
        int result = BattleRewardService.CalculateExperiencePerMercenary(
            totalExperience,
            mercenaryCount);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GrantVictoryRewards_WhenStorageIsFull_LogsCapacityMessageAndDoesNotAddDrop()
    {
        GameObject root = Track(new GameObject("Full Storage Reward Test"));
        MerchantData merchantData = root.AddComponent<MerchantData>();
        MerchantInventory inventory = root.AddComponent<MerchantInventory>();
        root.AddComponent<ProgressionManager>();
        ItemDataSO filler = Track(CreateItem("Filler"));
        ItemDataSO reward = Track(CreateItem("Guaranteed Reward"));
        Assert.That(inventory.TryAddItem(filler, 30), Is.True);

        EnemyDataSO enemy = Track(CreateEnemy(10, false, 1f, 0));
        enemy.itemDrops = new[]
        {
            new ItemDropEntry
            {
                item = reward,
                amount = 1,
                dropChance = 1f
            }
        };
        List<(string Message, BattleLogType Type)> messages =
            new List<(string Message, BattleLogType Type)>();
        BattleRewardService service = new BattleRewardService(
            merchantData,
            inventory,
            (message, type) => messages.Add((message, type)),
            () => 0f);

        service.GrantVictoryRewards(
            new List<EnemyDataSO> { enemy },
            new List<MercenaryInstance>());

        Assert.That(inventory.GetItemAmount(reward), Is.Zero);
        Assert.That(inventory.GetUsedStorageSlots(), Is.EqualTo(30));
        Assert.That(
            messages.Exists(entry =>
                entry.Type == BattleLogType.System &&
                entry.Message.Contains("倉庫が満杯")),
            Is.True);
        Assert.That(
            messages.Exists(entry =>
                entry.Type == BattleLogType.Reward &&
                entry.Message.Contains("Guaranteed Reward")),
            Is.False);
    }

    private static EnemyDataSO CreateEnemy(
        int grade,
        bool isBoss,
        float experienceMultiplier,
        int goldReward)
    {
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.monsterGrade = grade;
        enemy.isBoss = isBoss;
        enemy.experienceMultiplier = experienceMultiplier;
        enemy.goldReward = goldReward;
        return enemy;
    }

    private static ItemDataSO CreateItem(string itemName)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = itemName;
        item.itemName = itemName;
        item.itemType = ItemType.Material;
        return item;
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
