using System;
using UnityEngine;
using DesertArena.Core;
using DesertArena.Enemies;

namespace DesertArena.Levels
{
    /// <summary>
    /// Manages a single level run: timer, kill tracking, and completion logic.
    /// Fires events on level start and end for other systems to react to.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        #region Events

        /// <summary>Fired when the level begins.</summary>
        public event Action OnLevelStarted;

        /// <summary>Fired when the level ends. Passes true for victory, false otherwise.</summary>
        public event Action<bool> OnLevelEnded;

        #endregion

        #region Serialized Fields

        [Header("Level Settings")]
        [SerializeField] [Tooltip("Duration of the level in seconds.")]
        private float levelDuration = 120f;

        [SerializeField] [Tooltip("Reference to the EnemySpawner in the scene.")]
        private EnemySpawner enemySpawner;

        [SerializeField] [Tooltip("ArenaData for the current arena (optional).")]
        private ArenaData arenaData;

        #endregion

        #region Private Fields

        private float _levelTimer;
        private int _killCount;
        private bool _levelActive;
        private bool _bossKilled;

        #endregion

        #region Properties

        /// <summary>Remaining time in the current level.</summary>
        public float LevelTimer => _levelTimer;

        /// <summary>Total kills this level.</summary>
        public int KillCount => _killCount;

        /// <summary>Whether the level is currently active.</summary>
        public bool IsLevelActive => _levelActive;

        /// <summary>Current arena index from GameManager.</summary>
        public int CurrentArena =>
            GameManager.HasInstance ? GameManager.Instance.CurrentArena : 0;

        /// <summary>Current level index from GameManager.</summary>
        public int CurrentLevel =>
            GameManager.HasInstance ? GameManager.Instance.CurrentLevel : 0;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
            EventBus.OnBossDied += HandleBossDied;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
            EventBus.OnBossDied -= HandleBossDied;
        }

        private void Start()
        {
            StartLevel();
        }

        private void Update()
        {
            if (!_levelActive) return;

            _levelTimer -= Time.deltaTime;

            if (_levelTimer <= 0f)
            {
                _levelTimer = 0f;
                HandleTimerExpired();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begins a new level run, resetting timer and kill count.
        /// </summary>
        public void StartLevel()
        {
            _levelTimer = levelDuration;
            _killCount = 0;
            _bossKilled = false;
            _levelActive = true;

            OnLevelStarted?.Invoke();
            EventBus.RaiseLevelStarted();
        }

        #endregion

        #region Private Methods

        private void HandleEnemyKilled(EnemyType type)
        {
            if (!_levelActive) return;

            _killCount++;

            // If boss is dead and no enemies remain, complete the level
            if (_bossKilled && enemySpawner != null && enemySpawner.CurrentEnemyCount <= 0)
            {
                CompleteLevel(true);
            }
        }

        private void HandleBossDied()
        {
            _bossKilled = true;

            // Check immediately in case all enemies are already dead
            if (enemySpawner != null && enemySpawner.CurrentEnemyCount <= 0)
            {
                CompleteLevel(true);
            }
        }

        private void HandleTimerExpired()
        {
            CompleteLevel(_bossKilled);
        }

        private void CompleteLevel(bool victory)
        {
            if (!_levelActive) return;
            _levelActive = false;

            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
            }

            OnLevelEnded?.Invoke(victory);
            EventBus.RaiseLevelEnded(victory);

            if (victory && GameManager.HasInstance)
            {
                GameManager.Instance.CompleteLevel();
            }
        }

        #endregion
    }
}
