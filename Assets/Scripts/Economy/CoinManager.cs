using System;
using UnityEngine;
using DesertArena.Core;
using DesertArena.Progression;
using DesertArena.Utilities;

namespace DesertArena.Economy
{
    /// <summary>
    /// Singleton that tracks the player's coin balance across sessions.
    /// Listens to <see cref="EventBus.OnEnemyKilled"/> and awards coins
    /// based on enemy type, with a reduced payout when replaying completed arenas.
    /// </summary>
    public class CoinManager : Singleton<CoinManager>
    {
        #region Constants

        private const string PREFS_TOTAL_COINS = "CoinManager_TotalCoins";

        /// <summary>Replay payout multiplier (25% of normal).</summary>
        private const float REPLAY_PAYOUT_MULTIPLIER = 0.25f;

        #endregion

        #region Serialized Fields

        [Header("Coin Rewards Per Enemy Type")]
        [SerializeField] [Tooltip("Coins awarded for killing a melee enemy.")]
        private int _meleeKillReward = 5;

        [SerializeField] [Tooltip("Coins awarded for killing a ranged enemy.")]
        private int _rangedKillReward = 8;

        [SerializeField] [Tooltip("Coins awarded for killing a boss enemy.")]
        private int _bossKillReward = 100;

        #endregion

        #region Private Fields

        private int _totalCoins;
        private int _levelCoins;

        #endregion

        #region Events

        /// <summary>Fired when the total coin balance changes. Passes new total.</summary>
        public event Action<int> OnTotalCoinsChanged;

        /// <summary>Fired when level coins change. Passes new level coin total.</summary>
        public event Action<int> OnLevelCoinsChanged;

        #endregion

        #region Properties

        /// <summary>Persistent total coin balance.</summary>
        public int TotalCoins => _totalCoins;

        /// <summary>Coins earned during the current level.</summary>
        public int LevelCoins => _levelCoins;

        #endregion

        #region Unity Lifecycle

        protected override void OnSingletonAwake()
        {
            LoadCoins();
        }

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
            EventBus.OnLevelStarted += HandleLevelStarted;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
            EventBus.OnLevelStarted -= HandleLevelStarted;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds coins to both the level tally and the persistent total.
        /// Applies the replay multiplier when the player is replaying a completed arena.
        /// </summary>
        /// <param name="amount">Base coin amount to add.</param>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            int finalAmount = amount;

            // Reduced payout when replaying a completed arena
            if (ProgressionManager.HasInstance && ProgressionManager.Instance.IsReplayingArena())
            {
                finalAmount = Mathf.Max(1, Mathf.RoundToInt(amount * REPLAY_PAYOUT_MULTIPLIER));
            }

            _levelCoins += finalAmount;
            _totalCoins += finalAmount;

            OnLevelCoinsChanged?.Invoke(_levelCoins);
            OnTotalCoinsChanged?.Invoke(_totalCoins);

            SaveCoins();
        }

        /// <summary>
        /// Attempts to spend coins. Returns true if the player had enough.
        /// </summary>
        /// <param name="amount">Number of coins to spend.</param>
        /// <returns>True if the purchase succeeded; false if insufficient funds.</returns>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || _totalCoins < amount) return false;

            _totalCoins -= amount;
            OnTotalCoinsChanged?.Invoke(_totalCoins);
            SaveCoins();
            return true;
        }

        /// <summary>Returns the number of coins earned this level.</summary>
        public int GetLevelCoins()
        {
            return _levelCoins;
        }

        /// <summary>Resets the level coin counter to zero.</summary>
        public void ResetLevelCoins()
        {
            _levelCoins = 0;
            OnLevelCoinsChanged?.Invoke(_levelCoins);
        }

        /// <summary>
        /// Deducts a percentage of level coins (used by the revive system).
        /// </summary>
        /// <param name="percentage">Fraction to deduct (0-1).</param>
        /// <returns>The number of coins deducted.</returns>
        public int DeductLevelCoinPercentage(float percentage)
        {
            int penalty = Mathf.RoundToInt(_levelCoins * Mathf.Clamp01(percentage));
            _levelCoins -= penalty;
            _totalCoins -= penalty;
            _totalCoins = Mathf.Max(0, _totalCoins);

            OnLevelCoinsChanged?.Invoke(_levelCoins);
            OnTotalCoinsChanged?.Invoke(_totalCoins);
            SaveCoins();

            return penalty;
        }

        #endregion

        #region Private Methods

        private void HandleEnemyKilled(EnemyType enemyType)
        {
            int reward = enemyType switch
            {
                EnemyType.Melee  => _meleeKillReward,
                EnemyType.Ranged => _rangedKillReward,
                EnemyType.Boss   => _bossKillReward,
                _                => _meleeKillReward
            };

            AddCoins(reward);
        }

        private void HandleLevelStarted()
        {
            ResetLevelCoins();
        }

        private void SaveCoins()
        {
            PlayerPrefs.SetInt(PREFS_TOTAL_COINS, _totalCoins);
            PlayerPrefs.Save();
        }

        private void LoadCoins()
        {
            _totalCoins = PlayerPrefs.GetInt(PREFS_TOTAL_COINS, 0);
        }

        #endregion
    }
}
