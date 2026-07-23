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

        equipmentCodexNormalTabButton = CreateActionButton(window, "通常装備", ShowNormalEquipmentCodexTab);
        RectTransform normalTabRect = equipmentCodexNormalTabButton.GetComponent<RectTransform>();
        normalTabRect.anchorMin = normalTabRect.anchorMax = new Vector2(0f, 1f);
        normalTabRect.pivot = new Vector2(0f, 1f);
        normalTabRect.anchoredPosition = new Vector2(250f, -20f);
        equipmentCodexSpecialTabButton = CreateActionButton(window, "特殊装備", ShowSpecialEquipmentCodexTab);
        RectTransform specialTabRect = equipmentCodexSpecialTabButton.GetComponent<RectTransform>();
        specialTabRect.anchorMin = specialTabRect.anchorMax = new Vector2(0f, 1f);
        specialTabRect.pivot = new Vector2(0f, 1f);
        specialTabRect.anchoredPosition = new Vector2(380f, -20f);

        equipmentCodexNormalRoot = CreateUIObject("Equipment Codex Normal Book", window);
        equipmentCodexNormalRoot.anchorMin = Vector2.zero;
        equipmentCodexNormalRoot.anchorMax = Vector2.one;
        equipmentCodexNormalRoot.offsetMin = new Vector2(34f, 34f);
        equipmentCodexNormalRoot.offsetMax = new Vector2(-34f, -88f);
        equipmentCodexBook = equipmentCodexNormalRoot.gameObject.AddComponent<BookPageUI>();
        equipmentCodexBook.Initialize(string.Empty, uiFont, uiBodyFont);
        equipmentCodexSpecialRoot = CreateUIObject("Equipment Codex Special Pages", window);
        equipmentCodexSpecialRoot.anchorMin = Vector2.zero;
        equipmentCodexSpecialRoot.anchorMax = Vector2.one;
        equipmentCodexSpecialRoot.offsetMin = new Vector2(34f, 34f);
        equipmentCodexSpecialRoot.offsetMax = new Vector2(-34f, -88f);
        equipmentSpecialCodexPage = equipmentCodexSpecialRoot.gameObject.AddComponent<EquipmentSpecialCodexPageUI>();
        equipmentSpecialCodexPage.Initialize(uiFont, uiBodyFont);
#if UNITY_EDITOR
        equipmentCodexNormalRoot.offsetMin = new Vector2(34f, 76f);
        equipmentCodexSpecialRoot.offsetMin = new Vector2(34f, 76f);
        BuildEquipmentCodexDebugButtons(window);
#endif
        ShowNormalEquipmentCodexTab();

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

    private void BuildEquipmentSlotSelectionOverlay()
    {
        equipmentSlotSelectionOverlay = CreateUIObject(
            "Equipment Slot Selection Overlay",
            overlayRoot);
        equipmentSlotSelectionOverlay.anchorMin = Vector2.zero;
        equipmentSlotSelectionOverlay.anchorMax = Vector2.one;
        equipmentSlotSelectionOverlay.offsetMin = Vector2.zero;
        equipmentSlotSelectionOverlay.offsetMax = Vector2.zero;
        equipmentSlotSelectionOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject(
            "Equipment Slot Selection Window",
            equipmentSlotSelectionOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 600f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        equipmentSlotSelectionTitle = CreateText(
            window,
            string.Empty,
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -66f),
            new Vector2(-28f, -20f),
            ParchmentTextColor);
        equipmentSlotSelectionContent = CreateScrollableContent(
            window,
            "Equipment Slot Selection Viewport",
            "Equipment Slot Selection Content",
            new Vector2(28f, 86f),
            new Vector2(-28f, -82f));
        Button closeButton = CreateActionButton(
            window,
            "閉じる",
            HideEquipmentSlotSelection);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot =
            new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 48f);
        closeRect.anchoredPosition = Vector2.zero + new Vector2(0f, 26f);
        equipmentSlotSelectionOverlay.gameObject.SetActive(false);
    }

    private void ShowEquipmentSlotSelection(EquipmentSlot slot)
    {
        selectedEquipmentSlot = slot;
        selectedConsumableSlotIndex = -1;
        equipmentSlotSelectionTitle.text =
            JapaneseDisplayText.GetEquipmentSlot(slot) + "を選択";
        RebuildEquipmentSlotSelection();
        equipmentSlotSelectionOverlay.SetAsLastSibling();
        equipmentSlotSelectionOverlay.gameObject.SetActive(true);
    }

    private void ShowConsumableSlotSelection(int slotIndex)
    {
        selectedConsumableSlotIndex = slotIndex;
        equipmentSlotSelectionTitle.text =
            "消耗品スロット " + (slotIndex + 1) + " を選択";
        RebuildEquipmentSlotSelection();
        equipmentSlotSelectionOverlay.SetAsLastSibling();
        equipmentSlotSelectionOverlay.gameObject.SetActive(true);
    }

    private void RebuildEquipmentSlotSelection()
    {
        ClearChildren(equipmentSlotSelectionContent);
        MercenaryInstance mercenary = characterEquipmentController.SelectedDetailMercenary;
        if (mercenary == null)
        {
            return;
        }
        float top = 0f;
        if (selectedConsumableSlotIndex >= 0)
        {
            CreateSlotSelectionActionRow("取り外す", "このスロットの消耗品を倉庫へ戻します。", top, () =>
            {
                characterEquipmentController.UnloadConsumable(selectedConsumableSlotIndex);
                HideEquipmentSlotSelection();
            });
            top -= 76f;
            foreach (InventoryItemStack stack in merchantInventory.Items)
            {
                if (stack?.Item == null || stack.Amount <= 0 ||
                    stack.Item.itemType != ItemType.Consumable)
                {
                    continue;
                }
                CreateConsumableSlotSelectionRow(stack, top);
                top -= 92f;
            }
        }
        else
        {
            CreateSlotSelectionActionRow("外す", "現在の装備を倉庫へ戻します。", top, () =>
            {
                characterEquipmentController.UnequipSelectedEquipment(selectedEquipmentSlot);
                HideEquipmentSlotSelection();
            });
            top -= 76f;
            foreach (EquipmentInstance equipment in merchantInventory.EquipmentInstances)
            {
                if (equipment?.BaseItem == null ||
                    equipment.BaseItem.equipmentSlot != selectedEquipmentSlot ||
                    !equipment.BaseItem.CanEquip(mercenary.MercenaryClass))
                {
                    continue;
                }
                CreateEquipmentInstanceSlotSelectionRow(equipment, top);
                top -= 116f;
            }
            foreach (InventoryItemStack stack in merchantInventory.Items)
            {
                if (stack?.Item == null || stack.Amount <= 0 ||
                    stack.Item.equipmentSlot != selectedEquipmentSlot ||
                    !stack.Item.CanEquip(mercenary.MercenaryClass))
                {
                    continue;
                }
                CreateEquipmentSlotSelectionRow(stack, top);
                top -= 116f;
            }
        }
        if (top == (selectedConsumableSlotIndex >= 0 ? -76f : -76f))
        {
            CreateText(equipmentSlotSelectionContent, "選択できる所持品はありません。", 16, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(12f, -118f), new Vector2(-12f, -70f), MutedTextColor);
        }
        equipmentSlotSelectionContent.sizeDelta =
            new Vector2(0f, Mathf.Max(398f, -top));
    }

    private void CreateSlotSelectionActionRow(string label, string detail, float top, UnityEngine.Events.UnityAction action)
    {
        RectTransform row = CreateSlotSelectionRow(label, top, 66f);
        CreateText(row, "<b>" + label + "</b>\n" + detail, 15, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(14f, -56f), new Vector2(-120f, -8f), ParchmentTextColor);
        Button button = CreateActionButton(row, label, action);
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(96f, 40f);
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);
    }

    private void CreateConsumableSlotSelectionRow(InventoryItemStack stack, float top)
    {
        RectTransform row = CreateSlotSelectionRow(stack.Item.itemName, top, 82f);
        CreateSlotSelectionIcon(row, stack.Item);
        CreateText(row, "<b>" + JapaneseDisplayText.GetItemName(stack.Item) + "</b>  所持 " + stack.Amount + "\n" + ItemPresentationService.BuildDetailText(stack.Item), 14, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(76f, -72f), new Vector2(-112f, -8f), ParchmentTextColor);
        Button button = CreateActionButton(row, "設定", () =>
        {
            characterEquipmentController.LoadConsumable(selectedConsumableSlotIndex, stack.Item);
            HideEquipmentSlotSelection();
        });
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(82f, 40f);
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);
    }

    private void CreateEquipmentSlotSelectionRow(InventoryItemStack stack, float top)
    {
        MercenaryInstance mercenary = characterEquipmentController.SelectedDetailMercenary;
        RectTransform row = CreateSlotSelectionRow(stack.Item.itemName, top, 106f);
        CreateSlotSelectionIcon(row, stack.Item);
        string comparison = CharacterEquipmentController.BuildEquipmentComparisonText(stack.Item, mercenary.GetEquippedItem(selectedEquipmentSlot), mercenary.GetEquippedInstance(selectedEquipmentSlot));
        CreateText(row, "<b>" + JapaneseDisplayText.GetItemName(stack.Item) + "</b>  R" + stack.Item.equipmentRank + "  所持 " + stack.Amount + "\n" + comparison, 14, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(76f, -96f), new Vector2(-112f, -8f), ParchmentTextColor);
        Button button = CreateActionButton(row, "装備", () =>
        {
            characterEquipmentController.EquipSelectedEquipment(stack.Item);
            HideEquipmentSlotSelection();
        });
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(82f, 40f);
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);
    }

    private void CreateEquipmentInstanceSlotSelectionRow(EquipmentInstance equipment, float top)
    {
        MercenaryInstance mercenary = characterEquipmentController.SelectedDetailMercenary;
        RectTransform row = CreateSlotSelectionRow(equipment.InstanceId, top, 106f);
        CreateSlotSelectionIcon(row, equipment.BaseItem);
        string comparison = CharacterEquipmentController.BuildEquipmentInstanceComparisonText(equipment, mercenary.GetEquippedInstance(selectedEquipmentSlot), mercenary.GetEquippedItem(selectedEquipmentSlot));
        CreateText(row, "<b>[" + JapaneseDisplayText.GetEquipmentQuality(equipment.Quality) + "] " + CharacterEquipmentController.GetEquipmentDisplayName(equipment) + "</b>\n" + comparison, 14, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(76f, -96f), new Vector2(-112f, -8f), CharacterEquipmentController.GetEquipmentQualityColor(equipment.Quality));
        Button button = CreateActionButton(row, "装備", () =>
        {
            characterEquipmentController.EquipSelectedEquipment(equipment);
            HideEquipmentSlotSelection();
        });
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(82f, 40f);
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);
    }

    private RectTransform CreateSlotSelectionRow(string name, float top, float height)
    {
        RectTransform row = CreateUIObject(name, equipmentSlotSelectionContent);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - height);
        row.offsetMax = new Vector2(0f, top);
        Image image = row.gameObject.AddComponent<Image>();
        image.color = RowColor;
        return row;
    }

    private void CreateSlotSelectionIcon(RectTransform row, ItemDataSO item)
    {
        RectTransform iconRect = CreateUIObject("Item Icon", row);
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.sizeDelta = new Vector2(52f, 52f);
        iconRect.anchoredPosition = new Vector2(14f, 0f);
        Image image = iconRect.gameObject.AddComponent<Image>();
        Sprite sprite = ItemPresentationService.ResolveSprite(item);
        image.sprite = sprite;
        image.color = sprite != null ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
    }

    private void HideEquipmentSlotSelection()
    {
        equipmentSlotSelectionOverlay?.gameObject.SetActive(false);
        selectedConsumableSlotIndex = -1;
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
            else
            {
                CreateEmptyEquipmentSlotRow(slot, top);
                top -= 76f;
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
            "選択",
            () => ShowConsumableSlotSelection(slotIndex));
        RectTransform selectRect = button.GetComponent<RectTransform>();
        selectRect.sizeDelta = new Vector2(76f, 40f);
        selectRect.anchoredPosition = new Vector2(-88f, 0f);
        Button unloadButton = CreateActionButton(
            row,
            "取り外し",
            () => characterEquipmentController.UnloadConsumable(slotIndex));
        unloadButton.interactable = !slot.IsEmpty;
        RectTransform unloadRect = unloadButton.GetComponent<RectTransform>();
        unloadRect.sizeDelta = new Vector2(76f, 40f);
        unloadRect.anchoredPosition = new Vector2(-8f, 0f);
    }

    private void CreateEmptyEquipmentSlotRow(EquipmentSlot slot, float top)
    {
        RectTransform row = CreateUIObject(
            "Empty " + slot + " Slot",
            characterEquipmentList);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 66f);
        row.offsetMax = new Vector2(0f, top);
        row.gameObject.AddComponent<Image>().color = RowColor;
        CreateText(
            row,
            "<b>" + JapaneseDisplayText.GetEquipmentSlot(slot) + "</b>  未装備",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(12f, -56f),
            new Vector2(-96f, -10f),
            Color.white);
        Button button = CreateActionButton(
            row,
            "選択",
            () => ShowEquipmentSlotSelection(slot));
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(76f, 40f);
        rect.anchoredPosition = new Vector2(-8f, 0f);
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
        if (isEquipped)
        {
            Button selectButton = CreateActionButton(
                row,
                "選択",
                () => ShowEquipmentSlotSelection(item.equipmentSlot));
            RectTransform selectRect = selectButton.GetComponent<RectTransform>();
            selectRect.sizeDelta = new Vector2(76f, 40f);
            selectRect.anchoredPosition = new Vector2(-88f, 0f);
        }
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
        if (isEquipped)
        {
            Button selectButton = CreateActionButton(
                row,
                "選択",
                () => ShowEquipmentSlotSelection(item.equipmentSlot));
            RectTransform selectRect = selectButton.GetComponent<RectTransform>();
            selectRect.sizeDelta = new Vector2(64f, 40f);
            selectRect.anchoredPosition = new Vector2(-160f, 0f);
        }
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
        List<ItemDataSO> equipmentItems = new List<ItemDataSO>();
        List<BookPageUI.Entry> entries = new List<BookPageUI.Entry>();
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item == null || !item.IsEquipment)
            {
                continue;
            }

            equipmentItems.Add(item);
        }

        EquipmentCodexEntries codexEntries = EquipmentCodexEntryBuilder.Build(equipmentItems);
        List<ItemDataSO> normalEquipment =
            new List<ItemDataSO>(codexEntries.NormalEquipment);
        normalEquipment.Sort((left, right) =>
        {
            int rankComparison = left.equipmentRank.CompareTo(right.equipmentRank);
            if (rankComparison != 0)
            {
                return rankComparison;
            }
            int slotComparison = left.equipmentSlot.CompareTo(right.equipmentSlot);
            if (slotComparison != 0)
            {
                return slotComparison;
            }
            int nameComparison = string.Compare(
                JapaneseDisplayText.GetItemName(left),
                JapaneseDisplayText.GetItemName(right),
                System.StringComparison.Ordinal);
            return nameComparison != 0
                ? nameComparison
                : string.Compare(
                    left.name,
                    right.name,
                    System.StringComparison.Ordinal);
        });
        foreach (ItemDataSO item in normalEquipment)
        {
            bool discovered = IsEquipmentDiscovered(item);
            entries.Add(new BookPageUI.Entry
            {
                Name = JapaneseDisplayText.GetItemName(item),
                Subtitle = EquipmentRankPresentation.GetRichText(item),
                Detail = BuildEquipmentCodexDetail(item),
                Sprite = ItemPresentationService.ResolveSprite(item),
                Discovered = discovered
            });
        }

        equipmentCodexBook.SetEntries(entries);
        equipmentSpecialCodexPage.SetPages(EquipmentSpecialPageModelBuilder.Build(codexEntries, IsEquipmentDiscovered));
        ShowNormalEquipmentCodexTab();
        equipmentCollectionOverlay.SetAsLastSibling();
        equipmentCollectionOverlay.gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    private void BuildEquipmentCodexDebugButtons(RectTransform window)
    {
        CreateText(
            window, "[DEBUG] 発見状況", 12, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(20f, 54f), new Vector2(140f, 76f),
            ParchmentMutedColor);
        Button allButton = CreateActionButton(
            window,
            "全て発見",
            DiscoverAllEquipmentForEditor);
        SetEquipmentCodexDebugButtonPosition(allButton, 150f);
        Button partialButton = CreateActionButton(
            window,
            "一部発見",
            DiscoverPartialEquipmentForEditor);
        SetEquipmentCodexDebugButtonPosition(partialButton, 265f);
        Button resetButton = CreateActionButton(
            window,
            "発見をリセット",
            ResetEquipmentDiscoveryForEditor);
        SetEquipmentCodexDebugButtonPosition(resetButton, 380f);
    }

    private static void SetEquipmentCodexDebugButtonPosition(
        Button button,
        float x)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(105f, 28f);
        rect.anchoredPosition = new Vector2(x, 18f);
    }

    private void DiscoverAllEquipmentForEditor()
    {
        if (merchantInventory == null)
        {
            return;
        }
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            merchantInventory.RegisterEquipmentDiscovery(item);
        }
        RefreshEquipmentCollectionAfterEditorDiscoveryChange();
    }

    private void DiscoverPartialEquipmentForEditor()
    {
        if (merchantInventory == null)
        {
            return;
        }
        merchantInventory.ClearEquipmentDiscoveryForEditor();
        List<ItemDataSO> equipment = new List<ItemDataSO>();
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item != null && item.IsEquipment)
            {
                equipment.Add(item);
            }
        }
        EquipmentCodexEntries entries = EquipmentCodexEntryBuilder.Build(equipment);
        for (int index = 0; index < entries.SetGroups.Count; index++)
        {
            EquipmentCodexSetGroup group = entries.SetGroups[index];
            if (index % 3 != 0 && group.Equipment.Count > 0)
            {
                merchantInventory.RegisterEquipmentDiscovery(group.Equipment[0]);
            }
        }
        for (int index = 0; index < entries.HighRankSingleEquipment.Count; index++)
        {
            if (index % 2 == 0)
            {
                merchantInventory.RegisterEquipmentDiscovery(
                    entries.HighRankSingleEquipment[index]);
            }
        }
        RefreshEquipmentCollectionAfterEditorDiscoveryChange();
    }

    private void ResetEquipmentDiscoveryForEditor()
    {
        if (merchantInventory == null)
        {
            return;
        }
        merchantInventory.ClearEquipmentDiscoveryForEditor();
        RefreshEquipmentCollectionAfterEditorDiscoveryChange();
    }

    private void RefreshEquipmentCollectionAfterEditorDiscoveryChange()
    {
        bool showSpecial = equipmentCodexSpecialRoot != null &&
            equipmentCodexSpecialRoot.gameObject.activeSelf;
        ShowEquipmentCollection();
        if (showSpecial)
        {
            ShowSpecialEquipmentCodexTab();
        }
    }
#endif

    private static string BuildEquipmentCodexDetail(ItemDataSO item)
    {
        string target = item.allClassesCanEquip
            ? "全職業"
            : JapaneseDisplayText.GetMercenaryClass(item.requiredClass);
        string effectText = ShortenEquipmentCodexText(
            EquipmentEffectTextFormatter.FormatList(item.equipmentEffects),
            30);
        return string.Format(
            "{0} / {1}\nHP {2:+#;-#;0}  攻 {3:+#;-#;0}\n防 {4:+#;-#;0}  速 {5:+0.##;-0.##;0}\n価格 {6}G\n効果: {7}",
            JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot),
            target,
            item.bonusMaxHP,
            item.bonusAttack,
            item.bonusDefense,
            item.bonusAttackSpeed,
            item.basePrice,
            effectText);
    }

    private static string ShortenEquipmentCodexText(
        string value,
        int maximumLength)
    {
        string normalized = string.IsNullOrWhiteSpace(value)
            ? "なし"
            : value.Replace("\r\n", "、").Replace("\n", "、");
        return normalized.Length <= maximumLength
            ? normalized
            : normalized.Substring(0, maximumLength - 1) + "…";
    }

    private void HideEquipmentCollection()
    {
        equipmentCollectionOverlay?.gameObject.SetActive(false);
    }

    private bool IsEquipmentDiscovered(ItemDataSO item)
    {
        return merchantInventory != null && merchantInventory.HasDiscoveredEquipment(item);
    }

    private void ShowNormalEquipmentCodexTab()
    {
        if (equipmentCodexNormalRoot == null || equipmentCodexSpecialRoot == null)
        {
            return;
        }
        equipmentCodexNormalRoot.gameObject.SetActive(true);
        equipmentCodexSpecialRoot.gameObject.SetActive(false);
        equipmentCodexNormalTabButton.targetGraphic.color = ImportantButtonColor;
        equipmentCodexSpecialTabButton.targetGraphic.color = WoodButtonColor;
    }

    private void ShowSpecialEquipmentCodexTab()
    {
        if (equipmentCodexNormalRoot == null || equipmentCodexSpecialRoot == null)
        {
            return;
        }
        equipmentCodexNormalRoot.gameObject.SetActive(false);
        equipmentCodexSpecialRoot.gameObject.SetActive(true);
        equipmentCodexNormalTabButton.targetGraphic.color = WoodButtonColor;
        equipmentCodexSpecialTabButton.targetGraphic.color = ImportantButtonColor;
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
