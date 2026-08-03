using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildHirePage() => hirePartyPresenter.BuildHirePage();

    private void BuildCompanyPage() => hirePartyPresenter.BuildCompanyPage();

    private void BuildPartyPage() => hirePartyPresenter.BuildPartyPage();



    private void BuildJobChangePage()
    {
        Text title = CreateText(
            jobChangePage,
            $"転職神殿（転職可能 Lv{MercenaryClassProgression.PromotionLevel}）",
            17,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(0f, -34f),
            Vector2.zero,
            ParchmentTextColor);

        RectTransform viewport =
            CreateUIObject("Job Change Viewport", jobChangePage);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -48f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        jobChangeList = CreateUIObject("Job Change List", viewport);
        jobChangeList.anchorMin = new Vector2(0f, 1f);
        jobChangeList.anchorMax = new Vector2(1f, 1f);
        jobChangeList.pivot = new Vector2(0.5f, 1f);

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = jobChangeList;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        JobChangePageUI pageUI =
            jobChangePage.GetComponent<JobChangePageUI>() ??
            jobChangePage.gameObject.AddComponent<JobChangePageUI>();
        pageUI.Initialize(title, scroll, jobChangeList);
        pageUI.Configure(
            uiFont,
            ParchmentTextColor,
            MutedTextColor,
            ButtonTextColor,
            RowColor,
            WoodButtonColor,
            FrameColor,
            null,
            17);
        pageUI.ConfigureJobChangeList(
            hireAndPartyController.GetPromotionCandidates,
            hireAndPartyController.ShouldShowSpecialPromotion,
            hireAndPartyController.PromoteMercenary,
            ShowPromotionPreview);
        pageRouter.Register(jobChangePage);
    }

    private void BuildPromotionPreviewOverlay()
    {
        promotionPreview.overlay = CreateUIObject("Promotion Preview Overlay", overlayRoot);
        promotionPreview.overlay.gameObject.SetActive(false);
        promotionPreview.overlay.anchorMin = Vector2.zero;
        promotionPreview.overlay.anchorMax = Vector2.one;
        promotionPreview.overlay.offsetMin = Vector2.zero;
        promotionPreview.overlay.offsetMax = Vector2.zero;
        promotionPreview.overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        RectTransform window = CreateUIObject("Promotion Preview Window", promotionPreview.overlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(700f, 520f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(window, "転職確認", 26, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(28f, -68f), new Vector2(-28f, -18f), ParchmentTextColor);
        promotionPreview.text = CreateText(window, string.Empty, 15, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(34f, -360f), new Vector2(-34f, -82f), ParchmentTextColor);
        promotionPreview.reasonText = CreateText(window, string.Empty, 14, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(34f, -414f), new Vector2(-34f, -362f), MutedTextColor);
        promotionPreview.confirmButton = CreateActionButton(window, "転職する", ConfirmPromotionPreview);
        RectTransform confirmRect = promotionPreview.confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot = new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 25f);
        Button cancel = CreateActionButton(window, "キャンセル", HidePromotionPreview);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 25f);
    }

    private void ShowPromotionPreview(MercenaryInstance mercenary, MercenaryClass target)
    {
        promotionPreview.mercenary = mercenary;
        promotionPreview.target = target;
        PromotionPreview preview = new PromotionPreview(mercenary, target);
        bool special = target == MercenaryClassProgression.GetSpecialClass(mercenary.MercenaryClass);
        ItemDataSO certificate = special && !mercenary.IsUnique ? hireAndPartyController.GetSpecialJobCertificate() : null;
        int certificateCount = certificate != null ? merchantInventory.GetItemAmount(certificate) : 0;
        bool canPromote = mercenary.CanPromote && (!special || mercenary.IsUnique || certificateCount > 0);
        promotionPreview.text.text = BuildPromotionPreviewText(mercenary, preview, certificate, certificateCount);
        promotionPreview.reasonText.text = canPromote ? string.Empty : certificate != null ? "転職証が不足しています。" : "転職条件を満たしていません。";
        promotionPreview.confirmButton.interactable = canPromote;
        promotionPreview.overlay.SetAsLastSibling();
        promotionPreview.overlay.gameObject.SetActive(true);
    }

    private string BuildPromotionPreviewText(MercenaryInstance mercenary, PromotionPreview preview, ItemDataSO certificate, int certificateCount)
    {
        string equipmentWarning = BuildPromotionEquipmentWarning(mercenary, preview.TargetClass);
        string certificateText = certificate == null ? "消費する証: なし" : $"消費する証: {JapaneseDisplayText.GetItemName(certificate)} {certificateCount}/1";
        System.Collections.Generic.List<MercenarySkillDefinition> skills =
            MercenaryClassProgression.GetCombatSkills(preview.TargetClass);
        string skillText = "解禁予定スキル: " + string.Join("、", skills.ConvertAll(skill => skill.Name));
        return $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)} → {JapaneseDisplayText.GetMercenaryClass(preview.TargetClass)}\n" +
            $"HP {mercenary.MaxHP} → {preview.MaxHP} ({preview.MaxHP - mercenary.MaxHP:+#;-#;0})\n" +
            $"攻撃 {mercenary.Attack} → {preview.Attack} ({preview.Attack - mercenary.Attack:+#;-#;0})\n" +
            $"防御 {mercenary.Defense} → {preview.Defense} ({preview.Defense - mercenary.Defense:+#;-#;0})\n" +
            $"魔力 {mercenary.MaxMagicPower} → {preview.MaxMagicPower} ({preview.MaxMagicPower - mercenary.MaxMagicPower:+#;-#;0})\n" +
            $"速度 {mercenary.AttackSpeed:0.00} → {preview.AttackSpeed:0.00} ({preview.AttackSpeed - mercenary.AttackSpeed:+0.00;-0.00;0})\n" +
            $"レベル上限: {preview.LevelCap}  |  クリティカル {preview.CriticalRate * 100f:0}%  |  回避 {preview.EvasionRate * 100f:0}%\n" +
            certificateText + "\n" + skillText + "\n" + equipmentWarning;
    }

    private static string BuildPromotionEquipmentWarning(MercenaryInstance mercenary, MercenaryClass target)
    {
        System.Collections.Generic.List<string> names = new System.Collections.Generic.List<string>();
        foreach (EquipmentSlot slot in new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Accessory })
        {
            ItemDataSO item = mercenary.GetEquippedItem(slot);
            if (item != null && !item.CanEquip(target)) names.Add(JapaneseDisplayText.GetItemName(item));
        }
        return names.Count == 0 ? "装備適合: 問題なし" : "装備不可になる装備: " + string.Join("、", names);
    }

    private void ConfirmPromotionPreview()
    {
        if (promotionPreview.mercenary == null ||
            !promotionPreview.mercenary.CanPromote ||
            MercenaryClassProgression.GetBaseClass(promotionPreview.target) !=
            promotionPreview.mercenary.OriginalClass ||
            MercenaryClassProgression.IsBaseClass(promotionPreview.target))
        {
            HidePromotionPreview();
            return;
        }

        hireAndPartyController.PromoteMercenary(promotionPreview.mercenary, promotionPreview.target);
        HidePromotionPreview();
    }

    private void HidePromotionPreview()
    {
        promotionPreview.overlay?.gameObject.SetActive(false);
        promotionPreview.mercenary = null;
    }

































    private void HandleMercenaryHired(MercenaryInstance mercenary)
    {
        dailyResultController.RecordMercenaryHired(mercenary);
        dailyResultController.CaptureMercenarySnapshot(mercenary);
        TryUnlockHiddenIsland();
        RefreshPage(companyPage);
    }

    private void HandleMercenaryDismissed(MercenaryInstance mercenary)
    {
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        RefreshPage(healPage);
        RefreshPage(jobChangePage);
    }

    private void HandlePartyChanged()
    {
        dailyResultController.RememberDailyPartyMembers();
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        if (startBattleButton != null && !battleManager.IsBattling)
        {
            startBattleButton.interactable = partyManager.Members.Count > 0;
        }
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void HandleCandidatesChanged()
    {
        RefreshPage(hirePage);
        RefreshUI();
    }

    private void HandleHealingChanged()
    {
        RefreshPage(companyPage);
        RefreshPage(partyPage);
        RefreshPage(healPage);
        RefreshUI();
    }

    private void ShowHirePage()
    {
        if (!TownServicePolicy.IsHiringAvailable(townProgressState.CurrentTownIndex))
        {
            ShowTownMap();
            statusText.text =
                $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}には傭兵を雇用できる酒場がありません。";
            return;
        }
        SwitchToPage(hirePage, hireTabButton);
        statusText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}の雇用候補  |  " +
            $"Lv{mercenaryGenerator.CurrentMinimumLevel}～" +
            $"Lv{mercenaryGenerator.CurrentMaximumLevel}  |  " +
            "雇用する傭兵を選択してください。";
    }

    private void ShowCompanyPage()
    {
        SwitchToPage(companyPage, companyTabButton);
        statusText.text =
            $"商人Lv{merchantData.MerchantLevel} " +
            $"獲得G進行 {merchantData.MerchantExperience:N0}/" +
            $"{merchantData.ExperienceToNextLevel:N0}  |  " +
            $"技能ポイント {merchantData.MerchantSkillPoints}  |  " +
            $"傭兵 {hireManager.HiredMercenaries.Count}人  |  " +
            $"雇用成功率 {merchantData.GetHireSuccessRate() * 100f:0}%";
    }

    private void ShowTransportOverlay()
    {
    }

    private void ShowExpeditionOverlay()
    {
        ShowExpeditionManagementOverlay();
    }

    private void ShowPartyPage()
    {
        SwitchToPage(partyPage, partyTabButton);
        statusText.text = $"パーティー人数: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void ShowHealPage()
    {
        SwitchToPage(healPage, healTabButton);
        statusText.text =
            $"治療費: 失ったHP 1につき {healingManager.HealCostPerHP} G";
    }

    private void ShowJobChangePage()
    {
        if (!TownServicePolicy.IsJobChangeAvailable(
                townProgressState.CurrentTownIndex))
        {
            statusText.text =
                "転職神殿はエルド交易都市以降の町で利用できます。";
            return;
        }

        SwitchToPage(jobChangePage);
        statusText.text =
            $"Lv{MercenaryClassProgression.PromotionLevel}以上の基本職が転職できます。";
    }

}
