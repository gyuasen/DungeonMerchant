using System.Collections.Generic;
using UnityEngine;

public interface IPersistentGameAsset
{
    string PersistentId { get; }
}

public static class GameAssetRepository
{
    public static IReadOnlyList<T> LoadAll<T>()
        where T : Object
    {
        return Resources.LoadAll<T>(string.Empty);
    }

    public static T FindByName<T>(string assetName)
        where T : Object
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        foreach (T asset in Resources.LoadAll<T>(string.Empty))
        {
            if (asset != null && asset.name == assetName)
            {
                return asset;
            }
        }

        return null;
    }

    public static T FindByPersistentId<T>(
        string persistentId,
        string legacyAssetName = null)
        where T : Object
    {
        foreach (T asset in Resources.LoadAll<T>(string.Empty))
        {
            if (asset is IPersistentGameAsset persistentAsset &&
                persistentAsset.PersistentId == persistentId)
            {
                return asset;
            }
        }

        return FindByName<T>(legacyAssetName);
    }

    public static string GetPersistentId(Object asset)
    {
        return asset is IPersistentGameAsset persistentAsset
            ? persistentAsset.PersistentId
            : asset != null ? asset.name : string.Empty;
    }
}
