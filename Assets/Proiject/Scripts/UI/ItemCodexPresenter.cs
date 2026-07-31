using System.Collections.Generic;
using UnityEngine;

public sealed class ItemCodexPresenter
{
    private readonly MerchantInventory merchantInventory;

    public ItemCodexPresenter(MerchantInventory merchantInventory)
    {
        this.merchantInventory = merchantInventory;
    }

    public List<BookPageUI.Entry> BuildEntries()
    {
        List<ItemDataSO> items = new List<ItemDataSO>();
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item != null &&
                !item.IsEquipment &&
                (item.hideFlags & HideFlags.DontSave) == 0 &&
                !string.IsNullOrWhiteSpace(item.PersistentId))
            {
                items.Add(item);
            }
        }

        items.Sort((left, right) =>
        {
            int typeComparison = left.itemType.CompareTo(right.itemType);
            return typeComparison != 0
                ? typeComparison
                : string.Compare(
                    JapaneseDisplayText.GetItemName(left),
                    JapaneseDisplayText.GetItemName(right),
                    System.StringComparison.CurrentCulture);
        });

        List<BookPageUI.Entry> entries = new List<BookPageUI.Entry>();
        foreach (ItemDataSO item in items)
        {
            entries.Add(new BookPageUI.Entry
            {
                Name = JapaneseDisplayText.GetItemName(item),
                Detail = BuildDetail(item),
                Sprite = ItemPresentationService.ResolveSprite(item),
                Discovered = merchantInventory != null &&
                    merchantInventory.HasDiscoveredItem(item)
            });
        }

        return entries;
    }

    private static string BuildDetail(ItemDataSO item)
    {
        string category = item.itemType == ItemType.Consumable
            ? "消耗品"
            : item.itemType == ItemType.Material
                ? "素材"
                : "アイテム";
        string description = string.IsNullOrWhiteSpace(item.description)
            ? "説明なし"
            : item.description;
        return $"{category} / 基準価格 {item.basePrice:N0}G\n{description}";
    }
}
