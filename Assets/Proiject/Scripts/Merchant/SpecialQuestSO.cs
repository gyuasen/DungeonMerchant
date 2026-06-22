using UnityEngine;

[CreateAssetMenu(
    fileName = "SpecialQuest",
    menuName = "DungeonMerchant/Special Quest")]
public class SpecialQuestSO : ScriptableObject
{
    public string questTitle;
    public QuestType questType;
    public string targetName;
    [Min(1)] public int requiredAmount = 1;
    [Min(0)] public int goldReward = 500;
    [Min(0)] public int merchantExperienceReward = 100;

    public QuestRecord CreateRecord()
    {
        return new QuestRecord
        {
            title = questTitle,
            questType = questType,
            targetName = targetName,
            requiredAmount = requiredAmount,
            goldReward = goldReward,
            experienceReward = merchantExperienceReward,
            isSpecial = true,
            specialQuestId = name
        };
    }
}
