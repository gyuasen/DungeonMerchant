using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Owns the daily-result ("一日のリザルト") snapshot data and builds the
/// overlay text shown when the day changes. Extracted from
/// SimpleMercenaryHireUI (step 3.3). Overlay creation/show/hide routing
/// stays in SimpleMercenaryHireUI.DailyResult.cs; only the data and
/// content-building logic lives here.
/// </summary>
public sealed class DailyResultController
{
    private const string PositiveColor = "#65D88A";
    private const string NegativeColor = "#FF7474";
    private const string NeutralColor = "#AEB6BE";
    private const string HeadingColor = "#5A3B24";
    private const string AccentColor = "#B86B2B";
    private readonly MerchantData merchantData;
    private readonly MercenaryHireManager hireManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly MerchantInventory merchantInventory;
    private readonly ProgressionManager progressionManager;
    private readonly Func<EquipmentInstance, string> getEquipmentDisplayName;

    private int dailySnapshotDay;
    private int dailySnapshotGold;
    private int dailySnapshotMerchantLevel;
    private int dailySnapshotMerchantExperience;
    private int dailySnapshotSkillPoints;
    private int dailySnapshotNegotiation;
    private int dailySnapshotLeadership;
    private int dailySnapshotAppraisal;
    private int dailySnapshotLogistics;
    private int dailySnapshotStorageUsed;
    private int dailySnapshotStorageCapacity;
    private readonly Dictionary<string, DailyMercenarySnapshot>
        dailyMercenarySnapshots =
            new Dictionary<string, DailyMercenarySnapshot>();
    private readonly Dictionary<string, int> dailyInventoryAmounts =
        new Dictionary<string, int>();
    private readonly Dictionary<string, int> dailyAcquiredItems =
        new Dictionary<string, int>();
    private readonly HashSet<string> knownEquipmentInstanceIds =
        new HashSet<string>();
    private readonly List<string> dailyAcquiredEquipment =
        new List<string>();
    private readonly List<string> dailyTransportEvents = new List<string>();
    private readonly List<string> dailyQuestCompletionLines = new List<string>();
    private readonly List<string> trainingCompletionLines = new List<string>();
    private readonly HashSet<string> trainingCompletionKeys =
        new HashSet<string>();
    private readonly HashSet<string> dailyHiredMercenaryIds =
        new HashSet<string>();
    private readonly List<GoldTransaction> dailyGoldLedger =
        new List<GoldTransaction>();

    private sealed class DailyMercenarySnapshot
    {
        public string Name;
        public int Level;
        public int Experience;
        public int MaxHP;
        public int CurrentHP;
        public int Attack;
        public int Defense;
        public int MaxMagicPower;
        public float AttackSpeed;
        public BattleStatusEffect StatusEffect;
        public bool ContractActive;
        public bool ContractNeedsRenewal;
        public bool WasDefeated;
        public bool WasInParty;
    }

    public DailyResultController(
        MerchantData merchantData,
        MercenaryHireManager hireManager,
        MercenaryPartyManager partyManager,
        MerchantInventory merchantInventory,
        ProgressionManager progressionManager,
        Func<EquipmentInstance, string> getEquipmentDisplayName)
    {
        this.merchantData = merchantData;
        this.hireManager = hireManager;
        this.partyManager = partyManager;
        this.merchantInventory = merchantInventory;
        this.progressionManager = progressionManager;
        this.getEquipmentDisplayName = getEquipmentDisplayName;
        if (progressionManager != null)
        {
            progressionManager.QuestCompleted += HandleQuestCompleted;
        }
        if (merchantData != null)
        {
            merchantData.GoldTransactionRecorded += HandleGoldTransactionRecorded;
        }
    }

    /// <summary>
    /// Builds the daily-result overlay text for the transition into
    /// <paramref name="currentDay"/>. Returns null when there is nothing to
    /// show yet (no full day has elapsed since the last snapshot); the
    /// caller should still call <see cref="CaptureDailySnapshot"/> either
    /// way, matching the original behavior.
    /// </summary>
    public string BuildDailyResultText(int currentDay)
    {
        if (currentDay <= dailySnapshotDay)
        {
            return null;
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine(Heading(
            $"{dailySnapshotDay}日目の終了 → {currentDay}日目"));
        result.AppendLine();
        result.AppendLine(Heading("【商人】"));

        bool hasMerchantChange = false;
        int goldChange = merchantData.Gold - dailySnapshotGold;
        if (goldChange != 0)
        {
            result.AppendLine(ColorByDirection(
                $"所持金  {dailySnapshotGold}G → {merchantData.Gold}G " +
                $"({FormatSignedValue(goldChange)}G)", goldChange > 0));
            hasMerchantChange = true;
        }
        if (merchantData.MerchantLevel > dailySnapshotMerchantLevel)
        {
            result.AppendLine(Emphasis(
                $"★ レベルアップ  Lv{dailySnapshotMerchantLevel} → " +
                $"Lv{merchantData.MerchantLevel}"));
            hasMerchantChange = true;
        }
        if (merchantData.MerchantLevel == dailySnapshotMerchantLevel &&
            merchantData.MerchantExperience >
            dailySnapshotMerchantExperience)
        {
            result.AppendLine(Positive(
                $"獲得G進行  +{merchantData.MerchantExperience - dailySnapshotMerchantExperience} " +
                $"({merchantData.MerchantExperience}/" +
                $"{merchantData.ExperienceToNextLevel})"));
            hasMerchantChange = true;
        }
        else if (merchantData.MerchantLevel > dailySnapshotMerchantLevel)
        {
            result.AppendLine(Positive(
                $"現在の獲得G進行  {merchantData.MerchantExperience}/" +
                $"{merchantData.ExperienceToNextLevel}"));
        }
        if (merchantData.MerchantSkillPoints != dailySnapshotSkillPoints)
        {
            result.AppendLine(ColorByDirection(
                $"技能ポイント  {dailySnapshotSkillPoints} → " +
                $"{merchantData.MerchantSkillPoints}",
                merchantData.MerchantSkillPoints > dailySnapshotSkillPoints));
            hasMerchantChange = true;
        }
        hasMerchantChange |= AppendRankChange(
            result, "交渉", dailySnapshotNegotiation, merchantData.Negotiation);
        hasMerchantChange |= AppendRankChange(
            result, "統率", dailySnapshotLeadership, merchantData.Leadership);
        hasMerchantChange |= AppendRankChange(
            result, "鑑定", dailySnapshotAppraisal, merchantData.Appraisal);
        hasMerchantChange |= AppendRankChange(
            result, "物流", dailySnapshotLogistics, merchantData.Logistics);
        if (!hasMerchantChange)
        {
            result.AppendLine(Neutral("大きな変化はありません。"));
        }

        int storageUsed = merchantInventory != null
            ? merchantInventory.GetUsedStorageSlots()
            : 0;
        int storageCapacity = progressionManager != null
            ? progressionManager.StorageCapacity
            : 0;
        int storageRemaining = Math.Max(0, storageCapacity - storageUsed);
        result.AppendLine();
        result.AppendLine(Heading("【倉庫】"));
        if (dailySnapshotStorageUsed != storageUsed ||
            dailySnapshotStorageCapacity != storageCapacity)
        {
            result.AppendLine(Neutral(
                $"使用量 {dailySnapshotStorageUsed}/{dailySnapshotStorageCapacity} → " +
                $"{storageUsed}/{storageCapacity}"));
        }
        else
        {
            result.AppendLine(Neutral($"使用量 {storageUsed}/{storageCapacity}"));
        }
        result.AppendLine(Neutral($"空き容量 {storageRemaining}"));
        if (storageCapacity > 0 && storageRemaining == 0)
        {
            result.AppendLine(Negative("！倉庫が満杯です。売却または倉庫拡張を行ってください。"));
        }

        result.AppendLine();
        result.AppendLine(Heading("【入手アイテム】"));
        if (dailyAcquiredItems.Count == 0 &&
            dailyAcquiredEquipment.Count == 0)
        {
            result.AppendLine(Neutral("入手したアイテムはありません。"));
        }

        else
        {
            foreach (KeyValuePair<string, int> entry in dailyAcquiredItems)
            {
                result.AppendLine(Positive($"・{EscapeRichText(entry.Key)} ×{entry.Value}"));
            }
            foreach (string equipmentName in dailyAcquiredEquipment)
            {
                result.AppendLine(Positive($"・{EscapeRichText(equipmentName)}"));
            }
        }

        result.AppendLine();
        result.AppendLine(Heading("【輸送】"));
        if (dailyTransportEvents.Count == 0)
        {
            result.AppendLine(Neutral("輸送に関する報告はありません。"));
        }
        else
        {
            foreach (string transportEvent in dailyTransportEvents)
            {
                result.AppendLine(transportEvent);
            }
        }

        result.AppendLine();
        result.AppendLine(Heading("【依頼達成】"));
        if (dailyQuestCompletionLines.Count == 0)
        {
            result.AppendLine(Neutral("依頼達成はありません。"));
        }
        else
        {
            foreach (string line in dailyQuestCompletionLines)
            {
                result.AppendLine(Emphasis(line));
            }
        }

        List<string> mercenaryLines = new List<string>();
        List<string> contractLines = new List<string>();
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary == null)
            {
                continue;
            }
            dailyMercenarySnapshots.TryGetValue(
                mercenary.InstanceId,
                out DailyMercenarySnapshot previous);

            bool inParty = partyManager != null && partyManager.Contains(mercenary);
            bool isNewHire = dailyHiredMercenaryIds.Contains(mercenary.InstanceId) ||
                previous == null;
            string changeLine = BuildMercenaryChangeLine(
                mercenary,
                previous,
                inParty,
                isNewHire);
            if (!string.IsNullOrEmpty(changeLine))
            {
                mercenaryLines.Add(changeLine);
            }

            if (previous != null &&
                previous.ContractActive &&
                !mercenary.IsContractActive)
            {
                bool removedFromParty =
                    previous.WasInParty && !inParty;
                contractLines.Add(
                    Negative($"! {EscapeRichText(previous.Name)}: " +
                    $"{JapaneseDisplayText.GetContractType(mercenary.ContractType)}が終了" +
                    (removedFromParty ? "（編成から外れました）" : string.Empty)));
            }
        }

        result.AppendLine();
        result.AppendLine(Heading("【傭兵の成長・現在状況】"));
        foreach (string trainingLine in trainingCompletionLines)
        {
            result.AppendLine(trainingLine);
        }
        if (mercenaryLines.Count == 0)
        {
            result.AppendLine(Neutral("本日、状態が変化した傭兵はいません。"));
        }
        else
        {
            foreach (string line in mercenaryLines)
            {
                result.AppendLine(line);
            }
        }

        result.AppendLine();
        result.AppendLine(Heading("【契約】"));
        if (contractLines.Count == 0)
        {
            result.AppendLine(Neutral("契約終了による編成変更はありません。"));
        }
        else
        {
            foreach (string line in contractLines)
            {
                result.AppendLine(line);
            }
            result.AppendLine(Negative("商会画面から契約を更新できます。"));
        }

        AppendDailyGoldLedger(
            result,
            dailySnapshotDay,
            currentDay,
            goldChange);
        RemoveGoldLedgerEntries(dailySnapshotDay, currentDay);

        return result.ToString().TrimEnd();
    }

    private void HandleGoldTransactionRecorded(GoldTransaction transaction)
    {
        if (transaction != null)
        {
            dailyGoldLedger.Add(transaction);
        }
    }

    private void HandleQuestCompleted(QuestCompletionInfo completion)
    {
        if (completion?.Quest == null)
        {
            return;
        }
        string target = completion.Quest.questType == QuestType.ItemDelivery
            ? EscapeRichText(JapaneseDisplayText.GetItemNameByRawName(completion.Quest.targetName)) +
              " ×" + completion.DeliveredAmount
            : EscapeRichText(JapaneseDisplayText.GetEnemyName(completion.Quest.targetName));
        dailyQuestCompletionLines.Add(
            "依頼達成: " + EscapeRichText(completion.Quest.title) + " (" + target + ") " +
            completion.GoldReward + "G / " + GetTownName(completion.TownIndex));
    }

    public void CaptureDailySnapshot(int currentDay)
    {
        if (merchantData == null || hireManager == null)
        {
            return;
        }

        dailySnapshotDay = currentDay;
        dailySnapshotGold = merchantData.Gold;
        dailySnapshotMerchantLevel = merchantData.MerchantLevel;
        dailySnapshotMerchantExperience = merchantData.MerchantExperience;
        dailySnapshotSkillPoints = merchantData.MerchantSkillPoints;
        dailySnapshotNegotiation = merchantData.Negotiation;
        dailySnapshotLeadership = merchantData.Leadership;
        dailySnapshotAppraisal = merchantData.Appraisal;
        dailySnapshotLogistics = merchantData.Logistics;
        dailySnapshotStorageUsed = merchantInventory != null
            ? merchantInventory.GetUsedStorageSlots()
            : 0;
        dailySnapshotStorageCapacity = progressionManager != null
            ? progressionManager.StorageCapacity
            : 0;
        dailyMercenarySnapshots.Clear();

        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            CaptureMercenarySnapshot(mercenary);
        }
        ResetDailyInventoryTracking();
        dailyTransportEvents.Clear();
        dailyQuestCompletionLines.Clear();
        trainingCompletionLines.Clear();
        trainingCompletionKeys.Clear();
        dailyHiredMercenaryIds.Clear();
    }

    public string RecordTrainingCompleted(TrainingReservation reservation)
    {
        if (reservation == null)
        {
            return string.Empty;
        }

        string key = reservation.MercenaryInstanceId + ":" +
            reservation.CompletionDay;
        if (!trainingCompletionKeys.Add(key))
        {
            return string.Empty;
        }

        MercenaryInstance mercenary = null;
        foreach (MercenaryInstance candidate in hireManager.HiredMercenaries)
        {
            if (candidate != null &&
                candidate.InstanceId == reservation.MercenaryInstanceId)
            {
                mercenary = candidate;
                break;
            }
        }

        string name = mercenary != null
            ? EscapeRichText(mercenary.MercenaryName)
            : "傭兵";
        string line = Emphasis(
            $"★ 修練完了: {name}がLv{reservation.TargetLevel}になった");
        trainingCompletionLines.Add(line);
        return line;
    }

    public void ConsumeRecordedTrainingCompletion(string line)
    {
        if (!string.IsNullOrEmpty(line))
        {
            trainingCompletionLines.Remove(line);
        }
    }

    #if false
    public void RecordTransportEvent(TransportEvent transportEvent)
    {
        if (transportEvent?.Convoy == null)
        {
            return;
        }
        TransportConvoy convoy = transportEvent.Convoy;
        string origin = GetTownName(convoy.originTownIndex);
        string destination = GetTownName(convoy.destinationTownIndex);
        switch (transportEvent.Type)
        {
            case TransportEventType.RaidRepelled:
                dailyTransportEvents.Add($"輸送: {origin}→{destination} 襲撃を撃退");
                break;
            case TransportEventType.RaidLoss:
                dailyTransportEvents.Add($"輸送: 積荷{transportEvent.LostCargo}個を損失");
                break;
            case TransportEventType.Arrived:
                dailyTransportEvents.Add(
                    $"輸送部隊が{destination}に到着、積荷{transportEvent.Gold:N0}個を倉庫へ搬入しました");
                break;
        }
    }

    #endif

    public void RecordRemoteSaleEvent(RemoteSaleEvent remoteSaleEvent)
    {
        if (remoteSaleEvent == null || remoteSaleEvent.Order == null)
        {
            return;
        }
        RemoteSaleOrder order = remoteSaleEvent.Order;
        string town = GetTownName(order.TownIndex);
        string itemName = order.IsEquipment
            ? JapaneseDisplayText.GetItemName(order.Equipment.BaseItem)
            : JapaneseDisplayText.GetItemName(order.Item) + "×" + order.Amount;
        dailyTransportEvents.Add("遠隔売却: " + town + "で" + itemName + "を" +
            remoteSaleEvent.Gold + "Gで売却");
    }

    #if false
    public void RecordExpeditionEvent(ExpeditionEvent expeditionEvent)
    {
        if (expeditionEvent?.Expedition?.dungeon == null)
        {
            return;
        }
        string dungeonName = expeditionEvent.Expedition.dungeon.dungeonName;
        if (expeditionEvent.Type == ExpeditionEventType.Failed)
        {
            dailyTransportEvents.Add("遠征: " + dungeonName + " を周回できず、隊員が負傷");
            return;
        }
        dailyTransportEvents.Add("遠征: " + dungeonName + " を周回、" + expeditionEvent.Gold + "Gと素材を獲得");
    }

    public void RecordExpeditionLimitedEquipment(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return;
        }

        dailyTransportEvents.Add(
            "遠征: 限定装備『" +
            JapaneseDisplayText.GetItemName(equipment.BaseItem) +
            "』を持ち帰った！");
    }

    #endif

    public void CaptureMercenarySnapshot(MercenaryInstance mercenary)
    {
        if (mercenary == null ||
            string.IsNullOrEmpty(mercenary.InstanceId))
        {
            return;
        }
        dailyMercenarySnapshots[mercenary.InstanceId] =
            new DailyMercenarySnapshot
            {
                Name = mercenary.MercenaryName,
                Level = mercenary.Level,
                Experience = mercenary.CurrentExperience,
                MaxHP = mercenary.MaxHP,
                CurrentHP = mercenary.CurrentHP,
                Attack = mercenary.Attack,
                Defense = mercenary.Defense,
                MaxMagicPower = mercenary.MaxMagicPower,
                AttackSpeed = mercenary.AttackSpeed,
                StatusEffect = mercenary.StatusEffect,
                ContractActive = mercenary.IsContractActive,
                ContractNeedsRenewal = mercenary.ContractNeedsRenewal,
                WasDefeated = mercenary.CurrentHP <= 0,
                WasInParty =
                    partyManager != null &&
                    partyManager.Contains(mercenary)
            };
    }

    public void RecordMercenaryHired(MercenaryInstance mercenary)
    {
        if (mercenary != null && !string.IsNullOrEmpty(mercenary.InstanceId))
        {
            dailyHiredMercenaryIds.Add(mercenary.InstanceId);
        }
    }

    public void RememberDailyPartyMembers()
    {
        if (partyManager == null)
        {
            return;
        }
        foreach (MercenaryInstance mercenary in partyManager.Members)
        {
            if (mercenary != null &&
                dailyMercenarySnapshots.TryGetValue(
                    mercenary.InstanceId,
                    out DailyMercenarySnapshot snapshot))
            {
                snapshot.WasInParty = true;
            }
        }
    }

    private void ResetDailyInventoryTracking()
    {
        dailyAcquiredItems.Clear();
        dailyAcquiredEquipment.Clear();
        dailyInventoryAmounts.Clear();
        if (merchantInventory == null)
        {
            return;
        }

        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            if (stack?.Item == null)
            {
                continue;
            }
            string key = stack.Item.name;
            dailyInventoryAmounts[key] = stack.Amount;
        }
        foreach (EquipmentInstance equipment in
                 merchantInventory.EquipmentInstances)
        {
            RememberKnownEquipment(equipment);
        }
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary == null)
            {
                continue;
            }
            RememberKnownEquipment(mercenary.EquippedWeaponInstance);
            RememberKnownEquipment(mercenary.EquippedArmorInstance);
            RememberKnownEquipment(mercenary.EquippedAccessoryInstance);
        }
    }

    public void RecordDailyInventoryGains()
    {
        if (merchantInventory == null || dailySnapshotDay <= 0)
        {
            return;
        }

        Dictionary<string, int> currentAmounts =
            new Dictionary<string, int>();
        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            if (stack?.Item == null)
            {
                continue;
            }
            string key = stack.Item.name;
            currentAmounts[key] = stack.Amount;
            string displayName =
                JapaneseDisplayText.GetItemName(stack.Item);
            dailyInventoryAmounts.TryGetValue(key, out int previousAmount);
            int gained = stack.Amount - previousAmount;
            if (gained > 0)
            {
                dailyAcquiredItems.TryGetValue(
                    displayName,
                    out int acquiredAmount);
                dailyAcquiredItems[displayName] =
                    acquiredAmount + gained;
            }
        }
        dailyInventoryAmounts.Clear();
        foreach (KeyValuePair<string, int> entry in currentAmounts)
        {
            dailyInventoryAmounts[entry.Key] = entry.Value;
        }

        foreach (EquipmentInstance equipment in
                 merchantInventory.EquipmentInstances)
        {
            if (equipment == null ||
                string.IsNullOrEmpty(equipment.InstanceId) ||
                knownEquipmentInstanceIds.Contains(equipment.InstanceId))
            {
                continue;
            }
            knownEquipmentInstanceIds.Add(equipment.InstanceId);
            dailyAcquiredEquipment.Add(
                $"[{JapaneseDisplayText.GetEquipmentQuality(equipment.Quality)}] " +
                getEquipmentDisplayName(equipment));
        }
    }

    private void RememberKnownEquipment(EquipmentInstance equipment)
    {
        if (equipment != null &&
            !string.IsNullOrEmpty(equipment.InstanceId))
        {
            knownEquipmentInstanceIds.Add(equipment.InstanceId);
        }
    }

    private static bool AppendRankChange(
        StringBuilder result,
        string label,
        int before,
        int after)
    {
        if (before == after)
        {
            return false;
        }
        result.AppendLine(ColorByDirection(
            $"{label}  {before} → {after}", after > before));
        return true;
    }

    private static string BuildMercenaryChangeLine(
        MercenaryInstance mercenary,
        DailyMercenarySnapshot previous,
        bool inParty,
        bool isNewHire)
    {
        if (isNewHire)
        {
            return $"{EscapeRichText(mercenary.MercenaryName)}: " +
                Emphasis("新たに加入");
        }
        if (previous == null)
        {
            return string.Empty;
        }

        List<string> changes = new List<string>();
        AppendValueChange(changes, "HP", previous.CurrentHP, mercenary.CurrentHP,
            mercenary.CurrentHP > previous.CurrentHP ? "回復" : "被弾");
        AppendValueChange(changes, "Lv", previous.Level, mercenary.Level, null);
        AppendValueChange(changes, "EXP", previous.Experience,
            mercenary.CurrentExperience, null);
        AppendValueChange(changes, "最大HP", previous.MaxHP, mercenary.MaxHP, null);
        AppendValueChange(changes, "攻撃", previous.Attack, mercenary.Attack, null);
        AppendValueChange(changes, "防御", previous.Defense, mercenary.Defense, null);
        AppendValueChange(changes, "魔力", previous.MaxMagicPower,
            mercenary.MaxMagicPower, null);
        if (Math.Abs(mercenary.AttackSpeed - previous.AttackSpeed) > 0.001f)
        {
            changes.Add(ColorByDirection(
                $"速度 {previous.AttackSpeed:0.##}→{mercenary.AttackSpeed:0.##}",
                mercenary.AttackSpeed > previous.AttackSpeed));
        }
        if (previous.StatusEffect != mercenary.StatusEffect)
        {
            string status = mercenary.StatusEffect == BattleStatusEffect.None
                ? $"{JapaneseDisplayText.GetBattleStatus(previous.StatusEffect)}解除"
                : $"状態異常: {JapaneseDisplayText.GetBattleStatus(mercenary.StatusEffect)}";
            changes.Add(mercenary.StatusEffect == BattleStatusEffect.None
                ? Positive(status)
                : Negative(status));
        }
        bool isDefeated = mercenary.CurrentHP <= 0;
        if (previous.WasDefeated != isDefeated)
        {
            changes.Add(isDefeated ? Negative("戦闘不能") : Positive("戦闘不能から復帰"));
        }
        if (previous.ContractActive != mercenary.IsContractActive ||
            previous.ContractNeedsRenewal != mercenary.ContractNeedsRenewal)
        {
            changes.Add(mercenary.ContractNeedsRenewal
                ? Negative("契約更新待ち")
                : Positive("契約更新"));
        }
        if (previous.WasInParty != inParty)
        {
            changes.Add(inParty ? Positive("編成に加入") : Neutral("編成から離脱"));
        }
        return changes.Count == 0
            ? string.Empty
            : EscapeRichText(mercenary.MercenaryName) + ": " +
              string.Join("、", changes);
    }

    private static void AppendValueChange(
        List<string> changes,
        string label,
        int before,
        int after,
        string annotation)
    {
        if (before == after)
        {
            return;
        }
        string text = $"{label} {before}→{after}";
        if (!string.IsNullOrEmpty(annotation))
        {
            text += $"（{annotation}）";
        }
        changes.Add(ColorByDirection(text, after > before));
    }

    private static string ColorByDirection(string text, bool increased)
    {
        return increased ? Positive(text) : Negative(text);
    }

    private static string EscapeRichText(string value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("&", "＆").Replace("<", "＜")
                .Replace(">", "＞");
    }

    private void AppendDailyGoldLedger(
        StringBuilder result,
        int startAccountingDay,
        int endAccountingDayExclusive,
        int expectedNet)
    {
        Dictionary<GoldTransactionReason, int> totals =
            new Dictionary<GoldTransactionReason, int>();
        List<GoldTransaction> entries = new List<GoldTransaction>();
        foreach (GoldTransaction transaction in dailyGoldLedger)
        {
            if (transaction.AccountingDay >= startAccountingDay &&
                transaction.AccountingDay < endAccountingDayExclusive)
            {
                entries.Add(transaction);
            }
        }
        HashSet<string> cancelledTransactionIds =
            FindRefundedTransactionIds(entries);
        int net = 0;
        foreach (GoldTransaction transaction in entries)
        {
            if (cancelledTransactionIds.Contains(transaction.TransactionId))
            {
                continue;
            }
            totals.TryGetValue(transaction.Reason, out int total);
            totals[transaction.Reason] = total + transaction.SignedAmount;
            net += transaction.SignedAmount;
        }
        if (net != expectedNet)
        {
            int adjustment = expectedNet - net;
            totals.TryGetValue(GoldTransactionReason.Unclassified, out int total);
            totals[GoldTransactionReason.Unclassified] = total + adjustment;
            net += adjustment;
            UnityEngine.Debug.LogWarning(
                $"Daily gold ledger mismatch for days {startAccountingDay}" +
                $"-{endAccountingDayExclusive - 1}: ledger " +
                $"{net - adjustment}G, snapshot {expectedNet}G.");
        }

        result.AppendLine();
        result.AppendLine(Heading("【本日の金銭収支】"));
        AppendGoldLedgerSection(result, totals, true, "収入");
        AppendGoldLedgerSection(result, totals, false, "支出");
        result.AppendLine(ColorByDirection(
            $"差引  {FormatSignedValue(net)}G", net >= 0));
    }

    private static HashSet<string> FindRefundedTransactionIds(
        List<GoldTransaction> entries)
    {
        HashSet<string> cancelled = new HashSet<string>();
        foreach (GoldTransaction refund in entries)
        {
            if (refund.Reason != GoldTransactionReason.Refund ||
                refund.SignedAmount <= 0 ||
                string.IsNullOrEmpty(refund.RelatedTransactionId))
            {
                continue;
            }
            foreach (GoldTransaction payment in entries)
            {
                if (payment.TransactionId != refund.RelatedTransactionId ||
                    payment.SignedAmount >= 0 ||
                    -payment.SignedAmount != refund.SignedAmount)
                {
                    continue;
                }
                cancelled.Add(payment.TransactionId);
                cancelled.Add(refund.TransactionId);
                break;
            }
        }
        return cancelled;
    }

    private static void AppendGoldLedgerSection(
        StringBuilder result,
        Dictionary<GoldTransactionReason, int> totals,
        bool income,
        string title)
    {
        int sectionTotal = 0;
        foreach (KeyValuePair<GoldTransactionReason, int> entry in totals)
        {
            if ((income && entry.Value > 0) || (!income && entry.Value < 0))
            {
                sectionTotal += entry.Value;
            }
        }
        result.AppendLine(income
            ? Positive($"{title}  +{sectionTotal:N0}G")
            : Negative($"{title}  {sectionTotal:N0}G"));
        foreach (KeyValuePair<GoldTransactionReason, int> entry in totals)
        {
            if ((income && entry.Value <= 0) || (!income && entry.Value >= 0))
            {
                continue;
            }
            string amount = entry.Value > 0
                ? $"+{entry.Value:N0}G"
                : $"{entry.Value:N0}G";
            string line = $"  ・{GetGoldTransactionReasonName(entry.Key),-12} {amount}";
            result.AppendLine(income ? Positive(line) : Negative(line));
        }
    }

    private void RemoveGoldLedgerEntries(
        int startAccountingDay,
        int endAccountingDayExclusive)
    {
        dailyGoldLedger.RemoveAll(
            transaction => transaction.AccountingDay >= startAccountingDay &&
                transaction.AccountingDay < endAccountingDayExclusive);
    }

    private static string GetGoldTransactionReasonName(
        GoldTransactionReason reason)
    {
        switch (reason)
        {
            case GoldTransactionReason.ItemSale: return "商品売却";
            case GoldTransactionReason.QuestReward: return "依頼報酬";
            case GoldTransactionReason.BattleReward: return "戦闘報酬";
            case GoldTransactionReason.DungeonReward: return "ダンジョン報酬";
            case GoldTransactionReason.RemoteSale: return "遠隔売却";
            case GoldTransactionReason.MarketPurchase: return "仕入";
            case GoldTransactionReason.Blacksmith: return "鍛冶";
            case GoldTransactionReason.MercenaryHire: return "雇用";
            case GoldTransactionReason.ContractRenewal: return "契約更新";
            case GoldTransactionReason.ContractChange: return "契約変更";
            case GoldTransactionReason.Healing: return "治療費";
            case GoldTransactionReason.Training: return "修練";
            case GoldTransactionReason.DebtRepayment: return "借金返済";
            case GoldTransactionReason.StorageUpgrade: return "倉庫拡張";
            case GoldTransactionReason.StorageMaintenance: return "倉庫維持費";
            case GoldTransactionReason.ExplorationExpense: return "探索費";
            case GoldTransactionReason.Refund: return "返金";
            default: return "その他/未分類";
        }
    }

    private static string Color(string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }

    private static string Positive(string text) { return Color(text, PositiveColor); }
    private static string Negative(string text) { return Color(text, NegativeColor); }
    private static string Neutral(string text) { return Color(text, NeutralColor); }
    private static string Heading(string text) { return Color(text, HeadingColor); }
    private static string Emphasis(string text) { return $"<b><color={AccentColor}>{text}</color></b>"; }

    private static string FormatSignedValue(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static string GetTownName(int townIndex)
    {
        return townIndex >= 0 && townIndex < WorldMapService.TownNames.Length
            ? WorldMapService.TownNames[townIndex]
            : "不明な町";
    }
}
