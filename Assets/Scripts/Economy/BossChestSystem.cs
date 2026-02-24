using System;
using UnityEngine;
using DesertArena.Core;

namespace DesertArena.Economy
{
    /// <summary>
    /// Reward types that can drop from a boss chest.
    /// </summary>
    public enum ChestRewardType
    {
        Coins,
        SkinFragments,
        MetaTokens
    }

    /// <summary>
    /// Data container for a single chest reward roll.
    /// </summary>
    [Serializable]
    public struct ChestReward
    {
        /// <summary>Type of reward obtained.</summary>
        public ChestRewardType RewardType;

        /// <summary>Quantity of the reward.</summary>
        public int Amount;
    }

    /// <summary>
    /// Spawns a reward chest whenever a boss is defeated.
    /// The chest always drops, even if the player used revives.
    /// Loot table: 60 % coins (500-2000), 25 % skin fragments (1-3), 15 % meta tokens (1-2).
    /// </summary>
    public class BossChestSystem : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Loot Table Weights (must sum to 100)")]
        [SerializeField] [Tooltip("Chance for a coin drop (%).")]
        private int _coinChance = 60;

        [SerializeField] [Tooltip("Chance for a skin-fragment drop (%).")]
        private int _fragmentChance = 25;

        // Meta-token chance is the remainder (15 %).

        [Header("Coin Range")]
        [SerializeField] private int _minCoinDrop = 500;
        [SerializeField] private int _maxCoinDrop = 2000;

        [Header("Fragment Range")]
        [SerializeField] private int _minFragments = 1;
        [SerializeField] private int _maxFragments = 3;

        [Header("Meta Token Range")]
        [SerializeField] private int _minMetaTokens = 1;
        [SerializeField] private int _maxMetaTokens = 2;

        [Header("References")]
        [SerializeField] [Tooltip("Optional: chest prefab to spawn in the arena.")]
        private GameObject _chestPrefab;

        [SerializeField] [Tooltip("Optional: Animator for the chest open animation.")]
        private Animator _chestAnimator;

        #endregion

        #region Events

        /// <summary>Fired when a chest is ready to be opened.</summary>
        public event Action OnChestAvailable;

        /// <summary>Fired after a chest is opened. Passes the reward.</summary>
        public event Action<ChestReward> OnChestOpened;

        #endregion

        #region Private Fields

        private bool _chestReady;
        private ChestReward _pendingReward;

        #endregion

        #region Properties

        /// <summary>True when a chest is waiting to be opened.</summary>
        public bool IsChestReady => _chestReady;

        /// <summary>The reward that will be given when the chest is opened.</summary>
        public ChestReward PendingReward => _pendingReward;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnBossDied += HandleBossDied;
        }

        private void OnDisable()
        {
            EventBus.OnBossDied -= HandleBossDied;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the pending chest and invokes the callback with the reward.
        /// </summary>
        /// <param name="onRewardGranted">Called with the rolled reward.</param>
        public void OpenChest(Action<ChestReward> onRewardGranted = null)
        {
            if (!_chestReady) return;

            _chestReady = false;

            // Trigger open animation if available
            if (_chestAnimator != null)
            {
                _chestAnimator.SetTrigger("Open");
            }

            GrantReward(_pendingReward);

            onRewardGranted?.Invoke(_pendingReward);
            OnChestOpened?.Invoke(_pendingReward);
        }

        /// <summary>
        /// Opens the chest with a doubled reward (ad stub).
        /// Shows a rewarded ad, then doubles the reward amount on success.
        /// </summary>
        /// <param name="onRewardGranted">Called with the doubled reward.</param>
        public void OpenChestDoubled(Action<ChestReward> onRewardGranted = null)
        {
            if (!_chestReady) return;

            Revive.AdStubManager.ShowRewardedAd(
                onComplete: () =>
                {
                    ChestReward doubled = _pendingReward;
                    doubled.Amount *= 2;

                    _chestReady = false;

                    if (_chestAnimator != null)
                    {
                        _chestAnimator.SetTrigger("Open");
                    }

                    GrantReward(doubled);

                    onRewardGranted?.Invoke(doubled);
                    OnChestOpened?.Invoke(doubled);
                },
                onFailed: () =>
                {
                    Debug.Log("[BossChestSystem] Ad failed – opening chest at normal value.");
                    OpenChest(onRewardGranted);
                }
            );
        }

        #endregion

        #region Private Methods

        private void HandleBossDied()
        {
            _pendingReward = RollReward();
            _chestReady = true;

            // Optionally spawn a chest prefab in the world
            if (_chestPrefab != null)
            {
                Instantiate(_chestPrefab, transform.position, Quaternion.identity);
            }

            OnChestAvailable?.Invoke();
        }

        /// <summary>
        /// Rolls a random reward from the loot table.
        /// </summary>
        private ChestReward RollReward()
        {
            int roll = UnityEngine.Random.Range(0, 100);

            ChestReward reward = new ChestReward();

            if (roll < _coinChance)
            {
                reward.RewardType = ChestRewardType.Coins;
                reward.Amount = UnityEngine.Random.Range(_minCoinDrop, _maxCoinDrop + 1);
            }
            else if (roll < _coinChance + _fragmentChance)
            {
                reward.RewardType = ChestRewardType.SkinFragments;
                reward.Amount = UnityEngine.Random.Range(_minFragments, _maxFragments + 1);
            }
            else
            {
                reward.RewardType = ChestRewardType.MetaTokens;
                reward.Amount = UnityEngine.Random.Range(_minMetaTokens, _maxMetaTokens + 1);
            }

            return reward;
        }

        /// <summary>
        /// Grants the reward to the appropriate system.
        /// </summary>
        private void GrantReward(ChestReward reward)
        {
            switch (reward.RewardType)
            {
                case ChestRewardType.Coins:
                    if (CoinManager.HasInstance)
                    {
                        CoinManager.Instance.AddCoins(reward.Amount);
                    }
                    break;

                case ChestRewardType.SkinFragments:
                    // Skin fragment storage is handled by SkinManager
                    Debug.Log($"[BossChestSystem] Granted {reward.Amount} skin fragment(s).");
                    break;

                case ChestRewardType.MetaTokens:
                    if (MetaUpgradeSystem.Instance != null)
                    {
                        MetaUpgradeSystem.Instance.AddMetaTokens(reward.Amount);
                    }
                    break;
            }
        }

        #endregion
    }
}
