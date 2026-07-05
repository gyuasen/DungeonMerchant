using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HirePageUI : UIPageBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button contractButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    private UnityAction refreshAction;

    public Button ContractButton => contractButton;
    public RectTransform ListRoot => listRoot;

    public void Initialize(
        Text targetTitle,
        Button targetContractButton,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        titleText = targetTitle;
        contractButton = targetContractButton;
        scrollRect = targetScrollRect;
        listRoot = targetListRoot;
    }

    public void Configure(
        Font titleFont,
        Font buttonFont,
        Color titleColor,
        Color buttonTextColor,
        UnityAction onCycleContract,
        UnityAction onRefresh)
    {
        ConfigureText(
            titleText, titleFont, 15,
            TextAnchor.MiddleLeft, titleColor);
        ConfigureButton(
            contractButton,
            buttonFont,
            buttonTextColor,
            "契約: 日雇い",
            onCycleContract);

        scrollRect.content = listRoot;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        refreshAction = onRefresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}
