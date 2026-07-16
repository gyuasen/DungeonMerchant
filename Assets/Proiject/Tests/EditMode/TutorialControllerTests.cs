using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class TutorialControllerTests
{
    private const string CompletionKey =
        "DungeonMerchant.Tutorial.Completed";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(CompletionKey);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(CompletionKey);
    }

    [Test]
    public void FirstJourneyRoute_StatesRequiredActionsInOrder()
    {
        string route = TutorialController.FirstJourneyRoute;

        int townMap = route.IndexOf("町マップ");
        int hire = route.IndexOf("傭兵斡旋所");
        int party = route.IndexOf("傭兵一覧／メニューで編成");
        int dungeon = route.IndexOf("近隣ダンジョン");

        Assert.That(townMap, Is.GreaterThanOrEqualTo(0));
        Assert.That(hire, Is.GreaterThan(townMap));
        Assert.That(party, Is.GreaterThan(hire));
        Assert.That(dungeon, Is.GreaterThan(party));
    }

    [Test]
    public void Navigation_PresentsOnboardingAndFacilityPurposes()
    {
        List<string> titles = new List<string>();
        List<string> bodies = new List<string>();
        string nextLabel = null;
        TutorialController controller = CreateController(
            title => titles.Add(title),
            body => bodies.Add(body),
            label => nextLabel = label);

        controller.ShowTutorial();
        while (nextLabel != "完了")
        {
            controller.ShowNextStep();
        }

        CollectionAssert.IsSubsetOf(
            new[]
            {
                "1. 町マップを開く",
                "2. 傭兵斡旋所で雇う",
                "3. 傭兵一覧／メニューで編成",
                "4. 近隣ダンジョンへ"
            },
            titles);
        string allBodies = string.Join("\n", bodies);
        foreach (string facility in new[]
                 {
                     "傭兵斡旋所", "商会組合", "パーティー編成", "近隣ダンジョン",
                     "市場", "鍛冶屋", "倉庫", "治療院", "転職神殿", "全体マップ"
                 })
        {
            StringAssert.Contains(facility, allBodies);
        }
    }

    [Test]
    public void CompletingTutorial_SetsPreferenceAndClosesOverlay()
    {
        bool hidden = false;
        string status = null;
        string nextLabel = null;
        TutorialController controller = CreateController(
            _ => { },
            _ => { },
            label => nextLabel = label,
            () => hidden = true,
            message => status = message);

        controller.ShowTutorial();
        while (nextLabel != "完了")
        {
            controller.ShowNextStep();
        }
        controller.ShowNextStep();

        Assert.That(PlayerPrefs.GetInt(CompletionKey), Is.EqualTo(1));
        Assert.That(hidden, Is.True);
        StringAssert.Contains("完了", status);
    }

    [Test]
    public void ResetCompletion_AllowsTutorialToRunForNewGame()
    {
        PlayerPrefs.SetInt(CompletionKey, 1);

        TutorialController.ResetCompletion();

        Assert.That(PlayerPrefs.HasKey(CompletionKey), Is.False);
    }

    private static TutorialController CreateController(
        System.Action<string> setTitle,
        System.Action<string> setBody,
        System.Action<string> setNextLabel,
        System.Action hideOverlay = null,
        System.Action<string> setStatus = null)
    {
        return new TutorialController(
            setStatus ?? (_ => { }),
            () => { },
            hideOverlay ?? (() => { }),
            _ => { },
            setTitle,
            setBody,
            _ => { },
            setNextLabel,
            () => true);
    }
}
