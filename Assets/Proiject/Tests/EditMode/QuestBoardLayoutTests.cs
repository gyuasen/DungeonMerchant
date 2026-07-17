using NUnit.Framework;

public class QuestBoardLayoutTests
{
    [Test]
    public void GetPaperRotationDegrees_IsDeterministicForQuestIdentity()
    {
        QuestRecord quest = new QuestRecord
        {
            title = "討伐依頼",
            targetName = "Goblin",
            deadlineDay = 6,
            requiredAmount = 3
        };

        float first = QuestBoardLayout.GetPaperRotationDegrees(quest);
        float second = QuestBoardLayout.GetPaperRotationDegrees(quest);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.InRange(-3f, 3f));
    }
}
