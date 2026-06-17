using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory merchantInventory;

    [Header("Battle Data")]
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField] private List<EnemyDataSO> enemyPartyData =
        new List<EnemyDataSO>();
    [SerializeField, Min(1)] private int fallbackEnemyCount = 3;
    [SerializeField, Min(0.05f)] private float actionDelay = 0.5f;

    private readonly List<BattleUnit> playerUnits = new List<BattleUnit>();
    private readonly List<MercenaryInstance> battleMercenaries =
        new List<MercenaryInstance>();
    private readonly List<BattleUnit> enemyUnits = new List<BattleUnit>();
    private readonly List<EnemyDataSO> battleEnemyData = new List<EnemyDataSO>();
    private readonly List<EnemyDataSO> overrideEnemyEncounter =
        new List<EnemyDataSO>();
    private EnemyDataSO fallbackEnemyData;
    private ItemDataSO fallbackDropItem;

    public bool IsBattling { get; private set; }
    public EnemyDataSO EnemyData => enemyData;

    public event Action<string> BattleMessage;
    public event Action<string, BattleLogType> BattleMessageTyped;
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
        return StartBattle(partyMembers, null);
    }

    public bool StartBattle(
        IReadOnlyList<MercenaryInstance> partyMembers,
        IReadOnlyList<EnemyDataSO> enemyEncounter)
    {
        ResolveReferences();

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

        SetOverrideEnemyEncounter(enemyEncounter);

        if (BuildEnemyEncounterData().Count == 0)
        {
            SendBattleMessage("No enemy data is assigned.");
            return false;
        }

        CreateBattleUnits(partyMembers);
        StartCoroutine(BattleRoutine());
        return true;
    }

    public List<EnemyDataSO> CreateDefaultEnemyEncounter(int enemyCount)
    {
        ResolveReferences();

        List<EnemyDataSO> enemies = new List<EnemyDataSO>();
        EnemyDataSO enemy = enemyData != null ? enemyData : FindEnemyData();
        if (enemy == null)
        {
            return enemies;
        }

        int count = Mathf.Max(1, enemyCount);
        for (int i = 0; i < count; i++)
        {
            enemies.Add(enemy);
        }

        return enemies;
    }

    public string GetEncounterDescription()
    {
        ResolveReferences();

        List<EnemyDataSO> enemies = BuildEnemyEncounterData();
        if (enemies.Count == 0)
        {
            return "No enemy assigned";
        }

        if (enemies.Count == 1)
        {
            EnemyDataSO enemy = enemies[0];
            return $"{enemy.enemyName}  |  HP {enemy.maxHP}  ATK {enemy.attack}  " +
                   $"DEF {enemy.defense}  |  Reward {enemy.goldReward} G";
        }

        int totalGold = 0;
        foreach (EnemyDataSO enemy in enemies)
        {
            totalGold += enemy.goldReward;
        }

        return $"{enemies[0].enemyName} x{enemies.Count}  |  " +
               $"Total reward {totalGold} G";
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

        if (merchantInventory == null)
        {
            merchantInventory = GetComponent<MerchantInventory>();
        }

        if (merchantInventory == null)
        {
            merchantInventory = FindObjectOfType<MerchantInventory>();
        }

        if (enemyData == null)
        {
            enemyData = FindEnemyData();
        }
    }

    private EnemyDataSO FindEnemyData()
    {
        EnemyDataSO[] resourceEnemies = Resources.LoadAll<EnemyDataSO>(string.Empty);
        if (resourceEnemies.Length > 0)
        {
            return resourceEnemies[0];
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:EnemyDataSO",
            new[] { "Assets/Proiject/ScriptableObjects/Enemies" });

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            EnemyDataSO foundEnemy =
                AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (foundEnemy != null)
            {
                return foundEnemy;
            }
        }
#endif

        return GetFallbackSlimeData();
    }

    private EnemyDataSO GetFallbackSlimeData()
    {
        if (fallbackEnemyData != null)
        {
            return fallbackEnemyData;
        }

        fallbackEnemyData = ScriptableObject.CreateInstance<EnemyDataSO>();
        fallbackEnemyData.name = "Runtime Slime";
        fallbackEnemyData.enemyName = "Slime";
        fallbackEnemyData.maxHP = 20;
        fallbackEnemyData.attack = 5;
        fallbackEnemyData.defense = 1;
        fallbackEnemyData.attackSpeed = 1f;
        fallbackEnemyData.goldReward = 50;
        return fallbackEnemyData;
    }

    private void CreateBattleUnits(IReadOnlyList<MercenaryInstance> partyMembers)
    {
        playerUnits.Clear();
        battleMercenaries.Clear();
        enemyUnits.Clear();
        battleEnemyData.Clear();

        foreach (MercenaryInstance mercenary in partyMembers)
        {
            if (mercenary.IsIncapacitated)
            {
                continue;
            }

            playerUnits.Add(new BattleUnit(
                mercenary.MercenaryName,
                mercenary.MaxHP,
                mercenary.CurrentHP,
                mercenary.Attack,
                mercenary.Defense,
                mercenary.AttackSpeed,
                true));
            battleMercenaries.Add(mercenary);
        }

        if (playerUnits.Count == 0)
        {
            return;
        }

        List<EnemyDataSO> enemies = BuildEnemyEncounterData();
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyDataSO enemy = enemies[i];
            string unitName = enemies.Count == 1
                ? enemy.enemyName
                : $"{enemy.enemyName} {i + 1}";

            enemyUnits.Add(new BattleUnit(
                unitName,
                enemy.maxHP,
                enemy.maxHP,
                enemy.attack,
                enemy.defense,
                enemy.attackSpeed,
                false));
            battleEnemyData.Add(enemy);
        }
    }

    private List<EnemyDataSO> BuildEnemyEncounterData()
    {
        List<EnemyDataSO> enemies = new List<EnemyDataSO>();

        foreach (EnemyDataSO enemy in overrideEnemyEncounter)
        {
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }

        if (enemies.Count > 0)
        {
            return enemies;
        }

        foreach (EnemyDataSO enemy in enemyPartyData)
        {
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }

        if (enemies.Count > 0)
        {
            return enemies;
        }

        if (enemyData != null)
        {
            int count = Mathf.Max(1, fallbackEnemyCount);
            for (int i = 0; i < count; i++)
            {
                enemies.Add(enemyData);
            }
        }

        return enemies;
    }

    private void SetOverrideEnemyEncounter(IReadOnlyList<EnemyDataSO> enemyEncounter)
    {
        overrideEnemyEncounter.Clear();
        if (enemyEncounter == null)
        {
            return;
        }

        foreach (EnemyDataSO enemy in enemyEncounter)
        {
            if (enemy != null)
            {
                overrideEnemyEncounter.Add(enemy);
            }
        }
    }

    private IEnumerator BattleRoutine()
    {
        IsBattling = true;
        if (playerUnits.Count == 0)
        {
            SendBattleMessage("All party members are incapacitated.", BattleLogType.System);
            CompleteBattle(false);
            yield break;
        }

        SendBattleMessage(
            $"Battle started: {playerUnits.Count} mercenaries vs {enemyUnits.Count} enemies",
            BattleLogType.System);

        while (IsBattling)
        {
            foreach (BattleUnit playerUnit in playerUnits)
            {
                if (playerUnit.IsDead)
                {
                    continue;
                }

                yield return new WaitForSeconds(actionDelay);
                BattleUnit enemyTarget = GetFirstLivingEnemyUnit();
                if (enemyTarget == null)
                {
                    CompleteBattle(true);
                    yield break;
                }

                Attack(playerUnit, enemyTarget);

                if (GetFirstLivingEnemyUnit() == null)
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

            foreach (BattleUnit enemy in enemyUnits)
            {
                if (enemy.IsDead)
                {
                    continue;
                }

                target = GetFirstLivingPlayerUnit();
                if (target == null)
                {
                    CompleteBattle(false);
                    yield break;
                }

                yield return new WaitForSeconds(actionDelay);
                Attack(enemy, target);

                if (GetFirstLivingPlayerUnit() == null)
                {
                    CompleteBattle(false);
                    yield break;
                }
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
            $"{damageDealt} damage, HP {target.CurrentHP}/{target.MaxHP}",
            attacker.IsPlayerSide ? BattleLogType.Player : BattleLogType.Enemy);
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

    private BattleUnit GetFirstLivingEnemyUnit()
    {
        foreach (BattleUnit enemy in enemyUnits)
        {
            if (!enemy.IsDead)
            {
                return enemy;
            }
        }

        return null;
    }

    private void CompleteBattle(bool victory)
    {
        IsBattling = false;
        ResolveReferences();
        ApplyBattleResultsToMercenaries();

        if (victory)
        {
            int totalGoldReward = CalculateGoldReward();
            if (merchantData != null)
            {
                merchantData.AddGold(totalGoldReward);
            }

            SendBattleMessage($"Victory! Reward: {totalGoldReward} G", BattleLogType.Reward);
            GrantItemRewards();
        }
        else
        {
            SendBattleMessage("Defeat.", BattleLogType.System);
        }

        BattleCompleted?.Invoke(victory);
        overrideEnemyEncounter.Clear();
    }

    private void ApplyBattleResultsToMercenaries()
    {
        for (int i = 0; i < battleMercenaries.Count && i < playerUnits.Count; i++)
        {
            MercenaryInstance mercenary = battleMercenaries[i];
            BattleUnit unit = playerUnits[i];
            mercenary.SetCurrentHP(unit.CurrentHP);
            SendBattleMessage(
                $"{mercenary.MercenaryName} HP carried over: " +
                $"{mercenary.CurrentHP}/{mercenary.MaxHP}",
                BattleLogType.System);
        }
    }

    private int CalculateGoldReward()
    {
        int totalGoldReward = 0;
        foreach (EnemyDataSO enemy in battleEnemyData)
        {
            if (enemy != null)
            {
                totalGoldReward += enemy.goldReward;
            }
        }

        return totalGoldReward;
    }

    private void GrantItemRewards()
    {
        ResolveReferences();

        if (merchantInventory == null)
        {
            SendBattleMessage("No merchant inventory is assigned.", BattleLogType.System);
            return;
        }

        bool droppedAnyItem = false;
        foreach (EnemyDataSO defeatedEnemy in battleEnemyData)
        {
            if (defeatedEnemy == null || defeatedEnemy.itemDrops == null)
            {
                continue;
            }

            foreach (ItemDropEntry drop in defeatedEnemy.itemDrops)
            {
                if (drop == null || drop.item == null || drop.amount <= 0)
                {
                    continue;
                }

                if (UnityEngine.Random.value > drop.dropChance)
                {
                    continue;
                }

                merchantInventory.AddItem(drop.item, drop.amount);
                SendBattleMessage($"Loot: {drop.item.itemName} x{drop.amount}", BattleLogType.Reward);
                droppedAnyItem = true;
            }
        }

        if (!droppedAnyItem)
        {
            ItemDataSO fallbackItem = GetFallbackDropItem();
            merchantInventory.AddItem(fallbackItem, 1);
            SendBattleMessage($"Loot: {fallbackItem.itemName} x1", BattleLogType.Reward);
        }
    }

    private ItemDataSO GetFallbackDropItem()
    {
        if (fallbackDropItem != null)
        {
            return fallbackDropItem;
        }

        ItemDataSO[] resourceItems = Resources.LoadAll<ItemDataSO>(string.Empty);
        if (resourceItems.Length > 0)
        {
            fallbackDropItem = resourceItems[0];
            return fallbackDropItem;
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:ItemDataSO",
            new[] { "Assets/Proiject/ScriptableObjects/Items" });

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            fallbackDropItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (fallbackDropItem != null)
            {
                return fallbackDropItem;
            }
        }
#endif

        fallbackDropItem = ScriptableObject.CreateInstance<ItemDataSO>();
        fallbackDropItem.name = "Runtime Monster Fang";
        fallbackDropItem.itemName = "Monster Fang";
        fallbackDropItem.itemType = ItemType.Material;
        fallbackDropItem.rarity = ItemRarity.Common;
        fallbackDropItem.description = "A common monster material for testing trade flow.";
        fallbackDropItem.basePrice = 25;
        return fallbackDropItem;
    }

    private void SendBattleMessage(string message)
    {
        SendBattleMessage(message, BattleLogType.System);
    }

    private void SendBattleMessage(string message, BattleLogType logType)
    {
        Debug.Log(message);
        BattleMessage?.Invoke(message);
        BattleMessageTyped?.Invoke(message, logType);
    }
}
