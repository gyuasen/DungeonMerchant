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
    private readonly List<string> trainingCompletionLines = new List<string>();
    private readonly HashSet<string> trainingCompletionKeys =
        new HashSet<string>();

    private sealed class DailyMercenarySnapshot
    {
        public string Name;
        public int Level;
        public int Experience;
        public int MaxHP;
        public int Attack;
        public int Defense;
        public int MaxMagicPower;
        public float AttackSpeed;
        public bool ContractActive;
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
        result.AppendLine(
            $"{dailySnapshotDay}日目の終了 → {currentDay}日目");
        result.AppendLine();
        result.AppendLine("【商人】");

        bool hasMerchantChange = false;
        int goldChange = merchantData.Gold - dailySnapshotGold;
        if (goldChange != 0)
        {
            result.AppendLine(
                $"所持金  {dailySnapshotGold}G → {merchantData.Gold}G " +
                $"({FormatSignedValue(goldChange)}G)");
            hasMerchantChange = true;
        }
        if (merchantData.MerchantLevel > dailySnapshotMerchantLevel)
        {
            result.AppendLine(
                $"★ レベルアップ  Lv{dailySnapshotMerchantLevel} → " +
                $"Lv{merchantData.MerchantLevel}");
            hasMerchantChange = true;
        }
        if (merchantData.MerchantLevel == dailySnapshotMerchantLevel &&
            merchantData.MerchantExperience >
            dailySnapshotMerchantExperience)
        {
            result.AppendLine(
                $"獲得G進行  +{merchantData.MerchantExperience - dailySnapshotMerchantExperience} " +
                $"({merchantData.MerchantExperience}/" +
                $"{merchantData.ExperienceToNextLevel})");
            hasMerchantChange = true;
        }
        else if (merchantData.MerchantLevel > dailySnapshotMerchantLevel)
        {
            result.AppendLine(
                $"現在の獲得G進行  {merchantData.MerchantExperience}/" +
                $"{merchantData.ExperienceToNextLevel}");
        }
        if (merchantData.MerchantSkillPoints != dailySnapshotSkillPoints)
        {
            result.AppendLine(
                $"技能ポイント  {dailySnapshotSkillPoints} → " +
                $"{merchantData.MerchantSkillPoints}");
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
            result.AppendLine("大きな変化はありません。");
        }

        int storageUsed = merchantInventory != null
            ? merchantInventory.GetUsedStorageSlots()
            : 0;
        int storageCapacity = progressionManager != null
            ? progressionManager.StorageCapacity
            : 0;
        int storageRemaining = Math.Max(0, storageCapacity - storageUsed);
        result.AppendLine();
        result.AppendLine("【倉庫】");
        if (dailySnapshotStorageUsed != storageUsed ||
            dailySnapshotStorageCapacity != storageCapacity)
        {
            result.AppendLine(
                $"使用量 {dailySnapshotStorageUsed}/{dailySnapshotStorageCapacity} → " +
                $"{storageUsed}/{storageCapacity}");
        }
        else
        {
            result.AppendLine($"使用量 {storageUsed}/{storageCapacity}");
        }
        result.AppendLine($"空き容量 {storageRemaining}");
        if (storageCapacity > 0 && storageRemaining == 0)
        {
            result.AppendLine("！倉庫が満杯です。売却または倉庫拡張を行ってください。");
        }

        result.AppendLine();
        result.AppendLine("【入手アイテム】");
        if (dailyAcquiredItems.Count == 0 &&
            dailyAcquiredEquipment.Count == 0)
        {
            result.AppendLine("入手したアイテムはありません。");
        }

        else
        {
            foreach (KeyValuePair<string, int> entry in dailyAcquiredItems)
            {
                result.AppendLine($"・{entry.Key} ×{entry.Value}");
            }
            foreach (string equipmentName in dailyAcquiredEquipment)
            {
                result.AppendLine($"・{equipmentName}");
            }
        }

        result.AppendLine();
        result.AppendLine("【輸送】");
        if (dailyTransportEvents.Count == 0)
        {
            result.AppendLine("輸送に関する報告はありません。");
        }
        else
        {
            foreach (string transportEvent in dailyTransportEvents)
            {
                result.AppendLine(transportEvent);
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

            StringBuilder growth = new StringBuilder();
            bool leveledUp =
                previous != null && mercenary.Level > previous.Level;
            if (leveledUp)
            {
                growth.Append(
                    $" / Lv{previous.Level}→{mercenary.Level}");
            }
            else if (previous != null &&
                     mercenary.CurrentExperience > previous.Experience)
            {
                growth.Append(
                    $" / EXP +" +
                    $"{mercenary.CurrentExperience - previous.Experience}");
            }
            if (previous != null)
            {
                AppendStatChange(
                    growth, "HP", mercenary.MaxHP - previous.MaxHP);
                AppendStatChange(
                    growth, "攻撃", mercenary.Attack - previous.Attack);
                AppendStatChange(
                    growth, "防御", mercenary.Defense - previous.Defense);
                AppendStatChange(
                    growth,
                    "魔力",
                    mercenary.MaxMagicPower - previous.MaxMagicPower);
                float speedChange =
                    mercenary.AttackSpeed - previous.AttackSpeed;
                if (speedChange > 0.001f)
                {
                    growth.Append($" / 速度 +{speedChange:0.##}");
                }
            }

            string experienceText = mercenary.IsAtLevelCap
                ? "MAX"
                : $"{mercenary.CurrentExperience}/" +
                  $"{mercenary.ExperienceToNextLevel}";
            string contractText = mercenary.ContractNeedsRenewal
                ? "更新待ち"
                : mercenary.ContractEndDay > 0
                    ? $"{mercenary.ContractEndDay}日目まで"
                    : "無期限";
            bool inParty = partyManager.Contains(mercenary);
            mercenaryLines.Add(
                $"{(leveledUp ? "★ " : "・")}{mercenary.MercenaryName} " +
                $"[{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}] " +
                $"Lv{mercenary.Level}{growth}\n" +
                $"  HP {mercenary.CurrentHP}/{mercenary.MaxHP} / " +
                $"EXP {experienceText} / 攻撃 {mercenary.Attack} / " +
                $"防御 {mercenary.Defense} / 魔力 {mercenary.MaxMagicPower}\n" +
                $"  速度 {mercenary.AttackSpeed:0.##} / " +
                $"会心 {mercenary.CriticalRate * 100f:0.#}% / " +
                $"回避 {mercenary.EvasionRate * 100f:0.#}% / " +
                $"{JapaneseDisplayText.GetContractType(mercenary.ContractType)} " +
                $"({contractText}) / {(inParty ? "編成中" : "待機")}");

            if (previous != null &&
                previous.ContractActive &&
                !mercenary.IsContractActive)
            {
                bool removedFromParty =
                    previous.WasInParty && !partyManager.Contains(mercenary);
                contractLines.Add(
                    $"! {previous.Name}: " +
                    $"{JapaneseDisplayText.GetContractType(mercenary.ContractType)}が終了" +
                    (removedFromParty ? "（編成から外れました）" : string.Empty));
            }
        }

        result.AppendLine();
        result.AppendLine("【傭兵の成長・現在状況】");
        foreach (string trainingLine in trainingCompletionLines)
        {
            result.AppendLine(trainingLine);
        }
        if (mercenaryLines.Count == 0)
        {
            result.AppendLine("雇用中の傭兵はいません。");
        }
        else
        {
            foreach (string line in mercenaryLines)
            {
                result.AppendLine(line);
            }
        }

        result.AppendLine();
        result.AppendLine("【契約】");
        if (contractLines.Count == 0)
        {
            result.AppendLine("契約終了による編成変更はありません。");
        }
        else
        {
            foreach (string line in contractLines)
            {
                result.AppendLine(line);
            }
            result.AppendLine("商会画面から契約を更新できます。");
        }

        return result.ToString().TrimEnd();
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
        trainingCompletionLines.Clear();
        trainingCompletionKeys.Clear();
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

        string name = mercenary != null ? mercenary.MercenaryName : "傭兵";
        string line = $"★ 修練完了: {name}がLv{reservation.TargetLevel}になった";
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
                Attack = mercenary.Attack,
                Defense = mercenary.Defense,
                MaxMagicPower = mercenary.MaxMagicPower,
                AttackSpeed = mercenary.AttackSpeed,
                ContractActive = mercenary.IsContractActive,
                WasInParty =
                    partyManager != null &&
                    partyManager.Contains(mercenary)
            };
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
        result.AppendLine($"{label}  {before} → {after}");
        return true;
    }

    private static void AppendStatChange(
        StringBuilder result,
        string label,
        int difference)
    {
        if (difference <= 0)
        {
            return;
        }
        result.Append($" / {label} +{difference}");
    }

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
