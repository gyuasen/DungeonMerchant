using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private static bool enterGameAfterSceneReload;
    private RectTransform titleOverlay;
    private RectTransform titleWindow;
    private RectTransform newGameConfirmationWindow;
    private RectTransform settingsWindow;
    private Button continueButton;
    private Text volumeValueText;
    private Coroutine titleBuildCoroutine;
    private bool titleEntryCompleted;

    private void OnEnable()
    {
        if (titleBuildCoroutine == null)
        {
            titleBuildCoroutine = StartCoroutine(BuildTitleOverlayWhenReady());
        }
    }

    private IEnumerator BuildTitleOverlayWhenReady()
    {
        // SimpleMercenaryHireUI.Start builds overlayRoot and uiFactory. Waiting
        // one frame also keeps this partial independent of Start's ordering.
        yield return null;
        while (overlayRoot == null || uiFactory == null)
        {
            yield return null;
        }

        BuildAndShowTitleOverlay();
        titleBuildCoroutine = null;
    }

    private void BuildAndShowTitleOverlay()
    {
        if (titleEntryCompleted)
        {
            return;
        }

        if (enterGameAfterSceneReload)
        {
            enterGameAfterSceneReload = false;
            titleEntryCompleted = true;
            HandleTitleEntryCompleted();
            return;
        }

        if (titleOverlay != null)
        {
            titleOverlay.SetAsLastSibling();
            titleOverlay.gameObject.SetActive(true);
            RefreshTitleButtons();
            return;
        }

        titleOverlay = CreateUIObject("Title Overlay", overlayRoot);
        titleOverlay.anchorMin = Vector2.zero;
        titleOverlay.anchorMax = Vector2.one;
        titleOverlay.offsetMin = Vector2.zero;
        titleOverlay.offsetMax = Vector2.zero;
        Image dimmer = titleOverlay.gameObject.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.84f);

        titleWindow = CreateTitleWindow("Title Window");
        CreateText(
            titleWindow,
            "傭兵商会",
            40,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(34f, -84f),
            new Vector2(-34f, -32f),
            ParchmentTextColor);
        CreateText(
            titleWindow,
            "― 借金と冒険の商会経営譚 ―\n傭兵を率い、一億Gの借金を返済せよ。",
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(34f, -136f),
            new Vector2(-34f, -102f),
            ParchmentMutedColor);

        continueButton = CreateTitleButton(
            titleWindow, "続きから", new Vector2(0f, 35f), ContinueGame);
        CreateTitleButton(
            titleWindow, "新しく始める", new Vector2(0f, -30f), ShowNewGameConfirmation);
        CreateTitleButton(
            titleWindow, "設定", new Vector2(0f, -95f), ShowSettings);
        CreateTitleButton(
            titleWindow, "終了", new Vector2(0f, -160f), QuitGame);

        newGameConfirmationWindow = CreateTitleWindow("New Game Confirmation");
        CreateText(
            newGameConfirmationWindow,
            "セーブデータを削除して、最初から始めますか？",
            20,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(34f, -110f),
            new Vector2(-34f, -52f),
            ParchmentTextColor);
        CreateText(
            newGameConfirmationWindow,
            "この操作は取り消せません。",
            15,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(34f, -158f),
            new Vector2(-34f, -190f),
            ParchmentMutedColor);
        CreateTitleButton(
            newGameConfirmationWindow,
            "削除して開始", new Vector2(-115f, -110f), StartNewGame);
        CreateTitleButton(
            newGameConfirmationWindow,
            "キャンセル", new Vector2(115f, -110f), HideNewGameConfirmation);

        settingsWindow = CreateTitleWindow("Settings Window");
        CreateText(
            settingsWindow, "設定", 28, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(34f, -92f),
            new Vector2(-34f, -38f), ParchmentTextColor);
        volumeValueText = CreateText(
            settingsWindow, string.Empty, 22, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Vector2(34f, -162f),
            new Vector2(-34f, -112f), ParchmentTextColor);
        CreateTitleButton(
            settingsWindow, "音量を下げる", new Vector2(-135f, -10f),
            () => ChangeVolume(-0.1f));
        CreateTitleButton(
            settingsWindow, "音量を上げる", new Vector2(135f, -10f),
            () => ChangeVolume(0.1f));
        CreateTitleButton(
            settingsWindow, "戻る", new Vector2(0f, -100f), HideSettings);

        RefreshTitleButtons();
        titleOverlay.SetAsLastSibling();
        titleOverlay.gameObject.SetActive(true);
    }

    private RectTransform CreateTitleWindow(string objectName)
    {
        RectTransform window = CreateUIObject(objectName, titleOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(620f, 500f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());
        return window;
    }

    private Button CreateTitleButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateActionButton(parent, label, action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(250f, 48f);
        rect.anchoredPosition = position;
        return button;
    }

    private void RefreshTitleButtons()
    {
        if (continueButton != null)
        {
            continueButton.interactable = saveManager != null && saveManager.HasSaveData;
        }

        if (titleWindow != null)
        {
            titleWindow.gameObject.SetActive(true);
        }
        if (newGameConfirmationWindow != null)
        {
            newGameConfirmationWindow.gameObject.SetActive(false);
        }
        if (settingsWindow != null)
        {
            settingsWindow.gameObject.SetActive(false);
        }
    }

    private void ContinueGame()
    {
        if (saveManager == null || !saveManager.HasSaveData)
        {
            RefreshTitleButtons();
            return;
        }

        titleOverlay.gameObject.SetActive(false);
        titleEntryCompleted = true;
        HandleTitleEntryCompleted();
    }

    private void ShowNewGameConfirmation()
    {
        titleWindow.gameObject.SetActive(false);
        newGameConfirmationWindow.SetAsLastSibling();
        newGameConfirmationWindow.gameObject.SetActive(true);
    }

    private void HideNewGameConfirmation()
    {
        RefreshTitleButtons();
    }

    private void ShowSettings()
    {
        titleWindow.gameObject.SetActive(false);
        settingsWindow.SetAsLastSibling();
        settingsWindow.gameObject.SetActive(true);
        RefreshVolumeLabel();
    }

    private void HideSettings()
    {
        RefreshTitleButtons();
    }

    private void ChangeVolume(float amount)
    {
        if (audioFeedbackService != null)
        {
            audioFeedbackService.Volume += amount;
            audioFeedbackService.Play(UISoundCue.Confirm);
        }
        RefreshVolumeLabel();
    }

    private void RefreshVolumeLabel()
    {
        if (volumeValueText != null)
        {
            float volume = audioFeedbackService != null
                ? audioFeedbackService.Volume
                : AudioFeedbackService.DefaultVolume;
            volumeValueText.text = $"効果音　{Mathf.RoundToInt(volume * 100f)}%";
        }
    }

    private void StartNewGame()
    {
        if (saveManager != null)
        {
            saveManager.DeleteSaveData();
        }
        TutorialController.ResetCompletion();

        enterGameAfterSceneReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested from title screen.");
#else
        Application.Quit();
#endif
    }
}
