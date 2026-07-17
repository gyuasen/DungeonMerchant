using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryGenerator : MonoBehaviour
{
    public const int CandidateSlotCount = 6;
    public const int CandidateRefreshIntervalDays = 3;
    public const int MaximumUniqueCandidateCount = 2;
    public const float UniqueCandidateSlotChance = 0.2f;
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
    [SerializeField] private bool avoidDuplicateNames = true;
    [SerializeField] private bool generateOnAwake = true;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private List<MercenaryDataSO> uniqueCandidatePool =
        new List<MercenaryDataSO>();

    [Header("Generated Candidates")]
    [SerializeField] private List<MercenaryInstance> candidates =
        new List<MercenaryInstance>();
    [SerializeField] private List<MercenaryDataSO> uniqueCandidates =
        new List<MercenaryDataSO>();

    private readonly List<MercenaryArchetypeSO> runtimeFallbackArchetypes =
        new List<MercenaryArchetypeSO>();
    private int currentTownIndex = 2;
    private bool isDayChangedSubscribed;

    public IReadOnlyList<MercenaryInstance> Candidates => candidates;
    public IReadOnlyList<MercenaryDataSO> UniqueCandidates => uniqueCandidates;
    public int CurrentMinimumLevel => TownMinimumLevels[currentTownIndex];
    public int CurrentMaximumLevel => TownMaximumLevels[currentTownIndex];

    public event Action CandidatesChanged;

    public int CurrentCandidateBlock => GetCurrentDay() / CandidateRefreshIntervalDays;

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
        if (candidates.Count == 0 && uniqueCandidates.Count == 0)
        {
            return;
        }
        candidates.Clear();
        uniqueCandidates.Clear();
        CandidatesChanged?.Invoke();
    }

    private void Awake()
    {
        ResolveReferences();
        if (generateOnAwake)
        {
            GenerateCandidates();
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
    }

    private void OnDisable()
    {
        if (dayManager != null && isDayChangedSubscribed)
        {
            dayManager.DayChanged -= HandleDayChanged;
            isDayChangedSubscribed = false;
        }
    }

    public void SetUniqueCandidatePool(
        IEnumerable<MercenaryDataSO> uniqueCandidates,
        bool regenerate = true)
    {
        uniqueCandidatePool.Clear();
        if (uniqueCandidates != null)
        {
            foreach (MercenaryDataSO candidate in uniqueCandidates)
            {
                if (candidate != null && !uniqueCandidatePool.Contains(candidate))
                {
                    uniqueCandidatePool.Add(candidate);
                }
            }
        }
        if (regenerate)
        {
            GenerateCandidates();
        }
    }

    [ContextMenu("Generate Candidates")]
    public void GenerateCandidates()
    {
        // Public entry point: resolve the day manager (and the DayChanged
        // subscription) here as well, so candidate blocks advance even when
        // Awake/OnEnable never ran for this component (EditMode tests add
        // components on an inactive GameObject).
        ResolveReferences();
        List<string> availableNames = ParseNames();
        List<MercenaryArchetypeSO> availableArchetypes = GetAvailableArchetypes();
        System.Random random = new System.Random(
            MarketHashUtility.ComputeRecruitmentSeed(
                currentTownIndex,
                CurrentCandidateBlock));
        candidates.Clear();
        uniqueCandidates.Clear();

        if (availableNames.Count == 0)
        {
            availableNames.AddRange(FallbackNames);
        }

        GenerateUniqueCandidates(random);
        int amount = CandidateSlotCount - uniqueCandidates.Count;
        amount = avoidDuplicateNames
            ? Mathf.Min(amount, availableNames.Count)
            : amount;
        List<MercenaryArchetypeSO> generationArchetypes =
            BuildGenerationArchetypeList(availableArchetypes, amount, random);

        for (int i = 0; i < amount; i++)
        {
            MercenaryArchetypeSO archetype = generationArchetypes[i];
            if (archetype == null)
            {
                continue;
            }

            string generatedName = TakeRandomName(availableNames, random);
            MercenaryInstance candidate =
                CreateMercenary(archetype, generatedName, random);
            int minimumLevel = TownMinimumLevels[currentTownIndex];
            int maximumLevel = TownMaximumLevels[currentTownIndex];
            int generatedLevel = random.Next(minimumLevel, maximumLevel + 1);
            candidate.PrepareAsRecruit(
                generatedLevel,
                TownHireCostMultipliers[currentTownIndex]);
            candidates.Add(candidate);
        }

        CandidatesChanged?.Invoke();
    }

    private List<MercenaryArchetypeSO> BuildGenerationArchetypeList(
        List<MercenaryArchetypeSO> availableArchetypes,
        int amount,
        System.Random random)
    {
        List<MercenaryArchetypeSO> result = new List<MercenaryArchetypeSO>();

        List<MercenaryClass> baseClasses =
            new List<MercenaryClass>(
                MercenaryClassProgression.GetBaseClasses());
        for (int i = baseClasses.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(0, i + 1);
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
            result.Add(GetRandomArchetype(availableArchetypes, random));
        }

        for (int i = result.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(0, i + 1);
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

    public bool RemoveUniqueCandidate(MercenaryDataSO candidate)
    {
        if (candidate == null || !uniqueCandidates.Remove(candidate))
        {
            return false;
        }
        CandidatesChanged?.Invoke();
        return true;
    }

    private void GenerateUniqueCandidates(System.Random random)
    {
        List<MercenaryDataSO> availableCandidates =
            new List<MercenaryDataSO>();
        foreach (MercenaryDataSO candidate in uniqueCandidatePool)
        {
            if (candidate != null && !IsUniqueMercenaryHired(candidate))
            {
                availableCandidates.Add(candidate);
            }
        }

        for (int slot = 0;
             slot < MaximumUniqueCandidateCount && availableCandidates.Count > 0;
             slot++)
        {
            if (random.NextDouble() >= UniqueCandidateSlotChance)
            {
                continue;
            }

            int index = random.Next(0, availableCandidates.Count);
            uniqueCandidates.Add(availableCandidates[index]);
            availableCandidates.RemoveAt(index);
        }
    }

    private bool IsUniqueMercenaryHired(MercenaryDataSO candidate)
    {
        if (hireManager == null)
        {
            return false;
        }

        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && mercenary.BaseData == candidate)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleDayChanged(int currentDay)
    {
        if (currentDay % CandidateRefreshIntervalDays == 0)
        {
            GenerateCandidates();
        }
    }

    private int GetCurrentDay()
    {
        return dayManager != null ? dayManager.CurrentDay : 1;
    }

    private void ResolveReferences()
    {
        if (dayManager == null)
        {
            dayManager = GetComponent<DayManager>() ??
                         FindObjectOfType<DayManager>();
        }
        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>() ??
                          FindObjectOfType<MercenaryHireManager>();
        }
        EnsureDayChangedSubscription();
    }

    private void EnsureDayChangedSubscription()
    {
        if (dayManager == null)
        {
            return;
        }

        dayManager.DayChanged -= HandleDayChanged;
        dayManager.DayChanged += HandleDayChanged;
        isDayChangedSubscribed = true;
    }

    private MercenaryInstance CreateMercenary(
        MercenaryArchetypeSO archetype,
        string generatedName,
        System.Random random)
    {
        return new MercenaryInstance(
            archetype,
            generatedName,
            VaryInt(archetype.baseMaxHP, archetype.statVariation, 1, random),
            VaryInt(archetype.baseAttack, archetype.statVariation, 0, random),
            VaryInt(archetype.baseDefense, archetype.statVariation, 0, random),
            VaryInt(archetype.baseMaxMagicPower, archetype.statVariation, 0, random),
            VaryFloat(archetype.baseAttackSpeed, archetype.statVariation, 0.1f, random),
            VaryInt(archetype.baseHireCost, archetype.hireCostVariation, 0, random));
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
        List<MercenaryArchetypeSO> availableArchetypes,
        System.Random random)
    {
        if (availableArchetypes == null || availableArchetypes.Count == 0)
        {
            return null;
        }

        int index = random.Next(0, availableArchetypes.Count);
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

    private string TakeRandomName(List<string> availableNames, System.Random random)
    {
        int index = random.Next(0, availableNames.Count);
        string selectedName = availableNames[index];

        if (avoidDuplicateNames)
        {
            availableNames.RemoveAt(index);
        }

        return selectedName;
    }

    private static int VaryInt(
        int baseValue,
        float variation,
        int minimum,
        System.Random random)
    {
        float multiplier = Mathf.Lerp(
            1f - variation,
            1f + variation,
            (float)random.NextDouble());
        return Mathf.Max(minimum, Mathf.RoundToInt(baseValue * multiplier));
    }

    private static float VaryFloat(
        float baseValue,
        float variation,
        float minimum,
        System.Random random)
    {
        float multiplier = Mathf.Lerp(
            1f - variation,
            1f + variation,
            (float)random.NextDouble());
        return Mathf.Max(minimum, baseValue * multiplier);
    }
}
