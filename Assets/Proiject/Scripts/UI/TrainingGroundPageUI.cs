using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TrainingGroundPageUI : ListPageUIBase
{
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, string> detailsProvider;
    private Func<MercenaryInstance, string> stateProvider;
    private Func<MercenaryInstance, bool> canStartProvider;
    private Action<MercenaryInstance> startTrainingAction;
    private Func<string> descriptionProvider;
    [SerializeField] private ScrollRect scrollRect;

    public bool HasLayout => scrollRect != null && ListRoot != null;

    public void Initialize(
        Text title,
        Text description,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        scrollRect = targetScrollRect;
        base.Initialize(title, description, targetListRoot);
        if (scrollRect != null)
        {
            scrollRect.content = targetListRoot;
        }
    }

    public void ConfigureTrainingGround(
        Func<IEnumerable<MercenaryInstance>> mercenaries,
        Func<MercenaryInstance, string> targetDetailsProvider,
        Func<MercenaryInstance, string> targetStateProvider,
        Func<MercenaryInstance, bool> targetCanStartProvider,
        Action<MercenaryInstance> targetStartTrainingAction,
        Func<string> targetDescriptionProvider)
    {
        mercenaryProvider = mercenaries;
        detailsProvider = targetDetailsProvider;
        stateProvider = targetStateProvider;
        canStartProvider = targetCanStartProvider;
        startTrainingAction = targetStartTrainingAction;
        descriptionProvider = targetDescriptionProvider;
    }

    public void SetDescription(string description)
    {
        if (DescriptionText != null)
        {
            DescriptionText.text = description;
        }
    }

    public override void Refresh()
    {
        SetDescription(descriptionProvider?.Invoke() ?? string.Empty);
        if (mercenaryProvider == null)
        {
            base.Refresh();
            return;
        }

        RebuildRows(
            mercenaryProvider.Invoke(),
            82f,
            430f,
            "No hired mercenaries.",
            mercenary => mercenary != null,
            (_, mercenary, rowTop) => CreateTrainingRow(mercenary, rowTop),
            (_, message) => CreateEmptyMessage(message));
    }

    private void CreateTrainingRow(MercenaryInstance mercenary, float top)
    {
        RectTransform row = CreateRow(
            "Training " + mercenary.InstanceId,
            ListRoot,
            top,
            RowColor,
            FrameColor);
        CreateText(
            row,
            detailsProvider?.Invoke(mercenary) ?? string.Empty,
            RowFont,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -40f),
            new Vector2(-170f, -8f),
            RowTextColor);
        CreateText(
            row,
            stateProvider?.Invoke(mercenary) ?? string.Empty,
            RowFont,
            13,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -72f),
            new Vector2(-170f, -42f),
            MutedTextColor);
        Button button = CreateActionButton(
            row,
            "Train",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => startTrainingAction?.Invoke(mercenary));
        button.interactable = canStartProvider?.Invoke(mercenary) == true;
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(130f, 46f);
        buttonRect.anchoredPosition = new Vector2(-18f, 0f);
    }
}
