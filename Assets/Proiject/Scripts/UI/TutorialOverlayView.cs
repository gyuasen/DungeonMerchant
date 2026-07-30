using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class TutorialOverlayView
{
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color ParchmentMutedColor = UITheme.ParchmentMutedColor;
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.82f);

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.TutorialReferences references;
    private readonly Transform parent;
    private readonly UnityAction onBack;
    private readonly UnityAction onNext;
    private readonly UnityAction onClose;

    public TutorialOverlayView(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.TutorialReferences references,
        Transform parent,
        UnityAction onBack,
        UnityAction onNext,
        UnityAction onClose)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.references = references ?? throw new ArgumentNullException(nameof(references));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.onBack = onBack;
        this.onNext = onNext;
        this.onClose = onClose;
    }

    public bool IsValid => references.IsValid;

    public void Build()
    {
        if (references.overlay != null)
        {
            return;
        }

        references.overlay = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Tutorial Overlay", parent);
        references.overlay.anchorMin = Vector2.zero;
        references.overlay.anchorMax = Vector2.one;
        references.overlay.offsetMin = Vector2.zero;
        references.overlay.offsetMax = Vector2.zero;
        references.overlay.gameObject.AddComponent<Image>().color = OverlayColor;

        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Tutorial Window", references.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 560f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());

        references.stepText = factory.CreateText(
            window, string.Empty, 15, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(34f, -58f), new Vector2(-34f, -24f),
            ParchmentMutedColor);

        references.titleText = factory.CreateText(
            window, string.Empty, 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(34f, -108f), new Vector2(-34f, -62f),
            ParchmentTextColor);

        Text firstJourneyRouteText = factory.CreateText(
            window, TutorialController.FirstJourneyRoute, 16, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(34f, -158f),
            new Vector2(-34f, -116f), ParchmentMutedColor);
        firstJourneyRouteText.horizontalOverflow = HorizontalWrapMode.Wrap;

        references.bodyText = factory.CreateText(
            window, string.Empty, 19, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(34f, -410f), new Vector2(-34f, -172f),
            ParchmentTextColor);
        references.bodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        references.bodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        references.bodyText.rectTransform.offsetMin = new Vector2(34f, 118f);
        references.bodyText.rectTransform.offsetMax = new Vector2(-34f, -172f);
        references.bodyText.lineSpacing = 1.15f;

        references.backButton = factory.CreateActionButton(
            window, "戻る", onBack);
        RectTransform backRect = references.backButton.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0f);
        backRect.pivot = new Vector2(0f, 0f);
        backRect.sizeDelta = new Vector2(130f, 46f);
        backRect.anchoredPosition = new Vector2(34f, 28f);

        references.closeButton = factory.CreateActionButton(
            window, "閉じる", onClose);
        RectTransform closeRect = references.closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(130f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 28f);

        references.nextButton = factory.CreateActionButton(
            window, "次へ", onNext);
        RectTransform nextRect = references.nextButton.GetComponent<RectTransform>();
        nextRect.anchorMin = nextRect.anchorMax = new Vector2(1f, 0f);
        nextRect.pivot = new Vector2(1f, 0f);
        nextRect.sizeDelta = new Vector2(150f, 46f);
        nextRect.anchoredPosition = new Vector2(-34f, 28f);

        references.overlay.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (!IsValid || references.overlay == null)
        {
            return;
        }

        references.overlay.SetAsLastSibling();
        references.overlay.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (references.overlay != null)
        {
            references.overlay.gameObject.SetActive(false);
        }
    }

    public void SetStepText(string text)
    {
        if (references.stepText != null)
        {
            references.stepText.text = text;
        }
    }

    public void SetTitleText(string text)
    {
        if (references.titleText != null)
        {
            references.titleText.text = text;
        }
    }

    public void SetBodyText(string text)
    {
        if (references.bodyText != null)
        {
            references.bodyText.text = text;
        }
    }

    public void SetBackInteractable(bool value)
    {
        if (references.backButton != null)
        {
            references.backButton.interactable = value;
        }
    }

    public void SetNextButtonLabel(string label)
    {
        if (references.nextButton == null)
        {
            return;
        }

        Text buttonLabel = references.nextButton.GetComponentInChildren<Text>();
        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }
}
