using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class EquipmentSetCatalogTests
{
    [Test]
    public void Catalog_DefinesEveryNonNoneSetWithPinnedBonuses()
    {
        Assert.That(EquipmentSetCatalog.Definitions.Count, Is.EqualTo(20));
        foreach (EquipmentSetId setId in System.Enum.GetValues(typeof(EquipmentSetId)))
        {
            if (setId == EquipmentSetId.None)
            {
                continue;
            }
            Assert.That(EquipmentSetCatalog.TryGet(setId, out EquipmentSetDefinition definition), Is.True);
            Assert.That(definition.TwoPiece.RequiredCount, Is.EqualTo(2));
            Assert.That(definition.ThreePiece.RequiredCount, Is.EqualTo(3));
        }
        AssertTier(EquipmentSetId.AncientGuardian, 30, 0, 8, 0f, 0, 12, 0, .05f);
        AssertTier(EquipmentSetId.Vanguard, 20, 0, 10, 0f, 0, 8, 0, 0f);
        AssertTier(EquipmentSetId.Windstalker, 0, 4, 0, .05f, 0, 7, 0, .04f);
        AssertTier(EquipmentSetId.ArcaneSage, 0, 8, 0, 0f, 0, 10, 0, .04f);
        AssertTier(EquipmentSetId.OniHunter, 10, 3, 0, 0f, 0, 5, 2, 0f);
        AssertTier(EquipmentSetId.NornCanopy, 15, 0, 0, 0f, 0, 2, 2, .01f);
        AssertTier(EquipmentSetId.GlaadSkyFortress, 15, 0, 0, 0f, 0, 2, 2, .01f);
        AssertTier(EquipmentSetId.VelmBlackIron, 20, 0, 0, 0f, 0, 3, 2, .015f);
        AssertTier(EquipmentSetId.AbyssThrone, 25, 0, 0, 0f, 0, 3, 3, .02f);
        AssertTier(EquipmentSetId.AstralDepths, 45, 0, 0, .04f, 0, 10, 10, 0f);
        AssertTier(EquipmentSetId.NornVerdantSettlement, 15, 0, 0, 0f, 0, 2, 2, .01f);
        AssertTier(EquipmentSetId.GlaadDragonScaleCanyon, 15, 0, 0, 0f, 0, 2, 2, .01f);
        AssertTier(EquipmentSetId.VelmFurnaceDefenseZone, 20, 0, 0, 0f, 0, 3, 2, .015f);
        AssertTier(EquipmentSetId.AbyssGatewayThreshold, 25, 0, 0, 0f, 0, 3, 3, .02f);
        AssertTier(EquipmentSetId.StartingCave, 5, 0, 0, 0f, 0, 1, 1, 0f);
        AssertTier(EquipmentSetId.LeafForestTrail, 5, 0, 0, 0f, 0, 1, 1, 0f);
        AssertTier(EquipmentSetId.EldUndergroundWaterway, 5, 0, 0, 0f, 0, 1, 1, 0f);
        AssertTier(EquipmentSetId.LowerMine, 10, 0, 0, 0f, 0, 2, 1, 0f);
        AssertTier(EquipmentSetId.EldOldQuarry, 10, 0, 0, 0f, 0, 2, 1, 0f);
        AssertTier(EquipmentSetId.MiddleRuins, 10, 0, 0, 0f, 0, 2, 1, 0f);
    }

    [Test]
    public void Catalog_UndefinedSetHasNoBonusOrDisplay()
    {
        EquipmentSetTier bonus = EquipmentSetCatalog.GetBonus((EquipmentSetId)999, 3);
        Assert.That(bonus.BonusMaxHP, Is.EqualTo(0));
        Assert.That(bonus.BonusAttack, Is.EqualTo(0));
        Assert.That(bonus.BonusDefense, Is.EqualTo(0));
        Assert.That(bonus.BonusAttackSpeed, Is.EqualTo(0f));
        Assert.That(EquipmentSetCatalog.BuildDetailText((EquipmentSetId)999), Is.EqualTo("セット効果: なし"));
    }

    [Test]
    public void Catalog_BuildsEquipmentSetDetailFromPinnedTierValues()
    {
        Assert.That(
            EquipmentSetCatalog.BuildDetailText(EquipmentSetId.Vanguard),
            Is.EqualTo("セット: 不屈の前衛\n2部位: 最大HP+20、防御+10\n3部位: 攻撃+8"));
        Assert.That(
            EquipmentSetCatalog.BuildDetailText(EquipmentSetId.AncientGuardian),
            Is.EqualTo("セット: 古代守護者\n2部位: 最大HP+30、防御+8\n3部位: 攻撃+12、攻撃速度+0.05"));
    }

    [Test]
    public void CodexBuilder_ClassifiesEquipmentWithoutOverlapAndSortsSetGroups()
    {
        List<ItemDataSO> items = new List<ItemDataSO>
        {
            CreateEquipment(EquipmentSetId.AbyssThrone, 9),
            CreateEquipment(EquipmentSetId.Vanguard, 3),
            CreateEquipment(EquipmentSetId.None, 9),
            CreateEquipment(EquipmentSetId.None, 8)
        };
        EquipmentCodexEntries entries = EquipmentCodexEntryBuilder.Build(items);
        List<ItemDataSO> classified = entries.NormalEquipment.Concat(entries.HighRankSingleEquipment).Concat(entries.SetGroups.SelectMany(group => group.Equipment)).ToList();
        Assert.That(entries.NormalEquipment.Count(), Is.EqualTo(1));
        Assert.That(entries.HighRankSingleEquipment.Count(), Is.EqualTo(1));
        Assert.That(entries.SetGroups.Count(), Is.EqualTo(2));
        Assert.That(entries.SetGroups[0].SetId, Is.EqualTo(EquipmentSetId.Vanguard));
        Assert.That(entries.SetGroups[1].SetId, Is.EqualTo(EquipmentSetId.AbyssThrone));
        Assert.That(entries.HighRankSingleEquipment.Contains(items[0]), Is.False);
        Assert.That(classified.Distinct().Count(), Is.EqualTo(items.Count));
        Assert.That(classified, Is.EquivalentTo(items));
        foreach (ItemDataSO item in items)
        {
            Object.DestroyImmediate(item);
        }
    }

    [Test]
    public void SpecialPageModel_BuildsSetSlotsAndHighRankSinglePages()
    {
        List<ItemDataSO> items = new List<ItemDataSO>
        {
            CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Weapon),
            CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Weapon),
            CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Armor),
            CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Armor),
            CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Accessory),
            CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Accessory),
            CreateEquipment(EquipmentSetId.None, 9, EquipmentSlot.Weapon)
        };
        EquipmentCodexEntries entries = EquipmentCodexEntryBuilder.Build(items);
        IReadOnlyList<EquipmentSpecialPageModel> pages = EquipmentSpecialPageModelBuilder.Build(entries, item => item == items[0] || item == items[6]);
        EquipmentSpecialPageModel setPage = pages.Single(page => page.Kind == EquipmentSpecialPageKind.Set);
        EquipmentSpecialPageModel singlePage = pages.Single(page => page.Kind == EquipmentSpecialPageKind.HighRankSingle);
        Assert.That(pages.Count, Is.EqualTo(2));
        Assert.That(setPage.Slots.Count, Is.EqualTo(3));
        Assert.That(setPage.Slots.All(slot => slot.Candidates.Count() == 2), Is.True);
        Assert.That(setPage.SetBonusText, Is.EqualTo(EquipmentSetCatalog.BuildDetailText(EquipmentSetId.Vanguard)));
        Assert.That(setPage.DiscoveredCount, Is.EqualTo(1));
        Assert.That(setPage.TotalCount, Is.EqualTo(6));
        Assert.That(singlePage.SingleItem.Rank, Is.EqualTo(9));
        Assert.That(singlePage.SingleItem.Item.equipmentSet, Is.EqualTo(EquipmentSetId.None));
        Assert.That(singlePage.DiscoveredCount, Is.EqualTo(1));
        foreach (ItemDataSO item in items)
        {
            Object.DestroyImmediate(item);
        }
    }

    [Test]
    public void SpecialPageModel_UsesAllSpecialAssetsAndPreservesOverallCount()
    {
        List<ItemDataSO> equipment = GameAssetRepository.LoadAll<ItemDataSO>().Where(item => item != null && item.IsEquipment).ToList();
        EquipmentCodexEntries entries = EquipmentCodexEntryBuilder.Build(equipment);
        IReadOnlyList<EquipmentSpecialPageModel> pages = EquipmentSpecialPageModelBuilder.Build(entries, _ => false);
        int specialEquipmentCount = pages.Sum(page => page.TotalCount);
        Assert.That(entries.SetGroups.Count(), Is.EqualTo(20));
        Assert.That(entries.HighRankSingleEquipment.Count(), Is.EqualTo(6));
        Assert.That(pages.Count, Is.EqualTo(26));
        Assert.That(entries.NormalEquipment.Count() + specialEquipmentCount, Is.EqualTo(equipment.Count));
    }

    [Test]
    public void EquipmentDetailPresentation_UsesDescriptionsCatalogNamesAndTierThresholds()
    {
        ItemDataSO setItem = CreateEquipment(EquipmentSetId.Vanguard, 3, EquipmentSlot.Weapon);
        setItem.description = "前衛を守るための装備。";
        ItemDataSO normalItem = CreateEquipment(EquipmentSetId.None, 3, EquipmentSlot.Weapon);
        Assert.That(CharacterEquipmentController.BuildEquipmentDescriptionText(setItem), Is.EqualTo("説明\n前衛を守るための装備。\n\n"));
        Assert.That(CharacterEquipmentController.BuildEquipmentDescriptionText(normalItem), Is.EqualTo(string.Empty));
        Assert.That(CharacterEquipmentController.BuildEquipmentSetMembershipText(setItem), Does.Contain("不屈の前衛"));
        Assert.That(CharacterEquipmentController.BuildEquipmentSetMembershipText(normalItem), Is.EqualTo(string.Empty));
        EquipmentSetCatalog.TryGet(EquipmentSetId.Vanguard, out EquipmentSetDefinition definition);
        Assert.That(CharacterEquipmentController.IsSetTierActive(definition.TwoPiece, 1), Is.False);
        Assert.That(CharacterEquipmentController.IsSetTierActive(definition.TwoPiece, 2), Is.True);
        Assert.That(CharacterEquipmentController.IsSetTierActive(definition.ThreePiece, 2), Is.False);
        Assert.That(CharacterEquipmentController.IsSetTierActive(definition.ThreePiece, 3), Is.True);
        Object.DestroyImmediate(setItem);
        Object.DestroyImmediate(normalItem);
    }

    private static void AssertTier(EquipmentSetId setId, int twoHp, int twoAttack, int twoDefense, float twoSpeed, int threeHp, int threeAttack, int threeDefense, float threeSpeed)
    {
        EquipmentSetCatalog.TryGet(setId, out EquipmentSetDefinition definition);
        Assert.That(definition.TwoPiece.BonusMaxHP, Is.EqualTo(twoHp));
        Assert.That(definition.TwoPiece.BonusAttack, Is.EqualTo(twoAttack));
        Assert.That(definition.TwoPiece.BonusDefense, Is.EqualTo(twoDefense));
        Assert.That(definition.TwoPiece.BonusAttackSpeed, Is.EqualTo(twoSpeed));
        Assert.That(definition.ThreePiece.BonusMaxHP, Is.EqualTo(threeHp));
        Assert.That(definition.ThreePiece.BonusAttack, Is.EqualTo(threeAttack));
        Assert.That(definition.ThreePiece.BonusDefense, Is.EqualTo(threeDefense));
        Assert.That(definition.ThreePiece.BonusAttackSpeed, Is.EqualTo(threeSpeed));
    }

    private static ItemDataSO CreateEquipment(EquipmentSetId setId, int rank, EquipmentSlot slot = EquipmentSlot.Weapon)
    {
        ItemDataSO item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemType = ItemType.Equipment;
        item.equipmentSet = setId;
        item.equipmentRank = rank;
        item.equipmentSlot = slot;
        return item;
    }
}
