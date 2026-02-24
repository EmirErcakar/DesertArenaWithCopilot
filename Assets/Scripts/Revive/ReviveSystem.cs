using System;
using UnityEngine;
using DesertArena.Core;
using DesertArena.Economy;
using DesertArena.Player;

namespace DesertArena.Revive
{
    /// <summary>
    /// Handles in-level revives. The player may revive up to 3 times per level.
    /// Each revive costs 15 % of the coins earned this level and grants
    /// 3 seconds of invulnerability. XP progress and boss HP are unchanged.
    /// </summary>
    public class ReviveSystem : MonoBehaviour
    {
        #region Constants

        private const int MAX_REVIVES_PER_LEVEL = 3;
        private const float COIN_PENALTY_PER_REVIVE = 0.15f;
        private const float INVULNERABILITY_DURATION = 3f;

        #endregion

        #region Serialized Fields

        [Header("References")]
        [SerializeField] [Tooltip("Reference to the player's PlayerStats component.")]
        private PlayerStats _playerStats;

        #endregion

        #region Private Fields

        private int _revivesUsed;

        #endregion

        #region Events

        /// <summary>Fired when a revive is consumed. Passes remaining revives.</summary>
        public event Action<int> OnReviveUsed;

        /// <summary>Fired when all revives are exhausted.</summary>
        public event Action OnRevivesExhausted;

        #endregion

        #region Properties

        /// <summary>Number of revives used this level.</summary>
        public int RevivesUsed => _revivesUsed;

        /// <summary>Number of revives remaining this level.</summary>
        public int RevivesRemaining => MAX_REVIVES_PER_LEVEL - _revivesUsed;

        /// <summary>Maximum revives allowed per level.</summary>
        public int MaxRevives => MAX_REVIVES_PER_LEVEL;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnLevelStarted += HandleLevelStarted;
        }

        private void OnDisable()
        {
            EventBus.OnLevelStarted -= HandleLevelStarted;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if the player has revives remaining.
        /// </summary>
        public bool CanRevive()
        {
            return _revivesUsed < MAX_REVIVES_PER_LEVEL;
        }

        /// <summary>
        /// Initiates a revive via a rewarded ad stub.
        /// On success: applies the coin penalty, revives the player at
        /// the same position with invulnerability, and fires events.
        /// </summary>
        public void Revive()
        {
            if (!CanRevive()) return;

            AdStubManager.ShowRewardedAd(
                onComplete: () =>
                {
                    ApplyRevive();
                },
                onFailed: () =>
                {
                    Debug.Log("[ReviveSystem] Ad failed – revive cancelled.");
                }
            );
        }

        /// <summary>
        /// Returns the coin penalty amount for the next revive.
        /// </summary>
        public int GetNextRevivePenalty()
        {
            if (!CoinManager.HasInstance) return 0;
            return Mathf.RoundToInt(CoinManager.Instance.GetLevelCoins() * COIN_PENALTY_PER_REVIVE);
        }

        #endregion

        #region Private Methods

        private void ApplyRevive()
        {
            _revivesUsed++;

            // Coin penalty: 15 % of level coins
            if (CoinManager.HasInstance)
            {
                CoinManager.Instance.DeductLevelCoinPercentage(COIN_PENALTY_PER_REVIVE);
            }

            // Revive the player in place with invulnerability
            if (_playerStats != null)
            {
                _playerStats.Revive();
                _playerStats.IsInvulnerable = true;
            }

            // Start invulnerability countdown
            StartCoroutine(InvulnerabilityCountdown());

            // Notify the rest of the game
            EventBus.RaisePlayerRevived();

            // Resume gameplay
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetState(GameManager.GameState.Playing);
            }

            OnReviveUsed?.Invoke(RevivesRemaining);

            if (!CanRevive())
            {
                OnRevivesExhausted?.Invoke();
            }
        }

        private System.Collections.IEnumerator InvulnerabilityCountdown()
        {
            yield return new WaitForSeconds(INVULNERABILITY_DURATION);

            if (_playerStats != null)
            {
                _playerStats.IsInvulnerable = false;
            }
        }

        private void HandleLevelStarted()
        {
            _revivesUsed = 0;
        }

        #endregion
    }
}
