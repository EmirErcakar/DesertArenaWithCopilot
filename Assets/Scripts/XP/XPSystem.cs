using UnityEngine;
using System;

/// <summary>
/// Tracks XP earned during a level and triggers the upgrade screen at thresholds.
/// About 3 upgrades per 120 s level (4 for high-kill play).
///
/// Inspector Setup:
///   - upgradeThresholds: set to e.g. { 50, 130, 230, 350 } for ~3-4 upgrades
///     (tune based on enemy XP rewards and expected kill rate)
///
/// Dependencies: GameManager, UpgradeScreenUI (listens to OnUpgradeTriggered)
/// </summary>
public class XPSystem : MonoBehaviour
{
    public static XPSystem Instance { get; private set; }

    [Header("XP Settings")]
    [SerializeField] private int[] upgradeThresholds = { 50, 130, 230, 350 };

    public int CurrentXP { get; private set; }

    /// <summary>Next threshold the player is progressing toward.</summary>
    public int NextThreshold =>
        nextThresholdIndex < upgradeThresholds.Length
            ? upgradeThresholds[nextThresholdIndex]
            : upgradeThresholds[upgradeThresholds.Length - 1];

    /// <summary>Fired with (currentXP, nextThreshold) whenever XP changes.</summary>
    public event Action<int, int> OnXPChanged;

    /// <summary>Fired when an XP threshold is crossed; causes game to pause.</summary>
    public event Action OnUpgradeTriggered;

    private int nextThresholdIndex;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Add XP (called by EnemyBase.HandleDeath).</summary>
    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        CurrentXP += amount;
        OnXPChanged?.Invoke(CurrentXP, NextThreshold);

        // Check for threshold crossings (player could gain a lot of XP at once)
        while (nextThresholdIndex < upgradeThresholds.Length
               && CurrentXP >= upgradeThresholds[nextThresholdIndex])
        {
            nextThresholdIndex++;
            TriggerUpgrade();
        }
    }

    /// <summary>Reset at the start of a new level run.</summary>
    public void ResetForNewLevel()
    {
        CurrentXP          = 0;
        nextThresholdIndex = 0;
        OnXPChanged?.Invoke(0, NextThreshold);
    }

    void TriggerUpgrade()
    {
        GameManager.Instance?.PauseGame();
        OnUpgradeTriggered?.Invoke();
    }
}
