public partial class SimpleMercenaryHireUI
{
    private OnboardingGuideBannerView onboardingGuideBannerView;

    private void BuildOnboardingGuideBanner()
    {
        if (onboardingGuideBannerView == null)
        {
            onboardingGuideBannerView = new OnboardingGuideBannerView(
                uiFactory,
                activeView != null ? activeView.Chrome : null,
                guildPanel,
                overlayRoot,
                () => onboardingGuideController != null &&
                    onboardingGuideController.IsEnabled &&
                    !onboardingGuideController.IsComplete,
                () => onboardingGuideController != null
                    ? onboardingGuideController.CurrentObjectiveText
                    : string.Empty,
                () => onboardingGuideController?.Skip());
        }

        onboardingGuideBannerView.Build();
    }

    private void HandleOnboardingGuideStateChanged(OnboardingGuideStep step)
    {
        onboardingGuideBannerView?.Refresh();
    }
}
