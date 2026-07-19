using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class BalanceExpansionSpecialEquipmentTests
{
    [Test]
    public void EveryImplementedEquipmentEffectType_ExistsInResources()
    {
        EquipmentEffectType[] expected = { EquipmentEffectType.BattleStartAttackBuff, EquipmentEffectType.BattleStartDefenseBuff, EquipmentEffectType.TurnRegeneration, EquipmentEffectType.DamageReduction, EquipmentEffectType.LowHpDamageBonus };
        EquipmentEffectType[] actual = Resources.LoadAll<ItemDataSO>("GameData/Items").Where(item => item.equipmentEffects != null).SelectMany(item => item.equipmentEffects).Where(effect => effect != null && effect.type != EquipmentEffectType.None && effect.type != EquipmentEffectType.RaceDamageBonus).Select(effect => effect.type).Distinct().ToArray();
        CollectionAssert.IsSubsetOf(expected, actual);
    }

    [Test]
    public void ExistingEquipmentEffectAssignments_MatchGeneratedAssets()
    {
        foreach (ExistingEquipmentEffectAssignment assignment in BalanceExpansionDefinition.ExistingEquipmentEffects)
        {
            EquipmentEffectDefinition actual = Resources.Load<ItemDataSO>(assignment.ResourcePath).equipmentEffects.Single(effect => effect.type == assignment.EquipmentEffectDefinition.type);
            Assert.That(actual.value, Is.EqualTo(assignment.EquipmentEffectDefinition.value));
            Assert.That(actual.secondaryValue, Is.EqualTo(assignment.EquipmentEffectDefinition.secondaryValue));
            Assert.That(actual.durationTurns, Is.EqualTo(assignment.EquipmentEffectDefinition.durationTurns));
        }
    }

    [Test]
    public void TurnRegenerationTotal_IsCappedAtFivePercent()
    {
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.EquipEquipment(CreateEquipment(EquipmentSlot.Weapon, EquipmentEffectType.TurnRegeneration, 0.025f));
        mercenary.EquipEquipment(CreateEquipment(EquipmentSlot.Armor, EquipmentEffectType.TurnRegeneration, 0.025f));
        mercenary.EquipEquipment(CreateEquipment(EquipmentSlot.Accessory, EquipmentEffectType.TurnRegeneration, 0.025f));
        Assert.That(mercenary.GetEquipmentEffectTotal(EquipmentEffectType.TurnRegeneration), Is.EqualTo(0.05f));
    }

    [Test]
    public void LowHpDamageBonus_ActivatesBelowButNotAtThreshold()
    {
        BattleEquipmentEffectSnapshot effects = new BattleEquipmentEffectSnapshot(1f, 0.20f, 0.30f, 1f, 0, 0f, 0, 0f, 0);
        BattleUnit atThreshold = new BattleUnit("At Threshold", maxHP: 100, currentHP: 30, attack: 100, defense: 0, attackSpeed: 1f, isPlayerSide: true);
        BattleUnit belowThreshold = new BattleUnit("Below Threshold", maxHP: 100, currentHP: 29, attack: 100, defense: 0, attackSpeed: 1f, isPlayerSide: true);
        atThreshold.ApplyEquipmentEffects(effects);
        belowThreshold.ApplyEquipmentEffects(effects);
        Assert.That(atThreshold.CalculateDamage(), Is.EqualTo(100));
        Assert.That(belowThreshold.CalculateDamage(), Is.EqualTo(120));
    }

    [Test]
    public void OpeningEquipmentBuff_DoesNotAddToConsumableBuff()
    {
        BattleUnit unit = new BattleUnit("Tester", maxHP: 100, currentHP: 100, attack: 100, defense: 0, attackSpeed: 1f, isPlayerSide: true);
        unit.ApplyEquipmentEffects(new BattleEquipmentEffectSnapshot(1f, 0f, 0f, 1f, 0, 0.15f, 3, 0f, 0));
        unit.BoostAttackForBattle(0.20f);
        Assert.That(unit.Attack, Is.EqualTo(120));
    }

    [Test]
    public void BaneDefinitions_HaveSpecifiedRanksAcquisitionAndBonuses()
    {
        BalanceExpansionEquipmentDefinition dragon = BalanceExpansionDefinition.Equipment.Single(item => item.Id == "item.expansion.dragonbane");
        BalanceExpansionEquipmentDefinition undead = BalanceExpansionDefinition.Equipment.Single(item => item.Id == "item.expansion.undeadbane");
        BalanceExpansionEquipmentDefinition beast = BalanceExpansionDefinition.Equipment.Single(item => item.Id == "item.expansion.beastbane");
        Assert.That(new[] { dragon.Rank, beast.Rank, undead.Rank }, Is.EqualTo(new[] { 6, 4, 4 }));
        Assert.That(new[] { dragon.AcquisitionType, beast.AcquisitionType, undead.AcquisitionType }, Is.EqualTo(new[] { ItemAcquisitionType.Blacksmith, ItemAcquisitionType.Blacksmith, ItemAcquisitionType.Market }));
        Assert.That(new[] { dragon.RaceDamageBonus, undead.RaceDamageBonus, beast.RaceDamageBonus }, Is.EqualTo(new[] { 0.35f, 0.30f, 0.28f }));
        Assert.That(undead.EnglishName, Is.EqualTo("Undead Purification Ward"));
        Assert.That(undead.JapaneseName, Is.EqualTo("不死祓いの護符"));
    }

    [Test]
    public void BaneRecipes_UseSpecifiedCraftingMaterials()
    {
        EquipmentRecipeSO dragonRecipe = Resources.Load<EquipmentRecipeSO>("GameData/Blacksmith/Expansion/item_expansion_dragonbaneRecipe");
        EquipmentRecipeSO beastRecipe = Resources.Load<EquipmentRecipeSO>("GameData/Blacksmith/Expansion/item_expansion_beastbaneRecipe");
        Assert.That(dragonRecipe.goldCost, Is.EqualTo(540));
        Assert.That(beastRecipe.goldCost, Is.EqualTo(400));
        Assert.That(dragonRecipe.materials[0].item.PersistentId, Is.EqualTo("item.LizardScale"));
        Assert.That(dragonRecipe.materials[0].amount, Is.EqualTo(3));
        Assert.That(beastRecipe.materials[0].item.PersistentId, Is.EqualTo("item.MonsterFang"));
        Assert.That(beastRecipe.materials[0].amount, Is.EqualTo(4));
        Assert.That(dragonRecipe.materials.Skip(1).All(material => material.item != null && material.item.materialClassification == MaterialClassification.CraftingMaterial), Is.True);
        Assert.That(beastRecipe.materials.Skip(1).All(material => material.item != null && material.item.materialClassification == MaterialClassification.CraftingMaterial), Is.True);
    }

    [Test]
    public void UndeadPurificationWard_IsFixedStockAtTownIndexZero()
    {
        GameObject root = new GameObject("Ward Stock Test");
        MarketStockManager market = root.AddComponent<MarketStockManager>();
        market.SetTownIndex(0);
        Assert.That(market.Stock.Count(entry => entry.Item.PersistentId == "item.expansion.undeadbane" && entry.Quantity == 1), Is.EqualTo(1));
        Object.DestroyImmediate(root);
    }

    [Test]
    public void BaneDamageBonuses_ResolveWithDefenseOneHundred()
    {
        Assert.That(PreviewBaneDamage(EnemyRace.Beast, 1.28f), Is.EqualTo(64));
        Assert.That(PreviewBaneDamage(EnemyRace.Undead, 1.30f), Is.EqualTo(65));
        Assert.That(PreviewBaneDamage(EnemyRace.Dragon, 1.35f), Is.EqualTo(68));
    }

    private static MercenaryInstance CreateMercenary()
    {
        MercenaryDataSO data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryClass = MercenaryClass.Warrior;
        data.maxHP = 100;
        data.attack = 10;
        data.defense = 3;
        data.attackSpeed = 1f;
        return new MercenaryInstance(data);
    }

    private static ItemDataSO CreateEquipment(EquipmentSlot slot, EquipmentEffectType type, float value)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemType = ItemType.Equipment;
        item.equipmentSlot = slot;
        item.allClassesCanEquip = true;
        item.equipmentEffects = new[] { new EquipmentEffectDefinition { type = type, value = value } };
        return item;
    }

    private static int PreviewBaneDamage(EnemyRace race, float multiplier)
    {
        BattleUnit attacker = new BattleUnit("Attacker", maxHP: 100, currentHP: 1, attack: 0, defense: 0, attackSpeed: 1f, isPlayerSide: true);
        BattleUnit target = new BattleUnit("Target", maxHP: 1000, currentHP: 1000, attack: 1, defense: 100, attackSpeed: 1f, isPlayerSide: false, race: race);
        attacker.ApplyEquipmentEffects(new BattleEquipmentEffectSnapshot(1f, 0f, 0f, 1f, 0, 0f, 0, 0f, 0, new Dictionary<EnemyRace, float> { { race, multiplier } }));
        return DamageResolver.PreviewDamage(new DamageRequest(100, DamageType.Physical, false, attacker, target, true));
    }
}
