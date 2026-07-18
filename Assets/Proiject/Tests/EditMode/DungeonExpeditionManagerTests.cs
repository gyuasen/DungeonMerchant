using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class DungeonExpeditionManagerTests
{
    private GameObject root;
    private MerchantData merchant;
    private MerchantInventory inventory;
    private DayManager dayManager;
    private MercenaryHireManager hire;
    private MercenaryPartyManager party;
    private TransportManager transport;
    private TownProgressState townProgress;
    private DungeonRunManager runs;
    private DungeonExpeditionManager expeditions;
    private readonly List<Object> assets = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Expedition Test");
        dayManager = root.AddComponent<DayManager>();
        merchant = root.AddComponent<MerchantData>();
        inventory = root.AddComponent<MerchantInventory>();
        townProgress = root.AddComponent<TownProgressState>();
        townProgress.Initialize(2, new[] { 2, 1 });
        hire = root.AddComponent<MercenaryHireManager>();
        party = root.AddComponent<MercenaryPartyManager>();
        transport = root.AddComponent<TransportManager>();
        runs = root.AddComponent<DungeonRunManager>();
        expeditions = root.AddComponent<DungeonExpeditionManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        foreach (Object asset in assets)
        {
            Object.DestroyImmediate(asset);
        }
    }

    [TestCase(DungeonGrade.Low, 100)]
    [TestCase(DungeonGrade.Lower, 220)]
    [TestCase(DungeonGrade.Middle, 420)]
    [TestCase(DungeonGrade.Upper, 750)]
    [TestCase(DungeonGrade.Highest, 1200)]
    public void RequiredStrength_IsPinned(DungeonGrade grade, int expected)
    {
        DungeonDataSO dungeon = CreateDungeon(grade, 1);
        Assert.That(expeditions.GetRequiredStrength(dungeon), Is.EqualTo(expected));
    }

    [Test]
    public void FormAndRecall_ReservesMercenary()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance member = Hire("member", 100, 100, 100);
        Assert.That(expeditions.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        Assert.That(expeditions.IsMercenaryOnExpeditionDuty(member.InstanceId), Is.True);
        expeditions.RecallExpedition(expeditions.ActiveExpeditions[0]);
        Assert.That(expeditions.IsMercenaryOnExpeditionDuty(member.InstanceId), Is.False);
    }

    [Test]
    public void Form_RejectsHiddenAndUnclearedDungeons()
    {
        MercenaryInstance member = Hire("member", 100, 100, 100);
        DungeonDataSO hidden = CreateDungeon(DungeonGrade.Low, 1);
        hidden.nearbyTownIndex = WorldMapService.HiddenIslandTownIndex;
        Assert.That(expeditions.TryFormExpedition(hidden, new[] { member }), Is.EqualTo(ExpeditionFormationResult.HiddenDungeon));
        DungeonDataSO uncleared = CreateDungeon(DungeonGrade.Low, 1);
        Assert.That(expeditions.TryFormExpedition(uncleared, new[] { member }), Is.EqualTo(ExpeditionFormationResult.DungeonNotCleared));
    }

    [Test]
    public void TryFormExpedition_RejectsUnavailableMembersAndFourMembers_WithoutSideEffects()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance partyMember = Hire("party", 100, 100, 100);
        Assert.That(party.TryAdd(partyMember), Is.True);
        AssertRejected(dungeon, new[] { partyMember });
        party.Remove(partyMember);

        MercenaryInstance transportMember = Hire("transport", 100, 100, 100);
        ItemDataSO cargo = CreateItem("cargo");
        inventory.AddItem(cargo, 1);
        Assert.That(transport.TryDepartConvoy(1, new[] { (cargo, 1) }, new[] { transportMember }), Is.EqualTo(TransportDepartureResult.Succeeded));
        AssertRejected(dungeon, new[] { transportMember });

        MercenaryInstance expeditionMember = Hire("expedition", 100, 100, 100);
        Assert.That(expeditions.TryFormExpedition(dungeon, new[] { expeditionMember }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        AssertRejected(dungeon, new[] { expeditionMember });

        MercenaryInstance one = Hire("one", 100, 100, 100);
        MercenaryInstance two = Hire("two", 100, 100, 100);
        MercenaryInstance three = Hire("three", 100, 100, 100);
        MercenaryInstance four = Hire("four", 100, 100, 100);
        AssertRejected(dungeon, new[] { one, two, three, four });
    }

    [Test]
    public void DayChanged_StrongExpedition_SimulatesThreeNormalEncountersAtNearbyTown()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        dungeon.clearGoldReward = 101;
        dungeon.firstEncounterEnemyCount = 1;
        dungeon.enemyCountIncreasePerEncounter = 1;
        dungeon.maxEnemyCountPerEncounter = 3;
        ItemDataSO material = CreateItem("material");
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.goldReward = 7;
        enemy.itemDrops = new[] { new ItemDropEntry { item = material, amount = 1, dropChance = 1f } };
        dungeon.normalEnemies = new[] { enemy };
        assets.Add(enemy);
        MercenaryInstance member = Hire("strong", 100, 100, 100);
        int goldBefore = merchant.Gold;
        int experienceBefore = member.CurrentExperience;
        expeditions.SetRandomProvider(() => 0f);
        Assert.That(expeditions.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();
        Assert.That(merchant.Gold, Is.EqualTo(goldBefore + 42));
        Assert.That(inventory.GetItemAmountIn(dungeon.nearbyTownIndex, material), Is.EqualTo(6));
        Assert.That(member.CurrentExperience, Is.GreaterThan(experienceBefore));
    }

    [Test]
    public void DayChanged_WeakExpedition_DamagesWithoutRewardsAndNeverKills()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        dungeon.grade = DungeonGrade.Highest;
        ItemDataSO material = CreateItem("material");
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.itemDrops = new[] { new ItemDropEntry { item = material, amount = 1, dropChance = 1f } };
        dungeon.normalEnemies = new[] { enemy };
        assets.Add(enemy);
        MercenaryInstance member = Hire("weak", 1, 0, 0);
        int goldBefore = merchant.Gold;
        expeditions.SetRandomProvider(() => 0f);
        Assert.That(expeditions.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();
        Assert.That(member.CurrentHP, Is.EqualTo(1));
        Assert.That(merchant.Gold, Is.EqualTo(goldBefore));
        Assert.That(inventory.GetItemAmountIn(dungeon.nearbyTownIndex, material), Is.Zero);
    }

    [Test]
    public void DayChanged_SpecialVariant_UsesDungeonGradeMutantCore()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        dungeon.firstEncounterEnemyCount = 1;
        dungeon.enemyCountIncreasePerEncounter = 0;
        dungeon.maxEnemyCountPerEncounter = 1;
        dungeon.specialVariantChance = 1f;
        dungeon.specialVariantSkillPool = new[] { EnemySkillType.PowerStrike };
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.category = EnemyCategory.Normal;
        dungeon.normalEnemies = new[] { enemy };
        assets.Add(enemy);
        ItemDataSO expectedCore = Resources.Load<ItemDataSO>("Items/Special/MutantCore");
        List<ItemDataSO> rewards = new List<ItemDataSO>();
        expeditions.ExpeditionEventOccurred += value => rewards.AddRange(value.Materials);
        MercenaryInstance member = Hire("strong", 100, 100, 100);
        expeditions.SetRandomProvider(() => 0f);

        Assert.That(expeditions.TryFormExpedition(dungeon, new[] { member }), Is.EqualTo(ExpeditionFormationResult.Succeeded));
        dayManager.AdvanceDay();

        Assert.That(expectedCore, Is.Not.Null);
        Assert.That(rewards, Does.Contain(expectedCore));
    }

    [Test]
    public void BattleRewardCalculation_MatchesExpeditionRewardFormula()
    {
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.goldReward = 11;
        ItemDataSO material = CreateItem("shared material");
        enemy.itemDrops = new[] { new ItemDropEntry { item = material, amount = 2, dropChance = 1f } };
        assets.Add(enemy);

        BattleRewardService.VictoryRewardCalculation calculation =
            BattleRewardService.CalculateVictoryRewards(
                new[] { enemy },
                2,
                // Keep this formula test focused on the configured item drop.
                // The second roll skips the new 30% magic-stone drop.
                () => 1f,
                null);

        Assert.That(calculation.Gold, Is.EqualTo(BattleRewardService.CalculateGoldReward(new[] { enemy })));
        Assert.That(calculation.ExperiencePerMercenary, Is.EqualTo(BattleRewardService.CalculateExperiencePerMercenary(BattleRewardService.CalculateExperienceReward(new[] { enemy }), 2)));
        Assert.That(calculation.ItemDrops.Count, Is.EqualTo(1));
        Assert.That(calculation.ItemDrops[0].item, Is.EqualTo(material));
    }

    [Test]
    public void DayChanged_LimitedEquipmentDrop_DepositsEquipmentInNearbyTown()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        dungeon.bossLimitedDropChance = 0.8f;
        ItemDataSO equipment = CreateEquipment("Oni Hunter Blade");
        dungeon.limitedEquipmentDrops = new[] { equipment };
        MercenaryInstance member = Hire("strong", 100, 100, 100);
        expeditions.SetRandomProvider(() => 0f);

        Assert.That(
            expeditions.TryFormExpedition(dungeon, new[] { member }),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));

        dayManager.AdvanceDay();

        IReadOnlyList<EquipmentInstance> stored =
            inventory.GetEquipmentInstancesIn(dungeon.nearbyTownIndex);
        Assert.That(stored.Count, Is.EqualTo(1));
        Assert.That(stored[0].BaseItem, Is.EqualTo(equipment));
    }

    [Test]
    public void DayChanged_LimitedEquipmentDropAboveThreshold_DoesNotDepositEquipment()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        dungeon.bossLimitedDropChance = 0.8f;
        dungeon.limitedEquipmentDrops = new[] { CreateEquipment("Oni Hunter Blade") };
        MercenaryInstance member = Hire("strong", 100, 100, 100);
        expeditions.SetRandomProvider(() => 0.5f);

        Assert.That(
            expeditions.TryFormExpedition(dungeon, new[] { member }),
            Is.EqualTo(ExpeditionFormationResult.Succeeded));

        dayManager.AdvanceDay();

        Assert.That(
            inventory.GetEquipmentInstancesIn(dungeon.nearbyTownIndex).Count,
            Is.Zero);
    }

    [Test]
    public void LimitedDropRateMultiplier_IsPinnedToHalf()
    {
        Assert.That(
            DungeonExpeditionManager.LimitedDropRateMultiplier,
            Is.EqualTo(0.5f));
    }

    [Test]
    public void SaveRoundTrip_PreservesExpedition()
    {
        DungeonDataSO dungeon = CreateClearedDungeon();
        MercenaryInstance member = Hire("member", 100, 100, 100);
        expeditions.TryFormExpedition(dungeon, new[] { member });
        List<SavedDungeonExpedition> saved = expeditions.CreateSaveData();
        GameObject restoredRoot = new GameObject("Restored Expedition Test");
        DungeonExpeditionManager restored = restoredRoot.AddComponent<DungeonExpeditionManager>();
        restored.Restore(saved, new Dictionary<string, MercenaryInstance> { { member.InstanceId, member } });
        Assert.That(restored.ActiveExpeditions.Count, Is.EqualTo(1));
        Assert.That(restored.IsMercenaryOnExpeditionDuty(member.InstanceId), Is.True);
        Object.DestroyImmediate(restoredRoot);
    }

    private DungeonDataSO CreateClearedDungeon()
    {
        DungeonDataSO dungeon = CreateDungeon(DungeonGrade.Low, 1);
        FieldInfo field = typeof(DungeonRunManager).GetField("availableDungeons", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(runs, new List<DungeonDataSO> { dungeon });
        runs.RestoreProgress(DungeonGrade.Low, string.Empty, string.Empty, new[] { new SavedDungeonFloorProgress { dungeonPersistentId = dungeon.PersistentId, dungeonAssetName = dungeon.name, clearedFloors = dungeon.totalFloors } });
        return dungeon;
    }

    private DungeonDataSO CreateDungeon(DungeonGrade grade, int floors)
    {
        DungeonDataSO dungeon = ScriptableObject.CreateInstance<DungeonDataSO>();
        dungeon.name = "TestDungeon" + assets.Count;
        dungeon.dungeonName = dungeon.name;
        dungeon.grade = grade;
        dungeon.totalFloors = floors;
        dungeon.nearbyTownIndex = 2;
        dungeon.clearGoldReward = 100;
        assets.Add(dungeon);
        return dungeon;
    }

    private MercenaryInstance Hire(string id, int hp, int attack, int defense)
    {
        MercenaryInstance member = MercenaryInstance.CreateRestored(id, null, null, id, MercenaryClass.Warrior, MercenaryContractType.Local, 1, 0, hp, hp, attack, defense, 0, 1f, 0);
        member.SetCurrentTownIndex(townProgress.CurrentTownIndex);
        hire.RestoreHiredMercenaries(new[] { member });
        return member;
    }

    private ItemDataSO CreateItem(string itemName)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = itemName + assets.Count;
        item.itemName = item.name;
        assets.Add(item);
        return item;
    }

    private ItemDataSO CreateEquipment(string itemName)
    {
        ItemDataSO item = CreateItem(itemName);
        item.itemType = ItemType.Equipment;
        // Limited drops must match the town's dungeon equipment rank
        // (WorldMapService.IsDungeonEquipmentRankAllowed); the fixtures use
        // nearbyTownIndex 2, so derive the rank instead of hardcoding 1.
        item.equipmentRank = WorldMapService.GetDungeonEquipmentRank(2);
        return item;
    }

    private void AssertRejected(
        DungeonDataSO dungeon,
        IReadOnlyList<MercenaryInstance> members)
    {
        int count = expeditions.ActiveExpeditions.Count;
        Assert.That(
            expeditions.TryFormExpedition(dungeon, members),
            Is.EqualTo(ExpeditionFormationResult.InvalidMembers));
        Assert.That(expeditions.ActiveExpeditions.Count, Is.EqualTo(count));
    }
}
