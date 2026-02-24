using System;
using UnityEngine;

namespace DesertArena.Core
{
    #region Supporting Types

    /// <summary>
    /// Enemy archetypes used to classify kills and targeting behavior.
    /// </summary>
    public enum EnemyType
    {
        Melee,
        Ranged,
        Boss
    }

    /// <summary>
    /// Placeholder for upgrade data passed through the event bus.
    /// Replace with a ScriptableObject-based implementation when ready.
    /// </summary>
    [Serializable]
    public class UpgradeData
    {
        public string upgradeName;
        public string description;
        public Sprite icon;
    }

    #endregion

    /// <summary>
    /// Static event bus for decoupled global game events.
    /// Subscribe in OnEnable and unsubscribe in OnDisable to avoid leaks.
    /// </summary>
    public static class EventBus
    {
        #region Combat Events

        /// <summary>Fired when any enemy is killed. Passes the type of enemy.</summary>
        public static event Action<EnemyType> OnEnemyKilled;

        /// <summary>Fired when a boss enemy spawns into the arena.</summary>
        public static event Action OnBossSpawned;

        /// <summary>Fired when a boss enemy is defeated.</summary>
        public static event Action OnBossDied;

        /// <summary>Fired when the boss countdown begins. Passes countdown duration.</summary>
        public static event Action<int> OnBossCountdownStarted;

        #endregion

        #region Level Events

        /// <summary>Fired when a level starts.</summary>
        public static event Action OnLevelStarted;

        /// <summary>Fired when a level ends. Passes true for victory, false otherwise.</summary>
        public static event Action<bool> OnLevelEnded;

        #endregion

        #region Economy Events

        /// <summary>Fired when the player collects coins. Passes coin amount.</summary>
        public static event Action<int> OnCoinCollected;

        /// <summary>Fired when the player gains XP. Passes XP amount.</summary>
        public static event Action<int> OnXPGained;

        #endregion

        #region Player Events

        /// <summary>Fired when the player is revived (e.g., via ad or ability).</summary>
        public static event Action OnPlayerRevived;

        #endregion

        #region Upgrade Events

        /// <summary>Fired when the player selects an upgrade. Passes upgrade data.</summary>
        public static event Action<UpgradeData> OnUpgradeSelected;

        /// <summary>Fired when the XP bar fills and an upgrade is available.</summary>
        public static event Action OnXPLevelUp;

        #endregion

        #region Raise Methods

        /// <summary>Call when an enemy is killed.</summary>
        public static void RaiseEnemyKilled(EnemyType enemyType)
        {
            OnEnemyKilled?.Invoke(enemyType);
        }

        /// <summary>Call when a boss spawns.</summary>
        public static void RaiseBossSpawned()
        {
            OnBossSpawned?.Invoke();
        }

        /// <summary>Call when a boss is defeated.</summary>
        public static void RaiseBossDied()
        {
            OnBossDied?.Invoke();
        }

        /// <summary>Call when the boss countdown begins.</summary>
        public static void RaiseBossCountdownStarted(int seconds)
        {
            OnBossCountdownStarted?.Invoke(seconds);
        }

        /// <summary>Call when a level starts.</summary>
        public static void RaiseLevelStarted()
        {
            OnLevelStarted?.Invoke();
        }

        /// <summary>Call when a level ends.</summary>
        public static void RaiseLevelEnded(bool victory)
        {
            OnLevelEnded?.Invoke(victory);
        }

        /// <summary>Call when coins are collected.</summary>
        public static void RaiseCoinCollected(int amount)
        {
            OnCoinCollected?.Invoke(amount);
        }

        /// <summary>Call when XP is gained.</summary>
        public static void RaiseXPGained(int amount)
        {
            OnXPGained?.Invoke(amount);
        }

        /// <summary>Call when the player is revived.</summary>
        public static void RaisePlayerRevived()
        {
            OnPlayerRevived?.Invoke();
        }

        /// <summary>Call when an upgrade is selected.</summary>
        public static void RaiseUpgradeSelected(UpgradeData data)
        {
            OnUpgradeSelected?.Invoke(data);
        }

        /// <summary>Call when the XP bar fills and an upgrade is available.</summary>
        public static void RaiseXPLevelUp()
        {
            OnXPLevelUp?.Invoke();
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clears all subscribers. Call during scene teardown or game reset.
        /// </summary>
        public static void ClearAll()
        {
            OnEnemyKilled = null;
            OnBossSpawned = null;
            OnBossDied = null;
            OnCoinCollected = null;
            OnXPGained = null;
            OnPlayerRevived = null;
            OnUpgradeSelected = null;
            OnXPLevelUp = null;
            OnBossCountdownStarted = null;
            OnLevelStarted = null;
            OnLevelEnded = null;
        }

        #endregion
    }
}
