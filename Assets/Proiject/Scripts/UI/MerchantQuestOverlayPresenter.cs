using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class MerchantQuestOverlayPresenter
{
    private static readonly Color ModalOverlayColor = UITheme.ModalOverlayColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color WhiteColor = Color.white;
    private static readonly Color LongTermGoalTextColor =
        new Color(1f, 0.9f, 0.68f, 1f);
    private static readonly Color SpecialQuestPaperColor =
        new Color(1f, 0.9f, 0.57f, 1f);
    private static readonly Color QuestPaperColor =
        new Color(1f, 0.96f, 0.82f, 1f);
    private static readonly Color MerchantStatusViewportColor =
        new Color(0f, 0f, 0f, 0.12f);
    private static readonly Color QuestBoardFallbackColor =
        new Color(0.24f, 0.12f, 0.055f, 1f);

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.QuestReferences questView;
    private readonly SimpleMercenaryHireUIView.MerchantStatusReferences
        merchantStatus;
    private readonly MerchantData merchantData;
    private readonly ProgressionManager progressionManager;
    private readonly MerchantStatusAndQuestController
        merchantStatusAndQuestController;
    private readonly Action refreshHealPage;
    private readonly Action refreshBlacksmithPage;
    private readonly Action refreshCompanyPage;
    private readonly Action refreshInventoryPage;
    private readonly Action refreshUI;

    public MerchantQuestOverlayPresenter(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.QuestReferences questView,
        SimpleMercenaryHireUIView.MerchantStatusReferences merchantStatus,
        MerchantData merchantData,
        ProgressionManager progressionManager,
        MerchantStatusAndQuestController merchantStatusAndQuestController,
        Action refreshHealPage,
        Action refreshBlacksmithPage,
        Action refreshCompanyPage,
        Action refreshInventoryPage,
        Action refreshUI)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.questView = questView ?? throw new ArgumentNullException(nameof(questView));
        this.merchantStatus = merchantStatus ??
            throw new ArgumentNullException(nameof(merchantStatus));
        this.merchantData = merchantData;
        this.progressionManager = progressionManager;
        this.merchantStatusAndQuestController = merchantStatusAndQuestController;
        this.refreshHealPage = refreshHealPage;
        this.refreshBlacksmithPage = refreshBlacksmithPage;
        this.refreshCompanyPage = refreshCompanyPage;
        this.refreshInventoryPage = refreshInventoryPage;
        this.refreshUI = refreshUI;
    }

    public void BuildMerchantStatusOverlay(RectTransform overlay)
    {
        merchantStatus.overlay = overlay;
        merchantStatus.overlay.gameObject.SetActive(false);
        merchantStatus.overlay.anchorMin = Vector2.zero;
        merchantStatus.overlay.anchorMax = Vector2.one;
        merchantStatus.overlay.offsetMin = Vector2.zero;
        merchantStatus.overlay.offsetMax = Vector2.zero;
        merchantStatus.overlay.gameObject.AddComponent<Image>().color =
            ModalOverlayColor;

        RectTransform window =
            CreateUIObject("Merchant Status Window", merchantStatus.overlay);
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
            MerchantStatusViewportColor;
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        merchantStatus.skillList = CreateUIObject("Merchant Skill List", viewport);
        merchantStatus.skillList.anchorMin = new Vector2(0f, 1f);
        merchantStatus.skillList.anchorMax = new Vector2(1f, 1f);
        merchantStatus.skillList.pivot = new Vector2(0.5f, 1f);

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = merchantStatus.skillList;
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

        merchantStatus.overlay.gameObject.SetActive(false);
    }

    public void BuildQuestOverlay(RectTransform overlay)
    {
        questView.overlay = overlay;
        questView.overlay.gameObject.SetActive(false);
        questView.overlay.anchorMin = Vector2.zero;
        questView.overlay.anchorMax = Vector2.one;
        questView.overlay.offsetMin = Vector2.zero;
        questView.overlay.offsetMax = Vector2.zero;
        questView.overlay.gameObject.AddComponent<Image>().color =
            ModalOverlayColor;

        RectTransform window = CreateUIObject("Quest Board Window", questView.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(860f, 620f);
        Image windowImage = window.gameObject.AddComponent<Image>();
        Sprite questBoardSprite = Resources.Load<Sprite>("UI/QuestBoard");
        if (questBoardSprite != null)
        {
            windowImage.sprite = questBoardSprite;
            windowImage.type = Image.Type.Sliced;
            windowImage.color = WhiteColor;
        }
        else
        {
            windowImage.color = QuestBoardFallbackColor;
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

        questView.list = CreateUIObject("Quest Board Content", viewport);
        questView.list.anchorMin = new Vector2(0f, 1f);
        questView.list.anchorMax = new Vector2(1f, 1f);
        questView.list.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = questView.list;
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
        questView.overlay.gameObject.SetActive(false);
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
            WhiteColor);
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

    public void HandleGoldChanged(int currentGold)
    {
        refreshHealPage?.Invoke();
        refreshBlacksmithPage?.Invoke();
        refreshUI?.Invoke();
    }

    public void HandleProgressionChanged()
    {
        refreshCompanyPage?.Invoke();
        refreshInventoryPage?.Invoke();
        if (merchantStatus.overlay != null &&
            merchantStatus.overlay.gameObject.activeSelf)
        {
            RebuildMerchantStatus();
        }
        if (questView.overlay != null && questView.overlay.gameObject.activeSelf)
        {
            RebuildQuestList();
        }
        refreshUI?.Invoke();
    }

    public void ShowQuestOverlay()
    {
        RebuildQuestList();
        questView.overlay.SetAsLastSibling();
        questView.overlay.gameObject.SetActive(true);
    }

    public void HideQuestOverlay()
    {
        HideQuestDetailWindow();
        questView.overlay?.gameObject.SetActive(false);
    }

    public void ShowMerchantStatusOverlay()
    {
        RebuildMerchantStatus();
        merchantStatus.overlay.SetAsLastSibling();
        merchantStatus.overlay.gameObject.SetActive(true);
    }

    public void HideMerchantStatusOverlay()
    {
        merchantStatus.overlay?.gameObject.SetActive(false);
    }

    public void RebuildMerchantStatus()
    {
        if (merchantStatus.skillList == null || merchantData == null)
        {
            return;
        }

        ClearChildren(merchantStatus.skillList);
        float top = 0f;

        RectTransform summaryRow =
            CreateRow("Merchant Summary", merchantStatus.skillList, top);
        CreateText(
            summaryRow,
            merchantStatusAndQuestController.BuildMerchantSummaryText(),
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -100f),
            new Vector2(-250f, -10f),
            WhiteColor);
        if (merchantStatusAndQuestController.ShouldShowRepayButtons())
        {
            BuildRepayStepper(summaryRow);
        }
        top -= 136f;

        CreateMerchantSkillRow(
            merchantStatus.skillList,
            MerchantSkillType.Negotiation,
            "交渉",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantStatus.skillList,
            MerchantSkillType.Leadership,
            "統率",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantStatus.skillList,
            MerchantSkillType.Appraisal,
            "鑑定",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantStatus.skillList,
            MerchantSkillType.Logistics,
            "兵站",
            top);
        top -= 112f;

        merchantStatus.skillList.sizeDelta =
            new Vector2(0f, Mathf.Max(470f, -top));
    }

    // 返済額を1万G単位で選び、確定ボタンで返済する。−／＋で1万ずつ増減し、
    // 上限は min(所持金, 残債)。残債が1万未満の最終返済は端数のまま選べる。
    // 返済後は RebuildMerchantStatus が走り、このUIごと作り直される。
    private void BuildRepayStepper(RectTransform parent)
    {
        int step = DebtManager.MonthlyMinimumPayment;
        int maxRepayable = merchantStatusAndQuestController.MaxRepayable();
        // 初期選択額は1万G（上限が1万未満ならその端数）。
        int selected = Mathf.Clamp(step, 0, maxRepayable);

        Text amountText = CreateText(
            parent,
            string.Empty,
            18,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(-210f, 20f),
            new Vector2(-60f, 56f),
            ParchmentTextColor);

        Button decrease = null;
        Button increase = null;
        Button confirm = null;

        void Refresh()
        {
            amountText.text = $"返済額 {selected:N0}G";
            if (decrease != null)
            {
                decrease.interactable = selected > 0;
            }
            if (increase != null)
            {
                increase.interactable = selected < maxRepayable;
            }
            if (confirm != null)
            {
                confirm.interactable =
                    merchantStatusAndQuestController.CanRepay() && selected > 0;
            }
        }

        // 1万G単位で増減する。上限側は端数（残債1万未満）にも張り付けられる。
        void Step(int direction)
        {
            if (direction > 0)
            {
                selected = Mathf.Min(maxRepayable, selected + step);
            }
            else
            {
                // 1万の倍数から下げるとき、端数上限からはまず倍数へ丸める。
                int lowered = selected - step;
                selected = Mathf.Max(0, (lowered / step) * step);
            }
            Refresh();
        }

        decrease = CreateActionButton(parent, "−1万", () => Step(-1));
        RectTransform decreaseRect = decrease.GetComponent<RectTransform>();
        decreaseRect.sizeDelta = new Vector2(64f, 40f);
        decreaseRect.anchoredPosition = new Vector2(-232f, -20f);

        increase = CreateActionButton(parent, "＋1万", () => Step(1));
        RectTransform increaseRect = increase.GetComponent<RectTransform>();
        increaseRect.sizeDelta = new Vector2(64f, 40f);
        increaseRect.anchoredPosition = new Vector2(-160f, -20f);

        Button full = CreateActionButton(
            parent,
            "全額",
            () =>
            {
                selected = maxRepayable;
                Refresh();
            });
        RectTransform fullRect = full.GetComponent<RectTransform>();
        fullRect.sizeDelta = new Vector2(56f, 40f);
        fullRect.anchoredPosition = new Vector2(-96f, -20f);
        full.interactable = maxRepayable > 0;

        confirm = CreateActionButton(
            parent,
            "この額で返済",
            () => merchantStatusAndQuestController.RepayDebt(selected));
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.sizeDelta = new Vector2(120f, 40f);
        confirmRect.anchoredPosition = new Vector2(-96f, -64f);

        Refresh();
    }

    public void RebuildQuestList()
    {
        if (questView.list == null || progressionManager == null)
        {
            return;
        }

        HideQuestDetailWindow();
        ClearChildren(questView.list);
        CreateText(
            questView.list,
            merchantStatusAndQuestController.BuildLongTermGoalText(),
            14,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(16f, -78f),
            new Vector2(-16f, -8f),
            LongTermGoalTextColor);

        RectTransform board = CreateUIObject("Quest Papers Board", questView.list);
        board.anchorMin = new Vector2(0f, 1f);
        board.anchorMax = new Vector2(1f, 1f);
        board.pivot = new Vector2(0.5f, 1f);
        const int columns = 4;
        const float paperWidth = 176f;
        const float paperHeight = 132f;
        const float horizontalSpacing = 18f;
        const float verticalSpacing = 20f;
        IReadOnlyList<QuestRecord> visibleQuests =
            progressionManager.GetAvailableQuestsForCurrentTown();
        int rowCount = Mathf.Max(
            1,
            Mathf.CeilToInt(visibleQuests.Count / (float)columns));
        float boardHeight = 24f + rowCount * paperHeight +
            Mathf.Max(0, rowCount - 1) * verticalSpacing;
        board.sizeDelta = new Vector2(0f, boardHeight);
        board.anchoredPosition = new Vector2(0f, -88f);
        for (int i = 0; i < visibleQuests.Count; i++)
        {
            QuestRecord quest = visibleQuests[i];
            QuestRecord questForDetail = quest;
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
                ? SpecialQuestPaperColor
                : QuestPaperColor;
            Button paperButton = paper.gameObject.AddComponent<Button>();
            paperButton.targetGraphic = paperImage;
            paperButton.onClick.AddListener(
                () => ShowQuestDetailWindow(questForDetail));
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
        questView.list.sizeDelta = new Vector2(0f, Mathf.Max(500f, 88f + boardHeight));
    }

    private void BuildQuestDetailWindow()
    {
        questView.detailWindow = CreateUIObject("Quest Detail Window", questView.overlay);
        questView.detailWindow.anchorMin = questView.detailWindow.anchorMax =
            questView.detailWindow.pivot = new Vector2(0.5f, 0.5f);
        questView.detailWindow.sizeDelta = new Vector2(560f, 430f);
        ApplyParchmentPanel(questView.detailWindow.gameObject.AddComponent<Image>());
        questView.detailWindow.gameObject.SetActive(false);
    }

    private void ShowQuestDetailWindow(QuestRecord quest)
    {
        if (progressionManager == null || quest == null)
        {
            return;
        }

        ClearChildren(questView.detailWindow);
        CreateText(
            questView.detailWindow,
            merchantStatusAndQuestController.BuildQuestTitle(quest),
            23,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            new Vector2(28f, -68f),
            new Vector2(-28f, -20f),
            ParchmentTextColor);
        CreateText(
            questView.detailWindow,
            quest.title + "\n\n" +
            merchantStatusAndQuestController.BuildQuestDetail(quest) + "\n\n" +
            $"報酬: {progressionManager.GetQuestGoldReward(quest):N0}G\n" +
            $"進行状況: {quest.currentAmount}/{quest.requiredAmount}\n" +
            $"期限: {quest.deadlineDay}日目",
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(42f, -330f),
            new Vector2(-42f, -92f),
            ParchmentTextColor);
        Button actionButton = CreateActionButton(
            questView.detailWindow,
            merchantStatusAndQuestController.GetQuestButtonLabel(quest),
            () =>
            {
                merchantStatusAndQuestController.AcceptQuest(quest.questId);
                HideQuestDetailWindow();
            });
        actionButton.interactable = progressionManager.CanAcceptQuestHere(quest);
        RectTransform actionRect = actionButton.GetComponent<RectTransform>();
        actionRect.anchorMin = actionRect.anchorMax = actionRect.pivot =
            new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(150f, 42f);
        actionRect.anchoredPosition = new Vector2(-88f, 24f);
        Button closeButton = CreateActionButton(
            questView.detailWindow,
            "閉じる",
            HideQuestDetailWindow);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot =
            new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(150f, 42f);
        closeRect.anchoredPosition = new Vector2(88f, 24f);
        questView.detailWindow.SetAsLastSibling();
        questView.detailWindow.gameObject.SetActive(true);
    }

    private void HideQuestDetailWindow()
    {
        if (questView.detailWindow != null)
        {
            questView.detailWindow.gameObject.SetActive(false);
        }
    }

    private RectTransform CreateUIObject(string objectName, Transform parent) =>
        SimpleMercenaryHireUIFactory.CreateUIObject(objectName, parent);

    private Text CreateText(
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color) =>
        factory.CreateText(
            parent, content, fontSize, fontStyle, alignment, offsetMin,
            offsetMax, color);

    private RectTransform CreateRow(
        string rowName,
        RectTransform parent,
        float top) =>
        factory.CreateRow(rowName, parent, top);

    private Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action) =>
        factory.CreateActionButton(parent, label, action);

    private static void ApplyParchmentPanel(Image target) =>
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(target);

    private static void AddFantasyFrame(Image image, float thickness) =>
        SimpleMercenaryHireUIFactory.AddFantasyFrame(image, thickness);

    private static void ApplyButtonTransitions(Button button) =>
        SimpleMercenaryHireUIFactory.ApplyButtonTransitions(button);

    private static void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
            child = null;
        }
    }

}
