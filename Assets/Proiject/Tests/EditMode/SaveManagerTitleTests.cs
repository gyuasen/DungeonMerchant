using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class SaveManagerTitleTests
{
    private GameObject root;
    private SaveManager saveManager;
    private string testSavePath;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Save Manager Title Test");
        saveManager = root.AddComponent<SaveManager>();
        testSavePath = Path.Combine(
            Path.GetTempPath(), $"dungeon-merchant-title-{System.Guid.NewGuid():N}.json");
        typeof(SaveManager)
            .GetField("savePathOverride", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(saveManager, testSavePath);
        saveManager.DeleteSaveData();
    }

    [TearDown]
    public void TearDown()
    {
        saveManager.DeleteSaveData();
        Object.DestroyImmediate(root);
        if (File.Exists(testSavePath))
        {
            File.Delete(testSavePath);
        }
    }

    [Test]
    public void HasSaveData_IsFalseAfterDelete()
    {
        Assert.That(saveManager.HasSaveData, Is.False);
    }

    [Test]
    public void DeleteSaveData_DoesNotThrowWhenNoSaveExists()
    {
        Assert.DoesNotThrow(() => saveManager.DeleteSaveData());
        Assert.That(saveManager.HasSaveData, Is.False);
    }

    [Test]
    public void DeleteSaveData_SuppressesQuitAutoSave()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(saveManager.SavePath));
        File.WriteAllText(saveManager.SavePath, "{}");
        saveManager.DeleteSaveData();

        typeof(SaveManager)
            .GetField("initialized", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(saveManager, true);
        typeof(SaveManager)
            .GetMethod("OnApplicationQuit", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(saveManager, null);

        Assert.That(saveManager.HasSaveData, Is.False);
    }
}
