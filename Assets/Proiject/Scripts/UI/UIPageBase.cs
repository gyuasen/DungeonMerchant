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
}

public sealed class SimpleUIPage : UIPageBase
{
}
