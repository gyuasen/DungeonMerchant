using System;
using UnityEngine;
using UnityEngine.UI;

public enum SimpleMercenaryHirePageSlot
{
    Hire,
    GlobalMap,
    WorldMap,
    TownMap,
    Company,
    Party,
    Heal,
    Battle,
    RoadBattle,
    Dungeon,
    Market,
    Blacksmith,
    Inventory,
    JobChange,
    TrainingGround
}

public enum SimpleMercenaryHireOverlaySlot
{
    CharacterDetail,
    EquipmentDetail,
    EquipmentCollection,
    MonsterCollection,
    MerchantStatus,
    Quest,
    TravelConfirmation,
    GlobalMenu,
    DailyResult
}

public sealed class SimpleMercenaryHireUIView : MonoBehaviour
{
    [Serializable]
    public sealed class ChromeReferences
    {
        public Text titleText;
        public Button mapButton;
        public Button townMapButton;
        public Text dayText;
        public Button merchantStatusButton;
        public Text goldText;
        public Button globalMenuButton;
        public Text statusText;
        public RectTransform onboardingBanner;
        public Text onboardingObjectiveText;
        public Button onboardingSkipButton;

        public bool IsValid =>
            titleText != null &&
            mapButton != null &&
            townMapButton != null &&
            dayText != null &&
            merchantStatusButton != null &&
            goldText != null &&
            globalMenuButton != null &&
            statusText != null;
    }

    [Serializable]
    public sealed class HireCompanyReferences
    {
        public HirePageUI hirePageUI;
        public Text hireTitle;
        public Button contractButton;
        public ScrollRect hireScrollRect;
        public RectTransform hireList;
        public CompanyPageUI companyPageUI;
        public Text companyTitle;
        public Button questButton;
        public ScrollRect companyScrollRect;
        public RectTransform companyList;

        public bool IsValid =>
            hireTitle != null &&
            contractButton != null &&
            hireScrollRect != null &&
            hireList != null &&
            companyTitle != null &&
            questButton != null &&
            companyScrollRect != null &&
            companyList != null;

        public HirePageUI GetOrCreateHirePageUI()
        {
            if (hirePageUI == null && hireTitle != null)
            {
                hirePageUI =
                    hireTitle.GetComponentInParent<HirePageUI>() ??
                    hireTitle.transform.parent.gameObject
                        .AddComponent<HirePageUI>();
                hirePageUI.Initialize(
                    hireTitle,
                    contractButton,
                    hireScrollRect,
                    hireList);
            }
            return hirePageUI;
        }

        public CompanyPageUI GetOrCreateCompanyPageUI()
        {
            if (companyPageUI == null && companyTitle != null)
            {
                companyPageUI =
                    companyTitle.GetComponentInParent<CompanyPageUI>() ??
                    companyTitle.transform.parent.gameObject
                        .AddComponent<CompanyPageUI>();
                companyPageUI.Initialize(
                    companyTitle,
                    questButton,
                    companyScrollRect,
                    companyList);
            }
            return companyPageUI;
        }
    }

    public const int CurrentLayoutVersion = 18;

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform guildPanel;
    [SerializeField] private int layoutVersion;
    [SerializeField] private RectTransform[] pages =
        Array.Empty<RectTransform>();
    [SerializeField] private ChromeReferences chrome;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private RectTransform[] overlays =
        Array.Empty<RectTransform>();
    [SerializeField] private HireCompanyReferences hireCompany;

    public Canvas Canvas => canvas;
    public RectTransform GuildPanel => guildPanel;
    public int LayoutVersion => layoutVersion;
    public ChromeReferences Chrome => chrome;
    public bool HasChromeLayout => chrome != null && chrome.IsValid;
    public RectTransform OverlayRoot =>
        overlayRoot != null ? overlayRoot : guildPanel;
    public bool HasOverlayLayout =>
        overlays != null &&
        overlays.Length ==
        Enum.GetValues(typeof(SimpleMercenaryHireOverlaySlot)).Length;
    public HireCompanyReferences HireCompany => hireCompany;
    public bool HasHireCompanyLayout =>
        hireCompany != null && hireCompany.IsValid;
    public bool HasPageLayout =>
        pages != null &&
        pages.Length == Enum.GetValues(typeof(SimpleMercenaryHirePageSlot)).Length;

    public void Initialize(
        Canvas targetCanvas,
        RectTransform targetGuildPanel,
        RectTransform[] targetPages = null,
        ChromeReferences targetChrome = null,
        RectTransform targetOverlayRoot = null,
        RectTransform[] targetOverlays = null,
        HireCompanyReferences targetHireCompany = null)
    {
        canvas = targetCanvas;
        guildPanel = targetGuildPanel;
        pages = targetPages ?? Array.Empty<RectTransform>();
        chrome = targetChrome;
        overlayRoot = targetOverlayRoot;
        overlays = targetOverlays ?? Array.Empty<RectTransform>();
        hireCompany = targetHireCompany;
        layoutVersion = targetHireCompany != null
            ? CurrentLayoutVersion
            : targetOverlays != null
            ? 5
            : targetOverlayRoot != null
            ? 4
            : targetChrome != null
            ? 3
            : targetPages != null
            ? 2
            : 0;
    }

    public RectTransform GetPage(SimpleMercenaryHirePageSlot slot)
    {
        int index = (int)slot;
        return HasPageLayout && pages[index] != null
            ? pages[index]
            : null;
    }

    public RectTransform GetOverlay(
        SimpleMercenaryHireOverlaySlot slot)
    {
        int index = (int)slot;
        return HasOverlayLayout && overlays[index] != null
            ? overlays[index]
            : null;
    }
}
