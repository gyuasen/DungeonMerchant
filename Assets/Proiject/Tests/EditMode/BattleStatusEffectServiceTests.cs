using NUnit.Framework;

public sealed class BattleStatusEffectServiceTests
{
    private readonly BattleStatusEffectService service =
        new BattleStatusEffectService();

    [Test]
    public void ProcessActionStart_PoisonPreservesDamageLogTypeAndTiming()
    {
        BattleUnit unit = Unit("毒対象", 100, 100, false);
        unit.ApplyStatus(BattleStatusEffect.Poison, 2);

        BattleStatusEffectResult result = service.ProcessActionStart(unit);

        Assert.That(result.Outcome, Is.EqualTo(BattleStatusEffectOutcome.None));
        Assert.That(result.PoisonDamage, Is.EqualTo(6));
        Assert.That(unit.CurrentHP, Is.EqualTo(94));
        Assert.That(unit.StatusTurns, Is.EqualTo(1));
        Assert.That(result.LogType, Is.EqualTo(BattleLogType.Player));
        Assert.That(result.LogMessage,
            Is.EqualTo(BattleLogFormatter.FormatPoisonDamage("毒対象", 6, 94, 100)));
    }

    [Test]
    public void ProcessActionStart_PoisonDeathReturnsDeathWithoutTickingOtherStatuses()
    {
        BattleUnit unit = Unit("致死毒", 20, 1, true);
        unit.ApplyStatus(BattleStatusEffect.Poison, 1);
        unit.StartTaunt(2);

        BattleStatusEffectResult result = service.ProcessActionStart(unit);

        Assert.That(result.Outcome, Is.EqualTo(BattleStatusEffectOutcome.UnitDied));
        Assert.That(result.IsUnitDead, Is.True);
        Assert.That(unit.CurrentHP, Is.EqualTo(0));
        Assert.That(unit.TauntTurns, Is.EqualTo(2));
        Assert.That(unit.StatusEffect, Is.EqualTo(BattleStatusEffect.None));
    }

    [Test]
    public void ProcessActionStart_ParalysisSkipsAndTicksStatusesImmediately()
    {
        BattleUnit unit = Unit("麻痺対象", 100, 100, true);
        unit.ApplyStatus(BattleStatusEffect.Paralysis, 1);
        unit.StartTaunt(2);

        BattleStatusEffectResult result = service.ProcessActionStart(unit);

        Assert.That(result.Outcome, Is.EqualTo(BattleStatusEffectOutcome.ActionSkipped));
        Assert.That(result.IsActionSkipped, Is.True);
        Assert.That(unit.StatusEffect, Is.EqualTo(BattleStatusEffect.None));
        Assert.That(unit.StatusTurns, Is.EqualTo(0));
        Assert.That(unit.TauntTurns, Is.EqualTo(1));
        Assert.That(result.LogType, Is.EqualTo(BattleLogType.System));
        Assert.That(result.LogMessage,
            Is.EqualTo(BattleLogFormatter.FormatParalysis("麻痺対象")));
    }

    [Test]
    public void TickAfterAction_LeavesStartOfTurnProcessingUntouched()
    {
        BattleUnit unit = Unit("通常行動", 100, 100, true);
        unit.StartTaunt(2);

        service.TickAfterAction(unit);

        Assert.That(unit.TauntTurns, Is.EqualTo(1));
    }

    private static BattleUnit Unit(
        string name, int maxHp, int currentHp, bool player)
    {
        return new BattleUnit(name, maxHp, currentHp, 10, 0, 1f, player);
    }
}
