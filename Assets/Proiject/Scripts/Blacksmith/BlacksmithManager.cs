using System;
using System.Collections.Generic;
using UnityEngine;

public class BlacksmithManager : MonoBehaviour
{
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MerchantInventory merchantInventory;
    [SerializeField] private List<EquipmentRecipeSO> recipes =
        new List<EquipmentRecipeSO>();
    [SerializeField] private List<EquipmentRecipeSO> availableRecipes =
        new List<EquipmentRecipeSO>();
    [SerializeField, Range(0, WorldMapService.HiddenIslandTownIndex)]
    private int currentTownIndex = 2;

    public IReadOnlyList<EquipmentRecipeSO> Recipes => availableRecipes;
    public EquipmentInstance LastCraftedEquipment { get; private set; }

    public event Action CraftingChanged;

    public void SetTownIndex(int townIndex)
    {
        currentTownIndex = Mathf.Clamp(
            townIndex,
            0,
            WorldMapService.HiddenIslandTownIndex);
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
            return false;
        }

        for (int i = 0; i < recipe.resultAmount; i++)
        {
            if (recipe.resultItem.IsEquipment)
            {
                EquipmentInstance equipment =
                    EquipmentInstance.CreateFixed(recipe.resultItem);
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
                 GameAssetRepository.LoadAll<EquipmentRecipeSO>())
        {
            AddRecipe(recipe);
        }
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
        return WorldMapService.IsBlacksmithEquipmentAllowedInTown(
            currentTownIndex,
            itemClass,
            item.equipmentRank,
            item.equipmentSlot);
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
