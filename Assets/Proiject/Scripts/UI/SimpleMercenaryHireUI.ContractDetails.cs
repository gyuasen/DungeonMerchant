using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private readonly List<Text> contractDetailsColumnTexts = new List<Text>();
    private readonly List<Image> contractDetailsColumnBackgrounds =
        new List<Image>();
    private readonly List<Outline> contractDetailsColumnOutlines =
        new List<Outline>();

    private static readonly Color ContractColumnDefaultBackground =
        new Color(0.3f, 0.2f, 0.12f, 0.13f);
    private static readonly Color ContractColumnSelectedBackground =
        new Color(0.36f, 0.24f, 0.1f, 0.32f);
    private static readonly Color ContractColumnLockedBackground =
        new Color(0.12f, 0.1f, 0.08f, 0.28f);

    private void ShowContractDetails(MercenaryDataSO candidate)
    {
        if (candidate == null)
        {
            return;
        }

        ShowContractDetails(candidate.mercenaryName, candidate, null);
    }

    private void ShowContractDetails(MercenaryInstance candidate)
    {
        if (candidate == null)
        {
            return;
        }

        ShowContractDetails(candidate.MercenaryName, null, candidate);
    }

    private void ShowContractDetails(
        string mercenaryName,
        MercenaryDataSO fixedCandidate,
        MercenaryInstance generatedCandidate)
    {
        BuildContractDetailsOverlay();
        IReadOnlyList<MercenaryContractDetailColumn> columns =
            MercenaryContractDetailModel.BuildColumns(
                hireManager.SelectedContract,
                contractType =>
                    merchantData != null &&
                    merchantData.IsContractUnlocked(contractType),
                contractType => fixedCandidate != null
                    ? hireManager.GetInitialContractCost(fixedCandidate, contractType)
                    : hireManager.GetInitialContractCost(generatedCandidate, contractType),
                contractType => fixedCandidate != null
                    ? hireManager.GetRenewalCost(fixedCandidate, contractType)
                    : hireManager.GetRenewalCost(generatedCandidate, contractType));

        for (int index = 0; index < columns.Count; index++)
        {
            ApplyContractDetailsColumn(index, columns[index]);
        }

        contractDetailsOverlay.name = "Contract Details Overlay " + mercenaryName;
        contractDetailsOverlay.SetAsLastSibling();
        contractDetailsOverlay.gameObject.SetActive(true);
    }

    private void ApplyContractDetailsColumn(
        int index,
        MercenaryContractDetailColumn column)
    {
        contractDetailsColumnTexts[index].text =
            MercenaryContractDetailModel.BuildColumnText(column);
        contractDetailsColumnTexts[index].color = column.IsUnlocked
            ? ParchmentTextColor
            : MutedTextColor;

        Image background = contractDetailsColumnBackgrounds[index];
        Outline outline = contractDetailsColumnOutlines[index];
        if (column.IsSelected)
        {
            background.color = ContractColumnSelectedBackground;
            outline.effectColor = ImportantButtonColor;
            outline.effectDistance = new Vector2(3f, -3f);
        }
        else if (!column.IsUnlocked)
        {
            background.color = ContractColumnLockedBackground;
            outline.effectColor = FrameColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
        else
        {
            background.color = ContractColumnDefaultBackground;
            outline.effectColor = FrameColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }

    private void BuildContractDetailsOverlay()
    {
        if (contractDetailsOverlay != null)
        {
            return;
        }

        contractDetailsOverlay = CreateUIObject("Contract Details Overlay", overlayRoot);
        contractDetailsOverlay.anchorMin = Vector2.zero;
        contractDetailsOverlay.anchorMax = Vector2.one;
        contractDetailsOverlay.offsetMin = Vector2.zero;
        contractDetailsOverlay.offsetMax = Vector2.zero;
        contractDetailsOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject("Contract Details Window", contractDetailsOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(980f, 610f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(
            window,
            "契約の詳細",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(40f, -70f),
            new Vector2(-40f, -20f),
            ParchmentTextColor);

        MercenaryContractType[] displayOrder =
            MercenaryContractDetailModel.DisplayOrder;
        for (int index = 0; index < displayOrder.Length; index++)
        {
            RectTransform column = CreateUIObject("Contract Column", window);
            column.anchorMin = column.anchorMax = column.pivot =
                new Vector2(0.5f, 0.5f);
            column.sizeDelta = new Vector2(286f, 430f);
            column.anchoredPosition = new Vector2(-306f + index * 306f, -18f);
            Image background = column.gameObject.AddComponent<Image>();
            background.color = ContractColumnDefaultBackground;
            Outline outline = column.gameObject.AddComponent<Outline>();
            outline.effectColor = FrameColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            Text text = CreateText(
                column,
                string.Empty,
                16,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Vector2(18f, -18f),
                new Vector2(-18f, -18f),
                ParchmentTextColor);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            contractDetailsColumnTexts.Add(text);
            contractDetailsColumnBackgrounds.Add(background);
            contractDetailsColumnOutlines.Add(outline);
        }

        Button closeButton = CreateActionButton(
            window,
            "閉じる",
            HideContractDetails);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot =
            new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 48f);
        closeRect.anchoredPosition = new Vector2(0f, 24f);
        contractDetailsOverlay.gameObject.SetActive(false);
    }

    private void HideContractDetails()
    {
        contractDetailsOverlay?.gameObject.SetActive(false);
    }
}
