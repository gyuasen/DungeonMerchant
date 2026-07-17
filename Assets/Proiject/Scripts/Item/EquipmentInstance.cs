using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentInstance
{
    [SerializeField] private string instanceId;
    [SerializeField] private ItemDataSO baseItem;
    [SerializeField] private EquipmentQuality quality;
    [SerializeField, Range(0, 10)] private int enhancementLevel;
    [SerializeField] private bool isLocked;
    [SerializeField] private List<EquipmentModifier> modifiers =
        new List<EquipmentModifier>();

    public string InstanceId => instanceId;
    public ItemDataSO BaseItem => baseItem;
    public EquipmentQuality Quality => quality;
    public int EnhancementLevel => enhancementLevel;
    public bool IsLocked => isLocked;
    public IReadOnlyList<EquipmentModifier> Modifiers => modifiers;

    public int BonusMaxHP => ScaleInt(baseItem.bonusMaxHP) +
        Mathf.RoundToInt(GetModifierValue(EquipmentModifierType.MaxHP));
    public int BonusAttack => ScaleInt(baseItem.bonusAttack) +
        Mathf.RoundToInt(GetModifierValue(EquipmentModifierType.Attack));
    public int BonusDefense => ScaleInt(baseItem.bonusDefense) +
        Mathf.RoundToInt(GetModifierValue(EquipmentModifierType.Defense));
    public float BonusAttackSpeed => ScaleFloat(baseItem.bonusAttackSpeed) +
        GetModifierValue(EquipmentModifierType.AttackSpeed);

    public EquipmentInstance(
        ItemDataSO baseItem,
        EquipmentQuality quality,
        IEnumerable<EquipmentModifier> modifiers)
    {
        instanceId = Guid.NewGuid().ToString("N");
        this.baseItem = baseItem;
        this.quality = quality;
        if (modifiers != null)
        {
            this.modifiers.AddRange(modifiers);
        }
    }

    public static EquipmentInstance CreateRestored(
        string instanceId,
        ItemDataSO baseItem,
        EquipmentQuality quality,
        IEnumerable<EquipmentModifier> modifiers,
        int enhancementLevel = 0,
        bool isLocked = false)
    {
        EquipmentInstance instance = new EquipmentInstance(baseItem, quality, modifiers);
        instance.instanceId = string.IsNullOrWhiteSpace(instanceId)
            ? Guid.NewGuid().ToString("N")
            : instanceId;
        instance.enhancementLevel = Mathf.Clamp(enhancementLevel, 0, 10);
        instance.isLocked = isLocked;
        return instance;
    }

    public static EquipmentInstance CreateFixed(ItemDataSO baseItem)
    {
        return new EquipmentInstance(
            baseItem,
            EquipmentQuality.Normal,
            Array.Empty<EquipmentModifier>());
    }

    public bool TryEnhance()
    {
        if (enhancementLevel >= 10)
        {
            return false;
        }

        enhancementLevel++;
        return true;
    }

    public void ToggleLock()
    {
        isLocked = !isLocked;
    }

    public float GetEnhancementSuccessRate()
    {
        float[] rates =
        {
            1f, 1f, 0.95f, 0.9f, 0.8f,
            0.7f, 0.6f, 0.5f, 0.4f, 0.3f
        };
        return enhancementLevel >= 10 ? 0f : rates[enhancementLevel];
    }

    public int GetEnhancementMaterialAmount()
    {
        return 1 + enhancementLevel / 3;
    }

    public int GetEnhancementCost()
    {
        return Mathf.Max(
            50,
            Mathf.RoundToInt(baseItem.basePrice * (0.35f + enhancementLevel * 0.15f)));
    }

    public float GetSellPriceQualityMultiplier()
    {
        switch (quality)
        {
            case EquipmentQuality.Poor: return 0.65f;
            case EquipmentQuality.Fine: return 1.2f;
            case EquipmentQuality.Rare: return 1.55f;
            case EquipmentQuality.Legendary: return 2.2f;
            default: return 1f;
        }
    }

    public static EquipmentInstance CreateRandom(ItemDataSO baseItem)
    {
        return CreateRandom(baseItem, () => UnityEngine.Random.value);
    }

    public static EquipmentInstance CreateRandom(
        ItemDataSO baseItem,
        Func<float> randomValue)
    {
        Func<float> provider = randomValue ?? (() => UnityEngine.Random.value);
        EquipmentQuality quality = RollQuality(provider);
        int modifierCount = GetModifierCount(quality);
        bool allowNegative = quality == EquipmentQuality.Poor;
        List<EquipmentModifierType> availableTypes =
            new List<EquipmentModifierType>
            {
                EquipmentModifierType.MaxHP,
                EquipmentModifierType.Attack,
                EquipmentModifierType.Defense,
                EquipmentModifierType.AttackSpeed
            };
        List<EquipmentModifier> modifiers = new List<EquipmentModifier>();

        for (int i = 0; i < modifierCount && availableTypes.Count > 0; i++)
        {
            int index = GetRandomIndex(provider, availableTypes.Count);
            EquipmentModifierType type = availableTypes[index];
            availableTypes.RemoveAt(index);
            modifiers.Add(new EquipmentModifier(
                type,
                RollModifierValue(type, quality, allowNegative, provider)));
        }

        return new EquipmentInstance(baseItem, quality, modifiers);
    }

    private float GetModifierValue(EquipmentModifierType type)
    {
        float total = 0f;
        foreach (EquipmentModifier modifier in modifiers)
        {
            if (modifier != null && modifier.type == type)
            {
                total += modifier.value;
            }
        }
        return total;
    }

    private int ScaleInt(int value)
    {
        return Mathf.RoundToInt(value * (1f + enhancementLevel * 0.1f));
    }

    private float ScaleFloat(float value)
    {
        return value * (1f + enhancementLevel * 0.1f);
    }

    private static EquipmentQuality RollQuality(Func<float> randomValue)
    {
        int roll = GetRandomIndex(randomValue, 100);
        if (roll < 15) return EquipmentQuality.Poor;
        if (roll < 55) return EquipmentQuality.Normal;
        if (roll < 80) return EquipmentQuality.Fine;
        if (roll < 95) return EquipmentQuality.Rare;
        return EquipmentQuality.Legendary;
    }

    private static int GetModifierCount(EquipmentQuality quality)
    {
        switch (quality)
        {
            case EquipmentQuality.Poor: return 1;
            case EquipmentQuality.Fine: return 1;
            case EquipmentQuality.Rare: return 2;
            case EquipmentQuality.Legendary: return 3;
            default: return 0;
        }
    }

    private static float RollModifierValue(
        EquipmentModifierType type,
        EquipmentQuality quality,
        bool allowNegative,
        Func<float> randomValue)
    {
        float qualityScale = quality == EquipmentQuality.Legendary
            ? 1.5f
            : quality == EquipmentQuality.Rare
                ? 1.25f
                : 1f;
        float value;
        switch (type)
        {
            case EquipmentModifierType.MaxHP:
                value = GetRandomRange(randomValue, 8f, 21f) * qualityScale;
                break;
            case EquipmentModifierType.AttackSpeed:
                value = GetRandomRange(randomValue, 0.03f, 0.11f) * qualityScale;
                break;
            default:
                value = GetRandomRange(randomValue, 2f, 7f) * qualityScale;
                break;
        }

        if (allowNegative && randomValue() < 0.7f)
        {
            value *= -1f;
        }
        return value;
    }

    private static int GetRandomIndex(Func<float> randomValue, int count)
    {
        return Mathf.Clamp(Mathf.FloorToInt(randomValue() * count), 0, count - 1);
    }

    private static float GetRandomRange(
        Func<float> randomValue,
        float minimum,
        float maximum)
    {
        return Mathf.Lerp(minimum, maximum, Mathf.Clamp01(randomValue()));
    }
}
