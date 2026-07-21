using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class ExistingDungeonEquipmentStage4Tests
{
    [TestCase("GameData/Dungeons/MiddleRuins", EquipmentSetId.MiddleRuins, "MistRuneBlade", EquipmentEffectType.RaceDamageBonus, .10f, 0, EnemyRace.Construct)]
    [TestCase("Dungeons/NornCanopyLabyrinth", EquipmentSetId.NornCanopy, "NornCanopyBlade", EquipmentEffectType.RaceDamageBonus, .12f, 0, EnemyRace.Construct)]
    [TestCase("Dungeons/GlaadSkyFortress", EquipmentSetId.GlaadSkyFortress, "GlaadFrostbrand", EquipmentEffectType.RaceDamageBonus, .15f, 0, EnemyRace.Dragon)]
    [TestCase("Dungeons/VelmBlackIronMine", EquipmentSetId.VelmBlackIron, "VelmBlackIronBreaker", EquipmentEffectType.RaceDamageBonus, .16f, 0, EnemyRace.Construct)]
    [TestCase("Dungeons/FinalBlackSoilAbyss", EquipmentSetId.AbyssThrone, "AbyssFang", EquipmentEffectType.LowHpDamageBonus, .20f, 0, EnemyRace.Unknown)]
    public void ExistingDungeon_DropsOnlyItsThreeSetItemsWithSpecifiedWeaponEffect(string path, EquipmentSetId expectedSet, string weaponName, EquipmentEffectType effectType, float value, int duration, EnemyRace race)
    {
        DungeonDataSO dungeon = Resources.Load<DungeonDataSO>(path);
        Assert.That(dungeon, Is.Not.Null);
        Assert.That(dungeon.limitedEquipmentDrops, Has.Length.EqualTo(3));
        Assert.That(dungeon.limitedEquipmentDrops.Select(item => item.equipmentSet).Distinct(), Is.EquivalentTo(new[] { expectedSet }));
        ItemDataSO weapon = dungeon.limitedEquipmentDrops.Single(item => item.name == weaponName);
        EquipmentEffectDefinition effect = weapon.equipmentEffects.Single();
        Assert.That(effect.type, Is.EqualTo(effectType));
        Assert.That(effect.value, Is.EqualTo(value));
        Assert.That(effect.durationTurns, Is.EqualTo(duration));
        Assert.That(effect.targetRace, Is.EqualTo(race));
        if (effectType == EquipmentEffectType.LowHpDamageBonus) Assert.That(effect.secondaryValue, Is.EqualTo(.30f));
    }

    [Test]
    public void ExistingDungeonEquipment_HasAllSpecifiedNonWeaponEffectsWithoutDuplicates()
    {
        AssertEffect("RuinweaveMantle", EquipmentEffectType.DamageReduction, .06f, 0, EnemyRace.Unknown);
        AssertEffect("GuardianEyeCharm", EquipmentEffectType.BattleStartDefenseBuff, .08f, 3, EnemyRace.Unknown);
        AssertEffect("NornBarkguard", EquipmentEffectType.DamageReduction, .08f, 0, EnemyRace.Unknown);
        AssertEffect("NornVerdantCharm", EquipmentEffectType.TurnRegeneration, .025f, 0, EnemyRace.Unknown);
        AssertEffect("GlaadWardenPlate", EquipmentEffectType.DamageReduction, .09f, 0, EnemyRace.Unknown);
        AssertEffect("GlaadSummitSigil", EquipmentEffectType.BattleStartAttackBuff, .10f, 3, EnemyRace.Unknown);
        AssertEffect("VelmDeepforgeArmor", EquipmentEffectType.DamageReduction, .11f, 0, EnemyRace.Unknown);
        AssertEffect("VelmEmberCore", EquipmentEffectType.LowHpDamageBonus, .16f, 0, EnemyRace.Unknown);
        AssertEffect("AbyssMantle", EquipmentEffectType.DamageReduction, .12f, 0, EnemyRace.Unknown);
        AssertEffect("AbyssSeal", EquipmentEffectType.RaceDamageBonus, .18f, 0, EnemyRace.Demon);
    }

    private static void AssertEffect(string assetName, EquipmentEffectType type, float value, int duration, EnemyRace race)
    {
        ItemDataSO item = Resources.Load<ItemDataSO>("GameData/Items/" + assetName);
        Assert.That(item, Is.Not.Null, assetName);
        Assert.That(item.equipmentEffects, Has.Length.EqualTo(1), assetName);
        EquipmentEffectDefinition effect = item.equipmentEffects[0];
        Assert.That(effect.type, Is.EqualTo(type), assetName);
        Assert.That(effect.value, Is.EqualTo(value), assetName);
        Assert.That(effect.durationTurns, Is.EqualTo(duration), assetName);
        Assert.That(effect.targetRace, Is.EqualTo(race), assetName);
        if (type == EquipmentEffectType.LowHpDamageBonus) Assert.That(effect.secondaryValue, Is.EqualTo(.30f), assetName);
    }
}
