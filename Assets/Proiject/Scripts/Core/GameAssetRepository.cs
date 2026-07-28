using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IPersistentGameAsset
{
    string PersistentId { get; }
}

public static class GameAssetRepository
{
    // Resources 配下のアセットは実行中に増減しないため、型ごとに一度だけ
    // ロードしてキャッシュする。従来は LoadAll / FindByName /
    // FindByPersistentId が呼ばれるたびに Resources 全体を走査していた
    // (UI更新や候補列挙から高頻度で呼ばれる)。キャッシュにより走査を1回に
    // 抑える。テストは ClearCache() でキャッシュを破棄できる。
    private sealed class TypeCache
    {
        public Object[] Assets;
        public Dictionary<string, Object> ByPersistentId;
        public Dictionary<string, Object> ByName;
    }

    private static readonly Dictionary<Type, TypeCache> Caches =
        new Dictionary<Type, TypeCache>();

    public static IReadOnlyList<T> LoadAll<T>()
        where T : Object
    {
        return GetCachedAssets<T>();
    }

    public static T FindByName<T>(string assetName)
        where T : Object
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return null;
        }

        return GetCache<T>().ByName.TryGetValue(assetName, out Object asset)
            ? asset as T
            : null;
    }

    public static T FindByPersistentId<T>(
        string persistentId,
        string legacyAssetName = null)
        where T : Object
    {
        if (!string.IsNullOrWhiteSpace(persistentId))
        {
            if (GetCache<T>().ByPersistentId.TryGetValue(
                    persistentId, out Object cached))
            {
                return cached as T;
            }

            // Save/restore tests and runtime-created content can hold transient
            // ScriptableObjects that do not live under a Resources folder.
            // These are not cached because they appear and disappear at runtime.
            foreach (T asset in Resources.FindObjectsOfTypeAll<T>())
            {
                if (asset is IPersistentGameAsset persistentAsset &&
                    persistentAsset.PersistentId == persistentId)
                {
                    return asset;
                }
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

    // テスト用: キャッシュを破棄する。Resources 構成を差し替えるテストや、
    // クリーンな状態から検証したいテストで使う。
    public static void ClearCache()
    {
        Caches.Clear();
    }

    private static T[] GetCachedAssets<T>()
        where T : Object
    {
        return (T[])GetCache<T>().Assets;
    }

    private static TypeCache GetCache<T>()
        where T : Object
    {
        Type type = typeof(T);
        if (Caches.TryGetValue(type, out TypeCache cache))
        {
            return cache;
        }

        T[] assets = Resources.LoadAll<T>(string.Empty);
        Dictionary<string, Object> byPersistentId =
            new Dictionary<string, Object>();
        Dictionary<string, Object> byName = new Dictionary<string, Object>();
        foreach (T asset in assets)
        {
            if (asset == null)
            {
                continue;
            }

            if (!byName.ContainsKey(asset.name))
            {
                byName[asset.name] = asset;
            }

            if (asset is IPersistentGameAsset persistentAsset &&
                !string.IsNullOrWhiteSpace(persistentAsset.PersistentId) &&
                !byPersistentId.ContainsKey(persistentAsset.PersistentId))
            {
                byPersistentId[persistentAsset.PersistentId] = asset;
            }
        }

        cache = new TypeCache
        {
            Assets = assets,
            ByPersistentId = byPersistentId,
            ByName = byName
        };
        Caches[type] = cache;
        return cache;
    }
}
