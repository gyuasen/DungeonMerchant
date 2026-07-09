using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Stateless/reusable UI-construction helpers extracted from
/// <see cref="SimpleMercenaryHireUI"/>. This class owns only pure UI
/// factory concerns (creating GameObjects, styling buttons/text/panels).
/// Cross-manager refresh logic, page routing, and layout binding remain
/// on <see cref="SimpleMercenaryHireUI"/>.
/// </summary>
public class SimpleMercenaryHireUIFactory
{
    // Duplicated from SimpleMercenaryHireUI.cs. The originals stay there
    // because other partial files still reference some of them directly;
    // unifying the two sets is a possible future cleanup, out of scope here.
    private static readonly Color BackgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
    private static readonly Color PanelColor = new Color(0.13f, 0.15f, 0.18f, 1f);
    private static readonly Color RowColor =
        new Color(0.27f, 0.16f, 0.09f, 0.94f);
    private static readonly Color InactiveColor =
        new Color(0.24f, 0.14f, 0.08f, 0.96f);
    private static readonly Color WoodButtonColor =
        new Color(0.35f, 0.22f, 0.13f, 1f);
    private static readonly Color FrameColor =
        new Color(0.72f, 0.52f, 0.27f, 0.9f);
    private static readonly Color ButtonTextColor =
        new Color(1f, 0.94f, 0.79f, 1f);

    private static Sprite parchmentPanelSprite;

    private readonly Font uiFont;
    private readonly Font uiBodyFont;

    public SimpleMercenaryHireUIFactory(Font uiFont, Font uiBodyFont)
    {
        this.uiFont = uiFont;
        this.uiBodyFont = uiBodyFont;
    }

    public Text CreateText(
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        RectTransform rect = CreateUIObject("Text", parent);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = fontStyle == FontStyle.Normal ? uiBodyFont : uiFont;
        text.fontSize = fontSize;
        // ParchmentTextColor and ParchmentMutedColor (defined on
        // SimpleMercenaryHireUI) are both Color.black, so comparing
        // against Color.black directly is equivalent to the original
        // "color == ParchmentTextColor || color == ParchmentMutedColor"
        // check without needing either constant here.
        bool isDirectParchmentText = color == Color.black;
        text.fontStyle = isDirectParchmentText
            ? FontStyle.Bold
            : fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    public RectTransform CreateRow(string rowName, RectTransform parent, float top)
    {
        GameObject rowPrefab =
            Resources.Load<GameObject>("UI/Templates/ListRow");
        RectTransform row = rowPrefab != null
            ? Object.Instantiate(rowPrefab, parent, false)
                .GetComponent<RectTransform>()
            : CreateUIObject(rowName, parent);
        row.name = rowName;
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 96f);
        row.offsetMax = new Vector2(0f, top);

        Image rowImage = row.GetComponent<Image>() ??
            row.gameObject.AddComponent<Image>();
        rowImage.color = RowColor;
        if (row.GetComponent<Outline>() == null)
        {
            AddFantasyFrame(rowImage, 1f);
        }
        return row;
    }

    public Button CreateNavigationButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateButtonFromPrefab(
            "UI/Templates/NavigationButton",
            $"{label} Tab",
            parent);
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.sizeDelta = new Vector2(84f, 38f);
        buttonRect.anchoredPosition = position;

        Image image = buttonRect.GetComponent<Image>() ??
            buttonRect.gameObject.AddComponent<Image>();
        image.color = InactiveColor;
        if (buttonRect.GetComponent<Outline>() == null)
        {
            AddFantasyFrame(image, 1.5f);
        }

        Button button = buttonRect.GetComponent<Button>() ??
            buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        ApplyButtonTransitions(button);
        SetOrCreateButtonLabel(buttonRect, label, 15);
        return button;
    }

    public Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateButtonFromPrefab(
            "UI/Templates/ActionButton",
            "Action Button",
            parent);
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.sizeDelta = new Vector2(130f, 52f);
        buttonRect.anchoredPosition = new Vector2(-18f, 0f);

        Image image = buttonRect.GetComponent<Image>() ??
            buttonRect.gameObject.AddComponent<Image>();
        image.color = WoodButtonColor;
        if (buttonRect.GetComponent<Outline>() == null)
        {
            AddFantasyFrame(image, 1.5f);
        }

        Button button = buttonRect.GetComponent<Button>() ??
            buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        ApplyButtonTransitions(button);
        SetOrCreateButtonLabel(buttonRect, label, 17);
        return button;
    }

    public static RectTransform CreateButtonFromPrefab(
        string resourcePath,
        string objectName,
        RectTransform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        RectTransform result = prefab != null
            ? Object.Instantiate(prefab, parent, false)
                .GetComponent<RectTransform>()
            : CreateUIObject(objectName, parent);
        result.name = objectName;
        return result;
    }

    public void SetOrCreateButtonLabel(
        RectTransform parent,
        string label,
        int fontSize)
    {
        Text existing = parent.GetComponentInChildren<Text>();
        if (existing == null)
        {
            CreateButtonLabel(parent, label, fontSize);
            return;
        }

        existing.text = label;
        existing.font = uiFont;
        existing.fontSize = fontSize;
        existing.fontStyle = FontStyle.Bold;
        existing.alignment = TextAnchor.MiddleCenter;
        existing.color = ButtonTextColor;
    }

    public void CreateButtonLabel(RectTransform parent, string label, int fontSize)
    {
        Text buttonText = CreateText(parent, label, fontSize, FontStyle.Bold,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero,
            ButtonTextColor);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;
    }

    public static void AddFantasyFrame(Image image, float thickness)
    {
        Outline outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = FrameColor;
        outline.effectDistance = new Vector2(thickness, -thickness);
        outline.useGraphicAlpha = true;
    }

    public static void ApplyButtonTransitions(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.18f, 1.12f, 0.96f, 1f);
        colors.pressedColor = new Color(0.76f, 0.68f, 0.56f, 1f);
        colors.selectedColor = new Color(1.08f, 1.02f, 0.88f, 1f);
        colors.disabledColor = new Color(0.42f, 0.38f, 0.32f, 0.72f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    public static void ApplyParchmentPanel(Image target)
    {
        if (parchmentPanelSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("UI/ParchmentPanel");
            if (texture != null)
            {
                texture.wrapMode = TextureWrapMode.Clamp;
                Rect paperRect = new Rect(
                    72f,
                    40f,
                    texture.width - 144f,
                    texture.height - 84f);
                parchmentPanelSprite = Sprite.Create(
                    texture,
                    paperRect,
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(54f, 54f, 54f, 54f));
            }
        }

        if (parchmentPanelSprite == null)
        {
            target.color = PanelColor;
            return;
        }

        target.sprite = parchmentPanelSprite;
        target.type = Image.Type.Sliced;
        target.color = Color.white;
    }

    public static void ConfigurePrefabText(
        Text text,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
    }

    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public RectTransform CreatePanel(Transform parent)
    {
        RectTransform panel = CreateUIObject("Guild Panel", parent);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820f, 620f);
        panel.anchoredPosition = Vector2.zero;

        Image panelImage = panel.gameObject.AddComponent<Image>();
        ApplyParchmentPanel(panelImage);
        return panel;
    }

    public static RectTransform CreatePage(string pageName, RectTransform parent)
    {
        RectTransform page = CreateUIObject(pageName, parent);
        page.anchorMin = new Vector2(0f, 0f);
        page.anchorMax = new Vector2(1f, 1f);
        page.offsetMin = new Vector2(28f, 64f);
        page.offsetMax = new Vector2(-28f, -126f);
        return page;
    }

    public Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Simple Hire UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = canvasObject.AddComponent<Image>();
        background.color = BackgroundColor;
        background.raycastTarget = false;
        return canvas;
    }

    public static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }
}
