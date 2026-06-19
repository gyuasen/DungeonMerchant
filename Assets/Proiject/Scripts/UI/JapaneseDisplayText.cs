public static class JapaneseDisplayText
{
    public static string GetMercenaryClass(MercenaryClass value)
    {
        switch (value)
        {
            case MercenaryClass.Warrior: return "戦士";
            case MercenaryClass.Archer: return "弓兵";
            case MercenaryClass.Mage: return "魔術師";
            default: return value.ToString();
        }
    }

    public static string GetContractType(MercenaryContractType value)
    {
        switch (value)
        {
            case MercenaryContractType.Temporary: return "臨時契約";
            case MercenaryContractType.Local: return "地域契約";
            case MercenaryContractType.Exclusive: return "専属契約";
            default: return value.ToString();
        }
    }

    public static string GetItemType(ItemType value)
    {
        switch (value)
        {
            case ItemType.Material: return "素材";
            case ItemType.Equipment: return "装備";
            case ItemType.Relic: return "遺物";
            case ItemType.Consumable: return "消耗品";
            default: return value.ToString();
        }
    }

    public static string GetItemRarity(ItemRarity value)
    {
        switch (value)
        {
            case ItemRarity.Common: return "一般";
            case ItemRarity.Uncommon: return "希少";
            case ItemRarity.Rare: return "レア";
            case ItemRarity.Epic: return "英雄級";
            default: return value.ToString();
        }
    }

    public static string GetItemName(ItemDataSO item)
    {
        if (item == null)
        {
            return "不明なアイテム";
        }

        switch (item.itemName)
        {
            case "Monster Fang": return "魔物の牙";
            case "Goblin Ear": return "ゴブリンの耳";
            case "Bat Wing": return "コウモリの翼";
            case "Cursed Bone": return "呪われた骨";
            case "Orc Tusk": return "オークの牙";
            case "Golem Core": return "ゴーレムの核";
            case "Dark Crystal": return "闇の結晶";
            case "Wyvern Scale": return "ワイバーンの鱗";
            case "Demon Steel": return "魔鋼";
            case "Abyss Dragon Scale": return "深淵竜の鱗";
            case "Ogre Crown": return "オーガの王冠";
            case "Tyrant Pickaxe": return "暴君の大つるはし";
            case "Guardian Eye": return "守護者の魔眼";
            case "Black Iron Emblem": return "黒鉄の紋章";
            case "Abyss Crown": return "深淵の王冠";
            case "Trade Goods": return "交易品";
            case "Iron Sword": return "鉄の剣";
            case "Steel Sword": return "鋼の剣";
            case "Short Bow": return "ショートボウ";
            case "Composite Bow": return "複合弓";
            case "Apprentice Staff": return "見習いの杖";
            case "Arcane Staff": return "秘術の杖";
            case "Goblin Hunter Sword": return "ゴブリン狩りの剣";
            case "Beastbone Bow": return "獣骨の弓";
            case "Hexwood Staff": return "呪木の杖";
            default: return item.itemName;
        }
    }

    public static string GetEquipmentSlot(EquipmentSlot value)
    {
        switch (value)
        {
            case EquipmentSlot.Weapon: return "武器";
            case EquipmentSlot.Armor: return "防具";
            case EquipmentSlot.Accessory: return "装飾品";
            default: return value.ToString();
        }
    }

    public static string GetEnemyName(string enemyName)
    {
        switch (enemyName)
        {
            case "Slime": return "スライム";
            case "Goblin": return "ゴブリン";
            case "Cave Bat": return "洞窟コウモリ";
            case "Skeleton": return "スケルトン";
            case "Orc": return "オーク";
            case "Stone Golem": return "ストーンゴーレム";
            case "Dark Mage": return "闇の魔術師";
            case "Wyvern": return "ワイバーン";
            case "Demon Knight": return "魔界騎士";
            case "Abyss Dragon": return "深淵竜";
            case "Cave Ogre": return "洞窟のオーガ";
            case "Mine Tyrant": return "廃坑の暴君";
            case "Ruin Guardian": return "遺跡の守護者";
            case "Black Iron General": return "黒鉄将軍";
            case "Abyss Lord": return "深淵の王";
            default: return enemyName;
        }
    }

    public static string GetMonsterGrade(EnemyDataSO enemy)
    {
        if (enemy == null)
        {
            return "等級不明";
        }

        int grade = enemy.monsterGrade < 1
            ? 1
            : enemy.monsterGrade > 10
                ? 10
                : enemy.monsterGrade;
        string bossLabel = enemy.isBoss ? " ボス" : string.Empty;
        return $"{grade}等級{bossLabel}";
    }

    public static string GetDungeonGrade(DungeonGrade value)
    {
        switch (value)
        {
            case DungeonGrade.Low: return "低級";
            case DungeonGrade.Lower: return "下級";
            case DungeonGrade.Middle: return "中級";
            case DungeonGrade.Upper: return "上級";
            case DungeonGrade.Highest: return "最上級";
            default: return value.ToString();
        }
    }
}
