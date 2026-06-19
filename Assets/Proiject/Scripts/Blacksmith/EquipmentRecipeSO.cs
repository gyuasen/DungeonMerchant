using UnityEngine;

[CreateAssetMenu(
    fileName = "EquipmentRecipe",
    menuName = "DungeonMerchant/Equipment Recipe")]
public class EquipmentRecipeSO : ScriptableObject
{
    public string recipeName;
    public ItemDataSO resultItem;
    [Min(1)] public int resultAmount = 1;
    [Min(0)] public int goldCost;
    public CraftingMaterialRequirement[] materials;
}
