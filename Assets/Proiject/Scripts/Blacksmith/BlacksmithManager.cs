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
    [NonSerialized] private bool recipesPopulated;

    public IReadOnlyList<EquipmentRecipeSO> Recipes
    {
        get
        {
            EnsureRecipesPopulated();
            return availableRecipes;
        }
    }
    public EquipmentInstance LastCraftedEquipment { get; private set; }

    public event Action CraftingChanged;

    public void SetTownIndex(int townIndex)
    {
        int nextTownIndex = Mathf.Clamp(
            townIndex,
            0,
            WorldMapService.HiddenIslandTownIndex);
        bool townChanged = currentTownIndex != nextTownIndex;
        currentTownIndex = nextTownIndex;
        EnsureRecipesPopulated();
        if (townChanged)
        {
            RefreshAvailableRecipes();
        }

        CraftingChanged?.Invoke();
    }

    private void OnEnable()
    {
        ResolveReferences();
        EnsureRecipesPopulated();
    }

    public bool CanCraft(EquipmentRecipeSO recipe)
    {
        ResolveReferences();
        EnsureRecipesPopulated();

        if (recipe == null ||
            recipe.resultItem == null ||
            recipe.resultAmount <= 0 ||
            !availableRecipes.Contains(recipe) ||
            merchantData == null ||
            merchantInventory == null ||
            !merchantData.CanPay(recipe.goldCost) ||
            (!recipe.resultItem.IsEquipment &&
             !merchantInventory.CanAddItem(
                 recipe.resultItem,
                 recipe.resultAmount)))
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

        if (!CanCraft(recipe))
        {
            return false;
        }

        bool addedItems = !recipe.resultItem.IsEquipment;
        if (addedItems &&
            !merchantInventory.TryAddItem(recipe.resultItem, recipe.resultAmount))
        {
            return false;
        }

        if (!merchantData.TryPayGold(recipe.goldCost))
        {
            RollbackCraftedItems(recipe, addedItems);
            return false;
        }

        if (!merchantInventory.TryConsumeMaterials(recipe.materials))
        {
            merchantData.AddGold(recipe.goldCost);
            RollbackCraftedItems(recipe, addedItems);
            return false;
        }

        if (recipe.resultItem.IsEquipment)
        {
            for (int i = 0; i < recipe.resultAmount; i++)
            {
                EquipmentInstance equipment =
                    EquipmentInstance.CreateFixed(recipe.resultItem);
                merchantInventory.AddEquipmentInstance(equipment);
                LastCraftedEquipment = equipment;
            }
        }
        Debug.Log($"Crafted {recipe.resultItem.itemName} x{recipe.resultAmount}");
        CraftingChanged?.Invoke();
        return true;
    }

    private void RollbackCraftedItems(
        EquipmentRecipeSO recipe,
        bool addedItems)
    {
        if (addedItems &&
            !merchantInventory.TryRemoveItem(
                recipe.resultItem,
                recipe.resultAmount))
        {
            Debug.LogError("Failed to roll back a crafted item inventory addition.", this);
        }
    }

    private void EnsureRecipesPopulated()
    {
        if (recipesPopulated)
        {
            return;
        }

        recipes.RemoveAll(recipe => recipe == null);

        foreach (EquipmentRecipeSO recipe in
                 GameAssetRepository.LoadAll<EquipmentRecipeSO>())
        {
            AddRecipe(recipe);
        }

        recipesPopulated = true;
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
        return IsRecipeAvailableInTown(recipe, currentTownIndex);
    }

    public static bool IsRecipeAvailableInTown(
        EquipmentRecipeSO recipe,
        int townIndex)
    {
        if (recipe?.resultItem == null)
        {
            return false;
        }

        ItemDataSO item = recipe.resultItem;
        if (item.itemName == "Mutant Core Charm")
        {
            return townIndex == 6;
        }

        MercenaryClass itemClass =
            MercenaryClassProgression.GetBaseClass(item.requiredClass);
        return WorldMapService.IsBlacksmithEquipmentAllowedInTown(
            townIndex,
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
