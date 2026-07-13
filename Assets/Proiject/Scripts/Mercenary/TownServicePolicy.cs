public static class TownServicePolicy
{
    private const int VelmTownIndex = 5;
    private const int AbyssTownIndex = 6;

    public static bool IsHiringAvailable(int townIndex)
    {
        return townIndex != VelmTownIndex &&
               townIndex != AbyssTownIndex &&
               townIndex != WorldMapService.HiddenIslandTownIndex;
    }

    public static bool IsHiddenIslandTown(int townIndex)
    {
        return townIndex == WorldMapService.HiddenIslandTownIndex;
    }
}
