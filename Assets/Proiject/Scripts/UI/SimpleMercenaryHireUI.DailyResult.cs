using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildDailyResultOverlay()
    {
        dailyResultOverlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.DailyResult,
                "Daily Result Overlay");
        dailyResultOverlay.anchorMin = Vector2.zero;
        dailyResultOverlay.anchorMax = Vector2.one;
        dailyResultOverlay.offsetMin = Vector2.zero;
        dailyResultOverlay.offsetMax = Vector2.zero;
        dailyResultOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.84f);

        RectTransform window =
            CreateUIObject("Daily Result Window", dailyResultOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 580f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "一日のリザルト",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(130f, -66f),
            new Vector2(-130f, -18f),
            ParchmentTextColor);

        RectTransform viewport =
            CreateUIObject("Daily Result Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 76f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        dailyResultContent =
            CreateUIObject("Daily Result Content", viewport);
        dailyResultContent.anchorMin = new Vector2(0f, 1f);
        dailyResultContent.anchorMax = new Vector2(1f, 1f);
        dailyResultContent.pivot = new Vector2(0.5f, 1f);
        dailyResultText = CreateText(
            dailyResultContent,
            string.Empty,
            17,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(16f, 16f),
            new Vector2(-16f, -16f),
            ParchmentTextColor);
        dailyResultText.rectTransform.anchorMin = Vector2.zero;
        dailyResultText.rectTransform.anchorMax = Vector2.one;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = dailyResultContent;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        Button closeButton =
            CreateActionButton(window, "確認", HideDailyResult);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax =
            new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 18f);

        dailyResultOverlay.gameObject.SetActive(false);
    }

    private void HideDailyResult()
    {
        dailyResultOverlay?.gameObject.SetActive(false);
    }

    private void HandleDayChanged(int currentDay)
    {
        if (TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            mercenaryGenerator.GenerateCandidates();
        }
        else
        {
            mercenaryGenerator.ClearCandidates();
        }
        RefreshPage(marketPage);
        RefreshPage(inventoryPage);
        RefreshPage(healPage);
        RefreshPage(companyPage);
        RefreshUI();
        string debtNotice = debtManager != null &&
                            (currentDay - 1) % DebtManager.DaysPerMonth == 0 &&
                            currentDay > 1
            ? debtManager.PaymentArrears > 0
                ? $" 月次返済後の滞納額は{debtManager.PaymentArrears:N0}Gです。"
                : $" 月次最低返済を完了しました。"
            : string.Empty;
        statusText.text =
            $"{currentDay}日目になりました。市場価格が更新されました。{debtNotice}";
        ShowDailyResult(currentDay);
    }

    private void ShowDailyResult(int currentDay)
    {
        if (dailyResultOverlay == null ||
            dailyResultText == null ||
            currentDay <= dailySnapshotDay)
        {
            CaptureDailySnapshot(currentDay);
            return;
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

        string resultText = result.ToString().TrimEnd();
        dailyResultText.text = resultText;
        int lineCount = resultText.Split('\n').Length;
        dailyResultContent.sizeDelta =
            new Vector2(0f, Mathf.Max(420f, 40f + lineCount * 34f));
        dailyResultOverlay.SetAsLastSibling();
        dailyResultOverlay.gameObject.SetActive(true);
        CaptureDailySnapshot(currentDay);
    }

    private void CaptureDailySnapshot(int currentDay)
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
        dailyMercenarySnapshots.Clear();

        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            CaptureMercenarySnapshot(mercenary);
        }
        ResetDailyInventoryTracking();
    }

    private void CaptureMercenarySnapshot(MercenaryInstance mercenary)
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

    private void RememberDailyPartyMembers()
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
        dailyInventoryNames.Clear();
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
            dailyInventoryNames[key] =
                JapaneseDisplayText.GetItemName(stack.Item);
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

    private void RecordDailyInventoryGains()
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
            dailyInventoryNames[key] = displayName;
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
                GetEquipmentDisplayName(equipment));
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

}
