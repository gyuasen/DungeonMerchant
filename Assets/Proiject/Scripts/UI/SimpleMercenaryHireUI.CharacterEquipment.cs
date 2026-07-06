using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildEquipmentDetailOverlay()
    {
        equipmentDetailOverlay = GetOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.EquipmentDetail,
            "Equipment Detail Overlay");
        equipmentDetailOverlay.anchorMin = Vector2.zero;
        equipmentDetailOverlay.anchorMax = Vector2.one;
        equipmentDetailOverlay.offsetMin = Vector2.zero;
        equipmentDetailOverlay.offsetMax = Vector2.zero;
        equipmentDetailOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.78f);

        RectTransform window = CreateUIObject("Equipment Detail Window", equipmentDetailOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(600f, 470f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        equipmentDetailTitle = CreateText(
            window, string.Empty, 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -66f), new Vector2(-28f, -20f),
            ParchmentTextColor);
        equipmentDetailText = CreateText(
            window, string.Empty, 17, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(28f, 92f), new Vector2(-28f, -82f),
            ParchmentTextColor);
        equipmentDetailText.rectTransform.anchorMin = Vector2.zero;
        equipmentDetailText.rectTransform.anchorMax = Vector2.one;
        equipmentDetailText.rectTransform.offsetMin = new Vector2(28f, 92f);
        equipmentDetailText.rectTransform.offsetMax = new Vector2(-28f, -82f);

        equipmentEnhanceButton =
            CreateActionButton(window, "強化", EnhanceSelectedEquipment);
        RectTransform enhanceRect = equipmentEnhanceButton.GetComponent<RectTransform>();
        enhanceRect.anchorMin = enhanceRect.anchorMax = new Vector2(1f, 0f);
        enhanceRect.pivot = new Vector2(1f, 0f);
        enhanceRect.anchoredPosition = new Vector2(-174f, 24f);

        equipmentSellButton =
            CreateActionButton(window, "売却", SellSelectedEquipment);
        equipmentSellButton.targetGraphic.color = ImportantButtonColor;
        RectTransform sellRect = equipmentSellButton.GetComponent<RectTransform>();
        sellRect.anchorMin = sellRect.anchorMax = new Vector2(1f, 0f);
        sellRect.pivot = new Vector2(1f, 0f);
        sellRect.anchoredPosition = new Vector2(-28f, 24f);

        equipmentLockButton =
            CreateActionButton(window, "ロック", ToggleSelectedEquipmentLock);
        RectTransform lockRect = equipmentLockButton.GetComponent<RectTransform>();
        lockRect.anchorMin = lockRect.anchorMax = new Vector2(0f, 0f);
        lockRect.pivot = new Vector2(0f, 0f);
        lockRect.anchoredPosition = new Vector2(28f, 24f);

        Button closeButton = CreateActionButton(window, "閉じる", HideEquipmentDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        equipmentDetailOverlay.gameObject.SetActive(false);
    }

    private void BuildEquipmentCollectionOverlay()
    {
        equipmentCollectionOverlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.EquipmentCollection,
                "Equipment Collection Overlay");
        equipmentCollectionOverlay.anchorMin = Vector2.zero;
        equipmentCollectionOverlay.anchorMax = Vector2.one;
        equipmentCollectionOverlay.offsetMin = Vector2.zero;
        equipmentCollectionOverlay.offsetMax = Vector2.zero;
        equipmentCollectionOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Equipment Collection Window", equipmentCollectionOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 560f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window, "装備図鑑", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f),
            ParchmentTextColor);

        RectTransform viewport = CreateUIObject("Collection Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 28f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        equipmentCollectionContent =
            CreateUIObject("Collection Content", viewport);
        equipmentCollectionContent.anchorMin = new Vector2(0f, 1f);
        equipmentCollectionContent.anchorMax = new Vector2(1f, 1f);
        equipmentCollectionContent.pivot = new Vector2(0.5f, 1f);
        equipmentCollectionText = CreateText(
            equipmentCollectionContent, string.Empty, 16, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(12f, 12f),
            new Vector2(-12f, -12f), ParchmentTextColor);
        equipmentCollectionText.supportRichText = true;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = equipmentCollectionContent;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        Button closeButton =
            CreateActionButton(window, "閉じる", HideEquipmentCollection);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        equipmentCollectionOverlay.gameObject.SetActive(false);
    }

    private void BuildCharacterDetailOverlay()
    {
        characterDetailOverlay = GetOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.CharacterDetail,
            "Character Detail Overlay");
        characterDetailOverlay.anchorMin = Vector2.zero;
        characterDetailOverlay.anchorMax = Vector2.one;
        characterDetailOverlay.offsetMin = Vector2.zero;
        characterDetailOverlay.offsetMax = Vector2.zero;

        Image overlayImage = characterDetailOverlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.78f);

        RectTransform window = CreateUIObject("Character Detail Window", characterDetailOverlay);
        window.anchorMin = new Vector2(0.5f, 0.5f);
        window.anchorMax = new Vector2(0.5f, 0.5f);
        window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(780f, 540f);
        window.anchoredPosition = Vector2.zero;

        Image windowImage = window.gameObject.AddComponent<Image>();
        ApplyParchmentPanel(windowImage);
        characterDetailTitle = CreateText(
            window,
            string.Empty,
            26,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -64f),
            new Vector2(-120f, -20f),
            ParchmentTextColor);

        characterStatusTabButton =
            CreateActionButton(window, "ステータス", ShowCharacterStatusPage);
        RectTransform statusTabRect =
            characterStatusTabButton.GetComponent<RectTransform>();
        statusTabRect.anchorMin = statusTabRect.anchorMax =
            new Vector2(0f, 1f);
        statusTabRect.pivot = new Vector2(0f, 1f);
        statusTabRect.sizeDelta = new Vector2(130f, 38f);
        statusTabRect.anchoredPosition = new Vector2(28f, -76f);

        characterEquipmentTabButton =
            CreateActionButton(window, "装備", ShowCharacterEquipmentPage);
        RectTransform equipmentTabRect =
            characterEquipmentTabButton.GetComponent<RectTransform>();
        equipmentTabRect.anchorMin = equipmentTabRect.anchorMax =
            new Vector2(0f, 1f);
        equipmentTabRect.pivot = new Vector2(0f, 1f);
        equipmentTabRect.sizeDelta = new Vector2(130f, 38f);
        equipmentTabRect.anchoredPosition = new Vector2(166f, -76f);

        characterStatusPage = CreateUIObject("Character Status Page", window);
        characterStatusPage.anchorMin = Vector2.zero;
        characterStatusPage.anchorMax = Vector2.one;
        characterStatusPage.offsetMin = new Vector2(28f, 28f);
        characterStatusPage.offsetMax = new Vector2(-28f, -122f);

        characterDetailText = CreateText(
            characterStatusPage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, 0f),
            new Vector2(-386f, 0f),
            ParchmentTextColor);
        characterDetailText.rectTransform.anchorMin = Vector2.zero;
        characterDetailText.rectTransform.anchorMax = Vector2.one;
        characterDetailText.rectTransform.offsetMin = Vector2.zero;
        characterDetailText.rectTransform.offsetMax = new Vector2(-386f, 0f);

        CreateText(
            characterStatusPage,
            "獲得スキル",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(360f, -36f),
            new Vector2(0f, 0f),
            ParchmentTextColor);

        RectTransform skillViewport =
            CreateUIObject("Skill Viewport", characterStatusPage);
        skillViewport.anchorMin = new Vector2(1f, 1f);
        skillViewport.anchorMax = new Vector2(1f, 1f);
        skillViewport.pivot = new Vector2(1f, 1f);
        skillViewport.sizeDelta = new Vector2(336f, 184f);
        skillViewport.anchoredPosition = new Vector2(0f, -46f);
        skillViewport.gameObject.AddComponent<Image>().color =
            new Color(0.28f, 0.16f, 0.07f, 0.12f);
        Mask skillMask = skillViewport.gameObject.AddComponent<Mask>();
        skillMask.showMaskGraphic = false;

        characterSkillList = CreateUIObject("Skill List", skillViewport);
        characterSkillList.anchorMin = new Vector2(0f, 1f);
        characterSkillList.anchorMax = new Vector2(1f, 1f);
        characterSkillList.pivot = new Vector2(0.5f, 1f);
        characterSkillList.anchoredPosition = Vector2.zero;

        ScrollRect skillScroll = skillViewport.gameObject.AddComponent<ScrollRect>();
        skillScroll.content = characterSkillList;
        skillScroll.viewport = skillViewport;
        skillScroll.horizontal = false;
        skillScroll.vertical = true;
        skillScroll.movementType = ScrollRect.MovementType.Clamped;
        skillScroll.scrollSensitivity = 24f;

        characterSkillDetailText = CreateText(
            characterStatusPage,
            "スキルを選択すると詳細を表示します。",
            15,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(360f, 0f),
            new Vector2(0f, -250f),
            ParchmentMutedColor);
        characterSkillDetailText.rectTransform.anchorMin =
            new Vector2(1f, 0f);
        characterSkillDetailText.rectTransform.anchorMax =
            new Vector2(1f, 0f);
        characterSkillDetailText.rectTransform.pivot = new Vector2(1f, 0f);
        characterSkillDetailText.rectTransform.sizeDelta = new Vector2(336f, 146f);
        characterSkillDetailText.rectTransform.anchoredPosition = Vector2.zero;

        characterEquipmentPage =
            CreateUIObject("Character Equipment Page", window);
        characterEquipmentPage.anchorMin = Vector2.zero;
        characterEquipmentPage.anchorMax = Vector2.one;
        characterEquipmentPage.offsetMin = new Vector2(28f, 28f);
        characterEquipmentPage.offsetMax = new Vector2(-28f, -122f);

        CreateText(
            characterEquipmentPage,
            "装備変更",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -36f),
            new Vector2(0f, 0f),
            ParchmentTextColor);

        RectTransform equipmentViewport =
            CreateUIObject("Equipment Viewport", characterEquipmentPage);
        equipmentViewport.anchorMin = new Vector2(1f, 1f);
        equipmentViewport.anchorMax = new Vector2(1f, 1f);
        equipmentViewport.pivot = new Vector2(1f, 1f);
        equipmentViewport.sizeDelta = new Vector2(724f, 360f);
        equipmentViewport.anchoredPosition = new Vector2(0f, -46f);

        Image equipmentViewportImage =
            equipmentViewport.gameObject.AddComponent<Image>();
        equipmentViewportImage.color =
            new Color(0.28f, 0.16f, 0.07f, 0.12f);
        Mask equipmentMask = equipmentViewport.gameObject.AddComponent<Mask>();
        equipmentMask.showMaskGraphic = false;

        characterEquipmentList =
            CreateUIObject("Equipment Scroll Content", equipmentViewport);
        characterEquipmentList.anchorMin = new Vector2(0f, 1f);
        characterEquipmentList.anchorMax = new Vector2(1f, 1f);
        characterEquipmentList.pivot = new Vector2(0.5f, 1f);
        characterEquipmentList.anchoredPosition = Vector2.zero;

        characterEquipmentScrollRect =
            equipmentViewport.gameObject.AddComponent<ScrollRect>();
        characterEquipmentScrollRect.content = characterEquipmentList;
        characterEquipmentScrollRect.viewport = equipmentViewport;
        characterEquipmentScrollRect.horizontal = false;
        characterEquipmentScrollRect.vertical = true;
        characterEquipmentScrollRect.movementType = ScrollRect.MovementType.Clamped;
        characterEquipmentScrollRect.scrollSensitivity = 30f;

        Button closeButton = CreateActionButton(window, "閉じる", HideCharacterDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        characterDetailOverlay.gameObject.SetActive(false);
    }

    private void ShowCharacterDetails(MercenaryInstance mercenary)
    {
        if (mercenary == null || characterDetailOverlay == null)
        {
            return;
        }

        bool keepCurrentDetailPage =
            characterDetailOverlay.gameObject.activeSelf &&
            ReferenceEquals(selectedDetailMercenary, mercenary);
        selectedDetailMercenary = mercenary;
        if (!keepCurrentDetailPage)
        {
            showingCharacterStatusPage = true;
        }
        RefreshCharacterDetailText();
        RebuildCharacterSkillList();
        RebuildCharacterEquipmentList();
        ApplyCharacterDetailPageVisibility();
        characterDetailOverlay.SetAsLastSibling();
        characterDetailOverlay.gameObject.SetActive(true);
    }

    private void RefreshCharacterDetailText()
    {
        if (selectedDetailMercenary == null || characterDetailText == null)
        {
            return;
        }

        MercenaryInstance mercenary = selectedDetailMercenary;
        string source = mercenary.IsUnique ? "固有傭兵" : "量産型傭兵";
        string condition = mercenary.IsIncapacitated ? "戦闘不能" : "行動可能";
        string shortId = mercenary.InstanceId.Substring(0, 8).ToUpperInvariant();
        string experienceText = mercenary.IsAtLevelCap
            ? "上限到達"
            : $"{mercenary.CurrentExperience} / " +
              $"{mercenary.ExperienceToNextLevel}";

        characterDetailTitle.text = mercenary.MercenaryName;
        characterDetailText.text =
            $"種別: {source}\n" +
            $"ID: {shortId}\n" +
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
            $"雇用費: {mercenary.HireCost} G";
    }

    private void ShowCharacterStatusPage()
    {
        showingCharacterStatusPage = true;
        ApplyCharacterDetailPageVisibility();
    }

    private void ShowCharacterEquipmentPage()
    {
        showingCharacterStatusPage = false;
        ApplyCharacterDetailPageVisibility();
    }

    private void ApplyCharacterDetailPageVisibility()
    {
        if (characterStatusPage != null)
        {
            characterStatusPage.gameObject.SetActive(showingCharacterStatusPage);
        }

        if (characterEquipmentPage != null)
        {
            characterEquipmentPage.gameObject.SetActive(!showingCharacterStatusPage);
        }

        if (characterStatusTabButton != null)
        {
            characterStatusTabButton.targetGraphic.color =
                showingCharacterStatusPage ? AccentColor : RowColor;
        }

        if (characterEquipmentTabButton != null)
        {
            characterEquipmentTabButton.targetGraphic.color =
                showingCharacterStatusPage ? RowColor : AccentColor;
        }
    }

    private void HideCharacterDetails()
    {
        if (characterDetailOverlay != null)
        {
            characterDetailOverlay.gameObject.SetActive(false);
        }

        selectedDetailMercenary = null;
    }

    private void RebuildCharacterSkillList()
    {
        if (characterSkillList == null || selectedDetailMercenary == null)
        {
            return;
        }

        ClearChildren(characterSkillList);
        List<MercenarySkillInfo> skills =
            GetMercenarySkillInfos(selectedDetailMercenary);
        float top = 0f;
        for (int i = 0; i < skills.Count; i++)
        {
            MercenarySkillInfo skill = skills[i];
            RectTransform row = CreateRow($"Skill {skill.Name}", characterSkillList, top);
            CreateText(
                row,
                $"{skill.Name}\n{skill.ShortDescription}",
                14,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Vector2(12f, -62f),
                new Vector2(-92f, -8f),
                skill.Unlocked ? Color.white : MutedTextColor);
            Button detailButton = CreateActionButton(
                row,
                "詳細",
                () => ShowMercenarySkillDetail(skill));
            RectTransform detailRect = detailButton.GetComponent<RectTransform>();
            detailRect.sizeDelta = new Vector2(72f, 34f);
            detailRect.anchoredPosition = new Vector2(-8f, 0f);
            top -= 104f;
        }

        characterSkillList.sizeDelta = new Vector2(0f, Mathf.Max(184f, -top));
        if (skills.Count > 0)
        {
            ShowMercenarySkillDetail(skills[0]);
        }
        else if (characterSkillDetailText != null)
        {
            characterSkillDetailText.text = "獲得済みスキルはありません。";
        }
    }

    private void ShowMercenarySkillDetail(MercenarySkillInfo skill)
    {
        if (characterSkillDetailText == null)
        {
            return;
        }

        string state = skill.Unlocked ? "習得済み" : "未習得";
        characterSkillDetailText.text =
            $"{skill.Name}  [{state}]\n\n" +
            $"{skill.DetailDescription}";
    }

    private void RebuildCharacterEquipmentList()
    {
        if (characterEquipmentList == null || selectedDetailMercenary == null)
        {
            return;
        }

        ClearChildren(characterEquipmentList);
        float top = 0f;
        foreach (EquipmentSlot slot in
                 System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            ItemDataSO equipped =
                selectedDetailMercenary.GetEquippedItem(slot);
            EquipmentInstance equippedInstance =
                selectedDetailMercenary.GetEquippedInstance(slot);
            if (equippedInstance != null)
            {
                CreateEquipmentInstanceOptionRow(
                    equippedInstance,
                    true,
                    top);
                top -= 116f;
            }
            else if (equipped != null)
            {
                CreateEquipmentOptionRow(equipped, true, top);
                top -= 116f;
            }
        }

        foreach (EquipmentInstance equipment in merchantInventory.EquipmentInstances)
        {
            if (equipment?.BaseItem == null ||
                !equipment.BaseItem.CanEquip(selectedDetailMercenary.MercenaryClass))
            {
                continue;
            }

            CreateEquipmentInstanceOptionRow(equipment, false, top);
            top -= 116f;
        }

        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            ItemDataSO item = stack?.Item;
            if (item == null ||
                stack.Amount <= 0 ||
                !item.CanEquip(selectedDetailMercenary.MercenaryClass))
            {
                continue;
            }

            CreateEquipmentOptionRow(item, false, top);
            top -= 116f;
        }

        if (top == 0f)
        {
            CreateText(
                characterEquipmentList,
                "装備できる武器を所持していません",
                14,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Vector2(0f, -50f),
                new Vector2(0f, 0f),
                MutedTextColor);
        }

        characterEquipmentList.sizeDelta =
            new Vector2(0f, Mathf.Max(398f, -top));
        Canvas.ForceUpdateCanvases();
        if (characterEquipmentScrollRect != null)
        {
            characterEquipmentScrollRect.StopMovement();
            characterEquipmentScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void CreateEquipmentOptionRow(ItemDataSO item, bool isEquipped, float top)
    {
        RectTransform row = CreateUIObject(
            isEquipped ? $"Equipped {item.equipmentSlot}" : item.itemName,
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 106f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;

        string owned = isEquipped
            ? "装備中"
            : $"所持 {merchantInventory.GetItemAmount(item)}";
        string stats = isEquipped
            ? BuildEquipmentBonusText(item)
            : BuildEquipmentComparisonText(
                item,
                selectedDetailMercenary.GetEquippedItem(item.equipmentSlot),
                selectedDetailMercenary.GetEquippedInstance(
                    item.equipmentSlot));
        CreateText(
            row,
            $"<b>[{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}] " +
            $"{JapaneseDisplayText.GetItemName(item)}</b>  " +
            $"R{item.equipmentRank}  {owned}\n{stats}",
            15,
            isEquipped ? FontStyle.Bold : FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(12f, -96f),
            new Vector2(-96f, -10f),
            Color.white);

        Button button = CreateActionButton(
            row,
            isEquipped ? "解除" : "装備",
            isEquipped
                ? () => UnequipSelectedEquipment(item.equipmentSlot)
                : () => EquipSelectedEquipment(item));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);
    }

    private void CreateEquipmentInstanceOptionRow(
        EquipmentInstance equipment,
        bool isEquipped,
        float top)
    {
        ItemDataSO item = equipment.BaseItem;
        RectTransform row = CreateUIObject(
            isEquipped
                ? $"Equipped Quality {item.equipmentSlot}"
                : equipment.InstanceId,
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 106f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;

        string quality = JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        Color qualityColor = GetEquipmentQualityColor(equipment.Quality);
        string stats = BuildEquipmentInstanceComparisonText(
            equipment,
            selectedDetailMercenary.GetEquippedInstance(item.equipmentSlot),
            selectedDetailMercenary.GetEquippedItem(item.equipmentSlot));
        CreateText(
            row,
            $"<b>[{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}・" +
            $"{quality}] {GetEquipmentDisplayName(equipment)}</b>  " +
            $"R{item.equipmentRank}  {(isEquipped ? "装備中" : "個体装備")}\n{stats}",
            15,
            isEquipped ? FontStyle.Bold : FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(12f, -96f),
            new Vector2(-170f, -10f),
            qualityColor);

        Button button = CreateActionButton(
            row,
            isEquipped ? "解除" : "装備",
            isEquipped
                ? () => UnequipSelectedEquipment(item.equipmentSlot)
                : () => EquipSelectedEquipment(equipment));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);

        Button detailButton = CreateActionButton(
            row,
            "詳細",
            () => ShowEquipmentDetails(equipment));
        RectTransform detailRect = detailButton.GetComponent<RectTransform>();
        detailRect.sizeDelta = new Vector2(64f, 40f);
        detailRect.anchoredPosition = new Vector2(-92f, 0f);
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

    private static string GetEquipmentDisplayName(EquipmentInstance equipment)
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

            int count = mercenary.GetEquippedSetCount(setId);
            if (count <= 0)
            {
                continue;
            }

            string active = count >= 3
                ? "全効果"
                : count >= 2 ? "2部位効果" : "未発動";
            summaries.Add(
                $"{JapaneseDisplayText.GetEquipmentSet(setId)} {count}/3 {active}");
        }
        return summaries.Count > 0 ? string.Join(", ", summaries) : "なし";
    }

    private static Color GetEquipmentQualityColor(EquipmentQuality quality)
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

    private static string BuildEquipmentInstanceComparisonText(
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
               $"{FormatComparison(candidate.BonusAttackSpeed - currentSpeed)}";
    }

    private static string BuildEquipmentBonusText(ItemDataSO item)
    {
        return $"HP {FormatSigned(item.bonusMaxHP)}  " +
               $"攻撃 {FormatSigned(item.bonusAttack)}\n" +
               $"防御 {FormatSigned(item.bonusDefense)}  " +
               $"速度 {FormatSigned(item.bonusAttackSpeed)}";
    }

    private static string BuildEquipmentComparisonText(
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
               $"{FormatComparison(candidate.bonusAttackSpeed - currentSpeed)}";
    }

    private void UnequipSelectedEquipment(EquipmentSlot slot)
    {
        if (selectedDetailMercenary == null ||
            selectedDetailMercenary.GetEquippedItem(slot) == null)
        {
            return;
        }

        EquipmentInstance previousInstance =
            selectedDetailMercenary.GetEquippedInstance(slot);
        if (previousInstance != null)
        {
            selectedDetailMercenary.UnequipEquipmentInstance(slot);
            merchantInventory.AddEquipmentInstance(previousInstance);
        }
        else
        {
            ItemDataSO previousItem =
                selectedDetailMercenary.UnequipEquipment(slot);
            merchantInventory.AddItem(previousItem);
        }
        statusText.text =
            $"{selectedDetailMercenary.MercenaryName}の" +
            $"{JapaneseDisplayText.GetEquipmentSlot(slot)}を解除しました。";
        ShowCharacterDetails(selectedDetailMercenary);
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        SaveEquipmentChanges();
    }

    private void SaveEquipmentChanges()
    {
        if (saveManager == null)
        {
            saveManager = GetComponent<SaveManager>() ??
                          FindObjectOfType<SaveManager>();
        }

        if (saveManager == null)
        {
            Debug.LogWarning("装備変更を保存するSaveManagerが見つかりません。", this);
            return;
        }

        saveManager.SaveGame();
    }

    private void ShowEquipmentDetails(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null || equipmentDetailOverlay == null)
        {
            return;
        }

        selectedEquipmentDetail = equipment;
        ItemDataSO item = equipment.BaseItem;
        string quality = JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        equipmentDetailTitle.text =
            $"[{quality}] {GetEquipmentDisplayName(equipment)}";
        equipmentDetailTitle.color = GetEquipmentQualityColor(equipment.Quality);

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
        string setText = BuildEquipmentSetDetail(item.equipmentSet);
        string target = item.allClassesCanEquip
            ? "全職業"
            : JapaneseDisplayText.GetMercenaryClass(item.requiredClass);
        ItemDataSO enhancementMaterial =
            merchantInventory.GetEnhancementMaterial(equipment);
        string enhancementMaterialName = enhancementMaterial != null
            ? JapaneseDisplayText.GetItemName(enhancementMaterial)
            : "対応する強化鉱石";

        equipmentDetailText.text =
            $"種類: {JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}\n" +
            $"装備対象: {target}  ランク: {item.equipmentRank}\n" +
            $"品質: {quality}  強化: +{equipment.EnhancementLevel} / +10\n\n" +
            $"最終性能\n" +
            $"HP {FormatSigned(equipment.BonusMaxHP)}  " +
            $"攻撃 {FormatSigned(equipment.BonusAttack)}\n" +
            $"防御 {FormatSigned(equipment.BonusDefense)}  " +
            $"攻撃速度 {FormatSigned(equipment.BonusAttackSpeed)}\n\n" +
            $"追加効果\n{modifiers}\n\n{setText}\n\n" +
            $"次回強化: 成功率 " +
            $"{equipment.GetEnhancementSuccessRate() * 100f:0}%  " +
            $"{enhancementMaterialName} " +
            $"{equipment.GetEnhancementMaterialAmount()}個";

        bool canEnhance = equipment.EnhancementLevel < 10;
        equipmentEnhanceButton.interactable =
            canEnhance &&
            merchantData.CanPay(equipment.GetEnhancementCost()) &&
            enhancementMaterial != null &&
            merchantInventory.HasItem(
                enhancementMaterial,
                equipment.GetEnhancementMaterialAmount());
        equipmentEnhanceButton.GetComponentInChildren<Text>().text =
            canEnhance
                ? $"強化 {equipment.GetEnhancementCost()}G"
                : "強化完了";
        equipmentSellButton.interactable =
            IsEquipmentInInventory(equipment) && !equipment.IsLocked;
        equipmentSellButton.GetComponentInChildren<Text>().text =
            $"売却 {merchantInventory.GetSellPrice(equipment)}G";
        equipmentLockButton.GetComponentInChildren<Text>().text =
            equipment.IsLocked ? "ロック解除" : "ロック";

        equipmentDetailOverlay.SetAsLastSibling();
        equipmentDetailOverlay.gameObject.SetActive(true);
    }

    private static string BuildEquipmentSetDetail(EquipmentSetId setId)
    {
        if (setId == EquipmentSetId.None)
        {
            return "セット効果: なし";
        }

        switch (setId)
        {
            case EquipmentSetId.Vanguard:
                return "セット: 不屈の前衛\n" +
                       "2部位: 最大HP+20、防御+10\n" +
                       "3部位: 攻撃+8";
            case EquipmentSetId.Windstalker:
                return "セット: 風狩り\n" +
                       "2部位: 攻撃+5、攻撃速度+0.08\n" +
                       "3部位: 攻撃+10、攻撃速度+0.06";
            case EquipmentSetId.ArcaneSage:
                return "セット: 秘術賢者\n" +
                       "2部位: 攻撃+10\n" +
                       "3部位: 攻撃+15、攻撃速度+0.04";
            case EquipmentSetId.OniHunter:
                return "セット: 鬼狩り\n" +
                       "2部位: 最大HP+10、攻撃+3\n" +
                       "3部位: 攻撃+5、防御+2";
            default:
                return "セット: 古代守護者\n" +
                       "2部位: 最大HP+30、防御+8\n" +
                       "3部位: 攻撃+12、攻撃速度+0.08";
        }
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

    private void HideEquipmentDetails()
    {
        equipmentDetailOverlay?.gameObject.SetActive(false);
        selectedEquipmentDetail = null;
    }

    private void EnhanceSelectedEquipment()
    {
        EquipmentInstance equipment = selectedEquipmentDetail;
        if (equipment == null)
        {
            return;
        }

        EquipmentEnhancementResult result =
            merchantInventory.TryEnhanceEquipment(equipment);
        switch (result)
        {
            case EquipmentEnhancementResult.Succeeded:
                statusText.text =
                    $"{GetEquipmentDisplayName(equipment)}の強化に成功しました。";
                break;
            case EquipmentEnhancementResult.Failed:
                statusText.text =
                    "強化に失敗しました。装備と強化値は維持されます。";
                break;
            case EquipmentEnhancementResult.NotEnoughMaterial:
                statusText.text = "強化鉱石が不足しています。";
                break;
            case EquipmentEnhancementResult.NotEnoughGold:
                statusText.text = "ゴールドが不足しています。";
                break;
            default:
                statusText.text = "装備を強化できませんでした。";
                break;
        }
        RefreshPage(inventoryPage);
        if (selectedDetailMercenary != null)
        {
            ShowCharacterDetails(selectedDetailMercenary);
        }
        ShowEquipmentDetails(equipment);
        RefreshUI();
        SaveEquipmentChanges();
    }

    private void ToggleSelectedEquipmentLock()
    {
        if (selectedEquipmentDetail == null)
        {
            return;
        }

        merchantInventory.ToggleEquipmentLock(selectedEquipmentDetail);
        statusText.text = selectedEquipmentDetail.IsLocked
            ? "装備をロックしました。"
            : "装備のロックを解除しました。";
        RefreshPage(inventoryPage);
        ShowEquipmentDetails(selectedEquipmentDetail);
        SaveEquipmentChanges();
    }

    private void SellSelectedEquipment()
    {
        EquipmentInstance equipment = selectedEquipmentDetail;
        if (equipment == null || !IsEquipmentInInventory(equipment))
        {
            return;
        }

        HideEquipmentDetails();
        SellEquipment(equipment);
    }

    private void ShowEquipmentCollection()
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
            string set = item.equipmentSet != EquipmentSetId.None
                ? $" / {JapaneseDisplayText.GetEquipmentSet(item.equipmentSet)}"
                : string.Empty;
            string source = item.acquisitionType == ItemAcquisitionType.Dungeon
                ? "ダンジョン限定"
                : item.acquisitionType == ItemAcquisitionType.Blacksmith
                    ? "鍛冶屋"
                    : "市場";
            lines.Add(
                $"{(discovered ? "●" : "○")} [{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}] " +
                $"{name}{set} / {source}");
        }

        equipmentCollectionText.text =
            $"収集率 {discoveredCount}/{equipmentItems.Count}\n\n" +
            string.Join("\n", lines);
        float height = Mathf.Max(430f, 76f + lines.Count * 28f);
        equipmentCollectionContent.sizeDelta = new Vector2(0f, height);
        equipmentCollectionText.rectTransform.anchorMin = Vector2.zero;
        equipmentCollectionText.rectTransform.anchorMax = Vector2.one;
        equipmentCollectionText.rectTransform.offsetMin = new Vector2(12f, 12f);
        equipmentCollectionText.rectTransform.offsetMax = new Vector2(-12f, -12f);
        equipmentCollectionOverlay.SetAsLastSibling();
        equipmentCollectionOverlay.gameObject.SetActive(true);
    }

    private void HideEquipmentCollection()
    {
        equipmentCollectionOverlay?.gameObject.SetActive(false);
    }

    private static List<MercenarySkillInfo> GetMercenarySkillInfos(
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
            if (!definition.IsPassive)
            {
                continue;
            }
            bool unlocked = mercenary.Level >= definition.UnlockLevel;
            skills.Add(new MercenarySkillInfo
            {
                Name = definition.Name,
                ShortDescription = unlocked
                    ? $"パッシブ / Lv{definition.UnlockLevel}"
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

    private static string BuildMercenarySkillSummary(MercenaryInstance mercenary)
    {
        List<string> skills = new List<string>();
        foreach (MercenarySkillInfo skill in GetMercenarySkillInfos(mercenary))
        {
            if (skill.Unlocked)
            {
                skills.Add($"{skill.Name}: {skill.ShortDescription}");
            }
        }

        return skills.Count > 0
            ? string.Join(" / ", skills)
            : "スキル未設定";
    }

    private static List<ItemDataSO> FindAllEquipmentAssets()
    {
        List<ItemDataSO> results =
            new List<ItemDataSO>(
                GameAssetRepository.LoadAll<ItemDataSO>());
        results.RemoveAll(item => item == null || !item.IsEquipment);
        return results;
    }

}
