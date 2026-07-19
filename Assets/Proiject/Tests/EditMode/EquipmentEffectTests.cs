using NUnit.Framework;
using UnityEngine;

public sealed class EquipmentEffectTests
{
    [Test]
    public void EquipmentInstance_ReferencesBaseItemEffectsWithoutCopying()
    {
        ItemDataSO item = CreateEquipment(EquipmentSlot.Weapon,
            new EquipmentEffectDefinition { type = EquipmentEffectType.TurnRegeneration, value = 0.03f });
        EquipmentInstance instance = EquipmentInstance.CreateFixed(item);
        Assert.That(instance.EquipmentEffects, Is.SameAs(item.equipmentEffects));
    }

    [Test]
    public void GetEquipmentEffectTotal_AggregatesSlotsAndCapsDamageReduction()
    {
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.EquipEquipment(CreateEquipment(EquipmentSlot.Weapon,
            new EquipmentEffectDefinition { type = EquipmentEffectType.DamageReduction, value = 0.15f }));
        mercenary.EquipEquipment(CreateEquipment(EquipmentSlot.Armor,
            new EquipmentEffectDefinition { type = EquipmentEffectType.DamageReduction, value = 0.12f }));
        mercenary.EquipEquipment(CreateEquipment(EquipmentSlot.Accessory,
            new EquipmentEffectDefinition { type = EquipmentEffectType.DamageReduction, value = 0.10f }));
        Assert.That(mercenary.GetActiveEquipmentEffects().Count, Is.EqualTo(3));
        Assert.That(mercenary.GetEquipmentEffectTotal(EquipmentEffectType.DamageReduction), Is.EqualTo(0.30f));
    }

    [Test]
    public void BattleUnit_AppliesEquipmentEffectsAndExpiresTemporaryBuff()
    {
        BattleUnit unit = new BattleUnit("Test", 100, 20, 100, 0, 1f, true);
        unit.ApplyEquipmentEffects(new BattleEquipmentEffectSnapshot(1f, 0.25f, 0.30f, 0.8f, 10, 0.10f, 2, 0.20f, 2));
        Assert.That(unit.Attack, Is.EqualTo(110));
        Assert.That(unit.Defense, Is.EqualTo(0));
        Assert.That(unit.CalculateDamage(), Is.EqualTo(138));
        Assert.That(unit.EstimateDamageTaken(100), Is.EqualTo(80));
        Assert.That(unit.ProcessEquipmentTurnRegeneration(), Is.EqualTo(10));
        Assert.That(unit.CurrentHP, Is.EqualTo(30));
        unit.TickStatuses();
        unit.TickStatuses();
        Assert.That(unit.Attack, Is.EqualTo(100));
    }

    private static MercenaryInstance CreateMercenary()
    {
        MercenaryDataSO data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryName = "Effect Tester";
        data.mercenaryClass = MercenaryClass.Warrior;
        data.maxHP = 100;
        data.attack = 10;
        data.defense = 3;
        data.attackSpeed = 1f;
        return new MercenaryInstance(data);
    }

    private static ItemDataSO CreateEquipment(EquipmentSlot slot, EquipmentEffectDefinition effect)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemType = ItemType.Equipment;
        item.equipmentSlot = slot;
        item.allClassesCanEquip = true;
        item.equipmentEffects = new[] { effect };
        return item;
    }
}
