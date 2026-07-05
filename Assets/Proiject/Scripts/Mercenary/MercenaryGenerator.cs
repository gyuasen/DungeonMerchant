using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryGenerator : MonoBehaviour
{
    private static readonly string[] FallbackNames =
    {
        "Alden", "Brina", "Cato", "Daria", "Elric",
        "Faris", "Greta", "Hugo", "Ilse", "Jared",
        "Klaus", "Lydia", "Marek", "Nina", "Oskar",
        "Petra", "Quinn", "Rolf", "Sylvia", "Theo",
        "Ulric", "Viola", "Wolfram", "Xenia", "Yoren",
        "Zara", "Arno", "Bianca", "Cedric", "Daphne",
        "Eamon", "Flora", "Gideon", "Hilda", "Ivo",
        "Judith", "Kellan", "Luna", "Magnus", "Nora"
    };

    private static readonly int[] TownMinimumLevels =
    {
        5, 3, 1, 8, 10, 12, 13
    };

    private static readonly int[] TownMaximumLevels =
    {
        8, 5, 3, 10, 12, 14, 15
    };

    private static readonly float[] TownHireCostMultipliers =
    {
        1.25f, 1.1f, 1f, 1.4f, 1.55f, 1.7f, 1.85f
    };

    [Header("Generation Sources")]
    [SerializeField] private TextAsset nameList;
    [SerializeField] private List<MercenaryArchetypeSO> archetypes =
        new List<MercenaryArchetypeSO>();

    [Header("Generation Settings")]
    [SerializeField, Min(1)] private int candidateCount = 5;
    [SerializeField] private bool avoidDuplicateNames = true;
    [SerializeField] private bool generateOnAwake = true;

    [Header("Generated Candidates")]
    [SerializeField] private List<MercenaryInstance> candidates =
        new List<MercenaryInstance>();

    private readonly List<MercenaryArchetypeSO> runtimeFallbackArchetypes =
        new List<MercenaryArchetypeSO>();
    private int currentTownIndex = 2;

    public IReadOnlyList<MercenaryInstance> Candidates => candidates;
    public int CurrentMinimumLevel => TownMinimumLevels[currentTownIndex];
    public int CurrentMaximumLevel => TownMaximumLevels[currentTownIndex];

    public event Action CandidatesChanged;

    public void SetTownIndex(int townIndex, bool regenerate = true)
    {
        currentTownIndex = Mathf.Clamp(
            townIndex, 0, TownMinimumLevels.Length - 1);
        if (regenerate)
        {
            GenerateCandidates();
        }
    }

    public void ClearCandidates()
    {
        if (candidates.Count == 0)
        {
            return;
        }
        candidates.Clear();
        CandidatesChanged?.Invoke();
    }

    private void Awake()
    {
        if (generateOnAwake)
        {
            GenerateCandidates();
        }
    }

    [ContextMenu("Generate Candidates")]
    public void GenerateCandidates()
    {
        List<string> availableNames = ParseNames();
        List<MercenaryArchetypeSO> availableArchetypes = GetAvailableArchetypes();
        candidates.Clear();

        if (availableNames.Count == 0)
        {
            availableNames.AddRange(FallbackNames);
        }

        int amount = avoidDuplicateNames
            ? Mathf.Min(candidateCount, availableNames.Count)
            : candidateCount;
        List<MercenaryArchetypeSO> generationArchetypes =
            BuildGenerationArchetypeList(availableArchetypes, amount);

        for (int i = 0; i < amount; i++)
        {
            MercenaryArchetypeSO archetype = generationArchetypes[i];
            if (archetype == null)
            {
                continue;
            }

            string generatedName = TakeRandomName(availableNames);
            MercenaryInstance candidate =
                CreateMercenary(archetype, generatedName);
            int minimumLevel = TownMinimumLevels[currentTownIndex];
            int maximumLevel = TownMaximumLevels[currentTownIndex];
            int generatedLevel = UnityEngine.Random.Range(
                minimumLevel, maximumLevel + 1);
            candidate.PrepareAsRecruit(
                generatedLevel,
                TownHireCostMultipliers[currentTownIndex]);
            candidates.Add(candidate);
        }

        CandidatesChanged?.Invoke();
    }

    private List<MercenaryArchetypeSO> BuildGenerationArchetypeList(
        List<MercenaryArchetypeSO> availableArchetypes,
        int amount)
    {
        List<MercenaryArchetypeSO> result = new List<MercenaryArchetypeSO>();

        List<MercenaryClass> baseClasses =
            new List<MercenaryClass>(
                MercenaryClassProgression.GetBaseClasses());
        for (int i = baseClasses.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            MercenaryClass temporary = baseClasses[i];
            baseClasses[i] = baseClasses[swapIndex];
            baseClasses[swapIndex] = temporary;
        }

        foreach (MercenaryClass mercenaryClass in baseClasses)
        {
            if (result.Count >= amount)
            {
                break;
            }

            MercenaryArchetypeSO classArchetype = availableArchetypes.Find(
                archetype => archetype.mercenaryClass == mercenaryClass);
            if (classArchetype != null)
            {
                result.Add(classArchetype);
            }
        }

        while (result.Count < amount)
        {
            result.Add(GetRandomArchetype(availableArchetypes));
        }

        for (int i = result.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            MercenaryArchetypeSO temporary = result[i];
            result[i] = result[swapIndex];
            result[swapIndex] = temporary;
        }

        return result;
    }

    public bool RemoveCandidate(MercenaryInstance candidate)
    {
        if (candidate == null || !candidates.Remove(candidate))
        {
            return false;
        }

        CandidatesChanged?.Invoke();
        return true;
    }

    private MercenaryInstance CreateMercenary(
        MercenaryArchetypeSO archetype,
        string generatedName)
    {
        return new MercenaryInstance(
            archetype,
            generatedName,
            VaryInt(archetype.baseMaxHP, archetype.statVariation, 1),
            VaryInt(archetype.baseAttack, archetype.statVariation, 0),
            VaryInt(archetype.baseDefense, archetype.statVariation, 0),
            VaryInt(archetype.baseMaxMagicPower, archetype.statVariation, 0),
            VaryFloat(archetype.baseAttackSpeed, archetype.statVariation, 0.1f),
            VaryInt(archetype.baseHireCost, archetype.hireCostVariation, 0));
    }

    private List<string> ParseNames()
    {
        List<string> names = new List<string>();
        if (nameList == null)
        {
            return names;
        }

        string[] lines = nameList.text.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None);

        HashSet<string> uniqueNames = new HashSet<string>();
        foreach (string line in lines)
        {
            string name = line.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith("#"))
            {
                continue;
            }

            if (uniqueNames.Add(name))
            {
                names.Add(name);
            }
        }

        return names;
    }

    private List<MercenaryArchetypeSO> GetAvailableArchetypes()
    {
        List<MercenaryArchetypeSO> validArchetypes =
            archetypes.FindAll(archetype => archetype != null);

        EnsureClassArchetype(validArchetypes, MercenaryClass.Warrior);
        EnsureClassArchetype(validArchetypes, MercenaryClass.Archer);
        EnsureClassArchetype(validArchetypes, MercenaryClass.Mage);
        EnsureClassArchetype(validArchetypes, MercenaryClass.Priest);
        EnsureClassArchetype(validArchetypes, MercenaryClass.Rogue);
        EnsureClassArchetype(validArchetypes, MercenaryClass.Lancer);

        return validArchetypes;
    }

    private void EnsureClassArchetype(
        List<MercenaryArchetypeSO> availableArchetypes,
        MercenaryClass mercenaryClass)
    {
        foreach (MercenaryArchetypeSO archetype in availableArchetypes)
        {
            if (archetype.mercenaryClass == mercenaryClass)
            {
                return;
            }
        }

        availableArchetypes.Add(GetFallbackArchetype(mercenaryClass));
    }

    private MercenaryArchetypeSO GetRandomArchetype(
        List<MercenaryArchetypeSO> availableArchetypes)
    {
        if (availableArchetypes == null || availableArchetypes.Count == 0)
        {
            return null;
        }

        int index = UnityEngine.Random.Range(0, availableArchetypes.Count);
        return availableArchetypes[index];
    }

    private MercenaryArchetypeSO GetFallbackArchetype(MercenaryClass mercenaryClass)
    {
        foreach (MercenaryArchetypeSO archetype in runtimeFallbackArchetypes)
        {
            if (archetype.mercenaryClass == mercenaryClass)
            {
                return archetype;
            }
        }

        MercenaryArchetypeSO fallback =
            ScriptableObject.CreateInstance<MercenaryArchetypeSO>();
        fallback.name = $"Runtime {mercenaryClass} Archetype";
        fallback.mercenaryClass = mercenaryClass;
        fallback.contractType = MercenaryContractType.Temporary;
        fallback.statVariation = 0.15f;
        fallback.hireCostVariation = 0.2f;

        switch (mercenaryClass)
        {
            case MercenaryClass.Archer:
                fallback.baseMaxHP = 82;
                fallback.baseAttack = 13;
                fallback.baseDefense = 2;
                fallback.baseMaxMagicPower = 75;
                fallback.baseAttackSpeed = 1.25f;
                fallback.baseHireCost = 110;
                break;
            case MercenaryClass.Mage:
                fallback.baseMaxHP = 72;
                fallback.baseAttack = 16;
                fallback.baseDefense = 1;
                fallback.baseMaxMagicPower = 100;
                fallback.baseAttackSpeed = 0.9f;
                fallback.baseHireCost = 120;
                break;
            case MercenaryClass.Priest:
                fallback.baseMaxHP = 80;
                fallback.baseAttack = 9;
                fallback.baseDefense = 3;
                fallback.baseMaxMagicPower = 110;
                fallback.baseAttackSpeed = 0.95f;
                fallback.baseHireCost = 115;
                break;
            case MercenaryClass.Rogue:
                fallback.baseMaxHP = 78;
                fallback.baseAttack = 14;
                fallback.baseDefense = 2;
                fallback.baseMaxMagicPower = 70;
                fallback.baseAttackSpeed = 1.35f;
                fallback.baseHireCost = 120;
                break;
            case MercenaryClass.Lancer:
                fallback.baseMaxHP = 95;
                fallback.baseAttack = 13;
                fallback.baseDefense = 4;
                fallback.baseMaxMagicPower = 70;
                fallback.baseAttackSpeed = 1.05f;
                fallback.baseHireCost = 115;
                break;
            default:
                fallback.baseMaxHP = 100;
                fallback.baseAttack = 10;
                fallback.baseDefense = 3;
                fallback.baseMaxMagicPower = 60;
                fallback.baseAttackSpeed = 1f;
                fallback.baseHireCost = 100;
                break;
        }

        runtimeFallbackArchetypes.Add(fallback);
        return fallback;
    }

    private string TakeRandomName(List<string> availableNames)
    {
        int index = UnityEngine.Random.Range(0, availableNames.Count);
        string selectedName = availableNames[index];

        if (avoidDuplicateNames)
        {
            availableNames.RemoveAt(index);
        }

        return selectedName;
    }

    private static int VaryInt(int baseValue, float variation, int minimum)
    {
        float multiplier = UnityEngine.Random.Range(1f - variation, 1f + variation);
        return Mathf.Max(minimum, Mathf.RoundToInt(baseValue * multiplier));
    }

    private static float VaryFloat(float baseValue, float variation, float minimum)
    {
        float multiplier = UnityEngine.Random.Range(1f - variation, 1f + variation);
        return Mathf.Max(minimum, baseValue * multiplier);
    }
}
