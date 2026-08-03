using System;
using System.Collections.Generic;
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
    DailyResult,
    ItemCollection
}

public sealed class SimpleMercenaryHireUIView : MonoBehaviour
{
    [Serializable]
    public sealed class ExpeditionReferences
    {
        public RectTransform overlay;
        public bool IsValid => overlay != null;
    }

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
    /// Groups the road battle page widgets: the route label, the retreat and
    /// continue actions and the playback controls.
    /// </summary>
    [Serializable]
    public sealed class RoadBattleReferences
    {
        public Text routeText;
        public Button continueButton;
        public Button retreatButton;
        public Button speedButton;
        public Button pauseButton;
        public Button skipButton;
        public Button cargoReceiveButton;

        public bool IsValid =>
            routeText != null &&
            continueButton != null &&
            retreatButton != null;
    }

    /// <summary>
    /// Groups the travel confirmation overlay together with the cargo and
    /// companions the player has staged for the trip.
    /// </summary>
    [Serializable]
    public sealed class TravelConfirmationReferences
    {
        public RectTransform overlay;
        public Text text;
        public Text cargoSummaryText;
        public RectTransform cargoContent;

        /// <summary>Cargo staged for the trip, keyed by item.</summary>
        public readonly Dictionary<ItemDataSO, int> selectedCargo =
            new Dictionary<ItemDataSO, int>();

        /// <summary>Ids of the mercenaries staged to travel along.</summary>
        public readonly HashSet<string> selectedCompanions =
            new HashSet<string>();

        public bool IsValid =>
            overlay != null &&
            text != null &&
            cargoContent != null;
    }

    /// <summary>
    /// Groups the promotion preview overlay and the pending promotion it
    /// would apply.
    /// </summary>
    [Serializable]
    public sealed class PromotionPreviewReferences
    {
        public RectTransform overlay;
        public Text text;
        public Text reasonText;
        public Button confirmButton;

        /// <summary>Mercenary being promoted.</summary>
        public MercenaryInstance mercenary;

        /// <summary>Class the promotion would move the mercenary into.</summary>
        public MercenaryClass target;

        public bool IsValid =>
            overlay != null &&
            text != null &&
            confirmButton != null;
    }

    /// <summary>
    /// Groups the release confirmation overlay and the mercenary it targets.
    /// </summary>
    [Serializable]
    public sealed class ReleaseConfirmationReferences
    {
        public RectTransform overlay;
        public Text text;

        /// <summary>Mercenary that would be released on confirmation.</summary>
        public MercenaryInstance mercenary;

        public bool IsValid => overlay != null && text != null;
    }

    /// <summary>
    /// Groups the sell-quantity picker overlay together with the item and
    /// amount it is currently editing.
    /// </summary>
    [Serializable]
    public sealed class SellQuantityReferences
    {
        public RectTransform overlay;
        public Text titleText;
        public Image image;
        public Text imagePlaceholder;
        public Text detailText;

        /// <summary>Item being sold, or null while the picker is closed.</summary>
        public ItemDataSO item;

        /// <summary>Quantity currently dialled in by the player.</summary>
        public int amount;

        public bool IsValid =>
            overlay != null &&
            titleText != null &&
            detailText != null;
    }

    /// <summary>
    /// Groups the confirmation overlay shown before bulk-selling every
    /// sell-only material.
    /// </summary>
    [Serializable]
    public sealed class SellOnlyConfirmationReferences
    {
        public RectTransform overlay;
        public Text text;

        public bool IsValid => overlay != null && text != null;
    }

    /// <summary>
    /// Groups the storage capacity label and the upgrade confirmation overlay.
    /// </summary>
    [Serializable]
    public sealed class StorageUpgradeReferences
    {
        public Text capacityText;
        public RectTransform confirmationOverlay;
        public Text confirmationText;
        public Text confirmationReasonText;
        public Button confirmButton;

        public bool IsValid =>
            confirmationOverlay != null &&
            confirmationText != null &&
            confirmButton != null;
    }

    /// <summary>
    /// Groups the contract change confirmation overlay and the pending
    /// mercenary/contract the player is switching to.
    /// </summary>
    [Serializable]
    public sealed class ContractChangeReferences
    {
        public RectTransform confirmationOverlay;
        public Text confirmationText;
        public Button confirmButton;

        /// <summary>Mercenary whose contract is being changed.</summary>
        public MercenaryInstance mercenary;

        /// <summary>Contract type the confirmation would apply.</summary>
        public MercenaryContractType target;

        public bool IsValid =>
            confirmationOverlay != null &&
            confirmationText != null &&
            confirmButton != null;
    }

    /// <summary>
    /// Groups the contract details overlay widgets.
    /// </summary>
    [Serializable]
    public sealed class ContractDetailsReferences
    {
        public RectTransform overlay;
    }

    /// <summary>
    /// Groups the facility greeting overlay widgets.
    /// </summary>
    [Serializable]
    public sealed class FacilityGreetingReferences
    {
        public RectTransform overlay;
        public Text title;
        public Text dialogue;
        public Image portrait;

        public bool IsValid =>
            overlay != null &&
            title != null &&
            dialogue != null &&
            portrait != null;
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

    /// <summary>Groups the daily result overlay widgets.</summary>
    [Serializable]
    public sealed class DailyResultReferences
    {
        public RectTransform overlay;
        public RectTransform content;
        public Text text;

        public bool IsValid =>
            overlay != null &&
            content != null &&
            text != null;
    }

    /// <summary>
    /// Groups the quest board overlay: the scrolling quest list and the detail
    /// window opened from it.
    /// </summary>
    [Serializable]
    public sealed class QuestReferences
    {
        public RectTransform overlay;
        public RectTransform list;
        public RectTransform detailWindow;

        /// <summary>
        /// The detail window is intentionally excluded: it is created lazily
        /// the first time a quest is opened.
        /// </summary>
        public bool IsValid => overlay != null && list != null;
    }

    /// <summary>
    /// Groups the merchant status overlay and the skill list it renders.
    /// </summary>
    [Serializable]
    public sealed class MerchantStatusReferences
    {
        public RectTransform overlay;
        public RectTransform skillList;

        public bool IsValid => overlay != null && skillList != null;
    }

    /// <summary>
    /// Groups the monster codex overlay and the book page that renders its
    /// entries.
    /// </summary>
    [Serializable]
    public sealed class MonsterCodexReferences
    {
        public RectTransform overlay;
        public BookPageUI book;

        public bool IsValid => overlay != null && book != null;
    }

    [Serializable]
    public sealed class ItemCodexReferences
    {
        public RectTransform overlay;
        public BookPageUI book;

        public bool IsValid => overlay != null && book != null;
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
