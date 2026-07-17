using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TradeMaterialAssetUpdater
{
    private const string ItemFolder = "Assets/Proiject/Resources/GameData/Items";

    private static readonly MaterialDefinition[] NewMaterials =
    {
        new MaterialDefinition("MagicStoneLow", "item.material.magic_stone.low", "低級魔石", 90),
        new MaterialDefinition("MagicStoneLesser", "item.material.magic_stone.lesser", "下級魔石", 180),
        new MaterialDefinition("MagicStoneMiddle", "item.material.magic_stone.middle", "中級魔石", 360),
        new MaterialDefinition("MagicStoneGreater", "item.material.magic_stone.greater", "上級魔石", 720),
        new MaterialDefinition("MagicStoneHighest", "item.material.magic_stone.highest", "最上級魔石", 1440),
        new MaterialDefinition("IronOre", "item.material.iron_ore", "鉄鉱石", 55),
        new MaterialDefinition("SilverOre", "item.material.silver_ore", "銀鉱石", 180),
        new MaterialDefinition("MedicinalHerb", "item.material.medicinal_herb", "薬草", 45),
        new MaterialDefinition("AntidoteHerb", "item.material.antidote_herb", "毒消し草", 130),
        new MaterialDefinition("Hardwood", "item.material.hardwood", "堅木材", 70),
        new MaterialDefinition("Spiritwood", "item.material.spiritwood", "霊木材", 220)
    };

    [MenuItem("DungeonMerchant/Trade Materials/Apply Classification And Recipes")]
    public static void Apply()
    {
        CreateNewMaterials();
        ClassifyExistingMaterials();
        UpdateRecipes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateNewMaterials()
    {
        foreach (MaterialDefinition definition in NewMaterials)
        {
            string path = ItemFolder + "/" + definition.AssetName + ".asset";
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDataSO>();
                AssetDatabase.CreateAsset(item, path);
            }
            item.itemName = definition.DisplayName;
            item.itemType = ItemType.Material;
            item.materialClassification = MaterialClassification.CraftingMaterial;
            item.basePrice = definition.Price;
            item.description = "Crafting material gathered in the demon continent.";
            SetPersistentId(item, definition.PersistentId);
            EditorUtility.SetDirty(item);
        }
    }

    private static void ClassifyExistingMaterials()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ItemDataSO", new[] { ItemFolder }))
        {
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || item.itemType != ItemType.Material)
            {
                continue;
            }
            item.materialClassification = IsSellOnlyMaterial(item)
                ? MaterialClassification.SellOnly
                : MaterialClassification.CraftingMaterial;
            EditorUtility.SetDirty(item);
        }
    }

    private static bool IsSellOnlyMaterial(ItemDataSO item)
    {
        string value = (item.PersistentId + " " + item.itemName).ToLowerInvariant();
        return value.Contains("ear") || value.Contains("wing") ||
               value.Contains("crown") || value.Contains("seal") ||
               value.Contains("emblem") || value.Contains("charm") ||
               value.Contains("talisman") || value.Contains("eye");
    }

    private static void UpdateRecipes()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:EquipmentRecipeSO"))
        {
            EquipmentRecipeSO recipe = AssetDatabase.LoadAssetAtPath<EquipmentRecipeSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (recipe == null || recipe.resultItem == null)
            {
                continue;
            }
            ItemDataSO magicStone = MaterialCatalog.GetMagicStoneForEquipmentRank(recipe.resultItem.equipmentRank);
            if (magicStone == null)
            {
                throw new InvalidOperationException("Magic stone assets must be created before recipes.");
            }
            List<CraftingMaterialRequirement> requirements = new List<CraftingMaterialRequirement>(recipe.materials ?? Array.Empty<CraftingMaterialRequirement>());
            requirements.RemoveAll(requirement => requirement != null && requirement.item != null && requirement.item.PersistentId.StartsWith("item.material.magic_stone.", StringComparison.Ordinal));
            requirements.Add(new CraftingMaterialRequirement { item = magicStone, amount = MaterialCatalog.GetMagicStoneAmountForEquipmentRank(recipe.resultItem.equipmentRank) });
            recipe.materials = requirements.ToArray();
            EditorUtility.SetDirty(recipe);
        }
    }

    private static void SetPersistentId(ItemDataSO item, string persistentId)
    {
        SerializedObject serialized = new SerializedObject(item);
        serialized.FindProperty("persistentId").stringValue = persistentId;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private readonly struct MaterialDefinition
    {
        public readonly string AssetName;
        public readonly string PersistentId;
        public readonly string DisplayName;
        public readonly int Price;

        public MaterialDefinition(string assetName, string persistentId, string displayName, int price)
        {
            AssetName = assetName;
            PersistentId = persistentId;
            DisplayName = displayName;
            Price = price;
        }
    }
}
