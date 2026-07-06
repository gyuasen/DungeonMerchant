using System.Collections.Generic;
using NUnit.Framework;

public sealed class GameAssetRepositoryTests
{
    [Test]
    public void DungeonPersistentIds_ArePresentAndUnique()
    {
        IReadOnlyList<DungeonDataSO> dungeons =
            GameAssetRepository.LoadAll<DungeonDataSO>();
        HashSet<string> ids = new HashSet<string>();

        Assert.That(dungeons.Count, Is.GreaterThan(0));
        foreach (DungeonDataSO dungeon in dungeons)
        {
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.PersistentId, Is.Not.Null.And.Not.Empty);
            Assert.That(
                ids.Add(dungeon.PersistentId),
                Is.True,
                $"Duplicate dungeon PersistentId: {dungeon.PersistentId}");
            Assert.That(
                GameAssetRepository.FindByPersistentId<DungeonDataSO>(
                    dungeon.PersistentId,
                    dungeon.name),
                Is.SameAs(dungeon));
        }
    }
}
