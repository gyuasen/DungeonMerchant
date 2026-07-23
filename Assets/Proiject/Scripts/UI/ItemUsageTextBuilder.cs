using System.Collections.Generic;
using System.Linq;

public static class ItemUsageTextBuilder
{
    private const int MaximumLines = 8;
    private const int MaximumNamesPerCategory = 3;
    private static Dictionary<ItemDataSO, List<EquipmentRecipeSO>> recipesByMaterial;
    private static Dictionary<ItemDataSO, List<EnemyDataSO>> enemiesByDropItem;
    private static Dictionary<ItemDataSO, List<DungeonDataSO>> dungeonsByClearReward;

    public static string BuildAcquisitionText(ItemDataSO item)
    {
        if (item == null)
        {
            return "入手経路不明";
        }
        EnsureUsageCaches();
        List<string> sources = new List<string>();
        if (enemiesByDropItem.TryGetValue(item, out List<EnemyDataSO> enemies))
        {
            foreach (EnemyDataSO enemy in enemies)
            {
                string name = GetJapaneseEnemyNameOrEmpty(enemy);
                if (!string.IsNullOrEmpty(name))
                {
                    sources.Add(name + "がドロップ");
                }
            }
        }
        if (dungeonsByClearReward.TryGetValue(item, out List<DungeonDataSO> dungeons))
        {
            foreach (DungeonDataSO dungeon in dungeons)
            {
                string name = dungeon.dungeonName;
                if (ContainsJapaneseCharacter(name))
                {
                    sources.Add(name + "のクリア報酬");
                }
            }
        }
        if (item.acquisitionType == ItemAcquisitionType.Market)
        {
            sources.Add("市場で購入可");
        }
        return sources.Count > 0
            ? string.Join(" / ", sources.Distinct().Take(MaximumNamesPerCategory))
            : "入手経路不明";
    }

    public static string Build(ItemDataSO item)
    {
        return Build(item, TryGetCurrentTownIndex());
    }

    public static string Build(ItemDataSO item, int? currentTownIndex)
    {
        if (item == null)
        {
            return string.Empty;
        }

        List<string> lines = new List<string>();
        if (item.IsEquipment)
        {
            AddLine(lines, "装備可能: " + JapaneseDisplayText.GetMercenaryClass(item.requiredClass));
            AddLine(lines, "装備部位: " + JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot));
        }
        else if (item.itemType == ItemType.Consumable)
        {
            AddLine(lines, "使用効果: " + BuildConsumableEffectText(item));
        }
        else if (TryGetEnhancementRange(item, out string range))
        {
            AddLine(lines, "装備強化 " + range + " に使用");
        }
        else if (item.materialClassification == MaterialClassification.SellOnly)
        {
            AddLine(lines, "売却専用（店で売ってお金にする）");
        }

        EnsureUsageCaches();
        if (item.materialClassification == MaterialClassification.CraftingMaterial)
        {
            AddCraftedItemLines(lines, item, currentTownIndex);
        }
        AddDropEnemyLines(lines, item);
        if (lines.Count == 0)
        {
            AddLine(lines, "特別な用途はありません");
        }
        return string.Join("\n", lines);
    }

    private static string BuildConsumableEffectText(ItemDataSO item)
    {
        switch (item.consumableEffect)
        {
            case ConsumableEffectType.HealHP:
                return "HPを" + item.consumableHealAmount + "回復";
            case ConsumableEffectType.RestoreMagic:
                return "魔力を" + item.consumableHealAmount + "回復";
            case ConsumableEffectType.CurePoison:
                return "毒を治療";
            case ConsumableEffectType.CureParalysis:
                return "麻痺を治療";
            case ConsumableEffectType.CureAllStatus:
                return "状態異常を治療";
            case ConsumableEffectType.BoostAttack:
                return "戦闘中の攻撃力を上昇";
            case ConsumableEffectType.BoostDefense:
                return "戦闘中の防御力を上昇";
            case ConsumableEffectType.BoostSpeed:
                return "戦闘中の速度を上昇";
            default:
                return "使用効果なし";
        }
    }

    private static void AddCraftedItemLines(
        List<string> lines,
        ItemDataSO item,
        int? currentTownIndex)
    {
        if (!recipesByMaterial.TryGetValue(item, out List<EquipmentRecipeSO> recipes))
        {
            return;
        }

        if (!currentTownIndex.HasValue)
        {
            AddNamedLines(lines, "制作に使用", GetJapaneseResultItemNames(recipes));
            return;
        }

        List<EquipmentRecipeSO> availableRecipes = recipes
            .Where(recipe => BlacksmithManager.IsRecipeAvailableInTown(
                recipe,
                currentTownIndex.Value))
            .ToList();
        if (availableRecipes.Count == 0)
        {
            AddNamedLines(lines, "制作に使用", GetJapaneseResultItemNames(recipes));
            return;
        }

        AddNamedLines(
            lines,
            "制作に使用（この町で作れる）",
            GetJapaneseResultItemNames(availableRecipes));
        List<EquipmentRecipeSO> otherRecipes = recipes
            .Where(recipe => !availableRecipes.Contains(recipe))
            .ToList();
        AddNamedLines(
            lines,
            "制作に使用（他の町・上位）",
            GetJapaneseResultItemNames(otherRecipes));
    }

    private static List<string> GetJapaneseResultItemNames(
        IEnumerable<EquipmentRecipeSO> recipes)
    {
        return recipes
            .Where(recipe => recipe?.resultItem != null)
            .Select(recipe => GetJapaneseItemNameOrEmpty(recipe.resultItem))
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();
    }

    private static void AddDropEnemyLines(List<string> lines, ItemDataSO item)
    {
        if (!enemiesByDropItem.TryGetValue(item, out List<EnemyDataSO> enemies))
        {
            return;
        }

        List<string> names = enemies
            .Select(GetJapaneseEnemyNameOrEmpty)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();
        AddNamedLines(lines, "ドロップ元", names);
    }

    private static void AddNamedLines(List<string> lines, string heading, List<string> names)
    {
        if (names.Count == 0 || lines.Count >= MaximumLines)
        {
            return;
        }

        AddLine(lines, heading);
        int displayedCount = 0;
        while (displayedCount < names.Count && displayedCount < MaximumNamesPerCategory && lines.Count < MaximumLines)
        {
            AddLine(lines, names[displayedCount]);
            displayedCount++;
        }
        int omittedCount = names.Count - displayedCount;
        if (omittedCount > 0 && lines.Count < MaximumLines)
        {
            AddLine(lines, "他" + omittedCount + "件");
        }
    }

    private static void AddLine(List<string> lines, string text)
    {
        if (lines.Count < MaximumLines && !string.IsNullOrWhiteSpace(text))
        {
            lines.Add("・" + text);
        }
    }

    private static string GetJapaneseItemNameOrEmpty(ItemDataSO item)
    {
        string displayName = JapaneseDisplayText.GetItemName(item);
        return ContainsJapaneseCharacter(displayName) ? displayName : string.Empty;
    }

    private static string GetJapaneseEnemyNameOrEmpty(EnemyDataSO enemy)
    {
        if (enemy == null)
        {
            return string.Empty;
        }

        string displayName = JapaneseDisplayText.GetEnemyName(enemy.enemyName);
        return ContainsJapaneseCharacter(displayName) ? displayName : string.Empty;
    }

    private static bool ContainsJapaneseCharacter(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if ((character >= '\u3040' && character <= '\u30ff') ||
                (character >= '\u3400' && character <= '\u9fff') ||
                (character >= '\uff66' && character <= '\uff9f'))
            {
                return true;
            }
        }
        return false;
    }

    private static bool TryGetEnhancementRange(ItemDataSO item, out string range)
    {
        switch (item.PersistentId)
        {
            case "item.EnhancementOre": range = "+1〜+2"; return true;
            case "item.LowerGradeEnhancementOre": range = "+3〜+4"; return true;
            case "item.MiddleGradeEnhancementOre": range = "+5〜+6"; return true;
            case "item.UpperGradeEnhancementOre": range = "+7〜+8"; return true;
            case "item.HighestGradeEnhancementOre": range = "+9〜+10"; return true;
            default: range = string.Empty; return false;
        }
    }

    private static void EnsureUsageCaches()
    {
        if (recipesByMaterial != null && enemiesByDropItem != null &&
            dungeonsByClearReward != null)
        {
            return;
        }

        recipesByMaterial = new Dictionary<ItemDataSO, List<EquipmentRecipeSO>>();
        enemiesByDropItem = new Dictionary<ItemDataSO, List<EnemyDataSO>>();
        dungeonsByClearReward = new Dictionary<ItemDataSO, List<DungeonDataSO>>();
        foreach (EquipmentRecipeSO recipe in GameAssetRepository.LoadAll<EquipmentRecipeSO>())
        {
            if (recipe?.materials == null || recipe.resultItem == null)
            {
                continue;
            }

            foreach (CraftingMaterialRequirement requirement in recipe.materials)
            {
                if (requirement?.item == null)
                {
                    continue;
                }

                if (!recipesByMaterial.TryGetValue(requirement.item, out List<EquipmentRecipeSO> recipes))
                {
                    recipes = new List<EquipmentRecipeSO>();
                    recipesByMaterial.Add(requirement.item, recipes);
                }
                if (!recipes.Contains(recipe))
                {
                    recipes.Add(recipe);
                }
            }
        }

        foreach (EnemyDataSO enemy in GameAssetRepository.LoadAll<EnemyDataSO>())
        {
            if (enemy?.itemDrops == null)
            {
                continue;
            }

            foreach (ItemDropEntry drop in enemy.itemDrops)
            {
                if (drop?.item == null)
                {
                    continue;
                }

                if (!enemiesByDropItem.TryGetValue(drop.item, out List<EnemyDataSO> enemies))
                {
                    enemies = new List<EnemyDataSO>();
                    enemiesByDropItem.Add(drop.item, enemies);
                }
                if (!enemies.Contains(enemy))
                {
                    enemies.Add(enemy);
                }
            }
        }

        foreach (DungeonDataSO dungeon in GameAssetRepository.LoadAll<DungeonDataSO>())
        {
            if (dungeon?.clearItemRewards == null)
            {
                continue;
            }
            foreach (DungeonItemReward reward in dungeon.clearItemRewards)
            {
                if (reward?.item == null)
                {
                    continue;
                }
                if (!dungeonsByClearReward.TryGetValue(reward.item, out List<DungeonDataSO> dungeons))
                {
                    dungeons = new List<DungeonDataSO>();
                    dungeonsByClearReward.Add(reward.item, dungeons);
                }
                if (!dungeons.Contains(dungeon))
                {
                    dungeons.Add(dungeon);
                }
            }
        }
    }

    private static int? TryGetCurrentTownIndex()
    {
        TownProgressState townProgressState = UnityEngine.Object.FindObjectOfType<TownProgressState>();
        return townProgressState != null &&
               WorldMapService.IsValidTownIndex(townProgressState.CurrentTownIndex)
            ? townProgressState.CurrentTownIndex
            : null;
    }
}
