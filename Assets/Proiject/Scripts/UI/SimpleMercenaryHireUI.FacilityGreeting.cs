using System;
using UnityEngine;
using UnityEngine.UI;

// Facility keys: Tavern, Guild, Market, Blacksmith, Warehouse, Clinic, Temple.
public partial class SimpleMercenaryHireUI
{
    private void BuildFacilityGreetingOverlay()
    {
        facilityGreetingOverlay = CreateUIObject("Facility Greeting Overlay", overlayRoot);
        facilityGreetingOverlay.anchorMin = Vector2.zero;
        facilityGreetingOverlay.anchorMax = Vector2.one;
        facilityGreetingOverlay.offsetMin = Vector2.zero;
        facilityGreetingOverlay.offsetMax = Vector2.zero;
        facilityGreetingOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window = CreateUIObject("Facility Greeting Window", facilityGreetingOverlay);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(760f, 460f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        RectTransform content = CreateUIObject("Facility Greeting Content", window);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.offsetMin = new Vector2(32f, 78f);
        content.offsetMax = new Vector2(-32f, -28f);
        facilityGreetingTitle = CreateText(content, string.Empty, 25, FontStyle.Bold,
            TextAnchor.UpperLeft, new Vector2(0f, -52f), new Vector2(-220f, 0f),
            ParchmentTextColor);
        facilityGreetingDialogue = CreateText(content, string.Empty, 18, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(0f, -215f), new Vector2(-220f, -66f),
            ParchmentTextColor);
        facilityGreetingDialogue.horizontalOverflow = HorizontalWrapMode.Wrap;
        facilityGreetingDialogue.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform portraitRect = CreateUIObject("Staff Portrait", content);
        portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(1f, 0.5f);
        portraitRect.pivot = new Vector2(1f, 0.5f);
        portraitRect.sizeDelta = new Vector2(185f, 270f);
        portraitRect.anchoredPosition = new Vector2(0f, -8f);
        facilityGreetingPortrait = portraitRect.gameObject.AddComponent<Image>();
        facilityGreetingPortrait.preserveAspect = true;

        Button enterButton = CreateActionButton(window, "入る", EnterFacilityFromGreeting);
        RectTransform enterRect = enterButton.GetComponent<RectTransform>();
        enterRect.anchorMin = enterRect.anchorMax = new Vector2(1f, 0f);
        enterRect.pivot = new Vector2(1f, 0f);
        enterRect.sizeDelta = new Vector2(115f, 44f);
        enterRect.anchoredPosition = new Vector2(-152f, 20f);
        Button backButton = CreateActionButton(window, "戻る", HideFacilityGreeting);
        RectTransform backRect = backButton.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(1f, 0f);
        backRect.sizeDelta = new Vector2(115f, 44f);
        backRect.anchoredPosition = new Vector2(-24f, 20f);
        facilityGreetingOverlay.gameObject.SetActive(false);
    }

    private void OpenFacilityWithGreeting(string facilityKey, Action destination)
    {
        int currentDay = dayManager != null ? dayManager.CurrentDay : 1;
        int townIndex = townProgressState != null ? townProgressState.CurrentTownIndex : 0;
        if (!facilityGreetingController.ShouldShowGreeting(currentDay, townIndex, facilityKey))
        {
            destination?.Invoke();
            return;
        }
        string townName = townIndex >= 0 && townIndex < WorldMapService.TownNames.Length
            ? WorldMapService.TownNames[townIndex]
            : "この町";
        FacilityGreeting greeting = facilityGreetingController.GetGreeting(
            currentDay, townIndex, townName, facilityKey);
        facilityGreetingTitle.text = greeting.Title;
        facilityGreetingDialogue.text = greeting.Dialogue;
        Sprite portrait = Resources.Load<Sprite>("UI/Staff/" + facilityKey);
        facilityGreetingPortrait.sprite = portrait;
        facilityGreetingPortrait.gameObject.SetActive(portrait != null);
        pendingFacilityKey = facilityKey;
        pendingFacilityDestination = destination;
        facilityGreetingOverlay.SetAsLastSibling();
        facilityGreetingOverlay.gameObject.SetActive(true);
    }

    private void EnterFacilityFromGreeting()
    {
        int currentDay = dayManager != null ? dayManager.CurrentDay : 1;
        int townIndex = townProgressState != null ? townProgressState.CurrentTownIndex : 0;
        facilityGreetingController.MarkEntered(currentDay, townIndex, pendingFacilityKey);
        Action destination = pendingFacilityDestination;
        HideFacilityGreeting();
        destination?.Invoke();
    }

    private void HideFacilityGreeting()
    {
        pendingFacilityKey = null;
        pendingFacilityDestination = null;
        facilityGreetingOverlay?.gameObject.SetActive(false);
    }
}
