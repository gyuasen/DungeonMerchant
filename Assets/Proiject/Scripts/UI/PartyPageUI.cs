using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class PartyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;

    public void Initialize(Text title, RectTransform targetListRoot)
    {
        titleText = title;
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
        refreshAction = refresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}
