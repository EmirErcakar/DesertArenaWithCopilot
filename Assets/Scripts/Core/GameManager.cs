using System;
using UnityEngine;
using DesertArena.Utilities;

namespace DesertArena.Core
{
    /// <summary>
    /// Manages overall game state, arena/level progression, and time scale.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Enums

        /// <summary>
        /// All possible states the game can be in.
        /// </summary>
        public enum GameState
        {
            Menu,
            Playing,
            Paused,
            Upgrading,
            Dead,
            BossCountdown,
            Victory
        }

        #endregion

        #region Events

        /// <summary>Fired whenever the game state changes. Passes old and new state.</summary>
        public event Action<GameState, GameState> OnGameStateChanged;

        /// <summary>Fired when the current level/arena is completed.</summary>
        public event Action<int> OnLevelComplete;

        /// <summary>Fired when the player dies.</summary>
        public event Action OnPlayerDeath;

        #endregion

        #region Serialized Fields

        [Header("Level Settings")]
        [SerializeField]
        [Tooltip("The arena index the player is currently in (zero-based).")]
        private int _currentArena;

        [SerializeField]
        [Tooltip("The level index within the current arena (zero-based).")]
        private int _currentLevel;

        #endregion

        #region Private Fields

        private GameState _currentState = GameState.Menu;

        #endregion

        #region Properties

        /// <summary>Current game state.</summary>
        public GameState CurrentState => _currentState;

        /// <summary>Current arena index.</summary>
        public int CurrentArena => _currentArena;

        /// <summary>Current level index within the arena.</summary>
        public int CurrentLevel => _currentLevel;

        /// <summary>True when the game is actively being played.</summary>
        public bool IsPlaying => _currentState == GameState.Playing;

        #endregion

        #region Serialized Fields (Auto Start)

        [Header("Auto Start")]
        [SerializeField]
        [Tooltip("Sahne yüklendiğinde oyunu otomatik başlat (menü ekranı yoksa açık bırakın).")]
        private bool _autoStartGame = true;

        #endregion

        #region Unity Lifecycle

        protected override void OnSingletonAwake()
        {
            // Varsayılan durum Menu — Time.timeScale = 0 yapar.
            // _autoStartGame açıksa Start() içinde Playing'e geçilir.
            _currentState = GameState.Menu;
        }

        private void Start()
        {
            if (_autoStartGame)
            {
                StartGame();
            }
            else
            {
                // Manuel başlatma bekleniyorsa timeScale'i 0 yap
                HandleTimeScale(GameState.Menu);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Transitions to a new game state and adjusts Time.timeScale accordingly.
        /// </summary>
        /// <param name="newState">The state to transition to.</param>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            GameState previousState = _currentState;
            _currentState = newState;

            HandleTimeScale(newState);

            OnGameStateChanged?.Invoke(previousState, newState);

            if (newState == GameState.Dead)
            {
                OnPlayerDeath?.Invoke();
            }
        }

        /// <summary>
        /// Starts or restarts the game from the current arena/level.
        /// </summary>
        public void StartGame()
        {
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Pauses the game. Only works while Playing.
        /// </summary>
        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resumes the game from a paused or upgrading state.
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState == GameState.Paused || _currentState == GameState.Upgrading)
            {
                SetState(GameState.Playing);
            }
        }

        /// <summary>
        /// Signals that the current level has been completed and advances to the next.
        /// </summary>
        public void CompleteLevel()
        {
            _currentLevel++;
            OnLevelComplete?.Invoke(_currentLevel);
        }

        /// <summary>
        /// Advances to the next arena and resets the level counter.
        /// </summary>
        public void AdvanceArena()
        {
            _currentArena++;
            _currentLevel = 0;
        }

        /// <summary>
        /// Triggers the player death state.
        /// </summary>
        public void PlayerDied()
        {
            SetState(GameState.Dead);
        }

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            SetState(GameState.Menu);
        }

        #endregion

        #region Private Methods

        private void HandleTimeScale(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                case GameState.BossCountdown:
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                case GameState.Upgrading:
                case GameState.Dead:
                case GameState.Victory:
                case GameState.Menu:
                    Time.timeScale = 0f;
                    break;
            }
        }

        #endregion
    }
}
