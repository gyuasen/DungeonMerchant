using System;
using System.Collections.Generic;
using System.IO;
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
        foreach (SlimeVariantDefinition slime in BalanceExpansionDefinition.SlimeVariants) { BuildSlimeVariant(slime); }
        foreach (BalanceExpansionEquipmentDefinition equipment in BalanceExpansionDefinition.Equipment) { BuildEquipment(equipment); }
        foreach (BalanceExpansionConsumableDefinition consumable in BalanceExpansionDefinition.Consumables) { BuildConsumable(consumable); }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("DungeonMerchant/Apply Combat Balance Stage 1")]
    public static void ApplyCombatBalanceStage1()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { "Assets/Proiject/Resources" });
        foreach (string guid in guids)
        {
            EnemyDataSO enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            BalanceExpansionEnemyDefinition expansion = FindExpansionDefinition(enemy);
            ApplyCombatBalance(enemy, expansion == null ? EnemyCombatRole.Balance : expansion.Role, GetRegionalMultiplier(enemy));
            EditorUtility.SetDirty(enemy);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("DungeonMerchant/Apply Combat Balance Stage 2")]
    public static void ApplyCombatBalanceStage2()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Proiject/Resources" });
        foreach (string guid in guids)
        {
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || !item.IsEquipment)
            {
                continue;
            }

            if (item.equipmentSet == EquipmentSetId.None &&
                !IsLimitedEquipment(item))
            {
                ApplyEquipmentStats(item, item.equipmentRank, item.equipmentSlot, item.allClassesCanEquip ? -1 : GetClassIndex(item.requiredClass));
            }
            else
            {
                ApplyMinimumRankStats(item);
            }
            EditorUtility.SetDirty(item);
        }
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
        ApplyEquipmentStats(item, definition.Rank, definition.Slot, definition.ClassIndex);
        Id(item, definition.Id);
        string path = ItemPath + "/" + definition.Id.Replace('.', '_') + ".asset";
        Put(item, path);
        if (definition.AcquisitionType == ItemAcquisitionType.Blacksmith) { BuildRecipe(definition, AssetDatabase.LoadAssetAtPath<ItemDataSO>(path)); }
    }
    static void ApplyEquipmentStats(ItemDataSO item, int rank, EquipmentSlot slot, int classIndex)
    {
        int safeRank = Mathf.Clamp(rank, 1, 10);
        int[] hp;
        int[] attack;
        int[] defense;
        float[] speed;
        int[] price;
        GetEquipmentTable(slot, out hp, out attack, out defense, out speed, out price);
        float hpRate;
        float attackRate;
        float defenseRate;
        float speedRate;
        GetClassRates(classIndex, out hpRate, out attackRate, out defenseRate, out speedRate);
        int index = safeRank - 1;
        item.bonusMaxHP = Mathf.RoundToInt(hp[index] * hpRate);
        item.bonusAttack = Mathf.RoundToInt(attack[index] * attackRate);
        item.bonusDefense = Mathf.RoundToInt(defense[index] * defenseRate);
        item.bonusAttackSpeed = speed[index] * speedRate;
        item.basePrice = price[index];
    }
    static void GetEquipmentTable(EquipmentSlot slot, out int[] hp, out int[] attack, out int[] defense, out float[] speed, out int[] price)
    {
        if (slot == EquipmentSlot.Weapon) { hp = new[] { 2, 4, 6, 8, 10, 12, 15, 18, 22, 28 }; attack = new[] { 4, 8, 13, 14, 16, 19, 22, 25, 28, 34 }; defense = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10 }; speed = new[] { .010f, .015f, .020f, .025f, .030f, .035f, .040f, .045f, .050f, .060f }; price = new[] { 120, 220, 400, 720, 1300, 2300, 4100, 7300, 13000, 28600 }; return; }
        if (slot == EquipmentSlot.Armor) { hp = new[] { 8, 12, 18, 30, 38, 50, 62, 76, 90, 110 }; attack = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10 }; defense = new[] { 3, 4, 7, 10, 12, 14, 16, 19, 22, 28 }; speed = new[] { .005f, .010f, .015f, .020f, .025f, .030f, .035f, .040f, .045f, .055f }; price = new[] { 130, 240, 430, 780, 1400, 2500, 4500, 8000, 14300, 31500 }; return; }
        hp = new[] { 3, 6, 9, 12, 15, 18, 22, 28, 34, 45 }; attack = new[] { 2, 3, 5, 6, 7, 8, 9, 10, 12, 14 }; defense = new[] { 1, 2, 3, 4, 5, 6, 7, 9, 11, 14 }; speed = new[] { .010f, .015f, .020f, .025f, .030f, .035f, .040f, .045f, .050f, .060f }; price = new[] { 110, 200, 360, 650, 1170, 2100, 3800, 6800, 12200, 26800 };
    }
    static void GetClassRates(int classIndex, out float hp, out float attack, out float defense, out float speed)
    {
        hp = 1f; attack = 1f; defense = 1f; speed = 1f;
        if (classIndex == 0 || classIndex == 5) { hp = 1.20f; attack = .90f; defense = 1.25f; speed = .80f; }
        if (classIndex == 1 || classIndex == 4) { hp = .85f; attack = 1f; defense = .80f; speed = 1.50f; }
        if (classIndex == 2) { hp = .75f; attack = 1.35f; defense = .75f; speed = 1f; }
        if (classIndex == 3) { hp = 1.10f; attack = .90f; defense = 1.10f; speed = .90f; }
    }
    static int GetClassIndex(MercenaryClass value) { return value == MercenaryClass.Archer ? 1 : value == MercenaryClass.Mage ? 2 : value == MercenaryClass.Priest ? 3 : value == MercenaryClass.Rogue ? 4 : value == MercenaryClass.Lancer ? 5 : 0; }
    static bool IsLimitedEquipment(ItemDataSO item) { return item.acquisitionType == ItemAcquisitionType.Dungeon; }
    static void ApplyMinimumRankStats(ItemDataSO item)
    {
        int hp; int attack; int defense; float speed; int price;
        GetRankMinimum(item.equipmentSlot, item.equipmentRank, out hp, out attack, out defense, out speed, out price);
        item.bonusMaxHP = Mathf.Max(item.bonusMaxHP, hp);
        item.bonusAttack = Mathf.Max(item.bonusAttack, attack);
        item.bonusDefense = Mathf.Max(item.bonusDefense, defense);
        item.bonusAttackSpeed = Mathf.Max(item.bonusAttackSpeed, speed);
        item.basePrice = Mathf.Max(item.basePrice, price);
    }
    static void GetRankMinimum(EquipmentSlot slot, int rank, out int hp, out int attack, out int defense, out float speed, out int price)
    {
        int[] hpTable; int[] attackTable; int[] defenseTable; float[] speedTable; int[] priceTable;
        GetEquipmentTable(slot, out hpTable, out attackTable, out defenseTable, out speedTable, out priceTable);
        int index = Mathf.Clamp(rank, 1, 10) - 1;
        hp = hpTable[index]; attack = attackTable[index]; defense = defenseTable[index]; speed = speedTable[index]; price = priceTable[index];
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
        item.description = definition.Description;
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
        enemy.name = Path.GetFileNameWithoutExtension(path);
        enemy.enemyName = definition.EnglishName;
        enemy.monsterGrade = definition.Grade;
        enemy.battleVisualKey = definition.BattleVisualKey;
        ApplyCombatBalance(enemy, EnemyCombatRole.Balance, GetRegionalMultiplier(definition.DungeonAsset));
        Id(enemy, definition.Id);
        AddEnemyToDungeon(enemy, definition.DungeonAsset);
        EditorUtility.SetDirty(enemy);
    }
    static void BuildEnemy(BalanceExpansionEnemyDefinition definition)
    {
        EnemyDataSO basis = Load<EnemyDataSO>("GameData/Enemies/" + definition.BaseEnemyAsset);
        EnemyDataSO dropSource = Load<EnemyDataSO>("GameData/Enemies/" + definition.DropSourceAsset);
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.enemyName = definition.EnglishName;
        enemy.monsterGrade = definition.Grade;
        enemy.enemySkill = definition.Skill;
        enemy.race = definition.Race;
        enemy.maxMagicPower = basis.maxMagicPower;
        enemy.attackSpeed = basis.attackSpeed;
        enemy.criticalRate = basis.criticalRate;
        enemy.evasionRate = basis.evasionRate;
        enemy.itemDrops = dropSource.itemDrops;
        enemy.battleVisualKey = definition.BattleVisualKey;
        ApplyCombatBalance(enemy, definition.Role, GetRegionalMultiplier(definition.DungeonAsset));
        Id(enemy, definition.Id);
        string path = EnemyPath + "/" + definition.Id.Replace('.', '_') + ".asset";
        Put(enemy, path);
        AddEnemyToDungeon(AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path), definition.DungeonAsset == "UpperFortress" ? "GlaadSkyFortress" : definition.DungeonAsset);
    }
    static void BuildSlimeVariant(SlimeVariantDefinition definition)
    {
        EnemyDataSO template = Load<EnemyDataSO>("GameData/Enemies/" + definition.TemplateAsset);
        ItemDataSO slimeMucus = Load<ItemDataSO>("GameData/Items/SlimeMucus");
        EnemyDataSO enemy = ScriptableObject.CreateInstance<EnemyDataSO>();
        enemy.enemyName = definition.EnglishName;
        enemy.monsterGrade = definition.Grade;
        enemy.enemySkill = definition.Skill;
        enemy.race = EnemyRace.Slime;
        enemy.maxMagicPower = template.maxMagicPower;
        enemy.attackSpeed = template.attackSpeed;
        enemy.criticalRate = template.criticalRate;
        enemy.evasionRate = template.evasionRate;
        enemy.itemDrops = new[] { new ItemDropEntry { item = slimeMucus, amount = 1, dropChance = 1f } };
        enemy.battleVisualKey = definition.BattleVisualKey;
        ApplyCombatBalance(enemy, definition.Role, GetRegionalMultiplier(definition.DungeonAsset));
        Id(enemy, definition.Id);
        string path = EnemyPath + "/" + definition.Id.Replace('.', '_') + ".asset";
        Put(enemy, path);
        AddEnemyToDungeon(AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path), definition.DungeonAsset);
    }
    static void AddEnemyToDungeon(EnemyDataSO enemy, string dungeonAsset) { DungeonDataSO dungeon = LoadDungeon(dungeonAsset); List<EnemyDataSO> enemies = new List<EnemyDataSO>(); foreach (EnemyDataSO existingEnemy in dungeon.normalEnemies) { if (existingEnemy != null) { enemies.Add(existingEnemy); } } if (!enemies.Contains(enemy)) { enemies.Add(enemy); } dungeon.normalEnemies = enemies.ToArray(); EditorUtility.SetDirty(dungeon); }
    static DungeonDataSO LoadDungeon(string dungeonAsset) { DungeonDataSO dungeon = Resources.Load<DungeonDataSO>("Dungeons/" + dungeonAsset); if (dungeon != null) { return dungeon; } dungeon = Resources.Load<DungeonDataSO>("GameData/Dungeons/" + dungeonAsset); if (dungeon != null) { return dungeon; } throw new InvalidOperationException(dungeonAsset); }
    static T Load<T>(string path) where T : UnityEngine.Object { T result = Resources.Load<T>(path); if (result == null) { throw new InvalidOperationException(path); } return result; }
    static void Id(UnityEngine.Object item, string id) { SerializedObject serialized = new SerializedObject(item); serialized.FindProperty("persistentId").stringValue = id; serialized.ApplyModifiedPropertiesWithoutUndo(); }
    static void Put(UnityEngine.Object item, string path)
    {
        string assetName = Path.GetFileNameWithoutExtension(path);
        item.name = assetName;
        UnityEngine.Object existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(item, existing);
            existing.name = assetName;
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(item);
            return;
        }
        AssetDatabase.CreateAsset(item, path);
    }
    static void Folders() { foreach (string path in new[] { EnemyPath, ItemPath, RecipePath }) { string[] parts = path.Split('/'); string current = parts[0]; for (int index = 1; index < parts.Length; index++) { if (!AssetDatabase.IsValidFolder(current + "/" + parts[index])) { AssetDatabase.CreateFolder(current, parts[index]); } current += "/" + parts[index]; } } }
    static void ApplyCombatBalance(EnemyDataSO enemy, EnemyCombatRole role, float regionalMultiplier)
    {
        int[] hitPoints = { 2400, 1350, 760, 430, 250, 150, 95, 60, 38, 24 };
        int[] attacks = { 160, 115, 80, 54, 36, 24, 15, 10, 7, 5 };
        int[] defenses = { 105, 70, 45, 29, 18, 11, 7, 4, 2, 1 };
        int index = Mathf.Clamp(enemy.monsterGrade, 1, 10) - 1;
        float healthMultiplier = regionalMultiplier;
        float attackMultiplier = regionalMultiplier;
        float defenseMultiplier = regionalMultiplier;
        float speedMultiplier = 1f;
        if (enemy.isBoss)
        {
            healthMultiplier *= 4f;
            attackMultiplier *= 1.35f;
            defenseMultiplier *= 1.20f;
            speedMultiplier *= 1.05f;
        }
        ApplyRoleMultiplier(role, ref healthMultiplier, ref attackMultiplier, ref defenseMultiplier, ref speedMultiplier);
        enemy.maxHP = Mathf.RoundToInt(hitPoints[index] * healthMultiplier);
        enemy.attack = Mathf.RoundToInt(attacks[index] * attackMultiplier);
        enemy.defense = Mathf.RoundToInt(defenses[index] * defenseMultiplier);
        if (!enemy.combatBalanceStage1Applied)
        {
            enemy.attackSpeed *= speedMultiplier;
            enemy.combatBalanceStage1Applied = true;
        }
        enemy.goldReward = CalculateGoldReward(enemy.monsterGrade);
    }
    static void ApplyRoleMultiplier(EnemyCombatRole role, ref float health, ref float attack, ref float defense, ref float speed)
    {
        if (role == EnemyCombatRole.Attack) { health *= .90f; attack *= 1.15f; defense *= .90f; speed *= 1.05f; }
        if (role == EnemyCombatRole.Durability) { health *= 1.35f; attack *= .90f; defense *= 1.20f; speed *= .90f; }
        if (role == EnemyCombatRole.Speed) { health *= .85f; attack *= .95f; defense *= .90f; speed *= 1.20f; }
        if (role == EnemyCombatRole.Skill) { attack *= 1.05f; }
    }
    static int CalculateGoldReward(int grade) { return Mathf.RoundToInt(50f * Mathf.Pow(25f, (10 - Mathf.Clamp(grade, 1, 10)) / 9f)); }
    static BalanceExpansionEnemyDefinition FindExpansionDefinition(EnemyDataSO enemy)
    {
        foreach (BalanceExpansionEnemyDefinition definition in BalanceExpansionDefinition.Enemies) { if (enemy.PersistentId == definition.Id) { return definition; } }
        return null;
    }
    static float GetRegionalMultiplier(EnemyDataSO enemy)
    {
        string[] dungeonGuids = AssetDatabase.FindAssets("t:DungeonDataSO", new[] { "Assets/Proiject/Resources/Dungeons" });
        foreach (string guid in dungeonGuids)
        {
            DungeonDataSO dungeon = AssetDatabase.LoadAssetAtPath<DungeonDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (dungeon != null && (Contains(dungeon.normalEnemies, enemy) || dungeon.bossEnemy == enemy)) { return GetRegionalMultiplier(dungeon.name); }
        }
        return 1f;
    }
    static bool Contains(EnemyDataSO[] enemies, EnemyDataSO target) { foreach (EnemyDataSO enemy in enemies) { if (enemy == target) { return true; } } return false; }
    static float GetRegionalMultiplier(string dungeonName) { if (dungeonName == "GlaadSkyFortress" || dungeonName == "UpperFortress") { return .97f; } if (dungeonName == "VelmBlackIronMine") { return 1.05f; } return 1f; }
}

public static class EnemyRaceAssetAssigner
{
    private static readonly HashSet<string> Slimes = new HashSet<string>
    {
        "EnemyData", "Grade10BlueSlime", "Grade10GreenSlime", "Grade10MossSlime"
    };
    private static readonly HashSet<string> Undead = new HashSet<string>
    {
        "Grade07Zombie", "Grade07Wraith", "Grade07Skeleton", "Grade07BoneHound", "Grade07ArmoredSkeleton",
        "enemy_job_skeleton_archer", "enemy_job_skeleton_hexer", "enemy_job_skeleton_guard", "enemy_job_skeleton_reaper", "enemy_job_skeleton_captain"
    };
    private static readonly HashSet<string> Beasts = new HashSet<string>
    {
        "Grade10HornRabbit", "Grade09WildDog", "Grade09Kobold", "Grade08VenomMoth", "Grade08RockBeetle", "Grade08GiantRat", "Grade08CaveSpider", "Grade08CaveBat", "Grade06MarshLizard",
        "Grade04GlaadGaleHarpy", "MythicalGrade05ThunderhornKirin", "MythicalGrade03FlamewingGryphon", "MythicalGrade07MistfangWolf"
    };
    private static readonly HashSet<string> Dragons = new HashSet<string>
    {
        "Grade03Wyvern", "Grade06Lizardman", "Grade01AbyssDragon", "Grade03GlaadFrostDrake", "VelmMagmaDrake", "MythicalGrade01AstralDragon",
        "enemy_job_wyvern_hexer", "enemy_job_wyvern_skyrider", "enemy_job_wyvern_ironwing", "enemy_job_wyvern_ravager", "enemy_job_wyvern_captain"
    };
    private static readonly HashSet<string> Constructs = new HashSet<string>
    {
        "Grade05StoneGolem", "Grade05IronGolem", "Boss04RuinGuardian", "VelmEmberforgedAutomaton", "Grade01AstralSentinel"
    };
    private static readonly HashSet<string> Demons = new HashSet<string>
    {
        "Grade02DemonKnight", "Grade01AstralReaver", "Grade01AstralOracle", "Boss01AbyssLord", "Boss01CelestialJudge"
    };

    [MenuItem("Tools/DungeonMerchant/Enemy Race/Assign Missing Races")]
    public static void AssignMissingRaces()
    {
        int assignedCount = 0;
        string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { "Assets/Proiject/Resources" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyDataSO enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (enemy == null || enemy.race != EnemyRace.Unknown)
            {
                continue;
            }
            Undo.RecordObject(enemy, "Assign Enemy Race");
            enemy.race = GetRace(enemy.name);
            EditorUtility.SetDirty(enemy);
            assignedCount++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Assigned enemy races to " + assignedCount + " assets.");
    }

    private static EnemyRace GetRace(string assetName)
    {
        if (Slimes.Contains(assetName))
        {
            return EnemyRace.Slime;
        }
        if (Undead.Contains(assetName))
        {
            return EnemyRace.Undead;
        }
        if (Beasts.Contains(assetName))
        {
            return EnemyRace.Beast;
        }
        if (Dragons.Contains(assetName))
        {
            return EnemyRace.Dragon;
        }
        if (Constructs.Contains(assetName))
        {
            return EnemyRace.Construct;
        }
        if (Demons.Contains(assetName))
        {
            return EnemyRace.Demon;
        }
        return EnemyRace.Humanoid;
    }
}
