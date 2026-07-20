using System.Collections.Generic;
using NUnit.Framework;

public sealed class GameAssetRepositoryTests
{
    [Test]
    public void ItemPersistentIds_ArePresentAndUnique()
    {
        AssertPersistentIds<ItemDataSO>();
    }

    [Test]
    public void MercenaryPersistentIds_ArePresentAndUnique()
    {
        AssertPersistentIds<MercenaryDataSO>();
    }

    [Test]
    public void MercenaryArchetypePersistentIds_ArePresentAndUnique()
    {
        AssertPersistentIds<MercenaryArchetypeSO>();
    }

    [Test]
    public void EarlyRankClassEquipment_IsEvenlyDistributedAcrossBaseClasses()
    {
        Dictionary<MercenaryClass, int> counts =
            new Dictionary<MercenaryClass, int>();
        foreach (MercenaryClass baseClass in
                 MercenaryClassProgression.GetBaseClasses())
        {
            counts.Add(baseClass, 0);
        }

        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item == null || !item.IsEquipment ||
                item.allClassesCanEquip || item.equipmentRank > 3)
            {
                continue;
            }

            MercenaryClass baseClass =
                MercenaryClassProgression.GetBaseClass(item.requiredClass);
            if (counts.ContainsKey(baseClass))
            {
                counts[baseClass]++;
            }
        }

        int expected = counts[MercenaryClass.Warrior];
        Assert.That(expected, Is.GreaterThan(0));
        foreach (KeyValuePair<MercenaryClass, int> pair in counts)
        {
            Assert.That(pair.Value, Is.EqualTo(expected), pair.Key.ToString());
        }
    }

    [Test]
    public void DungeonPersistentIds_ArePresentAndUnique()
    {
        IReadOnlyList<DungeonDataSO> dungeons =
            GameAssetRepository.LoadAll<DungeonDataSO>();
        HashSet<string> ids = new HashSet<string>();

        Assert.That(dungeons.Count, Is.GreaterThan(0));
        foreach (DungeonDataSO dungeon in dungeons)
        {
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.PersistentId, Is.Not.Null.And.Not.Empty);
            Assert.That(
                ids.Add(dungeon.PersistentId),
                Is.True,
                $"Duplicate dungeon PersistentId: {dungeon.PersistentId}");
            Assert.That(
                GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                    dungeon.PersistentId,
                    dungeon.name),
                Is.SameAs(dungeon));
        }
    }

    [Test]
    public void HighestDungeonForEachTown_IsUniqueAndUsesExpectedEquipmentRank()
    {
        IReadOnlyList<DungeonDataSO> dungeons =
            GameAssetRepository.LoadAll<DungeonDataSO>();

        for (int townIndex = 0;
             townIndex < WorldMapService.TownCount;
             townIndex++)
        {
            DungeonDataSO highest = null;
            int highestCount = 0;
            foreach (DungeonDataSO dungeon in dungeons)
            {
                if (dungeon == null || dungeon.nearbyTownIndex != townIndex)
                {
                    continue;
                }

                if (highest == null || dungeon.grade > highest.grade)
                {
                    highest = dungeon;
                    highestCount = 1;
                }
                else if (dungeon.grade == highest.grade)
                {
                    highestCount++;
                }
            }

            Assert.That(
                highest,
                Is.Not.Null,
                $"Town {townIndex} has no dungeon.");
            Assert.That(
                highestCount,
                Is.EqualTo(1),
                $"Town {townIndex} has multiple highest-grade dungeons.");
            Assert.That(
                highest.limitedEquipmentDrops,
                Is.Not.Null.And.Not.Empty,
                $"{highest.name} has no unique equipment.");

            int expectedRank =
                WorldMapService.GetDungeonEquipmentRank(townIndex);
            foreach (ItemDataSO item in highest.limitedEquipmentDrops)
            {
                Assert.That(item, Is.Not.Null, $"{highest.name} has a missing drop.");
                Assert.That(item.IsEquipment, Is.True, item.name);
                Assert.That(
                    item.equipmentRank,
                    Is.EqualTo(expectedRank),
                    $"{highest.name}: {item.name}");
            }
        }
    }

    [Test]
    public void DungeonReorgPhase2_AddsTwoMiddleDungeonsWithRegionalRosters()
    {
        IReadOnlyList<DungeonDataSO> dungeons =
            GameAssetRepository.LoadAll<DungeonDataSO>();
        Assert.That(dungeons.Count, Is.EqualTo(15));

        AssertMiddleDungeon(
            "NornVerdantSettlement",
            "dungeon.norn_verdant_settlement",
            3,
            "enemy_job_orc_shaman",
            "enemy_job_orc_rider",
            "enemy_job_orc_bulwark",
            "enemy_job_orc_berserker",
            "enemy_job_orc_veteran",
            "Grade05OgreMage",
            "enemy_slime_slime_verdant");
        AssertMiddleDungeon(
            "GlaadDragonScaleCanyon",
            "dungeon.glaad_dragon_scale_canyon",
            4,
            "enemy_job_lizardman_shaman",
            "enemy_job_lizardman_stalker",
            "enemy_job_lizardman_scaleguard",
            "enemy_job_lizardman_ravager",
            "enemy_job_lizardman_captain",
            "Grade06Lizardman",
            "enemy_slime_slime_quicksilver");
        AssertUpperDungeon(
            "VelmFurnaceDefenseZone",
            "dungeon.velm_furnace_defense_zone",
            5,
            "Grade05IronGolem",
            "Grade05StoneGolem",
            "Grade04DarkMage",
            "Grade03GlaadSkyWarden",
            "VelmEmberforgedAutomaton",
            "enemy_slime_slime_frost_crystal");
        AssertUpperDungeon(
            "AbyssGatewayThreshold",
            "dungeon.abyss_gateway_threshold",
            6,
            "Grade02DemonKnight",
            "Grade04DarkMage",
            "Grade03Wyvern",
            "enemy_job_wyvern_hexer",
            "enemy_slime_slime_thunder",
            "AbyssSpawn");
    }

    [Test]
    public void DungeonReorgPhase5_UsesFormalRegionalBossesAndAbyssSpawn()
    {
        AssertReorgEnemy("GameData/Enemies/Expansion/AbyssSpawn", "奈落の眷属",
            EnemyRace.Demon, 4, false, "enemy.abyss_spawn", "Grade04_abyss_spawn", 430, 57, 29);
        AssertReorgEnemy("GameData/Enemies/Expansion/NornVerdantOrcHighChieftain", "翠樹の大族長",
            EnemyRace.Humanoid, 4, true, "enemy.boss.norn_verdant_orc_high_chieftain", "Grade04_norn_verdant_orc_high_chieftain", 1548, 84, 31);
        AssertReorgEnemy("GameData/Enemies/Expansion/GlaadDragonScaleKing", "竜鱗王",
            EnemyRace.Dragon, 4, true, "enemy.boss.glaad_dragon_scale_king", "Grade04_dragonscale_king", 1720, 73, 42);
        AssertReorgEnemy("GameData/Enemies/Expansion/VelmGrandFurnaceColossus", "大熔炉巨像",
            EnemyRace.Construct, 3, true, "enemy.boss.velm_grand_furnace_colossus", "Grade03_grand_furnace_colossus", 4104, 97, 65);
        AssertReorgEnemy("GameData/Enemies/Expansion/AbyssGatekeeper", "奈落の門衛",
            EnemyRace.Demon, 3, true, "enemy.boss.abyss_gatekeeper", "Grade03_abyss_gatekeeper", 3040, 113, 54);
        AssertReorgEnemy("GameData/Enemies/Expansion/EldOldQuarryGravelord", "旧採石場の骸王",
            EnemyRace.Undead, 6, true, "enemy.boss.eld_old_quarry_gravelord", "Grade06_eld_quarry_gravelord", 810, 29, 16);

        AssertBoss("GameData/Dungeons/NornVerdantSettlement", "NornVerdantOrcHighChieftain", EnemyRace.Humanoid);
        AssertBoss("GameData/Dungeons/GlaadDragonScaleCanyon", "GlaadDragonScaleKing", EnemyRace.Dragon);
        AssertBoss("GameData/Dungeons/VelmFurnaceDefenseZone", "VelmGrandFurnaceColossus", EnemyRace.Construct);
        DungeonDataSO abyss = UnityEngine.Resources.Load<DungeonDataSO>("GameData/Dungeons/AbyssGatewayThreshold");
        Assert.That(abyss.normalEnemies, Has.Some.Matches<EnemyDataSO>(enemy => enemy.name == "AbyssSpawn"));
        AssertBoss("GameData/Dungeons/AbyssGatewayThreshold", "AbyssGatekeeper", EnemyRace.Demon);
        AssertBoss("Dungeons/EldOldQuarry", "EldOldQuarryGravelord", EnemyRace.Undead);
        AssertBoss("GameData/Dungeons/LowerMine", "Boss06MineTyrant", EnemyRace.Humanoid);
    }

    [Test]
    public void VelmBlackIronMine_IsHighestAndKeepsRankEightLimitedEquipment()
    {
        DungeonDataSO mine = UnityEngine.Resources.Load<DungeonDataSO>(
            "Dungeons/VelmBlackIronMine");
        Assert.That(mine, Is.Not.Null);
        Assert.That(mine.grade, Is.EqualTo(DungeonGrade.Highest));
        Assert.That(mine.nearbyTownIndex, Is.EqualTo(5));
        Assert.That(mine.clearGoldReward, Is.EqualTo(1500));
        foreach (ItemDataSO item in mine.limitedEquipmentDrops)
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item.equipmentRank, Is.EqualTo(8));
        }

        AssertVelmEnemy("Enemies/Velm/VelmBlackIronDelver", 2, 1418, 121, 74, 874);
        AssertVelmEnemy("Enemies/Velm/VelmEmberforgedAutomaton", 2, 1914, 109, 88, 874);
        AssertVelmEnemy("Enemies/Velm/VelmMagmaDrake", 1, 2268, 193, 99, 1250);
        AssertVelmEnemy("Enemies/Velm/VelmDeepforgeHexer", 1, 2520, 176, 110, 1250);
        AssertVelmEnemy("Enemies/Velm/VelmDeepforgeOverlord", 1, 10080, 227, 132, 1250);
        EnemyDataSO overlord = UnityEngine.Resources.Load<EnemyDataSO>(
            "Enemies/Velm/VelmDeepforgeOverlord");
        Assert.That(overlord.experienceMultiplier, Is.EqualTo(4f));
    }

    [Test]
    public void DungeonSpecialBossChance_IsHiddenElementLevel()
    {
        foreach (DungeonDataSO dungeon in
                 GameAssetRepository.LoadAll<DungeonDataSO>())
        {
            Assert.That(
                dungeon.specialBossChance,
                Is.InRange(0f, 0.05f),
                $"{dungeon.name} special boss chance is too high.");
        }
    }

    [Test]
    public void DungeonSpecialVariantSkillPools_AreValidAndCoverNewSkills()
    {
        HashSet<EnemySkillType> configuredSkills =
            new HashSet<EnemySkillType>();

        foreach (DungeonDataSO dungeon in
                 GameAssetRepository.LoadAll<DungeonDataSO>())
        {
            Assert.That(
                dungeon.specialVariantSkillPool,
                Is.Not.Null.And.Not.Empty,
                $"{dungeon.name} has no special-variant skill pool.");

            HashSet<EnemySkillType> dungeonSkills =
                new HashSet<EnemySkillType>();
            foreach (EnemySkillType skill in dungeon.specialVariantSkillPool)
            {
                Assert.That(skill, Is.Not.EqualTo(EnemySkillType.None),
                    $"{dungeon.name} contains the None skill.");
                Assert.That(dungeonSkills.Add(skill), Is.True,
                    $"{dungeon.name} contains duplicate skill {skill}.");
                configuredSkills.Add(skill);
            }
        }

        EnemySkillType[] addedSkills =
        {
            EnemySkillType.ArcaneBolt,
            EnemySkillType.MeteorRain,
            EnemySkillType.CrushingBlow,
            EnemySkillType.BerserkRush,
            EnemySkillType.Regeneration,
            EnemySkillType.SoulBurst
        };
        foreach (EnemySkillType skill in addedSkills)
        {
            Assert.That(configuredSkills.Contains(skill), Is.True,
                $"No dungeon can grant the added skill {skill}.");
        }
    }

    [Test]
    public void GlaadAndVelmDungeons_UseDistinctRegionalEnemyRosters()
    {
        DungeonDataSO glaad =
            GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                "dungeon.GlaadSkyFortress");
        DungeonDataSO velm =
            GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                "dungeon.VelmBlackIronMine");

        Assert.That(glaad, Is.Not.Null);
        Assert.That(velm, Is.Not.Null);
        Assert.That(glaad.normalEnemies.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(velm.normalEnemies.Length, Is.GreaterThanOrEqualTo(3));
        Assert.That(glaad.bossEnemy, Is.Not.Null);
        Assert.That(velm.bossEnemy, Is.Not.Null);
        Assert.That(velm.bossEnemy, Is.Not.SameAs(glaad.bossEnemy));

        HashSet<EnemyDataSO> glaadEnemies =
            new HashSet<EnemyDataSO>(glaad.normalEnemies);
        foreach (EnemyDataSO enemy in velm.normalEnemies)
        {
            Assert.That(enemy, Is.Not.Null);
            Assert.That(glaadEnemies.Contains(enemy), Is.False,
                $"Shared regional enemy: {enemy.name}");
        }
    }

    [Test]
    public void VelmEnemyRoster_IsStrongerThanGlaadRosterOnAverage()
    {
        DungeonDataSO glaad =
            GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                "dungeon.GlaadSkyFortress");
        DungeonDataSO velm =
            GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                "dungeon.VelmBlackIronMine");

        Assert.That(AverageStat(velm.normalEnemies, enemy => enemy.maxHP),
            Is.GreaterThan(AverageStat(glaad.normalEnemies, enemy => enemy.maxHP)));
        Assert.That(AverageStat(velm.normalEnemies, enemy => enemy.attack),
            Is.GreaterThan(AverageStat(glaad.normalEnemies, enemy => enemy.attack)));
        Assert.That(velm.bossEnemy.maxHP, Is.GreaterThan(glaad.bossEnemy.maxHP));
        Assert.That(velm.bossEnemy.attack, Is.GreaterThan(glaad.bossEnemy.attack));
    }

    private static void AssertPersistentIds<T>()
        where T : UnityEngine.Object
    {
        IReadOnlyList<T> assets = GameAssetRepository.LoadAll<T>();
        HashSet<string> ids = new HashSet<string>();

        Assert.That(assets.Count, Is.GreaterThan(0));
        foreach (T asset in assets)
        {
            Assert.That(asset, Is.InstanceOf<IPersistentGameAsset>());
            string id = ((IPersistentGameAsset)asset).PersistentId;
            Assert.That(id, Is.Not.Null.And.Not.Empty, asset.name);
            Assert.That(
                ids.Add(id),
                Is.True,
                $"Duplicate {typeof(T).Name} PersistentId: {id}");
            Assert.That(
                GameAssetRepository.FindByPersistentId<T>(id, asset.name),
                Is.SameAs(asset));
        }
    }

    private static void AssertMiddleDungeon(
        string assetName,
        string persistentId,
        int nearbyTownIndex,
        params string[] enemyAssetNames)
    {
        DungeonDataSO dungeon = UnityEngine.Resources.Load<DungeonDataSO>(
            "GameData/Dungeons/" + assetName);
        Assert.That(dungeon, Is.Not.Null, assetName);
        Assert.That(dungeon.PersistentId, Is.EqualTo(persistentId));
        Assert.That(dungeon.grade, Is.EqualTo(DungeonGrade.Middle));
        Assert.That(dungeon.worldMapIndex, Is.EqualTo(1));
        Assert.That(dungeon.nearbyTownIndex, Is.EqualTo(nearbyTownIndex));
        Assert.That(dungeon.totalFloors, Is.EqualTo(5));
        Assert.That(dungeon.bossEnemy, Is.Not.Null);
        Assert.That(dungeon.normalEnemies, Has.Length.EqualTo(enemyAssetNames.Length));
        foreach (string enemyAssetName in enemyAssetNames)
        {
            Assert.That(dungeon.normalEnemies,
                Has.Some.Matches<EnemyDataSO>(enemy => enemy.name == enemyAssetName),
                assetName + ": " + enemyAssetName);
        }
    }

    private static void AssertUpperDungeon(
        string assetName,
        string persistentId,
        int nearbyTownIndex,
        params string[] enemyAssetNames)
    {
        DungeonDataSO dungeon = UnityEngine.Resources.Load<DungeonDataSO>(
            "GameData/Dungeons/" + assetName);
        Assert.That(dungeon, Is.Not.Null, assetName);
        Assert.That(dungeon.PersistentId, Is.EqualTo(persistentId));
        Assert.That(dungeon.grade, Is.EqualTo(DungeonGrade.Upper));
        Assert.That(dungeon.worldMapIndex, Is.EqualTo(2));
        Assert.That(dungeon.nearbyTownIndex, Is.EqualTo(nearbyTownIndex));
        Assert.That(dungeon.totalFloors, Is.EqualTo(6));
        Assert.That(dungeon.bossEnemy, Is.Not.Null);
        Assert.That(dungeon.normalEnemies, Has.Length.EqualTo(enemyAssetNames.Length));
        foreach (string enemyAssetName in enemyAssetNames)
        {
            Assert.That(dungeon.normalEnemies,
                Has.Some.Matches<EnemyDataSO>(enemy => enemy.name == enemyAssetName),
                assetName + ": " + enemyAssetName);
        }
    }

    private static void AssertReorgEnemy(
        string resourcePath,
        string japaneseName,
        EnemyRace race,
        int grade,
        bool isBoss,
        string persistentId,
        string battleVisualKey,
        int hitPoints,
        int attack,
        int defense)
    {
        EnemyDataSO enemy = UnityEngine.Resources.Load<EnemyDataSO>(resourcePath);
        Assert.That(enemy, Is.Not.Null, resourcePath);
        Assert.That(enemy.race, Is.EqualTo(race), resourcePath);
        Assert.That(enemy.monsterGrade, Is.EqualTo(grade), resourcePath);
        Assert.That(enemy.isBoss, Is.EqualTo(isBoss), resourcePath);
        Assert.That(enemy.PersistentId, Is.EqualTo(persistentId), resourcePath);
        Assert.That(enemy.battleVisualKey, Is.EqualTo(battleVisualKey), resourcePath);
        Assert.That(JapaneseDisplayText.GetEnemyName(enemy.enemyName), Is.EqualTo(japaneseName), resourcePath);
        Assert.That(enemy.maxHP, Is.EqualTo(hitPoints), resourcePath);
        Assert.That(enemy.attack, Is.EqualTo(attack), resourcePath);
        Assert.That(enemy.defense, Is.EqualTo(defense), resourcePath);
    }

    private static void AssertBoss(
        string dungeonResourcePath,
        string bossAssetName,
        EnemyRace expectedRace)
    {
        DungeonDataSO dungeon = UnityEngine.Resources.Load<DungeonDataSO>(dungeonResourcePath);
        Assert.That(dungeon, Is.Not.Null, dungeonResourcePath);
        Assert.That(dungeon.bossEnemy, Is.Not.Null, dungeonResourcePath);
        Assert.That(dungeon.bossEnemy.name, Is.EqualTo(bossAssetName), dungeonResourcePath);
        Assert.That(dungeon.bossEnemy.race, Is.EqualTo(expectedRace), dungeonResourcePath);
    }

    private static void AssertVelmEnemy(
        string resourcePath,
        int grade,
        int hitPoints,
        int attack,
        int defense,
        int gold)
    {
        EnemyDataSO enemy = UnityEngine.Resources.Load<EnemyDataSO>(resourcePath);
        Assert.That(enemy, Is.Not.Null, resourcePath);
        Assert.That(enemy.monsterGrade, Is.EqualTo(grade), resourcePath);
        Assert.That(enemy.maxHP, Is.EqualTo(hitPoints), resourcePath);
        Assert.That(enemy.attack, Is.EqualTo(attack), resourcePath);
        Assert.That(enemy.defense, Is.EqualTo(defense), resourcePath);
        Assert.That(enemy.goldReward, Is.EqualTo(gold), resourcePath);
    }

    private static float AverageStat(
        IReadOnlyList<EnemyDataSO> enemies,
        System.Func<EnemyDataSO, int> selector)
    {
        int total = 0;
        foreach (EnemyDataSO enemy in enemies)
        {
            Assert.That(enemy, Is.Not.Null);
            total += selector(enemy);
        }

        return enemies.Count > 0 ? total / (float)enemies.Count : 0f;
    }
}
