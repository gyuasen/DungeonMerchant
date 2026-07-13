using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class DungeonProgressStoreTests
{
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(DungeonProgressStore.UnlockedGradeSaveKey);
    }

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

        PlayerPrefs.DeleteKey(DungeonProgressStore.UnlockedGradeSaveKey);
    }

    [Test]
    public void Load_ClampsSavedHighestUnlockedGrade()
    {
        PlayerPrefs.SetInt(
            DungeonProgressStore.UnlockedGradeSaveKey,
            (int)DungeonGrade.Highest + 1);
        DungeonProgressStore store = new DungeonProgressStore();

        store.Load();

        Assert.That(store.HighestUnlockedGrade, Is.EqualTo(DungeonGrade.Highest));
    }

    [Test]
    public void RestoreProgress_WithLegacyAssetName_UsesPersistentIdWhenSaving()
    {
        DungeonDataSO dungeon = CreateDungeon(
            "Legacy Dungeon",
            "stable-dungeon-id",
            totalFloors: 5);
        List<DungeonDataSO> availableDungeons = new List<DungeonDataSO>
        {
            dungeon
        };
        DungeonProgressStore store = new DungeonProgressStore();

        store.RestoreProgress(
            DungeonGrade.Middle,
            new List<SavedDungeonFloorProgress>
            {
                new SavedDungeonFloorProgress
                {
                    dungeonAssetName = dungeon.name,
                    clearedFloors = 3
                }
            },
            availableDungeons);

        List<SavedDungeonFloorProgress> saved =
            store.CreateFloorProgressSaveData(availableDungeons);

        Assert.That(store.GetClearedFloors(dungeon), Is.EqualTo(3));
        Assert.That(store.HighestUnlockedGrade, Is.EqualTo(DungeonGrade.Middle));
        Assert.That(
            PlayerPrefs.GetInt(DungeonProgressStore.UnlockedGradeSaveKey),
            Is.EqualTo((int)DungeonGrade.Middle));
        Assert.That(saved, Has.Count.EqualTo(1));
        Assert.That(saved[0].dungeonPersistentId, Is.EqualTo(dungeon.PersistentId));
        Assert.That(saved[0].dungeonAssetName, Is.EqualTo(dungeon.name));
        Assert.That(saved[0].clearedFloors, Is.EqualTo(3));
    }

    [Test]
    public void Reset_ClearsProgressAndDeletesSavedGrade()
    {
        DungeonDataSO dungeon = CreateDungeon(
            "Reset Dungeon",
            "reset-dungeon-id");
        DungeonProgressStore store = new DungeonProgressStore();
        store.RestoreProgress(
            DungeonGrade.Upper,
            new List<SavedDungeonFloorProgress>
            {
                new SavedDungeonFloorProgress
                {
                    dungeonPersistentId = dungeon.PersistentId,
                    dungeonAssetName = dungeon.name,
                    clearedFloors = 2
                }
            },
            new List<DungeonDataSO> { dungeon });

        store.Reset();

        Assert.That(store.HighestUnlockedGrade, Is.EqualTo(DungeonGrade.Low));
        Assert.That(store.GetClearedFloors(dungeon), Is.EqualTo(0));
        Assert.That(
            PlayerPrefs.HasKey(DungeonProgressStore.UnlockedGradeSaveKey),
            Is.False);
    }

    private DungeonDataSO CreateDungeon(
        string assetName,
        string persistentId,
        int totalFloors = 3)
    {
        DungeonDataSO dungeon = Track(
            ScriptableObject.CreateInstance<DungeonDataSO>());
        dungeon.name = assetName;
        dungeon.totalFloors = totalFloors;
        SerializedObject serializedDungeon = new SerializedObject(dungeon);
        serializedDungeon.FindProperty("persistentId").stringValue = persistentId;
        serializedDungeon.ApplyModifiedPropertiesWithoutUndo();
        return dungeon;
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
