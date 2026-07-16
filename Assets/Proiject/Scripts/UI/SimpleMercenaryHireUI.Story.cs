using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private RectTransform storyOverlay;
    private Text storyTitleText;
    private Text storyBodyText;
    private Coroutine storyEntryCoroutine;

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
            TextAnchor.MiddleCenter, new Vector2(55f, -105f),
            new Vector2(-55f, -42f), ParchmentTextColor);
        storyBodyText = CreateText(
            window, string.Empty, 22, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(75f, -300f),
            new Vector2(-75f, -125f), ParchmentTextColor);

        Button closeButton = CreateActionButton(
            window, "物語を進める", CloseStoryOverlay);
        RectTransform buttonRect = closeButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot =
            new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(230f, 58f);
        buttonRect.anchoredPosition = new Vector2(0f, -155f);
        storyOverlay.gameObject.SetActive(false);
    }

    private void HandleStoryMilestoneCompleted(StoryMilestone milestone)
    {
        if (storyOverlay != null && !storyOverlay.gameObject.activeSelf)
        {
            ShowNextPendingStory();
        }
    }

    private void ShowNextPendingStory()
    {
        BuildStoryOverlay();
        if (storyProgressManager == null ||
            !storyProgressManager.TryDequeuePendingPresentation(
                out StoryMilestone milestone))
        {
            tutorialController?.ShowTutorialIfNeeded();
            return;
        }

        StoryMilestoneInfo info =
            storyProgressManager.GetMilestoneInfo(milestone);
        storyTitleText.text = info.Title;
        storyBodyText.text = info.Body;
        storyOverlay.SetAsLastSibling();
        storyOverlay.gameObject.SetActive(true);
    }

    private void CloseStoryOverlay()
    {
        storyOverlay.gameObject.SetActive(false);
        ShowNextPendingStory();
    }

}
