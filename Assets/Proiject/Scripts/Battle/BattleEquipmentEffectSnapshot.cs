using System.Collections.Generic;
using UnityEngine;

public readonly struct BattleEquipmentEffectSnapshot
{
    public readonly float DamageMultiplier;
    public readonly float LowHpDamageBonus;
    public readonly float LowHpThreshold;
    public readonly float DamageTakenMultiplier;
    public readonly int TurnRegenerationAmount;
    public readonly float BattleStartAttackBuffPercent;
    public readonly int BattleStartAttackBuffTurns;
    public readonly float BattleStartDefenseBuffPercent;
    public readonly int BattleStartDefenseBuffTurns;
    public readonly IReadOnlyDictionary<EnemyRace, float> RaceDamageMultipliers;

    public BattleEquipmentEffectSnapshot(float damageMultiplier, float lowHpDamageBonus, float lowHpThreshold, float damageTakenMultiplier, int turnRegenerationAmount, float battleStartAttackBuffPercent, int battleStartAttackBuffTurns, float battleStartDefenseBuffPercent, int battleStartDefenseBuffTurns, IReadOnlyDictionary<EnemyRace, float> raceDamageMultipliers = null)
    {
        DamageMultiplier = Mathf.Max(0f, damageMultiplier);
        LowHpDamageBonus = Mathf.Max(0f, lowHpDamageBonus);
        LowHpThreshold = Mathf.Clamp01(lowHpThreshold);
        DamageTakenMultiplier = Mathf.Clamp01(damageTakenMultiplier);
        TurnRegenerationAmount = Mathf.Max(0, turnRegenerationAmount);
        BattleStartAttackBuffPercent = Mathf.Max(0f, battleStartAttackBuffPercent);
        BattleStartAttackBuffTurns = Mathf.Max(0, battleStartAttackBuffTurns);
        BattleStartDefenseBuffPercent = Mathf.Max(0f, battleStartDefenseBuffPercent);
        BattleStartDefenseBuffTurns = Mathf.Max(0, battleStartDefenseBuffTurns);
        RaceDamageMultipliers = raceDamageMultipliers ?? new Dictionary<EnemyRace, float>();
    }
}
