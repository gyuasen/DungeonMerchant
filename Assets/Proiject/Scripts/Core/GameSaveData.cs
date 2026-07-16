using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 23;

    public int version = CurrentVersion;
    public int gold = 500;
    public int merchantLevel = 1;
    public int merchantExperience;
    public int lifetimeGoldEarned;
    public int merchantSkillPoints = 2;
    public int merchantNegotiation;
    public int merchantLeadership;
    public int merchantAppraisal;
    public int merchantLogistics;
    public int currentDay = 1;
    public int remainingDebt = DebtManager.InitialDebt;
    public int debtPaymentArrears;
    public int processedDebtMonths;
    public int currentTownIndex = 2;
    public List<int> unlockedTownIndices = new List<int> { 2 };
    public int highestUnlockedDungeonGrade;
    public string selectedDungeonAssetName;
    public string selectedDungeonPersistentId;
    public List<SavedDungeonFloorProgress> dungeonFloorProgress =
        new List<SavedDungeonFloorProgress>();
    public List<SavedInventoryItem> inventory = new List<SavedInventoryItem>();
    public List<SavedEquipmentInstance> equipmentInventory =
        new List<SavedEquipmentInstance>();
    public List<SavedMercenary> hiredMercenaries = new List<SavedMercenary>();
    public List<string> partyMemberIds = new List<string>();
    public List<SavedTransportConvoy> transportConvoys =
        new List<SavedTransportConvoy>();
    public List<string> discoveredEquipmentAssetNames = new List<string>();
    public List<string> discoveredEquipmentPersistentIds =
        new List<string>();
    public List<StoryMilestone> completedStoryMilestones =
        new List<StoryMilestone>();
    public ProgressionSaveData progression = new ProgressionSaveData();
}

[Serializable]
public class SavedTransportConvoy
{
    public int originTownIndex;
    public int destinationTownIndex;
    public int remainingDays;
    public int totalSegments;
    public List<SavedTransportCargo> cargo = new List<SavedTransportCargo>();
    public List<string> escortInstanceIds = new List<string>();
}

[Serializable]
public class SavedTransportCargo
{
    public string itemPersistentId;
    public string itemAssetName;
    public int amount;
}

[Serializable]
public class SavedInventoryItem
{
    public int townIndex;
    public string itemPersistentId;
    public string itemAssetName;
    public string itemName;
    public int amount;
}

[Serializable]
public class SavedMercenary
{
    public string instanceId;
    public string baseDataAssetName;
    public string baseDataPersistentId;
    public string archetypeAssetName;
    public string archetypePersistentId;
    public string mercenaryName;
    public MercenaryClass mercenaryClass;
    public MercenaryContractType contractType;
    public int level;
    public int currentExperience;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int maxMagicPower;
    public float attackSpeed;
    public BattleStatusEffect statusEffect;
    public int hireCost;
    public int contractEndDay;
    public bool contractNeedsRenewal;
    public string equippedWeaponAssetName;
    public string equippedWeaponPersistentId;
    public SavedEquipmentInstance equippedWeaponInstance;
    public string equippedArmorAssetName;
    public string equippedArmorPersistentId;
    public SavedEquipmentInstance equippedArmorInstance;
    public string equippedAccessoryAssetName;
    public string equippedAccessoryPersistentId;
    public SavedEquipmentInstance equippedAccessoryInstance;
}

[Serializable]
public class SavedDungeonFloorProgress
{
    public string dungeonPersistentId;
    public string dungeonAssetName;
    public int clearedFloors;
}

[Serializable]
public class SavedEquipmentInstance
{
    public int townIndex;
    public string instanceId;
    public string baseItemAssetName;
    public string baseItemPersistentId;
    public EquipmentQuality quality;
    public int enhancementLevel;
    public bool isLocked;
    public List<SavedEquipmentModifier> modifiers =
        new List<SavedEquipmentModifier>();
}

[Serializable]
public class SavedEquipmentModifier
{
    public EquipmentModifierType type;
    public float value;
}
