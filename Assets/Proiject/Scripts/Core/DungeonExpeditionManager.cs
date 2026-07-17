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
    public EquipmentInstance LimitedEquipment { get; }

    public ExpeditionEvent(
        ExpeditionEventType type,
        DungeonExpedition expedition,
        int gold,
        IReadOnlyList<ItemDataSO> materials,
        EquipmentInstance limitedEquipment = null)
    {
        Type = type;
        Expedition = expedition;
        Gold = gold;
        Materials = materials;
        LimitedEquipment = limitedEquipment;
    }
}

public class DungeonExpeditionManager : MonoBehaviour
{
    public const float LimitedDropRateMultiplier = 0.5f;
    [SerializeField] private List<DungeonExpedition> activeExpeditions = new List<DungeonExpedition>();
    [SerializeField] private DungeonRunManager dungeonRunManager;
    [SerializeField] private MerchantInventory inventory;
    [SerializeField] private MerchantData merchantData;
    [SerializeField] private MercenaryHireManager hireManager;
    [SerializeField] private MercenaryPartyManager partyManager;
    [SerializeField] private TransportManager transportManager;
    [SerializeField] private DayManager dayManager;
    private Func<float> randomValue = () => UnityEngine.Random.value;
    private bool isDayChangedSubscribed;

    public IReadOnlyList<DungeonExpedition> ActiveExpeditions => activeExpeditions;
    public event Action ExpeditionChanged;
    public event Action<ExpeditionEvent> ExpeditionEventOccurred;

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

    public bool IsMercenaryOnExpeditionDuty(string instanceId)
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
        // Expeditions exclude bosses, special bosses, job certificates, and boss relic rewards.
        if (dungeon == null)
        {
            return 0;
        }
        switch (dungeon.grade)
        {
            case DungeonGrade.Low: return 60;
            case DungeonGrade.Lower: return 120;
            case DungeonGrade.Middle: return 200;
            case DungeonGrade.Upper: return 300;
            case DungeonGrade.Highest: return 420;
            default: return 420;
        }
    }

    public int GetExpeditionStrength(DungeonExpedition expedition)
    {
        int strength = 0;
        foreach (MercenaryInstance member in GetMembers(expedition))
        {
            if (member.CurrentHP > 0)
            {
                strength += (member.Attack + member.Defense + member.MaxHP / 10) * member.Level;
            }
        }
        return strength;
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
        activeExpeditions.Clear();
        if (saved != null && mercenaries != null)
        {
            foreach (SavedDungeonExpedition value in saved)
            {
                DungeonDataSO dungeon = value == null ? null : GameAssetRepository.FindByPersistentId<DungeonDataSO>(value.dungeonPersistentId, value.dungeonAssetName);
                if (dungeon == null || value.memberInstanceIds == null || value.memberInstanceIds.Count < 1 || value.memberInstanceIds.Count > 3)
                {
                    continue;
                }
                DungeonExpedition expedition = new DungeonExpedition { dungeon = dungeon };
                bool valid = true;
                foreach (string id in value.memberInstanceIds)
                {
                    if (string.IsNullOrWhiteSpace(id) || !mercenaries.ContainsKey(id) || expedition.memberInstanceIds.Contains(id))
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
        if (dungeonRunManager == null || dungeonRunManager.GetClearedFloors(dungeon) < dungeon.totalFloors)
        {
            return ExpeditionFormationResult.DungeonNotCleared;
        }
        if (members == null || members.Count < 1 || members.Count > 3)
        {
            return ExpeditionFormationResult.InvalidMembers;
        }
        HashSet<string> ids = new HashSet<string>();
        foreach (MercenaryInstance member in members)
        {
            if (member == null || !member.IsContractActive || !ids.Add(member.InstanceId) || !IsHired(member) || (partyManager != null && partyManager.Contains(member)) || (transportManager != null && transportManager.IsMercenaryOnTransportDuty(member.InstanceId)) || IsMercenaryOnExpeditionDuty(member.InstanceId))
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
            ProcessExpedition(expedition);
        }
        ExpeditionChanged?.Invoke();
    }

    private void ProcessExpedition(DungeonExpedition expedition)
    {
        int strength = GetExpeditionStrength(expedition);
        int required = GetRequiredStrength(expedition.dungeon);
        List<MercenaryInstance> members = GetMembers(expedition);
        if (strength < required)
        {
            float lack = required <= 0 ? 1f : Mathf.Clamp01((required - strength) / (float)required);
            foreach (MercenaryInstance member in members)
            {
                int damage = Mathf.Max(1, Mathf.CeilToInt(member.MaxHP * (.1f + lack * .2f)));
                member.SetCurrentHP(Mathf.Max(1, member.CurrentHP - damage));
            }
            ExpeditionEventOccurred?.Invoke(new ExpeditionEvent(ExpeditionEventType.Failed, expedition, 0, Array.Empty<ItemDataSO>()));
            return;
        }
        List<ItemDataSO> materials = new List<ItemDataSO>();
        int gold = GrantNormalEncounterRewards(expedition.dungeon, members, materials);
        EquipmentInstance limitedEquipment = TryDepositLimitedEquipment(expedition.dungeon);
        ExpeditionEventOccurred?.Invoke(
            new ExpeditionEvent(
                ExpeditionEventType.Succeeded,
                expedition,
                gold,
                materials,
                limitedEquipment));
    }

    private int GrantNormalEncounterRewards(
        DungeonDataSO dungeon,
        IReadOnlyList<MercenaryInstance> members,
        List<ItemDataSO> awardedMaterials)
    {
        if (dungeon == null)
        {
            return 0;
        }
        int totalGold = 0;
        int encounterCount = RollRange(3, 6);
        for (int encounterNumber = 1; encounterNumber <= encounterCount; encounterNumber++)
        {
            int enemyCount = Mathf.Min(
                Mathf.Max(1, dungeon.maxEnemyCountPerEncounter),
                Mathf.Max(1, dungeon.firstEncounterEnemyCount) +
                ((encounterNumber - 1) * Mathf.Max(0, dungeon.enemyCountIncreasePerEncounter)));
            List<EnemyDataSO> enemies = CreateNormalEncounter(dungeon, enemyCount);
            BattleRewardService.VictoryRewardCalculation rewards =
                BattleRewardService.CalculateVictoryRewards(
                    enemies,
                    members == null ? 0 : members.Count,
                    randomValue,
                    null);
            totalGold += rewards.Gold;
            foreach (MercenaryInstance member in members)
            {
                if (member != null)
                {
                    member.AddExperience(rewards.ExperiencePerMercenary);
                }
            }
            foreach (ItemDropEntry drop in rewards.ItemDrops)
            {
                if (drop != null && drop.item != null && inventory != null && inventory.DepositItemTo(dungeon.nearbyTownIndex, drop.item, drop.amount))
                {
                    for (int amount = 0; amount < drop.amount; amount++)
                    {
                        awardedMaterials.Add(drop.item);
                    }
                }
            }
        }
        merchantData?.AddGold(totalGold);
        return totalGold;
    }

    private List<EnemyDataSO> CreateNormalEncounter(DungeonDataSO dungeon, int enemyCount)
    {
        List<EnemyDataSO> result = new List<EnemyDataSO>();
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
                enemy = DungeonEnemyVariantService.CreateSpecialVariant(
                    enemy,
                    dungeon.specialVariantSkillPool,
                    dungeon.grade,
                    false,
                    randomValue);
                specialVariantAdded = enemy != null && enemy.isSpecialVariant;
            }
            result.Add(enemy);
        }
        return result;
    }

    private EnemyDataSO GetRandomNormalEnemy(DungeonDataSO dungeon)
    {
        List<EnemyDataSO> candidates = new List<EnemyDataSO>();
        if (dungeon != null && dungeon.normalEnemies != null)
        {
            foreach (EnemyDataSO enemy in dungeon.normalEnemies)
            {
                if (enemy != null && !enemy.isBoss)
                {
                    candidates.Add(enemy);
                }
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
        if (dungeon == null ||
            inventory == null ||
            dungeon.bossLimitedDropChance <= 0f ||
            randomValue() > dungeon.bossLimitedDropChance * LimitedDropRateMultiplier)
        {
            return null;
        }

        EquipmentInstance equipment =
            DungeonRewardService.TryCreateLimitedEquipment(dungeon, randomValue);
        if (equipment == null)
        {
            return null;
        }

        inventory.DepositEquipmentTo(dungeon.nearbyTownIndex, equipment);
        return equipment;
    }

    private List<MercenaryInstance> GetMembers(DungeonExpedition expedition)
    {
        List<MercenaryInstance> result = new List<MercenaryInstance>();
        if (expedition == null || hireManager == null)
        {
            return result;
        }
        foreach (MercenaryInstance mercenary in hireManager.HiredMercenaries)
        {
            if (mercenary != null && expedition.memberInstanceIds.Contains(mercenary.InstanceId))
            {
                result.Add(mercenary);
            }
        }
        return result;
    }

    private bool IsHired(MercenaryInstance member)
    {
        if (hireManager == null)
        {
            return false;
        }
        foreach (MercenaryInstance hired in hireManager.HiredMercenaries)
        {
            if (ReferenceEquals(hired, member))
            {
                return true;
            }
        }
        return false;
    }

    private void ResolveReferences()
    {
        if (dungeonRunManager == null) dungeonRunManager = GetComponent<DungeonRunManager>() ?? FindObjectOfType<DungeonRunManager>();
        if (inventory == null) inventory = GetComponent<MerchantInventory>() ?? FindObjectOfType<MerchantInventory>();
        if (merchantData == null) merchantData = GetComponent<MerchantData>() ?? FindObjectOfType<MerchantData>();
        if (hireManager == null) hireManager = GetComponent<MercenaryHireManager>() ?? FindObjectOfType<MercenaryHireManager>();
        if (partyManager == null) partyManager = GetComponent<MercenaryPartyManager>() ?? FindObjectOfType<MercenaryPartyManager>();
        if (transportManager == null) transportManager = GetComponent<TransportManager>() ?? FindObjectOfType<TransportManager>();
        if (dayManager == null) dayManager = GetComponent<DayManager>() ?? FindObjectOfType<DayManager>();
        EnsureDayChangedSubscription();
    }

    private void EnsureDayChangedSubscription()
    {
        if (dayManager == null)
        {
            return;
        }

        dayManager.DayChanged -= HandleDayChanged;
        dayManager.DayChanged += HandleDayChanged;
        isDayChangedSubscribed = true;
    }
}
