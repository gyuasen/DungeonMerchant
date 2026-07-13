using NUnit.Framework;

public sealed class BattleLogFormatterTests
{
    [Test]
    public void FormatBattleStart_PreservesExistingText()
    {
        Assert.That(
            BattleLogFormatter.FormatBattleStart(2, 3),
            Is.EqualTo("戦闘開始: 傭兵2人 vs 敵3体"));
    }

    [Test]
    public void FormatAttack_PreservesCriticalAndHpText()
    {
        Assert.That(
            BattleLogFormatter.FormatAttack("A", "B", true, 12, 8, 20),
            Is.EqualTo("AがBを攻撃: クリティカル！ 12ダメージ、HP 8/20"));
    }

    [Test]
    public void FormatStatusAndRewards_ArePureAndExact()
    {
        Assert.That(BattleLogFormatter.FormatPoisonDamage("A", 3, 7, 20),
            Is.EqualTo("Aは毒で3ダメージ。 HP 7/20"));
        Assert.That(BattleLogFormatter.FormatParalysis("A"),
            Is.EqualTo("Aは麻痺して行動できません。"));
        Assert.That(BattleLogFormatter.FormatVictoryGold(50),
            Is.EqualTo("勝利！ 報酬: 50 G"));
        Assert.That(BattleLogFormatter.FormatItemDrop("薬草", 2),
            Is.EqualTo("戦利品: 薬草 x2"));
    }

    [Test]
    public void FormatRepresentativeSkillMessages_PreserveExistingText()
    {
        Assert.That(
            BattleLogFormatter.FormatAreaDamageSkill("敵", "毒霧", 2, 14, "毒"),
            Is.EqualTo("敵がスキル「毒霧」を発動: 2人へ合計14ダメージ、毒を付与。"));
        Assert.That(
            BattleLogFormatter.FormatMultiStrikeSkill("敵", "三連爪", 2, 3, 18),
            Is.EqualTo("敵がスキル「三連爪」を発動: 2/3回命中、合計18ダメージ。"));
        Assert.That(
            BattleLogFormatter.FormatHealSkillWithMagic("僧侶", "回復", "戦士", 9, 20, 100),
            Is.EqualTo("僧侶がスキル「回復」を発動: 戦士のHPを9回復。 魔力 20/100"));
        Assert.That(
            BattleLogFormatter.FormatDamageSkill("敵", "氷結牙", true, "戦士", 11, "麻痺"),
            Is.EqualTo("敵がスキル「氷結牙」を発動: クリティカル！ 戦士に11ダメージ、麻痺を付与。"));
        Assert.That(
            BattleLogFormatter.FormatSkillEvaded("戦士", "毒刃", "敵"),
            Is.EqualTo("戦士の「毒刃」を敵が回避しました。"));
    }
}
