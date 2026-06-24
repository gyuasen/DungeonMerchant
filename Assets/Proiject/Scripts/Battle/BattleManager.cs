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
    [SerializeField, Min(0)] private int magicPowerGainPerAction = 20;
    [SerializeField, Min(0)] private int warriorTauntCost = 35;
    [SerializeField, Min(0)] private int archerDoubleShotCost = 45;
    [SerializeField, Min(0)] private int mageFireballCost = 50;
    [SerializeField, Range(0f, 1f)] private float playerSkillUseChance = 0.6f;

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
    public event Action<IReadOnlyList<EnemyDataSO>> EnemiesDefeated;

    public bool StartBattle()
    {
        ResolveReferences();

        if (partyManager == null)
        {
            SendBattleMessage("パーティー管理が設定されていません。");
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
            SendBattleMessage("すでに戦闘中です。");
            return false;
        }

        if (partyMembers == null || partyMembers.Count == 0)
        {
            SendBattleMessage("パーティーに傭兵を1人以上編成してください。");
            return false;
        }

        SetOverrideEnemyEncounter(enemyEncounter);

        if (BuildEnemyEncounterData().Count == 0)
        {
            SendBattleMessage("敵データが設定されていません。");
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
            return "敵が設定されていません";
        }

        if (enemies.Count == 1)
        {
            EnemyDataSO enemy = enemies[0];
            return $"{JapaneseDisplayText.GetEnemyName(enemy.enemyName)}  |  " +
                   $"{JapaneseDisplayText.GetMonsterGrade(enemy)}  |  HP {enemy.maxHP}  " +
                   $"攻撃 {enemy.attack}  防御 {enemy.defense}  " +
                   $"魔力 {enemy.maxMagicPower}  速度 {enemy.attackSpeed:0.00}  |  " +
                   $"報酬 {enemy.goldReward} G";
        }

        int totalGold = 0;
        foreach (EnemyDataSO enemy in enemies)
        {
            totalGold += enemy.goldReward;
        }

        return $"{JapaneseDisplayText.GetEnemyName(enemies[0].enemyName)} x{enemies.Count}  |  " +
               $"合計報酬 {totalGold} G";
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
        fallbackEnemyData.maxMagicPower = 20;
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
                true,
                mercenary.MercenaryClass,
                mercenary.MaxMagicPower));
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
            string enemyName = JapaneseDisplayText.GetEnemyName(enemy.enemyName);
            if (enemy.isBoss)
            {
                enemyName = $"ボス {enemyName}";
            }
            string unitName = enemies.Count == 1
                ? enemyName
                : $"{enemyName} {i + 1}";

            enemyUnits.Add(new BattleUnit(
                unitName,
                enemy.maxHP,
                enemy.maxHP,
                enemy.attack,
                enemy.defense,
                enemy.attackSpeed,
                false,
                MercenaryClass.Warrior,
                enemy.maxMagicPower));
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
            SendBattleMessage("パーティー全員が戦闘不能です。", BattleLogType.System);
            CompleteBattle(false);
            yield break;
        }

        SendBattleMessage(
            $"戦闘開始: 傭兵{playerUnits.Count}人 vs 敵{enemyUnits.Count}体",
            BattleLogType.System);

        while (IsBattling)
        {
            List<BattleUnit> actionOrder = BuildActionOrder();
            foreach (BattleUnit unit in actionOrder)
            {
                if (unit.IsDead)
                {
                    continue;
                }

                yield return new WaitForSeconds(actionDelay);

                if (unit.IsPlayerSide)
                {
                    unit.GainMagicPower(CalculateMagicGain(unit));
                    BattleUnit enemyTarget = GetFirstLivingEnemyUnit();
                    if (enemyTarget == null)
                    {
                        CompleteBattle(true);
                        yield break;
                    }

                    if (!TryUsePlayerSkill(unit, enemyTarget))
                    {
                        Attack(unit, enemyTarget);
                    }
                    unit.TickStatuses();

                    if (GetFirstLivingEnemyUnit() == null)
                    {
                        CompleteBattle(true);
                        yield break;
                    }
                }
                else
                {
                    BattleUnit target = GetEnemyTarget();
                    if (target == null)
                    {
                        CompleteBattle(false);
                        yield break;
                    }

                    Attack(unit, target);

                    if (GetFirstLivingPlayerUnit() == null)
                    {
                        CompleteBattle(false);
                        yield break;
                    }
                }
            }
        }
    }

    private List<BattleUnit> BuildActionOrder()
    {
        List<BattleUnit> units = new List<BattleUnit>();
        units.AddRange(playerUnits);
        units.AddRange(enemyUnits);
        units.RemoveAll(unit => unit == null || unit.IsDead);
        units.Sort((left, right) =>
            right.AttackSpeed.CompareTo(left.AttackSpeed));
        return units;
    }

    private int CalculateMagicGain(BattleUnit unit)
    {
        return Mathf.Max(
            1,
            Mathf.RoundToInt(magicPowerGainPerAction * unit.AttackSpeed));
    }

    private void Attack(BattleUnit attacker, BattleUnit target)
    {
        int previousHP = target.CurrentHP;
        target.TakeDamage(attacker.CalculateDamage());
        int damageDealt = previousHP - target.CurrentHP;

        SendBattleMessage(
            $"{attacker.UnitName}が{target.UnitName}を攻撃: " +
            $"{damageDealt}ダメージ、HP {target.CurrentHP}/{target.MaxHP}",
            attacker.IsPlayerSide ? BattleLogType.Player : BattleLogType.Enemy);
    }

    private bool TryUsePlayerSkill(BattleUnit attacker, BattleUnit primaryTarget)
    {
        if (UnityEngine.Random.value > playerSkillUseChance)
        {
            return false;
        }

        if (CanDefeatWithNormalAttack(attacker, primaryTarget))
        {
            return false;
        }

        switch (attacker.MercenaryClass)
        {
            case MercenaryClass.Warrior:
                return TryUseWarriorTaunt(attacker, primaryTarget);
            case MercenaryClass.Archer:
                return TryUseArcherDoubleShot(attacker, primaryTarget);
            case MercenaryClass.Mage:
                return TryUseMageFireball(attacker, primaryTarget);
            default:
                return false;
        }
    }

    private bool TryUseWarriorTaunt(BattleUnit attacker, BattleUnit target)
    {
        if (attacker.IsTaunting)
        {
            return false;
        }

        if (!attacker.TryConsumeMagicPower(warriorTauntCost))
        {
            return false;
        }

        attacker.StartTaunt(2);
        SendBattleMessage(
            $"{attacker.UnitName}がスキル「挑発」を発動: " +
            $"敵の攻撃を引きつけます。状態: {attacker.StatusSummary}。 " +
            $"魔力 {attacker.CurrentMagicPower}/{attacker.MaxMagicPower}",
            BattleLogType.Player);
        return true;
    }

    private bool TryUseArcherDoubleShot(BattleUnit attacker, BattleUnit target)
    {
        if (!HasUsefulSkillTarget(
                attacker,
                Mathf.RoundToInt(attacker.Attack * 0.75f),
                1) ||
            !attacker.TryConsumeMagicPower(archerDoubleShotCost))
        {
            return false;
        }

        int totalDamage = 0;
        for (int i = 0; i < 2; i++)
        {
            BattleUnit shotTarget = GetUsefulSkillTarget(
                attacker,
                Mathf.RoundToInt(attacker.Attack * 0.75f));
            if (shotTarget == null)
            {
                break;
            }

            int previousHP = shotTarget.CurrentHP;
            shotTarget.TakeDamage(Mathf.RoundToInt(attacker.Attack * 0.75f));
            totalDamage += previousHP - shotTarget.CurrentHP;
        }

        SendBattleMessage(
            $"{attacker.UnitName}がスキル「連射」を発動: " +
            $"合計{totalDamage}ダメージ。 " +
            $"魔力 {attacker.CurrentMagicPower}/{attacker.MaxMagicPower}",
            BattleLogType.Player);
        return true;
    }

    private bool TryUseMageFireball(BattleUnit attacker, BattleUnit target)
    {
        int fireballDamage = Mathf.RoundToInt(attacker.Attack * 1.65f);
        BattleUnit skillTarget = GetUsefulSkillTarget(attacker, fireballDamage);
        if (skillTarget == null ||
            !attacker.TryConsumeMagicPower(mageFireballCost))
        {
            return false;
        }

        int previousHP = skillTarget.CurrentHP;
        skillTarget.TakeDamage(fireballDamage);
        int damageDealt = previousHP - skillTarget.CurrentHP;
        SendBattleMessage(
            $"{attacker.UnitName}がスキル「火球」を発動: " +
            $"{skillTarget.UnitName}に{damageDealt}ダメージ。 " +
            $"魔力 {attacker.CurrentMagicPower}/{attacker.MaxMagicPower}",
            BattleLogType.Player);
        return true;
    }

    private bool CanDefeatWithNormalAttack(BattleUnit attacker, BattleUnit target)
    {
        return target != null &&
               target.CurrentHP <= target.EstimateDamageTaken(attacker.CalculateDamage());
    }

    private bool HasUsefulSkillTarget(
        BattleUnit attacker,
        int rawSkillDamage,
        int maxTargets)
    {
        int found = 0;
        foreach (BattleUnit enemy in enemyUnits)
        {
            if (IsUsefulSkillTarget(attacker, enemy, rawSkillDamage))
            {
                found++;
                if (found >= maxTargets)
                {
                    return true;
                }
            }
        }

        return found > 0;
    }

    private BattleUnit GetUsefulSkillTarget(
        BattleUnit attacker,
        int rawSkillDamage)
    {
        BattleUnit bestTarget = null;
        int bestHP = -1;
        foreach (BattleUnit enemy in enemyUnits)
        {
            if (!IsUsefulSkillTarget(attacker, enemy, rawSkillDamage))
            {
                continue;
            }

            if (enemy.CurrentHP > bestHP)
            {
                bestHP = enemy.CurrentHP;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private bool IsUsefulSkillTarget(
        BattleUnit attacker,
        BattleUnit target,
        int rawSkillDamage)
    {
        if (target == null || target.IsDead)
        {
            return false;
        }

        int normalDamage = target.EstimateDamageTaken(attacker.CalculateDamage());
        if (target.CurrentHP <= normalDamage)
        {
            return false;
        }

        int skillDamage = target.EstimateDamageTaken(rawSkillDamage);
        if (skillDamage <= normalDamage)
        {
            return false;
        }

        return skillDamage <= target.CurrentHP + Mathf.Max(8, normalDamage);
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

    private BattleUnit GetEnemyTarget()
    {
        foreach (BattleUnit playerUnit in playerUnits)
        {
            if (playerUnit.IsTaunting)
            {
                return playerUnit;
            }
        }

        return GetFirstLivingPlayerUnit();
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

            SendBattleMessage($"勝利！ 報酬: {totalGoldReward} G", BattleLogType.Reward);
            GrantExperienceRewards();
            GrantItemRewards();
            EnemiesDefeated?.Invoke(battleEnemyData);
        }
        else
        {
            SendBattleMessage("敗北しました。", BattleLogType.System);
        }

        BattleCompleted?.Invoke(victory);
        overrideEnemyEncounter.Clear();
    }

    private void GrantExperienceRewards()
    {
        if (battleMercenaries.Count == 0)
        {
            return;
        }

        int totalExperience = CalculateExperienceReward();
        int experiencePerMercenary =
            Mathf.Max(1, totalExperience / battleMercenaries.Count);

        foreach (MercenaryInstance mercenary in battleMercenaries)
        {
            int previousLevel = mercenary.Level;
            int levelsGained = mercenary.AddExperience(experiencePerMercenary);

            SendBattleMessage(
                $"{mercenary.MercenaryName}が経験値{experiencePerMercenary}を獲得 " +
                $"({mercenary.CurrentExperience}/{mercenary.ExperienceToNextLevel})",
                BattleLogType.Reward);

            if (levelsGained > 0)
            {
                SendBattleMessage(
                    $"{mercenary.MercenaryName}がレベル{previousLevel}から" +
                    $"レベル{mercenary.Level}に上昇！",
                    BattleLogType.Reward);
            }
        }
    }

    private int CalculateExperienceReward()
    {
        int totalExperience = 0;

        foreach (EnemyDataSO enemy in battleEnemyData)
        {
            if (enemy == null)
            {
                continue;
            }

            int grade = Mathf.Clamp(enemy.monsterGrade, 1, 10);
            int strengthRank = 11 - grade;
            int enemyExperience = 10 * strengthRank * strengthRank;
            if (enemy.isBoss)
            {
                enemyExperience *= 2;
            }

            totalExperience += enemyExperience;
        }

        return Mathf.Max(1, totalExperience);
    }

    private void ApplyBattleResultsToMercenaries()
    {
        for (int i = 0; i < battleMercenaries.Count && i < playerUnits.Count; i++)
        {
            MercenaryInstance mercenary = battleMercenaries[i];
            BattleUnit unit = playerUnits[i];
            mercenary.SetCurrentHP(unit.CurrentHP);
            SendBattleMessage(
                $"{mercenary.MercenaryName}の戦闘後HP: " +
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
            SendBattleMessage("商人在庫が設定されていません。", BattleLogType.System);
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
                SendBattleMessage(
                    $"戦利品: {JapaneseDisplayText.GetItemName(drop.item)} x{drop.amount}",
                    BattleLogType.Reward);
                droppedAnyItem = true;
            }
        }

        if (!droppedAnyItem)
        {
            ItemDataSO fallbackItem = GetFallbackDropItem();
            merchantInventory.AddItem(fallbackItem, 1);
            SendBattleMessage(
                $"戦利品: {JapaneseDisplayText.GetItemName(fallbackItem)} x1",
                BattleLogType.Reward);
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
