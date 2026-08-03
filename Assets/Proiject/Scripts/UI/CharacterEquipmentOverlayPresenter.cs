using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CharacterEquipmentOverlayPresenter
{
    private static readonly Color RowColor = UITheme.RowColor;
    private static readonly Color AccentColor = UITheme.AccentColor;
    private static readonly Color WoodButtonColor = UITheme.WoodButtonColor;
    private static readonly Color ImportantButtonColor = UITheme.ImportantButtonColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.CharacterDetailReferences characterDetail;
    private readonly SimpleMercenaryHireUIView.EquipmentDetailReferences equipmentDetail;
    private readonly SimpleMercenaryHireUIView.EquipmentSlotSelectionReferences slotSelection;
    private readonly SimpleMercenaryHireUIView.EquipmentCodexReferences equipmentCodex;
    /// <summary>
    /// overlayRoot は BuildUI() で確定するため、本Presenterの生成時点では
    /// まだ null である。値ではなく解決用のデリゲートを保持し、実際に
    /// オーバーレイを作る時点で解決する。
    /// </summary>
    private readonly Func<Transform> overlayRootProvider;
    private readonly MerchantInventory merchantInventory;
    private readonly CharacterEquipmentController characterEquipmentController;
    private readonly Font uiFont;
    private readonly Font uiBodyFont;
    private readonly Func<SimpleMercenaryHireOverlaySlot, string, RectTransform>
        getOrCreateOverlay;

    public CharacterEquipmentOverlayPresenter(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.CharacterDetailReferences characterDetail,
        SimpleMercenaryHireUIView.EquipmentDetailReferences equipmentDetail,
        SimpleMercenaryHireUIView.EquipmentSlotSelectionReferences slotSelection,
        SimpleMercenaryHireUIView.EquipmentCodexReferences equipmentCodex,
        Func<Transform> overlayRootProvider,
        MerchantInventory merchantInventory,
        CharacterEquipmentController characterEquipmentController,
        Font uiFont,
        Font uiBodyFont,
        Func<SimpleMercenaryHireOverlaySlot, string, RectTransform>
            getOrCreateOverlay)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.characterDetail = characterDetail ??
            throw new ArgumentNullException(nameof(characterDetail));
        this.equipmentDetail = equipmentDetail ??
            throw new ArgumentNullException(nameof(equipmentDetail));
        this.slotSelection = slotSelection ??
            throw new ArgumentNullException(nameof(slotSelection));
        this.equipmentCodex = equipmentCodex ??
            throw new ArgumentNullException(nameof(equipmentCodex));
        this.overlayRootProvider = overlayRootProvider ??
            throw new ArgumentNullException(nameof(overlayRootProvider));
        // この2つは以降で無条件に参照するため必須依存として扱う。
        // Font は未設定でも Unity 既定にフォールバックするので許容する。
        // MerchantInventory は MonoBehaviour で、未設定・破棄済みの参照は
        // C# 上は非 null な "fake null" になり得る。?? では素通りするため
        // Unity の == null 判定で検査する。
        if (merchantInventory == null)
        {
            throw new ArgumentNullException(nameof(merchantInventory));
        }

        this.merchantInventory = merchantInventory;
        this.characterEquipmentController = characterEquipmentController ??
            throw new ArgumentNullException(nameof(characterEquipmentController));
        this.uiFont = uiFont;
        this.uiBodyFont = uiBodyFont;
        this.getOrCreateOverlay = getOrCreateOverlay ??
            throw new ArgumentNullException(nameof(getOrCreateOverlay));
    }

    /// <summary>
    /// オーバーレイの親を解決する。overlayRoot は BuildUI() の完了まで
    /// 確定しないため、未確定のまま使うと null を親にした UI をシーン直下へ
    /// 作ってしまう。原因の分かる例外にして早期に気付けるようにする。
    /// </summary>
    private Transform ResolveOverlayRoot()
    {
        Transform root = overlayRootProvider();
        if (root == null)
        {
            throw new InvalidOperationException(
                "装備オーバーレイの生成には BuildUI() の完了が必要です。");
        }

        return root;
    }

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

    private RectTransform CreateRow(string rowName, RectTransform parent, float top) =>
        factory.CreateRow(rowName, parent, top);

    private Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action) =>
        factory.CreateActionButton(parent, label, action);

    private static RectTransform CreateUIObject(string objectName, Transform parent) =>
        SimpleMercenaryHireUIFactory.CreateUIObject(objectName, parent);

    private static void ApplyParchmentPanel(Image target) =>
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(target);

    private static void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
            child = null;
        }
    }

    private static RectTransform CreateScrollableContent(
        RectTransform parent,
        string viewportName,
        string contentName,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        RectTransform viewport = CreateUIObject(viewportName, parent);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = offsetMin;
        viewport.offsetMax = offsetMax;
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform content = CreateUIObject(contentName, viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        return content;
    }
    public void BuildEquipmentDetailOverlay()
    {
        equipmentDetail.overlay = getOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.EquipmentDetail,
            "Equipment Detail Overlay");
        equipmentDetail.overlay.gameObject.SetActive(false);
        equipmentDetail.overlay.anchorMin = Vector2.zero;
        equipmentDetail.overlay.anchorMax = Vector2.one;
        equipmentDetail.overlay.offsetMin = Vector2.zero;
        equipmentDetail.overlay.offsetMax = Vector2.zero;
        equipmentDetail.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.78f);

        RectTransform window = CreateUIObject("Equipment Detail Window", equipmentDetail.overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(600f, 470f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        equipmentDetail.title = CreateText(
            window, string.Empty, 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -66f), new Vector2(-28f, -20f),
            ParchmentTextColor);
        equipmentDetail.bodyText = CreateText(
            window, string.Empty, 17, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(28f, 92f), new Vector2(-28f, -82f),
            ParchmentTextColor);
        equipmentDetail.bodyText.rectTransform.anchorMin = Vector2.zero;
        equipmentDetail.bodyText.rectTransform.anchorMax = Vector2.one;
        equipmentDetail.bodyText.rectTransform.offsetMin = new Vector2(28f, 92f);
        equipmentDetail.bodyText.rectTransform.offsetMax = new Vector2(-28f, -82f);

        equipmentDetail.enhanceButton = CreateActionButton(
            window,
            "強化",
            () => characterEquipmentController.EnhanceSelectedEquipment());
        RectTransform enhanceRect = equipmentDetail.enhanceButton.GetComponent<RectTransform>();
        enhanceRect.anchorMin = enhanceRect.anchorMax = new Vector2(1f, 0f);
        enhanceRect.pivot = new Vector2(1f, 0f);
        enhanceRect.anchoredPosition = new Vector2(-174f, 24f);

        equipmentDetail.sellButton = CreateActionButton(
            window,
            "売却",
            () => characterEquipmentController.SellSelectedEquipment());
        equipmentDetail.sellButton.targetGraphic.color = ImportantButtonColor;
        RectTransform sellRect = equipmentDetail.sellButton.GetComponent<RectTransform>();
        sellRect.anchorMin = sellRect.anchorMax = new Vector2(1f, 0f);
        sellRect.pivot = new Vector2(1f, 0f);
        sellRect.anchoredPosition = new Vector2(-28f, 24f);

        equipmentDetail.lockButton = CreateActionButton(
            window,
            "ロック",
            () => characterEquipmentController.ToggleSelectedEquipmentLock());
        RectTransform lockRect = equipmentDetail.lockButton.GetComponent<RectTransform>();
        lockRect.anchorMin = lockRect.anchorMax = new Vector2(0f, 0f);
        lockRect.pivot = new Vector2(0f, 0f);
        lockRect.anchoredPosition = new Vector2(28f, 24f);

        Button closeButton = CreateActionButton(window, "閉じる", HideEquipmentDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        equipmentDetail.overlay.gameObject.SetActive(false);
    }

    public void BuildEquipmentCollectionOverlay()
    {
        equipmentCodex.overlay =
            getOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.EquipmentCollection,
                "Equipment Collection Overlay");
        equipmentCodex.overlay.gameObject.SetActive(false);
        equipmentCodex.overlay.anchorMin = Vector2.zero;
        equipmentCodex.overlay.anchorMax = Vector2.one;
        equipmentCodex.overlay.offsetMin = Vector2.zero;
        equipmentCodex.overlay.offsetMax = Vector2.zero;
        equipmentCodex.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Equipment Collection Window", equipmentCodex.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(820f, 600f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window, "装備図鑑", 26, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -64f), new Vector2(-120f, -20f),
            ParchmentTextColor);

        equipmentCodex.normalTabButton = CreateActionButton(window, "通常装備", ShowNormalEquipmentCodexTab);
        RectTransform normalTabRect = equipmentCodex.normalTabButton.GetComponent<RectTransform>();
        normalTabRect.anchorMin = normalTabRect.anchorMax = new Vector2(0f, 1f);
        normalTabRect.pivot = new Vector2(0f, 1f);
        normalTabRect.anchoredPosition = new Vector2(250f, -20f);
        equipmentCodex.specialTabButton = CreateActionButton(window, "特殊装備", ShowSpecialEquipmentCodexTab);
        RectTransform specialTabRect = equipmentCodex.specialTabButton.GetComponent<RectTransform>();
        specialTabRect.anchorMin = specialTabRect.anchorMax = new Vector2(0f, 1f);
        specialTabRect.pivot = new Vector2(0f, 1f);
        specialTabRect.anchoredPosition = new Vector2(380f, -20f);

        equipmentCodex.normalRoot = CreateUIObject("Equipment Codex Normal Book", window);
        equipmentCodex.normalRoot.anchorMin = Vector2.zero;
        equipmentCodex.normalRoot.anchorMax = Vector2.one;
        equipmentCodex.normalRoot.offsetMin = new Vector2(34f, 34f);
        equipmentCodex.normalRoot.offsetMax = new Vector2(-34f, -88f);
        equipmentCodex.book = equipmentCodex.normalRoot.gameObject.AddComponent<BookPageUI>();
        equipmentCodex.book.Initialize(string.Empty, uiFont, uiBodyFont);
        equipmentCodex.specialRoot = CreateUIObject("Equipment Codex Special Pages", window);
        equipmentCodex.specialRoot.anchorMin = Vector2.zero;
        equipmentCodex.specialRoot.anchorMax = Vector2.one;
        equipmentCodex.specialRoot.offsetMin = new Vector2(34f, 34f);
        equipmentCodex.specialRoot.offsetMax = new Vector2(-34f, -88f);
        equipmentCodex.specialPage = equipmentCodex.specialRoot.gameObject.AddComponent<EquipmentSpecialCodexPageUI>();
        equipmentCodex.specialPage.Initialize(uiFont, uiBodyFont);
#if UNITY_EDITOR
        equipmentCodex.normalRoot.offsetMin = new Vector2(34f, 76f);
        equipmentCodex.specialRoot.offsetMin = new Vector2(34f, 76f);
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
        equipmentCodex.overlay.gameObject.SetActive(false);
    }

    public void BuildCharacterDetailOverlay()
    {
        characterDetail.overlay = getOrCreateOverlay(
            SimpleMercenaryHireOverlaySlot.CharacterDetail,
            "Character Detail Overlay");
        characterDetail.overlay.gameObject.SetActive(false);
        characterDetail.overlay.anchorMin = Vector2.zero;
        characterDetail.overlay.anchorMax = Vector2.one;
        characterDetail.overlay.offsetMin = Vector2.zero;
        characterDetail.overlay.offsetMax = Vector2.zero;

        Image overlayImage = characterDetail.overlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.78f);

        RectTransform window = CreateUIObject("Character Detail Window", characterDetail.overlay);
        window.anchorMin = new Vector2(0.5f, 0.5f);
        window.anchorMax = new Vector2(0.5f, 0.5f);
        window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(780f, 540f);
        window.anchoredPosition = Vector2.zero;

        Image windowImage = window.gameObject.AddComponent<Image>();
        ApplyParchmentPanel(windowImage);
        characterDetail.title = CreateText(
            window,
            string.Empty,
            26,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -64f),
            new Vector2(-120f, -20f),
            ParchmentTextColor);

        characterDetail.statusTabButton =
            CreateActionButton(window, "ステータス", ShowCharacterStatusPage);
        RectTransform statusTabRect =
            characterDetail.statusTabButton.GetComponent<RectTransform>();
        statusTabRect.anchorMin = statusTabRect.anchorMax =
            new Vector2(0f, 1f);
        statusTabRect.pivot = new Vector2(0f, 1f);
        statusTabRect.sizeDelta = new Vector2(130f, 38f);
        statusTabRect.anchoredPosition = new Vector2(28f, -76f);

        characterDetail.equipmentTabButton =
            CreateActionButton(window, "装備", ShowCharacterEquipmentPage);
        RectTransform equipmentTabRect =
            characterDetail.equipmentTabButton.GetComponent<RectTransform>();
        equipmentTabRect.anchorMin = equipmentTabRect.anchorMax =
            new Vector2(0f, 1f);
        equipmentTabRect.pivot = new Vector2(0f, 1f);
        equipmentTabRect.sizeDelta = new Vector2(130f, 38f);
        equipmentTabRect.anchoredPosition = new Vector2(166f, -76f);

        characterDetail.statusPage = CreateUIObject("Character Status Page", window);
        characterDetail.statusPage.anchorMin = Vector2.zero;
        characterDetail.statusPage.anchorMax = Vector2.one;
        characterDetail.statusPage.offsetMin = new Vector2(28f, 28f);
        characterDetail.statusPage.offsetMax = new Vector2(-28f, -122f);

        characterDetail.statusText = CreateText(
            characterDetail.statusPage,
            string.Empty,
            16,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, 0f),
            new Vector2(-386f, 0f),
            ParchmentTextColor);
        characterDetail.statusText.rectTransform.anchorMin = Vector2.zero;
        characterDetail.statusText.rectTransform.anchorMax = Vector2.one;
        characterDetail.statusText.rectTransform.offsetMin = Vector2.zero;
        characterDetail.statusText.rectTransform.offsetMax = new Vector2(-386f, 0f);

        CreateText(
            characterDetail.statusPage,
            "獲得スキル",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(360f, -36f),
            new Vector2(0f, 0f),
            ParchmentTextColor);

        RectTransform skillViewport =
            CreateUIObject("Skill Viewport", characterDetail.statusPage);
        skillViewport.anchorMin = new Vector2(1f, 1f);
        skillViewport.anchorMax = new Vector2(1f, 1f);
        skillViewport.pivot = new Vector2(1f, 1f);
        skillViewport.sizeDelta = new Vector2(336f, 184f);
        skillViewport.anchoredPosition = new Vector2(0f, -46f);
        skillViewport.gameObject.AddComponent<Image>().color =
            new Color(0.28f, 0.16f, 0.07f, 0.12f);
        Mask skillMask = skillViewport.gameObject.AddComponent<Mask>();
        skillMask.showMaskGraphic = false;

        characterDetail.skillList = CreateUIObject("Skill List", skillViewport);
        characterDetail.skillList.anchorMin = new Vector2(0f, 1f);
        characterDetail.skillList.anchorMax = new Vector2(1f, 1f);
        characterDetail.skillList.pivot = new Vector2(0.5f, 1f);
        characterDetail.skillList.anchoredPosition = Vector2.zero;

        ScrollRect skillScroll = skillViewport.gameObject.AddComponent<ScrollRect>();
        skillScroll.content = characterDetail.skillList;
        skillScroll.viewport = skillViewport;
        skillScroll.horizontal = false;
        skillScroll.vertical = true;
        skillScroll.movementType = ScrollRect.MovementType.Clamped;
        skillScroll.scrollSensitivity = 24f;

        characterDetail.skillDetailText = CreateText(
            characterDetail.statusPage,
            "スキルを選択すると詳細を表示します。",
            15,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(360f, 0f),
            new Vector2(0f, -250f),
            ParchmentMutedColor);
        characterDetail.skillDetailText.rectTransform.anchorMin =
            new Vector2(1f, 0f);
        characterDetail.skillDetailText.rectTransform.anchorMax =
            new Vector2(1f, 0f);
        characterDetail.skillDetailText.rectTransform.pivot = new Vector2(1f, 0f);
        characterDetail.skillDetailText.rectTransform.sizeDelta = new Vector2(336f, 146f);
        characterDetail.skillDetailText.rectTransform.anchoredPosition = Vector2.zero;

        characterDetail.equipmentPage =
            CreateUIObject("Character Equipment Page", window);
        characterDetail.equipmentPage.anchorMin = Vector2.zero;
        characterDetail.equipmentPage.anchorMax = Vector2.one;
        characterDetail.equipmentPage.offsetMin = new Vector2(28f, 28f);
        characterDetail.equipmentPage.offsetMax = new Vector2(-28f, -122f);

        CreateText(
            characterDetail.equipmentPage,
            "装備変更",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -36f),
            new Vector2(0f, 0f),
            ParchmentTextColor);

        RectTransform equipmentViewport =
            CreateUIObject("Equipment Viewport", characterDetail.equipmentPage);
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

        characterDetail.equipmentList =
            CreateUIObject("Equipment Scroll Content", equipmentViewport);
        characterDetail.equipmentList.anchorMin = new Vector2(0f, 1f);
        characterDetail.equipmentList.anchorMax = new Vector2(1f, 1f);
        characterDetail.equipmentList.pivot = new Vector2(0.5f, 1f);
        characterDetail.equipmentList.anchoredPosition = Vector2.zero;

        characterDetail.equipmentScrollRect =
            equipmentViewport.gameObject.AddComponent<ScrollRect>();
        characterDetail.equipmentScrollRect.content = characterDetail.equipmentList;
        characterDetail.equipmentScrollRect.viewport = equipmentViewport;
        characterDetail.equipmentScrollRect.horizontal = false;
        characterDetail.equipmentScrollRect.vertical = true;
        characterDetail.equipmentScrollRect.movementType = ScrollRect.MovementType.Clamped;
        characterDetail.equipmentScrollRect.scrollSensitivity = 30f;

        Button closeButton = CreateActionButton(window, "閉じる", HideCharacterDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);

        characterDetail.overlay.gameObject.SetActive(false);
    }

    public void ShowCharacterDetails(MercenaryInstance mercenary)
    {
        if (mercenary == null || characterDetail.overlay == null)
        {
            return;
        }

        bool keepCurrentDetailPage =
            characterDetail.overlay.gameObject.activeSelf &&
            ReferenceEquals(
                characterEquipmentController.SelectedDetailMercenary,
                mercenary);
        characterEquipmentController.SelectedDetailMercenary = mercenary;
        if (!keepCurrentDetailPage)
        {
            characterDetail.showingStatusPage = true;
        }
        characterEquipmentController.RefreshCharacterDetailText();
        RebuildCharacterSkillList();
        RebuildCharacterEquipmentList();
        ApplyCharacterDetailPageVisibility();
        characterDetail.overlay.SetAsLastSibling();
        characterDetail.overlay.gameObject.SetActive(true);
    }

    private void ShowCharacterStatusPage()
    {
        characterDetail.showingStatusPage = true;
        ApplyCharacterDetailPageVisibility();
    }

    private void ShowCharacterEquipmentPage()
    {
        characterDetail.showingStatusPage = false;
        ApplyCharacterDetailPageVisibility();
    }

    public void BuildEquipmentSlotSelectionOverlay()
    {
        slotSelection.overlay = CreateUIObject(
            "Equipment Slot Selection Overlay",
            ResolveOverlayRoot());
        slotSelection.overlay.anchorMin = Vector2.zero;
        slotSelection.overlay.anchorMax = Vector2.one;
        slotSelection.overlay.offsetMin = Vector2.zero;
        slotSelection.overlay.offsetMax = Vector2.zero;
        slotSelection.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject(
            "Equipment Slot Selection Window",
            slotSelection.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 600f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        slotSelection.title = CreateText(
            window,
            string.Empty,
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(28f, -66f),
            new Vector2(-28f, -20f),
            ParchmentTextColor);
        slotSelection.content = CreateScrollableContent(
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
        slotSelection.overlay.gameObject.SetActive(false);
    }

    private void ShowEquipmentSlotSelection(EquipmentSlot slot)
    {
        slotSelection.selectedSlot = slot;
        slotSelection.selectedConsumableSlotIndex = -1;
        slotSelection.title.text =
            JapaneseDisplayText.GetEquipmentSlot(slot) + "を選択";
        RebuildEquipmentSlotSelection();
        slotSelection.overlay.SetAsLastSibling();
        slotSelection.overlay.gameObject.SetActive(true);
    }

    private void ShowConsumableSlotSelection(int slotIndex)
    {
        slotSelection.selectedConsumableSlotIndex = slotIndex;
        slotSelection.title.text =
            "消耗品スロット " + (slotIndex + 1) + " を選択";
        RebuildEquipmentSlotSelection();
        slotSelection.overlay.SetAsLastSibling();
        slotSelection.overlay.gameObject.SetActive(true);
    }

    private void RebuildEquipmentSlotSelection()
    {
        ClearChildren(slotSelection.content);
        MercenaryInstance mercenary = characterEquipmentController.SelectedDetailMercenary;
        if (mercenary == null)
        {
            return;
        }
        float top = 0f;
        if (slotSelection.selectedConsumableSlotIndex >= 0)
        {
            CreateSlotSelectionActionRow("取り外す", "このスロットの消耗品を倉庫へ戻します。", top, () =>
            {
                characterEquipmentController.UnloadConsumable(slotSelection.selectedConsumableSlotIndex);
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
                characterEquipmentController.UnequipSelectedEquipment(slotSelection.selectedSlot);
                HideEquipmentSlotSelection();
            });
            top -= 76f;
            foreach (EquipmentInstance equipment in merchantInventory.EquipmentInstances)
            {
                if (equipment?.BaseItem == null ||
                    equipment.BaseItem.equipmentSlot != slotSelection.selectedSlot ||
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
                    stack.Item.equipmentSlot != slotSelection.selectedSlot ||
                    !stack.Item.CanEquip(mercenary.MercenaryClass))
                {
                    continue;
                }
                CreateEquipmentSlotSelectionRow(stack, top);
                top -= 116f;
            }
        }
        if (top == (slotSelection.selectedConsumableSlotIndex >= 0 ? -76f : -76f))
        {
            CreateText(slotSelection.content, "選択できる所持品はありません。", 16, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(12f, -118f), new Vector2(-12f, -70f), MutedTextColor);
        }
        slotSelection.content.sizeDelta =
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
            characterEquipmentController.LoadConsumable(slotSelection.selectedConsumableSlotIndex, stack.Item);
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
        string comparison = CharacterEquipmentController.BuildEquipmentComparisonText(stack.Item, mercenary.GetEquippedItem(slotSelection.selectedSlot), mercenary.GetEquippedInstance(slotSelection.selectedSlot));
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
        string comparison = CharacterEquipmentController.BuildEquipmentInstanceComparisonText(equipment, mercenary.GetEquippedInstance(slotSelection.selectedSlot), mercenary.GetEquippedItem(slotSelection.selectedSlot));
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
        RectTransform row = CreateUIObject(name, slotSelection.content);
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
        slotSelection.overlay?.gameObject.SetActive(false);
        slotSelection.selectedConsumableSlotIndex = -1;
    }

    private void ApplyCharacterDetailPageVisibility()
    {
        if (characterDetail.statusPage != null)
        {
            characterDetail.statusPage.gameObject.SetActive(characterDetail.showingStatusPage);
        }

        if (characterDetail.equipmentPage != null)
        {
            characterDetail.equipmentPage.gameObject.SetActive(!characterDetail.showingStatusPage);
        }

        if (characterDetail.statusTabButton != null)
        {
            characterDetail.statusTabButton.targetGraphic.color =
                characterDetail.showingStatusPage ? AccentColor : RowColor;
        }

        if (characterDetail.equipmentTabButton != null)
        {
            characterDetail.equipmentTabButton.targetGraphic.color =
                characterDetail.showingStatusPage ? RowColor : AccentColor;
        }
    }

    private void HideCharacterDetails()
    {
        if (characterDetail.overlay != null)
        {
            characterDetail.overlay.gameObject.SetActive(false);
        }

        characterEquipmentController.SelectedDetailMercenary = null;
    }

    private void RebuildCharacterSkillList()
    {
        if (characterDetail.skillList == null ||
            characterEquipmentController.SelectedDetailMercenary == null)
        {
            return;
        }

        ClearChildren(characterDetail.skillList);
        List<MercenarySkillInfo> skills =
            CharacterEquipmentController.GetMercenarySkillInfos(
                characterEquipmentController.SelectedDetailMercenary);
        float top = 0f;
        for (int i = 0; i < skills.Count; i++)
        {
            MercenarySkillInfo skill = skills[i];
            RectTransform row = CreateRow($"Skill {skill.Name}", characterDetail.skillList, top);
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

        characterDetail.skillList.sizeDelta = new Vector2(0f, Mathf.Max(184f, -top));
        if (skills.Count > 0)
        {
            ShowMercenarySkillDetail(skills[0]);
        }
        else if (characterDetail.skillDetailText != null)
        {
            characterDetail.skillDetailText.text = "獲得済みスキルはありません。";
        }
    }

    private void ShowMercenarySkillDetail(MercenarySkillInfo skill)
    {
        if (characterDetail.skillDetailText == null)
        {
            return;
        }

        string state = skill.Unlocked ? "習得済み" : "未習得";
        characterDetail.skillDetailText.text =
            $"{skill.Name}  [{state}]\n\n" +
            $"{skill.DetailDescription}";
    }

    private void RebuildCharacterEquipmentList()
    {
        MercenaryInstance selectedMercenary =
            characterEquipmentController.SelectedDetailMercenary;
        if (characterDetail.equipmentList == null || selectedMercenary == null)
        {
            return;
        }

        ClearChildren(characterDetail.equipmentList);
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
                characterDetail.equipmentList,
                "装備できる武器を所持していません",
                14,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Vector2(0f, -50f),
                new Vector2(0f, 0f),
                MutedTextColor);
        }

        characterDetail.equipmentList.sizeDelta =
            new Vector2(0f, Mathf.Max(398f, -top));
        Canvas.ForceUpdateCanvases();
        if (characterDetail.equipmentScrollRect != null)
        {
            characterDetail.equipmentScrollRect.StopMovement();
            characterDetail.equipmentScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void CreateConsumableSlotRow(int slotIndex, float top)
    {
        MercenaryConsumableSlot slot =
            characterEquipmentController.SelectedDetailMercenary.ConsumableSlots[slotIndex];
        RectTransform row = CreateUIObject(
            $"Consumable Slot {slotIndex + 1}",
            characterDetail.equipmentList);
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
            characterDetail.equipmentList);
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
            characterDetail.equipmentList);
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
            characterDetail.equipmentList);
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
            characterDetail.equipmentList);
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

    private void HideEquipmentDetails()
    {
        equipmentDetail.overlay?.gameObject.SetActive(false);
        characterEquipmentController.SelectedEquipmentDetail = null;
    }

    public void ShowEquipmentCollection()
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

        equipmentCodex.book.SetEntries(entries);
        equipmentCodex.specialPage.SetPages(EquipmentSpecialPageModelBuilder.Build(codexEntries, IsEquipmentDiscovered));
        ShowNormalEquipmentCodexTab();
        equipmentCodex.overlay.SetAsLastSibling();
        equipmentCodex.overlay.gameObject.SetActive(true);
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
        bool showSpecial = equipmentCodex.specialRoot != null &&
            equipmentCodex.specialRoot.gameObject.activeSelf;
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
        equipmentCodex.overlay?.gameObject.SetActive(false);
    }

    private bool IsEquipmentDiscovered(ItemDataSO item)
    {
        return merchantInventory != null && merchantInventory.HasDiscoveredEquipment(item);
    }

    private void ShowNormalEquipmentCodexTab()
    {
        if (equipmentCodex.normalRoot == null || equipmentCodex.specialRoot == null)
        {
            return;
        }
        equipmentCodex.normalRoot.gameObject.SetActive(true);
        equipmentCodex.specialRoot.gameObject.SetActive(false);
        equipmentCodex.normalTabButton.targetGraphic.color = ImportantButtonColor;
        equipmentCodex.specialTabButton.targetGraphic.color = WoodButtonColor;
    }

    private void ShowSpecialEquipmentCodexTab()
    {
        if (equipmentCodex.normalRoot == null || equipmentCodex.specialRoot == null)
        {
            return;
        }
        equipmentCodex.normalRoot.gameObject.SetActive(false);
        equipmentCodex.specialRoot.gameObject.SetActive(true);
        equipmentCodex.normalTabButton.targetGraphic.color = WoodButtonColor;
        equipmentCodex.specialTabButton.targetGraphic.color = ImportantButtonColor;
    }

    public bool HasEquipmentDetailOverlay => equipmentDetail.overlay != null;

    public void SetEquipmentDetailTitle(string title, Color color)
    {
        equipmentDetail.title.text = title;
        equipmentDetail.title.color = color;
    }

    public void SetEquipmentDetailText(string text)
    {
        equipmentDetail.bodyText.text = text;
    }

    public void SetEnhanceButton(bool interactable, string label)
    {
        equipmentDetail.enhanceButton.interactable = interactable;
        equipmentDetail.enhanceButton.GetComponentInChildren<Text>().text = label;
    }

    public void SetSellButton(bool interactable, string label)
    {
        equipmentDetail.sellButton.interactable = interactable;
        equipmentDetail.sellButton.GetComponentInChildren<Text>().text = label;
    }

    public void SetLockButtonLabel(string label)
    {
        equipmentDetail.lockButton.GetComponentInChildren<Text>().text = label;
    }

    public void ShowEquipmentDetailOverlay()
    {
        equipmentDetail.overlay.SetAsLastSibling();
        equipmentDetail.overlay.gameObject.SetActive(true);
    }

    public void HideEquipmentDetailOverlay()
    {
        HideEquipmentDetails();
    }
}
