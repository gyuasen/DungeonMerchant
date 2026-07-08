using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class EconomyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    protected RectTransform ListRoot => listRoot;
    protected Font RowFont { get; private set; }
    protected Color RowTextColor { get; private set; } = Color.white;
    protected Color MutedTextColor { get; private set; } = Color.gray;
    protected Color ButtonTextColor { get; private set; } = Color.white;
    protected Color RowColor { get; private set; } =
        new Color(0.27f, 0.16f, 0.09f, 0.94f);
    protected Color ButtonColor { get; private set; } =
        new Color(0.35f, 0.22f, 0.13f, 1f);
    protected Color FrameColor { get; private set; } =
        new Color(0.72f, 0.52f, 0.27f, 0.9f);

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
        Color mutedTextColor,
        Color buttonTextColor,
        Color rowColor,
        Color buttonColor,
        Color frameColor,
        UnityAction refresh)
    {
        RowFont = font;
        MutedTextColor = mutedTextColor;
        ButtonTextColor = buttonTextColor;
        RowColor = rowColor;
        ButtonColor = buttonColor;
        FrameColor = frameColor;

        ConfigureText(
            titleText, font, 15,
            TextAnchor.MiddleLeft, color);
        if (descriptionText != null)
        {
            ConfigureText(
                descriptionText, font, 15,
                TextAnchor.MiddleLeft, color);
        }
        refreshAction = refresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }

    protected void RebuildRows<T>(
        IEnumerable<T> entries,
        float rowHeight,
        float minimumHeight,
        string emptyMessage,
        Func<T, bool> shouldShow,
        Action<RectTransform, T, float> createRow,
        Action<RectTransform, string> createEmptyMessage)
    {
        ClearChildren(listRoot);
        listRoot.sizeDelta = new Vector2(0f, minimumHeight);

        float rowTop = 0f;
        bool createdAnyRow = false;
        if (entries != null)
        {
            foreach (T entry in entries)
            {
                if (shouldShow != null && !shouldShow(entry))
                {
                    continue;
                }

                createRow?.Invoke(listRoot, entry, rowTop);
                rowTop -= rowHeight;
                createdAnyRow = true;
            }
        }

        if (!createdAnyRow)
        {
            createEmptyMessage?.Invoke(listRoot, emptyMessage);
            return;
        }

        listRoot.sizeDelta =
            new Vector2(0f, Mathf.Max(minimumHeight, -rowTop));
    }

    protected void CreateEmptyMessage(string message)
    {
        CreateText(
            ListRoot,
            message,
            RowFont,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -180f),
            new Vector2(0f, -80f),
            MutedTextColor);
    }
}
