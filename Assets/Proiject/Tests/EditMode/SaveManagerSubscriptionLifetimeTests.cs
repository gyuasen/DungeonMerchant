using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class SaveManagerSubscriptionLifetimeTests
{
    private GameObject publisherRoot;
    private GameObject saveManagerRoot;
    private string savePath;

    [SetUp]
    public void SetUp()
    {
        publisherRoot = new GameObject("Save Manager External Publisher Test");
        saveManagerRoot = new GameObject("Save Manager Subscription Lifetime Test");
        savePath = Path.Combine(
            Path.GetTempPath(),
            $"save-manager-subscription-{Guid.NewGuid():N}.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (saveManagerRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(saveManagerRoot);
        }
        if (publisherRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(publisherRoot);
        }
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    [Test]
    public void DestroyedSaveManager_ExternalGoldChanged_DoesNotThrow()
    {
        MerchantData merchantData = publisherRoot.AddComponent<MerchantData>();
        SaveManager saveManager = saveManagerRoot.AddComponent<SaveManager>();
        SetSavePath(saveManager);
        saveManager.InitializeAndLoad();

        UnityEngine.Object.DestroyImmediate(saveManagerRoot);
        saveManagerRoot = null;

        Assert.DoesNotThrow(() => merchantData.AddGold(1));
    }

    private void SetSavePath(SaveManager saveManager)
    {
        typeof(SaveManager)
            .GetField(
                "savePathOverride",
                BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(saveManager, savePath);
    }
}
