using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the standalone title screen without depending on the game-scene UI.
/// </summary>
public sealed class TitleSceneController : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";

    private Font titleFont;
    private Font bodyFont;
    private AudioFeedbackService audioFeedbackService;
    private RectTransform canvasRoot;
    private RectTransform menuRoot;
    private RectTransform newGameWindow;
    private RectTransform settingsWindow;
    private Button continueButton;
    private Text volumeValueText;

    private void Awake()
    {
        titleFont = LoadTitleFont();
        bodyFont = Resources.Load<Font>("Fonts/ZenKurenaido-Regular") ??
            titleFont;
        audioFeedbackService = GetComponent<AudioFeedbackService>() ??
            gameObject.AddComponent<AudioFeedbackService>();

        BuildScreen();
    }

    private void Start()
    {
        // The canvas is a scene-root object, not a child of this controller,
        // so the buttons must be registered from the canvas root.
        audioFeedbackService.RegisterButtonsUnder(canvasRoot);
    }

    private void BuildScreen()
    {
        SimpleMercenaryHireUIFactory.EnsureEventSystem();

        GameObject canvasObject = new GameObject(
            "Title Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = canvasObject.GetComponent<RectTransform>();
        canvasRoot = root;
        CreateBackground(root);
        CreateTitle(root);
        CreateMenu(root);
        CreateCopyright(root);
        CreateNewGameWindow(root);
        CreateSettingsWindow(root);
        RefreshMenu();
    }

    private void CreateBackground(RectTransform root)
    {
        Sprite backgroundSprite = Resources.Load<Sprite>("UI/TitleBackground");
        if (backgroundSprite != null)
        {
            RectTransform imageRect = CreateRect("Title Background", root);
            Stretch(imageRect);
            Image image = imageRect.gameObject.AddComponent<Image>();
            image.sprite = backgroundSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            AspectRatioFitter fitter =
                imageRect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = backgroundSprite.rect.width /
                Mathf.Max(1f, backgroundSprite.rect.height);
        }
        else
        {
            RectTransform gradientRect = CreateRect("Title Gradient", root);
            Stretch(gradientRect);
            Image gradient = gradientRect.gameObject.AddComponent<Image>();
            gradient.sprite = CreateVerticalGradient(
                new Color(0.055f, 0.085f, 0.16f),
                new Color(0.005f, 0.008f, 0.018f));
            gradient.type = Image.Type.Simple;
            gradient.raycastTarget = false;
        }

        RectTransform dimmerRect = CreateRect("Title Dimmer", root);
        Stretch(dimmerRect);
        Image dimmer = dimmerRect.gameObject.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.35f);
        dimmer.raycastTarget = false;
    }

    private void CreateTitle(RectTransform root)
    {
        Text title = CreateText(
            "傭兵商会", root, titleFont, 72, FontStyle.Bold,
            TextAnchor.MiddleCenter, new Color(0.91f, 0.71f, 0.37f));
        title.rectTransform.anchorMin = new Vector2(0.1f, 0.68f);
        title.rectTransform.anchorMax = new Vector2(0.9f, 0.86f);
        title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
        Shadow titleShadow = title.gameObject.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0.08f, 0.035f, 0f, 0.9f);
        titleShadow.effectDistance = new Vector2(3f, -3f);

        Text subtitle = CreateText(
            "― 借金と冒険の商会経営譚 ―", root, bodyFont, 22,
            FontStyle.Normal, TextAnchor.MiddleCenter, UITheme.MutedTextColor);
        subtitle.rectTransform.anchorMin = new Vector2(0.1f, 0.63f);
        subtitle.rectTransform.anchorMax = new Vector2(0.9f, 0.69f);
        subtitle.rectTransform.offsetMin = subtitle.rectTransform.offsetMax = Vector2.zero;
    }

    private void CreateMenu(RectTransform root)
    {
        menuRoot = CreateRect("Title Menu", root);
        menuRoot.anchorMin = new Vector2(0.5f, 0f);
        menuRoot.anchorMax = new Vector2(0.5f, 0f);
        menuRoot.pivot = new Vector2(0.5f, 0f);
        menuRoot.sizeDelta = new Vector2(350f, 295f);
        menuRoot.anchoredPosition = new Vector2(0f, 75f);

        continueButton = CreateMenuButton(menuRoot, "続きから", 105f, ContinueGame);
        CreateMenuButton(menuRoot, "新しく始める", 35f, ShowNewGameConfirmation);
        CreateMenuButton(menuRoot, "設定", -35f, ShowSettings);
        CreateMenuButton(menuRoot, "終了", -105f, QuitGame);
    }

    private void CreateNewGameWindow(RectTransform root)
    {
        newGameWindow = CreateParchmentWindow("New Game Confirmation", root);
        CreateWindowText(
            newGameWindow, "セーブデータを削除して、\n最初から始めますか？", 24,
            new Vector2(45f, -165f), new Vector2(-45f, -62f));
        CreateWindowText(
            newGameWindow, "この操作は取り消せません。", 16,
            new Vector2(45f, -216f), new Vector2(-45f, -180f));
        CreateDialogButton(newGameWindow, "削除して始める", -125f, StartNewGame);
        CreateDialogButton(newGameWindow, "キャンセル", 125f, RefreshMenu);
        newGameWindow.gameObject.SetActive(false);
    }

    private void CreateSettingsWindow(RectTransform root)
    {
        settingsWindow = CreateParchmentWindow("Settings Window", root);
        CreateWindowText(settingsWindow, "設定", 30,
            new Vector2(45f, -105f), new Vector2(-45f, -50f));
        volumeValueText = CreateWindowText(settingsWindow, string.Empty, 22,
            new Vector2(45f, -180f), new Vector2(-45f, -125f));
        CreateDialogButton(settingsWindow, "音量を下げる", -125f,
            () => ChangeVolume(-0.1f));
        CreateDialogButton(settingsWindow, "音量を上げる", 125f,
            () => ChangeVolume(0.1f));
        Button back = CreateDialogButton(settingsWindow, "戻る", 0f, RefreshMenu);
        back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -155f);
        settingsWindow.gameObject.SetActive(false);
    }

    private void CreateCopyright(RectTransform root)
    {
        Text copyright = CreateText(
            "© 2026 YugaSen", root, bodyFont, 15, FontStyle.Normal,
            TextAnchor.MiddleCenter, UITheme.MutedTextColor);
        copyright.rectTransform.anchorMin = new Vector2(0f, 0f);
        copyright.rectTransform.anchorMax = new Vector2(1f, 0f);
        copyright.rectTransform.offsetMin = new Vector2(0f, 16f);
        copyright.rectTransform.offsetMax = new Vector2(0f, 42f);
    }

    private Button CreateMenuButton(
        RectTransform parent,
        string label,
        float y,
        UnityEngine.Events.UnityAction action)
    {
        return CreateButton(parent, label, new Vector2(0f, y), action);
    }

    private Button CreateDialogButton(
        RectTransform parent,
        string label,
        float x,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateButton(parent, label, new Vector2(x, -95f), action);
        button.GetComponent<RectTransform>().sizeDelta = new Vector2(235f, 52f);
        return button;
    }

    private Button CreateButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateRect(label + " Button", parent);
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot =
            new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(300f, 52f);
        buttonRect.anchoredPosition = position;
        Image image = buttonRect.gameObject.AddComponent<Image>();
        image.color = UITheme.WoodButtonColor;
        SimpleMercenaryHireUIFactory.AddFantasyFrame(image, 2f);
        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        UITheme.ApplyButtonTransitions(button);
        Text text = CreateText(label, buttonRect, titleFont, 20, FontStyle.Bold,
            TextAnchor.MiddleCenter, UITheme.ButtonTextColor);
        Stretch(text.rectTransform);
        return button;
    }

    private RectTransform CreateParchmentWindow(string name, RectTransform root)
    {
        RectTransform window = CreateRect(name, root);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(620f, 370f);
        Image image = window.gameObject.AddComponent<Image>();
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(image);
        return window;
    }

    private Text CreateWindowText(
        RectTransform parent,
        string content,
        int size,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        Text text = CreateText(content, parent, bodyFont, size, FontStyle.Normal,
            TextAnchor.MiddleCenter, UITheme.ParchmentTextColor);
        text.rectTransform.anchorMin = new Vector2(0f, 1f);
        text.rectTransform.anchorMax = new Vector2(1f, 1f);
        text.rectTransform.pivot = new Vector2(0.5f, 1f);
        text.rectTransform.offsetMin = offsetMin;
        text.rectTransform.offsetMax = offsetMax;
        return text;
    }

    private static Text CreateText(
        string content,
        RectTransform parent,
        Font font,
        int size,
        FontStyle style,
        TextAnchor alignment,
        Color color)
    {
        RectTransform rect = CreateRect("Text", parent);
        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private void RefreshMenu()
    {
        continueButton.interactable = SaveManager.SaveFileExists();
        menuRoot.gameObject.SetActive(true);
        newGameWindow.gameObject.SetActive(false);
        settingsWindow.gameObject.SetActive(false);
    }

    private void ContinueGame()
    {
        if (SaveManager.SaveFileExists())
        {
            SceneManager.LoadScene(GameSceneName);
        }
        else
        {
            RefreshMenu();
        }
    }

    private void ShowNewGameConfirmation()
    {
        menuRoot.gameObject.SetActive(false);
        newGameWindow.gameObject.SetActive(true);
    }

    private void StartNewGame()
    {
        SaveManager.DeleteSaveFileAndProgress();
        TutorialController.ResetCompletion();
        SceneManager.LoadScene(GameSceneName);
    }

    private void ShowSettings()
    {
        menuRoot.gameObject.SetActive(false);
        settingsWindow.gameObject.SetActive(true);
        RefreshVolumeLabel();
    }

    private void ChangeVolume(float amount)
    {
        audioFeedbackService.Volume += amount;
        audioFeedbackService.Play(UISoundCue.Confirm);
        RefreshVolumeLabel();
    }

    private void RefreshVolumeLabel()
    {
        volumeValueText.text =
            $"音量: {Mathf.RoundToInt(audioFeedbackService.Volume * 100f)}%";
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested from title screen.");
#else
        Application.Quit();
#endif
    }

    private static Font LoadTitleFont()
    {
        string[] fontNames =
        {
            "Yu Mincho Demibold",
            "Yu Mincho",
            "Yu Gothic UI"
        };
        Font font = Font.CreateDynamicFontFromOSFont(fontNames, 28);
        return font ?? Resources.Load<Font>("Fonts/ZenKurenaido-Regular") ??
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static Sprite CreateVerticalGradient(Color top, Color bottom)
    {
        Texture2D texture = new Texture2D(1, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { bottom, top });
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 2f),
            new Vector2(0.5f, 0.5f));
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
