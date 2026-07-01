using UnityEngine;

public partial class SimpleMercenaryHireUI
{
    private static readonly string[] TownNames =
    {
        "エルド交易都市",
        "リーフ森林都市",
        "セイル港湾都市",
        "ノルン樹冠都市",
        "グラード山塞都市",
        "ヴェルム黒鉄都市",
        "アビス辺境都市"
    };

    private static readonly int[] TownWorldMapIndices =
    {
        0, 0, 0, 1, 1, 2, 2
    };

    private static readonly int[] TownProgressionOrder =
    {
        2, 1, 0, 3, 4, 5, 6
    };

    private static readonly string[] WorldRegionNames =
    {
        "東方平原地域",
        "北西山岳森林地域",
        "南西黒土地域"
    };

    private static int GetWorldMapIndexForTown(int townIndex)
    {
        return townIndex >= 0 && townIndex < TownWorldMapIndices.Length
            ? TownWorldMapIndices[townIndex]
            : 0;
    }

    private static int GetTownProgressionPosition(int townIndex)
    {
        return System.Array.IndexOf(TownProgressionOrder, townIndex);
    }

    private static bool AreTownsAdjacent(int leftTown, int rightTown)
    {
        int leftPosition = GetTownProgressionPosition(leftTown);
        int rightPosition = GetTownProgressionPosition(rightTown);
        return leftPosition >= 0 &&
               rightPosition >= 0 &&
               Mathf.Abs(leftPosition - rightPosition) == 1;
    }

    private static int GetNextTownToward(
        int originTown,
        int destinationTown)
    {
        int originPosition = GetTownProgressionPosition(originTown);
        int destinationPosition = GetTownProgressionPosition(destinationTown);
        if (originPosition < 0 ||
            destinationPosition < 0 ||
            originPosition == destinationPosition)
        {
            return -1;
        }

        int nextPosition = originPosition +
                           (destinationPosition > originPosition ? 1 : -1);
        return TownProgressionOrder[nextPosition];
    }
}
