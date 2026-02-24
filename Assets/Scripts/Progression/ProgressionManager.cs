using System;
using UnityEngine;
using DesertArena.Utilities;
using DesertArena.Core;

namespace DesertArena.Progression
{
    /// <summary>
    /// Tracks the player's progress through 20 arenas of 35 levels each.
    /// The player must complete all 35 levels in an arena to unlock the next one.
    /// Provides replay detection for reduced rewards.
    /// All progress is persisted via PlayerPrefs.
    /// </summary>
    public class ProgressionManager : Singleton<ProgressionManager>
    {
        #region Constants

        /// <summary>Total number of arenas in the game.</summary>
        public const int TOTAL_ARENAS = 20;

        /// <summary>Number of levels per arena.</summary>
        public const int LEVELS_PER_ARENA = 35;

        private const string PREFS_CURRENT_ARENA = "Prog_CurrentArena";
        private const string PREFS_CURRENT_LEVEL = "Prog_CurrentLevel";
        private const string PREFS_HIGHEST_ARENA = "Prog_HighestArena";
        private const string PREFS_HIGHEST_LEVEL_PREFIX = "Prog_HighestLevel_Arena_";

        #endregion

        #region Private Fields

        private int _currentArena;
        private int _currentLevel;
        private int _highestArenaUnlocked;
        private int[] _highestLevelPerArena;

        #endregion

        #region Events

        /// <summary>Fired when a level is completed. Passes (arena, level).</summary>
        public event Action<int, int> OnLevelCompleted;

        /// <summary>Fired when a new arena is unlocked. Passes the arena index.</summary>
        public event Action<int> OnArenaUnlocked;

        #endregion

        #region Properties

        /// <summary>The arena the player is currently playing.</summary>
        public int CurrentArena => _currentArena;

        /// <summary>The level within the current arena.</summary>
        public int CurrentLevel => _currentLevel;

        /// <summary>The highest arena the player has unlocked.</summary>
        public int HighestArenaUnlocked => _highestArenaUnlocked;

        #endregion

        #region Unity Lifecycle

        protected override void OnSingletonAwake()
        {
            _highestLevelPerArena = new int[TOTAL_ARENAS];
            LoadProgress();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Marks the current level as complete, advances progress, and
        /// unlocks the next arena if all levels in the current one are done.
        /// </summary>
        public void CompleteLevel()
        {
            // Update highest level for this arena
            if (_currentLevel > GetHighestLevelInArena(_currentArena))
            {
                _highestLevelPerArena[_currentArena] = _currentLevel;
                PlayerPrefs.SetInt(
                    PREFS_HIGHEST_LEVEL_PREFIX + _currentArena,
                    _currentLevel
                );
            }

            OnLevelCompleted?.Invoke(_currentArena, _currentLevel);

            // Advance to next level
            _currentLevel++;

            // Check for arena completion
            if (_currentLevel >= LEVELS_PER_ARENA)
            {
                int nextArena = _currentArena + 1;

                if (nextArena < TOTAL_ARENAS && nextArena > _highestArenaUnlocked)
                {
                    _highestArenaUnlocked = nextArena;
                    PlayerPrefs.SetInt(PREFS_HIGHEST_ARENA, _highestArenaUnlocked);
                    OnArenaUnlocked?.Invoke(nextArena);
                }

                // Stay on the last level of the arena until player moves on
                _currentLevel = LEVELS_PER_ARENA - 1;
            }

            SaveProgress();
        }

        /// <summary>
        /// Returns true if the arena at the given index is unlocked.
        /// Arena 0 is always unlocked.
        /// </summary>
        public bool IsArenaUnlocked(int arenaIndex)
        {
            if (arenaIndex <= 0) return true;
            return arenaIndex <= _highestArenaUnlocked;
        }

        /// <summary>
        /// Returns the highest level completed in a given arena (0-based).
        /// </summary>
        public int GetHighestLevelInArena(int arenaIndex)
        {
            if (arenaIndex < 0 || arenaIndex >= TOTAL_ARENAS) return 0;
            return _highestLevelPerArena[arenaIndex];
        }

        /// <summary>
        /// Returns a formatted string of the player's current progress.
        /// </summary>
        public string GetCurrentProgress()
        {
            return $"Arena {_currentArena + 1} / {TOTAL_ARENAS}  –  " +
                   $"Level {_currentLevel + 1} / {LEVELS_PER_ARENA}";
        }

        /// <summary>
        /// Returns true if the player is replaying an arena they have
        /// already completed (i.e., the arena is below the highest unlocked).
        /// Used to apply reduced coin rewards.
        /// </summary>
        public bool IsReplayingArena()
        {
            return _currentArena < _highestArenaUnlocked;
        }

        /// <summary>
        /// Selects an arena and level to play. Validates that the arena is unlocked.
        /// </summary>
        /// <param name="arena">Arena index (0-based).</param>
        /// <param name="level">Level index within the arena (0-based).</param>
        /// <returns>True if the selection was valid.</returns>
        public bool SelectArenaLevel(int arena, int level)
        {
            if (arena < 0 || arena >= TOTAL_ARENAS) return false;
            if (!IsArenaUnlocked(arena)) return false;
            if (level < 0 || level >= LEVELS_PER_ARENA) return false;

            _currentArena = arena;
            _currentLevel = level;
            SaveProgress();
            return true;
        }

        #endregion

        #region Private Methods

        private void SaveProgress()
        {
            PlayerPrefs.SetInt(PREFS_CURRENT_ARENA, _currentArena);
            PlayerPrefs.SetInt(PREFS_CURRENT_LEVEL, _currentLevel);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _currentArena = PlayerPrefs.GetInt(PREFS_CURRENT_ARENA, 0);
            _currentLevel = PlayerPrefs.GetInt(PREFS_CURRENT_LEVEL, 0);
            _highestArenaUnlocked = PlayerPrefs.GetInt(PREFS_HIGHEST_ARENA, 0);

            for (int i = 0; i < TOTAL_ARENAS; i++)
            {
                _highestLevelPerArena[i] = PlayerPrefs.GetInt(
                    PREFS_HIGHEST_LEVEL_PREFIX + i, 0
                );
            }
        }

        #endregion
    }
}
