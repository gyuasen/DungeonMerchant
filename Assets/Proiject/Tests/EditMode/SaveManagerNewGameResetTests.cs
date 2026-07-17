using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class SaveManagerNewGameResetTests
{
    private GameObject root;
    private string testSavePath;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Save Manager New Game Reset Test");
        testSavePath = Path.Combine(
            Path.GetTempPath(),
            $"dungeon-merchant-new-game-{Guid.NewGuid():N}.json");
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        if (File.Exists(testSavePath))
        {
            File.Delete(testSavePath);
        }
    }

    [Test]
    public void InitializeAndLoad_WithoutSave_ResetsToNewGameDefaults()
    {
        MerchantData merchant = root.AddComponent<MerchantData>();
        DayManager dayManager = root.AddComponent<DayManager>();
        MerchantInventory inventory = root.AddComponent<MerchantInventory>();
        MercenaryHireManager hireManager = root.AddComponent<MercenaryHireManager>();
        root.AddComponent<MercenaryPartyManager>();
        TransportManager transportManager = root.AddComponent<TransportManager>();
        DungeonExpeditionManager expeditionManager =
            root.AddComponent<DungeonExpeditionManager>();
        MonsterCodexManager codex = root.AddComponent<MonsterCodexManager>();
        ProgressionManager progression = root.AddComponent<ProgressionManager>();
        DebtManager debt = root.AddComponent<DebtManager>();
        TownProgressState towns = root.AddComponent<TownProgressState>();
        StoryProgressManager story = root.AddComponent<StoryProgressManager>();
        SaveManager saveManager = root.AddComponent<SaveManager>();
        SetSavePath(saveManager);

        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.name = "Reset Test Item";
        item.itemName = "Reset Test Item";
        merchant.AddGold(900);
        merchant.RestoreSkills(7, 2, 2, 2, 2);
        dayManager.AdvanceDays(12);
        debt.Restore(1, 99, 3);
        towns.RestoreTownProgress(0, new[] { 0, 1, 2 });
        inventory.AddItem(item, 4);
        hireManager.RestoreHiredMercenaries(new[] { CreateMercenary() });
        codex.RestoreEncounteredEnemies(new[] { "old-enemy" });
        progression.Restore(new ProgressionSaveData
        {
            storageTier = 2,
            totalDungeonClears = 9,
            profitableDungeonClears = 4
        });
        story.RestoreCompletedMilestones(
            new[] { StoryMilestone.FirstDungeonClear });

        saveManager.InitializeAndLoad();

        Assert.That(merchant.Gold, Is.EqualTo(500));
        Assert.That(merchant.MerchantLevel, Is.EqualTo(1));
        Assert.That(merchant.MerchantSkillPoints, Is.EqualTo(2));
        Assert.That(dayManager.CurrentDay, Is.EqualTo(1));
        Assert.That(debt.RemainingDebt, Is.EqualTo(DebtManager.InitialDebt));
        Assert.That(debt.PaymentArrears, Is.Zero);
        Assert.That(towns.CurrentTownIndex, Is.EqualTo(2));
        Assert.That(towns.GetUnlockedTownIndices(), Is.EqualTo(new[] { 2 }));
        Assert.That(inventory.GetItemAmount(item), Is.Zero);
        Assert.That(hireManager.HiredMercenaries, Is.Empty);
        Assert.That(transportManager.ActiveConvoys, Is.Empty);
        Assert.That(expeditionManager.ActiveExpeditions, Is.Empty);
        Assert.That(codex.EncounteredEnemyIds, Is.Empty);
        Assert.That(progression.StorageTier, Is.Zero);
        Assert.That(progression.TotalDungeonClears, Is.Zero);
        Assert.That(progression.ProfitableDungeonClears, Is.Zero);
        Assert.That(
            story.CompletedMilestones,
            Is.EquivalentTo(new[] { StoryMilestone.OpeningDebtNotice }));
        UnityEngine.Object.DestroyImmediate(item);
    }

    private void SetSavePath(SaveManager saveManager)
    {
        typeof(SaveManager)
            .GetField(
                "savePathOverride",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(saveManager, testSavePath);
    }

    private static MercenaryInstance CreateMercenary()
    {
        return MercenaryInstance.CreateRestored(
            "reset-test-mercenary",
            null,
            null,
            "Reset Test Mercenary",
            MercenaryClass.Warrior,
            MercenaryContractType.Local,
            1,
            0,
            10,
            10,
            1,
            1,
            0,
            1f,
            0);
    }
}
