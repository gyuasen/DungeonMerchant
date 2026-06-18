using UnityEngine;

[CreateAssetMenu(
    fileName = "DungeonData",
    menuName = "DungeonMerchant/Dungeon Data")]
public class DungeonDataSO : ScriptableObject
{
    [Header("基本情報")]
    public string dungeonName = "はじまりの洞窟";
    public DungeonGrade grade = DungeonGrade.Low;

    [TextArea]
    public string description = "商隊の近くにある小さな洞窟。";

    [Header("遭遇設定")]
    [Min(1)] public int encounterCount = 3;
    [Min(1)] public int firstEncounterEnemyCount = 2;
    [Min(0)] public int enemyCountIncreasePerEncounter = 1;
    public EnemyDataSO[] normalEnemies;
    public EnemyDataSO bossEnemy;

    [Header("踏破報酬")]
    [Min(0)] public int clearGoldReward = 100;
    public DungeonItemReward[] clearItemRewards;
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
