using System.Collections.Generic;

public enum MercenaryClass
{
    Warrior,
    Archer,
    Mage,
    Priest,
    Rogue,
    Lancer,
    Knight,
    Berserker,
    Sniper,
    Ranger,
    Sage,
    Elementalist,
    Bishop,
    Paladin,
    Assassin,
    Ninja,
    Dragoon,
    GuardianLancer,
    Warlord,
    Beastmaster,
    Chronomancer,
    Saint,
    Shadow,
    DragonKnight
}

public enum MercenarySkillId
{
    Taunt,
    DoubleShot,
    Fireball,
    Heal,
    PoisonBlade,
    PiercingThrust,
    ShieldBash,
    Volley,
    FrostNova,
    Smite,
    ShadowFlurry,
    SweepingThrust,
    GuardCounter,
    AimedShot,
    ManaBolt,
    PrayerLight,
    VitalStrike,
    GuardianThrust,
    WarlordCommand,
    BeastPack,
    TimeLock,
    SaintsGrace,
    ShadowRend,
    DragonBreath,
    AegisPierce,
    RagingCombo,
    ArmorBreakArrow,
    GaleVolley,
    ArcaneBurst,
    StormCircle,
    GreaterHeal,
    HolyShieldBash,
    FatalFlurry,
    BindingBlade,
    SkySpearCombo,
    FortressPierce,
    WarfrontSmash,
    BeastKingFangs,
    TemporalRift,
    DivineHymn,
    VoidPierce,
    DragonfallBreath
}

public class MercenarySkillDefinition
{
    public MercenarySkillId Id;
    public string Name;
    public int MagicCost;
    public float Power;
    public string Description;
    public int UnlockLevel = 1;
    public bool IsPassive;
    public int BonusMaxHP;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusMaxMagicPower;
    public float BonusAttackSpeed;
    public float BonusCriticalRate;
    public float BonusEvasionRate;
}

public static class MercenaryClassProgression
{
    public const int PromotionLevel = 15;
    public const int AdvancedLevelCap = 30;
    public const int SpecialLevelCap = 50;

    public static int GetPassiveBonus(
        MercenaryClass value,
        int level,
        System.Func<MercenarySkillDefinition, int> selector)
    {
        int total = 0;
        foreach (MercenarySkillDefinition skill in GetSkillProgression(value))
        {
            if (skill.IsPassive && level >= skill.UnlockLevel)
            {
                total += selector(skill);
            }
        }
        return total;
    }

    public static float GetPassiveBonusFloat(
        MercenaryClass value,
        int level,
        System.Func<MercenarySkillDefinition, float> selector)
    {
        float total = 0f;
        foreach (MercenarySkillDefinition skill in GetSkillProgression(value))
        {
            if (skill.IsPassive && level >= skill.UnlockLevel)
            {
                total += selector(skill);
            }
        }
        return total;
    }

    public static MercenaryClass[] GetBaseClasses()
    {
        return new[]
        {
            MercenaryClass.Warrior, MercenaryClass.Archer,
            MercenaryClass.Mage, MercenaryClass.Priest,
            MercenaryClass.Rogue, MercenaryClass.Lancer
        };
    }

    public static bool IsBaseClass(MercenaryClass value)
    {
        return value == GetBaseClass(value);
    }

    public static bool IsSpecialClass(MercenaryClass value)
    {
        return value == MercenaryClass.Warlord ||
               value == MercenaryClass.Beastmaster ||
               value == MercenaryClass.Chronomancer ||
               value == MercenaryClass.Saint ||
               value == MercenaryClass.Shadow ||
               value == MercenaryClass.DragonKnight;
    }

    public static int GetLevelCap(MercenaryClass value)
    {
        return IsBaseClass(value)
            ? PromotionLevel
            : IsSpecialClass(value)
                ? SpecialLevelCap
                : AdvancedLevelCap;
    }

    public static MercenaryClass GetBaseClass(MercenaryClass value)
    {
        switch (value)
        {
            case MercenaryClass.Knight:
            case MercenaryClass.Berserker:
            case MercenaryClass.Warlord: return MercenaryClass.Warrior;
            case MercenaryClass.Sniper:
            case MercenaryClass.Ranger:
            case MercenaryClass.Beastmaster: return MercenaryClass.Archer;
            case MercenaryClass.Sage:
            case MercenaryClass.Elementalist:
            case MercenaryClass.Chronomancer: return MercenaryClass.Mage;
            case MercenaryClass.Bishop:
            case MercenaryClass.Paladin:
            case MercenaryClass.Saint: return MercenaryClass.Priest;
            case MercenaryClass.Assassin:
            case MercenaryClass.Ninja:
            case MercenaryClass.Shadow: return MercenaryClass.Rogue;
            case MercenaryClass.Dragoon:
            case MercenaryClass.GuardianLancer:
            case MercenaryClass.DragonKnight: return MercenaryClass.Lancer;
            default: return value;
        }
    }

    public static MercenaryClass[] GetAdvancedClasses(MercenaryClass value)
    {
        switch (GetBaseClass(value))
        {
            case MercenaryClass.Warrior:
                return new[] { MercenaryClass.Knight, MercenaryClass.Berserker };
            case MercenaryClass.Archer:
                return new[] { MercenaryClass.Sniper, MercenaryClass.Ranger };
            case MercenaryClass.Mage:
                return new[] { MercenaryClass.Sage, MercenaryClass.Elementalist };
            case MercenaryClass.Priest:
                return new[] { MercenaryClass.Bishop, MercenaryClass.Paladin };
            case MercenaryClass.Rogue:
                return new[] { MercenaryClass.Assassin, MercenaryClass.Ninja };
            default:
                return new[] { MercenaryClass.Dragoon, MercenaryClass.GuardianLancer };
        }
    }

    public static MercenaryClass GetSpecialClass(MercenaryClass value)
    {
        switch (GetBaseClass(value))
        {
            case MercenaryClass.Warrior: return MercenaryClass.Warlord;
            case MercenaryClass.Archer: return MercenaryClass.Beastmaster;
            case MercenaryClass.Mage: return MercenaryClass.Chronomancer;
            case MercenaryClass.Priest: return MercenaryClass.Saint;
            case MercenaryClass.Rogue: return MercenaryClass.Shadow;
            default: return MercenaryClass.DragonKnight;
        }
    }

    public static MercenarySkillDefinition GetPrimarySkill(MercenaryClass value)
    {
        switch (GetBaseClass(value))
        {
            case MercenaryClass.Warrior:
                return Skill(MercenarySkillId.Taunt, "挑発", 35, 0f,
                    "敵の攻撃を自分に引きつける。");
            case MercenaryClass.Archer:
                return Skill(MercenarySkillId.DoubleShot, "連射", 45, 0.75f,
                    "低威力の射撃を2回行う。");
            case MercenaryClass.Mage:
                return Skill(MercenarySkillId.Fireball, "火球", 50, 1.65f,
                    "敵1体へ高威力の魔法攻撃を行う。");
            case MercenaryClass.Priest:
                return Skill(MercenarySkillId.Heal, "治癒", 40, 1.2f,
                    "最も傷ついた味方を回復する。");
            case MercenaryClass.Rogue:
                return Skill(MercenarySkillId.PoisonBlade, "毒刃", 35, 1.05f,
                    "敵を攻撃し、毒を付与する。");
            default:
                return Skill(MercenarySkillId.PiercingThrust, "貫通突き", 40, 0.70f,
                    "防御の影響を抑えた一撃を放つ。");
        }
    }

    public static List<MercenarySkillDefinition> GetCombatSkills(
        MercenaryClass value)
    {
        List<MercenarySkillDefinition> skills =
            new List<MercenarySkillDefinition> { GetPrimarySkill(value) };
        switch (GetBaseClass(value))
        {
            case MercenaryClass.Warrior:
                skills.Add(Skill(MercenarySkillId.ShieldBash, "盾撃", 30, 0.9f,
                    "防御的な打撃で敵を短時間ひるませる。"));
                skills.Add(Skill(MercenarySkillId.GuardCounter, "守勢反撃", 25, 1.1f,
                    "守りを崩さず反撃する。"));
                break;
            case MercenaryClass.Archer:
                skills.Add(Skill(MercenarySkillId.Volley, "斉射", 45, 0.65f,
                    "敵全体へ矢を降らせる。"));
                skills.Add(Skill(MercenarySkillId.AimedShot, "狙い撃ち", 30, 1.15f,
                    "単体へ精密な一射を放つ。"));
                break;
            case MercenaryClass.Mage:
                skills.Add(Skill(MercenarySkillId.FrostNova, "氷結波", 45, 0.7f,
                    "敵全体を攻撃し、行動を妨げる。"));
                skills.Add(Skill(MercenarySkillId.ManaBolt, "魔力弾", 30, 1.2f,
                    "低燃費の単体魔法を放つ。"));
                break;
            case MercenaryClass.Priest:
                skills.Add(Skill(MercenarySkillId.Smite, "聖撃", 35, 1.25f,
                    "敵1体へ聖なる攻撃を行う。"));
                skills.Add(Skill(MercenarySkillId.PrayerLight, "祈りの光", 25, 0.55f,
                    "傷ついた味方全員を小回復する。"));
                break;
            case MercenaryClass.Rogue:
                skills.Add(Skill(MercenarySkillId.ShadowFlurry, "影連撃", 40, 0.62f,
                    "素早い2連撃を放つ。"));
                skills.Add(Skill(MercenarySkillId.VitalStrike, "急所狙い", 30, 1.2f,
                    "隙を突く単体攻撃を放つ。"));
                break;
            default:
                skills.Add(Skill(MercenarySkillId.SweepingThrust, "薙ぎ突き", 40, 0.62f,
                    "敵全体へ槍の一閃を放つ。"));
                skills.Add(Skill(MercenarySkillId.GuardianThrust, "守護突き", 30, 1.1f,
                    "隊列を守る堅実な突きを放つ。"));
                break;
        }

        MercenarySkillDefinition advancedSkill = GetAdvancedCombatSkill(value);
        if (advancedSkill != null)
        {
            skills.Add(advancedSkill);
        }

        MercenarySkillDefinition specialSkill = GetSpecialCombatSkill(value);
        if (specialSkill != null)
        {
            skills.Add(specialSkill);
        }

        MercenarySkillDefinition specialAdditionalSkill =
            GetSpecialAdditionalCombatSkill(value);
        if (specialAdditionalSkill != null)
        {
            skills.Add(specialAdditionalSkill);
        }
        return skills;
    }

    private static MercenarySkillDefinition GetAdvancedCombatSkill(
        MercenaryClass value)
    {
        switch (value)
        {
            case MercenaryClass.Knight:
                return Skill(MercenarySkillId.AegisPierce, "イージス貫き", 50,
                    0.75f, "防御を貫く堅牢な一撃を放つ。", 25);
            case MercenaryClass.Berserker:
                return Skill(MercenarySkillId.RagingCombo, "激昂連斬", 55,
                    0.55f, "荒々しい三連撃を繰り出す。", 25);
            case MercenaryClass.Sniper:
                return Skill(MercenarySkillId.ArmorBreakArrow, "装甲砕き", 50,
                    0.80f, "防御を無視する狙撃を放つ。", 25);
            case MercenaryClass.Ranger:
                return Skill(MercenarySkillId.GaleVolley, "疾風散射", 55,
                    0.75f, "風をまとった矢を敵全体へ放つ。", 25);
            case MercenaryClass.Sage:
                return Skill(MercenarySkillId.ArcaneBurst, "秘術爆裂", 50,
                    1.45f, "凝縮した魔力で敵一体を撃ち抜く。", 25);
            case MercenaryClass.Elementalist:
                return Skill(MercenarySkillId.StormCircle, "嵐の環", 60,
                    0.65f, "敵全体を攻撃し、麻痺させる。", 25);
            case MercenaryClass.Bishop:
                return Skill(MercenarySkillId.GreaterHeal, "大治癒", 55,
                    1.55f, "最も傷ついた味方を大きく回復する。", 25);
            case MercenaryClass.Paladin:
                return Skill(MercenarySkillId.HolyShieldBash, "聖盾撃", 50,
                    1.10f, "敵一体を打ち据え、麻痺させる。", 25);
            case MercenaryClass.Assassin:
                return Skill(MercenarySkillId.FatalFlurry, "致命乱舞", 55,
                    0.45f, "急所を狙う四連撃を放つ。", 25);
            case MercenaryClass.Ninja:
                return Skill(MercenarySkillId.BindingBlade, "影縫い", 50,
                    1.05f, "敵一体を斬りつけ、麻痺させる。", 25);
            case MercenaryClass.Dragoon:
                return Skill(MercenarySkillId.SkySpearCombo, "天槍連舞", 55,
                    0.55f, "跳躍から鋭い三連突きを放つ。", 25);
            case MercenaryClass.GuardianLancer:
                return Skill(MercenarySkillId.FortressPierce, "城塞貫通", 50,
                    0.72f, "防御を無視する守護の突きを放つ。", 25);
            default:
                return null;
        }
    }

    private static MercenarySkillDefinition GetSpecialCombatSkill(
        MercenaryClass value)
    {
        switch (value)
        {
            case MercenaryClass.Warlord:
                return Skill(MercenarySkillId.WarlordCommand, "戦陣号令", 55,
                    1.45f, "敵一体を指揮官の一撃で打ち崩す。", 20);
            case MercenaryClass.Beastmaster:
                return Skill(MercenarySkillId.BeastPack, "獣群急襲", 50,
                    0.65f, "使役獣の群れで敵全体を攻撃する。", 24);
            case MercenaryClass.Chronomancer:
                return Skill(MercenarySkillId.TimeLock, "時縛り", 50,
                    0.95f, "敵一体にダメージを与え、麻痺させる。", 28);
            case MercenaryClass.Saint:
                return Skill(MercenarySkillId.SaintsGrace, "聖者の恩寵", 55,
                    0.85f, "味方全体を癒やす祝福を授ける。", 32);
            case MercenaryClass.Shadow:
                return Skill(MercenarySkillId.ShadowRend, "影裂き", 55,
                    0.52f, "影から三連撃を繰り出す。", 36);
            case MercenaryClass.DragonKnight:
                return Skill(MercenarySkillId.DragonBreath, "竜炎の息吹", 60,
                    0.70f, "竜の炎で敵全体を焼き払う。", 40);
            default:
                return null;
        }
    }

    private static MercenarySkillDefinition GetSpecialAdditionalCombatSkill(
        MercenaryClass value)
    {
        switch (value)
        {
            case MercenaryClass.Warlord:
                return Skill(MercenarySkillId.WarfrontSmash, "覇軍震撃", 70,
                    0.75f, "戦場を震わせ、敵全体を打ち崩す。", 45);
            case MercenaryClass.Beastmaster:
                return Skill(MercenarySkillId.BeastKingFangs, "獣王連牙", 70,
                    0.44f, "獣王の牙による四連撃を放つ。", 45);
            case MercenaryClass.Chronomancer:
                return Skill(MercenarySkillId.TemporalRift, "時空断層", 65,
                    0.82f, "防御を越えて敵一体を断ち切る。", 45);
            case MercenaryClass.Saint:
                return Skill(MercenarySkillId.DivineHymn, "神聖詠唱", 70,
                    1.0f, "味方全体を力強く回復する。", 45);
            case MercenaryClass.Shadow:
                return Skill(MercenarySkillId.VoidPierce, "虚無穿ち", 65,
                    0.85f, "防御を無視する影の一閃を放つ。", 45);
            case MercenaryClass.DragonKnight:
                return Skill(MercenarySkillId.DragonfallBreath, "天竜爆炎", 75,
                    0.78f, "天竜の炎で敵全体を焼き払う。", 45);
            default:
                return null;
        }
    }

    public static List<MercenarySkillDefinition> GetSkillProgression(
        MercenaryClass value)
    {
        List<MercenarySkillDefinition> result =
            new List<MercenarySkillDefinition>
            {
                GetPrimarySkill(value)
            };
        List<MercenarySkillDefinition> combatSkills = GetCombatSkills(value);
        result.AddRange(combatSkills.GetRange(1, combatSkills.Count - 1));
        MercenaryClass baseClass = GetBaseClass(value);
        switch (baseClass)
        {
            case MercenaryClass.Warrior:
                result.Add(Passive("堅守", 5, "防御+3。", defense: 3));
                result.Add(Passive("闘志", 10, "攻撃+3。", attack: 3));
                break;
            case MercenaryClass.Archer:
                result.Add(Passive("鷹の目", 5, "クリティカル率+5%。",
                    critical: 0.05f));
                result.Add(Passive("軽足", 10, "回避率+4%。",
                    evasion: 0.04f));
                break;
            case MercenaryClass.Mage:
                result.Add(Passive("魔力循環", 5, "最大魔力+15。",
                    magic: 15));
                result.Add(Passive("魔導増幅", 10, "攻撃+4。",
                    attack: 4));
                break;
            case MercenaryClass.Priest:
                result.Add(Passive("祈り", 5, "最大魔力+15。",
                    magic: 15));
                result.Add(Passive("慈愛", 10, "最大HP+12。",
                    hp: 12));
                break;
            case MercenaryClass.Rogue:
                result.Add(Passive("急所狙い", 5, "クリティカル率+6%。",
                    critical: 0.06f));
                result.Add(Passive("身かわし", 10, "回避率+5%。",
                    evasion: 0.05f));
                break;
            default:
                result.Add(Passive("槍術鍛錬", 5, "攻撃+3。",
                    attack: 3));
                result.Add(Passive("長柄防御", 10, "防御+3。",
                    defense: 3));
                break;
        }

        if (!IsBaseClass(value))
        {
            string theme = GetJobTheme(value);
            AddRoleMastery(result, baseClass, $"{theme}の心得", 20, 1);
            AddRoleMastery(result, baseClass, $"{theme}の奥義", 30, 2);
        }
        if (IsSpecialClass(value))
        {
            string theme = GetJobTheme(value);
            AddRoleMastery(result, baseClass, $"{theme}の真髄", 40, 3);
            AddRoleMastery(result, baseClass, $"{theme}の極致", 50, 4);
        }
        return result;
    }

    private static void AddRoleMastery(
        List<MercenarySkillDefinition> skills,
        MercenaryClass baseClass,
        string name,
        int level,
        int tier)
    {
        switch (baseClass)
        {
            case MercenaryClass.Warrior:
                skills.Add(Passive(name, level,
                    $"最大HP+{8 * tier}、防御+{tier}。",
                    hp: 8 * tier, defense: tier));
                break;
            case MercenaryClass.Archer:
                skills.Add(Passive(name, level,
                    $"攻撃+{2 * tier}、クリティカル率+{2 * tier}%。",
                    attack: 2 * tier, critical: 0.02f * tier));
                break;
            case MercenaryClass.Mage:
                skills.Add(Passive(name, level,
                    $"攻撃+{2 * tier}、最大魔力+{8 * tier}。",
                    attack: 2 * tier, magic: 8 * tier));
                break;
            case MercenaryClass.Priest:
                skills.Add(Passive(name, level,
                    $"最大HP+{6 * tier}、最大魔力+{8 * tier}。",
                    hp: 6 * tier, magic: 8 * tier));
                break;
            case MercenaryClass.Rogue:
                skills.Add(Passive(name, level,
                    $"速度+{0.02f * tier:0.00}、回避率+{2 * tier}%。",
                    speed: 0.02f * tier, evasion: 0.02f * tier));
                break;
            default:
                skills.Add(Passive(name, level,
                    $"攻撃+{2 * tier}、防御+{tier}。",
                    attack: 2 * tier, defense: tier));
                break;
        }
    }

    private static string GetJobTheme(MercenaryClass value)
    {
        switch (value)
        {
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
            default: return "竜騎士";
        }
    }

    private static MercenarySkillDefinition Passive(
        string name,
        int level,
        string description,
        int hp = 0,
        int attack = 0,
        int defense = 0,
        int magic = 0,
        float speed = 0f,
        float critical = 0f,
        float evasion = 0f)
    {
        return new MercenarySkillDefinition
        {
            Name = name,
            UnlockLevel = level,
            Description = description,
            IsPassive = true,
            BonusMaxHP = hp,
            BonusAttack = attack,
            BonusDefense = defense,
            BonusMaxMagicPower = magic,
            BonusAttackSpeed = speed,
            BonusCriticalRate = critical,
            BonusEvasionRate = evasion
        };
    }

    private static MercenarySkillDefinition Skill(
        MercenarySkillId id, string name, int cost, float power,
        string description, int unlockLevel = 1)
    {
        return new MercenarySkillDefinition
        {
            Id = id, Name = name, MagicCost = cost,
            Power = power, Description = description, UnlockLevel = unlockLevel
        };
    }
}
