using System;

[Serializable]
public class EquipmentModifier
{
    public EquipmentModifierType type;
    public float value;

    public EquipmentModifier(EquipmentModifierType type, float value)
    {
        this.type = type;
        this.value = value;
    }
}
