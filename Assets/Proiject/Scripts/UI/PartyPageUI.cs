using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class PartyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;
    private Func<int> maxSlotProvider;
    private Func<IReadOnlyList<MercenaryInstance>> memberProvider;
    private Action<RectTransform, MercenaryInstance, int, float> createMemberRow;
    private Action<RectTransform, int, float> createEmptyRow;

    public void Initialize(Text title, RectTransform targetListRoot)
    {
        titleText = title;
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
        refreshAction = refresh;
    }

    public void ConfigurePartyList(
        Func<int> maxSlots,
        Func<IReadOnlyList<MercenaryInstance>> members,
        Action<RectTransform, MercenaryInstance, int, float> memberRowFactory,
        Action<RectTransform, int, float> emptyRowFactory)
    {
        maxSlotProvider = maxSlots;
        memberProvider = members;
        createMemberRow = memberRowFactory;
        createEmptyRow = emptyRowFactory;
    }

    public override void Refresh()
    {
        if (maxSlotProvider == null || memberProvider == null)
        {
            refreshAction?.Invoke();
            return;
        }

        ClearChildren(listRoot);

        IReadOnlyList<MercenaryInstance> members =
            memberProvider.Invoke() ?? Array.Empty<MercenaryInstance>();
        float rowTop = 0f;
        for (int slotIndex = 0; slotIndex < maxSlotProvider.Invoke(); slotIndex++)
        {
            if (slotIndex < members.Count)
            {
                createMemberRow?.Invoke(
                    listRoot,
                    members[slotIndex],
                    slotIndex,
                    rowTop);
            }
            else
            {
                createEmptyRow?.Invoke(listRoot, slotIndex, rowTop);
            }

            rowTop -= 112f;
        }
    }
}
