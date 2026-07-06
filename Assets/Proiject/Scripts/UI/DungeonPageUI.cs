using UnityEngine.Events;

public sealed class DungeonPageUI : BattlePageUIBase
{
    private UnityAction selectionRefreshAction;

    public void ConfigureSelectionRefresh(UnityAction refresh)
    {
        selectionRefreshAction = refresh;
    }

    public void RefreshSelection()
    {
        selectionRefreshAction?.Invoke();
    }
}
