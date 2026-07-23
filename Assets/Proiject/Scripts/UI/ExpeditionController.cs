using System;
using System.Collections.Generic;

public sealed class ExpeditionController
{
    private readonly DungeonExpeditionManager expeditionManager;
    private readonly DungeonRunManager dungeonRunManager;
    private readonly MercenaryHireManager hireManager;
    private readonly MercenaryPartyManager partyManager;
    private readonly TransportManager transportManager;
    private readonly Action<string> setStatus;
    private readonly Action redraw;
    private readonly List<MercenaryInstance> selectedMembers = new List<MercenaryInstance>();
    private DungeonDataSO selectedDungeon;

    public ExpeditionController(DungeonExpeditionManager expeditionManager, DungeonRunManager dungeonRunManager, MercenaryHireManager hireManager, MercenaryPartyManager partyManager, TransportManager transportManager, Action<string> setStatus, Action redraw)
    {
        this.expeditionManager = expeditionManager;
        this.dungeonRunManager = dungeonRunManager;
        this.hireManager = hireManager;
        this.partyManager = partyManager;
        this.transportManager = transportManager;
        this.setStatus = setStatus;
        this.redraw = redraw;
    }

    public DungeonDataSO SelectedDungeon => selectedDungeon;
    public IReadOnlyList<DungeonExpedition> ActiveExpeditions => expeditionManager != null ? expeditionManager.ActiveExpeditions : Array.Empty<DungeonExpedition>();

    public IEnumerable<DungeonDataSO> GetAvailableDungeons()
    {
        if (dungeonRunManager == null)
        {
            yield break;
        }
        foreach (DungeonDataSO dungeon in dungeonRunManager.AvailableDungeons)
        {
            if (dungeon != null && dungeon.nearbyTownIndex != WorldMapService.HiddenIslandTownIndex && dungeonRunManager.GetClearedFloors(dungeon) >= dungeon.totalFloors)
            {
                yield return dungeon;
            }
        }
    }

    public IEnumerable<MercenaryInstance> GetAvailableMembers()
    {
        if (hireManager == null)
        {
            yield break;
        }
        TrainingGroundManager trainingGroundManager =
            UnityEngine.Object.FindObjectOfType<TrainingGroundManager>();
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && mercenary.IsContractActive && (trainingGroundManager == null || !trainingGroundManager.IsMercenaryTraining(mercenary.InstanceId)) && (partyManager == null || !partyManager.Contains(mercenary)) && (transportManager == null || !transportManager.IsMercenaryOnTransportDuty(mercenary.InstanceId)) && (expeditionManager == null || !expeditionManager.IsMercenaryOnExpeditionDuty(mercenary.InstanceId)))
            {
                yield return mercenary;
            }
        }
    }

    public void SelectDungeon(DungeonDataSO dungeon)
    {
        selectedDungeon = dungeon;
        redraw?.Invoke();
    }

    public bool IsSelected(MercenaryInstance mercenary)
    {
        return mercenary != null && selectedMembers.Contains(mercenary);
    }

    public void ToggleMember(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            return;
        }
        if (selectedMembers.Remove(mercenary))
        {
            redraw?.Invoke();
            return;
        }
        if (selectedMembers.Count >= 3)
        {
            setStatus?.Invoke("遠征隊員は3人まで選択できます");
            return;
        }
        selectedMembers.Add(mercenary);
        redraw?.Invoke();
    }

    public int GetSelectedStrength()
    {
        int result = 0;
        foreach (MercenaryInstance member in selectedMembers)
        {
            result += (member.Attack + member.Defense + member.MaxHP / 10) * member.Level;
        }
        return result;
    }

    public int GetRequiredStrength()
    {
        return expeditionManager != null ? expeditionManager.GetRequiredStrength(selectedDungeon) : 0;
    }

    public void Dispatch()
    {
        ExpeditionFormationResult result = expeditionManager != null ? expeditionManager.TryFormExpedition(selectedDungeon, selectedMembers) : ExpeditionFormationResult.InvalidDungeon;
        setStatus?.Invoke(GetResultMessage(result));
        if (result == ExpeditionFormationResult.Succeeded)
        {
            selectedDungeon = null;
            selectedMembers.Clear();
        }
        redraw?.Invoke();
    }

    public void Recall(DungeonExpedition expedition)
    {
        expeditionManager?.RecallExpedition(expedition);
        setStatus?.Invoke("遠征部隊を呼び戻しました");
        redraw?.Invoke();
    }

    public static string GetResultMessage(ExpeditionFormationResult result)
    {
        switch (result)
        {
            case ExpeditionFormationResult.Succeeded: return "遠征部隊を派遣しました";
            case ExpeditionFormationResult.DungeonNotCleared: return "完全踏破済みのダンジョンのみ遠征できます";
            case ExpeditionFormationResult.HiddenDungeon: return "隠しダンジョンは遠征できません";
            case ExpeditionFormationResult.InvalidMembers: return "隊員は雇用済み・編成外・任務外から1〜3人選んでください";
            default: return "遠征先を選択してください";
        }
    }
}
