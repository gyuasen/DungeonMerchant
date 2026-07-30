using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class DailyResultOverlayView
{
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.84f);
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.DailyResultReferences references;
    private readonly Transform parent;
    private readonly Action onClose;

    public DailyResultOverlayView(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.DailyResultReferences references,
        Transform parent,
        Action onClose)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.references = references ?? throw new ArgumentNullException(nameof(references));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.onClose = onClose;
    }

    public bool IsValid => references.IsValid;
    public bool IsShowing => references.overlay != null &&
                             references.overlay.gameObject.activeSelf;

    public void Build()
    {
        if (references.IsValid)
        {
            return;
        }

        if (references.overlay == null)
        {
            references.overlay = SimpleMercenaryHireUIFactory.CreateUIObject(
                "Daily Result Overlay", parent);
        }

        references.overlay.gameObject.SetActive(false);
        references.overlay.anchorMin = Vector2.zero;
        references.overlay.anchorMax = Vector2.one;
        references.overlay.offsetMin = Vector2.zero;
        references.overlay.offsetMax = Vector2.zero;
        references.overlay.gameObject.AddComponent<Image>().color = OverlayColor;

        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Daily Result Window", references.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 580f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());

        factory.CreateText(
            window, "一日のリザルト", 28, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(130f, -66f),
            new Vector2(-130f, -18f), ParchmentTextColor);

        RectTransform viewport = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Daily Result Viewport", window);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(28f, 76f);
        viewport.offsetMax = new Vector2(-28f, -82f);
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        references.content = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Daily Result Content", viewport);
        references.content.anchorMin = new Vector2(0f, 1f);
        references.content.anchorMax = new Vector2(1f, 1f);
        references.content.pivot = new Vector2(0.5f, 1f);
        references.text = factory.CreateText(
            references.content, string.Empty, 17, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(16f, 16f),
            new Vector2(-16f, -16f), ParchmentTextColor);
        references.text.supportRichText = true;
        references.text.rectTransform.anchorMin = Vector2.zero;
        references.text.rectTransform.anchorMax = Vector2.one;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = references.content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        Button closeButton = factory.CreateActionButton(window, "確認", Close);
        RectTransform closeRect = closeButton.transform as RectTransform;
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(180f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 18f);

        references.overlay.gameObject.SetActive(false);
    }

    public void Show(string resultText)
    {
        if (!IsValid)
        {
            return;
        }

        references.text.text = resultText;
        int lineCount = resultText.Split('\n').Length;
        references.content.sizeDelta =
            new Vector2(0f, Mathf.Max(420f, 40f + lineCount * 34f));
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

    private void Close()
    {
        Hide();
        onClose?.Invoke();
    }

    public void AppendText(string text)
    {
        if (references.text != null)
        {
            references.text.text += "\n" + text;
        }
    }
}
