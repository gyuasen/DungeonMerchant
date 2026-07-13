using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores dungeon progression independently from the active dungeon run.
/// </summary>
public sealed class DungeonProgressStore
{
    public const string UnlockedGradeSaveKey =
        "DungeonMerchant.Dungeon.HighestUnlockedGrade";

    private readonly Dictionary<string, int> clearedFloorsByDungeon =
        new Dictionary<string, int>();

    public DungeonGrade HighestUnlockedGrade { get; private set; } =
        DungeonGrade.Low;

    public void Load()
    {
        int savedGrade = PlayerPrefs.GetInt(
            UnlockedGradeSaveKey,
            (int)DungeonGrade.Low);
        HighestUnlockedGrade = ClampGrade(savedGrade);
    }

    public void Save()
    {
        PlayerPrefs.SetInt(UnlockedGradeSaveKey, (int)HighestUnlockedGrade);
        PlayerPrefs.Save();
    }

    public void Reset()
    {
        HighestUnlockedGrade = DungeonGrade.Low;
        clearedFloorsByDungeon.Clear();
        PlayerPrefs.DeleteKey(UnlockedGradeSaveKey);
        PlayerPrefs.Save();
    }

    public int GetClearedFloors(DungeonDataSO dungeon)
    {
        if (dungeon == null ||
            !clearedFloorsByDungeon.TryGetValue(
                GetDungeonProgressKey(dungeon),
                out int clearedFloors))
        {
            return 0;
        }

        return Mathf.Clamp(clearedFloors, 0, Mathf.Max(1, dungeon.totalFloors));
    }

    public void RecordClearedFloor(DungeonDataSO dungeon, int completedFloor)
    {
        if (dungeon == null)
        {
            return;
        }

        clearedFloorsByDungeon[GetDungeonProgressKey(dungeon)] = Mathf.Max(
            GetClearedFloors(dungeon),
            Mathf.Max(0, completedFloor));
    }

    public List<SavedDungeonFloorProgress> CreateFloorProgressSaveData(
        IReadOnlyList<DungeonDataSO> availableDungeons)
    {
        List<SavedDungeonFloorProgress> result =
            new List<SavedDungeonFloorProgress>();
        foreach (KeyValuePair<string, int> pair in clearedFloorsByDungeon)
        {
            DungeonDataSO dungeon = ResolveDungeonByProgressKey(
                pair.Key,
                availableDungeons);
            result.Add(new SavedDungeonFloorProgress
            {
                dungeonPersistentId =
                    dungeon != null ? dungeon.PersistentId : pair.Key,
                dungeonAssetName = dungeon != null ? dungeon.name : pair.Key,
                clearedFloors = pair.Value
            });
        }

        return result;
    }

    public void RestoreProgress(
        DungeonGrade restoredHighestGrade,
        IReadOnlyList<SavedDungeonFloorProgress> savedFloorProgress,
        IReadOnlyList<DungeonDataSO> availableDungeons)
    {
        HighestUnlockedGrade = ClampGrade((int)restoredHighestGrade);
        Save();

        clearedFloorsByDungeon.Clear();
        if (savedFloorProgress == null)
        {
            return;
        }

        foreach (SavedDungeonFloorProgress progress in savedFloorProgress)
        {
            if (progress == null ||
                (string.IsNullOrWhiteSpace(progress.dungeonPersistentId) &&
                 string.IsNullOrWhiteSpace(progress.dungeonAssetName)))
            {
                continue;
            }

            DungeonDataSO restoredDungeon = ResolveDungeon(
                progress.dungeonPersistentId,
                progress.dungeonAssetName,
                availableDungeons);
            string progressKey = restoredDungeon != null
                ? GetDungeonProgressKey(restoredDungeon)
                : !string.IsNullOrWhiteSpace(progress.dungeonPersistentId)
                    ? progress.dungeonPersistentId
                    : progress.dungeonAssetName;
            clearedFloorsByDungeon[progressKey] = Mathf.Max(
                0,
                progress.clearedFloors);
        }
    }

    public DungeonDataSO ResolveDungeon(
        string persistentId,
        string legacyAssetName,
        IReadOnlyList<DungeonDataSO> availableDungeons)
    {
        if (availableDungeons != null)
        {
            foreach (DungeonDataSO dungeon in availableDungeons)
            {
                if (dungeon == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(persistentId) &&
                    dungeon.PersistentId == persistentId)
                {
                    return dungeon;
                }

                if (string.IsNullOrWhiteSpace(persistentId) &&
                    dungeon.name == legacyAssetName)
                {
                    return dungeon;
                }
            }
        }

        return GameAssetRepository.FindByPersistentId<DungeonDataSO>(
            persistentId,
            legacyAssetName);
    }

    private static DungeonGrade ClampGrade(int grade)
    {
        return (DungeonGrade)Mathf.Clamp(
            grade,
            (int)DungeonGrade.Low,
            (int)DungeonGrade.Highest);
    }

    private static string GetDungeonProgressKey(DungeonDataSO dungeon)
    {
        return dungeon != null ? dungeon.PersistentId : string.Empty;
    }

    private static DungeonDataSO ResolveDungeonByProgressKey(
        string progressKey,
        IReadOnlyList<DungeonDataSO> availableDungeons)
    {
        if (availableDungeons != null)
        {
            foreach (DungeonDataSO dungeon in availableDungeons)
            {
                if (dungeon != null &&
                    (dungeon.PersistentId == progressKey ||
                     dungeon.name == progressKey))
                {
                    return dungeon;
                }
            }
        }

        return GameAssetRepository.FindByPersistentId<DungeonDataSO>(
            progressKey,
            progressKey);
    }
}
