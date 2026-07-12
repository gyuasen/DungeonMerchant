using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class PartyPageUI : ListPageUIBase
{
    private Func<int> maxSlotProvider;
    private Func<IReadOnlyList<MercenaryInstance>> memberProvider;
    private Action<MercenaryInstance> removeMemberAction;

    public void Initialize(Text title, RectTransform targetListRoot)
    {
        Initialize(title, null, targetListRoot);
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
            base.Refresh();
            return;
        }

        // Fixed-slot layout with placeholder rows: does not map onto
        // RebuildRows, so the loop stays page-specific.
        ClearChildren(ListRoot);

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
                ListRoot,
                top,
                RowColor,
                FrameColor);
        CreateText(
            row,
            $"{slotIndex + 1}. {mercenary.MercenaryName}",
            RowFont,
            22,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -42f),
            new Vector2(-160f, -12f),
            RowTextColor);

        CreateText(
            row,
            BuildMercenaryDetails(mercenary),
            RowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -76f),
            new Vector2(-160f, -48f),
            MutedTextColor);

        CreateActionButton(
            row,
            "外す",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => removeMemberAction?.Invoke(mercenary));
    }

    private void CreateEmptyPartyRow(int slotIndex, float top)
    {
        RectTransform row =
            CreateRow(
                $"Empty Party Slot {slotIndex + 1}",
                ListRoot,
                top,
                RowColor,
                FrameColor);

        CreateText(
            row,
            $"{slotIndex + 1}. 空き枠",
            RowFont,
            20,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -58f),
            new Vector2(-18f, -28f),
            MutedTextColor);
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
