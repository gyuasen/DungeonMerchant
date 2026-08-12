using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the standalone debt-cleared ending without the game-scene UI.
/// </summary>
public sealed class EndingSceneController : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";
    private const string LetterBody =
        "親愛なる我が子へ\n\n" +
        "この手紙が読まれているということは、お前があの莫大な借金を返し終えたということだね。\n\n" +
        "目を覚ました時、医師からすべてを聞いた。商会は潰れ、お前に五千万もの負債が残されたと。剣も握れぬお前が、たった一人で魔大陸へ渡ったと聞いて、私たちがどれほど胸を痛めたか——けれど、それ以上に誇らしかった。\n\n" +
        "お前は、私たちが最も大切にしてきたものを受け継いでいた。人、物、金、時間、そして信用。そのすべてに責任を持って向き合う、商人の心だ。\n\n" +
        "商会はもう、私たちのものではない。お前が自らの手で築き直した、お前の商会だ。\n\n" +
        "よく帰ってきてくれた。おかえり。\n\n" +
        "——父と母より";
    private const string CreditsBody =
        "五千万Gの負債は返済された。\n\n" +
        "物流網を結び直した商会は、\n" +
        "これからも魔大陸で人と品を繋いでいく。\n\n\n" +
        "― 完 ―";

    private Font titleFont;
    private Font bodyFont;
    private AudioFeedbackService audioFeedbackService;
    private RectTransform canvasRoot;
    private RectTransform letterWindow;
    private RectTransform creditsWindow;

    private void Awake()
    {
        titleFont = LoadTitleFont();
        bodyFont = Resources.Load<Font>("Fonts/ZenKurenaido-Regular") ?? titleFont;
        audioFeedbackService = GetComponent<AudioFeedbackService>() ??
            gameObject.AddComponent<AudioFeedbackService>();
        BuildScreen();
    }

    private void Start() => audioFeedbackService.RegisterButtonsUnder(canvasRoot);

    private void BuildScreen()
    {
        SimpleMercenaryHireUIFactory.EnsureEventSystem();

        GameObject canvasObject = new GameObject(
            "Ending Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRoot = canvasObject.GetComponent<RectTransform>();

        CreateBackground(canvasRoot);
        letterWindow = CreateLetterWindow(canvasRoot);
        creditsWindow = CreateCreditsWindow(canvasRoot);
        ShowLetter();
    }

    private static void CreateBackground(RectTransform root)
    {
        RectTransform backgroundRect = CreateRect("Ending Background", root);
        Stretch(backgroundRect);
        Image background = backgroundRect.gameObject.AddComponent<Image>();
        background.color = new Color(0.025f, 0.035f, 0.065f, 1f);
        background.raycastTarget = false;

        RectTransform vignetteRect = CreateRect("Ending Dimmer", root);
        Stretch(vignetteRect);
        Image vignette = vignetteRect.gameObject.AddComponent<Image>();
        vignette.color = new Color(0f, 0f, 0f, 0.32f);
        vignette.raycastTarget = false;
    }

    private RectTransform CreateLetterWindow(RectTransform root)
    {
        RectTransform window = CreateParchmentWindow("Parents' Letter", root);
        Text heading = CreateText("", window, titleFont, 20, FontStyle.Bold,
            TextAnchor.MiddleCenter, UITheme.ParchmentTextColor);
        heading.rectTransform.anchorMin = new Vector2(0f, 1f);
        heading.rectTransform.anchorMax = new Vector2(1f, 1f);
        heading.rectTransform.offsetMin = new Vector2(65f, -62f);
        heading.rectTransform.offsetMax = new Vector2(-65f, -25f);

        CreateScrollText(window, LetterBody, TextAnchor.UpperLeft);
        CreateButton(window, "手紙を閉じる", new Vector2(0f, -293f), ShowCredits);
        return window;
    }

    private RectTransform CreateCreditsWindow(RectTransform root)
    {
        RectTransform window = CreateParchmentWindow("Epilogue", root);
        Text heading = CreateText("エピローグ　商会、再興", window, titleFont, 30,
            FontStyle.Bold, TextAnchor.MiddleCenter, UITheme.ParchmentTextColor);
        heading.rectTransform.anchorMin = new Vector2(0f, 1f);
        heading.rectTransform.anchorMax = new Vector2(1f, 1f);
        heading.rectTransform.offsetMin = new Vector2(65f, -105f);
        heading.rectTransform.offsetMax = new Vector2(-65f, -45f);

        Text body = CreateText(CreditsBody, window, bodyFont, 24, FontStyle.Normal,
            TextAnchor.MiddleCenter, UITheme.ParchmentTextColor);
        body.rectTransform.anchorMin = new Vector2(0f, 0f);
        body.rectTransform.anchorMax = new Vector2(1f, 1f);
        body.rectTransform.offsetMin = new Vector2(85f, 88f);
        body.rectTransform.offsetMax = new Vector2(-85f, -125f);
        CreateButton(window, "商売を続ける", new Vector2(0f, -293f), ContinueGame);
        return window;
    }

    private void CreateScrollText(
        RectTransform parent, string content, TextAnchor alignment)
    {
        RectTransform viewport = CreateRect("Letter Viewport", parent);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = new Vector2(70f, 88f);
        viewport.offsetMax = new Vector2(-54f, -75f);
        // Mask はマスク画像のアルファがしきい値未満のピクセルを切り抜くため、
        // 透明にしすぎると内側のテキストごと消える。他画面のビューポートと
        // 同じ 0.01f に揃える。
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        RectTransform contentRoot = CreateRect("Letter Content", viewport);
        contentRoot.anchorMin = new Vector2(0f, 1f);
        contentRoot.anchorMax = new Vector2(1f, 1f);
        contentRoot.pivot = new Vector2(0.5f, 1f);
        contentRoot.sizeDelta = new Vector2(0f, 1120f);
        Text letter = CreateText(content, contentRoot, bodyFont, 20, FontStyle.Normal,
            alignment, UITheme.ParchmentTextColor);
        Stretch(letter.rectTransform);
        letter.horizontalOverflow = HorizontalWrapMode.Wrap;
        letter.verticalOverflow = VerticalWrapMode.Overflow;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.viewport = viewport;
        scrollRect.content = contentRoot;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    private Button CreateButton(
        RectTransform parent, string label, Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateRect(label + " Button", parent);
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot =
            new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(260f, 54f);
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

    private static RectTransform CreateParchmentWindow(string name, RectTransform root)
    {
        RectTransform window = CreateRect(name, root);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(900f, 690f);
        SimpleMercenaryHireUIFactory.ApplyParchmentPanel(
            window.gameObject.AddComponent<Image>());
        return window;
    }

    private void ShowLetter()
    {
        letterWindow.gameObject.SetActive(true);
        creditsWindow.gameObject.SetActive(false);
    }

    private void ShowCredits()
    {
        letterWindow.gameObject.SetActive(false);
        creditsWindow.gameObject.SetActive(true);
    }

    private static void ContinueGame() => SceneManager.LoadScene(GameSceneName);

    private static Text CreateText(string content, RectTransform parent, Font font,
        int size, FontStyle style, TextAnchor alignment, Color color)
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

    private static Font LoadTitleFont()
    {
        Font font = Font.CreateDynamicFontFromOSFont(
            new[] { "Yu Mincho Demibold", "Yu Mincho", "Yu Gothic UI" }, 28);
        return font ?? Resources.Load<Font>("Fonts/ZenKurenaido-Regular") ??
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
