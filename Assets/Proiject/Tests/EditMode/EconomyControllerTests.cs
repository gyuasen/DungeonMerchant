using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class EconomyControllerTests
{
    private GameObject root;
    private MerchantInventory inventory;
    private MerchantData merchantData;
    private EconomyController economyController;
    private readonly List<UnityEngine.Object> createdObjects =
        new List<UnityEngine.Object>();
    private string status;
    private int refreshCount;

    [SetUp]
    public void SetUp()
    {
        status = null;
        refreshCount = 0;
        root = new GameObject("Economy Controller Test");
        inventory = root.AddComponent<MerchantInventory>();
        merchantData = root.AddComponent<MerchantData>();
        economyController = new EconomyController(
            inventory,
            null,
            null,
            message => status = message,
            () => { },
            () => { },
            () => { },
            () => refreshCount++,
            _ => { },
            _ => { });
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        foreach (UnityEngine.Object created in createdObjects)
        {
            if (created != null)
            {
                UnityEngine.Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    [Test]
    public void SellItem_WithAmount_RemovesRequestedAmountAndCreditsExactGold()
    {
        ItemDataSO item = CreateItem("Iron Ore", 37);
        inventory.AddItem(item, 5);
        int goldBefore = merchantData.Gold;

        economyController.SellItem(item, 3);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(2));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore + 111));
        Assert.That(status, Does.Contain("3個"));
        Assert.That(status, Does.Contain("111G"));
        Assert.That(refreshCount, Is.EqualTo(1));
    }

    [Test]
    public void SellItem_WithAmountAboveOwned_DoesNotChangeInventoryOrGold()
    {
        ItemDataSO item = CreateItem("Iron Ore", 37);
        inventory.AddItem(item, 2);
        int goldBefore = merchantData.Gold;

        economyController.SellItem(item, 3);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(2));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Does.Contain("売却できませんでした"));
        Assert.That(refreshCount, Is.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void SellItem_WithNonPositiveAmount_IsNoOp(int amount)
    {
        ItemDataSO item = CreateItem("Iron Ore", 37);
        inventory.AddItem(item, 2);
        int goldBefore = merchantData.Gold;

        economyController.SellItem(item, amount);

        Assert.That(inventory.GetItemAmount(item), Is.EqualTo(2));
        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Is.Null);
        Assert.That(refreshCount, Is.Zero);
    }

    [Test]
    public void SellItem_WithNullItem_IsNoOp()
    {
        int goldBefore = merchantData.Gold;

        economyController.SellItem(null, 1);

        Assert.That(merchantData.Gold, Is.EqualTo(goldBefore));
        Assert.That(status, Is.Null);
        Assert.That(refreshCount, Is.Zero);
    }

    private ItemDataSO CreateItem(string itemName, int basePrice)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemName = itemName;
        item.basePrice = basePrice;
        createdObjects.Add(item);
        return item;
    }
}
