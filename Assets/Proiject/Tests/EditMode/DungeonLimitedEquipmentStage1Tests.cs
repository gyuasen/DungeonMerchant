using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class DungeonLimitedEquipmentStage1Tests
{
    [TestCase("NornVerdantSettlement", 6, EquipmentSetId.NornVerdantSettlement, "item.dungeon.norn_verdant_chieftain_hatchet", EquipmentEffectType.LowHpDamageBonus, .12f, EnemyRace.Unknown)]
    [TestCase("GlaadDragonScaleCanyon", 7, EquipmentSetId.GlaadDragonScaleCanyon, "item.dungeon.glaad_reversed_scale_lance", EquipmentEffectType.RaceDamageBonus, .14f, EnemyRace.Dragon)]
    [TestCase("VelmFurnaceDefenseZone", 8, EquipmentSetId.VelmFurnaceDefenseZone, "item.dungeon.velm_great_furnace_siege_hammer", EquipmentEffectType.RaceDamageBonus, .16f, EnemyRace.Construct)]
    [TestCase("AbyssGatewayThreshold", 9, EquipmentSetId.AbyssGatewayThreshold, "item.dungeon.abyss_gateway_sealing_blade", EquipmentEffectType.RaceDamageBonus, .18f, EnemyRace.Demon)]
    public void ReorganizedDungeon_UsesItsThreeDedicatedEquipmentDrops(string dungeonAsset, int expectedRank, EquipmentSetId expectedSet, string expectedWeaponId, EquipmentEffectType expectedWeaponEffect, float expectedWeaponValue, EnemyRace expectedWeaponRace)
    {
        DungeonDataSO dungeon = Resources.Load<DungeonDataSO>("GameData/Dungeons/" + dungeonAsset);
        Assert.That(dungeon, Is.Not.Null);
        Assert.That(dungeon.limitedEquipmentDrops, Has.Length.EqualTo(3));
        HashSet<EquipmentSlot> slots = new HashSet<EquipmentSlot>();
        foreach (ItemDataSO item in dungeon.limitedEquipmentDrops)
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item.itemType, Is.EqualTo(ItemType.Equipment));
            Assert.That(item.acquisitionType, Is.EqualTo(ItemAcquisitionType.Dungeon));
            Assert.That(item.equipmentRank, Is.EqualTo(expectedRank));
            Assert.That(item.equipmentSet, Is.EqualTo(expectedSet));
            Assert.That(item.allClassesCanEquip, Is.True);
            Assert.That(item.PersistentId, Does.StartWith("item.dungeon."));
            Assert.That(slots.Add(item.equipmentSlot), Is.True);
            Assert.That(WorldMapService.IsDungeonEquipmentRankAllowed(dungeon.nearbyTownIndex, item.equipmentRank), Is.True);
        }
        ItemDataSO weapon = dungeon.limitedEquipmentDrops.Single(item => item.equipmentSlot == EquipmentSlot.Weapon);
        EquipmentEffectDefinition effect = weapon.equipmentEffects.Single();
        Assert.That(weapon.PersistentId, Is.EqualTo(expectedWeaponId));
        Assert.That(effect.type, Is.EqualTo(expectedWeaponEffect));
        Assert.That(effect.value, Is.EqualTo(expectedWeaponValue));
        Assert.That(effect.targetRace, Is.EqualTo(expectedWeaponRace));
    }

    [Test]
    public void NewDedicatedEquipment_HasSpecifiedEffectsAndRankBaseStats()
    {
        AssertEquipment("NornAncientBarkBattlegear", 6, EquipmentSlot.Armor, 50, 5, 14, .030f, EquipmentEffectType.DamageReduction, .07f, 0, EnemyRace.Unknown);
        AssertEquipment("NornVerdantChieftainHatchet", 6, EquipmentSlot.Weapon, 12, 19, 5, .035f, EquipmentEffectType.LowHpDamageBonus, .12f, 0, EnemyRace.Unknown);
        AssertEquipment("NornVerdantOathNecklace", 6, EquipmentSlot.Accessory, 18, 8, 6, .035f, EquipmentEffectType.TurnRegeneration, .02f, 0, EnemyRace.Unknown);
        AssertEquipment("GlaadReversedScaleLance", 7, EquipmentSlot.Weapon, 15, 22, 6, .040f, EquipmentEffectType.RaceDamageBonus, .14f, 0, EnemyRace.Dragon);
        AssertEquipment("GlaadCanyonScaleArmor", 7, EquipmentSlot.Armor, 62, 6, 16, .035f, EquipmentEffectType.BattleStartDefenseBuff, .10f, 3, EnemyRace.Unknown);
        AssertEquipment("GlaadWingchaserDragonMark", 7, EquipmentSlot.Accessory, 22, 9, 7, .040f, EquipmentEffectType.BattleStartAttackBuff, .08f, 3, EnemyRace.Unknown);
        AssertEquipment("VelmFurnaceguardHeatArmor", 8, EquipmentSlot.Armor, 76, 7, 19, .040f, EquipmentEffectType.DamageReduction, .10f, 0, EnemyRace.Unknown);
        AssertEquipment("VelmGreatFurnaceSiegeHammer", 8, EquipmentSlot.Weapon, 18, 25, 7, .045f, EquipmentEffectType.RaceDamageBonus, .16f, 0, EnemyRace.Construct);
        AssertEquipment("VelmEverflameTuningCore", 8, EquipmentSlot.Accessory, 28, 10, 9, .045f, EquipmentEffectType.BattleStartDefenseBuff, .12f, 3, EnemyRace.Unknown);
        AssertEquipment("AbyssGatewardenBlackGarb", 9, EquipmentSlot.Armor, 90, 8, 22, .045f, EquipmentEffectType.BattleStartDefenseBuff, .15f, 3, EnemyRace.Unknown);
        AssertEquipment("AbyssGatewaySealingBlade", 9, EquipmentSlot.Weapon, 22, 28, 8, .050f, EquipmentEffectType.RaceDamageBonus, .18f, 0, EnemyRace.Demon);
        AssertEquipment("AbyssChainSealRing", 9, EquipmentSlot.Accessory, 34, 13, 11, .050f, EquipmentEffectType.RaceDamageBonus, .15f, 0, EnemyRace.Dragon);
    }

    private static void AssertEquipment(string assetName, int rank, EquipmentSlot slot, int hp, int attack, int defense, float speed, EquipmentEffectType effectType, float effectValue, int duration, EnemyRace targetRace)
    {
        ItemDataSO item = Resources.Load<ItemDataSO>("GameData/Items/" + assetName);
        Assert.That(item, Is.Not.Null, assetName);
        Assert.That(new[] { item.equipmentRank, (int)item.equipmentSlot, item.bonusMaxHP, item.bonusAttack, item.bonusDefense }, Is.EqualTo(new[] { rank, (int)slot, hp, attack, defense }), assetName);
        Assert.That(item.bonusAttackSpeed, Is.EqualTo(speed));
        EquipmentEffectDefinition effect = item.equipmentEffects.Single();
        Assert.That(effect.type, Is.EqualTo(effectType));
        Assert.That(effect.value, Is.EqualTo(effectValue));
        Assert.That(effect.durationTurns, Is.EqualTo(duration));
        Assert.That(effect.targetRace, Is.EqualTo(targetRace));
        if (effectType == EquipmentEffectType.LowHpDamageBonus)
        {
            Assert.That(effect.secondaryValue, Is.EqualTo(.30f));
        }
    }
}
