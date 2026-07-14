using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class JobChangePageUI : ListPageUIBase
{
    [SerializeField] private ScrollRect scrollRect;
    private Func<IEnumerable<MercenaryInstance>> mercenaryProvider;
    private Func<MercenaryInstance, bool> shouldShowSpecialPromotion;
    private Action<MercenaryInstance, MercenaryClass> promoteAction;
    private MercenaryInstance pendingMercenary;
    private MercenaryClass pendingTarget;

    public void Initialize(
        Text title,
        ScrollRect targetScrollRect,
        RectTransform targetListRoot)
    {
        base.Initialize(title, null, targetListRoot);
        scrollRect = targetScrollRect;
        scrollRect.content = targetListRoot;
    }

    public void ConfigureJobChangeList(
        Func<IEnumerable<MercenaryInstance>> targetMercenaryProvider,
        Func<MercenaryInstance, bool> targetShouldShowSpecialPromotion,
        Action<MercenaryInstance, MercenaryClass> targetPromoteAction)
    {
        mercenaryProvider = targetMercenaryProvider;
        shouldShowSpecialPromotion = targetShouldShowSpecialPromotion;
        promoteAction = targetPromoteAction;
    }

    public override void Refresh()
    {
        if (mercenaryProvider == null)
        {
            base.Refresh();
            return;
        }

        RebuildRows(
            mercenaryProvider.Invoke(),
            112f,
            430f,
            null,
            null,
            (_, mercenary, top) => CreateJobChangeRow(mercenary, top),
            null);
    }

    private void CreateJobChangeRow(
        MercenaryInstance mercenary,
        float top)
    {
        RectTransform row =
            CreateRow(
                $"Job Change {mercenary.InstanceId}",
                ListRoot,
                top,
                RowColor,
                FrameColor);
        CreateText(
            row,
            $"{mercenary.MercenaryName}  Lv{mercenary.Level}  " +
            $"{JapaneseDisplayText.GetMercenaryClass(mercenary.MercenaryClass)}",
            RowFont,
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            new Vector2(16f, -44f),
            new Vector2(-370f, -8f),
            RowTextColor);

        if (mercenary.CanPromote)
        {
            CreatePromotionButtons(row, mercenary);
            return;
        }

        CreateUnavailableMessage(row, mercenary);
    }

    private void CreatePromotionButtons(
        RectTransform row,
        MercenaryInstance mercenary)
    {
        if (pendingMercenary == mercenary)
        {
            CreatePromotionConfirmation(row, mercenary);
            return;
        }

        MercenaryClass[] advanced =
            MercenaryClassProgression.GetAdvancedClasses(
                mercenary.MercenaryClass);
        CreatePromotionButton(row, mercenary, advanced[0], -18f);
        CreatePromotionButton(row, mercenary, advanced[1], -130f);

        bool showSpecial =
            shouldShowSpecialPromotion?.Invoke(mercenary) ?? false;
        if (!showSpecial)
        {
            return;
        }

        CreatePromotionButton(
            row,
            mercenary,
            MercenaryClassProgression.GetSpecialClass(
                mercenary.MercenaryClass),
            -242f);
    }

    private void CreatePromotionButton(
        RectTransform row,
        MercenaryInstance mercenary,
        MercenaryClass target,
        float x)
    {
        Button button = CreateActionButton(
            row,
            JapaneseDisplayText.GetMercenaryClass(target),
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            () => RequestPromotion(mercenary, target));
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(104f, 44f);
        rect.anchoredPosition = new Vector2(x, 0f);
    }

    private void RequestPromotion(
        MercenaryInstance mercenary,
        MercenaryClass target)
    {
        pendingMercenary = mercenary;
        pendingTarget = target;
        Refresh();
    }

    private void CreatePromotionConfirmation(
        RectTransform row,
        MercenaryInstance mercenary)
    {
        bool consumesCertificate =
            !mercenary.IsUnique &&
            pendingTarget == MercenaryClassProgression.GetSpecialClass(
                mercenary.MercenaryClass);
        string suffix = consumesCertificate ? "（転職証を消費）" : string.Empty;
        CreateText(
            row,
            $"{JapaneseDisplayText.GetMercenaryClass(pendingTarget)}へ転職{suffix}",
            RowFont,
            14,
            FontStyle.Bold,
            TextAnchor.MiddleRight,
            new Vector2(250f, -78f),
            new Vector2(-250f, -44f),
            RowTextColor);

        Button confirm = CreateActionButton(
            row,
            "決定",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            ConfirmPromotion);
        RectTransform confirmRect = confirm.GetComponent<RectTransform>();
        confirmRect.sizeDelta = new Vector2(104f, 44f);
        confirmRect.anchoredPosition = new Vector2(-18f, 0f);

        Button cancel = CreateActionButton(
            row,
            "取消",
            RowFont,
            ButtonColor,
            FrameColor,
            ButtonTextColor,
            CancelPromotion);
        RectTransform cancelRect = cancel.GetComponent<RectTransform>();
        cancelRect.sizeDelta = new Vector2(104f, 44f);
        cancelRect.anchoredPosition = new Vector2(-130f, 0f);
    }

    private void ConfirmPromotion()
    {
        MercenaryInstance mercenary = pendingMercenary;
        MercenaryClass target = pendingTarget;
        pendingMercenary = null;
        promoteAction?.Invoke(mercenary, target);
        Refresh();
    }

    private void CancelPromotion()
    {
        pendingMercenary = null;
        Refresh();
    }

    private void CreateUnavailableMessage(
        RectTransform row,
        MercenaryInstance mercenary)
    {
        string message = MercenaryClassProgression.IsBaseClass(
            mercenary.MercenaryClass)
            ? $"Lv{MercenaryClassProgression.PromotionLevel}で転職可能"
            : "転職済み";
        CreateText(
            row,
            message,
            RowFont,
            14,
            FontStyle.Normal,
            TextAnchor.MiddleRight,
            new Vector2(16f, -72f),
            new Vector2(-18f, -42f),
            MutedTextColor);
    }
}
