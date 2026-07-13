using UnityEngine;

/// <summary>
/// Small view abstraction over the equipment-detail overlay widgets.
/// Bundles the fine-grained UI setters that CharacterEquipmentController
/// previously received as individual delegates (step B-2), so the
/// controller depends on one view surface instead of eight lambdas.
/// Implemented by SimpleMercenaryHireUI
/// (SimpleMercenaryHireUI.CharacterEquipment.cs).
/// </summary>
public interface IEquipmentDetailView
{
    /// <summary>Whether the overlay has been built and can be shown.</summary>
    bool HasOverlay { get; }

    void SetTitle(string title, Color color);

    void SetDetailText(string text);

    void SetEnhanceButton(bool interactable, string label);

    void SetSellButton(bool interactable, string label);

    void SetLockButtonLabel(string label);

    void ShowOverlay();

    /// <summary>Hides the overlay and clears the detail selection.</summary>
    void HideOverlay();
}
