using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UISoundCue
{
    UIClick,
    Confirm,
    Warning,
    BattleAttack,
    Reward
}

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class AudioFeedbackService : MonoBehaviour
{
    public const string VolumePlayerPrefsKey = "DungeonMerchant.Audio.Volume";
    public const float DefaultVolume = 0.8f;

    private const int SampleRate = 44100;

    private static readonly Dictionary<UISoundCue, AudioClip> clipCache =
        new Dictionary<UISoundCue, AudioClip>();

    private readonly HashSet<Button> registeredButtons = new HashSet<Button>();
    private AudioSource audioSource;
    private float volume = DefaultVolume;
    private bool initialized;

    public static AudioFeedbackService Active { get; private set; }

    public float Volume
    {
        get
        {
            Initialize();
            return volume;
        }
        set
        {
            Initialize();
            volume = Mathf.Clamp01(value);
            ApplyVolume();
            PlayerPrefs.SetFloat(VolumePlayerPrefsKey, volume);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        Active = this;
        Initialize();
    }

    public void Initialize()
    {
        if (Active == null)
        {
            Active = this;
        }

        if (initialized)
        {
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        volume = Mathf.Clamp01(PlayerPrefs.GetFloat(
            VolumePlayerPrefsKey,
            DefaultVolume));
        ApplyVolume();
        EnsureClipCache();
        initialized = true;
    }

    private void Start()
    {
        Initialize();
        RegisterButtonsUnder(transform);
    }

    private void OnDestroy()
    {
        foreach (Button button in registeredButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayButtonClick);
            }
        }

        registeredButtons.Clear();
        if (Active == this)
        {
            Active = null;
        }
    }

    public void Play(UISoundCue cue)
    {
        Initialize();

        AudioClip clip = GetOrCreateClip(cue);
        if (audioSource != null && clip != null && volume > 0f)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void RegisterButtonsUnder(Transform root)
    {
        Initialize();

        if (root == null)
        {
            return;
        }

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            RegisterButton(buttons[i]);
        }
    }

    public void RegisterButton(Button button)
    {
        Initialize();
        if (button != null && registeredButtons.Add(button))
        {
            button.onClick.AddListener(PlayButtonClick);
        }
    }

    private void PlayButtonClick()
    {
        Play(UISoundCue.UIClick);
    }

    private void ApplyVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private static void EnsureClipCache()
    {
        Array cues = Enum.GetValues(typeof(UISoundCue));
        for (int i = 0; i < cues.Length; i++)
        {
            GetOrCreateClip((UISoundCue)cues.GetValue(i));
        }
    }

    private static AudioClip GetOrCreateClip(UISoundCue cue)
    {
        if (clipCache.TryGetValue(cue, out AudioClip clip) && clip != null)
        {
            return clip;
        }

        clip = CreateClip(cue);
        clipCache[cue] = clip;
        return clip;
    }

    private static AudioClip CreateClip(UISoundCue cue)
    {
        float duration = GetDuration(cue);
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)SampleRate;
            float progress = i / (float)Mathf.Max(1, sampleCount - 1);
            samples[i] = GenerateSample(cue, time, progress);
        }

        AudioClip clip = AudioClip.Create(
            $"Generated {cue}", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float GetDuration(UISoundCue cue)
    {
        switch (cue)
        {
            case UISoundCue.UIClick:
                return 0.045f;
            case UISoundCue.Confirm:
                return 0.14f;
            case UISoundCue.Warning:
                return 0.2f;
            case UISoundCue.BattleAttack:
                return 0.11f;
            case UISoundCue.Reward:
                return 0.28f;
            default:
                throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
        }
    }

    private static float GenerateSample(
        UISoundCue cue,
        float time,
        float progress)
    {
        float attack = Mathf.Clamp01(progress * 18f);
        float decay = 1f - progress;
        float envelope = attack * decay * decay;

        switch (cue)
        {
            case UISoundCue.UIClick:
                return Mathf.Sin(2f * Mathf.PI * 1100f * time) * envelope * 0.24f;
            case UISoundCue.Confirm:
                return TwoStageTone(time, progress, 660f, 880f) * envelope * 0.25f;
            case UISoundCue.Warning:
                return (Mathf.Sin(2f * Mathf.PI * 220f * time) +
                        0.35f * Mathf.Sin(2f * Mathf.PI * 330f * time)) *
                    envelope * 0.22f;
            case UISoundCue.BattleAttack:
                float frequency = Mathf.Lerp(760f, 120f, progress);
                float noise = DeterministicNoise(time) * (1f - progress);
                return (Mathf.Sin(2f * Mathf.PI * frequency * time) * 0.55f +
                        noise * 0.45f) * envelope * 0.34f;
            case UISoundCue.Reward:
                return RewardTone(time, progress) * envelope * 0.24f;
            default:
                return 0f;
        }
    }

    private static float TwoStageTone(
        float time,
        float progress,
        float firstFrequency,
        float secondFrequency)
    {
        float frequency = progress < 0.45f ? firstFrequency : secondFrequency;
        return Mathf.Sin(2f * Mathf.PI * frequency * time);
    }

    private static float RewardTone(float time, float progress)
    {
        float frequency = progress < 0.33f
            ? 523.25f
            : progress < 0.66f
                ? 659.25f
                : 783.99f;
        return Mathf.Sin(2f * Mathf.PI * frequency * time);
    }

    private static float DeterministicNoise(float time)
    {
        float value = Mathf.Sin(time * 123456.789f) * 43758.5453f;
        return Mathf.Repeat(value, 1f) * 2f - 1f;
    }
}
