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
    public ItemAcquisitionType acquisitionType = ItemAcquisitionType.Market;

    [Header("Equipment")]
    public EquipmentSlot equipmentSlot = EquipmentSlot.Weapon;
    public MercenaryClass requiredClass = MercenaryClass.Warrior;
    public bool allClassesCanEquip;
    public EquipmentSetId equipmentSet = EquipmentSetId.None;
    [Min(1)] public int equipmentRank = 1;
    public int bonusMaxHP;
    public int bonusAttack;
    public int bonusDefense;
    public float bonusAttackSpeed;

    public bool IsEquipment => itemType == ItemType.Equipment;

    public bool CanEquip(MercenaryClass mercenaryClass)
    {
        return IsEquipment &&
               (allClassesCanEquip || requiredClass == mercenaryClass);
    }
}

public enum EquipmentSetId
{
    None,
    AncientGuardian,
    Vanguard,
    Windstalker,
    ArcaneSage
}
