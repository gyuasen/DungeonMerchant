using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryGenerator : MonoBehaviour
{
    private static readonly string[] FallbackNames =
    {
        "Alden",
        "Brina",
        "Cato",
        "Daria",
        "Elric"
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

    private MercenaryArchetypeSO fallbackArchetype;

    public IReadOnlyList<MercenaryInstance> Candidates => candidates;

    public event Action CandidatesChanged;

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

        for (int i = 0; i < amount; i++)
        {
            MercenaryArchetypeSO archetype = GetRandomArchetype(availableArchetypes);
            if (archetype == null)
            {
                continue;
            }

            string generatedName = TakeRandomName(availableNames);
            candidates.Add(CreateMercenary(archetype, generatedName));
        }

        CandidatesChanged?.Invoke();
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

        if (validArchetypes.Count == 0)
        {
            validArchetypes.Add(GetFallbackArchetype());
        }

        return validArchetypes;
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

    private MercenaryArchetypeSO GetFallbackArchetype()
    {
        if (fallbackArchetype != null)
        {
            return fallbackArchetype;
        }

        fallbackArchetype = ScriptableObject.CreateInstance<MercenaryArchetypeSO>();
        fallbackArchetype.name = "Runtime Warrior Archetype";
        fallbackArchetype.mercenaryClass = MercenaryClass.Warrior;
        fallbackArchetype.contractType = MercenaryContractType.Temporary;
        fallbackArchetype.baseMaxHP = 100;
        fallbackArchetype.baseAttack = 10;
        fallbackArchetype.baseDefense = 3;
        fallbackArchetype.baseAttackSpeed = 1f;
        fallbackArchetype.baseHireCost = 100;
        return fallbackArchetype;
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
