using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class JobChangePageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;

    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text title,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = title;
        scrollRect = targetScrollRect;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font font,
        Color titleColor,
        UnityAction refresh)
    {
        ConfigureText(
            titleText,
            font,
            17,
            TextAnchor.MiddleLeft,
            titleColor);
        scrollRect.content = listRoot;
        refreshAction = refresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}
