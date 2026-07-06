using System.Collections.Generic;
using System.Text;
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
        string description,
        float top)
    {
        int rank = merchantData.GetSkillRank(skill);
        RectTransform row = CreateRow($"Merchant Skill {skill}", parent, top);
        CreateText(
            row,
            $"{label}  Lv{rank}/{MerchantData.MaxSkillRank}",
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -45f),
            new Vector2(-160f, -10f),
            Color.white);
        CreateText(
            row,
            description,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -78f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        Button increaseButton = CreateActionButton(
            row,
            rank >= MerchantData.MaxSkillRank ? "最大" : "+1",
            () => IncreaseMerchantSkill(skill));
        increaseButton.interactable =
            merchantData.MerchantSkillPoints > 0 &&
            rank < MerchantData.MaxSkillRank;
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

    private void IncreaseMerchantSkill(MerchantSkillType skill)
    {
        if (merchantData.TryIncreaseSkill(skill))
        {
            statusText.text =
                $"商人技能を強化しました。残りポイント " +
                $"{merchantData.MerchantSkillPoints}";
        }
        RebuildMerchantStatus();
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
        string growthProgress = merchantData.MerchantLevel >=
                                MerchantData.MaxMerchantLevel
            ? "最高レベル"
            : $"獲得G進行 {merchantData.MerchantExperience:N0}/" +
              $"{merchantData.ExperienceToNextLevel:N0}";
        string debtSummary = debtManager == null
            ? "借金情報なし"
            : debtManager.IsDebtCleared
                ? "借金完済 - ゲームクリア"
                : $"借金残高 {debtManager.RemainingDebt:N0}G  |  " +
                  $"{debtManager.CurrentMonth}月目  |  " +
                  $"次回最低返済 {debtManager.NextMinimumPayment:N0}G " +
                  $"（あと{debtManager.DaysUntilPayment}日）";
        CreateText(
            summaryRow,
            $"商人Lv {merchantData.MerchantLevel}  " +
            $"{growthProgress}  " +
            $"所持金 {merchantData.Gold}G\n" +
            $"未使用技能ポイント {merchantData.MerchantSkillPoints}  |  " +
            $"累計獲得 {merchantData.LifetimeGoldEarned:N0}G\n" +
            debtSummary,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -100f),
            new Vector2(-250f, -10f),
            Color.white);
        if (debtManager != null && !debtManager.IsDebtCleared)
        {
            Button fixedPayment = CreateActionButton(
                summaryRow,
                "1万G返済",
                () => RepayDebt(DebtManager.MonthlyMinimumPayment));
            RectTransform fixedRect = fixedPayment.GetComponent<RectTransform>();
            fixedRect.anchoredPosition = new Vector2(-80f, 24f);
            fixedPayment.interactable = merchantData.Gold > 0;

            Button fullPayment = CreateActionButton(
                summaryRow,
                "全額返済",
                () => RepayDebt(debtManager.RemainingDebt));
            RectTransform fullRect = fullPayment.GetComponent<RectTransform>();
            fullRect.anchoredPosition = new Vector2(-80f, -24f);
            fullPayment.interactable = merchantData.Gold > 0;
        }
        top -= 136f;

        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Negotiation,
            "交渉",
            $"仕入れ {merchantData.GetMarketBuyMultiplier() * 100f:0}% / " +
            $"売却 {merchantData.GetMarketSellMultiplier() * 100f:0}%\n" +
            "Lv3 値切り術 / Lv7 商談の達人",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Leadership,
            "統率",
            $"雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}% / " +
            $"契約更新費 {merchantData.GetRenewalCostMultiplier() * 100f:0}%\n" +
            "Lv3 人を見る目 / Lv7 契約管理",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Appraisal,
            "鑑定",
            $"依頼ゴールド {merchantData.GetQuestGoldMultiplier() * 100f:0}% / " +
            "依頼収入は商人Lvへ反映\n" +
            "Lv3 目利き / Lv7 慧眼",
            top);
        top -= 112f;
        CreateMerchantSkillRow(
            merchantSkillList,
            MerchantSkillType.Logistics,
            "兵站",
            $"倉庫容量 +{merchantData.GetStorageCapacityBonus()} / " +
            $"探索費用 {merchantData.GetExplorationExpenseMultiplier() * 100f:0}%\n" +
            "Lv3 荷役整理 / Lv7 遠征計画",
            top);
        top -= 112f;

        merchantSkillList.sizeDelta =
            new Vector2(0f, Mathf.Max(470f, -top));
    }

    private void RepayDebt(int amount)
    {
        if (debtManager == null)
        {
            return;
        }

        int paid = debtManager.Repay(amount);
        statusText.text = paid > 0
            ? debtManager.IsDebtCleared
                ? "借金1億Gを完済しました。ゲームクリアです！"
                : $"{paid:N0}Gを返済しました。"
            : "返済できる所持金がありません。";
        RebuildMerchantStatus();
        RefreshUI();
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
            "長期目標\n" + progressionManager.GetAchievementSummary(),
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
            string type = quest.isSpecial ? "特殊" : "通常";
            string state = quest.completed
                ? "達成済み"
                : quest.expired
                    ? "期限切れ"
                    : quest.accepted ? "進行中" : "未受注";
            CreateText(
                row,
                $"[{type}] {quest.title}  {state}",
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -40f),
                new Vector2(-150f, -10f),
                Color.white);
            string target = quest.questType == QuestType.ItemDelivery
                ? JapaneseDisplayText.GetItemNameByRawName(quest.targetName)
                : JapaneseDisplayText.GetEnemyName(quest.targetName);
            CreateText(
                row,
                $"{target} {quest.currentAmount}/{quest.requiredAmount}  " +
                $"期限 {quest.deadlineDay}日  報酬 " +
                $"{progressionManager.GetQuestGoldReward(quest)}G",
                13,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Vector2(16f, -76f),
                new Vector2(-150f, -48f),
                MutedTextColor);
            Button button = CreateActionButton(
                row,
                quest.accepted ? state : "受注",
                () => AcceptQuest(index));
            button.interactable =
                !quest.accepted && !quest.completed && !quest.expired;
            top -= 112f;
        }
        questList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -top));
    }

    private void AcceptQuest(int index)
    {
        if (progressionManager.AcceptQuest(index))
        {
            statusText.text = "依頼を受注しました。";
        }
        RebuildQuestList();
        RefreshUI();
    }

    private void UpgradeStorage()
    {
        if (progressionManager != null &&
            progressionManager.TryUpgradeStorage())
        {
            statusText.text = "倉庫を拡張しました。";
        }
        else
        {
            statusText.text = "商人レベルまたはゴールドが不足しています。";
        }
        RefreshUI();
    }

    private void RenewContract(MercenaryInstance mercenary)
    {
        if (hireManager.TryRenewContract(mercenary))
        {
            statusText.text = $"{mercenary.MercenaryName}の契約を更新しました。";
        }
        else
        {
            statusText.text = "契約を更新できませんでした。";
        }
        RefreshPage(companyPage);
        RefreshUI();
    }

}
