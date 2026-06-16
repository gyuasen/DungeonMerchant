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
    public float attackSpeed = 1.0f;

    [Header("Employment")]
    public int hireCost = 100;
}