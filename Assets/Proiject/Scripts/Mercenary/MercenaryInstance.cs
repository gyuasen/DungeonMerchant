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
    [SerializeField] private int maxMagicPower;
    [SerializeField] private float attackSpeed;
    [SerializeField] private int hireCost;
    [SerializeField] private int contractEndDay;
    [SerializeField] private bool contractNeedsRenewal;
    [SerializeField] private ItemDataSO equippedWeapon;
    [SerializeField] private EquipmentInstance equippedWeaponInstance;
    [SerializeField] private ItemDataSO equippedArmor;
    [SerializeField] private EquipmentInstance equippedArmorInstance;
    [SerializeField] private ItemDataSO equippedAccessory;
    [SerializeField] private EquipmentInstance equippedAccessoryInstance;

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
    public int MaxHP => maxHP + GetEquipmentBonusMaxHP() + GetSetBonusMaxHP() +
        GetSkillBonusMaxHP();
    public int Attack => attack + GetEquipmentBonusAttack() + GetSetBonusAttack() +
        GetSkillBonusAttack();
    public int Defense => defense + GetEquipmentBonusDefense() + GetSetBonusDefense() +
        GetSkillBonusDefense();
    public float AttackSpeed =>
        attackSpeed + GetEquipmentBonusAttackSpeed() + GetSetBonusAttackSpeed() +
        GetSkillBonusAttackSpeed();
    public int MaxMagicPower => maxMagicPower + GetSkillBonusMaxMagicPower();
    public int HireCost => hireCost;
    public int ContractEndDay => contractEndDay;
    public bool ContractNeedsRenewal => contractNeedsRenewal;
    public bool IsContractActive => !contractNeedsRenewal;
    public string SkillBoardName => IsUnique
        ? $"{mercenaryName}専用スキルボード"
        : $"{mercenaryClass}標準スキルボード";
    public int BaseMaxHP => maxHP;
    public int BaseAttack => attack;
    public int BaseDefense => defense;
    public int BaseMaxMagicPower => maxMagicPower;
    public float BaseAttackSpeed => attackSpeed;
    public ItemDataSO EquippedWeapon =>
        equippedWeaponInstance?.BaseItem ?? equippedWeapon;
    public EquipmentInstance EquippedWeaponInstance => equippedWeaponInstance;
    public ItemDataSO EquippedArmor =>
        equippedArmorInstance?.BaseItem ?? equippedArmor;
    public EquipmentInstance EquippedArmorInstance => equippedArmorInstance;
    public ItemDataSO EquippedAccessory =>
        equippedAccessoryInstance?.BaseItem ?? equippedAccessory;
    public EquipmentInstance EquippedAccessoryInstance =>
        equippedAccessoryInstance;
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
        maxMagicPower = mercenaryData.maxMagicPower;
        attackSpeed = mercenaryData.attackSpeed;
        hireCost = mercenaryData.hireCost;
        ApplyClassStatBonuses();
    }

    public MercenaryInstance(
        MercenaryArchetypeSO sourceArchetype,
        string generatedName,
        int generatedMaxHP,
        int generatedAttack,
        int generatedDefense,
        int generatedMaxMagicPower,
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
        maxMagicPower = generatedMaxMagicPower;
        attackSpeed = generatedAttackSpeed;
        hireCost = generatedHireCost;
        ApplyClassStatBonuses();
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

    public void SetContract(
        MercenaryContractType type,
        int currentDay)
    {
        contractType = type;
        contractNeedsRenewal = false;
        switch (type)
        {
            case MercenaryContractType.Exclusive:
                contractEndDay = 0;
                break;
            case MercenaryContractType.Temporary:
                contractEndDay = currentDay + 6;
                break;
            default:
                contractEndDay = currentDay;
                break;
        }
    }

    public void UpdateContractForDay(int currentDay)
    {
        if (contractType != MercenaryContractType.Exclusive &&
            currentDay > contractEndDay)
        {
            contractNeedsRenewal = true;
        }
    }

    public int GetRenewalCost()
    {
        return contractType == MercenaryContractType.Temporary
            ? Mathf.Max(1, hireCost / 2)
            : Mathf.Max(1, hireCost / 3);
    }

    public void RenewContract(int currentDay)
    {
        SetContract(contractType, currentDay);
    }

    public bool EquipWeapon(ItemDataSO weapon)
    {
        return weapon != null &&
               weapon.equipmentSlot == EquipmentSlot.Weapon &&
               EquipEquipment(weapon);
    }

    public bool EquipWeapon(EquipmentInstance weapon)
    {
        return weapon?.BaseItem != null &&
               weapon.BaseItem.equipmentSlot == EquipmentSlot.Weapon &&
               EquipEquipment(weapon);
    }

    public ItemDataSO UnequipWeapon()
    {
        return UnequipEquipment(EquipmentSlot.Weapon);
    }

    public EquipmentInstance UnequipWeaponInstance()
    {
        return UnequipEquipmentInstance(EquipmentSlot.Weapon);
    }

    public void RestoreEquippedWeapon(ItemDataSO weapon)
    {
        RestoreEquippedEquipment(EquipmentSlot.Weapon, weapon);
    }

    public void RestoreEquippedWeapon(EquipmentInstance weapon)
    {
        RestoreEquippedEquipment(EquipmentSlot.Weapon, weapon);
    }

    public ItemDataSO GetEquippedItem(EquipmentSlot slot)
    {
        EquipmentInstance instance = GetEquippedInstance(slot);
        if (instance != null)
        {
            return instance.BaseItem;
        }

        switch (slot)
        {
            case EquipmentSlot.Armor: return equippedArmor;
            case EquipmentSlot.Accessory: return equippedAccessory;
            default: return equippedWeapon;
        }
    }

    public EquipmentInstance GetEquippedInstance(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Armor: return equippedArmorInstance;
            case EquipmentSlot.Accessory: return equippedAccessoryInstance;
            default: return equippedWeaponInstance;
        }
    }

    public bool EquipEquipment(ItemDataSO equipment)
    {
        if (equipment == null || !equipment.CanEquip(mercenaryClass))
        {
            return false;
        }

        SetEquipment(equipment.equipmentSlot, equipment, null);
        return true;
    }

    public bool EquipEquipment(EquipmentInstance equipment)
    {
        if (equipment?.BaseItem == null ||
            !equipment.BaseItem.CanEquip(mercenaryClass))
        {
            return false;
        }

        SetEquipment(equipment.BaseItem.equipmentSlot, null, equipment);
        return true;
    }

    public ItemDataSO UnequipEquipment(EquipmentSlot slot)
    {
        ItemDataSO previous = GetEquippedInstance(slot) == null
            ? GetEquippedItem(slot)
            : null;
        SetEquipment(slot, null, null);
        return previous;
    }

    public EquipmentInstance UnequipEquipmentInstance(EquipmentSlot slot)
    {
        EquipmentInstance previous = GetEquippedInstance(slot);
        SetEquipment(slot, null, null);
        return previous;
    }

    public void RestoreEquippedEquipment(
        EquipmentSlot slot,
        ItemDataSO equipment)
    {
        bool isValid =
            equipment != null &&
            equipment.equipmentSlot == slot &&
            equipment.CanEquip(mercenaryClass);
        SetEquipment(slot, isValid ? equipment : null, null);
    }

    public void RestoreEquippedEquipment(
        EquipmentSlot slot,
        EquipmentInstance equipment)
    {
        bool isValid =
            equipment?.BaseItem != null &&
            equipment.BaseItem.equipmentSlot == slot &&
            equipment.BaseItem.CanEquip(mercenaryClass);
        SetEquipment(slot, null, isValid ? equipment : null);
    }

    private void SetEquipment(
        EquipmentSlot slot,
        ItemDataSO item,
        EquipmentInstance instance)
    {
        switch (slot)
        {
            case EquipmentSlot.Armor:
                equippedArmor = item;
                equippedArmorInstance = instance;
                break;
            case EquipmentSlot.Accessory:
                equippedAccessory = item;
                equippedAccessoryInstance = instance;
                break;
            default:
                equippedWeapon = item;
                equippedWeaponInstance = instance;
                break;
        }

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
                maxMagicPower += 5;
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
        int restoredMaxMagicPower,
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
            maxMagicPower = Mathf.Max(0, restoredMaxMagicPower),
            attackSpeed = Mathf.Max(0.1f, restoredAttackSpeed),
            hireCost = Mathf.Max(0, restoredHireCost)
        };
        mercenary.EnsureRestoredMagicStat();
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

    public void RestoreContractState(int endDay, bool needsRenewal)
    {
        contractEndDay = endDay;
        contractNeedsRenewal = needsRenewal;
    }

    private int GetEquipmentBonusMaxHP()
    {
        return GetBonusMaxHP(EquipmentSlot.Weapon) +
               GetBonusMaxHP(EquipmentSlot.Armor) +
               GetBonusMaxHP(EquipmentSlot.Accessory);
    }

    private int GetEquipmentBonusAttack()
    {
        return GetBonusAttack(EquipmentSlot.Weapon) +
               GetBonusAttack(EquipmentSlot.Armor) +
               GetBonusAttack(EquipmentSlot.Accessory);
    }

    private int GetEquipmentBonusDefense()
    {
        return GetBonusDefense(EquipmentSlot.Weapon) +
               GetBonusDefense(EquipmentSlot.Armor) +
               GetBonusDefense(EquipmentSlot.Accessory);
    }

    private float GetEquipmentBonusAttackSpeed()
    {
        return GetBonusAttackSpeed(EquipmentSlot.Weapon) +
               GetBonusAttackSpeed(EquipmentSlot.Armor) +
               GetBonusAttackSpeed(EquipmentSlot.Accessory);
    }

    public int GetEquippedSetCount(EquipmentSetId setId)
    {
        if (setId == EquipmentSetId.None)
        {
            return 0;
        }

        int count = 0;
        foreach (EquipmentSlot slot in
                 (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot)))
        {
            ItemDataSO item = GetEquippedItem(slot);
            if (item != null && item.equipmentSet == setId)
            {
                count++;
            }
        }
        return count;
    }

    private int GetSetBonusMaxHP()
    {
        int bonus = 0;
        if (GetEquippedSetCount(EquipmentSetId.AncientGuardian) >= 2) bonus += 30;
        if (GetEquippedSetCount(EquipmentSetId.Vanguard) >= 2) bonus += 20;
        return bonus;
    }

    private int GetSetBonusAttack()
    {
        int bonus = 0;
        if (GetEquippedSetCount(EquipmentSetId.AncientGuardian) >= 3) bonus += 12;
        if (GetEquippedSetCount(EquipmentSetId.Vanguard) >= 3) bonus += 8;
        if (GetEquippedSetCount(EquipmentSetId.Windstalker) >= 2) bonus += 5;
        if (GetEquippedSetCount(EquipmentSetId.Windstalker) >= 3) bonus += 10;
        if (GetEquippedSetCount(EquipmentSetId.ArcaneSage) >= 2) bonus += 10;
        if (GetEquippedSetCount(EquipmentSetId.ArcaneSage) >= 3) bonus += 15;
        return bonus;
    }

    private int GetSetBonusDefense()
    {
        int bonus = 0;
        if (GetEquippedSetCount(EquipmentSetId.AncientGuardian) >= 2) bonus += 8;
        if (GetEquippedSetCount(EquipmentSetId.Vanguard) >= 2) bonus += 10;
        return bonus;
    }

    private float GetSetBonusAttackSpeed()
    {
        float bonus = 0f;
        if (GetEquippedSetCount(EquipmentSetId.AncientGuardian) >= 3) bonus += 0.08f;
        if (GetEquippedSetCount(EquipmentSetId.Windstalker) >= 2) bonus += 0.08f;
        if (GetEquippedSetCount(EquipmentSetId.Windstalker) >= 3) bonus += 0.06f;
        if (GetEquippedSetCount(EquipmentSetId.ArcaneSage) >= 3) bonus += 0.04f;
        return bonus;
    }

    private int GetSkillBonusMaxHP()
    {
        int bonus = mercenaryClass == MercenaryClass.Warrior && level >= 2
            ? 10
            : 0;
        return IsUnique &&
               level >= Mathf.Max(1, baseData.uniqueSkillUnlockLevel)
            ? bonus + baseData.uniqueSkillBonusMaxHP
            : bonus;
    }

    private int GetSkillBonusAttack()
    {
        int bonus = mercenaryClass == MercenaryClass.Mage && level >= 2
            ? 4
            : 0;
        return IsUnique &&
               level >= Mathf.Max(1, baseData.uniqueSkillUnlockLevel)
            ? bonus + baseData.uniqueSkillBonusAttack
            : bonus;
    }

    private int GetSkillBonusDefense()
    {
        int bonus = mercenaryClass == MercenaryClass.Warrior && level >= 2
            ? 3
            : 0;
        return IsUnique &&
               level >= Mathf.Max(1, baseData.uniqueSkillUnlockLevel)
            ? bonus + baseData.uniqueSkillBonusDefense
            : bonus;
    }

    private float GetSkillBonusAttackSpeed()
    {
        float bonus = mercenaryClass == MercenaryClass.Archer && level >= 2
            ? 0.05f
            : 0f;
        return IsUnique &&
               level >= Mathf.Max(1, baseData.uniqueSkillUnlockLevel)
            ? bonus + baseData.uniqueSkillBonusAttackSpeed
            : bonus;
    }

    private int GetSkillBonusMaxMagicPower()
    {
        int bonus = IsUnique &&
                    level >= Mathf.Max(1, baseData.uniqueSkillUnlockLevel)
            ? baseData.uniqueSkillBonusMaxMagicPower
            : 0;
        return bonus;
    }

    private void ApplyClassStatBonuses()
    {
        switch (mercenaryClass)
        {
            case MercenaryClass.Archer:
                attackSpeed += 0.15f;
                break;
            case MercenaryClass.Mage:
                maxMagicPower += 25;
                break;
        }
    }

    private void EnsureRestoredMagicStat()
    {
        if (maxMagicPower > 0)
        {
            return;
        }

        switch (mercenaryClass)
        {
            case MercenaryClass.Mage:
                maxMagicPower = 125;
                break;
            case MercenaryClass.Archer:
                maxMagicPower = 75;
                break;
            default:
                maxMagicPower = 60;
                break;
        }
    }

    private int GetBonusMaxHP(EquipmentSlot slot)
    {
        EquipmentInstance instance = GetEquippedInstance(slot);
        ItemDataSO item = GetEquippedItem(slot);
        return instance != null
            ? instance.BonusMaxHP
            : item != null ? item.bonusMaxHP : 0;
    }

    private int GetBonusAttack(EquipmentSlot slot)
    {
        EquipmentInstance instance = GetEquippedInstance(slot);
        ItemDataSO item = GetEquippedItem(slot);
        return instance != null
            ? instance.BonusAttack
            : item != null ? item.bonusAttack : 0;
    }

    private int GetBonusDefense(EquipmentSlot slot)
    {
        EquipmentInstance instance = GetEquippedInstance(slot);
        ItemDataSO item = GetEquippedItem(slot);
        return instance != null
            ? instance.BonusDefense
            : item != null ? item.bonusDefense : 0;
    }

    private float GetBonusAttackSpeed(EquipmentSlot slot)
    {
        EquipmentInstance instance = GetEquippedInstance(slot);
        ItemDataSO item = GetEquippedItem(slot);
        return instance != null
            ? instance.BonusAttackSpeed
            : item != null ? item.bonusAttackSpeed : 0f;
    }

    private MercenaryInstance()
    {
    }
}
