using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleMercenaryHireUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private MercenaryGenerator mercenaryGenerator;
    [SerializeField] private BattleManager battleManager;

    [Header("Hire Candidates")]
    [SerializeField] private List<MercenaryDataSO> candidates = new List<MercenaryDataSO>();

    private readonly List<Button> hireButtons = new List<Button>();
    private readonly List<MercenaryDataSO> displayedCandidates = new List<MercenaryDataSO>();
    private readonly List<Button> generatedHireButtons = new List<Button>();
    private readonly List<MercenaryInstance> displayedGeneratedCandidates =
        new List<MercenaryInstance>();
    private readonly HashSet<MercenaryDataSO> hiredCandidates = new HashSet<MercenaryDataSO>();

    private RectTransform hirePage;
    private RectTransform hireList;
    private RectTransform companyPage;
    private RectTransform partyPage;
    private RectTransform battlePage;
    private RectTransform companyList;
    private RectTransform partyList;
    private Button hireTabButton;
    private Button companyTabButton;
    private Button partyTabButton;
    private Button battleTabButton;
    private Button startBattleButton;
    private Text goldText;
    private Text statusText;
    private Text battleLogText;
    private Font uiFont;

    private static readonly Color BackgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
    private static readonly Color PanelColor = new Color(0.13f, 0.15f, 0.18f, 1f);
    private static readonly Color RowColor = new Color(0.19f, 0.21f, 0.24f, 1f);
    private static readonly Color AccentColor = new Color(0.2f, 0.65f, 0.48f, 1f);
    private static readonly Color InactiveColor = new Color(0.25f, 0.28f, 0.32f, 1f);
    private static readonly Color MutedTextColor = new Color(0.7f, 0.74f, 0.78f, 1f);

    private void Start()
    {
        ResolveReferences();

        if (!HasRequiredReferences())
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CacheAlreadyHiredCandidates();
        EnsureEventSystem();
        BuildUI();
        hireManager.MercenaryHired += HandleMercenaryHired;
        partyManager.PartyChanged += HandlePartyChanged;
        mercenaryGenerator.CandidatesChanged += HandleCandidatesChanged;
        battleManager.BattleMessage += HandleBattleMessage;
        battleManager.BattleCompleted += HandleBattleCompleted;
        ShowHirePage();
        RefreshUI();
    }

    private void ResolveReferences()
    {
        if (hireManager == null)
        {
            hireManager = GetComponent<MercenaryHireManager>();
        }

        if (partyManager == null)
        {
            partyManager = GetComponent<MercenaryPartyManager>();
        }

        if (mercenaryGenerator == null)
        {
            mercenaryGenerator = GetComponent<MercenaryGenerator>();
        }

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

        if (merchantData == null)
        {
            merchantData = FindObjectOfType<MerchantData>();
        }
    }

    private bool HasRequiredReferences()
    {
        bool hasAllReferences = true;

        if (merchantData == null)
        {
            Debug.LogError("Simple hire UI is missing MerchantData.", this);
            hasAllReferences = false;
        }

        if (hireManager == null)
        {
            Debug.LogError("Simple hire UI is missing MercenaryHireManager.", this);
            hasAllReferences = false;
        }

        if (partyManager == null)
        {
            Debug.LogError("Simple hire UI is missing MercenaryPartyManager.", this);
            hasAllReferences = false;
        }

        if (mercenaryGenerator == null)
        {
            Debug.LogError("Simple hire UI is missing MercenaryGenerator.", this);
            hasAllReferences = false;
        }

        if (battleManager == null)
        {
            Debug.LogError("Simple hire UI is missing BattleManager.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private void OnDestroy()
    {
        if (hireManager != null)
        {
            hireManager.MercenaryHired -= HandleMercenaryHired;
        }

        if (partyManager != null)
        {
            partyManager.PartyChanged -= HandlePartyChanged;
        }

        if (mercenaryGenerator != null)
        {
            mercenaryGenerator.CandidatesChanged -= HandleCandidatesChanged;
        }

        if (battleManager != null)
        {
            battleManager.BattleMessage -= HandleBattleMessage;
            battleManager.BattleCompleted -= HandleBattleCompleted;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = CreateCanvas();
        RectTransform panel = CreatePanel(canvas.transform);

        CreateText(panel, "MERCENARY GUILD", 28, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), Color.white);

        goldText = CreateText(panel, string.Empty, 20, FontStyle.Bold, TextAnchor.MiddleRight,
            new Vector2(28f, -62f), new Vector2(-28f, -18f), AccentColor);

        hireTabButton = CreateNavigationButton(panel, "HIRE", new Vector2(28f, -78f), ShowHirePage);
        companyTabButton = CreateNavigationButton(
            panel,
            "COMPANY",
            new Vector2(174f, -78f),
            ShowCompanyPage);
        partyTabButton = CreateNavigationButton(
            panel,
            "PARTY",
            new Vector2(320f, -78f),
            ShowPartyPage);
        battleTabButton = CreateNavigationButton(
            panel,
            "BATTLE",
            new Vector2(466f, -78f),
            ShowBattlePage);

        hirePage = CreatePage("Hire Page", panel);
        companyPage = CreatePage("Company Page", panel);
        partyPage = CreatePage("Party Page", panel);
        battlePage = CreatePage("Battle Page", panel);

        BuildHirePage();
        BuildCompanyPage();
        BuildPartyPage();
        BuildBattlePage();

        statusText = CreateText(panel, "Select a mercenary to hire.", 15, FontStyle.Normal,
            TextAnchor.MiddleLeft, new Vector2(28f, 22f), new Vector2(-28f, 54f), MutedTextColor);
        statusText.rectTransform.anchorMin = new Vector2(0f, 0f);
        statusText.rectTransform.anchorMax = new Vector2(1f, 0f);
        statusText.rectTransform.pivot = new Vector2(0.5f, 0f);
    }

    private void BuildHirePage()
    {
        CreateText(hirePage, "Available contracts", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        RectTransform viewport = CreateUIObject("Hire Viewport", hirePage);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(0f, -44f);

        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        hireList = CreateUIObject("Hire List", viewport);
        hireList.anchorMin = new Vector2(0f, 1f);
        hireList.anchorMax = new Vector2(1f, 1f);
        hireList.pivot = new Vector2(0.5f, 1f);
        hireList.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = hireList;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        RebuildHireList();
    }

    private void RebuildHireList()
    {
        ClearChildren(hireList);
        hireButtons.Clear();
        displayedCandidates.Clear();
        generatedHireButtons.Clear();
        displayedGeneratedCandidates.Clear();

        float rowTop = 0f;
        foreach (MercenaryDataSO candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            CreateCandidateRow(hireList, candidate, rowTop);
            rowTop -= 112f;
        }

        foreach (MercenaryInstance candidate in mercenaryGenerator.Candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            CreateGeneratedCandidateRow(hireList, candidate, rowTop);
            rowTop -= 112f;
        }

        hireList.sizeDelta = new Vector2(0f, Mathf.Max(430f, -rowTop));
    }

    private void BuildCompanyPage()
    {
        CreateText(companyPage, "Hired mercenaries", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        companyList = CreateUIObject("Company List", companyPage);
        companyList.anchorMin = new Vector2(0f, 0f);
        companyList.anchorMax = new Vector2(1f, 1f);
        companyList.offsetMin = Vector2.zero;
        companyList.offsetMax = new Vector2(0f, -44f);
    }

    private void BuildPartyPage()
    {
        CreateText(partyPage, "Exploration party", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        partyList = CreateUIObject("Party List", partyPage);
        partyList.anchorMin = new Vector2(0f, 0f);
        partyList.anchorMax = new Vector2(1f, 1f);
        partyList.offsetMin = Vector2.zero;
        partyList.offsetMax = new Vector2(0f, -44f);
    }

    private void BuildBattlePage()
    {
        EnemyDataSO enemy = battleManager.EnemyData;
        string enemyDescription = enemy == null
            ? "No enemy assigned"
            : $"{enemy.enemyName}  |  HP {enemy.maxHP}  ATK {enemy.attack}  " +
              $"DEF {enemy.defense}  |  Reward {enemy.goldReward} G";

        CreateText(battlePage, "Battle preparation", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, -30f), new Vector2(0f, 0f), MutedTextColor);

        CreateText(battlePage, enemyDescription, 18, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, -78f), new Vector2(-160f, -42f), Color.white);

        startBattleButton = CreateActionButton(
            battlePage,
            "START",
            StartPartyBattle);
        RectTransform startRect = startBattleButton.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(1f, 1f);
        startRect.anchorMax = new Vector2(1f, 1f);
        startRect.pivot = new Vector2(1f, 1f);
        startRect.anchoredPosition = new Vector2(0f, -36f);

        RectTransform logPanel = CreateUIObject("Battle Log", battlePage);
        logPanel.anchorMin = new Vector2(0f, 0f);
        logPanel.anchorMax = new Vector2(1f, 1f);
        logPanel.offsetMin = Vector2.zero;
        logPanel.offsetMax = new Vector2(0f, -104f);

        Image logBackground = logPanel.gameObject.AddComponent<Image>();
        logBackground.color = RowColor;

        battleLogText = CreateText(logPanel, "Ready for battle.", 14, FontStyle.Normal,
            TextAnchor.UpperLeft, new Vector2(16f, 16f), new Vector2(-16f, -16f),
            MutedTextColor);
        battleLogText.rectTransform.anchorMin = Vector2.zero;
        battleLogText.rectTransform.anchorMax = Vector2.one;
        battleLogText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        battleLogText.rectTransform.offsetMin = new Vector2(16f, 16f);
        battleLogText.rectTransform.offsetMax = new Vector2(-16f, -16f);
    }

    private void RebuildCompanyList()
    {
        ClearChildren(companyList);

        if (hireManager.HiredMercenaries.Count == 0)
        {
            CreateText(companyList, "No mercenaries hired yet.", 18, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -180f), new Vector2(0f, -80f),
                MutedTextColor);
            return;
        }

        float rowTop = 0f;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            CreateCompanyRow(companyList, mercenary, rowTop);
            rowTop -= 112f;
        }
    }

    private void RebuildPartyList()
    {
        ClearChildren(partyList);

        float rowTop = 0f;
        for (int slotIndex = 0; slotIndex < partyManager.MaxPartySize; slotIndex++)
        {
            if (slotIndex < partyManager.Members.Count)
            {
                CreatePartyRow(partyList, partyManager.Members[slotIndex], slotIndex, rowTop);
            }
            else
            {
                CreateEmptyPartyRow(partyList, slotIndex, rowTop);
            }

            rowTop -= 112f;
        }
    }

    private void CreateCandidateRow(RectTransform parent, MercenaryDataSO candidate, float top)
    {
        RectTransform row = CreateRow(candidate.mercenaryName, parent, top);

        CreateText(row, candidate.mercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"{candidate.mercenaryClass}  |  {candidate.contractType}  |  " +
            $"HP {candidate.maxHP}  ATK {candidate.attack}  DEF {candidate.defense}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button hireButton = CreateActionButton(
            row,
            $"{candidate.hireCost} G",
            () => Hire(candidate));

        hireButtons.Add(hireButton);
        displayedCandidates.Add(candidate);
    }

    private void CreateCompanyRow(RectTransform parent, MercenaryInstance mercenary, float top)
    {
        RectTransform row = CreateRow(mercenary.MercenaryName, parent, top);
        CreateText(row, mercenary.MercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"LV {mercenary.Level}  |  {mercenary.MercenaryClass}  |  {mercenary.ContractType}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        string shortId = mercenary.InstanceId.Substring(0, 8).ToUpperInvariant();
        CreateText(row, $"ID {shortId}", 13, FontStyle.Normal, TextAnchor.MiddleRight,
            new Vector2(18f, -64f), new Vector2(-170f, -30f), MutedTextColor);

        string actionLabel = partyManager.Contains(mercenary) ? "REMOVE" : "ADD";
        CreateActionButton(row, actionLabel, () => TogglePartyMember(mercenary));
    }

    private void CreateGeneratedCandidateRow(
        RectTransform parent,
        MercenaryInstance candidate,
        float top)
    {
        RectTransform row = CreateRow(candidate.MercenaryName, parent, top);

        CreateText(row, candidate.MercenaryName, 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"{candidate.MercenaryClass}  |  {candidate.ContractType}  |  " +
            $"HP {candidate.MaxHP}  ATK {candidate.Attack}  DEF {candidate.Defense}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        Button hireButton = CreateActionButton(
            row,
            $"{candidate.HireCost} G",
            () => HireGeneratedCandidate(candidate));

        generatedHireButtons.Add(hireButton);
        displayedGeneratedCandidates.Add(candidate);
    }

    private void CreatePartyRow(
        RectTransform parent,
        MercenaryInstance mercenary,
        int slotIndex,
        float top)
    {
        RectTransform row = CreateRow($"Party Slot {slotIndex + 1}", parent, top);
        CreateText(row, $"{slotIndex + 1}. {mercenary.MercenaryName}", 22, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -42f), new Vector2(-160f, -12f), Color.white);

        string details =
            $"LV {mercenary.Level}  |  {mercenary.MercenaryClass}  |  " +
            $"HP {mercenary.CurrentHP}/{mercenary.MaxHP}";

        CreateText(row, details, 14, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(18f, -76f), new Vector2(-160f, -48f), MutedTextColor);

        CreateActionButton(row, "REMOVE", () => RemovePartyMember(mercenary));
    }

    private void CreateEmptyPartyRow(RectTransform parent, int slotIndex, float top)
    {
        RectTransform row = CreateRow($"Empty Party Slot {slotIndex + 1}", parent, top);

        CreateText(row, $"{slotIndex + 1}. EMPTY SLOT", 20, FontStyle.Bold,
            TextAnchor.MiddleLeft, new Vector2(18f, -58f), new Vector2(-18f, -28f),
            MutedTextColor);
    }

    private RectTransform CreateRow(string rowName, RectTransform parent, float top)
    {
        RectTransform row = CreateUIObject(rowName, parent);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.offsetMin = new Vector2(0f, top - 96f);
        row.offsetMax = new Vector2(0f, top);

        Image rowImage = row.gameObject.AddComponent<Image>();
        rowImage.color = RowColor;
        return row;
    }

    private Button CreateNavigationButton(
        RectTransform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateUIObject($"{label} Tab", parent);
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.sizeDelta = new Vector2(132f, 38f);
        buttonRect.anchoredPosition = position;

        Image image = buttonRect.gameObject.AddComponent<Image>();
        image.color = InactiveColor;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateButtonLabel(buttonRect, label, 15);
        return button;
    }

    private Button CreateActionButton(
        RectTransform parent,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform buttonRect = CreateUIObject("Action Button", parent);
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.sizeDelta = new Vector2(130f, 52f);
        buttonRect.anchoredPosition = new Vector2(-18f, 0f);

        Image image = buttonRect.gameObject.AddComponent<Image>();
        image.color = AccentColor;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateButtonLabel(buttonRect, label, 17);
        return button;
    }

    private void CreateButtonLabel(RectTransform parent, string label, int fontSize)
    {
        Text buttonText = CreateText(parent, label, fontSize, FontStyle.Bold,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Color.white);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;
    }

    private void Hire(MercenaryDataSO candidate)
    {
        if (!hireManager.TryHireMercenary(candidate))
        {
            statusText.text = $"Could not hire {candidate.mercenaryName}.";
            RefreshUI();
            return;
        }

        hiredCandidates.Add(candidate);
        statusText.text = $"{candidate.mercenaryName} joined your company.";
        RefreshUI();
    }

    private void HireGeneratedCandidate(MercenaryInstance candidate)
    {
        if (!hireManager.TryHireMercenary(candidate))
        {
            statusText.text = $"Could not hire {candidate.MercenaryName}.";
            RefreshUI();
            return;
        }

        statusText.text = $"{candidate.MercenaryName} joined your company.";
        mercenaryGenerator.RemoveCandidate(candidate);
        RefreshUI();
    }

    private void HandleMercenaryHired(MercenaryInstance mercenary)
    {
        RebuildCompanyList();
    }

    private void HandlePartyChanged()
    {
        RebuildCompanyList();
        RebuildPartyList();
        if (startBattleButton != null && !battleManager.IsBattling)
        {
            startBattleButton.interactable = partyManager.Members.Count > 0;
        }
        statusText.text = $"Party members: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void HandleCandidatesChanged()
    {
        RebuildHireList();
        RefreshUI();
    }

    private void StartPartyBattle()
    {
        battleLogText.text = string.Empty;
        startBattleButton.interactable = false;

        if (!battleManager.StartBattle(partyManager.Members))
        {
            startBattleButton.interactable = true;
        }
    }

    private void HandleBattleMessage(string message)
    {
        if (string.IsNullOrEmpty(battleLogText.text))
        {
            battleLogText.text = message;
        }
        else
        {
            battleLogText.text += $"\n{message}";
        }
    }

    private void HandleBattleCompleted(bool victory)
    {
        startBattleButton.interactable = partyManager.Members.Count > 0;
        RefreshUI();
        statusText.text = victory ? "Battle won." : "Battle lost.";
    }

    private void TogglePartyMember(MercenaryInstance mercenary)
    {
        if (partyManager.Contains(mercenary))
        {
            RemovePartyMember(mercenary);
            return;
        }

        if (!partyManager.TryAdd(mercenary))
        {
            statusText.text = "The party is full.";
        }
    }

    private void RemovePartyMember(MercenaryInstance mercenary)
    {
        partyManager.Remove(mercenary);
    }

    private void CacheAlreadyHiredCandidates()
    {
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary.BaseData != null)
            {
                hiredCandidates.Add(mercenary.BaseData);
            }
        }
    }

    private void ShowHirePage()
    {
        hirePage.gameObject.SetActive(true);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, true);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(battleTabButton, false);
        statusText.text = "Select a mercenary to hire.";
    }

    private void ShowCompanyPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(true);
        partyPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, true);
        SetTabActive(partyTabButton, false);
        SetTabActive(battleTabButton, false);
        RebuildCompanyList();
        statusText.text = $"Company mercenaries: {hireManager.HiredMercenaries.Count}";
    }

    private void ShowPartyPage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(true);
        battlePage.gameObject.SetActive(false);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, true);
        SetTabActive(battleTabButton, false);
        RebuildPartyList();
        statusText.text = $"Party members: {partyManager.Members.Count}/{partyManager.MaxPartySize}";
    }

    private void ShowBattlePage()
    {
        hirePage.gameObject.SetActive(false);
        companyPage.gameObject.SetActive(false);
        partyPage.gameObject.SetActive(false);
        battlePage.gameObject.SetActive(true);
        SetTabActive(hireTabButton, false);
        SetTabActive(companyTabButton, false);
        SetTabActive(partyTabButton, false);
        SetTabActive(battleTabButton, true);
        startBattleButton.interactable =
            partyManager.Members.Count > 0 && !battleManager.IsBattling;
        statusText.text = $"Battle party: {partyManager.Members.Count} mercenaries";
    }

    private void RefreshUI()
    {
        goldText.text = $"GOLD  {merchantData.Gold}";

        for (int i = 0; i < hireButtons.Count; i++)
        {
            MercenaryDataSO candidate = displayedCandidates[i];
            hireButtons[i].interactable =
                !hiredCandidates.Contains(candidate) && hireManager.CanAfford(candidate);
        }

        for (int i = 0; i < generatedHireButtons.Count; i++)
        {
            generatedHireButtons[i].interactable =
                hireManager.CanAfford(displayedGeneratedCandidates[i]);
        }
    }

    private static void SetTabActive(Button button, bool isActive)
    {
        button.targetGraphic.color = isActive ? AccentColor : InactiveColor;
    }

    private static void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Simple Hire UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = canvasObject.AddComponent<Image>();
        background.color = BackgroundColor;
        background.raycastTarget = false;
        return canvas;
    }

    private RectTransform CreatePanel(Transform parent)
    {
        RectTransform panel = CreateUIObject("Guild Panel", parent);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820f, 620f);
        panel.anchoredPosition = Vector2.zero;

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColor;
        return panel;
    }

    private static RectTransform CreatePage(string pageName, RectTransform parent)
    {
        RectTransform page = CreateUIObject(pageName, parent);
        page.anchorMin = new Vector2(0f, 0f);
        page.anchorMax = new Vector2(1f, 1f);
        page.offsetMin = new Vector2(28f, 64f);
        page.offsetMax = new Vector2(-28f, -126f);
        return page;
    }

    private Text CreateText(
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        RectTransform rect = CreateUIObject("Text", parent);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
