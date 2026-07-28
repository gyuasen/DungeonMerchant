using UnityEngine;

public readonly struct PromotionPreview
{
    public readonly struct StatDelta
    {
        public const int MaxHP = 15;
        public const int Attack = 5;
        public const int Defense = 3;
        public const int MaxMagicPower = 15;
        public const float AttackSpeed = 0.04f;
    }

    public PromotionPreview(MercenaryInstance mercenary, MercenaryClass targetClass)
    {
        TargetClass = targetClass;
        MaxHP = mercenary.MaxHP + StatDelta.MaxHP + GetProgressionDifference(mercenary, targetClass, skill => skill.BonusMaxHP);
        Attack = mercenary.Attack + StatDelta.Attack + GetProgressionDifference(mercenary, targetClass, skill => skill.BonusAttack);
        Defense = mercenary.Defense + StatDelta.Defense + GetProgressionDifference(mercenary, targetClass, skill => skill.BonusDefense);
        MaxMagicPower = mercenary.MaxMagicPower + StatDelta.MaxMagicPower + GetProgressionDifference(mercenary, targetClass, skill => skill.BonusMaxMagicPower);
        AttackSpeed = mercenary.AttackSpeed + StatDelta.AttackSpeed + GetProgressionDifferenceFloat(mercenary, targetClass, skill => skill.BonusAttackSpeed);
        CriticalRate = mercenary.CriticalRate + GetProgressionDifferenceFloat(mercenary, targetClass, skill => skill.BonusCriticalRate);
        EvasionRate = mercenary.EvasionRate + GetProgressionDifferenceFloat(mercenary, targetClass, skill => skill.BonusEvasionRate);
        LevelCap = MercenaryClassProgression.GetLevelCap(targetClass);
    }

    public MercenaryClass TargetClass { get; }
    public int MaxHP { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int MaxMagicPower { get; }
    public float AttackSpeed { get; }
    public float CriticalRate { get; }
    public float EvasionRate { get; }
    public int LevelCap { get; }

    public static void ApplyBasePromotion(MercenaryInstance mercenary)
    {
        mercenary.ApplyPromotionBaseStats(
            StatDelta.MaxHP,
            StatDelta.Attack,
            StatDelta.Defense,
            StatDelta.MaxMagicPower,
            StatDelta.AttackSpeed);
    }

    private static int GetProgressionDifference(MercenaryInstance mercenary, MercenaryClass target, System.Func<MercenarySkillDefinition, int> selector)
    {
        return MercenaryClassProgression.GetPassiveBonus(target, mercenary.Level, selector) -
               MercenaryClassProgression.GetPassiveBonus(mercenary.MercenaryClass, mercenary.Level, selector);
    }

    private static float GetProgressionDifferenceFloat(MercenaryInstance mercenary, MercenaryClass target, System.Func<MercenarySkillDefinition, float> selector)
    {
        return MercenaryClassProgression.GetPassiveBonusFloat(target, mercenary.Level, selector) -
               MercenaryClassProgression.GetPassiveBonusFloat(mercenary.MercenaryClass, mercenary.Level, selector);
    }
}
