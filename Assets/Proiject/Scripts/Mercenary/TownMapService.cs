using System.Collections.Generic;
using UnityEngine;

public static class TownMapService
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

    public static readonly string[] WorldRegionNames =
    {
        "東方平原地域",
        "北西山岳森林地域",
        "南西黒土地域"
    };

    private static readonly int[] TownWorldMapIndices =
    {
        0, 0, 0, 1, 1, 2, 2
    };

    private static readonly int[] TownProgressionOrder =
    {
        2, 1, 0, 3, 4, 5, 6
    };

    public static int TownCount => TownNames.Length;

    public readonly struct TravelValidationResult
    {
        public TravelValidationResult(
            bool canTravel,
            bool isUnlockTravel,
            string failureMessage)
        {
            CanTravel = canTravel;
            IsUnlockTravel = isUnlockTravel;
            FailureMessage = failureMessage;
        }

        public bool CanTravel { get; }
        public bool IsUnlockTravel { get; }
        public string FailureMessage { get; }
    }

    public static int GetWorldMapIndexForTown(int townIndex)
    {
        return townIndex >= 0 && townIndex < TownWorldMapIndices.Length
            ? TownWorldMapIndices[townIndex]
            : 0;
    }

    public static int GetTownProgressionPosition(int townIndex)
    {
        return System.Array.IndexOf(TownProgressionOrder, townIndex);
    }

    public static int GetTownAtProgressionPosition(int position)
    {
        return position >= 0 && position < TownProgressionOrder.Length
            ? TownProgressionOrder[position]
            : -1;
    }

    public static bool AreTownsAdjacent(int leftTown, int rightTown)
    {
        int leftPosition = GetTownProgressionPosition(leftTown);
        int rightPosition = GetTownProgressionPosition(rightTown);
        return leftPosition >= 0 &&
               rightPosition >= 0 &&
               Mathf.Abs(leftPosition - rightPosition) == 1;
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

    public static int GetNextUnlockableTownIndex(
        IEnumerable<int> unlockedTownIndices)
    {
        HashSet<int> unlocked = ToSet(unlockedTownIndices);
        foreach (int townIndex in TownProgressionOrder)
        {
            if (!unlocked.Contains(townIndex))
            {
                return townIndex;
            }
        }
        return -1;
    }

    public static bool HasUnlockedTownInWorld(
        int worldMapIndex,
        IEnumerable<int> unlockedTownIndices)
    {
        foreach (int townIndex in ToSet(unlockedTownIndices))
        {
            if (GetWorldMapIndexForTown(townIndex) == worldMapIndex)
            {
                return true;
            }
        }
        return false;
    }

    public static bool CanEnterWorldRegion(
        int worldMapIndex,
        int currentTownIndex,
        IEnumerable<int> unlockedTownIndices,
        DungeonRunManager dungeonRunManager)
    {
        if (worldMapIndex <= 0 ||
            GetWorldMapIndexForTown(currentTownIndex) == worldMapIndex ||
            HasUnlockedTownInWorld(worldMapIndex, unlockedTownIndices))
        {
            return true;
        }

        int gateTownIndex = worldMapIndex == 1 ? 0 : 4;
        DungeonDataSO gateDungeon =
            dungeonRunManager?.GetHighestGradeDungeonNearTown(gateTownIndex);
        return gateDungeon != null &&
               dungeonRunManager.GetClearedFloors(gateDungeon) >=
               Mathf.Max(1, gateDungeon.totalFloors);
    }

    public static TravelValidationResult ValidateTravelRequest(
        int currentTownIndex,
        int destinationTownIndex,
        IEnumerable<int> unlockedTownIndices,
        bool hasPartyMembers,
        DungeonRunManager dungeonRunManager)
    {
        destinationTownIndex = Mathf.Clamp(
            destinationTownIndex,
            0,
            TownCount - 1);

        if (!AreTownsAdjacent(destinationTownIndex, currentTownIndex))
        {
            int nextTownIndex =
                GetNextTownToward(currentTownIndex, destinationTownIndex);
            string message =
                $"{TownNames[destinationTownIndex]}へ直接は移動できません。" +
                (nextTownIndex >= 0
                    ? $"先に{TownNames[nextTownIndex]}を経由してください。"
                    : string.Empty);
            return new TravelValidationResult(false, false, message);
        }

        int destinationWorld = GetWorldMapIndexForTown(destinationTownIndex);
        if (!CanEnterWorldRegion(
                destinationWorld,
                currentTownIndex,
                unlockedTownIndices,
                dungeonRunManager))
        {
            return new TravelValidationResult(
                false,
                false,
                $"{WorldRegionNames[destinationWorld]}の解放条件を満たしていません。");
        }

        HashSet<int> unlocked = ToSet(unlockedTownIndices);
        bool isUnlockTravel = !unlocked.Contains(destinationTownIndex);
        if (isUnlockTravel)
        {
            int nextTownIndex = GetNextUnlockableTownIndex(unlocked);
            if (destinationTownIndex != nextTownIndex)
            {
                string message = nextTownIndex >= 0
                    ? $"先に{TownNames[nextTownIndex]}への移動クエストを攻略してください。"
                    : "これ以上解放できる町はありません。";
                return new TravelValidationResult(false, true, message);
            }
        }

        if (!hasPartyMembers)
        {
            return new TravelValidationResult(
                false,
                isUnlockTravel,
                "町の移動には街道戦闘が発生するため、傭兵の編成が必要です。");
        }

        return new TravelValidationResult(true, isUnlockTravel, string.Empty);
    }

    private static HashSet<int> ToSet(IEnumerable<int> values)
    {
        HashSet<int> result = new HashSet<int>();
        if (values == null)
        {
            return result;
        }

        foreach (int value in values)
        {
            if (value >= 0 && value < TownCount)
            {
                result.Add(value);
            }
        }
        return result;
    }
}
