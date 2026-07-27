public static class TownServicePolicy
{
    private const int VelmTownIndex = 5;
    private const int AbyssTownIndex = 6;
    private const int JobChangeUnlockTownIndex = 0; // エルド交易都市

    // 転職神殿は「進行順でエルド以降」の町で使える。町の訪問順は
    // WorldMapService.TownProgressionOrder（セイル→リーフ→エルド→…）で決まり、
    // エルドより手前のセイル・リーフでは使えない。隠し島は別扱いで対象外。
    public static bool IsJobChangeAvailable(int townIndex)
    {
        if (IsHiddenIslandTown(townIndex))
        {
            return false;
        }

        int[] order = WorldMapService.TownProgressionOrder;
        int rank = System.Array.IndexOf(order, townIndex);
        int unlockRank = System.Array.IndexOf(order, JobChangeUnlockTownIndex);
        return rank >= 0 && unlockRank >= 0 && rank >= unlockRank;
    }

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

    public static bool IsTrainingGroundAvailable(int townIndex)
    {
        return townIndex >= 0 &&
               townIndex <= 6 &&
               townIndex != 2;
    }
}
