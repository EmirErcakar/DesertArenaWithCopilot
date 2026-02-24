using System;
using UnityEngine;
using DesertArena.Utilities;

namespace DesertArena.Economy
{
    /// <summary>
    /// Types of persistent meta-upgrades the player can purchase with meta tokens.
    /// </summary>
    public enum MetaUpgradeType
    {
        MaxHP,
        BaseDamage,
        MoveSpeed,
        CoinBonus
    }

    /// <summary>
    /// Manages persistent meta-upgrades purchased with meta tokens.
    /// Each upgrade has a max level cap and provides a cumulative bonus.
    /// Data is saved in PlayerPrefs.
    /// </summary>
    public class MetaUpgradeSystem : Singleton<MetaUpgradeSystem>
    {
        #region Constants

        private const string PREFS_META_TOKENS = "Meta_Tokens";
        private const string PREFS_UPGRADE_PREFIX = "Meta_Upgrade_";
        private const int MAX_UPGRADE_LEVEL = 5;

        #endregion

        #region Serialized Fields

        [Header("Upgrade Costs (per level)")]
        [SerializeField] [Tooltip("Meta-token cost for each upgrade level (index = level).")]
        private int[] _upgradeCosts = { 1, 2, 3, 5, 8 };

        [Header("Bonus Per Level")]
        [SerializeField] [Tooltip("MaxHP bonus per level (flat amount).")]
        private float _hpBonusPerLevel = 10f;

        [SerializeField] [Tooltip("Base damage bonus per level (flat amount).")]
        private float _damageBonusPerLevel = 2f;

        [SerializeField] [Tooltip("Move speed bonus per level (flat amount).")]
        private float _speedBonusPerLevel = 0.3f;

        [SerializeField] [Tooltip("Coin bonus per level (multiplier, e.g. 0.05 = +5%).")]
        private float _coinBonusPerLevel = 0.05f;

        #endregion

        #region Private Fields

        private int _metaTokens;

        #endregion

        #region Events

        /// <summary>Fired when meta-token balance changes. Passes new total.</summary>
        public event Action<int> OnMetaTokensChanged;

        /// <summary>Fired when an upgrade level changes. Passes the upgrade type and new level.</summary>
        public event Action<MetaUpgradeType, int> OnUpgradeLevelChanged;

        #endregion

        #region Properties

        /// <summary>Current meta-token balance.</summary>
        public int MetaTokens => _metaTokens;

        #endregion

        #region Unity Lifecycle

        protected override void OnSingletonAwake()
        {
            LoadData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the current level of a specific meta-upgrade (0 to MAX_UPGRADE_LEVEL).
        /// </summary>
        public int GetUpgradeLevel(MetaUpgradeType type)
        {
            return PlayerPrefs.GetInt(PrefsKey(type), 0);
        }

        /// <summary>
        /// Attempts to purchase the next level of a meta-upgrade.
        /// </summary>
        /// <returns>True if the upgrade succeeded; false if max level or insufficient tokens.</returns>
        public bool UpgradeMetaStat(MetaUpgradeType type)
        {
            int currentLevel = GetUpgradeLevel(type);

            if (currentLevel >= MAX_UPGRADE_LEVEL) return false;

            int cost = GetUpgradeCost(currentLevel);
            if (_metaTokens < cost) return false;

            _metaTokens -= cost;
            int newLevel = currentLevel + 1;

            PlayerPrefs.SetInt(PrefsKey(type), newLevel);
            PlayerPrefs.SetInt(PREFS_META_TOKENS, _metaTokens);
            PlayerPrefs.Save();

            OnMetaTokensChanged?.Invoke(_metaTokens);
            OnUpgradeLevelChanged?.Invoke(type, newLevel);

            return true;
        }

        /// <summary>
        /// Returns the flat or multiplicative bonus for a given meta-upgrade type.
        /// </summary>
        public float GetMetaBonus(MetaUpgradeType type)
        {
            int level = GetUpgradeLevel(type);

            return type switch
            {
                MetaUpgradeType.MaxHP      => level * _hpBonusPerLevel,
                MetaUpgradeType.BaseDamage => level * _damageBonusPerLevel,
                MetaUpgradeType.MoveSpeed  => level * _speedBonusPerLevel,
                MetaUpgradeType.CoinBonus  => level * _coinBonusPerLevel,
                _                          => 0f
            };
        }

        /// <summary>
        /// Returns the meta-token cost for upgrading from a given level to the next.
        /// </summary>
        /// <param name="currentLevel">The current level of the upgrade.</param>
        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= _upgradeCosts.Length)
                return int.MaxValue;

            return _upgradeCosts[currentLevel];
        }

        /// <summary>
        /// Returns the maximum level cap for all meta-upgrades.
        /// </summary>
        public int GetMaxLevel()
        {
            return MAX_UPGRADE_LEVEL;
        }

        /// <summary>
        /// Adds meta tokens to the player's balance.
        /// </summary>
        /// <param name="amount">Number of tokens to add.</param>
        public void AddMetaTokens(int amount)
        {
            if (amount <= 0) return;

            _metaTokens += amount;
            PlayerPrefs.SetInt(PREFS_META_TOKENS, _metaTokens);
            PlayerPrefs.Save();

            OnMetaTokensChanged?.Invoke(_metaTokens);
        }

        #endregion

        #region Private Methods

        private string PrefsKey(MetaUpgradeType type)
        {
            return PREFS_UPGRADE_PREFIX + type.ToString();
        }

        private void LoadData()
        {
            _metaTokens = PlayerPrefs.GetInt(PREFS_META_TOKENS, 0);
        }

        #endregion
    }
}
