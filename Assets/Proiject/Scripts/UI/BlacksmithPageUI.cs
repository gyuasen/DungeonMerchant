using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class BlacksmithPageUI : ListPageUIBase
{
    private Func<IEnumerable<EquipmentRecipeSO>> recipeProvider;
    private Func<EquipmentRecipeSO, bool> shouldShowRecipe;
    private Func<ItemDataSO, int> ownedAmountProvider;
    private Func<EquipmentRecipeSO, bool> canCraftRecipe;
    private Action<EquipmentRecipeSO> craftAction;
    private Action<Button, EquipmentRecipeSO> registerCraftButton;

    public void ConfigureBlacksmith(
        Font font,
        Color color,
        Color mutedTextColor,
        Color buttonTextColor,
        Color rowColor,
        Color buttonColor,
        Color frameColor,
        Func<IEnumerable<EquipmentRecipeSO>> recipes,
        Func<EquipmentRecipeSO, bool> shouldShow,
        Func<ItemDataSO, int> targetOwnedAmountProvider,
        Func<EquipmentRecipeSO, bool> targetCanCraftRecipe,
        Action<EquipmentRecipeSO> targetCraftAction,
        Action<Button, EquipmentRecipeSO> targetRegisterCraftButton)
    {
        Configure(
            font,
            color,
            mutedTextColor,
            buttonTextColor,
            rowColor,
            buttonColor,
            frameColor,
            null);
        recipeProvider = recipes;
        shouldShowRecipe = shouldShow;
        ownedAmountProvider = targetOwnedAmountProvider;
        canCraftRecipe = targetCanCraftRecipe;
        craftAction = targetCraftAction;
        registerCraftButton = targetRegisterCraftButton;
    }

    public override void Refresh()
    {
        RebuildRows(
            recipeProvider?.Invoke(),
            140f,
            430f,
            "制作可能なレシピはありません。",
            shouldShowRecipe,
            (_, recipe, rowTop) => CreateBlacksmithRow(recipe, rowTop),
            (_, message) => CreateEmptyMessage(message));
    }

    private void CreateBlacksmithRow(EquipmentRecipeSO recipe, float top)
    {
        ItemDataSO item = recipe.resultItem;
        RectTransform row =
            CreateRow(
                recipe.name,
                ListRoot,
                top,
                RowColor,
                FrameColor);
        row.offsetMin = new Vector2(0f, top - 124f);

        CreateText(
            row,
            JapaneseDisplayText.GetItemName(item),
            RowFont,
            21,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -38f),
            new Vector2(-160f, -8f),
            RowTextColor);

        string stats =
            EquipmentRankPresentation.GetRichText(item) + "  |  " +
            $"{JapaneseDisplayText.GetMercenaryClass(item.requiredClass)}用  |  " +
            $"攻撃+{item.bonusAttack}  " +
            $"防御+{item.bonusDefense}  HP+{item.bonusMaxHP}";
        CreateText(
            row,
            stats,
            RowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -70f),
            new Vector2(-160f, -42f),
            MutedTextColor);

        CreateText(
            row,
            $"{BuildRecipeMaterialText(recipe)}  |  費用 {recipe.goldCost} G",
            RowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(18f, -104f),
            new Vector2(-160f, -76f),
            MutedTextColor);

        Button craftButton = CreateActionButton(
            row,
            "制作",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => craftAction?.Invoke(recipe));
        craftButton.interactable = canCraftRecipe?.Invoke(recipe) == true;
        registerCraftButton?.Invoke(craftButton, recipe);
    }

    private string BuildRecipeMaterialText(EquipmentRecipeSO recipe)
    {
        if (recipe.materials == null || recipe.materials.Length == 0)
        {
            return "素材なし";
        }

        List<string> materialTexts = new List<string>();
        foreach (CraftingMaterialRequirement requirement in recipe.materials)
        {
            if (requirement == null || requirement.item == null)
            {
                continue;
            }

            int owned = ownedAmountProvider?.Invoke(requirement.item) ?? 0;
            materialTexts.Add(
                $"{JapaneseDisplayText.GetItemName(requirement.item)} " +
                $"{owned}/{requirement.amount}");
        }

        return materialTexts.Count > 0
            ? string.Join("、", materialTexts)
            : "素材なし";
    }
}
