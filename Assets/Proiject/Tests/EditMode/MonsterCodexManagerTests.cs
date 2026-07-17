using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class MonsterCodexManagerTests
{
    [Test]
    public void RecordEncounter_AddsIdOnlyOnce()
    {
        MonsterCodexManager manager = CreateManager();
        EnemyDataSO enemy = CreateEnemy("slime-id");

        manager.RecordEncounter(enemy);
        manager.RecordEncounter(enemy);

        Assert.That(manager.EncounteredEnemyIds, Is.EquivalentTo(new[] { "slime-id" }));
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(enemy);
    }

    [Test]
    public void RecordEncounter_RuntimeFallbackIsExcluded()
    {
        MonsterCodexManager manager = CreateManager();
        EnemyDataSO fallback = CreateEnemy("fallback-id");
        fallback.hideFlags = HideFlags.DontSave;

        manager.RecordEncounter(fallback);

        Assert.That(manager.EncounteredEnemyIds, Is.Empty);
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(fallback);
    }

    [Test]
    public void RecordEncounter_SpecialVariantUsesOriginalPersistentId()
    {
        MonsterCodexManager manager = CreateManager();
        EnemyDataSO special = CreateEnemy("special-copy");
        special.isSpecialVariant = true;
        special.runtimeSourcePersistentId = "original-id";

        manager.RecordEncounter(special);

        Assert.That(manager.EncounteredEnemyIds, Is.EquivalentTo(new[] { "original-id" }));
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(special);
    }

    [Test]
    public void RestoreEncounteredEnemies_RoundTripsSaveList()
    {
        MonsterCodexManager manager = CreateManager();
        manager.RestoreEncounteredEnemies(new List<string> { "a", "a", "b" });
        GameSaveData data = new GameSaveData();
        data.encounteredEnemyIds.AddRange(manager.EncounteredEnemyIds);
        manager.RestoreEncounteredEnemies(data.encounteredEnemyIds);

        Assert.That(manager.EncounteredEnemyIds, Is.EquivalentTo(new[] { "a", "b" }));
        Object.DestroyImmediate(manager.gameObject);
    }

    [Test]
    public void Migrate_OldSaveStartsWithEmptyMonsterCodex()
    {
        GameSaveData data = new GameSaveData { version = 24, encounteredEnemyIds = null };

        SaveDataMigrator.Migrate(data);

        Assert.That(data.encounteredEnemyIds, Is.Not.Null);
        Assert.That(data.encounteredEnemyIds, Is.Empty);
    }

    private static MonsterCodexManager CreateManager()
    {
        return new GameObject("Monster Codex Test").AddComponent<MonsterCodexManager>();
    }

    private static EnemyDataSO CreateEnemy(string id)
    {
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.name = id;
        return enemy;
    }
}
