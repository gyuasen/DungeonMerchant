using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class MercenarySkillDefinitionRefactorTests
{
    [TestCase(MercenaryClass.Warrior, 110, 20, 13, 1.00f)]
    [TestCase(MercenaryClass.Archer, 100, 20, 10, 1.05f)]
    [TestCase(MercenaryClass.Mage, 100, 24, 10, 1.00f)]
    public void LevelTwoPassive_AppliesExactlyOnce(
        MercenaryClass mercenaryClass,
        int expectedHp,
        int expectedAttack,
        int expectedDefense,
        float expectedSpeed)
    {
        MercenaryInstance levelOne = CreateRestored(mercenaryClass, 1);
        MercenaryInstance levelTwo = CreateRestored(mercenaryClass, 2);

        Assert.That(levelOne.MaxHP, Is.EqualTo(100));
        Assert.That(levelOne.Attack, Is.EqualTo(20));
        Assert.That(levelOne.Defense, Is.EqualTo(10));
        Assert.That(levelOne.AttackSpeed, Is.EqualTo(1f));
        Assert.That(levelTwo.MaxHP, Is.EqualTo(expectedHp));
        Assert.That(levelTwo.Attack, Is.EqualTo(expectedAttack));
        Assert.That(levelTwo.Defense, Is.EqualTo(expectedDefense));
        Assert.That(levelTwo.AttackSpeed,
            Is.EqualTo(expectedSpeed).Within(0.0001f));
    }

    [TestCase(MercenaryClass.Knight, 110, 20, 13, 1.00f)]
    [TestCase(MercenaryClass.Sniper, 100, 20, 10, 1.05f)]
    [TestCase(MercenaryClass.Sage, 100, 24, 10, 1.00f)]
    public void AdvancedClass_LevelTwoPassive_UsesOriginalBaseClassParity(
        MercenaryClass mercenaryClass,
        int expectedHp,
        int expectedAttack,
        int expectedDefense,
        float expectedSpeed)
    {
        MercenaryInstance mercenary = CreateRestored(mercenaryClass, 2);

        Assert.That(mercenary.OriginalClass,
            Is.EqualTo(MercenaryClassProgression.GetBaseClass(mercenaryClass)));
        Assert.That(mercenary.MaxHP, Is.EqualTo(expectedHp));
        Assert.That(mercenary.Attack, Is.EqualTo(expectedAttack));
        Assert.That(mercenary.Defense, Is.EqualTo(expectedDefense));
        Assert.That(mercenary.AttackSpeed,
            Is.EqualTo(expectedSpeed).Within(0.0001f));
    }

    [TestCase(MercenaryClass.Warrior, "挑発", "敵の攻撃を自分に引きつけます。ダメージを与えるスキルではありませんが、味方を守りたい場面で有効です。", "基礎体力", "最大HPが10、防御が3上昇します。前衛として長く戦えるようになります。")]
    [TestCase(MercenaryClass.Archer, "連射", "攻撃力を少し下げた射撃を2回行います。通常攻撃より有効な対象がいる場合に自動発動します。", "速射訓練", "攻撃速度が0.05上昇します。行動順が早くなり、魔力の回復機会も増えやすくなります。")]
    [TestCase(MercenaryClass.Mage, "火球", "敵1体に高威力の魔法攻撃を行います。通常攻撃では倒しきれない相手への決定打になります。", "魔力集中", "攻撃が4上昇します。通常攻撃と火球の両方の威力が上がります。")]
    public void SkillInfos_UseDefinitionDisplayTextAndUnlockState(
        MercenaryClass mercenaryClass,
        string primaryName,
        string primaryDescription,
        string passiveName,
        string passiveDescription)
    {
        List<MercenarySkillInfo> levelOne =
            CharacterEquipmentController.GetMercenarySkillInfos(
                CreateRestored(mercenaryClass, 1));
        List<MercenarySkillInfo> levelTwo =
            CharacterEquipmentController.GetMercenarySkillInfos(
                CreateRestored(mercenaryClass, 2));

        Assert.That(levelOne[0].Name, Is.EqualTo(primaryName));
        Assert.That(levelOne[0].DetailDescription, Is.EqualTo(primaryDescription));
        MercenarySkillInfo locked = levelOne.Find(skill => skill.Name == passiveName);
        MercenarySkillInfo unlocked = levelTwo.Find(skill => skill.Name == passiveName);
        Assert.That(locked.ShortDescription, Is.EqualTo("未習得 / Lv2"));
        Assert.That(locked.Unlocked, Is.False);
        Assert.That(locked.DetailDescription, Is.EqualTo(passiveDescription));
        Assert.That(unlocked.ShortDescription, Is.EqualTo("パッシブ / Lv2"));
        Assert.That(unlocked.Unlocked, Is.True);
        Assert.That(unlocked.DetailDescription, Is.EqualTo(passiveDescription));
    }

    [Test]
    public void UniqueSkillBonus_CombinesWithLevelTwoPassive()
    {
        MercenaryDataSO data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryClass = MercenaryClass.Warrior;
        data.maxHP = 100;
        data.attack = 20;
        data.defense = 10;
        data.attackSpeed = 1f;
        data.uniqueSkillUnlockLevel = 2;
        data.uniqueSkillBonusMaxHP = 7;
        data.uniqueSkillBonusAttack = 5;
        data.uniqueSkillBonusDefense = 2;
        data.uniqueSkillBonusAttackSpeed = 0.02f;
        MercenaryInstance mercenary = new MercenaryInstance(data);
        mercenary.AddExperience(mercenary.ExperienceToNextLevel);

        Assert.That(mercenary.MaxHP, Is.EqualTo(129));
        Assert.That(mercenary.Attack, Is.EqualTo(27));
        Assert.That(mercenary.Defense, Is.EqualTo(17));
        Assert.That(mercenary.AttackSpeed, Is.EqualTo(1.03f).Within(0.0001f));
        Object.DestroyImmediate(data);
    }

    private static MercenaryInstance CreateRestored(
        MercenaryClass mercenaryClass,
        int level)
    {
        return MercenaryInstance.CreateRestored("test", null, null, "test",
            mercenaryClass, MercenaryContractType.Local, level, 0,
            100, 100, 20, 10, 60, 1f, 0);
    }
}
