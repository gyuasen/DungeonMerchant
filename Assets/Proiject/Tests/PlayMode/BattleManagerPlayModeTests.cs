using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class BattleManagerPlayModeTests
{
    private const float CompletionTimeoutSeconds = 2f;
    private string isolatedSavePath;

    [UnitySetUp]
    public IEnumerator IsolateRuntimeSaveData()
    {
        yield return null;
        isolatedSavePath = Path.Combine(
            Application.temporaryCachePath,
            $"dungeon-merchant-playmode-{System.Guid.NewGuid():N}.json");
        foreach (SaveManager manager in Object.FindObjectsOfType<SaveManager>())
        {
            SetPrivateField(manager, "savePathOverride", isolatedSavePath);
        }
    }

    [UnityTearDown]
    public IEnumerator RemoveIsolatedSaveData()
    {
        yield return null;
        if (!string.IsNullOrEmpty(isolatedSavePath) &&
            File.Exists(isolatedSavePath))
        {
            File.Delete(isolatedSavePath);
        }
    }

    [UnityTest]
    public IEnumerator StartBattle_OneHpEnemy_CompletesWithVictory()
    {
        GameObject battleManagerObject = null;
        EnemyDataSO enemy = null;
        BattleManager battleManager = null;
        System.Action<bool> completedHandler = null;

        try
        {
            battleManagerObject = new GameObject("BattleManager PlayMode Test");
            battleManager = battleManagerObject.AddComponent<BattleManager>();
            SetPrivateField(battleManager, "actionDelay", 0f);

            enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
            enemy.enemyName = "Test Enemy";
            enemy.maxHP = 1;
            enemy.attack = 0;
            enemy.defense = 0;
            enemy.attackSpeed = 1f;
            enemy.evasionRate = 0f;

            MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
                "play-mode-test-mercenary",
                null,
                null,
                "Test Mercenary",
                MercenaryClass.Warrior,
                MercenaryContractType.Exclusive,
                1,
                0,
                100,
                100,
                999,
                0,
                0,
                10f,
                0);

            bool completed = false;
            bool victory = false;
            BattlePresentationRoster preparedRoster = null;
            List<BattlePresentationEvent> presentationEvents =
                new List<BattlePresentationEvent>();
            battleManager.BattleVisualsPrepared +=
                roster => preparedRoster = roster;
            battleManager.BattlePresentation +=
                presentationEvents.Add;
            battleManager.SetNextBattleBackground(null, "TestArena");
            completedHandler = result =>
            {
                completed = true;
                victory = result;
            };
            battleManager.BattleCompleted += completedHandler;

            Assert.That(
                battleManager.StartBattle(
                    new List<MercenaryInstance> { mercenary },
                    new List<EnemyDataSO> { enemy }),
                Is.True);

            float timeoutAt = Time.realtimeSinceStartup + CompletionTimeoutSeconds;
            while (!completed && Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            Assert.That(completed, Is.True, "BattleCompleted was not raised in time.");
            Assert.That(victory, Is.True);
            Assert.That(preparedRoster, Is.Not.Null);
            Assert.That(preparedRoster.Players.Count, Is.EqualTo(1));
            Assert.That(preparedRoster.Enemies.Count, Is.EqualTo(1));
            Assert.That(preparedRoster.BackgroundKey, Is.EqualTo("TestArena"));
            Assert.That(
                presentationEvents.Exists(
                    item => item.Type == BattlePresentationEventType.Action),
                Is.True);
            Assert.That(
                presentationEvents.Exists(
                    item => item.Type == BattlePresentationEventType.Damage),
                Is.True);
            Assert.That(
                presentationEvents.Exists(
                    item => item.Type == BattlePresentationEventType.Defeated),
                Is.True);
            Assert.That(
                presentationEvents.Exists(
                    item => item.Type ==
                            BattlePresentationEventType.BattleCompleted &&
                            item.Victory),
                Is.True);
        }
        finally
        {
            if (battleManager != null && completedHandler != null)
            {
                battleManager.BattleCompleted -= completedHandler;
            }

            if (battleManagerObject != null)
            {
                Object.DestroyImmediate(battleManagerObject);
            }

            if (enemy != null)
            {
                Object.DestroyImmediate(enemy);
            }
        }
    }

    [UnityTest]
    public IEnumerator RequestSkipToBattleEnd_RemovesActionDelayAndCompletesOnce()
    {
        GameObject managerObject = new GameObject("Battle Skip Test");
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        try
        {
            BattleManager manager = managerObject.AddComponent<BattleManager>();
            SetPrivateField(manager, "actionDelay", 10f);
            enemy.enemyName = "Slow Enemy";
            enemy.maxHP = 1;
            enemy.attackSpeed = 1f;

            MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
                "skip-test", null, null, "Tester", MercenaryClass.Warrior,
                MercenaryContractType.Exclusive, 1, 0, 100, 100, 999, 0,
                0, 1f, 0);
            int completionCount = 0;
            bool victory = false;
            manager.BattleCompleted += result =>
            {
                completionCount++;
                victory = result;
            };

            Assert.That(manager.StartBattle(
                new List<MercenaryInstance> { mercenary },
                new List<EnemyDataSO> { enemy }), Is.True);
            Assert.That(manager.RequestSkipToBattleEnd(), Is.True);

            float timeoutAt = Time.realtimeSinceStartup + CompletionTimeoutSeconds;
            while (completionCount == 0 && Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(victory, Is.True);
            Assert.That(manager.IsBattling, Is.False);
            Assert.That(manager.IsSkippingToBattleEnd, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(enemy);
        }
    }

    [Test]
    public void ToggleBattlePause_OutsideBattle_DoesNotChangeState()
    {
        GameObject managerObject = new GameObject("Battle Pause Outside Test");
        try
        {
            BattleManager manager = managerObject.AddComponent<BattleManager>();

            Assert.That(manager.IsPaused, Is.False);
            Assert.That(manager.ToggleBattlePause(), Is.False);
            Assert.That(manager.IsPaused, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(managerObject);
        }
    }

    [UnityTest]
    public IEnumerator ToggleBattlePause_WhileWaiting_PreventsActionUntilResumed()
    {
        GameObject managerObject = new GameObject("Battle Pause Test");
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        try
        {
            BattleManager manager = managerObject.AddComponent<BattleManager>();
            SetPrivateField(manager, "actionDelay", 0.25f);
            enemy.enemyName = "Pause Enemy";
            enemy.maxHP = 1000;
            enemy.attack = 0;
            enemy.attackSpeed = 1f;

            MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
                "pause-test", null, null, "Tester", MercenaryClass.Warrior,
                MercenaryContractType.Exclusive, 1, 0, 100, 100, 1, 0,
                0, 1f, 0);
            int actionCount = 0;
            manager.BattlePresentation += presentationEvent =>
            {
                if (presentationEvent.Type == BattlePresentationEventType.Action)
                {
                    actionCount++;
                }
            };

            Assert.That(manager.StartBattle(
                new List<MercenaryInstance> { mercenary },
                new List<EnemyDataSO> { enemy }), Is.True);
            Assert.That(manager.ToggleBattlePause(), Is.True);

            float timeoutAt = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            Assert.That(manager.IsPaused, Is.True);
            Assert.That(actionCount, Is.EqualTo(0));
            Assert.That(manager.ToggleBattlePause(), Is.False);

            timeoutAt = Time.realtimeSinceStartup + CompletionTimeoutSeconds;
            while (actionCount == 0 && Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            Assert.That(actionCount, Is.GreaterThan(0));
        }
        finally
        {
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(enemy);
        }
    }

    [UnityTest]
    public IEnumerator RequestSkipToBattleEnd_WhilePaused_ResumesAndCompletes()
    {
        GameObject managerObject = new GameObject("Battle Skip Paused Test");
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        try
        {
            BattleManager manager = managerObject.AddComponent<BattleManager>();
            SetPrivateField(manager, "actionDelay", 10f);
            enemy.enemyName = "Paused Skip Enemy";
            enemy.maxHP = 1;
            enemy.attack = 0;
            enemy.attackSpeed = 1f;

            MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
                "paused-skip-test", null, null, "Tester", MercenaryClass.Warrior,
                MercenaryContractType.Exclusive, 1, 0, 100, 100, 999, 0,
                0, 1f, 0);
            int completionCount = 0;
            manager.BattleCompleted += _ => completionCount++;

            Assert.That(manager.StartBattle(
                new List<MercenaryInstance> { mercenary },
                new List<EnemyDataSO> { enemy }), Is.True);
            Assert.That(manager.ToggleBattlePause(), Is.True);
            Assert.That(manager.RequestSkipToBattleEnd(), Is.True);
            Assert.That(manager.IsPaused, Is.False);

            float timeoutAt = Time.realtimeSinceStartup + CompletionTimeoutSeconds;
            while (completionCount == 0 && Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(manager.IsBattling, Is.False);
            Assert.That(manager.IsPaused, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(enemy);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"Missing private field: {fieldName}");
        field.SetValue(target, value);
    }
}
