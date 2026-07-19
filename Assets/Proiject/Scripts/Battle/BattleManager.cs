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

    [Header("Battle Presentation")]
    [SerializeField] private Sprite defaultBattleBackground;
    [SerializeField] private string defaultBattleBackgroundKey = "Default";

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
    private Sprite nextBattleBackground;
    private string nextBattleBackgroundKey;
    private readonly BattleStatusEffectService battleStatusEffectService =
        new BattleStatusEffectService();
    private readonly BattleConsumableService battleConsumableService =
        new BattleConsumableService();

    public bool IsBattling { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsSkippingToBattleEnd => skipToBattleEndRequested;
    public float BattleSpeedMultiplier { get; private set; } = 1f;
    public EnemyDataSO EnemyData => enemyData;

    public event Action<string> BattleMessage;
    public event Action<string, BattleLogType> BattleMessageTyped;
    public event Action<BattlePresentationRoster> BattleVisualsPrepared;
    public event Action<BattlePresentationEvent> BattlePresentation;
    public event Action<bool> BattleCompleted;
    public event Action<IReadOnlyList<EnemyDataSO>> EnemiesDefeated;

    public void SetNextBattleBackground(Sprite background, string resourceKey)
    {
        nextBattleBackground = background;
        nextBattleBackgroundKey = resourceKey;
    }

    public bool StartBattle()
    {
        ResolveReferences();

        if (partyManager == null)
        {
            ClearNextBattleBackground();
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
            ClearNextBattleBackground();
            SendBattleMessage("すでに戦闘中です。");
            return false;
        }

        if (partyMembers == null || partyMembers.Count == 0)
        {
            ClearNextBattleBackground();
            SendBattleMessage("パーティーに傭兵を1人以上編成してください。");
            return false;
        }

        SetOverrideEnemyEncounter(enemyEncounter);

        if (BuildEnemyEncounterData().Count == 0)
        {
            ClearNextBattleBackground();
            SendBattleMessage("敵データが設定されていません。");
            return false;
        }

        CreateBattleUnits(partyMembers);
        skipToBattleEndRequested = false;
        IsPaused = false;
        IsBattling = true;
        PrepareBattleVisuals();
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
        fallbackEnemyData.race = EnemyRace.Slime;
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

            BattleUnit unit = new BattleUnit(
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
                mercenary.Level);
            unit.ApplyEquipmentEffects(CreateEquipmentEffectSnapshot(mercenary));
            playerUnits.Add(unit);
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
                enemy.evasionRate,
                BattleStatusEffect.None,
                1,
                enemy.race));
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

                ProcessConsumableUse(unit);
                int regeneration = unit.ProcessEquipmentTurnRegeneration();
                if (regeneration > 0)
                {
                    RaisePresentation(new BattlePresentationEvent(
                        BattlePresentationEventType.Heal,
                        unit,
                        unit,
                        regeneration,
                        unit.CurrentHP,
                        unit.MaxHP,
                        actionKind: BattlePresentationActionKind.StatusEffect));
                }
                BattleStatusEffect previousStatus = unit.StatusEffect;
                BattleStatusEffectResult statusResult =
                    battleStatusEffectService.ProcessActionStart(unit);
                if (statusResult.PoisonDamage > 0)
                {
                    RaisePresentation(new BattlePresentationEvent(
                        BattlePresentationEventType.Damage,
                        unit,
                        unit,
                        statusResult.PoisonDamage,
                        unit.CurrentHP,
                        unit.MaxHP,
                        actionKind: BattlePresentationActionKind.StatusEffect));
                    if (unit.StatusEffect != previousStatus)
                    {
                        RaisePresentation(new BattlePresentationEvent(
                            BattlePresentationEventType.Status,
                            unit,
                            unit,
                            currentHP: unit.CurrentHP,
                            maxHP: unit.MaxHP,
                            statusEffect: unit.StatusEffect,
                            actionKind:
                                BattlePresentationActionKind.StatusEffect));
                    }
                    SendBattleMessage(statusResult.LogMessage, statusResult.LogType);
                    if (statusResult.IsUnitDead)
                    {
                        RaisePresentation(new BattlePresentationEvent(
                            BattlePresentationEventType.Defeated,
                            unit,
                            unit,
                            currentHP: unit.CurrentHP,
                            maxHP: unit.MaxHP));
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
                    RaisePresentation(new BattlePresentationEvent(
                        BattlePresentationEventType.Status,
                        unit,
                        unit,
                        currentHP: unit.CurrentHP,
                        maxHP: unit.MaxHP,
                        statusEffect: unit.StatusEffect,
                        actionKind: BattlePresentationActionKind.StatusEffect));
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

                    Dictionary<BattleUnit, BattleUnitPresentationState> before =
                        CapturePresentationStates();
                    BattleSkillResolver playerSkillResolver = CreateSkillResolver();
                    if (!playerSkillResolver.TryUsePlayerSkill(
                            unit, enemyTarget, out string playerSkillName))
                    {
                        Attack(unit, enemyTarget);
                    }
                    else
                    {
                        RaisePresentation(new BattlePresentationEvent(
                            BattlePresentationEventType.Action,
                            unit,
                            enemyTarget,
                            actionKind: BattlePresentationActionKind.Skill,
                            actionLabel: playerSkillName));
                        RaiseStateChangeEvents(unit, before);
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
                    BattleSkillResolver enemySkillResolver = CreateSkillResolver();
                    BattleUnit target = enemySkillResolver.GetEnemyTarget();
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
                    Dictionary<BattleUnit, BattleUnitPresentationState> before =
                        CapturePresentationStates();
                    if (!enemySkillResolver.TryUseEnemySkill(
                            unit, target, sourceData, out string enemySkillName))
                    {
                        Attack(unit, target);
                    }
                    else
                    {
                        RaisePresentation(new BattlePresentationEvent(
                            BattlePresentationEventType.Action,
                            unit,
                            target,
                            actionKind: BattlePresentationActionKind.Skill,
                            actionLabel: enemySkillName));
                        RaiseStateChangeEvents(unit, before);
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

    private void ProcessConsumableUse(BattleUnit unit)
    {
        if (unit == null || !unit.IsPlayerSide)
        {
            return;
        }

        int mercenaryIndex = playerUnits.IndexOf(unit);
        MercenaryInstance mercenary =
            mercenaryIndex >= 0 && mercenaryIndex < battleMercenaries.Count
                ? battleMercenaries[mercenaryIndex]
                : null;
        BattleConsumableResult result =
            battleConsumableService.ProcessActionStart(unit, mercenary);
        if (!result.Used)
        {
            return;
        }

        RaisePresentation(new BattlePresentationEvent(
            result.HealedAmount > 0
                ? BattlePresentationEventType.Heal
                : BattlePresentationEventType.Status,
            unit,
            unit,
            result.HealedAmount,
            unit.CurrentHP,
            unit.MaxHP,
            statusEffect: unit.StatusEffect,
            actionKind: BattlePresentationActionKind.StatusEffect,
            actionLabel: result.Item.itemName));
        SendBattleMessage(BattleLogFormatter.FormatConsumableUse(
            unit.UnitName,
            result.Item.itemName,
            result.HealedAmount),
            BattleLogType.Player);
    }

    private static BattleEquipmentEffectSnapshot CreateEquipmentEffectSnapshot(
        MercenaryInstance mercenary)
    {
        float attackBuff = mercenary.GetEquipmentEffectTotal(
            EquipmentEffectType.BattleStartAttackBuff);
        float defenseBuff = mercenary.GetEquipmentEffectTotal(
            EquipmentEffectType.BattleStartDefenseBuff);
        float regenerationRate = mercenary.GetEquipmentEffectTotal(
            EquipmentEffectType.TurnRegeneration);
        float damageReduction = mercenary.GetEquipmentEffectTotal(
            EquipmentEffectType.DamageReduction);
        float lowHpDamageBonus = mercenary.GetEquipmentEffectTotal(
            EquipmentEffectType.LowHpDamageBonus);
        int attackBuffTurns = GetEquipmentEffectMaximumDuration(
            mercenary,
            EquipmentEffectType.BattleStartAttackBuff);
        int defenseBuffTurns = GetEquipmentEffectMaximumDuration(
            mercenary,
            EquipmentEffectType.BattleStartDefenseBuff);
        float lowHpThreshold = GetEquipmentEffectMaximumSecondaryValue(
            mercenary,
            EquipmentEffectType.LowHpDamageBonus);
        Dictionary<EnemyRace, float> raceMultipliers =
            GetRaceDamageMultipliers(mercenary);
        return new BattleEquipmentEffectSnapshot(
            1f,
            lowHpDamageBonus,
            lowHpThreshold,
            1f - damageReduction,
            Mathf.RoundToInt(mercenary.MaxHP * regenerationRate),
            attackBuff,
            attackBuffTurns,
            defenseBuff,
            defenseBuffTurns,
            raceMultipliers);
    }

    private static Dictionary<EnemyRace, float> GetRaceDamageMultipliers(
        MercenaryInstance mercenary)
    {
        Dictionary<EnemyRace, float> multipliers =
            new Dictionary<EnemyRace, float>();
        foreach (EquipmentEffectDefinition effect in
                 mercenary.GetActiveEquipmentEffects())
        {
            if (effect == null || effect.type != EquipmentEffectType.RaceDamageBonus ||
                effect.targetRace == EnemyRace.Unknown)
            {
                continue;
            }
            float currentBonus = multipliers.ContainsKey(effect.targetRace)
                ? multipliers[effect.targetRace] - 1f
                : 0f;
            multipliers[effect.targetRace] = 1f + Mathf.Clamp(
                currentBonus + effect.value,
                0f,
                0.60f);
        }
        return multipliers;
    }

    private static int GetEquipmentEffectMaximumDuration(
        MercenaryInstance mercenary,
        EquipmentEffectType type)
    {
        int duration = 0;
        foreach (EquipmentEffectDefinition effect in
                 mercenary.GetActiveEquipmentEffects())
        {
            if (effect != null && effect.type == type)
            {
                duration = Mathf.Max(duration, effect.durationTurns);
            }
        }
        return duration;
    }

    private static float GetEquipmentEffectMaximumSecondaryValue(
        MercenaryInstance mercenary,
        EquipmentEffectType type)
    {
        float value = 0f;
        foreach (EquipmentEffectDefinition effect in
                 mercenary.GetActiveEquipmentEffects())
        {
            if (effect != null && effect.type == type)
            {
                value = Mathf.Max(value, effect.secondaryValue);
            }
        }
        return value;
    }

    private IEnumerator WaitForActionDelay()
    {
        while (IsBattling && IsPaused && !skipToBattleEndRequested)
        {
            yield return null;
        }

        float remaining =
            actionDelay / Mathf.Max(1f, BattleSpeedMultiplier);
        while (remaining > 0f && !skipToBattleEndRequested)
        {
            while (IsBattling && IsPaused && !skipToBattleEndRequested)
            {
                yield return null;
            }

            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        while (IsBattling && IsPaused && !skipToBattleEndRequested)
        {
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
        RaisePresentation(new BattlePresentationEvent(
            BattlePresentationEventType.Action,
            attacker,
            target,
            actionKind: BattlePresentationActionKind.NormalAttack));

        if (target.TryEvade())
        {
            RaisePresentation(new BattlePresentationEvent(
                BattlePresentationEventType.Evade,
                attacker,
                target,
                currentHP: target.CurrentHP,
                maxHP: target.MaxHP));
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
        DamageResolver.ResolveDamage(new DamageRequest(
            rawDamage,
            DamageType.Physical,
            critical,
            attacker,
            target,
            true));
        if (attacker.IsPlayerSide && target.Race != EnemyRace.Unknown &&
            attacker.GetRaceDamageMultiplier(target.Race) > 1f)
        {
            SendBattleMessage(
                $"{JapaneseDisplayText.GetEnemyRace(target.Race)}特攻が発動！",
                BattleLogType.Player);
        }
        int damageDealt = previousHP - target.CurrentHP;

        RaisePresentation(new BattlePresentationEvent(
            BattlePresentationEventType.Damage,
            attacker,
            target,
            damageDealt,
            target.CurrentHP,
            target.MaxHP,
            critical));
        if (target.IsDead)
        {
            RaisePresentation(new BattlePresentationEvent(
                BattlePresentationEventType.Defeated,
                attacker,
                target,
                currentHP: target.CurrentHP,
                maxHP: target.MaxHP));
        }

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

    public bool ToggleBattlePause()
    {
        if (!IsBattling)
        {
            return false;
        }

        IsPaused = !IsPaused;
        return IsPaused;
    }

    public bool RequestSkipToBattleEnd()
    {
        if (!IsBattling || skipToBattleEndRequested)
        {
            return false;
        }

        skipToBattleEndRequested = true;
        IsPaused = false;
        RaisePresentation(new BattlePresentationEvent(
            BattlePresentationEventType.SkipRequested));
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
        IsPaused = false;
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
        RaisePresentation(new BattlePresentationEvent(
            BattlePresentationEventType.BattleCompleted,
            victory: victory));
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

    private void PrepareBattleVisuals()
    {
        List<BattleVisualUnitDescriptor> players =
            new List<BattleVisualUnitDescriptor>();
        for (int i = 0; i < playerUnits.Count; i++)
        {
            BattleUnit unit = playerUnits[i];
            players.Add(new BattleVisualUnitDescriptor(
                unit,
                null,
                unit.MercenaryClass));
        }

        List<BattleVisualUnitDescriptor> enemies =
            new List<BattleVisualUnitDescriptor>();
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            EnemyDataSO source = i < battleEnemyData.Count
                ? battleEnemyData[i]
                : null;
            enemies.Add(new BattleVisualUnitDescriptor(
                enemyUnits[i],
                source,
                MercenaryClass.Warrior));
        }

        Sprite background = nextBattleBackground != null
            ? nextBattleBackground
            : defaultBattleBackground;
        string backgroundKey = !string.IsNullOrWhiteSpace(
            nextBattleBackgroundKey)
            ? nextBattleBackgroundKey
            : defaultBattleBackgroundKey;
        BattleVisualsPrepared?.Invoke(
            new BattlePresentationRoster(
                players,
                enemies,
                background,
                backgroundKey));
        nextBattleBackground = null;
        nextBattleBackgroundKey = null;
    }

    private Dictionary<BattleUnit, BattleUnitPresentationState>
        CapturePresentationStates()
    {
        Dictionary<BattleUnit, BattleUnitPresentationState> states =
            new Dictionary<BattleUnit, BattleUnitPresentationState>();
        foreach (BattleUnit unit in playerUnits)
        {
            states[unit] = new BattleUnitPresentationState(unit);
        }
        foreach (BattleUnit unit in enemyUnits)
        {
            states[unit] = new BattleUnitPresentationState(unit);
        }
        return states;
    }

    private void RaiseStateChangeEvents(
        BattleUnit actor,
        IReadOnlyDictionary<BattleUnit, BattleUnitPresentationState> before)
    {
        foreach (KeyValuePair<BattleUnit, BattleUnitPresentationState> pair in before)
        {
            BattleUnit unit = pair.Key;
            BattleUnitPresentationState previous = pair.Value;
            int hpChange = unit.CurrentHP - previous.CurrentHP;
            if (hpChange < 0)
            {
                RaisePresentation(new BattlePresentationEvent(
                    BattlePresentationEventType.Damage,
                    actor,
                    unit,
                    -hpChange,
                    unit.CurrentHP,
                    unit.MaxHP));
            }
            else if (hpChange > 0)
            {
                RaisePresentation(new BattlePresentationEvent(
                    BattlePresentationEventType.Heal,
                    actor,
                    unit,
                    hpChange,
                    unit.CurrentHP,
                    unit.MaxHP));
            }

            if (unit.StatusEffect != previous.StatusEffect)
            {
                RaisePresentation(new BattlePresentationEvent(
                    BattlePresentationEventType.Status,
                    actor,
                    unit,
                    currentHP: unit.CurrentHP,
                    maxHP: unit.MaxHP,
                    statusEffect: unit.StatusEffect));
            }

            if (!previous.IsDead && unit.IsDead)
            {
                RaisePresentation(new BattlePresentationEvent(
                    BattlePresentationEventType.Defeated,
                    actor,
                    unit,
                    currentHP: unit.CurrentHP,
                    maxHP: unit.MaxHP));
            }
        }
    }

    private void RaisePresentation(BattlePresentationEvent presentationEvent)
    {
        BattlePresentation?.Invoke(presentationEvent);
    }

    private void ClearNextBattleBackground()
    {
        nextBattleBackground = null;
        nextBattleBackgroundKey = null;
    }

    private readonly struct BattleUnitPresentationState
    {
        public int CurrentHP { get; }
        public BattleStatusEffect StatusEffect { get; }
        public bool IsDead { get; }

        public BattleUnitPresentationState(BattleUnit unit)
        {
            CurrentHP = unit.CurrentHP;
            StatusEffect = unit.StatusEffect;
            IsDead = unit.IsDead;
        }
    }
}
