using System;
using UnityEngine;
using DesertArena.Core;

namespace DesertArena.XP
{
    /// <summary>
    /// Tracks XP earned from enemy kills and triggers the upgrade screen
    /// when the XP bar fills. Tuned for approximately three upgrades
    /// per 120-second level, based on kill count rather than time.
    /// </summary>
    public class XPSystem : MonoBehaviour
    {
        #region Events

        /// <summary>Fired when the XP bar fills and an upgrade is available.</summary>
        public event Action OnXPLevelUp;

        #endregion

        #region Serialized Fields

        [Header("XP Settings")]
        [SerializeField] [Tooltip("XP required for the first upgrade.")]
        private int baseXPToNextLevel = 200;

        [SerializeField] [Tooltip("Additional XP required per subsequent upgrade.")]
        private int xpScalingPerLevel = 50;

        [Header("XP Per Enemy Type")]
        [SerializeField] [Tooltip("XP awarded for killing a melee enemy.")]
        private int meleeXP = 10;

        [SerializeField] [Tooltip("XP awarded for killing a ranged enemy.")]
        private int rangedXP = 20;

        [SerializeField] [Tooltip("XP awarded for killing a boss enemy.")]
        private int bossXP = 100;

        #endregion

        #region Private Fields

        private int _currentXP;
        private int _xpToNextLevel;
        private int _upgradeCount;

        #endregion

        #region Properties

        /// <summary>Current XP accumulated toward the next upgrade.</summary>
        public int CurrentXP => _currentXP;

        /// <summary>XP threshold for the next upgrade.</summary>
        public int XPToNextLevel => _xpToNextLevel;

        /// <summary>Normalized fill amount (0–1) for the XP bar UI.</summary>
        public float FillAmount =>
            _xpToNextLevel > 0 ? (float)_currentXP / _xpToNextLevel : 0f;

        /// <summary>Number of upgrades triggered this level.</summary>
        public int UpgradeCount => _upgradeCount;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ResetXP();
        }

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets XP progress for a new level run.
        /// </summary>
        public void ResetXP()
        {
            _currentXP = 0;
            _upgradeCount = 0;
            _xpToNextLevel = baseXPToNextLevel;
        }

        /// <summary>
        /// Adds the specified amount of XP and checks for level-up.
        /// </summary>
        /// <param name="amount">XP to add.</param>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            _currentXP += amount;

            if (_currentXP >= _xpToNextLevel)
            {
                TriggerUpgrade();
            }
        }

        #endregion

        #region Private Methods

        private void HandleEnemyKilled(EnemyType type)
        {
            int xp = type switch
            {
                EnemyType.Melee => meleeXP,
                EnemyType.Ranged => rangedXP,
                EnemyType.Boss => bossXP,
                _ => meleeXP
            };

            AddXP(xp);
        }

        private void TriggerUpgrade()
        {
            _currentXP -= _xpToNextLevel;
            _upgradeCount++;
            _xpToNextLevel = baseXPToNextLevel + (_upgradeCount * xpScalingPerLevel);

            OnXPLevelUp?.Invoke();
            EventBus.RaiseXPLevelUp();

            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetState(GameManager.GameState.Upgrading);
            }
        }

        #endregion
    }
}
