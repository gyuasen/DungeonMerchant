using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildTutorialOverlay()
    {
        tutorial.overlay = CreateUIObject("Tutorial Overlay", overlayRoot);
        tutorial.overlay.anchorMin = Vector2.zero;
        tutorial.overlay.anchorMax = Vector2.one;
        tutorial.overlay.offsetMin = Vector2.zero;
        tutorial.overlay.offsetMax = Vector2.zero;
        tutorial.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Tutorial Window", tutorial.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 560f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        tutorial.stepText = CreateText(
            window,
            string.Empty,
            15,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(34f, -58f),
            new Vector2(-34f, -24f),
            ParchmentMutedColor);

        tutorial.titleText = CreateText(
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

        tutorial.bodyText = CreateText(
            window,
            string.Empty,
            19,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(34f, -410f),
            new Vector2(-34f, -172f),
            ParchmentTextColor);
        tutorial.bodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        tutorial.bodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        tutorial.bodyText.rectTransform.offsetMin = new Vector2(34f, 118f);
        tutorial.bodyText.rectTransform.offsetMax = new Vector2(-34f, -172f);
        tutorial.bodyText.lineSpacing = 1.15f;

        tutorial.backButton =
            CreateActionButton(window, "戻る", tutorialController.ShowPreviousStep);
        RectTransform backRect =
            tutorial.backButton.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0f);
        backRect.pivot = new Vector2(0f, 0f);
        backRect.sizeDelta = new Vector2(130f, 46f);
        backRect.anchoredPosition = new Vector2(34f, 28f);

        tutorial.closeButton =
            CreateActionButton(window, "閉じる", HideTutorialOverlay);
        RectTransform closeRect =
            tutorial.closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(130f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 28f);

        tutorial.nextButton =
            CreateActionButton(window, "次へ", tutorialController.ShowNextStep);
        RectTransform nextRect =
            tutorial.nextButton.GetComponent<RectTransform>();
        nextRect.anchorMin = nextRect.anchorMax = new Vector2(1f, 0f);
        nextRect.pivot = new Vector2(1f, 0f);
        nextRect.sizeDelta = new Vector2(150f, 46f);
        nextRect.anchoredPosition = new Vector2(-34f, 28f);

        tutorial.overlay.gameObject.SetActive(false);
        tutorialController.Refresh();
    }

    private void ShowTutorialOverlay()
    {
        tutorialController.ShowTutorial();
    }

    private void HideTutorialOverlay()
    {
        tutorial.overlay?.gameObject.SetActive(false);
    }
}
