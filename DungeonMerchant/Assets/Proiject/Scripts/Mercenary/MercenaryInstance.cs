using System;
using UnityEngine;

[Serializable]
public class MercenaryInstance
{
    [SerializeField] private string instanceId;
    [SerializeField] private MercenaryDataSO baseData;
    [SerializeField] private MercenaryArchetypeSO archetype;
    [SerializeField] private string mercenaryName;
    [SerializeField] private MercenaryClass mercenaryClass;
    [SerializeField] private MercenaryContractType contractType;
    [SerializeField] private int level;
    [SerializeField] private int maxHP;
    [SerializeField] private int currentHP;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private float attackSpeed;
    [SerializeField] private int hireCost;

    public string InstanceId => instanceId;
    public MercenaryDataSO BaseData => baseData;
    public MercenaryArchetypeSO Archetype => archetype;
    public string MercenaryName => mercenaryName;
    public MercenaryClass MercenaryClass => mercenaryClass;
    public MercenaryContractType ContractType => contractType;
    public int Level => level;
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public int Attack => attack;
    public int Defense => defense;
    public float AttackSpeed => attackSpeed;
    public int HireCost => hireCost;
    public bool IsUnique => baseData != null;

    public MercenaryInstance(MercenaryDataSO mercenaryData)
    {
        if (mercenaryData == null)
        {
            throw new ArgumentNullException(nameof(mercenaryData));
        }

        instanceId = Guid.NewGuid().ToString("N");
        baseData = mercenaryData;
        mercenaryName = mercenaryData.mercenaryName;
        mercenaryClass = mercenaryData.mercenaryClass;
        contractType = mercenaryData.contractType;
        level = 1;
        maxHP = mercenaryData.maxHP;
        currentHP = maxHP;
        attack = mercenaryData.attack;
        defense = mercenaryData.defense;
        attackSpeed = mercenaryData.attackSpeed;
        hireCost = mercenaryData.hireCost;
    }

    public MercenaryInstance(
        MercenaryArchetypeSO sourceArchetype,
        string generatedName,
        int generatedMaxHP,
        int generatedAttack,
        int generatedDefense,
        float generatedAttackSpeed,
        int generatedHireCost)
    {
        if (sourceArchetype == null)
        {
            throw new ArgumentNullException(nameof(sourceArchetype));
        }

        instanceId = Guid.NewGuid().ToString("N");
        archetype = sourceArchetype;
        mercenaryName = generatedName;
        mercenaryClass = sourceArchetype.mercenaryClass;
        contractType = sourceArchetype.contractType;
        level = 1;
        maxHP = generatedMaxHP;
        currentHP = maxHP;
        attack = generatedAttack;
        defense = generatedDefense;
        attackSpeed = generatedAttackSpeed;
        hireCost = generatedHireCost;
    }
}
