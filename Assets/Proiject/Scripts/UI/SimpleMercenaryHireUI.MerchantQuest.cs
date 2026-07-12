using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildMerchantStatusOverlay()
    {
        merchantStatusOverlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.MerchantStatus,
                "Merchant Status Overlay");
        merchantStatusOverlay.anchorMin = Vector2.zero;
        merchantStatusOverlay.anchorMax = Vector2.one;
        merchantStatusOverlay.offsetMin = Vector2.zero;
        merchantStatusOverlay.offsetMax = Vector2.zero;
        merchantStatusOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Merchant Status Window", merchantStatusOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 580f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "商人ステータス",
            26,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -64f),
            new Vector2(-120f, -20f),
            ParchmentTextColor);

        RectTransform viewport =
            CreateUIObject("Merchant Status Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 28f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        merchantSkillList = CreateUIObject("Merchant Skill List", viewport);
        merchantSkillList.anchorMin = new Vector2(0f, 1f);
        merchantSkillList.anchorMax = new Vector2(1f, 1f);
        merchantSkillList.pivot = new Vector2(0.5f, 1f);

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = merchantSkillList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        Button closeButton =
            CreateActionButton(window, "閉じる", HideMerchantStatusOverlay);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        merchantStatusOverlay.gameObject.SetActive(false);
    }

    private void BuildQuestOverlay()
    {
        questOverlay = GetOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.Quest,
            "Quest Overlay");
        questOverlay.anchorMin = Vector2.zero;
        questOverlay.anchorMax = Vector2.one;
        questOverlay.offsetMin = Vector2.zero;
        questOverlay.offsetMax = Vector2.zero;
        questOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject("Quest Window", questOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 560f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(
            window, "依頼", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f),
            ParchmentTextColor);

        RectTransform viewport = CreateUIObject("Quest Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 28f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        questList = CreateUIObject("Quest List", viewport);
        questList.anchorMin = new Vector2(0f, 1f);
        questList.anchorMax = new Vector2(1f, 1f);
        questList.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = questList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        Button closeButton = CreateActionButton(window, "閉じる", HideQuestOverlay);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        questOverlay.gameObject.SetActive(false);
    }

    private void CreateMerchantSkillRow(
        RectTransform parent,
        MerchantSkillType skill,
        string label,
        float top)
    {
        RectTransform row = CreateRow($"Merchant Skill {skill}", parent, top);
        CreateText(
            row,
            merchantStatusAndQuestController.BuildSkillRowTitle(skill, label),
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -45f),
            new Vector2(-160f, -10f),
            Color.white);
        CreateText(
            row,
            merchantStatusAndQuestController.BuildSkillDescription(skill),
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -78f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        Button increaseButton = CreateActionButton(
            row,
            merchantStatusAndQuestController.GetSkillButtonLabel(skill),
            () => merchantStatusAndQuestController.IncreaseMerchantSkill(skill));
        increaseButton.interactable =
            merchantStatusAndQuestController.CanIncreaseSkill(skill);
    }

    private void HandleGoldChanged(int currentGold)
    {
        RefreshPage(healPage);
        RefreshPage(blacksmithPage);
        RefreshUI();
    }

    private void HandleProgressionChanged()
    {
        RefreshPage(companyPage);
        RefreshPage(inventoryPage);
        if (merchantStatusOverlay != null &&
            merchantStatusOverlay.gameObject.activeSelf)
        {
            RebuildMerchantStatus();
        }
        if (questOverlay != null && questOverlay.gameObject.activeSelf)
        {
            RebuildQuestList();
        }
        RefreshUI();
    }

    private void ShowQuestOverlay()
    {
        RebuildQuestList();
        questOverlay.SetAsLastSibling();
        questOverlay.gameObject.SetActive(true);
    }

    private void HideQuestOverlay()
    {
        questOverlay?.gameObject.SetActive(false);
    }

    private void ShowMerchantStatusOverlay()
    {
        RebuildMerchantStatus();
        merchantStatusOverlay.SetAsLastSibling();
        merchantStatusOverlay.gameObject.SetActive(true);
    }

    private void HideMerchantStatusOverlay()
    {
        merchantStatusOverlay?.gameObject.SetActive(false);
    }

    private void RebuildMerchantStatus()
    {
        if (merchantSkillList == null || merchantData == null)
        {
            return;
        }

        ClearChildren(merchantSkillList);
        float top = 0f;

        RectTransform summaryRow =
            CreateRow("Merchant Summary", merchantSkillList, top);
        CreateText(
            summaryRow,
            merchantStatusAndQuestController.BuildMerchantSummaryText(),
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -100f),
            new Vector2(-250f, -10f),
            Color.white);
        if (merchantStatusAndQuestController.ShouldShowRepayButtons())
        {
            Button fixedPayment = CreateActionButton(
                summaryRow,
                "1万G返済",
                () => merchantStatusAndQuestController.RepayDebt(
                    DebtManager.MonthlyMinimumPayment));
            RectTransform fixedRect = fixedPayment.GetComponent<RectTransform>();
            fixedRect.anchoredPosition = new Vector2(-80f, 24f);
            fixedPayment.interactable =
                merchantStatusAndQuestController.CanRepay();

            Button fullPayment = CreateActionButton(
                summaryRow,
                "全額返済",
                () => merchantStatusAndQuestController.RepayDebt(
                    debtManager.RemainingDebt));
            RectTransform fullRect = fullPayment.GetComponent<RectTransform>();
            fullRect.anchoredPosition = new Vector2(-80f, -24f);
            fullPayment.interactable =
                merchantStatusAndQuestController.CanRepay();
        }
        top -= 136f;

        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Negotiation,
            "交渉",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Leadership,
            "統率",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Appraisal,
            "鑑定",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Logistics,
            "兵站",
            top);
        top -= 112f;

        merchantSkillList.sizeDelta =
            new Vector2(0f, Mathf.Max(470f, -top));
    }

    private void RebuildQuestList()
    {
        if (questList == null || progressionManager == null)
        {
            return;
        }

        ClearChildren(questList);
        CreateText(
            questList,
            merchantStatusAndQuestController.BuildLongTermGoalText(),
            14,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(12f, -120f),
            new Vector2(-12f, 0f),
            MutedTextColor);
        float top = -132f;
        for (int i = 0; i < progressionManager.Quests.Count; i++)
        {
            int index = i;
            QuestRecord quest = progressionManager.Quests[i];
            RectTransform row = CreateRow($"Quest {i}", questList, top);
            CreateText(
                row,
                merchantStatusAndQuestController.BuildQuestTitle(quest),
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -40f),
                new Vector2(-150f, -10f),
                Color.white);
            CreateText(
                row,
                merchantStatusAndQuestController.BuildQuestDetail(quest),
                13,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -76f),
                new Vector2(-150f, -48f),
                MutedTextColor);
            Button button = CreateActionButton(
                row,
                merchantStatusAndQuestController.GetQuestButtonLabel(quest),
                () => merchantStatusAndQuestController.AcceptQuest(index));
            button.interactable =
                MerchantStatusAndQuestController.CanAcceptQuest(quest);
            top -= 112f;
        }
        questList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -top));
    }

}
