using UnityEngine.Events;

public abstract class BattlePageUIBase : UIPageBase
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
