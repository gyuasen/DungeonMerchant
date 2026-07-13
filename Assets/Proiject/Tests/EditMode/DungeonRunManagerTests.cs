using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Characterization tests for DungeonRunManager (Action 2.2 of the god-object
// refactor plan). StartRun() eventually calls StartNextEncounter(), which
// hands off to BattleManager.StartBattle(...) and its coroutine-based turn
// resolution (not pumped in EditMode) -- the same boundary already
// established by BattleManagerTests.cs. These tests therefore only cover the
// synchronous, side-effect-bounded surface: IsDungeonUnlocked(...),
// TrySelectDungeon(...), StartRun()'s early guard clauses (asserting only the
// `false` returns, never driving a run to a successful start),
// ChooseEventOption(...)'s guard clause, and public progress-store delegation.
// Driving a
// run to completion, event-choice resolution, HP/reward application, and
// DungeonCompleted/DungeonMessage firing from a real run are out of scope for
// the same reason BattleManagerTests.cs stops at StartBattle's guard clauses.
//
// IMPORTANT: DungeonRunManager.OnEnable() loads DungeonProgressStore, which
// reads PlayerPrefs key "DungeonMerchant.Dungeon.HighestUnlockedGrade" the
// instant AddComponent<DungeonRunManager>() runs. RestoreProgress(...) writes
// that same key through the store. Both SetUp and TearDown delete the key so
// this state never leaks across tests in this file, or into
// RoadEncounterServiceTests.cs / RoadTravelCompletionServiceTests.cs, which
// also construct DungeonRunManager instances.
public sealed class DungeonRunManagerTests
{
    private const string UnlockedGradeSaveKey =
        "DungeonMerchant.Dungeon.HighestUnlockedGrade";

    private readonly List<Object> createdObjects = new List<Object>();
    private GameObject root;
    private DungeonRunManager dungeonRunManager;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(UnlockedGradeSaveKey);

        root = new GameObject("Dungeon Run Manager Test");
        dungeonRunManager = root.AddComponent<DungeonRunManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);

        foreach (Object created in createdObjects)
        {
            if (created != null)
            {
                Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();

        PlayerPrefs.DeleteKey(UnlockedGradeSaveKey);
    }

    // --- IsDungeonUnlocked(DungeonDataSO) ---

    [Test]
    public void IsDungeonUnlocked_WorldMapIndexMismatch_ReturnsFalse()
    {
        // currentWorldMapIndex defaults to 0.
        DungeonDataSO dungeon = CreateDungeon(
            "Mismatched World", null, worldMapIndex: 1, nearbyTownIndex: 2);

        Assert.That(dungeonRunManager.IsDungeonUnlocked(dungeon), Is.False);
    }

    [Test]
    public void IsDungeonUnlocked_TownNotInDefaultUnlockedSet_ReturnsFalse()
    {
        // unlockedTownIndices defaults to { 2 } only.
        DungeonDataSO dungeon = CreateDungeon(
            "Locked Town", null, worldMapIndex: 0, nearbyTownIndex: 3);

        Assert.That(dungeonRunManager.IsDungeonUnlocked(dungeon), Is.False);
    }

    [Test]
    public void IsDungeonUnlocked_MatchingWorldMapAndDefaultUnlockedTown_ReturnsTrue()
    {
        DungeonDataSO dungeon = CreateDungeon(
            "Unlocked", null, worldMapIndex: 0, nearbyTownIndex: 2);

        Assert.That(dungeonRunManager.IsDungeonUnlocked(dungeon), Is.True);
    }

    // --- TrySelectDungeon(DungeonDataSO) ---

    [Test]
    public void TrySelectDungeon_NullData_ReturnsFalse()
    {
        Assert.That(dungeonRunManager.TrySelectDungeon(null), Is.False);
    }

    [Test]
    public void TrySelectDungeon_LockedDungeon_ReturnsFalseAndDoesNotChangeSelection()
    {
        // Reuses the "town not unlocked" scenario from
        // IsDungeonUnlocked_TownNotInDefaultUnlockedSet_ReturnsFalse.
        DungeonDataSO locked = CreateDungeon(
            "Locked", null, worldMapIndex: 0, nearbyTownIndex: 3);
        // Captured rather than assumed null: PopulateDungeonDataIfNeeded()
        // (called from OnEnable) may have already auto-selected a real
        // project dungeon asset via FindFirstUnlockedDungeon().
        DungeonDataSO selectionBefore = dungeonRunManager.SelectedDungeon;

        bool result = dungeonRunManager.TrySelectDungeon(locked);

        Assert.That(result, Is.False);
        Assert.That(dungeonRunManager.SelectedDungeon, Is.SameAs(selectionBefore));
    }

    [Test]
    public void TrySelectDungeon_UnlockedDungeon_ReturnsTrueUpdatesSelectionAndFiresStateChanged()
    {
        DungeonDataSO unlocked = CreateDungeon(
            "Unlocked", null, worldMapIndex: 0, nearbyTownIndex: 2);
        bool stateChanged = false;
        dungeonRunManager.DungeonStateChanged += () => stateChanged = true;

        bool result = dungeonRunManager.TrySelectDungeon(unlocked);

        Assert.That(result, Is.True);
        Assert.That(dungeonRunManager.SelectedDungeon, Is.SameAs(unlocked));
        Assert.That(stateChanged, Is.True);
    }

    // --- StartRun() guard clauses (synchronous, before StartNextEncounter's
    // call into BattleManager.StartBattle) ---

    [Test]
    public void StartRun_WithNoBattleManagerOrPartyManagerInScene_ReturnsFalseAndDoesNotThrow()
    {
        // Neither a BattleManager nor a MercenaryPartyManager exists in this
        // test's object graph. ResolveReferences() falls back to
        // FindObjectOfType for both; since every EditMode fixture that
        // constructs these components (this file, RoadEncounterServiceTests,
        // BattleManagerTests) destroys its GameObjects in TearDown, none
        // should be found, so the "battleManager == null || partyManager ==
        // null" guard fires before any party-size or battle logic runs.
        bool result = false;
        Assert.DoesNotThrow(() => result = dungeonRunManager.StartRun());

        Assert.That(result, Is.False);
        Assert.That(dungeonRunManager.IsRunning, Is.False);
    }

    [Test]
    public void StartRun_WithBattleManagerButEmptyParty_ReturnsFalseAndDoesNotThrow()
    {
        root.AddComponent<BattleManager>();
        MercenaryPartyManager partyManager = root.AddComponent<MercenaryPartyManager>();
        // A freshly constructed MercenaryPartyManager always starts with zero
        // members (per BattleManagerTests.cs's established assumption).
        Assert.That(partyManager.Members.Count, Is.EqualTo(0));

        bool result = false;
        Assert.DoesNotThrow(() => result = dungeonRunManager.StartRun());

        Assert.That(result, Is.False);
        Assert.That(dungeonRunManager.IsRunning, Is.False);
    }

    // --- ChooseEventOption(int) guard clause ---

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void ChooseEventOption_WhenNotRunning_ReturnsFalseForAnyIndex(int optionIndex)
    {
        // IsRunning is false on a freshly constructed instance, so the
        // "!IsRunning" guard short-circuits before the optionIndex range
        // check -- in-range (0, 1, 2) and out-of-range (-1, 3) indices all
        // return false here for the same reason.
        Assert.That(dungeonRunManager.IsRunning, Is.False);

        bool result = dungeonRunManager.ChooseEventOption(optionIndex);

        Assert.That(result, Is.False);
    }

    // --- Public progress-store delegation ---

    [Test]
    public void RestoreProgress_ThenCreateFloorProgressSaveData_RoundTripsClearedFloors()
    {
        DungeonDataSO dungeon = CreateDungeon(
            "RoundTripDungeon",
            null,
            worldMapIndex: 0,
            nearbyTownIndex: 2,
            totalFloors: 5);

        dungeonRunManager.RestoreProgress(
            DungeonGrade.Middle,
            dungeon.name,
            dungeon.PersistentId,
            new List<SavedDungeonFloorProgress>
            {
                new SavedDungeonFloorProgress
                {
                    dungeonPersistentId = dungeon.PersistentId,
                    dungeonAssetName = dungeon.name,
                    clearedFloors = 3
                }
            });

        List<SavedDungeonFloorProgress> saved =
            dungeonRunManager.CreateFloorProgressSaveData();
        SavedDungeonFloorProgress savedEntry = saved.Find(
            entry => entry.dungeonPersistentId == dungeon.PersistentId);

        Assert.That(dungeonRunManager.GetClearedFloors(dungeon), Is.EqualTo(3));
        Assert.That(savedEntry, Is.Not.Null);
        Assert.That(savedEntry.clearedFloors, Is.EqualTo(3));
        Assert.That(
            dungeonRunManager.HighestUnlockedGrade,
            Is.EqualTo(DungeonGrade.Middle));
    }

    private DungeonDataSO CreateDungeon(
        string assetName,
        string persistentId,
        int worldMapIndex,
        int nearbyTownIndex,
        int totalFloors = 3)
    {
        DungeonDataSO dungeon = Track(ScriptableObject.CreateInstance<DungeonDataSO>());
        dungeon.name = assetName;
        dungeon.worldMapIndex = worldMapIndex;
        dungeon.nearbyTownIndex = nearbyTownIndex;
        dungeon.totalFloors = totalFloors;
        if (!string.IsNullOrEmpty(persistentId))
        {
            SerializedObject serializedDungeon = new SerializedObject(dungeon);
            serializedDungeon.FindProperty("persistentId").stringValue =
                persistentId;
            serializedDungeon.ApplyModifiedPropertiesWithoutUndo();
        }
        return dungeon;
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
