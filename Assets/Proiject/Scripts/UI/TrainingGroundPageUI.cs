using System;

public sealed class TrainingGroundPageUI : ListPageUIBase
{
    private Action refreshAction;

    public void ConfigureTrainingGround(Action targetRefreshAction)
    {
        refreshAction = targetRefreshAction;
    }

    public override void Refresh()
    {
        refreshAction?.Invoke();
    }
}
