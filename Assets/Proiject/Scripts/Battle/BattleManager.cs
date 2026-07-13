using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField, Range(0f, 1f)] private float playerSkillUseChance = 0.6f;
    [SerializeField, Range(0f, 1f)] private float enemySkillUseChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float baseCriticalRate = 0.08f;
    [SerializeField, Min(1f)] private float criticalDamageMultiplier = 1.5f;

    private readonly List<BattleUnit> playerUnits = new List<BattleUnit>();
    private readonly List<MercenaryInstance> battleMercenaries =
        new List<MercenaryInstance>();
    private readonly List<BattleUnit> enemyUnits = new List<BattleUnit>();
    private readonly List<EnemyDataSO> battleEnemyData = new List<EnemyDataSO>();
    private readonly List<EnemyDataSO> overrideEnemyEncounter =
        new List<EnemyDataSO>();
    private EnemyDataSO fallbackEnemyData;
    private BattleRewardService battleRewardService;
    private bool skipToBattleEndRequested;
    private readonly BattleStatusEffectService battleStatusEffectService =
        new BattleStatusEffectService();

    public bool IsBattling { get; private set; }
    public bool IsSkippingToBattleEnd => skipToBattleEndRequested;
    public float BattleSpeedMultiplier { get; private set; } = 1f;
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
        skipToBattleEndRequested = false;
        IsBattling = true;
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
            return $"{GetEnemyDisplayName(enemy)}  |  " +
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

        return $"{GetEnemyDisplayName(enemies[0])} x{enemies.Count}  |  " +
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
        IReadOnlyList<EnemyDataSO> resourceEnemies =
            GameAssetRepository.LoadAll<EnemyDataSO>();
        if (resourceEnemies.Count > 0)
        {
            return resourceEnemies[0];
        }

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
                mercenary.MaxMagicPower,
                mercenary.CriticalRate,
                mercenary.EvasionRate,
                mercenary.StatusEffect,
                mercenary.Level));
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
            string enemyName = BuildEnemyDisplayName(enemy);
            string unitName = enemies.Count == 1
                ? enemyName
                : $"{enemyName} {i + 1}";
            unitName = ColorSpecialEnemyName(unitName, enemy.isSpecialVariant);

            enemyUnits.Add(new BattleUnit(
                unitName,
                enemy.maxHP,
                enemy.maxHP,
                enemy.attack,
                enemy.defense,
                enemy.attackSpeed,
                false,
                MercenaryClass.Warrior,
                enemy.maxMagicPower,
                baseCriticalRate + enemy.criticalRate,
                enemy.evasionRate));
            battleEnemyData.Add(enemy);
        }
    }

    private static string GetEnemyDisplayName(EnemyDataSO enemy)
    {
        if (enemy == null)
        {
            return "不明な敵";
        }
        return ColorSpecialEnemyName(
            BuildEnemyDisplayName(enemy),
            enemy.isSpecialVariant);
    }

    private static string BuildEnemyDisplayName(EnemyDataSO enemy)
    {
        string enemyName =
            JapaneseDisplayText.GetEnemyName(enemy.enemyName);
        if (enemy.isSpecialVariant)
        {
            string title = string.IsNullOrWhiteSpace(
                enemy.specialVariantTitle)
                ? "変異した"
                : enemy.specialVariantTitle;
            enemyName = $"{title}{enemyName}";
        }
        if (enemy.category == EnemyCategory.MythicalBeast)
        {
            enemyName = $"幻獣 {enemyName}";
        }
        if (enemy.isBoss)
        {
            enemyName = $"ボス {enemyName}";
        }
        return enemyName;
    }

    private static string ColorSpecialEnemyName(
        string enemyName,
        bool isSpecialVariant)
    {
        return isSpecialVariant
            ? $"<color=#D86BFF>{enemyName}</color>"
            : enemyName;
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
        if (playerUnits.Count == 0)
        {
            SendBattleMessage("パーティー全員が戦闘不能です。", BattleLogType.System);
            CompleteBattle(false);
            yield break;
        }

        SendBattleMessage(
            BattleLogFormatter.FormatBattleStart(
                playerUnits.Count,
                enemyUnits.Count),
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

                if (!skipToBattleEndRequested)
                {
                    yield return WaitForActionDelay();
                }

                BattleStatusEffectResult statusResult =
                    battleStatusEffectService.ProcessActionStart(unit);
                if (statusResult.PoisonDamage > 0)
                {
                    SendBattleMessage(statusResult.LogMessage, statusResult.LogType);
                    if (statusResult.IsUnitDead)
                    {
                        if (GetFirstLivingPlayerUnit() == null)
                        {
                            CompleteBattle(false);
                            yield break;
                        }
                        if (GetFirstLivingEnemyUnit() == null)
                        {
                            CompleteBattle(true);
                            yield break;
                        }
                        continue;
                    }
                }

                if (statusResult.IsActionSkipped)
                {
                    SendBattleMessage(statusResult.LogMessage, statusResult.LogType);
                    continue;
                }

                if (unit.IsPlayerSide)
                {
                    unit.GainMagicPower(CalculateMagicGain(unit));
                    BattleUnit enemyTarget = GetFirstLivingEnemyUnit();
                    if (enemyTarget == null)
                    {
                        CompleteBattle(true);
                        yield break;
                    }

                    if (!CreateSkillResolver().TryUsePlayerSkill(unit, enemyTarget))
                    {
                        Attack(unit, enemyTarget);
                    }
                    battleStatusEffectService.TickAfterAction(unit);

                    if (GetFirstLivingEnemyUnit() == null)
                    {
                        CompleteBattle(true);
                        yield break;
                    }
                }
                else
                {
                    BattleUnit target = CreateSkillResolver().GetEnemyTarget();
                    if (target == null)
                    {
                        CompleteBattle(false);
                        yield break;
                    }

                    int enemyIndex = enemyUnits.IndexOf(unit);
                    EnemyDataSO sourceData =
                        enemyIndex >= 0 && enemyIndex < battleEnemyData.Count
                            ? battleEnemyData[enemyIndex]
                            : null;
                    if (!CreateSkillResolver().TryUseEnemySkill(
                            unit, target, sourceData))
                    {
                        Attack(unit, target);
                    }
                    battleStatusEffectService.TickAfterAction(unit);

                    if (GetFirstLivingPlayerUnit() == null)
                    {
                        CompleteBattle(false);
                        yield break;
                    }
                }
            }

            if (skipToBattleEndRequested)
            {
                // Keep the UI responsive while removing per-action delays.
                yield return null;
            }
        }
    }

    private IEnumerator WaitForActionDelay()
    {
        float remaining =
            actionDelay / Mathf.Max(1f, BattleSpeedMultiplier);
        while (remaining > 0f && !skipToBattleEndRequested)
        {
            remaining -= Time.unscaledDeltaTime;
            yield return null;
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
        if (target.TryEvade())
        {
            SendBattleMessage(
                BattleLogFormatter.FormatEvadedAttack(
                    target.UnitName,
                    attacker.UnitName),
                attacker.IsPlayerSide
                    ? BattleLogType.Player
                    : BattleLogType.Enemy);
            return;
        }

        bool critical = attacker.RollCritical();
        int rawDamage = critical
            ? Mathf.RoundToInt(
                attacker.CalculateDamage() * criticalDamageMultiplier)
            : attacker.CalculateDamage();
        int previousHP = target.CurrentHP;
        target.TakeDamage(rawDamage);
        int damageDealt = previousHP - target.CurrentHP;

        SendBattleMessage(
            BattleLogFormatter.FormatAttack(
                attacker.UnitName,
                target.UnitName,
                critical,
                damageDealt,
                target.CurrentHP,
                target.MaxHP),
            attacker.IsPlayerSide ? BattleLogType.Player : BattleLogType.Enemy);
    }

    public float CycleBattleSpeed()
    {
        BattleSpeedMultiplier = BattleSpeedMultiplier < 1.5f
            ? 2f
            : BattleSpeedMultiplier < 3f
                ? 4f
                : 1f;
        return BattleSpeedMultiplier;
    }

    public bool RequestSkipToBattleEnd()
    {
        if (!IsBattling || skipToBattleEndRequested)
        {
            return false;
        }

        skipToBattleEndRequested = true;
        SendBattleMessage(
            "戦闘終了まで早送りします。",
            BattleLogType.System);
        return true;
    }

    private BattleSkillResolver CreateSkillResolver()
    {
        return new BattleSkillResolver(new BattleSkillResolverContext(
            playerUnits, enemyUnits, playerSkillUseChance, enemySkillUseChance,
            criticalDamageMultiplier, () => UnityEngine.Random.value, SendBattleMessage));
    }

    private BattleUnit GetFirstLivingPlayerUnit()
    {
        foreach (BattleUnit playerUnit in playerUnits)
        {
            if (!playerUnit.IsDead) return playerUnit;
        }
        return null;
    }

    private BattleUnit GetFirstLivingEnemyUnit()
    {
        foreach (BattleUnit enemyUnit in enemyUnits)
        {
            if (!enemyUnit.IsDead) return enemyUnit;
        }
        return null;
    }

    private void CompleteBattle(bool victory)
    {
        if (!IsBattling)
        {
            return;
        }

        IsBattling = false;
        skipToBattleEndRequested = false;
        ResolveReferences();
        BattleRewardService rewardService = GetBattleRewardService();
        rewardService.ApplyBattleResultsToMercenaries(battleMercenaries, playerUnits);
        if (victory)
        {
            rewardService.GrantVictoryRewards(battleEnemyData, battleMercenaries);
            EnemiesDefeated?.Invoke(battleEnemyData);
        }
        else
        {
            SendBattleMessage(BattleLogFormatter.FormatDefeat(), BattleLogType.System);
        }
        BattleCompleted?.Invoke(victory);
        overrideEnemyEncounter.Clear();
    }

    private BattleRewardService GetBattleRewardService()
    {
        if (battleRewardService == null || !battleRewardService.MatchesDependencies(merchantData, merchantInventory))
        {
            battleRewardService = new BattleRewardService(merchantData, merchantInventory, SendBattleMessage, () => UnityEngine.Random.value);
        }
        return battleRewardService;
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
