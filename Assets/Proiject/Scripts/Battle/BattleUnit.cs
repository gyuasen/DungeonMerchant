using UnityEngine;

public class BattleUnit
{
    public string UnitName { get; private set; }
    public bool IsPlayerSide { get; private set; }

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public float AttackSpeed { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    public BattleUnit(
        string unitName,
        int maxHP,
        int currentHP,
        int attack,
        int defense,
        float attackSpeed,
        bool isPlayerSide)
    {
        UnitName = unitName;
        IsPlayerSide = isPlayerSide;
        MaxHP = maxHP;
        CurrentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Attack = attack;
        Defense = defense;
        AttackSpeed = attackSpeed;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, damage - Defense);
        CurrentHP -= finalDamage;
        CurrentHP = Mathf.Max(0, CurrentHP);
    }

    public int CalculateDamage()
    {
        return Attack;
    }
}
