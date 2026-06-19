using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "game-save.json";

    private MerchantData merchantData;
    private DayManager dayManager;
    private MerchantInventory merchantInventory;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private HealingManager healingManager;
    private BattleManager battleManager;
    private DungeonRunManager dungeonRunManager;
    private bool initialized;
    private bool isLoading;

    public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public void InitializeAndLoad()
    {
        if (initialized)
        {
            return;
        }

        ResolveReferences();
        LoadGame();
        Subscribe();
        initialized = true;
    }

    [ContextMenu("ゲームを保存")]
    public void SaveGame()
    {
        if (isLoading)
        {
            return;
        }

        ResolveReferences();
        GameSaveData data = CreateSaveData();
        string directory = Path.GetDirectoryName(SavePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"ゲームを保存しました: {SavePath}");
    }

    [ContextMenu("ゲームを読込")]
    public void LoadGame()
    {
        ResolveReferences();
        if (!File.Exists(SavePath))
        {
            return;
        }

        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(
                File.ReadAllText(SavePath));
            if (data == null)
            {
                return;
            }

            isLoading = true;
            ApplySaveData(data);
            Debug.Log($"ゲームを読み込みました: {SavePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"セーブデータの読込に失敗しました: {exception.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    [ContextMenu("セーブデータを削除")]
    public void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        dungeonRunManager?.ResetDungeonProgress();
        Debug.Log("セーブデータを削除しました。");
    }

    private void OnApplicationQuit()
    {
        if (initialized)
        {
            SaveGame();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized)
        {
            SaveGame();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private GameSaveData CreateSaveData()
    {
        GameSaveData data = new GameSaveData
        {
            gold = merchantData != null ? merchantData.Gold : 500,
            currentDay = dayManager != null ? dayManager.CurrentDay : 1,
            highestUnlockedDungeonGrade = dungeonRunManager != null
                ? (int)dungeonRunManager.HighestUnlockedGrade
                : 0,
            selectedDungeonAssetName =
                dungeonRunManager != null && dungeonRunManager.SelectedDungeon != null
                    ? dungeonRunManager.SelectedDungeon.name
                    : string.Empty
        };

        if (merchantInventory != null)
        {
            foreach (InventoryItemStack stack in merchantInventory.Items)
            {
                if (stack?.Item == null || stack.Amount <= 0)
                {
                    continue;
                }

                data.inventory.Add(new SavedInventoryItem
                {
                    itemAssetName = stack.Item.name,
                    itemName = stack.Item.itemName,
                    amount = stack.Amount
                });
            }
        }

        if (hireManager != null)
        {
            foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
            {
                if (mercenary == null)
                {
                    continue;
                }

                data.hiredMercenaries.Add(new SavedMercenary
                {
                    instanceId = mercenary.InstanceId,
                    baseDataAssetName = mercenary.BaseData != null
                        ? mercenary.BaseData.name
                        : string.Empty,
                    archetypeAssetName = mercenary.Archetype != null
                        ? mercenary.Archetype.name
                        : string.Empty,
                    mercenaryName = mercenary.MercenaryName,
                    mercenaryClass = mercenary.MercenaryClass,
                    contractType = mercenary.ContractType,
                    level = mercenary.Level,
                    currentExperience = mercenary.CurrentExperience,
                    maxHP = mercenary.BaseMaxHP,
                    currentHP = mercenary.CurrentHP,
                    attack = mercenary.BaseAttack,
                    defense = mercenary.BaseDefense,
                    attackSpeed = mercenary.BaseAttackSpeed,
                    hireCost = mercenary.HireCost,
                    equippedWeaponAssetName = mercenary.EquippedWeapon != null
                        ? mercenary.EquippedWeapon.name
                        : string.Empty
                });
            }
        }

        if (partyManager != null)
        {
            foreach (MercenaryInstance mercenary in partyManager.Members)
            {
                if (mercenary != null)
                {
                    data.partyMemberIds.Add(mercenary.InstanceId);
                }
            }
        }

        return data;
    }

    private void ApplySaveData(GameSaveData data)
    {
        merchantData?.SetGold(data.gold);
        dayManager?.SetCurrentDay(data.currentDay);

        List<InventoryItemStack> restoredItems = new List<InventoryItemStack>();
        if (data.inventory != null)
        {
            foreach (SavedInventoryItem savedItem in data.inventory)
            {
                if (savedItem == null)
                {
                    continue;
                }

                ItemDataSO item = FindItem(savedItem.itemAssetName, savedItem.itemName);
                if (item != null && savedItem.amount > 0)
                {
                    restoredItems.Add(new InventoryItemStack(item, savedItem.amount));
                }
            }
        }
        merchantInventory?.RestoreItems(restoredItems);

        List<MercenaryInstance> restoredMercenaries = new List<MercenaryInstance>();
        if (data.hiredMercenaries != null)
        {
            foreach (SavedMercenary savedMercenary in data.hiredMercenaries)
            {
                if (savedMercenary != null)
                {
                    restoredMercenaries.Add(RestoreMercenary(savedMercenary));
                }
            }
        }
        hireManager?.RestoreHiredMercenaries(restoredMercenaries);

        Dictionary<string, MercenaryInstance> mercenaryById =
            new Dictionary<string, MercenaryInstance>();
        foreach (MercenaryInstance mercenary in restoredMercenaries)
        {
            mercenaryById[mercenary.InstanceId] = mercenary;
        }

        List<MercenaryInstance> restoredParty = new List<MercenaryInstance>();
        if (data.partyMemberIds != null)
        {
            foreach (string instanceId in data.partyMemberIds)
            {
                if (!string.IsNullOrWhiteSpace(instanceId) &&
                    mercenaryById.TryGetValue(instanceId, out MercenaryInstance mercenary))
                {
                    restoredParty.Add(mercenary);
                }
            }
        }
        partyManager?.RestoreParty(restoredParty);

        dungeonRunManager?.RestoreProgress(
            (DungeonGrade)Mathf.Clamp(
                data.highestUnlockedDungeonGrade,
                (int)DungeonGrade.Low,
                (int)DungeonGrade.Highest),
            data.selectedDungeonAssetName);
    }

    private MercenaryInstance RestoreMercenary(SavedMercenary saved)
    {
        MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
            saved.instanceId,
            FindMercenaryData(saved.baseDataAssetName),
            FindArchetype(saved.archetypeAssetName),
            saved.mercenaryName,
            saved.mercenaryClass,
            saved.contractType,
            saved.level,
            saved.currentExperience,
            saved.maxHP,
            saved.currentHP,
            saved.attack,
            saved.defense,
            saved.attackSpeed,
            saved.hireCost);
        mercenary.RestoreEquippedWeapon(
            FindItem(saved.equippedWeaponAssetName, string.Empty));
        mercenary.SetCurrentHP(saved.currentHP);
        return mercenary;
    }

    private void Subscribe()
    {
        if (merchantData != null) merchantData.GoldChanged += HandleChanged;
        if (dayManager != null) dayManager.DayChanged += HandleChanged;
        if (merchantInventory != null) merchantInventory.InventoryChanged += HandleChanged;
        if (hireManager != null) hireManager.MercenaryHired += HandleMercenaryChanged;
        if (partyManager != null) partyManager.PartyChanged += HandleChanged;
        if (healingManager != null) healingManager.HealingChanged += HandleChanged;
        if (battleManager != null) battleManager.BattleCompleted += HandleBattleCompleted;
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
            dungeonRunManager.DungeonStateChanged += HandleChanged;
        }
    }

    private void Unsubscribe()
    {
        if (merchantData != null) merchantData.GoldChanged -= HandleChanged;
        if (dayManager != null) dayManager.DayChanged -= HandleChanged;
        if (merchantInventory != null) merchantInventory.InventoryChanged -= HandleChanged;
        if (hireManager != null) hireManager.MercenaryHired -= HandleMercenaryChanged;
        if (partyManager != null) partyManager.PartyChanged -= HandleChanged;
        if (healingManager != null) healingManager.HealingChanged -= HandleChanged;
        if (battleManager != null) battleManager.BattleCompleted -= HandleBattleCompleted;
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
            dungeonRunManager.DungeonStateChanged -= HandleChanged;
        }
    }

    private void HandleChanged() => SaveGame();
    private void HandleChanged(int value) => SaveGame();
    private void HandleMercenaryChanged(MercenaryInstance mercenary) => SaveGame();
    private void HandleBattleCompleted(bool victory) => SaveGame();
    private void HandleDungeonCompleted(bool cleared) => SaveGame();

    private ItemDataSO FindItem(string assetName, string itemName)
    {
        foreach (ItemDataSO item in FindAssets<ItemDataSO>(
            "Assets/Proiject/ScriptableObjects/Items"))
        {
            if (item.name == assetName || item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }

    private MercenaryDataSO FindMercenaryData(string assetName)
    {
        return FindAssetByName<MercenaryDataSO>(
            assetName,
            "Assets/Proiject/ScriptableObjects/Mercenaries");
    }

    private MercenaryArchetypeSO FindArchetype(string assetName)
    {
        return FindAssetByName<MercenaryArchetypeSO>(
            assetName,
            "Assets/Proiject/ScriptableObjects/Mercenaries");
    }

    private T FindAssetByName<T>(string assetName, string editorFolder)
        where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        foreach (T asset in FindAssets<T>(editorFolder))
        {
            if (asset.name == assetName)
            {
                return asset;
            }
        }
        return null;
    }

    private List<T> FindAssets<T>(string editorFolder) where T : UnityEngine.Object
    {
        List<T> assets = new List<T>(Resources.LoadAll<T>(string.Empty));
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { editorFolder });
        foreach (string guid in guids)
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null && !assets.Contains(asset))
            {
                assets.Add(asset);
            }
        }
#endif
        return assets;
    }

    private void ResolveReferences()
    {
        merchantData = GetComponent<MerchantData>() ?? FindObjectOfType<MerchantData>();
        dayManager = GetComponent<DayManager>() ?? FindObjectOfType<DayManager>();
        merchantInventory =
            GetComponent<MerchantInventory>() ?? FindObjectOfType<MerchantInventory>();
        hireManager =
            GetComponent<MercenaryHireManager>() ?? FindObjectOfType<MercenaryHireManager>();
        partyManager =
            GetComponent<MercenaryPartyManager>() ?? FindObjectOfType<MercenaryPartyManager>();
        healingManager =
            GetComponent<HealingManager>() ?? FindObjectOfType<HealingManager>();
        battleManager = GetComponent<BattleManager>() ?? FindObjectOfType<BattleManager>();
        dungeonRunManager =
            GetComponent<DungeonRunManager>() ?? FindObjectOfType<DungeonRunManager>();
    }
}
