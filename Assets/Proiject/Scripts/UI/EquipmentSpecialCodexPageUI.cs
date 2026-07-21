using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentSpecialCodexPageUI : MonoBehaviour
{
    private readonly List<EquipmentSpecialPageModel> pages = new List<EquipmentSpecialPageModel>();
    private Font titleFont;
    private Font bodyFont;
    private RectTransform content;
    private Text pageNumberText;
    private Button previousButton;
    private Button nextButton;
    private int pageIndex;

    public void Initialize(Font configuredTitleFont, Font configuredBodyFont)
    {
        titleFont = configuredTitleFont;
        bodyFont = configuredBodyFont != null ? configuredBodyFont : configuredTitleFont;
        RectTransform root = transform as RectTransform;
        content = CreatePanel(root, "Special Page", new Vector2(.5f, .54f), new Vector2(700f, 430f), new Color(.92f, .82f, .61f, 1f));
        previousButton = CreateButton(root, "◀ 前の特殊装備", new Vector2(.2f, 0f));
        nextButton = CreateButton(root, "次の特殊装備 ▶", new Vector2(.8f, 0f));
        pageNumberText = CreateText(root, string.Empty, 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 0f), new Vector2(130f, 32f), new Vector2(0f, 24f));
        previousButton.onClick.AddListener(Previous);
        nextButton.onClick.AddListener(Next);
    }

    public void SetPages(IEnumerable<EquipmentSpecialPageModel> source)
    {
        pages.Clear();
        if (source != null)
        {
            pages.AddRange(source);
        }
        pageIndex = 0;
        Refresh();
    }

    private void Previous()
    {
        pageIndex = Mathf.Max(0, pageIndex - 1);
        Refresh();
    }

    private void Next()
    {
        pageIndex = Mathf.Min(pages.Count - 1, pageIndex + 1);
        Refresh();
    }

    private void Refresh()
    {
        for (int index = content.childCount - 1; index >= 0; index--)
        {
            Destroy(content.GetChild(index).gameObject);
        }
        bool hasPage = pages.Count > 0;
        if (hasPage)
        {
            BuildPage(pages[pageIndex]);
        }
        else
        {
            CreateText(content, "特殊装備はまだありません", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), new Vector2(500f, 40f), Vector2.zero);
        }
        pageNumberText.text = hasPage ? string.Format("{0} / {1}", pageIndex + 1, pages.Count) : "0 / 0";
        previousButton.interactable = hasPage && pageIndex > 0;
        nextButton.interactable = hasPage && pageIndex < pages.Count - 1;
    }

    private void BuildPage(EquipmentSpecialPageModel page)
    {
        bool isUndiscoveredSet = page.Kind == EquipmentSpecialPageKind.Set && page.DiscoveredCount == 0;
        string titleText = page.Kind == EquipmentSpecialPageKind.Set
            ? isUndiscoveredSet ? "セット: ？？？" : "セット: " + page.Title
            : "特殊装備";
        Text title = CreateText(content, titleText, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 1f), new Vector2(620f, 38f), new Vector2(0f, -25f));
        title.color = isUndiscoveredSet ? new Color(.35f, .21f, .09f, .55f) : page.AccentColor;
        if (page.Kind == EquipmentSpecialPageKind.Set)
        {
            for (int index = 0; index < page.Slots.Count; index++)
            {
                BuildSlot(page.Slots[index], index);
            }
            string setBonusText = isUndiscoveredSet ? "未発見" : page.SetBonusText;
            CreateText(content, setBonusText, 14, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(.5f, 0f), new Vector2(650f, 54f), new Vector2(0f, 43f));
        }
        else
        {
            BuildItem(CreatePanel(content, "Single Item", new Vector2(.5f, .52f), new Vector2(620f, 255f), new Color(.35f, .21f, .09f, .12f)), page.SingleItem);
        }
        CreateText(content, string.Format("登録済み {0}/{1}", page.DiscoveredCount, page.TotalCount), 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 0f), new Vector2(240f, 30f), new Vector2(0f, 10f));
    }

    private void BuildSlot(EquipmentSpecialSlotModel slot, int index)
    {
        float[] horizontalAnchors = { .174f, .5f, .826f };
        RectTransform panel = CreatePanel(content, JapaneseDisplayText.GetEquipmentSlot(slot.Slot), new Vector2(horizontalAnchors[index], .542f), new Vector2(216f, 250f), new Color(.35f, .21f, .09f, .22f));
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(.35f, .21f, .09f, .46f);
        outline.effectDistance = new Vector2(1f, -1f);
        CreateText(panel, JapaneseDisplayText.GetEquipmentSlot(slot.Slot), 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 1f), new Vector2(200f, 28f), new Vector2(0f, -14f));
        bool hasMultipleCandidates = slot.Candidates.Count > 1;
        float candidateHeight = hasMultipleCandidates ? 96f : 204f;
        float top = -34f;
        foreach (EquipmentSpecialItemModel item in slot.Candidates)
        {
            Text text = CreateText(panel, BuildItemText(item, hasMultipleCandidates), hasMultipleCandidates ? 10 : 12, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(200f, candidateHeight), new Vector2(8f, top));
            text.supportRichText = true;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            top -= candidateHeight + 4f;
        }
    }

    private void BuildItem(RectTransform panel, EquipmentSpecialItemModel item)
    {
        Text text = CreateText(panel, BuildItemText(item, false), 16, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(580f, 220f), new Vector2(20f, -20f));
        text.supportRichText = true;
    }

    private static string BuildItemText(
        EquipmentSpecialItemModel item,
        bool hasMultipleCandidates)
    {
        if (!item.Discovered)
        {
            return "<b>？？？</b>\n未発見";
        }
        int descriptionLength = hasMultipleCandidates ? 64 : 160;
        int effectLength = hasMultipleCandidates ? 48 : 96;
        string description = string.IsNullOrWhiteSpace(item.Description)
            ? "説明なし"
            : ShortenText(item.Description, descriptionLength);
        string effectText = ShortenText(item.EffectText, effectLength);
        return string.Format("<b>{0}  Rank {1}</b>\n{2}\n特殊効果: {3}", item.Name, item.Rank, description, effectText);
    }

    private static string ShortenText(string value, int maximumLength)
    {
        string normalized = string.IsNullOrWhiteSpace(value)
            ? "なし"
            : value.Replace("\r\n", "、").Replace("\n", "、");
        return normalized.Length <= maximumLength
            ? normalized
            : normalized.Substring(0, maximumLength - 1) + "…";
    }

    private RectTransform CreatePanel(RectTransform parent, string name, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        panel.anchorMin = panel.anchorMax = anchor;
        panel.pivot = new Vector2(.5f, .5f);
        panel.sizeDelta = size;
        panelObject.GetComponent<Image>().color = color;
        return panel;
    }

    private Button CreateButton(RectTransform parent, string label, Vector2 anchor)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.sizeDelta = new Vector2(145f, 38f);
        buttonObject.GetComponent<Image>().color = new Color(.35f, .21f, .09f, .8f);
        CreateText(rect, label, 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), rect.sizeDelta, Vector2.zero);
        return buttonObject.GetComponent<Button>();
    }

    private Text CreateText(RectTransform parent, string value, int size, FontStyle style, TextAnchor anchor, Vector2 anchorPosition, Vector2 dimensions, Vector2 position)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchorPosition;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
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
        text.color = new Color(.18f, .10f, .04f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
