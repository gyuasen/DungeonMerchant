using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class BalanceExpansionDefinitionTests
{
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
