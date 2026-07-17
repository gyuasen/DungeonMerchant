using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.TestTools;

public sealed class RuntimeUIPlayModeTests
{
    [UnityTest]
    public IEnumerator Bootstrap_BuildsTitleAndAllPrimaryPages()
    {
        SimpleMercenaryHireUI ui = null;
        float timeoutAt = Time.realtimeSinceStartup + 5f;
        while (ui == null && Time.realtimeSinceStartup < timeoutAt)
        {
            ui = Object.FindObjectOfType<SimpleMercenaryHireUI>();
            yield return null;
        }

        Assert.That(ui, Is.Not.Null, "Runtime UI was not bootstrapped.");

        // The title screen moved to its own scene (Title.unity +
        // TitleSceneController); the game-scene UI no longer owns a title
        // overlay, so only the primary pages are asserted here.

        foreach (string fieldName in new[]
                 {
                     "globalMapPage", "worldMapPage", "townMapPage",
                     "hirePage", "companyPage", "partyPage", "healPage",
                     "battlePage", "roadBattlePage", "dungeonPage",
                     "marketPage", "blacksmithPage", "inventoryPage",
                     "jobChangePage"
                 })
        {
            Assert.That(
                GetPrivateField<RectTransform>(ui, fieldName),
                Is.Not.Null,
                $"Primary page was not built: {fieldName}");
        }

        RectTransform battlePage =
            GetPrivateField<RectTransform>(ui, "battlePage");
        RectTransform dungeonEventPanel =
            GetPrivateField<RectTransform>(ui, "dungeonEventPanel");
        Assert.That(dungeonEventPanel, Is.Not.Null);
        Assert.That(dungeonEventPanel.parent, Is.EqualTo(battlePage));
        Assert.That(dungeonEventPanel.gameObject.activeSelf, Is.False);

        Assert.That(
            GetPrivateField<Button>(ui, "battlePauseButton"),
            Is.Not.Null);
        Assert.That(
            GetPrivateField<Button>(ui, "roadPauseButton"),
            Is.Not.Null);
        Assert.That(
            GetPrivateField<Text>(ui, "dungeonEventPreviewText"),
            Is.Not.Null);
        Button firstEventChoice =
            GetPrivateField<Button>(ui, "firstDungeonEventButton");
        Assert.That(firstEventChoice.targetGraphic, Is.TypeOf<Image>());
        Assert.That(
            firstEventChoice.GetComponent<EventTrigger>(),
            Is.Not.Null);
        Text firstEventLabel = firstEventChoice.GetComponentInChildren<Text>();
        Assert.That(firstEventLabel.resizeTextForBestFit, Is.True);
        Assert.That(
            firstEventLabel.horizontalOverflow,
            Is.EqualTo(HorizontalWrapMode.Wrap));
        Assert.That(
            firstEventLabel.verticalOverflow,
            Is.EqualTo(VerticalWrapMode.Overflow));

        Assert.That(Object.FindObjectOfType<SaveManager>(), Is.Not.Null);
        Assert.That(Object.FindObjectOfType<StoryProgressManager>(), Is.Not.Null);
        Assert.That(Object.FindObjectOfType<AudioFeedbackService>(), Is.Not.Null);
    }

    [UnityTest]
    public IEnumerator Bootstrap_HasOneActivePrimaryPageBehindTitle()
    {
        yield return null;
        SimpleMercenaryHireUI ui =
            Object.FindObjectOfType<SimpleMercenaryHireUI>();
        Assert.That(ui, Is.Not.Null);

        int activePages = 0;
        foreach (string fieldName in new[]
                 {
                     "globalMapPage", "worldMapPage", "townMapPage",
                     "hirePage", "companyPage", "partyPage", "healPage",
                     "battlePage", "roadBattlePage", "dungeonPage",
                     "marketPage", "blacksmithPage", "inventoryPage",
                     "jobChangePage"
                 })
        {
            RectTransform page =
                GetPrivateField<RectTransform>(ui, fieldName);
            if (page != null && page.gameObject.activeSelf)
            {
                activePages++;
            }
        }

        Assert.That(activePages, Is.EqualTo(1));
    }

    private static T GetPrivateField<T>(object target, string fieldName)
        where T : class
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field: {fieldName}");
        return field.GetValue(target) as T;
    }
}
