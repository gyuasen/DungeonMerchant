using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int version = 3;
    public int gold = 500;
    public int currentDay = 1;
    public int highestUnlockedDungeonGrade;
    public string selectedDungeonAssetName;
    public List<SavedInventoryItem> inventory = new List<SavedInventoryItem>();
    public List<SavedMercenary> hiredMercenaries = new List<SavedMercenary>();
    public List<string> partyMemberIds = new List<string>();
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
    public string equippedWeaponAssetName;
}
