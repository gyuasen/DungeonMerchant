using System.Collections.Generic;
using NUnit.Framework;

public sealed class DungeonRosterParityTests
{
    [Test]
    public void ReorganizedDungeons_UseTheirApprovedEnemyRosters()
    {
        AssertRoster("dungeon.DungeonData", 6,
            "EnemyData", "Grade10GreenSlime", "Grade10BlueSlime",
            "Grade10MossSlime", "Grade10HornRabbit", "Grade09WildDog");
        AssertRoster("dungeon.LeafForestTrail", 7,
            "Grade09Kobold", "enemy_job_kobold_hexer",
            "enemy_job_kobold_prowler", "enemy_job_kobold_bulwark",
            "enemy_job_kobold_ravager", "enemy_job_kobold_packleader",
            "Grade09WildDog");
        AssertRoster("dungeon.LowerMine", 8,
            "Grade07Zombie", "Grade07BoneHound", "Grade08CaveBat",
            "Grade08GiantRat", "Grade08CaveSpider", "Grade08VenomMoth",
            "Grade08RockBeetle", "enemy_slime_slime_venom");
        AssertRoster("dungeon.MiddleRuins", 6,
            "Grade05StoneGolem", "Grade05IronGolem", "Grade05OgreMage",
            "Grade06Hobgoblin", "Grade06Troll", "Grade06MarshLizard");
        AssertRoster("dungeon.NornCanopyLabyrinth", 5,
            "Grade04DarkMage", "Grade05IronGolem", "Grade05StoneGolem",
            "enemy_slime_slime_thunder", "Grade06Orc");
        AssertRoster("dungeon.velm_furnace_defense_zone", 6,
            "Grade05IronGolem", "Grade05StoneGolem", "Grade04DarkMage",
            "enemy_slime_slime_frost_crystal", "Grade07Wraith",
            "Grade05OgreMage");
    }

    [Test]
    public void RemovedRegionalEnemies_AppearOnlyInTheirApprovedDungeon()
    {
        AssertAppearsInExactlyOneDungeon(
            "VelmEmberforgedAutomaton", "dungeon.VelmBlackIronMine");
        AssertAppearsInExactlyOneDungeon(
            "Grade03GlaadSkyWarden", "dungeon.GlaadSkyFortress");
        AssertAppearsInExactlyOneDungeon(
            "Grade07Skeleton", "dungeon.EldOldQuarry");
        AssertAppearsInExactlyOneDungeon(
            "Grade07ArmoredSkeleton", "dungeon.EldOldQuarry");
        AssertAppearsInExactlyOneDungeon(
            "Grade06Lizardman", "dungeon.glaad_dragon_scale_canyon");
    }

    private static void AssertRoster(
        string dungeonPersistentId,
        int expectedCount,
        params string[] expectedEnemyNames)
    {
        DungeonDataSO dungeon =
            GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                dungeonPersistentId);

        Assert.That(dungeon, Is.Not.Null, dungeonPersistentId);
        Assert.That(dungeon.normalEnemies, Has.Length.EqualTo(expectedCount));
        foreach (string expectedEnemyName in expectedEnemyNames)
        {
            Assert.That(dungeon.normalEnemies,
                Has.Some.Matches<EnemyDataSO>(
                    enemy => enemy != null && enemy.name == expectedEnemyName),
                dungeonPersistentId + ": " + expectedEnemyName);
        }
    }

    private static void AssertAppearsInExactlyOneDungeon(
        string enemyName,
        string expectedDungeonPersistentId)
    {
        int count = 0;
        string actualDungeonPersistentId = string.Empty;
        foreach (DungeonDataSO dungeon in
                 GameAssetRepository.LoadAll<DungeonDataSO>())
        {
            if (dungeon?.normalEnemies == null)
            {
                continue;
            }

            foreach (EnemyDataSO enemy in dungeon.normalEnemies)
            {
                if (enemy != null && enemy.name == enemyName)
                {
                    count++;
                    actualDungeonPersistentId = dungeon.PersistentId;
                }
            }
        }

        Assert.That(count, Is.EqualTo(1), enemyName);
        Assert.That(actualDungeonPersistentId,
            Is.EqualTo(expectedDungeonPersistentId), enemyName);
    }
}
