public enum BattleStatusEffectOutcome
{
    None,
    ActionSkipped,
    UnitDied
}

public sealed class BattleStatusEffectResult
{
    public BattleStatusEffectOutcome Outcome { get; }
    public string LogMessage { get; }
    public BattleLogType LogType { get; }
    public int PoisonDamage { get; }

    public bool IsActionSkipped => Outcome == BattleStatusEffectOutcome.ActionSkipped;
    public bool IsUnitDead => Outcome == BattleStatusEffectOutcome.UnitDied;

    private BattleStatusEffectResult(
        BattleStatusEffectOutcome outcome,
        string logMessage,
        BattleLogType logType,
        int poisonDamage)
    {
        Outcome = outcome;
        LogMessage = logMessage;
        LogType = logType;
        PoisonDamage = poisonDamage;
    }

    public static BattleStatusEffectResult NoEffect()
    {
        return new BattleStatusEffectResult(
            BattleStatusEffectOutcome.None, null, BattleLogType.System, 0);
    }

    public static BattleStatusEffectResult Poison(
        BattleUnit unit, int damage)
    {
        return new BattleStatusEffectResult(
            unit.IsDead
                ? BattleStatusEffectOutcome.UnitDied
                : BattleStatusEffectOutcome.None,
            BattleLogFormatter.FormatPoisonDamage(
                unit.UnitName, damage, unit.CurrentHP, unit.MaxHP),
            unit.IsPlayerSide ? BattleLogType.Enemy : BattleLogType.Player,
            damage);
    }

    public static BattleStatusEffectResult Paralysis(BattleUnit unit)
    {
        return new BattleStatusEffectResult(
            BattleStatusEffectOutcome.ActionSkipped,
            BattleLogFormatter.FormatParalysis(unit.UnitName),
            BattleLogType.System,
            0);
    }
}

/// <summary>
/// Applies battle status effects at the same points as BattleRoutine did.
/// This service contains no Unity lifecycle or coroutine dependency.
/// </summary>
public sealed class BattleStatusEffectService
{
    public BattleStatusEffectResult ProcessActionStart(BattleUnit unit)
    {
        if (unit == null || unit.IsDead)
        {
            return BattleStatusEffectResult.NoEffect();
        }

        int poisonDamage = unit.ProcessPoisonDamage();
        if (poisonDamage > 0)
        {
            return BattleStatusEffectResult.Poison(unit, poisonDamage);
        }

        if (unit.ConsumeParalysisTurn())
        {
            unit.TickStatuses();
            return BattleStatusEffectResult.Paralysis(unit);
        }

        return BattleStatusEffectResult.NoEffect();
    }

    public void TickAfterAction(BattleUnit unit)
    {
        if (unit == null)
        {
            return;
        }

        unit.TickStatuses();
    }
}
