using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterCodexPresenter
{
    private readonly MonsterCodexManager monsterCodexManager;

    public MonsterCodexPresenter(MonsterCodexManager monsterCodexManager)
    {
        this.monsterCodexManager = monsterCodexManager;
    }

    public List<BookPageUI.Entry> BuildEntries()
    {
        List<EnemyDataSO> enemies = new List<EnemyDataSO>();
        foreach (EnemyDataSO enemy in GameAssetRepository.LoadAll<EnemyDataSO>())
        {
            if (enemy != null && !enemy.isSpecialVariant &&
                (enemy.hideFlags & HideFlags.DontSave) == 0)
            {
                enemies.Add(enemy);
            }
        }

        enemies.Sort((left, right) => left.monsterGrade.CompareTo(right.monsterGrade));
        List<BookPageUI.Entry> entries = new List<BookPageUI.Entry>();
        foreach (EnemyDataSO enemy in enemies)
        {
            entries.Add(new BookPageUI.Entry
            {
                Name = JapaneseDisplayText.GetEnemyName(enemy.enemyName),
                Detail = BuildDetail(enemy),
                Sprite = EnemySpriteResolver.Resolve(enemy),
                Discovered = monsterCodexManager != null &&
                    monsterCodexManager.HasEncountered(enemy)
            });
        }

        return entries;
    }

    private static string BuildDetail(EnemyDataSO enemy)
    {
        List<string> drops = new List<string>();
        int validDropCount = 0;
        if (enemy.itemDrops != null)
        {
            foreach (ItemDropEntry drop in enemy.itemDrops)
            {
                if (drop != null && drop.item != null)
                {
                    validDropCount++;
                    if (drops.Count < 2)
                    {
                        drops.Add(JapaneseDisplayText.GetItemName(drop.item));
                    }
                }
            }
        }

        return string.Format(
            "{0} / {1}\nHP {2}  攻 {3}  防 {4}  速 {5:0.##}\nドロップ: {6}",
            JapaneseDisplayText.GetMonsterGradeWithStrengthHint(enemy),
            JapaneseDisplayText.GetMonsterCategory(enemy.category),
            enemy.maxHP,
            enemy.attack,
            enemy.defense,
            enemy.attackSpeed,
            drops.Count == 0
                ? "なし"
                : string.Join("、", drops) +
                    (validDropCount > drops.Count ? "\u2026" : string.Empty));
    }
}
