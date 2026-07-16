using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryPageUI : ListPageUIBase
{
    private Func<IEnumerable<InventoryItemStack>> itemProvider;
    private Func<IEnumerable<EquipmentInstance>> equipmentProvider;
    private Func<InventoryItemStack, bool> shouldShowItem;
    private Func<EquipmentInstance, bool> shouldShowEquipment;
    private Func<ItemDataSO, int> sellPriceProvider;
    private Func<ItemDataSO, float> sellMultiplierProvider;
    private Func<ItemDataSO, float> demandMultiplierProvider;
    private Func<EquipmentInstance, string> equipmentNameProvider;
    private Func<EquipmentQuality, Color> equipmentQualityColorProvider;
    private Action<ItemDataSO> sellItemAction;
    private Action<ItemDataSO> useConsumableAction;
    private Action<EquipmentInstance> showEquipmentDetailsAction;

    public void ConfigureInventory(
        Font font,
        Color color,
        Color mutedTextColor,
        Color buttonTextColor,
        Color rowColor,
        Color buttonColor,
        Color frameColor,
        Func<IEnumerable<InventoryItemStack>> items,
        Func<IEnumerable<EquipmentInstance>> equipment,
        Func<InventoryItemStack, bool> itemFilter,
        Func<EquipmentInstance, bool> equipmentFilter,
        Func<ItemDataSO, int> targetSellPriceProvider,
        Func<ItemDataSO, float> targetSellMultiplierProvider,
        Func<ItemDataSO, float> targetDemandMultiplierProvider,
        Func<EquipmentInstance, string> targetEquipmentNameProvider,
        Func<EquipmentQuality, Color> targetEquipmentQualityColorProvider,
        Action<ItemDataSO> targetSellItemAction,
        Action<ItemDataSO> targetUseConsumableAction,
        Action<EquipmentInstance> targetShowEquipmentDetailsAction)
    {
        Configure(
            font,
            color,
            mutedTextColor,
            buttonTextColor,
            rowColor,
            buttonColor,
            frameColor,
            null);
        itemProvider = items;
        equipmentProvider = equipment;
        shouldShowItem = itemFilter;
        shouldShowEquipment = equipmentFilter;
        sellPriceProvider = targetSellPriceProvider;
        sellMultiplierProvider = targetSellMultiplierProvider;
        demandMultiplierProvider = targetDemandMultiplierProvider;
        equipmentNameProvider = targetEquipmentNameProvider;
        equipmentQualityColorProvider = targetEquipmentQualityColorProvider;
        sellItemAction = targetSellItemAction;
        useConsumableAction = targetUseConsumableAction;
        showEquipmentDetailsAction = targetShowEquipmentDetailsAction;
    }

    // 通常アイテムと装備という2種類のコレクションを1本のrowTopで連続配置する
    // ため、共通テンプレートのRebuildRows<T>（1コレクション前提でClearChildren
    // する）には素直に収まらない。HirePageUIと同じ理由で、装飾（色/フォント/
    // Configure）のみ基底へ委譲し、Refresh()は独自実装のまま残す。
    public override void Refresh()
    {
        ClearChildren(ListRoot);
        ListRoot.sizeDelta = new Vector2(0f, 430f);

        float rowTop = 0f;
        bool createdAnyRow = false;
        foreach (InventoryItemStack stack in
                 itemProvider?.Invoke() ?? Array.Empty<InventoryItemStack>())
        {
            if (shouldShowItem != null && !shouldShowItem(stack))
            {
                continue;
            }

            CreateInventoryRow(stack, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        foreach (EquipmentInstance equipment in
                 equipmentProvider?.Invoke() ?? Array.Empty<EquipmentInstance>())
        {
            if (shouldShowEquipment != null &&
                !shouldShowEquipment(equipment))
            {
                continue;
            }

            CreateEquipmentInventoryRow(equipment, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        if (!createdAnyRow)
        {
            CreateEmptyMessage("在庫はありません。");
            return;
        }

        ListRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void CreateInventoryRow(InventoryItemStack stack, float top)
    {
        ItemDataSO item = stack.Item;
        RectTransform row =
            CreateRow(
                item.itemName,
                ListRoot,
                top,
                RowColor,
                FrameColor);

        CreateText(
            row,
            $"{JapaneseDisplayText.GetItemName(item)} x{stack.Amount}",
            RowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            RowTextColor);

        int sellPrice = sellPriceProvider?.Invoke(item) ?? 0;
        int percent = Mathf.RoundToInt(
            (sellMultiplierProvider?.Invoke(item) ?? 0f) * 100f);
        string details =
            $"{JapaneseDisplayText.GetItemRarity(item.rarity)}  |  " +
            $"{JapaneseDisplayText.GetItemType(item.itemType)}  |  基準 {item.basePrice} G  |  " +
            $"本日 {sellPrice} G ({percent}%)";
        CreateText(
            row,
            details,
            RowFont,
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        CreateDemandIndicator(row, item, -76f);

        Button sellButton = CreateActionButton(
            row,
            "売却",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => sellItemAction?.Invoke(item));
        if (item.itemType != ItemType.Consumable)
        {
            return;
        }

        RectTransform sellRect = sellButton.GetComponent<RectTransform>();
        sellRect.sizeDelta = new Vector2(100f, 44f);
        sellRect.anchoredPosition = new Vector2(-18f, -22f);

        Button useButton = CreateActionButton(
            row,
            "使用",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => useConsumableAction?.Invoke(item));
        RectTransform useRect = useButton.GetComponent<RectTransform>();
        useRect.sizeDelta = new Vector2(100f, 44f);
        useRect.anchoredPosition = new Vector2(-18f, 26f);
    }

    private void CreateEquipmentInventoryRow(
        EquipmentInstance equipment,
        float top)
    {
        RectTransform row =
            CreateRow(
                equipment.InstanceId,
                ListRoot,
                top,
                RowColor,
                FrameColor);
        string quality =
            JapaneseDisplayText.GetEquipmentQuality(equipment.Quality);
        Color qualityColor =
            equipmentQualityColorProvider?.Invoke(equipment.Quality) ??
            RowTextColor;

        CreateText(
            row,
            $"{(equipment.IsLocked ? "[LOCK] " : string.Empty)}" +
            $"[{quality}] {GetEquipmentDisplayName(equipment)}",
            RowFont,
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            qualityColor);

        string details =
            EquipmentRankPresentation.GetRichText(equipment.BaseItem) + "  |  " +
            $"HP {FormatSigned(equipment.BonusMaxHP)}  " +
            $"攻撃 {FormatSigned(equipment.BonusAttack)}  " +
            $"防御 {FormatSigned(equipment.BonusDefense)}  " +
            $"速度 {FormatSigned(equipment.BonusAttackSpeed)}";
        CreateText(
            row,
            details,
            RowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        CreateActionButton(
            row,
            "詳細",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => showEquipmentDetailsAction?.Invoke(equipment));
    }

    private string GetEquipmentDisplayName(EquipmentInstance equipment)
    {
        return equipmentNameProvider?.Invoke(equipment) ??
               equipment.BaseItem?.itemName ??
               "不明な装備";
    }

    private void CreateDemandIndicator(
        RectTransform row,
        ItemDataSO item,
        float verticalPosition)
    {
        float demandMultiplier = demandMultiplierProvider?.Invoke(item) ?? 1f;
        string indicator = demandMultiplier > 1.05f ? "相場高▲" :
            demandMultiplier < 0.95f ? "相場安▼" : string.Empty;
        if (string.IsNullOrEmpty(indicator))
        {
            return;
        }

        Color indicatorColor = demandMultiplier > 1.05f
            ? new Color(0.18f, 0.52f, 0.24f)
            : new Color(0.72f, 0.18f, 0.12f);
        CreateText(
            row,
            indicator,
            RowFont,
            14,
            FontStyle.Bold,
            TextAnchor.MiddleRight,
            new Vector2(-150f, verticalPosition),
            new Vector2(-18f, verticalPosition + 28f),
            indicatorColor);
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private static string FormatSigned(float value)
    {
        return value >= 0f ? $"+{value:0.##}" : value.ToString("0.##");
    }
}
