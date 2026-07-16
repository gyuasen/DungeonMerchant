using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class MarketPageUI : ListPageUIBase
{
    private Func<IEnumerable<MarketStockEntry>> stockProvider;
    private Func<MarketStockEntry, bool> shouldShowEntry;
    private Func<MarketStockEntry, bool> canBuyEntry;
    private Action<MarketStockEntry> buyAction;
    private Action<Button, MarketStockEntry> registerBuyButton;
    private Text demandSummaryText;
    private Func<string> demandSummaryProvider;

    public void ConfigureMarket(
        Font font,
        Color color,
        Color mutedTextColor,
        Color buttonTextColor,
        Color rowColor,
        Color buttonColor,
        Color frameColor,
        Func<IEnumerable<MarketStockEntry>> stock,
        Func<MarketStockEntry, bool> shouldShow,
        Func<MarketStockEntry, bool> targetCanBuyEntry,
        Action<MarketStockEntry> targetBuyAction,
        Action<Button, MarketStockEntry> targetRegisterBuyButton,
        Text targetDemandSummaryText,
        Func<string> targetDemandSummaryProvider)
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
        stockProvider = stock;
        shouldShowEntry = shouldShow;
        canBuyEntry = targetCanBuyEntry;
        buyAction = targetBuyAction;
        registerBuyButton = targetRegisterBuyButton;
        demandSummaryText = targetDemandSummaryText;
        demandSummaryProvider = targetDemandSummaryProvider;
    }

    public override void Refresh()
    {
        if (demandSummaryText != null)
        {
            demandSummaryText.text = demandSummaryProvider?.Invoke() ?? string.Empty;
        }

        RebuildRows(
            stockProvider?.Invoke(),
            112f,
            430f,
            "本日仕入れ可能な商品はありません。",
            shouldShowEntry,
            (_, entry, rowTop) => CreateMarketRow(entry, rowTop),
            (_, message) => CreateEmptyMessage(message));
    }

    private void CreateMarketRow(MarketStockEntry entry, float top)
    {
        ItemDataSO item = entry.Item;
        RectTransform row =
            CreateRow(
                item.itemName,
                ListRoot,
                top,
                RowColor,
                FrameColor);

        CreateText(
            row,
            $"{JapaneseDisplayText.GetItemName(item)} x{entry.Quantity}",
            RowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            RowTextColor);

        string details =
            EquipmentRankPresentation.GetRichText(item) + "  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(item.requiredClass)}用  |  " +
            $"{JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot)}  |  " +
            $"攻撃+{item.bonusAttack}  " +
            $"防御+{item.bonusDefense}  HP+{item.bonusMaxHP}  |  " +
            $"仕入れ {entry.BuyPrice} G";
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

        Button buyButton = CreateActionButton(
            row,
            "購入",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => buyAction?.Invoke(entry));
        buyButton.interactable = canBuyEntry?.Invoke(entry) == true;
        registerBuyButton?.Invoke(buyButton, entry);
    }
}
