using NUnit.Framework;
using UnityEngine;

public sealed class ItemPresentationServiceTests
{
    private GameObject root;

    [TearDown]
    public void TearDown()
    {
        if (root != null)
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void ResolveSprite_ReturnsCodexSpriteBeforeOtherSources()
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = "AbyssSeal";
        Sprite expected = Resources.Load<Sprite>(
            "UI/Codex/Equipment/AbyssSeal");

        Assert.That(expected, Is.Not.Null);
        Assert.That(ItemPresentationService.ResolveSprite(item), Is.EqualTo(expected));

        Object.DestroyImmediate(item);
    }

    [Test]
    public void ResolveSprite_ReturnsNullWhenNoSpriteExists()
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = "NoItemPresentationSprite";

        Assert.That(ItemPresentationService.ResolveSprite(item), Is.Null);

        Object.DestroyImmediate(item);
    }

    [Test]
    public void BuildDetailText_DescribesEquipmentConsumableAndMaterial()
    {
        ItemDataSO equipment = CreateItem(ItemType.Equipment);
        equipment.equipmentRank = 4;
        equipment.bonusAttack = 12;
        equipment.description = "Equipment description";
        ItemDataSO consumable = CreateItem(ItemType.Consumable);
        consumable.consumableEffect = ConsumableEffectType.HealHP;
        consumable.consumableHealAmount = 25;
        consumable.description = "Consumable description";
        ItemDataSO material = CreateItem(ItemType.Material);
        material.materialClassification = MaterialClassification.CraftingMaterial;
        material.description = "Material description";

        StringAssert.Contains("部位:", ItemPresentationService.BuildDetailText(equipment));
        StringAssert.Contains("攻 +12", ItemPresentationService.BuildDetailText(equipment));
        StringAssert.Contains("HP回復 25", ItemPresentationService.BuildDetailText(consumable));
        StringAssert.Contains("素材分類:", ItemPresentationService.BuildDetailText(material));

        Object.DestroyImmediate(equipment);
        Object.DestroyImmediate(consumable);
        Object.DestroyImmediate(material);
    }

    [Test]
    public void BlacksmithCanCraft_MatchesRecipeMaterialSufficiency()
    {
        root = new GameObject("Blacksmith Detail Availability Test");
        root.AddComponent<MerchantData>();
        MerchantInventory inventory = root.AddComponent<MerchantInventory>();
        BlacksmithManager blacksmith = root.AddComponent<BlacksmithManager>();
        EquipmentRecipeSO recipe = Resources.Load<EquipmentRecipeSO>(
            "GameData/Blacksmith/Expansion/item_expansion_rank4_0Recipe");
        blacksmith.SetTownIndex(0);

        Assert.That(recipe, Is.Not.Null);
        Assert.That(blacksmith.CanCraft(recipe), Is.False);
        foreach (CraftingMaterialRequirement requirement in recipe.materials)
        {
            Assert.That(inventory.TryAddItem(requirement.item, requirement.amount), Is.True);
        }
        Assert.That(blacksmith.CanCraft(recipe), Is.True);

        Assert.That(inventory.TryRemoveItem(recipe.materials[0].item, 1), Is.True);
        Assert.That(blacksmith.CanCraft(recipe), Is.False);
    }

    private static ItemDataSO CreateItem(ItemType itemType)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemType = itemType;
        item.itemName = "Presentation Test";
        item.basePrice = 100;
        return item;
    }
}
