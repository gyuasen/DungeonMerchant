using System;
using System.Collections.Generic;

public enum EnemyCombatRole { Skill, Speed, Durability, Attack, Balance }
public sealed class BalanceExpansionEnemyDefinition
{
    public readonly string Id, EnglishName, JapaneseName, BaseEnemyAsset, DropSourceAsset, DungeonAsset, BattleVisualKey;
    public readonly int Grade;
    public readonly EnemySkillType Skill;
    public readonly EnemyCombatRole Role;
    public readonly EnemyRace Race;
    public BalanceExpansionEnemyDefinition(string id, string en, string ja, int grade, string basis, string dropSource, string dungeon, EnemySkillType skill, EnemyCombatRole role, EnemyRace race, string visual) { Id = id; EnglishName = en; JapaneseName = ja; Grade = grade; BaseEnemyAsset = basis; DropSourceAsset = dropSource; DungeonAsset = dungeon; Skill = skill; Role = role; Race = race; BattleVisualKey = visual; }
}
public sealed class BalanceExpansionNormalEnemyDefinition
{
    public readonly string Id, EnglishName, JapaneseName, AssetName, DungeonAsset, BattleVisualKey;
    public readonly int Grade;
    public BalanceExpansionNormalEnemyDefinition(string id, string en, string ja, int grade, string assetName, string dungeonAsset, string battleVisualKey) { Id = id; EnglishName = en; JapaneseName = ja; Grade = grade; AssetName = assetName; DungeonAsset = dungeonAsset; BattleVisualKey = battleVisualKey; }
}
public sealed class SlimeVariantDefinition
{
    public readonly string Id, EnglishName, JapaneseName, TemplateAsset, DropSourceAsset, DungeonAsset, BattleVisualKey;
    public readonly int Grade;
    public readonly EnemySkillType Skill;
    public readonly EnemyCombatRole Role;
    public readonly EnemyRace Race = EnemyRace.Slime;
    public SlimeVariantDefinition(string id, string en, string ja, int grade, string templateAsset, string dropSourceAsset, string dungeonAsset, EnemySkillType skill, EnemyCombatRole role) { Id = id; EnglishName = en; JapaneseName = ja; Grade = grade; TemplateAsset = templateAsset; DropSourceAsset = dropSourceAsset; DungeonAsset = dungeonAsset; Skill = skill; Role = role; BattleVisualKey = "Grade" + grade.ToString("00") + "_" + id.Replace("enemy.slime.", string.Empty); }
}
public sealed class BalanceExpansionEquipmentDefinition
{
    public readonly string Id, EnglishName, JapaneseName;
    public readonly int Rank, ClassIndex;
    public readonly EquipmentSlot Slot;
    public readonly ItemAcquisitionType AcquisitionType;
    public readonly EnemyRace TargetRace;
    public readonly float RaceDamageBonus;
    public BalanceExpansionEquipmentDefinition(string id, string en, string ja, int rank, int classIndex, EquipmentSlot slot, ItemAcquisitionType acquisitionType, EnemyRace targetRace = EnemyRace.Unknown, float raceDamageBonus = 0f) { Id = id; EnglishName = en; JapaneseName = ja; Rank = rank; ClassIndex = classIndex; Slot = slot; AcquisitionType = acquisitionType; TargetRace = targetRace; RaceDamageBonus = raceDamageBonus; }
}
public sealed class ExistingEquipmentEffectAssignment
{
    public readonly string ResourcePath;
    public readonly EquipmentEffectDefinition EquipmentEffectDefinition;
    public ExistingEquipmentEffectAssignment(string resourcePath, EquipmentEffectDefinition equipmentEffectDefinition) { ResourcePath = resourcePath; EquipmentEffectDefinition = equipmentEffectDefinition; }
}
public sealed class BalanceExpansionConsumableDefinition
{
    public readonly string Id, EnglishName, JapaneseName, Description;
    public readonly ConsumableEffectType Effect;
    public readonly int Price, Amount;
    public BalanceExpansionConsumableDefinition(
        string id,
        string en,
        string ja,
        string description,
        ConsumableEffectType effect,
        int price,
        int amount = 0)
    {
        Id = id;
        EnglishName = en;
        JapaneseName = ja;
        Description = description;
        Effect = effect;
        Price = price;
        Amount = amount;
    }
}
public static class BalanceExpansionDefinition
{
    // Stage 5 enemies are authored directly as assets because their boss state and
    // dungeon references must remain stable even when the batch generator is not run.
    // Keep their display metadata here so every UI uses the same Japanese-name path.
    public static readonly IReadOnlyList<BalanceExpansionEnemyDefinition> DungeonReorgEnemies = new[]
    {
        new BalanceExpansionEnemyDefinition("enemy.abyss_spawn", "Abyss Spawn", "奈落の眷属", 4, "Grade02DemonKnight", "Grade04DarkMage", "AbyssGatewayThreshold", EnemySkillType.HexBolt, EnemyCombatRole.Skill, EnemyRace.Demon, "Grade04_abyss_spawn"),
        new BalanceExpansionEnemyDefinition("enemy.boss.norn_verdant_orc_high_chieftain", "Norn Verdant Orc High Chieftain", "翠樹の大族長", 4, "Grade06Orc", "Grade06Orc", "NornVerdantSettlement", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack, EnemyRace.Humanoid, "Grade04_norn_verdant_orc_high_chieftain"),
        new BalanceExpansionEnemyDefinition("enemy.boss.glaad_dragon_scale_king", "Glaad Dragon Scale King", "竜鱗王", 4, "Grade06Lizardman", "Grade03Wyvern", "GlaadDragonScaleCanyon", EnemySkillType.ArmorPierce, EnemyCombatRole.Balance, EnemyRace.Dragon, "Grade04_dragonscale_king"),
        new BalanceExpansionEnemyDefinition("enemy.boss.velm_grand_furnace_colossus", "Velm Grand Furnace Colossus", "大熔炉巨像", 3, "Grade05IronGolem", "Grade05IronGolem", "VelmFurnaceDefenseZone", EnemySkillType.Reconstitute, EnemyCombatRole.Durability, EnemyRace.Construct, "Grade03_grand_furnace_colossus"),
        new BalanceExpansionEnemyDefinition("enemy.boss.abyss_gatekeeper", "Abyss Gatekeeper", "奈落の門衛", 3, "Grade02DemonKnight", "Grade02DemonKnight", "AbyssGatewayThreshold", EnemySkillType.MeteorRain, EnemyCombatRole.Skill, EnemyRace.Demon, "Grade03_abyss_gatekeeper"),
        new BalanceExpansionEnemyDefinition("enemy.boss.eld_old_quarry_gravelord", "Eld Old Quarry Gravelord", "旧採石場の骸王", 6, "Grade07Skeleton", "Grade07Skeleton", "EldOldQuarry", EnemySkillType.SoulBurst, EnemyCombatRole.Durability, EnemyRace.Undead, "Grade06_eld_quarry_gravelord")
    };

    public static readonly IReadOnlyList<ExistingEquipmentEffectAssignment> ExistingEquipmentEffects = new[]
    {
        X("GameData/Items/ChampionEmblem", EquipmentEffectType.BattleStartAttackBuff, 0.15f, 0f, 3),
        X("GameData/Items/IronVanguardArmor", EquipmentEffectType.BattleStartDefenseBuff, 0.15f, 0f, 3),
        X("GameData/Items/NornVerdantCharm", EquipmentEffectType.TurnRegeneration, 0.025f),
        X("GameData/Items/AstralCore", EquipmentEffectType.TurnRegeneration, 0.025f),
        X("GameData/Items/GolemPlate", EquipmentEffectType.DamageReduction, 0.10f),
        X("GameData/Items/AstralAegis", EquipmentEffectType.DamageReduction, 0.15f),
        X("GameData/Items/OniHunterCleaver", EquipmentEffectType.LowHpDamageBonus, 0.20f, 0.30f),
        X("GameData/Items/AbyssFang", EquipmentEffectType.LowHpDamageBonus, 0.20f, 0.30f)
    };
    public static readonly IReadOnlyList<BalanceExpansionNormalEnemyDefinition> NormalEnemies = new[] { N("wyvern", "Wyvern", "ワイバーン", 3, "Grade03Wyvern", "GlaadSkyFortress", "Grade03_wyvern") };
    public static readonly IReadOnlyList<SlimeVariantDefinition> SlimeVariants = new[]
    {
        S("slime_acid", "Acid Slime", "酸液スライム", 9, "EldUndergroundWaterway", EnemySkillType.VenomStrike, EnemyCombatRole.Attack),
        S("slime_venom", "Venom Slime", "猛毒スライム", 8, "LowerMine", EnemySkillType.ToxicCloud, EnemyCombatRole.Skill),
        S("slime_stone", "Stone Slime", "岩殻スライム", 7, "EldOldQuarry", EnemySkillType.Ironhide, EnemyCombatRole.Durability),
        S("slime_quicksilver", "Quicksilver Slime", "水銀スライム", 6, "GlaadDragonScaleCanyon", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed),
        S("slime_verdant", "Verdant Slime", "翠樹スライム", 5, "NornVerdantSettlement", EnemySkillType.Regeneration, EnemyCombatRole.Balance),
        S("slime_thunder", "Thunder Slime", "雷光スライム", 4, "NornCanopyLabyrinth", EnemySkillType.ParalyzingRoar, EnemyCombatRole.Speed),
        S("slime_frost_crystal", "Frost Crystal Slime", "霜晶スライム", 3, "GlaadSkyFortress", EnemySkillType.FrostBite, EnemyCombatRole.Durability),
        S("slime_magma", "Magma Slime", "溶鉄スライム", 2, "VelmBlackIronMine", EnemySkillType.FlameBreath, EnemyCombatRole.Attack),
        S("slime_astral", "Astral Slime", "星核スライム", 1, "AstralDepths", EnemySkillType.MeteorRain, EnemyCombatRole.Skill)
    };
    public static readonly IReadOnlyList<BalanceExpansionEnemyDefinition> Enemies = new[] { E("goblin_assassin", "Goblin Assassin", "ゴブリンアサシン", 9, "Grade08GiantRat", "EldUndergroundWaterway", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed), E("goblin_magician", "Goblin Magician", "ゴブリンマジシャン", 9, "Grade08CaveSpider", "EldUndergroundWaterway", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("goblin_knight", "Goblin Knight", "ゴブリンナイト", 9, "Grade08RockBeetle", "EldUndergroundWaterway", EnemySkillType.Ironhide, EnemyCombatRole.Durability), E("goblin_raider", "Goblin Raider", "ゴブリンレイダー", 9, "Grade08GiantRat", "EldUndergroundWaterway", EnemySkillType.CleavingRush, EnemyCombatRole.Attack), E("goblin_veteran", "Goblin Veteran", "ゴブリンベテラン", 9, "Grade08CaveBat", "EldUndergroundWaterway", EnemySkillType.DoubleStrike, EnemyCombatRole.Balance), E("skeleton_archer", "Skeleton Archer", "スケルトンアーチャー", 7, "Grade06MarshLizard", "EldOldQuarry", EnemySkillType.PiercingShot, EnemyCombatRole.Speed), E("skeleton_hexer", "Skeleton Hexer", "スケルトン呪術師", 7, "Grade06Lizardman", "EldOldQuarry", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("skeleton_guard", "Skeleton Guard", "スケルトンガード", 7, "Grade06Troll", "EldOldQuarry", EnemySkillType.Ironhide, EnemyCombatRole.Durability), E("skeleton_reaper", "Skeleton Reaper", "スケルトンリーパー", 7, "Grade06Hobgoblin", "EldOldQuarry", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack), E("skeleton_captain", "Skeleton Captain", "スケルトン隊長", 7, "Grade06Orc", "EldOldQuarry", EnemySkillType.PowerStrike, EnemyCombatRole.Balance), E("orc_shaman", "Orc Shaman", "オークシャーマン", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("orc_rider", "Orc Rider", "オークライダー", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed), E("orc_bulwark", "Orc Bulwark", "オークブルワーク", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.Ironhide, EnemyCombatRole.Durability), E("orc_berserker", "Orc Berserker", "オークバーサーカー", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack), E("orc_veteran", "Orc Veteran", "オーク古参兵", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.CleavingRush, EnemyCombatRole.Balance), E("wyvern_hexer", "Wyvern Hexwing", "呪翼のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("wyvern_skyrider", "Wyvern Galewing", "疾風翼のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed), E("wyvern_ironwing", "Wyvern Ironwing", "ワイバーン鉄翼", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.Reconstitute, EnemyCombatRole.Durability), E("wyvern_ravager", "Wyvern Razortalon", "猛爪のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack), E("wyvern_captain", "Wyvern Packlord", "群れ長のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.CleavingRush, EnemyCombatRole.Balance) };
    static BalanceExpansionDefinition()
    {
        var enemies = new List<BalanceExpansionEnemyDefinition>(Enemies);
        enemies.Add(E("kobold_hexer", "Kobold Hexer", "コボルト呪術師", 9, "Grade08CaveSpider", "Grade09Kobold", "LeafForestTrail", EnemySkillType.HexBolt, EnemyCombatRole.Skill, EnemyRace.Beast));
        enemies.Add(E("kobold_prowler", "Kobold Prowler", "コボルト追跡者", 9, "Grade08GiantRat", "Grade09Kobold", "LeafForestTrail", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed, EnemyRace.Beast));
        enemies.Add(E("kobold_bulwark", "Kobold Bulwark", "コボルト堅盾兵", 9, "Grade08RockBeetle", "Grade09Kobold", "LeafForestTrail", EnemySkillType.Ironhide, EnemyCombatRole.Durability, EnemyRace.Beast));
        enemies.Add(E("kobold_ravager", "Kobold Ravager", "コボルト襲撃者", 9, "Grade08GiantRat", "Grade09Kobold", "LeafForestTrail", EnemySkillType.CleavingRush, EnemyCombatRole.Attack, EnemyRace.Beast));
        enemies.Add(E("kobold_packleader", "Kobold Packleader", "コボルト群れ長", 9, "Grade08CaveBat", "Grade09Kobold", "LeafForestTrail", EnemySkillType.DoubleStrike, EnemyCombatRole.Balance, EnemyRace.Beast));
        enemies.Add(E("lizardman_shaman", "Lizardman Shaman", "リザードマン祈祷師", 6, "Grade05OgreMage", "Grade06Lizardman", "GlaadDragonScaleCanyon", EnemySkillType.HexBolt, EnemyCombatRole.Skill, EnemyRace.Dragon));
        enemies.Add(E("lizardman_stalker", "Lizardman Stalker", "リザードマン追跡兵", 6, "Grade05OgreMage", "Grade06Lizardman", "GlaadDragonScaleCanyon", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed, EnemyRace.Dragon));
        enemies.Add(E("lizardman_scaleguard", "Lizardman Scaleguard", "リザードマン鱗衛兵", 6, "Grade05IronGolem", "Grade06Lizardman", "GlaadDragonScaleCanyon", EnemySkillType.Ironhide, EnemyCombatRole.Durability, EnemyRace.Dragon));
        enemies.Add(E("lizardman_ravager", "Lizardman Ravager", "リザードマン裂爪兵", 6, "Grade05OgreMage", "Grade06Lizardman", "GlaadDragonScaleCanyon", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack, EnemyRace.Dragon));
        enemies.Add(E("lizardman_captain", "Lizardman Captain", "リザードマン隊長", 6, "Grade05StoneGolem", "Grade06Lizardman", "GlaadDragonScaleCanyon", EnemySkillType.PowerStrike, EnemyCombatRole.Balance, EnemyRace.Dragon));
        Enemies = enemies;
    }

    public static readonly IReadOnlyList<BalanceExpansionEquipmentDefinition> Equipment = BuildEquipment();
    public static readonly IReadOnlyList<BalanceExpansionConsumableDefinition> Consumables =
        new[]
        {
            C("attack_potion", "Attack Potion", "攻撃薬", "リーフの錬金術師が魔物の体液を戦意へ変えた調合薬。", ConsumableEffectType.BoostAttack, 160),
            C("defense_potion", "Defense Potion", "防御薬", "リーフの治療師が魔物の硬皮から守りを引き出した調合薬。", ConsumableEffectType.BoostDefense, 160),
            C("magic_elixir", "Magic Elixir", "魔力活性薬", "リーフの錬金術師が魔物の魔力腺を澄ませた活性薬。", ConsumableEffectType.RestoreMagic, 220, 50),
            C("swiftness_potion", "Swiftness Potion", "韋駄天薬", "リーフの薬師が魔物の腱と薬草を煎じた駆け足の薬。", ConsumableEffectType.BoostSpeed, 180),
            C("greater_antidote", "Greater Antidote", "上解毒薬", "リーフの薬師が魔物の毒腺を浄めて精製した上質な解毒薬。", ConsumableEffectType.CureAllStatus, 220)
        };
    static BalanceExpansionNormalEnemyDefinition N(string id, string en, string ja, int grade, string assetName, string dungeon, string visual) { return new BalanceExpansionNormalEnemyDefinition("enemy.normal." + id, en, ja, grade, assetName, dungeon, visual); }
    static SlimeVariantDefinition S(string id, string en, string ja, int grade, string dungeon, EnemySkillType skill, EnemyCombatRole role) { return new SlimeVariantDefinition("enemy.slime." + id, en, ja, grade, "EnemyData", "EnemyData", dungeon, skill, role); }
    static BalanceExpansionEnemyDefinition E(string id, string en, string ja, int grade, string basis, string dungeon, EnemySkillType skill, EnemyCombatRole role)
    {
        string dropSource = GetDropSourceAsset(id);
        EnemyRace race = GetRace(id);
        return new BalanceExpansionEnemyDefinition("enemy.job." + id, en, ja, grade, basis, dropSource, dungeon, skill, role, race, "Grade" + grade.ToString("00") + "_" + id);
    }

    static BalanceExpansionEnemyDefinition E(string id, string en, string ja, int grade, string basis, string dropSource, string dungeon, EnemySkillType skill, EnemyCombatRole role, EnemyRace race)
    {
        return new BalanceExpansionEnemyDefinition("enemy.job." + id, en, ja, grade, basis, dropSource, dungeon, skill, role, race, "Grade" + grade.ToString("00") + "_" + id);
    }

    static string GetDropSourceAsset(string id)
    {
        if (id.StartsWith("goblin_"))
        {
            return "Grade09Goblin";
        }
        if (id.StartsWith("skeleton_"))
        {
            return "Grade07Skeleton";
        }
        if (id.StartsWith("orc_"))
        {
            return "Grade06Orc";
        }
        return "Grade03Wyvern";
    }

    static EnemyRace GetRace(string id)
    {
        if (id.StartsWith("skeleton_"))
        {
            return EnemyRace.Undead;
        }
        if (id.StartsWith("wyvern_"))
        {
            return EnemyRace.Dragon;
        }
        return EnemyRace.Humanoid;
    }
    static BalanceExpansionConsumableDefinition C(
        string id,
        string en,
        string ja,
        string description,
        ConsumableEffectType effect,
        int price,
        int amount = 0)
    {
        return new BalanceExpansionConsumableDefinition(
            "item.expansion.consumable." + id,
            en,
            ja,
            description,
            effect,
            price,
            amount);
    }
    static ExistingEquipmentEffectAssignment X(string resourcePath, EquipmentEffectType type, float value, float secondaryValue = 0f, int durationTurns = 0) { return new ExistingEquipmentEffectAssignment(resourcePath, new EquipmentEffectDefinition { type = type, value = value, secondaryValue = secondaryValue, durationTurns = durationTurns }); }
    static IReadOnlyList<BalanceExpansionEquipmentDefinition> BuildEquipment()
    {
        var result = new List<BalanceExpansionEquipmentDefinition>();
        string[] english = { "Warrior", "Archer", "Mage" };
        string[,,] names = { { { "青鋼の長剣", "青鋼の胸甲", "城壁兵の護符" }, { "狩人の長弓", "狩人の羽外套", "鷹目のペンダント" }, { "灰梣の魔杖", "月紋の術衣", "月石の指輪" } }, { { "鋼獅子の戦斧", "鋲打ちの衛士鎧", "兵長の鉄章" }, { "風切りの複合弓", "疾風狩りの革外套", "風読みの耳飾り" }, { "青晶の儀杖", "星織りの法衣", "青晶の魔導環" } }, { { "ノルン樹冠の戦剣", "樹冠織の重鎧", "ノルン琥珀の護章" }, { "ノルン枝角弓", "翠葉なめしの狩衣", "樹霊の羽根飾り" }, { "ノルン霊木の杖", "苔紋の秘術衣", "深森の魔晶珠" } }, { { "ヴェルム黒鉄の巨剣", "黒鉄炉心の重甲", "熾火核の戦章" }, { "黒鉄弦の剛弓", "灰翼鱗の射手外套", "炉火鷹の眼飾り" }, { "熾火晶の大杖", "熔鉱織の賢者衣", "ヴェルム熾火の環" } } };
        for (int rank = 4; rank <= 7; rank++)
        {
            for (int classIndex = 0; classIndex < 3; classIndex++)
            {
                int nameRankIndex = rank - 4;
                result.Add(new BalanceExpansionEquipmentDefinition("item.expansion.rank" + rank + "." + classIndex, "Rank " + rank + " " + english[classIndex] + " Weapon", names[nameRankIndex, classIndex, 0], rank, classIndex, EquipmentSlot.Weapon, ItemAcquisitionType.Blacksmith));
                ItemAcquisitionType acquisition = rank <= 5 ? ItemAcquisitionType.Market : ItemAcquisitionType.Blacksmith;
                result.Add(new BalanceExpansionEquipmentDefinition("item.expansion.rank" + rank + "." + classIndex + ".armor", "Rank " + rank + " " + english[classIndex] + " Armor", names[nameRankIndex, classIndex, 1], rank, classIndex, EquipmentSlot.Armor, acquisition));
                result.Add(new BalanceExpansionEquipmentDefinition("item.expansion.rank" + rank + "." + classIndex + ".accessory", "Rank " + rank + " " + english[classIndex] + " Accessory", names[nameRankIndex, classIndex, 2], rank, classIndex, EquipmentSlot.Accessory, acquisition));
            }
        }
        result.Add(new BalanceExpansionEquipmentDefinition("item.expansion.dragonbane", "Dragonbane Blade", "竜狩りの剣", 6, 0, EquipmentSlot.Weapon, ItemAcquisitionType.Blacksmith, EnemyRace.Dragon, 0.35f));
        result.Add(new BalanceExpansionEquipmentDefinition("item.expansion.undeadbane", "Undead Purification Ward", "不死祓いの護符", 4, 2, EquipmentSlot.Accessory, ItemAcquisitionType.Market, EnemyRace.Undead, 0.30f));
        result.Add(new BalanceExpansionEquipmentDefinition("item.expansion.beastbane", "Beast Hunter Bow", "獣狩りの弓", 4, 1, EquipmentSlot.Weapon, ItemAcquisitionType.Blacksmith, EnemyRace.Beast, 0.28f));
        return result;
    }
}
