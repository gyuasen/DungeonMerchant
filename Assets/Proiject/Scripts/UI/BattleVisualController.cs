using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleVisualController : MonoBehaviour
{
    private readonly Dictionary<BattleUnit, UnitSlot> slots =
        new Dictionary<BattleUnit, UnitSlot>();
    private readonly Queue<BattlePresentationEvent> eventQueue =
        new Queue<BattlePresentationEvent>();

    private BattleManager battleManager;
    private Font font;
    private Coroutine playbackRoutine;
    private Coroutine introRoutine;
    private Text resultText;
    private Text actionBanner;
    private bool introPending;
    private bool introIsBoss;

    public void Configure(BattleManager manager, Font displayFont)
    {
        Unsubscribe();
        battleManager = manager;
        font = displayFont;
        if (battleManager == null)
        {
            return;
        }

        battleManager.BattleVisualsPrepared += HandleRosterPrepared;
        battleManager.BattlePresentation += HandlePresentation;
    }

    public void MoveTo(RectTransform destination)
    {
        if (destination == null)
        {
            return;
        }

        RectTransform rect = (RectTransform)transform;
        rect.SetParent(destination, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsFirstSibling();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnEnable()
    {
        TryStartIntro();
    }

    private void Unsubscribe()
    {
        if (battleManager == null)
        {
            return;
        }
        battleManager.BattleVisualsPrepared -= HandleRosterPrepared;
        battleManager.BattlePresentation -= HandlePresentation;
    }

    private void HandleRosterPrepared(BattlePresentationRoster roster)
    {
        StopPlayback();
        slots.Clear();
        ClearChildren();
        transform.SetAsFirstSibling();

        Image background = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        background.sprite = null;
        background.raycastTarget = false;
        background.color = new Color(0.11f, 0.075f, 0.04f, 1f);

        Sprite backgroundSprite = ResolveBattleBackground(roster);
        if (backgroundSprite != null)
        {
            RectTransform backgroundRect =
                CreateRect("Battle Background", transform);
            Stretch(backgroundRect);
            Image backgroundImage =
                backgroundRect.gameObject.AddComponent<Image>();
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.preserveAspect = true;
            backgroundImage.raycastTarget = false;
            AspectRatioFitter backgroundFitter =
                backgroundRect.gameObject.AddComponent<AspectRatioFitter>();
            backgroundFitter.aspectMode =
                AspectRatioFitter.AspectMode.EnvelopeParent;
            backgroundFitter.aspectRatio =
                backgroundSprite.rect.width /
                Mathf.Max(1f, backgroundSprite.rect.height);
        }

        RectTransform shade = CreateRect("Battlefield Shade", transform);
        Stretch(shade);
        Image shadeImage = shade.gameObject.AddComponent<Image>();
        shadeImage.color = new Color(0.04f, 0.025f, 0.015f, 0.25f);
        shadeImage.raycastTarget = false;

        RectTransform headerShade = CreateRect("Header Shade", transform);
        headerShade.anchorMin = new Vector2(0f, 0.78f);
        headerShade.anchorMax = Vector2.one;
        headerShade.offsetMin = headerShade.offsetMax = Vector2.zero;
        Image headerImage = headerShade.gameObject.AddComponent<Image>();
        headerImage.color = new Color(0.04f, 0.025f, 0.015f, 0.6f);
        headerImage.raycastTarget = false;

        CreateSide(
            roster.Enemies,
            false,
            new Vector2(0f, 0.48f),
            new Vector2(1f, 0.79f));
        CreateSide(
            roster.Players,
            true,
            new Vector2(0f, 0.24f),
            new Vector2(1f, 0.50f));

        actionBanner = CreateText(
            "Action Banner",
            (RectTransform)transform,
            string.Empty,
            24,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        actionBanner.rectTransform.anchorMin = new Vector2(0.2f, 0.45f);
        actionBanner.rectTransform.anchorMax = new Vector2(0.8f, 0.58f);
        actionBanner.rectTransform.offsetMin = Vector2.zero;
        actionBanner.rectTransform.offsetMax = Vector2.zero;
        Outline bannerOutline = actionBanner.gameObject.AddComponent<Outline>();
        bannerOutline.effectColor = new Color(0.08f, 0.03f, 0f, 0.95f);
        bannerOutline.effectDistance = new Vector2(2f, -2f);
        actionBanner.gameObject.SetActive(false);

        resultText = CreateText(
            "Battle Result",
            (RectTransform)transform,
            string.Empty,
            28,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        Stretch(resultText.rectTransform);
        resultText.gameObject.SetActive(false);

        introPending = true;
        introIsBoss = HasBoss(roster);
        TryStartIntro();
    }

    private void CreateSide(
        IReadOnlyList<BattleVisualUnitDescriptor> descriptors,
        bool playerSide,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return;
        }

        RectTransform side = CreateRect(
            playerSide ? "Player Visuals" : "Enemy Visuals",
            (RectTransform)transform);
        side.anchorMin = anchorMin;
        side.anchorMax = anchorMax;
        side.offsetMin = new Vector2(12f, 4f);
        side.offsetMax = new Vector2(-12f, -4f);

        for (int i = 0; i < descriptors.Count; i++)
        {
            BattleVisualUnitDescriptor descriptor = descriptors[i];
            float left = i / (float)descriptors.Count;
            float right = (i + 1) / (float)descriptors.Count;
            UnitSlot slot = CreateSlot(
                descriptor,
                side,
                new Vector2(left, 0f),
                new Vector2(right, 1f));
            slots[descriptor.Unit] = slot;
        }
    }

    private UnitSlot CreateSlot(
        BattleVisualUnitDescriptor descriptor,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform panel = CreateRect(descriptor.Unit.UnitName, parent);
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
        panel.offsetMin = new Vector2(4f, 3f);
        panel.offsetMax = new Vector2(-4f, -3f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = descriptor.EnemyData != null &&
                           descriptor.EnemyData.isSpecialVariant
            ? new Color(0.28f, 0.12f, 0.34f, 0.68f)
            : new Color(0.16f, 0.10f, 0.055f, 0.6f);
        Outline targetOutline = panel.gameObject.AddComponent<Outline>();
        targetOutline.effectColor = new Color(1f, 0.72f, 0.2f, 0.95f);
        targetOutline.effectDistance = new Vector2(3f, -3f);
        targetOutline.enabled = false;
        CanvasGroup canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        RectTransform portrait = CreateRect("Portrait", panel);
        portrait.anchorMin = new Vector2(0.08f, 0.28f);
        portrait.anchorMax = new Vector2(0.92f, 0.96f);
        portrait.offsetMin = portrait.offsetMax = Vector2.zero;
        AspectRatioFitter portraitFitter =
            portrait.gameObject.AddComponent<AspectRatioFitter>();
        portraitFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        portraitFitter.aspectRatio = 1f;

        bool hasImage;
        if (descriptor.IsPlayerSide)
        {
            RawImage rawImage = portrait.gameObject.AddComponent<RawImage>();
            rawImage.color = Color.white;
            hasImage = MercenaryPortraitProvider.TryApply(
                rawImage,
                descriptor.MercenaryClass);
        }
        else
        {
            Image image = portrait.gameObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.sprite = ResolveEnemySprite(descriptor.EnemyData);
            hasImage = image.sprite != null;
            image.color = hasImage
                ? Color.white
                : new Color(0.26f, 0.20f, 0.14f, 1f);
        }

        Text fallback = CreateText(
            "Placeholder",
            portrait,
            GetPlaceholderLabel(descriptor),
            30,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        Stretch(fallback.rectTransform);
        fallback.color = descriptor.EnemyData != null &&
                         descriptor.EnemyData.isSpecialVariant
            ? new Color(0.93f, 0.55f, 1f)
            : new Color(0.95f, 0.84f, 0.62f);
        fallback.gameObject.SetActive(!hasImage);

        Text name = CreateText(
            "Name",
            panel,
            descriptor.Unit.UnitName,
            12,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        name.supportRichText = true;
        name.rectTransform.anchorMin = new Vector2(0.02f, 0.17f);
        name.rectTransform.anchorMax = new Vector2(0.98f, 0.30f);
        name.rectTransform.offsetMin = name.rectTransform.offsetMax = Vector2.zero;

        RectTransform hpBackground = CreateRect("HP Background", panel);
        hpBackground.anchorMin = new Vector2(0.08f, 0.09f);
        hpBackground.anchorMax = new Vector2(0.92f, 0.15f);
        hpBackground.offsetMin = hpBackground.offsetMax = Vector2.zero;
        hpBackground.gameObject.AddComponent<Image>().color =
            new Color(0.08f, 0.04f, 0.02f, 0.95f);

        RectTransform hpFillRect = CreateRect("HP Fill", hpBackground);
        Stretch(hpFillRect);
        Image hpFill = hpFillRect.gameObject.AddComponent<Image>();
        hpFill.color = new Color(0.34f, 0.75f, 0.28f, 1f);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;

        Text hpText = CreateText(
            "HP Text",
            panel,
            string.Empty,
            10,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        hpText.rectTransform.anchorMin = new Vector2(0.02f, 0f);
        hpText.rectTransform.anchorMax = new Vector2(0.98f, 0.09f);
        hpText.rectTransform.offsetMin = hpText.rectTransform.offsetMax = Vector2.zero;

        Text status = CreateText(
            "Status",
            panel,
            string.Empty,
            11,
            FontStyle.Bold,
            TextAnchor.UpperRight);
        status.rectTransform.anchorMin = new Vector2(0.55f, 0.72f);
        status.rectTransform.anchorMax = new Vector2(0.96f, 0.94f);
        status.rectTransform.offsetMin = status.rectTransform.offsetMax = Vector2.zero;
        status.color = new Color(1f, 0.78f, 0.3f);

        UnitSlot slot = new UnitSlot(
            panel,
            portrait,
            hpFill,
            hpText,
            status,
            canvasGroup,
            targetOutline);
        slot.SetHP(descriptor.Unit.CurrentHP, descriptor.Unit.MaxHP);
        return slot;
    }

    private void HandlePresentation(BattlePresentationEvent presentationEvent)
    {
        if (presentationEvent == null)
        {
            return;
        }

        if (presentationEvent.Type == BattlePresentationEventType.SkipRequested)
        {
            StopPlayback();
            eventQueue.Clear();
            return;
        }

        if (battleManager != null && battleManager.IsSkippingToBattleEnd)
        {
            ApplyImmediately(presentationEvent);
            return;
        }

        if (!isActiveAndEnabled)
        {
            ApplyImmediately(presentationEvent);
            return;
        }

        eventQueue.Enqueue(presentationEvent);
        if (playbackRoutine == null)
        {
            playbackRoutine = StartCoroutine(PlayQueue());
        }
    }

    private IEnumerator PlayQueue()
    {
        if (introRoutine != null)
        {
            yield return introRoutine;
        }
        while (eventQueue.Count > 0)
        {
            BattlePresentationEvent presentationEvent = eventQueue.Dequeue();
            yield return PlayEvent(presentationEvent);
        }
        playbackRoutine = null;
    }

    private IEnumerator PlayEvent(BattlePresentationEvent presentationEvent)
    {
        switch (presentationEvent.Type)
        {
            case BattlePresentationEventType.Action:
                if (presentationEvent.ActionKind ==
                        BattlePresentationActionKind.Skill &&
                    !string.IsNullOrWhiteSpace(presentationEvent.ActionLabel))
                {
                    yield return ShowActionBanner(presentationEvent.ActionLabel);
                }
                yield return AnimateAction(
                    presentationEvent.Actor,
                    presentationEvent.Target);
                break;
            case BattlePresentationEventType.Damage:
                yield return AnimateHP(presentationEvent);
                yield return AnimateImpact(
                    presentationEvent.Target,
                    presentationEvent.IsCritical
                        ? new Color(1f, 0.78f, 0.15f)
                        : new Color(1f, 0.3f, 0.2f),
                    presentationEvent.IsCritical
                        ? $"CRITICAL {presentationEvent.Amount}"
                        : $"-{presentationEvent.Amount}",
                    presentationEvent.IsCritical);
                break;
            case BattlePresentationEventType.Heal:
                yield return AnimateHP(presentationEvent);
                yield return AnimateImpact(
                    presentationEvent.Target,
                    new Color(0.3f, 1f, 0.45f),
                    $"+{presentationEvent.Amount}",
                    false);
                break;
            case BattlePresentationEventType.Evade:
                yield return AnimateEvade(presentationEvent.Target);
                break;
            case BattlePresentationEventType.Status:
                SetStatus(presentationEvent.Target, presentationEvent.StatusEffect);
                yield return Wait(0.12f);
                break;
            case BattlePresentationEventType.Defeated:
                UpdateHP(presentationEvent);
                yield return AnimateDefeat(presentationEvent.Target);
                break;
            case BattlePresentationEventType.BattleCompleted:
                ShowResult(presentationEvent.Victory);
                yield return Wait(0.25f);
                break;
        }
    }

    private IEnumerator AnimateAction(BattleUnit actor, BattleUnit target)
    {
        if (!slots.TryGetValue(actor, out UnitSlot slot))
        {
            yield break;
        }

        UnitSlot targetSlot = null;
        if (target != null)
        {
            slots.TryGetValue(target, out targetSlot);
        }
        if (targetSlot != null)
        {
            targetSlot.TargetOutline.enabled = true;
        }

        Vector2 start = slot.Panel.anchoredPosition;
        Vector2 direction = actor.IsPlayerSide
            ? Vector2.up
            : Vector2.down;
        float duration = Duration(0.12f);
        yield return Tween(duration, value =>
        {
            float arc = Mathf.Sin(value * Mathf.PI);
            slot.Panel.anchoredPosition = start + direction * (18f * arc);
            slot.Panel.localScale = Vector3.one * (1f + 0.08f * arc);
        });
        slot.Panel.anchoredPosition = start;
        slot.Panel.localScale = Vector3.one;
        if (targetSlot != null)
        {
            targetSlot.TargetOutline.enabled = false;
        }
    }

    private IEnumerator AnimateImpact(
        BattleUnit target,
        Color color,
        string amountLabel,
        bool critical)
    {
        if (!slots.TryGetValue(target, out UnitSlot slot))
        {
            yield break;
        }

        Text popup = CreatePopup(slot.Panel, amountLabel, color);
        Vector2 start = slot.Panel.anchoredPosition;
        RectTransform battlefield = (RectTransform)transform;
        Vector2 battlefieldStart = battlefield.anchoredPosition;
        float duration = Duration(0.18f);
        yield return Tween(duration, value =>
        {
            float shake = Mathf.Sin(value * Mathf.PI * 6f) * (1f - value) * 5f;
            slot.Panel.anchoredPosition = start + Vector2.right * shake;
            slot.Group.alpha = 0.55f + Mathf.Abs(Mathf.Sin(value * Mathf.PI * 3f)) * 0.45f;
            popup.rectTransform.anchoredPosition = new Vector2(0f, value * 24f);
            if (critical)
            {
                float screenShake = Mathf.Sin(value * Mathf.PI * 10f) *
                                    (1f - value) * 7f;
                battlefield.anchoredPosition = battlefieldStart +
                                               Vector2.right * screenShake;
            }
        });
        slot.Panel.anchoredPosition = start;
        slot.Group.alpha = 1f;
        battlefield.anchoredPosition = battlefieldStart;
        Destroy(popup.gameObject);
    }

    private IEnumerator AnimateHP(BattlePresentationEvent presentationEvent)
    {
        if (!slots.TryGetValue(presentationEvent.Target, out UnitSlot slot))
        {
            yield break;
        }

        int start = slot.CurrentHP;
        int target = presentationEvent.CurrentHP;
        int maximum = Mathf.Max(1, presentationEvent.MaxHP);
        yield return Tween(Duration(0.14f), value =>
        {
            int current = Mathf.RoundToInt(Mathf.Lerp(start, target, value));
            slot.SetHP(current, maximum);
        });
    }

    private IEnumerator ShowActionBanner(string label)
    {
        if (actionBanner == null)
        {
            yield break;
        }

        actionBanner.text = label;
        actionBanner.color = new Color(1f, 0.86f, 0.38f);
        actionBanner.gameObject.SetActive(true);
        actionBanner.transform.SetAsLastSibling();
        actionBanner.rectTransform.localScale = Vector3.one * 0.85f;
        yield return Tween(Duration(0.12f), value =>
        {
            actionBanner.rectTransform.localScale = Vector3.one *
                Mathf.Lerp(0.85f, 1f, value);
        });
        yield return Wait(0.12f);
        actionBanner.gameObject.SetActive(false);
    }

    private IEnumerator AnimateEvade(BattleUnit target)
    {
        if (!slots.TryGetValue(target, out UnitSlot slot))
        {
            yield break;
        }
        Text popup = CreatePopup(slot.Panel, "回避", new Color(0.4f, 0.85f, 1f));
        Vector2 start = slot.Panel.anchoredPosition;
        yield return Tween(Duration(0.16f), value =>
        {
            slot.Panel.anchoredPosition = start +
                Vector2.right * (Mathf.Sin(value * Mathf.PI) * 18f);
        });
        slot.Panel.anchoredPosition = start;
        Destroy(popup.gameObject);
    }

    private IEnumerator AnimateDefeat(BattleUnit target)
    {
        if (!slots.TryGetValue(target, out UnitSlot slot))
        {
            yield break;
        }
        yield return Tween(Duration(0.22f), value =>
        {
            slot.Group.alpha = Mathf.Lerp(1f, 0.25f, value);
            slot.Portrait.localScale = Vector3.one * Mathf.Lerp(1f, 0.75f, value);
        });
    }

    private void ApplyImmediately(BattlePresentationEvent presentationEvent)
    {
        if (presentationEvent.Type == BattlePresentationEventType.Damage ||
            presentationEvent.Type == BattlePresentationEventType.Heal ||
            presentationEvent.Type == BattlePresentationEventType.Defeated)
        {
            UpdateHP(presentationEvent);
        }
        if (presentationEvent.Type == BattlePresentationEventType.Status)
        {
            SetStatus(presentationEvent.Target, presentationEvent.StatusEffect);
        }
        if (presentationEvent.Type == BattlePresentationEventType.Defeated &&
            slots.TryGetValue(presentationEvent.Target, out UnitSlot defeated))
        {
            defeated.Group.alpha = 0.25f;
        }
        if (presentationEvent.Type == BattlePresentationEventType.BattleCompleted)
        {
            ShowResult(presentationEvent.Victory);
        }
    }

    private void UpdateHP(BattlePresentationEvent presentationEvent)
    {
        if (slots.TryGetValue(presentationEvent.Target, out UnitSlot slot))
        {
            slot.SetHP(presentationEvent.CurrentHP, presentationEvent.MaxHP);
        }
    }

    private void SetStatus(BattleUnit target, BattleStatusEffect effect)
    {
        if (!slots.TryGetValue(target, out UnitSlot slot))
        {
            return;
        }
        slot.Status.text = effect == BattleStatusEffect.Poison
            ? "毒"
            : effect == BattleStatusEffect.Paralysis
                ? "麻痺"
                : string.Empty;
    }

    private void ShowResult(bool victory)
    {
        if (resultText == null)
        {
            return;
        }
        resultText.text = victory ? "勝利" : "敗北";
        resultText.color = victory
            ? new Color(1f, 0.86f, 0.38f)
            : new Color(1f, 0.38f, 0.32f);
        resultText.gameObject.SetActive(true);
        resultText.transform.SetAsLastSibling();
    }

    private Sprite ResolveEnemySprite(EnemyDataSO data)
    {
        if (data == null)
        {
            return null;
        }
        if (data.battleSprite != null)
        {
            return data.battleSprite;
        }

        string key = string.IsNullOrWhiteSpace(data.battleVisualKey)
            ? data.name
            : data.battleVisualKey.Trim();
        return Resources.Load<Sprite>($"Battle/Enemies/{key}");
    }

    private static Sprite ResolveBattleBackground(
        BattlePresentationRoster roster)
    {
        if (roster == null)
        {
            return Resources.Load<Sprite>("Battle/Backgrounds/Default");
        }
        if (roster.BackgroundSprite != null)
        {
            return roster.BackgroundSprite;
        }

        string key = string.IsNullOrWhiteSpace(roster.BackgroundKey)
            ? "Default"
            : roster.BackgroundKey.Trim();
        Sprite background = Resources.Load<Sprite>(
            $"Battle/Backgrounds/{key}");
        return background != null
            ? background
            : Resources.Load<Sprite>("Battle/Backgrounds/Default");
    }

    private static string GetPlaceholderLabel(BattleVisualUnitDescriptor descriptor)
    {
        if (descriptor.IsPlayerSide)
        {
            return JapaneseDisplayText.GetMercenaryClass(
                descriptor.MercenaryClass);
        }
        return descriptor.EnemyData != null && descriptor.EnemyData.isBoss
            ? "BOSS"
            : "ENEMY";
    }

    private Text CreatePopup(RectTransform parent, string content, Color color)
    {
        Text popup = CreateText(
            "Battle Popup",
            parent,
            content,
            20,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        popup.rectTransform.anchorMin = popup.rectTransform.anchorMax =
            new Vector2(0.5f, 0.55f);
        popup.rectTransform.sizeDelta = new Vector2(150f, 36f);
        popup.rectTransform.anchoredPosition = Vector2.zero;
        popup.color = color;
        popup.transform.SetAsLastSibling();
        return popup;
    }

    private Text CreateText(
        string objectName,
        RectTransform parent,
        string content,
        int fontSize,
        FontStyle style,
        TextAnchor alignment)
    {
        RectTransform rect = CreateRect(objectName, parent);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font != null
            ? font
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = new Color(0.96f, 0.89f, 0.72f);
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject instance = new GameObject(
            objectName,
            typeof(RectTransform));
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void StopPlayback()
    {
        if (playbackRoutine != null)
        {
            StopCoroutine(playbackRoutine);
            playbackRoutine = null;
        }
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }
        eventQueue.Clear();
    }

    private void TryStartIntro()
    {
        if (!introPending || !isActiveAndEnabled || resultText == null ||
            introRoutine != null)
        {
            return;
        }
        introRoutine = StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        introPending = false;
        resultText.text = introIsBoss ? "BOSS ENCOUNTER" : "ENCOUNTER";
        resultText.color = introIsBoss
            ? new Color(1f, 0.38f, 0.28f)
            : new Color(1f, 0.86f, 0.38f);
        resultText.gameObject.SetActive(true);
        resultText.transform.SetAsLastSibling();
        resultText.rectTransform.localScale = Vector3.one * 0.8f;
        yield return Tween(Duration(0.18f), value =>
        {
            resultText.rectTransform.localScale = Vector3.one *
                Mathf.Lerp(0.8f, 1f, value);
        });
        yield return Wait(0.25f);
        resultText.gameObject.SetActive(false);
        resultText.rectTransform.localScale = Vector3.one;
        introRoutine = null;
    }

    private static bool HasBoss(BattlePresentationRoster roster)
    {
        if (roster?.Enemies == null)
        {
            return false;
        }
        foreach (BattleVisualUnitDescriptor enemy in roster.Enemies)
        {
            if (enemy?.EnemyData != null && enemy.EnemyData.isBoss)
            {
                return true;
            }
        }
        return false;
    }

    private float Duration(float baseDuration)
    {
        float speed = battleManager != null
            ? Mathf.Max(1f, battleManager.BattleSpeedMultiplier)
            : 1f;
        return baseDuration / speed;
    }

    private IEnumerator Wait(float baseDuration)
    {
        float remaining = Duration(baseDuration);
        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private static IEnumerator Tween(
        float duration,
        System.Action<float> update)
    {
        if (duration <= 0f)
        {
            update(1f);
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            update(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        update(1f);
    }

    private sealed class UnitSlot
    {
        public RectTransform Panel { get; }
        public RectTransform Portrait { get; }
        public Image HPFill { get; }
        public Text HPText { get; }
        public Text Status { get; }
        public CanvasGroup Group { get; }
        public Outline TargetOutline { get; }
        public int CurrentHP { get; private set; }

        public UnitSlot(
            RectTransform panel,
            RectTransform portrait,
            Image hpFill,
            Text hpText,
            Text status,
            CanvasGroup group,
            Outline targetOutline)
        {
            Panel = panel;
            Portrait = portrait;
            HPFill = hpFill;
            HPText = hpText;
            Status = status;
            Group = group;
            TargetOutline = targetOutline;
        }

        public void SetHP(int current, int maximum)
        {
            int safeMaximum = Mathf.Max(1, maximum);
            int safeCurrent = Mathf.Clamp(current, 0, safeMaximum);
            CurrentHP = safeCurrent;
            HPFill.fillAmount = safeCurrent / (float)safeMaximum;
            HPFill.color = HPFill.fillAmount <= 0.25f
                ? new Color(0.9f, 0.2f, 0.15f)
                : HPFill.fillAmount <= 0.5f
                    ? new Color(0.95f, 0.65f, 0.15f)
                    : new Color(0.34f, 0.75f, 0.28f);
            HPText.text = $"HP {safeCurrent}/{safeMaximum}";
        }
    }
}
