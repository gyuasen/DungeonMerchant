using UnityEngine;

public static class MaterialCatalog
{
    public const float MagicStoneDropChance = 0.30f;

    public static ItemDataSO GetMagicStoneForEnemyGrade(int enemyGrade)
    {
        return Resources.Load<ItemDataSO>(GetMagicStoneResourcePathForEnemyGrade(enemyGrade));
    }

    public static string GetMagicStoneResourcePathForEnemyGrade(int enemyGrade)
    {
        int grade = Mathf.Clamp(enemyGrade, 1, 10);
        return grade >= 9 ? "GameData/Items/MagicStoneLow" :
            grade >= 7 ? "GameData/Items/MagicStoneLesser" :
            grade >= 5 ? "GameData/Items/MagicStoneMiddle" :
            grade >= 3 ? "GameData/Items/MagicStoneGreater" :
            "GameData/Items/MagicStoneHighest";
    }

    public static ItemDataSO GetMagicStoneForEquipmentRank(int equipmentRank)
    {
        return GetMagicStoneForEnemyGrade(11 - Mathf.Clamp(equipmentRank, 1, 10));
    }

    public static int GetMagicStoneAmountForEquipmentRank(int equipmentRank)
    {
        return Mathf.Clamp((Mathf.Clamp(equipmentRank, 1, 10) + 2) / 3, 1, 3);
    }

    public static string GetClassificationTag(ItemDataSO item)
    {
        if (item == null || item.itemType != ItemType.Material)
        {
            return string.Empty;
        }

        return item.materialClassification == MaterialClassification.SellOnly ? "売却用" :
            item.materialClassification == MaterialClassification.CraftingMaterial ? "材料" :
            string.Empty;
    }
}
