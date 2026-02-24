using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages upgrade availability, controlled RNG selection, duplicate rules, and applies upgrades.
///
/// Inspector Setup:
///   - allUpgrades: drag ALL UpgradeDefinition ScriptableObjects into this list
///
/// Controlled RNG rules:
///   Slot 1 → always a Stat option
///   Slot 2 → always a WeaponFeature option (falls back to stat/random if none available)
///   Slot 3 → random from remaining eligible options
///
/// Duplicate rule: same StatType can appear at most maxTimesPerRun times per run.
/// Reroll: max 1 per level; costs coins OR a rewarded ad.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Pool")]
    [SerializeField] private List<UpgradeDefinition> allUpgrades = new List<UpgradeDefinition>();

    // Tracks how many times each stat type has been TAKEN this run
    private readonly Dictionary<UpgradeDefinition.StatType, int> statPickedCount
        = new Dictionary<UpgradeDefinition.StatType, int>();

    private bool rerollUsedThisLevel;
    private GameObject cachedPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        cachedPlayer = GameObject.FindGameObjectWithTag("Player");
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public void ResetForNewLevel() => rerollUsedThisLevel = false;

    public void ResetForNewRun()
    {
        statPickedCount.Clear();
        rerollUsedThisLevel = false;
    }

    public bool CanReroll() => !rerollUsedThisLevel;

    /// <summary>
    /// Generate 3 upgrade choices.
    /// Pass previousChoices when rerolling to ensure at least 1 new option.
    /// </summary>
    public List<UpgradeDefinition> GetUpgradeChoices(List<UpgradeDefinition> previousChoices = null)
    {
        List<UpgradeDefinition> statOptions    = GetAvailableStats();
        List<UpgradeDefinition> featureOptions = GetAvailableFeatures();
        List<UpgradeDefinition> allOptions     = GetAllAvailable();

        var result = new List<UpgradeDefinition>(3);

        // Slot 1 — stat
        var stat = PickRandom(statOptions, result);
        if (stat != null) result.Add(stat);

        // Slot 2 — weapon feature (fallback to any available)
        var feature = PickRandom(featureOptions, result);
        if (feature != null) result.Add(feature);
        else
        {
            var fallback = PickRandom(allOptions, result);
            if (fallback != null) result.Add(fallback);
        }

        // Slot 3 — random (prefer something different)
        while (result.Count < 3)
        {
            var r = PickRandom(allOptions, result);
            if (r != null) result.Add(r);
            else break; // pool exhausted
        }

        // Reroll guarantee: at least 1 new card vs previous selection
        if (previousChoices != null && previousChoices.Count > 0 && AreIdentical(result, previousChoices))
        {
            SwapOneCard(result, allOptions, previousChoices);
        }

        return result;
    }

    /// <summary>Consume reroll and return new choices.</summary>
    public List<UpgradeDefinition> Reroll(List<UpgradeDefinition> currentChoices)
    {
        if (!CanReroll()) return currentChoices;
        rerollUsedThisLevel = true;
        return GetUpgradeChoices(currentChoices);
    }

    /// <summary>Apply the chosen upgrade to the player.</summary>
    public void ApplyUpgrade(UpgradeDefinition upgrade)
    {
        if (upgrade == null) return;

        // Track stat picks for duplicate rule
        if (upgrade.upgradeType == UpgradeDefinition.UpgradeType.Stat
            && upgrade.statType  != UpgradeDefinition.StatType.None)
        {
            if (!statPickedCount.ContainsKey(upgrade.statType))
                statPickedCount[upgrade.statType] = 0;
            statPickedCount[upgrade.statType]++;
        }

        ApplyToPlayer(upgrade);
    }

    // ──────────────────────────────────────────────
    // Private — pool filtering
    // ──────────────────────────────────────────────

    List<UpgradeDefinition> GetAvailableStats()
    {
        var list = new List<UpgradeDefinition>();
        foreach (var u in allUpgrades)
        {
            if (u.upgradeType != UpgradeDefinition.UpgradeType.Stat) continue;
            if (u.statType == UpgradeDefinition.StatType.None) continue;
            if (TimesPickedForStat(u.statType) >= u.maxTimesPerRun) continue;
            list.Add(u);
        }
        return list;
    }

    List<UpgradeDefinition> GetAvailableFeatures()
    {
        var list = new List<UpgradeDefinition>();
        foreach (var u in allUpgrades)
            if (u.upgradeType == UpgradeDefinition.UpgradeType.WeaponFeature)
                list.Add(u);
        return list;
    }

    List<UpgradeDefinition> GetAllAvailable()
    {
        var list = new List<UpgradeDefinition>();
        foreach (var u in allUpgrades)
        {
            if (u.upgradeType == UpgradeDefinition.UpgradeType.Stat
                && u.statType  != UpgradeDefinition.StatType.None
                && TimesPickedForStat(u.statType) >= u.maxTimesPerRun)
                continue;
            list.Add(u);
        }
        return list;
    }

    int TimesPickedForStat(UpgradeDefinition.StatType stat)
    {
        return statPickedCount.TryGetValue(stat, out int c) ? c : 0;
    }

    // ──────────────────────────────────────────────
    // Private — RNG helpers
    // ──────────────────────────────────────────────

    UpgradeDefinition PickRandom(List<UpgradeDefinition> pool,
                                 List<UpgradeDefinition> exclude = null)
    {
        if (pool == null || pool.Count == 0) return null;

        // Build filtered list (avoid already-chosen options)
        var filtered = new List<UpgradeDefinition>(pool.Count);
        foreach (var u in pool)
            if (exclude == null || !exclude.Contains(u))
                filtered.Add(u);

        if (filtered.Count == 0)
            return pool[Random.Range(0, pool.Count)]; // fallback: allow duplicate

        return filtered[Random.Range(0, filtered.Count)];
    }

    static bool AreIdentical(List<UpgradeDefinition> a, List<UpgradeDefinition> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    void SwapOneCard(List<UpgradeDefinition> result,
                     List<UpgradeDefinition> pool,
                     List<UpgradeDefinition> previous)
    {
        // Find a candidate not in previous
        foreach (var candidate in pool)
        {
            if (!previous.Contains(candidate))
            {
                result[result.Count - 1] = candidate;
                return;
            }
        }
        // All options are the same as previous (tiny pool) — nothing we can do
    }

    // ──────────────────────────────────────────────
    // Private — apply to player
    // ──────────────────────────────────────────────

    void ApplyToPlayer(UpgradeDefinition upgrade)
    {
        // Refresh in case player was re-created (future-proof)
        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (cachedPlayer == null) return;

        switch (upgrade.statType)
        {
            case UpgradeDefinition.StatType.Attack:
                var wc = cachedPlayer.GetComponent<WeaponController>();
                if (wc != null) wc.damageBonus += upgrade.value;
                break;

            case UpgradeDefinition.StatType.AttackSpeed:
                var wc2 = cachedPlayer.GetComponent<WeaponController>();
                if (wc2 != null) wc2.attackSpeedBonus += upgrade.value;
                break;

            case UpgradeDefinition.StatType.MoveSpeed:
                cachedPlayer.GetComponent<PlayerController>()?.ApplyMoveSpeedBonus(upgrade.value);
                break;

            case UpgradeDefinition.StatType.Health:
                var hc = cachedPlayer.GetComponent<HealthComponent>();
                if (hc != null) hc.SetMaxHealth(hc.MaxHealth + upgrade.value, false);
                break;

            // WeaponFeature upgrades are handled by game-specific logic; extend here.
        }
    }
}
