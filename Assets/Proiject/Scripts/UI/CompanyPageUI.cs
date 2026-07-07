using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class CompanyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button questButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Action<RectTransform, MercenaryInstance, float> createRow;
    private Action<RectTransform, string> createEmptyMessage;

    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text title,
        Button quest,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = title;
        questButton = quest;
        scrollRect = targetScrollRect;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font titleFont,
        Font buttonFont,
        Color titleColor,
        Color buttonTextColor,
        UnityAction showQuests,
        UnityAction refresh)
    {
        ConfigureText(
            titleText, titleFont, 15,
            TextAnchor.MiddleLeft, titleColor);
        ConfigureButton(
            questButton, buttonFont, buttonTextColor,
            "依頼", showQuests);
        scrollRect.content = listRoot;
        refreshAction = refresh;
    }

    public void ConfigureCompanyList(
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
                "雇用済みの傭兵はいません。");
            return;
        }

        listRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }
}
