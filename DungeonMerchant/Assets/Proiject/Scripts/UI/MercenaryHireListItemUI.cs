using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MercenaryHireListItemUI : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text contractText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button hireButton;

    private MercenaryDataSO mercenary;
    private Action<MercenaryDataSO> onHireClicked;

    public void Setup(
        MercenaryDataSO mercenaryData,
        Action<MercenaryDataSO> hireAction,
        bool canAfford)
    {
        if (mercenaryData == null || !HasRequiredReferences())
        {
            Debug.LogError("Mercenary hire list item references are incomplete.", this);
            gameObject.SetActive(false);
            return;
        }

        mercenary = mercenaryData;
        onHireClicked = hireAction;

        nameText.text = mercenary.mercenaryName;
        classText.text = mercenary.mercenaryClass.ToString();
        contractText.text = mercenary.contractType.ToString();
        costText.text = $"{mercenary.hireCost} G";

        hireButton.interactable = canAfford;
        hireButton.onClick.RemoveListener(NotifyHireClicked);
        hireButton.onClick.AddListener(NotifyHireClicked);
    }

    private void OnDestroy()
    {
        if (hireButton != null)
        {
            hireButton.onClick.RemoveListener(NotifyHireClicked);
        }
    }

    private void NotifyHireClicked()
    {
        onHireClicked?.Invoke(mercenary);
    }

    private bool HasRequiredReferences()
    {
        return nameText != null &&
               classText != null &&
               contractText != null &&
               costText != null &&
               hireButton != null;
    }
}
