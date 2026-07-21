using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class LowRankDungeonEquipmentStage2Tests
{
    [TestCase("GameData/Dungeons/DungeonData", EquipmentSetId.StartingCave, "item.dungeon.starting_cave_pioneer_moss_blade", EquipmentEffectType.None, EnemyRace.Unknown)]
    [TestCase("Dungeons/LeafForestTrail", EquipmentSetId.LeafForestTrail, "item.dungeon.leaf_packripper_fang_sword", EquipmentEffectType.RaceDamageBonus, EnemyRace.Beast)]
    [TestCase("Dungeons/EldUndergroundWaterway", EquipmentSetId.EldUndergroundWaterway, "item.dungeon.eld_watergate_warden_hatchet", EquipmentEffectType.RaceDamageBonus, EnemyRace.Humanoid)]
    public void LowRankDungeon_UsesOnlyItsDedicatedRankThreeSet(string resourcePath, EquipmentSetId expectedSet, string weaponId, EquipmentEffectType weaponEffect, EnemyRace targetRace)
    {
        DungeonDataSO dungeon = Resources.Load<DungeonDataSO>(resourcePath);
        Assert.That(dungeon, Is.Not.Null);
        Assert.That(dungeon.limitedEquipmentDrops, Has.Length.EqualTo(3));
        Assert.That(dungeon.limitedEquipmentDrops.Select(item => item.equipmentSlot), Is.EquivalentTo(new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Accessory }));
        foreach (ItemDataSO item in dungeon.limitedEquipmentDrops)
        {
            Assert.That(item.acquisitionType, Is.EqualTo(ItemAcquisitionType.Dungeon));
            Assert.That(item.equipmentRank, Is.EqualTo(3));
            Assert.That(item.equipmentSet, Is.EqualTo(expectedSet));
            Assert.That(item.PersistentId, Does.StartWith("item.dungeon."));
            Assert.That(WorldMapService.IsDungeonEquipmentRankAllowed(dungeon.nearbyTownIndex, item.equipmentRank), Is.True);
        }
        ItemDataSO weapon = dungeon.limitedEquipmentDrops.Single(item => item.equipmentSlot == EquipmentSlot.Weapon);
        Assert.That(weapon.PersistentId, Is.EqualTo(weaponId));
        if (weaponEffect == EquipmentEffectType.None)
        {
            Assert.That(weapon.equipmentEffects, Is.Null.Or.Empty);
        }
        else
        {
            EquipmentEffectDefinition effect = weapon.equipmentEffects.Single();
            Assert.That(effect.type, Is.EqualTo(weaponEffect));
            Assert.That(effect.value, Is.EqualTo(.08f));
            Assert.That(effect.targetRace, Is.EqualTo(targetRace));
        }
    }

    [Test]
    public void DedicatedEquipment_UsesRankThreeBaseStatsAndSpecifiedEffects()
    {
        AssertEquipment("StartingCavePioneerMossBlade", EquipmentSlot.Weapon, 6, 13, 2, .020f, EquipmentEffectType.None, 0, EnemyRace.Unknown);
        AssertEquipment("StartingCaveLampTravelWear", EquipmentSlot.Armor, 18, 2, 7, .015f, EquipmentEffectType.None, 0, EnemyRace.Unknown);
        AssertEquipment("StartingCaveOriginDropletCharm", EquipmentSlot.Accessory, 9, 5, 3, .020f, EquipmentEffectType.TurnRegeneration, 0, EnemyRace.Unknown);
        AssertEquipment("LeafPackripperFangSword", EquipmentSlot.Weapon, 6, 13, 2, .020f, EquipmentEffectType.RaceDamageBonus, 0, EnemyRace.Beast);
        AssertEquipment("LeafHidingHuntWear", EquipmentSlot.Armor, 18, 2, 7, .015f, EquipmentEffectType.None, 0, EnemyRace.Unknown);
        AssertEquipment("LeafHowlSilencingBell", EquipmentSlot.Accessory, 9, 5, 3, .020f, EquipmentEffectType.None, 0, EnemyRace.Unknown);
        AssertEquipment("EldWatergateWardenHatchet", EquipmentSlot.Weapon, 6, 13, 2, .020f, EquipmentEffectType.RaceDamageBonus, 0, EnemyRace.Humanoid);
        AssertEquipment("EldStagnantWaterCloak", EquipmentSlot.Armor, 18, 2, 7, .015f, EquipmentEffectType.BattleStartDefenseBuff, 2, EnemyRace.Unknown);
        AssertEquipment("EldBlueRustWaterwayKey", EquipmentSlot.Accessory, 9, 5, 3, .020f, EquipmentEffectType.None, 0, EnemyRace.Unknown);
    }

    private static void AssertEquipment(string assetName, EquipmentSlot slot, int hp, int attack, int defense, float speed, EquipmentEffectType effectType, int duration, EnemyRace race)
    {
        ItemDataSO item = Resources.Load<ItemDataSO>("GameData/Items/" + assetName);
        Assert.That(item, Is.Not.Null, assetName);
        Assert.That(item.equipmentRank, Is.EqualTo(3));
        Assert.That(item.equipmentSlot, Is.EqualTo(slot));
        Assert.That(new[] { item.bonusMaxHP, item.bonusAttack, item.bonusDefense }, Is.EqualTo(new[] { hp, attack, defense }));
        Assert.That(item.bonusAttackSpeed, Is.EqualTo(speed));
        if (effectType == EquipmentEffectType.None) Assert.That(item.equipmentEffects, Is.Null.Or.Empty);
        else
        {
            EquipmentEffectDefinition effect = item.equipmentEffects.Single();
            Assert.That(effect.type, Is.EqualTo(effectType));
            float expectedValue = effectType == EquipmentEffectType.TurnRegeneration
                ? .01f
                : effectType == EquipmentEffectType.BattleStartDefenseBuff
                    ? .05f
                    : .08f;
            Assert.That(effect.value, Is.EqualTo(expectedValue));
            Assert.That(effect.durationTurns, Is.EqualTo(duration));
            Assert.That(effect.targetRace, Is.EqualTo(race));
        }
    }
}
