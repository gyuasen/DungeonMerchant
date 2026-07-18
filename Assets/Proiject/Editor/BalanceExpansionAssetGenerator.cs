using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BalanceExpansionAssetGenerator
{
    const string EnemyPath = "Assets/Proiject/Resources/GameData/Enemies/Expansion";
    const string ItemPath = "Assets/Proiject/Resources/GameData/Items/Expansion";
    const string RecipePath = "Assets/Proiject/Resources/GameData/Blacksmith/Expansion";
    public static void BuildFromCommandLine() { Build(); }
    [MenuItem("DungeonMerchant/Build Balance Expansion Assets")]
    public static void Build()
    {
        Folders();
        foreach (BalanceExpansionNormalEnemyDefinition enemy in BalanceExpansionDefinition.NormalEnemies) { BuildNormalEnemy(enemy); }
        foreach (BalanceExpansionEnemyDefinition enemy in BalanceExpansionDefinition.Enemies) { BuildEnemy(enemy); }
        foreach (BalanceExpansionEquipmentDefinition equipment in BalanceExpansionDefinition.Equipment) { BuildEquipment(equipment); }
        foreach (BalanceExpansionConsumableDefinition consumable in BalanceExpansionDefinition.Consumables) { BuildConsumable(consumable); }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    static void BuildEquipment(BalanceExpansionEquipmentDefinition definition)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = definition.EnglishName;
        item.description = definition.EnglishName;
        item.itemType = ItemType.Equipment;
        item.acquisitionType = definition.AcquisitionType;
        item.equipmentSlot = definition.Slot;
        item.requiredClass = new[] { MercenaryClass.Warrior, MercenaryClass.Archer, MercenaryClass.Mage }[definition.ClassIndex];
        item.equipmentRank = definition.Rank;
        ApplyEquipmentStats(item, definition);
        Id(item, definition.Id);
        string path = ItemPath + "/" + definition.Id.Replace('.', '_') + ".asset";
        Put(item, path);
        if (definition.AcquisitionType == ItemAcquisitionType.Blacksmith) { BuildRecipe(definition, AssetDatabase.LoadAssetAtPath<ItemDataSO>(path)); }
    }
    static void ApplyEquipmentStats(ItemDataSO item, BalanceExpansionEquipmentDefinition definition)
    {
        int rank = definition.Rank;
        if (definition.Slot == EquipmentSlot.Weapon) { item.bonusAttack = rank * 3 + (definition.ClassIndex == 0 ? 2 : definition.ClassIndex == 2 ? 1 : 0); item.bonusMaxHP = definition.ClassIndex == 0 ? rank * 4 : definition.ClassIndex == 1 ? 2 : 1; item.bonusAttackSpeed = definition.ClassIndex == 1 ? .06f : 0f; item.basePrice = 180 + rank * 95; return; }
        if (definition.Slot == EquipmentSlot.Armor) { item.bonusMaxHP = definition.ClassIndex == 0 ? rank * 11 : rank * 7; item.bonusDefense = rank * 2 + (definition.ClassIndex == 0 ? 2 : 0); item.bonusAttack = definition.ClassIndex == 2 ? 4 : 0; item.bonusAttackSpeed = definition.ClassIndex == 1 ? .03f : 0f; item.basePrice = 255 + rank * 55; return; }
        item.bonusMaxHP = definition.ClassIndex == 0 ? rank * 4 : definition.ClassIndex == 1 ? rank * 2 : rank * 2 + 1;
        item.bonusAttack = definition.ClassIndex == 2 ? rank + 3 : definition.ClassIndex == 0 ? 2 : 3;
        item.bonusDefense = definition.ClassIndex == 0 ? rank : 1;
        item.bonusAttackSpeed = definition.ClassIndex == 1 ? .04f + rank * .005f : definition.ClassIndex == 2 ? .01f + rank * .003f : .01f;
        item.basePrice = 280 + rank * 60;
    }
    static void BuildRecipe(BalanceExpansionEquipmentDefinition definition, ItemDataSO item)
    {
        EquipmentRecipeSO recipe = ScriptableObject.CreateInstance<EquipmentRecipeSO>();
        recipe.recipeName = item.itemName;
        recipe.resultItem = item;
        recipe.goldCost = 120 + definition.Rank * 70;
        ItemDataSO magicStone = MaterialCatalog.GetMagicStoneForEquipmentRank(definition.Rank);
        if (magicStone == null) { throw new InvalidOperationException("Run Trade Materials/Apply Classification And Recipes first."); }
        recipe.materials = new[] { new CraftingMaterialRequirement { item = Load<ItemDataSO>("GameData/Items/MonsterFang"), amount = definition.Rank }, new CraftingMaterialRequirement { item = Load<ItemDataSO>("GameData/Items/EnhancementOre"), amount = Math.Max(1, definition.Rank - 3) }, new CraftingMaterialRequirement { item = magicStone, amount = MaterialCatalog.GetMagicStoneAmountForEquipmentRank(definition.Rank) } };
        Put(recipe, RecipePath + "/" + definition.Id.Replace('.', '_') + "Recipe.asset");
    }
    static void BuildConsumable(BalanceExpansionConsumableDefinition definition)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = definition.EnglishName;
        item.description = definition.EnglishName;
        item.itemType = ItemType.Consumable;
        item.acquisitionType = ItemAcquisitionType.Market;
        item.basePrice = definition.Price;
        item.consumableEffect = definition.Effect;
        item.consumableHealAmount = definition.Amount;
        Id(item, definition.Id);
        Put(item, ItemPath + "/" + definition.Id.Replace('.', '_') + ".asset");
    }
    static void BuildNormalEnemy(BalanceExpansionNormalEnemyDefinition definition)
    {
        string path = "Assets/Proiject/Resources/GameData/Enemies/" + definition.AssetName + ".asset";
        EnemyDataSO enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
        if (enemy == null) { throw new InvalidOperationException(path); }
        enemy.enemyName = definition.EnglishName;
        enemy.monsterGrade = definition.Grade;
        enemy.battleVisualKey = definition.BattleVisualKey;
        Id(enemy, definition.Id);
        AddEnemyToDungeon(enemy, definition.DungeonAsset);
        EditorUtility.SetDirty(enemy);
    }
    static void BuildEnemy(BalanceExpansionEnemyDefinition definition) { EnemyDataSO basis = Load<EnemyDataSO>("GameData/Enemies/" + definition.BaseEnemyAsset); EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>(); enemy.enemyName = definition.EnglishName; enemy.monsterGrade = definition.Grade; enemy.enemySkill = definition.Skill; enemy.maxHP = basis.maxHP; enemy.attack = basis.attack; enemy.defense = basis.defense; enemy.maxMagicPower = basis.maxMagicPower; enemy.attackSpeed = basis.attackSpeed; enemy.criticalRate = basis.criticalRate; enemy.evasionRate = basis.evasionRate; enemy.goldReward = basis.goldReward; enemy.experienceMultiplier = basis.experienceMultiplier; enemy.itemDrops = basis.itemDrops; if (definition.Role == EnemyCombatRole.Skill) { enemy.maxMagicPower += 25; } if (definition.Role == EnemyCombatRole.Speed) { enemy.attackSpeed += .2f; enemy.evasionRate += .08f; } if (definition.Role == EnemyCombatRole.Durability) { enemy.maxHP += 12; enemy.defense += 3; } if (definition.Role == EnemyCombatRole.Attack) { enemy.attack += 4; } enemy.battleVisualKey = definition.BattleVisualKey; Id(enemy, definition.Id); string path = EnemyPath + "/" + definition.Id.Replace('.', '_') + ".asset"; Put(enemy, path); AddEnemyToDungeon(AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path), definition.DungeonAsset == "UpperFortress" ? "GlaadSkyFortress" : definition.DungeonAsset); }
    static void AddEnemyToDungeon(EnemyDataSO enemy, string dungeonAsset) { DungeonDataSO dungeon = Load<DungeonDataSO>("Dungeons/" + dungeonAsset); List<EnemyDataSO> enemies = new List<EnemyDataSO>(); foreach (EnemyDataSO existingEnemy in dungeon.normalEnemies) { if (existingEnemy != null) { enemies.Add(existingEnemy); } } if (!enemies.Contains(enemy)) { enemies.Add(enemy); } dungeon.normalEnemies = enemies.ToArray(); EditorUtility.SetDirty(dungeon); }
    static T Load<T>(string path) where T : UnityEngine.Object { T result = Resources.Load<T>(path); if (result == null) { throw new InvalidOperationException(path); } return result; }
    static void Id(UnityEngine.Object item, string id) { SerializedObject serialized = new SerializedObject(item); serialized.FindProperty("persistentId").stringValue = id; serialized.ApplyModifiedPropertiesWithoutUndo(); }
    static void Put(UnityEngine.Object item, string path)
    {
        if (item is EnemyDataSO enemy && path.Contains("enemy_job_wyvern_"))
        {
            // Glaad precedes Velm; preserve that regional progression after
            // adding the five role-specialised wyvern variants.
            enemy.maxHP = Mathf.RoundToInt(enemy.maxHP * 0.85f);
            enemy.attack = Mathf.RoundToInt(enemy.attack * 0.85f);
        }

        UnityEngine.Object existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(item, existing);
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(item);
            return;
        }
        AssetDatabase.CreateAsset(item, path);
    }
    static void Folders() { foreach (string path in new[] { EnemyPath, ItemPath, RecipePath }) { string[] parts = path.Split('/'); string current = parts[0]; for (int index = 1; index < parts.Length; index++) { if (!AssetDatabase.IsValidFolder(current + "/" + parts[index])) { AssetDatabase.CreateFolder(current, parts[index]); } current += "/" + parts[index]; } } }
}
