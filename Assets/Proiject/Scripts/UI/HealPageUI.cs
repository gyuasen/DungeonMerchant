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

    public void ConfigureHealList(
        Func<IEnumerable<MercenaryInstance>> mercenaries,
        Func<MercenaryInstance, int> targetMissingHpProvider,
        Func<MercenaryInstance, int> targetHealCostProvider,
        Func<MercenaryInstance, bool> targetCanHealProvider,
        Action<MercenaryInstance> targetHealAction)
    {
        mercenaryProvider = mercenaries;
        missingHpProvider = targetMissingHpProvider;
        healCostProvider = targetHealCostProvider;
        canHealProvider = targetCanHealProvider;
        healAction = targetHealAction;
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
            mercenary => mercenary != null,
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
    }

    private string BuildHealDetails(MercenaryInstance mercenary)
    {
        int missingHP = missingHpProvider?.Invoke(mercenary) ?? 0;
        int healCost = healCostProvider?.Invoke(mercenary) ?? 0;
        string condition = mercenary.IsIncapacitated ? "再活性治療対象  |  " : string.Empty;
        return
            $"{condition}HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
            $"不足 {missingHP}  |  全回復 {healCost} G";
    }
}
