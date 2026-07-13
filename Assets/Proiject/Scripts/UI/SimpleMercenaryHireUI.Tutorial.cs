using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildTutorialOverlay()
    {
        tutorialOverlay = CreateUIObject("Tutorial Overlay", overlayRoot);
        tutorialOverlay.anchorMin = Vector2.zero;
        tutorialOverlay.anchorMax = Vector2.one;
        tutorialOverlay.offsetMin = Vector2.zero;
        tutorialOverlay.offsetMax = Vector2.zero;
        tutorialOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Tutorial Window", tutorialOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 560f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        tutorialStepText = CreateText(
            window,
            string.Empty,
            15,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(34f, -58f),
            new Vector2(-34f, -24f),
            ParchmentMutedColor);

        tutorialTitleText = CreateText(
            window,
            string.Empty,
            28,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(34f, -108f),
            new Vector2(-34f, -62f),
            ParchmentTextColor);

        Text firstJourneyRouteText = CreateText(
            window,
            TutorialController.FirstJourneyRoute,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(34f, -158f),
            new Vector2(-34f, -116f),
            ParchmentMutedColor);
        firstJourneyRouteText.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        tutorialBodyText = CreateText(
            window,
            string.Empty,
            19,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(34f, -410f),
            new Vector2(-34f, -172f),
            ParchmentTextColor);
        tutorialBodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        tutorialBodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        tutorialBodyText.rectTransform.offsetMin = new Vector2(34f, 118f);
        tutorialBodyText.rectTransform.offsetMax = new Vector2(-34f, -172f);
        tutorialBodyText.lineSpacing = 1.15f;

        tutorialBackButton =
            CreateActionButton(window, "戻る", tutorialController.ShowPreviousStep);
        RectTransform backRect =
            tutorialBackButton.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0f);
        backRect.pivot = new Vector2(0f, 0f);
        backRect.sizeDelta = new Vector2(130f, 46f);
        backRect.anchoredPosition = new Vector2(34f, 28f);

        tutorialCloseButton =
            CreateActionButton(window, "閉じる", HideTutorialOverlay);
        RectTransform closeRect =
            tutorialCloseButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(130f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 28f);

        tutorialNextButton =
            CreateActionButton(window, "次へ", tutorialController.ShowNextStep);
        RectTransform nextRect =
            tutorialNextButton.GetComponent<RectTransform>();
        nextRect.anchorMin = nextRect.anchorMax = new Vector2(1f, 0f);
        nextRect.pivot = new Vector2(1f, 0f);
        nextRect.sizeDelta = new Vector2(150f, 46f);
        nextRect.anchoredPosition = new Vector2(-34f, 28f);

        tutorialOverlay.gameObject.SetActive(false);
        tutorialController.Refresh();
    }

    private void ShowTutorialOverlay()
    {
        tutorialController.ShowTutorial();
    }

    private void HideTutorialOverlay()
    {
        tutorialOverlay?.gameObject.SetActive(false);
    }
}
