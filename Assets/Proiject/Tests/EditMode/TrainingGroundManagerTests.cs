using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class TrainingGroundManagerTests
{
    private GameObject root;
    private DayManager dayManager;
    private MerchantData merchantData;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private HealingManager healingManager;
    private MerchantInventory inventory;
    private TransportManager transportManager;
    private TrainingGroundManager trainingGroundManager;
    private TownProgressState townProgressState;
    private readonly List<MercenaryDataSO> createdData =
        new List<MercenaryDataSO>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Training Ground Manager Test");
        dayManager = root.AddComponent<DayManager>();
        merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<MarketPriceManager>();
        inventory = root.AddComponent<MerchantInventory>();
        townProgressState = root.AddComponent<TownProgressState>();
        townProgressState.Initialize(1, new[] { 2, 1 });
        hireManager = root.AddComponent<MercenaryHireManager>();
        partyManager = root.AddComponent<MercenaryPartyManager>();
        transportManager = root.AddComponent<TransportManager>();
        root.AddComponent<DungeonExpeditionManager>();
        healingManager = root.AddComponent<HealingManager>();
        trainingGroundManager = root.AddComponent<TrainingGroundManager>();
        merchantData.SetGold(100000);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        foreach (MercenaryDataSO data in createdData)
        {
            Object.DestroyImmediate(data);
        }

        createdData.Clear();
    }

    [TestCase(2, 100)]
    [TestCase(5, 625)]
    [TestCase(10, 2500)]
    [TestCase(20, 10000)]
    public void GetCost_UsesTargetLevelSquared(int targetLevel, int expected)
    {
        Assert.That(TrainingCostService.GetCost(targetLevel), Is.EqualTo(expected));
    }

    [Test]
    public void Training_CompletesOnceAndAddsOnlyMissingExperience()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        trainee.AddExperience(25);
        int hpBefore = trainee.BaseMaxHP;
        Hire(trainee, benchmark);
        int completedCount = 0;
        trainingGroundManager.TrainingCompleted += _ => completedCount++;

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.True);
        Assert.That(merchantData.Gold, Is.EqualTo(99900));
        dayManager.AdvanceDays(2);
        dayManager.AdvanceDay();

        Assert.That(trainee.Level, Is.EqualTo(2));
        Assert.That(trainee.CurrentExperience, Is.Zero);
        Assert.That(trainee.BaseMaxHP, Is.EqualTo(hpBefore + 12));
        Assert.That(completedCount, Is.EqualTo(1));
        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
    }

    [Test]
    public void CanStartTraining_RejectsLevelCap()
    {
        MercenaryInstance capped = CreateMercenary("Capped", 15);
        MercenaryInstance benchmark =
            CreateMercenary("Benchmark", 20, MercenaryClass.Knight);
        Hire(capped, benchmark);

        Assert.That(trainingGroundManager.TryStartTraining(capped), Is.False);
    }

    [Test]
    public void TryStartTraining_UsesTownServicePolicyWithoutManualConfiguration()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        Hire(trainee, benchmark);

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.True);

        trainingGroundManager.Restore(null);
        townProgressState.SetCurrentTown(2);
        trainee.SetCurrentTownIndex(2);

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.False);
    }

    [Test]
    public void RestoreReservations_DiscardsUnavailableTown()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        Hire(trainee, CreateMercenary("Benchmark", 10));

        trainingGroundManager.RestoreReservations(new[]
        {
            new TrainingReservation(
                trainee.InstanceId,
                2,
                1,
                2,
                2,
                100)
        });

        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
    }

    [Test]
    public void CanStartTraining_EnforcesHighestOwnedLevelMinusTwoBoundary()
    {
        MercenaryInstance levelOne = CreateMercenary("Level One", 1);
        MercenaryInstance levelEight = CreateMercenary("Level Eight", 8);
        MercenaryInstance levelTen = CreateMercenary("Level Ten", 10);
        Hire(levelOne, levelEight, levelTen);

        Assert.That(trainingGroundManager.CanStartTraining(levelOne), Is.True);
        Assert.That(trainingGroundManager.CanStartTraining(levelEight), Is.False);
        Assert.That(trainingGroundManager.CanStartTraining(levelTen), Is.False);
    }

    [Test]
    public void CanStartTraining_RejectsWhenHighestLevelDoesNotLeaveTwoLevels()
    {
        MercenaryInstance levelOne = CreateMercenary("Level One", 1);
        MercenaryInstance levelThree = CreateMercenary("Level Three", 3);
        Hire(levelOne, levelThree);

        Assert.That(trainingGroundManager.CanStartTraining(levelOne), Is.False);

        Hire(new MercenaryInstance[] { CreateMercenary("Only One", 10) });
        hireManager.RestoreHiredMercenaries(new[] { CreateMercenary("Solo", 10) });
        Assert.That(trainingGroundManager.CanStartTraining(
            hireManager.HiredMercenaries[0]), Is.False);

        MercenaryInstance first = CreateMercenary("First", 1);
        MercenaryInstance second = CreateMercenary("Second", 1);
        hireManager.RestoreHiredMercenaries(new[] { first, second });
        Assert.That(trainingGroundManager.CanStartTraining(first), Is.False);
    }

    [Test]
    public void TryStartTraining_RejectsFourthConcurrentReservation()
    {
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        MercenaryInstance first = CreateMercenary("First", 1);
        MercenaryInstance second = CreateMercenary("Second", 1);
        MercenaryInstance third = CreateMercenary("Third", 1);
        MercenaryInstance fourth = CreateMercenary("Fourth", 1);
        Hire(benchmark, first, second, third, fourth);

        Assert.That(trainingGroundManager.TryStartTraining(first), Is.True);
        Assert.That(trainingGroundManager.TryStartTraining(second), Is.True);
        Assert.That(trainingGroundManager.TryStartTraining(third), Is.True);
        Assert.That(trainingGroundManager.TryStartTraining(fourth), Is.False);
        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.EqualTo(3));
    }

    [Test]
    public void TryStartTraining_InsufficientGoldHasNoSideEffects()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        Hire(trainee, benchmark);
        merchantData.SetGold(99);

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.False);
        Assert.That(merchantData.Gold, Is.EqualTo(99));
        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
        Assert.That(trainingGroundManager.IsMercenaryTraining(trainee.InstanceId),
            Is.False);
    }

    [Test]
    public void Training_CompletesAfterBenchmarkMercenaryIsReleased()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        Hire(trainee, benchmark);

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.True);
        Assert.That(hireManager.TryReleaseMercenary(benchmark), Is.True);
        dayManager.AdvanceDay();

        Assert.That(trainee.Level, Is.EqualTo(2));
        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
    }

    [Test]
    public void Training_RejectsPartyTransportHealingAndReleaseUntilComplete()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        trainee.TakeDamage(10);
        Hire(trainee, benchmark);
        ItemDataSO cargo = ScriptableObject.CreateInstance<ItemDataSO>();
        cargo.itemName = "Cargo";
        inventory.AddItem(cargo, 1);
        int damagedHP = trainee.CurrentHP;
        int baseMaxHPBeforeTraining = trainee.BaseMaxHP;

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.True);
        Assert.That(partyManager.TryAdd(trainee), Is.False);
        partyManager.RestoreParty(new[] { trainee });
        Assert.That(partyManager.Contains(trainee), Is.False);
        Assert.That(healingManager.CanHeal(trainee), Is.False);
        Assert.That(healingManager.TryHealFull(trainee), Is.False);
        Assert.That(hireManager.TryReleaseMercenary(trainee), Is.False);
        Assert.That(transportManager.TryDepartConvoy(
            2,
            new[] { (cargo, 1) },
            new[] { trainee }),
            Is.EqualTo(TransportDepartureResult.InvalidEscort));

        dayManager.AdvanceDay();

        Assert.That(trainee.CurrentHP, Is.EqualTo(
            damagedHP + trainee.BaseMaxHP - baseMaxHPBeforeTraining));
        Assert.That(partyManager.TryAdd(trainee), Is.True);
        Assert.That(partyManager.Remove(trainee), Is.True);
        Assert.That(healingManager.CanHeal(trainee), Is.True);
        Assert.That(hireManager.TryReleaseMercenary(trainee), Is.True);
        Object.DestroyImmediate(cargo);
    }

    [Test]
    public void Restore_SavesWithoutRepaymentAndCompletesOnlyOnce()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        Hire(trainee, benchmark);
        int completedCount = 0;
        trainingGroundManager.TrainingCompleted += _ => completedCount++;

        Assert.That(trainingGroundManager.TryStartTraining(trainee), Is.True);
        List<SavedTrainingAssignment> saved =
            trainingGroundManager.CreateSaveData();
        int goldAfterPayment = merchantData.Gold;

        trainingGroundManager.Restore(saved);

        Assert.That(merchantData.Gold, Is.EqualTo(goldAfterPayment));
        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.EqualTo(1));
        dayManager.AdvanceDays(2);
        dayManager.AdvanceDay();
        Assert.That(trainee.Level, Is.EqualTo(2));
        Assert.That(completedCount, Is.EqualTo(1));
    }

    [Test]
    public void Restore_DiscardsInvalidTrainingAssignments()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        Hire(trainee, CreateMercenary("Benchmark", 10));
        List<SavedTrainingAssignment> invalid =
            new List<SavedTrainingAssignment>
            {
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = "missing",
                    trainingTownIndex = 2,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 2,
                    paidCost = 100
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = trainee.InstanceId,
                    trainingTownIndex = 2,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 2,
                    paidCost = 100
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = trainee.InstanceId,
                    trainingTownIndex = 2,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 2,
                    paidCost = 100
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = "other",
                    trainingTownIndex = -1,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 2,
                    paidCost = 100
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = trainee.InstanceId,
                    trainingTownIndex = 2,
                    startDay = 1,
                    completionDay = 1,
                    targetLevel = 2,
                    paidCost = 100
                }
            };

        trainingGroundManager.Restore(invalid);

        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
    }

    [Test]
    public void Restore_DiscardsInvalidTownScheduleLevelCapAndCost()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        MercenaryInstance capped = CreateMercenary("Capped", 15);
        Hire(trainee, capped,
            CreateMercenary("Benchmark", 20, MercenaryClass.Knight));
        List<SavedTrainingAssignment> invalid =
            new List<SavedTrainingAssignment>
            {
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = trainee.InstanceId,
                    trainingTownIndex = 2,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 2,
                    paidCost = 100
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = capped.InstanceId,
                    trainingTownIndex = 1,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 16,
                    paidCost = 6400
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = "invalid-day",
                    trainingTownIndex = 1,
                    startDay = 1,
                    completionDay = 3,
                    targetLevel = 2,
                    paidCost = 100
                },
                new SavedTrainingAssignment
                {
                    mercenaryInstanceId = "invalid-cost",
                    trainingTownIndex = 1,
                    startDay = 1,
                    completionDay = 2,
                    targetLevel = 2,
                    paidCost = 99
                }
            };

        trainingGroundManager.Restore(invalid);

        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
    }

    [Test]
    public void Restore_DueReservationCompletesOnceAndIsNotSavedAgain()
    {
        MercenaryInstance trainee = CreateMercenary("Trainee", 1);
        Hire(trainee, CreateMercenary("Benchmark", 10));
        int completedCount = 0;
        trainingGroundManager.TrainingCompleted += _ => completedCount++;
        dayManager.AdvanceDay();

        trainingGroundManager.Restore(new[]
        {
            new SavedTrainingAssignment
            {
                mercenaryInstanceId = trainee.InstanceId,
                trainingTownIndex = 1,
                startDay = 1,
                completionDay = 2,
                targetLevel = 2,
                paidCost = 100
            }
        });

        Assert.That(trainee.Level, Is.EqualTo(2));
        Assert.That(completedCount, Is.EqualTo(1));
        Assert.That(trainingGroundManager.CreateSaveData(), Is.Empty);

        trainingGroundManager.Restore(trainingGroundManager.CreateSaveData());

        Assert.That(completedCount, Is.EqualTo(1));
    }

    [Test]
    public void Training_ThreeReservationsCompleteAndNotifyExactlyOnceEach()
    {
        MercenaryInstance benchmark = CreateMercenary("Benchmark", 10);
        MercenaryInstance first = CreateMercenary("First", 1);
        MercenaryInstance second = CreateMercenary("Second", 1);
        MercenaryInstance third = CreateMercenary("Third", 1);
        Hire(benchmark, first, second, third);
        List<string> completedIds = new List<string>();
        trainingGroundManager.TrainingCompleted += reservation =>
            completedIds.Add(reservation.MercenaryInstanceId);

        Assert.That(trainingGroundManager.TryStartTraining(first), Is.True);
        Assert.That(trainingGroundManager.TryStartTraining(second), Is.True);
        Assert.That(trainingGroundManager.TryStartTraining(third), Is.True);
        dayManager.AdvanceDays(2);

        Assert.That(completedIds, Is.EquivalentTo(new[]
        {
            first.InstanceId,
            second.InstanceId,
            third.InstanceId
        }));
        Assert.That(first.Level, Is.EqualTo(2));
        Assert.That(second.Level, Is.EqualTo(2));
        Assert.That(third.Level, Is.EqualTo(2));
        Assert.That(trainingGroundManager.ActiveTrainingCount, Is.Zero);
    }

    private MercenaryInstance CreateMercenary(string name, int level)
    {
        return CreateMercenary(name, level, MercenaryClass.Warrior);
    }

    private MercenaryInstance CreateMercenary(
        string name,
        int level,
        MercenaryClass mercenaryClass)
    {
        MercenaryDataSO data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryName = name;
        data.mercenaryClass = mercenaryClass;
        data.maxHP = 100;
        data.hireCost = 0;
        createdData.Add(data);
        MercenaryInstance mercenary = new MercenaryInstance(data);
        int levelCap = MercenaryClassProgression.GetLevelCap(mercenaryClass);
        Assert.That(level, Is.LessThanOrEqualTo(levelCap),
            $"{name}: {mercenaryClass} cannot reach level {level} (cap {levelCap}).");
        while (mercenary.Level < level)
        {
            int previousLevel = mercenary.Level;
            mercenary.AddExperience(mercenary.ExperienceToNextLevel);
            Assert.That(mercenary.Level, Is.GreaterThan(previousLevel),
                $"{name}: level stopped rising at {previousLevel}.");
        }

        return mercenary;
    }

    private void Hire(params MercenaryInstance[] mercenaries)
    {
        foreach (MercenaryInstance mercenary in mercenaries)
        {
            mercenary.SetCurrentTownIndex(townProgressState.CurrentTownIndex);
        }

        hireManager.RestoreHiredMercenaries(mercenaries);
    }
}
