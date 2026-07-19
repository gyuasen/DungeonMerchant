using System.Collections.Generic;
using UnityEngine;

public static class EquipmentEffectTextFormatter
{
    public static string Format(EquipmentEffectDefinition effect)
    {
        if (effect == null)
        {
            return string.Empty;
        }

        switch (effect.type)
        {
            case EquipmentEffectType.BattleStartAttackBuff:
                return $"戦闘開始時、{Mathf.Max(0, effect.durationTurns)}ターン攻撃力+{effect.value * 100f:0.#}%";
            case EquipmentEffectType.BattleStartDefenseBuff:
                return $"戦闘開始時、{Mathf.Max(0, effect.durationTurns)}ターン防御力+{effect.value * 100f:0.#}%";
            case EquipmentEffectType.TurnRegeneration:
                return $"毎ターン最大HPの{effect.value * 100f:0.#}%回復";
            case EquipmentEffectType.DamageReduction:
                return $"被ダメージ-{effect.value * 100f:0.#}%";
            case EquipmentEffectType.LowHpDamageBonus:
                return $"HP{effect.secondaryValue * 100f:0.#}%以下で与ダメージ+{effect.value * 100f:0.#}%";
            case EquipmentEffectType.RaceDamageBonus:
                return effect.targetRace == EnemyRace.Unknown
                    ? string.Empty
                    : $"{JapaneseDisplayText.GetEnemyRace(effect.targetRace)}特攻：与ダメージ+{effect.value * 100f:0.#}%";
            default:
                return string.Empty;
        }
    }

    public static string FormatList(
        IEnumerable<EquipmentEffectDefinition> effects,
        string emptyText = "特殊効果なし")
    {
        if (effects == null)
        {
            return emptyText;
        }

        List<string> lines = new List<string>();
        foreach (EquipmentEffectDefinition effect in effects)
        {
            string text = Format(effect);
            if (!string.IsNullOrEmpty(text))
            {
                lines.Add(text);
            }
        }
        return lines.Count > 0 ? string.Join("\n", lines) : emptyText;
    }
}
