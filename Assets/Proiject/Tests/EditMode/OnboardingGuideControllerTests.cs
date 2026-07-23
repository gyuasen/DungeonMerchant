using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class OnboardingGuideControllerTests
{
    private GameObject root;
    private OnboardingGuideController controller;
    private string savePath;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Onboarding Guide Test");
        controller = root.AddComponent<OnboardingGuideController>();
        savePath = Path.Combine(
            Path.GetTempPath(),
            $"onboarding-guide-{Guid.NewGuid():N}.json");
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    [Test]
    public void TryComplete_OnlyAcceptsTheCurrentStep()
    {
        controller.Restore(true, OnboardingGuideStep.Opening);

        Assert.That(
            controller.TryComplete(OnboardingGuideStep.FormParty),
            Is.False);
        Assert.That(controller.CurrentStep, Is.EqualTo(OnboardingGuideStep.Opening));
        Assert.That(
            controller.TryComplete(OnboardingGuideStep.Opening),
            Is.True);
        Assert.That(
            controller.CurrentStep,
            Is.EqualTo(OnboardingGuideStep.HireMercenary));
    }

    [Test]
    public void TryComplete_Opening_AdvancesToHireMercenary()
    {
        controller.Restore(true, OnboardingGuideStep.Opening);

        Assert.That(controller.TryComplete(OnboardingGuideStep.Opening), Is.True);
        Assert.That(
            controller.CurrentStep,
            Is.EqualTo(OnboardingGuideStep.HireMercenary));
    }

    [Test]
    public void Skip_DisablesTheGuidePermanently()
    {
        controller.Restore(true, OnboardingGuideStep.HireMercenary);

        Assert.That(controller.Skip(), Is.True);
        Assert.That(controller.IsEnabled, Is.False);
        Assert.That(controller.CurrentStep, Is.EqualTo(OnboardingGuideStep.Skipped));

        controller.Restore(false, OnboardingGuideStep.Skipped);

        Assert.That(controller.CurrentStep, Is.EqualTo(OnboardingGuideStep.Skipped));
        Assert.That(controller.Advance(), Is.False);
    }

    [Test]
    public void Migrate_Version29_DisablesOnboardingGuide()
    {
        GameSaveData data = new GameSaveData
        {
            version = 29,
            onboardingEnabled = true,
            onboardingStep = OnboardingGuideStep.HireMercenary
        };

        SaveDataMigrator.Migrate(data);

        Assert.That(data.version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(data.onboardingEnabled, Is.False);
        Assert.That(data.onboardingStep, Is.EqualTo(OnboardingGuideStep.Completed));
    }

    [Test]
    public void InitializeAndLoad_Version29Save_DisablesOnboardingGuide()
    {
        File.WriteAllText(
            savePath,
            JsonUtility.ToJson(new GameSaveData
            {
                version = 29,
                onboardingEnabled = true,
                onboardingStep = OnboardingGuideStep.HireMercenary
            }));
        SaveManager saveManager = root.AddComponent<SaveManager>();
        typeof(SaveManager)
            .GetField(
                "savePathOverride",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(saveManager, savePath);

        saveManager.InitializeAndLoad();

        Assert.That(saveManager.HasExistingSaveAtInitialization, Is.True);
        Assert.That(controller.IsEnabled, Is.False);
        Assert.That(controller.CurrentStep, Is.EqualTo(OnboardingGuideStep.Completed));
    }

    [Test]
    public void InitializeAndLoad_OpeningGuideSave_AdvancesToHireMercenary()
    {
        File.WriteAllText(
            savePath,
            JsonUtility.ToJson(new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                onboardingEnabled = true,
                onboardingStep = OnboardingGuideStep.Opening
            }));
        SaveManager saveManager = root.AddComponent<SaveManager>();
        typeof(SaveManager)
            .GetField(
                "savePathOverride",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(saveManager, savePath);

        saveManager.InitializeAndLoad();

        Assert.That(controller.IsEnabled, Is.True);
        Assert.That(
            controller.CurrentStep,
            Is.EqualTo(OnboardingGuideStep.HireMercenary));
    }

    [Test]
    public void Restore_SerializedProgress_ContinuesFromSavedStep()
    {
        controller.Restore(true, OnboardingGuideStep.FormParty);
        GameSaveData saved = new GameSaveData
        {
            onboardingEnabled = controller.IsEnabled,
            onboardingStep = controller.CurrentStep
        };
        GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(
            JsonUtility.ToJson(saved));

        controller.Restore(loaded.onboardingEnabled, loaded.onboardingStep);

        Assert.That(controller.IsEnabled, Is.True);
        Assert.That(
            controller.CurrentStep,
            Is.EqualTo(OnboardingGuideStep.FormParty));
    }

    [Test]
    public void TryComplete_OpenWarehouseWithoutSellableInventory_ReturnsToDeparture()
    {
        controller.Restore(true, OnboardingGuideStep.OpenWarehouse);

        Assert.That(
            controller.TryComplete(OnboardingGuideStep.OpenWarehouse),
            Is.True);
        Assert.That(
            controller.CurrentStep,
            Is.EqualTo(OnboardingGuideStep.DepartDungeon));
    }

    [Test]
    public void Restore_PreservesShownSupplementalCards()
    {
        controller.Restore(
            true,
            OnboardingGuideStep.OpenMarket,
            new[] { OnboardingGuideCard.Warehouse });

        Assert.That(
            controller.ShownCards,
            Does.Contain(OnboardingGuideCard.Warehouse));
    }
}
