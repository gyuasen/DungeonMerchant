using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class ItemUsageTextBuilderTests
{
    [Test]
    public void Build_UsesJapaneseBulletsForEnhancementOreAndSellOnlyItems()
    {
        ItemDataSO ore = Resources.Load<ItemDataSO>(
            "GameData/Items/EnhancementOre");
        ItemDataSO sellOnly = ScriptableObject.CreateInstance<ItemDataSO>();
        sellOnly.materialClassification = MaterialClassification.SellOnly;
        sellOnly.itemName = "InternalTradeGoods";

        StringAssert.StartsWith("・", ItemUsageTextBuilder.Build(ore));
        StringAssert.Contains("+1〜+2", ItemUsageTextBuilder.Build(ore));
        StringAssert.Contains("売却専用", ItemUsageTextBuilder.Build(sellOnly));
        StringAssert.DoesNotContain("InternalTradeGoods", ItemUsageTextBuilder.Build(sellOnly));

        Object.DestroyImmediate(sellOnly);
    }

    [Test]
    public void Build_ExplainsConsumablesAndEquipmentWithoutRawAssetNames()
    {
        ItemDataSO consumable = ScriptableObject.CreateInstance<ItemDataSO>();
        consumable.itemName = "InternalPotion";
        consumable.itemType = ItemType.Consumable;
        consumable.consumableEffect = ConsumableEffectType.HealHP;
        consumable.consumableHealAmount = 30;
        ItemDataSO equipment = ScriptableObject.CreateInstance<ItemDataSO>();
        equipment.itemName = "InternalEquipment";
        equipment.itemType = ItemType.Equipment;

        string consumableUsage = ItemUsageTextBuilder.Build(consumable);
        string equipmentUsage = ItemUsageTextBuilder.Build(equipment);
        StringAssert.Contains("HPを30回復", consumableUsage);
        StringAssert.Contains("装備可能", equipmentUsage);
        StringAssert.DoesNotContain("InternalPotion", consumableUsage);
        StringAssert.DoesNotContain("InternalEquipment", equipmentUsage);

        Object.DestroyImmediate(consumable);
        Object.DestroyImmediate(equipment);
    }

    [Test]
    public void Build_ListsJapaneseCraftedEquipmentAndOmitsAfterThreeEntries()
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            "GameData/Items/MonsterFang");

        string usage = ItemUsageTextBuilder.Build(material, null);

        StringAssert.Contains("制作に使用", usage);
        StringAssert.Contains("他", usage);
        StringAssert.DoesNotContain("Arcane Pendant", usage);
        StringAssert.DoesNotContain("Arcane Staff", usage);
        Assert.LessOrEqual(usage.Split('\n').Length, 8);
    }

    [Test]
    public void Build_PrioritizesRecipesAvailableInTheCurrentTown()
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            "GameData/Items/MonsterFang");
        List<EquipmentRecipeSO> recipes = GetRecipesUsing(material);
        int townIndex = Enumerable.Range(0, WorldMapService.HiddenIslandTownIndex + 1)
            .First(index => recipes.Any(recipe =>
                BlacksmithManager.IsRecipeAvailableInTown(recipe, index)));
        EquipmentRecipeSO availableRecipe = recipes.First(recipe =>
            BlacksmithManager.IsRecipeAvailableInTown(recipe, townIndex));
        EquipmentRecipeSO unavailableRecipe = recipes.FirstOrDefault(recipe =>
            !BlacksmithManager.IsRecipeAvailableInTown(recipe, townIndex));

        string usage = ItemUsageTextBuilder.Build(material, townIndex);
        string availableName = JapaneseDisplayText.GetItemName(availableRecipe.resultItem);

        StringAssert.Contains("この町で作れる", usage);
        StringAssert.Contains(availableName, usage);
        if (unavailableRecipe != null &&
            ContainsJapaneseCharacter(JapaneseDisplayText.GetItemName(unavailableRecipe.resultItem)) &&
            usage.Contains(JapaneseDisplayText.GetItemName(unavailableRecipe.resultItem)))
        {
            Assert.Less(
                usage.IndexOf(availableName),
                usage.IndexOf(JapaneseDisplayText.GetItemName(unavailableRecipe.resultItem)));
        }
        Assert.LessOrEqual(usage.Split('\n').Length, 8);
    }

    [Test]
    public void Build_FallsBackWhenNoRecipeIsAvailableInTheTown()
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            "GameData/Items/MonsterFang");
        List<EquipmentRecipeSO> recipes = GetRecipesUsing(material);
        int townIndex = Enumerable.Range(0, WorldMapService.HiddenIslandTownIndex + 1)
            .First(index => recipes.All(recipe =>
                !BlacksmithManager.IsRecipeAvailableInTown(recipe, index)));

        string usage = ItemUsageTextBuilder.Build(material, townIndex);

        StringAssert.Contains("制作に使用", usage);
        StringAssert.DoesNotContain("この町で作れる", usage);
    }

    [Test]
    public void Build_FallsBackWithoutCurrentTownInformation()
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            "GameData/Items/MonsterFang");

        Assert.DoesNotThrow(() => ItemUsageTextBuilder.Build(material, null));
        StringAssert.DoesNotContain(
            "この町で作れる",
            ItemUsageTextBuilder.Build(material, null));
    }

    [Test]
    public void Build_ListsJapaneseEnemiesThatDropTheItem()
    {
        ItemDataSO material = Resources.Load<ItemDataSO>(
            "GameData/Items/MonsterFang");
        EnemyDataSO enemy = GameAssetRepository.LoadAll<EnemyDataSO>()
            .First(candidate => candidate.itemDrops != null && candidate.itemDrops.Any(drop =>
                drop != null && drop.item == material &&
                ContainsJapaneseCharacter(JapaneseDisplayText.GetEnemyName(candidate.enemyName))));

        string usage = ItemUsageTextBuilder.Build(material);
        string enemyName = JapaneseDisplayText.GetEnemyName(enemy.enemyName);

        StringAssert.Contains("ドロップ元", usage);
        StringAssert.Contains(enemyName, usage);
        StringAssert.DoesNotContain(enemy.enemyName, usage);
    }

    private static bool ContainsJapaneseCharacter(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Any(character =>
            (character >= '\u3040' && character <= '\u30ff') ||
            (character >= '\u3400' && character <= '\u9fff') ||
            (character >= '\uff66' && character <= '\uff9f'));
    }

    private static List<EquipmentRecipeSO> GetRecipesUsing(ItemDataSO material)
    {
        return GameAssetRepository.LoadAll<EquipmentRecipeSO>()
            .Where(recipe => recipe?.materials != null && recipe.materials.Any(requirement =>
                requirement?.item == material))
            .ToList();
    }
}
