using System;

public static class QuestBoardLayout
{
    public static float GetPaperRotationDegrees(QuestRecord quest)
    {
        string identifier = quest != null && !string.IsNullOrEmpty(quest.specialQuestId)
            ? quest.specialQuestId
            : quest != null
                ? $"{quest.title}|{quest.targetName}|{quest.deadlineDay}|{quest.requiredAmount}"
                : string.Empty;
        uint hash = 2166136261;
        for (int i = 0; i < identifier.Length; i++)
        {
            hash ^= identifier[i];
            hash *= 16777619;
        }
        return hash % 601 / 100f - 3f;
    }
}
