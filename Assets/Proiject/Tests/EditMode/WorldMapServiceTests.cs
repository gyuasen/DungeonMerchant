using NUnit.Framework;

public sealed class WorldMapServiceTests
{
    [Test]
    public void TownData_HasExpectedProgressionAndRegions()
    {
        Assert.That(WorldMapService.TownCount, Is.EqualTo(7));
        Assert.That(WorldMapService.WorldRegionCount, Is.EqualTo(3));
        Assert.That(WorldMapService.GetTownName(2), Is.EqualTo("セイル港湾都市"));
        Assert.That(WorldMapService.GetTownName(6), Is.EqualTo("アビス辺境都市"));
        Assert.That(
            WorldMapService.GetWorldRegionName(1),
            Is.EqualTo("北西山岳森林地域"));
    }

    [TestCase(2, 1, true)]
    [TestCase(1, 0, true)]
    [TestCase(0, 3, true)]
    [TestCase(3, 4, true)]
    [TestCase(4, 5, true)]
    [TestCase(5, 6, true)]
    [TestCase(2, 0, false)]
    [TestCase(0, 4, false)]
    public void AreTownsAdjacent_UsesProgressionRoute(
        int leftTown,
        int rightTown,
        bool expected)
    {
        Assert.That(
            WorldMapService.AreTownsAdjacent(leftTown, rightTown),
            Is.EqualTo(expected));
    }

    [TestCase(2, 0, 1)]
    [TestCase(0, 5, 3)]
    [TestCase(6, 3, 5)]
    [TestCase(3, 3, -1)]
    public void GetNextTownToward_ReturnsNextRouteStep(
        int originTown,
        int destinationTown,
        int expectedNextTown)
    {
        Assert.That(
            WorldMapService.GetNextTownToward(originTown, destinationTown),
            Is.EqualTo(expectedNextTown));
    }

    [Test]
    public void CreateRestoredUnlockedTownIndices_WhenLegacySave_UnlocksRouteToCurrentTown()
    {
        var restored =
            WorldMapService.CreateRestoredUnlockedTownIndices(4, null);

        Assert.That(restored, Does.Contain(2));
        Assert.That(restored, Does.Contain(1));
        Assert.That(restored, Does.Contain(0));
        Assert.That(restored, Does.Contain(3));
        Assert.That(restored, Does.Contain(4));
        Assert.That(restored, Has.No.Member(5));
    }

    [Test]
    public void CreateRestoredUnlockedTownIndices_WhenModernSave_IgnoresInvalidTowns()
    {
        var restored =
            WorldMapService.CreateRestoredUnlockedTownIndices(
                6,
                new[] { 2, 5, 99, -1 });

        Assert.That(restored, Does.Contain(2));
        Assert.That(restored, Does.Contain(5));
        Assert.That(restored, Does.Contain(6));
        Assert.That(restored, Has.No.Member(99));
        Assert.That(restored, Has.No.Member(-1));
    }

    [TestCase(1, 0)]
    [TestCase(2, 4)]
    [TestCase(0, -1)]
    public void GetGateTownIndexForWorldRegion_ReturnsConfiguredGate(
        int worldMapIndex,
        int expectedGateTown)
    {
        Assert.That(
            WorldMapService.GetGateTownIndexForWorldRegion(worldMapIndex),
            Is.EqualTo(expectedGateTown));
    }

    [Test]
    public void CanEnterWorldRegion_UsesCurrentTownUnlockedTownOrGateClear()
    {
        Assert.That(
            WorldMapService.CanEnterWorldRegion(
                1,
                3,
                new[] { 2 },
                _ => false),
            Is.True);
        Assert.That(
            WorldMapService.CanEnterWorldRegion(
                2,
                3,
                new[] { 2, 5 },
                _ => false),
            Is.True);
        Assert.That(
            WorldMapService.CanEnterWorldRegion(
                2,
                3,
                new[] { 2, 3 },
                gateTown => gateTown == 4),
            Is.True);
        Assert.That(
            WorldMapService.CanEnterWorldRegion(
                2,
                3,
                new[] { 2, 3 },
                _ => false),
            Is.False);
    }

    [Test]
    public void ValidateTravelRequest_RejectsNonAdjacentDestination()
    {
        WorldMapService.TravelValidationResult result =
            WorldMapService.ValidateTravelRequest(
                2,
                0,
                new[] { 2 },
                true,
                _ => false);

        Assert.That(result.CanTravel, Is.False);
        Assert.That(result.FailureMessage, Does.Contain("直接は移動できません"));
    }

    [Test]
    public void ValidateTravelRequest_RejectsEmptyParty()
    {
        WorldMapService.TravelValidationResult result =
            WorldMapService.ValidateTravelRequest(
                2,
                1,
                new[] { 2 },
                false,
                _ => false);

        Assert.That(result.CanTravel, Is.False);
        Assert.That(result.IsUnlockTravel, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("傭兵の編成が必要"));
    }

    [Test]
    public void ValidateTravelRequest_AllowsNextUnlockTravel()
    {
        WorldMapService.TravelValidationResult result =
            WorldMapService.ValidateTravelRequest(
                2,
                1,
                new[] { 2 },
                true,
                _ => false);

        Assert.That(result.CanTravel, Is.True);
        Assert.That(result.IsUnlockTravel, Is.True);
        Assert.That(result.FailureMessage, Is.Empty);
    }
}
