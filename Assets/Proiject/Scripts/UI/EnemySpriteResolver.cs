using UnityEngine;

public static class EnemySpriteResolver
{
    private const string EnemyResourcePath = "Battle/Enemies/";
    private const string SpecialVariantSuffix = " Special Variant";

    public static Sprite Resolve(EnemyDataSO enemy)
    {
        if (enemy == null)
        {
            return null;
        }

        if (enemy.battleSprite != null)
        {
            return enemy.battleSprite;
        }

        Sprite sprite = ResolveByKey(enemy.battleVisualKey);
        if (sprite != null)
        {
            return sprite;
        }

        sprite = ResolveByKey(enemy.name);
        if (sprite != null)
        {
            return sprite;
        }

        if (!enemy.isSpecialVariant)
        {
            return null;
        }

        EnemyDataSO source = GameAssetRepository.FindByPersistentId<EnemyDataSO>(
            enemy.runtimeSourcePersistentId);
        if (source != null && source != enemy)
        {
            return Resolve(source);
        }

        if (!enemy.name.EndsWith(
                SpecialVariantSuffix,
                System.StringComparison.Ordinal))
        {
            return null;
        }

        string sourceName = enemy.name.Substring(
            0,
            enemy.name.Length - SpecialVariantSuffix.Length);
        return ResolveByKey(sourceName);
    }

    private static Sprite ResolveByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        string trimmedKey = key.Trim();
        Sprite sprite = Resources.Load<Sprite>(trimmedKey);
        return sprite != null
            ? sprite
            : Resources.Load<Sprite>(EnemyResourcePath + trimmedKey);
    }
}
