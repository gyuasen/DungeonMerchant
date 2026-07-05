using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class EconomyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;

    public void Initialize(
        Text title,
        Text description,
        RectTransform targetListRoot)
    {
        titleText = title;
        descriptionText = description;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font font,
        Color color,
        UnityAction refresh)
    {
        ConfigureText(
            titleText, font, 15,
            TextAnchor.MiddleLeft, color);
        if (descriptionText != null)
        {
            ConfigureText(
                descriptionText, font, 15,
                TextAnchor.MiddleLeft, color);
        }
        refreshAction = refresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}

public sealed class MarketPageUI : EconomyPageUI
{
}

public sealed class BlacksmithPageUI : EconomyPageUI
{
}

public sealed class InventoryPageUI : EconomyPageUI
{
}
