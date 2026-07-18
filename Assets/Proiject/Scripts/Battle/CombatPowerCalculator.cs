using System.Collections.Generic;
using UnityEngine;

public static class CombatPowerCalculator
{
    public static int Calculate(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return 0;
        }

        float power = mercenary.Attack * 2f +
                      mercenary.MaxHP * (100f + mercenary.Defense) / 800f +
                      mercenary.AttackSpeed * 35f +
                      GetRoleBonus(mercenary.MercenaryClass);
        return Mathf.Max(0, Mathf.RoundToInt(power));
    }

    public static int Calculate(IEnumerable<MercenaryInstance> mercenaries)
    {
        int total = 0;
        if (mercenaries == null)
        {
            return total;
        }

        foreach (MercenaryInstance mercenary in mercenaries)
        {
            if (mercenary != null && mercenary.CurrentHP > 0)
            {
                total += Calculate(mercenary);
            }
        }
        return total;
    }

    private static int GetRoleBonus(MercenaryClass mercenaryClass)
    {
        List<MercenarySkillDefinition> skills =
            MercenaryClassProgression.GetCombatSkills(mercenaryClass);
        int bonus = HasHealingSkill(skills) ? 80 : 0;
        bonus += HasAreaAttack(skills) ? 50 : 0;
        bonus += HasHighValueSingleTargetSkill(skills) ? 30 : 0;
        return bonus;
    }

    private static bool HasHealingSkill(IEnumerable<MercenarySkillDefinition> skills)
    {
        foreach (MercenarySkillDefinition skill in skills)
        {
            if (skill.Id == MercenarySkillId.Heal ||
                skill.Id == MercenarySkillId.PrayerLight ||
                skill.Id == MercenarySkillId.GreaterHeal ||
                skill.Id == MercenarySkillId.SaintsGrace ||
                skill.Id == MercenarySkillId.DivineHymn)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasAreaAttack(IEnumerable<MercenarySkillDefinition> skills)
    {
        foreach (MercenarySkillDefinition skill in skills)
        {
            if (skill.Id == MercenarySkillId.Volley ||
                skill.Id == MercenarySkillId.FrostNova ||
                skill.Id == MercenarySkillId.SweepingThrust ||
                skill.Id == MercenarySkillId.GaleVolley ||
                skill.Id == MercenarySkillId.StormCircle ||
                skill.Id == MercenarySkillId.BeastPack ||
                skill.Id == MercenarySkillId.WarfrontSmash ||
                skill.Id == MercenarySkillId.DragonBreath ||
                skill.Id == MercenarySkillId.DragonfallBreath)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasHighValueSingleTargetSkill(
        IEnumerable<MercenarySkillDefinition> skills)
    {
        foreach (MercenarySkillDefinition skill in skills)
        {
            if (skill.Power >= 1.2f &&
                skill.Id != MercenarySkillId.GreaterHeal &&
                skill.Id != MercenarySkillId.SaintsGrace &&
                skill.Id != MercenarySkillId.DivineHymn)
            {
                return true;
            }
        }
        return false;
    }
}
