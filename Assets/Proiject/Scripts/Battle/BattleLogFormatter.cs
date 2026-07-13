/// <summary>
/// Pure formatting for battle log messages. This class deliberately has no
/// Unity or battle-state dependency so message text can be characterized in
/// EditMode without running a battle coroutine.
/// </summary>
public static class BattleLogFormatter
{
    public static string FormatBattleStart(int playerCount, int enemyCount)
    {
        return $"戦闘開始: 傭兵{playerCount}人 vs 敵{enemyCount}体";
    }

    public static string FormatPoisonDamage(
        string unitName, int damage, int currentHp, int maxHp)
    {
        return $"{unitName}は毒で{damage}ダメージ。 " +
               $"HP {currentHp}/{maxHp}";
    }

    public static string FormatParalysis(string unitName)
    {
        return $"{unitName}は麻痺して行動できません。";
    }

    public static string FormatEvadedAttack(string targetName, string attackerName)
    {
        return $"{targetName}は{attackerName}の攻撃を回避しました。";
    }

    public static string FormatAttack(
        string attackerName,
        string targetName,
        bool critical,
        int damage,
        int currentHp,
        int maxHp)
    {
        return $"{attackerName}が{targetName}を攻撃: " +
               (critical ? "クリティカル！ " : string.Empty) +
               $"{damage}ダメージ、HP {currentHp}/{maxHp}";
    }

    public static string FormatDefeat()
    {
        return "敗北しました。";
    }

    public static string FormatPostBattleHp(
        string mercenaryName, int currentHp, int maxHp, string statusSummary)
    {
        return $"{mercenaryName}の戦闘後HP: " +
               $"{currentHp}/{maxHp}  状態: {statusSummary}";
    }

    public static string FormatVictoryGold(int gold)
    {
        return $"勝利！ 報酬: {gold} G";
    }

    public static string FormatLevelCap(string mercenaryName, int levelCap)
    {
        return $"{mercenaryName}は" +
               $"レベル上限（Lv{levelCap}）に到達しています。";
    }

    public static string FormatExperience(
        string mercenaryName, int experience, int currentExperience, int toNextLevel)
    {
        return $"{mercenaryName}が経験値{experience}を獲得 " +
               $"({currentExperience}/{toNextLevel})";
    }

    public static string FormatLevelUp(
        string mercenaryName, int previousLevel, int currentLevel)
    {
        return $"{mercenaryName}がレベル{previousLevel}から" +
               $"レベル{currentLevel}に上昇！";
    }

    public static string FormatItemDrop(string itemName, int amount)
    {
        return $"戦利品: {itemName} x{amount}";
    }

    public static string FormatSkillActivation(string attacker, string skill, string detail)
    {
        return $"{attacker}がスキル「{skill}」を発動: {detail}";
    }

    public static string FormatSkillEvaded(string attacker, string skill, string target)
    {
        return $"{attacker}の「{skill}」を{target}が回避しました。";
    }

    public static string FormatSkillActivationNoDetail(string attacker, string skill)
    {
        return $"{attacker}がスキル「{skill}」を発動。";
    }

    public static string FormatPureDamageSkill(
        string attacker, string skill, bool critical, string target, int damage)
    {
        return FormatSkillActivation(attacker, skill,
            (critical ? "クリティカル！ " : string.Empty) +
            $"{target}へ防御無視の{damage}ダメージ。");
    }

    public static string FormatAreaDamageSkill(
        string attacker, string skill, int affected, int damage, string status)
    {
        return FormatSkillActivation(attacker, skill,
            $"{affected}人へ合計{damage}ダメージ" +
            (string.IsNullOrEmpty(status) ? "。" : $"、{status}を付与。"));
    }

    public static string FormatMultiStrikeSkill(
        string attacker, string skill, int landedHits, int hitCount, int damage)
    {
        return FormatSkillActivation(attacker, skill,
            $"{landedHits}/{hitCount}回命中、合計{damage}ダメージ。");
    }

    public static string FormatHealSkill(string attacker, string skill, int healed)
    {
        return FormatSkillActivation(attacker, skill, $"HPを{healed}回復。");
    }

    public static string FormatRecoil(string attacker, int damage)
    {
        return $"{attacker}は反動で{damage}ダメージ。";
    }

    public static string FormatLifeDrain(
        string attacker, bool critical, int damage, int heal)
    {
        return FormatSkillActivation(attacker, "吸命",
            (critical ? "クリティカル！ " : string.Empty) +
            $"{damage}ダメージを与え、HPを{heal}回復。");
    }

    public static string FormatTaunt(
        string attacker, string status, int magic, int maxMagic)
    {
        return FormatSkillActivation(attacker, "挑発",
            $"敵の攻撃を引きつけます。状態: {status}。 魔力 {magic}/{maxMagic}");
    }

    public static string FormatDoubleShot(
        string attacker, int damage, int criticalCount, int magic, int maxMagic)
    {
        return FormatSkillActivation(attacker, "連射",
            $"合計{damage}ダメージ" +
            (criticalCount > 0 ? $"（クリティカル{criticalCount}回）" : string.Empty) +
            $"。次の2行動はクリティカル率+15%。 魔力 {magic}/{maxMagic}");
    }

    public static string FormatSkillEvadedWithMagic(
        string attacker, string skill, string target, int magic, int maxMagic)
    {
        return $"{attacker}がスキル「{skill}」を発動しましたが、" +
               $"{target}が回避しました。 魔力 {magic}/{maxMagic}";
    }

    public static string FormatDamageSkillWithMagic(
        string attacker, string skill, bool critical, string target, int damage,
        int magic, int maxMagic)
    {
        return FormatSkillActivation(attacker, skill,
            (critical ? "クリティカル！ " : string.Empty) +
            $"{target}に{damage}ダメージ。 魔力 {magic}/{maxMagic}");
    }

    public static string FormatHealSkillWithMagic(
        string attacker, string skill, string target, int healed, int magic, int maxMagic)
    {
        return FormatSkillActivation(attacker, skill,
            $"{target}のHPを{healed}回復。 魔力 {magic}/{maxMagic}");
    }

    public static string FormatStatusDamageSkill(
        string attacker, string skill, int damage, string status)
    {
        return FormatSkillActivation(attacker, skill,
            $"{damage}ダメージ、{status}を付与。");
    }

    public static string FormatPureDamageSkillSimple(
        string attacker, string skill, int damage)
    {
        return FormatSkillActivation(attacker, skill,
               $"防御を貫通して{damage}ダメージ。");
    }

    public static string FormatDamageSkill(
        string attacker, string skill, bool critical, string target, int damage,
        string status)
    {
        return FormatSkillActivation(attacker, skill,
            (critical ? "クリティカル！ " : string.Empty) +
            $"{target}に{damage}ダメージ" +
            (string.IsNullOrEmpty(status) ? "。" : $"、{status}を付与。"));
    }
}
