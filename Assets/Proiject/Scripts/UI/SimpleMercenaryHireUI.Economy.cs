using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void HandleInventoryChanged()
    {
        dailyResultController.RecordDailyInventoryGains();
        TryUnlockHiddenIsland();
        RefreshPage(inventoryPage);
        RefreshPage(blacksmithPage);
        RefreshUI();
    }

    private void HandleMarketStockChanged()
    {
        RefreshPage(marketPage);
        RefreshUI();
    }

    private void HandleCraftingChanged()
    {
        RefreshPage(inventoryPage);
        RefreshPage(blacksmithPage);
        RefreshUI();
    }

    private void HandlePricesChanged()
    {
        RefreshPage(inventoryPage);
        RefreshUI();
    }

    private void AdvanceDay()
    {
        dayManager.AdvanceDay();
    }

    private void ShowMarketPage()
    {
        SwitchToPage(marketPage, marketTabButton);
        statusText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}市場  |  " +
            $"仕入れ商品: {marketStockManager.Stock.Count}種類 / " +
            marketPriceManager.GetMarketSummary();
    }

    private void ShowBlacksmithPage()
    {
        SwitchToPage(blacksmithPage, blacksmithTabButton);
        statusText.text =
            $"{WorldMapService.TownNames[townProgressState.CurrentTownIndex]}鍛冶屋  |  " +
            $"レシピ: {blacksmithManager.Recipes.Count}種類";
        RefreshUI();
    }

    private void ShowInventoryPage()
    {
        UpdateStorageCapacityText();
        SwitchToPage(inventoryPage, inventoryTabButton);
        statusText.text =
            $"倉庫 {merchantInventory.GetUsedStorageSlots()}/" +
            $"{(progressionManager != null ? progressionManager.StorageCapacity : 0)}  |  " +
            $"{marketPriceManager.GetMarketSummary()}  |  " +
            $"維持費 {(progressionManager != null ? progressionManager.StorageMaintenanceCost : 0)}G/日";
    }

    private void UpdateStorageCapacityText()
    {
        if (storageUpgrade.capacityText == null)
        {
            return;
        }

        int used = merchantInventory != null
            ? merchantInventory.GetUsedStorageSlots()
            : 0;
        int capacity = progressionManager != null
            ? progressionManager.StorageCapacity
            : 0;
        int remaining = Mathf.Max(0, capacity - used);
        string expansion = progressionManager == null
            ? string.Empty
            : progressionManager.IsStorageAtMaximumTier
                ? "最大拡張済み"
                : $"次回 {progressionManager.NextStorageCapacity}枠 / " +
                  $"{progressionManager.StorageUpgradeCost:N0}G / " +
                  $"商人Lv{progressionManager.NextStorageRequiredMerchantLevel}";

        storageUpgrade.capacityText.text =
            $"倉庫 {used}/{capacity}（空き {remaining}）  |  {expansion}";
        storageUpgrade.capacityText.color = capacity > 0 && remaining == 0
            ? new Color(0.65f, 0.08f, 0.04f)
            : remaining <= Mathf.Max(3, Mathf.CeilToInt(capacity * 0.1f))
                ? new Color(0.72f, 0.35f, 0.04f)
                : ParchmentTextColor;
    }

    private void BuildStorageUpgradeConfirmationOverlay()
    {
        storageUpgrade.confirmationOverlay = CreateUIObject(
            "Storage Upgrade Confirmation Overlay",
            overlayRoot);
        storageUpgrade.confirmationOverlay.gameObject.SetActive(false);
        storageUpgrade.confirmationOverlay.anchorMin = Vector2.zero;
        storageUpgrade.confirmationOverlay.anchorMax = Vector2.one;
        storageUpgrade.confirmationOverlay.offsetMin = Vector2.zero;
        storageUpgrade.confirmationOverlay.offsetMax = Vector2.zero;
        storageUpgrade.confirmationOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject(
            "Storage Upgrade Confirmation Window",
            storageUpgrade.confirmationOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(560f, 340f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "倉庫を拡張しますか？",
            26,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(28f, -72f),
            new Vector2(-28f, -22f),
            ParchmentTextColor);

        storageUpgrade.confirmationText = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -190f),
            new Vector2(-36f, -82f),
            ParchmentTextColor);
        storageUpgrade.confirmationReasonText = CreateText(
            window,
            string.Empty,
            15,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -238f),
            new Vector2(-36f, -190f),
            MutedTextColor);

        storageUpgrade.confirmButton = CreateActionButton(
            window,
            "拡張する",
            ConfirmStorageUpgrade);
        RectTransform confirmRect =
            storageUpgrade.confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = confirmRect.pivot =
            new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 26f);
        storageUpgrade.confirmButton.targetGraphic.color = AccentColor;

        Button cancelButton = CreateActionButton(
            window,
            "キャンセル",
            HideStorageUpgradeConfirmation);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax = cancelRect.pivot =
            new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 26f);

    }

    private void ShowStorageUpgradeConfirmation()
    {
        RefreshStorageUpgradeConfirmation();
        storageUpgrade.confirmationOverlay.SetAsLastSibling();
        storageUpgrade.confirmationOverlay.gameObject.SetActive(true);
    }

    private void HideStorageUpgradeConfirmation()
    {
        storageUpgrade.confirmationOverlay?.gameObject.SetActive(false);
    }

    private void ConfirmStorageUpgrade()
    {
        if (merchantStatusAndQuestController.TryUpgradeStorage())
        {
            HideStorageUpgradeConfirmation();
            return;
        }

        RefreshStorageUpgradeConfirmation();
    }

    private void RefreshStorageUpgradeConfirmation()
    {
        if (storageUpgrade.confirmationText == null ||
            storageUpgrade.confirmationReasonText == null ||
            storageUpgrade.confirmButton == null)
        {
            return;
        }

        if (progressionManager == null || merchantData == null)
        {
            storageUpgrade.confirmationText.text = "倉庫情報を取得できません。";
            storageUpgrade.confirmationReasonText.text = string.Empty;
            storageUpgrade.confirmButton.interactable = false;
            return;
        }

        if (progressionManager.IsStorageAtMaximumTier)
        {
            storageUpgrade.confirmationText.text =
                $"現在の容量: {progressionManager.StorageCapacity}枠\n倉庫は最大まで拡張済みです。";
            storageUpgrade.confirmationReasonText.text =
                "これ以上拡張できません。";
            storageUpgrade.confirmButton.interactable = false;
            return;
        }

        int cost = progressionManager.StorageUpgradeCost;
        int requiredLevel = progressionManager.NextStorageRequiredMerchantLevel;
        int missingGold = Mathf.Max(0, cost - merchantData.Gold);
        storageUpgrade.confirmationText.text =
            $"容量: {progressionManager.StorageCapacity}枠 → " +
            $"{progressionManager.NextStorageCapacity}枠\n" +
            $"必要金額: {cost:N0}G  |  所持金: {merchantData.Gold:N0}G\n" +
            $"必要商人レベル: Lv{requiredLevel}（現在 Lv{merchantData.MerchantLevel}）";
        if (merchantData.MerchantLevel < requiredLevel)
        {
            storageUpgrade.confirmationReasonText.text =
                $"商人レベルが不足しています。（あと {requiredLevel - merchantData.MerchantLevel}）";
        }
        else if (missingGold > 0)
        {
            storageUpgrade.confirmationReasonText.text =
                $"資金が不足しています。（あと {missingGold:N0}G）";
        }
        else
        {
            storageUpgrade.confirmationReasonText.text = "拡張できます。";
        }

        storageUpgrade.confirmButton.interactable =
            progressionManager.CanUpgradeStorage();
    }

}
