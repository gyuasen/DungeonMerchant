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
