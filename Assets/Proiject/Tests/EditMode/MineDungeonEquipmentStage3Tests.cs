using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class MineDungeonEquipmentStage3Tests
{
    [TestCase("GameData/Dungeons/LowerMine", EquipmentSetId.LowerMine, "item.dungeon.lower_mine_tunnelbreaker_hammer", EquipmentEffectType.BattleStartAttackBuff, .07f, 2, EnemyRace.Unknown)]
    [TestCase("Dungeons/EldOldQuarry", EquipmentSetId.EldOldQuarry, "item.dungeon.eld_soulmason_iron_hammer", EquipmentEffectType.RaceDamageBonus, .10f, 0, EnemyRace.Undead)]
    public void MineDungeon_UsesDedicatedRankFourEquipment(string resourcePath, EquipmentSetId expectedSet, string weaponId, EquipmentEffectType effectType, float effectValue, int duration, EnemyRace race)
    {
        DungeonDataSO dungeon = Resources.Load<DungeonDataSO>(resourcePath);
        Assert.That(dungeon, Is.Not.Null);
        Assert.That(dungeon.limitedEquipmentDrops, Has.Length.EqualTo(3));
        Assert.That(dungeon.limitedEquipmentDrops.Select(item => item.equipmentSlot), Is.EquivalentTo(new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Accessory }));
        foreach (ItemDataSO item in dungeon.limitedEquipmentDrops)
        {
            Assert.That(item.acquisitionType, Is.EqualTo(ItemAcquisitionType.Dungeon));
            Assert.That(item.equipmentRank, Is.EqualTo(4));
            Assert.That(item.equipmentSet, Is.EqualTo(expectedSet));
            Assert.That(WorldMapService.IsDungeonEquipmentRankAllowed(dungeon.nearbyTownIndex, item.equipmentRank), Is.True);
        }
        ItemDataSO weapon = dungeon.limitedEquipmentDrops.Single(item => item.equipmentSlot == EquipmentSlot.Weapon);
        EquipmentEffectDefinition effect = weapon.equipmentEffects.Single();
        Assert.That(weapon.PersistentId, Is.EqualTo(weaponId));
        Assert.That(effect.type, Is.EqualTo(effectType));
        Assert.That(effect.value, Is.EqualTo(effectValue));
        Assert.That(effect.durationTurns, Is.EqualTo(duration));
        Assert.That(effect.targetRace, Is.EqualTo(race));
    }

    [Test]
    public void DedicatedEquipment_UsesRankFourBaseStatsAndSpecifiedEffects()
    {
        AssertEquipment("LowerMineTunnelbreakerHammer", EquipmentSlot.Weapon, 8, 14, 3, .025f, EquipmentEffectType.BattleStartAttackBuff, .07f, 2, EnemyRace.Unknown);
        AssertEquipment("LowerMineClosedMinerHeavyWear", EquipmentSlot.Armor, 30, 3, 10, .020f, EquipmentEffectType.DamageReduction, .04f, 0, EnemyRace.Unknown);
        AssertEquipment("LowerMineLampeaterMinerLamp", EquipmentSlot.Accessory, 12, 6, 4, .025f, EquipmentEffectType.None, 0f, 0, EnemyRace.Unknown);
        AssertEquipment("EldSoulmasonIronHammer", EquipmentSlot.Weapon, 8, 14, 3, .025f, EquipmentEffectType.RaceDamageBonus, .10f, 0, EnemyRace.Undead);
        AssertEquipment("EldTombstoneWardenBreastplate", EquipmentSlot.Armor, 30, 3, 10, .020f, EquipmentEffectType.BattleStartDefenseBuff, .07f, 2, EnemyRace.Unknown);
        AssertEquipment("EldCorpseKingSealingStoneRing", EquipmentSlot.Accessory, 12, 6, 4, .025f, EquipmentEffectType.TurnRegeneration, .01f, 0, EnemyRace.Unknown);
    }

    private static void AssertEquipment(string assetName, EquipmentSlot slot, int hp, int attack, int defense, float speed, EquipmentEffectType effectType, float effectValue, int duration, EnemyRace race)
    {
        ItemDataSO item = Resources.Load<ItemDataSO>("GameData/Items/" + assetName);
        Assert.That(item, Is.Not.Null, assetName);
        Assert.That(item.equipmentRank, Is.EqualTo(4));
        Assert.That(item.equipmentSlot, Is.EqualTo(slot));
        Assert.That(new[] { item.bonusMaxHP, item.bonusAttack, item.bonusDefense }, Is.EqualTo(new[] { hp, attack, defense }));
        Assert.That(item.bonusAttackSpeed, Is.EqualTo(speed));
        if (effectType == EquipmentEffectType.None) Assert.That(item.equipmentEffects, Is.Null.Or.Empty);
        else
        {
            EquipmentEffectDefinition effect = item.equipmentEffects.Single();
            Assert.That(effect.type, Is.EqualTo(effectType));
            Assert.That(effect.value, Is.EqualTo(effectValue));
            Assert.That(effect.durationTurns, Is.EqualTo(duration));
            Assert.That(effect.targetRace, Is.EqualTo(race));
        }
    }
}
