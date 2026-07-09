using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Characterization tests for BattleManager (Action 2.1 of the god-object
// refactor plan). BattleManager.StartBattle(...) hands off the actual turn
// resolution to a coroutine (BattleRoutine) via StartCoroutine, which is not
// pumped in EditMode. These tests therefore only cover the synchronous,
// pre-coroutine surface: the guard clauses in the StartBattle overloads, and
// the fully synchronous helper methods CreateDefaultEnemyEncounter(...) and
// GetEncounterDescription(). See the end-of-run report for a summary of what
// remains out of scope and why.
public sealed class BattleManagerTests
{
    private GameObject root;
    private BattleManager battleManager;
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Battle Manager Test");
        battleManager = root.AddComponent<BattleManager>();
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
    }

    // --- StartBattle guard clauses (synchronous, before StartCoroutine) ---

    [Test]
    public void StartBattle_NoArgOverload_WithNoPartyManagerOrMembers_ReturnsFalse()
    {
        // No MercenaryPartyManager has been added anywhere in this test's
        // object graph. Even if ResolveReferences() finds a stray one via
        // FindObjectOfType (e.g. left over from another test), a freshly
        // constructed MercenaryPartyManager always starts with zero members,
        // so this assertion holds either way: the empty-party guard fires.
        bool result = battleManager.StartBattle();

        Assert.That(result, Is.False);
        Assert.That(battleManager.IsBattling, Is.False);
    }

    [Test]
    public void StartBattle_OneArgOverload_WithNullPartyMembers_ReturnsFalse()
    {
        bool result = battleManager.StartBattle((IReadOnlyList<MercenaryInstance>)null);

        Assert.That(result, Is.False);
        Assert.That(battleManager.IsBattling, Is.False);
    }

    [Test]
    public void StartBattle_OneArgOverload_WithEmptyPartyMembers_ReturnsFalse()
    {
        bool result = battleManager.StartBattle(new List<MercenaryInstance>());

        Assert.That(result, Is.False);
        Assert.That(battleManager.IsBattling, Is.False);
    }

    [Test]
    public void StartBattle_TwoArgOverload_WithEmptyParty_ReturnsFalse_EvenWithEnemyEncounterSupplied()
    {
        // The empty-party guard runs before enemy resolution, so supplying a
        // valid enemy encounter must not change the outcome.
        EnemyDataSO enemy = TrackAsset(CreateEnemy("Slime", 5, 100, 12, 4, 30, 1.2f, 75));

        bool result = battleManager.StartBattle(
            new List<MercenaryInstance>(),
            new List<EnemyDataSO> { enemy });

        Assert.That(result, Is.False);
        Assert.That(battleManager.IsBattling, Is.False);
    }

    // --- CreateDefaultEnemyEncounter(int) ---

    [TestCase(1, 1)]
    [TestCase(3, 3)]
    [TestCase(5, 5)]
    [TestCase(0, 1)]
    [TestCase(-2, 1)]
    public void CreateDefaultEnemyEncounter_WithInjectedEnemyData_ReturnsMaxOneOrRequestedCopiesOfSameEnemy(
        int requestedCount,
        int expectedCount)
    {
        EnemyDataSO enemy = TrackAsset(CreateEnemy("Slime", 5, 100, 12, 4, 30, 1.2f, 75));
        SetPrivateField(battleManager, "enemyData", enemy);

        List<EnemyDataSO> result = battleManager.CreateDefaultEnemyEncounter(requestedCount);

        Assert.That(result.Count, Is.EqualTo(expectedCount));
        foreach (EnemyDataSO entry in result)
        {
            Assert.That(entry, Is.SameAs(enemy));
        }
    }

    [TestCase(1)]
    [TestCase(4)]
    public void CreateDefaultEnemyEncounter_WithNoEnemyDataConfigured_StillReturnsRequestedCount(
        int requestedCount)
    {
        // Verified by reading FindEnemyData()/GetFallbackSlimeData(): when no
        // enemyData is set on the component and no matching asset is found
        // via Resources.LoadAll, GetFallbackSlimeData() lazily creates a
        // runtime "Runtime Slime" EnemyDataSO and caches it, so FindEnemyData()
        // can never return null. (In this project Resources.LoadAll would
        // actually find real assets under Assets/Proiject/Resources/GameData/
        // Enemies/*.asset anyway.) As a direct consequence, the
        // "if (enemy == null) return enemies;" empty-list branch inside
        // CreateDefaultEnemyEncounter is unreachable through the public API on
        // a default-constructed BattleManager: it always returns a non-empty
        // list of exactly Mathf.Max(1, requestedCount) entries. This test pins
        // that guaranteed-fallback behavior instead of an unreachable
        // "returns empty" case.
        List<EnemyDataSO> result = battleManager.CreateDefaultEnemyEncounter(requestedCount);

        Assert.That(result.Count, Is.EqualTo(requestedCount));
        Assert.That(result, Has.None.Null);
    }

    // --- GetEncounterDescription() ---

    [Test]
    public void GetEncounterDescription_WithSingleConfiguredEnemy_ReturnsSingleEnemyFormat()
    {
        EnemyDataSO enemy = TrackAsset(CreateEnemy("Slime", 5, 100, 12, 4, 30, 1.2f, 75));
        GetEnemyPartyDataList(battleManager).Add(enemy);

        string description = battleManager.GetEncounterDescription();

        Assert.That(
            description,
            Is.EqualTo(
                "スライム  |  5等級  |  HP 100  攻撃 12  防御 4  魔力 30  速度 1.20  |  報酬 75 G"));
    }

    [Test]
    public void GetEncounterDescription_WithMultipleConfiguredEnemies_ReturnsAggregateFormat()
    {
        EnemyDataSO first = TrackAsset(CreateEnemy("Slime", 5, 100, 12, 4, 30, 1.2f, 75));
        EnemyDataSO second = TrackAsset(CreateEnemy("Goblin", 5, 90, 10, 3, 20, 1.0f, 50));
        List<EnemyDataSO> enemyPartyData = GetEnemyPartyDataList(battleManager);
        enemyPartyData.Add(first);
        enemyPartyData.Add(second);

        string description = battleManager.GetEncounterDescription();

        Assert.That(description, Is.EqualTo("スライム x2  |  合計報酬 125 G"));
    }

    [Test]
    public void GetEncounterDescription_WithNoEnemyDataConfigured_NeverReturnsUnconfiguredMessage()
    {
        // Same guaranteed-fallback reasoning as
        // CreateDefaultEnemyEncounter_WithNoEnemyDataConfigured_StillReturnsRequestedCount:
        // GetEncounterDescription() calls ResolveReferences() before
        // BuildEnemyEncounterData(), so private field `enemyData` is always
        // non-null by the time the enemy count is checked. The
        // "敵が設定されていません" branch is therefore unreachable through the
        // public API on a default-constructed BattleManager in this project.
        string description = battleManager.GetEncounterDescription();

        Assert.That(description, Is.Not.EqualTo("敵が設定されていません"));
        Assert.That(description, Does.Contain(" G"));
    }

    private static EnemyDataSO CreateEnemy(
        string enemyName,
        int monsterGrade,
        int maxHP,
        int attack,
        int defense,
        int maxMagicPower,
        float attackSpeed,
        int goldReward)
    {
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.enemyName = enemyName;
        enemy.monsterGrade = monsterGrade;
        enemy.maxHP = maxHP;
        enemy.attack = attack;
        enemy.defense = defense;
        enemy.maxMagicPower = maxMagicPower;
        enemy.attackSpeed = attackSpeed;
        enemy.goldReward = goldReward;
        return enemy;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(target, value);
    }

    private static List<EnemyDataSO> GetEnemyPartyDataList(BattleManager manager)
    {
        FieldInfo field = typeof(BattleManager).GetField(
            "enemyPartyData",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return (List<EnemyDataSO>)field.GetValue(manager);
    }

    private T TrackAsset<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
