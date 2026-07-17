using UnityEngine;

public sealed class BattleConsumableResult
{
    public bool Used { get; }
    public ItemDataSO Item { get; }
    public int HealedAmount { get; }
    public BattleStatusEffect CuredStatus { get; }
    private BattleConsumableResult(bool used, ItemDataSO item, int healedAmount, BattleStatusEffect curedStatus) { Used = used; Item = item; HealedAmount = healedAmount; CuredStatus = curedStatus; }
    public static BattleConsumableResult None() { return new BattleConsumableResult(false, null, 0, BattleStatusEffect.None); }
    public static BattleConsumableResult Use(ItemDataSO item, int healedAmount = 0, BattleStatusEffect curedStatus = BattleStatusEffect.None) { return new BattleConsumableResult(true, item, healedAmount, curedStatus); }
}

public sealed class BattleConsumableService
{
    const float BattleBuffPercent = .20f;
    const float SpeedBuffPercent = .15f;
    public BattleConsumableResult ProcessActionStart(BattleUnit unit, MercenaryInstance mercenary)
    {
        if (unit == null || mercenary == null || unit.IsDead) { return BattleConsumableResult.None(); }
        BattleConsumableResult statusResult = TryUseStatusCure(unit, mercenary);
        if (statusResult.Used) { return statusResult; }
        if (unit.CurrentHP * 100 < unit.MaxHP * 40) { BattleConsumableResult healingResult = TryUseHealing(unit, mercenary); if (healingResult.Used) { return healingResult; } }
        if (unit.MaxMagicPower > 0 && unit.CurrentMagicPower * 100 < unit.MaxMagicPower * 30) { BattleConsumableResult magicResult = TryUseMagicRestore(unit, mercenary); if (magicResult.Used) { return magicResult; } }
        return TryUseOpeningBuff(unit, mercenary);
    }
    static BattleConsumableResult TryUseStatusCure(BattleUnit unit, MercenaryInstance mercenary)
    {
        if (unit.StatusEffect == BattleStatusEffect.None) { return BattleConsumableResult.None(); }
        ConsumableEffectType singleEffect = unit.StatusEffect == BattleStatusEffect.Poison ? ConsumableEffectType.CurePoison : ConsumableEffectType.CureParalysis;
        ItemDataSO item = FindConsumable(mercenary, singleEffect) ?? FindConsumable(mercenary, ConsumableEffectType.CureAllStatus);
        if (item == null || !mercenary.TryConsumeConsumable(item)) { return BattleConsumableResult.None(); }
        BattleStatusEffect status = unit.StatusEffect;
        return unit.CureStatus(status) ? BattleConsumableResult.Use(item, 0, status) : BattleConsumableResult.None();
    }
    static BattleConsumableResult TryUseHealing(BattleUnit unit, MercenaryInstance mercenary)
    {
        ItemDataSO item = FindSmallestHealingItem(mercenary);
        if (item == null || !mercenary.TryConsumeConsumable(item)) { return BattleConsumableResult.None(); }
        int before = unit.CurrentHP;
        unit.Heal(item.consumableHealAmount);
        return BattleConsumableResult.Use(item, unit.CurrentHP - before);
    }
    static BattleConsumableResult TryUseMagicRestore(BattleUnit unit, MercenaryInstance mercenary)
    {
        ItemDataSO item = FindConsumable(mercenary, ConsumableEffectType.RestoreMagic);
        if (item == null || !mercenary.TryConsumeConsumable(item)) { return BattleConsumableResult.None(); }
        int before = unit.CurrentMagicPower;
        unit.GainMagicPower(item.consumableHealAmount);
        return BattleConsumableResult.Use(item, unit.CurrentMagicPower - before);
    }
    static BattleConsumableResult TryUseOpeningBuff(BattleUnit unit, MercenaryInstance mercenary)
    {
        ItemDataSO item = FindConsumable(mercenary, ConsumableEffectType.BoostAttack) ?? FindConsumable(mercenary, ConsumableEffectType.BoostDefense) ?? FindConsumable(mercenary, ConsumableEffectType.BoostSpeed);
        if (item == null || !mercenary.TryConsumeConsumable(item)) { return BattleConsumableResult.None(); }
        if (item.consumableEffect == ConsumableEffectType.BoostAttack) { unit.BoostAttackForBattle(BattleBuffPercent); }
        if (item.consumableEffect == ConsumableEffectType.BoostDefense) { unit.BoostDefenseForBattle(BattleBuffPercent); }
        if (item.consumableEffect == ConsumableEffectType.BoostSpeed) { unit.BoostSpeedForBattle(SpeedBuffPercent); }
        return BattleConsumableResult.Use(item);
    }
    static ItemDataSO FindSmallestHealingItem(MercenaryInstance mercenary)
    {
        ItemDataSO result = null;
        foreach (MercenaryConsumableSlot slot in mercenary.ConsumableSlots) { ItemDataSO item = slot.Item; if (slot.IsEmpty || item.consumableEffect != ConsumableEffectType.HealHP || item.consumableHealAmount <= 0) { continue; } if (result == null || item.consumableHealAmount < result.consumableHealAmount) { result = item; } }
        return result;
    }
    static ItemDataSO FindConsumable(MercenaryInstance mercenary, ConsumableEffectType effect)
    {
        foreach (MercenaryConsumableSlot slot in mercenary.ConsumableSlots) { if (!slot.IsEmpty && slot.Item.consumableEffect == effect) { return slot.Item; } }
        return null;
    }
}
