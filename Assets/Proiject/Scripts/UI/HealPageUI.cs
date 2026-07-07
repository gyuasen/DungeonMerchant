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
    private Action<RectTransform, MercenaryInstance, float> createRow;
    private Action<RectTransform, string> createEmptyMessage;

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
        ConfigureText(
            descriptionText, font, 15,
            TextAnchor.MiddleLeft, color);
        refreshAction = refresh;
    }

    public void ConfigureHealList(
        Func<IEnumerable<MercenaryInstance>> mercenaries,
        Action<RectTransform, MercenaryInstance, float> rowFactory,
        Action<RectTransform, string> emptyFactory)
    {
        mercenaryProvider = mercenaries;
        createRow = rowFactory;
        createEmptyMessage = emptyFactory;
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

            createRow?.Invoke(listRoot, mercenary, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        if (!createdAnyRow)
        {
            createEmptyMessage?.Invoke(
                listRoot,
                "治療できる傭兵はいません。");
            return;
        }

        listRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }
}
