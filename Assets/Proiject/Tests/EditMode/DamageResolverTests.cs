using NUnit.Framework;
using System.Collections.Generic;

public sealed class DamageResolverTests
{
    [TestCase(0, 100)]
    [TestCase(50, 67)]
    [TestCase(100, 50)]
    [TestCase(200, 33)]
    public void ResolveDamage_PreservesFixedDefenseMitigation(
        int defense,
        int expectedDamage)
    {
        BattleUnit target = CreateTarget(defense);
        DamageRequest request = new DamageRequest(
            100,
            DamageType.Physical,
            false,
            null,
            target);
        DamageResult result = DamageResolver.ResolveDamage(request);
        Assert.That(result.FinalDamage, Is.EqualTo(expectedDamage));
        Assert.That(target.CurrentHP, Is.EqualTo(500 - expectedDamage));
    }

    [Test]
    public void PreviewDamage_MatchesResolvedDamageForDamageReduction()
    {
        BattleUnit target = CreateTarget(100);
        target.ApplyEquipmentEffects(new BattleEquipmentEffectSnapshot(
            1f,
            0f,
            0f,
            0.8f,
            0,
            0f,
            0,
            0f,
            0));
        DamageRequest request = new DamageRequest(
            100,
            DamageType.Magic,
            true,
            null,
            target);
        int preview = DamageResolver.PreviewDamage(request);
        DamageResult result = DamageResolver.ResolveDamage(request);
        Assert.That(preview, Is.EqualTo(40));
        Assert.That(result.FinalDamage, Is.EqualTo(preview));
    }

    [Test]
    public void ResolveDamage_PureIgnoresDefenseAndDamageReduction()
    {
        BattleUnit target = CreateTarget(200);
        target.ApplyEquipmentEffects(new BattleEquipmentEffectSnapshot(
            1f,
            0f,
            0f,
            0.7f,
            0,
            0f,
            0,
            0f,
            0));
        DamageResult result = DamageResolver.ResolveDamage(new DamageRequest(
            30,
            DamageType.Pure,
            false,
            null,
            target));
        Assert.That(result.FinalDamage, Is.EqualTo(30));
        Assert.That(target.CurrentHP, Is.EqualTo(470));
    }

    [Test]
    public void ResolveDamage_AppliesRaceBonusBeforeDefenseMitigation()
    {
        BattleUnit attacker = new BattleUnit("Attacker", 100, 100, 10, 0, 1f, true);
        attacker.ApplyEquipmentEffects(new BattleEquipmentEffectSnapshot(
            1f, 0f, 0f, 1f, 0, 0f, 0, 0f, 0,
            new Dictionary<EnemyRace, float> { { EnemyRace.Dragon, 1.5f } }));
        BattleUnit target = new BattleUnit("Dragon", 500, 500, 1, 100, 1f, false,
            race: EnemyRace.Dragon);
        DamageRequest request = new DamageRequest(100, DamageType.Physical,
            false, attacker, target, true);
        Assert.That(DamageResolver.PreviewDamage(request), Is.EqualTo(75));
        Assert.That(DamageResolver.ResolveDamage(request).FinalDamage, Is.EqualTo(75));
    }

    private static BattleUnit CreateTarget(int defense)
    {
        return new BattleUnit("Target", 500, 500, 1, defense, 1f, false);
    }
}
