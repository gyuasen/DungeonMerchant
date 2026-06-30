using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyData",
    menuName = "DungeonMerchant/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    [Range(1, 10)] public int monsterGrade = 10;
    public bool isBoss;
    public EnemyCategory category = EnemyCategory.Normal;
    public EnemySkillType enemySkill = EnemySkillType.None;
    public bool isSpecialVariant;
    public string specialVariantTitle;

    [Header("Stats")]
    public int maxHP = 80;
    public int attack = 8;
    public int defense = 2;
    [Min(0)] public int maxMagicPower = 40;
    public float attackSpeed = 1.0f;
    [Range(0f, 0.75f)] public float criticalRate;
    [Range(0f, 0.75f)] public float evasionRate;

    [Header("Reward")]
    public int goldReward = 50;
    [Min(1f)] public float experienceMultiplier = 1f;
    public ItemDropEntry[] itemDrops;
}

public enum EnemyCategory
{
    Normal,
    MythicalBeast
}

[System.Serializable]
public class ItemDropEntry
{
    public ItemDataSO item;
    [Min(1)] public int amount = 1;
    [Range(0f, 1f)] public float dropChance = 1f;
}
