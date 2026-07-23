using System;
using UnityEngine;

/// <summary>
/// Owns the tutorial content (step titles/bodies), the step navigation
/// state. Extracted from
/// SimpleMercenaryHireUI (2nd improvement plan A-1). Overlay
/// construction and Show/Hide routing stay in
/// SimpleMercenaryHireUI.Tutorial.cs; only the state, content and
/// step logic live here.
/// </summary>
public sealed class TutorialController
{
    public const string FirstJourneyRoute =
        "町マップ  →  酒場  →  メニューのパーティー編成  →  近隣ダンジョン";

    public static void ResetCompletion()
    {
    }

    private readonly string[] tutorialTitles =
    {
        "商会を再建する理由",
        "最初の探索隊を編成する",
        "結界都市の施設",
        "探索と戦闘",
        "装備と成長",
        "日数と経営"
    };

    private readonly string[] tutorialBodies =
    {
        "両親は事故で意識不明となり、商会は取り潰された。残された債務は一億ゴールド。\n\n" +
        "両親の治療を続け、商会を再建するには利益が要る。金で命の価値は決められない。だが、金がなければ救えない命もある。",

        "商人であるあなたは戦えません。町マップから「酒場」へ行き、傭兵と契約してください。\n\n" +
        "雇った傭兵は、上部メニューの「パーティー編成」で探索隊に加えてください。編成ができたら「近隣ダンジョン」へ向かいます。わずかな資金を、最初の取引と探索に使いましょう。",

        "町は魔物から守られた結界都市です。\n" +
        "酒場：傭兵と契約する　／　市場：品物を売買する\n" +
        "鍛冶屋：装備を整える　／　倉庫：町ごとの品を預ける\n" +
        "治療院：負傷者を治療する\n\n" +
        "商会組合では、輸送部隊と遠征部隊を管理します。",

        "近隣ダンジョンでは、傭兵が魔物を退けて素材と報酬を持ち帰ります。出撃前に編成と装備を確認しましょう。\n\n" +
        "HPは命そのものではなく、外敵から身を守る対外防護値です。HPが尽きた傭兵は戦闘不能になるだけで、仲間が回収できれば生還できます。\n" +
        "対外防護値が止まった者には、治療院で再活性治療が必要です。",

        "手に入れた素材は市場で売るほか、鍛冶屋で装備にできます。装備ランクを上げ、条件を満たした傭兵は転職神殿で転職させましょう。\n\n" +
        "倉庫は町ごとに管理されます。どの町に何を置くかを考え、必要な装備と積荷を整えてください。",

        "行動や日送りで日数が進み、傭兵の契約費と月ごとの返済が発生します。\n\n" +
        "町ごとに需要と相場は異なります。安く仕入れた品や素材を、求められる結界都市へ輸送部隊で運び、利益と信用を積み上げましょう。\n\n" +
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
