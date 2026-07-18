using NUnit.Framework;
using UnityEngine;

public sealed class MercenaryLocationTests
{
    private GameObject root;
    private TownProgressState townProgress;
    private MercenaryHireManager hireManager;
    private MercenaryPartyManager partyManager;
    private HealingManager healingManager;
    private MerchantData merchantData;
    private MercenaryDataSO data;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Mercenary Location Test");
        townProgress = root.AddComponent<TownProgressState>();
        merchantData = root.AddComponent<MerchantData>();
        hireManager = root.AddComponent<MercenaryHireManager>();
        partyManager = root.AddComponent<MercenaryPartyManager>();
        healingManager = root.AddComponent<HealingManager>();
        data = ScriptableObject.CreateInstance<MercenaryDataSO>();
        data.mercenaryName = "Tester";
        data.maxHP = 100;
        townProgress.Initialize(2, new[] { 2, 3 });
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(data);
        Object.DestroyImmediate(root);
    }

    [Test]
    public void TryHireMercenary_SetsCurrentTownLocation()
    {
        MercenaryInstance mercenary = new MercenaryInstance(data);

        Assert.That(hireManager.TryHireMercenary(mercenary), Is.True);
        Assert.That(mercenary.CurrentTownIndex, Is.EqualTo(2));
    }

    [Test]
    public void TryAdd_RejectsMercenaryInAnotherTown_AndAcceptsCurrentTown()
    {
        MercenaryInstance mercenary = CreateHiredMercenary(3);

        Assert.That(partyManager.TryAdd(mercenary), Is.False);

        mercenary.SetCurrentTownIndex(2);

        Assert.That(partyManager.TryAdd(mercenary), Is.True);
    }

    [Test]
    public void SetCurrentTown_UpdatesAllPartyMemberLocations()
    {
        MercenaryInstance mercenary = CreateHiredMercenary(2);
        Assert.That(partyManager.TryAdd(mercenary), Is.True);

        townProgress.SetCurrentTown(3);

        Assert.That(mercenary.CurrentTownIndex, Is.EqualTo(3));
    }

    [Test]
    public void HealingList_FiltersMercenariesOutsideCurrentTown()
    {
        MercenaryInstance local = new MercenaryInstance(data);
        MercenaryInstance remote = new MercenaryInstance(data);
        local.SetCurrentTownIndex(2);
        remote.SetCurrentTownIndex(3);
        hireManager.RestoreHiredMercenaries(new[] { local, remote });
        local.SetCurrentHP(50);
        remote.SetCurrentHP(50);

        Assert.That(healingManager.GetMercenariesAtCurrentTown(),
            Is.EquivalentTo(new[] { local }));
    }

    [Test]
    public void MigrateV27_PlacesMercenariesInSavedCurrentTown()
    {
        GameSaveData save = new GameSaveData
        {
            version = 27,
            currentTownIndex = 3
        };
        save.hiredMercenaries.Add(new SavedMercenary { townIndex = 0 });

        SaveDataMigrator.Migrate(save);

        Assert.That(save.version, Is.EqualTo(28));
        Assert.That(save.hiredMercenaries[0].townIndex, Is.EqualTo(3));
    }

    private MercenaryInstance CreateHiredMercenary(int townIndex)
    {
        MercenaryInstance mercenary = new MercenaryInstance(data);
        mercenary.SetCurrentTownIndex(townIndex);
        hireManager.RestoreHiredMercenaries(new[] { mercenary });
        return mercenary;
    }
}
