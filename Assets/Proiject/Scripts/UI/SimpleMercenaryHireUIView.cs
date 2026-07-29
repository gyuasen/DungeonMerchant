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

    /// <summary>
    /// Groups the battle page widgets: the log panel and its scrolling parts
    /// plus the playback controls.
    /// </summary>
    [Serializable]
    public sealed class BattleViewReferences
    {
        public RectTransform logPanel;
        public RectTransform logContent;
        public RectTransform logViewport;
        public ScrollRect logScrollRect;
        public Text logText;
        public Text pageTitleText;
        public Text encounterText;
        public Button speedButton;
        public Button pauseButton;
        public Button skipButton;

        public bool IsValid =>
            logPanel != null &&
            logText != null &&
            logScrollRect != null;
    }

    /// <summary>
    /// Groups the dungeon page widgets: the selection list, the random-event
    /// panel and the floor-result panel.
    /// </summary>
    [Serializable]
    public sealed class DungeonViewReferences
    {
        public RectTransform selectionList;
        public Text statusText;
        public Text eventTitleText;
        public Text eventDescriptionText;
        public Text eventPreviewText;
        public RectTransform eventPanel;
        public RectTransform resultPanel;
        public Text resultText;
        public Button nextFloorButton;

        public bool IsValid =>
            selectionList != null &&
            statusText != null &&
            eventPanel != null &&
            resultPanel != null;
    }

    /// <summary>
    /// Groups the equipment codex overlay: the shared book page plus the
    /// normal/special tab roots and their tab buttons.
    /// </summary>
    [Serializable]
    public sealed class EquipmentCodexReferences
    {
        public RectTransform overlay;
        public BookPageUI book;
        public EquipmentSpecialCodexPageUI specialPage;
        public RectTransform normalRoot;
        public RectTransform specialRoot;
        public Button normalTabButton;
        public Button specialTabButton;

        public bool IsValid =>
            overlay != null &&
            normalRoot != null &&
            specialRoot != null;
    }

    /// <summary>
    /// Groups the equipment detail overlay widgets and its action buttons.
    /// </summary>
    [Serializable]
    public sealed class EquipmentDetailReferences
    {
        public RectTransform overlay;
        public Text title;
        public Text bodyText;
        public Button enhanceButton;
        public Button sellButton;
        public Button lockButton;

        public bool IsValid =>
            overlay != null &&
            title != null &&
            bodyText != null;
    }

    /// <summary>
    /// Groups the slot-selection overlay shown when choosing which slot an
    /// item should occupy, together with the pending selection state.
    /// </summary>
    [Serializable]
    public sealed class EquipmentSlotSelectionReferences
    {
        public RectTransform overlay;
        public RectTransform content;
        public Text title;

        /// <summary>Equipment slot currently being assigned.</summary>
        public EquipmentSlot selectedSlot;

        /// <summary>
        /// Consumable slot index being assigned, or -1 when the pending
        /// selection targets an equipment slot instead.
        /// </summary>
        public int selectedConsumableSlotIndex = -1;

        public bool IsValid =>
            overlay != null &&
            content != null &&
            title != null;
    }

    /// <summary>
    /// Groups the character detail overlay widgets, covering both the status
    /// tab and the equipment tab plus the tab-selection state.
    /// </summary>
    [Serializable]
    public sealed class CharacterDetailReferences
    {
        public RectTransform overlay;
        public Text title;
        public Text statusText;
        public RectTransform statusPage;
        public RectTransform equipmentPage;
        public RectTransform skillList;
        public Text skillDetailText;
        public Button statusTabButton;
        public Button equipmentTabButton;
        public RectTransform equipmentList;
        public ScrollRect equipmentScrollRect;

        /// <summary>
        /// True while the status tab is showing; false while the equipment tab
        /// is showing. Kept here so the tab state travels with its widgets.
        /// </summary>
        public bool showingStatusPage = true;

        public bool IsValid =>
            overlay != null &&
            title != null &&
            statusText != null &&
            statusPage != null &&
            equipmentPage != null &&
            equipmentList != null;
    }

    /// <summary>
    /// Groups the tutorial overlay widgets so the owning UI keeps a single
    /// field instead of one per widget. Mirrors <see cref="ChromeReferences"/>.
    /// </summary>
    [Serializable]
    public sealed class TutorialReferences
    {
        public RectTransform overlay;
        public Text stepText;
        public Text titleText;
        public Text bodyText;
        public Button backButton;
        public Button nextButton;
        public Button closeButton;

        /// <summary>
        /// The close button is intentionally excluded: it only hides the
        /// overlay and the tutorial can still render without it.
        /// </summary>
        public bool IsValid =>
            stepText != null &&
            titleText != null &&
            bodyText != null &&
            backButton != null &&
            nextButton != null;
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
