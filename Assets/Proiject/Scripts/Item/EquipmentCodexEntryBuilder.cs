using System.Collections.Generic;

public enum EquipmentCodexEntryKind
{
    Normal,
    Set,
    HighRankSingle
}

public sealed class EquipmentCodexSetGroup
{
    public EquipmentCodexSetGroup(
        EquipmentSetId setId,
        EquipmentSetDefinition definition,
        List<ItemDataSO> equipment)
    {
        SetId = setId;
        Definition = definition;
        Equipment = equipment;
    }

    public EquipmentSetId SetId { get; }
    public EquipmentSetDefinition Definition { get; }
    public IReadOnlyList<ItemDataSO> Equipment { get; }
}

public sealed class EquipmentCodexEntries
{
    internal EquipmentCodexEntries(List<ItemDataSO> normalEquipment, List<ItemDataSO> highRankSingleEquipment, List<EquipmentCodexSetGroup> setGroups)
    {
        NormalEquipment = normalEquipment;
        HighRankSingleEquipment = highRankSingleEquipment;
        SetGroups = setGroups;
    }

    public IReadOnlyList<ItemDataSO> NormalEquipment { get; }
    public IReadOnlyList<ItemDataSO> HighRankSingleEquipment { get; }
    public IReadOnlyList<EquipmentCodexSetGroup> SetGroups { get; }
}

public static class EquipmentCodexEntryBuilder
{
    public static EquipmentCodexEntryKind Classify(ItemDataSO item)
    {
        if (item != null && item.equipmentSet != EquipmentSetId.None)
        {
            return EquipmentCodexEntryKind.Set;
        }
        return item != null && item.equipmentRank >= 9
            ? EquipmentCodexEntryKind.HighRankSingle
            : EquipmentCodexEntryKind.Normal;
    }

    public static EquipmentCodexEntries Build(IEnumerable<ItemDataSO> items)
    {
        List<ItemDataSO> normalEquipment = new List<ItemDataSO>();
        List<ItemDataSO> highRankSingleEquipment = new List<ItemDataSO>();
        Dictionary<EquipmentSetId, List<ItemDataSO>> equipmentBySet = new Dictionary<EquipmentSetId, List<ItemDataSO>>();
        foreach (ItemDataSO item in items)
        {
            if (item == null || !item.IsEquipment)
            {
                continue;
            }
            switch (Classify(item))
            {
                case EquipmentCodexEntryKind.Set:
                    if (!equipmentBySet.TryGetValue(item.equipmentSet, out List<ItemDataSO> setEquipment))
                    {
                        setEquipment = new List<ItemDataSO>();
                        equipmentBySet.Add(item.equipmentSet, setEquipment);
                    }
                    setEquipment.Add(item);
                    break;
                case EquipmentCodexEntryKind.HighRankSingle:
                    highRankSingleEquipment.Add(item);
                    break;
                default:
                    normalEquipment.Add(item);
                    break;
            }
        }
        List<EquipmentCodexSetGroup> setGroups = new List<EquipmentCodexSetGroup>();
        foreach (KeyValuePair<EquipmentSetId, List<ItemDataSO>> pair in equipmentBySet)
        {
            EquipmentSetCatalog.TryGet(pair.Key, out EquipmentSetDefinition definition);
            setGroups.Add(new EquipmentCodexSetGroup(pair.Key, definition, pair.Value));
        }
        setGroups.Sort((left, right) => GetDisplayOrder(left).CompareTo(GetDisplayOrder(right)));
        return new EquipmentCodexEntries(normalEquipment, highRankSingleEquipment, setGroups);
    }

    private static int GetDisplayOrder(EquipmentCodexSetGroup group)
    {
        return group.Definition != null ? group.Definition.DisplayOrder : int.MaxValue;
    }
}
