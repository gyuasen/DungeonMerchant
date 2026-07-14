using System;
using System.Collections.Generic;
using NUnit.Framework;

public sealed class BattleSkillResolverTests
{
    [Test]
    public void BaseClasses_HaveTwoCombatSkillsWithDistinctRoles()
    {
        HashSet<MercenarySkillId> addedSkillIds =
            new HashSet<MercenarySkillId>();
        foreach (MercenaryClass baseClass in MercenaryClassProgression.GetBaseClasses())
        {
            List<MercenarySkillDefinition> skills =
                MercenaryClassProgression.GetCombatSkills(baseClass);
            Assert.That(skills.Count, Is.GreaterThanOrEqualTo(3), baseClass.ToString());
            Assert.That(skills[0].Id, Is.Not.EqualTo(skills[1].Id));
            Assert.That(skills[1].Description, Is.Not.Empty);
            Assert.That(addedSkillIds.Add(skills[1].Id), Is.True, skills[1].Name);
            Assert.That(addedSkillIds.Add(skills[2].Id), Is.True, skills[2].Name);
        }
        Assert.That(addedSkillIds.Count, Is.EqualTo(12));
    }

    [TestCase(MercenaryClass.Warlord, MercenarySkillId.WarlordCommand, 20)]
    [TestCase(MercenaryClass.Beastmaster, MercenarySkillId.BeastPack, 24)]
    [TestCase(MercenaryClass.Chronomancer, MercenarySkillId.TimeLock, 28)]
    [TestCase(MercenaryClass.Saint, MercenarySkillId.SaintsGrace, 32)]
    [TestCase(MercenaryClass.Shadow, MercenarySkillId.ShadowRend, 36)]
    [TestCase(MercenaryClass.DragonKnight, MercenarySkillId.DragonBreath, 40)]
    public void SpecialClasses_HaveUniqueCombatSkillAtConfiguredLevel(
        MercenaryClass mercenaryClass,
        MercenarySkillId expectedSkill,
        int expectedUnlockLevel)
    {
        List<MercenarySkillDefinition> skills =
            MercenaryClassProgression.GetCombatSkills(mercenaryClass);
        MercenarySkillDefinition specialSkill = skills.Find(
            skill => skill.Id == expectedSkill);

        Assert.That(specialSkill, Is.Not.Null);
        Assert.That(specialSkill.UnlockLevel, Is.EqualTo(expectedUnlockLevel));
        Assert.That(specialSkill.MagicCost, Is.GreaterThan(0));
        Assert.That(specialSkill.Description, Is.Not.Empty);
    }

    [TestCase(MercenaryClass.Warlord, MercenarySkillId.WarlordCommand)]
    [TestCase(MercenaryClass.Beastmaster, MercenarySkillId.BeastPack)]
    [TestCase(MercenaryClass.Chronomancer, MercenarySkillId.TimeLock)]
    [TestCase(MercenaryClass.Saint, MercenarySkillId.SaintsGrace)]
    [TestCase(MercenaryClass.Shadow, MercenarySkillId.ShadowRend)]
    [TestCase(MercenaryClass.DragonKnight, MercenarySkillId.DragonBreath)]
    public void SpecialClassSkill_IsSelectedConsumesMagicAndWritesLog(
        MercenaryClass mercenaryClass,
        MercenarySkillId expectedSkill)
    {
        List<string> logs = new List<string>();
        BattleUnit attacker = Unit(
            "specialist", 120, 120, 20, 0, true,
            mercenaryClass, 100, 50);
        attacker.GainMagicPower(80);
        BattleUnit primaryTarget = Unit("target", 500, 500, 4, 0, false);
        BattleUnit secondaryTarget = Unit("secondary", 500, 500, 4, 0, false);
        BattleUnit ally = Unit("ally", 120, 40, 10, 0, true);
        int randomCall = 0;
        BattleSkillResolver resolver = new BattleSkillResolver(
            new BattleSkillResolverContext(
                new List<BattleUnit> { attacker, ally },
                new List<BattleUnit> { primaryTarget, secondaryTarget },
                1f,
                0f,
                1.5f,
                () => randomCall++ == 0 ? 0f : 0.6f,
                (message, _) => logs.Add(message)));
        MercenarySkillDefinition skill =
            MercenaryClassProgression.GetCombatSkills(mercenaryClass).Find(
                definition => definition.Id == expectedSkill);

        Assert.That(resolver.TryUsePlayerSkill(attacker, primaryTarget), Is.True);
        Assert.That(attacker.CurrentMagicPower,
            Is.EqualTo(attacker.MaxMagicPower - skill.MagicCost));
        Assert.That(logs.Exists(message => message.Contains(skill.Name)), Is.True);

        switch (expectedSkill)
        {
            case MercenarySkillId.BeastPack:
            case MercenarySkillId.DragonBreath:
                Assert.That(secondaryTarget.CurrentHP, Is.LessThan(secondaryTarget.MaxHP));
                break;
            case MercenarySkillId.TimeLock:
                Assert.That(primaryTarget.StatusEffect,
                    Is.EqualTo(BattleStatusEffect.Paralysis));
                break;
            case MercenarySkillId.SaintsGrace:
                Assert.That(ally.CurrentHP, Is.GreaterThan(40));
                break;
            default:
                Assert.That(primaryTarget.CurrentHP, Is.LessThan(primaryTarget.MaxHP));
                break;
        }
    }

    [Test]
    public void SpecialClassSkill_IsNotSelectedBeforeUnlockLevel()
    {
        List<string> logs = new List<string>();
        BattleUnit attacker = Unit(
            "young warlord", 120, 120, 20, 0, true,
            MercenaryClass.Warlord, 100, 19);
        attacker.GainMagicPower(80);
        BattleUnit target = Unit("target", 500, 500, 4, 0, false);
        int randomCall = 0;
        BattleSkillResolver resolver = new BattleSkillResolver(
            new BattleSkillResolverContext(
                new List<BattleUnit> { attacker },
                new List<BattleUnit> { target },
                1f, 0f, 1.5f,
                () => randomCall++ == 0 ? 0f : 0.99f,
                (message, _) => logs.Add(message)));

        Assert.That(resolver.TryUsePlayerSkill(attacker, target), Is.True);
        Assert.That(
            logs.Exists(message => message.Contains("戦陣号令")),
            Is.False);
    }

    [TestCase(MercenaryClass.Knight, MercenarySkillId.AegisPierce)]
    [TestCase(MercenaryClass.Berserker, MercenarySkillId.RagingCombo)]
    [TestCase(MercenaryClass.Sniper, MercenarySkillId.ArmorBreakArrow)]
    [TestCase(MercenaryClass.Ranger, MercenarySkillId.GaleVolley)]
    [TestCase(MercenaryClass.Sage, MercenarySkillId.ArcaneBurst)]
    [TestCase(MercenaryClass.Elementalist, MercenarySkillId.StormCircle)]
    [TestCase(MercenaryClass.Bishop, MercenarySkillId.GreaterHeal)]
    [TestCase(MercenaryClass.Paladin, MercenarySkillId.HolyShieldBash)]
    [TestCase(MercenaryClass.Assassin, MercenarySkillId.FatalFlurry)]
    [TestCase(MercenaryClass.Ninja, MercenarySkillId.BindingBlade)]
    [TestCase(MercenaryClass.Dragoon, MercenarySkillId.SkySpearCombo)]
    [TestCase(MercenaryClass.GuardianLancer, MercenarySkillId.FortressPierce)]
    [TestCase(MercenaryClass.Warlord, MercenarySkillId.WarfrontSmash)]
    [TestCase(MercenaryClass.Beastmaster, MercenarySkillId.BeastKingFangs)]
    [TestCase(MercenaryClass.Chronomancer, MercenarySkillId.TemporalRift)]
    [TestCase(MercenaryClass.Saint, MercenarySkillId.DivineHymn)]
    [TestCase(MercenaryClass.Shadow, MercenarySkillId.VoidPierce)]
    [TestCase(MercenaryClass.DragonKnight, MercenarySkillId.DragonfallBreath)]
    public void AddedClassSkill_IsSelectedAtUnlockLevelAndResolves(
        MercenaryClass mercenaryClass,
        MercenarySkillId expectedSkill)
    {
        List<string> logs = new List<string>();
        MercenarySkillDefinition skill =
            MercenaryClassProgression.GetCombatSkills(mercenaryClass).Find(
                definition => definition.Id == expectedSkill);
        BattleUnit attacker = Unit(
            "veteran", 120, 120, 20, 0, true, mercenaryClass,
            100, skill.UnlockLevel);
        attacker.GainMagicPower(80);
        BattleUnit primaryTarget = Unit("target", 500, 500, 4, 0, false);
        BattleUnit secondaryTarget = Unit("secondary", 500, 500, 4, 0, false);
        BattleUnit ally = Unit("ally", 120, 40, 10, 0, true);
        int randomCall = 0;
        BattleSkillResolver resolver = new BattleSkillResolver(
            new BattleSkillResolverContext(
                new List<BattleUnit> { attacker, ally },
                new List<BattleUnit> { primaryTarget, secondaryTarget },
                1f, 0f, 1.5f,
                () => randomCall++ == 0 ? 0f : 0.99f,
                (message, _) => logs.Add(message)));

        Assert.That(
            resolver.TryUsePlayerSkill(
                attacker,
                primaryTarget,
                out string selectedSkillName),
            Is.True);
        Assert.That(selectedSkillName, Is.EqualTo(skill.Name));
        Assert.That(attacker.CurrentMagicPower,
            Is.EqualTo(attacker.MaxMagicPower - skill.MagicCost));
        Assert.That(logs.Exists(message => message.Contains(skill.Name)), Is.True);
    }

    [Test]
    public void EvadedParalysisDamageSkill_DoesNotApplyParalysis()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            UnityEngine.Random.InitState(seed);
            BattleUnit attacker = Unit(
                "warrior", 120, 120, 20, 0, true,
                MercenaryClass.Warrior, 100);
            attacker.GainMagicPower(80);
            BattleUnit target = Unit(
                "evasive", 500, 500, 4, 0, false,
                evasionRate: 0.75f);
            int randomCall = 0;
            BattleSkillResolver resolver = new BattleSkillResolver(
                new BattleSkillResolverContext(
                    new List<BattleUnit> { attacker },
                    new List<BattleUnit> { target },
                    1f, 0f, 1.5f,
                    () => randomCall++ == 0 ? 0f : 0.34f,
                    (message, _) => { }));

            Assert.That(resolver.TryUsePlayerSkill(attacker, target), Is.True);
            if (target.CurrentHP == target.MaxHP)
            {
                Assert.That(target.StatusEffect,
                    Is.EqualTo(BattleStatusEffect.None));
                return;
            }
        }

        Assert.Fail("No evasion was produced by the configured deterministic seeds.");
    }

    [TestCase(0, EnemySkillType.ArmorPierce)]
    [TestCase(1, EnemySkillType.FlameBreath)]
    [TestCase(5, EnemySkillType.Execute)]
    [TestCase(6, EnemySkillType.FrostBite)]
    [TestCase(7, EnemySkillType.DoubleStrike)]
    [TestCase(-1, EnemySkillType.FlameBreath)]
    public void GetDefaultEnemySkill_PreservesGradeBasedSelection(
        int grade, EnemySkillType expected)
    {
        Assert.That(BattleSkillResolver.GetDefaultEnemySkill(grade), Is.EqualTo(expected));
    }

    [Test]
    public void GetEnemyTarget_PrefersFirstLivingTauntingPlayer()
    {
        BattleUnit first = Unit("first", 100, 100, 10, 2, true);
        BattleUnit taunting = Unit("taunt", 100, 100, 10, 2, true);
        taunting.StartTaunt(2);
        BattleSkillResolver resolver = Resolver(
            new List<BattleUnit> { first, taunting },
            new List<BattleUnit>());

        Assert.That(resolver.GetEnemyTarget(), Is.SameAs(taunting));
    }

    [Test]
    public void IsUsefulSkillTarget_RejectsNormalAttackFinishes_AndSelectsMeaningfulUpgrade()
    {
        BattleUnit attacker = Unit("attacker", 100, 100, 10, 0, true);
        BattleUnit normalFinish = Unit("finish", 10, 10, 1, 0, false);
        BattleUnit useful = Unit("useful", 30, 30, 1, 0, false);

        Assert.That(BattleSkillResolver.IsUsefulSkillTarget(attacker, normalFinish, 16), Is.False);
        Assert.That(BattleSkillResolver.IsUsefulSkillTarget(attacker, useful, 16), Is.True);
    }

    [Test]
    public void CanDefeatWithNormalAttack_UsesTargetDefense()
    {
        BattleUnit attacker = Unit("attacker", 100, 100, 10, 0, true);
        BattleUnit target = Unit("target", 100, 7, 1, 3, false);

        Assert.That(BattleSkillResolver.CanDefeatWithNormalAttack(attacker, target), Is.True);
    }

    private static BattleSkillResolver Resolver(
        IReadOnlyList<BattleUnit> players, IReadOnlyList<BattleUnit> enemies)
    {
        return new BattleSkillResolver(new BattleSkillResolverContext(
            players, enemies, 0.6f, 0.3f, 1.5f, () => 1f,
            (message, type) => { }));
    }

    private static BattleUnit Unit(
        string name,
        int maxHp,
        int currentHp,
        int attack,
        int defense,
        bool player,
        MercenaryClass mercenaryClass = MercenaryClass.Warrior,
        int maxMagicPower = 0,
        int level = 1,
        float evasionRate = 0f)
    {
        return new BattleUnit(
            name,
            maxHp,
            currentHp,
            attack,
            defense,
            1f,
            player,
            mercenaryClass,
            maxMagicPower,
            0f,
            evasionRate,
            BattleStatusEffect.None,
            level);
    }
}
