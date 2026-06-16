using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "DungeonMerchant/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "Unknown Item";
    public ItemType itemType = ItemType.Material;
    public ItemRarity rarity = ItemRarity.Common;

    [TextArea]
    public string description;

    [Header("Economy")]
    [Min(0)] public int basePrice = 20;
}
