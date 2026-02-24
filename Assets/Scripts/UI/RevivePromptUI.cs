using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertArena.Core;
using DesertArena.Revive;

namespace DesertArena.UI
{
    /// <summary>
    /// Full-screen prompt shown when the player dies.
    /// Displays a countdown timer, remaining revives, the coin penalty,
    /// and buttons to watch an ad to revive or give up.
    /// </summary>
    public class RevivePromptUI : MonoBehaviour
    {
        #region Constants

        private const float COUNTDOWN_DURATION = 5f;

        #endregion

        #region Serialized Fields

        [Header("References")]
        [SerializeField] [Tooltip("Root panel of the revive prompt.")]
        private GameObject _panel;

        [SerializeField] [Tooltip("ReviveSystem in the scene.")]
        private ReviveSystem _reviveSystem;

        [Header("Text Elements")]
        [SerializeField] [Tooltip("Title text (e.g. 'REVIVE?').")]
        private TextMeshProUGUI _titleText;

        [SerializeField] [Tooltip("Countdown timer text.")]
        private TextMeshProUGUI _countdownText;

        [SerializeField] [Tooltip("Remaining revives text.")]
        private TextMeshProUGUI _revivesRemainingText;

        [SerializeField] [Tooltip("Coin penalty text.")]
        private TextMeshProUGUI _penaltyText;

        [Header("Buttons")]
        [SerializeField] [Tooltip("Button to watch an ad and revive.")]
        private Button _reviveButton;

        [SerializeField] [Tooltip("Button to give up and end the level.")]
        private Button _giveUpButton;

        #endregion

        #region Private Fields

        private float _countdownTimer;
        private bool _isCountingDown;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnPlayerDeath += ShowPrompt;
            }

            if (_reviveButton != null) _reviveButton.onClick.AddListener(OnReviveClicked);
            if (_giveUpButton != null) _giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }

        private void OnDisable()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnPlayerDeath -= ShowPrompt;
            }

            if (_reviveButton != null) _reviveButton.onClick.RemoveListener(OnReviveClicked);
            if (_giveUpButton != null) _giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
        }

        private void Update()
        {
            if (!_isCountingDown) return;

            // Use unscaled time because the game is paused (timeScale = 0)
            _countdownTimer -= Time.unscaledDeltaTime;

            if (_countdownText != null)
            {
                _countdownText.text = Mathf.CeilToInt(Mathf.Max(_countdownTimer, 0f)).ToString();
            }

            if (_countdownTimer <= 0f)
            {
                _isCountingDown = false;
                OnGiveUpClicked();
            }
        }

        #endregion

        #region Private Methods

        private void ShowPrompt()
        {
            if (_reviveSystem == null) return;

            if (!_reviveSystem.CanRevive())
            {
                EndLevel();
                return;
            }

            if (_panel != null) _panel.SetActive(true);

            // Title
            if (_titleText != null)
            {
                _titleText.text = "REVIVE?";
            }

            // Remaining revives
            if (_revivesRemainingText != null)
            {
                _revivesRemainingText.text =
                    $"Revives left: {_reviveSystem.RevivesRemaining} / {_reviveSystem.MaxRevives}";
            }

            // Coin penalty
            if (_penaltyText != null)
            {
                int penalty = _reviveSystem.GetNextRevivePenalty();
                _penaltyText.text = $"Coin penalty: -{penalty}";
            }

            // Revive button availability
            if (_reviveButton != null)
            {
                _reviveButton.interactable = _reviveSystem.CanRevive();
            }

            // Start countdown
            _countdownTimer = COUNTDOWN_DURATION;
            _isCountingDown = true;
        }

        private void OnReviveClicked()
        {
            _isCountingDown = false;

            if (_reviveSystem != null)
            {
                _reviveSystem.Revive();
            }

            if (_panel != null) _panel.SetActive(false);
        }

        private void OnGiveUpClicked()
        {
            _isCountingDown = false;

            if (_panel != null) _panel.SetActive(false);

            EndLevel();
        }

        private void EndLevel()
        {
            EventBus.RaiseLevelEnded(false);
        }

        #endregion
    }
}
