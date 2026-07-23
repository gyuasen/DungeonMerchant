using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the character-detail / equipment-detail selection state, the
/// detail/comparison/collection text building and the equipment business
/// actions (equip/unequip/enhance/lock/sell/consumable use). Extracted
/// from SimpleMercenaryHireUI (step 3.8). Overlay construction, page
/// routing and delegate wiring stay in
/// SimpleMercenaryHireUI.CharacterEquipment.cs; only the feature state,
/// content building and business actions live here.
/// </summary>
public sealed class CharacterEquipmentController
{
    private readonly MerchantData merchantData;
    private readonly MerchantInventory merchantInventory;
    private readonly MercenaryHireManager hireManager;
    private readonly BattleManager battleManager;
    private readonly EconomyController economyController;
    private readonly IEquipmentDetailView equipmentDetailView;
    private readonly Action<string> setStatus;
    private readonly Action<string, string> setCharacterDetailContent;
    private readonly Action<MercenaryInstance> showCharacterDetails;
    private readonly Action refreshCompanyPage;
    private readonly Action refreshPartyPage;
    private readonly Action refreshInventoryPage;
    private readonly Action refreshUI;
    private readonly Action saveEquipmentChanges;
    private readonly Action saveGame;

    public MercenaryInstance SelectedDetailMercenary { get; set; }
    public EquipmentInstance SelectedEquipmentDetail { get; set; }

    public CharacterEquipmentController(
        MerchantData merchantData,
        MerchantInventory merchantInventory,
        MercenaryHireManager hireManager,
        BattleManager battleManager,
        EconomyController economyController,
        IEquipmentDetailView equipmentDetailView,
        Action<string> setStatus,
        Action<string, string> setCharacterDetailContent,
        Action<MercenaryInstance> showCharacterDetails,
        Action refreshCompanyPage,
        Action refreshPartyPage,
        Action refreshInventoryPage,
        Action refreshUI,
        Action saveEquipmentChanges,
        Action saveGame)
    {
        this.merchantData = merchantData;
        this.merchantInventory = merchantInventory;
        this.hireManager = hireManager;
        this.battleManager = battleManager;
        this.economyController = economyController;
        this.equipmentDetailView = equipmentDetailView;
        this.setStatus = setStatus;
        this.setCharacterDetailContent = setCharacterDetailContent;
        this.showCharacterDetails = showCharacterDetails;
        this.refreshCompanyPage = refreshCompanyPage;
        this.refreshPartyPage = refreshPartyPage;
        this.refreshInventoryPage = refreshInventoryPage;
        this.refreshUI = refreshUI;
        this.saveEquipmentChanges = saveEquipmentChanges;
        this.saveGame = saveGame;
    }

    public void RefreshCharacterDetailText()
    {
        if (SelectedDetailMercenary == null)
        {
            return;
        }

        MercenaryInstance mercenary = SelectedDetailMercenary;
        string condition = mercenary.IsIncapacitated ? "戦闘不能" : "行動可能";
        string experienceText = mercenary.IsAtLevelCap
            ? "上限到達"
            : $"{mercenary.CurrentExperience} / " +
              $"{mercenary.ExperienceToNextLevel}";

        setCharacterDetailContent(
            mercenary.MercenaryName,
            $"職業: {JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}\n" +
            $"契約: {JapaneseDisplayText.GetContractType(mercenary.ContractType)}\n" +
            $"契約期限: {(mercenary.ContractEndDay > 0 ? mercenary.ContractEndDay + "日" : "無期限")}" +
            $"{(mercenary.ContractNeedsRenewal ? "（更新待ち）" : string.Empty)}\n" +
            $"状態: {condition}\n\n" +
            $"レベル: {mercenary.Level} / {mercenary.LevelCap}\n" +
            $"経験値: {experienceText}\n\n" +
            $"HP: {mercenary.CurrentHP} / {mercenary.MaxHP}\n" +
            $"攻撃: {mercenary.Attack}\n" +
            $"防御: {mercenary.Defense}\n" +
            $"行動速度: {mercenary.AttackSpeed:0.00}\n" +
            $"クリティカル率: {mercenary.CriticalRate * 100f:0}%\n" +
            $"回避率: {mercenary.EvasionRate * 100f:0}%\n" +
            $"最大魔力: {mercenary.MaxMagicPower}\n" +
            $"状態異常: {JapaneseDisplayText.GetBattleStatus(mercenary.StatusEffect)}\n" +
            $"武器: {GetEquippedEquipmentName(mercenary, EquipmentSlot.Weapon)}\n" +
            $"防具: {GetEquippedEquipmentName(mercenary, EquipmentSlot.Armor)}\n" +
            $"装飾品: {GetEquippedEquipmentName(mercenary, EquipmentSlot.Accessory)}\n" +
            $"セット: {BuildActiveSetSummary(mercenary)}\n" +
            $"スキルボード: {mercenary.SkillBoardName}\n" +
            $"獲得スキル: {GetMercenarySkillInfos(mercenary).Count}\n" +
            $"雇用費: {mercenary.HireCost} G");
    }

    public void EquipSelectedEquipment(ItemDataSO equipment)
    {
        if (SelectedDetailMercenary == null ||
            equipment == null ||
            !merchantInventory.HasItem(equipment))
        {
            return;
        }

        EquipmentSlot slot = equipment.equipmentSlot;
        ItemDataSO previousItem =
            SelectedDetailMercenary.GetEquippedItem(slot);
        EquipmentInstance previousInstance =
            SelectedDetailMercenary.GetEquippedInstance(slot);
        if (!SelectedDetailMercenary.EquipEquipment(equipment) ||
            !merchantInventory.TryRemoveItem(equipment))
        {
            if (previousInstance != null)
            {
                SelectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousInstance);
            }
            else
            {
                SelectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousItem);
            }
            return;
        }

        if (previousInstance != null)
        {
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else if (previousItem != null)
        {
            merchantInventory.AddItem(previousItem);
        }

        setStatus(
            $"{SelectedDetailMercenary.MercenaryName}に" +
            $"{JapaneseDisplayText.GetItemName(equipment)}を装備しました。");
        showCharacterDetails(SelectedDetailMercenary);
        refreshCompanyPage();
        refreshPartyPage();
        saveEquipmentChanges();
    }

    public void LoadConsumable(int slotIndex, ItemDataSO item)
    {
        if (SelectedDetailMercenary == null || item == null ||
            item.itemType != ItemType.Consumable ||
            !merchantInventory.HasItem(item) ||
            !SelectedDetailMercenary.TryLoadConsumable(slotIndex, item, 1))
        {
            return;
        }

        if (!merchantInventory.TryRemoveItem(item))
        {
            SelectedDetailMercenary.RemoveConsumableSlot(slotIndex, out int amount);
            if (amount > 1)
            {
                SelectedDetailMercenary.TryLoadConsumable(slotIndex, item, amount - 1);
            }
            return;
        }

        setStatus($"{SelectedDetailMercenary.MercenaryName}に{item.itemName}を装填しました。");
        showCharacterDetails(SelectedDetailMercenary);
        refreshInventoryPage();
        saveEquipmentChanges();
    }

    public void UnloadConsumable(int slotIndex)
    {
        if (SelectedDetailMercenary == null)
        {
            return;
        }

        ItemDataSO item = SelectedDetailMercenary.RemoveConsumableSlot(
            slotIndex,
            out int amount);
        if (item == null || amount <= 0)
        {
            return;
        }

        if (!merchantInventory.CanAddItem(item, amount))
        {
            setStatus("倉庫が満杯のため、消耗品を戻せませんでした。");
            SelectedDetailMercenary.TryLoadConsumable(slotIndex, item, amount);
            return;
        }

        if (!merchantInventory.TryAddItem(item, amount))
        {
            SelectedDetailMercenary.TryLoadConsumable(slotIndex, item, amount);
            setStatus("倉庫が満杯のため、消耗品を戻せませんでした。");
            return;
        }
        setStatus($"{item.itemName} x{amount}を倉庫へ戻しました。");
        showCharacterDetails(SelectedDetailMercenary);
        refreshInventoryPage();
        saveEquipmentChanges();
    }

    public void EquipSelectedEquipment(EquipmentInstance equipment)
    {
        if (SelectedDetailMercenary == null ||
            equipment?.BaseItem == null)
        {
            return;
        }

        EquipmentSlot slot = equipment.BaseItem.equipmentSlot;
        ItemDataSO previousItem =
            SelectedDetailMercenary.GetEquippedItem(slot);
        EquipmentInstance previousInstance =
            SelectedDetailMercenary.GetEquippedInstance(slot);
        if (!SelectedDetailMercenary.EquipEquipment(equipment) ||
            !merchantInventory.TryRemoveEquipmentInstance(equipment))
        {
            if (previousInstance != null)
            {
                SelectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousInstance);
            }
            else
            {
                SelectedDetailMercenary.RestoreEquippedEquipment(
                    slot,
                    previousItem);
            }
            return;
        }

        if (previousInstance != null)
        {
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else if (previousItem != null)
        {
            merchantInventory.AddItem(previousItem);
        }

        setStatus(
            $"{SelectedDetailMercenary.MercenaryName}に" +
            $"[{JapaneseDisplayText.GetEquipmentQuality(equipment.Quality)}] " +
            $"{JapaneseDisplayText.GetItemName(equipment.BaseItem)}を装備しました。");
        showCharacterDetails(SelectedDetailMercenary);
        refreshCompanyPage();
        refreshPartyPage();
        saveEquipmentChanges();
    }

    public void UnequipSelectedEquipment(EquipmentSlot slot)
    {
        if (SelectedDetailMercenary == null ||
            SelectedDetailMercenary.GetEquippedItem(slot) == null)
        {
            return;
        }

        EquipmentInstance previousInstance =
            SelectedDetailMercenary.GetEquippedInstance(slot);
        if (previousInstance != null)
        {
            SelectedDetailMercenary.UnequipEquipmentInstance(slot);
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else
        {
            ItemDataSO previousItem =
                SelectedDetailMercenary.UnequipEquipment(slot);
            if (previousItem != null &&
                merchantInventory.TryAddItem(previousItem))
            {
                setStatus(
                    $"{SelectedDetailMercenary.MercenaryName}の" +
                    $"{JapaneseDisplayText.GetEquipmentSlot(slot)}を外しました。");
            }
            else
            {
                SelectedDetailMercenary.RestoreEquippedEquipment(slot, previousItem);
                setStatus("倉庫が満杯のため、装備を外せませんでした。");
                return;
            }
        }
        setStatus(
            $"{SelectedDetailMercenary.MercenaryName}の" +
            $"{JapaneseDisplayText.GetEquipmentSlot(slot)}を解除しました。");
        showCharacterDetails(SelectedDetailMercenary);
        refreshCompanyPage();
        refreshPartyPage();
        saveEquipmentChanges();
    }

    public void ShowEquipmentDetails(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null || !equipmentDetailView.HasOverlay)
        {
            return;
        }

        SelectedEquipmentDetail = equipment;
        ItemDataSO item = equipment.BaseItem;
        string quality = JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        equipmentDetailView.SetTitle(
            $"[{quality}] {GetEquipmentDisplayName(equipment)}",
            GetEquipmentQualityColor(equipment.Quality));

        List<string> modifierLines = new List<string>();
        foreach (EquipmentModifier modifier in equipment.Modifiers)
        {
            if (modifier != null)
            {
                modifierLines.Add(
                    $"{JapaneseDisplayText.GetEquipmentModifier(modifier.type)} " +
                    $"{FormatSigned(modifier.value)}");
            }
        }

        string modifiers = modifierLines.Count > 0
            ? string.Join("\n", modifierLines)
            : "追加効果なし";
        string specialEffects = EquipmentEffectTextFormatter.FormatList(
            equipment.EquipmentEffects);
        string setText = BuildEquipmentSetDetail(
            item.equipmentSet,
            SelectedDetailMercenary);
        string descriptionText = BuildEquipmentDescriptionText(item);
        string target = item.allClassesCanEquip
            ? "全職業"
            : JapaneseDisplayText.GetMercenaryClass(item.requiredClass);
        ItemDataSO enhancementMaterial =
            merchantInventory.GetEnhancementMaterial(equipment);
        string enhancementMaterialName = enhancementMaterial != null
            ? JapaneseDisplayText.GetItemName(enhancementMaterial)
            : "対応する強化鉱石";

        equipmentDetailView.SetDetailText(
            $"種類: {JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}\n" +
            $"装備対象: {target}  {EquipmentRankPresentation.GetRichText(item)}\n" +
            $"品質: {quality}  強化: +{equipment.EnhancementLevel} / +10\n\n" +
            $"最終性能\n" +
            $"HP {FormatSigned(equipment.BonusMaxHP)}  " +
            $"攻撃 {FormatSigned(equipment.BonusAttack)}\n" +
            $"防御 {FormatSigned(equipment.BonusDefense)}  " +
            $"攻撃速度 {FormatSigned(equipment.BonusAttackSpeed)}\n\n" +
            $"{descriptionText}" +
            $"追加効果\n{modifiers}\n\n特殊効果\n{specialEffects}\n\n{setText}\n\n" +
            $"次回強化: 成功率 " +
            $"{equipment.GetEnhancementSuccessRate() * 100f:0}%  " +
            $"{enhancementMaterialName} " +
            $"{equipment.GetEnhancementMaterialAmount()}個");

        bool canEnhance = equipment.EnhancementLevel < 10;
        equipmentDetailView.SetEnhanceButton(
            canEnhance &&
            merchantData.CanPay(equipment.GetEnhancementCost()) &&
            enhancementMaterial != null &&
            merchantInventory.HasItem(
                enhancementMaterial,
                equipment.GetEnhancementMaterialAmount()),
            canEnhance
                ? $"強化 {equipment.GetEnhancementCost()}G"
                : "強化完了");
        equipmentDetailView.SetSellButton(
            IsEquipmentInInventory(equipment) && !equipment.IsLocked,
            $"売却 {merchantInventory.GetSellPrice(equipment)}G");
        equipmentDetailView.SetLockButtonLabel(
            equipment.IsLocked ? "ロック解除" : "ロック");

        equipmentDetailView.ShowOverlay();
    }

    public void EnhanceSelectedEquipment()
    {
        EquipmentInstance equipment = SelectedEquipmentDetail;
        if (equipment == null)
        {
            return;
        }

        EquipmentEnhancementResult result =
            merchantInventory.TryEnhanceEquipment(equipment);
        switch (result)
        {
            case EquipmentEnhancementResult.Succeeded:
                setStatus(
                    $"{GetEquipmentDisplayName(equipment)}の強化に成功しました。");
                break;
            case EquipmentEnhancementResult.Failed:
                setStatus(
                    "強化に失敗しました。装備と強化値は維持されます。");
                break;
            case EquipmentEnhancementResult.NotEnoughMaterial:
                setStatus("強化鉱石が不足しています。");
                break;
            case EquipmentEnhancementResult.NotEnoughGold:
                setStatus("ゴールドが不足しています。");
                break;
            default:
                setStatus("装備を強化できませんでした。");
                break;
        }
        refreshInventoryPage();
        if (SelectedDetailMercenary != null)
        {
            showCharacterDetails(SelectedDetailMercenary);
        }
        ShowEquipmentDetails(equipment);
        refreshUI();
        saveEquipmentChanges();
    }

    public void ToggleSelectedEquipmentLock()
    {
        if (SelectedEquipmentDetail == null)
        {
            return;
        }

        merchantInventory.ToggleEquipmentLock(SelectedEquipmentDetail);
        setStatus(SelectedEquipmentDetail.IsLocked
            ? "装備をロックしました。"
            : "装備のロックを解除しました。");
        refreshInventoryPage();
        ShowEquipmentDetails(SelectedEquipmentDetail);
        saveEquipmentChanges();
    }

    public void SellSelectedEquipment()
    {
        EquipmentInstance equipment = SelectedEquipmentDetail;
        if (equipment == null || !IsEquipmentInInventory(equipment))
        {
            return;
        }

        equipmentDetailView.HideOverlay();
        economyController.SellEquipment(equipment);
    }

    public void UseConsumable(ItemDataSO item)
    {
        if (item == null ||
            item.itemType != ItemType.Consumable ||
            battleManager.IsBattling)
        {
            setStatus("現在はこの消費アイテムを使用できません。");
            return;
        }

        BattleStatusEffect targetStatus;
        switch (item.consumableEffect)
        {
            case ConsumableEffectType.CurePoison:
                targetStatus = BattleStatusEffect.Poison;
                break;
            case ConsumableEffectType.CureParalysis:
                targetStatus = BattleStatusEffect.Paralysis;
                break;
            case ConsumableEffectType.CureAllStatus:
                targetStatus = BattleStatusEffect.None;
                break;
            default:
                setStatus("この消費アイテムには使用効果がありません。");
                return;
        }

        MercenaryInstance target = null;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null &&
                mercenary.HasStatusEffect &&
                (targetStatus == BattleStatusEffect.None ||
                 mercenary.StatusEffect == targetStatus))
            {
                target = mercenary;
                break;
            }
        }

        if (target == null)
        {
            setStatus("治療対象となる傭兵がいません。");
            return;
        }

        if (!merchantInventory.TryRemoveItem(item) ||
            !target.CureStatusEffect(targetStatus))
        {
            setStatus("消費アイテムを使用できませんでした。");
            return;
        }

        setStatus(
            $"{JapaneseDisplayText.GetItemName(item)}を使用し、" +
            $"{target.MercenaryName}の状態異常を治療しました。");
        refreshInventoryPage();
        refreshCompanyPage();
        RefreshCharacterDetailText();
        saveGame();
    }

    public string BuildEquipmentCollectionText(out int lineCount)
    {
        List<ItemDataSO> equipmentItems = FindAllEquipmentAssets();
        equipmentItems.Sort((left, right) =>
            string.Compare(
                JapaneseDisplayText.GetItemName(left),
                JapaneseDisplayText.GetItemName(right),
                System.StringComparison.Ordinal));

        List<string> lines = new List<string>();
        int discoveredCount = 0;
        foreach (ItemDataSO item in equipmentItems)
        {
            bool discovered = merchantInventory.HasDiscoveredEquipment(item);
            if (discovered)
            {
                discoveredCount++;
            }

            string name = discovered
                ? JapaneseDisplayText.GetItemName(item)
                : "？？？？？？";
            string set = BuildEquipmentSetMembershipText(item);
            string source = item.acquisitionType == ItemAcquisitionType.Dungeon
                ? "ダンジョン限定"
                : item.acquisitionType == ItemAcquisitionType.Blacksmith
                    ? "鍛冶屋"
                    : "市場";
            lines.Add(
                $"{(discovered ? "●" : "○")} [{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}] " +
                $"{name}{set} / {source}");
        }

        lineCount = lines.Count;
        return $"収集率 {discoveredCount}/{equipmentItems.Count}\n\n" +
               string.Join("\n", lines);
    }

    public static string GetEquipmentDisplayName(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null)
        {
            return "不明な装備";
        }

        string enhancement = equipment.EnhancementLevel > 0
            ? $" +{equipment.EnhancementLevel}"
            : string.Empty;
        return JapaneseDisplayText.GetItemName(equipment.BaseItem) + enhancement;
    }

    public static Color GetEquipmentQualityColor(EquipmentQuality quality)
    {
        switch (quality)
        {
            case EquipmentQuality.Poor: return new Color(0.62f, 0.62f, 0.62f);
            case EquipmentQuality.Fine: return new Color(0.38f, 0.82f, 0.48f);
            case EquipmentQuality.Rare: return new Color(0.35f, 0.62f, 1f);
            case EquipmentQuality.Legendary: return new Color(1f, 0.68f, 0.18f);
            default: return Color.white;
        }
    }

    public static string BuildEquipmentInstanceComparisonText(
        EquipmentInstance candidate,
        EquipmentInstance equippedInstance,
        ItemDataSO equippedItem)
    {
        int currentHP = equippedInstance != null
            ? equippedInstance.BonusMaxHP
            : equippedItem != null ? equippedItem.bonusMaxHP : 0;
        int currentAttack = equippedInstance != null
            ? equippedInstance.BonusAttack
            : equippedItem != null ? equippedItem.bonusAttack : 0;
        int currentDefense = equippedInstance != null
            ? equippedInstance.BonusDefense
            : equippedItem != null ? equippedItem.bonusDefense : 0;
        float currentSpeed = equippedInstance != null
            ? equippedInstance.BonusAttackSpeed
            : equippedItem != null ? equippedItem.bonusAttackSpeed : 0f;

        return $"HP {FormatSigned(candidate.BonusMaxHP)} " +
               $"{FormatComparison(candidate.BonusMaxHP - currentHP)}  " +
               $"攻撃 {FormatSigned(candidate.BonusAttack)} " +
               $"{FormatComparison(candidate.BonusAttack - currentAttack)}\n" +
               $"防御 {FormatSigned(candidate.BonusDefense)} " +
               $"{FormatComparison(candidate.BonusDefense - currentDefense)}  " +
               $"速度 {FormatSigned(candidate.BonusAttackSpeed)} " +
               $"{FormatComparison(candidate.BonusAttackSpeed - currentSpeed)}\n" +
               $"特殊効果: {EquipmentEffectTextFormatter.FormatList(candidate.EquipmentEffects)}" +
               BuildEquipmentSetMembershipText(candidate.BaseItem);
    }

    public static string BuildEquipmentBonusText(ItemDataSO item)
    {
        return $"HP {FormatSigned(item.bonusMaxHP)}  " +
               $"攻撃 {FormatSigned(item.bonusAttack)}\n" +
               $"防御 {FormatSigned(item.bonusDefense)}  " +
               $"速度 {FormatSigned(item.bonusAttackSpeed)}" +
               BuildEquipmentSetMembershipText(item);
    }

    public static string BuildEquipmentComparisonText(
        ItemDataSO candidate,
        ItemDataSO equipped,
        EquipmentInstance equippedInstance)
    {
        int currentHP = equippedInstance != null
            ? equippedInstance.BonusMaxHP
            : equipped != null ? equipped.bonusMaxHP : 0;
        int currentAttack = equippedInstance != null
            ? equippedInstance.BonusAttack
            : equipped != null ? equipped.bonusAttack : 0;
        int currentDefense = equippedInstance != null
            ? equippedInstance.BonusDefense
            : equipped != null ? equipped.bonusDefense : 0;
        float currentSpeed = equippedInstance != null
            ? equippedInstance.BonusAttackSpeed
            : equipped != null ? equipped.bonusAttackSpeed : 0f;

        return $"HP {FormatSigned(candidate.bonusMaxHP)} " +
               $"{FormatComparison(candidate.bonusMaxHP - currentHP)}  " +
               $"攻撃 {FormatSigned(candidate.bonusAttack)} " +
               $"{FormatComparison(candidate.bonusAttack - currentAttack)}\n" +
               $"防御 {FormatSigned(candidate.bonusDefense)} " +
               $"{FormatComparison(candidate.bonusDefense - currentDefense)}  " +
               $"速度 {FormatSigned(candidate.bonusAttackSpeed)} " +
               $"{FormatComparison(candidate.bonusAttackSpeed - currentSpeed)}\n" +
               $"特殊効果: {EquipmentEffectTextFormatter.FormatList(candidate.equipmentEffects)}" +
               BuildEquipmentSetMembershipText(candidate);
    }

    public static List<MercenarySkillInfo> GetMercenarySkillInfos(
        MercenaryInstance mercenary)
    {
        List<MercenarySkillInfo> skills = new List<MercenarySkillInfo>();
        switch (MercenaryClassProgression.GetBaseClass(
                    mercenary.MercenaryClass))
        {
            case MercenaryClass.Warrior:
                skills.Add(new MercenarySkillInfo
                {
                    Name = "挑発",
                    ShortDescription = "戦闘スキル / 魔力35",
                    DetailDescription =
                        "敵の攻撃を自分に引きつけます。ダメージを与えるスキルではありませんが、味方を守りたい場面で有効です。"
                });
                break;
            case MercenaryClass.Archer:
                skills.Add(new MercenarySkillInfo
                {
                    Name = "連射",
                    ShortDescription = "戦闘スキル / 魔力45",
                    DetailDescription =
                        "攻撃力を少し下げた射撃を2回行います。通常攻撃より有効な対象がいる場合に自動発動します。"
                });
                break;
            case MercenaryClass.Mage:
                skills.Add(new MercenarySkillInfo
                {
                    Name = "火球",
                    ShortDescription = "戦闘スキル / 魔力50",
                    DetailDescription =
                        "敵1体に高威力の魔法攻撃を行います。通常攻撃では倒しきれない相手への決定打になります。"
                });
                break;
        }

        if (skills.Count == 0)
        {
            MercenarySkillDefinition skill =
                MercenaryClassProgression.GetPrimarySkill(
                    mercenary.MercenaryClass);
            skills.Add(new MercenarySkillInfo
            {
                Name = skill.Name,
                ShortDescription =
                    $"戦闘スキル / 魔力{skill.MagicCost}",
                DetailDescription = skill.Description
            });
        }

        List<MercenarySkillDefinition> progressionSkills =
            MercenaryClassProgression.GetSkillProgression(
                mercenary.MercenaryClass);
        foreach (MercenarySkillDefinition definition in progressionSkills)
        {
            if (definition.Id == MercenaryClassProgression.GetPrimarySkill(
                    mercenary.MercenaryClass).Id)
            {
                continue;
            }
            bool unlocked = mercenary.Level >= definition.UnlockLevel;
            skills.Add(new MercenarySkillInfo
            {
                Name = definition.Name,
                ShortDescription = unlocked
                    ? (definition.IsPassive
                        ? $"パッシブ / Lv{definition.UnlockLevel}"
                        : $"戦闘スキル / MP{definition.MagicCost}")
                    : $"未習得 / Lv{definition.UnlockLevel}",
                DetailDescription = definition.Description,
                Unlocked = unlocked
            });
        }

        if (mercenary.Level >= 2)
        {
            switch (MercenaryClassProgression.GetBaseClass(
                        mercenary.MercenaryClass))
            {
                case MercenaryClass.Warrior:
                    skills.Add(new MercenarySkillInfo
                    {
                        Name = "基礎体力",
                        ShortDescription = "パッシブ / Lv2",
                        DetailDescription =
                            "最大HPが10、防御が3上昇します。前衛として長く戦えるようになります。"
                    });
                    break;
                case MercenaryClass.Archer:
                    skills.Add(new MercenarySkillInfo
                    {
                        Name = "速射訓練",
                        ShortDescription = "パッシブ / Lv2",
                        DetailDescription =
                            "攻撃速度が0.05上昇します。行動順が早くなり、魔力の回復機会も増えやすくなります。"
                    });
                    break;
                case MercenaryClass.Mage:
                    skills.Add(new MercenarySkillInfo
                    {
                        Name = "魔力集中",
                        ShortDescription = "パッシブ / Lv2",
                        DetailDescription =
                            "攻撃が4上昇します。通常攻撃と火球の両方の威力が上がります。"
                    });
                    break;
            }
        }
        else
        {
            string passiveName;
            string passiveDescription;
            switch (MercenaryClassProgression.GetBaseClass(
                        mercenary.MercenaryClass))
            {
                case MercenaryClass.Warrior:
                    passiveName = "基礎体力";
                    passiveDescription = "Lv2で習得。最大HP+10、防御+3。";
                    break;
                case MercenaryClass.Archer:
                    passiveName = "速射訓練";
                    passiveDescription = "Lv2で習得。攻撃速度+0.05。";
                    break;
                case MercenaryClass.Mage:
                    passiveName = "魔力集中";
                    passiveDescription = "Lv2で習得。攻撃+4。";
                    break;
                default:
                    passiveName = null;
                    passiveDescription = null;
                    break;
            }

            if (!string.IsNullOrEmpty(passiveName))
            {
                skills.Add(new MercenarySkillInfo
                {
                    Name = passiveName,
                    ShortDescription = "未習得 / Lv2",
                    DetailDescription = passiveDescription,
                    Unlocked = false
                });
            }
        }

        if (mercenary.IsUnique &&
            mercenary.BaseData != null)
        {
            MercenaryDataSO data = mercenary.BaseData;
            bool unlocked =
                mercenary.Level >= Mathf.Max(1, data.uniqueSkillUnlockLevel);
            skills.Add(new MercenarySkillInfo
            {
                Name = data.uniqueSkillName,
                ShortDescription = unlocked
                    ? "固有スキル"
                    : $"未習得 / Lv{data.uniqueSkillUnlockLevel}",
                DetailDescription =
                    $"固有傭兵専用の能力です。\n" +
                    $"習得Lv: {data.uniqueSkillUnlockLevel}\n" +
                    $"最大HP+{data.uniqueSkillBonusMaxHP}、" +
                    $"攻撃+{data.uniqueSkillBonusAttack}、" +
                    $"防御+{data.uniqueSkillBonusDefense}、" +
                    $"魔力+{data.uniqueSkillBonusMaxMagicPower}、" +
                    $"速度+{data.uniqueSkillBonusAttackSpeed:0.00}",
                Unlocked = unlocked
            });
        }
        return skills;
    }

    private static string GetEquippedEquipmentName(
        MercenaryInstance mercenary,
        EquipmentSlot slot)
    {
        ItemDataSO item = mercenary?.GetEquippedItem(slot);
        if (item == null)
        {
            return "なし";
        }

        EquipmentInstance instance = mercenary.GetEquippedInstance(slot);
        string name = JapaneseDisplayText.GetItemName(item);
        return instance != null
            ? $"[{JapaneseDisplayText.GetEquipmentQuality(instance.Quality)}] " +
              $"{GetEquipmentDisplayName(instance)}"
            : name;
    }

    private static string BuildActiveSetSummary(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return "なし";
        }

        List<string> summaries = new List<string>();
        foreach (EquipmentSetId setId in
                 (EquipmentSetId[])System.Enum.GetValues(typeof(EquipmentSetId)))
        {
            if (setId == EquipmentSetId.None)
            {
                continue;
            }

            if (!EquipmentSetCatalog.TryGet(
                    setId,
                    out EquipmentSetDefinition definition))
            {
                continue;
            }

            int count = mercenary.GetEquippedSetCount(setId);
            if (count <= 0)
            {
                continue;
            }

            string active = count >= definition.ThreePiece.RequiredCount
                ? "全効果"
                : count >= definition.TwoPiece.RequiredCount
                    ? $"{definition.TwoPiece.RequiredCount}部位効果"
                    : "未発動";
            summaries.Add(
                $"{definition.DisplayName} " +
                $"{count}/{definition.ThreePiece.RequiredCount} {active}");
        }
        return summaries.Count > 0 ? string.Join(", ", summaries) : "なし";
    }

    public static string BuildEquipmentDescriptionText(ItemDataSO item)
    {
        return item != null && !string.IsNullOrWhiteSpace(item.description)
            ? $"説明\n{item.description}\n\n"
            : string.Empty;
    }

    public static string BuildEquipmentSetMembershipText(ItemDataSO item)
    {
        if (item == null || !EquipmentSetCatalog.TryGet(
                item.equipmentSet,
                out EquipmentSetDefinition definition))
        {
            return string.Empty;
        }
        return $"  <color=#{ColorUtility.ToHtmlStringRGB(definition.AccentColor)}>[{definition.DisplayName}]</color>";
    }

    public static string BuildEquipmentSetDetail(
        EquipmentSetId setId,
        MercenaryInstance mercenary)
    {
        if (!EquipmentSetCatalog.TryGet(setId, out EquipmentSetDefinition definition))
        {
            return "セット効果: なし";
        }
        int equippedCount = mercenary != null
            ? mercenary.GetEquippedSetCount(setId)
            : 0;
        string color = ColorUtility.ToHtmlStringRGB(definition.AccentColor);
        return $"セット: <color=#{color}>{definition.DisplayName}</color>\n" +
               BuildSetTierStatus(definition.TwoPiece, equippedCount, definition.AccentColor) + "\n" +
               BuildSetTierStatus(definition.ThreePiece, equippedCount, definition.AccentColor);
    }

    public static bool IsSetTierActive(EquipmentSetTier tier, int equippedCount)
    {
        return equippedCount >= tier.RequiredCount;
    }

    private static string BuildSetTierStatus(
        EquipmentSetTier tier,
        int equippedCount,
        Color accentColor)
    {
        bool active = IsSetTierActive(tier, equippedCount);
        string color = active
            ? ColorUtility.ToHtmlStringRGB(accentColor)
            : "777777";
        return $"<color=#{color}>{tier.RequiredCount}部位 ({equippedCount}/{tier.RequiredCount}): " +
               $"{EquipmentSetCatalog.BuildTierBonusText(tier)}</color>";
    }

    private static List<ItemDataSO> FindAllEquipmentAssets()
    {
        List<ItemDataSO> results =
            new List<ItemDataSO>(
                GameAssetRepository.LoadAll<ItemDataSO>());
        results.RemoveAll(item => item == null || !item.IsEquipment);
        return results;
    }

    private bool IsEquipmentInInventory(EquipmentInstance equipment)
    {
        foreach (EquipmentInstance owned in merchantInventory.EquipmentInstances)
        {
            if (ReferenceEquals(owned, equipment))
            {
                return true;
            }
        }
        return false;
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private static string FormatSigned(float value)
    {
        return value >= 0f ? $"+{value:0.00}" : value.ToString("0.00");
    }

    private static string FormatComparison(int difference)
    {
        return FormatComparison((float)difference, "0");
    }

    private static string FormatComparison(float difference)
    {
        return FormatComparison(difference, "0.00");
    }

    private static string FormatComparison(float difference, string format)
    {
        const string IncreaseColor = "#65D88A";
        const string DecreaseColor = "#FF7474";
        const string EqualColor = "#AEB6BE";
        string color = difference > 0f
            ? IncreaseColor
            : difference < 0f
                ? DecreaseColor
                : EqualColor;
        string sign = difference > 0f ? "+" : string.Empty;
        return $"<color={color}>({sign}{difference.ToString(format)})</color>";
    }
}

/// <summary>
/// Skill display row data. Moved out of SimpleMercenaryHireUI (where it
/// was a private nested class) in step 3.8 so the controller can expose
/// it from GetMercenarySkillInfos.
/// </summary>
public class MercenarySkillInfo
{
    public string Name;
    public string ShortDescription;
    public string DetailDescription;
    public bool Unlocked = true;
}
