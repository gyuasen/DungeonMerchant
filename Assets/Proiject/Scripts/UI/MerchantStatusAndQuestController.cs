using System;

/// <summary>
/// Owns the merchant-status and quest content building (summary text,
/// skill row labels/descriptions, quest row text) and the business
/// actions (skill point allocation, debt repayment, quest accept,
/// storage upgrade, contract renewal). Extracted from
/// SimpleMercenaryHireUI (step 3.9). Overlay construction, row
/// construction and Show/Hide routing stay in
/// SimpleMercenaryHireUI.MerchantQuest.cs; only the content and
/// business actions live here.
/// </summary>
public sealed class MerchantStatusAndQuestController
{
    private readonly MerchantData merchantData;
    private readonly ProgressionManager progressionManager;
    private readonly DebtManager debtManager;
    private readonly MercenaryHireManager hireManager;
    private readonly Action<string> setStatus;
    private readonly Action rebuildMerchantStatus;
    private readonly Action rebuildQuestList;
    private readonly Action refreshCompanyPage;
    private readonly Action refreshUI;

    public MerchantStatusAndQuestController(
        MerchantData merchantData,
        ProgressionManager progressionManager,
        DebtManager debtManager,
        MercenaryHireManager hireManager,
        Action<string> setStatus,
        Action rebuildMerchantStatus,
        Action rebuildQuestList,
        Action refreshCompanyPage,
        Action refreshUI)
    {
        this.merchantData = merchantData;
        this.progressionManager = progressionManager;
        this.debtManager = debtManager;
        this.hireManager = hireManager;
        this.setStatus = setStatus;
        this.rebuildMerchantStatus = rebuildMerchantStatus;
        this.rebuildQuestList = rebuildQuestList;
        this.refreshCompanyPage = refreshCompanyPage;
        this.refreshUI = refreshUI;
    }

    public string BuildMerchantSummaryText()
    {
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
        return $"商人Lv {merchantData.MerchantLevel}  " +
               $"{growthProgress}  " +
               $"所持金 {merchantData.Gold}G\n" +
               $"未使用技能ポイント {merchantData.MerchantSkillPoints}  |  " +
               $"累計獲得 {merchantData.LifetimeGoldEarned:N0}G\n" +
               debtSummary;
    }

    public bool ShouldShowRepayButtons()
    {
        return debtManager != null && !debtManager.IsDebtCleared;
    }

    public bool CanRepay()
    {
        return merchantData.Gold > 0;
    }

    public string BuildSkillRowTitle(MerchantSkillType skill, string label)
    {
        int rank = merchantData.GetSkillRank(skill);
        return $"{label}  Lv{rank}/{MerchantData.MaxSkillRank}";
    }

    public string BuildSkillDescription(MerchantSkillType skill)
    {
        switch (skill)
        {
            case MerchantSkillType.Negotiation:
                return $"仕入れ {merchantData.GetMarketBuyMultiplier() * 100f:0}% / " +
                       $"売却 {merchantData.GetMarketSellMultiplier() * 100f:0}%\n" +
                       "Lv3 値切り術 / Lv7 商談の達人";
            case MerchantSkillType.Leadership:
                return $"雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}% / " +
                       $"契約更新費 {merchantData.GetRenewalCostMultiplier() * 100f:0}%\n" +
                       "Lv3 人を見る目 / Lv7 契約管理";
            case MerchantSkillType.Appraisal:
                return $"依頼ゴールド {merchantData.GetQuestGoldMultiplier() * 100f:0}% / " +
                       "依頼収入は商人Lvへ反映\n" +
                       "Lv3 目利き / Lv7 慧眼";
            case MerchantSkillType.Logistics:
                return $"倉庫容量 +{merchantData.GetStorageCapacityBonus()} / " +
                       $"探索費用 {merchantData.GetExplorationExpenseMultiplier() * 100f:0}%\n" +
                       "Lv3 荷役整理 / Lv7 遠征計画";
            default:
                return string.Empty;
        }
    }

    public string GetSkillButtonLabel(MerchantSkillType skill)
    {
        return merchantData.GetSkillRank(skill) >= MerchantData.MaxSkillRank
            ? "最大"
            : "+1";
    }

    public bool CanIncreaseSkill(MerchantSkillType skill)
    {
        return merchantData.MerchantSkillPoints > 0 &&
               merchantData.GetSkillRank(skill) < MerchantData.MaxSkillRank;
    }

    public void IncreaseMerchantSkill(MerchantSkillType skill)
    {
        if (merchantData.TryIncreaseSkill(skill))
        {
            setStatus(
                $"商人技能を強化しました。残りポイント " +
                $"{merchantData.MerchantSkillPoints}");
        }
        rebuildMerchantStatus();
        refreshUI();
    }

    public void RepayDebt(int amount)
    {
        if (debtManager == null)
        {
            return;
        }

        int paid = debtManager.Repay(amount);
        setStatus(paid > 0
            ? debtManager.IsDebtCleared
                ? "借金1億Gを完済しました。ゲームクリアです！"
                : $"{paid:N0}Gを返済しました。"
            : "返済できる所持金がありません。");
        rebuildMerchantStatus();
        refreshUI();
    }

    public string BuildLongTermGoalText()
    {
        return "長期目標\n" + progressionManager.GetAchievementSummary();
    }

    public string BuildQuestTitle(QuestRecord quest)
    {
        string type = quest.isSpecial ? "特殊" : "通常";
        return $"[{type}] {quest.title}  {GetQuestState(quest)}";
    }

    public string BuildQuestDetail(QuestRecord quest)
    {
        string target = quest.questType == QuestType.ItemDelivery
            ? JapaneseDisplayText.GetItemNameByRawName(quest.targetName)
            : JapaneseDisplayText.GetEnemyName(quest.targetName);
        return $"{target} {quest.currentAmount}/{quest.requiredAmount}  " +
               $"期限 {quest.deadlineDay}日  報酬 " +
               $"{progressionManager.GetQuestGoldReward(quest)}G";
    }

    public string GetQuestButtonLabel(QuestRecord quest)
    {
        return quest.accepted ? GetQuestState(quest) : "受注";
    }

    public static bool CanAcceptQuest(QuestRecord quest)
    {
        return !quest.accepted && !quest.completed && !quest.expired;
    }

    public void AcceptQuest(int index)
    {
        if (progressionManager.AcceptQuest(index))
        {
            setStatus("依頼を受注しました。");
        }
        rebuildQuestList();
        refreshUI();
    }

    public void UpgradeStorage()
    {
        TryUpgradeStorage();
    }

    public bool TryUpgradeStorage()
    {
        if (progressionManager != null && progressionManager.TryUpgradeStorage())
        {
            setStatus("倉庫を拡張しました。");
            refreshUI();
            return true;
        }
        setStatus("商人レベルまたはゴールドが不足しています。");
        refreshUI();
        return false;
    }

    public void RenewContract(MercenaryInstance mercenary)
    {
        if (hireManager.TryRenewContract(mercenary))
        {
            setStatus($"{mercenary.MercenaryName}の契約を更新しました。");
        }
        else
        {
            setStatus("契約を更新できませんでした。");
        }
        refreshCompanyPage();
        refreshUI();
    }

    private static string GetQuestState(QuestRecord quest)
    {
        return quest.completed
            ? "達成済み"
            : quest.expired
                ? "期限切れ"
                : quest.accepted ? "進行中" : "未受注";
    }
}
