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
        UnityAction refresh)
    {
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

}
