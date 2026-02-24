using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the boss chest reward table.
/// Boss ALWAYS drops a chest (even after revives — chest is not penalised).
///
/// Inspector Setup:
///   - rewardTable: configure entries with name, type, minAmount, maxAmount, weight.
///
/// Suggested initial table:
///   Coins          — min 200, max 500, weight 60
///   SkinFragments  — min 1,   max 3,   weight 30
///   MetaToken      — min 1,   max 1,   weight 10
///
/// Attach to the GameManager or a dedicated ChestSystem GameObject.
/// </summary>
public class ChestRewardSystem : MonoBehaviour
{
    public static ChestRewardSystem Instance { get; private set; }

    [System.Serializable]
    public class RewardEntry
    {
        public string     rewardName;
        public RewardType type;
        public int        minAmount;
        public int        maxAmount;
        [Range(0, 100)]
        public int        weight = 10;
    }

    public enum RewardType { Coins, SkinFragments, MetaToken }

    [Header("Reward Table")]
    [SerializeField]
    private List<RewardEntry> rewardTable = new List<RewardEntry>
    {
        // Default entries — customise in the Inspector
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    /// <summary>Pick a random reward entry from the weighted table.</summary>
    public RewardEntry RollReward()
    {
        if (rewardTable == null || rewardTable.Count == 0) return null;

        int totalWeight = 0;
        foreach (var e in rewardTable) totalWeight += e.weight;
        if (totalWeight <= 0) return null;

        int roll       = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var e in rewardTable)
        {
            cumulative += e.weight;
            if (roll < cumulative) return e;
        }
        return rewardTable[rewardTable.Count - 1];
    }

    /// <summary>Grant the reward to the player (coins credited; others are stubs).</summary>
    public void GrantReward(RewardEntry reward, int multiplier = 1)
    {
        if (reward == null) return;
        int amount = Random.Range(reward.minAmount, reward.maxAmount + 1) * multiplier;

        switch (reward.type)
        {
            case RewardType.Coins:
                CoinManager.Instance?.AddCoins(amount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Chest] +{amount} Coins");
#endif
                break;

            case RewardType.SkinFragments:
                // TODO: connect to skin fragment inventory
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Chest] +{amount} Skin Fragments (stub)");
#endif
                break;

            case RewardType.MetaToken:
                // TODO: connect to meta upgrade system
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Chest] +{amount} Meta Token(s) (stub)");
#endif
                break;
        }
    }
}
