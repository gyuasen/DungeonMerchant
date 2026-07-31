using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single source of truth for the fantasy UI color palette and shared
/// button transition colors. Values were previously duplicated across
/// SimpleMercenaryHireUI, SimpleMercenaryHireUIFactory, UIPageBase and
/// several page UI classes; they are now defined only here.
/// </summary>
public static class UITheme
{
    public const string PositiveTextHex = "#65D88A";
    public const string NegativeTextHex = "#FF7474";
    public const string WarningTextHex = "#F2B84B";
    public const string NeutralTextHex = "#AEB6BE";
    public const string ResultHeadingTextHex = "#5A3B24";
    public const string ResultAccentTextHex = "#B86B2B";

    public static readonly Color BackgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
    public static readonly Color PanelColor = new Color(0.13f, 0.15f, 0.18f, 1f);
    public static readonly Color RowColor =
        new Color(0.27f, 0.16f, 0.09f, 0.94f);
    public static readonly Color AccentColor =
        new Color(0.18f, 0.36f, 0.24f, 1f);
    public static readonly Color InactiveColor =
        new Color(0.24f, 0.14f, 0.08f, 0.96f);
    public static readonly Color WoodButtonColor =
        new Color(0.35f, 0.22f, 0.13f, 1f);
    public static readonly Color ImportantButtonColor =
        new Color(0.43f, 0.15f, 0.12f, 1f);
    public static readonly Color FrameColor =
        new Color(0.72f, 0.52f, 0.27f, 0.9f);
    public static readonly Color ButtonTextColor =
        new Color(1f, 0.94f, 0.79f, 1f);
    public static readonly Color MutedTextColor =
        new Color(0.82f, 0.73f, 0.59f, 1f);
    public static readonly Color ParchmentTextColor =
        Color.black;
    public static readonly Color ParchmentMutedColor =
        Color.black;
    public static readonly Color TooltipBackgroundColor =
        new Color(0.075f, 0.045f, 0.025f, 0.96f);
    public static readonly Color TooltipTextColor =
        new Color(0.97f, 0.9f, 0.74f, 1f);
    public static readonly Color ModalOverlayColor =
        new Color(0f, 0f, 0f, 0.82f);

    /// <summary>
    /// Applies the shared button transition ColorBlock. Identical values
    /// were previously duplicated in SimpleMercenaryHireUIFactory
    /// .ApplyButtonTransitions and UIPageBase.ConfigureButton.
    /// </summary>
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
}
