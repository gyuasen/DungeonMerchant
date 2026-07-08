using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UIPageBase : MonoBehaviour
{
    public bool IsVisible => gameObject.activeSelf;

    public virtual void Show()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void Refresh()
    {
    }

    protected static void ConfigureText(
        Text text,
        Font font,
        int fontSize,
        TextAnchor alignment,
        Color color)
    {
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
    }

    protected static void ConfigureButton(
        Button button,
        Font font,
        Color textColor,
        string label,
        UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        Text text = button.GetComponentInChildren<Text>();
        if (text == null)
        {
            RectTransform labelRect = CreateUIObject(
                "Label",
                button.GetComponent<RectTransform>());
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            text = labelRect.gameObject.AddComponent<Text>();
        }

        ConfigureText(
            text, font, 17, TextAnchor.MiddleCenter, textColor);

        text.text = label;
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

    protected static void ClearChildren(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    protected static RectTransform CreateUIObject(
        string objectName,
        RectTransform parent)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    protected static Text CreateText(
        RectTransform parent,
        string content,
        Font font,
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
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Normal;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    protected static RectTransform CreateRow(
        string rowName,
        RectTransform parent,
        float top,
        Color rowColor,
        Color frameColor)
    {
        GameObject rowPrefab =
            Resources.Load<GameObject>("UI/Templates/ListRow");
        RectTransform row = rowPrefab != null
            ? Instantiate(rowPrefab, parent, false)
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
        rowImage.color = rowColor;
        if (row.GetComponent<Outline>() == null)
        {
            AddFrame(rowImage, frameColor, 1f);
        }
        return row;
    }

    protected static Button CreateActionButton(
        RectTransform parent,
        string label,
        Font font,
        Color buttonColor,
        Color frameColor,
        Color textColor,
        UnityAction action)
    {
        GameObject prefab = Resources.Load<GameObject>(
            "UI/Templates/ActionButton");
        RectTransform buttonRect = prefab != null
            ? Instantiate(prefab, parent, false)
                .GetComponent<RectTransform>()
            : CreateUIObject("Action Button", parent);
        buttonRect.name = "Action Button";
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.sizeDelta = new Vector2(130f, 52f);
        buttonRect.anchoredPosition = new Vector2(-18f, 0f);

        Image image = buttonRect.GetComponent<Image>() ??
            buttonRect.gameObject.AddComponent<Image>();
        image.color = buttonColor;
        if (buttonRect.GetComponent<Outline>() == null)
        {
            AddFrame(image, frameColor, 1.5f);
        }

        Button button = buttonRect.GetComponent<Button>() ??
            buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ConfigureButton(button, font, textColor, label, action);
        return button;
    }

    private static void AddFrame(
        Image image,
        Color frameColor,
        float thickness)
    {
        Outline outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = frameColor;
        outline.effectDistance = new Vector2(thickness, -thickness);
        outline.useGraphicAlpha = true;
    }
}
