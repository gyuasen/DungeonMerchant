using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class JobChangePageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, bool> shouldShowSpecialPromotion;
    private Action<MercenaryInstance, MercenaryClass> promoteAction;
    private Font rowFont;
    private Color rowTextColor = Color.white;
    private Color mutedTextColor = Color.gray;
    private Color buttonTextColor = Color.white;
    private Color rowColor = new Color(0.27f, 0.16f, 0.09f, 0.94f);
    private Color buttonColor = new Color(0.35f, 0.22f, 0.13f, 1f);
    private Color frameColor = new Color(0.72f, 0.52f, 0.27f, 0.9f);

    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text title,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = title;
        scrollRect = targetScrollRect;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font font,
        Color titleColor,
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
            titleText,
            font,
            17,
            TextAnchor.MiddleLeft,
            titleColor);
        scrollRect.content = listRoot;
        refreshAction = refresh;
    }

    public void ConfigureJobChangeList(
        Func<IEnumerable<MercenaryInstance>> targetMercenaryProvider,
        Func<MercenaryInstance, bool> targetShouldShowSpecialPromotion,
        Action<MercenaryInstance, MercenaryClass> targetPromoteAction)
    {
        mercenaryProvider = targetMercenaryProvider;
        shouldShowSpecialPromotion = targetShouldShowSpecialPromotion;
        promoteAction = targetPromoteAction;
    }

    public override void Refresh()
    {
        if (mercenaryProvider == null)
        {
            refreshAction?.Invoke();
            return;
        }

        ClearChildren(listRoot);

        float top = 0f;
        foreach (MercenaryInstance mercenary in
                 mercenaryProvider.Invoke() ??
                 Array.Empty<MercenaryInstance>())
        {
            CreateJobChangeRow(mercenary, top);
            top -= 112f;
        }

        listRoot.sizeDelta =
            new Vector2(0f, Mathf.Max(430f, -top));
    }

    private void CreateJobChangeRow(
        MercenaryInstance mercenary,
        float top)
    {
        RectTransform row =
            CreateRow(
                $"Job Change {mercenary.InstanceId}",
                listRoot,
                top,
                rowColor,
                frameColor);
        CreateText(
            row,
            $"{mercenary.MercenaryName}  Lv{mercenary.Level}  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}",
            rowFont,
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -44f),
            new Vector2(-370f, -8f),
            rowTextColor);

        if (mercenary.CanPromote)
        {
            CreatePromotionButtons(row, mercenary);
            return;
        }

        CreateUnavailableMessage(row, mercenary);
    }

    private void CreatePromotionButtons(
        RectTransform row,
        MercenaryInstance mercenary)
    {
        MercenaryClass[] advanced =
            MercenaryClassProgression.GetAdvancedClasses(
                mercenary.MercenaryClass);
        CreatePromotionButton(row, mercenary, advanced[0], -18f);
        CreatePromotionButton(row, mercenary, advanced[1], -130f);

        bool showSpecial =
            shouldShowSpecialPromotion?.Invoke(mercenary) ?? false;
        if (!showSpecial)
        {
            return;
        }

        CreatePromotionButton(
            row,
            mercenary,
            MercenaryClassProgression.GetSpecialClass(
                mercenary.MercenaryClass),
            -242f);
    }

    private void CreatePromotionButton(
        RectTransform row,
        MercenaryInstance mercenary,
        MercenaryClass target,
        float x)
    {
        Button button = CreateActionButton(
            row,
            JapaneseDisplayText.GetMercenaryClass(target),
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => promoteAction?.Invoke(mercenary, target));
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(104f, 44f);
        rect.anchoredPosition = new Vector2(x, 0f);
    }

    private void CreateUnavailableMessage(
        RectTransform row,
        MercenaryInstance mercenary)
    {
        string message = MercenaryClassProgression.IsBaseClass(
            mercenary.MercenaryClass)
            ? $"Lv{MercenaryClassProgression.PromotionLevel}で転職可能"
            : "転職済み";
        CreateText(
            row,
            message,
            rowFont,
            14,
            FontStyle.Normal,
            TextAnchor.MiddleRight,
            new Vector2(16f, -72f),
            new Vector2(-18f, -42f),
            mutedTextColor);
    }
}
