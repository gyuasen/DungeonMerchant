using NUnit.Framework;

public sealed class DungeonBattleControllerLogTests
{
    [Test]
    public void AppendBattleMessage_PreservesOnlySpecialEnemyColorTag()
    {
        DungeonBattleController controller = CreateController();

        string log = controller.AppendBattleMessage(
            "<color=#D86BFF>異形の敵</color> <b>攻撃</b>",
            BattleLogType.Enemy);

        Assert.That(log, Does.Contain("<color=#D86BFF>異形の敵</color>"));
        Assert.That(log, Does.Contain("&lt;b&gt;攻撃&lt;/b&gt;"));
    }

    [Test]
    public void AppendBattleMessage_KeepsLatestTwoHundredFiftyLines()
    {
        DungeonBattleController controller = CreateController();
        string log = string.Empty;
        for (int i = 0; i < 260; i++)
        {
            log = controller.AppendBattleMessage($"line-{i}", BattleLogType.System);
        }

        Assert.That(log.Split('\n').Length, Is.EqualTo(250));
        Assert.That(log, Does.Not.Contain("line-0\n"));
        Assert.That(log, Does.Contain("line-259"));
    }

    private static DungeonBattleController CreateController()
    {
        return new DungeonBattleController(
            null, null, null, null,
            null, null, null, null,
            null, null, null, null,
            null, null, null, null);
    }
}
