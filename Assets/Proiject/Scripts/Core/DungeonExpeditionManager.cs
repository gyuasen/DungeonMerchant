using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonExpedition
{
    public DungeonDataSO dungeon;
    public List<string> memberInstanceIds = new List<string>();
}

public enum ExpeditionFormationResult
{
    Succeeded,
    InvalidDungeon,
    DungeonNotCleared,
    HiddenDungeon,
    DungeonAlreadyAssigned,
    InvalidMembers
}

public enum ExpeditionEventType
{
    Succeeded,
    Failed
}

public sealed class ExpeditionEvent
{
    public ExpeditionEventType Type { get; }
    public DungeonExpedition Expedition { get; }
    public int Gold { get; }
    public IReadOnlyList<ItemDataSO> Materials { get; }
    public int ExperiencePerMercenary { get; }
    public EquipmentInstance LimitedEquipment { get; }

    public ExpeditionEvent(ExpeditionEventType type, DungeonExpedition expedition, int gold, IReadOnlyList<ItemDataSO> materials, EquipmentInstance limitedEquipment = null, int experiencePerMercenary = 0)
    {
        Type = type;
        Expedition = expedition;
        Gold = gold;
        Materials = materials;
        LimitedEquipment = limitedEquipment;
        ExperiencePerMercenary = experiencePerMercenary;
    }
}

public sealed class DungeonExpeditionManager : MonoBehaviour
{
    public const float LimitedDropRateMultiplier = .5f;

    [SerializeField] private List<DungeonExpedition> activeExpeditions = new List<DungeonExpedition>();
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private DayManager dayManager;
    private Func<float> randomValue = () => UnityEngine.Random.value;
    private bool isDayChangedSubscribed;

    public IReadOnlyList<DungeonExpedition> ActiveExpeditions => activeExpeditions;
    public event Action ExpeditionChanged;
    public event Action<ExpeditionEvent> ExpeditionEventOccurred;
    public event Action<ExpeditionEvent, int> ExpeditionDayResolved;

    private void OnEnable()
    {
        ResolveReferences();
    }

    private void OnDisable()
    {
        if (dayManager != null && isDayChangedSubscribed)
        {
            dayManager.DayChanged -= HandleDayChanged;
            isDayChangedSubscribed = false;
        }
    }

    public void SetRandomProvider(Func<float> provider)
    {
        randomValue = provider ?? (() => UnityEngine.Random.value);
    }

    public ExpeditionFormationResult TryFormExpedition(DungeonDataSO dungeon, IReadOnlyList<MercenaryInstance> members)
    {
        ExpeditionFormationResult result = Validate(dungeon, members);
        if (result != ExpeditionFormationResult.Succeeded)
        {
            return result;
        }

        DungeonExpedition expedition = new DungeonExpedition { dungeon = dungeon };
        foreach (MercenaryInstance member in members)
        {
            expedition.memberInstanceIds.Add(member.InstanceId);
        }
        activeExpeditions.Add(expedition);
        ExpeditionChanged?.Invoke();
        return ExpeditionFormationResult.Succeeded;
    }

    public void RecallExpedition(DungeonExpedition expedition)
    {
        if (expedition != null && activeExpeditions.Remove(expedition))
        {
            ExpeditionChanged?.Invoke();
        }
    }

    public void ClearActiveExpeditions()
    {
        if (activeExpeditions.Count > 0)
        {
            activeExpeditions.Clear();
            ExpeditionChanged?.Invoke();
        }
    }

    public bool IsMercenaryOnExpedition(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return false;
        }
        foreach (DungeonExpedition expedition in activeExpeditions)
        {
            if (expedition != null && expedition.memberInstanceIds.Contains(instanceId))
            {
                return true;
            }
        }
        return false;
    }

    public int GetRequiredStrength(DungeonDataSO dungeon)
    {
        if (dungeon == null)
        {
            return 0;
        }
        switch (dungeon.grade)
        {
            case DungeonGrade.Low: return 100;
            case DungeonGrade.Lower: return 220;
            case DungeonGrade.Middle: return 420;
            case DungeonGrade.Upper: return 750;
            default: return 1200;
        }
    }

    public int GetExpeditionStrength(DungeonExpedition expedition)
    {
        return CombatPowerCalculator.Calculate(GetMembers(expedition));
    }

    public List<SavedDungeonExpedition> CreateSaveData()
    {
        List<SavedDungeonExpedition> saved = new List<SavedDungeonExpedition>();
        foreach (DungeonExpedition expedition in activeExpeditions)
        {
            if (expedition?.dungeon == null)
            {
                continue;
            }
            saved.Add(new SavedDungeonExpedition
            {
                dungeonPersistentId = expedition.dungeon.PersistentId,
                dungeonAssetName = expedition.dungeon.name,
                memberInstanceIds = new List<string>(expedition.memberInstanceIds)
            });
        }
        return saved;
    }

    public void Restore(List<SavedDungeonExpedition> saved, IReadOnlyDictionary<string, MercenaryInstance> mercenaries)
    {
        ResolveReferences();
        activeExpeditions.Clear();
        if (saved != null && mercenaries != null)
        {
            foreach (SavedDungeonExpedition value in saved)
            {
                DungeonDataSO dungeon = value == null ? null : GameAssetRepository.FindByPersistentId<DungeonDataSO>(value.dungeonPersistentId, value.dungeonAssetName);
                if (dungeon == null ||
                    dungeon.nearbyTownIndex == WorldMapService.HiddenIslandTownIndex ||
                    dungeonRunManager == null ||
                    dungeonRunManager.GetClearedFloors(dungeon) < Mathf.Max(1, dungeon.totalFloors) ||
                    value.memberInstanceIds == null ||
                    value.memberInstanceIds.Count < 1 ||
                    value.memberInstanceIds.Count > 3 ||
                    HasExpeditionForDungeon(dungeon))
                {
                    continue;
                }
                DungeonExpedition expedition = new DungeonExpedition { dungeon = dungeon };
                bool valid = true;
                foreach (string id in value.memberInstanceIds)
                {
                    if (string.IsNullOrWhiteSpace(id) ||
                        !mercenaries.TryGetValue(id, out MercenaryInstance mercenary) ||
                        mercenary == null ||
                        !mercenary.IsContractActive ||
                        mercenary.CurrentTownIndex != dungeon.nearbyTownIndex ||
                        expedition.memberInstanceIds.Contains(id) ||
                        IsMercenaryOnExpedition(id) ||
                        MercenaryDutyService.IsOnNonExpeditionDuty(id))
                    {
                        valid = false;
                        break;
                    }
                    expedition.memberInstanceIds.Add(id);
                }
                if (valid)
                {
                    activeExpeditions.Add(expedition);
                }
            }
        }
        ExpeditionChanged?.Invoke();
    }

    private ExpeditionFormationResult Validate(DungeonDataSO dungeon, IReadOnlyList<MercenaryInstance> members)
    {
        ResolveReferences();
        if (dungeon == null)
        {
            return ExpeditionFormationResult.InvalidDungeon;
        }
        if (dungeon.nearbyTownIndex == WorldMapService.HiddenIslandTownIndex)
        {
            return ExpeditionFormationResult.HiddenDungeon;
        }
        if (dungeonRunManager == null || dungeonRunManager.GetClearedFloors(dungeon) < Mathf.Max(1, dungeon.totalFloors))
        {
            return ExpeditionFormationResult.DungeonNotCleared;
        }
        if (HasExpeditionForDungeon(dungeon))
        {
            return ExpeditionFormationResult.DungeonAlreadyAssigned;
        }
        if (members == null || members.Count < 1 || members.Count > 3)
        {
            return ExpeditionFormationResult.InvalidMembers;
        }
        HashSet<string> ids = new HashSet<string>();
        foreach (MercenaryInstance member in members)
        {
            if (member == null || !member.IsContractActive || !ids.Add(member.InstanceId) || !IsHired(member) || member.CurrentTownIndex != dungeon.nearbyTownIndex || MercenaryDutyService.IsOnNonExpeditionDuty(member.InstanceId) || IsMercenaryOnExpedition(member.InstanceId))
            {
                return ExpeditionFormationResult.InvalidMembers;
            }
        }
        return ExpeditionFormationResult.Succeeded;
    }

    private void HandleDayChanged(int day)
    {
        ResolveReferences();
        foreach (DungeonExpedition expedition in activeExpeditions)
        {
            ProcessExpedition(expedition, day - 1);
        }
        ExpeditionChanged?.Invoke();
    }

    private void ProcessExpedition(DungeonExpedition expedition, int accountingDay)
    {
        if (expedition?.dungeon == null)
        {
            return;
        }
        List<MercenaryInstance> members = GetMembers(expedition);
        int required = GetRequiredStrength(expedition.dungeon);
        int strength = GetExpeditionStrength(expedition);
        if (strength < required)
        {
            foreach (MercenaryInstance member in members)
            {
                int damage = Mathf.Max(1, Mathf.CeilToInt(member.MaxHP * (.1f + Mathf.Clamp01((required - strength) / (float)required) * .2f)));
                member.SetCurrentHP(Mathf.Max(1, member.CurrentHP - damage));
            }
            SettleExpeditionHealing(expedition.dungeon, members, accountingDay);
            NotifyExpeditionResolved(new ExpeditionEvent(ExpeditionEventType.Failed, expedition, 0, Array.Empty<ItemDataSO>()), accountingDay);
            return;
        }
        List<ItemDataSO> materials = new List<ItemDataSO>();
        int gold = GrantNormalEncounterRewards(expedition.dungeon, members, strength, required, materials, out int experiencePerMercenary);
        EquipmentInstance limitedEquipment = TryDepositLimitedEquipment(expedition.dungeon);
        SettleExpeditionHealing(expedition.dungeon, members, accountingDay);
        NotifyExpeditionResolved(new ExpeditionEvent(ExpeditionEventType.Succeeded, expedition, gold, materials, limitedEquipment, experiencePerMercenary), accountingDay);
    }

    private int GrantNormalEncounterRewards(DungeonDataSO dungeon, IReadOnlyList<MercenaryInstance> members, int strength, int requiredStrength, List<ItemDataSO> awardedMaterials, out int totalExperiencePerMercenary)
    {
        int totalGold = 0;
        totalExperiencePerMercenary = 0;
        for (int encounterNumber = 1; encounterNumber <= RollRange(3, 6); encounterNumber++)
        {
            int enemyCount = Mathf.Min(Mathf.Max(1, dungeon.maxEnemyCountPerEncounter), Mathf.Max(1, dungeon.firstEncounterEnemyCount) + ((encounterNumber - 1) * Mathf.Max(0, dungeon.enemyCountIncreasePerEncounter)));
            List<EnemyDataSO> enemies = CreateNormalEncounter(dungeon, enemyCount);
            BattleRewardService.VictoryRewardCalculation rewards = BattleRewardService.CalculateVictoryRewards(enemies, members.Count, randomValue, null);
            ApplySuccessfulEncounterDamage(dungeon, enemies, members, strength, requiredStrength);
            totalGold += rewards.Gold;
            totalExperiencePerMercenary += rewards.ExperiencePerMercenary;
            foreach (MercenaryInstance member in members)
            {
                member?.AddExperience(rewards.ExperiencePerMercenary);
            }
            foreach (ItemDropEntry drop in rewards.ItemDrops)
            {
                if (drop?.item != null && inventory != null && inventory.DepositItemTo(dungeon.nearbyTownIndex, drop.item, drop.amount))
                {
                    for (int amount = 0; amount < drop.amount; amount++)
                    {
                        awardedMaterials.Add(drop.item);
                    }
                }
            }
        }
        int accountingDay = dayManager != null
            ? dayManager.CurrentDay - 1
            : 0;
        merchantData?.AddGold(
            totalGold,
            GoldTransactionReason.ExpeditionReward,
            dungeon.dungeonName,
            accountingDay);
        return totalGold;
    }

    // This intentionally approximates combat rather than running a full BattleManager simulation.
    private static void ApplySuccessfulEncounterDamage(
        DungeonDataSO dungeon,
        IReadOnlyList<EnemyDataSO> enemies,
        IReadOnlyList<MercenaryInstance> members,
        int strength,
        int requiredStrength)
    {
        if (members == null || members.Count == 0)
        {
            return;
        }

        int enemyCount = 0;
        int totalAttack = 0;
        float enemyGradeFactor = 0f;
        if (enemies != null)
        {
            foreach (EnemyDataSO enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }
                enemyCount++;
                totalAttack += Mathf.Max(0, enemy.attack);
                // Monster grade 1 is the strongest grade in the reward/balance model.
                enemyGradeFactor += 1f + (10 - Mathf.Clamp(enemy.monsterGrade, 1, 10)) * .06f;
            }
        }

        enemyCount = Mathf.Max(1, enemyCount);
        enemyGradeFactor = enemyCount > 0 ? enemyGradeFactor / enemyCount : 1f;
        if (enemyGradeFactor <= 0f)
        {
            enemyGradeFactor = 1f;
        }
        float dungeonGradeFactor = 1f + (int)dungeon.grade * .2f;
        float strengthRatio = requiredStrength <= 0 ? 1f : strength / (float)requiredStrength;
        float strengthMitigation = 1f - Mathf.Clamp01((strengthRatio - 1f) / 2f) * .5f;
        float damagePercent = ((.015f * enemyCount * enemyGradeFactor * dungeonGradeFactor) +
            (totalAttack * .001f)) * strengthMitigation;

        foreach (MercenaryInstance member in members)
        {
            if (member == null)
            {
                continue;
            }
            int damage = Mathf.Max(1, Mathf.CeilToInt(member.MaxHP * damagePercent));
            member.SetCurrentHP(Mathf.Max(1, member.CurrentHP - damage));
        }
    }

    private void SettleExpeditionHealing(
        DungeonDataSO dungeon,
        IReadOnlyList<MercenaryInstance> members,
        int accountingDay)
    {
        if (dungeon == null || members == null || members.Count == 0)
        {
            return;
        }

        int totalCost = 0;
        foreach (MercenaryInstance member in members)
        {
            if (member != null)
            {
                totalCost += HealingCostService.CalculateFullHealCost(
                    member.MaxHP,
                    member.CurrentHP,
                    false).TotalCost;
            }
        }
        if (totalCost <= 0 || merchantData == null || !merchantData.TryPayGold(
            totalCost,
            GoldTransactionReason.ExpeditionHealing,
            "別動隊の治療費: " + dungeon.dungeonName,
            accountingDay))
        {
            return;
        }

        foreach (MercenaryInstance member in members)
        {
            if (member != null)
            {
                member.SetCurrentHP(member.MaxHP);
            }
        }
    }

    private void NotifyExpeditionResolved(ExpeditionEvent expeditionEvent, int accountingDay)
    {
        ExpeditionEventOccurred?.Invoke(expeditionEvent);
        ExpeditionDayResolved?.Invoke(expeditionEvent, accountingDay);
    }

    private List<EnemyDataSO> CreateNormalEncounter(DungeonDataSO dungeon, int enemyCount)
    {
        List<EnemyDataSO> enemies = new List<EnemyDataSO>();
        bool specialVariantAdded = false;
        for (int index = 0; index < enemyCount; index++)
        {
            EnemyDataSO enemy = GetRandomNormalEnemy(dungeon);
            if (enemy == null)
            {
                continue;
            }
            if (!specialVariantAdded && enemy.category == EnemyCategory.Normal && randomValue() < dungeon.specialVariantChance)
            {
                enemy = DungeonEnemyVariantService.CreateSpecialVariant(enemy, dungeon.specialVariantSkillPool, dungeon.grade, false, randomValue);
                specialVariantAdded = enemy != null && enemy.isSpecialVariant;
            }
            enemies.Add(enemy);
        }
        return enemies;
    }

    private EnemyDataSO GetRandomNormalEnemy(DungeonDataSO dungeon)
    {
        List<EnemyDataSO> candidates = new List<EnemyDataSO>();
        if (dungeon.normalEnemies == null)
        {
            return null;
        }
        foreach (EnemyDataSO enemy in dungeon.normalEnemies)
        {
            if (enemy != null && !enemy.isBoss)
            {
                candidates.Add(enemy);
            }
        }
        return candidates.Count == 0 ? null : candidates[RollRange(0, candidates.Count)];
    }

    private int RollRange(int minInclusive, int maxExclusive)
    {
        return Mathf.Clamp(Mathf.FloorToInt(randomValue() * (maxExclusive - minInclusive)) + minInclusive, minInclusive, maxExclusive - 1);
    }

    private EquipmentInstance TryDepositLimitedEquipment(DungeonDataSO dungeon)
    {
        if (inventory == null || dungeon.bossLimitedDropChance <= 0f || randomValue() > dungeon.bossLimitedDropChance * LimitedDropRateMultiplier)
        {
            return null;
        }
        EquipmentInstance equipment = DungeonRewardService.TryCreateLimitedEquipment(dungeon, randomValue);
        if (equipment != null)
        {
            inventory.DepositEquipmentTo(dungeon.nearbyTownIndex, equipment);
        }
        return equipment;
    }

    private List<MercenaryInstance> GetMembers(DungeonExpedition expedition)
    {
        List<MercenaryInstance> members = new List<MercenaryInstance>();
        if (expedition == null || hireManager == null)
        {
            return members;
        }
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && expedition.memberInstanceIds.Contains(mercenary.InstanceId))
            {
                members.Add(mercenary);
            }
        }
        return members;
    }

    private bool HasExpeditionForDungeon(DungeonDataSO dungeon)
    {
        foreach (DungeonExpedition expedition in activeExpeditions)
        {
            if (expedition != null && expedition.dungeon == dungeon)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsHired(MercenaryInstance mercenary)
    {
        if (hireManager == null)
        {
            return false;
        }
        foreach (MercenaryInstance hired in hireManager.HiredMercenaries)
        {
            if (ReferenceEquals(hired, mercenary))
            {
                return true;
            }
        }
        return false;
    }

    private void ResolveReferences()
    {
        dungeonRunManager = dungeonRunManager ?? GetComponent<DungeonRunManager>() ?? FindObjectOfType<DungeonRunManager>();
        inventory = inventory ?? GetComponent<MerchantInventory>() ?? FindObjectOfType<MerchantInventory>();
        merchantData = merchantData ?? GetComponent<MerchantData>() ?? FindObjectOfType<MerchantData>();
        hireManager = hireManager ?? GetComponent<MercenaryHireManager>() ?? FindObjectOfType<MercenaryHireManager>();
        dayManager = dayManager ?? GetComponent<DayManager>() ?? FindObjectOfType<DayManager>();
        if (dayManager != null && !isDayChangedSubscribed)
        {
            dayManager.DayChanged += HandleDayChanged;
            isDayChangedSubscribed = true;
        }
    }
}
