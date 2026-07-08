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
    private Action<MercenaryInstance> removeMemberAction;
    private Font rowFont;
    private Color rowTextColor = Color.white;
    private Color mutedTextColor = Color.gray;
    private Color buttonTextColor = Color.white;
    private Color rowColor = new Color(0.27f, 0.16f, 0.09f, 0.94f);
    private Color buttonColor = new Color(0.35f, 0.22f, 0.13f, 1f);
    private Color frameColor = new Color(0.72f, 0.52f, 0.27f, 0.9f);

    public void Initialize(Text title, RectTransform targetListRoot)
    {
        titleText = title;
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
        refreshAction = refresh;
    }

    public void ConfigurePartyList(
        Func<int> maxSlots,
        Func<IReadOnlyList<MercenaryInstance>> members,
        Action<MercenaryInstance> targetRemoveMemberAction)
    {
        maxSlotProvider = maxSlots;
        memberProvider = members;
        removeMemberAction = targetRemoveMemberAction;
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
                CreatePartyRow(members[slotIndex], slotIndex, rowTop);
            }
            else
            {
                CreateEmptyPartyRow(slotIndex, rowTop);
            }

            rowTop -= 112f;
        }
    }

    private void CreatePartyRow(
        MercenaryInstance mercenary,
        int slotIndex,
        float top)
    {
        RectTransform row =
            CreateRow(
                $"Party Slot {slotIndex + 1}",
                listRoot,
                top,
                rowColor,
                frameColor);
        CreateText(
            row,
            $"{slotIndex + 1}. {mercenary.MercenaryName}",
            rowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            rowTextColor);

        CreateText(
            row,
            BuildMercenaryDetails(mercenary),
            rowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            mutedTextColor);

        CreateActionButton(
            row,
            "外す",
            rowFont,
            buttonColor,
            frameColor,
            buttonTextColor,
            () => removeMemberAction?.Invoke(mercenary));
    }

    private void CreateEmptyPartyRow(int slotIndex, float top)
    {
        RectTransform row =
            CreateRow(
                $"Empty Party Slot {slotIndex + 1}",
                listRoot,
                top,
                rowColor,
                frameColor);

        CreateText(
            row,
            $"{slotIndex + 1}. 空き枠",
            rowFont,
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -58f),
            new Vector2(-18f, -28f),
            mutedTextColor);
    }

    private static string BuildMercenaryDetails(MercenaryInstance mercenary)
    {
        return
            $"レベル {mercenary.Level}  経験値 " +
            $"{mercenary.CurrentExperience}/{mercenary.ExperienceToNextLevel}  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";
    }
}
