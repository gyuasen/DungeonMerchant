using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryPageUI : EconomyPageUI
{
    private Func<IEnumerable<InventoryItemStack>> itemProvider;
    private Func<IEnumerable<EquipmentInstance>> equipmentProvider;
    private Func<InventoryItemStack, bool> shouldShowItem;
    private Func<EquipmentInstance, bool> shouldShowEquipment;
    private Action<RectTransform, InventoryItemStack, float> createItemRow;
    private Action<RectTransform, EquipmentInstance, float> createEquipmentRow;
    private Action<RectTransform, string> createEmptyMessage;

    public void ConfigureInventory(
        Font font,
        Color color,
        Func<IEnumerable<InventoryItemStack>> items,
        Func<IEnumerable<EquipmentInstance>> equipment,
        Func<InventoryItemStack, bool> itemFilter,
        Func<EquipmentInstance, bool> equipmentFilter,
        Action<RectTransform, InventoryItemStack, float> itemRowFactory,
        Action<RectTransform, EquipmentInstance, float> equipmentRowFactory,
        Action<RectTransform, string> emptyFactory)
    {
        Configure(font, color, null);
        itemProvider = items;
        equipmentProvider = equipment;
        shouldShowItem = itemFilter;
        shouldShowEquipment = equipmentFilter;
        createItemRow = itemRowFactory;
        createEquipmentRow = equipmentRowFactory;
        createEmptyMessage = emptyFactory;
    }

    public override void Refresh()
    {
        ClearChildren(ListRoot);
        ListRoot.sizeDelta = new Vector2(0f, 430f);

        float rowTop = 0f;
        bool createdAnyRow = false;
        foreach (InventoryItemStack stack in
                 itemProvider?.Invoke() ?? Array.Empty<InventoryItemStack>())
        {
            if (shouldShowItem != null && !shouldShowItem(stack))
            {
                continue;
            }

            createItemRow?.Invoke(ListRoot, stack, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        foreach (EquipmentInstance equipment in
                 equipmentProvider?.Invoke() ?? Array.Empty<EquipmentInstance>())
        {
            if (shouldShowEquipment != null &&
                !shouldShowEquipment(equipment))
            {
                continue;
            }

            createEquipmentRow?.Invoke(ListRoot, equipment, rowTop);
            rowTop -= 112f;
            createdAnyRow = true;
        }

        if (!createdAnyRow)
        {
            createEmptyMessage?.Invoke(ListRoot, "在庫はありません。");
            return;
        }

        ListRoot.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }
}
