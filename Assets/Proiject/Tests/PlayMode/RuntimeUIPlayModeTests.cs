using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
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

        RectTransform titleOverlay =
            GetPrivateField<RectTransform>(ui, "titleOverlay");
        Assert.That(titleOverlay, Is.Not.Null);
        Assert.That(titleOverlay.gameObject.activeSelf, Is.True);

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
