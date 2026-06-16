using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MerchantData merchantData;

    [Header("Battle Data")]
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField, Min(0.05f)] private float actionDelay = 0.5f;

    private readonly List<BattleUnit> playerUnits = new List<BattleUnit>();
    private BattleUnit enemyUnit;

    public bool IsBattling { get; private set; }
    public EnemyDataSO EnemyData => enemyData;

    public event Action<string> BattleMessage;
    public event Action<bool> BattleCompleted;

    public bool StartBattle()
    {
        ResolveReferences();

        if (partyManager == null)
        {
            SendBattleMessage("No party manager is assigned.");
            return false;
        }

        return StartBattle(partyManager.Members);
    }

    public bool StartBattle(IReadOnlyList<MercenaryInstance> partyMembers)
    {
        if (IsBattling)
        {
            SendBattleMessage("A battle is already in progress.");
            return false;
        }

        if (partyMembers == null || partyMembers.Count == 0)
        {
            SendBattleMessage("Add at least one mercenary to the party.");
            return false;
        }

        if (enemyData == null)
        {
            SendBattleMessage("No enemy data is assigned.");
            return false;
        }

        CreateBattleUnits(partyMembers);
        StartCoroutine(BattleRoutine());
        return true;
    }

    private void ResolveReferences()
    {
        if (partyManager == null)
        {
            partyManager = FindObjectOfType<MercenaryPartyManager>();
        }

        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }
    }

    private void CreateBattleUnits(IReadOnlyList<MercenaryInstance> partyMembers)
    {
        playerUnits.Clear();

        foreach (MercenaryInstance mercenary in partyMembers)
        {
            playerUnits.Add(new BattleUnit(
                mercenary.MercenaryName,
                mercenary.MaxHP,
                mercenary.Attack,
                mercenary.Defense,
                mercenary.AttackSpeed));
        }

        enemyUnit = new BattleUnit(
            enemyData.enemyName,
            enemyData.maxHP,
            enemyData.attack,
            enemyData.defense,
            enemyData.attackSpeed);
    }

    private IEnumerator BattleRoutine()
    {
        IsBattling = true;
        SendBattleMessage(
            $"Battle started: {playerUnits.Count} mercenaries vs {enemyUnit.UnitName}");

        while (IsBattling)
        {
            foreach (BattleUnit playerUnit in playerUnits)
            {
                if (playerUnit.IsDead)
                {
                    continue;
                }

                yield return new WaitForSeconds(actionDelay);
                Attack(playerUnit, enemyUnit);

                if (enemyUnit.IsDead)
                {
                    CompleteBattle(true);
                    yield break;
                }
            }

            BattleUnit target = GetFirstLivingPlayerUnit();
            if (target == null)
            {
                CompleteBattle(false);
                yield break;
            }

            yield return new WaitForSeconds(actionDelay);
            Attack(enemyUnit, target);

            if (GetFirstLivingPlayerUnit() == null)
            {
                CompleteBattle(false);
                yield break;
            }
        }
    }

    private void Attack(BattleUnit attacker, BattleUnit target)
    {
        int previousHP = target.CurrentHP;
        target.TakeDamage(attacker.CalculateDamage());
        int damageDealt = previousHP - target.CurrentHP;

        SendBattleMessage(
            $"{attacker.UnitName} attacked {target.UnitName}: " +
            $"{damageDealt} damage, HP {target.CurrentHP}/{target.MaxHP}");
    }

    private BattleUnit GetFirstLivingPlayerUnit()
    {
        foreach (BattleUnit playerUnit in playerUnits)
        {
            if (!playerUnit.IsDead)
            {
                return playerUnit;
            }
        }

        return null;
    }

    private void CompleteBattle(bool victory)
    {
        IsBattling = false;

        if (victory)
        {
            if (merchantData != null)
            {
                merchantData.AddGold(enemyData.goldReward);
            }

            SendBattleMessage($"Victory! Reward: {enemyData.goldReward} G");
        }
        else
        {
            SendBattleMessage("Defeat.");
        }

        BattleCompleted?.Invoke(victory);
    }

    private void SendBattleMessage(string message)
    {
        Debug.Log(message);
        BattleMessage?.Invoke(message);
    }
}
