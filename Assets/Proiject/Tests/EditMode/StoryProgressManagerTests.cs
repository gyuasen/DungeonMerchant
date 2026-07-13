using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class StoryProgressManagerTests
{
    private GameObject root;
    private MerchantData merchantData;
    private MercenaryHireManager hireManager;
    private TownProgressState townProgressState;
    private StoryProgressManager storyProgressManager;
    private readonly List<Object> createdObjects = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Story Progress Test Root");
        root.SetActive(false);
        merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<DayManager>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        townProgressState = root.AddComponent<TownProgressState>();
        root.AddComponent<DebtManager>();
        storyProgressManager = root.AddComponent<StoryProgressManager>();
        root.SetActive(true);

        InvokeOnEnable(hireManager);
        InvokeOnEnable(storyProgressManager);
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
    public void TryComplete_NotifiesOnlyOnce_AndProvidesJapaneseStoryData()
    {
        int notifications = 0;
        storyProgressManager.MilestoneCompleted += _ => notifications++;

        Assert.That(storyProgressManager.TryComplete(StoryMilestone.OpeningDebtNotice),
            Is.True);
        Assert.That(storyProgressManager.TryComplete(StoryMilestone.OpeningDebtNotice),
            Is.False);
        Assert.That(notifications, Is.EqualTo(1));

        foreach (StoryMilestone milestone in
                 (StoryMilestone[])System.Enum.GetValues(typeof(StoryMilestone)))
        {
            StoryMilestoneInfo info = storyProgressManager.GetMilestoneInfo(milestone);
            Assert.That(info.Title, Is.Not.Empty);
            Assert.That(info.Body, Is.Not.Empty);
        }
    }

    [Test]
    public void ExistingEvents_CompleteHireAndTownMilestones()
    {
        merchantData.SetGold(100);
        MercenaryDataSO data = Track(ScriptableObject.CreateInstance<MercenaryDataSO>());
        data.mercenaryName = "Story Hire";
        data.mercenaryClass = MercenaryClass.Warrior;
        data.hireCost = 10;

        Assert.That(hireManager.TryHireMercenary(data), Is.True);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.FirstMercenary),
            Is.True);

        townProgressState.UnlockTown(1);

        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.LeafUnlocked),
            Is.True);
    }

    [Test]
    public void RestoreCompletedMilestones_DoesNotNotifyDuringLoad()
    {
        int notifications = 0;
        storyProgressManager.MilestoneCompleted += _ => notifications++;

        storyProgressManager.BeginRestore();
        townProgressState.UnlockTown(1);
        storyProgressManager.RestoreCompletedMilestones(
            new[] { StoryMilestone.OpeningDebtNotice });

        Assert.That(notifications, Is.Zero);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.OpeningDebtNotice),
            Is.True);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.LeafUnlocked),
            Is.False);
    }

    [Test]
    public void PendingPresentation_IsQueuedOnceAndCanBeConsumed()
    {
        storyProgressManager.TryComplete(StoryMilestone.OpeningDebtNotice);

        Assert.That(storyProgressManager.TryDequeuePendingPresentation(
            out StoryMilestone milestone), Is.True);
        Assert.That(milestone, Is.EqualTo(StoryMilestone.OpeningDebtNotice));
        Assert.That(storyProgressManager.TryDequeuePendingPresentation(out _),
            Is.False);
    }

    private T Track<T>(T created) where T : Object
    {
        createdObjects.Add(created);
        return created;
    }

    private static void InvokeOnEnable(MonoBehaviour component)
    {
        component.GetType()
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(component, null);
    }
}
