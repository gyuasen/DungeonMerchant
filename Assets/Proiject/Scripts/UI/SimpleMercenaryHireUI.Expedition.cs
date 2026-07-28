using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private RectTransform expeditionOverlay;
    private DungeonDataSO expeditionDungeon;
    private readonly List<MercenaryInstance> selectedExpeditionMembers =
        new List<MercenaryInstance>();
    private DungeonExpedition expeditionPendingRecall;
    private ExpeditionLootPolicy selectedExpeditionLootPolicy = ExpeditionLootPolicy.Store;

    private bool CanShowExpeditionAction(DungeonDataSO dungeon)
    {
        return dungeon != null &&
               dungeon.nearbyTownIndex != WorldMapService.HiddenIslandTownIndex &&
               dungeonRunManager.GetClearedFloors(dungeon) >=
               Mathf.Max(1, dungeon.totalFloors);
    }

    private bool HasExpedition(DungeonDataSO dungeon)
    {
        if (dungeonExpeditionManager == null || dungeon == null)
        {
            return false;
        }
        foreach (DungeonExpedition expedition in dungeonExpeditionManager.ActiveExpeditions)
        {
            if (expedition != null && expedition.dungeon == dungeon)
            {
                return true;
            }
        }
        return false;
    }

    private void ShowExpeditionForDungeon(DungeonDataSO dungeon)
    {
        if (HasExpedition(dungeon))
        {
            ShowExpeditionManagementOverlay();
            return;
        }
        if (!CanShowExpeditionAction(dungeon))
        {
            return;
        }
        expeditionDungeon = dungeon;
        selectedExpeditionMembers.Clear();
        selectedExpeditionLootPolicy = ExpeditionLootPolicy.Store;
        RebuildExpeditionFormationOverlay();
    }

    private void RebuildExpeditionFormationOverlay()
    {
        DestroyExpeditionOverlay();
        expeditionOverlay = CreateOverlayWindow("別動隊を送る", out RectTransform window);
        Text description = CreateText(
            window,
            BuildExpeditionFormationSummary(),
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(28f, -72f),
            new Vector2(-28f, -184f),
            ParchmentTextColor);
        description.supportRichText = true;

        RectTransform list = CreateUIObject("Mercenary Candidates", window);
        list.anchorMin = new Vector2(0f, 0f);
        list.anchorMax = new Vector2(1f, 0f);
        list.pivot = new Vector2(.5f, 0f);
        list.anchoredPosition = new Vector2(0f, 72f);
        list.sizeDelta = new Vector2(-40f, 250f);
        float top = 0f;
        if (hireManager != null)
        {
            foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
            {
                CreateExpeditionCandidateRow(list, mercenary, top);
                top -= 42f;
            }
        }

        Button confirm = CreateActionButton(
            window,
            "この編成で毎日周回する",
            ConfirmExpeditionFormation);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(.5f, 0f);
        confirmRect.pivot = new Vector2(.5f, 0f);
        confirmRect.sizeDelta = new Vector2(250f, 44f);
        confirmRect.anchoredPosition = new Vector2(-132f, 20f);
        confirm.interactable = selectedExpeditionMembers.Count >= 1 &&
                              selectedExpeditionMembers.Count <= 3;
        CreateLootPolicyButton(window, selectedExpeditionLootPolicy, CycleFormationLootPolicy);
        Button cancel = CreateActionButton(window, "キャンセル", DestroyExpeditionOverlay);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = new Vector2(.5f, 0f);
        cancelRect.pivot = new Vector2(.5f, 0f);
        cancelRect.sizeDelta = new Vector2(130f, 44f);
        cancelRect.anchoredPosition = new Vector2(150f, 20f);
        expeditionOverlay.SetAsLastSibling();
    }

    private string BuildExpeditionFormationSummary()
    {
        int strength = CombatPowerCalculator.Calculate(selectedExpeditionMembers);
        int required = dungeonExpeditionManager.GetRequiredStrength(expeditionDungeon);
        bool stable = strength >= required;
        string state = stable
            ? "<color=#65D88A>安定周回</color>"
            : "<color=#FF7474>戦力不足：報酬なし・HP減少</color>";
        return expeditionDungeon.dungeonName + "\n" +
               "最寄り町: " + WorldMapService.GetTownName(expeditionDungeon.nearbyTownIndex) + "\n" +
               "毎日1回、自動で周回。呼び戻すまで継続します。\n" +
               "Gold・素材・経験値・低確率の限定装備を最寄り町の倉庫へ格納。\n" +
               "隊員 " + selectedExpeditionMembers.Count + "/3  |  戦力 " + strength + "/" + required + "  " + state + "\n" +
               "派遣中の傭兵は他の用途に使えません。";
    }

    private void CreateExpeditionCandidateRow(RectTransform list, MercenaryInstance mercenary, float top)
    {
        if (mercenary == null)
        {
            return;
        }
        RectTransform row = CreateUIObject("Candidate " + mercenary.InstanceId, list);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 38f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;
        bool selected = selectedExpeditionMembers.Contains(mercenary);
        string unavailableReason = GetExpeditionUnavailableReason(mercenary);
        bool selectable = selected || string.IsNullOrEmpty(unavailableReason);
        string label = selected ? "選択中" : selectable ? "選ぶ" : unavailableReason;
        CreateText(
            row,
            mercenary.MercenaryName + "  HP " + mercenary.CurrentHP + "/" + mercenary.MaxHP +
            "  戦力 " + CombatPowerCalculator.Calculate(new[] { mercenary }),
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(10f, -36f),
            new Vector2(-170f, -2f),
            selectable ? ParchmentTextColor : ParchmentMutedColor);
        Button button = CreateActionButton(row, label, () => ToggleExpeditionMember(mercenary));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, .5f);
        buttonRect.anchorMax = new Vector2(1f, .5f);
        buttonRect.pivot = new Vector2(1f, .5f);
        buttonRect.anchoredPosition = new Vector2(-6f, 0f);
        buttonRect.sizeDelta = new Vector2(150f, 30f);
        button.interactable = selectable;
    }

    private string GetExpeditionUnavailableReason(MercenaryInstance mercenary)
    {
        if (selectedExpeditionMembers.Contains(mercenary))
        {
            return string.Empty;
        }
        if (mercenary.CurrentTownIndex != expeditionDungeon.nearbyTownIndex)
        {
            return "別の町にいる";
        }
        switch (MercenaryDutyService.GetDuty(mercenary.InstanceId))
        {
            case MercenaryDuty.Party: return "編成中";
            case MercenaryDuty.Training: return "修練中";
            case MercenaryDuty.RoadTransit: return "街道同行中";
            case MercenaryDuty.Expedition: return "別動隊参加中";
        }
        return mercenary.IsContractActive ? string.Empty : "契約停止中";
    }

    private void ToggleExpeditionMember(MercenaryInstance mercenary)
    {
        if (selectedExpeditionMembers.Remove(mercenary))
        {
            RebuildExpeditionFormationOverlay();
            return;
        }
        if (selectedExpeditionMembers.Count < 3 &&
            string.IsNullOrEmpty(GetExpeditionUnavailableReason(mercenary)))
        {
            selectedExpeditionMembers.Add(mercenary);
        }
        RebuildExpeditionFormationOverlay();
    }

    private void ConfirmExpeditionFormation()
    {
        ExpeditionFormationResult result = dungeonExpeditionManager.TryFormExpedition(
            expeditionDungeon,
            selectedExpeditionMembers,
            selectedExpeditionLootPolicy);
        if (result == ExpeditionFormationResult.Succeeded)
        {
            DestroyExpeditionOverlay();
            dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
            ShowExpeditionManagementOverlay();
            return;
        }
        RebuildExpeditionFormationOverlay();
    }

    private void ShowExpeditionManagementOverlay()
    {
        DestroyExpeditionOverlay();
        expeditionOverlay = CreateOverlayWindow("別動隊管理", out RectTransform window);
        RectTransform list = CreateUIObject("Active Expeditions", window);
        list.anchorMin = new Vector2(0f, 0f);
        list.anchorMax = new Vector2(1f, 1f);
        list.offsetMin = new Vector2(28f, 72f);
        list.offsetMax = new Vector2(-28f, -72f);
        float top = 0f;
        foreach (DungeonExpedition expedition in dungeonExpeditionManager.ActiveExpeditions)
        {
            CreateExpeditionManagementCard(list, expedition, top);
            top -= 158f;
        }
        if (dungeonExpeditionManager.ActiveExpeditions.Count == 0)
        {
            CreateText(list, "稼働中の別動隊はいません。", 17, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, -80f), new Vector2(0f, -30f), ParchmentMutedColor);
        }
        Button close = CreateActionButton(window, "閉じる", DestroyExpeditionOverlay);
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(.5f, 0f);
        closeRect.pivot = new Vector2(.5f, 0f);
        closeRect.sizeDelta = new Vector2(160f, 44f);
        closeRect.anchoredPosition = new Vector2(0f, 20f);
        expeditionOverlay.SetAsLastSibling();
    }

    private void CreateExpeditionManagementCard(RectTransform list, DungeonExpedition expedition, float top)
    {
        if (expedition?.dungeon == null)
        {
            return;
        }
        RectTransform card = CreateUIObject("Expedition " + expedition.dungeon.name, list);
        card.anchorMin = new Vector2(0f, 1f);
        card.anchorMax = new Vector2(1f, 1f);
        card.pivot = new Vector2(.5f, 1f);
        card.offsetMin = new Vector2(0f, top - 148f);
        card.offsetMax = new Vector2(0f, top);
        card.gameObject.AddComponent<Image>().color = RowColor;
        List<MercenaryInstance> members = new List<MercenaryInstance>();
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && expedition.memberInstanceIds.Contains(mercenary.InstanceId))
            {
                members.Add(mercenary);
            }
        }
        int strength = dungeonExpeditionManager.GetExpeditionStrength(expedition);
        int required = dungeonExpeditionManager.GetRequiredStrength(expedition.dungeon);
        StringBuilder names = new StringBuilder();
        foreach (MercenaryInstance member in members)
        {
            if (names.Length > 0)
            {
                names.Append(" / ");
            }
            names.Append(member.MercenaryName).Append(" HP ").Append(member.CurrentHP).Append("/").Append(member.MaxHP);
        }
        string state = strength >= required ? "安定周回" : "戦力不足（報酬なし・HP減少）";
        CreateText(card, expedition.dungeon.dungeonName + "\n" + names + "\n戦力 " + strength + "/" + required + "  " + state + "\n報酬は最寄り町の倉庫へ", 15, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(12f, -102f), new Vector2(-170f, -8f), ParchmentTextColor);
        Button recall = CreateActionButton(card, "呼び戻す", () => ShowRecallConfirmation(expedition));
        RectTransform recallRect = recall.GetComponent<RectTransform>();
        recallRect.anchorMin = recallRect.anchorMax = new Vector2(1f, .5f);
        recallRect.pivot = new Vector2(1f, .5f);
        recallRect.anchoredPosition = new Vector2(-8f, 0f);
        recallRect.sizeDelta = new Vector2(135f, 38f);
        CreateLootPolicyButton(
            card,
            expedition.lootPolicy,
            () => CycleExpeditionLootPolicy(expedition));
    }

    private void CreateLootPolicyButton(
        RectTransform parent,
        ExpeditionLootPolicy policy,
        UnityEngine.Events.UnityAction onClick)
    {
        Button button = CreateActionButton(parent, GetLootPolicyLabel(policy), onClick);
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(.5f, 0f);
        buttonRect.pivot = new Vector2(.5f, 0f);
        buttonRect.sizeDelta = new Vector2(280f, 32f);
        buttonRect.anchoredPosition = new Vector2(-65f, 62f);
    }

    private void CycleFormationLootPolicy()
    {
        selectedExpeditionLootPolicy = GetNextLootPolicy(selectedExpeditionLootPolicy);
        RebuildExpeditionFormationOverlay();
    }

    private void CycleExpeditionLootPolicy(DungeonExpedition expedition)
    {
        dungeonExpeditionManager.SetExpeditionLootPolicy(
            expedition,
            GetNextLootPolicy(expedition.lootPolicy));
        ShowExpeditionManagementOverlay();
    }

    private static ExpeditionLootPolicy GetNextLootPolicy(ExpeditionLootPolicy policy)
    {
        switch (policy)
        {
            case ExpeditionLootPolicy.Store: return ExpeditionLootPolicy.SellNonEquipment;
            case ExpeditionLootPolicy.SellNonEquipment: return ExpeditionLootPolicy.SellAll;
            default: return ExpeditionLootPolicy.Store;
        }
    }

    private static string GetLootPolicyLabel(ExpeditionLootPolicy policy)
    {
        switch (policy)
        {
            case ExpeditionLootPolicy.SellNonEquipment: return "獲得品: 装備以外売却";
            case ExpeditionLootPolicy.SellAll: return "獲得品: すべて売却";
            default: return "獲得品: 倉庫";
        }
    }

    private void ShowRecallConfirmation(DungeonExpedition expedition)
    {
        DestroyExpeditionOverlay();
        expeditionPendingRecall = expedition;
        expeditionOverlay = CreateOverlayWindow("別動隊を呼び戻す", out RectTransform window);
        CreateText(window, "呼び戻すと別動隊の毎日周回は停止します。\n傭兵は最寄り町に留まり、再び他の任務に参加できます。", 17, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(40f, -280f), new Vector2(-40f, -90f), ParchmentTextColor);
        Button confirm = CreateActionButton(window, "呼び戻す", ConfirmRecallExpedition);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(.5f, 0f);
        confirmRect.pivot = new Vector2(.5f, 0f);
        confirmRect.sizeDelta = new Vector2(160f, 44f);
        confirmRect.anchoredPosition = new Vector2(-95f, 22f);
        Button cancel = CreateActionButton(window, "キャンセル", ShowExpeditionManagementOverlay);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = new Vector2(.5f, 0f);
        cancelRect.pivot = new Vector2(.5f, 0f);
        cancelRect.sizeDelta = new Vector2(160f, 44f);
        cancelRect.anchoredPosition = new Vector2(95f, 22f);
        expeditionOverlay.SetAsLastSibling();
    }

    private void ConfirmRecallExpedition()
    {
        if (expeditionPendingRecall != null)
        {
            dungeonExpeditionManager.RecallExpedition(expeditionPendingRecall);
        }
        expeditionPendingRecall = null;
        dungeonPage.GetComponent<DungeonPageUI>()?.RefreshSelection();
        ShowExpeditionManagementOverlay();
    }

    private RectTransform CreateOverlayWindow(string title, out RectTransform window)
    {
        RectTransform overlay = CreateUIObject("Expedition Overlay", overlayRoot);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, .78f);
        window = CreateUIObject("Window", overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(.5f, .5f);
        window.sizeDelta = new Vector2(760f, 600f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, title, 25, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(24f, -56f), new Vector2(-24f, -16f), ParchmentTextColor);
        return overlay;
    }

    private void DestroyExpeditionOverlay()
    {
        if (expeditionOverlay != null)
        {
            Destroy(expeditionOverlay.gameObject);
            expeditionOverlay = null;
        }
    }
}
