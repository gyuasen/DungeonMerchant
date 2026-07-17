using UnityEngine;

/// <summary>
/// Shared stable-hash helpers for day/town-seeded market pricing.
/// Consolidates the formerly duplicated private hash methods of
/// MarketPriceManager (CalculateStableHash) and MarketStockManager
/// (GetStableHash / GetStableIndex). Each entry point reproduces its
/// original caller's ingredient order bit-for-bit — do not reorder the
/// mixing steps, existing prices/stock rolls depend on them.
/// </summary>
public static class MarketHashUtility
{
    /// <summary>
    /// MarketPriceManager's sell-price hash: seed → day → itemType →
    /// rarity → item name characters.
    /// </summary>
    public static int ComputeItemHash(int seed, int day, ItemDataSO item)
    {
        unchecked
        {
            int hash = seed;
            hash = hash * 31 + day;
            return MixItem(hash, item);
        }
    }

    /// <summary>
    /// MarketStockManager's stock hash: seed → day → townIndex → salt →
    /// itemType → rarity → item name characters.
    /// </summary>
    public static int ComputeItemHash(
        int seed,
        int day,
        int townIndex,
        int salt,
        ItemDataSO item)
    {
        unchecked
        {
            int hash = seed;
            hash = hash * 31 + day;
            hash = hash * 31 + townIndex;
            hash = hash * 31 + salt;
            return MixItem(hash, item);
        }
    }

    /// <summary>
    /// MarketStockManager's stable slot index (xor/multiply scheme,
    /// unrelated to the *31 item hashes above).
    /// </summary>
    public static int ComputeStableIndex(
        int day,
        int slot,
        int townIndex,
        int itemCount)
    {
        unchecked
        {
            int hash = day * 73856093;
            hash ^= slot * 19349663;
            hash ^= townIndex * 83492791;
            return Mathf.Abs(hash) % Mathf.Max(1, itemCount);
        }
    }

    public static int ComputeRecruitmentSeed(int townIndex, int candidateBlock)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + townIndex;
            hash = hash * 31 + candidateBlock;
            return hash;
        }
    }

    private static int MixItem(int hash, ItemDataSO item)
    {
        unchecked
        {
            hash = hash * 31 + (int)item.itemType;
            hash = hash * 31 + (int)item.rarity;

            string key = string.IsNullOrWhiteSpace(item.itemName)
                ? item.name
                : item.itemName;

            foreach (char character in key)
            {
                hash = hash * 31 + character;
            }

            return hash;
        }
    }
}
