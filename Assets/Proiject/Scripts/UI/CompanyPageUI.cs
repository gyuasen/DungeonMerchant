using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class CompanyPageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button questButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;

    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text title,
        Button quest,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = title;
        questButton = quest;
        scrollRect = targetScrollRect;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font titleFont,
        Font buttonFont,
        Color titleColor,
        Color buttonTextColor,
        UnityAction showQuests,
        UnityAction refresh)
    {
        ConfigureText(
            titleText, titleFont, 15,
            TextAnchor.MiddleLeft, titleColor);
        ConfigureButton(
            questButton, buttonFont, buttonTextColor,
            "依頼", showQuests);
        scrollRect.content = listRoot;
        refreshAction = refresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}
