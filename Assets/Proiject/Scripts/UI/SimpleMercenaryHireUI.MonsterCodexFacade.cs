using UnityEngine;

public partial class SimpleMercenaryHireUI
{
    private void BuildMonsterCollectionOverlay()
    {
        RectTransform prefabOverlay = activeView != null
            ? activeView.GetOverlay(SimpleMercenaryHireOverlaySlot.MonsterCollection)
            : null;
        monsterCodexOverlayView = new MonsterCodexOverlayView(
            uiFactory,
            new SimpleMercenaryHireUIView.MonsterCodexReferences
            {
                overlay = prefabOverlay
            },
            overlayRoot,
            uiFont,
            uiBodyFont,
            monsterCodexManager,
            HideMonsterCollection);
        monsterCodexOverlayView.Build();
    }

    private void ShowMonsterCollection()
    {
        monsterCodexOverlayView?.Show();
    }

    private void HideMonsterCollection()
    {
        monsterCodexOverlayView?.Hide();
    }
}
