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
    private Action<MarketStockEntry> detailAction;

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
        Func<string> targetDemandSummaryProvider,
        Action<MarketStockEntry> targetDetailAction)
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
        detailAction = targetDetailAction;
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

        CreateItemIcon(row, item);

        CreateText(
            row,
            $"{JapaneseDisplayText.GetItemName(item)} x{entry.Quantity}",
            RowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(82f, -42f),
            new Vector2(-300f, -12f),
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
            new Vector2(82f, -76f),
            new Vector2(-300f, -48f),
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

        Button detailButton = CreateActionButton(
            row,
            "詳細",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => detailAction?.Invoke(entry));
        RectTransform detailRect = detailButton.GetComponent<RectTransform>();
        detailRect.anchoredPosition = new Vector2(-160f, 0f);
    }

    private void CreateItemIcon(RectTransform row, ItemDataSO item)
    {
        RectTransform iconRect = CreateUIObject("Item Icon", row);
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.sizeDelta = new Vector2(54f, 54f);
        iconRect.anchoredPosition = new Vector2(18f, 0f);
        Image icon = iconRect.gameObject.AddComponent<Image>();
        Sprite sprite = ItemPresentationService.ResolveSprite(item);
        icon.sprite = sprite;
        icon.color = sprite != null ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
        if (sprite == null)
        {
            CreateText(iconRect, "?", RowFont, 28, FontStyle.Bold,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Color.white);
        }
    }
}
