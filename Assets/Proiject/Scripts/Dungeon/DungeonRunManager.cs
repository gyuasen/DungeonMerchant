using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private MercenaryPartyManager partyManager;

    [Header("Run Settings")]
    [SerializeField, Min(1)] private int encounterCount = 3;
    [SerializeField, Min(1)] private int firstEncounterEnemyCount = 2;
    [SerializeField, Min(0)] private int enemyCountIncreasePerEncounter = 1;

    public bool IsRunning { get; private set; }
    public int CurrentEncounter { get; private set; }
    public int EncounterCount => encounterCount;

    public event Action<string> DungeonMessage;
    public event Action DungeonStateChanged;
    public event Action<bool> DungeonCompleted;

    private void OnDestroy()
    {
        if (battleManager != null)
        {
            battleManager.BattleCompleted -= HandleBattleCompleted;
        }
    }

    public bool StartRun()
    {
        ResolveReferences();

        if (IsRunning)
        {
            SendDungeonMessage("A dungeon run is already in progress.");
            return false;
        }

        if (battleManager == null || partyManager == null)
        {
            SendDungeonMessage("Dungeon run references are incomplete.");
            return false;
        }

        if (partyManager.Members.Count == 0)
        {
            SendDungeonMessage("Add mercenaries to the party before entering a dungeon.");
            return false;
        }

        IsRunning = true;
        CurrentEncounter = 0;
        SubscribeToBattle();
        SendDungeonMessage($"Dungeon run started. Encounters: {encounterCount}");
        DungeonStateChanged?.Invoke();
        return StartNextEncounter();
    }

    public void AbandonRun()
    {
        if (!IsRunning || (battleManager != null && battleManager.IsBattling))
        {
            return;
        }

        CompleteRun(false, "Dungeon run abandoned.");
    }

    private bool StartNextEncounter()
    {
        if (!IsRunning)
        {
            return false;
        }

        CurrentEncounter++;
        if (CurrentEncounter > encounterCount)
        {
            CompleteRun(true, "Dungeon cleared.");
            return true;
        }

        int enemyCount = firstEncounterEnemyCount +
            ((CurrentEncounter - 1) * enemyCountIncreasePerEncounter);
        List<EnemyDataSO> enemies =
            battleManager.CreateDefaultEnemyEncounter(enemyCount);

        SendDungeonMessage(
            $"Encounter {CurrentEncounter}/{encounterCount}: " +
            $"{enemyCount} enemies appeared.");
        DungeonStateChanged?.Invoke();

        bool started = battleManager.StartBattle(partyManager.Members, enemies);
        if (!started)
        {
            CompleteRun(false, "Dungeon run stopped: battle could not start.");
        }

        return started;
    }

    private void HandleBattleCompleted(bool victory)
    {
        if (!IsRunning)
        {
            return;
        }

        if (!victory)
        {
            CompleteRun(false, "Dungeon run failed.");
            return;
        }

        SendDungeonMessage(
            $"Encounter {CurrentEncounter}/{encounterCount} cleared.");
        StartNextEncounter();
    }

    private void CompleteRun(bool cleared, string message)
    {
        IsRunning = false;
        SendDungeonMessage(message);
        DungeonStateChanged?.Invoke();
        DungeonCompleted?.Invoke(cleared);
    }

    private void SubscribeToBattle()
    {
        battleManager.BattleCompleted -= HandleBattleCompleted;
        battleManager.BattleCompleted += HandleBattleCompleted;
    }

    private void ResolveReferences()
    {
        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>();
        }

        if (partyManager == null)
        {
            partyManager = FindObjectOfType<MercenaryPartyManager>();
        }
    }

    private void SendDungeonMessage(string message)
    {
        Debug.Log(message);
        DungeonMessage?.Invoke(message);
    }
}
