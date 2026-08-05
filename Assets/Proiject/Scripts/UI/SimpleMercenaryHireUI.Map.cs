using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SimpleMercenaryHireUI
{
    private void BuildTravelConfirmationOverlay()
    {
        travelConfirmation.overlay =
            GetOrCreateOverlay(
                SimpleMercenaryHireOverlaySlot.TravelConfirmation,
                "Travel Confirmation Overlay");
        travelConfirmation.overlay.gameObject.SetActive(false);
        travelConfirmation.overlay.anchorMin = Vector2.zero;
        travelConfirmation.overlay.anchorMax = Vector2.one;
        travelConfirmation.overlay.offsetMin = Vector2.zero;
        travelConfirmation.overlay.offsetMax = Vector2.zero;
        travelConfirmation.overlay.gameObject.AddComponent<Image>().color =
            new Color(0f, 0f, 0f, 0.84f);

        RectTransform window =
            CreateUIObject("Travel Confirmation Window", travelConfirmation.overlay);
        window.anchorMin = window.anchorMax = window.pivot =
            new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(780f, 650f);
        ApplyParchmentPanel(window.gameObject.AddComponent<Image>());

        CreateText(
            window,
            "町を移動しますか？",
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(24f, -58f),
            new Vector2(-24f, -16f),
            ParchmentTextColor);

        travelConfirmation.text = CreateText(
            window,
            string.Empty,
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(36f, -116f),
            new Vector2(-36f, -64f),
            ParchmentMutedColor);

        travelConfirmation.cargoSummaryText = CreateText(
            window,
            string.Empty,
            17,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(36f, -154f),
            new Vector2(-36f, -120f),
            ParchmentTextColor);
        travelConfirmation.cargoContent = CreateScrollableContent(
            window,
            "Travel Cargo Viewport",
            "Travel Cargo Content",
            new Vector2(36f, 96f),
            new Vector2(-36f, -170f));

        Button confirmButton =
            CreateActionButton(
                window,
                "移動する",
                ConfirmTownTravelWithCargo);
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax =
            confirmRect.pivot = new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(180f, 48f);
        confirmRect.anchoredPosition = new Vector2(-105f, 28f);
        confirmButton.targetGraphic.color = AccentColor;

        Button cancelButton =
            CreateActionButton(window, "キャンセル", HideTravelConfirmation);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.anchorMin = cancelRect.anchorMax =
            cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.sizeDelta = new Vector2(180f, 48f);
        cancelRect.anchoredPosition = new Vector2(105f, 28f);

        travelConfirmation.overlay.gameObject.SetActive(false);
    }

    private void ConfirmTownTravelWithCargo()
    {
        List<RoadCargoEntry> cargo = new List<RoadCargoEntry>();
        foreach (KeyValuePair<ItemDataSO, int> entry in travelConfirmation.selectedCargo)
        {
            if (entry.Key != null && entry.Value > 0)
            {
                cargo.Add(new RoadCargoEntry(entry.Key, entry.Value));
            }
        }
        townTravelController.ConfirmTownTravel(
            cargo,
            new List<string>(travelConfirmation.selectedCompanions));
    }

    private void RefreshTravelCargoSelection()
    {
        if (travelConfirmation.cargoContent == null || townTravelController == null)
        {
            return;
        }

        for (int i = travelConfirmation.cargoContent.childCount - 1; i >= 0; i--)
        {
            Destroy(travelConfirmation.cargoContent.GetChild(i).gameObject);
        }

        int origin = townTravelController.ConfirmationOriginTownIndex;
        int destination = townTravelController.ConfirmationDestinationTownIndex;
        int used = 0;
        foreach (int amount in travelConfirmation.selectedCargo.Values)
        {
            used += amount;
        }
        int capacity = roadCargoSession != null ? roadCargoSession.Capacity : 0;
        travelConfirmation.cargoSummaryText.text =
            $"積載量 {used} / {capacity}　同行傭兵 {travelConfirmation.selectedCompanions.Count}人";
        float top = -10f;
        bool hasCargo = false;
        foreach (InventoryItemStack stack in merchantInventory.GetItemsIn(origin))
        {
            ItemDataSO item = stack?.Item;
            if (item == null ||
                (item.itemType != ItemType.Material &&
                 item.itemType != ItemType.Consumable))
            {
                continue;
            }

            hasCargo = true;
            travelConfirmation.selectedCargo.TryGetValue(item, out int selected);
            int currentPrice = GetTravelSellPrice(origin, item);
            int destinationPrice = GetTravelSellPrice(destination, item);
            int difference = destinationPrice - currentPrice;
            CreateTravelCargoRow(
                item,
                stack.Amount,
                selected,
                currentPrice,
                destinationPrice,
                difference,
                ref top);
        }

        if (!hasCargo)
        {
            CreateScrollLabel(travelConfirmation.cargoContent, "積める素材・消耗品はありません。", ref top);
        }
        top -= 12f;
        CreateScrollLabel(travelConfirmation.cargoContent, "同行する傭兵", ref top);
        bool hasCompanion = false;
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (!CanTravelWithCompanion(mercenary, origin))
            {
                continue;
            }
            hasCompanion = true;
            CreateTravelCompanionRow(mercenary, ref top);
        }
        if (!hasCompanion)
        {
            CreateScrollLabel(
                travelConfirmation.cargoContent,
                "同行できる非編成傭兵はいません。",
                ref top);
        }
        travelConfirmation.cargoContent.sizeDelta = new Vector2(0f, Mathf.Max(260f, -top + 12f));
    }

    private bool CanTravelWithCompanion(MercenaryInstance mercenary, int origin)
    {
        return mercenary != null && mercenary.IsContractActive &&
               mercenary.CurrentTownIndex == origin &&
               !MercenaryDutyService.IsOnDuty(mercenary.InstanceId);
    }

    private void CreateTravelCompanionRow(
        MercenaryInstance mercenary,
        ref float top)
    {
        bool selected = travelConfirmation.selectedCompanions.Contains(mercenary.InstanceId);
        CreateScrollLabel(
            travelConfirmation.cargoContent,
            mercenary.MercenaryName + (mercenary.IsIncapacitated ? "（戦闘不能）" : ""),
            ref top);
        Button toggle = CreateActionButton(
            travelConfirmation.cargoContent,
            selected ? "解除" : "同行",
            () => ToggleTravelCompanion(mercenary.InstanceId));
        ConfigureTravelCargoStepButton(toggle, new Vector2(-18f, top + 18f));
        top -= 8f;
    }

    private void ToggleTravelCompanion(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return;
        }
        if (!travelConfirmation.selectedCompanions.Add(instanceId))
        {
            travelConfirmation.selectedCompanions.Remove(instanceId);
        }
        RefreshTravelCargoSelection();
    }

    private void CreateTravelCargoRow(
        ItemDataSO item,
        int owned,
        int selected,
        int currentPrice,
        int destinationPrice,
        int difference,
        ref float top)
    {
        string differenceText = difference >= 0 ? $"+{difference}" : difference.ToString();
        Text label = CreateText(
            travelConfirmation.cargoContent,
            $"{JapaneseDisplayText.GetItemName(item)} 所持 {owned} / 積載 {selected}\n" +
            $"売値 {currentPrice}G → {destinationPrice}G （差額 {differenceText}G）",
            14,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(12f, top - 50f),
            new Vector2(-130f, top),
            ParchmentMutedColor);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;

        Button minus = CreateActionButton(
            travelConfirmation.cargoContent,
            "－",
            () => ChangeTravelCargo(item, -1));
        ConfigureTravelCargoStepButton(minus, new Vector2(-112f, top - 30f));
        Button plus = CreateActionButton(
            travelConfirmation.cargoContent,
            "＋",
            () => ChangeTravelCargo(item, 1));
        ConfigureTravelCargoStepButton(plus, new Vector2(-56f, top - 30f));
        top -= 58f;
    }

    private static void ConfigureTravelCargoStepButton(Button button, Vector2 position)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(48f, 32f);
        rect.anchoredPosition = position;
    }

    private void ChangeTravelCargo(ItemDataSO item, int delta)
    {
        if (item == null || delta == 0)
        {
            return;
        }

        travelConfirmation.selectedCargo.TryGetValue(item, out int selected);
        int owned = merchantInventory.GetItemAmountIn(
            townTravelController.ConfirmationOriginTownIndex,
            item);
        int used = 0;
        foreach (int amount in travelConfirmation.selectedCargo.Values)
        {
            used += amount;
        }
        int capacity = roadCargoSession != null ? roadCargoSession.Capacity : 0;
        if (delta > 0 && (selected >= owned || used >= capacity))
        {
            return;
        }

        selected = Mathf.Clamp(selected + delta, 0, owned);
        if (selected == 0)
        {
            travelConfirmation.selectedCargo.Remove(item);
        }
        else
        {
            travelConfirmation.selectedCargo[item] = selected;
        }
        RefreshTravelCargoSelection();
    }

    private int GetTravelSellPrice(int townIndex, ItemDataSO item)
    {
        int basePrice = marketPriceManager != null
            ? marketPriceManager.GetSellPrice(item)
            : item.basePrice;
        return Mathf.Max(1, Mathf.RoundToInt(basePrice *
            WorldMapService.GetTownDemandMultiplier(townIndex, item)));
    }



    // Swaps the town map background to the current town's dedicated image,
    // falling back to the shared TownMap when a town image is missing.














    private void HideTravelConfirmation()
    {
        travelConfirmation.overlay?.gameObject.SetActive(false);
        travelConfirmation.selectedCargo.Clear();
        travelConfirmation.selectedCompanions.Clear();
        townTravelController.ClearTravelConfirmation();
    }

    private IEnumerator ContinueTownTravelBattleRoutine()
    {
        yield return null;

        townTravelController.StartNextTravelEncounter();
    }





    private void SyncDungeonUnlocks()
    {
        if (dungeonRunManager == null)
        {
            dungeonRunManager =
                GetComponent<DungeonRunManager>() ??
                FindObjectOfType<DungeonRunManager>();
        }

        dungeonRunManager?.SetUnlockedTownIndices(townProgressState.GetUnlockedTownIndices());
    }

    private bool TryUnlockHiddenIsland()
    {
        bool unlocked = HiddenIslandUnlockService.TryUnlock(
            townProgressState,
            dungeonRunManager,
            merchantInventory,
            hireManager != null ? hireManager.HiredMercenaries : null);
        if (unlocked)
        {
            SyncDungeonUnlocks();
            saveManager?.SaveGame();
        }
        return unlocked;
    }

}
