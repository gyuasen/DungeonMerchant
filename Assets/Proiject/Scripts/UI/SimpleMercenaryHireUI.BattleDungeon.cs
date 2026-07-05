using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildBattlePage()
    {
        battlePageTitleText = CreateText(
            battlePage, "ダンジョン戦闘", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), ParchmentMutedColor);

        battleEncounterText = CreateText(
            battlePage, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f),
            ParchmentTextColor);

        startBattleButton = CreateActionButton(
            battlePage,
            "開始",
            StartPartyBattle);
        RectTransform startRect = startBattleButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);
        startBattleButton.gameObject.SetActive(false);

        battleSpeedButton =
            CreateActionButton(battlePage, "速度 x1", CycleBattleSpeed);
        RectTransform battleSpeedRect =
            battleSpeedButton.GetComponent<RectTransform>();
        battleSpeedRect.anchorMin = battleSpeedRect.anchorMax =
            new Vector2(1f, 1f);
        battleSpeedRect.pivot = new Vector2(1f, 1f);
        battleSpeedRect.sizeDelta = new Vector2(100f, 38f);
        battleSpeedRect.anchoredPosition = new Vector2(-140f, -36f);

        battleLogPanel = CreateUIObject("Battle Log", battlePage);
        battleLogPanel.anchorMin = new Vector2(0f, 0f);
        battleLogPanel.anchorMax = new Vector2(1f, 1f);
        battleLogPanel.offsetMin = Vector2.zero;
        battleLogPanel.offsetMax = new Vector2(0f, -104f);

        Image logBackground = battleLogPanel.gameObject.AddComponent<Image>();
        logBackground.color = RowColor;

        battleLogViewport =
            CreateUIObject("Battle Log Viewport", battleLogPanel);
        battleLogViewport.anchorMin = Vector2.zero;
        battleLogViewport.anchorMax = Vector2.one;
        battleLogViewport.offsetMin = new Vector2(16f, 16f);
        battleLogViewport.offsetMax = new Vector2(-16f, -16f);

        Image viewportImage = battleLogViewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = battleLogViewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        battleLogContent = CreateUIObject("Battle Log Content", battleLogViewport);
        battleLogContent.anchorMin = new Vector2(0f, 1f);
        battleLogContent.anchorMax = new Vector2(1f, 1f);
        battleLogContent.pivot = new Vector2(0.5f, 1f);
        battleLogContent.anchoredPosition = Vector2.zero;
        battleLogContent.sizeDelta = new Vector2(0f, 430f);

        battleLogScrollRect = battleLogViewport.gameObject.AddComponent<ScrollRect>();
        battleLogScrollRect.content = battleLogContent;
        battleLogScrollRect.viewport = battleLogViewport;
        battleLogScrollRect.horizontal = false;
        battleLogScrollRect.vertical = true;
        battleLogScrollRect.movementType = ScrollRect.MovementType.Clamped;
        battleLogScrollRect.scrollSensitivity = 28f;

        battleLogText = CreateText(battleLogContent, "戦闘準備完了。", 14, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(16f, 16f), new Vector2(-16f, -16f),
            MutedTextColor);
        battleLogText.supportRichText = true;
        battleLogText.rectTransform.anchorMin = Vector2.zero;
        battleLogText.rectTransform.anchorMax = Vector2.one;
        battleLogText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        battleLogText.rectTransform.offsetMin = new Vector2(0f, 8f);
        battleLogText.rectTransform.offsetMax = new Vector2(0f, -8f);

        BattlePageUI pageUI =
            battlePage.GetComponent<BattlePageUI>() ??
            battlePage.gameObject.AddComponent<BattlePageUI>();
        pageUI.Configure(RefreshBattlePage);
        pageRouter.Register(battlePage);
    }

    private void BuildRoadBattlePage()
    {
        CreateText(
            roadBattlePage,
            "街道戦闘",
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -38f),
            new Vector2(0f, 0f),
            ParchmentTextColor);

        roadBattleRouteText = CreateText(
            roadBattlePage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -82f),
            new Vector2(0f, -42f),
            ParchmentTextColor);

        roadSpeedButton =
            CreateActionButton(roadBattlePage, "速度 x1", CycleBattleSpeed);
        RectTransform roadSpeedRect =
            roadSpeedButton.GetComponent<RectTransform>();
        roadSpeedRect.anchorMin = roadSpeedRect.anchorMax =
            new Vector2(1f, 1f);
        roadSpeedRect.pivot = new Vector2(1f, 1f);
        roadSpeedRect.sizeDelta = new Vector2(100f, 38f);
        roadSpeedRect.anchoredPosition = new Vector2(-270f, -4f);

        roadContinueButton =
            CreateActionButton(
                roadBattlePage,
                "次へ進む",
                ContinueTownTravel);
        RectTransform continueRect =
            roadContinueButton.GetComponent<RectTransform>();
        continueRect.anchorMin = continueRect.anchorMax =
            new Vector2(1f, 1f);
        continueRect.pivot = new Vector2(1f, 1f);
        continueRect.sizeDelta = new Vector2(120f, 40f);
        continueRect.anchoredPosition = new Vector2(-130f, -4f);

        roadRetreatButton =
            CreateActionButton(
                roadBattlePage,
                "撤退する",
                RetreatFromTownTravel);
        RectTransform retreatRect =
            roadRetreatButton.GetComponent<RectTransform>();
        retreatRect.anchorMin = retreatRect.anchorMax =
            new Vector2(1f, 1f);
        retreatRect.pivot = new Vector2(1f, 1f);
        retreatRect.sizeDelta = new Vector2(120f, 40f);
        retreatRect.anchoredPosition = new Vector2(0f, -4f);
        roadRetreatButton.targetGraphic.color = ImportantButtonColor;

        roadContinueButton.gameObject.SetActive(false);
        roadRetreatButton.gameObject.SetActive(false);

        RoadBattlePageUI pageUI =
            roadBattlePage.GetComponent<RoadBattlePageUI>() ??
            roadBattlePage.gameObject.AddComponent<RoadBattlePageUI>();
        pageUI.Configure(RefreshRoadBattlePage);
        pageRouter.Register(roadBattlePage);
    }

    private void BuildDungeonPage()
    {
        CreateText(dungeonPage, "ダンジョン探索", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(0f, -30f), new Vector2(0f, 0f),
            ParchmentMutedColor);

        dungeonStatusText = CreateText(
            dungeonPage,
            "パーティーを編成してダンジョンへ向かいましょう。",
            14,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -154f),
            new Vector2(-170f, -42f),
            ParchmentTextColor);

        startDungeonButton = CreateActionButton(
            dungeonPage,
            "探索開始",
            StartDungeonRun);
        RectTransform startRect = startDungeonButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);

        dungeonSelectionList = CreateUIObject("Dungeon Selection List", dungeonPage);
        dungeonSelectionList.anchorMin = new Vector2(0f, 0f);
        dungeonSelectionList.anchorMax = new Vector2(1f, 1f);
        dungeonSelectionList.offsetMin = Vector2.zero;
        dungeonSelectionList.offsetMax = new Vector2(0f, -174f);

        dungeonEventTitleText = CreateText(
            dungeonPage,
            string.Empty,
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -230f),
            new Vector2(0f, -184f),
            ParchmentTextColor);

        dungeonEventDescriptionText = CreateText(
            dungeonPage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, -300f),
            new Vector2(0f, -238f),
            ParchmentTextColor);

        firstDungeonEventButton = CreateActionButton(
            dungeonPage,
            "選択肢1",
            () => ChooseDungeonEventOption(0));
        PositionDungeonEventButton(firstDungeonEventButton, 0f);

        secondDungeonEventButton = CreateActionButton(
            dungeonPage,
            "選択肢2",
            () => ChooseDungeonEventOption(1));
        PositionDungeonEventButton(secondDungeonEventButton, 248f);

        thirdDungeonEventButton = CreateActionButton(
            dungeonPage,
            "撤退",
            () => ChooseDungeonEventOption(2));
        PositionDungeonEventButton(thirdDungeonEventButton, 496f);

        dungeonResultPanel =
            CreateUIObject("Dungeon Floor Result", dungeonPage);
        dungeonResultPanel.anchorMin = Vector2.zero;
        dungeonResultPanel.anchorMax = Vector2.one;
        dungeonResultPanel.offsetMin = new Vector2(40f, 42f);
        dungeonResultPanel.offsetMax = new Vector2(-40f, -42f);
        Image resultBackground =
            dungeonResultPanel.gameObject.AddComponent<Image>();
        resultBackground.color = RowColor;
        AddFantasyFrame(resultBackground, 2f);

        dungeonResultText = CreateText(
            dungeonResultPanel,
            string.Empty,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(36f, 100f),
            new Vector2(-36f, -70f),
            ButtonTextColor);
        dungeonResultText.rectTransform.anchorMin = Vector2.zero;
        dungeonResultText.rectTransform.anchorMax = Vector2.one;

        dungeonNextFloorButton = CreateActionButton(
            dungeonResultPanel,
            "次のフロアへ進む",
            ContinueToNextDungeonFloor);
        RectTransform nextFloorRect =
            dungeonNextFloorButton.GetComponent<RectTransform>();
        nextFloorRect.anchorMin = nextFloorRect.anchorMax =
            new Vector2(0.5f, 0f);
        nextFloorRect.pivot = new Vector2(0.5f, 0f);
        nextFloorRect.sizeDelta = new Vector2(220f, 50f);
        nextFloorRect.anchoredPosition = new Vector2(-125f, 28f);

        Button returnTownButton = CreateActionButton(
            dungeonResultPanel,
            "町へ戻る",
            ReturnToTownAfterDungeon);
        RectTransform returnTownRect =
            returnTownButton.GetComponent<RectTransform>();
        returnTownRect.anchorMin = returnTownRect.anchorMax =
            new Vector2(0.5f, 0f);
        returnTownRect.pivot = new Vector2(0.5f, 0f);
        returnTownRect.sizeDelta = new Vector2(220f, 50f);
        returnTownRect.anchoredPosition = new Vector2(125f, 28f);

        dungeonResultPanel.gameObject.SetActive(false);

        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();

        DungeonPageUI pageUI =
            dungeonPage.GetComponent<DungeonPageUI>() ??
            dungeonPage.gameObject.AddComponent<DungeonPageUI>();
        pageUI.Configure(RefreshDungeonPage);
        pageRouter.Register(dungeonPage);
    }

    private static void PositionDungeonEventButton(Button button, float x)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(228f, 52f);
        rect.anchoredPosition = new Vector2(x, 18f);
    }

    private void StartPartyBattle()
    {
        ResetBattleLog();
        startBattleButton.interactable = false;

        if (!battleManager.StartBattle(partyManager.Members))
        {
            startBattleButton.interactable = true;
        }
    }

    private void StartDungeonRun()
    {
        DungeonDataSO selected = dungeonRunManager.SelectedDungeon;
        if (selected == null || selected.nearbyTownIndex != currentTownIndex)
        {
            statusText.text =
                $"{TownNames[currentTownIndex]}近隣のダンジョンを選択してください。";
            ShowDungeonPage();
            return;
        }

        ShowBattlePage();
        ResetBattleLog();

        if (!dungeonRunManager.StartRun())
        {
            ShowDungeonPage();
        }
        else
        {
            startBattleButton.gameObject.SetActive(false);
            battlePageTitleText.text = "ダンジョン戦闘";
            battleEncounterText.text =
                $"{dungeonRunManager.DungeonName}  |  " +
                $"第{dungeonRunManager.CurrentFloor}/" +
                $"{dungeonRunManager.TotalFloors}フロア";
        }

        RefreshUI();
    }

    private void ResetBattleLog()
    {
        battleLogLines.Clear();
        battleLogText.text = string.Empty;

        if (battleLogScrollCoroutine != null)
        {
            StopCoroutine(battleLogScrollCoroutine);
            battleLogScrollCoroutine = null;
        }

        Canvas.ForceUpdateCanvases();
        float viewportHeight = battleLogViewport != null
            ? battleLogViewport.rect.height
            : 0f;
        battleLogContent.sizeDelta = new Vector2(0f, Mathf.Max(1f, viewportHeight));
        battleLogContent.anchoredPosition = Vector2.zero;

        if (battleLogScrollRect != null)
        {
            battleLogScrollRect.StopMovement();
            battleLogScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void SelectDungeon(DungeonDataSO data)
    {
        if (data == null || data.nearbyTownIndex != currentTownIndex)
        {
            statusText.text =
                $"{TownNames[currentTownIndex]}からはこのダンジョンへ入れません。";
            return;
        }

        if (!dungeonRunManager.TrySelectDungeon(data))
        {
            statusText.text = "このダンジョンはまだ選択できません。";
            return;
        }

        RebuildDungeonSelectionList();
        ShowDungeonPage();
    }

    private void RebuildDungeonSelectionList()
    {
        if (dungeonSelectionList == null)
        {
            return;
        }

        ClearChildren(dungeonSelectionList);
        dungeonSelectButtons.Clear();
        displayedDungeons.Clear();

        float rowTop = 0f;
        foreach (DungeonDataSO data in dungeonRunManager.AvailableDungeons)
        {
            if (data == null || data.nearbyTownIndex != currentTownIndex)
            {
                continue;
            }

            CreateDungeonSelectionRow(data, rowTop);
            rowTop -= 50f;
        }

        if (displayedDungeons.Count == 0)
        {
            CreateText(
                dungeonSelectionList,
                $"{TownNames[currentTownIndex]}近隣に探索可能なダンジョンはありません。",
                16,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                new Vector2(0f, -110f),
                new Vector2(0f, -40f),
                ParchmentTextColor);
        }
    }

    private void CreateDungeonSelectionRow(DungeonDataSO data, float top)
    {
        RectTransform row = CreateUIObject(data.dungeonName, dungeonSelectionList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 44f);
        row.offsetMax = new Vector2(0f, top);

        Image image = row.gameObject.AddComponent<Image>();
        image.color = RowColor;

        string grade = JapaneseDisplayText.GetDungeonGrade(data.grade);
        string nearbyTown = data.nearbyTownIndex >= 0 &&
                            data.nearbyTownIndex < TownNames.Length
            ? TownNames[data.nearbyTownIndex]
            : "町未設定";
        int clearedFloors = dungeonRunManager.GetClearedFloors(data);
        int totalFloors = Mathf.Max(1, data.totalFloors);
        string floorProgress = clearedFloors >= totalFloors
            ? $"完全攻略 {totalFloors}/{totalFloors}F"
            : $"次回 {clearedFloors + 1}/{totalFloors}F";
        string details =
            $"{nearbyTown}近隣  |  {grade}  |  {data.dungeonName}  |  " +
            $"{floorProgress}  |  {GetDungeonEnemyGradeSummary(data)}";
        CreateText(row, details, 14, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(14f, -44f), new Vector2(-130f, -8f), Color.white);

        bool unlocked = dungeonRunManager.IsDungeonUnlocked(data);
        bool selected = dungeonRunManager.SelectedDungeon == data;
        string label = selected ? "選択中" : unlocked ? "選択" : "未開放";
        Button button = CreateActionButton(row, label, () => SelectDungeon(data));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(108f, 34f);
        button.interactable = unlocked && !selected;
        dungeonSelectButtons.Add(button);
        displayedDungeons.Add(data);
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

    private static string BuildDungeonRewardPreview(DungeonDataSO data)
    {
        if (data == null)
        {
            return "報酬情報なし";
        }

        List<string> guaranteed = new List<string>();
        if (data.clearItemRewards != null)
        {
            foreach (DungeonItemReward reward in data.clearItemRewards)
            {
                if (reward?.item != null && reward.amount > 0)
                {
                    guaranteed.Add(
                        $"{JapaneseDisplayText.GetItemName(reward.item)}×{reward.amount}");
                }
            }
        }

        List<string> limited = new List<string>();
        Dictionary<EquipmentSetId, int> setCounts =
            new Dictionary<EquipmentSetId, int>();
        if (data.limitedEquipmentDrops != null)
        {
            foreach (ItemDataSO item in data.limitedEquipmentDrops)
            {
                if (item != null)
                {
                    if (item.equipmentSet != EquipmentSetId.None)
                    {
                        if (!setCounts.ContainsKey(item.equipmentSet))
                        {
                            setCounts[item.equipmentSet] = 0;
                        }
                        setCounts[item.equipmentSet]++;
                    }
                    else
                    {
                        limited.Add(JapaneseDisplayText.GetItemName(item));
                    }
                }
            }
        }

        foreach (KeyValuePair<EquipmentSetId, int> entry in setCounts)
        {
            limited.Add(
                $"{JapaneseDisplayText.GetEquipmentSet(entry.Key)}セット" +
                $"（{entry.Value}種）");
        }

        return $"確定: {(guaranteed.Count > 0 ? string.Join("、", guaranteed) : "なし")}\n" +
               $"限定: {(limited.Count > 0 ? string.Join("、", limited) : "なし")} / " +
               $"イベント{data.eventLimitedDropChance * 100f:0.#}%・" +
               $"ボス{data.bossLimitedDropChance * 100f:0.#}%";
    }

    private void ChooseDungeonEventOption(int optionIndex)
    {
        if (!dungeonRunManager.ChooseEventOption(optionIndex))
        {
            statusText.text = "その選択肢は現在選べません。";
            UpdateDungeonEventUI();
            return;
        }

        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();

        if (dungeonRunManager.IsRunning && battleManager.IsBattling)
        {
            ShowBattlePage();
        }
        else
        {
            ShowDungeonPage();
        }

        RefreshUI();
    }

    private void HandleBattleMessage(string message, BattleLogType logType)
    {
        string coloredMessage = ColorizeBattleMessage(message, logType);
        battleLogLines.Add(coloredMessage);
        battleLogText.text = string.Join("\n", battleLogLines);

        if (battleLogContent != null)
        {
            UpdateBattleLogContentHeight();
        }

        ScrollBattleLogToLatest();
    }

    private void UpdateBattleLogContentHeight()
    {
        if (battleLogContent == null || battleLogText == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        float viewportHeight = battleLogViewport != null
            ? battleLogViewport.rect.height
            : 0f;
        float height = Mathf.Max(viewportHeight, battleLogText.preferredHeight + 32f);
        battleLogContent.sizeDelta = new Vector2(0f, height);
    }

    private void ScrollBattleLogToLatest()
    {
        if (battleLogScrollRect == null)
        {
            return;
        }

        if (battleLogScrollCoroutine != null)
        {
            StopCoroutine(battleLogScrollCoroutine);
        }

        battleLogScrollCoroutine = StartCoroutine(ScrollBattleLogToLatestRoutine());
    }

    private IEnumerator ScrollBattleLogToLatestRoutine()
    {
        yield return null;
        UpdateBattleLogContentHeight();
        Canvas.ForceUpdateCanvases();
        battleLogScrollRect.verticalNormalizedPosition = 0f;
        battleLogScrollCoroutine = null;
    }

    private static string ColorizeBattleMessage(string message, BattleLogType logType)
    {
        string escapedMessage = EscapeRichText(message);
        switch (logType)
        {
            case BattleLogType.Player:
                return $"<color=#5CA8FF>{escapedMessage}</color>";
            case BattleLogType.Enemy:
                return $"<color=#FF6B6B>{escapedMessage}</color>";
            case BattleLogType.Reward:
                return $"<color=#6FE3A0>{escapedMessage}</color>";
            default:
                return escapedMessage;
        }
    }

    private static string EscapeRichText(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private void HandleBattleCompleted(bool victory)
    {
        startBattleButton.interactable = partyManager.Members.Count > 0;
        RebuildCompanyList();
        RebuildPartyList();
        RebuildHealList();
        RefreshUI();

        if (pendingTravelTownIndex >= 0)
        {
            int destinationTownIndex = pendingTravelTownIndex;
            bool wasUnlock = pendingTravelWasUnlock;
            bool openDungeonAfterTravel = pendingOpenDungeonAfterTravel;

            if (victory &&
                pendingTravelEncounterIndex < pendingTravelEncounterCount)
            {
                isAwaitingRoadTravelChoice = true;
                roadContinueButton.gameObject.SetActive(true);
                roadRetreatButton.gameObject.SetActive(true);
                roadBattleRouteText.text =
                    $"接敵 {pendingTravelEncounterIndex}/" +
                    $"{pendingTravelEncounterCount} を突破しました。\n" +
                    "次の区間へ進むか、出発した町へ撤退してください。";
                statusText.text = "街道戦闘を続行しますか？";
                return;
            }

            pendingTravelTownIndex = -1;
            pendingTravelWasUnlock = false;
            pendingOpenDungeonAfterTravel = false;
            pendingTravelEncounterCount = 0;
            pendingTravelEncounterIndex = 0;
            isAwaitingRoadTravelChoice = false;

            if (victory)
            {
                unlockedTownIndices.Add(destinationTownIndex);
                currentTownIndex = destinationTownIndex;
                viewedWorldMapIndex = CurrentWorldMapIndex;
                dungeonRunManager.SetCurrentWorldMapIndex(
                    viewedWorldMapIndex);
                ApplyTownServiceSettings(false, false);
                dayManager.AdvanceDay();
                SyncDungeonUnlocks();
                RefreshTownMapButtons();
                if (openDungeonAfterTravel)
                {
                    OpenNearbyDungeon();
                }
                else
                {
                    ShowTownMap();
                }
                statusText.text = wasUnlock
                    ? $"街道戦闘に勝利し、{TownNames[destinationTownIndex]}を解放しました。"
                    : $"街道戦闘に勝利し、{TownNames[destinationTownIndex]}へ到着しました。";
                saveManager?.SaveGame();
            }
            else
            {
                ShowWorldMap();
                statusText.text = wasUnlock
                    ? $"街道戦闘に敗北しました。{TownNames[destinationTownIndex]}は未解放のままです。"
                    : $"街道戦闘に敗北したため、{TownNames[destinationTownIndex]}へ移動できませんでした。";
            }
            return;
        }

        statusText.text = victory ? "戦闘に勝利しました。" : "戦闘に敗北しました。";
    }

    private void HandleDungeonMessage(string message)
    {
        statusText.text = message;
        if (dungeonStatusText != null)
        {
            dungeonStatusText.text = message;
        }
    }

    private void HandleDungeonStateChanged()
    {
        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            ShowDungeonPage();
        }

        RefreshUI();
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        string result = progressionManager != null
            ? progressionManager.LastExplorationResult
            : string.Empty;
        statusText.text = cleared
            ? dungeonRunManager.IsSelectedDungeonFullyCleared
                ? "ダンジョンを完全攻略しました。"
                : $"フロアを攻略しました。次回は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "ダンジョン探索を終了しました。";
        if (!string.IsNullOrEmpty(result))
        {
            statusText.text += $" {result}";
        }
        ShowDungeonPage();
        bool fullyCleared =
            dungeonRunManager.IsSelectedDungeonFullyCleared;
        dungeonResultText.text = cleared
            ? fullyCleared
                ? $"{dungeonRunManager.DungeonName}\n完全攻略！\n\n" +
                  "すべてのフロアを攻略しました。"
                : $"フロア攻略完了\n\n" +
                  $"次は第{dungeonRunManager.CurrentFloor}フロアです。"
            : "探索終了\n\n町へ戻って態勢を整えましょう。";
        dungeonNextFloorButton.gameObject.SetActive(
            cleared && !fullyCleared);
        dungeonResultPanel.SetAsLastSibling();
        dungeonResultPanel.gameObject.SetActive(true);
        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();
        RefreshUI();
    }

    private void UpdateDungeonEventUI()
    {
        if (dungeonEventTitleText == null ||
            dungeonEventDescriptionText == null ||
            firstDungeonEventButton == null ||
            secondDungeonEventButton == null ||
            thirdDungeonEventButton == null)
        {
            return;
        }

        bool showEvent = dungeonRunManager.IsAwaitingEventChoice;
        if (dungeonSelectionList != null)
        {
            dungeonSelectionList.gameObject.SetActive(!dungeonRunManager.IsRunning);
        }

        dungeonEventTitleText.gameObject.SetActive(showEvent);
        dungeonEventDescriptionText.gameObject.SetActive(showEvent);
        firstDungeonEventButton.gameObject.SetActive(showEvent);
        secondDungeonEventButton.gameObject.SetActive(showEvent);
        thirdDungeonEventButton.gameObject.SetActive(showEvent);

        if (!showEvent)
        {
            return;
        }

        dungeonEventTitleText.text = dungeonRunManager.EventTitle;
        dungeonEventDescriptionText.text = dungeonRunManager.EventDescription;
        SetButtonLabel(firstDungeonEventButton, dungeonRunManager.FirstOptionLabel);
        SetButtonLabel(secondDungeonEventButton, dungeonRunManager.SecondOptionLabel);
        SetButtonLabel(thirdDungeonEventButton, dungeonRunManager.ThirdOptionLabel);
    }

    private static void SetButtonLabel(Button button, string label)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = label;
        }
    }

    private void CycleBattleSpeed()
    {
        float speed = battleManager.CycleBattleSpeed();
        string label = $"速度 x{speed:0}";
        if (battleSpeedButton != null)
        {
            SetButtonLabel(battleSpeedButton, label);
        }
        if (roadSpeedButton != null)
        {
            SetButtonLabel(roadSpeedButton, label);
        }
        statusText.text = $"戦闘速度を{speed:0}倍に変更しました。";
    }

    private void ShowBattlePage()
    {
        MoveBattleLogTo(battlePage);
        SwitchToPage(battlePage, battleTabButton);
    }

    private void RefreshBattlePage()
    {
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !battleManager.IsBattling;
        startBattleButton.gameObject.SetActive(false);
        statusText.text = $"戦闘参加: 傭兵{partyManager.Members.Count}人";
    }

    private void UseConsumable(ItemDataSO item)
    {
        if (item == null ||
            item.itemType != ItemType.Consumable ||
            battleManager.IsBattling)
        {
            statusText.text = "現在はこの消費アイテムを使用できません。";
            return;
        }

        BattleStatusEffect targetStatus;
        switch (item.consumableEffect)
        {
            case ConsumableEffectType.CurePoison:
                targetStatus = BattleStatusEffect.Poison;
                break;
            case ConsumableEffectType.CureParalysis:
                targetStatus = BattleStatusEffect.Paralysis;
                break;
            case ConsumableEffectType.CureAllStatus:
                targetStatus = BattleStatusEffect.None;
                break;
            default:
                statusText.text = "この消費アイテムには使用効果がありません。";
                return;
        }

        MercenaryInstance target = null;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null &&
                mercenary.HasStatusEffect &&
                (targetStatus == BattleStatusEffect.None ||
                 mercenary.StatusEffect == targetStatus))
            {
                target = mercenary;
                break;
            }
        }

        if (target == null)
        {
            statusText.text = "治療対象となる傭兵がいません。";
            return;
        }

        if (!merchantInventory.TryRemoveItem(item) ||
            !target.CureStatusEffect(targetStatus))
        {
            statusText.text = "消費アイテムを使用できませんでした。";
            return;
        }

        statusText.text =
            $"{JapaneseDisplayText.GetItemName(item)}を使用し、" +
            $"{target.MercenaryName}の状態異常を治療しました。";
        RebuildInventoryList();
        RebuildCompanyList();
        RefreshCharacterDetailText();
        saveManager?.SaveGame();
    }

    private void ShowRoadBattlePage(
        int originTownIndex,
        int destinationTownIndex)
    {
        MoveBattleLogTo(roadBattlePage);
        SwitchToPage(roadBattlePage);
    }

    private void RefreshRoadBattlePage()
    {
        mapButton?.gameObject.SetActive(false);
        townMapButton?.gameObject.SetActive(false);
        roadContinueButton.gameObject.SetActive(
            isAwaitingRoadTravelChoice);
        roadRetreatButton.gameObject.SetActive(
            isAwaitingRoadTravelChoice);
        roadBattleRouteText.text =
            $"{TownNames[currentTownIndex]} → " +
            $"{TownNames[pendingTravelTownIndex]}\n" +
            $"接敵 {pendingTravelEncounterIndex}/" +
            $"{pendingTravelEncounterCount}  |  " +
            (pendingRoadRareEncounter
                ? "幻獣の気配を確認！"
                : "両地域の通常モンスターが街道を塞いでいます。");
    }

    private void MoveBattleLogTo(RectTransform destinationPage)
    {
        if (battleLogPanel == null || destinationPage == null)
        {
            return;
        }

        battleLogPanel.SetParent(destinationPage, false);
        battleLogPanel.anchorMin = Vector2.zero;
        battleLogPanel.anchorMax = Vector2.one;
        battleLogPanel.offsetMin = Vector2.zero;
        battleLogPanel.offsetMax = new Vector2(0f, -104f);
    }

    private void ShowDungeonPage()
    {
        SwitchToPage(dungeonPage, dungeonTabButton);
    }

    private void RefreshDungeonPage()
    {
        dungeonResultPanel?.gameObject.SetActive(false);
        EnsureNearbyDungeonSelected();

        if (dungeonRunManager.IsAwaitingEventChoice)
        {
            dungeonStatusText.text =
                $"遭遇 {dungeonRunManager.CurrentEncounter}/" +
                $"{dungeonRunManager.EncounterCount} を突破。次の行動を選んでください。";
        }
        else
        {
            dungeonStatusText.text = dungeonRunManager.IsRunning
                ? $"第{dungeonRunManager.CurrentFloor}/" +
                  $"{dungeonRunManager.TotalFloors}フロア探索中: " +
                  $"{dungeonRunManager.CurrentEncounter}/" +
                  $"{dungeonRunManager.EncounterCount}"
                : $"{dungeonRunManager.DungeonName}  |  " +
                  $"第{dungeonRunManager.CurrentFloor}/" +
                  $"{dungeonRunManager.TotalFloors}フロア  |  " +
                  $"遭遇{dungeonRunManager.EncounterCount}回\n" +
                  $"フロア報酬 " +
                  $"{Mathf.Max(0, dungeonRunManager.SelectedDungeon != null ? dungeonRunManager.SelectedDungeon.floorClearGoldReward : 0)} G  |  " +
                  $"完全攻略報酬 {dungeonRunManager.ClearGoldReward} G\n" +
                  BuildDungeonRewardPreview(
                      dungeonRunManager.SelectedDungeon);
        }

        UpdateDungeonEventUI();
        RebuildDungeonSelectionList();
        statusText.text = $"探索パーティー: 傭兵{partyManager.Members.Count}人";
        RefreshUI();
    }

    private void ContinueToNextDungeonFloor()
    {
        dungeonResultPanel?.gameObject.SetActive(false);
        StartDungeonRun();
    }

    private void ReturnToTownAfterDungeon()
    {
        dungeonResultPanel?.gameObject.SetActive(false);
        ShowTownMap();
        statusText.text = $"{TownNames[currentTownIndex]}へ戻りました。";
    }

    private void EnsureNearbyDungeonSelected()
    {
        if (dungeonRunManager.IsRunning)
        {
            return;
        }

        DungeonDataSO selected = dungeonRunManager.SelectedDungeon;
        if (selected != null && selected.nearbyTownIndex == currentTownIndex)
        {
            return;
        }

        DungeonDataSO nearby =
            dungeonRunManager.GetDungeonNearTown(currentTownIndex);
        if (nearby != null && dungeonRunManager.IsDungeonUnlocked(nearby))
        {
            dungeonRunManager.TrySelectDungeon(nearby);
        }
    }

}
