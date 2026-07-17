using UnityEngine;

public class BattleUnit
{
    public string UnitName { get; private set; }
    public bool IsPlayerSide { get; private set; }
    public MercenaryClass MercenaryClass { get; private set; }
    public int Level { get; private set; }

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxMagicPower { get; private set; }
    public int CurrentMagicPower { get; private set; }

    public int Attack => Mathf.RoundToInt(baseAttack * (1f + attackBonusPercent));
    public int Defense => Mathf.RoundToInt(baseDefense * (1f + defenseBonusPercent));
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

    public bool IsDead => CurrentHP <= 0;
    public bool IsTaunting => !IsDead && TauntTurns > 0;
    public int EffectiveDefense => Defense;
    public float AttackBonusPercent => attackBonusPercent;
    public float DefenseBonusPercent => defenseBonusPercent;
    public float SpeedBonusPercent => speedBonusPercent;
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
        int level = 1)
    {
        UnitName = unitName;
        IsPlayerSide = isPlayerSide;
        MercenaryClass = mercenaryClass;
        Level = Mathf.Max(1, level);
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
        int finalDamage = Mathf.Max(1, damage - EffectiveDefense);
        CurrentHP -= finalDamage;
        CurrentHP = Mathf.Max(0, CurrentHP);
    }

    public void TakePureDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(1, damage));
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
        TakePureDamage(damage);
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
        return Mathf.Max(1, damage - EffectiveDefense);
    }

    public int CalculateDamage()
    {
        return Attack;
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

    public void BoostAttackForBattle(float percent)
    {
        attackBonusPercent = Mathf.Max(attackBonusPercent, Mathf.Max(0f, percent));
    }

    public void BoostDefenseForBattle(float percent)
    {
        defenseBonusPercent = Mathf.Max(defenseBonusPercent, Mathf.Max(0f, percent));
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
    }
}
