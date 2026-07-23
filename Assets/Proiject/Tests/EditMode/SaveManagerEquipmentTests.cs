using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class SaveManagerEquipmentTests
{
    [Test]
    public void CreateSavedEquipment_InvalidLegacyInstance_IsSkipped()
    {
        EquipmentInstance invalidEquipment =
            new EquipmentInstance(null, EquipmentQuality.Normal, Array.Empty<EquipmentModifier>());
        MethodInfo method = typeof(SaveManager).GetMethod(
            "CreateSavedEquipment",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(method, Is.Not.Null);
        object result = method.Invoke(null, new object[] { invalidEquipment });

        Assert.That(result, Is.Null);
    }

    [Test]
    public void RestoreMercenaryEquipment_EmptyInstanceUsesLegacyReference()
    {
        ItemDataSO ring = Resources.Load<ItemDataSO>(
            "GameData/Items/AbyssChainSealRing");
        Assert.That(ring, Is.Not.Null);
        GameSaveData data = new GameSaveData();
        SavedMercenary saved = new SavedMercenary
        {
            equippedAccessoryAssetName = ring.name,
            equippedAccessoryPersistentId = ring.PersistentId,
            equippedAccessoryInstance = new SavedEquipmentInstance()
        };
        data.hiredMercenaries.Add(saved);

        SaveDataMigrator.Migrate(data);

        Assert.That(saved.equippedAccessoryInstance, Is.Null);
        MercenaryInstance mercenary = MercenaryInstance.CreateRestored(
            "test",
            null,
            null,
            "Test",
            MercenaryClass.Warrior,
            MercenaryContractType.Exclusive,
            1,
            0,
            100,
            100,
            10,
            10,
            0,
            1f,
            0);
        GameObject managerObject = new GameObject("Save Manager Test");
        SaveManager manager = managerObject.AddComponent<SaveManager>();
        MethodInfo method = typeof(SaveManager).GetMethod(
            "RestoreMercenaryEquipment",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method.Invoke(manager, new object[]
        {
            mercenary,
            EquipmentSlot.Accessory,
            saved.equippedAccessoryPersistentId,
            saved.equippedAccessoryAssetName,
            saved.equippedAccessoryInstance
        });

        EquipmentInstance restored = mercenary.GetEquippedInstance(
            EquipmentSlot.Accessory);
        Assert.That(restored, Is.Not.Null);
        Assert.That(restored.BaseItem, Is.EqualTo(ring));
        Assert.That(restored.Quality, Is.EqualTo(EquipmentQuality.Normal));
        Assert.That(restored.EnhancementLevel, Is.Zero);
        Assert.That(restored.Modifiers, Is.Empty);
        UnityEngine.Object.DestroyImmediate(managerObject);
    }

    [Test]
    public void NormalizeSavedMercenaryEquipment_CreatesCompleteInstance()
    {
        ItemDataSO ring = Resources.Load<ItemDataSO>(
            "GameData/Items/AbyssChainSealRing");
        Assert.That(ring, Is.Not.Null);
        SavedMercenary saved = new SavedMercenary
        {
            equippedAccessoryAssetName = ring.name,
            equippedAccessoryPersistentId = ring.PersistentId
        };
        MethodInfo method = typeof(SaveManager).GetMethod(
            "NormalizeSavedMercenaryEquipment",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.Invoke(null, new object[]
        {
            saved,
            EquipmentSlot.Accessory,
            ring,
            null
        });

        Assert.That(saved.equippedAccessoryInstance, Is.Not.Null);
        Assert.That(saved.equippedAccessoryInstance.baseItemPersistentId,
            Is.EqualTo(ring.PersistentId));
        Assert.That(saved.equippedAccessoryInstance.baseItemAssetName,
            Is.EqualTo(ring.name));
        Assert.That(saved.equippedAccessoryInstance.quality,
            Is.EqualTo(EquipmentQuality.Normal));
        Assert.That(saved.equippedAccessoryInstance.enhancementLevel, Is.Zero);
        Assert.That(saved.equippedAccessoryInstance.modifiers, Is.Empty);
    }
}
