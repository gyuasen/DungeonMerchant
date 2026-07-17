using UnityEngine;
using UnityEngine.UI;

// Expedition overlay construction and subscriptions. Selection state remains
// in ExpeditionController, keeping the manager independent from UI concerns.
public partial class SimpleMercenaryHireUI
{
    private void BuildExpeditionOverlay()
    {
        expeditionOverlay = CreateUIObject("Expedition Overlay", overlayRoot);
        expeditionOverlay.anchorMin = Vector2.zero;
        expeditionOverlay.anchorMax = Vector2.one;
        expeditionOverlay.offsetMin = Vector2.zero;
        expeditionOverlay.offsetMax = Vector2.zero;
        expeditionOverlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, .82f);
        RectTransform window = CreateUIObject("Expedition Window", expeditionOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(.5f, .5f);
        window.sizeDelta = new Vector2(900f, 650f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "遠征部隊", 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(28f, -64f), new Vector2(-140f, -20f), ParchmentTextColor);
        expeditionContent = CreateScrollableContent(window, "Expedition Viewport", "Expedition Content", new Vector2(28f, 86f), new Vector2(-28f, -82f));
        Button dispatch = CreateActionButton(window, "派遣", expeditionController.Dispatch);
        RectTransform dispatchRect = dispatch.GetComponent<RectTransform>();
        dispatchRect.anchorMin = dispatchRect.anchorMax = new Vector2(1f, 0f);
        dispatchRect.pivot = new Vector2(1f, 0f);
        dispatchRect.sizeDelta = new Vector2(110f, 42f);
        dispatchRect.anchoredPosition = new Vector2(-140f, 24f);
        Button close = CreateActionButton(window, "閉じる", HideExpeditionOverlay);
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 0f);
        closeRect.pivot = new Vector2(1f, 0f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-28f, 24f);
        expeditionOverlay.gameObject.SetActive(false);
    }

    private void ShowExpeditionOverlay()
    {
        if (expeditionOverlay == null)
        {
            BuildExpeditionOverlay();
        }
        if (expeditionOverlay == null)
        {
            return;
        }
        RefreshExpeditionOverlay();
        expeditionOverlay.SetAsLastSibling();
        expeditionOverlay.gameObject.SetActive(true);
    }

    private void HideExpeditionOverlay()
    {
        expeditionOverlay?.gameObject.SetActive(false);
    }

    private void RefreshExpeditionOverlay()
    {
        if (dungeonExpeditionManager == null)
        {
            ResolveReferences();
            if (dungeonExpeditionManager != null)
            {
                expeditionController = new ExpeditionController(
                    dungeonExpeditionManager,
                    dungeonRunManager,
                    hireManager,
                    partyManager,
                    transportManager,
                    message => statusText.text = message,
                    RefreshExpeditionOverlay);
            }
        }
        if (expeditionContent == null)
        {
            return;
        }
        for (int i = expeditionContent.childCount - 1; i >= 0; i--)
        {
            Destroy(expeditionContent.GetChild(i).gameObject);
        }
        float top = -12f;
        if (expeditionController == null || dungeonExpeditionManager == null)
        {
            CreateScrollSection(expeditionContent, "遠征部隊", ref top);
            CreateScrollLabel(expeditionContent,
                "遠征システムを準備中です。しばらくしてから開き直してください。",
                ref top);
            expeditionContent.sizeDelta = new Vector2(
                0f,
                Mathf.Max(420f, -top + 12f));
            return;
        }
        CreateScrollSection(expeditionContent, "進行中の遠征", ref top);
        bool active = false;
        foreach (DungeonExpedition expedition in expeditionController.ActiveExpeditions)
        {
            active = true;
            int strength = dungeonExpeditionManager.GetExpeditionStrength(expedition);
            int required = dungeonExpeditionManager.GetRequiredStrength(expedition.dungeon);
            CreateScrollLabel(expeditionContent, expedition.dungeon.dungeonName + " / " + expedition.memberInstanceIds.Count + "人 / 戦力 " + strength + "/" + required, ref top);
            DungeonExpedition selected = expedition;
            CreateScrollButton(expeditionContent, "呼び戻す", () => expeditionController.Recall(selected), ref top);
        }
        if (!active) CreateScrollLabel(expeditionContent, "進行中の遠征はありません", ref top);
        CreateScrollSection(expeditionContent, "新規編成: 踏破済みダンジョン", ref top);
        bool hasDungeon = false;
        foreach (DungeonDataSO dungeon in expeditionController.GetAvailableDungeons())
        {
            hasDungeon = true;
            DungeonDataSO selected = dungeon;
            CreateScrollButton(expeditionContent, (expeditionController.SelectedDungeon == dungeon ? "● " : "○ ") + dungeon.dungeonName + " / 要求戦力 " + dungeonExpeditionManager.GetRequiredStrength(dungeon), () => expeditionController.SelectDungeon(selected), ref top);
        }
        CreateScrollSection(expeditionContent, "隊員（最大3人）", ref top);
        if (!hasDungeon)
        {
            CreateScrollLabel(expeditionContent,
                "踏破済みダンジョンがありません。完全踏破すると遠征先に選べます。",
                ref top);
        }
        bool hasMember = false;
        foreach (MercenaryInstance mercenary in expeditionController.GetAvailableMembers())
        {
            hasMember = true;
            MercenaryInstance selected = mercenary;
            CreateScrollButton(expeditionContent, (expeditionController.IsSelected(mercenary) ? "● " : "○ ") + mercenary.MercenaryName + " Lv" + mercenary.Level, () => expeditionController.ToggleMember(selected), ref top);
        }
        if (!hasMember)
        {
            CreateScrollLabel(expeditionContent,
                "派遣可能な傭兵がいません。編成・輸送・遠征任務中の傭兵は選べません。",
                ref top);
        }
        CreateScrollLabel(expeditionContent, "部隊戦力 " + expeditionController.GetSelectedStrength() + " / 要求戦力 " + expeditionController.GetRequiredStrength(), ref top);
        expeditionContent.sizeDelta = new Vector2(0f, Mathf.Max(420f, -top + 12f));
    }

    private void HandleExpeditionChanged()
    {
        RefreshPage(companyPage);
        if (expeditionOverlay != null && expeditionOverlay.gameObject.activeSelf) RefreshExpeditionOverlay();
    }

    private void HandleExpeditionEvent(ExpeditionEvent expeditionEvent)
    {
        dailyResultController.RecordExpeditionEvent(expeditionEvent);
        if (expeditionEvent?.LimitedEquipment != null)
        {
            dailyResultController.RecordExpeditionLimitedEquipment(
                expeditionEvent.LimitedEquipment);
        }
        HandleExpeditionChanged();
    }
}
