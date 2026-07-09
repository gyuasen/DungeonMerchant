using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Characterization tests for ProgressionManager (Action 2.3 of the
// god-object refactor plan). CreateSaveData()/Restore(ProgressionSaveData)
// are the fully synchronous, side-effect-bounded surface covered here.
//
// OnEnable() (fired the instant AddComponent<ProgressionManager>() runs)
// calls PopulateSpecialQuests() and GenerateNormalQuestsIfNeeded() -- both
// read GameAssetRepository.LoadAll<T>() (Resources.LoadAll<T>(string.Empty)),
// which is a plain Unity API call that never throws even when nothing is
// found, and GenerateNormalQuestsIfNeeded() simply appends
// UnityEngine.Random-based filler QuestRecords to `quests` until at least 3
// non-special/non-completed/non-expired entries exist. Restore(...) also
// calls GenerateNormalQuestsIfNeeded() internally. Neither method is asserted
// on directly here -- both were confirmed (by reading the source) to be
// no-throw with no other managers wired, and they do not touch
// storageTier/totalDungeonClears/profitableDungeonClears/totalGoldEarned,
// which is all these tests check.
//
// IMPORTANT precedence quirk: the public TotalGoldEarned getter returns
// merchantData.LifetimeGoldEarned when a MerchantData is resolved (via
// ResolveReferences(), called from OnEnable()), and only falls back to the
// private totalGoldEarned field (the one Restore(...) writes) when
// merchantData is null. Most tests below therefore avoid adding a
// MerchantData component anywhere in the object graph so TotalGoldEarned
// reliably reflects the restored field; TotalGoldEarned_WithMerchantDataPresent_...
// exists specifically to pin down the opposite case.
public sealed class ProgressionManagerTests
{
    private GameObject root;
    private ProgressionManager progressionManager;
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Progression Manager Test");
        progressionManager = root.AddComponent<ProgressionManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);

        foreach (Object created in createdObjects)
        {
            if (created != null)
            {
                Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    [Test]
    public void Restore_ThenCreateSaveData_RoundTripsAllFields()
    {
        ProgressionSaveData data = new ProgressionSaveData
        {
            storageTier = 2,
            totalDungeonClears = 7,
            profitableDungeonClears = 3,
            totalGoldEarned = 4200,
            quests = new List<QuestRecord>()
        };

        progressionManager.Restore(data);
        ProgressionSaveData saved = progressionManager.CreateSaveData();

        Assert.That(saved.storageTier, Is.EqualTo(2));
        Assert.That(saved.totalDungeonClears, Is.EqualTo(7));
        Assert.That(saved.profitableDungeonClears, Is.EqualTo(3));
        Assert.That(saved.totalGoldEarned, Is.EqualTo(4200));
    }

    [Test]
    public void Restore_ThenPublicGetters_ReflectRestoredValues_WhenNoMerchantDataPresent()
    {
        // No MerchantData exists anywhere in this test's object graph, so
        // ResolveReferences() leaves the private merchantData field null and
        // TotalGoldEarned falls back to the restored totalGoldEarned field.
        ProgressionSaveData data = new ProgressionSaveData
        {
            storageTier = 1,
            totalDungeonClears = 5,
            profitableDungeonClears = 2,
            totalGoldEarned = 999,
            quests = new List<QuestRecord>()
        };

        progressionManager.Restore(data);

        Assert.That(progressionManager.StorageTier, Is.EqualTo(1));
        Assert.That(progressionManager.TotalDungeonClears, Is.EqualTo(5));
        Assert.That(progressionManager.ProfitableDungeonClears, Is.EqualTo(2));
        Assert.That(progressionManager.TotalGoldEarned, Is.EqualTo(999));
    }

    [Test]
    public void TotalGoldEarned_WithMerchantDataPresent_PrefersMerchantDataOverRestoredField()
    {
        // Pins down the double-bookkeeping precedence: when a MerchantData is
        // resolvable, TotalGoldEarned reads merchantData.LifetimeGoldEarned
        // and ignores whatever was just written into the private
        // totalGoldEarned field via Restore(...).
        GameObject withMerchantDataRoot =
            Track(new GameObject("Progression Manager With MerchantData Test"));
        MerchantData merchantData = withMerchantDataRoot.AddComponent<MerchantData>();
        merchantData.AddGold(1234); // LifetimeGoldEarned becomes 1234.

        // Added after MerchantData so ResolveReferences() (run from this new
        // ProgressionManager's OnEnable) finds it via GetComponent on the
        // same GameObject.
        ProgressionManager withMerchantData =
            withMerchantDataRoot.AddComponent<ProgressionManager>();

        // TotalGoldEarned's getter reads the private merchantData field
        // directly without calling ResolveReferences() itself, so it only
        // sees a resolved reference if OnEnable() already ran. Call a public
        // method that triggers ResolveReferences() (CanStore has no side
        // effects relevant here) to make resolution deterministic regardless
        // of OnEnable timing in the EditMode test host.
        withMerchantData.CanStore();

        withMerchantData.Restore(new ProgressionSaveData
        {
            storageTier = 0,
            totalDungeonClears = 0,
            profitableDungeonClears = 0,
            totalGoldEarned = 9999, // Deliberately different from 1234.
            quests = new List<QuestRecord>()
        });

        Assert.That(withMerchantData.TotalGoldEarned, Is.EqualTo(1234));
        Assert.That(withMerchantData.TotalGoldEarned, Is.Not.EqualTo(9999));
    }

    [Test]
    public void Restore_Null_DoesNotThrowAndDoesNotChangeState()
    {
        progressionManager.Restore(new ProgressionSaveData
        {
            storageTier = 2,
            totalDungeonClears = 9,
            profitableDungeonClears = 4,
            totalGoldEarned = 555,
            quests = new List<QuestRecord>()
        });

        Assert.DoesNotThrow(() => progressionManager.Restore(null));

        Assert.That(progressionManager.StorageTier, Is.EqualTo(2));
        Assert.That(progressionManager.TotalDungeonClears, Is.EqualTo(9));
        Assert.That(progressionManager.ProfitableDungeonClears, Is.EqualTo(4));
        Assert.That(progressionManager.TotalGoldEarned, Is.EqualTo(555));
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }
}
