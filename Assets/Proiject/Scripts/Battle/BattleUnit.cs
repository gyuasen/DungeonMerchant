using UnityEngine;

public class BattleUnit
{
    public string UnitName { get; private set; }
    public bool IsPlayerSide { get; private set; }
    public MercenaryClass MercenaryClass { get; private set; }

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxMagicPower { get; private set; }
    public int CurrentMagicPower { get; private set; }

    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public float AttackSpeed { get; private set; }
    public int TauntTurns { get; private set; }

    public bool IsDead => CurrentHP <= 0;
    public bool IsTaunting => !IsDead && TauntTurns > 0;
    public int EffectiveDefense => Defense;
    public string StatusSummary => IsTaunting ? "挑発" : "なし";

    public BattleUnit(
        string unitName,
        int maxHP,
        int currentHP,
        int attack,
        int defense,
        float attackSpeed,
        bool isPlayerSide,
        MercenaryClass mercenaryClass = MercenaryClass.Warrior,
        int maxMagicPower = 0)
    {
        UnitName = unitName;
        IsPlayerSide = isPlayerSide;
        MercenaryClass = mercenaryClass;
        MaxHP = maxHP;
        CurrentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Attack = attack;
        Defense = defense;
        AttackSpeed = attackSpeed;
        MaxMagicPower = Mathf.Max(0, maxMagicPower);
        CurrentMagicPower = isPlayerSide
            ? Mathf.Min(MaxMagicPower, 20)
            : 0;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, damage - EffectiveDefense);
        CurrentHP -= finalDamage;
        CurrentHP = Mathf.Max(0, CurrentHP);
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

    }
}
