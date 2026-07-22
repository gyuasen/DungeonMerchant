using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class SpeciesDropMappingTests
{
    [TestCase("Grade10BlueSlime", "Slime Mucus")]
    [TestCase("Grade10HornRabbit", "Rabbit Horn")]
    [TestCase("Grade09WildDog", "Monster Fang")]
    [TestCase("Grade09Kobold", "Monster Fang")]
    [TestCase("Grade08CaveBat", "Bat Wing")]
    [TestCase("Grade08CaveSpider", "Spider Silk")]
    [TestCase("Grade08GiantRat", "Giant Rat Pelt")]
    [TestCase("Grade08RockBeetle", "Beetle Shell")]
    [TestCase("Grade08VenomMoth", "Venom Moth Powder")]
    [TestCase("Grade07Wraith", "Spirit Remnant")]
    [TestCase("Grade06Hobgoblin", "Goblin Ear")]
    [TestCase("Grade05OgreMage", "Ogre Bloodstone")]
    public void RepresentativeEnemies_DropOnlyTheirSpeciesMaterial(
        string enemyAsset,
        string expectedItemName)
    {
        EnemyDataSO enemy = Resources.Load<EnemyDataSO>(
            "GameData/Enemies/" + enemyAsset);

        Assert.That(enemy, Is.Not.Null, enemyAsset);
        Assert.That(enemy.itemDrops, Has.Length.EqualTo(1), enemyAsset);
        Assert.That(enemy.itemDrops[0].item.itemName,
            Is.EqualTo(expectedItemName), enemyAsset);
    }

    [Test]
    public void ExpansionAndRegionalEnemies_KeepTheirSpeciesMaterial()
    {
        AssertDrop("GameData/Enemies/Expansion/enemy_job_goblin_assassin",
            "Goblin Ear");
        AssertDrop("GameData/Enemies/Expansion/enemy_job_skeleton_archer",
            "Cursed Bone");
        AssertDrop("GameData/Enemies/Expansion/enemy_job_orc_berserker",
            "Orc Tusk");
        AssertDrop("GameData/Enemies/Expansion/enemy_job_wyvern_hexer",
            "Wyvern Scale");
        AssertDrop("Enemies/Velm/VelmBlackIronDelver",
            "Black Iron Ore Fragment");
        AssertDrop("Enemies/Glaad/Grade03GlaadFrostDrake", "Wyvern Scale");
    }

    [Test]
    public void MonsterFang_RemainsAvailableInTheStartingDungeon()
    {
        DungeonDataSO dungeon = Resources.Load<DungeonDataSO>("GameData/Dungeons/DungeonData");

        Assert.That(dungeon.normalEnemies.Any(enemy =>
            enemy.enemyName == "Wild Dog" && HasDrop(enemy, "Monster Fang")), Is.True);
    }

    [Test]
    public void SaleAndLeafBlacksmithRecipes_UseOnlyMaterialsAvailableAtFirstTown()
    {
        DungeonDataSO startingCave = Resources.Load<DungeonDataSO>(
            "GameData/Dungeons/DungeonData");
        DungeonDataSO lowerMine = Resources.Load<DungeonDataSO>(
            "GameData/Dungeons/LowerMine");
        Assert.That(startingCave, Is.Not.Null);
        Assert.That(lowerMine, Is.Not.Null);

        AssertRecipesUseOnlyAvailableMaterials(
            2,
            CollectObtainableMaterials(startingCave),
            new[]
            {
                "GameData/Blacksmith/ArcaneStaffRecipe",
                "GameData/Blacksmith/CompositeBowRecipe",
                "GameData/Blacksmith/LancerSteelLanceRecipe",
                "GameData/Blacksmith/PriestBlessedStaffRecipe",
                "GameData/Blacksmith/RogueSwiftDaggerRecipe",
                "GameData/Blacksmith/SteelSwordRecipe"
            });
        AssertRecipesUseOnlyAvailableMaterials(
            1,
            CollectObtainableMaterials(startingCave, lowerMine),
            new[]
            {
                "GameData/Blacksmith/ArcanePendantRecipe",
                "GameData/Blacksmith/BonePrayerVestmentRecipe",
                "GameData/Blacksmith/HexwoodStaffRecipe",
                "GameData/Blacksmith/RunewovenRobeRecipe",
                "GameData/Blacksmith/SanctifiedMaceRecipe",
                "GameData/Blacksmith/SpiritBeadRecipe",
                "GameData/Blacksmith/BeastboneBowRecipe",
                "GameData/Blacksmith/GolemPlateRecipe",
                "GameData/Blacksmith/HawkeyeCharmRecipe",
                "GameData/Blacksmith/IronVanguardArmorRecipe",
                "GameData/Blacksmith/OrcboneSpearRecipe",
                "GameData/Blacksmith/ShadowhideArmorRecipe",
                "GameData/Blacksmith/WindrunnerLeatherRecipe",
                "GameData/Blacksmith/WyvernCrestRecipe"
            });
    }

    private static void AssertRecipesUseOnlyAvailableMaterials(
        int firstTownIndex,
        HashSet<ItemDataSO> obtainableMaterials,
        IEnumerable<string> recipePaths)
    {
        foreach (string recipePath in recipePaths)
        {
            EquipmentRecipeSO recipe = Resources.Load<EquipmentRecipeSO>(recipePath);
            Assert.That(recipe, Is.Not.Null, recipePath);
            Assert.That(BlacksmithManager.IsRecipeAvailableInTown(recipe, firstTownIndex), Is.True,
                recipePath);
            Assert.That(recipe.materials.All(requirement =>
                requirement != null && obtainableMaterials.Contains(requirement.item)),
                Is.True,
                recipePath);
        }
    }

    private static HashSet<ItemDataSO> CollectObtainableMaterials(
        params DungeonDataSO[] dungeons)
    {
        HashSet<ItemDataSO> obtainableMaterials = new HashSet<ItemDataSO>();
        foreach (DungeonDataSO dungeon in dungeons)
        {
            if (dungeon == null)
            {
                continue;
            }

            foreach (EnemyDataSO enemy in dungeon.normalEnemies ?? new EnemyDataSO[0])
            {
                if (enemy == null)
                {
                    continue;
                }

                foreach (ItemDropEntry drop in enemy.itemDrops ?? new ItemDropEntry[0])
                {
                    if (drop?.item != null)
                    {
                        obtainableMaterials.Add(drop.item);
                    }
                }

                ItemDataSO magicStone =
                    MaterialCatalog.GetMagicStoneForEnemyGrade(enemy.monsterGrade);
                if (magicStone != null)
                {
                    obtainableMaterials.Add(magicStone);
                }
            }

            foreach (DungeonItemReward reward in dungeon.clearItemRewards ??
                     new DungeonItemReward[0])
            {
                if (reward?.item != null)
                {
                    obtainableMaterials.Add(reward.item);
                }
            }
        }
        return obtainableMaterials;
    }

    [Test]
    public void EveryRecipeMaterial_HasAnAcquisitionSource()
    {
        HashSet<ItemDataSO> enemyDrops = new HashSet<ItemDataSO>(
            Resources.LoadAll<EnemyDataSO>("GameData/Enemies")
                .Concat(Resources.LoadAll<EnemyDataSO>("Enemies"))
                .SelectMany(enemy => enemy.itemDrops ?? new ItemDropEntry[0])
                .Where(drop => drop.item != null)
                .Select(drop => drop.item));
        HashSet<ItemDataSO> systemicDrops = CollectSystemicDropMaterials();

        foreach (string guid in AssetDatabase.FindAssets("t:EquipmentRecipeSO"))
        {
            EquipmentRecipeSO recipe = AssetDatabase.LoadAssetAtPath<EquipmentRecipeSO>(
                AssetDatabase.GUIDToAssetPath(guid));
            foreach (CraftingMaterialRequirement requirement in recipe.materials)
            {
                Assert.That(enemyDrops.Contains(requirement.item) ||
                            systemicDrops.Contains(requirement.item),
                    Is.True,
                    recipe.name + ": " + requirement.item.itemName);
            }
        }
    }

    private static bool HasDrop(EnemyDataSO enemy, string itemName)
    {
        return enemy.itemDrops.Any(drop => drop.item.itemName == itemName);
    }

    private static void AssertDrop(string resourcePath, string expectedItemName)
    {
        EnemyDataSO enemy = Resources.Load<EnemyDataSO>(resourcePath);
        Assert.That(enemy, Is.Not.Null, resourcePath);
        Assert.That(HasDrop(enemy, expectedItemName), Is.True, resourcePath);
    }

    private static HashSet<ItemDataSO> CollectSystemicDropMaterials()
    {
        HashSet<ItemDataSO> results = new HashSet<ItemDataSO>();
        foreach (DungeonGrade dungeonGrade in new[]
                 {
                     DungeonGrade.Low,
                     DungeonGrade.Lower,
                     DungeonGrade.Middle,
                     DungeonGrade.Upper,
                     DungeonGrade.Highest
                 })
        {
            AddResourceItem(results,
                DungeonEnemyVariantService.GetMutantCoreResourcePath(dungeonGrade));
            foreach (DungeonEventType eventType in new[]
                     {
                         DungeonEventType.MineralVein,
                         DungeonEventType.HerbGrove,
                         DungeonEventType.QualityGrove
                     })
            {
                DungeonEventChoiceResult result =
                    DungeonEnvironmentEventService.ResolveEnvironmentalChoice(
                        eventType,
                        1,
                        dungeonGrade);
                if (result.MaterialItem != null)
                {
                    results.Add(result.MaterialItem);
                }
            }
        }
        for (int enemyGrade = 1; enemyGrade <= 10; enemyGrade++)
        {
            ItemDataSO magicStone =
                MaterialCatalog.GetMagicStoneForEnemyGrade(enemyGrade);
            if (magicStone != null)
            {
                results.Add(magicStone);
            }
        }
        foreach (ItemDataSO item in Resources.LoadAll<ItemDataSO>("GameData/Items"))
        {
            if (item.PersistentId.Contains("EnhancementOre"))
            {
                results.Add(item);
            }
        }
        return results;
    }

    private static void AddResourceItem(HashSet<ItemDataSO> results, string path)
    {
        ItemDataSO item = Resources.Load<ItemDataSO>(path);
        if (item != null)
        {
            results.Add(item);
        }
    }
}
