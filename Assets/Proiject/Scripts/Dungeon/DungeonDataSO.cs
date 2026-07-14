using UnityEngine;

[CreateAssetMenu(
    fileName = "DungeonData",
    menuName = "DungeonMerchant/Dungeon Data")]
public class DungeonDataSO : ScriptableObject, IPersistentGameAsset
{
    [Header("基本情報")]
    [SerializeField] private string persistentId;
    public string dungeonName = "はじまりの洞窟";
    public DungeonGrade grade = DungeonGrade.Low;
    [Min(0)] public int worldMapIndex;
    [Range(0, WorldMapService.HiddenIslandTownIndex)]
    public int nearbyTownIndex = 2;
    [Min(1)] public int totalFloors = 3;

    [TextArea]
    public string description = "商隊の近くにある小さな洞窟。";

    [Header("遭遇設定")]
    [Min(1)] public int encounterCount = 3;
    [Min(1)] public int firstEncounterEnemyCount = 2;
    [Min(0)] public int enemyCountIncreasePerEncounter = 1;
    [Min(0)] public int enemyCountIncreasePerFloor = 1;
    [Min(1)] public int maxEnemyCountPerEncounter = 5;
    public EnemyDataSO[] normalEnemies;
    public EnemyDataSO bossEnemy;

    [Header("特殊個体")]
    [Range(0f, 1f)] public float specialVariantChance = 0.08f;
    [Range(0f, 1f)] public float specialBossChance = 0.03f;
    public EnemySkillType[] specialVariantSkillPool;

    [Header("Battle Background")]
    [Tooltip("Optional background sprite used for this dungeon's battles.")]
    public Sprite battleBackground;
    [Tooltip("Optional Resources path key under Battle/Backgrounds.")]
    public string battleBackgroundKey;

    [Header("踏破報酬")]
    [Min(0)] public int floorClearGoldReward = 40;
    [Min(0)] public int clearGoldReward = 100;
    public DungeonItemReward[] clearItemRewards;

    [Header("Limited Equipment Drops")]
    [Range(0f, 1f)] public float eventLimitedDropChance = 0.03f;
    [Range(0f, 1f)] public float bossLimitedDropChance = 0.08f;
    public ItemDataSO[] limitedEquipmentDrops;
    public string PersistentId =>
        string.IsNullOrWhiteSpace(persistentId) ? name : persistentId;
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
