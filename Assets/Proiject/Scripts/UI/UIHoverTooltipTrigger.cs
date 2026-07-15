using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIHoverTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    private const float TooltipWidth = 304f;
    private const float TooltipPadding = 10f;

    private BattleUnit unit;
    private EnemyDataSO enemyData;
    private Font font;
    private RectTransform tooltipParent;
    private RectTransform tooltipRect;
    private Text tooltipText;

    public void Configure(
        BattleUnit sourceUnit,
        EnemyDataSO sourceEnemyData,
        Font displayFont,
        RectTransform parent)
    {
        unit = sourceUnit;
        enemyData = sourceEnemyData;
        font = displayFont;
        tooltipParent = parent;
        CreateTooltip();
        Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unit == null || tooltipParent == null)
        {
            return;
        }

        CreateTooltip();
        tooltipText.text = BuildContent();
        tooltipRect.sizeDelta = new Vector2(
            TooltipWidth,
            unit.IsPlayerSide ? 176f : 198f);
        PositionTooltip(eventData);
        tooltipRect.gameObject.SetActive(true);
        tooltipRect.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    private void OnDisable()
    {
        Hide();
    }

    private void Update()
    {
        if (tooltipRect != null && tooltipRect.gameObject.activeSelf)
        {
            tooltipText.text = BuildContent();
        }
    }

    private void OnDestroy()
    {
        if (tooltipRect != null)
        {
            Destroy(tooltipRect.gameObject);
        }
    }

    private void CreateTooltip()
    {
        if (tooltipRect != null || tooltipParent == null)
        {
            return;
        }

        GameObject tooltip = new GameObject(
            "Unit Hover Tooltip",
            typeof(RectTransform),
            typeof(Image),
            typeof(Outline));
        tooltipRect = tooltip.GetComponent<RectTransform>();
        tooltipRect.SetParent(tooltipParent, false);
        tooltipRect.sizeDelta = new Vector2(TooltipWidth, 198f);

        Image background = tooltip.GetComponent<Image>();
        background.color = new Color(0.075f, 0.045f, 0.025f, 0.96f);
        background.raycastTarget = false;
        Outline outline = tooltip.GetComponent<Outline>();
        outline.effectColor = new Color(0.88f, 0.64f, 0.22f, 0.9f);
        outline.effectDistance = new Vector2(1f, -1f);

        GameObject content = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(Text));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.SetParent(tooltipRect, false);
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(9f, 7f);
        contentRect.offsetMax = new Vector2(-9f, -7f);

        tooltipText = content.GetComponent<Text>();
        tooltipText.font = font != null
            ? font
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipText.fontSize = 13;
        tooltipText.fontStyle = FontStyle.Normal;
        tooltipText.alignment = TextAnchor.UpperLeft;
        tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
        tooltipText.verticalOverflow = VerticalWrapMode.Overflow;
        tooltipText.color = new Color(0.97f, 0.9f, 0.74f);
        tooltipText.raycastTarget = false;
    }

    private string BuildContent()
    {
        string side = unit.IsPlayerSide
            ? JapaneseDisplayText.GetMercenaryClass(unit.MercenaryClass)
            : JapaneseDisplayText.GetMonsterGrade(enemyData);
        string status = JapaneseDisplayText.GetBattleStatus(unit.StatusEffect);
        if (unit.IsTaunting)
        {
            status = status == JapaneseDisplayText.GetBattleStatus(
                BattleStatusEffect.None)
                ? "挑発"
                : status + "・挑発";
        }

        string content =
            $"{unit.UnitName}\n" +
            $"{side}  Lv{unit.Level}\n" +
            $"HP {unit.CurrentHP}/{unit.MaxHP}  MP " +
            $"{unit.CurrentMagicPower}/{unit.MaxMagicPower}\n" +
            $"攻撃 {unit.Attack}  防御 {unit.Defense}\n" +
            $"速度 {unit.AttackSpeed:0.00}  会心 " +
            $"{unit.CriticalRate * 100f:0}%  回避 " +
            $"{unit.EvasionRate * 100f:0}%\n" +
            $"状態 {status}";

        if (!unit.IsPlayerSide)
        {
            EnemySkillType skill = enemyData != null &&
                enemyData.enemySkill != EnemySkillType.None
                ? enemyData.enemySkill
                : BattleSkillResolver.GetDefaultEnemySkill(
                    enemyData != null ? enemyData.monsterGrade : 1);
            content += "\n敵スキル " +
                BattleSkillResolver.GetEnemySkillDisplayName(skill);
        }

        return content;
    }

    private void PositionTooltip(PointerEventData eventData)
    {
        Camera eventCamera = eventData != null
            ? eventData.enterEventCamera
            : null;
        Vector2 pointerPosition = eventData != null
            ? eventData.position
            : Input.mousePosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipParent,
                pointerPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        Rect parentRect = tooltipParent.rect;
        bool isRightHalf = localPoint.x >= parentRect.center.x;
        bool isUpperHalf = localPoint.y >= parentRect.center.y;
        tooltipRect.pivot = new Vector2(
            isRightHalf ? 1f : 0f,
            isUpperHalf ? 1f : 0f);

        Vector2 offset = new Vector2(
            isRightHalf ? -16f : 16f,
            isUpperHalf ? -16f : 16f);
        Vector2 position = localPoint + offset;
        Vector2 size = tooltipRect.sizeDelta;
        position.x = Mathf.Clamp(
            position.x,
            parentRect.xMin + TooltipPadding + tooltipRect.pivot.x * size.x,
            parentRect.xMax - TooltipPadding -
                (1f - tooltipRect.pivot.x) * size.x);
        position.y = Mathf.Clamp(
            position.y,
            parentRect.yMin + TooltipPadding + tooltipRect.pivot.y * size.y,
            parentRect.yMax - TooltipPadding -
                (1f - tooltipRect.pivot.y) * size.y);
        tooltipRect.anchoredPosition = position;
    }

    private void Hide()
    {
        if (tooltipRect != null)
        {
            tooltipRect.gameObject.SetActive(false);
        }
    }
}
