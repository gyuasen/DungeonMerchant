using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ItemCodexOverlayView
{
    private static readonly Color OverlayColor = UITheme.ModalOverlayColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.ItemCodexReferences references;
    private readonly Transform parent;
    private readonly Font titleFont;
    private readonly Font bodyFont;
    private readonly ItemCodexPresenter presenter;
    private readonly UnityAction onClose;

    public ItemCodexOverlayView(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.ItemCodexReferences references,
        Transform parent,
        Font titleFont,
        Font bodyFont,
        MerchantInventory merchantInventory,
        UnityAction onClose)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.references = references ?? throw new ArgumentNullException(nameof(references));
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        this.titleFont = titleFont;
        this.bodyFont = bodyFont;
        presenter = new ItemCodexPresenter(merchantInventory);
        this.onClose = onClose;
    }

    public void Build()
    {
        if (references.overlay == null)
        {
            references.overlay = SimpleMercenaryHireUIFactory.CreateUIObject(
                "Item Collection Overlay", parent);
        }

        references.overlay.gameObject.SetActive(false);
        references.overlay.anchorMin = Vector2.zero;
        references.overlay.anchorMax = Vector2.one;
        references.overlay.offsetMin = Vector2.zero;
        references.overlay.offsetMax = Vector2.zero;
        references.overlay.gameObject.AddComponent<Image>().color = OverlayColor;
        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Item Collection Window", references.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 560f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());
        RectTransform bookRoot = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Item Codex Book", window);
        bookRoot.anchorMin = Vector2.zero;
        bookRoot.anchorMax = Vector2.one;
        bookRoot.offsetMin = new Vector2(28f, 28f);
        bookRoot.offsetMax = new Vector2(-28f, -82f);
        references.book = bookRoot.gameObject.AddComponent<BookPageUI>();
        references.book.Initialize(string.Empty, titleFont, bodyFont);
        Button closeButton = factory.CreateActionButton(window, "閉じる", onClose);
        RectTransform closeRect = closeButton.transform as RectTransform;
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(100f, 42f);
        closeRect.anchoredPosition = new Vector2(-18f, -18f);
        references.overlay.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (!references.IsValid)
        {
            return;
        }

        references.book.SetEntries(presenter.BuildEntries());
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
}
