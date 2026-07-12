using UnityEngine.Events;

// Merged base class replacing the former identical duplicates BattlePageUIBase and MapPageUIBase.
public abstract class RefreshOnlyPageUIBase : UIPageBase
{
    private UnityAction refreshAction;

    public void Configure(UnityAction refresh)
    {
        refreshAction = refresh;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}
