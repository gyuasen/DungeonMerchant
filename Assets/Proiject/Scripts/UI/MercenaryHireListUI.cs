using System.Collections.Generic;
using UnityEngine;

public class MercenaryHireListUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private Transform listContent;
    [SerializeField] private MercenaryHireListItemUI itemPrefab;

    [Header("Hire Candidates")]
    [SerializeField] private List<MercenaryDataSO> candidates = new List<MercenaryDataSO>();

    private readonly List<MercenaryHireListItemUI> spawnedItems =
        new List<MercenaryHireListItemUI>();

    private void Start()
    {
        RebuildList();
    }

    public void RebuildList()
    {
        if (!CanBuildList())
        {
            return;
        }

        ClearList();

        foreach (MercenaryDataSO candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            MercenaryHireListItemUI item = Instantiate(itemPrefab, listContent);
            item.Setup(candidate, HireCandidate, hireManager.CanAfford(candidate));
            spawnedItems.Add(item);
        }
    }

    private void HireCandidate(MercenaryDataSO candidate)
    {
        if (hireManager.TryHireMercenary(candidate))
        {
            candidates.Remove(candidate);
            RebuildList();
        }
    }

    private bool CanBuildList()
    {
        if (hireManager == null || listContent == null || itemPrefab == null)
        {
            Debug.LogError("Mercenary hire list references are incomplete.", this);
            return false;
        }

        return true;
    }

    private void ClearList()
    {
        foreach (MercenaryHireListItemUI item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }
}
