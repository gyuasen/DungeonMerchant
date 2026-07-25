using UnityEngine;

public enum MercenaryDuty
{
    None,
    Party,
    Training,
    RoadTransit,
    Expedition
}

public static class MercenaryDutyService
{
    public static bool IsOnDuty(string instanceId)
    {
        return GetDuty(instanceId) != MercenaryDuty.None;
    }

    public static MercenaryDuty GetDuty(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return MercenaryDuty.None;
        }

        MercenaryPartyManager partyManager = Object.FindObjectOfType<MercenaryPartyManager>();
        if (partyManager != null)
        {
            foreach (MercenaryInstance member in partyManager.Members)
            {
                if (member != null && member.InstanceId == instanceId)
                {
                    return MercenaryDuty.Party;
                }
            }
        }

        TrainingGroundManager trainingGroundManager = Object.FindObjectOfType<TrainingGroundManager>();
        if (trainingGroundManager != null && trainingGroundManager.IsMercenaryTraining(instanceId))
        {
            return MercenaryDuty.Training;
        }

        RoadCargoSession roadCargoSession = Object.FindObjectOfType<RoadCargoSession>();
        if (roadCargoSession != null && roadCargoSession.IsCompanionInTransit(instanceId))
        {
            return MercenaryDuty.RoadTransit;
        }

        DungeonExpeditionManager expeditionManager = Object.FindObjectOfType<DungeonExpeditionManager>();
        return expeditionManager != null && expeditionManager.IsMercenaryOnExpedition(instanceId)
            ? MercenaryDuty.Expedition
            : MercenaryDuty.None;
    }

    public static bool IsOnNonExpeditionDuty(string instanceId)
    {
        MercenaryDuty duty = GetDuty(instanceId);
        return duty != MercenaryDuty.None && duty != MercenaryDuty.Expedition;
    }

    public static bool IsOnDutyExcept(string instanceId, MercenaryDuty allowedDuty)
    {
        MercenaryDuty duty = GetDuty(instanceId);
        return duty != MercenaryDuty.None && duty != allowedDuty;
    }
}
