using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlacksmithManager : MonoBehaviour
{
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private List<EquipmentRecipeSO> recipes =
        new List<EquipmentRecipeSO>();
    [SerializeField] private List<EquipmentRecipeSO> availableRecipes =
        new List<EquipmentRecipeSO>();
    [SerializeField, Range(0, 6)] private int currentTownIndex = 2;

    public IReadOnlyList<EquipmentRecipeSO> Recipes => availableRecipes;
    public EquipmentInstance LastCraftedEquipment { get; private set; }

    public event Action CraftingChanged;

    public void SetTownIndex(int townIndex)
    {
        currentTownIndex = Mathf.Clamp(townIndex, 0, 6);
        RefreshAvailableRecipes();
        CraftingChanged?.Invoke();
    }

    private void OnEnable()
    {
        ResolveReferences();
        PopulateRecipesIfNeeded();
        RefreshAvailableRecipes();
    }

    public bool CanCraft(EquipmentRecipeSO recipe)
    {
        ResolveReferences();

        if (recipe == null ||
            recipe.resultItem == null ||
            recipe.resultAmount <= 0 ||
            !availableRecipes.Contains(recipe) ||
            merchantData == null ||
            merchantInventory == null ||
            !merchantData.CanPay(recipe.goldCost))
        {
            return false;
        }

        if (recipe.materials == null)
        {
            return true;
        }

        foreach (CraftingMaterialRequirement requirement in recipe.materials)
        {
            if (requirement == null ||
                requirement.item == null ||
                requirement.amount <= 0 ||
                !merchantInventory.HasItem(requirement.item, requirement.amount))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryCraft(EquipmentRecipeSO recipe)
    {
        ResolveReferences();
        LastCraftedEquipment = null;

        if (!CanCraft(recipe) || !merchantData.TryPayGold(recipe.goldCost))
        {
            return false;
        }

        if (!merchantInventory.TryConsumeMaterials(recipe.materials))
        {
            merchantData.AddGold(recipe.goldCost);
            return false;
        }

        for (int i = 0; i < recipe.resultAmount; i++)
        {
            if (recipe.resultItem.IsEquipment)
            {
                EquipmentInstance equipment =
                    EquipmentInstance.CreateRandom(recipe.resultItem);
                merchantInventory.AddEquipmentInstance(equipment);
                LastCraftedEquipment = equipment;
            }
            else
            {
                merchantInventory.AddItem(recipe.resultItem);
            }
        }
        Debug.Log($"Crafted {recipe.resultItem.itemName} x{recipe.resultAmount}");
        CraftingChanged?.Invoke();
        return true;
    }

    private void PopulateRecipesIfNeeded()
    {
        recipes.RemoveAll(recipe => recipe == null);

        foreach (EquipmentRecipeSO recipe in
                 Resources.LoadAll<EquipmentRecipeSO>(string.Empty))
        {
            AddRecipe(recipe);
        }

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(
            "t:EquipmentRecipeSO",
            new[] { "Assets/Proiject/ScriptableObjects/Blacksmith" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AddRecipe(AssetDatabase.LoadAssetAtPath<EquipmentRecipeSO>(path));
        }
#endif
        RefreshAvailableRecipes();
    }

    private void AddRecipe(EquipmentRecipeSO recipe)
    {
        if (recipe != null && !recipes.Contains(recipe))
        {
            recipes.Add(recipe);
        }
    }

    private void RefreshAvailableRecipes()
    {
        availableRecipes.Clear();
        foreach (EquipmentRecipeSO recipe in recipes)
        {
            if (IsRecipeAvailableInCurrentTown(recipe))
            {
                availableRecipes.Add(recipe);
            }
        }
    }

    private bool IsRecipeAvailableInCurrentTown(EquipmentRecipeSO recipe)
    {
        if (recipe?.resultItem == null)
        {
            return false;
        }

        ItemDataSO item = recipe.resultItem;
        if (item.itemName == "Mutant Core Charm")
        {
            return currentTownIndex == 6;
        }

        MercenaryClass itemClass =
            MercenaryClassProgression.GetBaseClass(item.requiredClass);
        switch (currentTownIndex)
        {
            case 2:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Mage;
            case 1:
                return itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Rogue ||
                       item.equipmentSlot == EquipmentSlot.Accessory;
            case 0:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Priest ||
                       itemClass == MercenaryClass.Lancer ||
                       item.equipmentSlot == EquipmentSlot.Accessory;
            case 3:
                return itemClass == MercenaryClass.Archer ||
                       itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Priest;
            case 4:
                return itemClass == MercenaryClass.Warrior ||
                       itemClass == MercenaryClass.Lancer ||
                       item.equipmentSlot == EquipmentSlot.Armor;
            case 5:
                return itemClass == MercenaryClass.Mage ||
                       itemClass == MercenaryClass.Rogue ||
                       itemClass == MercenaryClass.Lancer ||
                       item.equipmentSlot == EquipmentSlot.Weapon;
            default:
                return item.equipmentRank >= 3;
        }
    }

    private void ResolveReferences()
    {
        merchantData = merchantData != null
            ? merchantData
            : GetComponent<MerchantData>() ?? FindObjectOfType<MerchantData>();
        merchantInventory = merchantInventory != null
            ? merchantInventory
            : GetComponent<MerchantInventory>() ?? FindObjectOfType<MerchantInventory>();
    }
}
