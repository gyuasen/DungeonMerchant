using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class RoadEncounterServiceTests
{
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (Object created in createdObjects)
        {
            if (created != null)
            {
                Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    [TestCase(0, 1, 5)]
    [TestCase(1, 2, 4)]
    [TestCase(5, 6, 4)]
    public void CreateEncounter_UsesConfiguredRouteLimit(
        int originTown,
        int destinationTown,
        int expectedCount)
    {
        GameObject root = Track(new GameObject("Road Encounter Test"));
        DungeonRunManager dungeonManager =
            root.AddComponent<DungeonRunManager>();
        RoadEncounterService service =
            root.AddComponent<RoadEncounterService>();

        List<DungeonDataSO> dungeons = GetDungeonList(dungeonManager);
        dungeons.Clear();
        dungeons.Add(CreateDungeon(originTown, "Origin"));
        dungeons.Add(CreateDungeon(destinationTown, "Destination"));
        service.Initialize(dungeonManager, null);

        List<EnemyDataSO> encounter = service.CreateEncounter(
            originTown,
            destinationTown,
            out _);

        Assert.That(encounter.Count, Is.EqualTo(expectedCount));
    }

    [TestCase(0, 1, 5)]
    [TestCase(1, 2, 4)]
    public void CreateEncounter_FallbackKeepsRouteLimit(
        int originTown,
        int destinationTown,
        int expectedCount)
    {
        GameObject root = Track(new GameObject("Road Encounter Fallback Test"));
        DungeonRunManager dungeonManager =
            root.AddComponent<DungeonRunManager>();
        BattleManager battleManager = root.AddComponent<BattleManager>();
        RoadEncounterService service =
            root.AddComponent<RoadEncounterService>();

        SetEnemyData(battleManager, CreateEnemy("Fallback"));
        service.Initialize(dungeonManager, battleManager);

        List<EnemyDataSO> encounter = service.CreateEncounter(
            originTown,
            destinationTown,
            out _);

        Assert.That(encounter.Count, Is.EqualTo(expectedCount));
    }

    private DungeonDataSO CreateDungeon(int townIndex, string prefix)
    {
        DungeonDataSO dungeon = Track(
            ScriptableObject.CreateInstance<DungeonDataSO>());
        dungeon.nearbyTownIndex = townIndex;
        dungeon.grade = DungeonGrade.Highest;
        dungeon.normalEnemies = new EnemyDataSO[8];
        for (int i = 0; i < dungeon.normalEnemies.Length; i++)
        {
            EnemyDataSO enemy = Track(
                ScriptableObject.CreateInstance<EnemyDataSO>());
            enemy.name = $"{prefix}_{i}";
            dungeon.normalEnemies[i] = enemy;
        }
        return dungeon;
    }

    private EnemyDataSO CreateEnemy(string name)
    {
        EnemyDataSO enemy = Track(
            ScriptableObject.CreateInstance<EnemyDataSO>());
        enemy.name = name;
        enemy.enemyName = name;
        return enemy;
    }

    private static List<DungeonDataSO> GetDungeonList(
        DungeonRunManager manager)
    {
        FieldInfo field = typeof(DungeonRunManager).GetField(
            "availableDungeons",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return (List<DungeonDataSO>)field.GetValue(manager);
    }

    private static void SetEnemyData(
        BattleManager manager,
        EnemyDataSO enemy)
    {
        FieldInfo field = typeof(BattleManager).GetField(
            "enemyData",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(manager, enemy);
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
