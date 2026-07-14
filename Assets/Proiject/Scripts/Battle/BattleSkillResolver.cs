using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resolves skill selection and effects without coroutine or scene dependencies.
/// Mutable combat state remains in the supplied <see cref="BattleUnit"/> instances.
/// </summary>
public sealed class BattleSkillResolver
{
    private readonly BattleSkillResolverContext context;

    public BattleSkillResolver(BattleSkillResolverContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public BattleUnit GetEnemyTarget()
    {
        foreach (BattleUnit player in context.PlayerUnits)
        {
            if (player != null && player.IsTaunting)
            {
                return player;
            }
        }

        foreach (BattleUnit player in context.PlayerUnits)
        {
            if (player != null && !player.IsDead)
            {
                return player;
            }
        }
        return null;
    }

    public bool TryUseEnemySkill(BattleUnit attacker, BattleUnit target, EnemyDataSO data)
    {
        return TryUseEnemySkill(attacker, target, data, out _);
    }

    public bool TryUseEnemySkill(
        BattleUnit attacker,
        BattleUnit target,
        EnemyDataSO data,
        out string skillName)
    {
        skillName = string.Empty;
        if (data == null || context.RandomValue() > context.EnemySkillUseChance)
        {
            return false;
        }

        EnemySkillType skill = data.enemySkill != EnemySkillType.None
            ? data.enemySkill : GetDefaultEnemySkill(data.monsterGrade);
        skillName = GetEnemySkillDisplayName(skill);
        switch (skill)
        {
            case EnemySkillType.PowerStrike:
                return UseEnemyDamageSkill(
                    attacker,
                    target,
                    "強撃",
                    1.45f,
                    BattleStatusEffect.None);
            case EnemySkillType.VenomStrike:
                return UseEnemyDamageSkill(
                    attacker,
                    target,
                    "毒牙",
                    0.9f,
                    BattleStatusEffect.Poison);
            case EnemySkillType.ParalyzingRoar:
                Log(BattleLogFormatter.FormatSkillActivationNoDetail(attacker.UnitName, "麻痺の咆哮"), BattleLogType.Enemy);
                target.ApplyStatus(BattleStatusEffect.Paralysis, 1);
                return true;
            case EnemySkillType.CriticalFocus:
                attacker.BoostCriticalRate(0.25f, 3);
                Log(BattleLogFormatter.FormatSkillActivation(attacker.UnitName, "研ぎ澄ます", "次の2行動はクリティカル率+25%。"), BattleLogType.Enemy);
                return true;
            case EnemySkillType.DoubleStrike:
                UseEnemyDamageSkill(attacker, target, "連撃", 0.72f, BattleStatusEffect.None);
                if (!target.IsDead)
                {
                    UseEnemyDamageSkill(
                        attacker,
                        target,
                        "連撃",
                        0.72f,
                        BattleStatusEffect.None);
                }
                return true;
            case EnemySkillType.LifeDrain:
                return UseEnemyLifeDrain(attacker, target);
            case EnemySkillType.ArmorPierce:
                return UseEnemyPureDamageSkill(
                    attacker, target, "装甲穿ち", 1.05f);
            case EnemySkillType.FlameBreath:
                return UseEnemyAreaDamageSkill(
                    attacker,
                    "灼熱の息",
                    0.68f,
                    BattleStatusEffect.None);
            case EnemySkillType.FrostBite:
                return UseEnemyDamageSkill(
                    attacker,
                    target,
                    "氷結牙",
                    0.82f,
                    BattleStatusEffect.Paralysis);
            case EnemySkillType.TripleStrike:
                return UseEnemyMultiStrike(
                    attacker, target, "三連爪", 3, 0.52f);
            case EnemySkillType.BattleHeal:
                return UseEnemyBattleHeal(attacker);
            case EnemySkillType.SacrificialStrike:
                return UseEnemySacrificialStrike(attacker, target);
            case EnemySkillType.Execute:
                return UseEnemyExecute(attacker, target);
            case EnemySkillType.ArcaneBolt:
                return UseEnemyMagicDamageSkill(
                    attacker,
                    target,
                    "魔弾",
                    0.55f,
                    0.28f,
                    BattleStatusEffect.None);
            case EnemySkillType.MeteorRain:
                return UseEnemyMagicAreaDamageSkill(
                    attacker,
                    "星火雨",
                    0.45f,
                    0.12f);
            case EnemySkillType.CrushingBlow:
                return UseEnemyDamageSkill(
                    attacker,
                    target,
                    "粉砕撃",
                    1.15f,
                    BattleStatusEffect.Paralysis);
            case EnemySkillType.BerserkRush:
                return UseEnemyMultiStrike(
                    attacker, target, "狂乱連撃", 4, 0.38f);
            case EnemySkillType.Regeneration:
                return UseEnemyRegeneration(attacker);
            case EnemySkillType.SoulBurst:
                return UseEnemySoulBurst(attacker);
            case EnemySkillType.ToxicCloud:
                return UseEnemyAreaDamageSkill(
                    attacker,
                    "毒霧",
                    0.42f,
                    BattleStatusEffect.Poison);
            default:
                return false;
        }
    }

    public bool TryUsePlayerSkill(BattleUnit attacker, BattleUnit primaryTarget)
    {
        return TryUsePlayerSkill(attacker, primaryTarget, out _);
    }

    public bool TryUsePlayerSkill(
        BattleUnit attacker,
        BattleUnit primaryTarget,
        out string skillName)
    {
        skillName = string.Empty;
        if (context.RandomValue() > context.PlayerSkillUseChance ||
            CanDefeatWithNormalAttack(attacker, primaryTarget))
        {
            return false;
        }

        List<MercenarySkillDefinition> skills =
            MercenaryClassProgression.GetCombatSkills(attacker.MercenaryClass);
        skills.RemoveAll(skill =>
            skill == null || attacker.Level < Mathf.Max(1, skill.UnlockLevel));
        if (skills.Count == 0)
        {
            return false;
        }
        int skillIndex = Mathf.Clamp(
            Mathf.FloorToInt(context.RandomValue() * skills.Count),
            0,
            skills.Count - 1);
        MercenarySkillDefinition skill = skills[skillIndex];
        skillName = skill.Name;
        switch (skill.Id)
        {
            case MercenarySkillId.Taunt:
                return TryUseWarriorTaunt(attacker, skill);
            case MercenarySkillId.DoubleShot:
                return TryUseArcherDoubleShot(attacker, skill);
            case MercenarySkillId.Fireball:
                return TryUseMageFireball(attacker, skill);
            case MercenarySkillId.Heal:
                return TryUsePriestHeal(attacker, skill);
            case MercenarySkillId.PoisonBlade:
                return TryUsePoisonBlade(attacker, primaryTarget, skill);
            case MercenarySkillId.PiercingThrust:
            case MercenarySkillId.AegisPierce:
            case MercenarySkillId.ArmorBreakArrow:
            case MercenarySkillId.FortressPierce:
            case MercenarySkillId.TemporalRift:
            case MercenarySkillId.VoidPierce:
                return TryUsePiercingThrust(attacker, primaryTarget, skill);
            case MercenarySkillId.ShieldBash:
            case MercenarySkillId.HolyShieldBash:
            case MercenarySkillId.BindingBlade:
                return TryUseStatusDamagePlayer(attacker, primaryTarget, skill,
                    BattleStatusEffect.Paralysis);
            case MercenarySkillId.Volley:
            case MercenarySkillId.SweepingThrust:
            case MercenarySkillId.GaleVolley:
            case MercenarySkillId.WarfrontSmash:
            case MercenarySkillId.DragonfallBreath:
                return TryUsePlayerAreaDamage(attacker, skill,
                    BattleStatusEffect.None);
            case MercenarySkillId.FrostNova:
            case MercenarySkillId.StormCircle:
                return TryUsePlayerAreaDamage(attacker, skill,
                    BattleStatusEffect.Paralysis);
            case MercenarySkillId.Smite:
                return TryUseDirectPlayerDamage(
                    attacker, primaryTarget, skill, out _);
            case MercenarySkillId.ShadowFlurry:
                return TryUsePlayerMultiStrike(attacker, primaryTarget, skill, 2);
            case MercenarySkillId.GuardCounter:
            case MercenarySkillId.AimedShot:
            case MercenarySkillId.ManaBolt:
            case MercenarySkillId.VitalStrike:
            case MercenarySkillId.GuardianThrust:
            case MercenarySkillId.WarlordCommand:
            case MercenarySkillId.ArcaneBurst:
                return TryUseDirectPlayerDamage(
                    attacker, primaryTarget, skill, out _);
            case MercenarySkillId.PrayerLight:
            case MercenarySkillId.SaintsGrace:
            case MercenarySkillId.DivineHymn:
                return TryUsePartyHeal(attacker, skill);
            case MercenarySkillId.GreaterHeal:
                return TryUsePriestHeal(attacker, skill);
            case MercenarySkillId.BeastPack:
            case MercenarySkillId.DragonBreath:
                return TryUsePlayerAreaDamage(attacker, skill,
                    BattleStatusEffect.None);
            case MercenarySkillId.TimeLock:
                return TryUseStatusDamagePlayer(attacker, primaryTarget, skill,
                    BattleStatusEffect.Paralysis);
            case MercenarySkillId.ShadowRend:
                return TryUsePlayerMultiStrike(attacker, primaryTarget, skill, 3);
            case MercenarySkillId.RagingCombo:
            case MercenarySkillId.SkySpearCombo:
                return TryUsePlayerMultiStrike(attacker, primaryTarget, skill, 3);
            case MercenarySkillId.FatalFlurry:
            case MercenarySkillId.BeastKingFangs:
                return TryUsePlayerMultiStrike(attacker, primaryTarget, skill, 4);
            default:
                return false;
        }
    }

    public static EnemySkillType GetDefaultEnemySkill(int monsterGrade)
    {
        EnemySkillType[] lower =
        {
            EnemySkillType.PowerStrike,
            EnemySkillType.VenomStrike,
            EnemySkillType.FrostBite,
            EnemySkillType.DoubleStrike,
            EnemySkillType.ArcaneBolt,
            EnemySkillType.CrushingBlow,
            EnemySkillType.Regeneration
        };
        EnemySkillType[] upper =
        {
            EnemySkillType.ArmorPierce,
            EnemySkillType.FlameBreath,
            EnemySkillType.MeteorRain,
            EnemySkillType.BerserkRush,
            EnemySkillType.SoulBurst,
            EnemySkillType.Execute,
            EnemySkillType.TripleStrike,
            EnemySkillType.BattleHeal,
            EnemySkillType.SacrificialStrike,
            EnemySkillType.ToxicCloud
        };
        EnemySkillType[] candidates = monsterGrade <= 5 ? upper : lower;
        int candidateIndex = monsterGrade <= 5
            ? Math.Abs(monsterGrade)
            : monsterGrade - 4;
        return candidates[candidateIndex % candidates.Length];
    }

    public static string GetEnemySkillDisplayName(EnemySkillType skill)
    {
        switch (skill)
        {
            case EnemySkillType.PowerStrike: return "強撃";
            case EnemySkillType.VenomStrike: return "毒牙";
            case EnemySkillType.ParalyzingRoar: return "麻痺の咆哮";
            case EnemySkillType.CriticalFocus: return "急所集中";
            case EnemySkillType.DoubleStrike: return "連撃";
            case EnemySkillType.LifeDrain: return "生命吸収";
            case EnemySkillType.ArmorPierce: return "装甲貫通";
            case EnemySkillType.FlameBreath: return "灼熱の息";
            case EnemySkillType.FrostBite: return "氷結牙";
            case EnemySkillType.TripleStrike: return "三連爪";
            case EnemySkillType.BattleHeal: return "戦場治癒";
            case EnemySkillType.SacrificialStrike: return "捨身撃";
            case EnemySkillType.Execute: return "処刑";
            case EnemySkillType.ArcaneBolt: return "魔弾";
            case EnemySkillType.MeteorRain: return "星屑雨";
            case EnemySkillType.CrushingBlow: return "粉砕撃";
            case EnemySkillType.BerserkRush: return "狂乱連撃";
            case EnemySkillType.Regeneration: return "再生";
            case EnemySkillType.SoulBurst: return "魂爆";
            case EnemySkillType.ToxicCloud: return "毒霧";
            default: return string.Empty;
        }
    }

    public static bool CanDefeatWithNormalAttack(BattleUnit attacker, BattleUnit target)
    {
        return target != null && target.CurrentHP <= target.EstimateDamageTaken(attacker.CalculateDamage());
    }

    public static bool IsUsefulSkillTarget(BattleUnit attacker, BattleUnit target, int rawSkillDamage)
    {
        if (target == null || target.IsDead)
        {
            return false;
        }

        int normalDamage = target.EstimateDamageTaken(attacker.CalculateDamage());
        if (target.CurrentHP <= normalDamage)
        {
            return false;
        }

        int skillDamage = target.EstimateDamageTaken(rawSkillDamage);
        return skillDamage > normalDamage &&
               skillDamage <= target.CurrentHP + Math.Max(8, normalDamage);
    }

    private bool UseEnemyPureDamageSkill(BattleUnit attacker, BattleUnit target, string skillName, float power)
    {
        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvaded(
                    attacker.UnitName,
                    skillName,
                    target.UnitName),
                BattleLogType.Enemy);
            return true;
        }
        bool critical = attacker.RollCritical();
        int damage = Math.Max(1, Round(attacker.Attack * power * (critical ? context.CriticalDamageMultiplier : 1f)));
        int before = target.CurrentHP;
        target.TakePureDamage(damage);
        Log(BattleLogFormatter.FormatPureDamageSkill(attacker.UnitName, skillName, critical, target.UnitName, before - target.CurrentHP), BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemyAreaDamageSkill(BattleUnit attacker, string skillName, float power, BattleStatusEffect status)
    {
        List<BattleUnit> targets = Living(context.PlayerUnits);
        if (targets.Count == 0)
        {
            return false;
        }
        int damage = 0;
        int affected = 0;
        foreach (BattleUnit target in targets)
        {
            if (target.TryEvade())
            {
                continue;
            }
            int before = target.CurrentHP;
            target.TakeDamage(Round(attacker.CalculateDamage() * power));
            damage += before - target.CurrentHP;
            affected++;
            if (!target.IsDead && status != BattleStatusEffect.None)
            {
                target.ApplyStatus(
                    status,
                    status == BattleStatusEffect.Poison ? 3 : 1);
            }
        }
        Log(BattleLogFormatter.FormatAreaDamageSkill(attacker.UnitName, skillName, affected, damage, status == BattleStatusEffect.None ? string.Empty : GetStatusName(status)), BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemyMagicDamageSkill(
        BattleUnit attacker,
        BattleUnit target,
        string skillName,
        float attackPower,
        float magicPower,
        BattleStatusEffect status)
    {
        if (target == null)
        {
            return false;
        }

        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvadedWithMagic(
                    attacker.UnitName,
                    skillName,
                    target.UnitName,
                    attacker.CurrentMagicPower,
                    attacker.MaxMagicPower),
                BattleLogType.Enemy);
            return true;
        }

        bool critical = attacker.RollCritical();
        int rawDamage = Round(
            (attacker.Attack * attackPower +
             attacker.MaxMagicPower * magicPower) *
            (critical ? context.CriticalDamageMultiplier : 1f));
        int before = target.CurrentHP;
        target.TakeDamage(rawDamage);
        int damage = before - target.CurrentHP;
        if (!target.IsDead && status != BattleStatusEffect.None)
        {
            target.ApplyStatus(status, 1);
        }
        Log(
            BattleLogFormatter.FormatDamageSkillWithMagic(
                attacker.UnitName,
                skillName,
                critical,
                target.UnitName,
                damage,
                attacker.CurrentMagicPower,
                attacker.MaxMagicPower),
            BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemyMagicAreaDamageSkill(
        BattleUnit attacker,
        string skillName,
        float attackPower,
        float magicPower)
    {
        List<BattleUnit> targets = Living(context.PlayerUnits);
        if (targets.Count == 0)
        {
            return false;
        }

        int rawDamage = Round(
            attacker.Attack * attackPower +
            attacker.MaxMagicPower * magicPower);
        int damage = 0;
        int affected = 0;
        foreach (BattleUnit target in targets)
        {
            if (target.TryEvade())
            {
                continue;
            }

            int before = target.CurrentHP;
            target.TakeDamage(rawDamage);
            damage += before - target.CurrentHP;
            affected++;
        }

        Log(
            BattleLogFormatter.FormatAreaDamageSkill(
                attacker.UnitName,
                skillName,
                affected,
                damage,
                string.Empty),
            BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemyMultiStrike(BattleUnit attacker, BattleUnit target, string skillName, int hitCount, float power)
    {
        int damage = 0;
        int landed = 0;
        for (int i = 0; i < hitCount && !target.IsDead; i++)
        {
            if (target.TryEvade())
            {
                continue;
            }
            int before = target.CurrentHP;
            target.TakeDamage(Round(attacker.CalculateDamage() * power));
            damage += before - target.CurrentHP;
            landed++;
        }
        Log(BattleLogFormatter.FormatMultiStrikeSkill(attacker.UnitName, skillName, landed, hitCount, damage), BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemyBattleHeal(BattleUnit attacker)
    {
        if (attacker.MaxHP - attacker.CurrentHP <
            Math.Max(1, attacker.MaxHP / 5))
        {
            return false;
        }
        int before = attacker.CurrentHP;
        attacker.Heal(Round(attacker.MaxHP * 0.28f));
        Log(BattleLogFormatter.FormatHealSkill(attacker.UnitName, "再生", attacker.CurrentHP - before), BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemyRegeneration(BattleUnit attacker)
    {
        if (attacker.IsDead ||
            attacker.CurrentHP > Mathf.RoundToInt(attacker.MaxHP * 0.55f))
        {
            return false;
        }

        int before = attacker.CurrentHP;
        attacker.Heal(Round(attacker.MaxHP * 0.22f));
        Log(
            BattleLogFormatter.FormatHealSkill(
                attacker.UnitName,
                "超再生",
                attacker.CurrentHP - before),
            BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemySacrificialStrike(BattleUnit attacker, BattleUnit target)
    {
        if (attacker.CurrentHP <= Math.Max(1, attacker.MaxHP / 10))
        {
            return false;
        }
        int recoil = Math.Max(1, Round(attacker.MaxHP * 0.08f));
        attacker.TakePureDamage(recoil);
        bool used = UseEnemyDamageSkill(attacker, target, "捨て身の猛撃", 1.85f, BattleStatusEffect.None);
        if (used)
        {
            Log(
                BattleLogFormatter.FormatRecoil(attacker.UnitName, recoil),
                BattleLogType.Enemy);
        }
        return used;
    }

    private bool UseEnemyExecute(BattleUnit attacker, BattleUnit target)
    {
        float ratio = target.MaxHP > 0
            ? (float)target.CurrentHP / target.MaxHP
            : 1f;
        return UseEnemyDamageSkill(
            attacker,
            target,
            ratio <= 0.35f ? "処刑撃" : "追い込み",
            ratio <= 0.35f ? 2f : 0.85f,
            BattleStatusEffect.None);
    }

    private bool UseEnemyLifeDrain(BattleUnit attacker, BattleUnit target)
    {
        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvaded(
                    attacker.UnitName,
                    "吸命",
                    target.UnitName),
                BattleLogType.Enemy);
            return true;
        }
        bool critical = attacker.RollCritical();
        int before = target.CurrentHP;
        target.TakeDamage(Round(attacker.Attack * 0.9f * (critical ? context.CriticalDamageMultiplier : 1f)));
        int damage = before - target.CurrentHP;
        int heal = Math.Max(1, damage / 2);
        attacker.Heal(heal);
        Log(BattleLogFormatter.FormatLifeDrain(attacker.UnitName, critical, damage, heal), BattleLogType.Enemy);
        return true;
    }

    private bool UseEnemySoulBurst(BattleUnit attacker)
    {
        if (Living(context.PlayerUnits).Count == 0)
        {
            return false;
        }

        int recoil = Math.Max(1, Round(attacker.MaxHP * 0.06f));
        attacker.TakePureDamage(recoil);
        bool used = UseEnemyAreaDamageSkill(
            attacker,
            "魂魄爆裂",
            0.55f,
            BattleStatusEffect.None);
        if (used)
        {
            Log(
                BattleLogFormatter.FormatRecoil(attacker.UnitName, recoil),
                BattleLogType.Enemy);
        }
        return used;
    }

    private bool UseEnemyDamageSkill(BattleUnit attacker, BattleUnit target, string skillName, float power, BattleStatusEffect status)
    {
        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvaded(
                    attacker.UnitName,
                    skillName,
                    target.UnitName),
                BattleLogType.Enemy);
            return true;
        }
        bool critical = attacker.RollCritical();
        int before = target.CurrentHP;
        target.TakeDamage(Round(attacker.CalculateDamage() * power * (critical ? context.CriticalDamageMultiplier : 1f)));
        int damage = before - target.CurrentHP;
        if (!target.IsDead && status != BattleStatusEffect.None)
        {
            target.ApplyStatus(
                status,
                status == BattleStatusEffect.Poison ? 3 : 1);
        }
        Log(BattleLogFormatter.FormatDamageSkill(attacker.UnitName, skillName, critical, target.UnitName, damage, status != BattleStatusEffect.None && !target.IsDead ? GetStatusName(status) : string.Empty), BattleLogType.Enemy);
        return true;
    }

    private bool TryUseWarriorTaunt(BattleUnit attacker, MercenarySkillDefinition skill)
    {
        if (attacker.IsTaunting ||
            !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        attacker.StartTaunt(2);
        Log(BattleLogFormatter.FormatTaunt(attacker.UnitName, attacker.StatusSummary, attacker.CurrentMagicPower, attacker.MaxMagicPower), BattleLogType.Player);
        return true;
    }

    private bool TryUseArcherDoubleShot(BattleUnit attacker, MercenarySkillDefinition skill)
    {
        int rawDamage = Round(attacker.Attack * skill.Power);
        if (!HasUsefulSkillTarget(attacker, rawDamage, 1) ||
            !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        int damage = 0;
        int criticalCount = 0;
        for (int i = 0; i < 2; i++)
        {
            BattleUnit target = GetUsefulSkillTarget(attacker, rawDamage);
            if (target == null)
            {
                break;
            }
            if (target.TryEvade())
            {
                continue;
            }
            bool critical = attacker.RollCritical();
            if (critical)
            {
                criticalCount++;
            }
            int before = target.CurrentHP;
            target.TakeDamage(Round(attacker.Attack * skill.Power * (critical ? context.CriticalDamageMultiplier : 1f)));
            damage += before - target.CurrentHP;
        }
        attacker.BoostCriticalRate(0.15f, 3);
        Log(BattleLogFormatter.FormatDoubleShot(attacker.UnitName, damage, criticalCount, attacker.CurrentMagicPower, attacker.MaxMagicPower), BattleLogType.Player);
        return true;
    }

    private bool TryUseMageFireball(BattleUnit attacker, MercenarySkillDefinition skill)
    {
        int damage = Round(attacker.Attack * skill.Power);
        BattleUnit target = GetUsefulSkillTarget(attacker, damage);
        if (target == null || !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvadedWithMagic(
                    attacker.UnitName,
                    "火球",
                    target.UnitName,
                    attacker.CurrentMagicPower,
                    attacker.MaxMagicPower),
                BattleLogType.Player);
            return true;
        }
        bool critical = attacker.RollCritical();
        if (critical)
        {
            damage = Round(damage * context.CriticalDamageMultiplier);
        }
        int before = target.CurrentHP;
        target.TakeDamage(damage);
        Log(BattleLogFormatter.FormatDamageSkillWithMagic(attacker.UnitName, "火球", critical, target.UnitName, before - target.CurrentHP, attacker.CurrentMagicPower, attacker.MaxMagicPower), BattleLogType.Player);
        return true;
    }

    private bool TryUsePriestHeal(BattleUnit attacker, MercenarySkillDefinition skill)
    {
        BattleUnit target = null;
        int largestMissing = 0;
        foreach (BattleUnit ally in context.PlayerUnits)
        {
            if (ally == null || ally.IsDead)
            {
                continue;
            }
            int missing = ally.MaxHP - ally.CurrentHP;
            if (missing > largestMissing)
            {
                largestMissing = missing;
                target = ally;
            }
        }
        if (target == null ||
            largestMissing < Math.Max(8, target.MaxHP / 5) ||
            !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        int before = target.CurrentHP;
        target.Heal(Round(attacker.Attack * skill.Power));
        Log(BattleLogFormatter.FormatHealSkillWithMagic(attacker.UnitName, skill.Name, target.UnitName, target.CurrentHP - before, attacker.CurrentMagicPower, attacker.MaxMagicPower), BattleLogType.Player);
        return true;
    }

    private bool TryUsePoisonBlade(BattleUnit attacker, BattleUnit target, MercenarySkillDefinition skill)
    {
        if (target == null || !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvaded(
                    attacker.UnitName,
                    skill.Name,
                    target.UnitName),
                BattleLogType.Player);
            return true;
        }
        int before = target.CurrentHP;
        target.TakeDamage(Round(attacker.Attack * skill.Power));
        int damage = before - target.CurrentHP;
        if (!target.IsDead)
        {
            target.ApplyStatus(BattleStatusEffect.Poison, 3);
        }
        Log(BattleLogFormatter.FormatStatusDamageSkill(attacker.UnitName, skill.Name, damage, "毒"), BattleLogType.Player);
        return true;
    }

    private bool TryUseDirectPlayerDamage(
        BattleUnit attacker,
        BattleUnit target,
        MercenarySkillDefinition skill,
        out bool hit)
    {
        hit = false;
        if (target == null || !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        if (target.TryEvade())
        {
            Log(BattleLogFormatter.FormatSkillEvaded(attacker.UnitName, skill.Name, target.UnitName), BattleLogType.Player);
            return true;
        }
        bool critical = attacker.RollCritical();
        int before = target.CurrentHP;
        target.TakeDamage(Round(attacker.CalculateDamage() * skill.Power *
            (critical ? context.CriticalDamageMultiplier : 1f)));
        hit = true;
        Log(BattleLogFormatter.FormatDamageSkill(attacker.UnitName, skill.Name, critical,
            target.UnitName, before - target.CurrentHP, string.Empty), BattleLogType.Player);
        return true;
    }

    private bool TryUseStatusDamagePlayer(
        BattleUnit attacker,
        BattleUnit target,
        MercenarySkillDefinition skill,
        BattleStatusEffect status)
    {
        if (!TryUseDirectPlayerDamage(attacker, target, skill, out bool hit))
        {
            return false;
        }
        if (hit && target != null && !target.IsDead)
        {
            target.ApplyStatus(status, 1);
        }
        return true;
    }

    private bool TryUsePlayerAreaDamage(
        BattleUnit attacker,
        MercenarySkillDefinition skill,
        BattleStatusEffect status)
    {
        List<BattleUnit> targets = Living(context.EnemyUnits);
        if (targets.Count == 0 ||
            !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        int affected = 0;
        int damage = 0;
        foreach (BattleUnit target in targets)
        {
            if (target.TryEvade())
            {
                continue;
            }
            int before = target.CurrentHP;
            target.TakeDamage(Round(attacker.CalculateDamage() * skill.Power));
            damage += before - target.CurrentHP;
            affected++;
            if (!target.IsDead && status != BattleStatusEffect.None)
            {
                target.ApplyStatus(status, 1);
            }
        }
        Log(
            BattleLogFormatter.FormatAreaDamageSkill(
                attacker.UnitName,
                skill.Name,
                affected,
                damage,
                status == BattleStatusEffect.None
                    ? string.Empty
                    : GetStatusName(status)),
            BattleLogType.Player);
        return true;
    }

    private bool TryUsePartyHeal(
        BattleUnit attacker,
        MercenarySkillDefinition skill)
    {
        List<BattleUnit> targets = Living(context.PlayerUnits);
        int totalMissing = 0;
        foreach (BattleUnit target in targets)
        {
            totalMissing += target.MaxHP - target.CurrentHP;
        }
        if (targets.Count == 0 ||
            totalMissing < Math.Max(8, attacker.Attack / 2) ||
            !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }

        int healed = 0;
        foreach (BattleUnit target in targets)
        {
            int before = target.CurrentHP;
            target.Heal(Math.Max(1, Round(attacker.Attack * skill.Power)));
            healed += target.CurrentHP - before;
        }
        Log(
            BattleLogFormatter.FormatHealSkillWithMagic(
                attacker.UnitName,
                skill.Name,
                "味方全体",
                healed,
                attacker.CurrentMagicPower,
                attacker.MaxMagicPower),
            BattleLogType.Player);
        return true;
    }

    private bool TryUsePlayerMultiStrike(
        BattleUnit attacker,
        BattleUnit target,
        MercenarySkillDefinition skill,
        int hitCount)
    {
        if (target == null || !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        int landedHits = 0;
        int damage = 0;
        for (int i = 0; i < hitCount && !target.IsDead; i++)
        {
            if (!target.TryEvade())
            {
                int before = target.CurrentHP;
                target.TakeDamage(Round(attacker.CalculateDamage() * skill.Power));
                damage += before - target.CurrentHP;
                landedHits++;
            }
        }
        Log(
            BattleLogFormatter.FormatMultiStrikeSkill(
                attacker.UnitName,
                skill.Name,
                landedHits,
                hitCount,
                damage),
            BattleLogType.Player);
        return true;
    }

    private bool TryUsePiercingThrust(BattleUnit attacker, BattleUnit target, MercenarySkillDefinition skill)
    {
        if (target == null || !attacker.TryConsumeMagicPower(skill.MagicCost))
        {
            return false;
        }
        if (target.TryEvade())
        {
            Log(
                BattleLogFormatter.FormatSkillEvaded(
                    attacker.UnitName,
                    skill.Name,
                    target.UnitName),
                BattleLogType.Player);
            return true;
        }
        int before = target.CurrentHP;
        target.TakePureDamage(Round(attacker.Attack * skill.Power));
        Log(BattleLogFormatter.FormatPureDamageSkillSimple(attacker.UnitName, skill.Name, before - target.CurrentHP), BattleLogType.Player);
        return true;
    }

    private bool HasUsefulSkillTarget(BattleUnit attacker, int rawDamage, int maxTargets)
    {
        int found = 0;
        foreach (BattleUnit enemy in context.EnemyUnits)
        {
            if (!IsUsefulSkillTarget(attacker, enemy, rawDamage))
            {
                continue;
            }
            found++;
            if (found >= maxTargets)
            {
                return true;
            }
        }
        return found > 0;
    }

    private BattleUnit GetUsefulSkillTarget(BattleUnit attacker, int rawDamage)
    {
        BattleUnit best = null;
        int bestHp = -1;
        foreach (BattleUnit enemy in context.EnemyUnits)
        {
            if (IsUsefulSkillTarget(attacker, enemy, rawDamage) &&
                enemy.CurrentHP > bestHp)
            {
                bestHp = enemy.CurrentHP;
                best = enemy;
            }
        }
        return best;
    }

    private static List<BattleUnit> Living(IReadOnlyList<BattleUnit> units)
    {
        List<BattleUnit> result = new List<BattleUnit>();
        foreach (BattleUnit unit in units)
        {
            if (unit != null && !unit.IsDead)
            {
                result.Add(unit);
            }
        }
        return result;
    }

    private static string GetStatusName(BattleStatusEffect status)
    {
        switch (status)
        {
            case BattleStatusEffect.Poison:
                return "毒";
            case BattleStatusEffect.Paralysis:
                return "麻痺";
            default:
                return "状態異常";
        }
    }

    private static int Round(float value)
    {
        return Mathf.RoundToInt(value);
    }

    private void Log(string message, BattleLogType type)
    {
        context.Log(message, type);
    }
}

public sealed class BattleSkillResolverContext
{
    public readonly IReadOnlyList<BattleUnit> PlayerUnits;
    public readonly IReadOnlyList<BattleUnit> EnemyUnits;
    public readonly float PlayerSkillUseChance;
    public readonly float EnemySkillUseChance;
    public readonly float CriticalDamageMultiplier;
    public readonly Func<float> RandomValue;
    public readonly Action<string, BattleLogType> Log;

    public BattleSkillResolverContext(
        IReadOnlyList<BattleUnit> playerUnits,
        IReadOnlyList<BattleUnit> enemyUnits,
        float playerSkillUseChance,
        float enemySkillUseChance,
        float criticalDamageMultiplier,
        Func<float> randomValue,
        Action<string, BattleLogType> log)
    {
        PlayerUnits = playerUnits ?? throw new ArgumentNullException(nameof(playerUnits));
        EnemyUnits = enemyUnits ?? throw new ArgumentNullException(nameof(enemyUnits));
        PlayerSkillUseChance = playerSkillUseChance;
        EnemySkillUseChance = enemySkillUseChance;
        CriticalDamageMultiplier = criticalDamageMultiplier;
        RandomValue = randomValue ?? throw new ArgumentNullException(nameof(randomValue));
        Log = log ?? throw new ArgumentNullException(nameof(log));
    }
}
