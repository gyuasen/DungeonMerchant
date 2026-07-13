using System;
using UnityEngine;

/// <summary>
/// Owns the tutorial content (step titles/bodies), the step navigation
/// state and the PlayerPrefs completion flag. Extracted from
/// SimpleMercenaryHireUI (2nd improvement plan A-1). Overlay
/// construction and Show/Hide routing stay in
/// SimpleMercenaryHireUI.Tutorial.cs; only the state, content and
/// step logic live here.
/// </summary>
public sealed class TutorialController
{
    public const string FirstJourneyRoute =
        "町マップ  →  傭兵斡旋所  →  傭兵一覧／メニューで編成  →  近隣ダンジョン";

    private const string TutorialCompletedPlayerPrefsKey =
        "DungeonMerchant.Tutorial.Completed";

    public static void ResetCompletion()
    {
        PlayerPrefs.DeleteKey(TutorialCompletedPlayerPrefsKey);
        PlayerPrefs.Save();
    }

    private readonly string[] tutorialTitles =
    {
        "商会の目的",
        "最初の目標",
        "1. 町マップを開く",
        "2. 傭兵斡旋所で雇う",
        "3. 傭兵一覧／メニューで編成",
        "4. 近隣ダンジョンへ",
        "施設早見表：商会と準備",
        "施設早見表：装備と回復",
        "日数と借金"
    };

    private readonly string[] tutorialBodies =
    {
        "あなたは傭兵商会を運営する商人です。\n" +
        "傭兵を雇い、パーティーを編成し、ダンジョンで利益を得ながら、" +
        "最終的に1億Gの借金返済を目指します。",

        "最初は、この順に進めれば冒険を始められます。\n\n" +
        "1. 町マップを開く\n" +
        "2. 傭兵斡旋所で傭兵を雇う\n" +
        "3. 傭兵一覧で確認し、メニューからパーティー編成\n" +
        "4. 町マップの近隣ダンジョンへ向かう",

        "画面上部の「町マップ」を押してください。\n" +
        "町マップは、雇用・編成・買い物・探索へ向かうための入口です。\n\n" +
        "まずは「傭兵斡旋所」を選びます。",

        "傭兵斡旋所では、候補者と契約期間を選んで傭兵を雇えます。\n\n" +
        "所持金を確認して、最初のパーティーに必要な傭兵を雇いましょう。" +
        "雇った傭兵は「傭兵一覧」に加わります。",

        "上部の「メニュー」を開き、「傭兵一覧」で雇用を確認します。\n" +
        "次に「パーティー編成」を開き、雇った傭兵を探索メンバーに加えてください。\n\n" +
        "町マップの「編成所」からも同じ編成画面を開けます。",

        "編成できたら町マップへ戻り、「近隣ダンジョン」を選びます。\n" +
        "ダンジョンは戦闘やイベントを進め、報酬と経験値を得る施設です。\n\n" +
        "戦闘前に、出撃メンバーがそろっているか確認しましょう。",

        "傭兵斡旋所：新しい傭兵を雇う\n" +
        "商会本部／傭兵一覧：雇用済み傭兵を確認する\n" +
        "編成所／パーティー編成：探索メンバーを決める\n" +
        "近隣ダンジョン：戦闘・探索で報酬を得る",

        "市場：装備や品物を仕入れる\n" +
        "鍛冶屋：素材から装備を作る\n" +
        "倉庫：所持品を確認する\n" +
        "治療院：負傷した傭兵を治療する\n" +
        "転職神殿：条件を満たした傭兵を転職させる\n" +
        "全体マップ：別の町や地域へ移動する",

        "行動によって日数が進み、30日ごとに最低返済額の支払いが発生します。\n" +
        "所持金、傭兵契約、装備、治療費を確認しながら、長期的に利益を増やしてください。\n\n" +
        "迷ったときはメニューから、このチュートリアルを見返せます。"
    };

    private int tutorialStepIndex;

    private readonly Action<string> setStatus;
    private readonly Action showOverlay;
    private readonly Action hideOverlay;
    private readonly Action<string> setStepText;
    private readonly Action<string> setTitleText;
    private readonly Action<string> setBodyText;
    private readonly Action<bool> setBackInteractable;
    private readonly Action<string> setNextButtonLabel;
    private readonly Func<bool> hasOverlayWidgets;

    public TutorialController(
        Action<string> setStatus,
        Action showOverlay,
        Action hideOverlay,
        Action<string> setStepText,
        Action<string> setTitleText,
        Action<string> setBodyText,
        Action<bool> setBackInteractable,
        Action<string> setNextButtonLabel,
        Func<bool> hasOverlayWidgets)
    {
        this.setStatus = setStatus;
        this.showOverlay = showOverlay;
        this.hideOverlay = hideOverlay;
        this.setStepText = setStepText;
        this.setTitleText = setTitleText;
        this.setBodyText = setBodyText;
        this.setBackInteractable = setBackInteractable;
        this.setNextButtonLabel = setNextButtonLabel;
        this.hasOverlayWidgets = hasOverlayWidgets;
    }

    public void ShowTutorialIfNeeded()
    {
        if (PlayerPrefs.GetInt(TutorialCompletedPlayerPrefsKey, 0) == 0)
        {
            ShowTutorial();
        }
    }

    public void ShowTutorial()
    {
        tutorialStepIndex = 0;
        Refresh();
        showOverlay();
    }

    public void ShowPreviousStep()
    {
        tutorialStepIndex = Mathf.Max(0, tutorialStepIndex - 1);
        Refresh();
    }

    public void ShowNextStep()
    {
        if (tutorialStepIndex >= tutorialTitles.Length - 1)
        {
            PlayerPrefs.SetInt(TutorialCompletedPlayerPrefsKey, 1);
            PlayerPrefs.Save();
            hideOverlay();
            setStatus("チュートリアルを完了しました。メニューからいつでも見返せます。");
            return;
        }

        tutorialStepIndex++;
        Refresh();
    }

    public void Refresh()
    {
        if (!hasOverlayWidgets())
        {
            return;
        }

        tutorialStepIndex =
            Mathf.Clamp(tutorialStepIndex, 0, tutorialTitles.Length - 1);
        setStepText(
            $"{tutorialStepIndex + 1} / {tutorialTitles.Length}");
        setTitleText(tutorialTitles[tutorialStepIndex]);
        setBodyText(tutorialBodies[tutorialStepIndex]);
        setBackInteractable(tutorialStepIndex > 0);
        setNextButtonLabel(
            tutorialStepIndex >= tutorialTitles.Length - 1
                ? "完了"
                : "次へ");
    }
}
