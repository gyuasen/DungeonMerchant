using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class DungeonEquipmentSetBonusTests
{
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        foreach (Object created in createdObjects)
        {
            if (created != null)
            {
                Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    [TestCase(EquipmentSetId.NornCanopy, 120, 14, 8, 1.02f)]
    [TestCase(EquipmentSetId.GlaadSkyFortress, 122, 13, 9, 1.025f)]
    [TestCase(EquipmentSetId.VelmBlackIron, 128, 14, 11, 1.03f)]
    [TestCase(EquipmentSetId.AbyssThrone, 145, 17, 11, 1.035f)]
    [TestCase(EquipmentSetId.AstralDepths, 145, 20, 13, 1.04f)]
    [TestCase(EquipmentSetId.NornVerdantSettlement, 115, 12, 5, 1.01f)]
    [TestCase(EquipmentSetId.GlaadDragonScaleCanyon, 115, 12, 5, 1.01f)]
    [TestCase(EquipmentSetId.VelmFurnaceDefenseZone, 120, 13, 5, 1.015f)]
    [TestCase(EquipmentSetId.AbyssGatewayThreshold, 125, 13, 6, 1.02f)]
    public void FullDungeonSet_AppliesProgressiveAllRoleBonuses(
        EquipmentSetId setId,
        int expectedMaxHP,
        int expectedAttack,
        int expectedDefense,
        float expectedAttackSpeed)
    {
        MercenaryInstance mercenary = CreateMercenary();

        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Weapon, setId)), Is.True);
        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Armor, setId)), Is.True);
        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Accessory, setId)), Is.True);

        Assert.That(mercenary.GetEquippedSetCount(setId), Is.EqualTo(3));
        Assert.That(mercenary.MaxHP, Is.EqualTo(expectedMaxHP));
        Assert.That(mercenary.Attack, Is.EqualTo(expectedAttack));
        Assert.That(mercenary.Defense, Is.EqualTo(expectedDefense));
        Assert.That(mercenary.AttackSpeed,
            Is.EqualTo(expectedAttackSpeed).Within(0.0001f));
    }

    [TestCase(EquipmentSetId.NornVerdantSettlement, 15)]
    [TestCase(EquipmentSetId.GlaadDragonScaleCanyon, 15)]
    [TestCase(EquipmentSetId.VelmFurnaceDefenseZone, 20)]
    [TestCase(EquipmentSetId.AbyssGatewayThreshold, 25)]
    public void NewDungeonSets_TwoPiecesGrantOnlyTheirHealthBonus(
        EquipmentSetId setId,
        int expectedHealthBonus)
    {
        MercenaryInstance mercenary = CreateMercenary();
        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Weapon, setId)), Is.True);
        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Armor, setId)), Is.True);

        Assert.That(mercenary.MaxHP, Is.EqualTo(100 + expectedHealthBonus));
        Assert.That(mercenary.Attack, Is.EqualTo(10));
        Assert.That(mercenary.Defense, Is.EqualTo(3));
        Assert.That(mercenary.AttackSpeed, Is.EqualTo(1f));
    }

    [Test]
    public void DungeonSetEquipment_IsUsableByEveryBaseClass()
    {
        ItemDataSO equipment = CreateEquipment(
            EquipmentSlot.Weapon,
            EquipmentSetId.AstralDepths);

        foreach (MercenaryClass mercenaryClass in
                 MercenaryClassProgression.GetBaseClasses())
        {
            Assert.That(equipment.CanEquip(mercenaryClass), Is.True);
        }
    }

    [Test]
    public void ExistingOniHunterSet_RemainsUnchanged()
    {
        MercenaryInstance mercenary = CreateMercenary();

        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Weapon, EquipmentSetId.OniHunter)),
            Is.True);
        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Armor, EquipmentSetId.OniHunter)),
            Is.True);
        Assert.That(mercenary.EquipEquipment(
            CreateEquipment(EquipmentSlot.Accessory, EquipmentSetId.OniHunter)),
            Is.True);

        Assert.That(mercenary.MaxHP, Is.EqualTo(110));
        Assert.That(mercenary.Attack, Is.EqualTo(18));
        Assert.That(mercenary.Defense, Is.EqualTo(5));
        Assert.That(mercenary.AttackSpeed, Is.EqualTo(1f));
    }

    private MercenaryInstance CreateMercenary()
    {
        MercenaryDataSO data = Track(
            ScriptableObject.CreateInstance<MercenaryDataSO>());
        data.mercenaryName = "Set Tester";
        data.mercenaryClass = MercenaryClass.Warrior;
        data.maxHP = 100;
        data.attack = 10;
        data.defense = 3;
        data.attackSpeed = 1f;
        return new MercenaryInstance(data);
    }

    private ItemDataSO CreateEquipment(
        EquipmentSlot slot,
        EquipmentSetId setId)
    {
        ItemDataSO equipment = Track(
            ScriptableObject.CreateInstance<ItemDataSO>());
        equipment.itemType = ItemType.Equipment;
        equipment.equipmentSlot = slot;
        equipment.equipmentSet = setId;
        equipment.allClassesCanEquip = true;
        return equipment;
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
