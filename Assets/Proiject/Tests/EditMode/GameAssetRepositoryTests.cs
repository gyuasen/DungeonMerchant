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
