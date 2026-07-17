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
        int hire = route.IndexOf("酒場");
        int party = route.IndexOf("パーティー編成");
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
                "商会を再建する理由",
                "最初の探索隊を編成する",
                "結界都市の施設",
                "探索と戦闘",
                "装備と成長",
                "日数と経営"
            },
            titles);
        string allBodies = string.Join("\n", bodies);
        foreach (string facility in new[]
                 {
                     "酒場", "市場", "鍛冶屋", "倉庫", "治療院", "商会組合",
                     "輸送部隊", "遠征部隊", "パーティー編成", "近隣ダンジョン",
                     "転職神殿"
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
