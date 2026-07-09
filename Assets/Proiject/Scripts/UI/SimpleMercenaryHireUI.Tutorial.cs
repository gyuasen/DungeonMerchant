using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private const string TutorialCompletedPlayerPrefsKey =
        "DungeonMerchant.Tutorial.Completed";

    private readonly string[] tutorialTitles =
    {
        "商会の目的",
        "最初にやること",
        "町と施設",
        "探索と戦闘",
        "装備と成長",
        "日数と借金"
    };

    private readonly string[] tutorialBodies =
    {
        "あなたは傭兵商会を運営する商人です。\n" +
        "傭兵を雇い、編成し、町やダンジョンで利益を出しながら、" +
        "最終的に1億Gの借金返済を目指します。",

        "まずは町マップから雇用施設を開き、傭兵を雇ってください。\n" +
        "雇った傭兵は商会画面で確認でき、編成画面でパーティーに入れると探索や戦闘へ出せます。",

        "全体マップでは地域を選び、町マップでは施設を選びます。\n" +
        "町によって使える施設や商品が変わります。新しい町へ移動するには、街道戦闘を突破する必要があります。",

        "ダンジョン探索では、複数回の戦闘やイベントを越えてフロア攻略を進めます。\n" +
        "戦闘ログはスクロールで確認できます。味方は青、敵は赤、報酬は緑で表示されます。",

        "傭兵は経験値で成長し、装備で能力を伸ばせます。\n" +
        "装備は市場、ダンジョン、鍛冶屋から入手できます。詳細画面では装備比較や変更もできます。",

        "行動によって日数が進み、30日ごとに最低返済額の支払いが発生します。\n" +
        "所持金、商人レベル、倉庫、傭兵契約、治療費を見ながら、長期的に利益を増やしてください。"
    };

    private int tutorialStepIndex;

    private void BuildTutorialOverlay()
    {
        tutorialOverlay = CreateUIObject("Tutorial Overlay", overlayRoot);
        tutorialOverlay.anchorMin = Vector2.zero;
        tutorialOverlay.anchorMax = Vector2.one;
        tutorialOverlay.offsetMin = Vector2.zero;
        tutorialOverlay.offsetMax = Vector2.zero;
        tutorialOverlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.82f);

        RectTransform window =
            CreateUIObject("Tutorial Window", tutorialOverlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(720f, 500f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        tutorialStepText = CreateText(
            window,
            string.Empty,
            15,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(34f, -58f),
            new Vector2(-34f, -24f),
            ParchmentMutedColor);

        tutorialTitleText = CreateText(
            window,
            string.Empty,
            28,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(34f, -108f),
            new Vector2(-34f, -62f),
            ParchmentTextColor);

        tutorialBodyText = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(34f, -330f),
            new Vector2(-34f, -122f),
            ParchmentTextColor);
        tutorialBodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        tutorialBodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        tutorialBodyText.rectTransform.offsetMin = new Vector2(34f, 118f);
        tutorialBodyText.rectTransform.offsetMax = new Vector2(-34f, -122f);

        tutorialBackButton =
            CreateActionButton(window, "戻る", ShowPreviousTutorialStep);
        RectTransform backRect =
            tutorialBackButton.GetComponent<RectTransform>();
        backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0f);
        backRect.pivot = new Vector2(0f, 0f);
        backRect.sizeDelta = new Vector2(130f, 46f);
        backRect.anchoredPosition = new Vector2(34f, 28f);

        tutorialCloseButton =
            CreateActionButton(window, "閉じる", HideTutorialOverlay);
        RectTransform closeRect =
            tutorialCloseButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(130f, 46f);
        closeRect.anchoredPosition = new Vector2(0f, 28f);

        tutorialNextButton =
            CreateActionButton(window, "次へ", ShowNextTutorialStep);
        RectTransform nextRect =
            tutorialNextButton.GetComponent<RectTransform>();
        nextRect.anchorMin = nextRect.anchorMax = new Vector2(1f, 0f);
        nextRect.pivot = new Vector2(1f, 0f);
        nextRect.sizeDelta = new Vector2(150f, 46f);
        nextRect.anchoredPosition = new Vector2(-34f, 28f);

        tutorialOverlay.gameObject.SetActive(false);
        RefreshTutorialOverlay();
    }

    private void ShowTutorialIfNeeded()
    {
        if (PlayerPrefs.GetInt(TutorialCompletedPlayerPrefsKey, 0) == 0)
        {
            ShowTutorialOverlay();
        }
    }

    private void ShowTutorialOverlay()
    {
        tutorialStepIndex = 0;
        RefreshTutorialOverlay();
        tutorialOverlay.SetAsLastSibling();
        tutorialOverlay.gameObject.SetActive(true);
    }

    private void HideTutorialOverlay()
    {
        tutorialOverlay?.gameObject.SetActive(false);
    }

    private void ShowPreviousTutorialStep()
    {
        tutorialStepIndex = Mathf.Max(0, tutorialStepIndex - 1);
        RefreshTutorialOverlay();
    }

    private void ShowNextTutorialStep()
    {
        if (tutorialStepIndex >= tutorialTitles.Length - 1)
        {
            PlayerPrefs.SetInt(TutorialCompletedPlayerPrefsKey, 1);
            PlayerPrefs.Save();
            HideTutorialOverlay();
            statusText.text = "チュートリアルを完了しました。メニューからいつでも見返せます。";
            return;
        }

        tutorialStepIndex++;
        RefreshTutorialOverlay();
    }

    private void RefreshTutorialOverlay()
    {
        if (tutorialTitleText == null ||
            tutorialBodyText == null ||
            tutorialStepText == null ||
            tutorialBackButton == null ||
            tutorialNextButton == null)
        {
            return;
        }

        tutorialStepIndex =
            Mathf.Clamp(tutorialStepIndex, 0, tutorialTitles.Length - 1);
        tutorialStepText.text =
            $"{tutorialStepIndex + 1} / {tutorialTitles.Length}";
        tutorialTitleText.text = tutorialTitles[tutorialStepIndex];
        tutorialBodyText.text = tutorialBodies[tutorialStepIndex];
        tutorialBackButton.interactable = tutorialStepIndex > 0;
        SetButtonLabel(
            tutorialNextButton,
            tutorialStepIndex >= tutorialTitles.Length - 1
                ? "完了"
                : "次へ");
    }
}
