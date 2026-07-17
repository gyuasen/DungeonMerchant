using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildMonsterCollectionOverlay()
    {
        monsterCollectionOverlay = GetOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.MonsterCollection,
            "Monster Collection Overlay");
        monsterCollectionOverlay.gameObject.SetActive(false);
        monsterCollectionOverlay.anchorMin = Vector2.zero;
        monsterCollectionOverlay.anchorMax = Vector2.one;
        monsterCollectionOverlay.offsetMin = Vector2.zero;
        monsterCollectionOverlay.offsetMax = Vector2.zero;
        monsterCollectionOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Monster Collection Window", monsterCollectionOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 560f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        RectTransform bookRoot = CreateUIObject("Monster Codex Book", window);
        bookRoot.anchorMin = Vector2.zero;
        bookRoot.anchorMax = Vector2.one;
        bookRoot.offsetMin = new Vector2(28f, 28f);
        bookRoot.offsetMax = new Vector2(-28f, -82f);
        monsterCodexBook = bookRoot.gameObject.AddComponent<BookPageUI>();
        monsterCodexBook.Initialize("魔物図鑑", uiFont, uiBodyFont);
        Button closeButton = CreateActionButton(window, "閉じる", HideMonsterCollection);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        monsterCollectionOverlay.gameObject.SetActive(false);
    }

    private void ShowMonsterCollection()
    {
        MonsterCodexManager codex = GetComponent<MonsterCodexManager>() ?? FindObjectOfType<MonsterCodexManager>();
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
            bool discovered = codex != null && codex.HasEncountered(enemy);
            entries.Add(new BookPageUI.Entry
            {
                Name = JapaneseDisplayText.GetEnemyName(enemy.enemyName),
                Detail = BuildMonsterCodexDetail(enemy),
                Sprite = GetMonsterSprite(enemy),
                Discovered = discovered
            });
        }

        monsterCodexBook.SetEntries(entries);
        monsterCollectionOverlay.SetAsLastSibling();
        monsterCollectionOverlay.gameObject.SetActive(true);
    }

    private static Sprite GetMonsterSprite(EnemyDataSO enemy)
    {
        if (enemy.battleSprite != null)
        {
            return enemy.battleSprite;
        }

        if (string.IsNullOrWhiteSpace(enemy.battleVisualKey))
        {
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>(enemy.battleVisualKey);
        return sprite != null
            ? sprite
            : Resources.Load<Sprite>("Battle/Enemies/" + enemy.battleVisualKey);
    }

    private static string BuildMonsterCodexDetail(EnemyDataSO enemy)
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
            JapaneseDisplayText.GetMonsterGrade(enemy),
            enemy.category,
            enemy.maxHP,
            enemy.attack,
            enemy.defense,
            enemy.attackSpeed,
            drops.Count == 0
                ? "なし"
                : string.Join("、", drops) + (validDropCount > drops.Count ? "…" : string.Empty));
    }

    private void HideMonsterCollection()
    {
        monsterCollectionOverlay.gameObject.SetActive(false);
    }
}
