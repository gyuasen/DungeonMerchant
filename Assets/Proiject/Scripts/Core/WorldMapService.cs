using System;
using System.Collections.Generic;

public static class WorldMapService
{
    public static readonly string[] TownNames =
    {
        "エルド交易都市",
        "リーフ森林都市",
        "セイル港湾都市",
        "ノルン樹冠都市",
        "グラード山塞都市",
        "ヴェルム黒鉄都市",
        "アビス辺境都市"
    };

    public static readonly int[] TownWorldMapIndices =
    {
        0, 0, 0, 1, 1, 2, 2
    };

    public static readonly int[] TownProgressionOrder =
    {
        2, 1, 0, 3, 4, 5, 6
    };

    public static readonly string[] WorldRegionNames =
    {
        "東方平原地域",
        "北西山岳森林地域",
        "南西黒土地域"
    };

    public static int TownCount => TownNames.Length;

    public static int WorldRegionCount => WorldRegionNames.Length;

    public static string GetTownName(int townIndex)
    {
        return townIndex >= 0 && townIndex < TownNames.Length
            ? TownNames[townIndex]
            : string.Empty;
    }

    public static bool IsValidTownIndex(int townIndex)
    {
        return townIndex >= 0 && townIndex < TownNames.Length;
    }

    public static string GetWorldRegionName(int worldMapIndex)
    {
        return worldMapIndex >= 0 && worldMapIndex < WorldRegionNames.Length
            ? WorldRegionNames[worldMapIndex]
            : string.Empty;
    }

    public static int GetWorldMapIndexForTown(int townIndex)
    {
        return townIndex >= 0 && townIndex < TownWorldMapIndices.Length
            ? TownWorldMapIndices[townIndex]
            : 0;
    }

    public static int GetTownProgressionPosition(int townIndex)
    {
        return Array.IndexOf(TownProgressionOrder, townIndex);
    }

    public static bool AreTownsAdjacent(int leftTown, int rightTown)
    {
        int leftPosition = GetTownProgressionPosition(leftTown);
        int rightPosition = GetTownProgressionPosition(rightTown);
        return leftPosition >= 0 &&
               rightPosition >= 0 &&
               Math.Abs(leftPosition - rightPosition) == 1;
    }

    public static int GetNextTownToward(
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

    public static HashSet<int> CreateRestoredUnlockedTownIndices(
        int currentTownIndex,
        IReadOnlyList<int> savedUnlockedTownIndices)
    {
        HashSet<int> result = new HashSet<int> { 2 };
        if (savedUnlockedTownIndices != null)
        {
            foreach (int townIndex in savedUnlockedTownIndices)
            {
                if (IsValidTownIndex(townIndex))
                {
                    result.Add(townIndex);
                }
            }
        }

        if (IsValidTownIndex(currentTownIndex))
        {
            result.Add(currentTownIndex);
        }

        if (savedUnlockedTownIndices == null)
        {
            int currentOrder = GetTownProgressionPosition(currentTownIndex);
            for (int i = 0; i <= currentOrder; i++)
            {
                result.Add(TownProgressionOrder[i]);
            }
        }

        return result;
    }

    public static bool HasUnlockedTownInWorld(
        IEnumerable<int> unlockedTownIndices,
        int worldMapIndex)
    {
        if (unlockedTownIndices == null)
        {
            return false;
        }

        foreach (int townIndex in unlockedTownIndices)
        {
            if (GetWorldMapIndexForTown(townIndex) == worldMapIndex)
            {
                return true;
            }
        }
        return false;
    }

    public static int GetNextUnlockableTownIndex(
        IEnumerable<int> unlockedTownIndices)
    {
        HashSet<int> unlocked =
            unlockedTownIndices as HashSet<int> ??
            new HashSet<int>(unlockedTownIndices ?? Array.Empty<int>());
        foreach (int townIndex in TownProgressionOrder)
        {
            if (!unlocked.Contains(townIndex))
            {
                return townIndex;
            }
        }
        return -1;
    }

    public static int GetGateTownIndexForWorldRegion(int worldMapIndex)
    {
        return worldMapIndex == 1 ? 0 :
            worldMapIndex == 2 ? 4 :
            -1;
    }

    public static bool CanEnterWorldRegion(
        int worldMapIndex,
        int currentTownIndex,
        IEnumerable<int> unlockedTownIndices,
        Func<int, bool> isGateTownFullyCleared)
    {
        if (worldMapIndex <= 0 ||
            GetWorldMapIndexForTown(currentTownIndex) == worldMapIndex ||
            HasUnlockedTownInWorld(unlockedTownIndices, worldMapIndex))
        {
            return true;
        }

        int gateTownIndex = GetGateTownIndexForWorldRegion(worldMapIndex);
        return gateTownIndex >= 0 &&
               isGateTownFullyCleared != null &&
               isGateTownFullyCleared(gateTownIndex);
    }
}
