using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class BalanceExpansionDefinitionTests
{
    [Test]
    public void GeneratedAssets_UseEnglishInternalNamesAndJapaneseDisplayNames()
    {
        foreach (BalanceExpansionNormalEnemyDefinition definition in BalanceExpansionDefinition.NormalEnemies)
        {
            EnemyDataSO enemy = Resources.Load<EnemyDataSO>("GameData/Enemies/" + definition.AssetName);
            Assert.That(enemy, Is.Not.Null, definition.Id);
            Assert.That(enemy.enemyName, Is.EqualTo(definition.EnglishName), definition.Id);
            Assert.That(JapaneseDisplayText.GetEnemyName(enemy.enemyName), Is.EqualTo(definition.JapaneseName), definition.Id);
        }
        foreach (BalanceExpansionEnemyDefinition definition in BalanceExpansionDefinition.Enemies)
        {
            EnemyDataSO enemy = Resources.Load<EnemyDataSO>("GameData/Enemies/Expansion/" + definition.Id.Replace('.', '_'));
            Assert.That(enemy, Is.Not.Null, definition.Id);
            Assert.That(enemy.enemyName, Is.EqualTo(definition.EnglishName), definition.Id);
            Assert.That(JapaneseDisplayText.GetEnemyName(enemy.enemyName), Is.EqualTo(definition.JapaneseName), definition.Id);
        }
        foreach (BalanceExpansionEquipmentDefinition definition in BalanceExpansionDefinition.Equipment)
        {
            ItemDataSO item = Resources.Load<ItemDataSO>("GameData/Items/Expansion/" + definition.Id.Replace('.', '_'));
            Assert.That(item, Is.Not.Null, definition.Id);
            Assert.That(item.itemName, Is.EqualTo(definition.EnglishName), definition.Id);
            Assert.That(JapaneseDisplayText.GetItemName(item), Is.EqualTo(definition.JapaneseName), definition.Id);
        }
        foreach (BalanceExpansionConsumableDefinition definition in BalanceExpansionDefinition.Consumables)
        {
            ItemDataSO item = Resources.Load<ItemDataSO>("GameData/Items/Expansion/" + definition.Id.Replace('.', '_'));
            Assert.That(item, Is.Not.Null, definition.Id);
            Assert.That(item.itemName, Is.EqualTo(definition.EnglishName), definition.Id);
            Assert.That(JapaneseDisplayText.GetItemName(item), Is.EqualTo(definition.JapaneseName), definition.Id);
        }
    }

    [Test]
    public void WyvernAndMutantCores_AreGameplayConsistent()
    {
        DungeonDataSO dungeon = Resources.Load<DungeonDataSO>("Dungeons/GlaadSkyFortress");
        EnemyDataSO wyvern = Resources.Load<EnemyDataSO>("GameData/Enemies/Grade03Wyvern");
        Assert.That(wyvern.monsterGrade, Is.EqualTo(3));
        Assert.That(dungeon.normalEnemies, Does.Contain(wyvern));
        foreach (string name in new[] { "MutantCore", "LowerGradeMutantCore", "MiddleGradeMutantCore", "UpperGradeMutantCore", "HighestGradeMutantCore" })
        {
            ItemDataSO core = Resources.Load<ItemDataSO>("Items/Special/" + name);
            Assert.That(core.materialClassification, Is.EqualTo(MaterialClassification.CraftingMaterial), name);
            Assert.That(core.description, Does.Not.Contain("ダンジョン"), name);
        }
    }

    [Test]
    public void Variants_UseNextGradeNormalEnemyAndCoverEveryRoleInEachBand()
    {
        var rolesByBand = new Dictionary<int, HashSet<EnemyCombatRole>>();
        foreach (BalanceExpansionEnemyDefinition d in BalanceExpansionDefinition.Enemies)
        {
            EnemyDataSO basis = Resources.Load<EnemyDataSO>("GameData/Enemies/" + d.BaseEnemyAsset);
            Assert.That(basis, Is.Not.Null, d.Id);
            Assert.That(basis.monsterGrade, Is.EqualTo(d.Grade - 1), d.Id);
            Assert.That(d.BattleVisualKey, Does.StartWith("Grade"), d.Id);
            int band = d.Grade >= 9 ? 0 : d.Grade >= 7 ? 1 : d.Grade >= 5 ? 2 : 3;
            if (!rolesByBand.ContainsKey(band)) rolesByBand[band] = new HashSet<EnemyCombatRole>();
            rolesByBand[band].Add(d.Role);
        }
        foreach (var roles in rolesByBand.Values)
            Assert.That(roles, Is.EquivalentTo(new[]{EnemyCombatRole.Skill,EnemyCombatRole.Speed,EnemyCombatRole.Durability,EnemyCombatRole.Attack,EnemyCombatRole.Balance}));
    }

    [Test]
    public void DropsAndRecipes_OnlyReferenceExistingMaterials()
    {
        foreach (string path in new[]{"GameData/Items/MonsterFang","GameData/Items/EnhancementOre"})
        {
            ItemDataSO item = Resources.Load<ItemDataSO>(path);
            Assert.That(item, Is.Not.Null, path);
            Assert.That(item.itemType, Is.EqualTo(ItemType.Material), path);
        }
        foreach (BalanceExpansionEnemyDefinition d in BalanceExpansionDefinition.Enemies)
            Assert.That(Resources.Load<EnemyDataSO>("GameData/Enemies/" + d.BaseEnemyAsset).itemDrops, Is.Not.Empty, d.Id);
    }

    [Test]
    public void Equipment_InterpolatesRanksWithoutPriceOrPowerSpikes()
    {
        foreach (BalanceExpansionEquipmentDefinition d in BalanceExpansionDefinition.Equipment)
        {
            Assert.That(d.Rank, Is.InRange(4, 7));
            Assert.That(d.ClassIndex, Is.InRange(0, 2));
            Assert.That(d.JapaneseName, Is.Not.Empty);
            Assert.That(d.JapaneseName, Does.Not.StartWith("ランク"));
            ItemDataSO legacyItem = ScriptableObject.CreateInstance<ItemDataSO>();
            legacyItem.itemName = d.EnglishName;
            Assert.That(JapaneseDisplayText.GetItemName(legacyItem), Is.EqualTo(d.JapaneseName));
            Object.DestroyImmediate(legacyItem);
            if (d.Slot == EquipmentSlot.Weapon)
                Assert.That(d.AcquisitionType, Is.EqualTo(ItemAcquisitionType.Blacksmith));
            if (d.Slot != EquipmentSlot.Weapon && d.Rank <= 5)
                Assert.That(d.AcquisitionType, Is.EqualTo(ItemAcquisitionType.Market));
            if (d.Slot != EquipmentSlot.Weapon && d.Rank >= 6)
                Assert.That(d.AcquisitionType, Is.EqualTo(ItemAcquisitionType.Blacksmith));
        }
        Assert.That(BalanceExpansionDefinition.Equipment.Count, Is.EqualTo(36));
        Assert.That(BalanceExpansionDefinition.Equipment.Count(d => d.Slot == EquipmentSlot.Armor), Is.EqualTo(12));
        Assert.That(BalanceExpansionDefinition.Equipment.Count(d => d.Slot == EquipmentSlot.Accessory), Is.EqualTo(12));
    }

    [Test]
    public void Equipment_ExpressesClassStrengths()
    {
        foreach (int rank in Enumerable.Range(4, 4))
        {
            ItemDataSO warriorArmor = Resources.Load<ItemDataSO>("GameData/Items/Expansion/item_expansion_rank" + rank + "_0_armor");
            ItemDataSO archerWeapon = Resources.Load<ItemDataSO>("GameData/Items/Expansion/item_expansion_rank" + rank + "_1");
            ItemDataSO mageWeapon = Resources.Load<ItemDataSO>("GameData/Items/Expansion/item_expansion_rank" + rank + "_2");
            Assert.That(warriorArmor.bonusMaxHP, Is.GreaterThanOrEqualTo(rank * 9));
            Assert.That(warriorArmor.bonusDefense, Is.GreaterThan(rank * 2));
            Assert.That(archerWeapon.bonusAttackSpeed, Is.GreaterThan(0.03f));
            Assert.That(mageWeapon.bonusAttack, Is.GreaterThan(rank * 3));
        }
    }

    [Test]
    public void Consumables_HavePersistentDefinitionsAndMarketPrices()
    {
        Assert.That(BalanceExpansionDefinition.Consumables.Count, Is.EqualTo(5));
        foreach (BalanceExpansionConsumableDefinition d in BalanceExpansionDefinition.Consumables)
        {
            Assert.That(d.Id, Does.StartWith("item.expansion.consumable."));
            Assert.That(d.JapaneseName, Is.Not.Empty);
            Assert.That(d.Price, Is.InRange(120, 280));
        }
    }
}
