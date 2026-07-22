using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GenericHoverTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    private string content;
    private Font font;
    private RectTransform parent;
    private RectTransform tooltip;
    private Text text;

    public void Configure(string tooltipContent, Font displayFont, RectTransform tooltipParent)
    {
        content = tooltipContent;
        font = displayFont;
        parent = tooltipParent;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrWhiteSpace(content) || parent == null)
        {
            return;
        }

        CreateTooltip();
        text.text = content;
        tooltip.sizeDelta = new Vector2(330f, Mathf.Clamp(70f + content.Length * 0.65f, 92f, 210f));
        Vector2 pointer = eventData != null ? eventData.position : Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, pointer,
            eventData != null ? eventData.enterEventCamera : null, out Vector2 local);
        Rect bounds = parent.rect;
        tooltip.pivot = new Vector2(local.x >= 0f ? 1f : 0f, local.y >= 0f ? 1f : 0f);
        Vector2 size = tooltip.sizeDelta;
        local.x = Mathf.Clamp(local.x + (tooltip.pivot.x > 0f ? -12f : 12f), bounds.xMin + tooltip.pivot.x * size.x + 8f, bounds.xMax - (1f - tooltip.pivot.x) * size.x - 8f);
        local.y = Mathf.Clamp(local.y + (tooltip.pivot.y > 0f ? -12f : 12f), bounds.yMin + tooltip.pivot.y * size.y + 8f, bounds.yMax - (1f - tooltip.pivot.y) * size.y - 8f);
        tooltip.anchoredPosition = local;
        tooltip.SetAsLastSibling();
        tooltip.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        if (tooltip != null)
        {
            Destroy(tooltip.gameObject);
        }
    }

    private void CreateTooltip()
    {
        if (tooltip != null)
        {
            return;
        }

        GameObject root = new GameObject("Item Hover Tooltip", typeof(RectTransform), typeof(Image));
        tooltip = root.GetComponent<RectTransform>();
        tooltip.SetParent(parent, false);
        Image background = root.GetComponent<Image>();
        background.color = new Color(0.075f, 0.045f, 0.025f, 0.96f);
        background.raycastTarget = false;
        GameObject label = new GameObject("Content", typeof(RectTransform), typeof(Text));
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.SetParent(tooltip, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10f, 8f);
        labelRect.offsetMax = new Vector2(-10f, -8f);
        text = label.GetComponent<Text>();
        text.font = font ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 13;
        text.alignment = TextAnchor.UpperLeft;
        text.color = new Color(0.97f, 0.9f, 0.74f);
        text.raycastTarget = false;
        root.SetActive(false);
    }

    private void Hide()
    {
        if (tooltip != null)
        {
            tooltip.gameObject.SetActive(false);
        }
    }
}
