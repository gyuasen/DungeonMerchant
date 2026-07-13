using UnityEngine;

/// <summary>Shared rank label and colour rules for every equipment surface.</summary>
public static class EquipmentRankPresentation
{
    public static int ClampRank(int rank) => Mathf.Clamp(rank, 1, 10);

    public static Color GetColor(int rank)
    {
        switch (ClampRank(rank))
        {
            case 1: return new Color(0.72f, 0.72f, 0.72f);
            case 2: return new Color(0.82f, 0.88f, 0.94f);
            case 3: return new Color(0.40f, 0.84f, 0.52f);
            case 4: return new Color(0.28f, 0.74f, 0.72f);
            case 5: return new Color(0.36f, 0.65f, 1.00f);
            case 6: return new Color(0.55f, 0.44f, 1.00f);
            case 7: return new Color(0.88f, 0.42f, 0.92f);
            case 8: return new Color(1.00f, 0.48f, 0.42f);
            case 9: return new Color(1.00f, 0.72f, 0.20f);
            default: return new Color(1.00f, 0.88f, 0.32f);
        }
    }

    public static string GetRichText(int rank)
    {
        Color color = GetColor(rank);
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>Rank {ClampRank(rank)}</color>";
    }

    public static string GetRichText(ItemDataSO item) =>
        GetRichText(item != null ? item.equipmentRank : 1);
}
