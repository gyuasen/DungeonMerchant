using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MarketPageUI : EconomyPageUI
{
    private Func<IEnumerable<MarketStockEntry>> stockProvider;
    private Func<MarketStockEntry, bool> shouldShowEntry;
    private Action<RectTransform, MarketStockEntry, float> createRow;
    private Action<RectTransform, string> createEmptyMessage;

    public void ConfigureMarket(
        Font font,
        Color color,
        Func<IEnumerable<MarketStockEntry>> stock,
        Func<MarketStockEntry, bool> shouldShow,
        Action<RectTransform, MarketStockEntry, float> rowFactory,
        Action<RectTransform, string> emptyFactory)
    {
        Configure(font, color, null);
        stockProvider = stock;
        shouldShowEntry = shouldShow;
        createRow = rowFactory;
        createEmptyMessage = emptyFactory;
    }

    public override void Refresh()
    {
        RebuildRows(
            stockProvider?.Invoke(),
            112f,
            430f,
            "本日仕入れ可能な商品はありません。",
            shouldShowEntry,
            createRow,
            createEmptyMessage);
    }
}
