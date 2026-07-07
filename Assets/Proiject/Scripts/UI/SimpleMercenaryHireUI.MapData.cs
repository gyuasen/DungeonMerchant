public partial class SimpleMercenaryHireUI
{
    private static string[] TownNames => TownMapService.TownNames;
    private static string[] WorldRegionNames => TownMapService.WorldRegionNames;

    private static int GetWorldMapIndexForTown(int townIndex)
    {
        return TownMapService.GetWorldMapIndexForTown(townIndex);
    }

    private static int GetTownProgressionPosition(int townIndex)
    {
        return TownMapService.GetTownProgressionPosition(townIndex);
    }

    private static bool AreTownsAdjacent(int leftTown, int rightTown)
    {
        return TownMapService.AreTownsAdjacent(leftTown, rightTown);
    }

    private static int GetNextTownToward(
        int originTown,
        int destinationTown)
    {
        return TownMapService.GetNextTownToward(originTown, destinationTown);
    }
}
