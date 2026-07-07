using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BlacksmithPageUI : EconomyPageUI
{
    private Func<IEnumerable<EquipmentRecipeSO>> recipeProvider;
    private Func<EquipmentRecipeSO, bool> shouldShowRecipe;
    private Action<RectTransform, EquipmentRecipeSO, float> createRow;
    private Action<RectTransform, string> createEmptyMessage;

    public void ConfigureBlacksmith(
        Font font,
        Color color,
        Func<IEnumerable<EquipmentRecipeSO>> recipes,
        Func<EquipmentRecipeSO, bool> shouldShow,
        Action<RectTransform, EquipmentRecipeSO, float> rowFactory,
        Action<RectTransform, string> emptyFactory)
    {
        Configure(font, color, null);
        recipeProvider = recipes;
        shouldShowRecipe = shouldShow;
        createRow = rowFactory;
        createEmptyMessage = emptyFactory;
    }

    public override void Refresh()
    {
        RebuildRows(
            recipeProvider?.Invoke(),
            140f,
            430f,
            "制作可能なレシピはありません。",
            shouldShowRecipe,
            createRow,
            createEmptyMessage);
    }
}
