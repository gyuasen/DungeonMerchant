using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class AudioFeedbackServiceTests
{
    private GameObject serviceObject;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(AudioFeedbackService.VolumePlayerPrefsKey);
    }

    [TearDown]
    public void TearDown()
    {
        if (serviceObject != null)
        {
            Object.DestroyImmediate(serviceObject);
        }

        PlayerPrefs.DeleteKey(AudioFeedbackService.VolumePlayerPrefsKey);
    }

    [Test]
    public void Awake_CreatesAndCachesEveryCueOnce()
    {
        AudioFeedbackService firstService = CreateService();
        IDictionary cache = GetClipCache();
        var originalClips = new Dictionary<UISoundCue, AudioClip>();

        foreach (UISoundCue cue in System.Enum.GetValues(typeof(UISoundCue)))
        {
            Assert.That(cache.Contains(cue), Is.True);
            AudioClip clip = (AudioClip)cache[cue];
            Assert.That(clip, Is.Not.Null);
            Assert.That(clip.samples, Is.GreaterThan(0));
            originalClips.Add(cue, clip);
        }

        Object.DestroyImmediate(firstService.gameObject);
        serviceObject = null;
        CreateService();

        foreach (KeyValuePair<UISoundCue, AudioClip> pair in originalClips)
        {
            Assert.That(cache[pair.Key], Is.SameAs(pair.Value));
        }
    }

    [Test]
    public void Volume_ClampsAppliesAndPersists()
    {
        AudioFeedbackService service = CreateService();

        service.Volume = 1.5f;

        Assert.That(service.Volume, Is.EqualTo(1f));
        Assert.That(service.GetComponent<AudioSource>().volume, Is.EqualTo(1f));
        Assert.That(
            PlayerPrefs.GetFloat(AudioFeedbackService.VolumePlayerPrefsKey),
            Is.EqualTo(1f));

        service.Volume = 0.35f;
        Object.DestroyImmediate(serviceObject);
        serviceObject = null;

        AudioFeedbackService restoredService = CreateService();
        Assert.That(restoredService.Volume, Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(
            restoredService.GetComponent<AudioSource>().volume,
            Is.EqualTo(0.35f).Within(0.0001f));
    }

    [Test]
    public void RegisterButtonsUnder_RegistersInactiveButtonsWithoutDuplicates()
    {
        AudioFeedbackService service = CreateService();
        GameObject activeButtonObject = new GameObject(
            "Active Button", typeof(RectTransform), typeof(Button));
        GameObject inactiveButtonObject = new GameObject(
            "Inactive Button", typeof(RectTransform), typeof(Button));
        activeButtonObject.transform.SetParent(service.transform, false);
        inactiveButtonObject.transform.SetParent(service.transform, false);
        inactiveButtonObject.SetActive(false);

        service.RegisterButtonsUnder(service.transform);
        service.RegisterButtonsUnder(service.transform);

        Assert.That(GetRegisteredButtonCount(service), Is.EqualTo(2));
    }

    private AudioFeedbackService CreateService()
    {
        serviceObject = new GameObject("Audio Feedback Service");
        AudioFeedbackService service =
            serviceObject.AddComponent<AudioFeedbackService>();
        service.Initialize();
        return service;
    }

    private static IDictionary GetClipCache()
    {
        FieldInfo field = typeof(AudioFeedbackService).GetField(
            "clipCache",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        return (IDictionary)field.GetValue(null);
    }

    private static int GetRegisteredButtonCount(AudioFeedbackService service)
    {
        FieldInfo field = typeof(AudioFeedbackService).GetField(
            "registeredButtons",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        IEnumerable buttons = (IEnumerable)field.GetValue(service);
        int count = 0;
        foreach (object unused in buttons)
        {
            count++;
        }

        return count;
    }
}
