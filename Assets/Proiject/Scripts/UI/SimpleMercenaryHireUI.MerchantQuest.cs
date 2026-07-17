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
        merchantStatusOverlay.gameObject.SetActive(false);
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
        questOverlay.gameObject.SetActive(false);
        questOverlay.anchorMin = Vector2.zero;
        questOverlay.anchorMax = Vector2.one;
        questOverlay.offsetMin = Vector2.zero;
        questOverlay.offsetMax = Vector2.zero;
        questOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject("Quest Board Window", questOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(860f, 620f);
        Image windowImage = window.gameObject.AddComponent<Image>();
        Sprite questBoardSprite = Resources.Load<Sprite>("UI/QuestBoard");
        if (questBoardSprite != null)
        {
            windowImage.sprite = questBoardSprite;
            windowImage.type = Image.Type.Sliced;
            windowImage.color = Color.white;
        }
        else
        {
            windowImage.color = new Color(0.24f, 0.12f, 0.055f, 1f);
            AddFantasyFrame(windowImage, 3f);
        }
        CreateText(
            window, "依頼", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f),
            ParchmentTextColor);

        RectTransform viewport = CreateUIObject("Quest Board Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(32f, 28f);
        viewport.offsetMax = new Vector2(-32f, -82f);
        viewport.gameObject.AddComponent<RectMask2D>();

        questList = CreateUIObject("Quest Board Content", viewport);
        questList.anchorMin = new Vector2(0f, 1f);
        questList.anchorMax = new Vector2(1f, 1f);
        questList.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = questList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        Button closeButton = CreateActionButton(window, "閉じる", HideQuestOverlay);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        BuildQuestDetailWindow();
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
        HideQuestDetailWindow();
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

        HideQuestDetailWindow();
        ClearChildren(questList);
        CreateText(
            questList,
            merchantStatusAndQuestController.BuildLongTermGoalText(),
            14,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(16f, -78f),
            new Vector2(-16f, -8f),
            new Color(1f, 0.9f, 0.68f, 1f));

        RectTransform board = CreateUIObject("Quest Papers Board", questList);
        board.anchorMin = new Vector2(0f, 1f);
        board.anchorMax = new Vector2(1f, 1f);
        board.pivot = new Vector2(0.5f, 1f);
        const int columns = 4;
        const float paperWidth = 176f;
        const float paperHeight = 132f;
        const float horizontalSpacing = 18f;
        const float verticalSpacing = 20f;
        int rowCount = Mathf.Max(
            1,
            Mathf.CeilToInt(progressionManager.Quests.Count / (float)columns));
        float boardHeight = 24f + rowCount * paperHeight +
            Mathf.Max(0, rowCount - 1) * verticalSpacing;
        board.sizeDelta = new Vector2(0f, boardHeight);
        board.anchoredPosition = new Vector2(0f, -88f);
        for (int i = 0; i < progressionManager.Quests.Count; i++)
        {
            int index = i;
            QuestRecord quest = progressionManager.Quests[i];
            RectTransform paper = CreateUIObject($"Quest Paper {i}", board);
            int column = i % columns;
            int row = i / columns;
            paper.anchorMin = new Vector2(0f, 1f);
            paper.anchorMax = new Vector2(0f, 1f);
            paper.pivot = new Vector2(0.5f, 0.5f);
            paper.sizeDelta = new Vector2(paperWidth, paperHeight);
            paper.anchoredPosition = new Vector2(
                16f + paperWidth * 0.5f + column * (paperWidth + horizontalSpacing),
                -12f - paperHeight * 0.5f - row * (paperHeight + verticalSpacing));
            paper.localRotation = Quaternion.Euler(
                0f,
                0f,
                QuestBoardLayout.GetPaperRotationDegrees(quest));
            Image paperImage = paper.gameObject.AddComponent<Image>();
            ApplyParchmentPanel(paperImage);
            paperImage.color = quest.isSpecial
                ? new Color(1f, 0.9f, 0.57f, 1f)
                : new Color(1f, 0.96f, 0.82f, 1f);
            Button paperButton = paper.gameObject.AddComponent<Button>();
            paperButton.targetGraphic = paperImage;
            paperButton.onClick.AddListener(() => ShowQuestDetailWindow(index));
            ApplyButtonTransitions(paperButton);
            CreateText(
                paper,
                quest.title,
                16,
                FontStyle.Bold,
                TextAnchor.UpperCenter,
                new Vector2(12f, -62f),
                new Vector2(-12f, -12f),
                ParchmentTextColor);
            CreateText(
                paper,
                $"{progressionManager.GetQuestGoldReward(quest):N0}G",
                18,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Vector2(12f, -112f),
                new Vector2(-12f, -70f),
                ParchmentTextColor);
        }
        questList.sizeDelta = new Vector2(0f, Mathf.Max(500f, 88f + boardHeight));
    }

    private void BuildQuestDetailWindow()
    {
        questDetailWindow = CreateUIObject("Quest Detail Window", questOverlay);
        questDetailWindow.anchorMin = questDetailWindow.anchorMax =
            questDetailWindow.pivot = new Vector2(0.5f, 0.5f);
        questDetailWindow.sizeDelta = new Vector2(560f, 430f);
        ApplyParchmentPanel(questDetailWindow.gameObject.AddComponent<Image>());
        questDetailWindow.gameObject.SetActive(false);
    }

    private void ShowQuestDetailWindow(int index)
    {
        if (progressionManager == null ||
            index < 0 ||
            index >= progressionManager.Quests.Count)
        {
            return;
        }

        ClearChildren(questDetailWindow);
        QuestRecord quest = progressionManager.Quests[index];
        CreateText(
            questDetailWindow,
            merchantStatusAndQuestController.BuildQuestTitle(quest),
            23,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            new Vector2(28f, -68f),
            new Vector2(-28f, -20f),
            ParchmentTextColor);
        CreateText(
            questDetailWindow,
            $"{quest.title}\n\n{merchantStatusAndQuestController.BuildQuestDetail(quest)}\n\n" +
            $"報酬: {progressionManager.GetQuestGoldReward(quest):N0}G / " +
            $"経験値 {progressionManager.GetQuestExperienceReward(quest):N0}\n" +
            $"進行状況: {quest.currentAmount}/{quest.requiredAmount}\n" +
            $"期限: {quest.deadlineDay}日目",
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(42f, -330f),
            new Vector2(-42f, -92f),
            ParchmentTextColor);
        Button actionButton = CreateActionButton(
            questDetailWindow,
            merchantStatusAndQuestController.GetQuestButtonLabel(quest),
            () =>
            {
                merchantStatusAndQuestController.AcceptQuest(index);
                HideQuestDetailWindow();
            });
        actionButton.interactable = MerchantStatusAndQuestController.CanAcceptQuest(quest);
        RectTransform actionRect = actionButton.GetComponent<RectTransform>();
        actionRect.anchorMin = actionRect.anchorMax = actionRect.pivot =
            new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(150f, 42f);
        actionRect.anchoredPosition = new Vector2(-88f, 24f);
        Button closeButton = CreateActionButton(
            questDetailWindow,
            "閉じる",
            HideQuestDetailWindow);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot =
            new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(150f, 42f);
        closeRect.anchoredPosition = new Vector2(88f, 24f);
        questDetailWindow.SetAsLastSibling();
        questDetailWindow.gameObject.SetActive(true);
    }

    private void HideQuestDetailWindow()
    {
        if (questDetailWindow != null)
        {
            questDetailWindow.gameObject.SetActive(false);
        }
    }

}
