using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class TradeMaterialTests
{
    [Test]
    public void EveryMaterialAsset_HasAClassification()
    {
        ItemDataSO[] items = Resources.LoadAll<ItemDataSO>("GameData/Items");
        Assert.That(items.Where(item => item.itemType == ItemType.Material), Is.All.Matches<ItemDataSO>(item => item.materialClassification != MaterialClassification.None));
    }

    [TestCase(10, "MagicStoneLow")]
    [TestCase(8, "MagicStoneLesser")]
    [TestCase(6, "MagicStoneMiddle")]
    [TestCase(4, "MagicStoneGreater")]
    [TestCase(1, "MagicStoneHighest")]
    public void MagicStoneDrop_MapsEnemyGradeToTheSpecifiedStone(int grade, string assetName)
    {
        StringAssert.EndsWith(assetName, MaterialCatalog.GetMagicStoneResourcePathForEnemyGrade(grade));
    }

    [Test]
    public void EveryBlacksmithRecipe_RequiresAtLeastTheStoneAmountForItsResultRank()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:EquipmentRecipeSO"))
        {
            EquipmentRecipeSO recipe = AssetDatabase.LoadAssetAtPath<EquipmentRecipeSO>(AssetDatabase.GUIDToAssetPath(guid));
            ItemDataSO expected = MaterialCatalog.GetMagicStoneForEquipmentRank(recipe.resultItem.equipmentRank);
            Assert.That(recipe.materials.Any(requirement =>
                requirement.item == expected &&
                requirement.amount >= MaterialCatalog.GetMagicStoneAmountForEquipmentRank(
                    recipe.resultItem.equipmentRank)),
                Is.True,
                recipe.name);
        }
    }

    [Test]
    public void EveryRecipe_UsesOnlyCraftingMaterials()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:EquipmentRecipeSO"))
        {
            EquipmentRecipeSO recipe = AssetDatabase.LoadAssetAtPath<EquipmentRecipeSO>(AssetDatabase.GUIDToAssetPath(guid));
            Assert.That(recipe.materials, Is.All.Matches<CraftingMaterialRequirement>(requirement => requirement.item != null && requirement.item.materialClassification == MaterialClassification.CraftingMaterial), recipe.name);
        }
    }

    [Test]
    public void EnvironmentalEvent_HighestGradeUsesCentralRegionMaterialAndBonus()
    {
        DungeonEventChoiceResult result = DungeonEnvironmentEventService.ResolveEnvironmentalChoice(DungeonEventType.MineralVein, 0, DungeonGrade.Highest);
        Assert.That(result.MaterialItem.PersistentId, Is.EqualTo("item.material.silver_ore"));
        Assert.That(result.MaterialAmount, Is.EqualTo(7));
        Assert.That(result.AddExplorationDelay, Is.True);
    }
}
