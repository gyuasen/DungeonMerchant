public static class TownServicePolicy
{
    private const int VelmTownIndex = 5;
    private const int AbyssTownIndex = 6;

    public static bool IsHiringAvailable(int townIndex)
    {
        return townIndex != VelmTownIndex &&
               townIndex != AbyssTownIndex;
    }
}
