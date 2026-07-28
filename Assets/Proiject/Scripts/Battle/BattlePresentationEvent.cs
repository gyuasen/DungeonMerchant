using System.Collections.Generic;

public enum BattlePresentationEventType
{
    Action,
    Damage,
    Heal,
    Evade,
    Status,
    Defeated,
    BattleCompleted,
    Reward,
    Log,
    PresentationComplete,
    SkipRequested
}

public enum BattlePresentationActionKind
{
    NormalAttack,
    Skill,
    StatusEffect
}

public enum BattleSoundCue
{
    None,
    Attack,
    Impact,
    Evade,
    Heal,
    Defeat,
    Skill,
    Victory,
    Loss,
    Reward
}

public sealed class BattleVisualUnitDescriptor
{
    public BattleUnit Unit { get; }
    public EnemyDataSO EnemyData { get; }
    public MercenaryClass MercenaryClass { get; }
    public bool IsPlayerSide => Unit != null && Unit.IsPlayerSide;

    public BattleVisualUnitDescriptor(
        BattleUnit unit,
        EnemyDataSO enemyData,
        MercenaryClass mercenaryClass)
    {
        Unit = unit;
        EnemyData = enemyData;
        MercenaryClass = mercenaryClass;
    }
}

public sealed class BattlePresentationRoster
{
    public IReadOnlyList<BattleVisualUnitDescriptor> Players { get; }
    public IReadOnlyList<BattleVisualUnitDescriptor> Enemies { get; }
    public UnityEngine.Sprite BackgroundSprite { get; }
    public string BackgroundKey { get; }

    public BattlePresentationRoster(
        IReadOnlyList<BattleVisualUnitDescriptor> players,
        IReadOnlyList<BattleVisualUnitDescriptor> enemies,
        UnityEngine.Sprite backgroundSprite = null,
        string backgroundKey = null)
    {
        Players = players;
        Enemies = enemies;
        BackgroundSprite = backgroundSprite;
        BackgroundKey = backgroundKey;
    }
}

public sealed class BattlePresentationEvent
{
    public BattlePresentationEventType Type { get; }
    public BattlePresentationActionKind ActionKind { get; }
    public BattleUnit Actor { get; }
    public BattleUnit Target { get; }
    public int Amount { get; }
    public int CurrentHP { get; }
    public int MaxHP { get; }
    public bool IsCritical { get; }
    public BattleStatusEffect StatusEffect { get; }
    public bool Victory { get; }
    public string ActionLabel { get; }
    public string LogMessage { get; }
    public BattleLogType LogType { get; }
    public BattleSoundCue SoundCue { get; }

    public BattlePresentationEvent(
        BattlePresentationEventType type,
        BattleUnit actor = null,
        BattleUnit target = null,
        int amount = 0,
        int currentHP = 0,
        int maxHP = 0,
        bool isCritical = false,
        BattleStatusEffect statusEffect = BattleStatusEffect.None,
        bool victory = false,
        BattlePresentationActionKind actionKind =
            BattlePresentationActionKind.NormalAttack,
        string actionLabel = null,
        string logMessage = null,
        BattleLogType logType = BattleLogType.System,
        BattleSoundCue soundCue = BattleSoundCue.None)
    {
        Type = type;
        Actor = actor;
        Target = target;
        Amount = amount;
        CurrentHP = currentHP;
        MaxHP = maxHP;
        IsCritical = isCritical;
        StatusEffect = statusEffect;
        Victory = victory;
        ActionKind = actionKind;
        ActionLabel = actionLabel;
        LogMessage = logMessage;
        LogType = logType;
        SoundCue = soundCue;
    }
}
