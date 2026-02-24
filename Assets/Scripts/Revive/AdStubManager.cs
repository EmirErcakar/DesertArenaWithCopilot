using System;
using System.Collections;
using UnityEngine;

namespace DesertArena.Revive
{
    /// <summary>
    /// Static stub for rewarded and interstitial ads.
    /// Replace the implementations in this class with a real ad-SDK
    /// integration (e.g., Unity Ads, AdMob) when ready for production.
    /// </summary>
    public static class AdStubManager
    {
        #region Private Fields

        /// <summary>Simulated ad delay in seconds.</summary>
        private const float AD_DELAY_SECONDS = 1.5f;

        #endregion

        #region Public Methods

        /// <summary>
        /// Simulates showing a rewarded ad. After a short delay the
        /// <paramref name="onComplete"/> callback is invoked.
        /// </summary>
        /// <param name="onComplete">Called when the ad finishes successfully.</param>
        /// <param name="onFailed">Called if the ad fails to show.</param>
        public static void ShowRewardedAd(Action onComplete, Action onFailed = null)
        {
            Debug.Log("[AdStubManager] Showing simulated rewarded ad...");
            AdStubRunner.Run(DelayedCallback(AD_DELAY_SECONDS, onComplete));
        }

        /// <summary>
        /// Stub for interstitial ads. Does nothing in this implementation.
        /// </summary>
        public static void ShowInterstitialAd()
        {
            Debug.Log("[AdStubManager] Interstitial ad stub called.");
        }

        /// <summary>
        /// Returns whether a rewarded ad is ready to show.
        /// Always returns true in this stub implementation.
        /// </summary>
        public static bool IsAdReady()
        {
            return true;
        }

        #endregion

        #region Private Methods

        private static IEnumerator DelayedCallback(float delay, Action callback)
        {
            yield return new WaitForSecondsRealtime(delay);
            Debug.Log("[AdStubManager] Simulated ad complete.");
            callback?.Invoke();
        }

        #endregion

        #region Helper MonoBehaviour

        /// <summary>
        /// Tiny helper that lets a static class start coroutines.
        /// Automatically creates and destroys itself as needed.
        /// </summary>
        private class AdStubRunner : MonoBehaviour
        {
            private static AdStubRunner _instance;

            /// <summary>
            /// Starts a coroutine on a hidden runner object.
            /// </summary>
            public static void Run(IEnumerator routine)
            {
                if (_instance == null)
                {
                    var go = new GameObject("[AdStubRunner]");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AdStubRunner>();
                }

                _instance.StartCoroutine(routine);
            }
        }

        #endregion
    }
}
