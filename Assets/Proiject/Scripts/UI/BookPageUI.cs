using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Runtime-built two-page parchment codex. Each page contains two entries.</summary>
public sealed class BookPageUI : MonoBehaviour
{
    private const float PageWidth = 310f;
    private const float PageHeight = 410f;

    public sealed class Entry
    {
        public string Name;
        public string Detail;
        public Sprite Sprite;
        public bool Discovered;
        public string Subtitle;
    }

    private const int EntriesPerPage = 2;
    private readonly List<Entry> entries = new List<Entry>();
    private RectTransform leftPage;
    private RectTransform rightPage;
    private Text pageNumberText;
    private Button previousButton;
    private Button nextButton;
    private int spreadIndex;
    private Font titleFont;
    private Font bodyFont;

    public void Initialize(string title, Font configuredTitleFont, Font configuredBodyFont)
    {
        titleFont = configuredTitleFont;
        bodyFont = configuredBodyFont != null ? configuredBodyFont : configuredTitleFont;
        RectTransform root = transform as RectTransform;
        if (!string.IsNullOrEmpty(title))
        {
            CreateText(root, title, 24, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(620f, 42f));
        }
        leftPage = CreatePage(root, "Left Page", new Vector2(0.2575f, 0.53f));
        rightPage = CreatePage(root, "Right Page", new Vector2(0.7425f, 0.53f));
        previousButton = CreateButton(root, "◀ 前のページ", new Vector2(0.2f, 0f));
        nextButton = CreateButton(root, "次のページ ▶", new Vector2(0.8f, 0f));
        pageNumberText = CreateText(root, string.Empty, 16, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(130f, 32f));
        previousButton.onClick.AddListener(PreviousSpread);
        nextButton.onClick.AddListener(NextSpread);
    }

    public void SetEntries(IEnumerable<Entry> source)
    {
        entries.Clear();
        if (source != null)
        {
            entries.AddRange(source);
        }

        spreadIndex = 0;
        Refresh();
    }

    private void PreviousSpread()
    {
        spreadIndex = Mathf.Max(0, spreadIndex - 1);
        Refresh();
    }

    private void NextSpread()
    {
        spreadIndex = Mathf.Min(SpreadCount - 1, spreadIndex + 1);
        Refresh();
    }

    private int SpreadCount => Mathf.Max(1, Mathf.CeilToInt(entries.Count / (float)(EntriesPerPage * 2)));

    private void Refresh()
    {
        PopulatePage(leftPage, spreadIndex * EntriesPerPage * 2);
        PopulatePage(rightPage, spreadIndex * EntriesPerPage * 2 + EntriesPerPage);
        pageNumberText.text = string.Format("{0} / {1}", spreadIndex + 1, SpreadCount);
        previousButton.interactable = spreadIndex > 0;
        nextButton.interactable = spreadIndex < SpreadCount - 1;
    }

    private void PopulatePage(RectTransform page, int startIndex)
    {
        for (int i = page.childCount - 1; i >= 0; i--)
        {
            Destroy(page.GetChild(i).gameObject);
        }

        for (int i = 0; i < EntriesPerPage; i++)
        {
            int index = startIndex + i;
            if (index < entries.Count)
            {
                CreateEntry(page, entries[index], i);
            }
        }
    }

    private RectTransform CreatePage(RectTransform parent, string name, Vector2 anchor)
    {
        GameObject pageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform page = pageObject.GetComponent<RectTransform>();
        page.SetParent(parent, false);
        page.anchorMin = page.anchorMax = anchor;
        page.pivot = new Vector2(0.5f, 0.5f);
        page.sizeDelta = new Vector2(PageWidth, PageHeight);
        page.GetComponent<Image>().color = new Color(0.92f, 0.82f, 0.61f, 1f);
        return page;
    }

    private void CreateEntry(RectTransform parent, Entry entry, int slot)
    {
        GameObject entryObject = new GameObject("Codex Entry", typeof(RectTransform), typeof(Image));
        RectTransform root = entryObject.GetComponent<RectTransform>();
        root.SetParent(parent, false);
        root.anchorMin = new Vector2(0f, 1f);
        root.anchorMax = new Vector2(1f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.offsetMin = new Vector2(12f, -192f - slot * 202f);
        root.offsetMax = new Vector2(-12f, -10f - slot * 202f);
        root.GetComponent<Image>().color = new Color(0.35f, 0.21f, 0.09f, 0.17f);
        Outline outline = root.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.21f, 0.09f, 0.45f);
        outline.effectDistance = new Vector2(1f, -1f);
        GameObject imageObject = new GameObject("Image", typeof(RectTransform), typeof(Image));
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.SetParent(root, false);
        imageRect.anchorMin = imageRect.anchorMax = new Vector2(0f, 1f);
        imageRect.pivot = new Vector2(0f, 1f);
        imageRect.sizeDelta = new Vector2(64f, 64f);
        imageRect.anchoredPosition = new Vector2(10f, -10f);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = entry.Discovered ? entry.Sprite : null;
        image.color = image.sprite == null ? new Color(0.22f, 0.14f, 0.08f, 0.28f) : Color.white;
        CreateText(imageRect, image.sprite == null ? "?" : string.Empty, 34, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));
        string name = entry.Discovered ? entry.Name : "？？？";
        string detail = entry.Discovered ? entry.Detail : "未発見";
        Text nameText = CreateText(root, name, 14, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(82f, -31f), new Vector2(-10f, -10f));
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 10;
        nameText.resizeTextMaxSize = 14;
        nameText.verticalOverflow = VerticalWrapMode.Truncate;
        if (entry.Discovered && !string.IsNullOrWhiteSpace(entry.Subtitle))
        {
            CreateText(root, entry.Subtitle, 12, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(82f, -51f), new Vector2(-10f, -33f));
        }
        Text detailText = CreateText(root, detail, 11, FontStyle.Normal, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(82f, -174f), new Vector2(-10f, -55f));
        detailText.supportRichText = true;
        detailText.verticalOverflow = VerticalWrapMode.Truncate;
    }

    private Button CreateButton(RectTransform parent, string label, Vector2 anchor)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.sizeDelta = new Vector2(125f, 38f);
        buttonObject.GetComponent<Image>().color = new Color(0.35f, 0.21f, 0.09f, 0.8f);
        CreateText(rect, label, 14, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, rect.sizeDelta);
        return buttonObject.GetComponent<Button>();
    }

    private Text CreateText(RectTransform parent, string value, int size, FontStyle style, TextAnchor anchor, Vector2 min, Vector2 max, Vector2 position, Vector2 dimensions)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        if (min == max)
        {
            rect.pivot = min;
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
        }
        else
        {
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = position;
            rect.offsetMax = dimensions;
        }

        Text text = textObject.GetComponent<Text>();
        text.font = style == FontStyle.Normal ? bodyFont : titleFont;
        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = new Color(0.18f, 0.10f, 0.04f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
