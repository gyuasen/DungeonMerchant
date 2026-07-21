using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "DungeonMerchant/Item Data")]
public class ItemDataSO : ScriptableObject, IPersistentGameAsset
{
    [Header("Basic Info")]
    [SerializeField] private string persistentId;
    public string itemName = "Unknown Item";
    public ItemType itemType = ItemType.Material;
    public ItemRarity rarity = ItemRarity.Common;
    public MaterialClassification materialClassification = MaterialClassification.None;

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
    [Range(1, 10)] public int equipmentRank = 1;
    public int bonusMaxHP;
    public int bonusAttack;
    public int bonusDefense;
    public float bonusAttackSpeed;
    public EquipmentEffectDefinition[] equipmentEffects;

    [Header("Consumable")]
    public ConsumableEffectType consumableEffect = ConsumableEffectType.None;
    [Min(0)] public int consumableHealAmount;

    public bool IsEquipment => itemType == ItemType.Equipment;
    public string PersistentId =>
        string.IsNullOrWhiteSpace(persistentId) ? name : persistentId;

    public bool CanEquip(MercenaryClass mercenaryClass)
    {
        return IsEquipment &&
               (allClassesCanEquip ||
                requiredClass ==
                MercenaryClassProgression.GetBaseClass(mercenaryClass));
    }
}

public enum ConsumableEffectType
{
    None,
    CurePoison,
    CureParalysis,
    CureAllStatus,
    HealHP,
    BoostAttack,
    BoostDefense,
    RestoreMagic,
    BoostSpeed
}

public enum MaterialClassification
{
    None,
    SellOnly,
    CraftingMaterial
}

public enum EquipmentSetId
{
    None,
    AncientGuardian,
    Vanguard,
    Windstalker,
    ArcaneSage,
    OniHunter,
    NornCanopy,
    GlaadSkyFortress,
    VelmBlackIron,
    AbyssThrone,
    AstralDepths,
    NornVerdantSettlement,
    GlaadDragonScaleCanyon,
    VelmFurnaceDefenseZone,
    AbyssGatewayThreshold
}

public enum EquipmentEffectType
{
    None = 0,
    BattleStartAttackBuff = 10,
    BattleStartDefenseBuff = 20,
    TurnRegeneration = 30,
    DamageReduction = 40,
    LowHpDamageBonus = 50,
    RaceDamageBonus = 60
}

[Serializable]
public class EquipmentEffectDefinition
{
    public EquipmentEffectType type;
    public float value;
    public float secondaryValue;
    public int durationTurns;
    public EnemyRace targetRace;
}
