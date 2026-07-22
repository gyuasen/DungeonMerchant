using NUnit.Framework;
using UnityEngine;

public sealed class PromotionPreviewTests
{
    [Test]
    public void Preview_MatchesPromotionWithoutMutatingSource()
    {
        MercenaryDataSO data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryClass = MercenaryClass.Warrior;
        MercenaryInstance mercenary = new MercenaryInstance(data);
        mercenary.AddExperience(1000000);
        int hpBefore = mercenary.MaxHP;
        int attackBefore = mercenary.Attack;
        int defenseBefore = mercenary.Defense;
        int magicBefore = mercenary.MaxMagicPower;
        float speedBefore = mercenary.AttackSpeed;
        PromotionPreview preview = new PromotionPreview(mercenary, MercenaryClass.Knight);

        Assert.That(mercenary.MaxHP, Is.EqualTo(hpBefore));
        Assert.That(mercenary.Attack, Is.EqualTo(attackBefore));
        Assert.That(mercenary.Defense, Is.EqualTo(defenseBefore));
        Assert.That(mercenary.MaxMagicPower, Is.EqualTo(magicBefore));
        Assert.That(mercenary.AttackSpeed, Is.EqualTo(speedBefore));
        Assert.That(mercenary.PromoteTo(MercenaryClass.Knight), Is.True);
        Assert.That(mercenary.MaxHP, Is.EqualTo(preview.MaxHP));
        Assert.That(mercenary.Attack, Is.EqualTo(preview.Attack));
        Assert.That(mercenary.Defense, Is.EqualTo(preview.Defense));
        Assert.That(mercenary.MaxMagicPower, Is.EqualTo(preview.MaxMagicPower));
        Assert.That(mercenary.AttackSpeed, Is.EqualTo(preview.AttackSpeed).Within(0.0001f));
        Assert.That(mercenary.CriticalRate, Is.EqualTo(preview.CriticalRate).Within(0.0001f));
        Assert.That(mercenary.EvasionRate, Is.EqualTo(preview.EvasionRate).Within(0.0001f));

        Object.DestroyImmediate(data);
    }

    [Test]
    public void Preview_MatchesPromotionForEveryBaseClassAndTarget()
    {
        foreach (MercenaryClass baseClass in MercenaryClassProgression.GetBaseClasses())
        {
            MercenaryClass[] targets = MercenaryClassProgression.GetAdvancedClasses(baseClass);
            foreach (MercenaryClass target in targets)
            {
                AssertPreviewMatchesPromotion(baseClass, target);
            }
            AssertPreviewMatchesPromotion(
                baseClass,
                MercenaryClassProgression.GetSpecialClass(baseClass));
        }
    }

    private static void AssertPreviewMatchesPromotion(
        MercenaryClass baseClass,
        MercenaryClass target)
    {
        MercenaryDataSO data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryClass = baseClass;
        MercenaryInstance mercenary = new MercenaryInstance(data);
        mercenary.AddExperience(1000000);
        PromotionPreview preview = new PromotionPreview(mercenary, target);

        Assert.That(mercenary.PromoteTo(target), Is.True, target.ToString());
        Assert.That(mercenary.MaxHP, Is.EqualTo(preview.MaxHP), target.ToString());
        Assert.That(mercenary.Attack, Is.EqualTo(preview.Attack), target.ToString());
        Assert.That(mercenary.Defense, Is.EqualTo(preview.Defense), target.ToString());
        Assert.That(mercenary.MaxMagicPower, Is.EqualTo(preview.MaxMagicPower), target.ToString());
        Assert.That(mercenary.AttackSpeed, Is.EqualTo(preview.AttackSpeed).Within(0.0001f), target.ToString());
        Assert.That(mercenary.CriticalRate, Is.EqualTo(preview.CriticalRate).Within(0.0001f), target.ToString());
        Assert.That(mercenary.EvasionRate, Is.EqualTo(preview.EvasionRate).Within(0.0001f), target.ToString());

        Object.DestroyImmediate(data);
    }
}
