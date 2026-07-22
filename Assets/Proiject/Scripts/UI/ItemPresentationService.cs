using System.Text;
using UnityEngine;

public static class ItemPresentationService
{
    public static Sprite ResolveSprite(ItemDataSO item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.name))
        {
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>(
            "UI/Codex/Equipment/" + item.name);
        return sprite != null
            ? sprite
            : Resources.Load<Sprite>("UI/Items/" + item.name);
    }

    public static string BuildDetailText(ItemDataSO item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        if (item.IsEquipment)
        {
            return BuildEquipmentDetailText(item);
        }

        return item.itemType == ItemType.Consumable
            ? BuildConsumableDetailText(item)
            : BuildMaterialDetailText(item);
    }

    private static string BuildEquipmentDetailText(ItemDataSO item)
    {
        string equipClass = item.allClassesCanEquip
            ? "全職業"
            : JapaneseDisplayText.GetMercenaryClass(item.requiredClass);
        string setText = EquipmentSetCatalog.TryGet(
            item.equipmentSet,
            out EquipmentSetDefinition set)
            ? "セット: " + set.DisplayName
            : "セット: なし";
        StringBuilder result = new StringBuilder();
        result.AppendLine(EquipmentRankPresentation.GetRichText(item));
        result.AppendLine("部位: " +
            JapaneseDisplayText.GetEquipmentSlot(item.equipmentSlot));
        result.AppendLine("装備可能: " + equipClass);
        result.AppendLine($"HP {item.bonusMaxHP:+#;-#;0}  攻 {item.bonusAttack:+#;-#;0}");
        result.AppendLine($"防 {item.bonusDefense:+#;-#;0}  速 {item.bonusAttackSpeed:+0.##;-0.##;0}");
        result.AppendLine("特殊効果: " +
            EquipmentEffectTextFormatter.FormatList(item.equipmentEffects));
        result.AppendLine(setText);
        AppendDescriptionAndPrice(result, item);
        return result.ToString();
    }

    private static string BuildConsumableDetailText(ItemDataSO item)
    {
        StringBuilder result = new StringBuilder();
        result.AppendLine("効果: " + GetConsumableEffectText(item));
        AppendDescriptionAndPrice(result, item);
        return result.ToString();
    }

    private static string BuildMaterialDetailText(ItemDataSO item)
    {
        StringBuilder result = new StringBuilder();
        result.AppendLine("素材分類: " + item.materialClassification);
        AppendDescriptionAndPrice(result, item);
        return result.ToString();
    }

    private static void AppendDescriptionAndPrice(
        StringBuilder result,
        ItemDataSO item)
    {
        if (!string.IsNullOrWhiteSpace(item.description))
        {
            result.AppendLine(item.description);
        }

        result.Append("基本価格: ").Append(item.basePrice).Append("G");
    }

    private static string GetConsumableEffectText(ItemDataSO item)
    {
        switch (item.consumableEffect)
        {
            case ConsumableEffectType.HealHP:
                return "HP回復 " + item.consumableHealAmount;
            case ConsumableEffectType.CurePoison:
                return "毒を治療";
            case ConsumableEffectType.CureParalysis:
                return "麻痺を治療";
            case ConsumableEffectType.CureAllStatus:
                return "状態異常を治療";
            case ConsumableEffectType.BoostAttack:
                return "攻撃力上昇";
            case ConsumableEffectType.BoostDefense:
                return "防御力上昇";
            case ConsumableEffectType.RestoreMagic:
                return "魔力回復 " + item.consumableHealAmount;
            case ConsumableEffectType.BoostSpeed:
                return "速度上昇";
            default:
                return "なし";
        }
    }
}
