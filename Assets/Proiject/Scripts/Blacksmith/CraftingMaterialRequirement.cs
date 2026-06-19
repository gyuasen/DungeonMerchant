using System;
using UnityEngine;

[Serializable]
public class CraftingMaterialRequirement
{
    public ItemDataSO item;
    [Min(1)] public int amount = 1;
}
