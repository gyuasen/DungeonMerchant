using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyData",
    menuName = "DungeonMerchant/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;

    [Header("Stats")]
    public int maxHP = 80;
    public int attack = 8;
    public int defense = 2;
    public float attackSpeed = 1.0f;

    [Header("Reward")]
    public int goldReward = 50;
    public ItemDropEntry[] itemDrops;
}

[System.Serializable]
public class ItemDropEntry
{
    public ItemDataSO item;
    [Min(1)] public int amount = 1;
    [Range(0f, 1f)] public float dropChance = 1f;
}
