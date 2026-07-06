using UnityEngine;

[CreateAssetMenu(
    fileName = "MercenaryArchetype",
    menuName = "DungeonMerchant/Mercenary Archetype")]
public class MercenaryArchetypeSO : ScriptableObject, IPersistentGameAsset
{
    [Header("Basic Info")]
    [SerializeField] private string persistentId;
    public MercenaryClass mercenaryClass;
    public MercenaryContractType contractType = MercenaryContractType.Temporary;

    [Header("Base Stats")]
    [Min(1)] public int baseMaxHP = 100;
    [Min(0)] public int baseAttack = 10;
    [Min(0)] public int baseDefense = 3;
    [Min(0)] public int baseMaxMagicPower = 60;
    [Min(0.1f)] public float baseAttackSpeed = 1f;
    [Min(0)] public int baseHireCost = 100;

    [Header("Random Variation")]
    [Range(0f, 0.5f)] public float statVariation = 0.15f;
    [Range(0f, 0.5f)] public float hireCostVariation = 0.2f;
    public string PersistentId =>
        string.IsNullOrWhiteSpace(persistentId) ? name : persistentId;
}
