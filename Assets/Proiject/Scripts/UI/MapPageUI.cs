using UnityEngine.Events;

public abstract class MapPageUIBase : UIPageBase
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

public sealed class GlobalMapPageUI : MapPageUIBase
{
}

public sealed class WorldMapPageUI : MapPageUIBase
{
}

public sealed class TownMapPageUI : MapPageUIBase
{
}
