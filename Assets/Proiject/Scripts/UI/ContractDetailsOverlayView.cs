using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ContractDetailsOverlayView
{
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color MutedTextColor = UITheme.MutedTextColor;
    private static readonly Color ImportantButtonColor = UITheme.ImportantButtonColor;
    private static readonly Color FrameColor = UITheme.FrameColor;
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.82f);
    private static readonly Color ContractColumnDefaultBackground =
        new Color(0.3f, 0.2f, 0.12f, 0.13f);
    private static readonly Color ContractColumnSelectedBackground =
        new Color(0.36f, 0.24f, 0.1f, 0.32f);
    private static readonly Color ContractColumnLockedBackground =
        new Color(0.12f, 0.1f, 0.08f, 0.28f);

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.ContractDetailsReferences references;
    private readonly Transform parent;
    private readonly MercenaryHireManager hireManager;
    private readonly MerchantData merchantData;
    private readonly UnityAction onClose;
    private readonly List<Text> columnTexts = new List<Text>();
    private readonly List<Image> columnBackgrounds = new List<Image>();
    private readonly List<Outline> columnOutlines = new List<Outline>();

    public ContractDetailsOverlayView(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.ContractDetailsReferences references,
        Transform parent,
        MercenaryHireManager hireManager,
        MerchantData merchantData,
        UnityAction onClose)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.references = references ?? throw new ArgumentNullException(nameof(references));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.hireManager = hireManager ?? throw new ArgumentNullException(nameof(hireManager));
        this.merchantData = merchantData;
        this.onClose = onClose;
    }

    public void Build()
    {
        if (references.overlay != null)
        {
            return;
        }

        references.overlay = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Contract Details Overlay", parent);
        references.overlay.anchorMin = Vector2.zero;
        references.overlay.anchorMax = Vector2.one;
        references.overlay.offsetMin = Vector2.zero;
        references.overlay.offsetMax = Vector2.zero;
        references.overlay.gameObject.AddComponent<Image>().color = OverlayColor;

        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Contract Details Window", references.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(980f, 610f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());
        factory.CreateText(
            window,
            "螂醍ｴ・・隧ｳ邏ｰ",
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
            RectTransform column = SimpleMercenaryHireUIFactory.CreateUIObject(
                "Contract Column", window);
            column.anchorMin = column.anchorMax = column.pivot =
                new Vector2(0.5f, 0.5f);
            column.sizeDelta = new Vector2(286f, 430f);
            column.anchoredPosition = new Vector2(-306f + index * 306f, -18f);
            Image background = column.gameObject.AddComponent<Image>();
            background.color = ContractColumnDefaultBackground;
            Outline outline = column.gameObject.AddComponent<Outline>();
            outline.effectColor = FrameColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            Text text = factory.CreateText(
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
            columnTexts.Add(text);
            columnBackgrounds.Add(background);
            columnOutlines.Add(outline);
        }

        /*
        Button closeButton = factory.CreateActionButton(
            window,
            "閉じる",
            onClose);
        */
        Button closeButton = factory.CreateActionButton(
            window,
            "\u9589\u3058\u308b",
            onClose);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot =
            new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 48f);
        closeRect.anchoredPosition = new Vector2(0f, 24f);
        references.overlay.gameObject.SetActive(false);
    }

    public void Show(MercenaryDataSO candidate)
    {
        if (candidate == null)
        {
            return;
        }

        Show(candidate.mercenaryName, candidate, null);
    }

    public void Show(MercenaryInstance candidate)
    {
        if (candidate == null)
        {
            return;
        }

        Show(candidate.MercenaryName, null, candidate);
    }

    public void Hide()
    {
        if (references.overlay != null)
        {
            references.overlay.gameObject.SetActive(false);
        }
    }

    private void Show(
        string mercenaryName,
        MercenaryDataSO fixedCandidate,
        MercenaryInstance generatedCandidate)
    {
        Build();
        IReadOnlyList<MercenaryContractDetailColumn> columns =
            MercenaryContractDetailModel.BuildColumns(
                hireManager.SelectedContract,
                contractType =>
                    merchantData != null && merchantData.IsContractUnlocked(contractType),
                contractType => fixedCandidate != null
                    ? hireManager.GetInitialContractCost(fixedCandidate, contractType)
                    : hireManager.GetInitialContractCost(generatedCandidate, contractType),
                contractType => fixedCandidate != null
                    ? hireManager.GetRenewalCost(fixedCandidate, contractType)
                    : hireManager.GetRenewalCost(generatedCandidate, contractType));

        for (int index = 0; index < columns.Count; index++)
        {
            ApplyColumn(index, columns[index]);
        }

        references.overlay.name = "Contract Details Overlay " + mercenaryName;
        references.overlay.SetAsLastSibling();
        references.overlay.gameObject.SetActive(true);
    }

    private void ApplyColumn(int index, MercenaryContractDetailColumn column)
    {
        columnTexts[index].text =
            MercenaryContractDetailModel.BuildColumnText(column);
        columnTexts[index].color = column.IsUnlocked
            ? ParchmentTextColor
            : MutedTextColor;

        Image background = columnBackgrounds[index];
        Outline outline = columnOutlines[index];
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
}
