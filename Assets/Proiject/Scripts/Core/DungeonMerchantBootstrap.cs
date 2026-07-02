using UnityEngine;

public static class DungeonMerchantBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureRuntimeObjects()
    {
        SimpleMercenaryHireUI existingUI =
            Object.FindObjectOfType<SimpleMercenaryHireUI>();

        GameObject root = existingUI != null
            ? existingUI.gameObject
            : Object.FindObjectOfType<MercenaryHireManager>() != null
            ? Object.FindObjectOfType<MercenaryHireManager>().gameObject
            : new GameObject("DungeonMerchant Runtime");

        EnsureComponent<MerchantData>(root);
        EnsureComponent<DayManager>(root);
        EnsureComponent<MarketPriceManager>(root);
        EnsureComponent<MerchantInventory>(root);
        EnsureComponent<MarketStockManager>(root);
        EnsureComponent<BlacksmithManager>(root);
        EnsureComponent<MercenaryHireManager>(root);
        EnsureComponent<HealingManager>(root);
        EnsureComponent<MercenaryPartyManager>(root);
        EnsureComponent<MercenaryGenerator>(root);
        EnsureComponent<BattleManager>(root);
        EnsureComponent<DungeonRunManager>(root);
        EnsureComponent<ProgressionManager>(root);
        EnsureComponent<DebtManager>(root);
        EnsureComponent<SimpleMercenaryHireUI>(root);
        SaveManager saveManager = EnsureComponent<SaveManager>(root);
        saveManager.InitializeAndLoad();

        Debug.Log("DungeonMerchant runtime objects were ensured automatically.");
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }
}
