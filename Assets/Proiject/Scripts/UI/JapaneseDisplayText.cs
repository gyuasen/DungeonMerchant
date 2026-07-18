public static class JapaneseDisplayText
{
    public static string GetMercenaryClass(MercenaryClass value)
    {
        switch (value)
        {
            case MercenaryClass.Warrior: return "戦士";
            case MercenaryClass.Archer: return "弓兵";
            case MercenaryClass.Mage: return "魔術師";
            case MercenaryClass.Priest: return "僧侶";
            case MercenaryClass.Rogue: return "盗賊";
            case MercenaryClass.Lancer: return "槍兵";
            case MercenaryClass.Knight: return "騎士";
            case MercenaryClass.Berserker: return "狂戦士";
            case MercenaryClass.Sniper: return "狙撃手";
            case MercenaryClass.Ranger: return "レンジャー";
            case MercenaryClass.Sage: return "賢者";
            case MercenaryClass.Elementalist: return "元素術師";
            case MercenaryClass.Bishop: return "司祭";
            case MercenaryClass.Paladin: return "聖騎士";
            case MercenaryClass.Assassin: return "暗殺者";
            case MercenaryClass.Ninja: return "忍者";
            case MercenaryClass.Dragoon: return "竜騎兵";
            case MercenaryClass.GuardianLancer: return "守護槍兵";
            case MercenaryClass.Warlord: return "覇軍";
            case MercenaryClass.Beastmaster: return "幻獣使い";
            case MercenaryClass.Chronomancer: return "時詠み";
            case MercenaryClass.Saint: return "聖者";
            case MercenaryClass.Shadow: return "影";
            case MercenaryClass.DragonKnight: return "竜騎士";
            default: return value.ToString();
        }
    }

    public static string GetContractType(MercenaryContractType value)
    {
        switch (value)
        {
            case MercenaryContractType.Temporary: return "臨時契約";
            case MercenaryContractType.Local: return "日雇い契約";
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

        foreach (BalanceExpansionEquipmentDefinition definition in
                 BalanceExpansionDefinition.Equipment)
        {
            if (item.itemName == definition.EnglishName)
            {
                return definition.JapaneseName;
            }
        }

        foreach (BalanceExpansionConsumableDefinition definition in
                 BalanceExpansionDefinition.Consumables)
        {
            if (item.itemName == definition.EnglishName)
            {
                return definition.JapaneseName;
            }
        }

        string redesignedMaterialName = GetRedesignedMaterialName(item.itemName);
        if (!string.IsNullOrEmpty(redesignedMaterialName))
        {
            return redesignedMaterialName;
        }

        switch (item.itemName)
        {
            case "Low Grade Mutant Core": return "低級変異核";
            case "Lower Grade Mutant Core": return "下級変異核";
            case "Middle Grade Mutant Core": return "中級変異核";
            case "Upper Grade Mutant Core": return "上級変異核";
            case "Highest Grade Mutant Core": return "最上級変異核";
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
            case "Iron Vanguard Armor": return "鉄壁の重鎧";
            case "Windrunner Leather": return "風走りの革鎧";
            case "Runewoven Robe": return "ルーン織りのローブ";
            case "Champion Emblem": return "勇士の紋章";
            case "Hawkeye Charm": return "鷹の目のお守り";
            case "Arcane Pendant": return "秘術の首飾り";
            case "Sanctified Mace": return "聖別のメイス";
            case "Bone Prayer Vestment": return "骨祈りの祭服";
            case "Spirit Bead": return "霊珠";
            case "Venom Fang Dagger": return "毒牙の短剣";
            case "Shadowhide Armor": return "影革の鎧";
            case "Bat Eye Charm": return "蝙蝠眼のお守り";
            case "Orcbone Spear": return "オーク骨の槍";
            case "Golem Plate": return "ゴーレムプレート";
            case "Wyvern Crest": return "飛竜の紋章";
            case "Minebreaker Hammer": return "坑道砕きの大槌";
            case "Deep Miner Armor": return "深鉱夫の鎧";
            case "Echo Stone Ring": return "残響石の指輪";
            case "Mist Rune Blade": return "霧紋の刃";
            case "Ruinweave Mantle": return "遺跡織りの外套";
            case "Guardian Eye Charm": return "守護眼のお守り";
            case "Black Iron Halberd": return "黒鉄の斧槍";
            case "Black Iron General Plate": return "黒鉄将軍の大鎧";
            case "Black Iron War Emblem": return "黒鉄の戦紋章";
            case "Oni Hunter Cleaver": return "鬼狩りの鉈";
            case "Oni Hunter Garb": return "鬼狩りの装束";
            case "Goblin Fang Talisman": return "小鬼牙のお守り";
            case "Mutant Core": return "変異核";
            case "Mutant Core Charm": return "変異核の護符";
            case "Iron Armor": return "鉄の鎧";
            case "Leather Armor": return "革の鎧";
            case "Apprentice Robe": return "見習いのローブ";
            case "Soldier Ring": return "兵士の指輪";
            case "Feather Charm": return "羽根のお守り";
            case "Mana Pendant": return "魔力の首飾り";
            case "Antidote": return "解毒薬";
            case "Paralysis Remedy": return "麻痺治し";
            case "Secret Job Certificate": return "秘伝の転職証";
            case "Enhancement Ore":
            case "Low Grade Enhancement Ore": return "低級強化鉱石";
            case "Lower Grade Enhancement Ore": return "下級強化鉱石";
            case "Middle Grade Enhancement Ore": return "中級強化鉱石";
            case "Upper Grade Enhancement Ore": return "上級強化鉱石";
            case "Highest Grade Enhancement Ore": return "最上級強化鉱石";
            case "Ancient Guardian Blade": return "古代守護者の刃";
            case "Ancient Guardian Armor": return "古代守護者の鎧";
            case "Ancient Guardian Seal": return "古代守護者の印";
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
        foreach (BalanceExpansionNormalEnemyDefinition definition in
                 BalanceExpansionDefinition.NormalEnemies)
        {
            if (enemyName == definition.EnglishName)
            {
                return definition.JapaneseName;
            }
        }

        foreach (BalanceExpansionEnemyDefinition definition in
                 BalanceExpansionDefinition.Enemies)
        {
            if (enemyName == definition.EnglishName)
            {
                return definition.JapaneseName;
            }
        }

        switch (enemyName)
        {
            case "Slime": return "スライム";
            case "Green Slime": return "グリーンスライム";
            case "Blue Slime": return "ブルースライム";
            case "Moss Slime": return "苔スライム";
            case "Horn Rabbit": return "角ウサギ";
            case "Goblin": return "ゴブリン";
            case "Kobold": return "コボルト";
            case "Goblin Scout": return "ゴブリン斥候";
            case "Wild Dog": return "野犬";
            case "Goblin Spearman": return "ゴブリン槍兵";
            case "Cave Bat": return "洞窟コウモリ";
            case "Giant Rat": return "大ネズミ";
            case "Cave Spider": return "洞窟グモ";
            case "Venom Moth": return "毒蛾";
            case "Rock Beetle": return "岩甲虫";
            case "Skeleton": return "スケルトン";
            case "Zombie": return "ゾンビ";
            case "Armored Skeleton": return "装甲スケルトン";
            case "Wraith": return "レイス";
            case "Bone Hound": return "骨猟犬";
            case "Orc": return "オーク";
            case "Lizardman": return "リザードマン";
            case "Hobgoblin": return "ホブゴブリン";
            case "Troll": return "トロル";
            case "Marsh Lizard": return "沼トカゲ";
            case "Stone Golem": return "ストーンゴーレム";
            case "Iron Golem": return "アイアンゴーレム";
            case "Ogre Mage": return "オーガメイジ";
            case "Dark Mage": return "闇の魔術師";
            case "Wyvern": return "ワイバーン";
            case "Demon Knight": return "魔界騎士";
            case "Abyss Dragon": return "深淵竜";
            case "Cave Ogre": return "洞窟のオーガ";
            case "Mine Tyrant": return "廃坑の暴君";
            case "Ruin Guardian": return "遺跡の守護者";
            case "Black Iron General": return "黒鉄将軍";
            case "Abyss Lord": return "深淵の王";
            case "Mistfang Wolf": return "霧牙狼";
            case "Thunderhorn Kirin": return "雷角麒麟";
            case "Flamewing Gryphon": return "炎翼グリフォン";
            case "Astral Dragon": return "星界竜";
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
        string categoryLabel =
            enemy.category == EnemyCategory.MythicalBeast
                ? " 幻獣"
                : string.Empty;
        string bossLabel = enemy.isBoss ? " ボス" : string.Empty;
        return $"{grade}等級{categoryLabel}{bossLabel}";
    }

    public static string GetMonsterCategory(EnemyCategory value)
    {
        switch (value)
        {
            case EnemyCategory.Normal:
                return "通常種";
            case EnemyCategory.MythicalBeast:
                return "魔獣";
            default:
                return value.ToString();
        }
    }

    private static string GetRedesignedMaterialName(string itemName)
    {
        switch (itemName)
        {
            case "Slime Mucus": return "スライムの粘液";
            case "Rabbit Horn": return "ウサギの角";
            case "Giant Rat Pelt": return "大ネズミの毛皮";
            case "Spider Silk": return "クモの糸";
            case "Beetle Shell": return "甲虫の殻";
            case "Venom Moth Powder": return "毒蛾の鱗粉";
            case "Spirit Remnant": return "霊魂の残滓";
            case "Lizard Scale": return "トカゲの鱗";
            case "Troll Hide": return "トロルの皮";
            case "Ogre Bloodstone": return "オーガの血石";
            case "Black Iron Ore Fragment": return "黒鉄鉱片";
            default: return string.Empty;
        }
    }

    public static string GetMonsterGradeWithStrengthHint(EnemyDataSO enemy)
    {
        int grade = enemy == null
            ? 10
            : UnityEngine.Mathf.Clamp(enemy.monsterGrade, 1, 10);
        string strengthHint = grade == 1
            ? "最強"
            : grade == 10
                ? "最弱"
                : "上位";
        return $"{GetMonsterGrade(enemy)}（{strengthHint}）";
    }

    public static string GetBattleStatus(BattleStatusEffect status)
    {
        switch (status)
        {
            case BattleStatusEffect.Poison: return "毒";
            case BattleStatusEffect.Paralysis: return "麻痺";
            default: return "正常";
        }
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

    public static string GetItemNameByRawName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return "不明なアイテム";
        }

        ItemDataSO temporary = UnityEngine.ScriptableObject.CreateInstance<ItemDataSO>();
        temporary.itemName = itemName;
        string displayName = GetItemName(temporary);
        UnityEngine.Object.Destroy(temporary);
        return displayName;
    }

    public static string GetEquipmentQuality(EquipmentQuality value)
    {
        switch (value)
        {
            case EquipmentQuality.Poor: return "粗悪";
            case EquipmentQuality.Normal: return "普通";
            case EquipmentQuality.Fine: return "良質";
            case EquipmentQuality.Rare: return "希少";
            case EquipmentQuality.Legendary: return "伝説";
            default: return value.ToString();
        }
    }

    public static string GetEquipmentSet(EquipmentSetId value)
    {
        switch (value)
        {
            case EquipmentSetId.AncientGuardian: return "古代守護者";
            case EquipmentSetId.Vanguard: return "不屈の前衛";
            case EquipmentSetId.Windstalker: return "風狩り";
            case EquipmentSetId.ArcaneSage: return "秘術賢者";
            case EquipmentSetId.OniHunter: return "鬼狩り";
            case EquipmentSetId.NornCanopy: return "ノルン樹冠";
            case EquipmentSetId.GlaadSkyFortress: return "グラード天嶺";
            case EquipmentSetId.VelmBlackIron: return "ヴェルム黒鉄";
            case EquipmentSetId.AbyssThrone: return "アビス玉座";
            case EquipmentSetId.AstralDepths: return "星幽深層";
            default: return "セットなし";
        }
    }

    public static string GetEquipmentModifier(EquipmentModifierType value)
    {
        switch (value)
        {
            case EquipmentModifierType.MaxHP: return "最大HP";
            case EquipmentModifierType.Attack: return "攻撃";
            case EquipmentModifierType.Defense: return "防御";
            case EquipmentModifierType.AttackSpeed: return "攻撃速度";
            default: return value.ToString();
        }
    }
}
