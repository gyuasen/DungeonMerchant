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
    [SerializeField] private int currentExperience;
    [SerializeField] private int maxHP;
    [SerializeField] private int currentHP;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private float attackSpeed;
    [SerializeField] private int hireCost;
    [SerializeField] private ItemDataSO equippedWeapon;

    public string InstanceId => instanceId;
    public MercenaryDataSO BaseData => baseData;
    public MercenaryArchetypeSO Archetype => archetype;
    public string MercenaryName => mercenaryName;
    public MercenaryClass MercenaryClass => mercenaryClass;
    public MercenaryContractType ContractType => contractType;
    public int Level => level;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => CalculateExperienceToNextLevel(level);
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP + GetEquipmentBonus(item => item.bonusMaxHP);
    public int Attack => attack + GetEquipmentBonus(item => item.bonusAttack);
    public int Defense => defense + GetEquipmentBonus(item => item.bonusDefense);
    public float AttackSpeed =>
        attackSpeed + (equippedWeapon != null ? equippedWeapon.bonusAttackSpeed : 0f);
    public int HireCost => hireCost;
    public int BaseMaxHP => maxHP;
    public int BaseAttack => attack;
    public int BaseDefense => defense;
    public float BaseAttackSpeed => attackSpeed;
    public ItemDataSO EquippedWeapon => equippedWeapon;
    public bool IsUnique => baseData != null;
    public bool IsIncapacitated => currentHP <= 0;

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
        currentExperience = 0;
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
        currentExperience = 0;
        maxHP = generatedMaxHP;
        currentHP = maxHP;
        attack = generatedAttack;
        defense = generatedDefense;
        attackSpeed = generatedAttackSpeed;
        hireCost = generatedHireCost;
    }

    public void SetCurrentHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, MaxHP);
    }

    public void TakeDamage(int damage)
    {
        SetCurrentHP(currentHP - Mathf.Max(0, damage));
    }

    public void Heal(int amount)
    {
        SetCurrentHP(currentHP + Mathf.Max(0, amount));
    }

    public void RestoreFullHP()
    {
        currentHP = MaxHP;
    }

    public bool EquipWeapon(ItemDataSO weapon)
    {
        if (weapon == null ||
            weapon.equipmentSlot != EquipmentSlot.Weapon ||
            !weapon.CanEquip(mercenaryClass))
        {
            return false;
        }

        equippedWeapon = weapon;
        currentHP = Mathf.Clamp(currentHP, 0, MaxHP);
        return true;
    }

    public ItemDataSO UnequipWeapon()
    {
        ItemDataSO previousWeapon = equippedWeapon;
        equippedWeapon = null;
        currentHP = Mathf.Clamp(currentHP, 0, MaxHP);
        return previousWeapon;
    }

    public void RestoreEquippedWeapon(ItemDataSO weapon)
    {
        equippedWeapon =
            weapon != null &&
            weapon.equipmentSlot == EquipmentSlot.Weapon &&
            weapon.CanEquip(mercenaryClass)
                ? weapon
                : null;
        currentHP = Mathf.Clamp(currentHP, 0, MaxHP);
    }

    public int AddExperience(int amount)
    {
        if (amount <= 0 || level >= 99)
        {
            return 0;
        }

        currentExperience += amount;
        int levelsGained = 0;

        while (level < 99 && currentExperience >= ExperienceToNextLevel)
        {
            currentExperience -= ExperienceToNextLevel;
            LevelUp();
            levelsGained++;
        }

        if (level >= 99)
        {
            currentExperience = 0;
        }

        return levelsGained;
    }

    public static int CalculateExperienceToNextLevel(int targetLevel)
    {
        int safeLevel = Mathf.Max(1, targetLevel);
        int levelOffset = safeLevel - 1;
        return 100 + (40 * levelOffset) + (10 * levelOffset * levelOffset);
    }

    private void LevelUp()
    {
        level++;

        int hpGrowth;
        int attackGrowth;
        int defenseGrowth;
        float speedGrowth;

        switch (mercenaryClass)
        {
            case MercenaryClass.Archer:
                hpGrowth = 8;
                attackGrowth = 3;
                defenseGrowth = 1;
                speedGrowth = 0.03f;
                break;
            case MercenaryClass.Mage:
                hpGrowth = 7;
                attackGrowth = 4;
                defenseGrowth = 1;
                speedGrowth = 0.01f;
                break;
            default:
                hpGrowth = 12;
                attackGrowth = 2;
                defenseGrowth = 2;
                speedGrowth = 0.01f;
                break;
        }

        maxHP += hpGrowth;
        if (currentHP > 0)
        {
            currentHP = Mathf.Min(MaxHP, currentHP + hpGrowth);
        }

        attack += attackGrowth;
        defense += defenseGrowth;
        attackSpeed += speedGrowth;
    }

    public static MercenaryInstance CreateRestored(
        string restoredInstanceId,
        MercenaryDataSO restoredBaseData,
        MercenaryArchetypeSO restoredArchetype,
        string restoredName,
        MercenaryClass restoredClass,
        MercenaryContractType restoredContractType,
        int restoredLevel,
        int restoredCurrentExperience,
        int restoredMaxHP,
        int restoredCurrentHP,
        int restoredAttack,
        int restoredDefense,
        float restoredAttackSpeed,
        int restoredHireCost)
    {
        MercenaryInstance mercenary = new MercenaryInstance
        {
            instanceId = string.IsNullOrWhiteSpace(restoredInstanceId)
                ? Guid.NewGuid().ToString("N")
                : restoredInstanceId,
            baseData = restoredBaseData,
            archetype = restoredArchetype,
            mercenaryName = restoredName,
            mercenaryClass = restoredClass,
            contractType = restoredContractType,
            level = Mathf.Max(1, restoredLevel),
            currentExperience = Mathf.Max(0, restoredCurrentExperience),
            maxHP = Mathf.Max(1, restoredMaxHP),
            attack = Mathf.Max(0, restoredAttack),
            defense = Mathf.Max(0, restoredDefense),
            attackSpeed = Mathf.Max(0.1f, restoredAttackSpeed),
            hireCost = Mathf.Max(0, restoredHireCost)
        };
        mercenary.currentHP = Mathf.Clamp(restoredCurrentHP, 0, mercenary.maxHP);
        if (mercenary.level >= 99)
        {
            mercenary.currentExperience = 0;
        }
        else
        {
            mercenary.currentExperience = Mathf.Min(
                mercenary.currentExperience,
                mercenary.ExperienceToNextLevel - 1);
        }
        return mercenary;
    }

    private int GetEquipmentBonus(Func<ItemDataSO, int> selector)
    {
        return equippedWeapon != null ? selector(equippedWeapon) : 0;
    }

    private MercenaryInstance()
    {
    }
}
