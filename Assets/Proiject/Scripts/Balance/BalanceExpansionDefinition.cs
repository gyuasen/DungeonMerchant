using System;
using System.Collections.Generic;

public enum EnemyCombatRole { Skill, Speed, Durability, Attack, Balance }
public sealed class BalanceExpansionEnemyDefinition
{
    public readonly string Id, EnglishName, JapaneseName, BaseEnemyAsset, DungeonAsset, BattleVisualKey;
    public readonly int Grade;
    public readonly EnemySkillType Skill;
    public readonly EnemyCombatRole Role;
    public BalanceExpansionEnemyDefinition(string id, string en, string ja, int grade, string basis, string dungeon, EnemySkillType skill, EnemyCombatRole role, string visual) { Id = id; EnglishName = en; JapaneseName = ja; Grade = grade; BaseEnemyAsset = basis; DungeonAsset = dungeon; Skill = skill; Role = role; BattleVisualKey = visual; }
}
public sealed class BalanceExpansionNormalEnemyDefinition
{
    public readonly string Id, EnglishName, JapaneseName, AssetName, DungeonAsset, BattleVisualKey;
    public readonly int Grade;
    public BalanceExpansionNormalEnemyDefinition(string id, string en, string ja, int grade, string assetName, string dungeonAsset, string battleVisualKey) { Id = id; EnglishName = en; JapaneseName = ja; Grade = grade; AssetName = assetName; DungeonAsset = dungeonAsset; BattleVisualKey = battleVisualKey; }
}
public sealed class BalanceExpansionEquipmentDefinition
{
    public readonly string Id, EnglishName, JapaneseName;
    public readonly int Rank, ClassIndex;
    public readonly EquipmentSlot Slot;
    public readonly ItemAcquisitionType AcquisitionType;
    public BalanceExpansionEquipmentDefinition(string id, string en, string ja, int rank, int classIndex, EquipmentSlot slot, ItemAcquisitionType acquisitionType) { Id = id; EnglishName = en; JapaneseName = ja; Rank = rank; ClassIndex = classIndex; Slot = slot; AcquisitionType = acquisitionType; }
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
    public static readonly IReadOnlyList<BalanceExpansionNormalEnemyDefinition> NormalEnemies = new[] { N("wyvern", "Wyvern", "ワイバーン", 3, "Grade03Wyvern", "GlaadSkyFortress", "Grade03_wyvern") };
    public static readonly IReadOnlyList<BalanceExpansionEnemyDefinition> Enemies = new[] { E("goblin_assassin", "Goblin Assassin", "ゴブリンアサシン", 9, "Grade08GiantRat", "EldUndergroundWaterway", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed), E("goblin_magician", "Goblin Magician", "ゴブリンマジシャン", 9, "Grade08CaveSpider", "EldUndergroundWaterway", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("goblin_knight", "Goblin Knight", "ゴブリンナイト", 9, "Grade08RockBeetle", "EldUndergroundWaterway", EnemySkillType.Ironhide, EnemyCombatRole.Durability), E("goblin_raider", "Goblin Raider", "ゴブリンレイダー", 9, "Grade08GiantRat", "EldUndergroundWaterway", EnemySkillType.CleavingRush, EnemyCombatRole.Attack), E("goblin_veteran", "Goblin Veteran", "ゴブリンベテラン", 9, "Grade08CaveBat", "EldUndergroundWaterway", EnemySkillType.DoubleStrike, EnemyCombatRole.Balance), E("skeleton_archer", "Skeleton Archer", "スケルトンアーチャー", 7, "Grade06MarshLizard", "EldOldQuarry", EnemySkillType.PiercingShot, EnemyCombatRole.Speed), E("skeleton_hexer", "Skeleton Hexer", "スケルトン呪術師", 7, "Grade06Lizardman", "EldOldQuarry", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("skeleton_guard", "Skeleton Guard", "スケルトンガード", 7, "Grade06Troll", "EldOldQuarry", EnemySkillType.Ironhide, EnemyCombatRole.Durability), E("skeleton_reaper", "Skeleton Reaper", "スケルトンリーパー", 7, "Grade06Hobgoblin", "EldOldQuarry", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack), E("skeleton_captain", "Skeleton Captain", "スケルトン隊長", 7, "Grade06Orc", "EldOldQuarry", EnemySkillType.PowerStrike, EnemyCombatRole.Balance), E("orc_shaman", "Orc Shaman", "オークシャーマン", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("orc_rider", "Orc Rider", "オークライダー", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed), E("orc_bulwark", "Orc Bulwark", "オークブルワーク", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.Ironhide, EnemyCombatRole.Durability), E("orc_berserker", "Orc Berserker", "オークバーサーカー", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack), E("orc_veteran", "Orc Veteran", "オーク古参兵", 5, "Grade04DarkMage", "NornCanopyLabyrinth", EnemySkillType.CleavingRush, EnemyCombatRole.Balance), E("wyvern_hexer", "Wyvern Hexwing", "呪翼のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.HexBolt, EnemyCombatRole.Skill), E("wyvern_skyrider", "Wyvern Galewing", "疾風翼のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.ShadowPounce, EnemyCombatRole.Speed), E("wyvern_ironwing", "Wyvern Ironwing", "ワイバーン鉄翼", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.Reconstitute, EnemyCombatRole.Durability), E("wyvern_ravager", "Wyvern Razortalon", "猛爪のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.BloodFrenzy, EnemyCombatRole.Attack), E("wyvern_captain", "Wyvern Packlord", "群れ長のワイバーン", 3, "Grade02DemonKnight", "UpperFortress", EnemySkillType.CleavingRush, EnemyCombatRole.Balance) };
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
    static BalanceExpansionEnemyDefinition E(string id, string en, string ja, int grade, string basis, string dungeon, EnemySkillType skill, EnemyCombatRole role) { return new BalanceExpansionEnemyDefinition("enemy.job." + id, en, ja, grade, basis, dungeon, skill, role, "Grade" + grade.ToString("00") + "_" + id); }
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
        return result;
    }
}
