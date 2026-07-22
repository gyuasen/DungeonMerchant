using System;
using System.Collections.Generic;

public static class WorldMapService
{
    public const int HiddenIslandTownIndex = 7;
    public const int HiddenIslandWorldMapIndex = 3;

    public static readonly string[] TownNames =
    {
        "エルド交易都市",
        "リーフ森林都市",
        "セイル港湾都市",
        "ノルン樹冠都市",
        "グラード山塞都市",
        "ヴェルム黒鉄都市",
        "アビス辺境都市",
        "アステラ秘匿都市"
    };

    public static readonly int[] TownWorldMapIndices =
    {
        0, 0, 0, 1, 1, 2, 2, HiddenIslandWorldMapIndex
    };

    public static readonly int[] TownProgressionOrder =
    {
        2, 1, 0, 3, 4, 5, 6
    };

    public static readonly string[] WorldRegionNames =
    {
        "東方平原地域",
        "北西山岳森林地域",
        "南西黒土地域",
        "中央島アステラ"
    };

    public static int TownCount => TownNames.Length;

    public static int WorldRegionCount => WorldRegionNames.Length;

    // Demand affects only the price paid when the player sells an item.  Keep
    // this table independent of daily market variation and Unity runtime APIs
    // so its results remain stable and easy to test.
    public static float GetTownDemandMultiplier(int townIndex, ItemDataSO item)
    {
        if (item == null)
        {
            return 1f;
        }

        float multiplier = GetTownDemandMultiplier(townIndex, item.itemType);
        if (townIndex == 1 && IsLeafSpecialtyMaterial(item))
        {
            multiplier = 1.25f;
        }
        if ((townIndex == 4 || townIndex == 5) && IsMineralMaterial(item))
        {
            multiplier = 1.20f;
        }

        return ClampTownDemandMultiplier(multiplier);
    }

    public static float GetTownDemandMultiplier(int townIndex, ItemType itemType)
    {
        float multiplier = 1f;
        switch (townIndex)
        {
            case 0: // Eld: trade hub
                multiplier = itemType == ItemType.Equipment ? 1.15f :
                    itemType == ItemType.Material ? 1.05f : 1f;
                break;
            case 2: // Sale: exports monster materials, imports consumables
                multiplier = itemType == ItemType.Material ? 1.15f :
                    itemType == ItemType.Consumable ? 0.85f : 1f;
                break;
            case 3: // Norn: inland canopy city
                multiplier = itemType == ItemType.Material ? 1.10f :
                    itemType == ItemType.Consumable ? 1.15f : 1f;
                break;
            case 4: // Glaad: mountain fortress
                multiplier = itemType == ItemType.Material ? 1.20f :
                    itemType == ItemType.Consumable ? 1.15f : 1f;
                break;
            case 5: // Velm: remote industrial city
                multiplier = itemType == ItemType.Consumable ? 1.25f :
                    itemType == ItemType.Equipment ? 1.10f : 1f;
                break;
            case 6: // Abyss: deepest frontier
                multiplier = itemType == ItemType.Consumable ? 1.30f :
                    itemType == ItemType.Equipment ? 1.10f : 1f;
                break;
            // Leaf (1) has only its named material specialties.  Astera (7)
            // deliberately stays outside the ordinary economy.
        }

        return ClampTownDemandMultiplier(multiplier);
    }

    private static bool IsLeafSpecialtyMaterial(ItemDataSO item)
    {
        return item.itemType == ItemType.Material &&
               (string.Equals(item.itemName, "Bat Wing",
                    StringComparison.Ordinal) ||
                string.Equals(item.itemName, "Slime Mucus",
                    StringComparison.Ordinal) ||
                string.Equals(item.itemName, "Venom Moth Powder",
                    StringComparison.Ordinal));
    }

    private static bool IsMineralMaterial(ItemDataSO item)
    {
        return item.itemType == ItemType.Material &&
               (item.PersistentId == "item.material.iron_ore" ||
                item.PersistentId == "item.material.silver_ore");
    }

    private static float ClampTownDemandMultiplier(float multiplier)
    {
        return Math.Max(0.8f, Math.Min(1.3f, multiplier));
    }

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
        if (leftTown == HiddenIslandTownIndex ||
            rightTown == HiddenIslandTownIndex)
        {
            return IsValidTownIndex(leftTown) &&
                   IsValidTownIndex(rightTown) &&
                   leftTown != rightTown;
        }

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
        if (worldMapIndex == HiddenIslandWorldMapIndex)
        {
            return HasUnlockedTownInWorld(
                unlockedTownIndices,
                HiddenIslandWorldMapIndex);
        }

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

    public static TravelValidationResult ValidateTravelRequest(
        int currentTownIndex,
        int destinationTownIndex,
        IEnumerable<int> unlockedTownIndices,
        bool hasPartyMembers,
        Func<int, bool> isGateTownFullyCleared)
    {
        destinationTownIndex = ClampTownIndex(destinationTownIndex);

        HashSet<int> unlocked =
            unlockedTownIndices as HashSet<int> ??
            new HashSet<int>(unlockedTownIndices ?? Array.Empty<int>());
        if (destinationTownIndex == HiddenIslandTownIndex ||
            currentTownIndex == HiddenIslandTownIndex)
        {
            if (!unlocked.Contains(HiddenIslandTownIndex))
            {
                return new TravelValidationResult(
                    false,
                    false,
                    "中央島へ至る航路はまだ発見されていません。");
            }

            if (!unlocked.Contains(destinationTownIndex))
            {
                return new TravelValidationResult(
                    false,
                    false,
                    "未解放の町へ中央島航路から移動することはできません。");
            }

            return new TravelValidationResult(true, false, string.Empty);
        }

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
                isGateTownFullyCleared))
        {
            return new TravelValidationResult(
                false,
                false,
                $"{WorldRegionNames[destinationWorld]}の解放条件を満たしていません。");
        }

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

    private static int ClampTownIndex(int townIndex)
    {
        if (townIndex < 0)
        {
            return 0;
        }
        return townIndex >= TownNames.Length
            ? TownNames.Length - 1
            : townIndex;
    }

    public readonly struct EquipmentRankRange
    {
        public EquipmentRankRange(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public int Minimum { get; }
        public int Maximum { get; }
        public bool Contains(int rank) => rank >= Minimum && rank <= Maximum;
    }

    public static EquipmentRankRange GetMarketEquipmentRankRange(int townIndex)
    {
        int rank = GetTownEquipmentRank(townIndex);
        return new EquipmentRankRange(rank, rank);
    }

    public static EquipmentRankRange GetBlacksmithEquipmentRankRange(int townIndex)
    {
        int rank = Math.Min(GetTownEquipmentRank(townIndex) + 1, 10);
        return new EquipmentRankRange(1, rank);
    }

    public static int GetDungeonEquipmentRank(int townIndex)
    {
        if (townIndex == HiddenIslandTownIndex)
        {
            return 10;
        }

        // Standard dungeon equipment is two ranks above the local market.
        // Rank 10 is deliberately excluded and reserved for hidden rewards.
        return Math.Min(GetTownEquipmentRank(townIndex) + 2, 9);
    }

    public static bool IsStandardDungeonEquipmentRank(
        int townIndex,
        int equipmentRank)
    {
        return equipmentRank == GetDungeonEquipmentRank(townIndex) &&
               equipmentRank < 10;
    }

    /// <summary>
    /// Returns the equipment ranks that may appear as limited drops in a town's
    /// dungeons.  Towns with more than one dungeon intentionally span from
    /// their market rank through their standard (highest) dungeon rank.
    /// </summary>
    public static EquipmentRankRange GetDungeonEquipmentRankRange(int townIndex)
    {
        if (townIndex == HiddenIslandTownIndex)
        {
            return new EquipmentRankRange(10, 10);
        }

        return new EquipmentRankRange(
            GetTownEquipmentRank(townIndex),
            GetDungeonEquipmentRank(townIndex));
    }

    public static bool IsDungeonEquipmentRankAllowed(
        int townIndex,
        int equipmentRank)
    {
        return GetDungeonEquipmentRankRange(townIndex).Contains(equipmentRank);
    }

    public static int GetNextTownInProgression(int townIndex)
    {
        int position = GetTownProgressionPosition(townIndex);
        return position >= 0 && position < TownProgressionOrder.Length - 1
            ? TownProgressionOrder[position + 1]
            : townIndex;
    }

    public static bool IsMarketEquipmentAllowedInTown(
        int townIndex,
        MercenaryClass baseClass,
        int equipmentRank,
        EquipmentSlot slot)
    {
        return GetMarketEquipmentRankRange(townIndex).Contains(equipmentRank);
    }

    public static bool IsBlacksmithEquipmentAllowedInTown(
        int townIndex,
        MercenaryClass baseClass,
        int equipmentRank,
        EquipmentSlot slot)
    {
        return GetBlacksmithEquipmentRankRange(townIndex).Contains(equipmentRank);
    }

    private static int GetTownEquipmentRank(int townIndex)
    {
        if (townIndex == HiddenIslandTownIndex)
        {
            return 10;
        }

        int position = GetTownProgressionPosition(townIndex);
        return position >= 0 ? Math.Min(position + 1, 10) : 1;
    }

}
