using NUnit.Framework;

// Action 4.2 の回帰ガード。MarketStockManager / BlacksmithManager にあった
// 町別 availability switch 文を WorldMapService のテーブルへ置き換えた際、
// 出力が完全一致することを検証する。
//
// Oracle*Rule メソッドは置き換え前の switch 文（git 履歴上の
// MarketStockManager.IsAvailableInCurrentTown / BlacksmithManager.
// IsRecipeAvailableInCurrentTown）からの逐語転記であり、新テーブルとは
// 独立に「元の真実」を表す。テーブル側を変更する場合は、意図的な仕様変更で
// ある場合に限り、このオラクルも同時に更新すること。
public sealed class TownAvailabilityParityTests
{
    private static readonly int[] TownIndices = { 0, 1, 2, 3, 4, 5, 6, 7, -1 };
    private static readonly MercenaryClass[] BaseClasses =
    {
        MercenaryClass.Warrior,
        MercenaryClass.Archer,
        MercenaryClass.Mage,
        MercenaryClass.Priest,
        MercenaryClass.Rogue,
        MercenaryClass.Lancer
    };
    private static readonly int[] Ranks = { 0, 1, 2, 3, 4 };
    private static readonly EquipmentSlot[] Slots =
    {
        EquipmentSlot.Weapon,
        EquipmentSlot.Armor,
        EquipmentSlot.Accessory
    };

    [Test]
    public void MarketTable_MatchesOriginalSwitch_ForAllCombinations()
    {
        foreach (int town in TownIndices)
        foreach (MercenaryClass itemClass in BaseClasses)
        foreach (int rank in Ranks)
        foreach (EquipmentSlot slot in Slots)
        {
            bool expected = OracleMarketRule(town, itemClass, rank, slot);
            bool actual = WorldMapService.IsMarketEquipmentAllowedInTown(
                town, itemClass, rank, slot);
            Assert.That(
                actual,
                Is.EqualTo(expected),
                $"market town={town} class={itemClass} rank={rank} slot={slot}");
        }
    }

    [Test]
    public void BlacksmithTable_MatchesOriginalSwitch_ForAllCombinations()
    {
        foreach (int town in TownIndices)
        foreach (MercenaryClass itemClass in BaseClasses)
        foreach (int rank in Ranks)
        foreach (EquipmentSlot slot in Slots)
        {
            bool expected = OracleBlacksmithRule(town, itemClass, rank, slot);
            bool actual = WorldMapService.IsBlacksmithEquipmentAllowedInTown(
                town, itemClass, rank, slot);
            Assert.That(
                actual,
                Is.EqualTo(expected),
                $"blacksmith town={town} class={itemClass} rank={rank} slot={slot}");
        }
    }

    // 旧 MarketStockManager.IsAvailableInCurrentTown の switch を逐語転記。
    private static bool OracleMarketRule(
        int currentTownIndex,
        MercenaryClass itemClass,
        int equipmentRank,
        EquipmentSlot equipmentSlot)
    {
        switch (currentTownIndex)
        {
            case 2:
                return equipmentRank <= 1;
            case 1:
                return equipmentRank <= 1 ||
                       itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Rogue;
            case 0:
                return true;
            case 3:
                return itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Priest ||
                       equipmentSlot == EquipmentSlot.Accessory;
            case 4:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Lancer ||
                       itemClass == MercenaryClass.Priest;
            case 5:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Lancer ||
                       equipmentRank >= 2;
            default:
                return equipmentRank >= 2 ||
                       equipmentSlot == EquipmentSlot.Accessory;
        }
    }

    // 旧 BlacksmithManager.IsRecipeAvailableInCurrentTown の switch を逐語転記
    // （"Mutant Core Charm" の特例はマネージャー側に残存しているため対象外）。
    private static bool OracleBlacksmithRule(
        int currentTownIndex,
        MercenaryClass itemClass,
        int equipmentRank,
        EquipmentSlot equipmentSlot)
    {
        switch (currentTownIndex)
        {
            case 2:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Mage;
            case 1:
                return itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Rogue ||
                       equipmentSlot == EquipmentSlot.Accessory;
            case 0:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Priest ||
                       itemClass == MercenaryClass.Lancer ||
                       equipmentSlot == EquipmentSlot.Accessory;
            case 3:
                return itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Priest;
            case 4:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Lancer ||
                       equipmentSlot == EquipmentSlot.Armor;
            case 5:
                return itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Rogue ||
                       itemClass == MercenaryClass.Lancer ||
                       equipmentSlot == EquipmentSlot.Weapon;
            default:
                return equipmentRank >= 3;
        }
    }
}
