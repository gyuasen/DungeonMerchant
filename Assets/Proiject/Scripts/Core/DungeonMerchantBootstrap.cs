using UnityEngine;
using UnityEngine.SceneManagement;

public static class DungeonMerchantBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // RuntimeInitializeOnLoadMethod fires only once per play session, so
        // scene transitions (Title -> game) must re-run the bootstrap through
        // sceneLoaded. Unsubscribe first so disabled domain reload in the
        // Editor cannot stack duplicate handlers.
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureRuntimeObjects();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
        {
            return;
        }

        EnsureRuntimeObjects();
    }

    private static void EnsureRuntimeObjects()
    {
        // The title scene runs standalone: it must not spawn the game
        // managers or the game UI on top of the title screen.
        if (Object.FindObjectOfType<TitleSceneController>() != null)
        {
            return;
        }

        SimpleMercenaryHireUI existingUI =
            Object.FindObjectOfType<SimpleMercenaryHireUI>();

        GameObject root = existingUI != null
            ? existingUI.gameObject
            : Object.FindObjectOfType<MercenaryHireManager>() != null
            ? Object.FindObjectOfType<MercenaryHireManager>().gameObject
            : new GameObject("DungeonMerchant Runtime");

        EnsureComponent<MerchantData>(root);
        EnsureComponent<DayManager>(root);
        EnsureComponent<TownProgressState>(root);
        EnsureComponent<MarketPriceManager>(root);
        EnsureComponent<MerchantInventory>(root);
        EnsureComponent<MarketStockManager>(root);
        EnsureComponent<BlacksmithManager>(root);
        EnsureComponent<MercenaryHireManager>(root);
        EnsureComponent<HealingManager>(root);
        EnsureComponent<TrainingGroundManager>(root);
        EnsureComponent<MercenaryPartyManager>(root);
        EnsureComponent<TransportManager>(root);
        EnsureComponent<DungeonExpeditionManager>(root);
        EnsureComponent<RemoteSaleManager>(root);
        EnsureComponent<MercenaryGenerator>(root);
        EnsureComponent<BattleManager>(root);
        EnsureComponent<MonsterCodexManager>(root);
        EnsureComponent<DungeonRunManager>(root);
        EnsureComponent<RoadEncounterService>(root);
        EnsureComponent<DebtManager>(root);
        EnsureComponent<ProgressionManager>(root);
        EnsureComponent<StoryProgressManager>(root);
        EnsureComponent<AudioFeedbackService>(root);
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
            // The scene may already hold this manager on a different
            // GameObject (e.g. the serialized BattleManager object). Adding a
            // second instance to the root splits event producers/consumers
            // across two copies, so reuse the scene-wide instance instead.
            component = Object.FindObjectOfType<T>();
        }

        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }
}
