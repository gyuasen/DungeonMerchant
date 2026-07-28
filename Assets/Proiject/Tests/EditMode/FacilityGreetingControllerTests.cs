using NUnit.Framework;

public sealed class FacilityGreetingControllerTests
{
    [Test]
    public void Greeting_IsDeterministicForSameDayTownAndFacility()
    {
        FacilityGreetingController controller = new FacilityGreetingController();

        FacilityGreeting first = controller.GetGreeting(4, 1, "セイル", FacilityGreetingController.BlacksmithKey);
        FacilityGreeting second = controller.GetGreeting(4, 1, "セイル", FacilityGreetingController.BlacksmithKey);

        Assert.That(second.Title, Is.EqualTo(first.Title));
        Assert.That(second.Dialogue, Is.EqualTo(first.Dialogue));
    }

    [Test]
    public void Greeting_ChangesOnFollowingDay()
    {
        FacilityGreetingController controller = new FacilityGreetingController();

        FacilityGreeting today = controller.GetGreeting(4, 1, "セイル", FacilityGreetingController.MarketKey);
        FacilityGreeting tomorrow = controller.GetGreeting(5, 1, "セイル", FacilityGreetingController.MarketKey);

        Assert.That(tomorrow.Dialogue, Is.Not.EqualTo(today.Dialogue));
    }

    [Test]
    public void EveryFacility_HasTitleAndDialogue()
    {
        FacilityGreetingController controller = new FacilityGreetingController();

        foreach (string facilityKey in FacilityGreetingController.FacilityKeys)
        {
            FacilityGreeting greeting = controller.GetGreeting(1, 0, "セイル", facilityKey);
            Assert.That(greeting.Title, Is.Not.Empty);
            Assert.That(greeting.Dialogue, Is.Not.Empty);
        }
    }

    [Test]
    public void TrainingGround_HasFacilityGreeting()
    {
        FacilityGreetingController controller = new FacilityGreetingController();

        FacilityGreeting greeting = controller.GetGreeting(
            1,
            1,
            "リーフ",
            FacilityGreetingController.TrainingGroundKey);

        Assert.That(greeting.Title, Is.Not.Empty);
        Assert.That(greeting.Dialogue, Is.Not.Empty);
    }

    [Test]
    public void VisitSkipState_ShowsEachFacilityOncePerSession()
    {
        FacilityGreetingController controller = new FacilityGreetingController();

        Assert.That(controller.ShouldShowGreeting(2, 0, FacilityGreetingController.TavernKey), Is.True);
        controller.MarkEntered(2, 0, FacilityGreetingController.TavernKey);

        Assert.That(controller.ShouldShowGreeting(2, 0, FacilityGreetingController.TavernKey), Is.False);
        Assert.That(controller.ShouldShowGreeting(3, 0, FacilityGreetingController.TavernKey), Is.False);
        Assert.That(controller.ShouldShowGreeting(2, 1, FacilityGreetingController.TavernKey), Is.False);
        Assert.That(controller.ShouldShowGreeting(2, 0, FacilityGreetingController.MarketKey), Is.True);
    }
}
