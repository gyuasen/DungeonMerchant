using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class HealPageUI : ListPageUIBase
{
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, int> missingHpProvider;
    private Func<MercenaryInstance, int> healCostProvider;
    private Func<MercenaryInstance, bool> canHealProvider;
    private Action<MercenaryInstance> healAction;
    private Action<MercenaryInstance> showDetailsAction;
    private Func<MercenaryInstance, bool> needsHealingProvider;
    private Func<MercenaryInstance, string> unavailableReasonProvider;

    public void ConfigureHealList(
        Func<IEnumerable<MercenaryInstance>> mercenaries,
        Func<MercenaryInstance, int> targetMissingHpProvider,
        Func<MercenaryInstance, int> targetHealCostProvider,
        Func<MercenaryInstance, bool> targetCanHealProvider,
        Action<MercenaryInstance> targetHealAction,
        Action<MercenaryInstance> targetShowDetailsAction,
        Func<MercenaryInstance, bool> targetNeedsHealingProvider,
        Func<MercenaryInstance, string> targetUnavailableReasonProvider)
    {
        mercenaryProvider = mercenaries;
        missingHpProvider = targetMissingHpProvider;
        healCostProvider = targetHealCostProvider;
        canHealProvider = targetCanHealProvider;
        healAction = targetHealAction;
        showDetailsAction = targetShowDetailsAction;
        needsHealingProvider = targetNeedsHealingProvider;
        unavailableReasonProvider = targetUnavailableReasonProvider;
    }

    public override void Refresh()
    {
        if (mercenaryProvider == null)
        {
            base.Refresh();
            return;
        }

        RebuildRows(
            mercenaryProvider.Invoke(),
            112f,
            430f,
            "治療できる傭兵はいません。",
            mercenary => mercenary != null && needsHealingProvider?.Invoke(mercenary) == true,
            (_, mercenary, rowTop) => CreateHealRow(mercenary, rowTop),
            (_, message) => CreateEmptyMessage(message));
    }

    private void CreateHealRow(MercenaryInstance mercenary, float top)
    {
        RectTransform row =
            CreateRow(
                $"{mercenary.MercenaryName} Treatment",
                ListRoot,
                top,
                RowColor,
                FrameColor);

        CreateText(
            row,
            mercenary.MercenaryName,
            RowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            RowTextColor);

        CreateText(
            row,
            BuildHealDetails(mercenary),
            RowFont,
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        Button healButton = CreateActionButton(
            row,
            "治療",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => healAction?.Invoke(mercenary));
        healButton.interactable = canHealProvider?.Invoke(mercenary) == true;
        RectTransform healRect = healButton.GetComponent<RectTransform>();
        healRect.anchoredPosition = new Vector2(-18f, 24f);
        Button detailButton = CreateActionButton(
            row,
            "詳細",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => showDetailsAction?.Invoke(mercenary));
        RectTransform detailRect = detailButton.GetComponent<RectTransform>();
        detailRect.anchoredPosition = new Vector2(-18f, -24f);
    }

    private string BuildHealDetails(MercenaryInstance mercenary)
    {
        int missingHP = missingHpProvider?.Invoke(mercenary) ?? 0;
        int healCost = healCostProvider?.Invoke(mercenary) ?? 0;
        string reason = unavailableReasonProvider?.Invoke(mercenary);
        string condition = mercenary.IsIncapacitated ? "再活性治療対象  |  " : string.Empty;
        if (!string.IsNullOrEmpty(reason))
        {
            condition += reason + "  |  ";
        }
        return
            $"{condition}HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
            $"不足 {missingHP}  |  全回復 {healCost} G";
    }
}
