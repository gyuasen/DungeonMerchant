using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private RectTransform CreateScrollableContent(
        RectTransform parent,
        string viewportName,
        string contentName,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        RectTransform viewport = CreateUIObject(viewportName, parent);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = offsetMin;
        viewport.offsetMax = offsetMax;
        viewport.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.1f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform content = CreateUIObject(contentName, viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        return content;
    }

    private void CreateScrollSection(
        RectTransform content,
        string text,
        ref float top)
    {
        CreateText(content, text, 19, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(14f, top - 30f), new Vector2(-14f, top), ParchmentTextColor);
        top -= 38f;
    }

    private void CreateScrollLabel(
        RectTransform content,
        string text,
        ref float top)
    {
        CreateText(content, text, 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(22f, top - 28f), new Vector2(-22f, top), ParchmentMutedColor);
        top -= 32f;
    }

    private void CreateScrollButton(
        RectTransform content,
        string text,
        UnityEngine.Events.UnityAction action,
        ref float top)
    {
        Button button = CreateActionButton(content, text, action);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(360f, 30f);
        rect.anchoredPosition = new Vector2(18f, top);
        top -= 34f;
    }
}
