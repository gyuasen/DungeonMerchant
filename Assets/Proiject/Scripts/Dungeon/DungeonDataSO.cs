using UnityEngine;

[CreateAssetMenu(
    fileName = "DungeonData",
    menuName = "DungeonMerchant/Dungeon Data")]
public class DungeonDataSO : ScriptableObject
{
    [Header("基本情報")]
    public string dungeonName = "はじまりの洞窟";
    public DungeonGrade grade = DungeonGrade.Low;
    [Min(0)] public int worldMapIndex;
    [Range(0, 2)] public int nearbyTownIndex = 2;
    [Min(1)] public int totalFloors = 3;

    [TextArea]
    public string description = "商隊の近くにある小さな洞窟。";

    [Header("遭遇設定")]
    [Min(1)] public int encounterCount = 3;
    [Min(1)] public int firstEncounterEnemyCount = 2;
    [Min(0)] public int enemyCountIncreasePerEncounter = 1;
    [Min(0)] public int enemyCountIncreasePerFloor = 1;
    public EnemyDataSO[] normalEnemies;
    public EnemyDataSO bossEnemy;

    [Header("踏破報酬")]
    [Min(0)] public int floorClearGoldReward = 40;
    [Min(0)] public int clearGoldReward = 100;
    public DungeonItemReward[] clearItemRewards;

    [Header("Limited Equipment Drops")]
    [Range(0f, 1f)] public float eventLimitedDropChance = 0.03f;
    [Range(0f, 1f)] public float bossLimitedDropChance = 0.08f;
    public ItemDataSO[] limitedEquipmentDrops;
}

public enum DungeonGrade
{
    Low,
    Lower,
    Middle,
    Upper,
    Highest
}

[System.Serializable]
public class DungeonItemReward
{
    public ItemDataSO item;
    [Min(1)] public int amount = 1;
}
