using System;
using UnityEngine;

namespace DesertArena.Economy
{
    /// <summary>
    /// 15-day daily login streak that awards increasing coin rewards.
    /// Day 1 gives 100 coins and Day 15 gives 1500 coins (linear increase).
    /// The streak resets back to Day 1 after the Day-15 reward is claimed.
    /// </summary>
    public class DailyStreakSystem : MonoBehaviour
    {
        #region Constants

        private const string PREFS_CURRENT_DAY = "DailyStreak_CurrentDay";
        private const string PREFS_LAST_CLAIM  = "DailyStreak_LastClaim";

        private const int MAX_STREAK_DAYS = 15;
        private const int DAY_1_REWARD    = 100;
        private const int DAY_15_REWARD   = 1500;

        #endregion

        #region Events

        /// <summary>Fired when a daily reward is claimed. Passes the coin amount.</summary>
        public event Action<int> OnRewardClaimed;

        #endregion

        #region Private Fields

        private int _currentDay;
        private string _lastClaimDate;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            LoadStreakData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if the player has not yet claimed today's reward.
        /// </summary>
        public bool CanClaimToday()
        {
            return _lastClaimDate != TodayKey();
        }

        /// <summary>
        /// Claims today's streak reward and adds coins to the player's balance.
        /// </summary>
        /// <returns>The number of coins awarded, or 0 if already claimed.</returns>
        public int ClaimReward()
        {
            if (!CanClaimToday()) return 0;

            int reward = GetStreakReward();

            // Grant coins
            if (CoinManager.HasInstance)
            {
                CoinManager.Instance.AddCoins(reward);
            }

            // Advance streak
            _lastClaimDate = TodayKey();
            _currentDay++;

            if (_currentDay > MAX_STREAK_DAYS)
            {
                _currentDay = 1;
            }

            SaveStreakData();
            OnRewardClaimed?.Invoke(reward);

            return reward;
        }

        /// <summary>
        /// Claims a doubled reward after watching an ad (stub).
        /// </summary>
        public void ClaimRewardDoubled()
        {
            if (!CanClaimToday()) return;

            int baseReward = GetStreakReward();

            Revive.AdStubManager.ShowRewardedAd(
                onComplete: () =>
                {
                    int doubled = baseReward * 2;

                    if (CoinManager.HasInstance)
                    {
                        CoinManager.Instance.AddCoins(doubled);
                    }

                    _lastClaimDate = TodayKey();
                    _currentDay++;

                    if (_currentDay > MAX_STREAK_DAYS)
                    {
                        _currentDay = 1;
                    }

                    SaveStreakData();
                    OnRewardClaimed?.Invoke(doubled);
                },
                onFailed: () =>
                {
                    Debug.Log("[DailyStreakSystem] Ad failed – claiming normal reward.");
                    ClaimReward();
                }
            );
        }

        /// <summary>
        /// Returns the current streak day (1 to 15).
        /// </summary>
        public int GetCurrentDay()
        {
            return _currentDay;
        }

        /// <summary>
        /// Returns the coin reward for the current streak day.
        /// Linearly interpolates from 100 (Day 1) to 1500 (Day 15).
        /// </summary>
        public int GetStreakReward()
        {
            float t = Mathf.InverseLerp(1, MAX_STREAK_DAYS, _currentDay);
            return Mathf.RoundToInt(Mathf.Lerp(DAY_1_REWARD, DAY_15_REWARD, t));
        }

        /// <summary>
        /// Returns the coin reward for a specific day (1 to 15).
        /// </summary>
        /// <param name="day">Day number (clamped to 1-15).</param>
        public int GetRewardForDay(int day)
        {
            day = Mathf.Clamp(day, 1, MAX_STREAK_DAYS);
            float t = Mathf.InverseLerp(1, MAX_STREAK_DAYS, day);
            return Mathf.RoundToInt(Mathf.Lerp(DAY_1_REWARD, DAY_15_REWARD, t));
        }

        #endregion

        #region Private Methods

        private string TodayKey()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }

        private void SaveStreakData()
        {
            PlayerPrefs.SetInt(PREFS_CURRENT_DAY, _currentDay);
            PlayerPrefs.SetString(PREFS_LAST_CLAIM, _lastClaimDate);
            PlayerPrefs.Save();
        }

        private void LoadStreakData()
        {
            _currentDay = PlayerPrefs.GetInt(PREFS_CURRENT_DAY, 1);
            _lastClaimDate = PlayerPrefs.GetString(PREFS_LAST_CLAIM, string.Empty);
        }

        #endregion
    }
}
