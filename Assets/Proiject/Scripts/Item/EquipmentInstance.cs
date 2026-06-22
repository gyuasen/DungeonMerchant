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

    public static EquipmentInstance CreateRandom(ItemDataSO baseItem)
    {
        EquipmentQuality quality = RollQuality();
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
            int index = UnityEngine.Random.Range(0, availableTypes.Count);
            EquipmentModifierType type = availableTypes[index];
            availableTypes.RemoveAt(index);
            modifiers.Add(new EquipmentModifier(
                type,
                RollModifierValue(type, quality, allowNegative)));
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

    private static EquipmentQuality RollQuality()
    {
        int roll = UnityEngine.Random.Range(0, 100);
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
        bool allowNegative)
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
                value = UnityEngine.Random.Range(8, 21) * qualityScale;
                break;
            case EquipmentModifierType.AttackSpeed:
                value = UnityEngine.Random.Range(0.03f, 0.11f) * qualityScale;
                break;
            default:
                value = UnityEngine.Random.Range(2, 7) * qualityScale;
                break;
        }

        if (allowNegative && UnityEngine.Random.value < 0.7f)
        {
            value *= -1f;
        }
        return value;
    }
}
