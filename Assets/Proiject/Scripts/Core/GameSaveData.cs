using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int version = 10;
    public int gold = 500;
    public int merchantLevel = 1;
    public int merchantExperience;
    public int merchantSkillPoints = 2;
    public int merchantNegotiation;
    public int merchantLeadership;
    public int merchantAppraisal;
    public int merchantLogistics;
    public int currentDay = 1;
    public int currentTownIndex;
    public int highestUnlockedDungeonGrade;
    public string selectedDungeonAssetName;
    public List<SavedInventoryItem> inventory = new List<SavedInventoryItem>();
    public List<SavedEquipmentInstance> equipmentInventory =
        new List<SavedEquipmentInstance>();
    public List<SavedMercenary> hiredMercenaries = new List<SavedMercenary>();
    public List<string> partyMemberIds = new List<string>();
    public List<string> discoveredEquipmentAssetNames = new List<string>();
    public ProgressionSaveData progression = new ProgressionSaveData();
}

[Serializable]
public class SavedInventoryItem
{
    public string itemAssetName;
    public string itemName;
    public int amount;
}

[Serializable]
public class SavedMercenary
{
    public string instanceId;
    public string baseDataAssetName;
    public string archetypeAssetName;
    public string mercenaryName;
    public MercenaryClass mercenaryClass;
    public MercenaryContractType contractType;
    public int level;
    public int currentExperience;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public float attackSpeed;
    public int hireCost;
    public int contractEndDay;
    public bool contractNeedsRenewal;
    public string equippedWeaponAssetName;
    public SavedEquipmentInstance equippedWeaponInstance;
    public string equippedArmorAssetName;
    public SavedEquipmentInstance equippedArmorInstance;
    public string equippedAccessoryAssetName;
    public SavedEquipmentInstance equippedAccessoryInstance;
}

[Serializable]
public class SavedEquipmentInstance
{
    public string instanceId;
    public string baseItemAssetName;
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
