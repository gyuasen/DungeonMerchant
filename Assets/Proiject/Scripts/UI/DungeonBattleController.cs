using System;
using System.Collections.Generic;

/// <summary>
/// Owns the dungeon selection/run actions, party battle start, battle
/// speed cycling, dungeon event choices and the battle-log line data.
/// Extracted from SimpleMercenaryHireUI (step 3.10). Page construction,
/// routing, the scroll coroutine and Text/ScrollRect updates stay in
/// SimpleMercenaryHireUI.BattleDungeon.cs; only the feature state and
/// business actions live here.
/// </summary>
public sealed class DungeonBattleController
{
    private readonly BattleManager battleManager;
    private readonly DungeonRunManager dungeonRunManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly TownProgressState townProgressState;
    private readonly Action<string> setStatus;
    private readonly Action resetBattleLog;
    private readonly Action showBattlePage;
    private readonly Action showDungeonPage;
    private readonly Action<bool> setStartBattleButtonInteractable;
    private readonly Action<bool> setStartBattleButtonActive;
    private readonly Action<string> setBattlePageTitle;
    private readonly Action<string> setBattleEncounterText;
    private readonly Action refreshPartyStatePages;
    private readonly Action updateDungeonEventUI;
    private readonly Action<string> setSpeedButtonLabels;
    private readonly Action refreshUI;

    private readonly List<string> battleLogLines = new List<string>();

    public DungeonBattleController(
        BattleManager battleManager,
        DungeonRunManager dungeonRunManager,
        MercenaryPartyManager partyManager,
        TownProgressState townProgressState,
        Action<string> setStatus,
        Action resetBattleLog,
        Action showBattlePage,
        Action showDungeonPage,
        Action<bool> setStartBattleButtonInteractable,
        Action<bool> setStartBattleButtonActive,
        Action<string> setBattlePageTitle,
        Action<string> setBattleEncounterText,
        Action refreshPartyStatePages,
        Action updateDungeonEventUI,
        Action<string> setSpeedButtonLabels,
        Action refreshUI)
    {
        this.battleManager = battleManager;
        this.dungeonRunManager = dungeonRunManager;
        this.partyManager = partyManager;
        this.townProgressState = townProgressState;
        this.setStatus = setStatus;
        this.resetBattleLog = resetBattleLog;
        this.showBattlePage = showBattlePage;
        this.showDungeonPage = showDungeonPage;
        this.setStartBattleButtonInteractable = setStartBattleButtonInteractable;
        this.setStartBattleButtonActive = setStartBattleButtonActive;
        this.setBattlePageTitle = setBattlePageTitle;
        this.setBattleEncounterText = setBattleEncounterText;
        this.refreshPartyStatePages = refreshPartyStatePages;
        this.updateDungeonEventUI = updateDungeonEventUI;
        this.setSpeedButtonLabels = setSpeedButtonLabels;
        this.refreshUI = refreshUI;
    }

    public void StartPartyBattle()
    {
        resetBattleLog();
        setStartBattleButtonInteractable(false);

        if (!battleManager.StartBattle(partyManager.Members))
        {
            setStartBattleButtonInteractable(true);
        }
    }

    public void StartDungeonRun()
    {
        DungeonDataSO selected = dungeonRunManager.SelectedDungeon;
        if (selected == null || selected.nearbyTownIndex != townProgressState.CurrentTownIndex)
        {
            setStatus(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}近隣のダンジョンを選択してください。");
            showDungeonPage();
            return;
        }

        showBattlePage();
        resetBattleLog();

        if (!dungeonRunManager.StartRun())
        {
            showDungeonPage();
        }
        else
        {
            setStartBattleButtonActive(false);
            setBattlePageTitle("ダンジョン戦闘");
            setBattleEncounterText(
                $"{dungeonRunManager.DungeonName}  |  " +
                $"第{dungeonRunManager.CurrentFloor}/" +
                $"{dungeonRunManager.TotalFloors}フロア");
        }

        refreshUI();
    }

    public void SelectDungeon(DungeonDataSO data)
    {
        if (data == null || data.nearbyTownIndex != townProgressState.CurrentTownIndex)
        {
            setStatus(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}からはこのダンジョンへ入れません。");
            return;
        }

        if (!dungeonRunManager.TrySelectDungeon(data))
        {
            setStatus("このダンジョンはまだ選択できません。");
            return;
        }

        showDungeonPage();
    }

    public static string BuildDungeonRewardPreview(DungeonDataSO data)
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

    public void ChooseDungeonEventOption(int optionIndex)
    {
        if (!dungeonRunManager.ChooseEventOption(optionIndex))
        {
            setStatus("その選択肢は現在選べません。");
            updateDungeonEventUI();
            return;
        }

        refreshPartyStatePages();

        if (dungeonRunManager.IsRunning && battleManager.IsBattling)
        {
            showBattlePage();
        }
        else
        {
            showDungeonPage();
        }

        refreshUI();
    }

    public void CycleBattleSpeed()
    {
        float speed = battleManager.CycleBattleSpeed();
        string label = $"速度 x{speed:0}";
        setSpeedButtonLabels(label);
        setStatus($"戦闘速度を{speed:0}倍に変更しました。");
    }

    public void OpenNearbyDungeon()
    {
        DungeonDataSO preferred =
            dungeonRunManager.GetDungeonNearTown(townProgressState.CurrentTownIndex);
        if (preferred == null)
        {
            setStatus(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}近隣に探索可能なダンジョンはありません。");
        }
        else if (!dungeonRunManager.TrySelectDungeon(preferred))
        {
            setStatus(
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}近隣のダンジョンは未開放です。");
        }
        showDungeonPage();
    }

    public void EnsureNearbyDungeonSelected()
    {
        if (dungeonRunManager.IsRunning)
        {
            return;
        }

        DungeonDataSO selected = dungeonRunManager.SelectedDungeon;
        if (selected != null && selected.nearbyTownIndex == townProgressState.CurrentTownIndex)
        {
            return;
        }

        DungeonDataSO nearby =
            dungeonRunManager.GetDungeonNearTown(townProgressState.CurrentTownIndex);
        if (nearby != null && dungeonRunManager.IsDungeonUnlocked(nearby))
        {
            dungeonRunManager.TrySelectDungeon(nearby);
        }
    }

    public void ClearBattleLog()
    {
        battleLogLines.Clear();
    }

    /// <summary>
    /// Colors and stores one battle message, then returns the full log
    /// text for the Text component (the caller handles height/scroll).
    /// </summary>
    public string AppendBattleMessage(string message, BattleLogType logType)
    {
        string coloredMessage = ColorizeBattleMessage(message, logType);
        battleLogLines.Add(coloredMessage);
        return string.Join("\n", battleLogLines);
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
}
