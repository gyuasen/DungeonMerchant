using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class StoryOverlayView
{
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.58f);
    private static readonly Color CategoryTextColor =
        new Color(0.38f, 0.25f, 0.12f, 1f);

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly Transform parent;
    private readonly Color parchmentTextColor;
    private readonly Action onClose;
    private RectTransform overlay;
    private Text titleText;
    private Text bodyText;
    private Text categoryText;
    private Button closeButton;

    public StoryOverlayView(
        SimpleMercenaryHireUIFactory factory,
        Transform parent,
        Color parchmentTextColor,
        Action onClose)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.parchmentTextColor = parchmentTextColor;
        this.onClose = onClose;
    }

    public bool IsShowing => overlay != null && overlay.gameObject.activeSelf;

    public void Build()
    {
        if (overlay != null)
        {
            return;
        }

        overlay = SimpleMercenaryHireUIFactory.CreateUIObject("Story Overlay", parent);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.gameObject.AddComponent<Image>().color = OverlayColor;

        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Story Window", overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 430f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());

        titleText = factory.CreateText(
            window, string.Empty, 30, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(55f, -118f),
            new Vector2(-55f, -55f), parchmentTextColor);
        categoryText = factory.CreateText(
            window, string.Empty, 17, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(55f, -48f),
            new Vector2(-55f, -18f), CategoryTextColor);
        bodyText = factory.CreateText(
            window, string.Empty, 22, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(75f, -300f),
            new Vector2(-75f, -125f), parchmentTextColor);

        closeButton = factory.CreateActionButton(
            window, "物語を閉じる", () => onClose?.Invoke());
        RectTransform buttonRect = closeButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot =
            new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(230f, 58f);
        buttonRect.anchoredPosition = new Vector2(0f, -155f);
        overlay.gameObject.SetActive(false);
    }

    public void Show(StoryPresentation presentation)
    {
        titleText.text = presentation.Title;
        bodyText.text = presentation.Body;
        categoryText.text = presentation.IsOnboarding ? "操作案内" : string.Empty;
        SetCloseButtonLabel(
            presentation.IsOnboarding ? "閉じる" : "物語を閉じる");
        overlay.SetAsLastSibling();
        overlay.gameObject.SetActive(true);
    }

    public void Hide()
    {
        overlay.gameObject.SetActive(false);
    }

    private void SetCloseButtonLabel(string label)
    {
        Text labelText = closeButton.GetComponentInChildren<Text>();
        if (labelText != null)
        {
            labelText.text = label;
        }
    }
}
