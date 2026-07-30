using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class FacilityGreetingOverlayView
{
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.82f);

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.FacilityGreetingReferences references;
    private readonly Transform parent;
    private readonly UnityAction onEnter;
    private readonly UnityAction onBack;

    public FacilityGreetingOverlayView(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.FacilityGreetingReferences references,
        Transform parent,
        UnityAction onEnter,
        UnityAction onBack)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.references = references ?? throw new ArgumentNullException(nameof(references));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.onEnter = onEnter;
        this.onBack = onBack;
    }

    public bool IsValid => references.IsValid;

    public void Build()
    {
        if (references.overlay != null)
        {
            return;
        }

        references.overlay = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Facility Greeting Overlay", parent);
        references.overlay.anchorMin = Vector2.zero;
        references.overlay.anchorMax = Vector2.one;
        references.overlay.offsetMin = Vector2.zero;
        references.overlay.offsetMax = Vector2.zero;
        references.overlay.gameObject.AddComponent<Image>().color = OverlayColor;

        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Facility Greeting Window", references.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 460f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());

        RectTransform content = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Facility Greeting Content", window);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.offsetMin = new Vector2(32f, 78f);
        content.offsetMax = new Vector2(-32f, -28f);
        references.title = factory.CreateText(
            content, string.Empty, 25, FontStyle.Bold, TextAnchor.UpperLeft,
            new Vector2(0f, -52f), new Vector2(-220f, 0f),
            ParchmentTextColor);
        references.dialogue = factory.CreateText(
            content, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(0f, -215f), new Vector2(-220f, -66f),
            ParchmentTextColor);
        references.dialogue.horizontalOverflow = HorizontalWrapMode.Wrap;
        references.dialogue.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform portraitRect = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Staff Portrait", content);
        portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(1f, 0.5f);
        portraitRect.pivot = new Vector2(1f, 0.5f);
        portraitRect.sizeDelta = new Vector2(185f, 270f);
        portraitRect.anchoredPosition = new Vector2(0f, -8f);
        references.portrait = portraitRect.gameObject.AddComponent<Image>();
        references.portrait.preserveAspect = true;

        Button enterButton = factory.CreateActionButton(window, "入る", onEnter);
        RectTransform enterRect = enterButton.GetComponent<RectTransform>();
        enterRect.anchorMin = enterRect.anchorMax = new Vector2(1f, 0f);
        enterRect.pivot = new Vector2(1f, 0f);
        enterRect.sizeDelta = new Vector2(115f, 44f);
        enterRect.anchoredPosition = new Vector2(-152f, 20f);
        Button backButton = factory.CreateActionButton(window, "戻る", onBack);
        RectTransform backRect = backButton.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(1f, 0f);
        backRect.sizeDelta = new Vector2(115f, 44f);
        backRect.anchoredPosition = new Vector2(-24f, 20f);
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

    public void SetTitle(string text)
    {
        if (references.title != null)
        {
            references.title.text = text;
        }
    }

    public void SetDialogue(string text)
    {
        if (references.dialogue != null)
        {
            references.dialogue.text = text;
        }
    }

    public void SetPortrait(Sprite portrait)
    {
        if (references.portrait == null)
        {
            return;
        }

        references.portrait.sprite = portrait;
        references.portrait.gameObject.SetActive(portrait != null);
    }
}
