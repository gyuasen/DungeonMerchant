using System.Collections.Generic;

public readonly struct HiddenIslandUnlockProgress
{
    public HiddenIslandUnlockProgress(
        bool abyssDungeonCleared,
        int discoveredDungeonEquipment,
        int requiredDungeonEquipment,
        int maxLevelSpecialMercenaries)
    {
        AbyssDungeonCleared = abyssDungeonCleared;
        DiscoveredDungeonEquipment = discoveredDungeonEquipment;
        RequiredDungeonEquipment = requiredDungeonEquipment;
        MaxLevelSpecialMercenaries = maxLevelSpecialMercenaries;
    }

    public bool AbyssDungeonCleared { get; }
    public int DiscoveredDungeonEquipment { get; }
    public int RequiredDungeonEquipment { get; }
    public int MaxLevelSpecialMercenaries { get; }

    public bool HasAllDungeonEquipment =>
        RequiredDungeonEquipment > 0 &&
        DiscoveredDungeonEquipment >= RequiredDungeonEquipment;

    public bool CanUnlock =>
        AbyssDungeonCleared &&
        HasAllDungeonEquipment &&
        MaxLevelSpecialMercenaries >= 3;
}

public static class HiddenIslandUnlockService
{
    private const int AbyssTownIndex = 6;

    public static HiddenIslandUnlockProgress Evaluate(
        DungeonRunManager dungeonRunManager,
        MerchantInventory merchantInventory,
        IReadOnlyList<MercenaryInstance> hiredMercenaries)
    {
        bool abyssCleared = IsAbyssDungeonCleared(dungeonRunManager);
        CountDungeonEquipment(
            dungeonRunManager,
            merchantInventory,
            out int discoveredEquipment,
            out int requiredEquipment);
        int specialMercenaries = CountMaxLevelSpecialMercenaries(
            hiredMercenaries);

        return new HiddenIslandUnlockProgress(
            abyssCleared,
            discoveredEquipment,
            requiredEquipment,
            specialMercenaries);
    }

    public static bool TryUnlock(
        TownProgressState townProgressState,
        DungeonRunManager dungeonRunManager,
        MerchantInventory merchantInventory,
        IReadOnlyList<MercenaryInstance> hiredMercenaries)
    {
        if (townProgressState == null ||
            townProgressState.IsTownUnlocked(
                WorldMapService.HiddenIslandTownIndex))
        {
            return false;
        }

        HiddenIslandUnlockProgress progress = Evaluate(
            dungeonRunManager,
            merchantInventory,
            hiredMercenaries);
        if (!progress.CanUnlock)
        {
            return false;
        }

        townProgressState.UnlockTown(WorldMapService.HiddenIslandTownIndex);
        return true;
    }

    private static bool IsAbyssDungeonCleared(
        DungeonRunManager dungeonRunManager)
    {
        if (dungeonRunManager == null)
        {
            return false;
        }

        DungeonDataSO abyssDungeon =
            dungeonRunManager.GetHighestGradeDungeonNearTown(AbyssTownIndex);
        return abyssDungeon != null &&
               dungeonRunManager.GetClearedFloors(abyssDungeon) >=
               System.Math.Max(1, abyssDungeon.totalFloors);
    }

    private static void CountDungeonEquipment(
        DungeonRunManager dungeonRunManager,
        MerchantInventory merchantInventory,
        out int discovered,
        out int required)
    {
        discovered = 0;
        required = 0;
        if (dungeonRunManager == null || merchantInventory == null)
        {
            return;
        }

        HashSet<string> countedEquipment = new HashSet<string>();
        foreach (DungeonDataSO dungeon in dungeonRunManager.AvailableDungeons)
        {
            if (dungeon == null ||
                dungeon.nearbyTownIndex == WorldMapService.HiddenIslandTownIndex ||
                dungeon.limitedEquipmentDrops == null)
            {
                continue;
            }

            foreach (ItemDataSO item in dungeon.limitedEquipmentDrops)
            {
                if (item == null ||
                    !item.IsEquipment ||
                    !countedEquipment.Add(item.PersistentId))
                {
                    continue;
                }

                required++;
                if (merchantInventory.HasDiscoveredEquipment(item))
                {
                    discovered++;
                }
            }
        }
    }

    private static int CountMaxLevelSpecialMercenaries(
        IReadOnlyList<MercenaryInstance> hiredMercenaries)
    {
        if (hiredMercenaries == null)
        {
            return 0;
        }

        int count = 0;
        foreach (MercenaryInstance mercenary in hiredMercenaries)
        {
            if (mercenary != null &&
                MercenaryClassProgression.IsSpecialClass(
                    mercenary.MercenaryClass) &&
                mercenary.IsAtLevelCap)
            {
                count++;
            }
        }
        return count;
    }
}
