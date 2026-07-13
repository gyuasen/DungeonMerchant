using NUnit.Framework;

public sealed class HiddenIslandUnlockServiceTests
{
    [TestCase(false, 9, 9, 3, false)]
    [TestCase(true, 8, 9, 3, false)]
    [TestCase(true, 9, 9, 2, false)]
    [TestCase(true, 9, 9, 3, true)]
    public void CanUnlock_RequiresAllThreeConditions(
        bool abyssCleared,
        int discoveredEquipment,
        int requiredEquipment,
        int specialMercenaries,
        bool expected)
    {
        HiddenIslandUnlockProgress progress =
            new HiddenIslandUnlockProgress(
                abyssCleared,
                discoveredEquipment,
                requiredEquipment,
                specialMercenaries);

        Assert.That(progress.CanUnlock, Is.EqualTo(expected));
    }

    [Test]
    public void CanUnlock_RejectsEmptyDungeonEquipmentCatalog()
    {
        HiddenIslandUnlockProgress progress =
            new HiddenIslandUnlockProgress(true, 0, 0, 3);

        Assert.That(progress.HasAllDungeonEquipment, Is.False);
        Assert.That(progress.CanUnlock, Is.False);
    }
}
