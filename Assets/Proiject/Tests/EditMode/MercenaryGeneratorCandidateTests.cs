using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class MercenaryGeneratorCandidateTests
{
    private readonly List<Object> createdObjects = new List<Object>();
    private GameObject root;
    private MerchantData merchantData;
    private DayManager dayManager;
    private MercenaryHireManager hireManager;
    private MercenaryGenerator generator;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Recruitment Candidate Test Root");
        root.SetActive(false);
        merchantData = root.AddComponent<MerchantData>();
        dayManager = root.AddComponent<DayManager>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        generator = root.AddComponent<MercenaryGenerator>();
        root.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        foreach (Object createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                Object.DestroyImmediate(createdObject);
            }
        }
    }

    [Test]
    public void SameTownAndCandidateBlock_GeneratesTheSameCandidates()
    {
        generator.SetTownIndex(2);
        generator.GenerateCandidates();
        string firstSnapshot = CreateSnapshot();

        Assert.That(
            generator.Candidates.Count + generator.UniqueCandidates.Count,
            Is.EqualTo(MercenaryGenerator.CandidateSlotCount));

        generator.GenerateCandidates();

        Assert.That(CreateSnapshot(), Is.EqualTo(firstSnapshot));
    }

    [Test]
    public void Candidates_RefreshOnlyAtThreeDayBoundary()
    {
        generator.SetTownIndex(2);
        string firstSnapshot = CreateSnapshot();

        dayManager.AdvanceDay();
        Assert.That(CreateSnapshot(), Is.EqualTo(firstSnapshot));

        dayManager.AdvanceDay();
        Assert.That(CreateSnapshot(), Is.Not.EqualTo(firstSnapshot));
    }

    [Test]
    public void UniqueCandidates_AreLimitedToTwoSlots()
    {
        generator.SetUniqueCandidatePool(CreateUniqueCandidates(12));

        for (int block = 0; block < 20; block++)
        {
            generator.GenerateCandidates();
            Assert.That(
                generator.UniqueCandidates.Count,
                Is.LessThanOrEqualTo(MercenaryGenerator.MaximumUniqueCandidateCount));
            Assert.That(
                generator.Candidates.Count + generator.UniqueCandidates.Count,
                Is.EqualTo(MercenaryGenerator.CandidateSlotCount));
            AdvanceToNextCandidateBlock();
        }
    }

    [Test]
    public void HiredUniqueCandidate_DoesNotReturnInLaterCandidateBlocks()
    {
        MercenaryDataSO uniqueCandidate = CreateUniqueCandidates(1)[0];
        generator.SetUniqueCandidatePool(new[] { uniqueCandidate });
        merchantData.SetGold(1000);

        for (int block = 0; block < 20 && generator.UniqueCandidates.Count == 0; block++)
        {
            AdvanceToNextCandidateBlock();
        }

        Assert.That(generator.UniqueCandidates, Contains.Item(uniqueCandidate));
        Assert.That(hireManager.TryHireMercenary(uniqueCandidate), Is.True);

        for (int block = 0; block < 20; block++)
        {
            AdvanceToNextCandidateBlock();
            Assert.That(
                generator.UniqueCandidates.Any(candidate => candidate == uniqueCandidate),
                Is.False);
        }
    }

    private string CreateSnapshot()
    {
        return string.Join(
            "|",
            generator.UniqueCandidates.Select(candidate => candidate.PersistentId)
                .Concat(generator.Candidates.Select(candidate =>
                    $"{candidate.MercenaryName}:{candidate.MercenaryClass}:{candidate.Level}:{candidate.HireCost}")));
    }

    private List<MercenaryDataSO> CreateUniqueCandidates(int count)
    {
        List<MercenaryDataSO> result = new List<MercenaryDataSO>();
        for (int index = 0; index < count; index++)
        {
            MercenaryDataSO candidate = Track(
                ScriptableObject.CreateInstance<MercenaryDataSO>());
            candidate.name = $"Unique {index}";
            candidate.mercenaryName = $"Unique {index}";
            candidate.hireCost = 10;
            result.Add(candidate);
        }
        return result;
    }

    private void AdvanceToNextCandidateBlock()
    {
        for (int day = 0; day < MercenaryGenerator.CandidateRefreshIntervalDays; day++)
        {
            dayManager.AdvanceDay();
        }
    }

    private T Track<T>(T createdObject) where T : Object
    {
        createdObjects.Add(createdObject);
        return createdObject;
    }
}
