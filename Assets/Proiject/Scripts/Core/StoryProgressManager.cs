using System;
using System.Collections.Generic;
using UnityEngine;

// Numeric values 0-5 and 7 deliberately retain the values used by the
// previous save format. Value 6 (the removed HiddenIsland milestone) is left
// undefined so old, obsolete entries are ignored during restore.
public enum StoryMilestone
{
    OpeningDebtNotice = 0,
    DebtRepaid10 = 1,
    DebtRepaid25 = 2,
    DebtRepaid50 = 3,
    DebtRepaid75 = 4,
    DebtRepaid90 = 5,
    DebtCleared = 7
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

public readonly struct StoryPresentation
{
    public StoryPresentation(
        string title,
        string body,
        StoryMilestone? milestone,
        Action onClosed,
        bool isEnding = false,
        bool isOnboarding = false)
    {
        Title = title;
        Body = body;
        Milestone = milestone;
        OnClosed = onClosed;
        IsEnding = isEnding;
        IsOnboarding = isOnboarding;
    }

    public string Title { get; }
    public string Body { get; }
    public StoryMilestone? Milestone { get; }
    public Action OnClosed { get; }
    public bool IsEnding { get; }
    public bool IsOnboarding { get; }
}

public sealed class StoryProgressManager : MonoBehaviour
{
    private static readonly StoryMilestone[] repaymentMilestones =
    {
        StoryMilestone.DebtRepaid10,
        StoryMilestone.DebtRepaid25,
        StoryMilestone.DebtRepaid50,
        StoryMilestone.DebtRepaid75,
        StoryMilestone.DebtRepaid90,
        StoryMilestone.DebtCleared
    };

    private static readonly int[] repaymentPercentages = { 10, 25, 50, 75, 90, 100 };

    [SerializeField] private DebtManager debtManager;
    [SerializeField] private MerchantData merchantData;

    private readonly HashSet<StoryMilestone> completedMilestones = new HashSet<StoryMilestone>();
    private readonly Queue<StoryPresentation> pendingPresentations = new Queue<StoryPresentation>();
    private bool isRestoring;

    public event Action<StoryMilestone> MilestoneCompleted;
    public event Action PresentationQueued;

    public IReadOnlyCollection<StoryMilestone> CompletedMilestones => completedMilestones;
    public bool IsRestoring => isRestoring;

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable() => Unsubscribe();

    // 明示的な依存注入シーム。テストや Bootstrap から確実に debtManager と
    // merchantData を結びつけ、ライフサイクル順序や fake-null に依存せず購読を
    // 成立させる。merchantData は所持金マイナス中の物語進行停止に使う。
    public void Initialize(DebtManager targetDebtManager, MerchantData targetMerchantData)
    {
        Unsubscribe();
        debtManager = targetDebtManager;
        merchantData = targetMerchantData;
        Subscribe();
    }

    public void BeginNewGame()
    {
        if (!isRestoring) TryComplete(StoryMilestone.OpeningDebtNotice);
    }

    public void BeginRestore() => isRestoring = true;

    public void RestoreCompletedMilestones(IEnumerable<StoryMilestone> restoredMilestones)
    {
        isRestoring = true;
        completedMilestones.Clear();
        pendingPresentations.Clear();
        if (restoredMilestones != null)
        {
            foreach (StoryMilestone milestone in restoredMilestones)
            {
                if (Enum.IsDefined(typeof(StoryMilestone), milestone))
                    completedMilestones.Add(milestone);
            }
        }

        // A pre-change save may have story flags caused by unrelated actions.
        // Derive the authoritative debt milestones without queuing old cards.
        AddAchievedDebtMilestonesWithoutPresentation();
        isRestoring = false;
    }

    public void EndRestore() => isRestoring = false;

    public bool IsCompleted(StoryMilestone milestone) => completedMilestones.Contains(milestone);

    public bool TryComplete(StoryMilestone milestone)
    {
        if (isRestoring || !completedMilestones.Add(milestone)) return false;

        StoryMilestoneInfo info = GetMilestoneInfo(milestone);
        pendingPresentations.Enqueue(new StoryPresentation(
            info.Title,
            info.Body,
            milestone,
            null,
            milestone == StoryMilestone.DebtCleared));
        MilestoneCompleted?.Invoke(milestone);
        PresentationQueued?.Invoke();
        return true;
    }

    public void EnqueueOnboardingPresentation(OnboardingGuideCard card, Action onClosed)
    {
        GetOnboardingCardText(card, out string title, out string body);
        pendingPresentations.Enqueue(new StoryPresentation(
            title, body, null, onClosed, isOnboarding: true));
        PresentationQueued?.Invoke();
    }

    public bool TryDequeuePresentation(out StoryPresentation presentation)
    {
        if (pendingPresentations.Count == 0)
        {
            presentation = default;
            return false;
        }

        presentation = pendingPresentations.Dequeue();
        return true;
    }

    public StoryMilestoneInfo GetMilestoneInfo(StoryMilestone milestone)
    {
        switch (milestone)
        {
            case StoryMilestone.OpeningDebtNotice:
                return new StoryMilestoneInfo(
                    "第一章　背負った五千万",
                    "両親は事故で意識を失い、築き上げた商会は消えた。残されたのは五千万Gの負債と、両親の治療費の請求だけ。私は剣を持たない。だが商人の頭ひとつで、この借金を返し、両親を救ってみせる。魔大陸へ渡ろう。");
            case StoryMilestone.DebtRepaid10:
                return new StoryMilestoneInfo(
                    "動き出した歯車",
                    "借金の一割を返した。傭兵が持ち帰った素材が、確かな金に変わっていく。両親が遺した信用のおかげで、臨時契約者たちも取引に応じてくれる。まだ道は遠いが、商会はもう一度動き始めた。");
            case StoryMilestone.DebtRepaid25:
                return new StoryMilestoneInfo(
                    "取り戻す商会",
                    "四分の一を返済。両親の商会が遺した取引先や販路の一部を、ようやく取り戻すことができた。失われた物流網が、少しずつ私の手で繋がり直していく。");
            case StoryMilestone.DebtRepaid50:
                return new StoryMilestoneInfo(
                    "軌道に乗った商会",
                    "借金の半分を返し終えた。商会は軌道に乗ってきた——あと半分。専属で契約してくれる傭兵も増え、多くの素材を安定して取引できるようになった。");
            case StoryMilestone.DebtRepaid75:
                return new StoryMilestoneInfo(
                    "見えてきた再建",
                    "四分の三を返済。物流網は魔大陸の奥へと伸び、高純度の素材や希少な品も商会を通るようになった。両親の治療も続けられている。かつて失われた商会の姿が、輪郭を取り戻しつつある。");
            case StoryMilestone.DebtRepaid90:
                return new StoryMilestoneInfo(
                    "安定した商い",
                    "残る借金は一割。魔大陸での取引は、かなり安定してきたと言っていい。ここまで来れば焦る必要はない。あともうひと踏ん張りだ。");
            case StoryMilestone.DebtCleared:
                return new StoryMilestoneInfo(
                    "エピローグ　商会、再興",
                    "親愛なる我が子へ\n\nこの手紙が読まれているということは、お前があの莫大な借金を返し終えたということだね。\n\n目を覚ました時、医師からすべてを聞いた。商会は潰れ、お前に五千万もの負債が残されたと。剣も握れぬお前が、たった一人で魔大陸へ渡ったと聞いて、私たちがどれほど胸を痛めたか——けれど、それ以上に誇らしかった。\n\nお前は、私たちが最も大切にしてきたものを受け継いでいた。人、物、金、時間、そして信用。そのすべてに責任を持って向き合う、商人の心だ。\n\n商会はもう、私たちのものではない。お前が自らの手で築き直した、お前の商会だ。\n\nよく頑張った。おかえり。\n\n——父と母より");
            default:
                return new StoryMilestoneInfo(string.Empty, string.Empty);
        }
    }

    private void HandleDebtChanged()
    {
        if (debtManager == null) return;

        // 所持金がマイナス（借金で返済を賄っている状態）の間は物語を進めない。
        // プラスへ復帰したとき GoldChanged 経由で再評価され、到達済みの節目を拾う。
        if (merchantData != null && merchantData.HasNegativeGold) return;

        for (int index = 0; index < repaymentMilestones.Length; index++)
        {
            if (HasRepaidAtLeast(debtManager.RemainingDebt, repaymentPercentages[index]))
                TryComplete(repaymentMilestones[index]);
        }
    }

    private void AddAchievedDebtMilestonesWithoutPresentation()
    {
        if (debtManager == null) return;

        for (int index = 0; index < repaymentMilestones.Length; index++)
        {
            if (HasRepaidAtLeast(debtManager.RemainingDebt, repaymentPercentages[index]))
                completedMilestones.Add(repaymentMilestones[index]);
        }
    }

    private static bool HasRepaidAtLeast(int remainingDebt, int percentage)
    {
        int clampedRemainingDebt = Mathf.Clamp(remainingDebt, 0, DebtManager.InitialDebt);
        long repaidDebt = DebtManager.InitialDebt - (long)clampedRemainingDebt;
        return repaidDebt * 100L >= (long)DebtManager.InitialDebt * percentage;
    }

    // 所持金変化でも物語の到達判定を再評価する。マイナスからプラスへ復帰した
    // 瞬間に、到達済みだが保留していた節目を拾うため。
    private void HandleGoldChanged(int currentGold) => HandleDebtChanged();

    private void Subscribe()
    {
        if (debtManager != null)
        {
            debtManager.DebtChanged -= HandleDebtChanged;
            debtManager.DebtChanged += HandleDebtChanged;
        }

        if (merchantData != null)
        {
            merchantData.GoldChanged -= HandleGoldChanged;
            merchantData.GoldChanged += HandleGoldChanged;
        }
    }

    private void Unsubscribe()
    {
        if (debtManager != null) debtManager.DebtChanged -= HandleDebtChanged;
        if (merchantData != null) merchantData.GoldChanged -= HandleGoldChanged;
    }

    private void ResolveReferences()
    {
        // Unity の未設定 SerializeField は C# 参照としては "fake null" になり得るため、
        // ?? ではなく Unity の == null 判定で解決する。?? だと未解決のまま購読が成立しない。
        if (debtManager == null)
        {
            debtManager = GetComponent<DebtManager>();
        }

        if (debtManager == null)
        {
            debtManager = FindObjectOfType<DebtManager>();
        }

        if (merchantData == null)
        {
            merchantData = GetComponent<MerchantData>();
        }

        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }
    }

    private static void GetOnboardingCardText(OnboardingGuideCard card, out string title, out string body)
    {
        switch (card)
        {
            case OnboardingGuideCard.Warehouse:
                title = "戦利品を利益に変える";
                body = "探索で得た素材や装備は倉庫に保管されます。アイテムを選ぶと、その日の価格で売却できます。\n\n売値は日によって変動し、町ごとの需要によっても異なります。急いで売るか、より高い日や町を待つかを選びましょう。";
                return;
            case OnboardingGuideCard.Market:
                title = "市場で仕入れる";
                body = "市場では、その町で流通する品を仕入れられます。価格と品揃えは日や町によって変わります。安く仕入れ、需要の高い町へ運べば交易利益を得られます。";
                return;
            default:
                title = "素材を装備に変える";
                body = "鍛冶屋では、素材と代金を使って装備を作れます。探索が難しくなったら、傭兵の装備を整えてから再挑戦しましょう。\n\nこれで最初の案内は完了です。探索と交易で利益を積み上げ、月10,000Gの返済に備えてください。";
                return;
        }
    }
}
