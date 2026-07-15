using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class BattleVisualControllerTests
{
    private GameObject controllerObject;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(controllerObject);
    }

    [Test]
    public void FinishPresentationImmediately_WhenBusy_CompletesAndNotifiesOnce()
    {
        controllerObject = new GameObject(
            "Battle Visual Controller",
            typeof(RectTransform));
        BattleVisualController controller =
            controllerObject.AddComponent<BattleVisualController>();
        SetPresentationBusy(controller, true);

        int completionCount = 0;
        controller.PresentationCompleted += () => completionCount++;

        controller.FinishPresentationImmediately();
        controller.FinishPresentationImmediately();

        Assert.That(controller.IsPresentationBusy, Is.False);
        Assert.That(completionCount, Is.EqualTo(1));
    }

    private static void SetPresentationBusy(
        BattleVisualController controller,
        bool value)
    {
        PropertyInfo property = typeof(BattleVisualController).GetProperty(
            nameof(BattleVisualController.IsPresentationBusy),
            BindingFlags.Instance | BindingFlags.Public);

        Assert.That(property, Is.Not.Null);
        property.SetValue(controller, value);
    }
}
