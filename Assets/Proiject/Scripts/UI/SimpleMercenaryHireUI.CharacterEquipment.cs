using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI : IEquipmentDetailView
{
    private void BuildEquipmentDetailOverlay()
    {
        equipmentDetailOverlay = GetOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.EquipmentDetail,
            "Equipment Detail Overlay");
        equipmentDetailOverlay.gameObject.SetActive(false);
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

        equipmentEnhanceButton = CreateActionButton(
            window,
            "強化",
            () => characterEquipmentController.EnhanceSelectedEquipment());
        RectTransform enhanceRect = equipmentEnhanceButton.GetComponent<RectTransform>();
        enhanceRect.anchorMin = enhanceRect.anchorMax = new Vector2(1f, 0f);
        enhanceRect.pivot = new Vector2(1f, 0f);
        enhanceRect.anchoredPosition = new Vector2(-174f, 24f);

        equipmentSellButton = CreateActionButton(
            window,
            "売却",
            () => characterEquipmentController.SellSelectedEquipment());
        equipmentSellButton.targetGraphic.color = ImportantButtonColor;
        RectTransform sellRect = equipmentSellButton.GetComponent<RectTransform>();
        sellRect.anchorMin = sellRect.anchorMax = new Vector2(1f, 0f);
        sellRect.pivot = new Vector2(1f, 0f);
        sellRect.anchoredPosition = new Vector2(-28f, 24f);

        equipmentLockButton = CreateActionButton(
            window,
            "ロック",
            () => characterEquipmentController.ToggleSelectedEquipmentLock());
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
        equipmentCollectionOverlay.gameObject.SetActive(false);
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
        window.sizeDelta = new Vector2(820f, 600f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window, "装備図鑑", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f),
            ParchmentTextColor);

        RectTransform bookRoot = CreateUIObject("Equipment Codex Book", window);
        bookRoot.anchorMin = Vector2.zero;
        bookRoot.anchorMax = Vector2.one;
        bookRoot.offsetMin = new Vector2(34f, 34f);
        bookRoot.offsetMax = new Vector2(-34f, -88f);
        equipmentCodexBook = bookRoot.gameObject.AddComponent<BookPageUI>();
        equipmentCodexBook.Initialize("装備図鑑", uiFont, uiBodyFont);

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
        characterDetailOverlay.gameObject.SetActive(false);
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
            ReferenceEquals(
                characterEquipmentController.SelectedDetailMercenary,
                mercenary);
        characterEquipmentController.SelectedDetailMercenary = mercenary;
        if (!keepCurrentDetailPage)
        {
            showingCharacterStatusPage = true;
        }
        characterEquipmentController.RefreshCharacterDetailText();
        RebuildCharacterSkillList();
        RebuildCharacterEquipmentList();
        ApplyCharacterDetailPageVisibility();
        characterDetailOverlay.SetAsLastSibling();
        characterDetailOverlay.gameObject.SetActive(true);
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

        characterEquipmentController.SelectedDetailMercenary = null;
    }

    private void RebuildCharacterSkillList()
    {
        if (characterSkillList == null ||
            characterEquipmentController.SelectedDetailMercenary == null)
        {
            return;
        }

        ClearChildren(characterSkillList);
        List<MercenarySkillInfo> skills =
            CharacterEquipmentController.GetMercenarySkillInfos(
                characterEquipmentController.SelectedDetailMercenary);
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
        MercenaryInstance selectedMercenary =
            characterEquipmentController.SelectedDetailMercenary;
        if (characterEquipmentList == null || selectedMercenary == null)
        {
            return;
        }

        ClearChildren(characterEquipmentList);
        float top = 0f;
        for (int slotIndex = 0; slotIndex < selectedMercenary.ConsumableSlots.Count; slotIndex++)
        {
            CreateConsumableSlotRow(slotIndex, top);
            top -= 76f;
        }
        foreach (EquipmentSlot slot in
                 System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            ItemDataSO equipped =
                selectedMercenary.GetEquippedItem(slot);
            EquipmentInstance equippedInstance =
                selectedMercenary.GetEquippedInstance(slot);
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
                !equipment.BaseItem.CanEquip(selectedMercenary.MercenaryClass))
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
                !item.CanEquip(selectedMercenary.MercenaryClass))
            {
                continue;
            }

            CreateEquipmentOptionRow(item, false, top);
            top -= 116f;
        }

        foreach (InventoryItemStack stack in merchantInventory.Items)
        {
            ItemDataSO item = stack?.Item;
            if (item == null || stack.Amount <= 0 ||
                item.itemType != ItemType.Consumable)
            {
                continue;
            }

            for (int slotIndex = 0; slotIndex < selectedMercenary.ConsumableSlots.Count; slotIndex++)
            {
                CreateConsumableLoadRow(slotIndex, item, top);
                top -= 76f;
            }
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

    private void CreateConsumableSlotRow(int slotIndex, float top)
    {
        MercenaryConsumableSlot slot =
            characterEquipmentController.SelectedDetailMercenary.ConsumableSlots[slotIndex];
        RectTransform row = CreateUIObject(
            $"Consumable Slot {slotIndex + 1}",
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 66f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;
        string itemText = slot.IsEmpty
            ? "空"
            : $"{JapaneseDisplayText.GetItemName(slot.Item)} x{slot.Count}/5";
        CreateText(
            row,
            $"<b>消耗品スロット {slotIndex + 1}</b>  {itemText}",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(12f, -56f),
            new Vector2(-96f, -10f),
            Color.white);
        Button button = CreateActionButton(
            row,
            "取り外し",
            () => characterEquipmentController.UnloadConsumable(slotIndex));
        button.interactable = !slot.IsEmpty;
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);
    }

    private void CreateConsumableLoadRow(
        int slotIndex,
        ItemDataSO item,
        float top)
    {
        MercenaryConsumableSlot slot =
            characterEquipmentController.SelectedDetailMercenary.ConsumableSlots[slotIndex];
        if (!slot.IsEmpty && slot.Item != item)
        {
            return;
        }

        RectTransform row = CreateUIObject(
            $"Load {item.itemName} Slot {slotIndex + 1}",
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 66f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;
        CreateText(
            row,
            $"{JapaneseDisplayText.GetItemName(item)}  倉庫 {merchantInventory.GetItemAmount(item)}\n" +
            $"消耗品スロット {slotIndex + 1}へ1個装填",
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(12f, -56f),
            new Vector2(-96f, -8f),
            Color.white);
        Button button = CreateActionButton(
            row,
            "装填",
            () => characterEquipmentController.LoadConsumable(slotIndex, item));
        button.interactable = slot.Count < MercenaryConsumableSlot.MaxCount;
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);
    }

    private void CreateEquipmentOptionRow(ItemDataSO item, bool isEquipped, float top)
    {
        MercenaryInstance selectedMercenary =
            characterEquipmentController.SelectedDetailMercenary;
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
            ? CharacterEquipmentController.BuildEquipmentBonusText(item)
            : CharacterEquipmentController.BuildEquipmentComparisonText(
                item,
                selectedMercenary.GetEquippedItem(item.equipmentSlot),
                selectedMercenary.GetEquippedInstance(
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
                ? () => characterEquipmentController.UnequipSelectedEquipment(
                    item.equipmentSlot)
                : (UnityEngine.Events.UnityAction)(() =>
                    characterEquipmentController.EquipSelectedEquipment(item)));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);
    }

    private void CreateEquipmentInstanceOptionRow(
        EquipmentInstance equipment,
        bool isEquipped,
        float top)
    {
        MercenaryInstance selectedMercenary =
            characterEquipmentController.SelectedDetailMercenary;
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
        Color qualityColor =
            CharacterEquipmentController.GetEquipmentQualityColor(equipment.Quality);
        string stats =
            CharacterEquipmentController.BuildEquipmentInstanceComparisonText(
                equipment,
                selectedMercenary.GetEquippedInstance(item.equipmentSlot),
                selectedMercenary.GetEquippedItem(item.equipmentSlot));
        CreateText(
            row,
            $"<b>[{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}・" +
            $"{quality}] " +
            $"{CharacterEquipmentController.GetEquipmentDisplayName(equipment)}</b>  " +
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
                ? () => characterEquipmentController.UnequipSelectedEquipment(
                    item.equipmentSlot)
                : (UnityEngine.Events.UnityAction)(() =>
                    characterEquipmentController.EquipSelectedEquipment(equipment)));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(76f, 40f);
        buttonRect.anchoredPosition = new Vector2(-8f, 0f);

        Button detailButton = CreateActionButton(
            row,
            "詳細",
            () => characterEquipmentController.ShowEquipmentDetails(equipment));
        RectTransform detailRect = detailButton.GetComponent<RectTransform>();
        detailRect.sizeDelta = new Vector2(64f, 40f);
        detailRect.anchoredPosition = new Vector2(-92f, 0f);
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

    private void HideEquipmentDetails()
    {
        equipmentDetailOverlay?.gameObject.SetActive(false);
        characterEquipmentController.SelectedEquipmentDetail = null;
    }

    private void ShowEquipmentCollection()
    {
        List<BookPageUI.Entry> entries = new List<BookPageUI.Entry>();
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item == null || !item.IsEquipment)
            {
                continue;
            }

            bool discovered = merchantInventory != null &&
                merchantInventory.HasDiscoveredEquipment(item);
            entries.Add(new BookPageUI.Entry
            {
                Name = JapaneseDisplayText.GetItemName(item) + "  " +
                    EquipmentRankPresentation.GetRichText(item),
                Detail = BuildEquipmentCodexDetail(item),
                Sprite = Resources.Load<Sprite>("UI/Codex/Equipment/" + item.name),
                Discovered = discovered
            });
        }

        entries.Sort((left, right) => string.Compare(left.Name, right.Name, System.StringComparison.Ordinal));
        equipmentCodexBook.SetEntries(entries);
        equipmentCollectionOverlay.SetAsLastSibling();
        equipmentCollectionOverlay.gameObject.SetActive(true);
    }

    private static string BuildEquipmentCodexDetail(ItemDataSO item)
    {
        string target = item.allClassesCanEquip
            ? "全職業"
            : JapaneseDisplayText.GetMercenaryClass(item.requiredClass);
        return string.Format(
            "{0} / {1}\n{2}  Rank {3}\nHP {4:+#;-#;0}  攻 {5:+#;-#;0}  防 {6:+#;-#;0}  速 {7:+0.##;-0.##;0}\n基本価格 {8} G",
            JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot),
            target,
            EquipmentRankPresentation.GetRichText(item),
            item.equipmentRank,
            item.bonusMaxHP,
            item.bonusAttack,
            item.bonusDefense,
            item.bonusAttackSpeed,
            item.basePrice) +
            "\n特殊効果\n" +
            EquipmentEffectTextFormatter.FormatList(item.equipmentEffects);
    }

    private void HideEquipmentCollection()
    {
        equipmentCollectionOverlay?.gameObject.SetActive(false);
    }

    // --- IEquipmentDetailView (equipment-detail overlay view surface for
    // CharacterEquipmentController; bodies are the former constructor
    // lambdas, moved verbatim in step B-2) ---

    bool IEquipmentDetailView.HasOverlay => equipmentDetailOverlay != null;

    void IEquipmentDetailView.SetTitle(string title, Color color)
    {
        equipmentDetailTitle.text = title;
        equipmentDetailTitle.color = color;
    }

    void IEquipmentDetailView.SetDetailText(string text)
    {
        equipmentDetailText.text = text;
    }

    void IEquipmentDetailView.SetEnhanceButton(bool interactable, string label)
    {
        equipmentEnhanceButton.interactable = interactable;
        equipmentEnhanceButton.GetComponentInChildren<Text>().text = label;
    }

    void IEquipmentDetailView.SetSellButton(bool interactable, string label)
    {
        equipmentSellButton.interactable = interactable;
        equipmentSellButton.GetComponentInChildren<Text>().text = label;
    }

    void IEquipmentDetailView.SetLockButtonLabel(string label)
    {
        equipmentLockButton.GetComponentInChildren<Text>().text = label;
    }

    void IEquipmentDetailView.ShowOverlay()
    {
        equipmentDetailOverlay.SetAsLastSibling();
        equipmentDetailOverlay.gameObject.SetActive(true);
    }

    void IEquipmentDetailView.HideOverlay()
    {
        HideEquipmentDetails();
    }
}
