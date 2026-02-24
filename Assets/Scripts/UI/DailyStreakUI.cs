using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertArena.Economy;

namespace DesertArena.UI
{
    /// <summary>
    /// Displays the 15-day daily streak grid.
    /// Highlights the current day, shows each day's reward, and provides
    /// "Claim" and "Claim x2 (Watch Ad)" buttons.
    /// </summary>
    public class DailyStreakUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] [Tooltip("Root panel for the daily streak UI.")]
        private GameObject _panel;

        [SerializeField] [Tooltip("DailyStreakSystem in the scene.")]
        private DailyStreakSystem _streakSystem;

        [Header("Grid")]
        [SerializeField] [Tooltip("Parent transform holding 15 day-slot GameObjects.")]
        private Transform _daySlotParent;

        [SerializeField] [Tooltip("Color for days already claimed.")]
        private Color _claimedColor = new Color(0.4f, 0.8f, 0.4f, 1f);

        [SerializeField] [Tooltip("Color for the current day.")]
        private Color _currentDayColor = new Color(1f, 0.85f, 0.2f, 1f);

        [SerializeField] [Tooltip("Color for future days.")]
        private Color _futureDayColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("Info")]
        [SerializeField] [Tooltip("Text showing current day and reward.")]
        private TextMeshProUGUI _infoText;

        [Header("Buttons")]
        [SerializeField] [Tooltip("Claim today's reward.")]
        private Button _claimButton;

        [SerializeField] [Tooltip("Claim doubled reward via ad.")]
        private Button _claimDoubleButton;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_claimButton != null) _claimButton.onClick.AddListener(OnClaimClicked);
            if (_claimDoubleButton != null) _claimDoubleButton.onClick.AddListener(OnClaimDoubleClicked);
        }

        private void OnDisable()
        {
            if (_claimButton != null) _claimButton.onClick.RemoveListener(OnClaimClicked);
            if (_claimDoubleButton != null) _claimDoubleButton.onClick.RemoveListener(OnClaimDoubleClicked);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the daily streak panel and refreshes all visual elements.
        /// </summary>
        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            RefreshUI();
        }

        /// <summary>
        /// Hides the daily streak panel.
        /// </summary>
        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        #endregion

        #region Private Methods

        private void RefreshUI()
        {
            if (_streakSystem == null) return;

            int currentDay = _streakSystem.GetCurrentDay();
            bool canClaim = _streakSystem.CanClaimToday();

            // Update day slots
            if (_daySlotParent != null)
            {
                for (int i = 0; i < _daySlotParent.childCount && i < 15; i++)
                {
                    Transform slot = _daySlotParent.GetChild(i);
                    int dayNumber = i + 1;

                    // Try to set reward text
                    var label = slot.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = $"Day {dayNumber}\n{_streakSystem.GetRewardForDay(dayNumber)}";
                    }

                    // Try to set background color
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                    {
                        if (dayNumber < currentDay)
                        {
                            img.color = _claimedColor;
                        }
                        else if (dayNumber == currentDay)
                        {
                            img.color = _currentDayColor;
                        }
                        else
                        {
                            img.color = _futureDayColor;
                        }
                    }
                }
            }

            // Update info text
            if (_infoText != null)
            {
                int reward = _streakSystem.GetStreakReward();
                _infoText.text = canClaim
                    ? $"Day {currentDay}: Claim {reward} coins!"
                    : $"Day {currentDay}: Already claimed today.";
            }

            // Button states
            if (_claimButton != null) _claimButton.interactable = canClaim;
            if (_claimDoubleButton != null) _claimDoubleButton.interactable = canClaim;
        }

        private void OnClaimClicked()
        {
            if (_streakSystem == null) return;

            _streakSystem.ClaimReward();
            RefreshUI();
        }

        private void OnClaimDoubleClicked()
        {
            if (_streakSystem == null) return;

            _streakSystem.ClaimRewardDoubled();

            // Delay refresh slightly so the ad callback finishes
            Invoke(nameof(RefreshUI), 2f);
        }

        #endregion
    }
}
