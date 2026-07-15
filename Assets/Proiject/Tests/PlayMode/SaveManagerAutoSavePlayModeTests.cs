using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class SaveManagerAutoSavePlayModeTests
{
    private GameObject root;
    private string savePath;

    [TearDown]
    public void TearDown()
    {
        if (root != null)
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    [UnityTest]
    public IEnumerator RequestAutoSave_MultipleCalls_CoalescesAndDeleteCancels()
    {
        root = new GameObject("Save Manager Auto Save Test");
        SaveManager manager = root.AddComponent<SaveManager>();
        savePath = Path.Combine(
            Application.temporaryCachePath,
            $"dungeon-merchant-autosave-{Guid.NewGuid():N}.json");
        SetField(manager, "savePathOverride", savePath);

        MethodInfo request = typeof(SaveManager).GetMethod(
            "RequestAutoSave",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(request, Is.Not.Null);

        request.Invoke(manager, null);
        Coroutine first = GetField<Coroutine>(
            manager,
            "pendingAutoSaveCoroutine");
        request.Invoke(manager, null);
        Coroutine second = GetField<Coroutine>(
            manager,
            "pendingAutoSaveCoroutine");

        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.SameAs(first));

        manager.DeleteSaveData();

        Assert.That(
            GetField<Coroutine>(manager, "pendingAutoSaveCoroutine"),
            Is.Null);
        yield return null;
    }

    private static T GetField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        return (T)field.GetValue(target);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        field.SetValue(target, value);
    }
}
