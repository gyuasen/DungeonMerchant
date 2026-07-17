using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoryMilestone
{
    OpeningDebtNotice,
    FirstMercenary,
    FirstDungeonClear,
    LeafUnlocked,
    RegionGateCleared,
    AbyssReached,
    HiddenIslandReached,
    DebtCleared
}

public readonly struct StoryMilestoneInfo
{
    public StoryMilestoneInfo(string title, string body)
    {
        Title = title;
        Body = body;
    }

    public string Title { get; }
    public string Body { get; }
}

public sealed class StoryProgressManager : MonoBehaviour
{
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private TownProgressState townProgressState;
    [SerializeField] private DebtManager debtManager;

    private readonly HashSet<StoryMilestone> completedMilestones =
        new HashSet<StoryMilestone>();
    private readonly Queue<StoryMilestone> pendingPresentations =
        new Queue<StoryMilestone>();
    private bool isRestoring;

    public event Action<StoryMilestone> MilestoneCompleted;

    public IReadOnlyCollection<StoryMilestone> CompletedMilestones =>
        completedMilestones;
    public bool IsRestoring => isRestoring;

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void BeginNewGame()
    {
        if (!isRestoring)
        {
            TryComplete(StoryMilestone.OpeningDebtNotice);
        }
    }

    public void BeginRestore()
    {
        isRestoring = true;
    }

    public void RestoreCompletedMilestones(
        IEnumerable<StoryMilestone> restoredMilestones)
    {
        completedMilestones.Clear();
        pendingPresentations.Clear();
        if (restoredMilestones != null)
        {
            foreach (StoryMilestone milestone in restoredMilestones)
            {
                if (Enum.IsDefined(typeof(StoryMilestone), milestone))
                {
                    completedMilestones.Add(milestone);
                }
            }
        }
        isRestoring = false;
    }

    public void EndRestore()
    {
        isRestoring = false;
    }

    public bool IsCompleted(StoryMilestone milestone)
    {
        return completedMilestones.Contains(milestone);
    }

    public bool TryComplete(StoryMilestone milestone)
    {
        if (isRestoring || !completedMilestones.Add(milestone))
        {
            return false;
        }

        pendingPresentations.Enqueue(milestone);
        MilestoneCompleted?.Invoke(milestone);
        return true;
    }

    public bool TryDequeuePendingPresentation(out StoryMilestone milestone)
    {
        if (pendingPresentations.Count == 0)
        {
            milestone = default;
            return false;
        }

        milestone = pendingPresentations.Dequeue();
        return true;
    }

    public StoryMilestoneInfo GetMilestoneInfo(StoryMilestone milestone)
    {
        switch (milestone)
        {
            case StoryMilestone.OpeningDebtNotice:
                return new StoryMilestoneInfo(
                    "第一章　魔大陸、再建の始まり",
                    "両親は輸送事故で意識不明となり、商会は取り潰された。残されたのは一億ゴールドの債務と、治療を待つ二人だけ。\n\n" +
                    "あなたは両親の原点、魔大陸へ渡った。旧取引先の臨時契約者たちは救いの手を差し出さない。だが、仕事と報酬を示せば取引には応じる。\n\n" +
                    "わずかな資金で傭兵を雇い、最初の探索隊を編成しよう。素材を利益に変え、両親と商会を取り戻すために。");
            case StoryMilestone.FirstMercenary:
                return new StoryMilestoneInfo(
                    "最初の契約書",
                    "羊皮紙に署名が刻まれ、商会に最初の傭兵が加わった。空っぽだった詰所に、武具を整える音が響く。\n\n" +
                    "一人では届かなかった場所へ、仲間となら進める。まずはセイル近郊の洞窟で商会の旗を掲げよう。");
            case StoryMilestone.FirstDungeonClear:
                return new StoryMilestoneInfo(
                    "洞窟に残した足跡",
                    "初めてのダンジョン踏破。その報せは港の酒場を巡り、商会の名を町に広めた。\n\n" +
                    "だが借金はまだ山のように残っている。手に入れた装備を整え、次の町へ続く街道に備えよう。");
            case StoryMilestone.LeafUnlocked:
                return new StoryMilestoneInfo(
                    "第二章　森都リーフへの道",
                    "街道の魔物を退け、深い森の向こうにリーフの灯が見えた。新しい市場、新しい鍛冶技術、そして未知の依頼が待っている。\n\n" +
                    "町ごとに扱う品と雇える傭兵は異なる。商会の拠点を広げ、より強い隊を育てよう。");
            case StoryMilestone.RegionGateCleared:
                return new StoryMilestoneInfo(
                    "国境を越える商隊",
                    "最初の地域を越え、地図の空白へ続く門が開いた。気候も魔物も、これまでとはまるで違う。\n\n" +
                    "上位職への道と高位装備を求め、商会は遠征を始める。準備不足の進軍は、傭兵と財産の両方を失うだろう。");
            case StoryMilestone.AbyssReached:
                return new StoryMilestoneInfo(
                    "深淵都市アビス",
                    "黒い大地の果て、深淵都市アビスへ到達した。ここでは傭兵を新たに雇えず、鍛え上げた仲間だけが頼りとなる。\n\n" +
                    "最奥のダンジョンを制した者には、世界の中央に眠る島の噂が明かされるという。");
            case StoryMilestone.HiddenIslandReached:
                return new StoryMilestoneInfo(
                    "終章　星幽島の出現",
                    "集めた固有装備が共鳴し、世界地図の中央に光の道が現れた。霧に隠されていた星幽島が、その姿を見せる。\n\n" +
                    "島にあるのは鍛冶屋と、最後のダンジョンだけ。ランク10の装備を巡る、商会最大の挑戦が始まる。");
            case StoryMilestone.DebtCleared:
                return new StoryMilestoneInfo(
                    "借用証書の燃える夜",
                    "最後の一枚まで返済を終え、借用証書は暖炉の炎に消えた。傭兵たちの歓声が、夜明け前の商会に響く。\n\n" +
                    "商会はもう誰のものでもない。あなたと仲間たちが築いた、自由な傭兵商会だ。");
            default:
                return new StoryMilestoneInfo(string.Empty, string.Empty);
        }
    }

    private void HandleMercenaryHired(MercenaryInstance mercenary)
    {
        TryComplete(StoryMilestone.FirstMercenary);
    }

    private void HandleDungeonCompleted(bool cleared)
    {
        if (cleared &&
            dungeonRunManager != null &&
            dungeonRunManager.IsSelectedDungeonFullyCleared)
        {
            TryComplete(StoryMilestone.FirstDungeonClear);
        }
    }

    private void HandleTownProgressChanged()
    {
        if (townProgressState == null)
        {
            return;
        }

        if (townProgressState.IsTownUnlocked(1))
        {
            TryComplete(StoryMilestone.LeafUnlocked);
        }
        if (WorldMapService.HasUnlockedTownInWorld(
                townProgressState.GetUnlockedTownIndices(), 1))
        {
            TryComplete(StoryMilestone.RegionGateCleared);
        }
        if (townProgressState.IsTownUnlocked(6))
        {
            TryComplete(StoryMilestone.AbyssReached);
        }
        if (townProgressState.IsTownUnlocked(WorldMapService.HiddenIslandTownIndex))
        {
            TryComplete(StoryMilestone.HiddenIslandReached);
        }
    }

    private void HandleDebtChanged()
    {
        if (debtManager != null && debtManager.IsDebtCleared)
        {
            TryComplete(StoryMilestone.DebtCleared);
        }
    }

    private void Subscribe()
    {
        if (hireManager != null)
        {
            hireManager.MercenaryHired -= HandleMercenaryHired;
            hireManager.MercenaryHired += HandleMercenaryHired;
        }
        if (dungeonRunManager != null)
        {
            dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
            dungeonRunManager.DungeonCompleted += HandleDungeonCompleted;
        }
        if (townProgressState != null)
        {
            townProgressState.TownProgressChanged -= HandleTownProgressChanged;
            townProgressState.TownProgressChanged += HandleTownProgressChanged;
        }
        if (debtManager != null)
        {
            debtManager.DebtChanged -= HandleDebtChanged;
            debtManager.DebtChanged += HandleDebtChanged;
        }
    }

    private void Unsubscribe()
    {
        if (hireManager != null) hireManager.MercenaryHired -= HandleMercenaryHired;
        if (dungeonRunManager != null) dungeonRunManager.DungeonCompleted -= HandleDungeonCompleted;
        if (townProgressState != null) townProgressState.TownProgressChanged -= HandleTownProgressChanged;
        if (debtManager != null) debtManager.DebtChanged -= HandleDebtChanged;
    }

    private void ResolveReferences()
    {
        hireManager = hireManager ?? GetComponent<MercenaryHireManager>() ?? FindObjectOfType<MercenaryHireManager>();
        dungeonRunManager = dungeonRunManager ?? GetComponent<DungeonRunManager>() ?? FindObjectOfType<DungeonRunManager>();
        townProgressState = townProgressState ?? GetComponent<TownProgressState>() ?? FindObjectOfType<TownProgressState>();
        debtManager = debtManager ?? GetComponent<DebtManager>() ?? FindObjectOfType<DebtManager>();
    }
}
