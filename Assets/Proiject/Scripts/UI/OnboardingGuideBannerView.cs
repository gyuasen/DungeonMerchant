using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class OnboardingGuideBannerView
{
    private static readonly Color ParchmentTextColor = UITheme.ParchmentTextColor;

    private readonly SimpleMercenaryHireUIFactory factory;
    private readonly SimpleMercenaryHireUIView.ChromeReferences chrome;
    private readonly Transform guildPanel;
    private readonly Transform overlayRoot;
    private readonly Func<bool> isGuideVisible;
    private readonly Func<string> getObjectiveText;
    private readonly UnityAction onSkip;

    private RectTransform banner;
    private Text objectiveText;
    private Button skipButton;
    private RectTransform skipConfirmationOverlay;

    public OnboardingGuideBannerView(
        SimpleMercenaryHireUIFactory factory,
        SimpleMercenaryHireUIView.ChromeReferences chrome,
        Transform guildPanel,
        Transform overlayRoot,
        Func<bool> isGuideVisible,
        Func<string> getObjectiveText,
        UnityAction onSkip)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.chrome = chrome;
        this.guildPanel = guildPanel ?? throw new ArgumentNullException(nameof(guildPanel));
        this.overlayRoot = overlayRoot ?? throw new ArgumentNullException(nameof(overlayRoot));
        this.isGuideVisible = isGuideVisible;
        this.getObjectiveText = getObjectiveText;
        this.onSkip = onSkip;
    }

    public void Build()
    {
        if (banner != null)
        {
            return;
        }

        banner = chrome != null ? chrome.onboardingBanner : null;
        objectiveText = chrome != null ? chrome.onboardingObjectiveText : null;
        skipButton = chrome != null ? chrome.onboardingSkipButton : null;
        if (banner == null)
        {
            banner = SimpleMercenaryHireUIFactory.CreateUIObject(
                "Onboarding Guide Banner", guildPanel);
            banner.anchorMin = banner.anchorMax = new Vector2(1f, 0f);
            banner.pivot = new Vector2(1f, 0f);
            banner.sizeDelta = new Vector2(410f, 42f);
            banner.anchoredPosition = new Vector2(-20f, 58f);
            Image background = banner.gameObject.AddComponent<Image>();
            background.color = new Color(0.13f, 0.15f, 0.18f, 0.94f);
            background.raycastTarget = false;
            objectiveText = factory.CreateText(
                banner,
                string.Empty,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Vector2(12f, 0f),
                new Vector2(-126f, 0f),
                ParchmentTextColor);
            skipButton = factory.CreateActionButton(
                banner,
                "案内を終了",
                ShowSkipConfirmation);
            RectTransform skipRect = skipButton.GetComponent<RectTransform>();
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(1f, 0.5f);
            skipRect.pivot = new Vector2(1f, 0.5f);
            skipRect.sizeDelta = new Vector2(108f, 30f);
            skipRect.anchoredPosition = new Vector2(-8f, 0f);
        }
        else
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(ShowSkipConfirmation);
        }

        BuildSkipConfirmationOverlay();
        Refresh();
    }

    public void Refresh()
    {
        if (banner == null)
        {
            return;
        }

        bool visible = isGuideVisible != null && isGuideVisible();
        banner.gameObject.SetActive(visible);
        if (visible)
        {
            objectiveText.text = getObjectiveText != null
                ? getObjectiveText()
                : string.Empty;
        }
    }

    private void BuildSkipConfirmationOverlay()
    {
        skipConfirmationOverlay = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Onboarding Skip Confirmation Overlay",
            overlayRoot);
        skipConfirmationOverlay.anchorMin = Vector2.zero;
        skipConfirmationOverlay.anchorMax = Vector2.one;
        skipConfirmationOverlay.offsetMin = Vector2.zero;
        skipConfirmationOverlay.offsetMax = Vector2.zero;
        skipConfirmationOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.58f);
        RectTransform window = SimpleMercenaryHireUIFactory.CreateUIObject(
            "Onboarding Skip Confirmation Window",
            skipConfirmationOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(580f, 280f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());
        factory.CreateText(
            window,
            "最初の案内を終了しますか？\n詳しい遊び方はメニューから確認できます。",
            20,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(38f, -190f),
            new Vector2(-38f, -62f),
            ParchmentTextColor);
        Button endButton = factory.CreateActionButton(
            window, "案内を終了", ConfirmSkip);
        Button continueButton = factory.CreateActionButton(
            window, "続ける", HideSkipConfirmation);
        PositionConfirmationButton(endButton, new Vector2(-105f, -92f));
        PositionConfirmationButton(continueButton, new Vector2(105f, -92f));
        skipConfirmationOverlay.gameObject.SetActive(false);
    }

    private static void PositionConfirmationButton(Button button, Vector2 position)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180f, 48f);
        rect.anchoredPosition = position;
    }

    private void ShowSkipConfirmation()
    {
        skipConfirmationOverlay.SetAsLastSibling();
        skipConfirmationOverlay.gameObject.SetActive(true);
    }

    private void HideSkipConfirmation()
    {
        skipConfirmationOverlay.gameObject.SetActive(false);
    }

    private void ConfirmSkip()
    {
        onSkip?.Invoke();
        HideSkipConfirmation();
    }
}
