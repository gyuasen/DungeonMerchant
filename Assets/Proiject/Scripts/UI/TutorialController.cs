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
        "町マップ  →  酒場  →  メニューのパーティー編成  →  近隣ダンジョン  →  素材を売却  →  返済";

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
        "両親は事故で意識を失い、築き上げた商会は消えた。残されたのは五千万Gの負債と、両親の治療費だけです。\n\n" +
        "剣を持たないあなたは、商人として魔大陸へ渡りました。傭兵を雇い、物流網を築き直し、利益で返済と治療を続けましょう。",

        "商人であるあなたは、自ら剣を振るえません。町マップから「酒場」へ行き、傭兵と契約してください。\n\n" +
        "雇った傭兵は、上部メニューの「パーティー編成」で探索隊に加えます。準備ができたら「近隣ダンジョン」へ向かい、最初の素材を持ち帰りましょう。",

        "町は魔物から守られた結界都市です。\n" +
        "酒場：傭兵と契約する　／　市場：品物を売買する\n" +
        "鍛冶屋：装備を整える　／　倉庫：町ごとの品を預ける\n" +
        "治療院：負傷者を治療する　／　転職神殿：傭兵の職を変える\n\n" +
        "商会組合では輸送部隊と別動隊を管理します。都市ごとの相場と需要を見て、物流網を広げてください。",

        "ダンジョンでは、傭兵が魔物を退けて素材と報酬を持ち帰ります。出撃前に探索隊の編成と装備を確認しましょう。\n\n" +
        "持ち帰った素材は倉庫で確認し、市場で売却すれば返済の原資になります。需要の高い都市まで輸送すれば、さらに大きな利益を狙えます。",

        "手に入れた素材は市場で売るほか、鍛冶屋で装備にできます。探索が難しくなったら、傭兵の装備を整えてから再挑戦しましょう。\n\n" +
        "倉庫は町ごとに管理されます。どの町に何を置くかを考え、必要な装備と積荷を整え、輸送部隊で利益の出る都市へ運んでください。",

        "行動や日送りで日数が進み、傭兵の契約費と月ごとの返済が発生します。五千万Gの借金を返しながら、両親の治療を続けましょう。\n\n" +
        "町ごとに需要と相場は異なります。安く仕入れた品や素材を、求められる結界都市へ輸送部隊で運び、利益と信用を積み上げてください。\n\n" +
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
        ShowTutorial();
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
