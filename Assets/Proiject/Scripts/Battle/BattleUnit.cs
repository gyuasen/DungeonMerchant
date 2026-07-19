using UnityEngine;

public class BattleUnit
{
    public string UnitName { get; private set; }
    public bool IsPlayerSide { get; private set; }
    public MercenaryClass MercenaryClass { get; private set; }
    public int Level { get; private set; }
    public EnemyRace Race { get; private set; }

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxMagicPower { get; private set; }
    public int CurrentMagicPower { get; private set; }

    public int Attack => Mathf.RoundToInt(baseAttack * (1f + GetAttackBonusPercent()));
    public int Defense => Mathf.RoundToInt(baseDefense * (1f + GetDefenseBonusPercent()));
    public float AttackSpeed => baseAttackSpeed * (1f + speedBonusPercent);
    public float CriticalRate { get; private set; }
    public float EvasionRate { get; private set; }
    public int TauntTurns { get; private set; }
    public BattleStatusEffect StatusEffect { get; private set; }
    public int StatusTurns { get; private set; }
    private float criticalRateBonus;
    private int criticalRateBonusTurns;
    private readonly int baseAttack;
    private readonly int baseDefense;
    private readonly float baseAttackSpeed;
    private float attackBonusPercent;
    private float defenseBonusPercent;
    private float speedBonusPercent;
    private int enemyHealCooldownTurns;
    private float equipmentAttackBonusPercent;
    private int equipmentAttackBonusTurns;
    private float equipmentDefenseBonusPercent;
    private int equipmentDefenseBonusTurns;
    private float equipmentDamageMultiplier = 1f;
    private float equipmentLowHpDamageBonus;
    private float equipmentLowHpThreshold;
    private float equipmentDamageTakenMultiplier = 1f;
    private int equipmentTurnRegenerationAmount;
    private System.Collections.Generic.IReadOnlyDictionary<EnemyRace, float> raceDamageMultipliers =
        new System.Collections.Generic.Dictionary<EnemyRace, float>();

    public bool IsDead => CurrentHP <= 0;
    public bool IsTaunting => !IsDead && TauntTurns > 0;
    public int EffectiveDefense => Defense;
    public float AttackBonusPercent => GetAttackBonusPercent();
    public float DefenseBonusPercent => GetDefenseBonusPercent();
    public float SpeedBonusPercent => speedBonusPercent;
    internal float DamageTakenMultiplier => equipmentDamageTakenMultiplier;
    public string StatusSummary
    {
        get
        {
            string status = StatusEffect == BattleStatusEffect.Poison
                ? "毒"
                : StatusEffect == BattleStatusEffect.Paralysis
                    ? "麻痺"
                    : string.Empty;
            if (IsTaunting)
            {
                status = string.IsNullOrEmpty(status)
                    ? "挑発"
                    : $"{status}・挑発";
            }
            return string.IsNullOrEmpty(status) ? "なし" : status;
        }
    }

    public BattleUnit(
        string unitName,
        int maxHP,
        int currentHP,
        int attack,
        int defense,
        float attackSpeed,
        bool isPlayerSide,
        MercenaryClass mercenaryClass = MercenaryClass.Warrior,
        int maxMagicPower = 0,
        float criticalRate = 0f,
        float evasionRate = 0f,
        BattleStatusEffect initialStatus = BattleStatusEffect.None,
        int level = 1,
        EnemyRace race = EnemyRace.Unknown)
    {
        UnitName = unitName;
        IsPlayerSide = isPlayerSide;
        MercenaryClass = mercenaryClass;
        Level = Mathf.Max(1, level);
        Race = race;
        MaxHP = maxHP;
        CurrentHP = Mathf.Clamp(currentHP, 0, maxHP);
        baseAttack = attack;
        baseDefense = defense;
        baseAttackSpeed = attackSpeed;
        CriticalRate = Mathf.Clamp(criticalRate, 0f, 0.75f);
        EvasionRate = Mathf.Clamp(evasionRate, 0f, 0.75f);
        MaxMagicPower = Mathf.Max(0, maxMagicPower);
        CurrentMagicPower = isPlayerSide
            ? Mathf.Min(MaxMagicPower, 20)
            : 0;
        if (initialStatus != BattleStatusEffect.None)
        {
            ApplyStatus(initialStatus, initialStatus == BattleStatusEffect.Poison ? 3 : 1);
        }
    }

    public void TakeDamage(int damage)
    {
        DamageResolver.ResolveDamage(new DamageRequest(
            damage,
            DamageType.Physical,
            false,
            null,
            this));
    }

    public void TakePureDamage(int damage)
    {
        DamageResolver.ResolveDamage(new DamageRequest(
            damage,
            DamageType.Pure,
            false,
            null,
            this));
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(
            MaxHP,
            CurrentHP + Mathf.Max(0, amount));
    }

    public bool TryEvade()
    {
        return !IsDead && UnityEngine.Random.value < EvasionRate;
    }

    public bool RollCritical()
    {
        return UnityEngine.Random.value <
               Mathf.Clamp(CriticalRate + criticalRateBonus, 0f, 0.9f);
    }

    public void BoostCriticalRate(float amount, int turns)
    {
        criticalRateBonus = Mathf.Max(criticalRateBonus, Mathf.Max(0f, amount));
        criticalRateBonusTurns = Mathf.Max(criticalRateBonusTurns, turns);
    }

    public void ApplyStatus(BattleStatusEffect effect, int turns)
    {
        if (effect == BattleStatusEffect.None || IsDead)
        {
            return;
        }

        StatusEffect = effect;
        StatusTurns = Mathf.Max(StatusTurns, Mathf.Max(1, turns));
    }

    public int ProcessPoisonDamage()
    {
        if (StatusEffect != BattleStatusEffect.Poison || StatusTurns <= 0)
        {
            return 0;
        }

        int damage = Mathf.Max(1, Mathf.RoundToInt(MaxHP * 0.06f));
        DamageResolver.ResolveDamage(new DamageRequest(
            damage,
            DamageType.Periodic,
            false,
            null,
            this));
        StatusTurns--;
        if (StatusTurns <= 0)
        {
            StatusEffect = BattleStatusEffect.None;
        }
        return damage;
    }

    public bool ConsumeParalysisTurn()
    {
        if (StatusEffect != BattleStatusEffect.Paralysis || StatusTurns <= 0)
        {
            return false;
        }

        StatusTurns--;
        if (StatusTurns <= 0)
        {
            StatusEffect = BattleStatusEffect.None;
        }
        return true;
    }

    public bool CureStatus(BattleStatusEffect effect)
    {
        if (StatusEffect == BattleStatusEffect.None ||
            (effect != BattleStatusEffect.None && StatusEffect != effect))
        {
            return false;
        }

        StatusEffect = BattleStatusEffect.None;
        StatusTurns = 0;
        return true;
    }

    public int EstimateDamageTaken(int damage)
    {
        return DamageResolver.PreviewDamage(new DamageRequest(
            damage,
            DamageType.Physical,
            false,
            null,
            this));
    }

    internal void ApplyResolvedDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }

    public int CalculateDamage()
    {
        float multiplier = equipmentDamageMultiplier;
        // Compare with integer cross-multiplication to avoid float division
        // rounding making CurrentHP/MaxHP dip just under an equal threshold
        // (e.g. 30/100 rendered slightly below 0.30f). Activates strictly
        // below the threshold, never exactly at it.
        if (MaxHP > 0 &&
            CurrentHP * 1000L < Mathf.RoundToInt(equipmentLowHpThreshold * 1000f) * (long)MaxHP)
        {
            multiplier *= 1f + equipmentLowHpDamageBonus;
        }
        return Mathf.Max(1, Mathf.RoundToInt(Attack * multiplier));
    }

    public void GainMagicPower(int amount)
    {
        if (MaxMagicPower <= 0 || amount <= 0)
        {
            return;
        }

        CurrentMagicPower =
            Mathf.Min(MaxMagicPower, CurrentMagicPower + amount);
    }

    public float GetRaceDamageMultiplier(EnemyRace targetRace)
    {
        if (targetRace == EnemyRace.Unknown || raceDamageMultipliers == null)
        {
            return 1f;
        }
        float multiplier;
        return raceDamageMultipliers.TryGetValue(targetRace, out multiplier)
            ? multiplier
            : 1f;
    }

    public void BoostAttackForBattle(float percent)
    {
        attackBonusPercent = Mathf.Max(attackBonusPercent, Mathf.Max(0f, percent));
    }

    public void BoostAttackForBattle(float percent, int turns)
    {
        if (turns <= 0)
        {
            BoostAttackForBattle(percent);
            return;
        }
        equipmentAttackBonusPercent = Mathf.Max(equipmentAttackBonusPercent, Mathf.Max(0f, percent));
        equipmentAttackBonusTurns = Mathf.Max(equipmentAttackBonusTurns, turns);
    }

    public void BoostDefenseForBattle(float percent)
    {
        defenseBonusPercent = Mathf.Max(defenseBonusPercent, Mathf.Max(0f, percent));
    }

    public void BoostDefenseForBattle(float percent, int turns)
    {
        if (turns <= 0)
        {
            BoostDefenseForBattle(percent);
            return;
        }
        equipmentDefenseBonusPercent = Mathf.Max(equipmentDefenseBonusPercent, Mathf.Max(0f, percent));
        equipmentDefenseBonusTurns = Mathf.Max(equipmentDefenseBonusTurns, turns);
    }

    public void ApplyEquipmentEffects(BattleEquipmentEffectSnapshot effects)
    {
        equipmentDamageMultiplier = effects.DamageMultiplier;
        equipmentLowHpDamageBonus = effects.LowHpDamageBonus;
        equipmentLowHpThreshold = effects.LowHpThreshold;
        equipmentDamageTakenMultiplier = effects.DamageTakenMultiplier;
        equipmentTurnRegenerationAmount = effects.TurnRegenerationAmount;
        raceDamageMultipliers = effects.RaceDamageMultipliers;
        if (effects.BattleStartAttackBuffTurns > 0)
        {
            BoostAttackForBattle(effects.BattleStartAttackBuffPercent, effects.BattleStartAttackBuffTurns);
        }
        if (effects.BattleStartDefenseBuffTurns > 0)
        {
            BoostDefenseForBattle(effects.BattleStartDefenseBuffPercent, effects.BattleStartDefenseBuffTurns);
        }
    }

    public int ProcessEquipmentTurnRegeneration()
    {
        if (IsDead || equipmentTurnRegenerationAmount <= 0)
        {
            return 0;
        }
        int before = CurrentHP;
        Heal(equipmentTurnRegenerationAmount);
        return CurrentHP - before;
    }

    public void BoostSpeedForBattle(float percent)
    {
        speedBonusPercent = Mathf.Max(speedBonusPercent, Mathf.Max(0f, percent));
    }

    public bool TryConsumeMagicPower(int cost)
    {
        if (cost <= 0)
        {
            return true;
        }

        if (CurrentMagicPower < cost)
        {
            return false;
        }

        CurrentMagicPower -= cost;
        return true;
    }

    public void StartTaunt(int turns)
    {
        TauntTurns = Mathf.Max(TauntTurns, turns);
    }

    public void TickStatuses()
    {
        if (enemyHealCooldownTurns > 0)
        {
            enemyHealCooldownTurns--;
        }

        if (TauntTurns > 0)
        {
            TauntTurns--;
        }

        if (criticalRateBonusTurns > 0)
        {
            criticalRateBonusTurns--;
            if (criticalRateBonusTurns <= 0)
            {
                criticalRateBonus = 0f;
            }
        }

        if (equipmentAttackBonusTurns > 0)
        {
            equipmentAttackBonusTurns--;
            if (equipmentAttackBonusTurns <= 0)
            {
                equipmentAttackBonusPercent = 0f;
            }
        }
        if (equipmentDefenseBonusTurns > 0)
        {
            equipmentDefenseBonusTurns--;
            if (equipmentDefenseBonusTurns <= 0)
            {
                equipmentDefenseBonusPercent = 0f;
            }
        }
    }

    public bool CanUseEnemyHeal()
    {
        return enemyHealCooldownTurns <= 0;
    }

    public void StartEnemyHealCooldown(int turns)
    {
        enemyHealCooldownTurns = Mathf.Max(enemyHealCooldownTurns, Mathf.Max(0, turns));
    }

    private float GetAttackBonusPercent()
    {
        return Mathf.Max(attackBonusPercent, equipmentAttackBonusPercent);
    }

    private float GetDefenseBonusPercent()
    {
        return Mathf.Max(defenseBonusPercent, equipmentDefenseBonusPercent);
    }
}
