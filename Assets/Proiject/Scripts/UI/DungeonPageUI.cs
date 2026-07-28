using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class DungeonPageUI : RefreshOnlyPageUIBase
{
    private RectTransform selectionListRoot;
    private Font rowFont;
    private Color rowTextColor = Color.white;
    private Color mutedTextColor = Color.gray;
    private Color rowColor = UITheme.RowColor;
    private Color buttonColor = UITheme.WoodButtonColor;
    private Color frameColor = UITheme.FrameColor;
    private Color buttonTextColor = Color.white;
    private UnityAction selectionRefreshAction;
    private Func<IEnumerable<DungeonDataSO>> dungeonProvider;
    private Func<int> currentTownIndexProvider;
    private Func<int, string> townNameProvider;
    private Func<DungeonDataSO, int> clearedFloorsProvider;
    private Func<DungeonDataSO, bool> dungeonUnlockedProvider;
    private Func<DungeonDataSO> selectedDungeonProvider;
    private Action<DungeonDataSO> selectDungeonAction;
    private Func<DungeonDataSO, bool> canExpeditionProvider;
    private Func<DungeonDataSO, bool> hasExpeditionProvider;
    private Action<DungeonDataSO> expeditionAction;

    public void ConfigureSelectionRefresh(UnityAction refresh)
    {
        selectionRefreshAction = refresh;
    }

    public void ConfigureSelectionList(
        RectTransform targetSelectionListRoot,
        Font targetFont,
        Color targetRowTextColor,
        Color targetMutedTextColor,
        Color targetRowColor,
        Color targetButtonColor,
        Color targetFrameColor,
        Color targetButtonTextColor,
        Func<IEnumerable<DungeonDataSO>> targetDungeonProvider,
        Func<int> targetCurrentTownIndexProvider,
        Func<int, string> targetTownNameProvider,
        Func<DungeonDataSO, int> targetClearedFloorsProvider,
        Func<DungeonDataSO, bool> targetDungeonUnlockedProvider,
        Func<DungeonDataSO> targetSelectedDungeonProvider,
        Action<DungeonDataSO> targetSelectDungeonAction,
        Func<DungeonDataSO, bool> targetCanExpeditionProvider = null,
        Func<DungeonDataSO, bool> targetHasExpeditionProvider = null,
        Action<DungeonDataSO> targetExpeditionAction = null)
    {
        selectionListRoot = targetSelectionListRoot;
        rowFont = targetFont;
        rowTextColor = targetRowTextColor;
        mutedTextColor = targetMutedTextColor;
        rowColor = targetRowColor;
        buttonColor = targetButtonColor;
        frameColor = targetFrameColor;
        buttonTextColor = targetButtonTextColor;
        dungeonProvider = targetDungeonProvider;
        currentTownIndexProvider = targetCurrentTownIndexProvider;
        townNameProvider = targetTownNameProvider;
        clearedFloorsProvider = targetClearedFloorsProvider;
        dungeonUnlockedProvider = targetDungeonUnlockedProvider;
        selectedDungeonProvider = targetSelectedDungeonProvider;
        selectDungeonAction = targetSelectDungeonAction;
        canExpeditionProvider = targetCanExpeditionProvider;
        hasExpeditionProvider = targetHasExpeditionProvider;
        expeditionAction = targetExpeditionAction;
    }

    public void RefreshSelection()
    {
        if (selectionListRoot == null ||
            dungeonProvider == null ||
            currentTownIndexProvider == null)
        {
            selectionRefreshAction?.Invoke();
            return;
        }

        ClearChildren(selectionListRoot);

        int currentTownIndex = currentTownIndexProvider.Invoke();
        float rowTop = 0f;
        bool createdAnyRow = false;
        foreach (DungeonDataSO dungeon in
                 dungeonProvider.Invoke() ?? Array.Empty<DungeonDataSO>())
        {
            if (dungeon == null ||
                dungeon.nearbyTownIndex != currentTownIndex)
            {
                continue;
            }

            CreateDungeonSelectionRow(dungeon, currentTownIndex, rowTop);
            rowTop -= 84f;
            createdAnyRow = true;
        }

        if (!createdAnyRow)
        {
            string townName = GetTownName(currentTownIndex);
            CreateText(
                selectionListRoot,
                $"{townName}近隣に探索可能なダンジョンはありません。",
                rowFont,
                16,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                new Vector2(0f, -110f),
                new Vector2(0f, -40f),
                mutedTextColor);
            selectionListRoot.sizeDelta = new Vector2(0f, 150f);
            return;
        }

        selectionListRoot.sizeDelta = new Vector2(0f, Mathf.Max(150f, -rowTop));
        selectionRefreshAction?.Invoke();
    }

    private void CreateDungeonSelectionRow(
        DungeonDataSO dungeon,
        int currentTownIndex,
        float top)
    {
        RectTransform row = CreateUIObject(dungeon.dungeonName, selectionListRoot);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 78f);
        row.offsetMax = new Vector2(0f, top);

        Image image = row.gameObject.AddComponent<Image>();
        image.color = rowColor;

        string nearbyTown = GetTownName(dungeon.nearbyTownIndex);
        int clearedFloors = clearedFloorsProvider?.Invoke(dungeon) ?? 0;
        int totalFloors = Mathf.Max(1, dungeon.totalFloors);
        string floorProgress = clearedFloors >= totalFloors
            ? $"完全攻略 {totalFloors}/{totalFloors}F"
            : $"次回 {clearedFloors + 1}/{totalFloors}F";
        string details =
            $"{nearbyTown}近隣  |  " +
            $"{JapaneseDisplayText.GetDungeonGrade(dungeon.grade)}  |  " +
            $"{dungeon.dungeonName}  |  " +
            $"{floorProgress}  |  {GetDungeonEnemyGradeSummary(dungeon)}";
        CreateText(
            row,
            details,
            rowFont,
            14,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(14f, -78f),
            new Vector2(-130f, -40f),
            rowTextColor);

        bool unlocked = dungeonUnlockedProvider?.Invoke(dungeon) == true;
        bool selected =
            ReferenceEquals(selectedDungeonProvider?.Invoke(), dungeon);
        string label = selected ? "選択中" : unlocked ? "選択" : "未開放";
        Button button = CreateActionButton(
            row,
            label,
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => selectDungeonAction?.Invoke(dungeon));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        button.interactable = unlocked && !selected &&
                              dungeon.nearbyTownIndex == currentTownIndex;

        bool canExpedition = canExpeditionProvider?.Invoke(dungeon) == true;
        bool hasExpedition = hasExpeditionProvider?.Invoke(dungeon) == true;
        bool showExpedition = canExpedition || hasExpedition;

        // 別動隊ボタンがある行では、選択ボタンを上・別動隊ボタンを下に分けて
        // 縦に並べ、重ならないようにする。別動隊が無い行は選択ボタンを縦中央に。
        if (showExpedition)
        {
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-8f, -6f);
            buttonRect.sizeDelta = new Vector2(142f, 30f);
        }
        else
        {
            buttonRect.sizeDelta = new Vector2(108f, 34f);
        }

        if (showExpedition)
        {
            Button expeditionButton = CreateActionButton(
                row,
                hasExpedition ? "別動隊 周回中" : "別動隊を送る",
                rowFont,
                buttonColor,
                frameColor,
                buttonTextColor,
                () => expeditionAction?.Invoke(dungeon));
            RectTransform expeditionRect = expeditionButton.GetComponent<RectTransform>();
            expeditionRect.anchorMin = new Vector2(1f, 0f);
            expeditionRect.anchorMax = new Vector2(1f, 0f);
            expeditionRect.pivot = new Vector2(1f, 0f);
            expeditionRect.anchoredPosition = new Vector2(-8f, 6f);
            expeditionRect.sizeDelta = new Vector2(142f, 30f);
            expeditionButton.interactable = unlocked;
        }
    }

    private string GetTownName(int townIndex)
    {
        if (townNameProvider == null)
        {
            return "町未設定";
        }

        string townName = townNameProvider.Invoke(townIndex);
        return string.IsNullOrEmpty(townName) ? "町未設定" : townName;
    }

    private static string GetDungeonEnemyGradeSummary(DungeonDataSO data)
    {
        List<int> grades = new List<int>();
        if (data.normalEnemies != null)
        {
            foreach (EnemyDataSO enemy in data.normalEnemies)
            {
                if (enemy != null && !grades.Contains(enemy.monsterGrade))
                {
                    grades.Add(enemy.monsterGrade);
                }
            }
        }

        grades.Sort((left, right) => right.CompareTo(left));
        string normalGrades = grades.Count > 0
            ? string.Join("・", grades)
            : "未設定";
        string bossGrade = data.bossEnemy != null
            ? data.bossEnemy.monsterGrade.ToString()
            : "なし";
        return $"通常{normalGrades}等級 / ボス{bossGrade}等級";
    }
}
