using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private RectTransform onboardingGuideBanner;
    private Text onboardingGuideObjectiveText;
    private Button onboardingGuideSkipButton;
    private RectTransform onboardingSkipConfirmationOverlay;

    private void BuildOnboardingGuideBanner()
    {
        if (onboardingGuideBanner != null)
        {
            return;
        }

        SimpleMercenaryHireUIView.ChromeReferences chrome =
            activeView != null ? activeView.Chrome : null;
        onboardingGuideBanner = chrome != null
            ? chrome.onboardingBanner
            : null;
        onboardingGuideObjectiveText = chrome != null
            ? chrome.onboardingObjectiveText
            : null;
        onboardingGuideSkipButton = chrome != null
            ? chrome.onboardingSkipButton
            : null;
        if (onboardingGuideBanner == null)
        {
            onboardingGuideBanner = CreateUIObject("Onboarding Guide Banner", guildPanel);
            onboardingGuideBanner.anchorMin = onboardingGuideBanner.anchorMax =
                new Vector2(1f, 0f);
            onboardingGuideBanner.pivot = new Vector2(1f, 0f);
            onboardingGuideBanner.sizeDelta = new Vector2(410f, 42f);
            onboardingGuideBanner.anchoredPosition = new Vector2(-20f, 58f);
            Image background = onboardingGuideBanner.gameObject.AddComponent<Image>();
            background.color = new Color(0.13f, 0.15f, 0.18f, 0.94f);
            background.raycastTarget = false;
            onboardingGuideObjectiveText = CreateText(
                onboardingGuideBanner,
                string.Empty,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(12f, 0f),
                new Vector2(-126f, 0f),
                ParchmentTextColor);
            onboardingGuideSkipButton = CreateActionButton(
                onboardingGuideBanner,
                "案内を終了",
                ShowOnboardingSkipConfirmation);
            RectTransform skipRect =
                onboardingGuideSkipButton.GetComponent<RectTransform>();
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(1f, 0.5f);
            skipRect.pivot = new Vector2(1f, 0.5f);
            skipRect.sizeDelta = new Vector2(108f, 30f);
            skipRect.anchoredPosition = new Vector2(-8f, 0f);
        }
        else
        {
            onboardingGuideSkipButton.onClick.RemoveAllListeners();
            onboardingGuideSkipButton.onClick.AddListener(
                ShowOnboardingSkipConfirmation);
        }

        BuildOnboardingSkipConfirmationOverlay();
        RefreshOnboardingGuideBanner();
    }

    private void BuildOnboardingSkipConfirmationOverlay()
    {
        onboardingSkipConfirmationOverlay = CreateUIObject(
            "Onboarding Skip Confirmation Overlay",
            overlayRoot);
        onboardingSkipConfirmationOverlay.anchorMin = Vector2.zero;
        onboardingSkipConfirmationOverlay.anchorMax = Vector2.one;
        onboardingSkipConfirmationOverlay.offsetMin = Vector2.zero;
        onboardingSkipConfirmationOverlay.offsetMax = Vector2.zero;
        onboardingSkipConfirmationOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.58f);
        RectTransform window = CreateUIObject(
            "Onboarding Skip Confirmation Window",
            onboardingSkipConfirmationOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(580f, 280f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        CreateText(
            window,
            "最初の案内を終了しますか？\n詳しい遊び方はメニューから確認できます。",
            20,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(38f, -190f),
            new Vector2(-38f, -62f),
            ParchmentTextColor);
        Button endButton = CreateActionButton(window, "案内を終了", ConfirmOnboardingSkip);
        Button continueButton = CreateActionButton(window, "続ける", HideOnboardingSkipConfirmation);
        PositionConfirmationButton(endButton, new Vector2(-105f, -92f));
        PositionConfirmationButton(continueButton, new Vector2(105f, -92f));
        onboardingSkipConfirmationOverlay.gameObject.SetActive(false);
    }

    private static void PositionConfirmationButton(Button button, Vector2 position)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180f, 48f);
        rect.anchoredPosition = position;
    }

    private void HandleOnboardingGuideStateChanged(OnboardingGuideStep step)
    {
        RefreshOnboardingGuideBanner();
    }

    private void RefreshOnboardingGuideBanner()
    {
        if (onboardingGuideBanner == null)
        {
            return;
        }

        bool visible = onboardingGuideController != null &&
            onboardingGuideController.IsEnabled &&
            !onboardingGuideController.IsComplete;
        onboardingGuideBanner.gameObject.SetActive(visible);
        if (visible)
        {
            onboardingGuideObjectiveText.text =
                onboardingGuideController.CurrentObjectiveText;
        }
    }

    private void ShowOnboardingSkipConfirmation()
    {
        onboardingSkipConfirmationOverlay.SetAsLastSibling();
        onboardingSkipConfirmationOverlay.gameObject.SetActive(true);
    }

    private void HideOnboardingSkipConfirmation()
    {
        onboardingSkipConfirmationOverlay.gameObject.SetActive(false);
    }

    private void ConfirmOnboardingSkip()
    {
        onboardingGuideController?.Skip();
        HideOnboardingSkipConfirmation();
    }
}
