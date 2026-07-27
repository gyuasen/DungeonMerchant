using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private const string EndingSceneName = "Ending";

    private RectTransform storyOverlay;
    private Text storyTitleText;
    private Text storyBodyText;
    private Text storyCategoryText;
    private Button storyCloseButton;
    private Coroutine storyEntryCoroutine;
    private StoryPresentation activeStoryPresentation;

    private void OnEnable()
    {
        if (storyEntryCoroutine == null)
        {
            storyEntryCoroutine = StartCoroutine(ShowInitialStoryWhenReady());
        }
    }

    private IEnumerator ShowInitialStoryWhenReady()
    {
        yield return null;
        while (overlayRoot == null || uiFactory == null)
        {
            yield return null;
        }

        ShowNextPendingStory();
        storyEntryCoroutine = null;
    }

    private void BuildStoryOverlay()
    {
        if (storyOverlay != null)
        {
            return;
        }

        storyOverlay = CreateUIObject("Story Overlay", overlayRoot);
        storyOverlay.anchorMin = Vector2.zero;
        storyOverlay.anchorMax = Vector2.one;
        storyOverlay.offsetMin = Vector2.zero;
        storyOverlay.offsetMax = Vector2.zero;
        storyOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.58f);

        RectTransform window = CreateUIObject("Story Window", storyOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 430f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        storyTitleText = CreateText(
            window, string.Empty, 30, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(55f, -118f),
            new Vector2(-55f, -55f), ParchmentTextColor);
        storyCategoryText = CreateText(
            window, string.Empty, 17, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(55f, -48f),
            new Vector2(-55f, -18f), new Color(0.38f, 0.25f, 0.12f, 1f));
        storyBodyText = CreateText(
            window, string.Empty, 22, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(75f, -300f),
            new Vector2(-75f, -125f), ParchmentTextColor);

        storyCloseButton = CreateActionButton(window, "物語を閉じる", CloseStoryOverlay);
        RectTransform buttonRect = storyCloseButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot =
            new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(230f, 58f);
        buttonRect.anchoredPosition = new Vector2(0f, -155f);
        storyOverlay.gameObject.SetActive(false);
    }

    private void HandleStoryPresentationQueued()
    {
        if (storyOverlay == null || !storyOverlay.gameObject.activeSelf)
        {
            ShowNextPendingStory();
        }
    }

    private void ShowNextPendingStory()
    {
        BuildStoryOverlay();
        if (storyProgressManager == null ||
            !storyProgressManager.TryDequeuePresentation(
                out StoryPresentation presentation))
        {
            return;
        }

        if (presentation.IsEnding)
        {
            // DebtCleared is queued only by TryComplete, never during restore.
            // Save again here so return from the ending always restores this state.
            saveManager?.SaveGame();
            SceneManager.LoadScene(EndingSceneName);
            return;
        }

        activeStoryPresentation = presentation;
        storyTitleText.text = presentation.Title;
        storyBodyText.text = presentation.Body;
        storyCategoryText.text = presentation.IsOnboarding ? "操作案内" : string.Empty;
        SetStoryButtonLabel(
            storyCloseButton,
            presentation.IsOnboarding ? "閉じる" : "物語を閉じる");
        storyOverlay.SetAsLastSibling();
        storyOverlay.gameObject.SetActive(true);
    }

    private void CloseStoryOverlay()
    {
        storyOverlay.gameObject.SetActive(false);
        if (activeStoryPresentation.Milestone == StoryMilestone.OpeningDebtNotice)
        {
            onboardingGuideController?.TryComplete(OnboardingGuideStep.Opening);
        }
        activeStoryPresentation.OnClosed?.Invoke();
        activeStoryPresentation = default;
        ShowNextPendingStory();
    }

    private static void SetStoryButtonLabel(Button button, string label)
    {
        Text labelText = button != null ? button.GetComponentInChildren<Text>() : null;
        if (labelText != null) labelText.text = label;
    }
}
