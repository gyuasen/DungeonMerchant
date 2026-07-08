public partial class SimpleMercenaryHireUI
{
    private static readonly string[] TownNames =
        WorldMapService.TownNames;

    private static readonly string[] WorldRegionNames =
        WorldMapService.WorldRegionNames;

    private static int GetWorldMapIndexForTown(int townIndex)
    {
        return WorldMapService.GetWorldMapIndexForTown(townIndex);
    }

    private static bool AreTownsAdjacent(int leftTown, int rightTown)
    {
        return WorldMapService.AreTownsAdjacent(leftTown, rightTown);
    }

    private static int GetNextTownToward(
        int originTown,
        int destinationTown)
    {
        return WorldMapService.GetNextTownToward(
            originTown,
            destinationTown);
    }
}
