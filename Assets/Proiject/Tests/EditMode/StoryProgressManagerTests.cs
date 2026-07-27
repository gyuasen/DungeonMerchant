using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class StoryProgressManagerTests
{
    private GameObject root;
    private DebtManager debtManager;
    private StoryProgressManager storyProgressManager;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Story Progress Test Root");
        root.SetActive(false);
        root.AddComponent<MerchantData>();
        root.AddComponent<DayManager>();
        root.AddComponent<MercenaryHireManager>();
        root.AddComponent<TownProgressState>();
        debtManager = root.AddComponent<DebtManager>();
        storyProgressManager = root.AddComponent<StoryProgressManager>();
        root.SetActive(true);

        // 明示的に依存を注入し、ライフサイクル順序に依存せず購読を成立させる。
        storyProgressManager.Initialize(debtManager);
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(root);

    [Test]
    public void DebtThresholds_CompleteAndQueueTheirMatchingMilestones()
    {
        StoryMilestone[] milestones =
        {
            StoryMilestone.DebtRepaid10, StoryMilestone.DebtRepaid25,
            StoryMilestone.DebtRepaid50, StoryMilestone.DebtRepaid75,
            StoryMilestone.DebtRepaid90, StoryMilestone.DebtCleared
        };
        int[] percentages = { 10, 25, 50, 75, 90, 100 };

        for (int index = 0; index < milestones.Length; index++)
        {
            debtManager.Restore(RemainingDebtForPercentage(percentages[index]), 0, 0);

            Assert.That(storyProgressManager.IsCompleted(milestones[index]), Is.True);
            Assert.That(storyProgressManager.TryDequeuePresentation(out StoryPresentation presentation), Is.True);
            Assert.That(presentation.Milestone, Is.EqualTo(milestones[index]));
            Assert.That(
                presentation.IsEnding,
                Is.EqualTo(milestones[index] == StoryMilestone.DebtCleared));
        }
    }

    [Test]
    public void DebtThresholds_SkippedThresholdsCompleteInOrderOnce()
    {
        var notifications = new List<StoryMilestone>();
        storyProgressManager.MilestoneCompleted += notifications.Add;

        debtManager.Restore(RemainingDebtForPercentage(50), 0, 0);

        CollectionAssert.AreEqual(new[]
        {
            StoryMilestone.DebtRepaid10, StoryMilestone.DebtRepaid25,
            StoryMilestone.DebtRepaid50
        }, notifications);
        debtManager.Restore(RemainingDebtForPercentage(50), 0, 0);
        Assert.That(notifications, Has.Count.EqualTo(3));
    }

    [Test]
    public void HireDungeonAndTownEvents_DoNotCompleteOrQueueStory()
    {
        InvokePrivate(storyProgressManager, "HandleDebtChanged");
        Assert.That(storyProgressManager.TryDequeuePresentation(out _), Is.False);

        root.GetComponent<TownProgressState>().UnlockTown(1);
        Assert.That(storyProgressManager.CompletedMilestones, Is.Empty);
        Assert.That(storyProgressManager.TryDequeuePresentation(out _), Is.False);
    }

    [Test]
    public void Restore_AddsReachedDebtMilestonesWithoutNotificationsOrPresentation()
    {
        int notifications = 0;
        int presentationNotifications = 0;
        storyProgressManager.MilestoneCompleted += _ => notifications++;
        storyProgressManager.PresentationQueued += () => presentationNotifications++;

        storyProgressManager.BeginRestore();
        debtManager.Restore(RemainingDebtForPercentage(75), 0, 0);
        storyProgressManager.RestoreCompletedMilestones(new[] { StoryMilestone.OpeningDebtNotice });

        Assert.That(notifications, Is.Zero);
        Assert.That(presentationNotifications, Is.Zero);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.DebtRepaid10), Is.True);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.DebtRepaid25), Is.True);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.DebtRepaid50), Is.True);
        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.DebtRepaid75), Is.True);
        Assert.That(storyProgressManager.TryDequeuePresentation(out _), Is.False);
    }

    [Test]
    public void DebtCleared_QueuesOneEndingTransitionTriggerOnly()
    {
        int presentationNotifications = 0;
        storyProgressManager.PresentationQueued += () => presentationNotifications++;

        debtManager.Restore(0, 0, 0);

        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.DebtCleared), Is.True);
        Assert.That(presentationNotifications, Is.EqualTo(6));
        for (int index = 0; index < 5; index++)
        {
            Assert.That(storyProgressManager.TryDequeuePresentation(out StoryPresentation presentation), Is.True);
            Assert.That(presentation.IsEnding, Is.False);
        }
        Assert.That(storyProgressManager.TryDequeuePresentation(out StoryPresentation ending), Is.True);
        Assert.That(ending.Milestone, Is.EqualTo(StoryMilestone.DebtCleared));
        Assert.That(ending.IsEnding, Is.True);
        Assert.That(storyProgressManager.TryDequeuePresentation(out _), Is.False);

        debtManager.Restore(0, 0, 0);
        Assert.That(presentationNotifications, Is.EqualTo(6));
    }

    [Test]
    public void Restore_DebtClearedDoesNotQueueOrReplayEndingTransition()
    {
        int presentationNotifications = 0;
        storyProgressManager.PresentationQueued += () => presentationNotifications++;

        storyProgressManager.BeginRestore();
        debtManager.Restore(0, 0, 0);
        storyProgressManager.RestoreCompletedMilestones(
            new[] { StoryMilestone.DebtCleared });

        Assert.That(storyProgressManager.IsCompleted(StoryMilestone.DebtCleared), Is.True);
        Assert.That(presentationNotifications, Is.Zero);
        Assert.That(storyProgressManager.TryDequeuePresentation(out _), Is.False);
    }

    [Test]
    public void OnboardingPresentation_IsMarkedSeparatelyFromStory()
    {
        storyProgressManager.EnqueueOnboardingPresentation(
            OnboardingGuideCard.Warehouse,
            null);

        Assert.That(storyProgressManager.TryDequeuePresentation(out StoryPresentation presentation), Is.True);
        Assert.That(presentation.Milestone, Is.Null);
        Assert.That(presentation.IsOnboarding, Is.True);
        Assert.That(presentation.IsEnding, Is.False);
    }

    [Test]
    public void TryComplete_NotifiesOnlyOnce_AndProvidesStoryData()
    {
        int notifications = 0;
        storyProgressManager.MilestoneCompleted += _ => notifications++;

        Assert.That(storyProgressManager.TryComplete(StoryMilestone.OpeningDebtNotice), Is.True);
        Assert.That(storyProgressManager.TryComplete(StoryMilestone.OpeningDebtNotice), Is.False);
        Assert.That(notifications, Is.EqualTo(1));

        foreach (StoryMilestone milestone in (StoryMilestone[])System.Enum.GetValues(typeof(StoryMilestone)))
        {
            StoryMilestoneInfo info = storyProgressManager.GetMilestoneInfo(milestone);
            Assert.That(info.Title, Is.Not.Empty);
            Assert.That(info.Body, Is.Not.Empty);
        }
    }

    [Test]
    public void MilestoneInfo_UsesConfirmedRepaymentNarrative()
    {
        StoryMilestoneInfo opening = storyProgressManager.GetMilestoneInfo(
            StoryMilestone.OpeningDebtNotice);
        StoryMilestoneInfo halfway = storyProgressManager.GetMilestoneInfo(
            StoryMilestone.DebtRepaid50);
        StoryMilestoneInfo ending = storyProgressManager.GetMilestoneInfo(
            StoryMilestone.DebtCleared);

        Assert.That(opening.Title, Is.EqualTo("第一章　背負った五千万"));
        StringAssert.Contains("五千万Gの負債", opening.Body);
        Assert.That(halfway.Title, Is.EqualTo("軌道に乗った商会"));
        StringAssert.Contains("あと半分", halfway.Body);
        Assert.That(ending.Title, Is.EqualTo("エピローグ　商会、再興"));
        StringAssert.Contains("親愛なる我が子へ", ending.Body);
    }

    private static int RemainingDebtForPercentage(int percentage) =>
        // InitialDebt(5000万)×percentage は int を溢れるため long で計算する。
        (int)(DebtManager.InitialDebt - ((long)DebtManager.InitialDebt * percentage / 100));

    private static void InvokePrivate(object target, string methodName) =>
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
}
