using UnityEngine;

[CreateAssetMenu(
    fileName = "MercenaryData",
    menuName = "DungeonMerchant/Mercenary Data")]
public class MercenaryDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string mercenaryName;
    public MercenaryClass mercenaryClass;
    public MercenaryContractType contractType;

    [Header("Stats")]
    public int maxHP = 100;
    public int attack = 10;
    public int defense = 3;
    [Min(0)] public int maxMagicPower = 60;
    public float attackSpeed = 1.0f;

    [Header("Employment")]
    public int hireCost = 100;

    [Header("Unique Skill")]
    [Min(1)] public int uniqueSkillUnlockLevel = 3;
    public string uniqueSkillName = "固有技能";
    public int uniqueSkillBonusMaxHP = 10;
    public int uniqueSkillBonusAttack = 3;
    public int uniqueSkillBonusDefense;
    public int uniqueSkillBonusMaxMagicPower;
    public float uniqueSkillBonusAttackSpeed = 0.03f;
}
