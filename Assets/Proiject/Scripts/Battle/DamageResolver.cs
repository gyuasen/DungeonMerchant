using UnityEngine;

public enum DamageType
{
    Physical,
    Magic,
    Pure,
    Periodic
}

public sealed class DamageRequest
{
    public int RawDamage { get; }
    public DamageType Type { get; }
    public bool IsCritical { get; }
    public BattleUnit Attacker { get; }
    public BattleUnit Target { get; }
    public bool ApplyRaceBonus { get; }
    public bool CheckEvasion { get; }

    public DamageRequest(
        int rawDamage,
        DamageType type,
        bool isCritical,
        BattleUnit attacker,
        BattleUnit target,
        bool applyRaceBonus = false,
        bool checkEvasion = false)
    {
        RawDamage = rawDamage;
        Type = type;
        IsCritical = isCritical;
        Attacker = attacker;
        Target = target;
        ApplyRaceBonus = applyRaceBonus;
        CheckEvasion = checkEvasion;
    }
}

public readonly struct DamageResult
{
    public readonly int FinalDamage;
    public readonly bool WasEvaded;
    public readonly bool WasDefeated;

    public DamageResult(int finalDamage, bool wasEvaded, bool wasDefeated)
    {
        FinalDamage = finalDamage;
        WasEvaded = wasEvaded;
        WasDefeated = wasDefeated;
    }
}

public static class DamageResolver
{
    private const int DefenseMitigationConstant = 100;

    public static DamageResult ResolveDamage(DamageRequest request)
    {
        if (request == null || request.Target == null || request.Target.IsDead)
        {
            return new DamageResult(0, false, request != null && request.Target != null && request.Target.IsDead);
        }

        if (request.CheckEvasion && request.Target.TryEvade())
        {
            return new DamageResult(0, true, false);
        }

        int finalDamage = PreviewDamage(request);
        request.Target.ApplyResolvedDamage(finalDamage);
        return new DamageResult(finalDamage, false, request.Target.IsDead);
    }

    public static int PreviewDamage(DamageRequest request)
    {
        if (request == null || request.Target == null)
        {
            return 0;
        }

        if (request.Type == DamageType.Pure || request.Type == DamageType.Periodic)
        {
            return Mathf.Max(1, request.RawDamage);
        }

        float rawDamage = request.RawDamage;
        if (request.ApplyRaceBonus && request.Attacker != null &&
            request.Target.Race != EnemyRace.Unknown)
        {
            rawDamage *= request.Attacker.GetRaceDamageMultiplier(request.Target.Race);
        }
        float mitigatedDamage = rawDamage * DefenseMitigationConstant /
                                (float)(DefenseMitigationConstant + request.Target.EffectiveDefense);
        mitigatedDamage *= request.Target.DamageTakenMultiplier;
        return Mathf.Max(1, Mathf.RoundToInt(mitigatedDamage));
    }
}
