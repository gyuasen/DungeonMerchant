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
}