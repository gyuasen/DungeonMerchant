using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Covers the branch-heavy travel decisions of <see cref="TownTravelController"/>:
/// travel validation, the confirmation state, the road-battle continue/retreat
/// guards and the pending-cargo queries. UI rendering and presentation are out
/// of scope; the controller's UI callbacks are captured instead.
/// </summary>
public sealed class TownTravelControllerTests
{
    private GameObject root;
    private TownProgressState townProgressState;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private BattleManager battleManager;
    private RoadEncounterService roadEncounterService;
    private DungeonRunManager dungeonRunManager;
    private DayManager dayManager;
    private MercenaryGenerator mercenaryGenerator;
    private MarketStockManager marketStockManager;
    private BlacksmithManager blacksmithManager;
    private MerchantData merchantData;
    private MerchantInventory inventory;
    private RoadCargoSession roadCargoSession;
    private TownTravelController controller;
    private MercenaryDataSO mercenaryData;
    private readonly List<UnityEngine.Object> createdObjects =
        new List<UnityEngine.Object>();

    private string status;
    private string travelConfirmationMessage;
    private int showTownMapCount;
    private int showWorldMapCount;
    private int hideTravelConfirmationCount;
    private int openNearbyDungeonCount;
    private int continueTravelBattleCount;
    private int refreshTownMapButtonsCount;
    private int syncDungeonUnlocksCount;
    private bool? lastRoadChoiceButtonsActive;

    [SetUp]
    public void SetUp()
    {
        status = null;
        travelConfirmationMessage = null;
        showTownMapCount = 0;
        showWorldMapCount = 0;
        hideTravelConfirmationCount = 0;
        openNearbyDungeonCount = 0;
        continueTravelBattleCount = 0;
        refreshTownMapButtonsCount = 0;
        syncDungeonUnlocksCount = 0;
        lastRoadChoiceButtonsActive = null;

        root = new GameObject("Town Travel Controller Test");
        merchantData = root.AddComponent<MerchantData>();
        root.AddComponent<MarketPriceManager>();
        townProgressState = root.AddComponent<TownProgressState>();
        // Sail (2) is the starting town of the fixed progression order and
        // Leaf (1) is its only unlocked neighbour, so unlock travel targets
        // Eld (0) two steps down the route.
        townProgressState.Initialize(2, new[] { 1, 2 });
        root.AddComponent<ProgressionManager>();
        inventory = root.AddComponent<MerchantInventory>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        partyManager = root.AddComponent<MercenaryPartyManager>();
        battleManager = root.AddComponent<BattleManager>();
        roadEncounterService = root.AddComponent<RoadEncounterService>();
        dungeonRunManager = root.AddComponent<DungeonRunManager>();
        dayManager = root.AddComponent<DayManager>();
        mercenaryGenerator = root.AddComponent<MercenaryGenerator>();
        marketStockManager = root.AddComponent<MarketStockManager>();
        blacksmithManager = root.AddComponent<BlacksmithManager>();
        roadCargoSession = root.AddComponent<RoadCargoSession>();

        mercenaryData = ScriptableObject.CreateInstance<MercenaryDataSO>();
        mercenaryData.mercenaryName = "Tester";
        mercenaryData.maxHP = 100;
        createdObjects.Add(mercenaryData);

        controller = CreateController();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        foreach (UnityEngine.Object created in createdObjects)
        {
            if (created != null)
            {
                UnityEngine.Object.DestroyImmediate(created);
            }
        }
        createdObjects.Clear();
    }

    [Test]
    public void TravelToTown_WithCurrentTown_ShowsTownMapWithoutAskingToConfirm()
    {
        controller.TravelToTown(townProgressState.CurrentTownIndex);

        Assert.That(showTownMapCount, Is.EqualTo(1));
        Assert.That(travelConfirmationMessage, Is.Null);
        Assert.That(
            controller.ConfirmationDestinationTownIndex,
            Is.EqualTo(-1),
            "Staying put must never arm the travel confirmation.");
    }

    [Test]
    public void TravelToDungeon_WithCurrentTown_OpensNearbyDungeonWithoutTravelling()
    {
        controller.TravelToDungeon(townProgressState.CurrentTownIndex);

        Assert.That(openNearbyDungeonCount, Is.EqualTo(1));
        Assert.That(travelConfirmationMessage, Is.Null);
        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(-1));
    }

    [Test]
    public void TravelToTown_WithAdjacentUnlockedTown_ArmsConfirmationForThatTown()
    {
        AddPartyMember();

        controller.TravelToTown(1);

        Assert.That(travelConfirmationMessage, Is.Not.Null);
        Assert.That(controller.ConfirmationOriginTownIndex, Is.EqualTo(2));
        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(1));
        Assert.That(
            travelConfirmationMessage,
            Does.Not.Contain("新しい町が解放されます"),
            "Travelling to an already unlocked town is not an unlock quest.");
    }

    [Test]
    public void TravelToTown_WithAdjacentLockedTown_AnnouncesTheUnlockReward()
    {
        AddPartyMember();
        townProgressState.SetCurrentTown(1);

        controller.TravelToTown(0);

        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(0));
        Assert.That(
            travelConfirmationMessage,
            Does.Contain("新しい町が解放されます"));
    }

    [Test]
    public void TravelToTown_WithoutPartyMembers_IsRejectedBeforeConfirmation()
    {
        controller.TravelToTown(1);

        Assert.That(status, Does.Contain("傭兵の編成が必要です"));
        Assert.That(travelConfirmationMessage, Is.Null);
        Assert.That(
            controller.ConfirmationDestinationTownIndex,
            Is.EqualTo(-1),
            "A rejected request must not leave a destination armed.");
    }

    [Test]
    public void TravelToTown_WithNonAdjacentTown_RejectsAndNamesTheNextStop()
    {
        AddPartyMember();

        controller.TravelToTown(3);

        Assert.That(status, Does.Contain("直接は移動できません"));
        Assert.That(
            status,
            Does.Contain(WorldMapService.TownNames[1]),
            "Sail(2)->Norn(3) must route through the next stop Leaf(1).");
        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(-1));
    }

    [Test]
    public void TravelToTown_WithHiddenIslandWhileRouteUndiscovered_IsRejected()
    {
        AddPartyMember();

        controller.TravelToTown(WorldMapService.HiddenIslandTownIndex);

        Assert.That(status, Does.Contain("中央島へ至る航路はまだ発見されていません"));
        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(-1));
    }

    [Test]
    public void TravelToTown_WithDiscoveredHiddenIslandRoute_PromisesNoRoadBattle()
    {
        AddPartyMember();
        townProgressState.UnlockTown(WorldMapService.HiddenIslandTownIndex);

        controller.TravelToTown(WorldMapService.HiddenIslandTownIndex);

        Assert.That(
            controller.ConfirmationDestinationTownIndex,
            Is.EqualTo(WorldMapService.HiddenIslandTownIndex));
        Assert.That(travelConfirmationMessage, Does.Contain("中央島航路"));
        Assert.That(
            travelConfirmationMessage,
            Does.Contain("街道戦闘と日数経過はありません"));
    }

    [Test]
    public void TravelToTown_WithPendingRoadCargo_RefusesToLeaveTheTown()
    {
        AddPartyMember();
        BeginRoadCargoSession(2, 1, 1);

        controller.TravelToTown(1);

        Assert.That(status, Does.Contain("未受取の街道荷物"));
        Assert.That(travelConfirmationMessage, Is.Null);
        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(-1));
    }

    [Test]
    public void ConfirmTownTravel_WithoutAnArmedDestination_DoesNothing()
    {
        controller.ConfirmTownTravel();

        Assert.That(hideTravelConfirmationCount, Is.Zero);
        Assert.That(status, Is.Null);
        Assert.That(controller.RoadTravelState.IsActive, Is.False);
    }

    [Test]
    public void ConfirmTownTravel_WithCargoTheStorageLacks_ReportsFailureAndStaysHome()
    {
        AddPartyMember();
        controller.TravelToTown(1);
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);

        controller.ConfirmTownTravel(
            new List<RoadCargoEntry> { new RoadCargoEntry(item, 3) });

        Assert.That(status, Does.Contain("荷物が倉庫に不足しています"));
        Assert.That(hideTravelConfirmationCount, Is.Zero);
        Assert.That(roadCargoSession.IsActive, Is.False);
        Assert.That(controller.RoadTravelState.IsActive, Is.False);
    }

    [Test]
    public void ClearTravelConfirmation_DisarmsTheDestination()
    {
        AddPartyMember();
        controller.TravelToTown(1);
        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(1));

        controller.ClearTravelConfirmation();

        Assert.That(controller.ConfirmationDestinationTownIndex, Is.EqualTo(-1));
    }

    [Test]
    public void ContinueTownTravel_WhenNotAwaitingChoice_IsIgnored()
    {
        controller.RoadTravelState.Begin(1, false, false, 3);

        controller.ContinueTownTravel();

        Assert.That(continueTravelBattleCount, Is.Zero);
        Assert.That(lastRoadChoiceButtonsActive, Is.Null);
        Assert.That(
            controller.RoadTravelState.EncounterIndex,
            Is.EqualTo(1),
            "Ignoring the request must not advance the encounter counter.");
    }

    [Test]
    public void ContinueTownTravel_WhileAwaitingChoice_AdvancesToTheNextEncounter()
    {
        controller.RoadTravelState.Begin(1, false, false, 3);
        controller.RoadTravelState.AwaitChoice();

        controller.ContinueTownTravel();

        Assert.That(controller.RoadTravelState.EncounterIndex, Is.EqualTo(2));
        Assert.That(controller.RoadTravelState.IsAwaitingChoice, Is.False);
        Assert.That(lastRoadChoiceButtonsActive, Is.False);
        Assert.That(continueTravelBattleCount, Is.EqualTo(1));
        Assert.That(status, Does.Contain("2/3"));
    }

    [Test]
    public void RetreatFromTownTravel_WhileAwaitingChoice_ClearsTravelAndReturnsHome()
    {
        controller.RoadTravelState.Begin(1, false, false, 3);
        controller.RoadTravelState.AwaitChoice();

        controller.RetreatFromTownTravel();

        Assert.That(controller.RoadTravelState.IsActive, Is.False);
        Assert.That(lastRoadChoiceButtonsActive, Is.False);
        Assert.That(showTownMapCount, Is.EqualTo(1));
        Assert.That(status, Does.Contain("街道から撤退し"));
    }

    [Test]
    public void RetreatFromTownTravel_ReturnsDepartedCargoToTheOriginTown()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        BeginRoadCargoSession(2, 1, 2, item);
        Assert.That(inventory.GetItemAmountIn(2, item), Is.Zero);
        controller.RoadTravelState.Begin(1, false, false, 3);
        controller.RoadTravelState.AwaitChoice();

        controller.RetreatFromTownTravel();

        Assert.That(
            inventory.GetItemAmountIn(2, item),
            Is.EqualTo(2),
            "Retreating must put the whole manifest back in the origin storage.");
        Assert.That(roadCargoSession.IsActive, Is.False);
    }

    [Test]
    public void RetreatFromTownTravel_WhenNotAwaitingChoice_IsIgnored()
    {
        controller.RoadTravelState.Begin(1, false, false, 3);

        controller.RetreatFromTownTravel();

        Assert.That(controller.RoadTravelState.IsActive, Is.True);
        Assert.That(showTownMapCount, Is.Zero);
        Assert.That(status, Is.Null);
    }

    [Test]
    public void HandleRoadBattleOutcome_WithoutActiveTravel_ReportsItDidNotHandleIt()
    {
        Assert.That(controller.HandleRoadBattleOutcome(true), Is.False);
        Assert.That(controller.HandleRoadBattleOutcome(false), Is.False);
        Assert.That(status, Is.Null);
    }

    [Test]
    public void HandleRoadBattleOutcome_VictoryMidRoute_AsksToContinueInsteadOfArriving()
    {
        controller.RoadTravelState.Begin(1, false, false, 3);

        bool handled = controller.HandleRoadBattleOutcome(true);

        Assert.That(handled, Is.True);
        Assert.That(controller.RoadTravelState.IsAwaitingChoice, Is.True);
        Assert.That(lastRoadChoiceButtonsActive, Is.True);
        Assert.That(status, Does.Contain("続行しますか"));
        Assert.That(
            townProgressState.CurrentTownIndex,
            Is.EqualTo(2),
            "A mid-route victory must not move the player yet.");
    }

    [Test]
    public void HandleRoadBattleOutcome_VictoryOnFinalEncounter_ArrivesUnlocksAndAdvancesTheDay()
    {
        int dayBefore = dayManager.CurrentDay;
        townProgressState.SetCurrentTown(1);
        controller.RoadTravelState.Begin(0, true, false, 1);

        bool handled = controller.HandleRoadBattleOutcome(true);

        Assert.That(handled, Is.True);
        Assert.That(townProgressState.CurrentTownIndex, Is.EqualTo(0));
        Assert.That(townProgressState.IsTownUnlocked(0), Is.True);
        Assert.That(dayManager.CurrentDay, Is.EqualTo(dayBefore + 1));
        Assert.That(controller.RoadTravelState.IsActive, Is.False);
        Assert.That(syncDungeonUnlocksCount, Is.EqualTo(1));
        Assert.That(refreshTownMapButtonsCount, Is.EqualTo(1));
        Assert.That(showTownMapCount, Is.EqualTo(1));
        Assert.That(openNearbyDungeonCount, Is.Zero);
        Assert.That(status, Does.Contain("解放しました"));
    }

    [Test]
    public void HandleRoadBattleOutcome_VictoryWithDungeonRequest_OpensTheDungeonInsteadOfTheTownMap()
    {
        controller.RoadTravelState.Begin(1, false, true, 1);

        Assert.That(controller.HandleRoadBattleOutcome(true), Is.True);

        Assert.That(townProgressState.CurrentTownIndex, Is.EqualTo(1));
        Assert.That(openNearbyDungeonCount, Is.EqualTo(1));
        Assert.That(showTownMapCount, Is.Zero);
    }

    [Test]
    public void HandleRoadBattleOutcome_Defeat_KeepsTheTownAndTheDayUnchanged()
    {
        int dayBefore = dayManager.CurrentDay;
        townProgressState.SetCurrentTown(1);
        controller.RoadTravelState.Begin(0, true, false, 1);

        bool handled = controller.HandleRoadBattleOutcome(false);

        Assert.That(handled, Is.True);
        Assert.That(townProgressState.CurrentTownIndex, Is.EqualTo(1));
        Assert.That(townProgressState.IsTownUnlocked(0), Is.False);
        Assert.That(dayManager.CurrentDay, Is.EqualTo(dayBefore));
        Assert.That(controller.RoadTravelState.IsActive, Is.False);
        Assert.That(showWorldMapCount, Is.EqualTo(1));
        Assert.That(status, Does.Contain("敗北"));
    }

    [Test]
    public void HandleRoadBattleOutcome_DefeatWithCargo_LosesAQuarterAndReportsTheLoss()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        BeginRoadCargoSession(2, 1, 4, item);
        controller.RoadTravelState.Begin(1, false, false, 1);

        Assert.That(controller.HandleRoadBattleOutcome(false), Is.True);

        Assert.That(
            inventory.GetItemAmountIn(2, item),
            Is.EqualTo(3),
            "A quarter of each stack (rounded up) is lost, the rest comes home.");
        Assert.That(status, Does.Contain("街道荷物を1個失い"));
    }

    [Test]
    public void CanReceivePendingRoadCargo_OnlyAtTheOriginOrDestinationTown()
    {
        Assert.That(
            controller.CanReceivePendingRoadCargo(),
            Is.False,
            "There is nothing to receive without an active cargo session.");

        BeginRoadCargoSession(2, 1, 1);

        Assert.That(
            controller.CanReceivePendingRoadCargo(),
            Is.True,
            "The player is standing in the origin town.");

        townProgressState.SetCurrentTown(0);

        Assert.That(
            controller.CanReceivePendingRoadCargo(),
            Is.False,
            "A town that is neither origin nor destination cannot receive it.");
    }

    [Test]
    public void TryReceivePendingRoadCargo_AtTheOriginTown_ReturnsTheCargoAndShowsTheTownMap()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        BeginRoadCargoSession(2, 1, 2, item);

        RoadCargoResolutionResult result = controller.TryReceivePendingRoadCargo();

        Assert.That(result, Is.EqualTo(RoadCargoResolutionResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(2, item), Is.EqualTo(2));
        Assert.That(roadCargoSession.IsActive, Is.False);
        Assert.That(showTownMapCount, Is.EqualTo(1));
        Assert.That(status, Does.Contain("倉庫へ搬入しました"));
    }

    [Test]
    public void TryReceivePendingRoadCargo_AtTheDestinationTown_DeliversTheCargoThere()
    {
        ItemDataSO item = NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        BeginRoadCargoSession(2, 1, 2, item);
        townProgressState.SetCurrentTown(1);

        RoadCargoResolutionResult result = controller.TryReceivePendingRoadCargo();

        Assert.That(result, Is.EqualTo(RoadCargoResolutionResult.Succeeded));
        Assert.That(inventory.GetItemAmountIn(1, item), Is.EqualTo(2));
        Assert.That(inventory.GetItemAmountIn(2, item), Is.Zero);
    }

    [Test]
    public void TryReceivePendingRoadCargo_WithNothingPending_IsANoOp()
    {
        RoadCargoResolutionResult result = controller.TryReceivePendingRoadCargo();

        Assert.That(result, Is.EqualTo(RoadCargoResolutionResult.NoActiveSession));
        Assert.That(showTownMapCount, Is.Zero);
        Assert.That(status, Is.Null);
    }

    [Test]
    public void CanEnterWorldRegion_AllowsTheStartingRegionAndBlocksUnclearedGatedOnes()
    {
        Assert.That(
            controller.CanEnterWorldRegion(0),
            Is.True,
            "The starting region is always enterable.");
        Assert.That(
            controller.CanEnterWorldRegion(2),
            Is.False,
            "Region 2 stays gated until its gate town's dungeon is fully cleared.");
        Assert.That(
            controller.CanEnterWorldRegion(WorldMapService.HiddenIslandWorldMapIndex),
            Is.False,
            "The hidden island is closed until its town is discovered.");

        townProgressState.UnlockTown(WorldMapService.HiddenIslandTownIndex);

        Assert.That(
            controller.CanEnterWorldRegion(WorldMapService.HiddenIslandWorldMapIndex),
            Is.True);
    }

    [Test]
    public void ApplyTownServiceSettings_InATownWithoutHiring_ClearsTheCandidateList()
    {
        // Velm (5) is one of the towns TownServicePolicy closes hiring in, so
        // arriving there must wipe whatever candidates the last town offered.
        Assert.That(TownServicePolicy.IsHiringAvailable(5), Is.False);
        controller.ApplyTownServiceSettings(true, false);
        townProgressState.SetCurrentTown(5);

        controller.ApplyTownServiceSettings(true, false);

        Assert.That(mercenaryGenerator.Candidates, Is.Empty);
    }

    [Test]
    public void ApplyTownServiceSettings_InAHiringTown_KeepsTheCandidateList()
    {
        Assert.That(
            TownServicePolicy.IsHiringAvailable(
                townProgressState.CurrentTownIndex),
            Is.True);
        mercenaryGenerator.SetTownIndex(townProgressState.CurrentTownIndex);
        int candidateCount = mercenaryGenerator.Candidates.Count;

        controller.ApplyTownServiceSettings(true, false);

        Assert.That(
            mercenaryGenerator.Candidates.Count,
            Is.EqualTo(candidateCount),
            "A hiring town regenerates the same deterministic candidate block.");
    }

    private TownTravelController CreateController()
    {
        return new TownTravelController(
            townProgressState,
            partyManager,
            battleManager,
            roadEncounterService,
            dungeonRunManager,
            dayManager,
            mercenaryGenerator,
            marketStockManager,
            blacksmithManager,
            null,
            roadCargoSession,
            message => status = message,
            () => showTownMapCount++,
            () => showWorldMapCount++,
            message => travelConfirmationMessage = message,
            () => hideTravelConfirmationCount++,
            () => { },
            (_, __) => { },
            active => lastRoadChoiceButtonsActive = active,
            _ => { },
            () => continueTravelBattleCount++,
            () => openNearbyDungeonCount++,
            () => syncDungeonUnlocksCount++,
            () => refreshTownMapButtonsCount++);
    }

    private MercenaryInstance AddPartyMember()
    {
        MercenaryInstance mercenary = new MercenaryInstance(mercenaryData);
        mercenary.SetCurrentTownIndex(townProgressState.CurrentTownIndex);
        hireManager.RestoreHiredMercenaries(new[] { mercenary });
        partyManager.RestoreParty(new[] { mercenary });
        Assert.That(
            partyManager.Members.Count,
            Is.EqualTo(1),
            "The travel tests need a party before travel validation runs.");
        return mercenary;
    }

    private void BeginRoadCargoSession(
        int originTownIndex,
        int destinationTownIndex,
        int amount,
        ItemDataSO item = null)
    {
        item = item ?? NormalItemAt(0);
        Assert.That(item, Is.Not.Null);
        Assert.That(
            inventory.DepositItemTo(originTownIndex, item, amount),
            Is.True);
        Assert.That(
            roadCargoSession.TryBegin(
                originTownIndex,
                destinationTownIndex,
                new List<RoadCargoEntry> { new RoadCargoEntry(item, amount) }),
            Is.EqualTo(RoadCargoDepartureResult.Succeeded));
    }

    private static ItemDataSO NormalItemAt(int index)
    {
        int current = 0;
        foreach (ItemDataSO item in GameAssetRepository.LoadAll<ItemDataSO>())
        {
            if (item != null &&
                (item.itemType == ItemType.Material ||
                 item.itemType == ItemType.Consumable))
            {
                if (current == index)
                {
                    return item;
                }
                current++;
            }
        }
        return null;
    }
}
