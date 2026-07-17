using System.Collections.Generic;
using UnityEngine;

public static class DungeonEnemyVariantService
{
    public static EnemyDataSO CreateSpecialVariant(
        EnemyDataSO source,
        IReadOnlyList<EnemySkillType> skillPool,
        DungeonGrade dungeonGrade,
        bool isBossVariant,
        System.Func<float> randomValueProvider = null)
    {
        if (source == null || skillPool == null || skillPool.Count == 0)
        {
            return source;
        }

        List<EnemySkillType> skills = CollectAvailableSkills(skillPool);
        if (skills.Count == 0)
        {
            return source;
        }

        EnemyDataSO variant = Object.Instantiate(source);
        variant.name = $"{source.name} Special Variant";
        variant.runtimeSourcePersistentId = source.PersistentId;
        variant.hideFlags = HideFlags.DontSave;
        variant.isSpecialVariant = true;
        System.Func<float> random = randomValueProvider ?? (() => Random.value);
        int skillIndex = Mathf.Clamp(
            Mathf.FloorToInt(random() * skills.Count),
            0,
            skills.Count - 1);
        variant.enemySkill = skills[skillIndex];
        variant.specialVariantTitle =
            GetSpecialVariantTitle(variant.enemySkill, isBossVariant);

        ApplyBaseVariantBonus(variant, source, isBossVariant);
        ApplySpecialSkillStatBonus(variant);
        AddSpecialVariantMaterialDrop(variant, dungeonGrade, isBossVariant);
        if (isBossVariant)
        {
            AddSpecialJobCertificateDrop(variant, dungeonGrade);
        }

        return variant;
    }

    private static List<EnemySkillType> CollectAvailableSkills(
        IReadOnlyList<EnemySkillType> skillPool)
    {
        List<EnemySkillType> skills = new List<EnemySkillType>();
        foreach (EnemySkillType skill in skillPool)
        {
            if (skill != EnemySkillType.None)
            {
                skills.Add(skill);
            }
        }

        return skills;
    }

    private static void ApplyBaseVariantBonus(
        EnemyDataSO variant,
        EnemyDataSO source,
        bool isBossVariant)
    {
        variant.maxHP = Mathf.RoundToInt(
            source.maxHP * (isBossVariant ? 1.4f : 1.18f));
        variant.attack = Mathf.RoundToInt(
            source.attack * (isBossVariant ? 1.25f : 1.12f));
        variant.defense = Mathf.RoundToInt(
            source.defense * (isBossVariant ? 1.2f : 1.1f));
        variant.goldReward = Mathf.RoundToInt(
            source.goldReward * (isBossVariant ? 3f : 1.75f));
        variant.experienceMultiplier =
            Mathf.Max(1f, source.experienceMultiplier) *
            (isBossVariant ? 2.5f : 2f);
    }

    private static void AddSpecialVariantMaterialDrop(
        EnemyDataSO variant,
        DungeonGrade dungeonGrade,
        bool isBossVariant)
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            GetMutantCoreResourcePath(dungeonGrade));
        if (variant == null || material == null)
        {
            return;
        }

        List<ItemDropEntry> drops = new List<ItemDropEntry>();
        if (variant.itemDrops != null)
        {
            drops.AddRange(variant.itemDrops);
        }

        drops.Add(new ItemDropEntry
        {
            item = material,
            amount = isBossVariant ? 2 : 1,
            dropChance = isBossVariant ? 1f : 0.5f
        });
        variant.itemDrops = drops.ToArray();
    }

    private static string GetMutantCoreResourcePath(DungeonGrade dungeonGrade)
    {
        switch (dungeonGrade)
        {
            case DungeonGrade.Lower: return "Items/Special/LowerGradeMutantCore";
            case DungeonGrade.Middle: return "Items/Special/MiddleGradeMutantCore";
            case DungeonGrade.Upper: return "Items/Special/UpperGradeMutantCore";
            case DungeonGrade.Highest: return "Items/Special/HighestGradeMutantCore";
            default: return "Items/Special/MutantCore";
        }
    }

    private static void AddSpecialJobCertificateDrop(
        EnemyDataSO variant,
        DungeonGrade dungeonGrade)
    {
        ItemDataSO certificate = Resources.Load<ItemDataSO>(
            "Items/JobChange/SecretJobCertificate");
        if (variant == null || certificate == null)
        {
            return;
        }

        List<ItemDropEntry> drops = new List<ItemDropEntry>();
        if (variant.itemDrops != null)
        {
            drops.AddRange(variant.itemDrops);
        }

        float chance = Mathf.Clamp01(0.5f + ((int)dungeonGrade * 0.125f));
        drops.Add(new ItemDropEntry
        {
            item = certificate,
            amount = 1,
            dropChance = chance
        });
        variant.itemDrops = drops.ToArray();
    }

    private static string GetSpecialVariantTitle(
        EnemySkillType skill,
        bool isBossVariant)
    {
        if (isBossVariant)
        {
            return "異形の";
        }

        switch (skill)
        {
            case EnemySkillType.PowerStrike: return "凶暴な";
            case EnemySkillType.VenomStrike: return "猛毒の";
            case EnemySkillType.ParalyzingRoar: return "震声の";
            case EnemySkillType.CriticalFocus: return "鋭眼の";
            case EnemySkillType.DoubleStrike: return "迅撃の";
            case EnemySkillType.LifeDrain: return "吸命の";
            case EnemySkillType.ArmorPierce: return "破甲の";
            case EnemySkillType.FlameBreath: return "炎息の";
            case EnemySkillType.FrostBite: return "氷牙の";
            case EnemySkillType.TripleStrike: return "裂爪の";
            case EnemySkillType.BattleHeal: return "再生する";
            case EnemySkillType.SacrificialStrike: return "狂戦の";
            case EnemySkillType.Execute: return "処刑者の";
            case EnemySkillType.ToxicCloud: return "瘴気の";
            default: return "変異した";
        }
    }

    private static void ApplySpecialSkillStatBonus(EnemyDataSO variant)
    {
        switch (variant.enemySkill)
        {
            case EnemySkillType.PowerStrike:
                variant.attack = Mathf.RoundToInt(variant.attack * 1.15f);
                break;
            case EnemySkillType.VenomStrike:
                variant.attackSpeed *= 1.1f;
                break;
            case EnemySkillType.ParalyzingRoar:
                variant.defense = Mathf.RoundToInt(variant.defense * 1.15f);
                break;
            case EnemySkillType.CriticalFocus:
                variant.criticalRate += 0.2f;
                break;
            case EnemySkillType.DoubleStrike:
                variant.attackSpeed *= 1.2f;
                break;
            case EnemySkillType.LifeDrain:
                variant.maxHP = Mathf.RoundToInt(variant.maxHP * 1.2f);
                break;
            case EnemySkillType.ArmorPierce:
                variant.attack = Mathf.RoundToInt(variant.attack * 1.1f);
                break;
            case EnemySkillType.FlameBreath:
                variant.maxMagicPower += 25;
                break;
            case EnemySkillType.FrostBite:
                variant.defense = Mathf.RoundToInt(variant.defense * 1.12f);
                break;
            case EnemySkillType.TripleStrike:
                variant.attackSpeed *= 1.15f;
                break;
            case EnemySkillType.BattleHeal:
                variant.maxHP = Mathf.RoundToInt(variant.maxHP * 1.25f);
                break;
            case EnemySkillType.SacrificialStrike:
                variant.attack = Mathf.RoundToInt(variant.attack * 1.18f);
                break;
            case EnemySkillType.Execute:
                variant.criticalRate += 0.12f;
                break;
            case EnemySkillType.ToxicCloud:
                variant.maxMagicPower += 20;
                variant.attackSpeed *= 1.08f;
                break;
        }
    }
}
