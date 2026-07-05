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

public sealed class BattlePageUI : BattlePageUIBase
{
}

public sealed class RoadBattlePageUI : BattlePageUIBase
{
}

public sealed class DungeonPageUI : BattlePageUIBase
{
}
