using NUnit.Framework;
using UnityEngine;

public sealed class BattleConsumableServiceTests
{
    [Test]
    public void Poison_UsesAntidoteAndCuresStatus()
    {
        ItemDataSO antidote = CreateConsumable(ConsumableEffectType.CurePoison, 0);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, antidote, 1);
        BattleUnit unit = CreateUnit(100, 100, BattleStatusEffect.Poison);

        BattleConsumableResult result =
            new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(result.Used, Is.True);
        Assert.That(unit.StatusEffect, Is.EqualTo(BattleStatusEffect.None));
        Assert.That(mercenary.ConsumableSlots[0].IsEmpty, Is.True);
    }

    [Test]
    public void BelowFortyPercent_UsesHealingPotion()
    {
        ItemDataSO potion = CreateConsumable(ConsumableEffectType.HealHP, 40);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, potion, 1);
        BattleUnit unit = CreateUnit(100, 39, BattleStatusEffect.None);

        BattleConsumableResult result =
            new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(result.Used, Is.True);
        Assert.That(result.HealedAmount, Is.EqualTo(40));
        Assert.That(unit.CurrentHP, Is.EqualTo(79));
    }

    [Test]
    public void EmptySlots_DoNothing()
    {
        BattleConsumableResult result = new BattleConsumableService()
            .ProcessActionStart(CreateUnit(100, 20, BattleStatusEffect.None), CreateMercenary());

        Assert.That(result.Used, Is.False);
    }

    [Test]
    public void ConsumingLastItem_EmptiesSlot()
    {
        ItemDataSO potion = CreateConsumable(ConsumableEffectType.HealHP, 40);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, potion, 1);

        new BattleConsumableService().ProcessActionStart(
            CreateUnit(100, 20, BattleStatusEffect.None), mercenary);

        Assert.That(mercenary.ConsumableSlots[0].Count, Is.EqualTo(0));
        Assert.That(mercenary.ConsumableSlots[0].Item, Is.Null);
    }

    [Test]
    public void Healing_UsesSmallerPotionFirst()
    {
        ItemDataSO small = CreateConsumable(ConsumableEffectType.HealHP, 40);
        ItemDataSO large = CreateConsumable(ConsumableEffectType.HealHP, 100);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, large, 1);
        mercenary.TryLoadConsumable(1, small, 1);
        BattleUnit unit = CreateUnit(200, 50, BattleStatusEffect.None);

        new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(unit.CurrentHP, Is.EqualTo(90));
        Assert.That(mercenary.ConsumableSlots[0].Count, Is.EqualTo(1));
        Assert.That(mercenary.ConsumableSlots[1].IsEmpty, Is.True);
    }

    [Test]
    public void SavedMercenary_SerializesConsumableSlots()
    {
        SavedMercenary saved = new SavedMercenary
        {
            consumableSlots = new[]
            {
                new SavedMercenaryConsumableSlot
                {
                    itemPersistentId = "item.HealingPotion",
                    count = 3
                },
                new SavedMercenaryConsumableSlot()
            }
        };

        SavedMercenary restored = JsonUtility.FromJson<SavedMercenary>(
            JsonUtility.ToJson(saved));

        Assert.That(restored.consumableSlots, Has.Length.EqualTo(2));
        Assert.That(restored.consumableSlots[0].itemPersistentId,
            Is.EqualTo("item.HealingPotion"));
        Assert.That(restored.consumableSlots[0].count, Is.EqualTo(3));
    }

    [Test]
    public void AttackPotion_UsesOnFirstActionAndLastsForBattle()
    {
        ItemDataSO potion = CreateConsumable(ConsumableEffectType.BoostAttack, 0);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, potion, 1);
        BattleUnit unit = CreateUnit(100, 100, BattleStatusEffect.None);

        BattleConsumableResult result = new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(result.Used, Is.True);
        Assert.That(unit.Attack, Is.EqualTo(12));
        Assert.That(unit.AttackBonusPercent, Is.EqualTo(.20f));
        unit.TickStatuses();
        Assert.That(unit.Attack, Is.EqualTo(12));
        BattleUnit nextBattleUnit = CreateUnit(100, 100, BattleStatusEffect.None);
        Assert.That(nextBattleUnit.AttackBonusPercent, Is.EqualTo(0f));
    }

    [Test]
    public void DefenseAndSpeedPotions_ApplyTheirSpecifiedEffects()
    {
        BattleUnit defenseUnit = CreateUnit(100, 100, BattleStatusEffect.None);
        MercenaryInstance defenseMercenary = CreateMercenary();
        defenseMercenary.TryLoadConsumable(0, CreateConsumable(ConsumableEffectType.BoostDefense, 0), 1);
        new BattleConsumableService().ProcessActionStart(defenseUnit, defenseMercenary);
        Assert.That(defenseUnit.Defense, Is.EqualTo(1));
        Assert.That(defenseUnit.DefenseBonusPercent, Is.EqualTo(.20f));

        BattleUnit speedUnit = CreateUnit(100, 100, BattleStatusEffect.None);
        MercenaryInstance speedMercenary = CreateMercenary();
        speedMercenary.TryLoadConsumable(0, CreateConsumable(ConsumableEffectType.BoostSpeed, 0), 1);
        new BattleConsumableService().ProcessActionStart(speedUnit, speedMercenary);
        Assert.That(speedUnit.AttackSpeed, Is.EqualTo(1.15f));
        Assert.That(speedUnit.SpeedBonusPercent, Is.EqualTo(.15f));
    }

    [Test]
    public void MagicElixir_OnlyUsesBelowThirtyPercentAndRestoresFifty()
    {
        ItemDataSO elixir = CreateConsumable(ConsumableEffectType.RestoreMagic, 50);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, elixir, 1);
        BattleUnit unit = new BattleUnit("Mage", 100, 100, 10, 1, 1f, true, MercenaryClass.Mage, 100);

        BattleConsumableResult result = new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(result.Used, Is.True);
        Assert.That(result.HealedAmount, Is.EqualTo(50));
        Assert.That(unit.CurrentMagicPower, Is.EqualTo(70));
    }

    [Test]
    public void SingleAntidote_HasPriorityOverGreaterAntidote()
    {
        ItemDataSO single = CreateConsumable(ConsumableEffectType.CurePoison, 0);
        ItemDataSO greater = CreateConsumable(ConsumableEffectType.CureAllStatus, 0);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, greater, 1);
        mercenary.TryLoadConsumable(1, single, 1);
        BattleUnit unit = CreateUnit(100, 100, BattleStatusEffect.Poison);

        BattleConsumableResult result = new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(result.Item, Is.EqualTo(single));
        Assert.That(mercenary.ConsumableSlots[0].Count, Is.EqualTo(1));
        Assert.That(mercenary.ConsumableSlots[1].IsEmpty, Is.True);
    }

    [Test]
    public void OneAction_ConsumesOnlyOnePotionInPriorityOrder()
    {
        ItemDataSO attack = CreateConsumable(ConsumableEffectType.BoostAttack, 0);
        ItemDataSO defense = CreateConsumable(ConsumableEffectType.BoostDefense, 0);
        MercenaryInstance mercenary = CreateMercenary();
        mercenary.TryLoadConsumable(0, defense, 1);
        mercenary.TryLoadConsumable(1, attack, 1);
        BattleUnit unit = CreateUnit(100, 100, BattleStatusEffect.None);

        new BattleConsumableService().ProcessActionStart(unit, mercenary);

        Assert.That(unit.AttackBonusPercent, Is.EqualTo(.20f));
        Assert.That(unit.DefenseBonusPercent, Is.EqualTo(0f));
        Assert.That(mercenary.ConsumableSlots[0].Count, Is.EqualTo(1));
    }

    private static ItemDataSO CreateConsumable(
        ConsumableEffectType effect,
        int healAmount)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemType = ItemType.Consumable;
        item.consumableEffect = effect;
        item.consumableHealAmount = healAmount;
        return item;
    }

    private static MercenaryInstance CreateMercenary()
    {
        return MercenaryInstance.CreateRestored(
            "test",
            null,
            null,
            "Tester",
            MercenaryClass.Warrior,
            MercenaryContractType.Exclusive,
            1,
            0,
            100,
            100,
            10,
            1,
            0,
            1f,
            0);
    }

    private static BattleUnit CreateUnit(
        int maxHp,
        int currentHp,
        BattleStatusEffect status)
    {
        return new BattleUnit(
            "Tester",
            maxHp,
            currentHp,
            10,
            1,
            1f,
            true,
            initialStatus: status);
    }
}
