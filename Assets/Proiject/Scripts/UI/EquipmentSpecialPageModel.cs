using System;
using System.Collections.Generic;
using System.Linq;

public enum EquipmentSpecialPageKind
{
    Set,
    HighRankSingle
}

public sealed class EquipmentSpecialItemModel
{
    public EquipmentSpecialItemModel(ItemDataSO item, bool discovered)
    {
        Item = item;
        Name = JapaneseDisplayText.GetItemName(item);
        Slot = item.equipmentSlot;
        Rank = item.equipmentRank;
        Description = item.description;
        EffectText = EquipmentEffectTextFormatter.FormatList(item.equipmentEffects);
        Discovered = discovered;
    }

    public ItemDataSO Item { get; }
    public string Name { get; }
    public EquipmentSlot Slot { get; }
    public int Rank { get; }
    public string Description { get; }
    public string EffectText { get; }
    public bool Discovered { get; }
}

public sealed class EquipmentSpecialSlotModel
{
    public EquipmentSpecialSlotModel(EquipmentSlot slot, IReadOnlyList<EquipmentSpecialItemModel> candidates)
    {
        Slot = slot;
        Candidates = candidates;
    }

    public EquipmentSlot Slot { get; }
    public IReadOnlyList<EquipmentSpecialItemModel> Candidates { get; }
}

public sealed class EquipmentSpecialPageModel
{
    public EquipmentSpecialPageModel(
        EquipmentSpecialPageKind kind,
        string title,
        UnityEngine.Color accentColor,
        IReadOnlyList<EquipmentSpecialSlotModel> slots,
        EquipmentSpecialItemModel singleItem,
        string setBonusText,
        int discoveredCount,
        int totalCount)
    {
        Kind = kind;
        Title = title;
        AccentColor = accentColor;
        Slots = slots;
        SingleItem = singleItem;
        SetBonusText = setBonusText;
        DiscoveredCount = discoveredCount;
        TotalCount = totalCount;
    }

    public EquipmentSpecialPageKind Kind { get; }
    public string Title { get; }
    public UnityEngine.Color AccentColor { get; }
    public IReadOnlyList<EquipmentSpecialSlotModel> Slots { get; }
    public EquipmentSpecialItemModel SingleItem { get; }
    public string SetBonusText { get; }
    public int DiscoveredCount { get; }
    public int TotalCount { get; }
}

public static class EquipmentSpecialPageModelBuilder
{
    public static IReadOnlyList<EquipmentSpecialPageModel> Build(
        EquipmentCodexEntries entries,
        Func<ItemDataSO, bool> isDiscovered)
    {
        List<EquipmentSpecialPageModel> pages = new List<EquipmentSpecialPageModel>();
        if (entries == null)
        {
            return pages;
        }
        Func<ItemDataSO, bool> discovery = isDiscovered ?? (_ => false);
        foreach (EquipmentCodexSetGroup group in entries.SetGroups)
        {
            pages.Add(BuildSetPage(group, discovery));
        }
        foreach (ItemDataSO item in entries.HighRankSingleEquipment.OrderBy(item => item.equipmentRank).ThenBy(item => JapaneseDisplayText.GetItemName(item), StringComparer.Ordinal))
        {
            pages.Add(BuildSinglePage(item, discovery));
        }
        return pages;
    }

    private static EquipmentSpecialPageModel BuildSetPage(EquipmentCodexSetGroup group, Func<ItemDataSO, bool> isDiscovered)
    {
        List<EquipmentSpecialItemModel> items = group.Equipment.Select(item => new EquipmentSpecialItemModel(item, isDiscovered(item))).ToList();
        List<EquipmentSpecialSlotModel> slots = new List<EquipmentSpecialSlotModel>();
        foreach (EquipmentSlot slot in (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot)))
        {
            List<EquipmentSpecialItemModel> candidates = items.Where(item => item.Slot == slot).OrderBy(item => item.Rank).ThenBy(item => item.Name, StringComparer.Ordinal).ToList();
            slots.Add(new EquipmentSpecialSlotModel(slot, candidates));
        }
        EquipmentSetDefinition definition = group.Definition;
        string title = definition != null ? definition.DisplayName : JapaneseDisplayText.GetEquipmentSet(group.SetId);
        UnityEngine.Color accentColor = definition != null ? definition.AccentColor : UnityEngine.Color.white;
        string bonusText = definition != null ? EquipmentSetCatalog.BuildDetailText(definition.Id) : "セット効果: なし";
        return new EquipmentSpecialPageModel(EquipmentSpecialPageKind.Set, title, accentColor, slots, null, bonusText, items.Count(item => item.Discovered), items.Count);
    }

    private static EquipmentSpecialPageModel BuildSinglePage(ItemDataSO item, Func<ItemDataSO, bool> isDiscovered)
    {
        EquipmentSpecialItemModel singleItem = new EquipmentSpecialItemModel(item, isDiscovered(item));
        return new EquipmentSpecialPageModel(EquipmentSpecialPageKind.HighRankSingle, singleItem.Name, UnityEngine.Color.white, Array.Empty<EquipmentSpecialSlotModel>(), singleItem, string.Empty, singleItem.Discovered ? 1 : 0, 1);
    }
}
