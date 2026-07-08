using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HealPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, int> missingHpProvider;
    private Func<MercenaryInstance, int> healCostProvider;
    private Func<MercenaryInstance, bool> canHealProvider;
    private Action<MercenaryInstance> healAction;
    private Font rowFont;
    private Color rowTextColor = Color.white;
    private Color mutedTextColor = Color.gray;
    private Color buttonTextColor = Color.white;
    private Color rowColor = new Color(0.27f, 0.16f, 0.09f, 0.94f);
    private Color buttonColor = new Color(0.35f, 0.22f, 0.13f, 1f);
    private Color frameColor = new Color(0.72f, 0.52f, 0.27f, 0.9f);

    public void Initialize(
        Text title,
        Text description,
        RectTransform targetListRoot)
    {
        titleText = title;
        descriptionText = description;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font font,
        Color color,
        Color targetMutedTextColor,
        Color targetButtonTextColor,
        Color targetRowColor,
        Color targetButtonColor,
        Color targetFrameColor,
        UnityAction refresh)
    {
        rowFont = font;
        mutedTextColor = targetMutedTextColor;
        buttonTextColor = targetButtonTextColor;
        rowColor = targetRowColor;
        buttonColor = targetButtonColor;
        frameColor = targetFrameColor;

        ConfigureText(
            titleText, font, 15,
            TextAnchor.MiddleLeft, color);
        ConfigureText(
            descriptionText, font, 15,
            TextAnchor.MiddleLeft, color);
        refreshAction = refresh;
    }

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
            refreshAction?.Invoke();
            return;
        }

        ClearChildren(listRoot);

        float rowTop = 0f;
        bool createdAnyRow = false;
        foreach (MercenaryInstance mercenary in
                 mercenaryProvider.Invoke() ??
                 Array.Empty<MercenaryInstance>())
        {
            if (mercenary == null)
            {
                continue;
            }

            CreateHealRow(mercenary, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        if (!createdAnyRow)
        {
            CreateEmptyMessage("治療できる傭兵はいません。");
            return;
        }

        listRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void CreateHealRow(MercenaryInstance mercenary, float top)
    {
        RectTransform row =
            CreateRow(
                $"{mercenary.MercenaryName} Treatment",
                listRoot,
                top,
                rowColor,
                frameColor);

        CreateText(
            row,
            mercenary.MercenaryName,
            rowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            rowTextColor);

        CreateText(
            row,
            BuildHealDetails(mercenary),
            rowFont,
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            mutedTextColor);

        Button healButton = CreateActionButton(
            row,
            "治療",
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => healAction?.Invoke(mercenary));
        healButton.interactable = canHealProvider?.Invoke(mercenary) == true;
    }

    private string BuildHealDetails(MercenaryInstance mercenary)
    {
        int missingHP = missingHpProvider?.Invoke(mercenary) ?? 0;
        int healCost = healCostProvider?.Invoke(mercenary) ?? 0;
        string condition = mercenary.IsIncapacitated ? "戦闘不能  |  " : string.Empty;
        return
            $"{condition}HP {mercenary.CurrentHP}/{mercenary.MaxHP}  |  " +
            $"不足 {missingHP}  |  全回復 {healCost} G";
    }

    private void CreateEmptyMessage(string message)
    {
        CreateText(
            listRoot,
            message,
            rowFont,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -180f),
            new Vector2(0f, -80f),
            mutedTextColor);
    }
}
