using System.Collections.Generic;
using UnityEngine;

/// <summary>Stores the base enemy assets encountered at the start of battle.</summary>
public sealed class MonsterCodexManager : MonoBehaviour
{
    private readonly HashSet<string> encounteredEnemyIds = new HashSet<string>();
    private BattleManager battleManager;

    public IReadOnlyCollection<string> EncounteredEnemyIds => encounteredEnemyIds;

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.BattleVisualsPrepared -= HandleBattleVisualsPrepared;
        }
    }

    public void RecordEncounter(EnemyDataSO enemy)
    {
        if (enemy != null &&
            enemy.isSpecialVariant &&
            !string.IsNullOrWhiteSpace(enemy.runtimeSourcePersistentId))
        {
            encounteredEnemyIds.Add(enemy.runtimeSourcePersistentId);
            return;
        }

        EnemyDataSO source = ResolveSourceEnemy(enemy);
        if (source == null || IsRuntimeFallback(source))
        {
            return;
        }

        encounteredEnemyIds.Add(source.PersistentId);
    }

    public bool HasEncountered(EnemyDataSO enemy)
    {
        if (enemy != null &&
            enemy.isSpecialVariant &&
            !string.IsNullOrWhiteSpace(enemy.runtimeSourcePersistentId))
        {
            return encounteredEnemyIds.Contains(enemy.runtimeSourcePersistentId);
        }

        EnemyDataSO source = ResolveSourceEnemy(enemy);
        return source != null && encounteredEnemyIds.Contains(source.PersistentId);
    }

    public void RestoreEncounteredEnemies(IEnumerable<string> ids)
    {
        encounteredEnemyIds.Clear();
        if (ids == null)
        {
            return;
        }

        foreach (string id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                encounteredEnemyIds.Add(id);
            }
        }
    }

    private void Subscribe()
    {
        BattleManager found = GetComponent<BattleManager>() ?? FindObjectOfType<BattleManager>();
        if (found == battleManager)
        {
            return;
        }

        if (battleManager != null)
        {
            battleManager.BattleVisualsPrepared -= HandleBattleVisualsPrepared;
        }

        battleManager = found;
        if (battleManager != null)
        {
            battleManager.BattleVisualsPrepared += HandleBattleVisualsPrepared;
        }
    }

    private void HandleBattleVisualsPrepared(BattlePresentationRoster roster)
    {
        if (roster == null || roster.Enemies == null)
        {
            return;
        }

        foreach (BattleVisualUnitDescriptor enemy in roster.Enemies)
        {
            RecordEncounter(enemy.EnemyData);
        }
    }

    private static EnemyDataSO ResolveSourceEnemy(EnemyDataSO enemy)
    {
        if (enemy == null || !enemy.isSpecialVariant)
        {
            return enemy;
        }

        foreach (EnemyDataSO candidate in GameAssetRepository.LoadAll<EnemyDataSO>())
        {
            if (candidate != null &&
                !candidate.isSpecialVariant &&
                (candidate.PersistentId == enemy.runtimeSourcePersistentId ||
                 candidate.enemyName == enemy.enemyName))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool IsRuntimeFallback(EnemyDataSO enemy)
    {
        return (enemy.hideFlags & HideFlags.DontSave) != 0 &&
               !enemy.isSpecialVariant;
    }
}
