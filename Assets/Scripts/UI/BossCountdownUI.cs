using System.Collections;
using UnityEngine;
using TMPro;
using DesertArena.Core;

namespace DesertArena.UI
{
    /// <summary>
    /// Displays a centered "BIG BOSS SPAWNING IN 10..9..8.." countdown
    /// overlay with a pulse animation. Triggered via <see cref="EventBus"/>
    /// when the EnemySpawner begins a boss countdown.
    /// </summary>
    public class BossCountdownUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Elements")]
        [SerializeField] [Tooltip("Root GameObject for the countdown overlay.")]
        private GameObject overlayRoot;

        [SerializeField] [Tooltip("Large centered countdown text.")]
        private TextMeshProUGUI countdownText;

        [Header("Pulse Animation")]
        [SerializeField] [Tooltip("Minimum scale during pulse.")]
        private float pulseMin = 0.9f;

        [SerializeField] [Tooltip("Maximum scale during pulse.")]
        private float pulseMax = 1.1f;

        [SerializeField] [Tooltip("Speed of the pulse oscillation.")]
        private float pulseSpeed = 4f;

        #endregion

        #region Private Fields

        private Coroutine _countdownCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (overlayRoot != null) overlayRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.OnBossCountdownStarted += StartCountdown;
        }

        private void OnDisable()
        {
            EventBus.OnBossCountdownStarted -= StartCountdown;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begins the boss countdown display for the given duration.
        /// </summary>
        /// <param name="seconds">Total countdown duration in seconds.</param>
        public void StartCountdown(int seconds)
        {
            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = StartCoroutine(CountdownRoutine(seconds));
        }

        #endregion

        #region Private Methods

        private IEnumerator CountdownRoutine(int totalSeconds)
        {
            if (overlayRoot != null) overlayRoot.SetActive(true);

            for (int i = totalSeconds; i > 0; i--)
            {
                if (countdownText != null)
                {
                    countdownText.text = $"BIG BOSS SPAWNING IN {i}...";
                }

                // Pulse animation for one second
                float elapsed = 0f;
                while (elapsed < 1f)
                {
                    elapsed += Time.deltaTime;
                    if (countdownText != null)
                    {
                        float scale = Mathf.Lerp(pulseMin, pulseMax,
                            (Mathf.Sin(elapsed * pulseSpeed * Mathf.PI) + 1f) * 0.5f);
                        countdownText.transform.localScale = Vector3.one * scale;
                    }
                    yield return null;
                }
            }

            if (countdownText != null)
            {
                countdownText.transform.localScale = Vector3.one;
            }
            if (overlayRoot != null) overlayRoot.SetActive(false);

            _countdownCoroutine = null;
        }

        #endregion
    }
}
