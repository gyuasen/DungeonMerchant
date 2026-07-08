using System.Collections.Generic;
using UnityEngine;

public sealed class RoadEncounterService : MonoBehaviour
{
    private const float RareEncounterChance = 0.08f;
    private const int EarlyRouteEnemyCount = 2;
    private const int MiddleRouteEnemyCount = 3;
    private const int LateRouteEnemyCount = 5;
    private DungeonRunManager dungeonRunManager;
    private BattleManager battleManager;

    public void Initialize(
        DungeonRunManager targetDungeonRunManager,
        BattleManager targetBattleManager)
    {
        dungeonRunManager = targetDungeonRunManager;
        battleManager = targetBattleManager;
    }

    public List<EnemyDataSO> CreateEncounter(
        int originTownIndex,
        int destinationTownIndex,
        out bool containsRareEnemy)
    {
        containsRareEnemy = false;
        int routeIndex = Mathf.Min(originTownIndex, destinationTownIndex);
        int enemyCount = GetEnemyCountForRoute(routeIndex);

        List<EnemyDataSO> candidates =
            GetEnemiesNearTown(originTownIndex);
        AddUnique(candidates, GetEnemiesNearTown(destinationTownIndex));

        List<EnemyDataSO> encounter = new List<EnemyDataSO>(enemyCount);
        while (encounter.Count < enemyCount && candidates.Count > 0)
        {
            EnemyDataSO selected =
                candidates[Random.Range(0, candidates.Count)];
            encounter.Add(selected);
        }

        FillFallbackEnemies(encounter, enemyCount);

        TryReplaceWithRareEnemy(encounter, routeIndex, out containsRareEnemy);
        TrimToLimit(encounter, enemyCount);
        return encounter;
    }

    private static int GetEnemyCountForRoute(int routeIndex)
    {
        if (routeIndex <= 0)
        {
            return EarlyRouteEnemyCount;
        }

        return routeIndex <= 2
            ? MiddleRouteEnemyCount
            : LateRouteEnemyCount;
    }

    private void FillFallbackEnemies(
        List<EnemyDataSO> encounter,
        int enemyCount)
    {
        if (encounter.Count >= enemyCount)
        {
            return;
        }

        List<EnemyDataSO> fallbackEnemies =
            battleManager != null
                ? battleManager.CreateDefaultEnemyEncounter(enemyCount)
                : new List<EnemyDataSO>();
        for (int i = 0;
             i < fallbackEnemies.Count && encounter.Count < enemyCount;
             i++)
        {
            if (fallbackEnemies[i] != null)
            {
                encounter.Add(fallbackEnemies[i]);
            }
        }
    }

    private List<EnemyDataSO> GetEnemiesNearTown(int townIndex)
    {
        List<EnemyDataSO> result = new List<EnemyDataSO>();
        DungeonDataSO dungeon =
            dungeonRunManager?.GetHighestGradeDungeonNearTown(townIndex);
        if (dungeon?.normalEnemies == null)
        {
            return result;
        }

        foreach (EnemyDataSO enemy in dungeon.normalEnemies)
        {
            if (enemy != null && !enemy.isBoss && !result.Contains(enemy))
            {
                result.Add(enemy);
            }
        }
        return result;
    }

    private static void AddUnique(
        List<EnemyDataSO> destination,
        List<EnemyDataSO> source)
    {
        foreach (EnemyDataSO enemy in source)
        {
            if (enemy != null && !destination.Contains(enemy))
            {
                destination.Add(enemy);
            }
        }
    }

    private static void TryReplaceWithRareEnemy(
        List<EnemyDataSO> encounter,
        int routeIndex,
        out bool replaced)
    {
        replaced = false;
        int targetGrade = routeIndex == 1 ? 7 : routeIndex == 0 ? 5 : -1;
        if (targetGrade < 0 ||
            encounter.Count == 0 ||
            Random.value >= RareEncounterChance)
        {
            return;
        }

        foreach (EnemyDataSO enemy in
                 Resources.LoadAll<EnemyDataSO>("Enemies/RoadRare"))
        {
            if (enemy != null &&
                enemy.category == EnemyCategory.MythicalBeast &&
                enemy.monsterGrade == targetGrade)
            {
                encounter[encounter.Count - 1] = enemy;
                replaced = true;
                return;
            }
        }
    }

    private static void TrimToLimit(
        List<EnemyDataSO> encounter,
        int enemyCount)
    {
        if (encounter.Count <= enemyCount)
        {
            return;
        }

        encounter.RemoveRange(
            enemyCount,
            encounter.Count - enemyCount);
    }
}
